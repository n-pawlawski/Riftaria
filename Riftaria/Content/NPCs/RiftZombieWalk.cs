using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Riftaria.Common.Systems;

namespace Riftaria.Content.NPCs
{
	/// <summary>
	/// Walks toward the player every tick. A bump still hops a step. Within
	/// <see cref="JumpRange"/> on the horizontal, they jump at the player too,
	/// higher when the player is above them.
	/// </summary>
	internal static class RiftZombieWalk
	{
		/// <summary>Horizontal distance, in pixels, that allows a jump.</summary>
		private const float JumpRange = 160f;

		private const float StepJump = 6.2f;

		private const float MaxJump = 11f;

		public static void Chase(NPC npc, float walkSpeed)
		{
			ref float jumpCooldown = ref npc.localAI[0];

			npc.TargetClosest(faceTarget: true);
			npc.spriteDirection = npc.direction;
			npc.velocity.X = npc.direction * walkSpeed;

			if (jumpCooldown > 0f) {
				jumpCooldown--;
			}

			bool close = false;
			float rise = 0f;
			if (npc.HasValidTarget) {
				Player player = Main.player[npc.target];
				close = Math.Abs(player.Center.X - npc.Center.X) <= JumpRange;
				rise = npc.Top.Y - player.Bottom.Y;
			}

			bool grounded = npc.collideY && npc.velocity.Y >= 0f;
			if (!grounded || jumpCooldown > 0f || (!npc.collideX && !close)) {
				return;
			}

			float jump = StepJump;
			if (close && rise > 32f) {
				float gravity = npc.gravity > 0.05f ? npc.gravity : 0.3f;
				jump = (float)Math.Sqrt(2f * gravity * (rise + 12f));
				if (jump < StepJump) {
					jump = StepJump;
				}

				if (jump > MaxJump) {
					jump = MaxJump;
				}
			}

			npc.velocity.Y = -jump;
			jumpCooldown = 35f;
		}

		public static void Animate(NPC npc)
		{
			int frames = Main.npcFrameCount[npc.type];
			if (frames <= 1 || npc.frame.Height <= 0) {
				return;
			}

			float speed = npc.velocity.X;
			npc.frameCounter += speed > 0.01f || speed < -0.01f ? 0.18 : 0.06;
			npc.frameCounter %= frames;
			npc.frame.Y = (int)npc.frameCounter * npc.frame.Height;
		}
	}

	/// <summary>Vanilla zombies inside a rift use <see cref="RiftZombieWalk"/>.</summary>
	public class RiftVanillaZombie : GlobalNPC
	{
		public override bool PreAI(NPC npc)
		{
			if (!RiftGateway.InRift || npc.type != NPCID.Zombie) {
				return true;
			}

			RiftZombieWalk.Chase(npc, 1f);
			RiftZombieWalk.Animate(npc);
			return false;
		}
	}
}
