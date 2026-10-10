using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Riftaria.UI
{
	[Autoload(Side = ModSide.Client)]
	public class SkillTreeUISystem : ModSystem
	{
		public static ModKeybind ToggleKey { get; private set; }

		private UserInterface userInterface;
		private SkillTreeUI panel;

		public bool Visible { get; private set; }

		public override void Load()
		{
			ToggleKey = KeybindLoader.RegisterKeybind(Mod, "SkillTree", "K");
		}

		public override void PostSetupContent()
		{
			userInterface = new UserInterface();
			panel = new SkillTreeUI();
			panel.Activate();
		}

		public override void Unload()
		{
			ToggleKey = null;
			userInterface = null;
			panel = null;
		}

		public void Open()
		{
			userInterface?.SetState(panel);
			Visible = true;
		}

		public void Close()
		{
			userInterface?.SetState(null);
			Visible = false;
		}

		public void Toggle()
		{
			if (Visible) {
				Close();
			}
			else {
				Open();
			}
		}

		public override void UpdateUI(GameTime gameTime)
		{
			if (userInterface?.CurrentState != null) {
				userInterface.Update(gameTime);
			}
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (index == -1) {
				return;
			}

			layers.Insert(index, new LegacyGameInterfaceLayer(
				"Riftaria: Skill Tree",
				() => {
					if (userInterface?.CurrentState != null) {
						userInterface.Draw(Main.spriteBatch, new GameTime());
					}

					return true;
				},
				InterfaceScaleType.UI));
		}
	}
}
