using System;
using System.Collections.Generic;

namespace Riftaria.Common.World.Pieces
{
	/// <summary>
	/// Tier 1 pieces. Round glades and low hollows on a walk that rolls gently
	/// up and down. Nothing is steeper than a tile every two columns, so the
	/// forest never needs a climb. The fight is the largest circle.
	/// </summary>
	public static class ForestPieces
	{
		public const int KindZombie = 0;
		public const int KindSlime = 1;

		private const int Walk = 8;

		/// <summary>The circle's centre sits this far above the floor, so the walk runs through its widest part.</summary>
		private const int Lift = 3;

		/// <summary>Sockets 0 and 1 are the ends of the walk. Shoulder sockets, if any, follow.</summary>
		private const int FirstUpper = 2;

		/// <summary>A tile up for every two across.</summary>
		private const double Grade = 26.5;

		public static RiftCatalog Catalog { get; } = new() {
			Start = rand => Glade(rand, 18, start: true),
			Boss = Arena,
			DeadEnd = Grove,
			Rooms = new[] {
				new RiftRoomEntry(5, rand => Glade(rand, rand.Next(14, 27), start: false)),
				new RiftRoomEntry(2, Hollow)
			},
			MainMin = 6,
			MainMax = 9,
			BranchMin = 1,
			BranchMax = 2,
			BranchRoomsMax = 1,
			LeanAngle = 0,
			LeanStrength = 2.0,
			MaxGrade = Grade,
			Shafts = false,
			LandingMin = 3,
			LandingMax = 8,
			SpanMin = 10,
			SpanMax = 36,
			DipMin = -20,
			DipMax = 20
		};

		/// <summary>
		/// A circle on the walk. Most glades also open a shoulder high on one side,
		/// facing out at a walkable slope, so a branch can climb away.
		/// </summary>
		private static RiftPiece Glade(Random rand, int radius, bool start)
		{
			int width = radius * 2;
			int height = radius + Lift;
			double cy = height - Lift;
			var sockets = new List<RiftSocket> {
				new(0, height - Walk / 2, 180, Walk, start ? SocketRole.Branch : SocketRole.Any),
				new(width, height - Walk / 2, 0, Walk, start ? SocketRole.Exit : SocketRole.Any)
			};

			if (rand.Next(3) > 0) {
				bool right = rand.Next(2) == 0;
				double normal = right ? -22 : -158;
				double rad = normal * Math.PI / 180.0;
				sockets.Add(new RiftSocket(radius + radius * Math.Cos(rad), cy + radius * Math.Sin(rad), normal, Walk));
			}

			var marks = new List<RiftMark>();
			if (!start && rand.Next(2) == 0) {
				marks.Add(new RiftMark(radius, height, rand.Next(2) == 0 ? KindZombie : KindSlime));
			}

			return new RiftPiece {
				Name = start ? "clearing" : "glade",
				Tags = start ? PieceTag.Start : PieceTag.Room,
				Width = width,
				Height = height,
				Sockets = sockets,
				Marks = marks,
				SpawnX = radius,
				SpawnY = height,
				Stamp = (grid, ox, oy, used) => {
					grid.Disk(ox + radius, oy + cy, radius, RiftGrid.Air, rockOnly: false, maxY: oy + height);
					grid.Fill(ox, oy + height - Walk, width, Walk, RiftGrid.Air);
					Stairs(grid, ox, oy, width, height, sockets, used);
				}
			};
		}

		/// <summary>A wide, low dome. A long stretch of open ground between glades.</summary>
		private static RiftPiece Hollow(Random rand)
		{
			int width = rand.Next(50, 71);
			int rise = rand.Next(16, 21);
			var marks = new List<RiftMark>();
			if (rand.Next(2) == 0) {
				marks.Add(new RiftMark(width / 2, rise, rand.Next(2) == 0 ? KindZombie : KindSlime));
			}

			return new RiftPiece {
				Name = "hollow",
				Tags = PieceTag.Room,
				Width = width,
				Height = rise,
				Sockets = new[] {
					new RiftSocket(0, rise - Walk / 2, 180, Walk),
					new RiftSocket(width, rise - Walk / 2, 0, Walk)
				},
				Marks = marks,
				Stamp = (grid, ox, oy, used) => {
					grid.Dome(ox, oy + rise, width, rise, RiftGrid.Air);
					grid.Fill(ox, oy + rise - Walk, width, Walk, RiftGrid.Air);
				}
			};
		}

