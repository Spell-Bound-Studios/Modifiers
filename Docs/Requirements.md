# Spellbound.Modifiers — Design & Requirements

> **Working draft — subject to change until 1.0; not published documentation.** Records design
> decisions as they are locked and flags what remains open.
>
> Status tags: **[LOCKED]** decided · **[PARTIAL]** direction set, details open · **[OPEN]** undecided
> · **[PROPOSED]** follows from a locked decision, not yet ratified.

## 1. Aim & scope — [LOCKED]

A game-agnostic system for things that exist to be modified at runtime. A host declares its objects
modifiable; the library reshapes their values, capabilities, and state through one uniform mechanism —
composed at authoring, altered by gear/talents/buffs/effects, propagated between related objects —
never per-type or per-effect branching. The library knows modifiable things, the behaviours they are
made of, and the modifiers that reshape them. It knows nothing about skills, players, or in game objects.

Dependency rule: the library depends only on `Spellbound.Core`. No game logic resides in the library,
ever.

## 2. The three laws — [LOCKED]

1. **A modifier only ever modifies a behaviour.**
2. **A behaviour is composed of unique stat definitions.**
3. **A modifier with no behaviour to land on does nothing.** If no behaviour owns the targeted stat or
   capability, the modifier no-ops — no fallback, no catch-all, no default target. Routing terminates
   at a behaviour, or it terminates at nothing.

A modifiable thing *is* its set of behaviours — there is nothing else to modify. Stats live on
behaviours; modifiable state lives on behaviours; gaining or losing a capability is adding or removing
a behaviour.

## 3. Summary of locked decisions

**Events & reactions:**
- **Two-tier event surface.** A macro **surface** on `BehaviourContainer.Events`; micro events owned by
  individual behaviours. Behaviours are the only publishers — each raises its moment up onto the surface.
  Modifiers subscribe on the surface. (§ 8)
- **Publish up, lazily.** `Raise` guards on `HasHandlers` — an event with no subscriber does zero work,
  and only a behaviour that can produce a moment carries the call site. No empty events fire.
- **Hash-keyed identity, never strings.** Event ids are FNV hashes via a library `EventRegistry`
  (reusing Core `StableHash` + `HashRegistry`, collision-checked). The game owns the vocabulary as code
  constants; the library names zero events — the same seam as stats and packers.
- **Contexts (payloads) are game-owned.** The bus carries a generic `<T>` it never inspects;
  `HitContext` / `CombatContext` are game logic. One context **per shape** (combat / resource / cast …),
  never per event — this prevents "100 events" from becoming 100 types.
- **Events are local; state replicates.** The library never puts an event on the wire. A triggered
  modifier reacts by changing behaviour state; that state is what crosses the wire (§ 11–12).
- **Law-clean.** The surface is a *projection of the behaviours* — remove a behaviour and its events
  vanish — so "a modifiable thing is its set of behaviours" holds. Events are the reaction axis;
  modification still terminates at a behaviour.

**Foundations:**
- **Escape clause: ABSOLUTE.** No callback path bypasses behaviours; the behaviour is always the seam
  to a non-behaviour (it holds state and announces; the host reacts).
- **Owner/Satellite: load-bearing**, retained. `Skill` is a satellite (`SyncWithOwner`). (§ 7)
- **Pipelines/mitigation: behaviour-owned.** A behaviour owns the ordered transformation; modifiers
  insert/reorder stages and add/remove mitigation rows. The library ships `IPipelineStage` +
  `IMitigationStrategy`; the game ships concretes. (§ 9)

## 4. Vocabulary & surfaces — [LOCKED, in code]

`ICanBeModified` (marker) · `IHasBehaviours` → `BehaviourContainer` · `SbBehaviour` (a pure capability
that owns its stats) · `SbModifier` (`Apply` / `Remove`, self-targets, `ISmartPacker`). A modifiable thing implements
`ICanBeModified` + `IHasBehaviours` directly and exposes its `BehaviourContainer` — there is no base class.

## 5. Stats & math — [LOCKED, in code]

PoE order — `(base + Σflat) × (1 + Σincreased) × Π(1 + more)`, first `Override` wins. Fixed-point int
(scale 10000) for determinism across machines / save / wire. Dirty-flagged reads. Stat identity =
GUID-derived hash (`StatDefinition` / `StatRegistry`).

## 6. Routing — [LOCKED]

Within a thing: a modifier lands on the behaviour(s) that own the targeted stat/capability; none →
no-op (law 3). Across things: a satellite reconciles an owner's set (§ 7). Self-targeting: a modifier
reaches the behaviour it modifies and no-ops where absent.

## 7. Roles: Owner / Satellite — [LOCKED]

Roles, not types — declared, not special-cased. An owner holds a broadcast set (`ModifierCache`) plus a
generation stamp; it never pushes (mutating just bumps the generation). A satellite holds a
`ModifierReceiver`, reconciles lazily (generation-gated), and pulls modifiers into its own behaviours,
where they route exactly like local ones.

## 8. Events & reactions — [LOCKED]

