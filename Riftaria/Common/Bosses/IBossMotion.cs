using Terraria;

namespace Riftaria.Common.Bosses
{
	/// <summary>
	/// One stretch of movement. <see cref="Step"/> runs every tick and returns
	/// true on the tick the stretch is over.
	/// </summary>
	public interface IBossMotion
	{
		bool Step(NPC npc);
	}
}
