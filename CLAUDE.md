# CLAUDE.md — Spellbound.Modifiers

Guidance for Claude and agents working in this repository. README is for external users; this file is for architectural context, conventions, and tripwires when CHANGING the library.

## What this library is

`Spellbound.Modifiers` is a standalone, drop-in stats and modifiers system. PoE-style semantics — base + ordered modifier chain → final value, computed at read time, deterministic. Slots into any Unity game's data layer without dragging engine-specific dependencies.

**Pre-1.0.** No backward-compatibility contract. **Architectural strength beats API stability** until 1.0 — when clarity demands renaming, restructuring, or removal, do it.

Two stakeholders to keep in mind:

- **Corsairs Isle** (`_GameLogic`) — first real customer; consumes this lib for every stat in the game.
- **External Unity developers** — install via `package.json` (`com.spellboundstudios.modifiers`); expect the README's mental model to "just work."

## Repo + git workflow

This directory is a **nested sibling repo** (`Spell-Bound-Studios/Modifiers`) cloned in place inside the outer Corsairs Isle game repo. Changes here do not appear in the outer repo's `git status`.

**Git policy (project-wide):** only the user runs state-mutating git operations (`add`, `commit`, `push`, `reset`, `revert`, `restore`, `checkout`, `rebase`, `merge`, `cherry-pick`, `clean`, `rm`, `mv`, `stash`, branch / tag deletion). Main Claude and every subagent restrict git usage to read-only inspection (`status`, `diff`, `log`, `show`, `blame`). Hard-denied in `.claude/settings.json` at the outer game repo and applies across every repo in the project. Do not propose workarounds.

- When the user commits / pushes, they do it from inside `Assets/_Project/Modifiers/` — this is its own repo.
- A feature spanning Modifiers + `_GameLogic` is two commits in two repos.
- The outer repo's `_GameLogic/Docs/` folder is **local-only** (gitignored) and carries forward-context planning docs. Read those before structural changes — especially `Docs/RoutingLayer.md`.

## Dependencies

- One runtime assembly: `Spellbound.Modifiers` (`Runtime/Spellbound.Modifiers.asmdef`).
- One runtime dependency: `Spellbound.Core` (asmdef GUID `c14a5db03514b8d4ba10b621ed3627d5`).
- Editor assembly: `Editor/` (property drawers).
- **No Unity-engine-specific networking, ECS, or rendering deps.** Anything that ties this lib to a specific runtime stack (PurrNet, Entities, URP, Cinemachine) belongs in the consumer.

## Mental model: four roles

The architectural spine. Every change reinforces these roles.

| Role | Location | Owns | Doesn't know |
|---|---|---|---|
| `SbBehaviour` | `Runtime/Behaviours/SbBehaviour.cs` | A capability AND the stats that govern it. Fixed-point ints, PoE math. | When it runs, what triggers it, what's next. |
| `SbModifier` | `Runtime/Modifiers/SbModifier.cs` | An apply/remove operation on a target. Carries `UniqueId` for instance-precise removal. Implements `IPacker`. | The target's other modifiers. Reaches in through `IHasBehaviours` / `IHasEvents`. |
| `ModifiableObject` | `Runtime/Modifiers/ModifiableObject.cs` | A composed target. Owns `BehaviourContainer` + `EventContainer`. A skill is just one whose `Initialize()` wires its behaviours. | What gameplay system owns or triggers it. |
| The game | (consumer) | Triggers, cooldowns, scheduling, networking, save/load, scenes, talent trees, loot tables. | Library internals — kept that way intentionally. |

**Power users implement the contracts directly** — `IModifier`, `IHasUniqueId`, `IHasBehaviours`, `IHasEvents`. The base classes are the 80% path, not the only path. Preserve that escape hatch.

There is **no separate `StatContainer`** — stats live ON `SbBehaviour` itself. There is **no `IHasStats`** — stats are scoped to specific behaviours and routed via `IHasBehaviours`. If you see references to `StatContainer` or `IHasStats` in commits / comments / memory, those are stale.

## PoE math

Order, fixed in `SbBehaviour.CalculateStat`:

