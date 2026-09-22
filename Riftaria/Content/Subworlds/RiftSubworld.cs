using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.Localization;
using Terraria.WorldBuilding;
using Riftaria.Common.Maps;
using Riftaria.Common.Systems;
using Riftaria.Common.World;
using Riftaria.Content.Worldgen.Rift;

namespace Riftaria.Content.Subworlds
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

		/// <summary>
		/// Runs once the destination world is loaded, which is the only moment the
		/// clear can be banked: the rift's own world data is thrown away on exit, so
		/// nothing written inside the subworld would survive.
		/// </summary>
		public override void ReadCopiedSubworldData()
		{
			int cleared = SubworldSystem.ReadCopiedWorldData<int>(ClearedKey);
			RiftRunState.LastClearedTier = cleared;

			if (cleared > 0 && HubWorldSystem.Instance.RecordClear(cleared)) {
				RiftRunSystem.Announce(
					Language.GetTextValue("Mods.Riftaria.Rift.TierUnlocked", cleared + 1),
					new Color(190, 160, 255));
			}
		}

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
