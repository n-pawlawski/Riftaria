using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Riftaria.Common.Bosses;
using Riftaria.Common.Drops;
using Riftaria.Common.Maps;
using Riftaria.Common.Systems;
using Riftaria.Common.World;
using Riftaria.Content.Items;
using Riftaria.Content.Projectiles;

namespace Riftaria.Content.NPCs
{
	/// <summary>
	/// A granite golem, drawn larger. It walks, leaps, throws boulders, and
	/// calls in smaller golems. Damage and sounds stay on the cloned body.
	/// </summary>
	public class RiftGolem : ModNPC
	{
		private const float SizeScale = 2.2f;

		private const int LifeMultiplier = 12;

		public override string Texture => "Terraria/Images/NPC_" + NPCID.GraniteGolem;

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.GraniteGolem];

			RiftBossRegistry.Define(Type, boss => {
				boss.Moves(
					new Advance(speed: 1.35f, ticks: 180),
					new Leap(windup: 16, speed: 6.5f, hop: 9f),
					new Hurl(windup: 28, ticks: 70, ModContent.ProjectileType<RiftBoulder>(), RiftBoulder.Gravity),
					new Rally(NPCID.GraniteGolem, count: 3, ticks: 110, maxAlive: 4));
			});
		}

		public override void SetDefaults()
		{
			NPC.CloneDefaults(NPCID.GraniteGolem);
			NPC.aiStyle = -1;
			AIType = 0;
			AnimationType = -1;

			NPC.boss = true;
			NPC.knockBackResist = 0f;
			NPC.noTileCollide = false;
			NPC.noGravity = false;

			NPC.lifeMax *= LifeMultiplier;
			NPC.life = NPC.lifeMax;

			NPC.scale = SizeScale;
			NPC.width = (int)(NPC.width * SizeScale);
			NPC.height = (int)(NPC.height * SizeScale);
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0f;

		public override void AI()
		{
			BossMotionRunner.Step(NPC);
		}

		public override void FindFrame(int frameHeight)
		{
			if (NPC.frame.Height <= 0) {
				NPC.frame.Height = frameHeight;
			}

			if (System.Math.Abs(NPC.velocity.X) > 0.15f) {
				RiftZombieWalk.Animate(NPC);
				return;
			}

			NPC.frame.Y = 0;
		}

		public override void PostAI()
		{
			if (NPC.timeLeft < 600) {
				NPC.timeLeft = 600;
			}
		}

		public override void OnKill()
		{
			BossSpawnedNpc.RemoveOf(NPC);
			if (NPC.whoAmI != RiftLayout.ObjectiveWho) {
				return;
			}

			RiftRunSystem.CompleteRun();
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			RiftLootBag.DropFrom(NPC, RiftBossIds.Golem, RiftRunState.ActiveTier);
		}
	}
}
