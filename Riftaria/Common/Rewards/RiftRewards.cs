using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Riftaria.Common.Maps;
using Riftaria.Common.Players;
using Riftaria.Content.Items;

namespace Riftaria.Common.Rewards
{
	/// <summary>
	/// Completion payout. With ore progression gone this is the only way gear and
	/// sigils enter the game, so it is also what keeps the loop closed.
	/// </summary>
	public static class RiftRewards
	{
		/// <summary>Chance a clear yields the tier's weapon.</summary>
		private const float WeaponChance = 0.35f;

		public static void GrantToEveryoneInRift(int tier)
		{
			RiftTier data = RiftTierRegistry.Get(tier);

			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];
				if (player.active && !player.dead) {
					Grant(player, data);
				}
			}
		}

		private static void Grant(Player player, RiftTier data)
		{
			RiftariaPlayer.Get(player).GrantXp(data.CompletionXp);

			IEntitySource source = player.GetSource_GiftOrReward("RiftClear");

			if (data.EssenceReward > 0) {
				player.QuickSpawnItem(source, ModContent.ItemType<RiftEssence>(), data.EssenceReward);
			}

			if (Main.rand.NextFloat() < WeaponChance) {
				player.QuickSpawnItem(source, ModContent.ItemType<RiftwovenBlade>());
			}

			// The rung that makes this a ladder rather than a treadmill.
			int nextTier = data.Tier + 1;
			if (data.NextTierSigilChance > 0f
				&& RiftTierRegistry.Exists(nextTier)
				&& Main.rand.NextFloat() < data.NextTierSigilChance) {
				player.QuickSpawnItem(source, RiftSigil.WithTier(nextTier));
			}
		}
	}
}
