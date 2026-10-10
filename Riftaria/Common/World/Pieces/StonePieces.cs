using System;
using System.Collections.Generic;

namespace Riftaria.Common.World.Pieces
{
	/// <summary>
	/// Tier 3 pieces. Every room has a flat walk along its floor with a socket
	/// at each end, so a level corridor can always pass through. The upper
	/// sockets lead up and away; a room only adds the steps to one when it is used.
	/// </summary>
	public static class StonePieces
	{
		public const int KindSkeleton = 0;
		public const int KindGolem = 1;
		public const int KindBat = 2;
		public const int KindMiner = 3;

		private const int Walk = 8;

		private const int WellW = 28;
		private const int WellRise = 56;
		private const int DiamondW = 46;
		private const int DiamondRise = 40;
		private const int RingW = 50;
		private const int RingRise = 48;
		private const int BossW = 96;
		private const int BossH = 42;

		/// <summary>Sockets 0 and 1 of every room are the two ends of its walk.</summary>
		private const int FirstUpper = 2;

		public static RiftCatalog Catalog { get; } = new() {
			Start = _ => Dome(),
			Boss = _ => Hall(),
			DeadEnd = Alcove,
			Rooms = new[] {
				new RiftRoomEntry(3, _ => Well()),
				new RiftRoomEntry(3, _ => Diamond()),
				new RiftRoomEntry(3, Ring)
			},
			MainMin = 3,
			MainMax = 5,
			BranchMin = 1,
			BranchMax = 3,
			BranchRoomsMax = 2,
			ApproachMark = KindMiner
		};

		private static RiftPiece Dome()
		{
			const int width = 52;
			const int rise = 22;
			var sockets = new List<RiftSocket> {
				new(0, rise - Walk / 2, 180, Walk, SocketRole.Branch),
				new(width, rise - Walk / 2, 0, Walk, SocketRole.Exit),
				OnEllipse(width / 2.0, rise, width / 2.0, rise, -40)
			};

			return new RiftPiece {
				Name = "dome",
				Tags = PieceTag.Start,
				Width = width,
				Height = rise,
				Sockets = sockets,
				SpawnX = 10,
				SpawnY = rise,
				Stamp = (grid, ox, oy, used) => {
					grid.Dome(ox, oy + rise, width, rise, RiftGrid.Air);
					grid.Fill(ox, oy + rise - Walk, width, Walk, RiftGrid.Air);
					Stairs(grid, ox, oy, width, rise, sockets, used);
				}
			};
		}

		/// <summary>A tall shaft with shelves up both walls. Its upper sockets leave from the top.</summary>
		private static RiftPiece Well()
		{
			var sockets = new List<RiftSocket> {
				new(0, WellRise - Walk / 2, 180, Walk),
				new(WellW, WellRise - Walk / 2, 0, Walk),
				new(0, 12, -150, Walk),
				new(WellW, 12, -30, Walk),
				new(WellW / 2.0, 0, -90, Walk)
			};

			return new RiftPiece {
				Name = "well",
				Tags = PieceTag.Room,
				Width = WellW,
				Height = WellRise,
				Sockets = sockets,
				Marks = new[] { new RiftMark(WellW / 2, WellRise, KindSkeleton) },
				Stamp = (grid, ox, oy, used) => {
					grid.Fill(ox, oy, WellW, WellRise, RiftGrid.Air);
					bool left = true;
					for (int y = WellRise - RiftConnector.RungPitch; y > 4; y -= RiftConnector.RungPitch) {
						int x0 = left ? ox + 2 : ox + WellW - 14;
						grid.Line(x0, x0 + 12, oy + y, RiftGrid.Ledge);
						left = !left;
					}
				}
			};
		}

		/// <summary>Sockets sit in the middle of the two upper faces, facing straight out of them.</summary>
		private static RiftPiece Diamond()
		{
			double rx = DiamondW / 2.0;
			double ry = DiamondRise / 2.0;
			double faceAngle = Math.Atan2(-rx, ry) * 180.0 / Math.PI;
			var sockets = new List<RiftSocket> {
				new(0, DiamondRise - Walk / 2, 180, Walk),
				new(DiamondW, DiamondRise - Walk / 2, 0, Walk),
				new(rx + rx / 2, ry / 2, faceAngle, Walk),
				new(rx / 2, ry / 2, -180 - faceAngle, Walk)
			};

			return new RiftPiece {
				Name = "diamond",
				Tags = PieceTag.Room,
				Width = DiamondW,
				Height = DiamondRise,
				Sockets = sockets,
				Marks = new[] { new RiftMark(DiamondW / 2, DiamondRise, KindGolem) },
				Stamp = (grid, ox, oy, used) => {
					grid.Diamond(ox, oy, DiamondW, DiamondRise, RiftGrid.Air);
					grid.Fill(ox, oy + DiamondRise - Walk, DiamondW, Walk, RiftGrid.Air);
					Stairs(grid, ox, oy, DiamondW, DiamondRise, sockets, used);
				}
			};
		}

