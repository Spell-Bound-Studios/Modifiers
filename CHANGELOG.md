# Changelog

## [0.1.3] - 2026-07-07

### Added
- Inline modifiers: `RolledContribution` (a self-describing packable — carries its own `statHash`/`type`/`value`, so it applies and displays with no registry lookup and no schema) for item implicits authored inline, no ScriptableObject required.
- `IRolledModifier` — the uniform face over named (`RolledModifier`) and inline (`RolledContribution`) modifiers: `TryApplyTo`/`RemoveFrom`/`SourceId`. Both are `ISmartPacker`, so a single mixed list (e.g. an item carrying Iron Will *and* an inline stat roll) packs polymorphically.
- `ContributionRange.Roll(rng)` and `RollContribution(rng, sourceId)` — roll a single range to a value or a self-describing contribution; `ModifierDefinition.Roll` now reuses it.

## [0.1.2] - 2026-07-06

### Added
- Derived contributions: `StatBlock.AddDerived(stat, type, sourceStat, ratioPerPoint, ...)` — read-time, chain-aware scaling (e.g. mana per intelligence). `ContributionRange.sourceStat` makes derivation authorable and rollable, so "gain X per point of Y" ships as ordinary modifier content.

## [0.1.1] - 2026-07-06

### Added
- Chain-aware `Modifiable.Changed` event: raises for own-block writes and re-raises ancestor changes, so a child (skill) hears a parent (player) equip.
- `StatTemplate.ApplyTo`/`RollInnate` log a warning on null rows instead of silently skipping.

## [0.1.0] - 2026-07-05

Ground-up rewrite of the modifier system.

### Added
- `Modifiable` with parent chains: entities compose a stat block and circuits; children resolve stats through their parents with merge-then-resolve math (one additive increased bucket across layers, multiplicative more, min-wins overrides).
- `StatBlock`: deterministic fixed-point contribution math (`Flat`/`Increased`/`More`/`Override`), conditional contributions with owner-perspective evaluation, per-stat dirty caching, source-id removal, condition-cycle guard, and a `Changed` event.
- Circuits: ordered `Stage`s holding prioritized, source-tracked grants; `Circuit.Evaluate` runs stages in defined order; `CircuitContext` carries the damage packet, a consequence channel (`Note`), and subject/owner perspectives; pooled contexts are allocation-free once warm.
- Conditions: `All`, `Any`, `Not`, `StatAtLeast` — shared by circuit gates and conditional stat contributions.
- Named modifiers: `ModifierDefinition` (rollable `ContributionRange` recipes with step quantization, display metadata), `RolledModifier` (packable roll artifact with apply/remove by source), `ModifierRegistry`, and `ModifierPool` built on a generic, reusable `WeightedPool<T>` (weighted sampling, without-replacement default, injected rng).
- Timed modifiers (buffs/debuffs): `TimedModifier` (packable, carries remaining duration) and `TimedModifierSet` (caller-ticked ledger: refresh-on-reapply, dispel, clear, restore-with-remaining, `Changed` event). The library never owns the clock.
- `Modifiable.RemoveSource` — one call strips everything a source granted: stat contributions and circuit grants.
- EditMode test suite (~130 tests), including performance benchmarks (Unity Performance Testing) and zero-allocation regression guards.
- Rebuilt sample scene: player/enemy/fireball/level demo — item roster, star-rolled unique enemies, level-wide inherited modifiers, shield regen, chaos shield bypass, circuit visualizer, and a quadrant nameplate HUD with a live legend.

### Changed
- `StatBlock.AddModifier` renamed to `AddContribution`; `ModifierType` renamed to `ContributionType`.
- `StatData` callback signatures track Core 1.1.0 (`byte surfaceIndex`); added `StatData.Context` constants.
- `StatRegistry` load failures reset cleanly; `StatSettings` precision locks after first conversion.
- Requires com.spellboundstudios.core 1.1.0 (dev branch).

### Removed
- Legacy behaviour/pipeline system (`SbBehaviour`, `BehaviourContainer`, `IModifier`/`SbModifier`, pipeline nodes, the event system) — superseded by `Modifiable` + circuits.
- `StatBlock.GetValue` block-local read — `Modifiable.GetValue` is the single read path.
- `Circuit.Root` — a circuit is its ordered stages.
