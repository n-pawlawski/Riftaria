using Terraria;
using Terraria.ID;
using Riftaria.Content.NPCs;

namespace Riftaria.Common.Bosses
{
	/// <summary>
	/// The spawn. Calls in zombies around the fight, then holds still.
	/// Skips the call once enough of them are still up.
	/// </summary>
	public sealed class Rally : IBossMotion
	{
		private readonly SpawnAroundHorizontally spawn;

		private readonly int ticks;

		private readonly int maxAlive;

		public Rally(int npcType, int count, int ticks, int maxAlive)
		{
			spawn = new SpawnAroundHorizontally(npcType, count);
			this.ticks = ticks;
			this.maxAlive = maxAlive;
		}

		public bool Step(NPC npc)
		{
			npc.noGravity = false;
			npc.velocity.X = 0f;
			npc.TargetClosest(faceTarget: true);
			npc.spriteDirection = npc.direction;

			if (npc.ai[1] <= 1f
				&& Main.netMode != NetmodeID.MultiplayerClient
				&& BossSpawnedNpc.CountOf(npc) < maxAlive) {
				spawn.Execute(new BossFight(npc, null));
			}

			return npc.ai[1] >= ticks;
		}
	}
}
