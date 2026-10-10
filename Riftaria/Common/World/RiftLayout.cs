using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Riftaria.Common.World.Pieces;

namespace Riftaria.Common.World
{
	/// <summary>
	/// Geometry of a generated rift. Every tier is snapped together from its
	/// biome's pieces; see <see cref="RiftPlans"/>.
	/// </summary>
	public static class RiftLayout
	{
		public const int CircleHeight = RiftMeasures.CircleHeight;

		private static RiftPiecePlan _plan;

		/// <summary>Set and rolled before the subworld is created, because the world size is its grid.</summary>
		public static RiftPiecePlan Plan {
			get => _plan ?? RiftPlans.Forest;
			set => _plan = value;
		}

		public static int Height => Plan.RequiredHeight();

		/// <summary>Sealed band around the edge of the subworld.</summary>
		public const int Border = RiftMeasures.Border;

		/// <summary>
		/// Clear height of the rectangle under the circles. A player fits, and so
		/// does the doubled zombie. Circles rise above this; they do not replace it.
		/// </summary>
		public const int PassageHeight = RiftMeasures.PassageHeight;

		/// <summary>
		/// Wide enough for the plan rolled at entry, and no further.
		/// Read when the subworld is created.
		/// </summary>
		public static int Width => Plan.RequiredWidth();

		public const ushort ShellTile = TileID.LihzahrdBrick;
		public const ushort FloorTile = TileID.Stone;
		public const ushort AccentTile = TileID.Granite;
		public const ushort BackWall = WallID.Granite;

		/// <summary>Row the player and every enemy stand on.</summary>
		public static int FloorY => Height / 2;

		public static int SpawnX => Border + 40;

		/// <summary>Centre of the arena at the far end, set during generation.</summary>
		public static Point ArenaCenter { get; internal set; }

		/// <summary>
		/// Tile under the objective's feet. Negative X means spawn at
		/// <see cref="ArenaCenter"/> instead.
		/// </summary>
		public static int ObjectiveFeetX { get; internal set; } = -1;

		public static int ObjectiveFeetY { get; internal set; }

		/// <summary>
		/// Boss chamber, in tiles. The eye waits to spawn until a player is inside.
		/// Left is negative when this rift has no chamber.
		/// </summary>
		public static int ObjectiveRoomLeft { get; internal set; } = -1;

		public static int ObjectiveRoomTop { get; internal set; }

		public static int ObjectiveRoomRight { get; internal set; }

		public static int ObjectiveRoomBottom { get; internal set; }

		/// <summary>Whether the objective has been spawned for this run.</summary>
		public static bool ObjectiveSpawned { get; internal set; }

		/// <summary>NPC type placed at <see cref="ArenaCenter"/>. Zero means the zombie.</summary>
		public static int ObjectiveNpc { get; internal set; }

		/// <summary>The spawned objective. Killing that NPC clears the rift.</summary>
		public static int ObjectiveWho { get; internal set; } = -1;

		/// <summary>Where the exit portal appears after the boss dies. Negative X means unset.</summary>
		public static int PortalX { get; internal set; } = -1;

		public static int PortalY { get; internal set; }

		/// <summary>Zombies and slimes standing in the circles before the objective.</summary>
		public static List<RiftPathEnemy> PathEnemies { get; } = new();

		public static void Reset()
		{
			ArenaCenter = Point.Zero;
			ObjectiveFeetX = -1;
			ObjectiveFeetY = 0;
			ObjectiveRoomLeft = -1;
			ObjectiveSpawned = false;
			ObjectiveNpc = 0;
			ObjectiveWho = -1;
			PortalX = -1;
			PortalY = 0;
			_plan = null;
			PathEnemies.Clear();
			RiftPlans.ClearAll();
		}

		public static void SetTile(int x, int y, ushort type)
		{
			if (!WorldGen.InWorld(x, y)) {
				return;
			}

			Tile tile = Main.tile[x, y];
			tile.HasTile = true;
			tile.TileType = type;
			tile.Slope = SlopeType.Solid;
			tile.IsHalfBlock = false;
			tile.LiquidAmount = 0;
		}

		public static void Carve(int x, int y, ushort wall = BackWall)
		{
			if (!WorldGen.InWorld(x, y)) {
				return;
			}

			Tile tile = Main.tile[x, y];
			tile.HasTile = false;
			tile.LiquidAmount = 0;
			tile.WallType = wall;
		}
	}

	/// <summary>One enemy placed on the path. Spawned when a player walks up to it.</summary>
	public sealed class RiftPathEnemy
	{
		public Point Tile { get; init; }

		public int NpcType { get; init; }

		public bool Spawned { get; set; }
	}
}
