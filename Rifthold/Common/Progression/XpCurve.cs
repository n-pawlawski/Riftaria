namespace Rifthold.Common.Progression
{
	/// <summary>Level thresholds. Quadratic, so later tiers need several clears.</summary>
	public static class XpCurve
	{
		public const int MaxLevel = 50;

		/// <summary>Stat points granted per level.</summary>
		public const int PointsPerLevel = 2;

		/// <summary>Experience needed to get from <paramref name="level"/> to the next one.</summary>
		public static long XpToNextLevel(int level)
		{
			if (level >= MaxLevel) {
				return long.MaxValue;
			}

			if (level < 1) {
				level = 1;
			}

			return 50 + (25L * level) + (5L * level * level);
		}
	}
}
