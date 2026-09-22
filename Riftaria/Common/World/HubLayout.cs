using Terraria;
using Terraria.ID;

namespace Riftaria.Common.World
{
	/// <summary>
	/// Geometry of the Hold, shared by the generation pass and the growth system so
	/// the two never disagree about where the floor is.
	/// </summary>
	public static class HubLayout
	{
		/// <summary>Playable width of the Hold in tiles.</summary>
		public const int InteriorWidth = 360;

		/// <summary>Playable height of the Hold in tiles.</summary>
		public const int InteriorHeight = 90;

		/// <summary>Thickness of the sealed shell around the interior.</summary>
		public const int ShellThickness = 6;

		public const ushort ShellTile = TileID.LihzahrdBrick;
		public const ushort FloorTile = TileID.GrayBrick;
		public const ushort PlatformTile = TileID.Platforms;
		public const ushort BackWall = WallID.GrayBrick;

		/// <summary>Tile row the player stands on.</summary>
		public static int FloorY => (int)(Main.maxTilesY * 0.32f);

		public static int CenterX => Main.maxTilesX / 2;

		public static int Left => CenterX - (InteriorWidth / 2);

		public static int Right => CenterX + (InteriorWidth / 2);

		/// <summary>Topmost air row inside the Hold.</summary>
		public static int CeilingY => FloorY - InteriorHeight;

		public static bool Contains(int x, int y)
			=> x >= Left && x <= Right && y > CeilingY && y < FloorY;

		/// <summary>Writes a tile directly. Fast enough to fill a whole world with.</summary>
		public static void SetTile(int x, int y, ushort type, bool active = true)
		{
			if (!WorldGen.InWorld(x, y)) {
				return;
			}

			Tile tile = Main.tile[x, y];
			tile.HasTile = active;
			tile.TileType = type;
			tile.Slope = SlopeType.Solid;
			tile.IsHalfBlock = false;
			tile.LiquidAmount = 0;
		}

		public static void SetWall(int x, int y, ushort wall)
		{
			if (!WorldGen.InWorld(x, y)) {
				return;
			}

			Main.tile[x, y].WallType = wall;
		}

		public static void Clear(int x, int y)
		{
			if (!WorldGen.InWorld(x, y)) {
				return;
			}

			Tile tile = Main.tile[x, y];
			tile.HasTile = false;
			tile.LiquidAmount = 0;
		}
	}
}
