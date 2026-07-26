# Changelog

## [0.2.3] - 2026-07-26

### Changed
- Package `displayName` is `Spellbound.Modifiers`, matching the other Spellbound packages in the Package Manager list.

## [0.2.2] - 2026-07-09

### Added
- `DerivedMagnitude` read accessors — `Amount`, `PerPoints`, `Stepped`, `Source`, `Perspective` — so a tooltip can format "per N points of X" lines (same category as 0.2.0's `RolledMagnitude.Min`/`Max`/`Step`).

### Changed
- `ContributionSpecification.Lines` renamed to `StatContributions` and each entry now carries the `ContributionType`: `(stat, type, amount)` — the elementary stat contributions a shape decomposes into, complete enough for shape-agnostic consumers. A tooltip can render any future shape as honest per-stat lines without a per-shape format case (the sample item card now does exactly that as its fallback); the registry validates through the same enumeration. Shape-idiomatic prose ("11-19 Fire Damage") remains a per-shape, game-side concern.

## [0.2.1] - 2026-07-09

### Fixed
- `ModifierGrantSet.Apply` now applies named grants under the `sourceId` parameter, re-keying each rolled record to it (the persisted record is untouched). Previously named lines applied under their roll-time stamped id while inline lines obeyed the parameter — so rolling at drop and applying at equip under a slot id split the set across two ids, and a roll-time id of `Contribution.None` produced permanent, unremovable contributions that stacked on every re-equip. Records remain self-keyed in standalone flows (`StatData`, `TimedModifierSet`, `TryApplyTo`/`RemoveFrom`); the grant set is set-keyed.

## [0.2.0] - 2026-07-09

One authored list for putting modifiers on a thing — named or inline per entry, same pathways underneath.

### Added
- `ModifierGrant` / `ModifierGrantSet` / `RolledGrants` — a `[SerializeReference]` list where each inspector entry is a `NamedModifierGrant` (a `ModifierDefinition` reference, hash-traceable) or a contribution line authored in place (no asset, no hash — its route back is the owning thing). `Roll(rng, sourceId)` at the owning instance's creation returns a packable `RolledGrants` (stat-keyed baked rolls + rolled named modifiers); `Apply` hydrates it back; everything lands under one source id, so `RemoveSource` strips the set.
- Contribution shapes, chosen per entry by the type picker: `SingleStatContribution` (stat + type + amount), `StatBandContribution` (low/high ends across two stats, any magnitude per end — "adds 1-3 to 3-5 fire damage", "5 to 10 per 25 strength"), `MultiStatContribution` (one amount rolled ONCE, shared by every listed stat — "+10-20 to all resistances"). New shapes are added by subclassing `ContributionSpecification`; the picker lists them automatically.
- `RolledMagnitude.Min` / `Max` / `Step` read accessors (tooltip roll-range display).
- Item sample: `ItemDefinition` (implicits as one grant list + a `ModifierPool`) and `ItemInstance` (construction = the drop moment; implicits roll once and belong to the instance), with a HUD item card — drop/equip/craft buttons, implicit lines shown without ranges, named lines with them. Staff item, T1 modifier pool, fire damage min/max stats; the fireball samples the range per cast.

### Changed
- `ContributionSpecification` is abstract and extends `ModifierGrant` — contribution lines are granted directly. `ModifierDefinition.contributions` is `[SerializeReference]`; **modifier assets re-authored** (all samples included). Registry validation walks each spec's `Lines`.
- `linkOrdered` → `keepOrdered`, now honest: the clamp engages against fixed ends too, adjusts whichever end rolled (never re-rolls), and `IsValid` rejects ordered bands with derived ends or statically inverted fixed pairs.

### Removed
- `ContributionSet` — superseded by `ModifierGrantSet`, whose `Roll`/`Apply` are the bake/hydrate verbs it lacked; an all-inline grant set is the same thing with a wire story.
- The `ContributionSpecification` foldout drawer — the generic `SerializeReferencePicker` covers grants, contribution shapes, and magnitudes; the paired-stat foldout (and its mistaken "re-rolls" tooltip) is gone.

## [0.1.9] - 2026-07-08

### Added
- `ContributionSet` — a serializable bundle of `ContributionSpecification`s a consumer embeds on its own type to author inline stat lines (fixed / rolled / derived / paired min-max) with no ScriptableObject per value. `RollAndApply(target, rng, sourceId)` rolls and applies them under a caller-supplied source id — the owning instance's hash for removal via `Modifiable.RemoveSource`, or `Contribution.None` for permanent. It has no wire format; the inline value is recoverable from the owning thing. Invalid specs are warned and skipped.
- Shared per-spec logic lifted onto `ContributionSpecification`: `Bake` (rolled endpoints + linkOrdered clamp), `ApplyBaked`, and `RollAndApply`. `ModifierDefinition.Roll` and `RolledModifier.ApplyTo` now delegate to these — one implementation for the named and inline paths, byte-identical results.

## [0.1.8] - 2026-07-08

One modifier concept, no categories, no batch rollers.

### Removed
- `ModifierPool.Roll` and `StatTemplate.RollInnate` — batch rollers that existed only to loop-and-stamp source ids, which is the sole reason the `Func` id-provider existed. Rolling is `definition.Roll(rng, sourceId)`; a caller loops over `pool.Sample(count, rng)` or `template.Modifiers` and chooses each id inline. No `Func`.

### Changed
- `StatTemplate.innateModifiers` / `InnateModifiers` → `modifiers` / `Modifiers`. "Innate" was never a modifier category — just the list of modifiers a template happens to hold.
- `Contribution.Innate` → `Contribution.None` — the reserved source id 0 meaning "no removable source," not a kind of modifier.

## [0.1.7] - 2026-07-08

### Changed
- `ModifierPool.Roll` and `StatTemplate.RollInnate` take a `Func<uint>` source-id provider — the caller owns modifier identity. The library no longer mints source ids from the content rng (which coupled ids to the value stream and, via a session-scoped counter, collided with persisted ids on reload). A persisting caller supplies stable ids (e.g. hashed from the owning instance's GUID); a throwaway caller supplies a counter.
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
