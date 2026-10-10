using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace Riftaria.Common.Bosses
{
	/// <summary>
	/// Stays <see cref="distance"/> pixels from the player and walks a circle
	/// around them. One full turn fits in <paramref name="ticks"/>.
	/// </summary>
	public sealed class Circuit : IBossMotion
	{
		private readonly float distance;

		private readonly int ticks;

		private readonly float radiansPerTick;

		public Circuit(float distance, int ticks)
		{
			this.distance = distance;
			this.ticks = ticks;
			radiansPerTick = ticks <= 0 ? 0f : MathHelper.TwoPi / ticks;
		}

		public bool Step(NPC npc)
		{
			npc.TargetClosest(faceTarget: false);
			if (!npc.HasValidTarget) {
				npc.velocity = Vector2.Zero;
				return npc.ai[1] >= ticks;
			}

			Player player = Main.player[npc.target];
			if (npc.ai[1] <= 1f) {
				Vector2 from = npc.Center - player.Center;
				npc.ai[2] = (float)Math.Atan2(from.Y, from.X);
			}

			npc.ai[2] += radiansPerTick;
			Vector2 desired = player.Center + new Vector2((float)Math.Cos(npc.ai[2]), (float)Math.Sin(npc.ai[2])) * distance;
			Vector2 to = desired - npc.Center;
			const float catchup = 8f;
			if (to.LengthSquared() > catchup * catchup) {
				to.Normalize();
				to *= catchup;
			}

			npc.velocity = to;
			npc.rotation = 0f;
			npc.spriteDirection = player.Center.X >= npc.Center.X ? 1 : -1;
			return npc.ai[1] >= ticks;
		}
	}
}
