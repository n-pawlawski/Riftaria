using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Rifthold.Common.Net;
using Rifthold.Common.Progression;
using Rifthold.Common.World;

namespace Rifthold.Common.Players
{
	/// <summary>
	/// Per-character progression: experience, level, unspent points and the ranks
	/// invested in each <see cref="StatNode"/>.
	/// </summary>
	public class RiftholdPlayer : ModPlayer
	{
		public int Level = 1;
		public long Xp;
		public int UnspentPoints;

		private readonly Dictionary<string, int> ranks = new();

		public static RiftholdPlayer Get(Player player) => player.GetModPlayer<RiftholdPlayer>();

		public long XpToNextLevel => XpCurve.XpToNextLevel(Level);

		public float XpProgress
		{
			get {
				long needed = XpToNextLevel;
				if (needed <= 0 || needed == long.MaxValue) {
					return 1f;
				}

				return MathHelper.Clamp(Xp / (float)needed, 0f, 1f);
			}
		}

		public int RankOf(string nodeId) => ranks.TryGetValue(nodeId, out int rank) ? rank : 0;

		public bool IsUnlocked(StatNode node) => StatTree.IsUnlocked(node, Level, RankOf);

		public bool CanSpendOn(StatNode node)
			=> node != null && UnspentPoints > 0 && RankOf(node.Id) < node.MaxRank && IsUnlocked(node);

		/// <summary>Spends one point. Returns false when the spend was not legal.</summary>
		public bool TrySpendPoint(StatNode node)
		{
			if (!CanSpendOn(node)) {
				return false;
			}

			ranks[node.Id] = RankOf(node.Id) + 1;
			UnspentPoints--;
			return true;
		}

		/// <summary>
		/// Adds experience and resolves any level-ups. Call this on the server (or in
		/// single player); it pushes the result to the owning client itself.
		/// </summary>
		public void GrantXp(long amount)
		{
			if (amount <= 0 || Level >= XpCurve.MaxLevel) {
				return;
			}

			Xp += amount;

			bool leveled = false;
			while (Level < XpCurve.MaxLevel && Xp >= XpToNextLevel) {
				Xp -= XpToNextLevel;
				Level++;
				UnspentPoints += XpCurve.PointsPerLevel;
				leveled = true;
			}

			if (Level >= XpCurve.MaxLevel) {
				Xp = 0;
			}

			if (leveled) {
				AnnounceLevel();
			}

			if (Main.netMode == NetmodeID.Server) {
				SendProgression(toWho: Player.whoAmI, fromWho: -1);
			}
		}

		private void AnnounceLevel()
		{
			NetworkText text = NetworkText.FromKey("Mods.Rifthold.Progression.LevelUp", Player.name, Level);
			Color colour = new(120, 200, 255);

			if (Main.netMode == NetmodeID.Server) {
				Terraria.Chat.ChatHelper.BroadcastChatMessage(text, colour);
			}
			else if (Main.netMode == NetmodeID.SinglePlayer) {
				Main.NewText(text.ToString(), colour);
			}
		}

		public override void SaveData(TagCompound tag)
		{
			tag["level"] = Level;
			tag["xp"] = Xp;
			tag["points"] = UnspentPoints;

			// Parallel lists rather than a dictionary so unknown node ids from an
			// older save can be dropped without the whole tag failing to load.
			List<string> ids = new();
			List<int> values = new();
			foreach (KeyValuePair<string, int> pair in ranks) {
				if (pair.Value <= 0) {
					continue;
				}

				ids.Add(pair.Key);
				values.Add(pair.Value);
			}

			tag["rankIds"] = ids;
			tag["rankValues"] = values;
		}

