using SubworldLibrary;
using Terraria;
using Terraria.Localization;
using Riftaria.Common.Maps;
using Riftaria.Common.Players;
using Riftaria.Content.Subworlds;

namespace Riftaria.Common.Systems
{
	/// <summary>The one place that decides whether a player may enter a rift, and moves them.</summary>
	public static class RiftGateway
	{
		public static bool InRift => SubworldSystem.IsActive<RiftSubworld>();

		/// <summary>
		/// Whether this player may open this tier. Separate from <see cref="TryEnter"/>
		/// so a caller can check before spending the sigil that pays for the run.
		/// </summary>
		public static bool CanEnter(Player player, int tier, bool ignoreRequirements, out string reason)
		{
			reason = null;

			if (InRift) {
				reason = Language.GetTextValue("Mods.Riftaria.Rift.AlreadyInRift");
				return false;
			}

			if (ignoreRequirements) {
				return true;
			}

			if (!RiftTierRegistry.Exists(tier)) {
				reason = Language.GetTextValue("Mods.Riftaria.Rift.NoSigil");
				return false;
			}

			RiftTier data = RiftTierRegistry.Get(tier);
			if (RiftariaPlayer.Get(player).Level < data.RequiredLevel) {
				reason = Language.GetTextValue("Mods.Riftaria.Rift.LevelTooLow", data.RequiredLevel);
				return false;
			}

			return true;
		}

		/// <summary>Validates entry and starts the transition.</summary>
		public static bool TryEnter(Player player, int tier, bool ignoreRequirements, out string reason)
		{
			if (!CanEnter(player, tier, ignoreRequirements, out reason)) {
				return false;
			}

			Enter(tier);
			return true;
		}

		/// <summary>Starts the transition with no checks. Only call after <see cref="CanEnter"/>.</summary>
		public static void Enter(int tier)
		{
			RiftRunState.BeginRun(tier);
			SubworldSystem.Enter<RiftSubworld>();
		}

		public static void ReturnToHub()
		{
			if (InRift) {
				SubworldSystem.Exit();
			}
		}
	}
}
