using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Riftaria.Common.Bosses;
using Riftaria.Common.Drops;
using Riftaria.Common.Maps;
using Riftaria.Common.Systems;
using Riftaria.Common.World;
using Riftaria.Content.Items;

namespace Riftaria.Content.NPCs
{
	/// <summary>
	/// A Demon Eye, drawn larger, with ten times the life. Damage and sounds
	/// stay on the cloned eye. The hitbox is the visible body of that sprite,
	/// not the cloned box, which includes the empty margin around the eye.
	/// <para>
	/// Lunge locks onto a player and flies until a wall. Circuit keeps its
	/// distance and circles. Brood spins in place and sheds smaller eyes.
	/// </para>
	/// </summary>
	public class RiftEye : ModNPC
	{
		private const int LifeMultiplier = 10;

		private const float SizeScale = 2.5f;

		public override string Texture => "Terraria/Images/NPC_" + NPCID.DemonEye;

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.DemonEye];

			RiftBossRegistry.Define(Type, boss => {
				boss.Moves(
					new Lunge(speed: 10f),
					new Circuit(distance: 200f, ticks: 240),
					new Brood(NPCID.DemonEye, count: 4, ticks: 180, spinPerTick: 0.22f));
			});
		}

		public override void SetDefaults()
		{
			NPC.CloneDefaults(NPCID.DemonEye);
			NPC.aiStyle = -1;
			AIType = 0;

			NPC.noGravity = true;
			NPC.noTileCollide = false;
			NPC.knockBackResist = 0f;
			NPC.boss = true;

			NPC.lifeMax *= LifeMultiplier;
			NPC.life = NPC.lifeMax;

			NPC.scale = SizeScale;
			// SetDefaults also runs while the mod is loading, off the thread that
			// is allowed to read textures. The real fit happens when it spawns.
			NPC.width = Math.Max(8, (int)(NPC.width * SizeScale * 0.5f));
			NPC.height = Math.Max(8, (int)(NPC.height * SizeScale * 0.5f));
		}

		public override void OnSpawn(IEntitySource source)
		{
			Vector2 center = NPC.Center;
			FitHitboxToSprite();
			NPC.Center = center;
		}

		/// <summary>
		/// The cloned box is taller and wider than the eye art. Size the hitbox
		/// to the opaque pixels, at the same scale the sprite is drawn.
		/// </summary>
		private void FitHitboxToSprite()
		{
			int frames = Math.Max(1, Main.npcFrameCount[NPCID.DemonEye]);
			var asset = TextureAssets.Npc[NPCID.DemonEye];
			if (!asset.IsLoaded) {
				return;
			}

			Texture2D tex = asset.Value;
			int frameHeight = tex.Height / frames;
			Color[] pixels = new Color[tex.Width * frameHeight];
			try {
				tex.GetData(0, new Rectangle(0, 0, tex.Width, frameHeight), pixels, 0, pixels.Length);
			}
			catch (Exception) {
				return;
			}

			int minX = tex.Width;
			int minY = frameHeight;
			int maxX = 0;
			int maxY = 0;
			for (int y = 0; y < frameHeight; y++) {
				for (int x = 0; x < tex.Width; x++) {
					if (pixels[y * tex.Width + x].A <= 16) {
						continue;
					}

					if (x < minX) {
						minX = x;
					}

					if (y < minY) {
						minY = y;
					}

					if (x > maxX) {
						maxX = x;
					}

					if (y > maxY) {
						maxY = y;
					}
				}
			}

			if (maxX < minX || maxY < minY) {
				return;
			}

			NPC.width = Math.Max(8, (int)((maxX - minX + 1) * SizeScale));
			NPC.height = Math.Max(8, (int)((maxY - minY + 1) * SizeScale));
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0f;

		public override void AI()
		{
			BossMotionRunner.Step(NPC);
		}

		public override void FindFrame(int frameHeight)
		{
			int frames = Main.npcFrameCount[Type];
			if (frames < 1 || frameHeight <= 0) {
				return;
			}

			NPC.frameCounter += 0.15;
			NPC.frameCounter %= frames;
			NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
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
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				RiftLootBag.DropFrom(NPC, RiftBossIds.Eye, RiftRunState.ActiveTier);
			}
		}
	}
}
