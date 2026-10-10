using Terraria;
using Terraria.ModLoader;
using Riftaria.Common.Drops;
using Riftaria.Common.Maps;

namespace Riftaria.Common.Systems
{
	/// <summary>
	/// Holds the loot plan of the rift being played. Drops are rolled on the
	/// server, so only the server and single player need a plan.
	/// </summary>
	public class RiftLootSystem : ModSystem
	{
		private static RiftLootPlan _activePlan;

		/// <summary>
		/// Built from the rift's own random during generation. If the rift was
		/// entered some other way, a plan is built on first use instead.
		/// </summary>
		public static RiftLootPlan ActivePlan
		{
			get {
				if (_activePlan == null || _activePlan.Tier != RiftRunState.ActiveTier) {
					_activePlan = RiftLootPlan.Build(LootFiles.ReadLoot(), RiftRunState.ActiveTier, static max => Main.rand.Next(max));
				}

				return _activePlan;
			}
		}

		public static void BuildPlan(int tier, System.Func<int, int> next)
			=> _activePlan = RiftLootPlan.Build(LootFiles.ReadLoot(), tier, next);

		public static void Clear() => _activePlan = null;

		public override void Unload() => Clear();
	}
}
