using System;
using System.Collections.Generic;

namespace Riftaria.Common.World
{
	/// <summary>
	/// Tier 2 outline. Short rooms run to a side wall, each with its own slope,
	/// then turn back until the depth is reached. The last room is a chamber of
	/// horizontal ledges. Rolled before the subworld is created, because the
	/// world size is this grid.
	/// </summary>
	public static class RiftEvilPlan
	{
		public const byte Rock = 0;
		public const byte Air = 1;
		public const byte Boss = 2;
		public const byte Floor = 3;
		public const byte Ledge = 4;

		private const int LenMin = 16;
		private const int LenMax = 32;
		private const int Span = 480;
		private const int Depth = 220;
		private const int Thick = 12;
		private const int Slope = 18;
		private const int Pitch = 20;
		private const int Bend = 12;
		private const int Inset = 6;

		/// <summary>The path turns this many tiles before the side, instead of at the wall.</summary>
		private const int TurnEarly = 50;

		/// <summary>Solid margin on every side of the world. Nothing is carved here.</summary>
		private const int Edge = RiftMeasures.Buffer;

		public static int Width { get; private set; }

		public static int Height { get; private set; }

		public static byte[] Cells { get; private set; }

		public static int SpawnX { get; private set; }

		public static int SpawnY { get; private set; }

		public static int ArenaX { get; private set; }

		public static int ArenaY { get; private set; }

		/// <summary>Boss chamber in tiles. Left is negative when this roll has no chamber.</summary>
		public static int RoomLeft { get; private set; } = -1;

		public static int RoomTop { get; private set; }

		public static int RoomRight { get; private set; }

		public static int RoomBottom { get; private set; }

		public static int RequiredWidth()
		{
			if (Cells == null) {
				Roll();
			}

			return Width;
		}

		public static int RequiredHeight()
		{
			if (Cells == null) {
				Roll();
			}

			return Height;
		}

		public static void Clear()
		{
			Cells = null;
			Width = 0;
			Height = 0;
			RoomLeft = -1;
		}

		public static void Roll()
		{
			var rand = new Random();
			int gradeHigh = Slope + Pitch;
			var specs = new List<Segment>();
			double x = Inset;
			double y = 0;
			int direction = 1;
			int grade = Slope;
			int? previousMin = null;
			int rooms = 0;

			while (y < Depth && rooms < 160) {
				bool hitWall = false;
				bool first = true;
				int floor = Slope - Pitch;
				int passMin = gradeHigh;
				while (y < Depth && rooms < 160 && !hitWall) {
					if (first) {
						int rolled = Slope + RollInclusive(rand, -Pitch, Pitch);
						grade = previousMin == null ? rolled : ClearGrade(previousMin.Value, rolled, gradeHigh);
						floor = grade;
						first = false;
					}
					else {
						grade = Clamp(grade + RollInclusive(rand, -Bend, Bend), floor, gradeHigh);
					}

					passMin = Math.Min(passMin, grade);
					double angle = direction > 0 ? grade : 180 - grade;
					Step step = Direction(angle);
					int length = RollInclusive(rand, LenMin, LenMax);
					double wallX = direction > 0 ? Span - Inset - TurnEarly : Inset + TurnEarly;
					double endX = x + step.C * length;
					bool pastWall = direction > 0 ? endX > wallX : endX < wallX;
					if (pastWall) {
						length = Math.Abs(step.C) < 0.08 ? length : (int)Math.Floor((wallX - x) / step.C);
						hitWall = true;
					}

					if (length < 8) {
						hitWall = true;
						break;
					}

					specs.Add(new Segment(length, angle));
					x += step.C * length;
					y += step.S * length;
					rooms++;
				}

				if (y >= Depth) {
					break;
				}

				direction *= -1;
				previousMin = passMin;
			}

			int bossW = Math.Min(Span, RollInclusive(rand, 120, 168));
			int bossH = RollInclusive(rand, 72, 96);
			double bossLeft = x - bossW / 2.0;
			if (bossLeft < 0) {
				bossLeft = 0;
			}

			if (bossLeft + bossW > Span) {
				bossLeft = Span - bossW;
			}

			var boss = new Room(bossLeft, y - 8, bossW, bossH);
			List<Placed> placed = Place(specs);
			Raster(placed, boss);
		}

