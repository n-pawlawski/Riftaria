using System.Collections.Generic;
using Riftaria.Common.World;

namespace Riftaria.Content.Worldgen.Rift
{
	/// <summary>
	/// Places the eleven circles on the rectangle. The floor itself is
	/// <see cref="RiftCircleFloors"/>, shared with the preview.
	/// </summary>
	internal static class RiftFloorPlan
	{
		public static List<BiomeRoom> Build(int verticalRange, out int[] floors)
		{
			if (RiftCirclePlan.Centers == null) {
				RiftCirclePlan.Roll();
			}

			floors = RiftCircleFloors.Build(
				RiftLayout.Width,
				verticalRange,
				static (min, max) => RiftGen.Rand.Next(min, max));
			return RiftCircleFloors.MakeRooms(floors);
		}
	}
}
