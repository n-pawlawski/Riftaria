using Terraria;

namespace Riftaria.Common.Bosses
{
	/// <summary>Pauses, jumps toward the player, and finishes when it lands.</summary>
	public sealed class Leap : IBossMotion
	{
		private readonly int windup;

		private readonly float speed;

		private readonly float hop;

		public Leap(int windup, float speed, float hop)
		{
			this.windup = windup;
			this.speed = speed;
			this.hop = hop;
		}

		public bool Step(NPC npc)
		{
			npc.noGravity = false;
			npc.TargetClosest(faceTarget: true);
			npc.spriteDirection = npc.direction;

			if (npc.ai[1] < windup) {
				npc.velocity.X = 0f;
				return false;
			}

			if (npc.ai[2] == 0f) {
				float dir = npc.direction == 0 ? 1f : npc.direction;
				npc.velocity.X = dir * speed;
				npc.velocity.Y = -hop;
				npc.ai[2] = 1f;
				return false;
			}

			if (npc.ai[1] > windup + 8f && npc.velocity.Y == 0f && npc.oldVelocity.Y > 0f) {
				return true;
			}

			return npc.ai[1] >= windup + 80f;
		}
	}
}
