using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;
using Riftaria.Common.World;
using Riftaria.Common.World.Pieces;

namespace Riftaria.Content.Worldgen.Rift
{
	/// <summary>
	/// Paints an assembled rift in its biome's tiles. Every tier uses it; only
	/// the profile and the pieces differ.
	/// </summary>
	internal static class RiftPieceStamp
	{
		public static void Apply(GenerationProgress progress, BiomeProfile biome, RiftPiecePlan plan)
		{
			if (!plan.Rolled) {
				plan.Roll();
			}

			// Subworld Library strips space, oceans and the underworld, but these
			// still control lighting and background, so pin them out of the way.
			Main.worldSurface = RiftLayout.Height - 42;
			Main.rockLayer = RiftLayout.Height;

			int width = plan.Width;
			int height = plan.Height;
			Fill(progress, biome, width, height);
			Carve(plan, biome, width, height);
			Paint(plan, biome, width, height);
			if (biome.PaintFaces) {
				PaintFaces(plan, biome, width, height);
			}

			Light(plan, biome, width, height);
			Frame(width, height);
			SolidBlocks(width, height);

			RiftAssembly rift = plan.Assembly;
			Main.spawnTileX = rift.SpawnX;
			Main.spawnTileY = rift.SpawnY;
			RiftLayout.ArenaCenter = new Point(rift.ArenaX, rift.ArenaY);
			RiftLayout.ObjectiveFeetX = rift.FeetX;
			RiftLayout.ObjectiveFeetY = rift.FeetY;
			RiftLayout.ObjectiveRoomLeft = rift.RoomLeft;
			RiftLayout.ObjectiveRoomTop = rift.RoomTop;
			RiftLayout.ObjectiveRoomRight = rift.RoomRight;
			RiftLayout.ObjectiveRoomBottom = rift.RoomBottom;
			RiftLayout.PortalX = rift.PortalX;
			RiftLayout.PortalY = rift.PortalY;
			RiftLayout.ObjectiveSpawned = false;
			RiftLayout.ObjectiveWho = -1;
		}

		/// <summary>One enemy per mark. A mark's kind indexes <paramref name="kinds"/>.</summary>
		public static void PlaceEnemies(RiftPiecePlan plan, int[] kinds)
		{
			RiftLayout.PathEnemies.Clear();
			foreach (RiftMark mark in plan.Assembly.Marks) {
				if (mark.X <= 0 || mark.Y <= 0 || mark.X >= plan.Width || mark.Y >= plan.Height) {
					continue;
				}

				int type = mark.Kind >= 0 && mark.Kind < kinds.Length ? kinds[mark.Kind] : kinds[0];
				RiftLayout.PathEnemies.Add(new RiftPathEnemy {
					Tile = new Point(mark.X, mark.Y),
					NpcType = type
				});
			}
		}

		private static void Fill(GenerationProgress progress, BiomeProfile biome, int width, int height)
		{
			for (int x = 0; x < width; x++) {
				progress.Set(x / (float)width * 0.7f);
				for (int y = 0; y < height; y++) {
					ushort type = IsBorder(x, y, width, height) ? RiftLayout.ShellTile : biome.Shell;
					RiftLayout.SetTile(x, y, type);
				}
			}
		}

