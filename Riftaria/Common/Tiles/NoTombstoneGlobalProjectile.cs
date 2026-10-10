using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Riftaria.Common.World;

namespace Riftaria.Common.Tiles
{
	/// <summary>
	/// Death drops a gravestone, which is a projectile that places a tile when it
	/// lands. With placement locked it would never resolve, so it is removed at
	/// spawn instead of being left to fail repeatedly.
	/// </summary>
	public class NoTombstoneGlobalProjectile : GlobalProjectile
	{
		public override void OnSpawn(Projectile projectile, Terraria.DataStructures.IEntitySource source)
		{
			if (WorldEditPolicy.CanPlace()) {
				return;
			}

			// ProjAIStyleID.GraveMarker covers Tombstone, GraveMarker, Headstone,
			// Obelisk and every Rich Gravestone variant in one check.
			if (projectile.aiStyle == ProjAIStyleID.GraveMarker) {
				projectile.active = false;
			}
		}
	}
}
