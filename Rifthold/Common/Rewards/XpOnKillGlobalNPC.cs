using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Rifthold.Common.Players;
using Rifthold.Common.Systems;

namespace Rifthold.Common.Rewards
{
	/// <summary>Per-kill experience, awarded only inside rifts.</summary>
	public class XpOnKillGlobalNPC : GlobalNPC
	{
		/// <summary>Everyone this close to the kill shares in it.</summary>
		private const float ShareRange = 2400f;

		public override void OnKill(NPC npc)
		{
			if (!RiftGateway.InRift || npc.friendly || npc.townNPC) {
				return;
			}

			// Statue and critter kills should not be a route to levelling.
			if (npc.SpawnedFromStatue || npc.lifeMax <= 5 || npc.damage <= 0) {
				return;
			}

			long xp = (npc.lifeMax / 8) + npc.damage;
			if (npc.boss) {
				xp *= 3;
			}

			if (xp <= 0) {
				return;
			}

			// Shared rather than attributed to the last hit, which avoids losing
			// experience to kills finished by traps, lava or another player's minion.
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];
				if (!player.active || player.dead) {
					continue;
				}

				if (Vector2.Distance(player.Center, npc.Center) <= ShareRange) {
					RiftholdPlayer.Get(player).GrantXp(xp);
				}
			}
		}
	}
}
