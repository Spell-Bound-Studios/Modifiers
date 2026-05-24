# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this library is

`Spellbound.Modifiers` is a **standalone, drop-in stats/modifiers system** designed to be a complete replacement for
anyone wanting Path-of-Exile-style modifier semantics in their Unity game. It is pre-1.0 — there is no
backward-compatibility contract yet. When clarity demands renaming, restructuring, or deleting public API, do it. *
*Architectural strength beats API stability** until 1.0.

The library has two stakeholders to keep in mind whenever you change it:

- **The author (Corsairs Isle / `_GameLogic`)** — consumes this lib for every stat in the game. Player-side glue lives
  in the outer repo at `_GameLogic/Player/Runtime/SbStatsComponent.cs` and `SbNetworkComponent.cs`.
- **External Unity developers** — install via `package.json` (`com.spellboundstudios.modifiers`) and expect the README's
  getting-started flow to "just work."

## This directory is its own git repository

`Assets/_Project/Modifiers/` is a **nested sibling repo** (`git@github.com:Spell-Bound-Studios/Modifiers.git`) cloned in
place inside the outer game repo. Changes here do not appear in `git status` from the outer `CorsairsIsleDev/` working
directory — commit and push from within this directory. See the project-level `CLAUDE.md` (one level up under
`Assets/_Project/`) for the multi-repo layout.

## Assembly + dependency wiring

- One runtime assembly: `Spellbound.Modifiers` (`Runtime/Spellbound.Modifiers.asmdef`).
- Depends on exactly one other assembly: `Spellbound.Core` (referenced by GUID `c14a5db03514b8d4ba10b621ed3627d5`).
  Concretely this gives us `[Immutable]`, `SpritePreview`-style tooling attributes, and the `Spellbound.Core.Tooling`
  namespace.
- `package.json` declares the same: `com.spellboundstudios.core` is the only runtime dependency.
- Editor assembly: `Editor/` (custom property drawers for `[DropdownPicker]` and `[SpritePreview]`).
- **No Unity-engine-specific networking, ECS, or rendering dependencies.** Keep it that way — anything that ties this
  lib to a specific runtime stack (PurrNet, Entities, URP) belongs in `_GameLogic`, not here.

## Architectural layer model (READ THE README)

`README.md` is the source of truth for the four-layer model (Data → Engine → Convenience → Educational). Read it before
non-trivial changes. Quick map from layer to current directory:

| Layer                           | Role                                                                           | Current location                                                                                 |
|---------------------------------|--------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------|
| 0 — Data                        | Pure structs, enums, DTOs, payloads                                            | `Runtime/Core/DataTransferObjects/`, `Runtime/Core/Payloads/`, `Runtime/Core/Modifiers/` (enums) |
| 1 — Engine                      | Contracts + containers + algorithms                                            | `Runtime/Core/Interfaces/`, `Runtime/Core/Containers/`, `Runtime/Core/Registries/`               |
| 2 — Convenience                 | Base classes, fluent extensions, inspector tooling, ScriptableObject authoring | `Runtime/API/`                                                                                   |
| 2 — Convenience (drag-and-drop) | `MonoBehaviour` components for designers who don't want to write boot code     | `Runtime/Components/`                                                                            |
| 3 — Educational                 | Concrete skills/behaviours/modifiers, demo scene                               | `Samples/`                                                                                       |

**Dependency direction is one-way.** A `using` from `Runtime/Core/*` into `Runtime/API/*` is an architectural
violation — flag it and push the offending code down a layer (or pull the dependency up). Same for anything in
`Samples/` leaking into `Runtime/`.

**Mismatch with the README's ideal layout that's worth knowing:**

- README documents `Samples~/` (tilde excludes from compilation, opt-in via Package Manager). Today this lib ships
  `Samples/` (no tilde), so samples compile alongside runtime. If you're polishing the library for external
  distribution, renaming to `Samples~/` is the right move — but coordinate with the demo scene that currently references
  those scripts directly.
