namespace Riftaria.Common.Maps
{
	/// <summary>
	/// State for the rift the player is entering or currently inside.
	/// <para>
	/// <see cref="PendingTier"/> is written in the hub just before entering and is
	/// the value handed across the world boundary. <see cref="ActiveTier"/> is the
	/// value read back inside the subworld. They are kept separate so a failed or
	/// interrupted transition cannot leave the hub thinking a run is live.
	/// </para>
	/// </summary>
	public static class RiftRunState
	{
		/// <summary>Tier queued up in the hub, copied into the subworld on entry.</summary>
		public static int PendingTier;

		/// <summary>Tier of the rift currently being played.</summary>
		public static int ActiveTier;

		/// <summary>Set once the Rift Warden dies.</summary>
		public static bool ObjectiveComplete;

		/// <summary>Ticks until an automatic return to the hub. Negative means no countdown.</summary>
		public static int ExitCountdown = -1;

		/// <summary>Tier cleared on the run just finished, read back in the hub for progression.</summary>
		public static int LastClearedTier;

		public static RiftTier ActiveTierData => RiftTierRegistry.Get(ActiveTier);

		public static void BeginRun(int tier)
		{
			PendingTier = tier;
			ObjectiveComplete = false;
			ExitCountdown = -1;
		}

		public static void Reset()
		{
			PendingTier = 0;
			ActiveTier = 0;
			ObjectiveComplete = false;
			ExitCountdown = -1;
			LastClearedTier = 0;
		}
	}
}