		public static bool Open(int x, int y)
		{
			if (Cells == null || x < 0 || y < 0 || x >= Width || y >= Height) {
				return false;
			}

			byte cell = Cells[y * Width + x];
			return cell == Air || cell == Boss || cell == Floor;
		}

		public static bool IsLedge(int x, int y)
		{
			if (Cells == null || x < 0 || y < 0 || x >= Width || y >= Height) {
				return false;
			}

			return Cells[y * Width + x] == Ledge;
		}

		private static void Raster(List<Placed> placed, Room boss)
		{
			List<Joint> joints = Joints(placed);
			double minX = boss.Left;
			double minY = boss.Top;
			double maxX = boss.Left + boss.Width;
			double maxY = boss.Top + boss.Height;
			foreach (Placed room in placed) {
				Aabb box = Bounds(room);
				minX = Math.Min(minX, box.MinX);
				minY = Math.Min(minY, box.MinY);
				maxX = Math.Max(maxX, box.MaxX);
				maxY = Math.Max(maxY, box.MaxY);
			}

			foreach (Joint joint in joints) {
				minX = Math.Min(minX, joint.X - joint.Radius);
				minY = Math.Min(minY, joint.Y - joint.Radius);
				maxX = Math.Max(maxX, joint.X + joint.Radius);
				maxY = Math.Max(maxY, joint.Y + joint.Radius);
			}

			int ox = Edge - (int)Math.Floor(minX);
			int oy = TopOffset(minY, maxY);
			Width = (int)Math.Ceiling(maxX) + ox + Edge;
			Height = (int)Math.Ceiling(maxY) + oy + Edge;
			RoomLeft = (int)Math.Floor(boss.Left + ox);
			RoomTop = (int)Math.Floor(boss.Top + oy);
			RoomRight = (int)Math.Ceiling(boss.Left + boss.Width + ox);
			RoomBottom = (int)Math.Ceiling(boss.Top + boss.Height + oy);
			Cells = new byte[Width * Height];

			foreach (Placed room in placed) {
				StampRoom(room, ox, oy);
			}

			foreach (Joint joint in joints) {
				StampDisk(joint.X, joint.Y, joint.Radius, ox, oy, Air, rockOnly: false);
			}

			StampBoss(boss, ox, oy);
			if (placed.Count > 0) {
				Tip entrance = TipOf(placed[placed.Count - 1]);
				StampDisk(entrance.X, entrance.Y, placed[placed.Count - 1].Thick / 2.0 + 0.6, ox, oy, Air, rockOnly: true);
			}

			SealBuffer(ox, oy, placed, joints);

			LayFloors();
			PlaceSpawn(placed, ox, oy);
			ArenaX = (int)Math.Floor(boss.Left + ox + boss.Width / 2.0);
			ArenaY = (int)Math.Floor(boss.Top + oy + 10);
			if (ArenaX < Edge + 1) {
				ArenaX = Edge + 1;
			}

			if (ArenaX >= Width - Edge) {
				ArenaX = Width - Edge - 1;
			}

			if (!Open(ArenaX, ArenaY)) {
				for (int dy = 1; dy < 48; dy++) {
					if (Open(ArenaX, ArenaY + dy)) {
						ArenaY += dy;
						break;
					}

					if (Open(ArenaX, ArenaY - dy)) {
						ArenaY -= dy;
						break;
					}
				}
			}
		}

		/// <summary>
		/// Turns the band around the chamber back to rock. Corridor tiles stay,
		/// so the path can still enter.
		/// </summary>
		private static void SealBuffer(int ox, int oy, List<Placed> placed, List<Joint> joints)
		{
			int pad = RiftMeasures.Buffer;
			int x0 = Math.Max(Edge, RoomLeft - pad);
			int y0 = Math.Max(Edge, RoomTop - pad);
			int x1 = Math.Min(Width - Edge, RoomRight + pad);
			int y1 = Math.Min(Height - Edge, RoomBottom + pad);
			for (int y = y0; y < y1; y++) {
				for (int x = x0; x < x1; x++) {
					if (x >= RoomLeft && x < RoomRight && y >= RoomTop && y < RoomBottom) {
						continue;
					}

					if (OnApproach(x, y, ox, oy, placed, joints)) {
						continue;
					}

					int index = y * Width + x;
					if (Cells[index] != Rock) {
						Cells[index] = Rock;
					}
				}
			}
		}

