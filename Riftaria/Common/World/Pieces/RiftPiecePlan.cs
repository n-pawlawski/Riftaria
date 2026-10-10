using System;

namespace Riftaria.Common.World.Pieces
{
	/// <summary>
	/// One tier's rolled rift. Rolled before the subworld is created, because
	/// the world size is this grid.
	/// </summary>
	public sealed class RiftPiecePlan
	{
		/// <summary>Seeds tried before giving up. Each failed seed is a layout that could not fit.</summary>
		private const int Attempts = 50;

		public string Name { get; }

		public RiftCatalog Catalog { get; }

		/// <summary>Null until rolled.</summary>
		public RiftAssembly Assembly { get; private set; }

		public RiftPiecePlan(string name, RiftCatalog catalog)
		{
			Name = name;
			Catalog = catalog;
		}

		public bool Rolled => Assembly != null;

		public int Width => Assembly?.Grid.Width ?? 0;

		public int Height => Assembly?.Grid.Height ?? 0;

		public int RequiredWidth()
		{
			if (Assembly == null) {
				Roll();
			}

			return Width;
		}

		public int RequiredHeight()
		{
			if (Assembly == null) {
				Roll();
			}

			return Height;
		}

		public void Clear() => Assembly = null;

		public void Roll() => Roll(new Random().Next());

		/// <summary>The same seed gives the same rift.</summary>
		public void Roll(int seed)
		{
			var rand = new Random(seed);
			RiftAssembly result = null;
			for (int i = 0; i < Attempts && result == null; i++) {
				result = RiftAssembler.Build(Catalog, rand.Next());
			}

			Assembly = result ?? throw new InvalidOperationException($"The {Name} rift could not be assembled.");
		}

		public byte Cell(int x, int y) => Assembly == null ? RiftGrid.Rock : Assembly.Grid.Get(x, y);

		public bool Open(int x, int y) => Assembly != null && Assembly.Grid.IsOpen(x, y);

		public bool IsLedge(int x, int y) => Cell(x, y) == RiftGrid.Ledge;

		public bool IsPlatform(int x, int y) => Cell(x, y) == RiftGrid.Platform;
	}
}
