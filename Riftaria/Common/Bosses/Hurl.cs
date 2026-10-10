using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Riftaria.Content.Projectiles;

namespace Riftaria.Common.Bosses
{
	/// <summary>
	/// Stands still, then throws one projectile on an arc toward the player.
	/// </summary>
	public sealed class Hurl : IBossMotion
	{
		private readonly int windup;

		private readonly int ticks;

		private readonly int projectileType;

		private readonly float gravity;

		public Hurl(int windup, int ticks, int projectileType = 0, float gravity = RiftArc.Gravity)
		{
			this.windup = windup;
			this.ticks = ticks;
			this.projectileType = projectileType;
			this.gravity = gravity;
		}

		public bool Step(NPC npc)
		{
			npc.noGravity = false;
			npc.TargetClosest(faceTarget: true);
			npc.velocity.X = 0f;
			npc.spriteDirection = npc.direction;

			if (npc.ai[1] == windup && Main.netMode != NetmodeID.MultiplayerClient && npc.HasValidTarget) {
				Throw(npc, Main.player[npc.target]);
			}

			return npc.ai[1] >= ticks;
		}

		private void Throw(NPC npc, Player player)
		{
			Vector2 origin = npc.Center + new Vector2(npc.direction * 12f, -npc.height * 0.2f);
			Vector2 to = player.Center - origin;
			float flight = MathHelper.Clamp(System.Math.Abs(to.X) / 5f, 36f, 80f);
			float vx = MathHelper.Clamp(to.X / flight, -8f, 8f);
			float vy = (to.Y / flight) - (0.5f * gravity * flight);
			if (vy > -6f) {
				vy = -6f;
			}

			int type = projectileType == 0 ? ModContent.ProjectileType<RiftArc>() : projectileType;
			Projectile.NewProjectile(
				npc.GetSource_FromAI(),
				origin,
				new Vector2(vx, vy),
				type,
				npc.damage,
				2f,
				Main.myPlayer);
		}
	}
}
