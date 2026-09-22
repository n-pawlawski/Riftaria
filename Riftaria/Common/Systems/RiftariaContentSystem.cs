using Terraria.ModLoader;
using Riftaria.Common.Maps;
using Riftaria.Common.Progression;
using Riftaria.Common.World;
using Riftaria.Content.Tiles;

namespace Riftaria.Common.Systems
{
	/// <summary>Populates the static registries once content ids are known.</summary>
	public class RiftariaContentSystem : ModSystem
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
