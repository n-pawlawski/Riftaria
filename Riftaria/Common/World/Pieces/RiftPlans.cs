namespace Riftaria.Common.World.Pieces
{
	/// <summary>The plan each biome builds from.</summary>
	public static class RiftPlans
	{
		public static RiftPiecePlan Forest { get; } = new("forest", ForestPieces.Catalog);

		public static RiftPiecePlan Evil { get; } = new("evil", EvilPieces.Catalog);

		public static RiftPiecePlan Stone { get; } = new("stone", StonePieces.Catalog);

		public static RiftPiecePlan For(string biome) => biome switch {
			BiomeId.Evil => Evil,
			BiomeId.Stone => Stone,
			_ => Forest
		};

		public static void ClearAll()
		{
			Forest.Clear();
			Evil.Clear();
			Stone.Clear();
		}
	}
}
