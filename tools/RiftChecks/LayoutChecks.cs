using Riftaria.Common.World;
using Riftaria.Common.World.Pieces;

namespace RiftTools
{
	public sealed class LayoutReport
	{
		public List<string> Lines { get; } = new();

		public List<string> Failures { get; } = new();

		public bool Ok => Failures.Count == 0;
	}

	/// <summary>
	/// Rolls many rifts of every tier and checks what the game relies on: the
	/// boss can be reached from spawn, so can every enemy, nothing else opens
	/// into the boss buffer, the world fits, and steep tunnels have rungs.
	/// </summary>
	public static class LayoutChecks
	{
		private const int Rolls = 1000;
		private const int MaxFailures = 20;

		public static LayoutReport Run(int seed = 12345, int rolls = Rolls)
		{
			var report = new LayoutReport();
			foreach (RiftPiecePlan plan in new[] { RiftPlans.Forest, RiftPlans.Evil, RiftPlans.Stone }) {
				RunPlan(report, plan, seed, rolls);
			}

			return report;
		}

		private static void RunPlan(LayoutReport report, RiftPiecePlan plan, int seed, int rolls)
		{
			var rand = new Random(seed);
			int failuresBefore = report.Failures.Count;
			var pieceCounts = new Dictionary<string, int>();
			long mainRooms = 0;
			long branches = 0;
			long steep = 0;
			long connectors = 0;
			int maxWidth = 0;
			int maxHeight = 0;
			int built = 0;

			for (int i = 0; i < rolls && report.Failures.Count - failuresBefore < MaxFailures; i++) {
				int rollSeed = rand.Next();
				try {
					plan.Roll(rollSeed);
				}
				catch (InvalidOperationException error) {
					report.Failures.Add($"{plan.Name} seed {rollSeed}: {error.Message}");
					continue;
				}

				RiftAssembly assembly = plan.Assembly;
				string problem = Check(assembly);
				if (problem != null) {
					report.Failures.Add($"{plan.Name} seed {rollSeed}: {problem}");
					continue;
				}

				built++;
				mainRooms += assembly.MainRooms;
				branches += assembly.Branches;
				connectors += assembly.Connectors.Count;
				steep += assembly.Connectors.Sum(c => c.Segments.Count(s => s.Steep));
				maxWidth = Math.Max(maxWidth, assembly.Grid.Width);
				maxHeight = Math.Max(maxHeight, assembly.Grid.Height);
				foreach (PlacedPiece piece in assembly.Pieces) {
					pieceCounts[piece.Piece.Name] = pieceCounts.GetValueOrDefault(piece.Piece.Name) + 1;
				}
			}

			plan.Clear();
			report.Lines.Add($"{plan.Name} layouts: {built} of {rolls} passed");
			if (built > 0) {
				report.Lines.Add($"  {mainRooms / (double)built:0.0} main rooms, {branches / (double)built:0.0} branches, {connectors / (double)built:0.0} connectors per rift");
				report.Lines.Add($"  {steep} steep runs with rungs; largest world {maxWidth} x {maxHeight}");
				report.Lines.Add("  pieces: " + string.Join(", ", pieceCounts.OrderByDescending(p => p.Value).Select(p => $"{p.Key} {p.Value}")));
			}
		}

