using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Riftaria.Common.Players;
using Riftaria.Common.Progression;

namespace Riftaria.UI
{
	/// <summary>The character sheet: level, unspent points, and the stat nodes.</summary>
	public class StatPanelUI : UIState
	{
		private UIPanel panel;
		private UIText header;
		private UIList list;

		public override void OnInitialize()
		{
			panel = new UIPanel {
				HAlign = 0.5f,
				VAlign = 0.5f,
				BackgroundColor = new Color(33, 43, 79) * 0.94f
			};
			panel.Width.Set(400f, 0f);
			panel.Height.Set(440f, 0f);
			Append(panel);

			UIText title = new(Language.GetText("Mods.Riftaria.UI.StatPanelTitle"), 1.15f) {
				HAlign = 0.5f
			};
			title.Top.Set(8f, 0f);
			panel.Append(title);

			header = new UIText(string.Empty, 0.9f) { HAlign = 0.5f };
			header.Top.Set(40f, 0f);
			panel.Append(header);

			list = new UIList { ListPadding = 8f };
			list.Width.Set(-16f, 1f);
			list.Height.Set(-110f, 1f);
			list.Top.Set(74f, 0f);
			panel.Append(list);

			UITextPanel<LocalizedText> close = new(Language.GetText("Mods.Riftaria.UI.Close"), 0.85f) {
				HAlign = 0.5f,
				VAlign = 1f,
				BackgroundColor = new Color(70, 84, 140)
			};
			close.Width.Set(120f, 0f);
			close.Height.Set(32f, 0f);
			close.OnLeftClick += (_, _) => ModContent.GetInstance<StatPanelSystem>().Close();
			panel.Append(close);
		}

		/// <summary>
		/// Rebuilt every time the panel opens. Nodes unlock as the character levels,
		/// and building on open avoids depending on registry load order at startup.
		/// </summary>
		public void Rebuild()
		{
			list.Clear();

			foreach (StatNode node in StatTree.Nodes) {
				list.Add(new StatRow(node));
			}

			Recalculate();
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (panel.ContainsPoint(Main.MouseScreen)) {
				Main.LocalPlayer.mouseInterface = true;
			}

			RiftariaPlayer progression = RiftariaPlayer.Get(Main.LocalPlayer);
			header.SetText(Language.GetTextValue("Mods.Riftaria.Progression.UnspentPoints",
				progression.UnspentPoints));
		}
	}

	/// <summary>One node: name, current effect, and a spend button.</summary>
	public class StatRow : UIElement
	{
		private readonly StatNode node;
		private UIText label;
		private UIText detail;
		private UITextPanel<string> spend;

		public StatRow(StatNode node)
		{
			this.node = node;
			Width.Set(0f, 1f);
			Height.Set(46f, 0f);
		}

		public override void OnInitialize()
		{
			label = new UIText(Language.GetTextValue($"Mods.Riftaria.Stats.{node.Id}.Name"), 0.95f);
			label.Left.Set(4f, 0f);
			Append(label);

			detail = new UIText(string.Empty, 0.8f);
			detail.Left.Set(4f, 0f);
			detail.Top.Set(22f, 0f);
			Append(detail);

			spend = new UITextPanel<string>(Language.GetTextValue("Mods.Riftaria.UI.Spend"), 0.9f) {
				VAlign = 0.5f,
				BackgroundColor = new Color(70, 110, 84)
			};
			spend.Width.Set(40f, 0f);
			spend.Height.Set(32f, 0f);
			spend.Left.Set(-48f, 1f);
			spend.OnLeftClick += (_, _) => RiftariaPlayer.Get(Main.LocalPlayer).RequestSpend(node);
			Append(spend);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			RiftariaPlayer progression = RiftariaPlayer.Get(Main.LocalPlayer);
			int rank = progression.RankOf(node.Id);

			label.SetText($"{Language.GetTextValue($"Mods.Riftaria.Stats.{node.Id}.Name")}  {rank}/{node.MaxRank}");

			if (!progression.IsUnlocked(node)) {
				detail.SetText(progression.Level < node.RequiredLevel
					? Language.GetTextValue("Mods.Riftaria.Progression.RequiresLevel", node.RequiredLevel)
					: Language.GetTextValue("Mods.Riftaria.Progression.RequiresNode",
						Language.GetTextValue($"Mods.Riftaria.Stats.{node.RequiresNodeId}.Name"),
						node.RequiresRank));
				detail.TextColor = new Color(190, 140, 140);
			}
			else {
				float total = node.PerRank * rank;
				string shown = node.Kind is StatKind.DamagePercent or StatKind.MoveSpeed
					? (total * 100f).ToString("0.#")
					: total.ToString("0.#");

				detail.SetText(Language.GetTextValue($"Mods.Riftaria.Stats.{node.Id}.Description", shown));
				detail.TextColor = Color.LightGray;
			}

			bool canSpend = progression.CanSpendOn(node);
			spend.BackgroundColor = canSpend ? new Color(70, 110, 84) : new Color(60, 60, 70);
			spend.TextColor = canSpend ? Color.White : Color.Gray;
		}
	}
}
