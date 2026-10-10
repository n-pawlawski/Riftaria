using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Riftaria.Common.Maps;
using Riftaria.Common.World;
using Riftaria.Common.World.Pieces;
using Riftaria.Content.NPCs;

namespace Riftaria.Content.Worldgen.Rift
{
	/// <summary>
	/// Carves the rift rolled at entry, snapped together from the tier's biome
	/// pieces, then sets its boss and the enemies along the way.
	/// </summary>
	public class RiftLayoutPass : GenPass
	{
		public RiftLayoutPass() : base("Rift Layout", 100f) { }

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Carving the rift";

			RiftTier tier = RiftRunState.ActiveTierData;
			BiomeProfile biome = BiomeCatalog.Get(tier.Biome);
			RiftPiecePlan plan = RiftLayout.Plan;
			RiftPieceStamp.Apply(progress, biome, plan);
			RiftLayout.ObjectiveNpc = Objective(biome.Id);
			RiftPieceStamp.PlaceEnemies(plan, PathKinds(biome.Id));
			progress.Set(1f);
		}

		/// <summary>Zero spawns the rift zombie.</summary>
		private static int Objective(string biome) => biome switch {
			BiomeId.Evil => ModContent.NPCType<RiftEye>(),
			BiomeId.Stone => ModContent.NPCType<RiftGolem>(),
			_ => 0
		};

		/// <summary>NPC for each mark kind in the biome's pieces.</summary>
		private static int[] PathKinds(string biome) => biome switch {
			BiomeId.Evil => new int[] { NPCID.EaterofSouls, NPCID.Crimera, NPCID.Corruptor },
			BiomeId.Stone => new int[] { NPCID.Skeleton, NPCID.GraniteGolem, NPCID.CaveBat, NPCID.UndeadMiner },
			_ => new int[] { NPCID.Zombie, NPCID.BlueSlime }
		};
	}
}
