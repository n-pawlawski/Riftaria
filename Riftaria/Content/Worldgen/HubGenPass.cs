using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using Riftaria.Common.World;

namespace Riftaria.Content.Worldgen
{
	/// <summary>
	/// Builds the Hold. Every vanilla pass is disabled before this runs, so this
	/// starts from an empty tilemap and is responsible for the entire world.
	/// </summary>
	public class HubGenPass : GenPass
	{
		public HubGenPass() : base("Riftaria Hold", 100f) { }

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Raising the Hold";

			int floorY = HubLayout.FloorY;
			int ceilingY = HubLayout.CeilingY;
			int left = HubLayout.Left;
			int right = HubLayout.Right;

			// The stone ground is the surface. The platform is built far above it so the
			// sky, not the forest backdrop, sits behind the line.
			Main.worldSurface = floorY + 20;
			Main.rockLayer = Main.maxTilesY - 40;

			FillBedrock(progress, floorY);
			CarveInterior(left, right, ceilingY, floorY);
			SealShell(left, right, ceilingY, floorY);
			CastleFront.Build(floorY);
			Main.spawnTileX = CastleFront.SpawnX;
			Main.spawnTileY = CastleFront.SpawnY;
			LightInterior(left, right, floorY);
			FrameHold(left, right, floorY);

			progress.Set(1f);
		}

		/// <summary>Everything at or below the floor becomes solid shell.</summary>
		private static void FillBedrock(GenerationProgress progress, int floorY)
		{
			float total = Main.maxTilesX;

			for (int x = 0; x < Main.maxTilesX; x++) {
				progress.Set(x / total * 0.7f);

				for (int y = 0; y < Main.maxTilesY; y++) {
					if (y < floorY) {
						HubLayout.Clear(x, y);
						continue;
					}

					// A band of floor tile under the Hold reads as a built surface;
					// the rest is shell nobody will ever see.
					bool underHold = x >= HubLayout.Left - HubLayout.ShellThickness
						&& x <= HubLayout.Right + HubLayout.ShellThickness
						&& y < floorY + 4;

					HubLayout.SetTile(x, y, underHold ? HubLayout.FloorTile : HubLayout.ShellTile);
				}
			}
		}

		private static void CarveInterior(int left, int right, int ceilingY, int floorY)
		{
			for (int x = left; x <= right; x++) {
				for (int y = ceilingY + 1; y < floorY; y++) {
					HubLayout.Clear(x, y);
				}
			}
		}

		/// <summary>Side walls only. The top stays open so the sky is visible.</summary>
		private static void SealShell(int left, int right, int ceilingY, int floorY)
		{
			int thickness = HubLayout.ShellThickness;

			for (int y = ceilingY - thickness; y <= floorY; y++) {
				for (int offset = 1; offset <= thickness; offset++) {
					HubLayout.SetTile(left - offset, y, HubLayout.ShellTile);
					HubLayout.SetTile(right + offset, y, HubLayout.ShellTile);
				}
			}
		}

		/// <summary>
		/// Torches on the back wall, near the floor. The Hold is hundreds of tiles
		/// wide, so the lights are spaced across it rather than clustered at spawn.
		/// </summary>
		private static void LightInterior(int left, int right, int floorY)
		{
			const int spacing = 12;
			int eyeLine = floorY - 4;
			int highLine = floorY - 12;

			for (int x = left + 6; x <= right - 6; x += spacing) {
				if (x >= CastleFront.MinX && x <= CastleFront.MaxX) {
					continue;
				}

				PlaceLight(x, eyeLine, floorY);
				int highX = x + (spacing / 2);
				if (highX < CastleFront.MinX || highX > CastleFront.MaxX) {
					PlaceLight(highX, highLine, floorY);
				}
			}
		}

		/// <summary>
		/// Frames the platform and the lit floor before the player sees them.
		/// An unframed chest, gate, or torch draws as a square for a moment,
		/// then the real sprite replaces it.
		/// </summary>
		private static void FrameHold(int left, int right, int floorY)
		{
			bool generating = WorldGen.gen;
			WorldGen.gen = false;

			try {
				FrameBox(CastleFront.MinX - 2, CastleFront.SpawnY - 8, CastleFront.MaxX + 2, CastleFront.SpawnY + 2);
				FrameBox(left, floorY - 16, right, floorY + 2);
			}
			finally {
				WorldGen.gen = generating;
			}
		}

		private static void FrameBox(int x0, int y0, int x1, int y1)
		{
			for (int x = x0; x <= x1; x++) {
				for (int y = y0; y <= y1; y++) {
					if (!WorldGen.InWorld(x, y)) {
						continue;
					}

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

		/// <summary>A wall torch, or a candle on the floor if the torch cannot anchor.</summary>
		private static void PlaceLight(int x, int y, int floorY)
		{
			if (!HubLayout.Contains(x, y)) {
				return;
			}

			if (WorldGen.PlaceTile(x, y, TileID.Torches, mute: true)) {
				return;
			}

			WorldGen.PlaceTile(x, floorY - 1, TileID.Candles, mute: true);
		}
	}
}
