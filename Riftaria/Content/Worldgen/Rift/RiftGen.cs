using Terraria.Utilities;

namespace Riftaria.Content.Worldgen.Rift
{
	/// <summary>
	/// Per-rift random. <see cref="Terraria.WorldGen.genRand"/> is read-only and
	/// keeps the hub's seed, so a rift rolls its own generator before carving.
	/// </summary>
	internal static class RiftGen
	{
		public static UnifiedRandom Rand { get; private set; } = new();

		public static void Reroll() => Rand = new UnifiedRandom();
	}
}
