using Terraria;

namespace Riftaria.Common.Bosses
{
	/// <summary>
	/// Plays the motions registered on a boss, one after another, then repeats.
	/// The NPC keeps the index in <c>ai[0]</c> and the ticks spent in <c>ai[1]</c>.
	/// </summary>
	public static class BossMotionRunner
	{
		public static void Step(NPC npc)
		{
			if (!RiftBossRegistry.TryGet(npc.type, out RiftBoss boss) || boss.Motions.Count == 0) {
				return;
			}

			int index = (int)npc.ai[0];
			if (index < 0 || index >= boss.Motions.Count) {
				index = 0;
				npc.ai[0] = 0;
			}

			npc.ai[1]++;
			if (!boss.Motions[index].Step(npc)) {
				return;
			}

			npc.ai[0] = (index + 1) % boss.Motions.Count;
			npc.ai[1] = 0f;
			npc.ai[2] = 0f;
			npc.ai[3] = 0f;
			npc.rotation = 0f;
			npc.netUpdate = true;
		}
	}
}
