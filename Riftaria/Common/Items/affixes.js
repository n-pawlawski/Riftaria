var AFFIXES = {
	"tags": [
		"melee",
		"ranged",
		"magic",
		"summon",
		"head",
		"body",
		"legs",
		"accessory"
	],
	"stats": [
		{
			"id": "AddedDamage",
			"label": "Adds # damage",
			"format": "flat",
			"scope": "local"
		},
		{
			"id": "LocalIncreasedDamage",
			"label": "#% increased damage",
			"format": "percent",
			"scope": "local"
		},
		{
			"id": "LocalAttackSpeed",
			"label": "#% increased attack speed",
			"format": "percent",
			"scope": "local"
		},
		{
			"id": "LocalCrit",
			"label": "+#% critical strike chance",
			"format": "flat",
			"scope": "local"
		},
		{
			"id": "LocalProjectileSpeed",
			"label": "#% increased projectile speed",
			"format": "percent",
			"scope": "local"
		},
		{
			"id": "LocalKnockback",
			"label": "#% increased knockback",
			"format": "percent",
			"scope": "local"
		},
		{
			"id": "LocalReducedManaCost",
			"label": "#% reduced mana cost",
			"format": "percent",
			"scope": "local"
		},
		{
			"id": "LocalIncreasedDefense",
			"label": "#% increased defense",
			"format": "percent",
			"scope": "local"
		},
		{
			"id": "IncreasedDamage",
			"label": "#% increased damage with all weapons",
			"format": "percent",
			"scope": "global"
		},
		{
			"id": "AttackSpeed",
			"label": "#% increased attack speed with all weapons",
			"format": "percent",
			"scope": "global"
		},
		{
			"id": "Crit",
			"label": "+#% critical strike chance with all weapons",
			"format": "flat",
			"scope": "global"
		},
		{
			"id": "Defense",
			"label": "+# defense",
			"format": "flat",
			"scope": "global"
		},
		{
			"id": "MaxLife",
			"label": "+# maximum life",
			"format": "flat",
			"scope": "global"
		},
		{
			"id": "MaxMana",
			"label": "+# maximum mana",
			"format": "flat",
			"scope": "global"
		},
		{
			"id": "LifeRegen",
			"label": "+# life regeneration",
			"format": "flat",
			"scope": "global"
		},
		{
			"id": "MoveSpeed",
			"label": "#% increased movement speed",
			"format": "percent",
			"scope": "global"
		},
		{
			"id": "DamageReduction",
			"label": "#% reduced damage taken",
			"format": "percent",
			"scope": "global"
		}
	],
	"rarities": {
		"magic": [
			{
				"count": 1,
				"weight": 60
			},
			{
				"count": 2,
				"weight": 40
			}
		],
		"rare": [
			{
				"count": 3,
				"weight": 30
			},
			{
				"count": 4,
				"weight": 35
			},
			{
				"count": 5,
				"weight": 25
			},
			{
				"count": 6,
				"weight": 10
			}
		]
	},
	"rareNames": {
		"first": [
			"Grim",
			"Dread",
			"Rift",
			"Ashen",
			"Hollow",
			"Storm",
			"Bone",
			"Blood",
			"Gloom",
			"Wraith",
			"Rune",
			"Ember"
		],
		"second": [
			"Bane",
			"Thirst",
			"Song",
			"Fang",
			"Ward",
			"Shroud",
			"Spire",
			"Gnash",
			"Veil",
			"Mark",
			"Coil",
			"Howl"
		]
	},
	"affixes": [
		{
			"id": "WeaponAddedDamage",
			"name": "Sharp",
			"kind": "prefix",
			"group": "WeaponFlatDamage",
			"stat": "AddedDamage",
			"tags": [
				"melee",
				"ranged",
				"magic",
				"summon"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 2,
					"max": 4,
					"weight": 1000
				},
				{
					"minLevel": 2,
					"min": 5,
					"max": 8,
					"weight": 800
				},
				{
					"minLevel": 3,
					"min": 9,
					"max": 13,
					"weight": 500
				}
			]
		},
		{
			"id": "WeaponIncreasedDamage",
			"name": "Heavy",
			"kind": "prefix",
			"group": "WeaponPercentDamage",
			"stat": "LocalIncreasedDamage",
			"tags": [
				"melee",
				"ranged",
				"magic",
				"summon"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 10,
					"max": 19,
					"weight": 1000
				},
				{
					"minLevel": 2,
					"min": 20,
					"max": 34,
					"weight": 700
				},
				{
					"minLevel": 3,
					"min": 35,
					"max": 49,
					"weight": 400
				}
			]
		},
		{
			"id": "WeaponProjectileSpeed",
			"name": "Taut",
			"kind": "prefix",
			"group": "WeaponProjectileSpeed",
			"stat": "LocalProjectileSpeed",
			"tags": [
				"ranged",
				"magic"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 10,
					"max": 15,
					"weight": 600
				},
				{
					"minLevel": 2,
					"min": 16,
					"max": 25,
					"weight": 500
				},
				{
					"minLevel": 3,
					"min": 26,
					"max": 35,
					"weight": 300
				}
			]
		},
		{
			"id": "WeaponManaCost",
			"name": "Frugal",
			"kind": "prefix",
			"group": "WeaponManaCost",
			"stat": "LocalReducedManaCost",
			"tags": [
				"magic"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 5,
					"max": 9,
					"weight": 600
				},
				{
					"minLevel": 2,
					"min": 10,
					"max": 14,
					"weight": 500
				},
				{
					"minLevel": 3,
					"min": 15,
					"max": 20,
					"weight": 300
				}
			]
		},
		{
			"id": "WeaponAttackSpeed",
			"name": "of Haste",
			"kind": "suffix",
			"group": "WeaponAttackSpeed",
			"stat": "LocalAttackSpeed",
			"tags": [
				"melee",
				"ranged",
				"magic",
				"summon"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 5,
					"max": 8,
					"weight": 1000
				},
				{
					"minLevel": 2,
					"min": 9,
					"max": 12,
					"weight": 700
				},
				{
					"minLevel": 3,
					"min": 13,
					"max": 16,
					"weight": 400
				}
			]
		},
		{
			"id": "WeaponCrit",
			"name": "of Precision",
			"kind": "suffix",
			"group": "WeaponCrit",
			"stat": "LocalCrit",
			"tags": [
				"melee",
				"ranged",
				"magic",
				"summon"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 2,
					"max": 4,
					"weight": 1000
				},
				{
					"minLevel": 2,
					"min": 5,
					"max": 7,
					"weight": 700
				},
				{
					"minLevel": 3,
					"min": 8,
					"max": 10,
					"weight": 400
				}
			]
		},
		{
			"id": "WeaponKnockback",
			"name": "of Force",
			"kind": "suffix",
			"group": "WeaponKnockback",
			"stat": "LocalKnockback",
			"tags": [
				"melee",
				"ranged",
				"summon"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 10,
					"max": 20,
					"weight": 600
				},
				{
					"minLevel": 2,
					"min": 21,
					"max": 35,
					"weight": 500
				},
				{
					"minLevel": 3,
					"min": 36,
					"max": 50,
					"weight": 300
				}
			]
		},
		{
			"id": "ArmorDefense",
			"name": "Sturdy",
			"kind": "prefix",
			"group": "FlatDefense",
			"stat": "Defense",
			"tags": [
				"head",
				"body",
				"legs"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 1,
					"max": 2,
					"weight": 1000
				},
				{
					"minLevel": 2,
					"min": 3,
					"max": 4,
					"weight": 700
				},
				{
					"minLevel": 3,
					"min": 5,
					"max": 7,
					"weight": 400
				}
			]
		},
		{
			"id": "ArmorIncreasedDefense",
			"name": "Reinforced",
			"kind": "prefix",
			"group": "PercentDefense",
			"stat": "LocalIncreasedDefense",
			"tags": [
				"head",
				"body",
				"legs"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 10,
					"max": 20,
					"weight": 1000
				},
				{
					"minLevel": 2,
					"min": 21,
					"max": 35,
					"weight": 700
				},
				{
					"minLevel": 3,
					"min": 36,
					"max": 50,
					"weight": 400
				}
			]
		},
		{
			"id": "ArmorLife",
			"name": "Hale",
			"kind": "prefix",
			"group": "MaxLife",
			"stat": "MaxLife",
			"tags": [
				"head",
				"body",
				"legs",
				"accessory"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 5,
					"max": 10,
					"weight": 1000
				},
				{
					"minLevel": 2,
					"min": 11,
					"max": 20,
					"weight": 700
				},
				{
					"minLevel": 3,
					"min": 21,
					"max": 30,
					"weight": 400
				}
			]
		},
		{
			"id": "ArmorMana",
			"name": "Lucid",
			"kind": "prefix",
			"group": "MaxMana",
			"stat": "MaxMana",
			"tags": [
				"head",
				"accessory"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 5,
					"max": 10,
					"weight": 800
				},
				{
					"minLevel": 2,
					"min": 11,
					"max": 20,
					"weight": 600
				},
				{
					"minLevel": 3,
					"min": 21,
					"max": 30,
					"weight": 300
				}
			]
		},
		{
			"id": "ArmorRegen",
			"name": "of Mending",
			"kind": "suffix",
			"group": "LifeRegen",
			"stat": "LifeRegen",
			"tags": [
				"head",
				"body",
				"legs",
				"accessory"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 1,
					"max": 1,
					"weight": 800
				},
				{
					"minLevel": 2,
					"min": 2,
					"max": 2,
					"weight": 600
				},
				{
					"minLevel": 3,
					"min": 3,
					"max": 4,
					"weight": 300
				}
			]
		},
		{
			"id": "ArmorMoveSpeed",
			"name": "of the Wind",
			"kind": "suffix",
			"group": "MoveSpeed",
			"stat": "MoveSpeed",
			"tags": [
				"legs",
				"accessory"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 3,
					"max": 5,
					"weight": 1000
				},
				{
					"minLevel": 2,
					"min": 6,
					"max": 8,
					"weight": 700
				},
				{
					"minLevel": 3,
					"min": 9,
					"max": 12,
					"weight": 400
				}
			]
		},
		{
			"id": "ArmorDamage",
			"name": "of Ferocity",
			"kind": "suffix",
			"group": "GlobalDamage",
			"stat": "IncreasedDamage",
			"tags": [
				"head",
				"body",
				"accessory"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 3,
					"max": 5,
					"weight": 1000
				},
				{
					"minLevel": 2,
					"min": 6,
					"max": 9,
					"weight": 700
				},
				{
					"minLevel": 3,
					"min": 10,
					"max": 14,
					"weight": 400
				}
			]
		},
		{
			"id": "ArmorCrit",
			"name": "of the Hawk",
			"kind": "suffix",
			"group": "GlobalCrit",
			"stat": "Crit",
			"tags": [
				"head",
				"accessory"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 1,
					"max": 2,
					"weight": 800
				},
				{
					"minLevel": 2,
					"min": 3,
					"max": 4,
					"weight": 600
				},
				{
					"minLevel": 3,
					"min": 5,
					"max": 6,
					"weight": 300
				}
			]
		},
		{
			"id": "ArmorAttackSpeed",
			"name": "of Fervor",
			"kind": "suffix",
			"group": "GlobalAttackSpeed",
			"stat": "AttackSpeed",
			"tags": [
				"body",
				"accessory"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 2,
					"max": 3,
					"weight": 800
				},
				{
					"minLevel": 2,
					"min": 4,
					"max": 6,
					"weight": 600
				},
				{
					"minLevel": 3,
					"min": 7,
					"max": 9,
					"weight": 300
				}
			]
		},
		{
			"id": "ArmorDamageReduction",
			"name": "of Warding",
			"kind": "suffix",
			"group": "DamageReduction",
			"stat": "DamageReduction",
			"tags": [
				"body"
			],
			"tiers": [
				{
					"minLevel": 1,
					"min": 1,
					"max": 2,
					"weight": 800
				},
				{
					"minLevel": 2,
					"min": 3,
					"max": 4,
					"weight": 600
				},
				{
					"minLevel": 3,
					"min": 5,
					"max": 6,
					"weight": 300
				}
			]
		}
	]
};