		private static string Check(RiftAssembly assembly)
		{
			RiftGrid grid = assembly.Grid;
			if (grid.Width > RiftAssembler.MaxWidth || grid.Height > RiftAssembler.MaxHeight) {
				return $"world is {grid.Width} x {grid.Height}, over the limit.";
			}

			bool[] reached = Flood(grid, assembly.SpawnX, assembly.SpawnY - 1, (x, y) => true);
			if (reached == null) {
				return $"spawn {assembly.SpawnX},{assembly.SpawnY} is not standing in open space.";
			}

			if (!AnyIn(grid, reached, assembly.RoomLeft, assembly.RoomTop, assembly.RoomRight, assembly.RoomBottom)) {
				return "the boss room cannot be reached from spawn.";
			}

			foreach (RiftMark mark in assembly.Marks) {
				int y = mark.Y - 1;
				if (!Passable(grid, mark.X, y) || !reached[y * grid.Width + mark.X]) {
					return $"enemy at {mark.X},{mark.Y} cannot be reached.";
				}
			}

			string buffer = CheckBuffer(assembly);
			if (buffer != null) {
				return buffer;
			}

			foreach (RiftConnector connector in assembly.Connectors) {
				foreach (RiftSegment segment in connector.Segments) {
					if (!segment.Steep || Math.Abs(segment.Y1 - segment.Y0) < RiftConnector.RungPitch * 2) {
						continue;
					}

					if (!HasRung(grid, segment, assembly.OffsetX, assembly.OffsetY)) {
						return $"steep run at {segment.X0 + assembly.OffsetX:0},{segment.Y0 + assembly.OffsetY:0} has no rungs.";
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Inside the buffer, every open cell must belong to the one way in. A
		/// second opening on the room, or a room within the buffer, fails.
		/// </summary>
		private static string CheckBuffer(RiftAssembly assembly)
		{
			RiftGrid grid = assembly.Grid;
			int pad = RiftMeasures.Buffer;
			int left = assembly.RoomLeft;
			int top = assembly.RoomTop;
			int right = assembly.RoomRight;
			int bottom = assembly.RoomBottom;
			bool InRing(int x, int y)
				=> x >= left - pad && x < right + pad && y >= top - pad && y < bottom + pad
				&& !(x >= left && x < right && y >= top && y < bottom);

			var doors = new List<(int X, int Y)>();
			for (int x = left - 1; x <= right; x++) {
				doors.Add((x, top - 1));
				doors.Add((x, bottom));
			}

			for (int y = top; y < bottom; y++) {
				doors.Add((left - 1, y));
				doors.Add((right, y));
			}

			bool[] entry = null;
			foreach ((int x, int y) in doors) {
				if (!Passable(grid, x, y)) {
					continue;
				}

				if (entry == null) {
					entry = Flood(grid, x, y, InRing);
					continue;
				}

				if (!entry[y * grid.Width + x]) {
					return "the boss room has more than one way in.";
				}
			}

			if (entry == null) {
				return "the boss room has no way in.";
			}

			for (int y = Math.Max(0, top - pad); y < Math.Min(grid.Height, bottom + pad); y++) {
				for (int x = Math.Max(0, left - pad); x < Math.Min(grid.Width, right + pad); x++) {
					if (InRing(x, y) && Passable(grid, x, y) && !entry[y * grid.Width + x]) {
						return $"open cell {x},{y} sits in the boss buffer apart from the way in.";
					}
				}
			}

			return null;
		}

		private static bool HasRung(RiftGrid grid, RiftSegment segment, int ox, int oy)
		{
			int x0 = (int)Math.Floor(Math.Min(segment.X0, segment.X1) + ox - segment.Thick);
			int x1 = (int)Math.Ceiling(Math.Max(segment.X0, segment.X1) + ox + segment.Thick);
			int y0 = (int)Math.Floor(Math.Min(segment.Y0, segment.Y1) + oy);
			int y1 = (int)Math.Ceiling(Math.Max(segment.Y0, segment.Y1) + oy);
			for (int y = y0; y <= y1; y++) {
				for (int x = x0; x <= x1; x++) {
					if (grid.Get(x, y) == RiftGrid.Platform) {
						return true;
					}
				}
			}

			return false;
		}

		private static bool Passable(RiftGrid grid, int x, int y)
			=> grid.IsOpen(x, y) || grid.Get(x, y) == RiftGrid.Platform;

		/// <summary>Four-way fill over open cells and platforms. Null if the start is solid.</summary>
		private static bool[] Flood(RiftGrid grid, int sx, int sy, Func<int, int, bool> within)
		{
			if (!Passable(grid, sx, sy)) {
				return null;
			}

			var seen = new bool[grid.Width * grid.Height];
			var stack = new Stack<(int X, int Y)>();
			stack.Push((sx, sy));
			seen[sy * grid.Width + sx] = true;
			while (stack.Count > 0) {
				(int x, int y) = stack.Pop();
				foreach ((int nx, int ny) in new[] { (x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1) }) {
					if (nx < 0 || ny < 0 || nx >= grid.Width || ny >= grid.Height) {
						continue;
					}

					int index = ny * grid.Width + nx;
					if (seen[index] || !Passable(grid, nx, ny) || !within(nx, ny)) {
						continue;
					}

					seen[index] = true;
					stack.Push((nx, ny));
				}
			}

			return seen;
		}

		private static bool AnyIn(RiftGrid grid, bool[] reached, int left, int top, int right, int bottom)
		{
			for (int y = top; y < bottom; y++) {
				for (int x = left; x < right; x++) {
					if (x >= 0 && y >= 0 && x < grid.Width && y < grid.Height && reached[y * grid.Width + x]) {
						return true;
					}
				}
			}

			return false;
		}
	}
}
