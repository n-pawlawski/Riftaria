using Terraria.ModLoader;
using Rifthold.Common.Maps;
using Rifthold.Common.Progression;
using Rifthold.Common.World;

namespace Rifthold
{
	// Split across files: see Rifthold.Networking.cs for packet handling.
	public partial class Rifthold : Mod
	{
		public static Rifthold Instance => ModContent.GetInstance<Rifthold>();

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