- README documents a `Tests/` directory. **There are currently no tests.** Adding edit-mode tests (especially around
  `StatContainer` calculation order and fixed-point math) is high-value work.

## The mental model: Behaviour / Modifier / Container / Skill

This is the architectural spine. Every decision should reinforce these roles:

- **`SbBehaviour` (Layer 2 base, `Runtime/API/SbBehaviour.cs`)** — a *pure capability*. Knows HOW to do exactly one
  thing (fire a projectile, deal cold damage, run a beam). Owns its own `StatContainer`. Does **not** know when it runs,
  what triggers it, or what comes after.
- **`SbModifier` (Layer 2 base, `Runtime/API/SbModifier.cs`)** — mutates a target via `Apply(ICanBeModified)` /
  `Remove(ICanBeModified)`. Uses the protected `TryGetBehaviour<T>` / `TryGetStats` / `TryGetEvents` helpers to reach
  into the target's containers. Carries its own `UniqueId` so stat removal can target the exact modifier instance that
  was added.
- **`ModifiableObject` (Layer 2 base, `Runtime/API/ModifiableObject.cs`)** — composes the three containers (`Stats`,
  `Behaviours`, `Events`). A "skill" in this lib is just a `ModifiableObject` that wires behaviours together in
  `Initialize()` — see `Samples/Scripts/Skills/Fireball.cs` and `RayOfFrost.cs`.
- **Orchestration belongs to the GAME, not the library.** The library does not define triggers, cooldowns, or
  scheduling. `Fireball.OnCast → _projectile.Launch → _fire.DealFireDamage` is a sample showing one orchestration shape;
  do not promote it into the library.

The three containers (`StatContainer`, `BehaviourContainer`, `EventContainer`) live in `Runtime/Core/Containers/`.
Composability interfaces (`IHasStats`, `IHasBehaviours`, `IHasEvents`, `ICanBeModified`, `IHasUniqueId`, `IModifier`)
live in `Runtime/Core/Interfaces/`. **A power user can implement these interfaces directly without ever
touching `SbModifier` / `ModifiableObject`.** Preserve that escape hatch — it's the contract with the 20% power user.

## Stat math is fixed-point, not float

`StatContainer` stores all values as scaled `int` (default scale = 10000 = four decimal places).
`StatSettings.SetDecimalPrecision(n)` configures this globally and is called by `StatDatabase.RegisterAll`. Reasons this
matters:

- Determinism (network sync, replay, save-load).
- The calculation order is fixed in `CalculateStat`:
  `Base → Flat (additive) → Increased (additive pool, applied once) → More (multiplicative chain) → Override (last one wins, ignores everything)`.
  This is the PoE model — do not casually reorder it.
- `_isDirty` flagging means `GetValue` only recalculates when modifiers change. Don't add code paths that read internal
  dictionaries directly; go through `GetValue`/`GetBase`.

`StatRegistry` (`Runtime/Core/Registries/StatRegistry.cs`) is a **global static** mapping `string ↔ int` for stat IDs.
Strict validation (toggled by `StatDatabase.RegisterAll(strictStatValidation: true)`) throws on any stat name not
declared in the asset — use this in shipping configurations. Because it's global state, **`StatRegistry.Clear()` is
required between unit tests**.

`StatContainer` implements `IPacker` (from `Spellbound.Core.Packing`) so containers can live inside packed per-instance
data slots (chunk data, save files, network sync). The wire format packs stat **names**, not IDs — IDs are process-local
and would otherwise drift between save and load or between server and client. On unpack the names are re-interned via
`StatRegistry.Register`, which means **the host's `StatDatabase` must be registered before any packed container is
unpacked** (otherwise strict validation throws). The library does not ship a per-instance `IDecodableData` wrapper
itself — that lives in the consumer game (see `_GameLogic/CorsairsWorld/Stats/StatsData.cs` for the Corsairs Isle
wrapper that pairs `StatContainer` with `IDefaultDataProvider<StatsData>` on `StatsModule`).

