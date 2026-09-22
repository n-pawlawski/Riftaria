using Terraria.ModLoader;
using Riftaria.Common.Maps;
using Riftaria.Common.World;
using Riftaria.Content.Tiles;

namespace Riftaria.Common.Systems
{
	/// <summary>Populates the static registries once content ids are known.</summary>
	public class RiftariaContentSystem : ModSystem
	{
		public override void PostSetupContent()
		{
			RiftTierRegistry.LoadDefaults();

			// The only thing a player is allowed to build. Everything else, including
			// every vanilla block, stays locked. Placement implies breakability, or a
			// misplaced Gate would be stuck where it landed.
			WorldEditPolicy.AllowPlacement(ModContent.TileType<MapDeviceTile>());
			WorldEditPolicy.AllowMining(ModContent.TileType<MapDeviceTile>());
		}
	}
}
