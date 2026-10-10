using System;
using SubworldLibrary;
using Terraria;
using Terraria.Localization;
using Riftaria.Common.Maps;
using Riftaria.Common.World;
using Riftaria.Common.World.Pieces;
using Riftaria.Content.Subworlds;

namespace Riftaria.Common.Systems
{
	/// <summary>The one place that decides whether a player may enter a rift, and moves them.</summary>
	public static class RiftGateway
	{
		public static bool InRift => SubworldSystem.IsActive<RiftSubworld>();

		/// <summary>
		/// The highest tier currently openable. Tier one is always available; every
		/// other tier opens by clearing the one below it.
		/// </summary>
		public static int HighestUnlockedTier => Math.Min(
			RiftTierRegistry.MaxTier,
			HubWorldSystem.Instance.HighestTierCleared + 1);

		public static bool IsUnlocked(int tier)
			=> tier >= RiftTierRegistry.FirstTier && tier <= HighestUnlockedTier;

		/// <summary>
		/// Whether this player may open this tier. Separate from <see cref="TryEnter"/>
		/// so the Gate can grey a row out rather than only refusing on click.
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
				reason = Language.GetTextValue("Mods.Riftaria.Rift.NoSuchTier", tier);
				return false;
			}

			if (!IsUnlocked(tier)) {
				reason = Language.GetTextValue("Mods.Riftaria.Rift.Locked", tier - 1);
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
			// The subworld reads its size as it is created, so the plan has to
			// exist before the transition starts.
			RiftLayout.Plan = RiftPlans.For(RiftTierRegistry.Get(tier).Biome);
			RiftLayout.Plan.Roll();

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