		public override void LoadData(TagCompound tag)
		{
			Level = tag.ContainsKey("level") ? tag.GetInt("level") : 1;
			Xp = tag.ContainsKey("xp") ? tag.GetLong("xp") : 0;
			UnspentPoints = tag.ContainsKey("points") ? tag.GetInt("points") : 0;

			if (Level < 1) {
				Level = 1;
			}

			ranks.Clear();
			if (!tag.ContainsKey("rankIds") || !tag.ContainsKey("rankValues")) {
				return;
			}

			List<string> ids = tag.Get<List<string>>("rankIds");
			List<int> values = tag.Get<List<int>>("rankValues");
			for (int i = 0; i < ids.Count && i < values.Count; i++) {
				ranks[ids[i]] = values[i];
			}
		}

		private float TotalOf(StatKind kind)
		{
			float total = 0f;
			foreach (StatNode node in StatTree.Nodes) {
				if (node.Kind == kind) {
					total += node.PerRank * RankOf(node.Id);
				}
			}

			return total;
		}

		public override void ResetEffects()
		{
			Player.statDefense += (int)TotalOf(StatKind.Defense);
			Player.GetCritChance(DamageClass.Generic) += TotalOf(StatKind.CritChance);
			Player.moveSpeed += TotalOf(StatKind.MoveSpeed);
		}

		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
		{
			base.ModifyMaxStats(out health, out mana);
			health.Base += TotalOf(StatKind.MaxLife);
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
		{
			damage += TotalOf(StatKind.DamagePercent);
		}

		public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath)
		{
			if (WorldEditPolicy.MiningEnabled) {
				return;
			}

			// The copper pickaxe and axe would be dead weight in a world that cannot
			// be dug, and handing them out reads as a bug rather than a rule.
			if (!itemsByMod.TryGetValue("Terraria", out List<Item> vanilla)) {
				return;
			}

			vanilla.RemoveAll(item => item.pick > 0 || item.axe > 0 || item.hammer > 0);
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
			=> SendProgression(toWho, fromWho);

		public override void CopyClientState(ModPlayer targetCopy)
		{
			RiftholdPlayer clone = (RiftholdPlayer)targetCopy;
			clone.Level = Level;
			clone.Xp = Xp;
			clone.UnspentPoints = UnspentPoints;

			clone.ranks.Clear();
			foreach (KeyValuePair<string, int> pair in ranks) {
				clone.ranks[pair.Key] = pair.Value;
			}
		}

		public override void SendClientChanges(ModPlayer clientPlayer)
		{
			RiftholdPlayer previous = (RiftholdPlayer)clientPlayer;
			if (previous.Level == Level && previous.Xp == Xp && previous.UnspentPoints == UnspentPoints
				&& SameRanks(previous)) {
				return;
			}

			SendProgression(toWho: -1, fromWho: Player.whoAmI);
		}

		private bool SameRanks(RiftholdPlayer other)
		{
			if (other.ranks.Count != ranks.Count) {
				return false;
			}

			foreach (KeyValuePair<string, int> pair in ranks) {
				if (!other.ranks.TryGetValue(pair.Key, out int value) || value != pair.Value) {
					return false;
				}
			}

			return true;
		}

		private void SendProgression(int toWho, int fromWho)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) {
				return;
			}

			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)RiftholdPacketType.PlayerProgression);
			packet.Write((byte)Player.whoAmI);
			WriteProgression(packet);
			packet.Send(toWho, fromWho);
		}

		internal void WriteProgression(BinaryWriter writer)
		{
			writer.Write(Level);
			writer.Write(Xp);
			writer.Write(UnspentPoints);
			writer.Write((byte)ranks.Count);
			foreach (KeyValuePair<string, int> pair in ranks) {
				writer.Write(pair.Key);
				writer.Write((byte)pair.Value);
			}
		}

		internal void ReadProgression(BinaryReader reader)
		{
			Level = reader.ReadInt32();
			Xp = reader.ReadInt64();
			UnspentPoints = reader.ReadInt32();

			ranks.Clear();
			int count = reader.ReadByte();
			for (int i = 0; i < count; i++) {
				string id = reader.ReadString();
				ranks[id] = reader.ReadByte();
			}
		}
	}
}
