# Terraria modding brief

Preliminary look at Terraria and tModLoader. Written against **tModLoader 1.4.4-stable** (latest GitHub stable tag checked: **v2026.06.3.6**, Terraria **1.4.4.9** content). Sources are at the end.

This is a new project. Do not port, reuse, or adapt anything from earlier work. New content, names, art, and systems start from Terraria + tModLoader only.

This is research, not a playable mod. A `.tmod` has to be built on a machine that already has Terraria and tModLoader installed.

---

## Why a Terraria mod

Terraria is a 2D sandbox action game. The loop we care about is already finished:

- A player who moves, mines, crafts, and fights
- Four unofficial combat styles (melee, ranged, magic, summoner)
- A long boss-gated progression from the first night through Hardmode to Moon Lord
- Tiles, walls, NPCs, projectiles, buffs, loot, biomes, and UI
- Pixel art, sound, and animation conventions that players already understand

A from-scratch game would spend most of its early work on that foundation. A tModLoader mod starts *after* that foundation. New work is original content and systems: weapons, enemies, hooks into vanilla, maybe a small biome or boss later.

We do **not** get to ship Terraria itself. Players still need a legal copy of Terraria. tModLoader is a separate Steam app that loads the base game and then loads mods.

## Clean start

Nothing from earlier projects comes along. That includes:

- Code, shaders, data, or configs
- Art, audio, names, lore, or UI copy
- Systems, combat rules, or “port this mechanic” leftovers
- Folder structure or branding

The only inputs are Terraria, tModLoader, ExampleMod, and original work we make for this mod.

---

## What Terraria already provides

### Combat

There is no formal class or XP system. Players specialize by gear:

| Style | How it plays | What we inherit |
| --- | --- | --- |
| Melee | Swords, spears, yoyos, flails, true melee and projectile melee | Use styles, swing animation, knockback |
| Ranged | Bows, guns, launchers, thrown specialist weapons | Ammo items, `Item.shoot`, consume-ammo hooks |
| Magic | Staffs, tomes, unique spell behaviors | Mana cost, mana regen, homing / AoE patterns |
| Summoner | Minions, sentries, whips | Minion slots, targeting, whip tagging |

Armor sets, accessories, prefixes (reforges), and buffs all plug into those damage types. Custom weapons can use vanilla damage classes on day one. Custom damage classes are possible later (`ModDamageClass`) but are an intermediate topic.

### Progression

Vanilla is gated by bosses and world state, not by a skill tree:

1. First night / pre-boss
2. Eye of Cthulhu, Eater of Worlds / Brain of Cthulhu, Skeletron, Queen Bee, Deerclops
3. Wall of Flesh → **Hardmode** (world conversion, new ores, new enemies)
4. Mechanical bosses → Plantera → Golem
5. Lunar events → Moon Lord

A first learning mod should sit in **pre-Hardmode**: dirt-cheap recipes, one weapon, one projectile, one enemy. Bosses, worldgen, and custom UI are the hard end of the API.

### World and entities

The game is a tile grid plus entity arrays:

- `Main.tile` — the world
- `Main.npc[]`, `Main.projectile[]`, `Main.item[]` — live instances
- Every content kind has a **Type** (what it is) and an **Index** (`whoAmI`, where it sits in the array)

New modders routinely mix those two integers up. `Main.projectile[ProjectileID.Shuriken]` does **not** mean “the shuriken.” It means “whatever projectile is sitting in slot 3.”

---

## The modding stack: tModLoader only

