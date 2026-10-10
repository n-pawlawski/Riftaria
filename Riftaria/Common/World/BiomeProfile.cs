using System.Collections.Generic;

namespace Riftaria.Common.World
{
	/// <summary>
	/// The tiles and stamps for one rift biome. Generation is still one pass:
	/// the profile decides what that pass paints, not a second algorithm.
	/// </summary>
	public sealed class BiomeProfile
	{
		public string Id { get; init; }

		/// <summary>Mass behind the painted faces. The world border stays the sealed shell.</summary>
		public ushort Shell { get; init; }

		/// <summary>Ground under the surface, and the faces of walls.</summary>
		public ushort Fill { get; init; }

		/// <summary>The tile the player walks on.</summary>
		public ushort Surface { get; init; }

		/// <summary>Trunks, hanging teeth, and other solid dressing.</summary>
		public ushort Accent { get; init; }

		/// <summary>Foliage placed on accent, such as a tree crown. Zero skips it.</summary>
		public ushort Canopy { get; init; }

		public ushort Wall { get; init; }

		/// <summary>Hanging tile. Zero means the biome grows none.</summary>
		public ushort Vine { get; init; }

		/// <summary>Non-solid growth on the surface. Zero means bare ground.</summary>
		public ushort Plant { get; init; }

		/// <summary>How far the floor may drift from the baseline, in tiles.</summary>
		public int VerticalRange { get; init; }

		/// <summary>How many tiles of <see cref="Fill"/> sit under the surface.</summary>
		public int FillDepth { get; init; }

		/// <summary>Paint the ceiling with the surface tile so hanging growth has an anchor.</summary>
		public bool GrassCeiling { get; init; }

		/// <summary>Walls and ceilings touching the air get <see cref="Fill"/>, not the bare shell.</summary>
		public bool PaintFaces { get; init; }

		/// <summary>Ledges become platforms instead of solid <see cref="Accent"/> shelves.</summary>
		public bool LedgePlatforms { get; init; }

		/// <summary>Platform tile style for ledges and rungs.</summary>
		public int PlatformStyle { get; init; }

		/// <summary>Columns between torches.</summary>
		public int TorchSpacing { get; init; } = 10;

		public IReadOnlyList<IBiomeFeature> Features { get; init; }
	}
}
