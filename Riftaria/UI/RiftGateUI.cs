using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Riftaria.Common.Maps;
using Riftaria.Common.Systems;
using Riftaria.Content.Items;

namespace Riftaria.UI
{
	/// <summary>Run selector. Lists the sigils the player is carrying.</summary>
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

			Player player = Main.LocalPlayer;
			bool found = false;

			for (int slot = 0; slot < player.inventory.Length; slot++) {
				Item item = player.inventory[slot];
				if (item.stack > 0 && item.ModItem is RiftSigil sigil) {
					list.Add(new RiftGateRow(slot, sigil.Tier));
					found = true;
				}
			}

			if (!found) {
				list.Add(new UIText(Language.GetText("Mods.Riftaria.UI.NoSigils"), 0.9f));
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

	/// <summary>One carried sigil, with the button that spends it.</summary>
	public class RiftGateRow : UIElement
	{
		private readonly int slot;
		private readonly int tier;

		public RiftGateRow(int slot, int tier)
		{
			this.slot = slot;
			this.tier = tier;
			Width.Set(0f, 1f);
			Height.Set(46f, 0f);
		}

		public override void OnInitialize()
		{
			RiftTier data = RiftTierRegistry.Get(tier);

			UIText label = new(Language.GetTextValue("Mods.Riftaria.Rift.Title", tier), 0.95f);
			label.Left.Set(4f, 0f);
			Append(label);

			UIText requirement = new(
				Language.GetTextValue("Mods.Riftaria.Rift.Requirement", data.RequiredLevel), 0.8f) {
				TextColor = Color.LightGray
			};
			requirement.Left.Set(4f, 0f);
			requirement.Top.Set(22f, 0f);
			Append(requirement);

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
			Player player = Main.LocalPlayer;

			// The inventory can have been rearranged since the panel was built.
			Item item = player.inventory[slot];
			if (item.ModItem is not RiftSigil sigil || sigil.Tier != tier) {
				ModContent.GetInstance<RiftGateUISystem>().Open();
				return;
			}

			if (!RiftGateway.CanEnter(player, tier, ignoreRequirements: false, out string reason)) {
				Main.NewText(reason, 255, 140, 140);
				return;
			}

			// Spend first: the transition is asynchronous, and a sigil that survives
			// the run it paid for is a duplication bug.
			item.TurnToAir();

			ModContent.GetInstance<RiftGateUISystem>().Close();
			Main.NewText(Language.GetTextValue("Mods.Riftaria.Rift.Opening", tier), 190, 160, 255);

			RiftGateway.Enter(tier);
		}
	}
}