Terraria has no first-party mod SDK. The practical stack is **[tModLoader](https://github.com/tModLoader/tModLoader)**.

| Fact | Detail |
| --- | --- |
| Distribution | Steam app `1281930`, also GitHub releases |
| Relationship to vanilla | Separate install. Vanilla and tModLoader can coexist. |
| Content base | Default Steam branch = **1.4.4.9**. Legacy Steam betas exist for 1.4.3 and 1.3. |
| Language | C# |
| Runtime / SDK | **.NET 8** only. .NET 9 and 10 will not work. |
| IDE | Visual Studio 2022 17.8+ (Windows, recommended). VS Code or Rider on Mac/Linux. |
| Publish | Steam Workshop (in-game). Friends-only `.tmod` sharing also works. |
| Release cadence | Stable updates monthly. Preview branch is for porting, not for playthroughs. |

There are three living lines of tModLoader. We should target **1.4.4-stable** and ignore 1.4.3-legacy / 1.3-legacy unless we later need a specific old mod as a reference.

tModLoader is MIT-licensed. Terraria itself is Re-Logic proprietary. The loader does not grant rights to redistribute vanilla art, audio, or the game binary.

---

## Toolchain

On the machine that will actually run the game:

1. Own Terraria on Steam and install it.
2. Install tModLoader from Steam (default / stable branch).
3. Install the **.NET 8 SDK** (x64). Confirm with `dotnet --list-sdks` — a line starting with `8.` should appear.
4. Open tModLoader → **Workshop → Develop Mods → Create Mod**.
5. Fill in internal name, display name, author, and a first item name. The internal name must be unique if we ever publish.
6. Click **Build + Reload**. The generated sword should appear as a workbench + 10 dirt recipe.

The generated project lives here:

- Windows: `Documents\My Games\Terraria\tModLoader\ModSources\<ModName>\`
- macOS: `~/Library/Application Support/Terraria/tModLoader/ModSources/<ModName>/`
- Linux: `~/.local/share/Terraria/tModLoader/ModSources/<ModName>/`

Built mods land in `tModLoader/Mods` as `<ModName>.tmod`.

This git repo should become the source of truth. Typical workflow: keep the C# here, copy or symlink into `ModSources`, build from the in-game menu or `dotnet msbuild`.

We **cannot** compile or launch the game in the current cloud environment. There is no Terraria install, no tModLoader assemblies, and no GPU game loop. Design and source can happen here; Build + Reload cannot.

---

## Anatomy of a generated mod

The in-game skeleton currently creates:

| File | Role |
| --- | --- |
| `<ModName>.cs` | The single `Mod` class. Global load/unload lives here. Keep it thin. |
| `build.txt` | Version, author, display name, `buildIgnore`. Required. |
| `description.txt` | In-game More Info text. |
| `description_workshop.txt` | Steam Workshop page (BBCode). |
| `icon.png` | 80×80 in-game icon. |
| `icon_small.png` | 30×30 bestiary / config icon. |
| `icon_workshop.png` | Optional, up to 512×512. |
| `<ModName>.csproj` | Project file. Open this, not loose `.cs` files. |
| `Properties/launchSettings.json` | Debug launch path into tModLoader. |
| `Content/Items/<Item>.cs` + `.png` | Starter `ModItem` (usually a sword). |
| `Localization/en-US_Mods.<ModName>.hjson` | Display names and tooltips. Auto-grows as content is added. |

ExampleMod on the **stable** branch is the reference layout we should copy as we grow:

```
ExampleMod/
  ExampleMod.cs                  # Mod entry (partial class)
  ExampleMod.ModCalls.cs
  ExampleMod.Networking.cs
  Common/                        # ModSystem, ModPlayer, configs, helpers
  Content/
    Items/  Projectiles/  NPCs/  Tiles/  Walls/
    Buffs/  Dusts/  Mounts/  Pets/  Prefixes/
    Biomes/  DamageClasses/  TileEntities/
    ...
  Assets/
  Localization/
```

Autoload is the rule that trips most beginners. tModLoader finds the texture from **namespace + class name**, not from the `.cs` filename:

- Class `AshenShortsword` in namespace `OurMod.Content.Items`
- Texture expected at `Content/Items/AshenShortsword.png`
- Folder path should match the namespace (`/` for `.`)
- Keep class name, file name, and PNG name the same. Missing PNG = `MissingResourceException` and the mod will not load.

Equipment, town NPCs, bosses, mounts, and highlight tiles autoload extra suffixes (`_Head`, `_Head_Boss`, `_Body`, `_Back`, `_Highlight`, …).

---

## Content types we will actually use

tModLoader content is inheritance. One class, one base type.

### Add new things

| Base class | Use it for |
| --- | --- |
| `Mod` | One per mod. Load/unload, rarely much else. |
| `ModItem` | Weapons, tools, accessories, placeables, ammo, materials |
| `ModProjectile` | Sword beams, bullets, spells, minions, spears, yoyos |
| `ModNPC` | Enemies, critters, town NPCs, bosses |
| `ModTile` / `ModWall` | Placeable world blocks |
| `ModBuff` | Potion effects, debuffs, minion tags |
| `ModDust` | Particles |
| `ModPrefix` | Reforges |
| `ModMount` | Mounts |
| `ModPlayer` | Per-player state (dash charges, custom resources) |
| `ModSystem` | World/mod-wide state, recipes that need extra logic, ticks |
| `ModConfig` | In-game settings |

### Change vanilla things

`GlobalItem`, `GlobalNPC`, `GlobalProjectile`, `GlobalTile`, and friends let us patch *existing* content without replacing it. That is how a learning mod can, for example, make wooden swords drop a custom material or give zombies a new loot rule.

### Reference content

```csharp
// Our content
Item.shoot = ModContent.ProjectileType<AshenBolt>();
Banner = ModContent.NPCType<AshenCrawler>();
Item.DefaultToPlaceableTile(ModContent.TileType<AshenBrick>());

// Vanilla content
Item.shoot = ProjectileID.WoodenArrowFriendly;
```

Cross-file references need the matching `using`. IDs are integers assigned at load time; never hardcode a guessed number for modded content.

---

## How “hooks” work

A hook is a `virtual` method on a `Mod*` / `Global*` class. Override it and tModLoader calls it at the right point in the update loop.

Example: do something while an item sits in the inventory (Cell Phone pattern):

```csharp
public override void UpdateInventory(Player player)
{
    // runs every tick this item is in the inventory
}
```

Common early hooks:

| Class | Hook | When it runs |
| --- | --- | --- |
| `ModItem` | `SetDefaults` | Once, to fill `Item.damage`, `useTime`, `shoot`, etc. |
| `ModItem` | `AddRecipes` | During recipe setup |
| `ModItem` | `Shoot` | When the item fires a projectile |
| `ModItem` | `OnHitNPC` | Melee / projectile-item hit |
| `ModProjectile` | `AI` | Every tick the projectile exists |
| `ModProjectile` | `OnHitNPC` | Projectile connects |
| `ModNPC` | `AI` | Every tick the NPC exists |
| `ModNPC` | `ModifyNPCLoot` | Drop table |
| `ModSystem` | `AddRecipes` / `PostSetupContent` | Cross-cutting setup |

Signatures come from [docs.tmodloader.net (stable)](https://docs.tmodloader.net/docs/stable/). Visual Studio’s override helper is faster than copying from the docs.

---

## Assets and licensing (important)

What we **can** do:

- Write original sprites as PNG (Terraria’s 2×2 pixel convention, vanilla sizes as a guide).
- Extract vanilla textures locally with TConvert and use them as a **size/orientation reference**.
- At runtime, *request* a vanilla texture by path (`Terraria/Images/Item_{id}`) or via `TextureAssets` after `Main.instance.LoadX`. That uses the player’s installed game, not a copy we ship.
- License our own C# and our own art separately if we publish.

What we **must not** do:

- Commit or Workshop-publish Re-Logic sprites, audio, or binaries.
- Treat tModLoader’s MIT license as covering Terraria.
- Use wiki PNG dumps as our item sprites — they are often cropped or restyled, and they are still Re-Logic art.

Spriting workflow the wiki recommends: look up a similar vanilla item’s Internal Item ID, open the extracted `Item_###.png`, match canvas size and facing, then draw a new sprite.

Placeholder art is fine for the first slice. A 16×16 or ~40–60px sword PNG is enough to load.

---

## Learning path (official, in order)

Do not start with a boss.

1. [Basic tModLoader Modding Guide](https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide) — generate skeleton, Build + Reload, change damage and tooltip.
2. [Basic Item](https://github.com/tModLoader/tModLoader/wiki/Basic-Item), [Autoload](https://github.com/tModLoader/tModLoader/wiki/Basic-Autoload), [Recipes](https://github.com/tModLoader/tModLoader/wiki/Basic-Recipe-Guide).
3. [ExampleMod weapons](https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Content/Items) — start at a gun or sword, then the projectile it shoots.
4. Easy wiki guides as needed: ModProjectile, ModNPC, NPC loot, ammo, glowmasks, localization.
5. Intermediate / advanced only when we hit them: worldgen, custom UI, netcode, detours, IL editing.

ExampleMod should be downloaded from **`stable.zip`**, not from the default GitHub branch. The default branch tracks upcoming tModLoader and can contain APIs that do not exist on the Steam stable build.

Community help lives on the tModLoader Discord. The most useful question shape is vanilla-first: “How does Molten Fury convert Wooden Arrows into Flaming Arrows?” rather than “how do I make my gun special.”

---

## What is easy vs hard

**Start here**

- Change numbers on the generated sword
- New `ModItem` with a recipe
- Sword that shoots a vanilla projectile
- New `ModProjectile` + wire it with `ModContent.ProjectileType<T>()`
- Simple walker enemy with a drop
- A `GlobalNPC` loot rule on a vanilla enemy

**Wait**

- Custom AI that feels good (projectiles, enemies, pets)
- A real boss (phases, arena, netcode, music, loot, bestiary)
- Custom UI
- World generation
- Cross-mod APIs (`modReferences`, `Call`)
- IL / MonoMod patches
- New damage classes and full class armor progressions

Large published mods (Calamity, Thorium, Spirit) are useful as *players* to see the ceiling. They are a poor first source-code target. ExampleMod is the source-code target.

---

## Recommended first slice

Once tModLoader is installed locally, do this and nothing else:

1. Generate the official skeleton with a unique internal name (not `TutorialMod` if we might publish).
2. Sync that folder into this repo.
3. Keep the generated sword. Retune it and give it a real name.
4. Add one `ModProjectile` the sword shoots.
5. Add one `ModNPC` that walks, dies, and drops the sword material.
6. Write English display names in the HJSON file.
7. Build + Reload, craft it, fight the enemy, confirm the projectile.

That slice teaches autoload, Types vs instances, recipes, loot, and the item → projectile → NPC loop. It is also the smallest thing that feels like “we are using Terraria’s combat instead of inventing it.”

Theme, name, and whether this stays a learning sandbox or becomes a real content mod can wait until that slice exists.

---

## Open decisions (not blockers)

- Internal / Workshop name
- Tone: original content pack vs vanilla-adjacent quality-of-life vs combat experiment
- Whether we stay pre-Hardmode for a while
- Art: placeholders vs a sprite pass
- License for our code vs our art (default copyright is fine until we publish)

---

## Sources

Checked 2026-09-19.

- [tModLoader wiki home](https://github.com/tModLoader/tModLoader/wiki)
- [tModLoader guide for developers](https://github.com/tModLoader/tModLoader/wiki/tModLoader-guide-for-developers)
- [Basic tModLoader Modding Guide](https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide)
- [Basic Item](https://github.com/tModLoader/tModLoader/wiki/Basic-Item)
- [Basic Autoload](https://github.com/tModLoader/tModLoader/wiki/Basic-Autoload)
- [Spriting](https://github.com/tModLoader/tModLoader/wiki/Spriting)
- [Assets](https://github.com/tModLoader/tModLoader/wiki/Assets)
- [Licensing information](https://github.com/tModLoader/tModLoader/wiki/Licensing-information)
- [tModLoader release cycle](https://github.com/tModLoader/tModLoader/wiki/tModLoader-Release-Cycle)
- [ExampleMod (stable)](https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod)
- [API docs, 1.4.4-stable](https://docs.tmodloader.net/docs/stable/)
- [tModLoader on the official Terraria wiki](https://terraria.wiki.gg/wiki/TModLoader)
- [Class setups](https://terraria.wiki.gg/wiki/Guide:Class_setups)
- [Steam: tModLoader](https://store.steampowered.com/app/1281930)
- [v2026.06.3.6 release](https://github.com/tModLoader/tModLoader/releases/tag/v2026.06.3.6)
