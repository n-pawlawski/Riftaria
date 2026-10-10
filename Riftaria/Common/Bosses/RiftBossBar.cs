using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.ModLoader;
using Riftaria.Common.Systems;

namespace Riftaria.Common.Bosses
{
	/// <summary>
	/// Terraria's boss bar for a rift boss. The icon is the boss's own first
	/// frame, so a boss that borrows a vanilla sprite does not need a new picture.
	/// </summary>
	public class RiftBossBar : ModBossBar
	{
		public override string Texture => "Terraria/Images/NPC_" + NPCID.Zombie;

		public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
		{
			if (info.npcIndexToAimAt < 0 || info.npcIndexToAimAt >= Main.maxNPCs) {
				return false;
			}

			NPC npc = Main.npc[info.npcIndexToAimAt];
			if (!npc.active || npc.life <= 0 || !RiftBossSystem.IsOnScreen(npc)) {
				return false;
			}

			if (!RiftBossRegistry.TryGet(npc.type, out RiftBoss boss) || !boss.ShowHealthBar) {
				return false;
			}

			return null;
		}

		public static void DrawFor(SpriteBatch spriteBatch, NPC npc)
		{
			Rectangle? frame = null;
			Asset<Texture2D> asset = FirstFrame(npc.type, ref frame) ?? FirstFrame(NPCID.Zombie, ref frame);
			if (asset == null || !frame.HasValue) {
				return;
			}

			BigProgressBarHelper.DrawFancyBar(spriteBatch, npc.life, npc.lifeMax, asset.Value, frame.Value);
		}

		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
			=> FirstFrame(NPCID.Zombie, ref iconFrame);

		public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
		{
			Rectangle? frame = null;
			Asset<Texture2D> asset = FirstFrame(npc.type, ref frame);
			if (asset != null && frame.HasValue) {
				drawParams.IconTexture = asset.Value;
				drawParams.IconFrame = frame.Value;
				if (frame.Value.Height > 0) {
					drawParams.IconScale = 26f / frame.Value.Height;
				}
			}

			return true;
		}

		private static Asset<Texture2D> FirstFrame(int type, ref Rectangle? iconFrame)
		{
			if (type < 0 || type >= TextureAssets.Npc.Length) {
				return null;
			}

			Asset<Texture2D> asset = TextureAssets.Npc[type];
			if (asset == null) {
				return null;
			}

			int frames = Main.npcFrameCount[type];
			if (frames <= 0) {
				frames = 1;
			}

			Texture2D texture = asset.Value;
			int height = System.Math.Max(1, texture.Height / frames);
			iconFrame = new Rectangle(0, 0, texture.Width, height);
			return asset;
		}
	}
}
