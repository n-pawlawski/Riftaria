using System;
using System.Collections.Generic;

namespace Riftaria.Common.World.Pieces
{
	/// <summary>
	/// Snaps pieces together at their sockets. A main path runs from the start
	/// to the boss, then a few short branches end in dead ends. Each join is
	/// either a direct snap, when the two openings face each other, or a
	/// connector built for it. Unused sockets stay rock.
	/// </summary>
	public static class RiftAssembler
	{
		/// <summary>Rock kept between pieces that are not joined, so walls do not wear through.</summary>
		public const int Margin = 6;

		/// <summary>Two openings within this many degrees of facing each other may snap directly.</summary>
		public const double SnapTolerance = 10;

		/// <summary>Sharpest turn a connector makes. Anything tighter folds back on itself.</summary>
		public const double MaxBend = 120;

		/// <summary>Largest world the subworld is asked to make.</summary>
		public const int MaxWidth = 1400;

		public const int MaxHeight = 700;

		/// <summary>Rock left between a piece and the parent it is joined to through a connector.</summary>
		private const int BesideGap = 2;

		private const double DirectChance = 0.35;

		/// <summary>Join attempts allowed for the main path, and again for each branch.</summary>
		private const int Budget = 3000;

		private const int BranchBudget = 400;

		/// <summary>Null when the pieces could not be fitted with this seed. Try another.</summary>
		public static RiftAssembly Build(RiftCatalog catalog, int seed)
		{
			var builder = new Builder(catalog, new Random(seed));
			return builder.Run() ? builder.Raster() : null;
		}

		private sealed class Part
		{
			public RiftBox Box;

			public int Owner;

			public int Margin;

			/// <summary>Owner this part may overlap freely: the parent, right at the socket.</summary>
			public int Touches = -1;

			/// <summary>Owner kept only <see cref="BesideMargin"/> away rather than the full margin.</summary>
			public int Beside = -1;

			public int BesideMargin;
		}

		private sealed class Link
		{
			public PlacedPiece From;

			public int FromSocket;

			public PlacedPiece To;

			public int ToSocket;

			/// <summary>Null for a direct snap.</summary>
			public RiftConnector Connector;

			public List<Part> Parts;
		}

		private sealed class Builder
		{
			private readonly RiftCatalog _catalog;
			private readonly Random _rand;
			private readonly List<PlacedPiece> _pieces = new();
			private readonly List<Link> _links = new();
			private readonly List<Part> _parts = new();
			private int _nextId;
			private int _spent;
			private int _limit;
			private PlacedPiece _start;
			private Link _bossLink;
			private int _mainRooms;
			private int _branches;

			public Builder(RiftCatalog catalog, Random rand)
			{
				_catalog = catalog;
				_rand = rand;
			}

			public bool Run()
			{
				RiftPiece start = _catalog.Start(_rand);
				_start = new PlacedPiece(start, 0, 0, _nextId++);
				_pieces.Add(_start);
				foreach (RiftBox band in start.Footprint()) {
					_parts.Add(new Part { Box = band, Owner = _start.Id, Margin = start.Margin });
				}

				_mainRooms = Roll(_catalog.MainMin, _catalog.MainMax);
				_spent = 0;
				_limit = Budget;
				if (!GrowMain(_start, _mainRooms)) {
					return false;
				}

				GrowBranches();
				return true;
			}

			/// <summary>Depth first, so a dead end backs up and tries another socket or piece.</summary>
			private bool GrowMain(PlacedPiece tip, int remaining)
			{
				bool boss = remaining == 0;
				foreach (int socket in Order(tip, s => s.CanLeave, _catalog.LeanStrength)) {
					for (int k = 0; k < 3; k++) {
						if (_spent > _limit) {
							return false;
						}

						RiftPiece piece = boss ? _catalog.Boss(_rand) : _catalog.Room(_rand);
						Link link = TryAttach(tip, socket, piece, boss);
						if (link == null) {
							continue;
						}

						Commit(link);
						if (boss) {
							_bossLink = link;
							return true;
						}

						if (GrowMain(link.To, remaining - 1)) {
							return true;
						}

						Undo(link);
					}
				}

				return false;
			}

			private void GrowBranches()
			{
				int count = Roll(_catalog.BranchMin, _catalog.BranchMax);
				for (int b = 0; b < count; b++) {
					for (int t = 0; t < 8; t++) {
						PlacedPiece host = _pieces[_rand.Next(_pieces.Count)];
						if (host == _bossLink.To) {
							continue;
						}

						_spent = 0;
						_limit = BranchBudget;
						if (GrowBranch(host, Roll(0, _catalog.BranchRoomsMax))) {
							_branches++;
							break;
						}
					}
				}
			}

