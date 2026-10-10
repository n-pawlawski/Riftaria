using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Riftaria.Common.Drops;
using Riftaria.Common.Systems;

namespace Riftaria.Content.Items
{
	/// <summary>
	/// Dropped by a boss. The bag carries its rift's boss pool, because it can be
	/// opened back in the Hold after that rift is gone. Affixes are rolled when
	/// it opens.
	/// </summary>
	public class RiftLootBag : ModItem
	{
		public string BossId { get; set; } = RiftBossIds.Zombie;

		public int NpcType { get; set; }

		public int Tier { get; set; }

		/// <summary>Null on bags from before pools were stored. Those build a fresh pool for their tier.</summary>
		public BossPool Pool { get; set; }

		public override string Texture => "Terraria/Images/Item_" + ItemID.EyeOfCthulhuBossBag;

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.maxStack = 1;
			Item.consumable = true;
			Item.rare = ItemRarityID.Orange;
		}

		private bool _give;

		public override bool CanRightClick() => true;

		public override bool ConsumeItem(Player player) => _give;

		public override void RightClick(Player player)
		{
			_give = false;
			string boss = string.IsNullOrEmpty(BossId) ? RiftBossIds.Zombie : BossId;
			BossPool pool = Pool ?? RiftLootPlan.Build(LootFiles.ReadLoot(), Tier, static max => Main.rand.Next(max)).BossFor(boss);
			List<LootPick> picks = RiftLootPlan.RollBoss(pool, static max => Main.rand.Next(max));
			if (picks.Count == 0) {
				Say("This bag's pool is empty.");
				return;
			}

			foreach (LootPick pick in picks) {
				Item reward = RiftLootDrops.Create(pick, Tier);
				if (reward == null) {
					Say("No item named " + pick.Base.Item + ".");
					continue;
				}

				player.QuickSpawnItem(player.GetSource_OpenItem(Type), reward);
				_give = true;
			}
		}

		private static void Say(string text)
		{
			if (Main.netMode != NetmodeID.Server) {
				Main.NewText(text);
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			string boss = NpcType > 0 ? Lang.GetNPCNameValue(NpcType) : BossId;
			string items = Pool == null ? "" : $", {Pool.Items} item{(Pool.Items == 1 ? "" : "s")}";
			tooltips.Add(new TooltipLine(Mod, "LootSource", $"{boss}, tier {Tier}{items}"));
		}

		public override void SaveData(TagCompound tag)
		{
			tag["boss"] = BossId ?? "";
			tag["npc"] = NpcType;
			tag["tier"] = Tier;
			if (Pool != null) {
				tag["pool"] = DataJson.Write(Pool);
			}
		}

		public override void LoadData(TagCompound tag)
		{
			BossId = tag.GetString("boss");
			NpcType = tag.GetInt("npc");
			Tier = tag.GetInt("tier");
			string pool = tag.GetString("pool");
			Pool = string.IsNullOrEmpty(pool) ? null : DataJson.Read<BossPool>(pool);
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(BossId ?? "");
			writer.Write(NpcType);
			writer.Write(Tier);
			writer.Write(Pool == null ? "" : DataJson.Write(Pool));
		}

		public override void NetReceive(BinaryReader reader)
		{
			BossId = reader.ReadString();
			NpcType = reader.ReadInt32();
			Tier = reader.ReadInt32();
			string pool = reader.ReadString();
			Pool = string.IsNullOrEmpty(pool) ? null : DataJson.Read<BossPool>(pool);
		}

		/// <summary>Server or single player. The bag takes this rift's pool for the boss.</summary>
		public static void DropFrom(NPC npc, string bossId, int tier)
		{
			int who = Item.NewItem(npc.GetSource_Loot(), npc.getRect(), ModContent.ItemType<RiftLootBag>());
			if (who < 0 || who >= Main.maxItems) {
				return;
			}

			if (Main.item[who].ModItem is not RiftLootBag bag) {
				return;
			}

			bag.BossId = bossId;
			bag.NpcType = npc.type;
			bag.Tier = tier;
			bag.Pool = RiftLootSystem.ActivePlan.BossFor(bossId);

			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, who);
			}
		}
	}
}
