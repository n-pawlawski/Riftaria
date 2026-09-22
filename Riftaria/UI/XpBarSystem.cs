using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Riftaria.Common.Maps;
using Riftaria.Common.Players;
using Riftaria.Common.Progression;
using Riftaria.Common.Systems;

namespace Riftaria.UI
{
	/// <summary>
	/// The permanent HUD: level, experience bar, unspent points, and the run
	/// objective while inside a rift. Drawn directly rather than through a UIState
	/// because none of it is interactive.
	/// </summary>
	[Autoload(Side = ModSide.Client)]
	public class XpBarSystem : ModSystem
	{
		private const int BarWidth = 300;
		private const int BarHeight = 14;
		private const int BottomMargin = 78;

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (index == -1) {
				return;
			}

			layers.Insert(index, new LegacyGameInterfaceLayer(
				"Riftaria: Experience Bar",
				() => {
					Draw(Main.spriteBatch);
					return true;
				},
				InterfaceScaleType.UI));
		}

		private static void Draw(SpriteBatch spriteBatch)
		{
			Player player = Main.LocalPlayer;
			if (player == null || !player.active || Main.gameMenu) {
				return;
			}

			RiftariaPlayer progression = RiftariaPlayer.Get(player);

			float screenWidth = Main.screenWidth / Main.UIScale;
			float screenHeight = Main.screenHeight / Main.UIScale;
			int left = (int)((screenWidth - BarWidth) / 2f);
			int top = (int)(screenHeight - BottomMargin);

			Texture2D pixel = TextureAssets.MagicPixel.Value;

			spriteBatch.Draw(pixel, new Rectangle(left - 2, top - 2, BarWidth + 4, BarHeight + 4),
				new Color(12, 12, 20, 200));
			spriteBatch.Draw(pixel, new Rectangle(left, top, BarWidth, BarHeight),
				new Color(38, 38, 54, 220));

			int filled = (int)(BarWidth * progression.XpProgress);
			if (filled > 0) {
				spriteBatch.Draw(pixel, new Rectangle(left, top, filled, BarHeight),
					new Color(120, 190, 255));
			}

			string label = Language.GetTextValue("Mods.Riftaria.Progression.Level", progression.Level);
			if (progression.Level < XpCurve.MaxLevel) {
				label += $"   {progression.Xp} / {progression.XpToNextLevel}";
			}

			Utils.DrawBorderString(spriteBatch, label,
				new Vector2(left, top - 20f), Color.White, 0.85f);

			if (progression.UnspentPoints > 0) {
				string points = Language.GetTextValue("Mods.Riftaria.Progression.UnspentPoints",
					progression.UnspentPoints);
				Utils.DrawBorderString(spriteBatch, points,
					new Vector2(left + BarWidth, top - 20f), new Color(255, 220, 120), 0.85f, 1f);
			}

			DrawObjective(spriteBatch, left, top);
		}

		/// <summary>Tells the player which way the Warden is, and when the rift closes.</summary>
		private static void DrawObjective(SpriteBatch spriteBatch, int left, int top)
		{
			if (!RiftGateway.InRift) {
				return;
			}

			string text;
			Color colour;

			if (RiftRunState.ObjectiveComplete) {
				int seconds = System.Math.Max(RiftRunState.ExitCountdown / 60, 0);
				text = Language.GetTextValue("Mods.Riftaria.Rift.ReturningIn", seconds);
				colour = new Color(150, 255, 180);
			}
			else {
				Vector2 arena = Common.World.RiftLayout.ArenaCenter.ToWorldCoordinates();
				int distance = (int)(Vector2.Distance(Main.LocalPlayer.Center, arena) / 16f);
				string direction = Language.GetTextValue(arena.X > Main.LocalPlayer.Center.X
					? "Mods.Riftaria.Rift.East"
					: "Mods.Riftaria.Rift.West");

				text = Language.GetTextValue("Mods.Riftaria.Rift.ObjectiveDirection", distance, direction);
				colour = new Color(230, 180, 255);
			}

			Utils.DrawBorderString(spriteBatch, text, new Vector2(left, top + BarHeight + 6f), colour, 0.85f);
		}
	}
}
