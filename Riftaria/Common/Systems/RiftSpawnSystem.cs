using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Riftaria.Common.Systems
{
	/// <summary>
	/// Spawn rules for both worlds. The Hold is a safe room. A rift is too:
	/// its enemies are placed on the path, not rolled from the ambient pool.
	/// </summary>
	public class RiftSpawnSystem : GlobalNPC
	{
		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
		{
			spawnRate = 0;
			maxSpawns = 0;
		}

		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
		{
			if (!RiftGateway.InRift) {
				return;
			}

			pool.Clear();
		}
	}
}
