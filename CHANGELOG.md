# Changelog

## [0.1.7] - 2026-07-08

### Added
- `ModifierSource.Next()` — a monotonic id allocator for rolled modifiers, independent of the roll rng. `ModifierPool.Roll` and `StatTemplate.RollInnate` use it instead of drawing source ids from the content rng (which coupled ids to the value stream and risked collisions).

### Changed
- Validation is now deep: `Magnitude.IsValid` (virtual) + `DerivedMagnitude.IsValid` (amount and source must be assigned); `ContributionSpecification.IsValid` checks the magnitude recursively and that a paired stat differs from the primary. A `DerivedMagnitude` missing its source or amount is a load-time error instead of a silent no-op.
- `ModifierRegistry` rejects two rolled contributions on the same stat within one definition — their baked values are stat-keyed and would collide.

## [0.1.6] - 2026-07-08

Collapsed to a single modifier model: every modifier is a `ModifierDefinition`, every roll is a hash plus baked values.

### Removed
- `ContributionRange`, `RolledContribution`, `IRolledModifier`, `ModifierGrant` — the by-value / inline path. A modifier's identity (its definition hash) is what alt-hover roll-ranges, reroll, trade, and networking all key off; a by-value mod has discarded its identity, so it was incoherent with those mechanics. One rolled type, one apply path.
- The 4-arg `AddDerived` shim (pre-divided ratio) — `DerivedMagnitude` uses the full `(amount, perPoints, stepped, perspective)` form.
- `ISmartPacker` / `[PackerId]` on `RolledModifier` — a homogeneous `List<RolledModifier>` packs with plain `PackList`.

### Changed
- `ContributionSpec` renamed to `ContributionSpecification` (no abbreviations).
- `RolledModifier` is `{ modifierHash, sourceId, baked[] }`, plain `IPacker`. Apply looks the definition up by hash and runs each `Magnitude` with the baked values.

## [0.1.5] - 2026-07-07

Redesign of the contribution model — separates structure (definition-owned, live) from rolled magnitude (baked per-instance).

### Added
- `Magnitude` (`[SerializeReference]`): `FixedMagnitude`, `RolledMagnitude`, `DerivedMagnitude`. Derived is `(amount, perPoints, stepped, perspective)` with a nested `ScalarMagnitude amount`, so "(1-2) per 10 Strength" composes a rolled coefficient with live per-attribute scaling. `perPoints` is authored legibly (the "per 20"), stepped gives breakpoints, perspective picks whose stat scales it.
- `ContributionSpec` — a definition contribution: stat + type + magnitude, optionally a linked `pairedStat`/`pairedMagnitude` (damage bands, low <= high enforced on roll).
- `StatBlock.AddDerived(stat, type, source, amount, perPoints, stepped, perspective, ...)`; the old `ratioPerPoint` overload remains as a continuous, owner-perspective shim.

### Changed
- `ModifierDefinition.contributions` is now `List<ContributionSpec>`; **existing modifier assets must be re-authored** in the inspector.
- `RolledModifier` carries `BakedRoll[] baked` (stat-keyed) instead of positional `values[]`. Apply reads structure live from the definition and pulls only rolled endpoints from the baked artifact — so a definition rebalance (e.g. "per 10" -> "per 20") reaches existing rolled items on next apply, and reordering a definition can no longer misalign saved values.
- `ModifierRegistry` load-time validation asserts each spec has a stat and magnitude assigned.

### Editor
- `MagnitudePropertyDrawer` (new `Spellbound.Modifiers.Editor` assembly): a type picker for the `[SerializeReference]` magnitude fields — pick `FixedMagnitude`/`RolledMagnitude`/`DerivedMagnitude` and its fields draw inline. Nested (a `DerivedMagnitude`'s `amount`) gets its own picker, filtered to `ScalarMagnitude`.

### Notes
- `ContributionRange` is retained for the simple inline path (`ModifierGrant`, `RolledContribution`); unifying it onto `Magnitude` is future work.

## [0.1.4] - 2026-07-07

### Added
- `ModifierGrant` — one authored unit that is either a named `ModifierDefinition` (Iron Will) or an inline `ContributionRange` (e.g. 1-3% life regen), rolling to an `IRolledModifier` either way. The reusable vocabulary for any modifier source: item implicits, drop pools, crafting. Roll a list of them, `SmartListToBytes` the result, done.

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
