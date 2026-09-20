using Terraria.ModLoader;
using Rifthold.Common.World;

namespace Rifthold.Common.Tiles
{
	/// <summary>The same lockdown for walls, which hammers would otherwise strip.</summary>
	public class NoMiningGlobalWall : GlobalWall
	{
		public override void KillWall(int i, int j, int type, ref bool fail)
		{
			if (!WorldEditPolicy.CanBreakWall(type)) {
				fail = true;
			}
		}

		public override bool CanExplode(int i, int j, int type) => WorldEditPolicy.CanBreakWall(type);

		public override bool CanPlace(int i, int j, int type) => WorldEditPolicy.CanPlace();
	}
}