		private static bool OnApproach(int x, int y, int ox, int oy, List<Placed> placed, List<Joint> joints)
		{
			foreach (Placed room in placed) {
				double rad = room.Angle * Math.PI / 180.0;
				double c = Math.Cos(rad);
				double s = Math.Sin(rad);
				double dx = (x + 0.5 - ox) - room.Cx;
				double dy = (y + 0.5 - oy) - room.Cy;
				double lx = dx * c + dy * s;
				double ly = -dx * s + dy * c;
				if (Math.Abs(lx) <= room.Length / 2.0 + 0.85 && Math.Abs(ly) <= room.Thick / 2.0 + 0.6) {
					return true;
				}
			}

			foreach (Joint joint in joints) {
				double dx = (x + 0.5 - ox) - joint.X;
				double dy = (y + 0.5 - oy) - joint.Y;
				double reach = joint.Radius + 0.6;
				if (dx * dx + dy * dy <= reach * reach) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Pushes the carve down until the first corridor is below the sky.
		/// The sky line sits at about a third of the surface depth, and adding
		/// tiles at the top moves that line down too.
		/// </summary>
		private static int TopOffset(double minY, double maxY)
		{
			int minTile = (int)Math.Floor(minY);
			int content = (int)Math.Ceiling(maxY) - minTile;
			int floorFromTop = (int)Math.Ceiling(-minY + Thick / 2.0) + 2;
			int top = Edge;
			for (int i = 0; i < 6; i++) {
				int height = content + top + Edge;
				int sky = (int)(0.35 * (height - 42));
				int floorY = top + floorFromTop;
				if (floorY >= sky + 48) {
					break;
				}

				int shortfall = sky + 48 - floorY;
				int add = (int)Math.Ceiling(shortfall / 0.65) + 1;
				top += add;
			}

			return top - minTile;
		}

		/// <summary>
		/// A third of the way along the first room, on the floor under the
		/// center of the tunnel. The tip of that room is a clipped cap.
		/// </summary>
		private static void PlaceSpawn(List<Placed> placed, int ox, int oy)
		{
			SpawnX = Edge + 8;
			SpawnY = Height / 2;
			if (placed.Count == 0) {
				return;
			}

			Placed room = placed[0];
			Step step = Direction(room.Angle);
			double startX = room.Cx - step.C * room.Length / 2.0;
			double startY = room.Cy - step.S * room.Length / 2.0;
			double travel = Math.Min(Math.Max(8.0, room.Length * 0.35), room.Length - 2.0);
			for (double along = travel; along >= 4.0; along -= 2.0) {
				int x = (int)Math.Floor(startX + step.C * along + ox);
				int y = (int)Math.Floor(startY + step.S * along + oy);
				if (TryFloor(x, y, out int floor)) {
					SpawnX = x;
					SpawnY = floor;
					return;
				}
			}
		}

		private static bool TryFloor(int x, int centerY, out int floor)
		{
			floor = 0;
			if (x < Edge + 1 || x >= Width - Edge) {
				return false;
			}

			for (int dy = 0; dy <= Thick; dy++) {
				int y = centerY + dy;
				if (y - 3 < Edge || y + 1 >= Height - Edge) {
					continue;
				}

				if (!Open(x, y) || Open(x, y + 1)) {
					continue;
				}

				if (Open(x, y - 1) && Open(x, y - 2) && Open(x, y - 3)) {
					floor = y + 1;
					return true;
				}
			}

			return false;
		}

		private static List<Placed> Place(List<Segment> specs)
		{
			double x = Inset;
			double y = 0;
			var placed = new List<Placed>(specs.Count);
			foreach (Segment spec in specs) {
				Step step = Direction(spec.Angle);
				placed.Add(new Placed(
					x + step.C * spec.Length / 2.0,
					y + step.S * spec.Length / 2.0,
					spec.Length,
					Thick,
					spec.Angle));
				x += step.C * spec.Length;
				y += step.S * spec.Length;
			}

			return placed;
		}

		private static int ClearGrade(int previous, int desired, int limit)
		{
			int need = Thick + 2;
			int distance = Thick * 2;
			int grade = desired;
			while (grade < limit && CenterGap(previous, grade, distance) < need) {
				grade++;
			}

			return grade;
		}

		private static double CenterGap(int previous, int next, int distance)
		{
			double earlier = previous * Math.PI / 180.0;
			double later = next * Math.PI / 180.0;
			double cosine = Math.Cos(earlier);
			if (Math.Abs(cosine) < 0.2) {
				return distance;
			}

			return distance * (Math.Sin(later) + Math.Sin(earlier) * Math.Cos(later) / cosine);
		}

		private static List<Joint> Joints(List<Placed> placed)
		{
			var joints = new List<Joint>();
			for (int i = 0; i < placed.Count - 1; i++) {
				if (AngleDelta(placed[i].Angle, placed[i + 1].Angle) < 1) {
					continue;
				}

				Tip tip = TipOf(placed[i]);
				joints.Add(new Joint(tip.X, tip.Y, placed[i].Thick / 2.0 + 0.6));
			}

			return joints;
		}

		private static Tip TipOf(Placed room)
		{
			Step step = Direction(room.Angle);
			return new Tip(room.Cx + step.C * room.Length / 2.0, room.Cy + step.S * room.Length / 2.0);
		}

		private static double AngleDelta(double a, double b)
		{
			double delta = Math.Abs(a - b) % 360.0;
			if (delta > 180) {
				delta = 360 - delta;
			}

			return delta;
		}

		private static void StampRoom(Placed room, int ox, int oy)
		{
			Aabb box = Bounds(room);
			int x0 = (int)Math.Floor(box.MinX + ox) - 1;
			int y0 = (int)Math.Floor(box.MinY + oy) - 1;
			int x1 = (int)Math.Ceiling(box.MaxX + ox) + 1;
			int y1 = (int)Math.Ceiling(box.MaxY + oy) + 1;
			double rad = room.Angle * Math.PI / 180.0;
			double c = Math.Cos(rad);
			double s = Math.Sin(rad);
			double halfL = room.Length / 2.0 + 0.85;
			double halfT = room.Thick / 2.0;
			for (int y = y0; y <= y1; y++) {
				for (int x = x0; x <= x1; x++) {
					if (!Inside(x, y)) {
						continue;
					}

					double dx = (x + 0.5 - ox) - room.Cx;
					double dy = (y + 0.5 - oy) - room.Cy;
					double lx = dx * c + dy * s;
					double ly = -dx * s + dy * c;
					if (Math.Abs(lx) <= halfL && Math.Abs(ly) <= halfT) {
						Cells[y * Width + x] = Air;
					}
				}
			}
		}

		private static void StampDisk(double cx, double cy, double radius, int ox, int oy, byte value, bool rockOnly)
		{
			int x0 = (int)Math.Floor(cx + ox - radius) - 1;
			int y0 = (int)Math.Floor(cy + oy - radius) - 1;
			int x1 = (int)Math.Ceiling(cx + ox + radius) + 1;
			int y1 = (int)Math.Ceiling(cy + oy + radius) + 1;
			double limit = radius * radius;
			for (int y = y0; y <= y1; y++) {
				for (int x = x0; x <= x1; x++) {
					if (!Inside(x, y)) {
						continue;
					}

					double dx = (x + 0.5 - ox) - cx;
					double dy = (y + 0.5 - oy) - cy;
					if (dx * dx + dy * dy > limit) {
						continue;
					}

					int index = y * Width + x;
					if (rockOnly && Cells[index] != Rock) {
						continue;
					}

					Cells[index] = value;
				}
			}
		}

		private static void StampBoss(Room boss, int ox, int oy)
		{
			int left = (int)Math.Floor(boss.Left + ox);
			int top = (int)Math.Floor(boss.Top + oy);
			int right = (int)Math.Ceiling(boss.Left + boss.Width + ox);
			int bottom = (int)Math.Ceiling(boss.Top + boss.Height + oy);
			for (int y = top; y < bottom; y++) {
				for (int x = left; x < right; x++) {
					if (!Inside(x, y)) {
						continue;
					}

					Cells[y * Width + x] = Boss;
				}
			}

			StampLedges(boss, ox, oy);
		}

		/// <summary>Four bands of platforms. The top of the room stays open for the corridor.</summary>
		private static void StampLedges(Room boss, int ox, int oy)
		{
			double[,] bars = {
				{ 0.30, 0.08, 0.92 },
				{ 0.48, 0.06, 0.40 },
				{ 0.48, 0.58, 0.94 },
				{ 0.66, 0.06, 0.42 },
				{ 0.66, 0.54, 0.94 },
				{ 0.84, 0.08, 0.90 }
			};

			for (int i = 0; i < bars.GetLength(0); i++) {
				int y0 = (int)Math.Floor(boss.Top + boss.Height * bars[i, 0] + oy);
				int x0 = (int)Math.Floor(boss.Left + boss.Width * bars[i, 1] + ox);
				int x1 = (int)Math.Ceiling(boss.Left + boss.Width * bars[i, 2] + ox);
				for (int x = x0; x < x1; x++) {
					if (!Inside(x, y0)) {
						continue;
					}

					int index = y0 * Width + x;
					if (Cells[index] == Boss || Cells[index] == Air) {
						Cells[index] = Ledge;
					}
				}
			}
		}

		private static void LayFloors()
		{
			for (int y = Edge; y < Height - Edge - 1; y++) {
				for (int x = Edge; x < Width - Edge; x++) {
					int here = y * Width + x;
					byte below = Cells[(y + 1) * Width + x];
					if ((Cells[here] == Air || Cells[here] == Boss) && below == Rock) {
						Cells[here] = Floor;
					}
				}
			}
		}

		private static Aabb Bounds(Placed room)
		{
			double rad = room.Angle * Math.PI / 180.0;
			double c = Math.Abs(Math.Cos(rad));
			double s = Math.Abs(Math.Sin(rad));
			double hx = room.Length / 2.0 * c + room.Thick / 2.0 * s;
			double hy = room.Length / 2.0 * s + room.Thick / 2.0 * c;
			return new Aabb(room.Cx - hx, room.Cy - hy, room.Cx + hx, room.Cy + hy);
		}

		private static bool Inside(int x, int y)
		{
			return x >= Edge && y >= Edge && x < Width - Edge && y < Height - Edge;
		}

		private static Step Direction(double angle)
		{
			double rad = angle * Math.PI / 180.0;
			return new Step(Math.Cos(rad), Math.Sin(rad));
		}

		private static int RollInclusive(Random rand, int min, int max)
		{
			if (max < min) {
				int swap = min;
				min = max;
				max = swap;
			}

			return rand.Next(min, max + 1);
		}

		private static int Clamp(int value, int min, int max)
		{
			if (value < min) {
				return min;
			}

			return value > max ? max : value;
		}

		private readonly struct Segment
		{
			public int Length { get; }

			public double Angle { get; }

			public Segment(int length, double angle)
			{
				Length = length;
				Angle = angle;
			}
		}

		private readonly struct Placed
		{
			public double Cx { get; }

			public double Cy { get; }

			public double Length { get; }

			public double Thick { get; }

			public double Angle { get; }

			public Placed(double cx, double cy, double length, double thick, double angle)
			{
				Cx = cx;
				Cy = cy;
				Length = length;
				Thick = thick;
				Angle = angle;
			}
		}

		private readonly struct Room
		{
			public double Left { get; }

			public double Top { get; }

			public double Width { get; }

			public double Height { get; }

			public Room(double left, double top, double width, double height)
			{
				Left = left;
				Top = top;
				Width = width;
				Height = height;
			}
		}

		private readonly struct Joint
		{
			public double X { get; }

			public double Y { get; }

			public double Radius { get; }

			public Joint(double x, double y, double radius)
			{
				X = x;
				Y = y;
				Radius = radius;
			}
		}

		private readonly struct Tip
		{
			public double X { get; }

			public double Y { get; }

			public Tip(double x, double y)
			{
				X = x;
				Y = y;
			}
		}

		private readonly struct Step
		{
			public double C { get; }

			public double S { get; }

			public Step(double c, double s)
			{
				C = c;
				S = s;
			}
		}

		private readonly struct Aabb
		{
			public double MinX { get; }

			public double MinY { get; }

			public double MaxX { get; }

			public double MaxY { get; }

			public Aabb(double minX, double minY, double maxX, double maxY)
			{
				MinX = minX;
				MinY = minY;
				MaxX = maxX;
				MaxY = maxY;
			}
		}
	}
}
