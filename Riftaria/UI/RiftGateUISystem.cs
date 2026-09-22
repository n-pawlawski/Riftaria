using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Riftaria.UI
{
	[Autoload(Side = ModSide.Client)]
	public class RiftGateUISystem : ModSystem
	{
		private UserInterface userInterface;
		private RiftGateUI panel;

		public bool Visible { get; private set; }

		public override void PostSetupContent()
		{
			userInterface = new UserInterface();
			panel = new RiftGateUI();
			panel.Activate();
		}

		public override void Unload()
		{
			userInterface = null;
			panel = null;
		}

		public void Open()
		{
			panel?.Rebuild();
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
			// Walking away from the gate should not leave the panel hanging open.
			if (Visible && Main.LocalPlayer is { active: true } player && player.dead) {
				Close();
			}

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
				"Riftaria: Rift Gate",
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
