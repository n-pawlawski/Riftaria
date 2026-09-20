using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Rifthold.Common.Maps;
using Rifthold.Common.Systems;

namespace Rifthold.Content.NPCs
{
	/// <summary>
	/// Shared base for rift trash. Movement borrows the vanilla fighter AI, but
	/// framing is done here rather than through AnimationType so the sprite sheet
	/// only has to supply the frames this class declares.
	/// </summary>
	public abstract class RiftEnemy : ModNPC
	{
		protected abstract int FrameCount { get; }

		/// <summary>Relative weight in the rift spawn pool.</summary>
		public abstract float SpawnWeight { get; }

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = FrameCount;
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = NPCAIStyleID.Fighter;
			AIType = NPCID.Zombie;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.knockBackResist = 0.5f;
			NPC.friendly = false;
			NPC.noGravity = false;
		}

		/// <summary>
		/// Tier scaling is applied per spawn rather than in SetDefaults, because the
		/// tier is only known once the rift is running.
		/// </summary>
		public override void OnSpawn(IEntitySource source)
		{
			if (!RiftGateway.InRift) {
				return;
			}

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

		public override void FindFrame(int frameHeight)
		{
			NPC.spriteDirection = NPC.direction;

			// Idle still ticks over slowly so a stuck enemy does not look frozen.
			NPC.frameCounter += NPC.velocity.X == 0f ? 0.06 : 0.18;
			NPC.frameCounter %= FrameCount;
			NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
		}

		/// <summary>Rift enemies exist only inside rifts; the Hold is a safe room.</summary>
		public override float SpawnChance(NPCSpawnInfo spawnInfo)
			=> RiftGateway.InRift ? SpawnWeight : 0f;
	}

	public class RiftHusk : RiftEnemy
	{
		protected override int FrameCount => 3;

		public override float SpawnWeight => 0.7f;

		public override void SetDefaults()
		{
			base.SetDefaults();
			NPC.width = 20;
			NPC.height = 40;
			NPC.lifeMax = 90;
			NPC.damage = 18;
			NPC.defense = 4;
			NPC.value = Item.buyPrice(silver: 2);
		}
	}

	public class RiftStalker : RiftEnemy
	{
		protected override int FrameCount => 3;

		public override float SpawnWeight => 0.3f;

		public override void SetDefaults()
		{
			base.SetDefaults();
			NPC.width = 20;
			NPC.height = 40;
			NPC.lifeMax = 60;
			NPC.damage = 24;
			NPC.defense = 2;
			NPC.knockBackResist = 0.75f;
			NPC.value = Item.buyPrice(silver: 3);
		}

		public override void AI()
		{
			// Fighter AI with a shove, so stalkers close distance faster than husks.
			NPC.velocity.X *= 1.012f;
			float cap = 4.5f;
			if (NPC.velocity.X > cap) {
				NPC.velocity.X = cap;
			}
			else if (NPC.velocity.X < -cap) {
				NPC.velocity.X = -cap;
			}
		}
	}
}
