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

### World Edit Rules — off by default, whitelists on both sides

The rule applies everywhere, the Hold included. No hub exception for now.

Mining and placement are both off by default, each with a per-tile whitelist. Both
mechanisms already exist in `WorldEditPolicy` as `AllowMining` and `AllowPlacement`;
what is missing is entries, not machinery. The Rift Gate is the only current member of
either list.

Guiding rule for the placement whitelist: allow what helps a player see and survive,
refuse what reshapes the layout. Torches, campfires and heart lanterns qualify. Blocks,
platforms and ropes do not, because they bypass authored traversal — which is the
entire reason the rule exists. Beds are out too, since setting a spawn point inside a
rift would defeat returning to the Hold on death.

Two things to fix when the lists get populated:

- Anything placeable must also be mineable, or it is permanent. A whitelisted torch
  that cannot be broken can never be picked back up, and every rift slowly fills with
  litter the player cannot clear.
- Walls cannot be whitelisted at all. `NoMiningGlobalWall` consults the global switches
  rather than a per-type list, unlike the tile hooks. The asymmetry is harmless while
  both lists are empty and becomes a bug as soon as they are not.

### Encounters — spawners, not ambient spawning

Vanilla ambient spawning is switched off entirely inside rifts. Nothing appears simply
because a player walked somewhere.

Enemies come from spawners: fixed points placed by generation, each emitting on an
interval. A spawner carries a pool drawn from the tier's biome, an interval, a cap on
how many of its own may be alive at once, and an activation radius so the far side of
the rift stays quiet while you fight the near side.

Spawners are destructible. Killing one stops its flow permanently, which gives a room a
reason to be fought rather than run past, and gives the rift a second verb alongside
reaching the far end.

A spawner should be a stationary NPC rather than a tile. That keeps mining fully
disabled with no allowlist exception, and inherits health bars, damage numbers and
death effects from vanilla for free. A destructible tile would have meant reaching for
a pickaxe, which cuts against the whole point of World Edit Rules.

The boss is the same mechanism with a count of one and a trigger instead of an
interval.

Deliberately simple. Waves and finite per-spawner budgets are later.

This settles the older question of bespoke versus re-pooled enemies in favour of
re-pooled: the pool comes from the tier's biome, which vanilla already populates.

Note for whenever implementation resumes: the prototype `RiftSpawnSystem` throttles and
filters vanilla spawning rather than replacing it. Under this model it should suppress
vanilla spawning outright and own a list of spawners instead.

---

## Open

- Do the replacement loot tables hold vanilla items re-weighted, entirely new items,
  or both? This is the difference between a tuning job and authoring ten tiers of gear.
- Which vanilla key items still need to exist? Tier unlocks cover most of their job,
  but the Pwnhammer and the Temple Key gated real content in the base game.
- Is gear a fixed item per tier, or a pool to roll from?
- Is there any reason to repeat a cleared tier once the next one is open?
- How many of the ten tiers does the first playable ship with?
- Which tiles go on each whitelist. Torches are the only confirmed member so far, and
  the mining list has no members at all beyond the intention to have some.
- Liquids are not covered by any of the hooks, so a bucket can still reshape hazards.

---

## Parked — settled for now, not forever

- **Farming a spawner.** An undestroyed spawner emits forever and its enemies drop
  loot, so a player who wants to farm one can. Making spawners destructible means the
  normal way of playing removes the faucet, so this is self-limiting rather than
  solved, and that is accepted for now. Comes back if rifts turn out to be worth
  grinding rather than finishing, at which point a finite budget per spawner or drops
  that taper over a run are the levers.
- **Whether destroying spawners is optional.** Right now the objective is still just to
  reach the far end, so clearing a room is a choice. Requiring every spawner dead
  before the arena opens is available later if runs feel like sprints.
- **Whether spawners drop anything themselves.** Unresolved and deliberately ignored.
- **Tool-weapons cannot be used at all.** `ToolLockGlobalItem` refuses any item with
  pick, axe or hammer power, so drills, chainsaws, the Picksaw and the axes and hammers
  that double as early weapons cannot be swung at an enemy either. Accepted for now.
  Comes back if vanilla items get re-weighted into the loot tables, since a few would
  arrive unusable; the fix is to block the digging action rather than the item.

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

### Queued 21 September, evening

**System / Encounters** (`3458764684378310138`, at 1756, 5400)

Child element IDs not yet read; read the frame before editing.

- Subtitle → "What you fight on the way, and where it comes from."
- How it works → vanilla ambient spawning off inside rifts; enemies come from spawners
  placed by generation; a spawner carries a pool from the tier's biome, an interval, a
  cap on its own live enemies, and an activation radius; spawners are destructible
  stationary NPCs, so killing one ends its flow; the boss is the same mechanism with a
  count of one and a trigger; simple on purpose, with waves and finite budgets later.
- Interacts with → add Boss Adapters alongside Rift Generation.

**System / World Edit Rules** (`3458764684378270266`)

Child element IDs not yet read; read the frame before editing.

- How it works → the rule applies in the Hold as well as in rifts; mining and placement
  are each off by default with a per-tile whitelist; the placement whitelist admits
  what helps a player see and survive but not what reshapes the layout; spawners are
  NPCs rather than tiles, so destroying them needs no exception and the no-mining rule
  holds without qualification.
- Status `3458764684378957947` → Shaped (`#067429`). The mechanism is settled and only
  the whitelist contents remain, which is content rather than a blocking decision.

**System and Loop Matrix**, revised totals

With Encounters and World Edit Rules both moving, the footer
`3458764684378957987` should read "Eleven shaped, one open, one deferred", leaving
Multiplayer as the only Open entry.

**System / Rift Generation** (`3458764684378270263`)

- How it works `3458764684378270293` → generation now also places spawners, so add that
  to the list of what a tier controls.

**System and Loop Matrix** (`3458764684378900897`)

- Encounters, status `3458764684378957952` → Shaped (`#067429`), assuming the loot
  faucet question below is treated as tuning rather than a blocker.
- Footer `3458764684378957987` → "Ten shaped, two open, one deferred", leaving World
  Edit Rules and Multiplayer as the only Open entries.

**Reward Tables** (`3458764684382508230`)

- Add the loot faucet conflict to the STILL OPEN list (`3458764684384272807`), since it
  is a reward problem as much as a spawning one.
