using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Riftaria.Common.Maps;
using Riftaria.Common.Systems;

namespace Riftaria.UI
{
	/// <summary>
	/// Run selector. Lists every tier in the table and marks which are open, so the
	/// ladder is visible from the first run rather than appearing a rung at a time.
	/// </summary>
	public class RiftGateUI : UIState
	{
		private UIPanel panel;
		private UIList list;

		public override void OnInitialize()
		{
			panel = new UIPanel {
				HAlign = 0.5f,
				VAlign = 0.5f,
				BackgroundColor = new Color(29, 39, 64) * 0.94f
			};
			panel.Width.Set(420f, 0f);
			panel.Height.Set(380f, 0f);
			Append(panel);

			UIText title = new(Language.GetText("Mods.Riftaria.UI.RiftGateTitle"), 1.15f) {
				HAlign = 0.5f
			};
			title.Top.Set(8f, 0f);
			panel.Append(title);

			list = new UIList { ListPadding = 8f };
			list.Width.Set(-16f, 1f);
			list.Height.Set(-100f, 1f);
			list.Top.Set(48f, 0f);
			panel.Append(list);

			UITextPanel<LocalizedText> close = new(Language.GetText("Mods.Riftaria.UI.Close"), 0.85f) {
				HAlign = 0.5f,
				VAlign = 1f,
				BackgroundColor = new Color(70, 84, 140)
			};
			close.Width.Set(120f, 0f);
			close.Height.Set(32f, 0f);
			close.OnLeftClick += (_, _) => ModContent.GetInstance<RiftGateUISystem>().Close();
			panel.Append(close);
		}

		public void Rebuild()
		{
			list.Clear();

			foreach (RiftTier tier in RiftTierRegistry.All) {
				list.Add(new RiftGateRow(tier.Tier, RiftGateway.IsUnlocked(tier.Tier)));
			}

			if (list.Count == 0) {
				list.Add(new UIText(Language.GetText("Mods.Riftaria.UI.NoTiers"), 0.9f));
			}

			Recalculate();
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (panel.ContainsPoint(Main.MouseScreen)) {
				Main.LocalPlayer.mouseInterface = true;
			}
		}
	}

	/// <summary>One tier, and the button that opens it.</summary>
	public class RiftGateRow : UIElement
	{
		private readonly int tier;
		private readonly bool unlocked;

		public RiftGateRow(int tier, bool unlocked)
		{
			this.tier = tier;
			this.unlocked = unlocked;
			Width.Set(0f, 1f);
			Height.Set(46f, 0f);
		}

		public override void OnInitialize()
		{
			UIText label = new(Language.GetTextValue("Mods.Riftaria.Rift.Title", tier), 0.95f) {
				TextColor = unlocked ? Color.White : Color.Gray
			};
			label.Left.Set(4f, 0f);
			Append(label);

			UIText state = new(
				unlocked
					? Language.GetTextValue("Mods.Riftaria.Rift.Open")
					: Language.GetTextValue("Mods.Riftaria.Rift.Locked", tier - 1),
				0.8f) {
				TextColor = unlocked ? Color.LightGray : Color.DarkGray
			};
			state.Left.Set(4f, 0f);
			state.Top.Set(22f, 0f);
			Append(state);

			if (!unlocked) {
				return;
			}

			UITextPanel<LocalizedText> enter = new(Language.GetText("Mods.Riftaria.UI.Enter"), 0.85f) {
				VAlign = 0.5f,
				BackgroundColor = new Color(86, 66, 130)
			};
			enter.Width.Set(90f, 0f);
			enter.Height.Set(32f, 0f);
			enter.Left.Set(-98f, 1f);
			enter.OnLeftClick += (_, _) => Launch();
			Append(enter);
		}

		private void Launch()
		{
			if (!RiftGateway.CanEnter(Main.LocalPlayer, tier, ignoreRequirements: false, out string reason)) {
				Main.NewText(reason, 255, 140, 140);
				return;
			}

			ModContent.GetInstance<RiftGateUISystem>().Close();
			Main.NewText(Language.GetTextValue("Mods.Riftaria.Rift.Opening", tier), 190, 160, 255);

			RiftGateway.Enter(tier);
		}
	}
}