		private static void Carve(RiftPiecePlan plan, BiomeProfile biome, int width, int height)
		{
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					if (IsBorder(x, y, width, height) || !plan.Open(x, y)) {
						continue;
					}

					RiftLayout.Carve(x, y, biome.Wall);
				}
			}
		}

		private static void Paint(RiftPiecePlan plan, BiomeProfile biome, int width, int height)
		{
			for (int y = 1; y < height - 1; y++) {
				for (int x = 1; x < width - 1; x++) {
					if (IsBorder(x, y, width, height)) {
						continue;
					}

					if (plan.IsLedge(x, y)) {
						RiftLayout.Carve(x, y, biome.Wall);
						if (biome.LedgePlatforms) {
							WorldGen.PlaceTile(x, y, TileID.Platforms, true, true, -1, biome.PlatformStyle);
						}
						else {
							RiftLayout.SetTile(x, y, biome.Accent);
						}

						continue;
					}

					if (plan.IsPlatform(x, y)) {
						RiftLayout.Carve(x, y, biome.Wall);
						WorldGen.PlaceTile(x, y, TileID.Platforms, true, true, -1, biome.PlatformStyle);
						continue;
					}

					if (plan.Open(x, y) || !plan.Open(x, y - 1)) {
						continue;
					}

					RiftLayout.SetTile(x, y, biome.Surface);
					for (int depth = 1; depth <= biome.FillDepth; depth++) {
						int below = y + depth;
						if (below >= height || IsBorder(x, below, width, height) || !Main.tile[x, below].HasTile) {
							break;
						}

						if (plan.IsLedge(x, below) || plan.IsPlatform(x, below)) {
							break;
						}

						RiftLayout.SetTile(x, below, biome.Fill);
					}
				}
			}
		}

		/// <summary>
		/// Rock touching the air at its side or from above becomes fill, so a
		/// round room has an earthen face rather than the bare shell. Ground the
		/// walk stands on was painted already.
		/// </summary>
		private static void PaintFaces(RiftPiecePlan plan, BiomeProfile biome, int width, int height)
		{
			ushort ceiling = biome.GrassCeiling ? biome.Surface : biome.Fill;
			for (int y = 1; y < height - 1; y++) {
				for (int x = 1; x < width - 1; x++) {
					if (IsBorder(x, y, width, height) || plan.Cell(x, y) != RiftGrid.Rock || plan.Open(x, y - 1)) {
						continue;
					}

					if (plan.Open(x, y + 1)) {
						RiftLayout.SetTile(x, y, ceiling);
					}
					else if (plan.Open(x - 1, y) || plan.Open(x + 1, y)) {
						RiftLayout.SetTile(x, y, biome.Fill);
					}
				}
			}
		}

		/// <summary>A torch on every floor in every few columns.</summary>
		private static void Light(RiftPiecePlan plan, BiomeProfile biome, int width, int height)
		{
			for (int x = RiftLayout.Border + 4; x < width - RiftLayout.Border; x += biome.TorchSpacing) {
				for (int y = RiftLayout.Border; y < height - RiftLayout.Border - 1; y++) {
					if (!plan.Open(x, y) || plan.Open(x, y + 1)) {
						continue;
					}

					if (!Main.tile[x, y].HasTile) {
						WorldGen.PlaceTile(x, y, TileID.Torches, true, false, -1, 0);
					}
				}
			}
		}

		/// <summary>
		/// Writes the frame the game will use once the chunk is on screen.
		/// During generation those checks are skipped, so a tile keeps the full
		/// square until you walk up to it and the square is replaced.
		/// </summary>
		private static void Frame(int width, int height)
		{
			bool generating = WorldGen.gen;
			WorldGen.gen = false;
			try {
				int x0 = RiftLayout.Border;
				int y0 = RiftLayout.Border;
				int x1 = width - RiftLayout.Border - 1;
				int y1 = height - RiftLayout.Border - 1;
				for (int x = x0; x <= x1; x++) {
					for (int y = y0; y <= y1; y++) {
						Tile tile = Main.tile[x, y];
						if (tile.WallType > 0) {
							WorldGen.SquareWallFrame(x, y, true);
						}

						if (tile.HasTile) {
							WorldGen.SquareTileFrame(x, y, true);
						}
					}
				}
			}
			finally {
				WorldGen.gen = generating;
			}
		}

		/// <summary>
		/// Framing cuts a tile that touches air into a ramp, and a ramp draws as a
		/// triangle. The rest of the block disappears, which opens a hole under the
		/// walk. Full blocks stay as they were painted.
		/// </summary>
		private static void SolidBlocks(int width, int height)
		{
			for (int x = RiftLayout.Border; x < width - RiftLayout.Border; x++) {
				for (int y = RiftLayout.Border; y < height - RiftLayout.Border; y++) {
					Tile tile = Main.tile[x, y];
					if (!tile.HasTile) {
						continue;
					}

					tile.Slope = SlopeType.Solid;
					tile.IsHalfBlock = false;
				}
			}
		}

		private static bool IsBorder(int x, int y, int width, int height)
		{
			return x < RiftLayout.Border
				|| y < RiftLayout.Border
				|| x >= width - RiftLayout.Border
				|| y >= height - RiftLayout.Border;
		}
	}
}
