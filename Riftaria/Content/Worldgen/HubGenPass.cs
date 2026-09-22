using Terraria;
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

			// Surface sits just above the Hold so the sky renders behind it, and the
			// rock layer is pushed off the bottom so nothing treats this as a cavern.
			Main.worldSurface = ceilingY - 20;
			Main.rockLayer = Main.maxTilesY - 40;
			Main.spawnTileX = HubLayout.CenterX;
			Main.spawnTileY = floorY;

			FillBedrock(progress, floorY);
			CarveInterior(left, right, ceilingY, floorY);
			SealShell(left, right, ceilingY, floorY);

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
					HubLayout.SetWall(x, y, HubLayout.BackWall);
				}
			}
		}

		/// <summary>Walls and a ceiling, so the Hold cannot be flown out of.</summary>
		private static void SealShell(int left, int right, int ceilingY, int floorY)
		{
			int thickness = HubLayout.ShellThickness;

			for (int y = ceilingY - thickness; y <= floorY; y++) {
				for (int offset = 1; offset <= thickness; offset++) {
					HubLayout.SetTile(left - offset, y, HubLayout.ShellTile);
					HubLayout.SetTile(right + offset, y, HubLayout.ShellTile);
				}
			}

			for (int x = left - thickness; x <= right + thickness; x++) {
				for (int offset = 0; offset < thickness; offset++) {
					HubLayout.SetTile(x, ceilingY - offset, HubLayout.ShellTile);
				}
			}
		}
	}
}
