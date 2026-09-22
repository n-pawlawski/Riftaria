using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Riftaria.Common.World
{
	/// <summary>
	/// Geometry of a generated rift. The floor is deliberately flat: with mining
	/// and placement disabled a player who cannot clear a ledge is stuck for good,
	/// so the MVP trades vertical variety for a layout that is always walkable.
	/// </summary>
	public static class RiftLayout
	{
		public const int Width = 1400;
		public const int Height = 350;

		/// <summary>Sealed band around the edge of the subworld.</summary>
		public const int Border = 24;

		public const ushort ShellTile = TileID.LihzahrdBrick;
		public const ushort FloorTile = TileID.Stone;
		public const ushort AccentTile = TileID.Granite;
		public const ushort BackWall = WallID.Granite;

		/// <summary>Row the player and every enemy stand on.</summary>
		public static int FloorY => Height / 2;

		public static int SpawnX => Border + 40;

		/// <summary>Centre of the arena at the far end, set during generation.</summary>
		public static Point ArenaCenter { get; internal set; }

		/// <summary>Whether the Rift Warden has been spawned for this run.</summary>
		public static bool WardenSpawned { get; internal set; }

		public static void Reset()
		{
			ArenaCenter = Point.Zero;
			WardenSpawned = false;
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
}