## Authoring flow (the "getting started" path)

This is the path an external user should walk. If you change anything below, update `README.md` in lockstep — the
getting-started feeling is the library's main pitch.

1. Create a `StatDatabase` asset: *Create → Spellbound/ModifierLib/Stat Database*. Set decimal precision.
2. Create one `StatDefinition` per stat: *Create → Spellbound/ModifierLib/Stat Definition*. Add it to the database's
   stat list. Optionally assign a `StatDisplayFormat` for UI formatting (prefix/suffix/decimals).
3. Register the database at game start. The drag-and-drop path is `StatDatabaseLoader` (`Runtime/Components/`): add the
   component to a GameObject in the bootstrap scene, assign the database (or put one in `Resources/`), done. Code path:
   call `statDatabase.RegisterAll(strictStatValidation: true)` yourself — see `Samples/Scripts/Example/StatDemo.cs`.
4. Subclass `SbBehaviour` for each capability. Use `protected override StatContainer InitializeStats()` to seed
   `SetBase("stat_name", value)`.
5. Subclass `ModifiableObject` (a "skill") and `Behaviours.Add(...)` in the constructor. Wire events in `Initialize()`.
6. Subclass `SbModifier` and use `TryGetBehaviour<T>` / `stats.AddFlat|AddIncreased|AddMore` (extension methods in
   `Runtime/API/ContainerExtensions.cs`) inside `Apply` / `Remove`.
7. Optionally, expose modifiable objects + modifiers as `[SerializeReference]` on a `ModdedCollection` ScriptableObject
   for designer-driven authoring (see `Runtime/API/ModdedCollection.cs`).

The end-to-end demo in `Samples/Scripts/Example/SkillModifierDemo.cs` exercises all of the above with both a code-built
skill (`Fireball`) and an SO-built skill (`RayOfFrost` via `ModdedCollection`).

## Conventions specific to this library

- **Copyright header** on every C# file: `// Copyright 2026 Spellbound Studio Inc.`
- Runtime namespace is **flat**: everything under `namespace Spellbound.Modifiers { ... }` regardless of subfolder.
  Samples use `Spellbound.Modifiers.Samples`. Editor code uses `Spellbound.Modifiers.Editor`. **Do not
  add `.Core`, `.API`, `.Containers` namespace segments** — directory structure expresses the layer, the namespace stays
  flat.
- Concrete `SbModifier` / `SbBehaviour` / `ModifiableObject` subclasses are typically `[Serializable]` and `sealed`. The
  `[Serializable]` is required for `[SerializeReference]` authoring (e.g. via `ModdedCollection` and the
  `DropdownPickerDrawer`).
- Stats are looked up by **string name** in user-facing code (`stats.GetValue("projectile_count")`) —
  `StatRegistry.Register` interns the string to an int the first time it's seen. Don't expose raw `int` stat IDs in
  user-facing API surfaces; keep them internal to `StatContainer` / `StatModifier`.
- **Use `Spellbound.Core.Logging.Log`** (`Log.Info/Warn/Error/Debug/Verbose`), not `UnityEngine.Debug.Log*`. Core is
  this lib's dependency precisely so we can leverage it — falling back to `Debug.Log` defeats its purpose. `Log.Error`
  is never stripped; the others are gated by `SPELLBOUND_LOG_*` scripting defines. Legacy `Debug.Log*` calls in
  `StatDatabase`, `StatContainer`, and the samples are pre-migration and should move to `Log.*` when touched.
- Outer-repo files use Allman-ish brace style with `_camelCase` private fields and properties wired through getters.
  Match that.

## Editor tooling worth knowing