```
Base
  → + Σ Flat
  → × (1 + Σ Increased)
  → × Π (1 + More)
  → unless any Override exists, in which case the last Override wins (ignores everything)
```

Fixed-point `int` throughout — default scale 10000 (four decimal places), configurable via `StatSettings.SetDecimalPrecision`. Deterministic across machines; survives serialization round-trips; matters for network sync, replay, save-load.

`_isDirty` flagging means `GetValue` only recalculates when modifiers actually changed. Don't add code paths that read internal dictionaries directly — go through `GetValue` / `GetBase`.

`StatRegistry` (`Runtime/Registries/StatRegistry.cs`) is **global static**, mapping `string ↔ int`. Strict validation (toggled via `StatDatabase.RegisterAll`) throws on stat names not declared in the asset — use strict in shipping configs. Because it's global state, **`StatRegistry.Clear()` is required between unit tests**.

`SbBehaviour` implements `IPacker`. **Currently packs stat names** (length-prefixed string + value bytes); on unpack the name is re-interned via `StatRegistry.Register` to recover the local id.

Stat ids ARE deterministic across builds when registration goes through `StatDatabase.RegisterAll` — the asset iterates a fixed list order; all clients load the same asset → same ids. Pack-by-name is a defensive choice: it survives the edge case where some code path calls `StatRegistry.Register("foo")` ad-hoc before the database registers (which would shift every later id by one).

**Long-term direction: pack stat ids** instead of names — tighter wire format, no string interning on the hot path. Blocker is locking down the registration surface so `Register` can only happen via `RegisterAll`. Switch is a small refactor in `SbBehaviour.Pack` / `Unpack` when we get there.

Constraint that holds either way: **the host's `StatDatabase` must be registered before any packed container is unpacked**, otherwise unknown names get phantom ids (or, under strict validation, throw).

## Authoring architecture

The locked-in shape for designer authoring:

