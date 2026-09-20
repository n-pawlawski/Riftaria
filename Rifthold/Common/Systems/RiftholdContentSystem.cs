using Terraria.ModLoader;
using Rifthold.Common.Maps;
using Rifthold.Common.Progression;

namespace Rifthold.Common.Systems
{
	/// <summary>Populates the static registries once content ids are known.</summary>
	public class RiftholdContentSystem : ModSystem
	{
		public override void PostSetupContent()
		{
			StatTree.LoadDefaults();
			RiftTierRegistry.LoadDefaults();
		}
	}
}
