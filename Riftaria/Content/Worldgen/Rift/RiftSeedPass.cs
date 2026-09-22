using Terraria;
using Terraria.IO;
using Terraria.WorldBuilding;

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
		}
	}
}
