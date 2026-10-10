using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Riftaria.Common.Players;
using Riftaria.Common.Skills;

namespace Riftaria.UI
{
	/// <summary>
	/// Level, experience, unspent points, and the line of life nodes. Nodes are
	/// bought left to right, so only the one after the last unlocked is clickable.
	/// </summary>
	public class SkillTreeUI : UIState
	{
		private const float NodeSize = 36f;
		private const float NodeGap = 14f;

		private UIPanel panel;
		private UIText levelText;
		private UIText pointsText;

		public override void OnInitialize()
		{
			float lineWidth = SkillTree.LifeLineLength * NodeSize + (SkillTree.LifeLineLength - 1) * NodeGap;

			panel = new UIPanel {
				HAlign = 0.5f,
				VAlign = 0.5f,
				BackgroundColor = new Color(29, 39, 64) * 0.94f
			};
			panel.Width.Set(lineWidth + 48f, 0f);
			panel.Height.Set(220f, 0f);
			Append(panel);

			UIText title = new(Language.GetText("Mods.Riftaria.UI.SkillTreeTitle"), 1.15f) {
				HAlign = 0.5f
			};
			title.Top.Set(8f, 0f);
			panel.Append(title);

			levelText = new UIText(string.Empty, 0.9f) {
				HAlign = 0.5f
			};
			levelText.Top.Set(40f, 0f);
			panel.Append(levelText);

			pointsText = new UIText(string.Empty, 0.9f) {
				HAlign = 0.5f
			};
			pointsText.Top.Set(62f, 0f);
			panel.Append(pointsText);

			UIElement line = new() {
				HAlign = 0.5f
			};
			line.Width.Set(lineWidth, 0f);
			line.Height.Set(NodeSize, 0f);
			line.Top.Set(100f, 0f);
			panel.Append(line);

			for (int i = 0; i < SkillTree.LifeLineLength; i++) {
				float left = i * (NodeSize + NodeGap);
				if (i > 0) {
					SkillTreeConnector connector = new(i);
					connector.Left.Set(left - NodeGap, 0f);
					connector.Top.Set(NodeSize / 2f - 2f, 0f);
					connector.Width.Set(NodeGap, 0f);
					connector.Height.Set(4f, 0f);
					line.Append(connector);
				}

				SkillTreeNode node = new(i);
				node.Left.Set(left, 0f);
				node.Width.Set(NodeSize, 0f);
				node.Height.Set(NodeSize, 0f);
				line.Append(node);
			}

			UITextPanel<LocalizedText> close = new(Language.GetText("Mods.Riftaria.UI.Close"), 0.85f) {
				HAlign = 0.5f,
				VAlign = 1f,
				BackgroundColor = new Color(70, 84, 140)
			};
			close.Width.Set(120f, 0f);
			close.Height.Set(32f, 0f);
			close.OnLeftClick += (_, _) => ModContent.GetInstance<SkillTreeUISystem>().Close();
			panel.Append(close);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			RiftLevelPlayer progress = Main.LocalPlayer.GetModPlayer<RiftLevelPlayer>();
			levelText.SetText(Language.GetTextValue("Mods.Riftaria.UI.LevelLine", progress.Level, progress.Experience, progress.XpToNext));
			pointsText.SetText(Language.GetTextValue("Mods.Riftaria.UI.SkillPoints", progress.SkillPoints));

			if (panel.ContainsPoint(Main.MouseScreen)) {
				Main.LocalPlayer.mouseInterface = true;
			}
		}
	}

	/// <summary>One +life node on the line.</summary>
	public class SkillTreeNode : UIPanel
	{
		private static readonly Color Unlocked = new(220, 180, 60);
		private static readonly Color Available = new(130, 90, 200);
		private static readonly Color Locked = new(70, 70, 80);

		private readonly int index;

		public SkillTreeNode(int index)
		{
			this.index = index;
			SetPadding(0f);
			OnLeftClick += (_, _) => Buy();
		}

		private void Buy()
		{
			RiftLevelPlayer progress = Main.LocalPlayer.GetModPlayer<RiftLevelPlayer>();
			if (index == progress.NodesUnlocked && progress.TryUnlockNext()) {
				SoundEngine.PlaySound(SoundID.Item4);
			}
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			RiftLevelPlayer progress = Main.LocalPlayer.GetModPlayer<RiftLevelPlayer>();
			Color color = index < progress.NodesUnlocked
				? Unlocked
				: index == progress.NodesUnlocked && progress.CanUnlockNext ? Available : Locked;

			BackgroundColor = color * 0.85f;
			BorderColor = IsMouseHovering ? Color.White : Color.Black;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);

			if (IsMouseHovering) {
				Main.hoverItemName = Language.GetTextValue("Mods.Riftaria.UI.LifeNode", SkillTree.LifePerNode);
			}
		}
	}

	/// <summary>The bar between two nodes, lit once the node to its right is unlocked.</summary>
	public class SkillTreeConnector : UIElement
	{
		private readonly int index;

		public SkillTreeConnector(int index)
		{
			this.index = index;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			bool lit = index < Main.LocalPlayer.GetModPlayer<RiftLevelPlayer>().NodesUnlocked;
			Color color = lit ? new Color(220, 180, 60) : new Color(70, 70, 80);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, GetDimensions().ToRectangle(), color);
		}
	}
}
