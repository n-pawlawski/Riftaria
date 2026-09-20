using SubworldLibrary;
using Terraria;
using Terraria.Localization;
using Rifthold.Common.Maps;
using Rifthold.Common.Players;
using Rifthold.Content.Subworlds;

namespace Rifthold.Common.Systems
{
	/// <summary>The one place that decides whether a player may enter a rift, and moves them.</summary>
	public static class RiftGateway
	{
		public static bool InRift => SubworldSystem.IsActive<RiftSubworld>();

		/// <summary>
		/// Validates entry and starts the transition. Returns false with a
		/// player-facing <paramref name="reason"/> when the run was refused.
		/// </summary>
		public static bool TryEnter(Player player, int tier, bool ignoreRequirements, out string reason)
		{
			reason = null;

			if (InRift) {
				reason = Language.GetTextValue("Mods.Rifthold.Rift.AlreadyInRift");
				return false;
			}

			if (!ignoreRequirements) {
				if (!RiftTierRegistry.Exists(tier)) {
					reason = Language.GetTextValue("Mods.Rifthold.Rift.NoSigil");
					return false;
				}

				RiftTier data = RiftTierRegistry.Get(tier);
				RiftholdPlayer progression = RiftholdPlayer.Get(player);
				if (progression.Level < data.RequiredLevel) {
					reason = Language.GetTextValue("Mods.Rifthold.Rift.LevelTooLow", data.RequiredLevel);
					return false;
				}
			}

			RiftRunState.BeginRun(tier);
			SubworldSystem.Enter<RiftSubworld>();
			return true;
		}

		public static void ReturnToHub()
		{
			if (InRift) {
				SubworldSystem.Exit();
			}
		}
	}
}
