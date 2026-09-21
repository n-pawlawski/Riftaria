# Rifthold — design decisions

The Miro board is the working planning surface. This file is the durable record of
decisions that have been settled there, plus a queue of board edits that have not
been applied yet.

Board: https://miro.com/app/board/uXjVHlbp6tM=/

---

## Settled

### Tier ladder — 10 tiers

One tier per section of the vanilla game, Pre-boss through Moon Lord. Each tier
inherits a vanilla biome, enemy pool, tileset and boss.

Balance is explicitly not tuned. Room counts, enemy scaling and reward weight are
placeholder until the loop has been played.

A tier is cheap in art and AI, because all of it already exists. It is not cheap in
integration, because each boss brings its own demands on the world it is fought in.

### Boss handling — three rungs, default to vanilla

1. Run the vanilla AI untouched. This is the default.
2. Patch the specific behaviour where a rift breaks it.
3. Author our own boss, only where patching cannot fix it.

Which rung a tier needs is found during testing or by predicting a conflict, not
declared up front. Expect adapters to move down the ladder as problems surface.

### Boss Adapters — one per tier, four fields

Each tier records what its boss demands of the world:

- **Preconditions** — what must be true before it can spawn. Night, a minimum count
  of biome tiles, a specific tile such as a Lihzahrd Altar.
- **Arena** — the shape generation must produce. A long flat corridor, an enclosed
  chamber, open sky.
- **Leash** — what voids the fight once it starts. Either satisfied by the arena or
  patched out of the AI.
- **Aftermath** — vanilla side effects on death to suppress or carry back to the hub.
  Hardmode conversion, Plantera altering dungeon spawns, the mechanical boss flags.

### Loot — three sources per tier

- **Chests**, placed by generation and filled by tier. Pays for exploring the layout.
- **Enemy drops**, per-enemy tables. Pays for fighting through.
- **The boss**, one guaranteed payout. Pays for finishing.

Vanilla drop tables are cleared inside rifts rather than extended. Nothing drops that
we did not put there. This is affordable only because tier unlocks have replaced the
gating job vanilla key items used to do.

Payout goes to everyone present, not to whoever landed the last hit.

Crafting is deliberately outside this model, with the stated intention of designing it
later.

### Tier unlocking — UI now, item later

Killing the boss unlocks the next tier directly. No item changes hands, and the Rift
Gate Panel stays the single place a tier is chosen.

A map item is the intention for later, not a rejected option. Adding it would revive
the Keys and Map Items system, which is currently parked.

---

## Open

- Do the replacement loot tables hold vanilla items re-weighted, entirely new items,
  or both? This is the difference between a tuning job and authoring ten tiers of gear.
- Which vanilla key items still need to exist? Tier unlocks cover most of their job,
  but the Pwnhammer and the Temple Key gated real content in the base game.
- Is gear a fixed item per tier, or a pool to roll from?
- Is there any reason to repeat a cleared tier once the next one is open?
- How many of the ten tiers does the first playable ship with?

---

## Pending board edits

Queued on 21 September after hitting the Miro free-plan daily limit of 100 tool calls.
Apply these when the limit resets.

**System / Boss Adapters** (`3458764684383973956`)

- Subtitle `3458764684383973960` → "One per tier. What a boss demands of the world,
  and how far we go to meet it."
- How it works `3458764684383973962` → change the Leash line to "Satisfied by the
  arena, or patched out of the AI", and replace the closing line with "Default to
  vanilla AI, patch where a rift breaks it, write our own only where patching cannot.
  Which of the three a tier needs is found in testing, not declared up front."

**System / The Objective** (`3458764684378310139`)

- How it works `3458764684378310157` → replace "Still open: the real vanilla boss, or
  a rift-specific variant of it" with "The real boss by default. Patched where a rift
  breaks it, replaced only where patching cannot." Change the completion line to
  "Completion unlocks the next tier directly and starts the return."

**Reward Tables** (`3458764684382508230`)

- Header `3458764684384272805` → "HOW THE NEXT TIER OPENS"
- Body `3458764684382508257` → the unlock decision, with the map item recorded as the
  later intention.

**System and Loop Matrix** (`3458764684378900897`)

Three systems are no longer blocked and move from Open to Shaped (`#067429`):

- The Objective, status `3458764684378957957`
- Rewards and Loot, status `3458764684378957962`
- Boss Adapters, status `3458764684384042051`

- Footer `3458764684378957987` → "Nine shaped, three open, one deferred." The three
  remaining Open are World Edit Rules, Encounters and Multiplayer.

**Backlog** (`3458764684381846730`)

- Keys and Map Items line in `3458764684381846734` → change from a conditional revival
  to a stated intention, since a map item is now planned rather than merely possible.
