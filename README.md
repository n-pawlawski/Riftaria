# Terraria Mod (working title)

A new **Terraria** mod built on **tModLoader**. This is a clean start: no code, art, lore, systems, or leftover design from any earlier project.

The goal is to learn combat, content, and systems work on top of Terraria instead of rebuilding movement, weapons, enemies, world generation, and art from scratch.

This first commit is a preliminary look at the game and its modding stack — not a playable `.tmod` yet. Building and running a mod requires a local copy of Terraria plus tModLoader. Those are not available in this environment.

## Current status

| Piece | Status |
| --- | --- |
| Terraria / tModLoader research | Done — see [docs/terraria-modding-brief.md](docs/terraria-modding-brief.md) |
| Mod identity / theme | Not chosen yet |
| First in-game content | Next, once tModLoader is installed locally |

Read the brief first. It covers:

- What Terraria already gives us (combat, classes, progression, world)
- Why tModLoader is the stack
- Toolchain (C# / .NET 8, Steam, Workshop)
- How a mod is structured
- The content types and hooks we will actually use
- Assets, licensing, and what we must not ship
- A recommended first slice (sword → projectile → simple enemy)

## How we will work

1. Keep design notes and source in this repo.
2. Generate the official mod skeleton inside tModLoader (`Workshop → Develop Mods → Create Mod`).
3. Copy or sync that `ModSources` folder here so the real C# lives in git.
4. Build and reload in-game on a machine that has Terraria + tModLoader.

Official starting points:

- [tModLoader wiki](https://github.com/tModLoader/tModLoader/wiki)
- [Basic tModLoader Modding Guide](https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide)
- [ExampleMod (stable)](https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod)
- [API docs (1.4.4-stable)](https://docs.tmodloader.net/docs/stable/)
