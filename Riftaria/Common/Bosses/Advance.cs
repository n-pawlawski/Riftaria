using Terraria;
using Riftaria.Content.NPCs;

namespace Riftaria.Common.Bosses
{
	/// <summary>Walks toward the player for a stretch, and jumps when close.</summary>
	public sealed class Advance : IBossMotion
	{
		private readonly float speed;

		private readonly int ticks;

		public Advance(float speed, int ticks)
		{
			this.speed = speed;
			this.ticks = ticks;
		}

		public bool Step(NPC npc)
		{
			npc.noGravity = false;
			RiftZombieWalk.Chase(npc, speed);
			return npc.ai[1] >= ticks;
		}
	}
}
