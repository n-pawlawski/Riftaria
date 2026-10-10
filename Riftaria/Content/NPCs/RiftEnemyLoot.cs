using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Riftaria.Common.Drops;
using Riftaria.Common.Maps;
using Riftaria.Common.Systems;
using Riftaria.Common.World;
using Riftaria.Content.Items;

namespace Riftaria.Content.NPCs
{
	/// <summary>
	/// General loot from enemies killed inside a rift, rolled from the rift's
	/// loot plan. Bosses drop their own bag, and adds a boss spawned are
	/// removed without a drop.
	/// </summary>
	public class RiftEnemyLoot : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			if (!RiftGateway.InRift || Main.netMode == NetmodeID.MultiplayerClient || !DropsLoot(npc)) {
				return;
			}

			RiftLootPlan plan = RiftLootSystem.ActivePlan;
			LootPick pick = plan.RollGeneral(static max => Main.rand.Next(max));
			if (pick != null) {
				RiftLootDrops.SpawnRolled(npc.GetSource_Loot(), npc.getRect(), pick, RiftRunState.ActiveTier);
			}
		}

		private static bool DropsLoot(NPC npc)
		{
			if (npc.friendly || npc.townNPC || npc.CountsAsACritter || npc.SpawnedFromStatue || npc.lifeMax <= 5) {
				return false;
			}

			if (npc.boss || npc.whoAmI == RiftLayout.ObjectiveWho || npc.type == ModContent.NPCType<RiftWarden>()) {
				return false;
			}

			return !(npc.TryGetGlobalNPC(out BossSpawnedNpc spawned) && spawned.FromBoss);
		}
	}
}
