using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Rifthold.Common.Maps;
using Rifthold.Common.Systems;

namespace Rifthold.Content.NPCs
{
	/// <summary>
	/// The objective. Killing it ends the run; it is spawned by
	/// <see cref="RiftRunSystem"/> when a player reaches the arena rather than at
	/// generation time, so it is never fighting an empty room.
	/// </summary>
	[AutoloadBossHead]
	public class RiftWarden : ModNPC
	{
		private const int FrameCount = 4;

		private const int HoverTicks = 120;
		private const int WindUpTicks = 30;
		private const int ChargeTicks = 45;

		private const float HoverHeight = 170f;
		private const float HoverSpeed = 9f;
		private const float ChargeSpeed = 14f;

		private enum Phase
		{
			Hover = 0,
			WindUp = 1,
			Charge = 2
		}

		private Phase State
		{
			get => (Phase)(int)NPC.ai[0];
			set => NPC.ai[0] = (float)value;
		}

		private ref float Timer => ref NPC.ai[1];

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = FrameCount;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
		}

		public override void SetDefaults()
		{
			NPC.width = 60;
			NPC.height = 60;
			NPC.lifeMax = 2400;
			NPC.damage = 40;
			NPC.defense = 14;
			NPC.knockBackResist = 0f;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			// The arena is sealed, but charges overshoot; passing through tiles is
			// far better than a boss wedged in a wall with the run unwinnable.
			NPC.noTileCollide = true;
			NPC.boss = true;
			NPC.npcSlots = 10f;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath14;
			NPC.value = Item.buyPrice(gold: 5);
			Music = MusicID.Boss1;
		}

		public override void OnSpawn(IEntitySource source)
		{
			float scale = RiftRunState.ActiveTierData.EnemyScaling;
			if (scale <= 1f) {
				return;
			}

			NPC.lifeMax = (int)(NPC.lifeMax * scale);
			NPC.life = NPC.lifeMax;
			NPC.damage = (int)(NPC.damage * scale);
			NPC.defense = (int)(NPC.defense * scale);
			NPC.netUpdate = true;
		}

		/// <summary>The Warden holds its arena; it never wanders off or times out.</summary>
		public override bool CheckActive() => false;

		public override void AI()
		{
			if (!HasLivingTarget()) {
				NPC.TargetClosest();
			}

			if (!HasLivingTarget()) {
				// Everyone is dead or gone. Drift up and leave rather than hovering forever.
				NPC.velocity.Y -= 0.2f;
				if (NPC.timeLeft > 60) {
					NPC.timeLeft = 60;
				}

				return;
			}

			Player target = Main.player[NPC.target];
			Timer++;

			switch (State) {
				case Phase.Hover:
					Hover(target);
					if (Timer > HoverTicks) {
						Advance(Phase.WindUp);
					}

					break;

				case Phase.WindUp:
					NPC.velocity *= 0.9f;
					if (Timer > WindUpTicks) {
						NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * ChargeSpeed;
						Advance(Phase.Charge);
					}

					break;

				case Phase.Charge:
					if (Timer > ChargeTicks) {
						Advance(Phase.Hover);
					}

					break;
			}

			NPC.rotation = NPC.velocity.X * 0.02f;
			NPC.spriteDirection = NPC.velocity.X < 0f ? -1 : 1;
		}

		private bool HasLivingTarget()
		{
			if (NPC.target < 0 || NPC.target >= Main.maxPlayers) {
				return false;
			}

			Player target = Main.player[NPC.target];
			return target.active && !target.dead;
		}

		private void Hover(Player target)
		{
			Vector2 desired = target.Center - new Vector2(0f, HoverHeight);
			Vector2 offset = desired - NPC.Center;
			float speed = MathHelper.Clamp(offset.Length() / 20f, 0f, HoverSpeed);

			NPC.velocity = Vector2.Lerp(NPC.velocity, offset.SafeNormalize(Vector2.Zero) * speed, 0.08f);
		}

		private void Advance(Phase next)
		{
			State = next;
			Timer = 0f;
			NPC.netUpdate = true;
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter += 0.15;
			NPC.frameCounter %= FrameCount;
			NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
		}

		public override void OnKill() => RiftRunSystem.CompleteRun();
	}
}
