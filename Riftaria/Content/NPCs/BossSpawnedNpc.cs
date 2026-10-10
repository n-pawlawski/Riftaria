using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Riftaria.Content.NPCs
{
	/// <summary>
	/// Remembers which boss spawned this NPC. Killing that boss removes it
	/// without a drop.
	/// </summary>
	public class BossSpawnedNpc : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		private int parentWho = -1;

		public bool FromBoss => parentWho >= 0;

		public override void OnSpawn(NPC npc, IEntitySource source)
		{
			if (source is EntitySource_Parent parent && parent.Entity is NPC owner) {
				parentWho = owner.whoAmI;
			}
		}

		public static int CountOf(NPC parent)
		{
			int who = parent.whoAmI;
			int count = 0;
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || i == who) {
					continue;
				}

				if (npc.TryGetGlobalNPC(out BossSpawnedNpc spawned) && spawned.parentWho == who) {
					count++;
				}
			}

			return count;
		}

		public static void RemoveOf(NPC parent)
		{
			int who = parent.whoAmI;
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || i == who) {
					continue;
				}

				if (!npc.TryGetGlobalNPC(out BossSpawnedNpc spawned) || spawned.parentWho != who) {
					continue;
				}

				npc.life = 0;
				npc.active = false;
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendData(MessageID.SyncNPC, number: i);
				}
			}
		}
	}
}
