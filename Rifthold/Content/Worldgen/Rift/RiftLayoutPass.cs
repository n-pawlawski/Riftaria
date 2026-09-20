using Microsoft.Xna.Framework;
using Terraria;
using Terraria.IO;
using Terraria.WorldBuilding;
using Rifthold.Common.Maps;
using Rifthold.Common.World;

namespace Rifthold.Content.Worldgen.Rift
{
	/// <summary>
	/// Carves the rift: a chain of rooms on a single flat floor running from the
	/// spawn on the left to the Warden's arena on the right.
	/// <para>
	/// Connectivity is guaranteed by construction rather than validated afterwards.
	/// Every room is carved onto the same floor row and joined to its neighbour by
	/// a corridor on that row, so a walkable path always exists.
	/// </para>
	/// </summary>
	public class RiftLayoutPass : GenPass
	{
		public RiftLayoutPass() : base("Rift Layout", 100f) { }

		private const int CorridorHeight = 7;
		private const int ArenaWidth = 120;
		private const int ArenaHeight = 44;

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Carving the rift";

			RiftTier tier = RiftRunState.ActiveTierData;
			int floorY = RiftLayout.FloorY;

			// Subworld Library strips space, oceans and the underworld, but these
			// still control lighting and background, so pin them out of the way.
			Main.worldSurface = RiftLayout.Height - 42;
			Main.rockLayer = RiftLayout.Height;
			Main.spawnTileX = RiftLayout.SpawnX;
			Main.spawnTileY = floorY;

			FillSolid(progress);

			int roomCount = Mathi.Clamp(tier.RoomCount, 3, 16);
			int usableStart = RiftLayout.Border + 20;
			int usableEnd = RiftLayout.Width - RiftLayout.Border - ArenaWidth - 40;
			int span = Mathi.Max(usableEnd - usableStart, 200);
			int step = span / roomCount;

			int previousX = RiftLayout.SpawnX;

			for (int i = 0; i < roomCount; i++) {
				int roomCenterX = usableStart + (step * i) + (step / 2);
				int roomWidth = WorldGen.genRand.Next(36, 70);
				int roomHeight = WorldGen.genRand.Next(14, 30);

				CarveRoom(roomCenterX, floorY, roomWidth, roomHeight);
				CarveCorridor(previousX, roomCenterX, floorY);
				Decorate(roomCenterX, floorY, roomWidth, roomHeight);

				previousX = roomCenterX;
				progress.Set(0.7f + (0.25f * (i / (float)roomCount)));
			}

			int arenaX = RiftLayout.Width - RiftLayout.Border - (ArenaWidth / 2) - 20;
			CarveRoom(arenaX, floorY, ArenaWidth, ArenaHeight);
			CarveCorridor(previousX, arenaX, floorY);

			RiftLayout.ArenaCenter = new Point(arenaX, floorY - (ArenaHeight / 2));
			RiftLayout.WardenSpawned = false;

			CarveRoom(RiftLayout.SpawnX, floorY, 40, 18);

			progress.Set(1f);
		}

		private static void FillSolid(GenerationProgress progress)
		{
			for (int x = 0; x < Main.maxTilesX; x++) {
				progress.Set(x / (float)Main.maxTilesX * 0.7f);

				for (int y = 0; y < Main.maxTilesY; y++) {
					bool edge = x < RiftLayout.Border
						|| y < RiftLayout.Border
						|| x >= Main.maxTilesX - RiftLayout.Border
						|| y >= Main.maxTilesY - RiftLayout.Border;

					RiftLayout.SetTile(x, y, edge ? RiftLayout.ShellTile : RiftLayout.FloorTile);
				}
			}
		}

		/// <summary>Hollows a room that sits on the shared floor row.</summary>
		private static void CarveRoom(int centerX, int floorY, int width, int height)
		{
			int left = centerX - (width / 2);
			int right = centerX + (width / 2);

			for (int x = left; x <= right; x++) {
				for (int y = floorY - height; y < floorY; y++) {
					RiftLayout.Carve(x, y);
				}
			}
		}

		private static void CarveCorridor(int fromX, int toX, int floorY)
		{
			int left = Mathi.Min(fromX, toX);
			int right = Mathi.Max(fromX, toX);

			for (int x = left; x <= right; x++) {
				for (int y = floorY - CorridorHeight; y < floorY; y++) {
					RiftLayout.Carve(x, y);
				}
			}
		}

		/// <summary>Pillars and ceiling teeth, purely so rooms read as distinct.</summary>
		private static void Decorate(int centerX, int floorY, int width, int height)
		{
			int left = centerX - (width / 2);
			int right = centerX + (width / 2);

			for (int x = left; x <= right; x++) {
				if (WorldGen.genRand.NextBool(9)) {
					int stack = WorldGen.genRand.Next(2, Mathi.Max(3, height / 3));
					for (int y = floorY - stack; y < floorY; y++) {
						RiftLayout.SetTile(x, y, RiftLayout.AccentTile);
					}
				}
				else if (WorldGen.genRand.NextBool(11)) {
					int stack = WorldGen.genRand.Next(2, Mathi.Max(3, height / 4));
					for (int i = 0; i < stack; i++) {
						RiftLayout.SetTile(x, floorY - height + i, RiftLayout.AccentTile);
					}
				}
			}
		}

		/// <summary>
		/// Local integer helpers. Terraria's Utils.Clamp is generic and MathHelper
		/// works in floats; these keep the layout maths readable.
		/// </summary>
		private static class Mathi
		{
			public static int Clamp(int value, int min, int max)
				=> value < min ? min : value > max ? max : value;

			public static int Min(int a, int b) => a < b ? a : b;

			public static int Max(int a, int b) => a > b ? a : b;
		}
	}
}
