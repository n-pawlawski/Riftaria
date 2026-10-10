using Microsoft.Xna.Framework;
using Terraria;

namespace Riftaria.Common.Bosses
{
	/// <summary>
	/// Lies on the ground, then sits up. Plays once. Later passes through the
	/// list skip it.
	/// </summary>
	public sealed class Awaken : IBossMotion
	{
		private readonly int ticks;

		public Awaken(int ticks)
		{
			this.ticks = ticks;
		}

		public bool Step(NPC npc)
		{
			if (npc.localAI[1] >= 1f) {
				npc.rotation = 0f;
				npc.gfxOffY = 0f;
				npc.noGravity = false;
				return true;
			}

			if (npc.ai[1] <= 1f) {
				npc.TargetClosest(faceTarget: true);
				npc.localAI[2] = npc.knockBackResist;
				npc.ai[2] = npc.Center.X;
				npc.ai[3] = npc.Bottom.Y;
			}

			npc.noGravity = true;
			npc.knockBackResist = 1f;
			npc.velocity = Vector2.Zero;
			int facing = npc.direction == 0 ? -1 : npc.direction;
			npc.direction = facing;
			npc.spriteDirection = facing;
			npc.Center = new Vector2(npc.ai[2], npc.ai[3] - npc.height / 2f);

			float t = MathHelper.Clamp(npc.ai[1] / ticks, 0f, 1f);
			float eased = t * t * (3f - 2f * t);
			float sign = facing >= 0 ? 1f : -1f;
			npc.rotation = MathHelper.Lerp(-sign * MathHelper.PiOver2, 0f, eased);
			npc.gfxOffY = (npc.height - npc.width) * 0.5f * (1f - eased);

			if (npc.ai[1] < ticks) {
				return false;
			}

			npc.localAI[1] = 1f;
			npc.rotation = 0f;
			npc.gfxOffY = 0f;
			npc.noGravity = false;
			npc.knockBackResist = npc.localAI[2];
			return true;
		}
	}
}