		/// <summary>
		/// The fight: the largest circle, entered from the left. Three rows of
		/// wood platforms; the middle row is split so you can drop through.
		/// </summary>
		private static RiftPiece Arena(Random rand)
		{
			int radius = rand.Next(32, 44);
			int width = radius * 2;
			int height = radius + Lift;
			double cy = height - Lift;
			int open = height;
			int low = height - (open > 28 ? 10 : 8);
			int mid = height - open / 2;
			int high = height - open * 3 / 4;

			int standX = radius;
			for (int x = width - 2; x > radius; x--) {
				double dx = x + 0.5 - radius;
				double ceiling = cy - Math.Sqrt(Math.Max(0, radius * radius - dx * dx));
				if (height - ceiling >= 8) {
					standX = x - 2;
					break;
				}
			}

			return new RiftPiece {
				Name = "arena",
				Tags = PieceTag.Boss,
				Width = width,
				Height = height,
				Margin = RiftMeasures.Buffer,
				Sockets = new[] { new RiftSocket(0, height - Walk / 2, 180, Walk, SocketRole.Entry) },
				Arena = new RiftArena {
					Left = 0,
					Top = 0,
					Right = width,
					Bottom = height,
					ArenaX = radius,
					ArenaY = height - 5,
					FeetX = standX,
					FeetY = height,
					PortalX = standX - 8 > radius ? standX - 8 : standX,
					PortalY = height - 2
				},
				Stamp = (grid, ox, oy, used) => {
					grid.Disk(ox + radius, oy + cy, radius, RiftGrid.Boss, rockOnly: false, maxY: oy + height);
					grid.Fill(ox, oy + height - Walk, width, Walk, RiftGrid.Boss);
					PlatformRow(grid, ox, oy + low, width, radius, split: false);
					PlatformRow(grid, ox, oy + mid, width, radius, split: true);
					PlatformRow(grid, ox, oy + high, width, radius, split: false);
				}
			};
		}

		private static void PlatformRow(RiftGrid grid, int ox, int y, int width, int radius, bool split)
		{
			int center = ox + radius;
			if (!split) {
				grid.Line(ox + 2, ox + width - 1, y, RiftGrid.Platform);
				return;
			}

			grid.Line(ox + 2, center - 3, y, RiftGrid.Platform);
			grid.Line(center + 4, ox + width - 1, y, RiftGrid.Platform);
		}

		/// <summary>A small dead end with one enemy, opening to the left or the right.</summary>
		private static RiftPiece Grove(Random rand)
		{
			const int width = 24;
			const int rise = 14;
			bool left = rand.Next(2) == 0;
			return new RiftPiece {
				Name = "grove",
				Tags = PieceTag.DeadEnd,
				Width = width,
				Height = rise,
				Sockets = new[] {
					left
						? new RiftSocket(0, rise - Walk / 2, 180, Walk, SocketRole.Entry)
						: new RiftSocket(width, rise - Walk / 2, 0, Walk, SocketRole.Entry)
				},
				Marks = new[] { new RiftMark(width / 2, rise, rand.Next(2) == 0 ? KindZombie : KindSlime) },
				Stamp = (grid, ox, oy, used) => {
					grid.Dome(ox, oy + rise, width, rise, RiftGrid.Air);
					grid.Fill(ox, oy + rise - Walk, width, Walk, RiftGrid.Air);
				}
			};
		}

		/// <summary>Platform steps up to a shoulder socket in use.</summary>
		private static void Stairs(RiftGrid grid, int ox, int oy, int width, int height, IReadOnlyList<RiftSocket> sockets, bool[] used)
		{
			for (int i = FirstUpper; i < sockets.Count; i++) {
				if (!used[i]) {
					continue;
				}

				RiftSocket socket = sockets[i];
				bool onLeft = socket.X < width / 2.0;
				int fromX = ox + width / 2;
				int toX = ox + (int)Math.Round(socket.X) + (onLeft ? 5 : -5);
				int bottom = oy + (int)Math.Round(socket.Y + socket.Width / 2.0);
				grid.StairsTo(fromX, oy + height, toX, bottom);
			}
		}
	}
}
