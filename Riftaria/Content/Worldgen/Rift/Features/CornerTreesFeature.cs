using System;
using Terraria;
using Riftaria.Common.World;
using Riftaria.Content.Worldgen.Rift;

namespace Riftaria.Content.Worldgen.Rift.Features
{
	/// <summary>
	/// Living-wood trees on grass ledges in the shoulders of a dome. The ledge sits
	/// six tiles above the highest ground under it, which clears a player, so
	/// the tree never stands on the path.
	/// </summary>
	internal sealed class CornerTreesFeature : IBiomeFeature
	{
		private const int Clearance = 6;
		private const int Trunk = 3;
		private const int Canopy = 2;

		public void Apply(BiomeRoom room, BiomeProfile profile)
		{
			if (profile.Accent == 0 || profile.Canopy == 0) {
				return;
			}

			int half = (room.Right - room.Left) / 2;
			int shoulder = half / 3;
			if (shoulder < 5) {
				shoulder = 5;
			}

			TryCorner(room, profile, room.Center - half + shoulder);
			TryCorner(room, profile, room.Center + half - shoulder);
		}

		private static void TryCorner(BiomeRoom room, BiomeProfile profile, int trunkX)
		{
			if (RiftGen.Rand.NextBool(4)) {
				return;
			}

			int shelfLeft = trunkX - 2;
			int shelfRight = trunkX + 2;
			if (shelfLeft < room.Left || shelfRight > room.Right) {
				return;
			}

			int peak = int.MaxValue;
			for (int x = shelfLeft; x <= shelfRight; x++) {
				int floor = room.FloorAt(x);
				if (floor < peak) {
					peak = floor;
				}
			}

			int shelfY = peak - Clearance - 1;
			int canopyTop = shelfY - Trunk - Canopy;
			if (canopyTop < room.CeilingAt(trunkX) || shelfY >= peak) {
				return;
			}

			for (int x = shelfLeft; x <= shelfRight; x++) {
				if (!WorldGen.InWorld(x, shelfY) || Main.tile[x, shelfY].HasTile) {
					return;
				}
			}

			for (int dy = 1; dy <= Canopy; dy++) {
				for (int dx = -2; dx <= 2; dx++) {
					if (Math.Abs(dx) == 2 && dy == 1) {
						continue;
					}

					int y = shelfY - Trunk - dy;
					if (y < room.CeilingAt(trunkX + dx)) {
						continue;
					}

					RiftLayout.SetTile(trunkX + dx, y, profile.Canopy);
				}
			}

			for (int i = 1; i <= Trunk; i++) {
				RiftLayout.SetTile(trunkX, shelfY - i, profile.Accent);
			}

			for (int x = shelfLeft; x <= shelfRight; x++) {
				RiftLayout.SetTile(x, shelfY, profile.Surface);
			}
		}
	}
}
