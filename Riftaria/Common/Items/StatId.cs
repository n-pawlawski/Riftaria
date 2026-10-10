using System;

namespace Riftaria.Common.Items
{
	/// <summary>
	/// Every stat an affix can roll. The names are the stat ids in affixes.js.
	/// Local stats only change the item they sit on. The rest are summed into
	/// the player's stat sheet from equipped armor and accessories.
	/// </summary>
	public enum StatId
	{
		AddedDamage,
		LocalIncreasedDamage,
		LocalAttackSpeed,
		LocalCrit,
		LocalProjectileSpeed,
		LocalKnockback,
		LocalReducedManaCost,
		LocalIncreasedDefense,
		IncreasedDamage,
		AttackSpeed,
		Crit,
		Defense,
		MaxLife,
		MaxMana,
		LifeRegen,
		MoveSpeed,
		DamageReduction
	}

	public static class Stats
	{
		public static readonly int Count = Enum.GetValues(typeof(StatId)).Length;

		/// <summary>Names only. A number such as "3" is not a stat id.</summary>
		public static bool TryParse(string id, out StatId stat)
		{
			stat = default;
			if (string.IsNullOrEmpty(id) || !char.IsLetter(id[0])) {
				return false;
			}

			return Enum.TryParse(id, false, out stat) && Enum.IsDefined(typeof(StatId), stat);
		}
	}
}
