using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Riftaria.Common.Bosses;
using Riftaria.Common.Drops;
using Riftaria.Common.Maps;
using Riftaria.Common.Systems;
using Riftaria.Content.Items;

namespace Riftaria.Content.NPCs
{
	/// <summary>
	/// A vanilla zombie with two stats changed: life and size. Damage, defense,
	/// knockback and sounds stay whatever <see cref="NPCID.Zombie"/> cloned in.
	/// <see cref="NPC.scale"/> draws that same sprite larger, so this does not
	/// have its own texture. Width and height are the hitbox and match the scale.
	/// <para>
	/// He starts on the ground and sits up, then walks, throws, and calls in
	/// more zombies. The walk is not the vanilla fighter. That AI reverses the
	/// moment a slope touches the hitbox, so a larger body on this floor shakes
	/// in place.
	/// </para>
	/// </summary>
	public class RiftZombie : ModNPC
	{
		private const float SizeScale = 2f;

		private const int LifeMultiplier = 2;

		public override string Texture => "Terraria/Images/NPC_" + NPCID.Zombie;

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Zombie];

			RiftBossRegistry.Define(Type, boss => {
				boss.Moves(
					new Awaken(ticks: 75),
					new Advance(speed: 1.15f, ticks: 200),
					new Hurl(windup: 24, ticks: 70),
					new Rally(NPCID.Zombie, count: 5, ticks: 100, maxAlive: 6));
			});
		}

		public override void SetDefaults()
		{
			NPC.CloneDefaults(NPCID.Zombie);
			// Fighter AI treats a sloped tile under the hitbox as a wall and
			// turns around every tick. Direction comes from the player instead.
			NPC.aiStyle = -1;
			AIType = 0;
			AnimationType = -1;

			NPC.lifeMax *= LifeMultiplier;
			NPC.life = NPC.lifeMax;

			// Marks it as a boss. The bottom bar is drawn by RiftBossSystem,
			// because a borrowed zombie sprite has no boss-head icon for the
			// game's own bar to attach to.
			NPC.boss = true;

			NPC.scale = SizeScale;
			NPC.width = (int)(NPC.width * SizeScale);
			NPC.height = (int)(NPC.height * SizeScale);
		}

		/// <summary>Placed by the run, not by the ambient pool.</summary>
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
			// The objective. A normal zombie would despawn once the player
			// stepped back down the path.
			if (NPC.timeLeft < 600) {
				NPC.timeLeft = 600;
			}
		}

		public override void OnKill()
		{
			BossSpawnedNpc.RemoveOf(NPC);
			RiftRunSystem.CompleteRun();
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			RiftLootBag.DropFrom(NPC, RiftBossIds.Zombie, RiftRunState.ActiveTier);
		}
	}
}
