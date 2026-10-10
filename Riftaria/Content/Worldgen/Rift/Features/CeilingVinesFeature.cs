using Terraria;
using Riftaria.Common.World;
using Riftaria.Content.Worldgen.Rift;

namespace Riftaria.Content.Worldgen.Rift.Features
{
	/// <summary>
	/// Vines from the ceiling. They are not solid, and they still stop a few
	/// tiles above the floor so a room does not turn into a curtain.
	/// </summary>
	internal sealed class CeilingVinesFeature : IBiomeFeature
	{
		private const int Headroom = 5;

		public void Apply(BiomeRoom room, BiomeProfile profile)
		{
			if (profile.Vine == 0) {
				return;
			}

			for (int x = room.Left + 1; x < room.Right; x++) {
				if (!RiftGen.Rand.NextBool(4)) {
					continue;
				}

				int floor = room.FloorAt(x);
				int ceiling = room.CeilingAt(x);
				int available = floor - ceiling - Headroom;
				if (available < 2) {
					continue;
				}

				int length = RiftGen.Rand.Next(2, Mathi.Min(available, 6) + 1);
				int anchor = ceiling - 1;
				// An overlapping circle may have already opened this row. Only the
				// real ceiling, which paint left solid, can hold a vine.
				if (!WorldGen.InWorld(x, anchor) || !Main.tile[x, anchor].HasTile) {
					continue;
				}

				for (int i = 0; i < length; i++) {
					int y = ceiling + i;
					if (!WorldGen.InWorld(x, y) || Main.tile[x, y].HasTile) {
						break;
					}

					if (!WorldGen.PlaceTile(x, y, profile.Vine, true, true)) {
						break;
					}
				}
			}
		}

		private static class Mathi
		{
			public static int Min(int a, int b) => a < b ? a : b;
		}
	}
}
