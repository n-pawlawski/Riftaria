using Terraria;
using Terraria.IO;
using Terraria.WorldBuilding;
using Riftaria.Common.Maps;
using Riftaria.Common.Systems;

namespace Riftaria.Content.Worldgen.Rift
{
	/// <summary>
	/// Subworlds inherit the main world's seed, which would make every rift
	/// identical. This re-rolls it before anything else consumes randomness.
	/// </summary>
	public class RiftSeedPass : GenPass
	{
		public RiftSeedPass() : base("Rift Seed", 0.01f) { }

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Destabilising";
			Main.ActiveWorldFileData.SetSeedToRandom();

			// genRand is read-only and still holds the hub seed. The layout reads
			// RiftGen instead, so two entries from one hub do not carve the same floor.
			RiftGen.Reroll();
			RiftLootSystem.BuildPlan(RiftRunState.ActiveTier, static max => RiftGen.Rand.Next(max));
		}
	}
}