			private bool GrowBranch(PlacedPiece tip, int rooms)
			{
				bool end = rooms == 0;
				foreach (int socket in Order(tip, s => s.CanBranch, lean: 0)) {
					for (int k = 0; k < 3; k++) {
						if (_spent > _limit) {
							return false;
						}

						RiftPiece piece = end ? _catalog.DeadEnd(_rand) : _catalog.Room(_rand);
						Link link = TryAttach(tip, socket, piece, boss: false);
						if (link == null) {
							continue;
						}

						Commit(link);
						if (end || GrowBranch(link.To, rooms - 1)) {
							return true;
						}

						Undo(link);
					}
				}

				return false;
			}

			/// <summary>
			/// Free sockets that fit, best first. A positive lean favours sockets
			/// facing the catalog's lean angle, so the main path heads away from the start.
			/// </summary>
			private List<int> Order(PlacedPiece piece, Func<RiftSocket, bool> fits, double lean)
			{
				var scored = new List<(int Index, double Score)>();
				for (int i = 0; i < piece.Used.Length; i++) {
					RiftSocket socket = piece.Socket(i);
					if (piece.Used[i] || !fits(socket)) {
						continue;
					}

					double facing = Math.Cos(Radians(socket.Angle - _catalog.LeanAngle));
					scored.Add((i, lean * facing + _rand.NextDouble()));
				}

				scored.Sort((a, b) => b.Score.CompareTo(a.Score));
				var order = new List<int>(scored.Count);
				foreach ((int index, double _) in scored) {
					order.Add(index);
				}

				return order;
			}

			/// <summary>
			/// Tries each of the piece's entries against one open socket. The
			/// connector runs out along the socket, then turns to meet the entry
			/// head on. The boss always gets a long level run, so its buffer holds.
			/// </summary>
			private Link TryAttach(PlacedPiece from, int fromSocket, RiftPiece piece, bool boss)
			{
				_spent++;
				RiftSocket open = from.Socket(fromSocket);
				double px = from.SocketX(fromSocket);
				double py = from.SocketY(fromSocket);

				foreach (int entry in Entries(piece)) {
					RiftSocket socket = piece.Sockets[entry];
					double inAngle = socket.Angle + 180;
					double bend = AngleDelta(open.Angle, inAngle);
					if (bend > MaxBend) {
						continue;
					}

					for (int t = 0; t < 3; t++) {
						bool direct = !boss
							&& t == 0
							&& bend <= SnapTolerance
							&& Math.Abs(open.Width - socket.Width) <= 2
							&& _rand.NextDouble() < DirectChance;
						double landOut = 0;
						double span = 0;
						double landIn = 0;
						double spanAngle = 0;
						if (!direct) {
							landOut = Roll(_catalog.LandingMin, _catalog.LandingMax);
							span = Roll(_catalog.SpanMin, _catalog.SpanMax);
							landIn = boss ? RiftMeasures.Buffer + Roll(0, 12) : Roll(_catalog.LandingMin, _catalog.LandingMax);
							spanAngle = SpanAngle(open.Angle, inAngle);
							if (AngleDelta(open.Angle, spanAngle) > MaxBend || AngleDelta(spanAngle, inAngle) > MaxBend) {
								continue;
							}
						}

						double ax = px + Math.Cos(Radians(open.Angle)) * landOut;
						double ay = py + Math.Sin(Radians(open.Angle)) * landOut;
						double bx = ax + Math.Cos(Radians(spanAngle)) * span;
						double by = ay + Math.Sin(Radians(spanAngle)) * span;
						double qx = bx + Math.Cos(Radians(inAngle)) * landIn;
						double qy = by + Math.Sin(Radians(inAngle)) * landIn;
						var placed = new PlacedPiece(piece, (int)Math.Round(qx - socket.X), (int)Math.Round(qy - socket.Y), _nextId);
						int linkOwner = _nextId + 1;
						double ex = placed.SocketX(entry);
						double ey = placed.SocketY(entry);
						bx = ex - Math.Cos(Radians(inAngle)) * landIn;
						by = ey - Math.Sin(Radians(inAngle)) * landIn;

						var parts = new List<Part>();
						foreach (RiftBox band in piece.Footprint()) {
							parts.Add(new Part {
								Box = Offset(band, placed.X, placed.Y),
								Owner = placed.Id,
								Margin = piece.Margin,
								Beside = from.Id,
								BesideMargin = boss ? piece.Margin : direct ? 0 : BesideGap
							});
						}

						RiftConnector connector = null;
						if (!direct) {
							connector = RiftConnector.Through(open.Width, (px, py), (ax, ay), (bx, by), (ex, ey));
							if (connector.Segments.Count == 0 || !Walkable(connector)) {
								continue;
							}

							int index = 0;
							foreach (RiftBox box in connector.Boxes()) {
								parts.Add(new Part {
									Box = box,
									Owner = linkOwner,
									Margin = Margin,
									Touches = index < 3 ? from.Id : -1,
									Beside = from.Id,
									BesideMargin = BesideGap
								});
								index++;
							}
						}

						if (Blocked(parts) || !Fits(parts)) {
							continue;
						}

						_nextId += 2;
						return new Link {
							From = from,
							FromSocket = fromSocket,
							To = placed,
							ToSocket = entry,
							Connector = connector,
							Parts = parts
						};
					}
				}

				return null;
			}

