using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Riftaria.Common.Drops;
using Riftaria.Common.Items;

namespace Riftaria.Content.Items
{
	/// <summary>Turns a <see cref="LootPick"/> into a real item with its affixes rolled.</summary>
	public static class RiftLootDrops
	{
		private static readonly Func<int, int> Next = static max => Main.rand.Next(max);

		/// <summary>Null when the base names an item that does not exist.</summary>
		public static Item Create(LootPick pick, int level)
		{
			int type = LootFiles.ResolveItem(pick?.Base?.Item);
			if (type <= 0) {
				return null;
			}

			var item = new Item();
			item.SetDefaults(type);
			Roll(item, pick, level);
			return item;
		}

		/// <summary>
		/// Drops a rolled item into the world. Server or single player only;
		/// enemies, bosses and later chests all go through here.
		/// </summary>
		public static void SpawnRolled(IEntitySource source, Rectangle area, LootPick pick, int level)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			int type = LootFiles.ResolveItem(pick?.Base?.Item);
			if (type <= 0) {
				return;
			}

			int who = Item.NewItem(source, area, type, 1, noBroadcast: true, prefixGiven: 0);
			if (who < 0 || who >= Main.maxItems) {
				return;
			}

			Roll(Main.item[who], pick, level);
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, who);
			}
		}

		private static void Roll(Item item, LootPick pick, int level)
		{
			RolledItem rolled = ItemRoller.RollItem(
				LootFiles.Affixes, pick.Base.Item, pick.Base.Tag, level, pick.Rarity,
				Lang.GetItemNameValue(item.type), Next);
			item.GetGlobalItem<RiftAffixItem>().Apply(rolled);
		}
	}
}
