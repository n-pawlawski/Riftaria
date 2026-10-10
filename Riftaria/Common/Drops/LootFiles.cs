using System;
using System.IO;
using System.Reflection;
using System.Text;
using Terraria.ID;
using Riftaria.Common.Items;

namespace Riftaria.Common.Drops
{
	/// <summary>
	/// The data files on disk. A dev build reads the source copy, so an edited
	/// file applies without another build. A packed build uses the copy stored
	/// in the mod.
	/// </summary>
	public static class LootFiles
	{
		public const string LootPath = "Common/Drops/loot-pools.js";

		public const string AffixPath = "Common/Items/affixes.js";

		private static AffixFile _affixes;

		private static DateTime _affixStamp;

		private static DateTime _nextAffixCheck;

		public static LootFile ReadLoot() => LootTable.Parse(Read(LootPath));

		/// <summary>
		/// Tooltips read this every frame, so the parsed file is cached and the
		/// source copy is checked for edits at most once a second.
		/// </summary>
		public static AffixFile Affixes
		{
			get {
				DateTime now = DateTime.UtcNow;
				if (_affixes != null && now < _nextAffixCheck) {
					return _affixes;
				}

				_nextAffixCheck = now.AddSeconds(1);
				string source = SourcePath(AffixPath);
				DateTime stamp = source != null && File.Exists(source) ? File.GetLastWriteTimeUtc(source) : DateTime.MinValue;
				if (_affixes == null || stamp != _affixStamp) {
					_affixes = AffixTable.Parse(Read(AffixPath));
					_affixStamp = stamp;
				}

				return _affixes;
			}
		}

		public static void Reset()
		{
			_affixes = null;
			_affixStamp = default;
			_nextAffixCheck = default;
		}

		public static string Read(string packedPath)
		{
			string source = SourcePath(packedPath);
			if (!string.IsNullOrEmpty(source) && File.Exists(source)) {
				return File.ReadAllText(source);
			}

			byte[] packed = Riftaria.Instance?.GetFileBytes(packedPath);
			return packed == null ? "" : Encoding.UTF8.GetString(packed);
		}

		/// <summary>A vanilla item id constant name, such as WoodenBow.</summary>
		public static int ResolveItem(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) {
				return 0;
			}

			FieldInfo field = typeof(ItemID).GetField(name.Trim(), BindingFlags.Public | BindingFlags.Static);
			if (field == null || !field.IsLiteral) {
				return 0;
			}

			object value = field.GetValue(null);
			if (value is short or int) {
				return Convert.ToInt32(value);
			}

			return 0;
		}

		private static string SourcePath(string packedPath)
		{
			string folder = Riftaria.Instance?.SourceFolder;
			if (string.IsNullOrEmpty(folder)) {
				return null;
			}

			return Path.Combine(folder, packedPath.Replace('/', Path.DirectorySeparatorChar));
		}
	}
}