			/// <summary>
			/// Halfway between the way out and the way in, tilted by a rolled dip.
			/// A positive dip points down whether the run heads left or right.
			/// </summary>
			private double SpanAngle(double outAngle, double inAngle)
			{
				double delta = ((inAngle - outAngle) % 360 + 540) % 360 - 180;
				double middle = outAngle + delta / 2;
				double dip = Roll(_catalog.DipMin, _catalog.DipMax);
				return Math.Cos(Radians(middle)) >= 0 ? middle + dip : middle - dip;
			}

			/// <summary>No run is steeper than the tier allows, and no turn folds back.</summary>
			private bool Walkable(RiftConnector connector)
			{
				for (int i = 0; i < connector.Segments.Count; i++) {
					RiftSegment segment = connector.Segments[i];
					if (!_catalog.Shafts && segment.Grade > _catalog.MaxGrade) {
						return false;
					}

					if (i > 0 && AngleDelta(Heading(connector.Segments[i - 1]), Heading(segment)) > MaxBend) {
						return false;
					}
				}

				return true;
			}

			private List<int> Entries(RiftPiece piece)
			{
				var entries = new List<int>();
				for (int i = 0; i < piece.Sockets.Count; i++) {
					if (piece.Sockets[i].CanEnter) {
						entries.Add(i);
					}
				}

				for (int i = entries.Count - 1; i > 0; i--) {
					int j = _rand.Next(i + 1);
					(entries[i], entries[j]) = (entries[j], entries[i]);
				}

				return entries;
			}

			/// <summary>New parts are not tested against each other; they belong to one join.</summary>
			private bool Blocked(List<Part> parts)
			{
				foreach (Part part in parts) {
					foreach (Part other in _parts) {
						if (other.Owner == part.Touches) {
							continue;
						}

						int margin = other.Owner == part.Beside ? part.BesideMargin : Math.Max(part.Margin, other.Margin);
						if (part.Box.Inflate(margin).Intersects(other.Box)) {
							return true;
						}
					}
				}

				return false;
			}

			/// <summary>The whole plan, with these parts added, still fits the largest world.</summary>
			private bool Fits(List<Part> parts)
			{
				Bounds(_parts, out double minX, out double minY, out double maxX, out double maxY);
				foreach (Part part in parts) {
					minX = Math.Min(minX, part.Box.MinX);
					minY = Math.Min(minY, part.Box.MinY);
					maxX = Math.Max(maxX, part.Box.MaxX);
					maxY = Math.Max(maxY, part.Box.MaxY);
				}

				int edge = RiftMeasures.Buffer;
				int width = (int)Math.Ceiling(maxX) - (int)Math.Floor(minX) + edge * 2;
				int height = (int)Math.Ceiling(maxY) + SkyTop(minY, maxY, edge) + edge;
				return width <= MaxWidth && height <= MaxHeight;
			}

			private void Commit(Link link)
			{
				_pieces.Add(link.To);
				_links.Add(link);
				_parts.AddRange(link.Parts);
				link.From.Used[link.FromSocket] = true;
				link.To.Used[link.ToSocket] = true;
			}

			private void Undo(Link link)
			{
				_pieces.Remove(link.To);
				_links.Remove(link);
				foreach (Part part in link.Parts) {
					_parts.Remove(part);
				}

				link.From.Used[link.FromSocket] = false;
				link.To.Used[link.ToSocket] = false;
			}

