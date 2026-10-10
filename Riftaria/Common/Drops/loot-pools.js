var LOOT_POOLS = {
	"bases": [
		{
			"item": "WoodenBow",
			"tag": "ranged",
			"minTier": 1,
			"maxTier": 2,
			"weight": 100
		},
		{
			"item": "CopperBow",
			"tag": "ranged",
			"minTier": 1,
			"maxTier": 2,
			"weight": 80
		},
		{
			"item": "IronBow",
			"tag": "ranged",
			"minTier": 2,
			"maxTier": 0,
			"weight": 80
		},
		{
			"item": "SilverBow",
			"tag": "ranged",
			"minTier": 2,
			"maxTier": 0,
			"weight": 60
		},
		{
			"item": "GoldBow",
			"tag": "ranged",
			"minTier": 3,
			"maxTier": 0,
			"weight": 60
		},
		{
			"item": "WoodenSword",
			"tag": "melee",
			"minTier": 1,
			"maxTier": 2,
			"weight": 100
		},
		{
			"item": "CopperBroadsword",
			"tag": "melee",
			"minTier": 1,
			"maxTier": 2,
			"weight": 80
		},
		{
			"item": "IronBroadsword",
			"tag": "melee",
			"minTier": 2,
			"maxTier": 0,
			"weight": 80
		},
		{
			"item": "GoldBroadsword",
			"tag": "melee",
			"minTier": 3,
			"maxTier": 0,
			"weight": 60
		},
		{
			"item": "WandofSparking",
			"tag": "magic",
			"minTier": 1,
			"maxTier": 0,
			"weight": 60
		},
		{
			"item": "AmethystStaff",
			"tag": "magic",
			"minTier": 2,
			"maxTier": 0,
			"weight": 60
		},
		{
			"item": "SapphireStaff",
			"tag": "magic",
			"minTier": 3,
			"maxTier": 0,
			"weight": 50
		},
		{
			"item": "BabyBirdStaff",
			"tag": "summon",
			"minTier": 1,
			"maxTier": 0,
			"weight": 40
		},
		{
			"item": "WoodHelmet",
			"tag": "head",
			"minTier": 1,
			"maxTier": 2,
			"weight": 80
		},
		{
			"item": "WoodBreastplate",
			"tag": "body",
			"minTier": 1,
			"maxTier": 2,
			"weight": 80
		},
		{
			"item": "WoodGreaves",
			"tag": "legs",
			"minTier": 1,
			"maxTier": 2,
			"weight": 80
		},
		{
			"item": "CopperHelmet",
			"tag": "head",
			"minTier": 1,
			"maxTier": 2,
			"weight": 70
		},
		{
			"item": "CopperChainmail",
			"tag": "body",
			"minTier": 1,
			"maxTier": 2,
			"weight": 70
		},
		{
			"item": "CopperGreaves",
			"tag": "legs",
			"minTier": 1,
			"maxTier": 2,
			"weight": 70
		},
		{
			"item": "IronHelmet",
			"tag": "head",
			"minTier": 2,
			"maxTier": 0,
			"weight": 70
		},
		{
			"item": "IronChainmail",
			"tag": "body",
			"minTier": 2,
			"maxTier": 0,
			"weight": 70
		},
		{
			"item": "IronGreaves",
			"tag": "legs",
			"minTier": 2,
			"maxTier": 0,
			"weight": 70
		},
		{
			"item": "GoldHelmet",
			"tag": "head",
			"minTier": 3,
			"maxTier": 0,
			"weight": 60
		},
		{
			"item": "GoldChainmail",
			"tag": "body",
			"minTier": 3,
			"maxTier": 0,
			"weight": 60
		},
		{
			"item": "GoldGreaves",
			"tag": "legs",
			"minTier": 3,
			"maxTier": 0,
			"weight": 60
		},
		{
			"item": "Shackle",
			"tag": "accessory",
			"minTier": 1,
			"maxTier": 0,
			"weight": 40
		},
		{
			"item": "Aglet",
			"tag": "accessory",
			"minTier": 1,
			"maxTier": 0,
			"weight": 40
		},
		{
			"item": "BandofRegeneration",
			"tag": "accessory",
			"minTier": 2,
			"maxTier": 0,
			"weight": 30
		},
		{
			"item": "AnkletoftheWind",
			"tag": "accessory",
			"minTier": 2,
			"maxTier": 0,
			"weight": 30
		},
		{
			"item": "FeralClaws",
			"tag": "accessory",
			"minTier": 3,
			"maxTier": 0,
			"weight": 25
		}
	],
	"tiers": [
		{
			"tier": 1,
			"general": {
				"dropChance": 0.06,
				"baseCount": 8,
				"rarity": {
					"normal": 60,
					"magic": 32,
					"rare": 8
				}
			},
			"boss": {
				"items": 2,
				"baseCount": 5,
				"rarity": {
					"normal": 0,
					"magic": 60,
					"rare": 40
				}
			}
		},
		{
			"tier": 2,
			"general": {
				"dropChance": 0.07,
				"baseCount": 10,
				"rarity": {
					"normal": 50,
					"magic": 38,
					"rare": 12
				}
			},
			"boss": {
				"items": 2,
				"baseCount": 6,
				"rarity": {
					"normal": 0,
					"magic": 50,
					"rare": 50
				}
			}
		},
		{
			"tier": 3,
			"general": {
				"dropChance": 0.08,
				"baseCount": 12,
				"rarity": {
					"normal": 40,
					"magic": 42,
					"rare": 18
				}
			},
			"boss": {
				"items": 3,
				"baseCount": 6,
				"rarity": {
					"normal": 0,
					"magic": 35,
					"rare": 65
				}
			}
		}
	],
	"bosses": [
		{
			"boss": "RiftEye",
			"tier": 0,
			"items": 1,
			"rarity": null
		}
	]
};
