using Terraria.ModLoader;
using Rifthold.Common.Maps;
using Rifthold.Common.Progression;
using Rifthold.Common.World;

namespace Rifthold
{
	// Split across files: see Rifthold.Networking.cs for packet handling.
	public partial class Rifthold : Mod
	{
		public static Rifthold Instance => ModContent.GetInstance<Rifthold>();

		public override void Unload()
		{
			// Static registries survive an assembly reload unless cleared explicitly.
			WorldEditPolicy.Reset();
			StatTree.Reset();
			RiftTierRegistry.Reset();
			RiftRunState.Reset();
		}
	}
}
