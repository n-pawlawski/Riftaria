using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Riftaria.Common.Bosses;
using Riftaria.Common.Players;

namespace Riftaria.Content.NPCs
{
	/// <summary>
	/// Experience for every player who hit the NPC before it died. Adds a boss
	/// spawned give nothing, so a boss cannot be farmed through its summons.
	/// </summary>
	public class RiftExperience : GlobalNPC
	{
		public const int MobExperience = 10;

		public const int BossExperience = 250;

		public override void OnKill(NPC npc)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient || !GivesExperience(npc)) {
				return;
			}

			int amount = IsBoss(npc) ? BossExperience : MobExperience;
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];
				if (player.active && npc.playerInteraction[i]) {
					player.GetModPlayer<RiftLevelPlayer>().AddExperience(amount);
				}
			}
		}

		private static bool GivesExperience(NPC npc)
		{
			if (npc.friendly || npc.townNPC || npc.CountsAsACritter || npc.SpawnedFromStatue || npc.lifeMax <= 5) {
				return false;
			}

			return !(npc.TryGetGlobalNPC(out BossSpawnedNpc spawned) && spawned.FromBoss);
		}

		private static bool IsBoss(NPC npc)
		{
			return npc.boss
				|| RiftBossRegistry.TryGet(npc.type, out _)
				|| npc.type == ModContent.NPCType<RiftWarden>();
		}
	}
}
