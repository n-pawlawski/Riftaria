using System;

namespace Riftaria.Common.World
{
	/// <summary>
	/// The rift outline for this pass: one walkable rectangle, and eleven circles
	/// stamped along it. The circles overlap, so neighbouring rooms share an opening.
	/// The rectangle's floor stays filled. A circle may cross that line in the sketch,
	/// but the tiles under the floor are not carved, or the room would be a bowl.
	/// </summary>
	public static class RiftCirclePlan
	{
		public const int Count = 11;

		/// <summary>Rolled once per entry, before the subworld asks for its width.</summary>
		public static int[] Radii { get; private set; }

		public static int[] Centers { get; private set; }

		public static void Roll()
		{
			var rand = new Random();
			Radii = new int[Count];
			Centers = new int[Count];

			for (int i = 0; i < Count - 1; i++) {
				Radii[i] = rand.Next(14, 27);
			}

			// The last circle is the fight. It is always the largest.
			Radii[Count - 1] = rand.Next(32, 44);

			int center = RiftMeasures.SpawnX;
			int minCenter = RiftMeasures.Border + Radii[0] + 2;
			if (center < minCenter) {
				center = minCenter;
			}

			Centers[0] = center;

			for (int i = 0; i < Count - 1; i++) {
				int smaller = Radii[i] < Radii[i + 1] ? Radii[i] : Radii[i + 1];
				int low = 8;
				int high = (smaller * 2) / 3;
				if (high <= low) {
					high = low + 1;
				}

				int overlap = rand.Next(low, high);
				center += Radii[i] + Radii[i + 1] - overlap;
				Centers[i + 1] = center;
			}
		}

		/// <summary>Width of the subworld for the plan already rolled.</summary>
		public static int RequiredWidth()
		{
			if (Centers == null) {
				Roll();
			}

			int right = Centers[Count - 1] + Radii[Count - 1];
			return right + RiftMeasures.Buffer + RiftMeasures.Border;
		}

		public static void Clear()
		{
			Radii = null;
			Centers = null;
		}
	}
}
