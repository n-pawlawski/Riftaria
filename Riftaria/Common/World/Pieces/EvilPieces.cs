using System;
using System.Collections.Generic;

namespace Riftaria.Common.World.Pieces
{
	/// <summary>
	/// Tier 2 pieces. Wide tunnels sink from pocket to pocket and through pits,
	/// always heading down, to a tall chamber of ledges. Every tunnel stays
	/// walkable; the steepest matches the old folded rift.
	/// </summary>
	public static class EvilPieces
	{
		public const int KindEater = 0;
		public const int KindCrimera = 1;
		public const int KindCorruptor = 2;

		/// <summary>Tunnel and walk height. A corruptor fits through.</summary>
		private const int Thick = 12;

		/// <summary>Angle a dip socket leaves the floor at, below level.</summary>
		private const int DipAngle = 30;

		public static RiftCatalog Catalog { get; } = new() {
			Start = _ => Pocket(null, 40, 20, start: true),
			Boss = Chamber,
			DeadEnd = Hollow,
			Rooms = new[] {
				new RiftRoomEntry(5, rand => Pocket(rand, rand.Next(30, 45), rand.Next(18, 23), start: false)),
				new RiftRoomEntry(2, Pit)
			},
			MainMin = 5,
			MainMax = 8,
			BranchMin = 1,
			BranchMax = 3,
			BranchRoomsMax = 1,
			LeanAngle = 90,
			LeanStrength = 0.8,
			MaxGrade = RiftConnector.MaxGrade,
			Shafts = false,
			LandingMin = 3,
			LandingMax = 8,
			SpanMin = 16,
			SpanMax = 40,
			DipMin = 8,
			DipMax = 34
		};

		/// <summary>
		/// A low cave on the walk. Each end of its floor has a level socket and a
		/// dip socket angled down and out, so the path can keep sinking.
		/// </summary>
		private static RiftPiece Pocket(Random rand, int width, int rise, bool start)
		{
			double mouth = rise - Thick / 2.0;
			var sockets = new List<RiftSocket> {
				new(0, mouth, 180, Thick, start ? SocketRole.Branch : SocketRole.Any),
				new(width, mouth, 0, Thick, start ? SocketRole.Exit : SocketRole.Any),
				new(0, mouth, 180 - DipAngle, Thick, start ? SocketRole.Branch : SocketRole.Any),
				new(width, mouth, DipAngle, Thick, start ? SocketRole.Exit : SocketRole.Any)
			};

			var marks = new List<RiftMark>();
			if (!start && rand.Next(5) < 3) {
				marks.Add(new RiftMark(width / 2, rise, rand.Next(3)));
			}

			return new RiftPiece {
				Name = start ? "maw" : "pocket",
				Tags = start ? PieceTag.Start : PieceTag.Room,
				Width = width,
				Height = rise,
				Sockets = sockets,
				Marks = marks,
				SpawnX = 10,
				SpawnY = rise,
				Stamp = (grid, ox, oy, used) => {
					grid.Dome(ox, oy + rise, width, rise, RiftGrid.Air);
					grid.Fill(ox, oy + rise - Thick, width, Thick, RiftGrid.Air);
				}
			};
		}

		/// <summary>
		/// A shaft to drop through. In at the top on either side, out along its
		/// floor. Ledges up alternate walls let a player climb back.
		/// </summary>
		private static RiftPiece Pit(Random rand)
		{
			const int width = 26;
			int depth = rand.Next(44, 61);
			return new RiftPiece {
				Name = "pit",
				Tags = PieceTag.Room,
				Width = width,
				Height = depth,
				Sockets = new[] {
					new RiftSocket(0, depth - Thick / 2.0, 180, Thick),
					new RiftSocket(width, depth - Thick / 2.0, 0, Thick),
					new RiftSocket(0, Thick / 2.0, 180, Thick),
					new RiftSocket(width, Thick / 2.0, 0, Thick)
				},
				Marks = new[] { new RiftMark(width / 2, depth, KindCorruptor) },
				Stamp = (grid, ox, oy, used) => {
					grid.Fill(ox, oy, width, depth, RiftGrid.Air);
					bool left = true;
					for (int y = depth - RiftConnector.RungPitch; y > 2; y -= RiftConnector.RungPitch) {
						int x0 = left ? ox + 1 : ox + width - 13;
						grid.Line(x0, x0 + 12, oy + y, RiftGrid.Ledge);
						left = !left;
					}
				}
			};
		}

		/// <summary>
		/// The eye's chamber. Four bands of ledges, and the way in high on one
		/// side, so the fight is below the arrival.
		/// </summary>
		private static RiftPiece Chamber(Random rand)
		{
			int width = rand.Next(120, 169);
			int height = rand.Next(72, 97);
			bool fromLeft = rand.Next(2) == 0;
			double[,] bars = {
				{ 0.30, 0.08, 0.92 },
				{ 0.48, 0.06, 0.40 },
				{ 0.48, 0.58, 0.94 },
				{ 0.66, 0.06, 0.42 },
				{ 0.66, 0.54, 0.94 },
				{ 0.84, 0.08, 0.90 }
			};

			return new RiftPiece {
				Name = "chamber",
				Tags = PieceTag.Boss,
				Width = width,
				Height = height,
				Margin = RiftMeasures.Buffer,
				Sockets = new[] {
					fromLeft
						? new RiftSocket(0, Thick / 2.0 + 2, 180, Thick, SocketRole.Entry)
						: new RiftSocket(width, Thick / 2.0 + 2, 0, Thick, SocketRole.Entry)
				},
				Arena = new RiftArena {
					Left = 0,
					Top = 0,
					Right = width,
					Bottom = height,
					ArenaX = width / 2,
					ArenaY = 10,
					FeetX = -1,
					PortalX = width / 2,
					PortalY = height - 3
				},
				Stamp = (grid, ox, oy, used) => {
					grid.Fill(ox, oy, width, height, RiftGrid.Boss);
					for (int i = 0; i < bars.GetLength(0); i++) {
						int y = oy + (int)Math.Floor(height * bars[i, 0]);
						int x0 = ox + (int)Math.Floor(width * bars[i, 1]);
						int x1 = ox + (int)Math.Ceiling(width * bars[i, 2]);
						grid.Line(x0, x1, y, RiftGrid.Ledge);
					}
				}
			};
		}

		/// <summary>A small dead end with one enemy, opening to the left or the right.</summary>
		private static RiftPiece Hollow(Random rand)
		{
			const int width = 26;
			const int rise = 16;
			bool left = rand.Next(2) == 0;
			double mouth = rise - Thick / 2.0;
			return new RiftPiece {
				Name = "hollow",
				Tags = PieceTag.DeadEnd,
				Width = width,
				Height = rise,
				Sockets = new[] {
					left
						? new RiftSocket(0, mouth, 180, Thick, SocketRole.Entry)
						: new RiftSocket(width, mouth, 0, Thick, SocketRole.Entry)
				},
				Marks = new[] { new RiftMark(width / 2, rise, rand.Next(3)) },
				Stamp = (grid, ox, oy, used) => {
					grid.Dome(ox, oy + rise, width, rise, RiftGrid.Air);
					grid.Fill(ox, oy + rise - Thick, width, Thick, RiftGrid.Air);
				}
			};
		}
	}
}