		/// <summary>A round chamber around a rock pillar, with two or three sockets rolled around its upper arc.</summary>
		private static RiftPiece Ring(Random rand)
		{
			const double radius = RingW / 2.0 - 1.0;
			double cx = RingW / 2.0;
			double cy = RingRise - radius;
			var sockets = new List<RiftSocket> {
				new(0, RingRise - Walk / 2, 180, Walk),
				new(RingW, RingRise - Walk / 2, 0, Walk)
			};

			var angles = new List<double>();
			int want = rand.Next(2, 4);
			for (int tries = 0; tries < 40 && angles.Count < want; tries++) {
				double angle = rand.Next(-160, -19);
				if (angles.TrueForAll(other => Math.Abs(other - angle) >= 40)) {
					angles.Add(angle);
				}
			}

			foreach (double angle in angles) {
				double rad = angle * Math.PI / 180.0;
				sockets.Add(new RiftSocket(cx + radius * Math.Cos(rad), cy + radius * Math.Sin(rad), angle, Walk));
			}

			return new RiftPiece {
				Name = "ring",
				Tags = PieceTag.Room,
				Width = RingW,
				Height = RingRise,
				Sockets = sockets,
				Marks = new[] { new RiftMark(8, RingRise - 12, KindBat) },
				Stamp = (grid, ox, oy, used) => {
					grid.Disk(ox + cx, oy + cy, radius, RiftGrid.Air, rockOnly: false, maxY: oy + RingRise);
					grid.Disk(ox + cx, oy + RingRise - 30, 7, RiftGrid.Rock, rockOnly: false, maxY: oy + RingRise - Walk);
					grid.Fill(ox, oy + RingRise - Walk, RingW, Walk, RiftGrid.Air);
					Stairs(grid, ox, oy, RingW, RingRise, sockets, used);
				}
			};
		}

		/// <summary>The sealed boss hall. One way in, at the left end of its floor.</summary>
		private static RiftPiece Hall()
		{
			return new RiftPiece {
				Name = "hall",
				Tags = PieceTag.Boss,
				Width = BossW,
				Height = BossH,
				Margin = RiftMeasures.Buffer,
				Sockets = new[] { new RiftSocket(0, BossH - Walk / 2, 180, Walk, SocketRole.Entry) },
				Arena = new RiftArena {
					Left = 0,
					Top = 0,
					Right = BossW,
					Bottom = BossH,
					ArenaX = BossW / 2,
					ArenaY = BossH - 8,
					FeetX = BossW - 10,
					FeetY = BossH,
					PortalX = BossW - 18,
					PortalY = BossH - 3
				},
				Stamp = (grid, ox, oy, used) => {
					grid.Fill(ox, oy, BossW, BossH, RiftGrid.Boss);
					HallBar(grid, ox, oy + BossH - 10, split: false);
					HallBar(grid, ox, oy + BossH - 21, split: true);
					HallBar(grid, ox, oy + BossH - 32, split: false);
				}
			};
		}

		/// <summary>Three rows above the entrance. The middle row is split so you can drop through.</summary>
		private static void HallBar(RiftGrid grid, int ox, int y, bool split)
		{
			int mid = ox + BossW / 2;
			if (!split) {
				grid.Line(ox + 6, ox + BossW - 6, y, RiftGrid.Platform);
				return;
			}

			grid.Line(ox + 6, mid - 5, y, RiftGrid.Platform);
			grid.Line(mid + 6, ox + BossW - 6, y, RiftGrid.Platform);
		}

		/// <summary>A small dead end with one enemy, opening to the left or the right.</summary>
		private static RiftPiece Alcove(Random rand)
		{
			const int width = 22;
			const int rise = 14;
			bool left = rand.Next(2) == 0;
			int kind = rand.Next(4);
			return new RiftPiece {
				Name = "alcove",
				Tags = PieceTag.DeadEnd,
				Width = width,
				Height = rise,
				Sockets = new[] {
					left
						? new RiftSocket(0, rise - Walk / 2, 180, Walk, SocketRole.Entry)
						: new RiftSocket(width, rise - Walk / 2, 0, Walk, SocketRole.Entry)
				},
				Marks = new[] { new RiftMark(width / 2, rise, kind) },
				Stamp = (grid, ox, oy, used) => {
					grid.Dome(ox, oy + rise, width, rise, RiftGrid.Air);
					grid.Fill(ox, oy + rise - Walk, width, Walk, RiftGrid.Air);
				}
			};
		}

		/// <summary>Socket on an upper arc at parametric angle <paramref name="t"/>, facing along the curve's normal.</summary>
		private static RiftSocket OnEllipse(double cx, double cy, double rx, double ry, double t)
		{
			double rad = t * Math.PI / 180.0;
			double x = cx + rx * Math.Cos(rad);
			double y = cy + ry * Math.Sin(rad);
			double normal = Math.Atan2(Math.Sin(rad) / ry, Math.Cos(rad) / rx) * 180.0 / Math.PI;
			return new RiftSocket(x, y, normal, Walk);
		}

		/// <summary>
		/// Platform steps from the floor up to each upper socket in use, starting
		/// on the far side of the room so the climb is gentle.
		/// </summary>
		private static void Stairs(RiftGrid grid, int ox, int oy, int width, int rise, IReadOnlyList<RiftSocket> sockets, bool[] used)
		{
			for (int i = FirstUpper; i < sockets.Count; i++) {
				if (!used[i]) {
					continue;
				}

				RiftSocket socket = sockets[i];
				bool onLeft = socket.X < width / 2.0;
				int fromX = ox + (onLeft ? width * 3 / 4 : width / 4);
				int toX = ox + (int)Math.Round(socket.X) + (onLeft ? 5 : -5);
				int bottom = oy + (int)Math.Round(socket.Y + socket.Width / 2.0);
				grid.StairsTo(fromX, oy + rise, toX, bottom);
			}
		}
	}
}
