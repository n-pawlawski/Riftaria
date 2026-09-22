using Terraria.ModLoader;
using Riftaria.Common.Maps;
using Riftaria.Common.Progression;
using Riftaria.Common.World;

namespace Riftaria
{
	// Split across files: see Riftaria.Networking.cs for packet handling.
	public partial class Riftaria : Mod
	{
		public static Riftaria Instance => ModContent.GetInstance<Riftaria>();

		/// <summary>Opens the character panel.</summary>
		public static ModKeybind CharacterPanelKey { get; private set; }

		public override void Load()
		{
			CharacterPanelKey = KeybindLoader.RegisterKeybind(this, "CharacterPanel", "K");
		}

		public override void Unload()
		{
			CharacterPanelKey = null;

			// Static registries survive an assembly reload unless cleared explicitly.
			WorldEditPolicy.Reset();
			StatTree.Reset();
			RiftTierRegistry.Reset();
			RiftRunState.Reset();
			RiftLayout.Reset();
		}
	}
}
