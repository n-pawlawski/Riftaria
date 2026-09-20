using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Rifthold.Common.Maps;
using Rifthold.Content.NPCs;

namespace Rifthold.Common.Systems
{
	/// <summary>Controls what spawns inside a rift, and how fast.</summary>
	public class RiftSpawnSystem : ModSystem
	{
		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
		{
			if (!RiftGateway.InRift) {
				return;
			}

			spawnRate = 24;
			maxSpawns = RiftRunState.ActiveTierData.EnemyBudget;
		}

		/// <summary>
		/// Strips everything the rift did not ask for. The pool is already populated
		/// with vanilla candidates and our own <c>SpawnChance</c> entries by this
		/// point, so this filters rather than clears; clearing would drop ours too.
		/// </summary>
		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
		{
			if (!RiftGateway.InRift) {
				return;
			}

			List<int> foreign = new();
			foreach (int type in pool.Keys) {
				if (ModContent.GetModNPC(type) is not RiftEnemy) {
					foreign.Add(type);
				}
			}

			foreach (int type in foreign) {
				pool.Remove(type);
			}
		}
	}
}
