namespace Riftaria.Common.World
{
	/// <summary>
	/// A stamp applied after a room is carved. It may add geometry, but the
	/// walk along the floor has to stay open: anything solid belongs above head height.
	/// </summary>
	public interface IBiomeFeature
	{
		void Apply(BiomeRoom room, BiomeProfile profile);
	}
}
