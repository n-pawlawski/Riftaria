using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Riftaria.Common.World;
using Riftaria.Common.World.Pieces;
using Riftaria.Content.Worldgen.Rift.Features;

namespace Riftaria.Content.Worldgen.Rift
{
	/// <summary>
	/// Terrain profiles. A new biome is a profile plus any features it needs;
	/// <see cref="RiftLayoutPass"/> already asks the profile for every tile.
	/// </summary>
	public static class BiomeCatalog
	{
		private static readonly Dictionary<string, BiomeProfile> Profiles = new() {
			[BiomeId.Forest] = Forest(),
			[BiomeId.Stone] = Stone(),
			[BiomeId.Evil] = Evil()
		};

		public static BiomeProfile Get(string id)
		{
			if (id != null && Profiles.TryGetValue(id, out BiomeProfile profile)) {
				return profile;
			}

			return Profiles[BiomeId.Stone];
		}

		private static BiomeProfile Forest() => new() {
			Id = BiomeId.Forest,
			Shell = TileID.Stone,
			Fill = TileID.Dirt,
			Surface = TileID.Grass,
			Accent = TileID.LivingWood,
			Canopy = TileID.LeafBlock,
			Wall = WallID.GrassUnsafe,
			VerticalRange = 10,
			FillDepth = 8,
			// Grass on the ceiling, weeds, vines and leaf blocks all draw as a
			// full square, then the chunk frame deletes or restyles them. The
			// floor stays grass. The solid shell does not pop.
			GrassCeiling = false,
			PaintFaces = true,
			TorchSpacing = 8
		};

		/// <summary>Tier 2. The stamp reads these tiles; the shape comes from <see cref="EvilPieces"/>.</summary>
		private static BiomeProfile Evil() => new() {
			Id = BiomeId.Evil,
			Shell = TileID.Ebonstone,
			Fill = TileID.Dirt,
			Surface = TileID.CorruptGrass,
			Wall = WallID.EbonstoneUnsafe,
			FillDepth = 4,
			LedgePlatforms = true,
			PlatformStyle = 1,
			TorchSpacing = 8
		};

		/// <summary>The previous flat rift's tiles. The shape comes from <see cref="StonePieces"/>.</summary>
		private static BiomeProfile Stone() => new() {
			Id = BiomeId.Stone,
			Shell = RiftLayout.FloorTile,
			Fill = RiftLayout.FloorTile,
			Surface = RiftLayout.FloorTile,
			Accent = RiftLayout.AccentTile,
			Wall = RiftLayout.BackWall,
			VerticalRange = 0,
			FillDepth = 4,
			Features = new IBiomeFeature[] {
				new CeilingAccentFeature()
			}
		};
	}
}
