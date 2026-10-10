using Terraria;
using Riftaria.Common.World;
using Riftaria.Content.Worldgen.Rift;

namespace Riftaria.Content.Worldgen.Rift.Features
{
	/// <summary>
	/// Short accent spikes on the ceiling. They stop above head height, which
	/// replaces the old floor pillars that sat on the only walkable row.
	/// </summary>
	internal sealed class CeilingAccentFeature : IBiomeFeature
	{
		private const int Headroom = 6;

		public void Apply(BiomeRoom room, BiomeProfile profile)
		{
			if (profile.Accent == 0) {
				return;
			}

			for (int x = room.Left + 1; x < room.Right; x++) {
				if (!RiftGen.Rand.NextBool(11)) {
					continue;
				}

				int floor = room.FloorAt(x);
				int ceiling = room.CeilingAt(x);
				int anchor = ceiling - 1;
				if (!WorldGen.InWorld(x, anchor) || !Main.tile[x, anchor].HasTile) {
					continue;
				}

				int open = floor - ceiling;
				int available = open - Headroom;
				if (available < 2) {
					continue;
				}

				int cap = open / 4;
				if (cap < 3) {
					cap = 3;
				}

				if (cap > available) {
					cap = available;
				}

				int stack = RiftGen.Rand.Next(2, cap + 1);

				for (int i = 0; i < stack; i++) {
					RiftLayout.SetTile(x, ceiling + i, profile.Accent);
				}
			}
		}
	}
}
