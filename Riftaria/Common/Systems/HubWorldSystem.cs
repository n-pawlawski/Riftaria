using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using Riftaria.Common.Maps;
using Riftaria.Content.Worldgen;

namespace Riftaria.Common.Systems
{
	/// <summary>
	/// Owns the main world. Riftaria is a total conversion: every world created
	/// while it is enabled becomes a Hold, because vanilla generation is replaced
	/// rather than added to.
	/// </summary>
	public class HubWorldSystem : ModSystem
	{
		/// <summary>The only vanilla pass kept; it initialises the tilemap.</summary>
		private const string ResetPassName = "Reset";

		/// <summary>Highest rift tier cleared in this world. Drives hub growth.</summary>
		public int HighestTierCleared;

		/// <summary>Set by the generation pass so loading a non-Hold world can be detected.</summary>
		public bool IsHoldWorld;

		public static HubWorldSystem Instance => ModContent.GetInstance<HubWorldSystem>();

		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			foreach (GenPass pass in tasks) {
				if (pass.Name != ResetPassName) {
					pass.Disable();
				}
			}

			tasks.Add(new HubGenPass());
		}

		/// <summary>The Hold has no hardmode, so none of the conversion passes should run.</summary>
		public override void ModifyHardmodeTasks(List<GenPass> list)
		{
			foreach (GenPass pass in list) {
				pass.Disable();
			}
		}

		public override void PostWorldGen()
		{
			IsHoldWorld = true;
			HighestTierCleared = 0;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["highestTierCleared"] = HighestTierCleared;
			tag["isHoldWorld"] = IsHoldWorld;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			HighestTierCleared = tag.ContainsKey("highestTierCleared") ? tag.GetInt("highestTierCleared") : 0;
			IsHoldWorld = tag.ContainsKey("isHoldWorld") && tag.GetBool("isHoldWorld");
		}

		public override void OnWorldLoad()
		{
			// A run never survives a world transition; only the hub remembers progress.
			RiftRunState.ObjectiveComplete = false;
			RiftRunState.ExitCountdown = -1;
			if (IsHoldWorld) {
				CastleFront.EnsureStarterGear();
			}
		}

		/// <summary>Records a clear and returns true when this unlocked a new tier.</summary>
		public bool RecordClear(int tier)
		{
			if (tier <= HighestTierCleared) {
				return false;
			}

			HighestTierCleared = tier;
			return true;
		}
	}
}
