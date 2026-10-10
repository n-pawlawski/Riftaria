using Terraria.ID;
using Terraria.ModLoader;
using Riftaria.Common.World;

namespace Riftaria.Common.Tiles
{
	/// <summary>
	/// Blocks tile destruction and placement. Every decision defers to
	/// <see cref="WorldEditPolicy"/> so opening up a mineable resource later is a
	/// one-line allowlist entry rather than an edit here.
	/// </summary>
	public class NoMiningGlobalTile : GlobalTile
	{
		/// <summary>Stops the tile registering a hit at all, rather than failing the swing.</summary>
		public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
			=> WorldEditPolicy.CanBreakTile(type);

		/// <summary>
		/// Backstop. Several code paths reach tile destruction without consulting
		/// CanKillTile, so the result is refused a second time here.
		/// <para>
		/// Weeds are the exception. Framing calls <c>PlantCheck</c>, which kills a
		/// plant it rejects and then frames the neighbours. Refusing that kill
		/// leaves the plant in place, so the next frame checks it again and the
		/// call stack never unwinds.
		/// </para>
		/// </summary>
		public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (IsEngineFoliage(type) || WorldEditPolicy.CanBreakTile(type)) {
				return;
			}

			fail = true;
			effectOnly = false;
			noItem = true;
		}

		/// <summary>Explosives must not be a way around the pickaxe rule.</summary>
		public override bool CanExplode(int i, int j, int type)
			=> WorldEditPolicy.CanBreakTile(type);

		public override bool CanPlace(int i, int j, int type) => WorldEditPolicy.IsPlacementAllowed(type);

		/// <summary>Plants and vines. The framing pass has to be able to remove a bad one.</summary>
		private static bool IsEngineFoliage(int type)
		{
			return type == TileID.Plants || type == TileID.Plants2 || type == TileID.Vines;
		}
	}
}
