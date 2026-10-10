using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Riftaria.Common.Drops;
using Riftaria.Common.Items;
using Riftaria.Common.Players;

namespace Riftaria.Content.Items
{
	/// <summary>
	/// Rarity and affixes on a vanilla item. Weapon stats are local and only
	/// change this item. Global stats on armor and accessories feed
	/// <see cref="RiftStatPlayer"/> while equipped.
	/// </summary>
	public class RiftAffixItem : GlobalItem
	{
		public static readonly Color MagicColor = new(136, 136, 255);

		public static readonly Color RareColor = new(255, 255, 119);

		public override bool InstancePerEntity => true;

		public bool Rolled { get; private set; }

		public Rarity Rarity { get; private set; }

		public string RolledName { get; private set; } = "";

		public int Level { get; private set; }

		public IReadOnlyList<RolledAffix> Affixes => _affixes;

		private List<RolledAffix> _affixes = new();

		private int[] _sums = new int[Stats.Count];

		public void Apply(RolledItem rolled)
		{
			Rolled = true;
			Rarity = rolled.Rarity;
			RolledName = rolled.Name ?? "";
			Level = rolled.Level;
			_affixes = new List<RolledAffix>(rolled.Affixes);
			Recount();
		}

		public int Sum(StatId stat) => Rolled ? _sums[(int)stat] : 0;

		public override GlobalItem Clone(Item from, Item to)
		{
			RiftAffixItem clone = (RiftAffixItem)base.Clone(from, to);
			clone._affixes = new List<RolledAffix>(_affixes);
			clone._sums = (int[])_sums.Clone();
			return clone;
		}

		public override bool CanReforge(Item item) => !Rolled;

		public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand) => Rolled ? false : null;

		public override void UpdateInventory(Item item, Player player) => ApplyName(item);

		public override void HoldItem(Item item, Player player) => ApplyName(item);

		public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
		{
			if (!Rolled) {
				return;
			}

			damage.Base += Sum(StatId.AddedDamage);
			damage += Sum(StatId.LocalIncreasedDamage) / 100f;
		}

		public override float UseSpeedMultiplier(Item item, Player player) => 1f + Sum(StatId.LocalAttackSpeed) / 100f;

		public override void ModifyWeaponCrit(Item item, Player player, ref float crit) => crit += Sum(StatId.LocalCrit);

		public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
		{
			if (Rolled) {
				knockback += Sum(StatId.LocalKnockback) / 100f;
			}
		}

		public override void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
		{
			if (Rolled) {
				reduce += Sum(StatId.LocalReducedManaCost) / 100f;
			}
		}

