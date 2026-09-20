using System.Collections.Generic;
using SubworldLibrary;
using Terraria;
using Terraria.WorldBuilding;
using Rifthold.Common.Maps;
using Rifthold.Common.World;
using Rifthold.Content.Worldgen.Rift;

namespace Rifthold.Content.Subworlds
{
	/// <summary>
	/// One subworld class serves every tier. Subworlds register statically at load,
	/// so the tier cannot be baked into the type; it is handed across the world
	/// boundary through the copied-data pipeline instead.
	/// </summary>
	public class RiftSubworld : Subworld
	{
		private const string TierKey = "riftTier";
		private const string ClearedKey = "riftClearedTier";

		public override int Width => RiftLayout.Width;

		public override int Height => RiftLayout.Height;

		/// <summary>Rifts are disposable. Nothing is written to disk on the way out.</summary>
		public override bool ShouldSave => false;

		/// <summary>
		/// Left at the default so loot and experience earned inside the rift travel
		/// home with the player. Turning this on would revert the entire run.
		/// </summary>
		public override bool NoPlayerSaving => false;

		/// <summary>
		/// Vanilla world updating stays off, which conveniently kills liquid flow,
		/// grass spread and other ways a sealed layout could change under the player.
		/// </summary>
		public override bool NormalUpdates => false;

		public override List<GenPass> Tasks => new() {
			new RiftSeedPass(),
			new RiftLayoutPass()
		};

		public override void CopyMainWorldData()
			=> SubworldSystem.CopyWorldData(TierKey, RiftRunState.PendingTier);

		public override void ReadCopiedMainWorldData()
			=> RiftRunState.ActiveTier = SubworldSystem.ReadCopiedWorldData<int>(TierKey);

		public override void CopySubworldData()
			=> SubworldSystem.CopyWorldData(ClearedKey, RiftRunState.ObjectiveComplete ? RiftRunState.ActiveTier : 0);

		public override void ReadCopiedSubworldData()
			=> RiftRunState.LastClearedTier = SubworldSystem.ReadCopiedWorldData<int>(ClearedKey);

		public override void OnLoad()
		{
			// Permanent dusk. It reads as "somewhere else" and keeps lighting stable.
			Main.dayTime = false;
			Main.time = 6000;
			Main.bloodMoon = false;
			Main.eclipse = false;

			RiftLayout.WardenSpawned = false;
		}

		public override void OnEnter()
		{
			RiftRunState.ObjectiveComplete = false;
			RiftRunState.ExitCountdown = -1;
		}

		public override void OnExit()
		{
			RiftRunState.ExitCountdown = -1;
			RiftLayout.Reset();
		}
	}
}
