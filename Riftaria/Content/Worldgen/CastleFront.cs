using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Riftaria.Common.World;
using Riftaria.Content.Tiles;

namespace Riftaria.Content.Worldgen
{
	/// <summary>
	/// A stone platform in the sky above the Hold's ground, with the Rift Gate
	/// in the middle. High enough that the forest backdrop stays off screen.
	/// </summary>
	public static class CastleFront
	{
		const int Lift = 130;

		const ushort Stone = TileID.GrayBrick;

		public static int SpawnX { get; private set; }

		public static int SpawnY { get; private set; }

		public static int MinX { get; private set; }

		public static int MaxX { get; private set; }

		public static void Build(int groundY)
		{
			int cx = HubLayout.CenterX;
			int deck = groundY - Lift;
			int left = cx - 60;
			int right = cx + 60;

			MinX = left;
			MaxX = right;
			SpawnX = cx - 8;
			SpawnY = deck;

			Fill(left, deck, right, deck + 1, Stone);
			PlaceGate(cx, deck);
			PlaceStarterChest(cx);
			SafetyWater(groundY);
		}

		/// <summary>
		/// The gate is a 3x3 whose origin is the bottom-center tile. That origin
		/// sits one row above the deck so the platform anchors it.
		/// </summary>
		static void PlaceGate(int cx, int deck)
		{
			int originY = deck - 1;
			for (int x = cx - 1; x <= cx + 1; x++) {
				for (int y = originY - 2; y <= originY; y++) {
					HubLayout.Clear(x, y);
				}
			}

			WorldGen.PlaceTile(cx, originY, ModContent.TileType<MapDeviceTile>(), mute: true);
		}

		/// <summary>
		/// A wood chest on the platform, left of where the player appears.
		/// The chest sits on the deck: its bottom row is the tile above the brick.
		/// </summary>
		static void PlaceStarterChest(int cx)
		{
			int x = cx - 16;
			int y = SpawnY - 1;
			int index = WorldGen.PlaceChest(x, y, TileID.Containers, false, 0);
			if (index < 0) {
				return;
			}

			Chest chest = Main.chest[index];
			for (int i = 0; i < chest.item.Length; i++) {
				chest.item[i].TurnToAir();
			}

			int slot = 0;
			Add(chest, ref slot, ItemID.DiamondHook);
			Add(chest, ref slot, ItemID.WoodHelmet);
			Add(chest, ref slot, ItemID.WoodBreastplate);
			Add(chest, ref slot, ItemID.WoodGreaves);
			Add(chest, ref slot, ItemID.HealingPotion, 20);
			Add(chest, ref slot, ItemID.LifeCrystal, 5);
			Add(chest, ref slot, ItemID.WoodenBow);
			Add(chest, ref slot, ItemID.WoodenArrow, 1000);
		}

		static void Add(Chest chest, ref int slot, int type, int stack = 1)
		{
			chest.item[slot].SetDefaults(type);
			chest.item[slot].stack = stack;
			slot++;
		}

		/// <summary>
		/// The Hold is already generated, so a new world is not required. The chest
		/// left of the gate drops the Vortex set and boots, and gains a Diamond Hook.
		/// </summary>
		public static void EnsureStarterGear()
		{
			int x = HubLayout.CenterX - 16;
			Chest chest = null;
			for (int i = 0; i < Main.maxChests; i++) {
				Chest candidate = Main.chest[i];
				if (candidate != null && Math.Abs(candidate.x - x) <= 2) {
					chest = candidate;
					break;
				}
			}

			if (chest == null) {
				return;
			}

			Remove(chest, ItemID.WingsVortex);
			Remove(chest, ItemID.VortexHelmet);
			Remove(chest, ItemID.VortexBreastplate);
			Remove(chest, ItemID.VortexLeggings);
			Remove(chest, ItemID.TerrasparkBoots);
			Pack(chest);
			Ensure(chest, ItemID.DiamondHook);
		}

		static void Remove(Chest chest, int type)
		{
			for (int i = 0; i < chest.item.Length; i++) {
				if (chest.item[i].type == type) {
					chest.item[i].TurnToAir();
				}
			}
		}

		static void Pack(Chest chest)
		{
			int write = 0;
			for (int read = 0; read < chest.item.Length; read++) {
				if (chest.item[read].IsAir) {
					continue;
				}

				if (write != read) {
					chest.item[write] = chest.item[read].Clone();
					chest.item[read].TurnToAir();
				}

				write++;
			}
		}

		static void Ensure(Chest chest, int type)
		{
			int empty = -1;
			for (int i = 0; i < chest.item.Length; i++) {
				Item item = chest.item[i];
				if (item.type == type && item.stack > 0) {
					return;
				}

				if (empty < 0 && item.IsAir) {
					empty = i;
				}
			}

			if (empty < 0) {
				return;
			}

			chest.item[empty].SetDefaults(type);
		}

		/// <summary>A sheet of water on the ground, so a fall off the platform is not lethal.</summary>
		static void SafetyWater(int groundY)
		{
			for (int x = MinX; x <= MaxX; x++) {
				Pour(x, groundY - 1);
			}
		}

		static void Pour(int x, int y)
		{
			if (!WorldGen.InWorld(x, y)) {
				return;
			}

			Tile tile = Main.tile[x, y];
			tile.HasTile = false;
			tile.WallType = 0;
			tile.LiquidType = LiquidID.Water;
			tile.LiquidAmount = 255;
		}

		static void Fill(int x0, int y0, int x1, int y1, ushort type)
		{
			int left = x0 < x1 ? x0 : x1;
			int right = x0 > x1 ? x0 : x1;
			int top = y0 < y1 ? y0 : y1;
			int bottom = y0 > y1 ? y0 : y1;

			for (int x = left; x <= right; x++) {
				for (int y = top; y <= bottom; y++) {
					HubLayout.SetTile(x, y, type);
				}
			}
		}
	}
}