		public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			if (Rolled) {
				velocity *= 1f + Sum(StatId.LocalProjectileSpeed) / 100f;
			}
		}

		/// <summary>Runs for armor and accessories while equipped.</summary>
		public override void UpdateEquip(Item item, Player player)
		{
			if (!Rolled) {
				return;
			}

			int increased = Sum(StatId.LocalIncreasedDefense);
			if (increased != 0) {
				player.statDefense += item.defense * increased / 100;
			}

			RiftStatPlayer sheet = player.GetModPlayer<RiftStatPlayer>();
			for (int i = 0; i < _sums.Length; i++) {
				if (_sums[i] != 0 && !IsLocal((StatId)i)) {
					sheet.Add((StatId)i, _sums[i]);
				}
			}
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (!Rolled) {
				return;
			}

			Color color = RarityColor(Rarity);
			int at = tooltips.FindIndex(line => line.Name == "ItemName");
			if (at >= 0) {
				tooltips[at].Text = RolledName.Length > 0 ? RolledName : Lang.GetItemNameValue(item.type);
				tooltips[at].OverrideColor = color;
			}

			var added = new List<TooltipLine>();
			if (Rarity == Rarity.Rare) {
				added.Add(new TooltipLine(Mod, "RiftBase", Lang.GetItemNameValue(item.type)) { OverrideColor = color });
			}

			added.Add(new TooltipLine(Mod, "RiftLevel", "Item level " + Level) { OverrideColor = Color.Gray });
			AffixFile file = LootFiles.Affixes;
			for (int i = 0; i < _affixes.Count; i++) {
				added.Add(new TooltipLine(Mod, "RiftAffix" + i, AffixTable.Describe(file, _affixes[i])) { OverrideColor = MagicColor });
			}

			int after = tooltips.FindLastIndex(line => line.Mod == "Terraria" && line.Name != "Price" && !line.Name.StartsWith("Sell"));
			tooltips.InsertRange(after < 0 ? tooltips.Count : after + 1, added);
		}

		public override void SaveData(Item item, TagCompound tag)
		{
			if (!Rolled) {
				return;
			}

			var list = new List<TagCompound>();
			foreach (RolledAffix affix in _affixes) {
				list.Add(new TagCompound {
					["id"] = affix.Id ?? "",
					["prefix"] = affix.Prefix,
					["stat"] = affix.Stat ?? "",
					["tier"] = affix.Tier,
					["value"] = affix.Value
				});
			}

			tag["rarity"] = (int)Rarity;
			tag["name"] = RolledName ?? "";
			tag["level"] = Level;
			tag["affixes"] = list;
		}

		public override void LoadData(Item item, TagCompound tag)
		{
			if (!tag.ContainsKey("rarity")) {
				return;
			}

			Rolled = true;
			Rarity = (Rarity)tag.GetInt("rarity");
			RolledName = tag.GetString("name");
			Level = tag.GetInt("level");
			_affixes = new List<RolledAffix>();
			foreach (TagCompound entry in tag.GetList<TagCompound>("affixes")) {
				_affixes.Add(new RolledAffix {
					Id = entry.GetString("id"),
					Prefix = entry.GetBool("prefix"),
					Stat = entry.GetString("stat"),
					Tier = entry.GetInt("tier"),
					Value = entry.GetInt("value")
				});
			}

			Recount();
		}

		public override void NetSend(Item item, BinaryWriter writer)
		{
			writer.Write(Rolled);
			if (!Rolled) {
				return;
			}

			writer.Write((byte)Rarity);
			writer.Write(RolledName ?? "");
			writer.Write(Level);
			writer.Write((byte)_affixes.Count);
			foreach (RolledAffix affix in _affixes) {
				writer.Write(affix.Id ?? "");
				writer.Write(affix.Prefix);
				writer.Write(affix.Stat ?? "");
				writer.Write((byte)affix.Tier);
				writer.Write(affix.Value);
			}
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			Rolled = reader.ReadBoolean();
			_affixes = new List<RolledAffix>();
			if (!Rolled) {
				Recount();
				return;
			}

			Rarity = (Rarity)reader.ReadByte();
			RolledName = reader.ReadString();
			Level = reader.ReadInt32();
			int count = reader.ReadByte();
			for (int i = 0; i < count; i++) {
				_affixes.Add(new RolledAffix {
					Id = reader.ReadString(),
					Prefix = reader.ReadBoolean(),
					Stat = reader.ReadString(),
					Tier = reader.ReadByte(),
					Value = reader.ReadInt32()
				});
			}

			Recount();
		}

		public static Color RarityColor(Rarity rarity) => rarity switch {
			Rarity.Magic => MagicColor,
			Rarity.Rare => RareColor,
			_ => Color.White
		};

		public static bool IsLocal(StatId stat) => stat switch {
			StatId.AddedDamage => true,
			StatId.LocalIncreasedDamage => true,
			StatId.LocalAttackSpeed => true,
			StatId.LocalCrit => true,
			StatId.LocalProjectileSpeed => true,
			StatId.LocalKnockback => true,
			StatId.LocalReducedManaCost => true,
			StatId.LocalIncreasedDefense => true,
			_ => false
		};

		private void Recount()
		{
			Array.Clear(_sums);
			foreach (RolledAffix affix in _affixes) {
				if (Stats.TryParse(affix.Stat, out StatId stat)) {
					_sums[(int)stat] += affix.Value;
				}
			}
		}

		private void ApplyName(Item item)
		{
			if (!Rolled || RolledName.Length == 0) {
				return;
			}

			string name = Rarity == Rarity.Rare
				? RolledName + " " + Lang.GetItemNameValue(item.type)
				: RolledName;
			item.SetNameOverride(name);
		}
	}
}