- **`Affix`** (`Runtime/Modifiers/Affix.cs`) — **abstract** base for anonymous stat-flavor modifiers. Owns data (stat / modifier type / value), `Initialize` chaining, `Pack` / `Unpack`. Apply / Remove stay abstract — the consumer ships a concrete subclass (e.g. Corsairs Isle's `StatAffix`) that implements routing to whichever `SbBehaviour` should receive the modifier.
- **`Trait`** (`Runtime/Modifiers/Trait.cs`) — `ScriptableObject` for a named, registered identity (DisplayName, Icon, Description, embedded `SbModifier` effect). For player-recognized things: "Iron Will", "Thick Hide". Tiered variants are separate assets sharing a C# class.
- **`TraitRef`** (`Runtime/Modifiers/TraitRef.cs`) — inline `SbModifier` wrapper around a `Trait` asset reference. Lets a `[SerializeReference] List<SbModifier>` hold mixed `Affix` + named-identity entries. Apply clones the trait's effect; Pack writes the trait's hashed uint key.
- **`TraitRegistry`** (`Runtime/Registries/TraitRegistry.cs`) — scans `Resources/Traits/` at first query. Indexes by string key AND FNV-1a uint hash. Asserts no collisions. `TraitRegistryLoader` is the eager-load drop-in component.
- **`ModifierPool`** (`Runtime/Modifiers/ModifierPool.cs`) — drop-generation `ScriptableObject`. Holds `[SerializeReference] List<PoolSlot>`. `PoolSlot` abstract; lib ships concrete `TraitSlot` and abstract `AffixSlot` (with a `CreateAffixInstance` template method consumers override). Pool-level `Sample(int count, System.Random rng)` does weight-proportional with-replacement sampling.
- **`ModifierCodec`** (`Runtime/Modifiers/ModifierCodec.cs`) — polymorphic byte[] codec for inventory item data, save sections, network frames. 1-byte type tags (`Affix = 0`, `TraitRef = 1`). Affix entries carry their concrete subclass full-name string for polymorphic decode; TraitRef entries carry the trait's hashed uint key.

**Why this shape**: one-SO-per-affix bloats unmanageably at scale (~thousands of anonymous stat tweaks × multiple item slots). Inline `[SerializeReference]` data for the anonymous mass; SO-backed `Trait`s only for named identities a player will actually see. See auto-memory `project_affix_and_trait_model` for the full reasoning.

## Stat ownership and routing

**Currently a placeholder.** `StatAffix.Apply` in the consumer game hardcodes `TryGetBehaviour<PassiveBehaviour>` — known temporary, not the end state.

**The end state** is a target-driven routing layer: a modifier declares intent ("modify stat X by value V"), and the target walks every `SbBehaviour` that owns the stat, dispatching the call to each. "+5 fire damage" lights up every behaviour on the target that owns `fire_damage` — no priority, no first-only, no per-affix choice. The lib should own as much of this routing primitive as possible; some extension points may inevitably be consumer-side.

**Plan / open decisions** at `_GameLogic/Docs/RoutingLayer.md`. Cross-session anchor in auto-memory `project_stat_affix_routing_roadmap`.

When game-logic asks for "another `Affix` subclass to route differently" — **push back**. One `StatAffix`, forever. Routing is a target concern, not a modifier-type concern. `TalentAffix` / `BuffAffix` / `GearAffix` are anti-patterns.

## Conventions

- **Copyright header** on every C# file: `// Copyright 2026 Spellbound Studio Inc.`
- **Flat namespace** — everything under `namespace Spellbound.Modifiers { ... }` regardless of subfolder. Samples use `Spellbound.Modifiers.Samples`; editor uses `Spellbound.Modifiers.Editor`. Do NOT add `.Behaviours`, `.Modifiers`, `.Registries` namespace segments — directory expresses organization; namespace stays flat.
- **`[Serializable]`** on concrete `SbModifier` / `SbBehaviour` / `Affix` / `PoolSlot` subclasses. Typically `sealed`. Required for `[SerializeReference]` authoring.
- **Stats by string name** in user-facing API (`pb.GetValue("projectile_count")`). `StatRegistry.Register` interns to int. Don't expose raw int stat ids in user-facing surfaces.
- **Use `Spellbound.Core.Logging.Log`** (`Log.Info / Warn / Error / Debug / Verbose`), NOT `UnityEngine.Debug.Log*`. Core is the dependency precisely so this is available. `Log.Error` is never stripped; others gated by `SPELLBOUND_LOG_*` defines.
- **Brace style**: Allman-ish, `_camelCase` private fields, properties wired through getters. Match the existing files.

## Editor tooling

- **`[DropdownPicker]`** (`Runtime/Attributes/DropdownPickerAttribute.cs`) — works on `SerializeReference` polymorphic fields AND `ObjectReference` fields. Picker lists concrete types implementing the field's declared type (or, with a filter attribute, only types carrying that marker). Drawer: `Editor/DropdownPickerDrawer.cs`.
- **`[PickableBehaviourAttribute]`** (`Runtime/Attributes/PickableBehaviourAttribute.cs`) — opt-in marker for `SbBehaviour` subclasses that should appear in dropdowns. Without it, the subclass is hidden from authoring menus — keeps scaffolding / internal behaviours out.
- **`[SpritePreview]`** — inline sprite preview in the inspector. Used on `StatDefinition.icon`.
- **`StatDefinition.OnValidate`** re-runs the display formatter against a preview value (150.55) so editors get live feedback on `StatDisplayFormat` changes.

## Tests

**No automated tests exist yet.** Adding edit-mode tests is high-value work. Cover at minimum: PoE calculation order, fixed-point precision, strict-validation throws, modifier add/remove by `UniqueId`, `ModifierCodec` round-trip (Affix + TraitRef), pool weight distribution.

When adding tests: `Tests/Editor/` (edit-mode) and `Tests/Runtime/` (play-mode) with their own `.asmdef`.

## Modifier-first scope

The lib targets ~10k+ stats and arbitrary numbers of behaviours / modifiers per target. Every new primitive must ask: **"can a modifier reach this?"** Before adding a hardcoded strategy, formula, or policy, check whether a talent / gear / buff might plausibly want to alter it. If yes, the strategy belongs on a `SbBehaviour` (which modifiers can add / remove / tune), not baked onto the data type.

### Common failure modes

- **Hardcoded calculators.** A `DamageCalculator` with fixed mitigation rules locks out conversions ("50% phys taken as fire"), redirections ("damage to mana before life"), and per-type custom math. The calculator is the composition of behaviours on the target — not a service class.
- **Narrow naming.** `Vital` implies life/death — wrong for rage, energy, focus, stamina, charges, soul. Use generic names like `ResourcePool`. Don't name primitives after their first use case.
- **Designing for one concrete scenario.** "Make the tree take damage" → built health-specific scaffolding. The right framing is "any target receives any damage type into any resource pool." Tree damage is one instance of the general model.
- **Bypassing `ModifiableObject` / `SbBehaviour` / `SbModifier`.** These are the lib's vocabulary. New gameplay rules are a new `SbBehaviour` subclass + an `SbModifier` that adds it — not a new top-level service or hardcoded receiver.

### Stats vs ResourcePools

Both have a `{baseline, current, ceiling}` shape but are distinct concepts:

- **Stats** are computed from base + modifiers. `GetValue(stat)` IS "current." There is no separately-stored current value. Buffs/debuffs are `SbModifier`s in the stat-holding `SbBehaviour`'s modifier list.
- **ResourcePools** store a `current` value that depletes / restores from gameplay events. The pool's `Max` is a **live read** from a referenced stat, so modifier-driven changes to the max-stat (e.g. `+50 max life` buff) automatically affect the pool's ceiling.

A transient effect like "shield for 100 damage for 10s" is a transient `ResourcePool` added at runtime by an `SbModifier.Apply` — modifiers can add pools, not just mutate stats.

## Where current context lives

When this file isn't enough, the most authoritative sources:

- **Auto-memory entries** (persist across sessions; read first when the topic touches them):
  - `project_affix_and_trait_model` — locked-in authoring architecture
  - `project_stat_affix_routing_roadmap` — THE central anchor for routing
  - `feedback_use_core_packing_not_json` — never JSON for byte[] round-trip
  - `feedback_lib_ships_interfaces_not_concretes` — pluggable strategies belong consumer-side
  - `feedback_no_silent_pragmas` — never silently suppress warnings
- **`_GameLogic/Docs/`** (outer repo, gitignored) — game-side forward-context planning. Check before lib changes that game-side might be about to need.
- **`README.md`** in this directory — external-user pitch; update in lockstep with public-API changes.

## Tripwires (push back when you see these)

- A request for a second concrete `Affix` subclass to route differently → routing layer's job, not a type-system job. Point at `_GameLogic/Docs/RoutingLayer.md`.
- A new Unity-engine-specific dependency in this asmdef (PurrNet / Entities / URP / Cinemachine / etc.) → belongs in the consumer.
- A "DamageCalculator" / "HealthSystem" / similar named service class → the calculator is the composition of behaviours on the target.
- Naming a primitive after its first use case (`HealthPool`, `FireResistanceStat`) → prefer generic names.
- `Debug.Log*` anywhere in lib code → use `Spellbound.Core.Logging.Log`.
- `JsonUtility` / `Encoding.UTF8` / custom byte[] serialization → use `Spellbound.Core.Packing` (`IPacker` + `Packer.*` helpers).
- `#pragma warning disable` without explicit user approval → never silently suppress warnings.
- Hardcoding `PassiveBehaviour` (or any consumer-side type) in lib code → the lib doesn't know consumer types.
- An SO created per stat affix (`+9_armor_t1.asset`, etc.) → use inline `Affix` via `[SerializeReference]`. Assets are for named identities only (`Trait`).

## When making changes

1. Read the relevant auto-memory entries before structural changes — the architecture has iterated and stale assumptions are common.
2. Verify the change doesn't leak an external dependency into the runtime asmdef. Only `Spellbound.Core` belongs there.
3. If touching `SbBehaviour.CalculateStat`, the codec, or the registries, write a test first (even if there's no test infra yet — start it). Math and serialization are the most catastrophic-to-regress.
4. If renaming or removing a public type/method, update `README.md` in lockstep — it's the de-facto user-facing documentation.
5. Two-repo commits when a feature spans Modifiers + `_GameLogic` (or any sibling lib).