			public RiftAssembly Raster()
			{
				Bounds(_parts, out double minX, out double minY, out double maxX, out double maxY);
				int edge = RiftMeasures.Buffer;
				int ox = edge - (int)Math.Floor(minX);
				int oy = SkyTop(minY, maxY, edge);
				int width = (int)Math.Ceiling(maxX) + ox + edge;
				int height = (int)Math.Ceiling(maxY) + oy + edge;
				var grid = new RiftGrid(width, height, edge);

				foreach (PlacedPiece piece in _pieces) {
					piece.Piece.Stamp(grid, piece.X + ox, piece.Y + oy, piece.Used);
				}

				var connectors = new List<RiftConnector>();
				foreach (Link link in _links) {
					if (link.Connector != null) {
						link.Connector.Carve(grid, ox, oy);
						connectors.Add(link.Connector);
						continue;
					}

					double radius = link.From.Socket(link.FromSocket).Width / 2.0;
					grid.Disk(link.From.SocketX(link.FromSocket) + ox, link.From.SocketY(link.FromSocket) + oy, radius, RiftGrid.Air, rockOnly: true);
				}

				foreach (RiftConnector connector in connectors) {
					connector.Rungs(grid, ox, oy);
				}

				grid.LayFloors();

				var marks = new List<RiftMark>();
				foreach (PlacedPiece piece in _pieces) {
					foreach (RiftMark mark in piece.Piece.Marks) {
						marks.Add(new RiftMark(mark.X + piece.X + ox, mark.Y + piece.Y + oy, mark.Kind));
					}
				}

				RiftConnector approach = _bossLink.Connector;
				if (_catalog.ApproachMark >= 0 && approach != null && approach.Segments.Count > 0) {
					RiftSegment run = approach.Segments[approach.Segments.Count - 1];
					double mx = (run.X0 + run.X1) / 2.0;
					double my = (run.Y0 + run.Y1) / 2.0 + run.Thick / 2.0;
					marks.Add(new RiftMark((int)Math.Floor(mx) + ox, (int)Math.Round(my) + oy, _catalog.ApproachMark));
				}

				PlacedPiece boss = _bossLink.To;
				RiftArena arena = boss.Piece.Arena;
				int bx = boss.X + ox;
				int by = boss.Y + oy;
				return new RiftAssembly {
					Grid = grid,
					SpawnX = _start.X + ox + _start.Piece.SpawnX,
					SpawnY = _start.Y + oy + _start.Piece.SpawnY,
					RoomLeft = bx + arena.Left,
					RoomTop = by + arena.Top,
					RoomRight = bx + arena.Right,
					RoomBottom = by + arena.Bottom,
					ArenaX = bx + arena.ArenaX,
					ArenaY = by + arena.ArenaY,
					FeetX = arena.FeetX < 0 ? -1 : bx + arena.FeetX,
					FeetY = by + arena.FeetY,
					PortalX = bx + arena.PortalX,
					PortalY = by + arena.PortalY,
					Marks = marks,
					Pieces = _pieces,
					Connectors = connectors,
					OffsetX = ox,
					OffsetY = oy,
					MainRooms = _mainRooms,
					Branches = _branches
				};
			}

			private int Roll(int min, int max) => _rand.Next(min, max + 1);
		}

		/// <summary>
		/// Pushes the plan down until its highest cell is below the sky. The sky
		/// line sits at about a third of the surface depth, and rows added at the
		/// top move that line down too. Same rule as the evil rift.
		/// </summary>
		private static int SkyTop(double minY, double maxY, int edge)
		{
			int minTile = (int)Math.Floor(minY);
			int content = (int)Math.Ceiling(maxY) - minTile;
			int top = edge;
			for (int i = 0; i < 6; i++) {
				int height = content + top + edge;
				int sky = (int)(0.35 * (height - 42));
				if (top >= sky + 48) {
					break;
				}

				top += (int)Math.Ceiling((sky + 48 - top) / 0.65) + 1;
			}

			return top - minTile;
		}

		private static void Bounds(List<Part> parts, out double minX, out double minY, out double maxX, out double maxY)
		{
			minX = double.MaxValue;
			minY = double.MaxValue;
			maxX = double.MinValue;
			maxY = double.MinValue;
			foreach (Part part in parts) {
				minX = Math.Min(minX, part.Box.MinX);
				minY = Math.Min(minY, part.Box.MinY);
				maxX = Math.Max(maxX, part.Box.MaxX);
				maxY = Math.Max(maxY, part.Box.MaxY);
			}
		}

		private static RiftBox Offset(RiftBox box, int x, int y) => new(box.MinX + x, box.MinY + y, box.MaxX + x, box.MaxY + y);

		private static double Radians(double degrees) => degrees * Math.PI / 180.0;

		private static double Heading(RiftSegment segment)
			=> Math.Atan2(segment.Y1 - segment.Y0, segment.X1 - segment.X0) * 180.0 / Math.PI;

		private static double AngleDelta(double a, double b)
		{
			double delta = Math.Abs(a - b) % 360.0;
			return delta > 180 ? 360 - delta : delta;
		}
	}
}
