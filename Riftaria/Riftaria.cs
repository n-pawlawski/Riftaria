using Terraria.ModLoader;
using Riftaria.Common.Maps;
using Riftaria.Common.World;

namespace Riftaria
{
	// HandlePacket is not implemented yet. Multiplayer is out of scope for the first
	// playable, and every send site is guarded by a netMode check.
	public class Riftaria : Mod
	{
		public static Riftaria Instance => ModContent.GetInstance<Riftaria>();

		public override void Unload()
		{
			// Static registries survive an assembly reload unless cleared explicitly.
			WorldEditPolicy.Reset();
			RiftTierRegistry.Reset();
			RiftRunState.Reset();
			RiftLayout.Reset();
		}
	}
}