The macro surface (`BehaviourContainer.Events`, an `EventContainer`) is the shared, generic,
archetype-agnostic surface modifiers subscribe to — one `skill_activated` hook raised by both a spell
and a melee swing. Behaviours own their specific micro moments (`OnBeginCast`, `OnBeginAttack`) and
raise the corresponding macro event up onto the surface when they fire. The surface is the only
subscription surface for modifiers; behaviours are the only publishers.

Mechanism (all generic — no event name or context type resides in the library):
- `EventContainer` keyed by `uint`, payload generic `<T>`, type-checked at dispatch, `HasHandlers` for
  laziness.
- `EventRegistry` — name→hash, collision check, hash→name for logs. Reuses Core
  `StableHash` / `HashRegistry`. Registers nothing itself.
- `BehaviourContainer.Events` (the surface); `Add` binds the surface into each behaviour.
- `SbBehaviour.Raise<T>(uint evt, T payload)` — guarded by `HasHandlers`, so no work occurs when
  unsubscribed.

`BehaviourContainer.Events` replaced the target-level `IHasEvents` and the two `ITriggers*` interfaces; they
are removed, and events are reached via `Behaviours.Events`.

### 8.1 Events across the wire — [LOCKED]

An event never crosses the wire; only data does. An action is sent to its target as a Core dispatch
(`IPackerDispatch`); the target's authority applies it and reports a **consequence** (an `ISmartPacker`
result) back to the causer. Each side then raises its own events locally — the target from its
replicated state change, the causer from the returned consequence. Which event a consequence raises is
decided on the causer (a damage result means "hit"; a lethal flag means "kill"); that mapping is local
and never transmitted. The library's sole obligation is the local raise; producing a context from a
consequence or a state change is game-side integration.

## 9. Pipelines & mitigation — [PARTIAL]

A behaviour owns an ordered stage pipeline over one flowing context (precedent: a defensive behaviour
owning damage stages plus a mitigation table mapping a defense stat → the damage stats it covers).
Modifiers insert/remove/reorder stages and add/remove mitigation rows — enabling damage-sequence
changes, stat remapping (cold resistance mitigates physical), immunities, and similar. The library owns
`IPipelineStage` + `IMitigationStrategy`; concretes are game-side. *Open: the exact library/game split
of the mitigation-row types.*

## 10. Laziness — [LOCKED]

No work until read or used. Dirty-recompute on next read; generation-gated reconcile on next propagate;
lazy id resolution; lazy instantiation (a satellite built on demand); event `Raise` is a no-op when
unsubscribed.

## 11. Spellbound.Core conformance (save + network) — [Core mapped; requirements pending]

The library must round-trip and replicate **through Core**, never inventing serialization. Core surface
to conform to:
- **Packing:** `Packer` (ref-`Span<byte>`), `IPacker` / `ISmartPacker`, `[PackerId]` +
  `SmartPackerRegistry` (polymorphic type tags), `Packer.WriteSmartList` / `ReadSmartList` — these
  supersede the old `ModifierCodec`.
- **Identity:** `StableHash` (FNV-1a 32), `IRegistryEntry`, `HashRegistry`.
- **Replication / persistence:** per-instance `IPackerObjectData` data slots
  (`Dictionary<InstanceDataKey, byte[]>`), `IPackerDispatch` deltas, `IDispatch<T>` handlers,
  `IObjectDataAccess` / `ObjectParent.TryWriteData` / `TryTransformData`.

*Conformance requirements: pending.*

## 12. The modifier DTO — [OPEN · #4]

Direction: a `ModifierState : IPackerObjectData` carries the persistent modifier list (packed via
`WriteSmartList`); its change/resolve callbacks reapply the set to the runtime behaviours. A paired
`ModifierDelta : IPackerDispatch` + `IDispatch<ModifierDelta>` handler replicates a single add/remove
as a small delta rather than the whole list. This keeps the library networking-agnostic while
save/network-compatible. *Design pending.*

## 13. Deliberately omitted — [LOCKED]

- Trigger systems (cooldowns, input bindings, schedulers) — the game orchestrates.
- Damage formulas / mitigation policies — concretes are game-side `SbBehaviour`s; the library ships
  only the seams.
- Resource pools as a primary type — derived from stats (max is a live stat read).
- Networking, ECS hooks, rendering — none. The library depends only on `Spellbound.Core`.

## 14. Open decisions — [OPEN]

- **Ownership signal — RESOLVED:** a behaviour owns a stat by declaring it in `DeclareOwnedStats()`; the
  runtime lazily seeds those defaults into base values (authored values win), so `HasBase ⊇ Declare` and
  routing respects declarations through the one signal.
- **The modifier DTO** (§ 12).
- **Triggered-modifier authoring** — code-only, or designer-pickable events via a registry-backed
  dropdown.
- **Cross-entity triggers** — for example on-kill, where the killer reacts to the victim's death,
  announced on the source by the interaction layer. To be designed when triggered modifiers land.
- **Mitigation-row library/game split** (§ 9).

## Integration order (after sign-off) — [PROPOSED]

Implementation does not begin until this document is approved. Thereafter, smallest-first:
`EventContainer` → `uint` · `EventRegistry` · `BehaviourContainer.Events` + bind-on-`Add` ·
`SbBehaviour.Board` / `BindBoard` / `Raise`. Each capability is proven in `Samples/` with a sample
event vocabulary.
