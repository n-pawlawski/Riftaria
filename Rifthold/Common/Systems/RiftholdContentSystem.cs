using Terraria.ModLoader;
using Rifthold.Common.Maps;
using Rifthold.Common.Progression;
using Rifthold.Common.World;
using Rifthold.Content.Tiles;

namespace Rifthold.Common.Systems
{
	/// <summary>Populates the static registries once content ids are known.</summary>
	public class RiftholdContentSystem : ModSystem
	{
		public override void PostSetupContent()
		{
			StatTree.LoadDefaults();
			RiftTierRegistry.LoadDefaults();

			// The only thing a player is allowed to build. Everything else, including
			// every vanilla block, stays locked for the MVP.
			WorldEditPolicy.AllowPlacement(ModContent.TileType<MapDeviceTile>());
		}
	}
}
