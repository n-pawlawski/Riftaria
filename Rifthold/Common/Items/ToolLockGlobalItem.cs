using Terraria;
using Terraria.ModLoader;
using Rifthold.Common.World;

namespace Rifthold.Common.Items
{
	/// <summary>
	/// Stops tools and placeables being used at all. The tile hooks already refuse
	/// the edit, but letting the swing play out reads as a bug rather than a rule,
	/// so the item is refused before it ever animates.
	/// </summary>
	public class ToolLockGlobalItem : GlobalItem
	{
		public override bool CanUseItem(Item item, Player player)
		{
			if (!WorldEditPolicy.MiningEnabled && IsDiggingTool(item)) {
				return false;
			}

			if (IsBlockedPlaceable(item)) {
				return false;
			}

			return base.CanUseItem(item, player);
		}

		private static bool IsDiggingTool(Item item) => item.pick > 0 || item.axe > 0 || item.hammer > 0;

		private static bool IsBlockedPlaceable(Item item)
		{
			if (item.createWall >= 0) {
				return !WorldEditPolicy.CanPlace();
			}

			if (item.createTile >= 0) {
				return !WorldEditPolicy.IsPlacementAllowed(item.createTile);
			}

			return false;
		}
	}
}