- `[DropdownPicker]` (`Runtime/API/Attributes/`) works on **both** `SerializeReference` polymorphic fields (lists
  managed types implementing the field's interface) **and** `ObjectReference` fields (lists matching ScriptableObject
  assets in the project). Implementation: `Editor/DropdownPickerDrawer.cs`. This is the mechanism that powers
  `ModdedCollection`'s inspector — `modifiableObject` picks a type, `modifiers` picks a list of types.
- `[SpritePreview]` renders a sprite preview inline in the inspector. Used on `StatDefinition.icon`.
- `StatDefinition.OnValidate` re-runs the display formatter against a preview value (150.55) so editors get live
  feedback on `StatDisplayFormat` changes.

## Testing and running

- **No automated tests exist in this library yet.** The Unity-level test runner is the outer project's, not this nested
  repo's. If you add tests, put them in `Tests/Editor/` (edit-mode) and `Tests/Runtime/` (play-mode) with their own
  `.asmdef`. Cover at minimum: PoE calculation order, fixed-point precision, strict validation, modifier add/remove by
  `UniqueId`.
- The interactive demo runs from `Samples/Scenes/StatExamples.unity` inside the Unity editor — open the scene and press
  play. There is no headless harness committed.

## Designing for modifier-first scope

The lib targets ~10k+ stats and arbitrary numbers of behaviours/modifiers per target. Every new primitive must ask: **"
can a modifier reach this?"** Before adding a hardcoded strategy, formula, or policy, check whether a talent / gear /
buff might plausibly want to alter it. If yes, the strategy belongs on a `SbBehaviour` (which modifiers can add /
remove / tune), not baked onto the data type.

Common failure modes to watch for:

- **Hardcoded calculators.** A `DamageCalculator` with fixed mitigation rules locks out conversions ("50% of phys taken
  as fire"), redirections ("damage to mana before life"), and per-type custom math. The "calculator" is the *composition
  of behaviours on the target* — not a service class.
- **Narrow naming.** `Vital` implies life/death — wrong for rage, energy, focus, stamina, charges, soul. Prefer generic
  names like `ResourcePool`. Same trap: don't name primitives after the first use case.
- **Designing for one concrete scenario.** "Make the tree take damage" → built health-specific scaffolding. The right
  framing is "any target receives any damage type into any resource pool." Tree damage is one instance of the general
  model.
- **Bypassing `ModifiableObject` / `SbBehaviour` / `SbModifier`.** These are the lib's language. New gameplay rules are
  usually a new `SbBehaviour` subclass + an `SbModifier` that adds it — not a new top-level service or hardcoded
  receiver.

### Stats vs ResourcePools

Both have a `{baseline, current, ceiling}` shape, but they are distinct concepts and should remain so:

- **Stats** are computed from base + modifiers. `GetValue(stat)` is "current." There is no separately-stored current
  value. Buffs/debuffs are `SbModifier`s on the stat container.
- **ResourcePools** store a `current` value that depletes / restores from gameplay events. The pool's `Max` is a *live
  read* from a referenced stat in the modifier container, so modifier-driven changes to the max-stat (e.g.
  `+50 max life` buff) automatically affect the pool's ceiling.

A transient effect like "shield for 100 damage for 10s" is a *transient ResourcePool* added at runtime by an
`SbModifier.Apply` — modifiers can add pools, not just mutate stats.

## When making changes here

1. Identify which layer the change belongs to. If a Data type starts growing methods, it's drifting to Engine — split
   it.
2. Check whether the change leaks an external dependency into the runtime asmdef. Only `Spellbound.Core` belongs there.
3. If you're touching containers or `StatContainer.CalculateStat`, write a test first (even if there is no test infra
   yet — start it). The math is the most catastrophic-to-regress part of the library.
4. If you rename or remove a public type/method, update the README's getting-started snippets and the sample scripts in
   the same commit — they are the de-facto documentation.
5. When committing: this is its own repo. `git status` and `git commit` must be run from inside
   `Assets/_Project/Modifiers/`. A feature spanning Modifiers + `_GameLogic` is two commits in two repos.
