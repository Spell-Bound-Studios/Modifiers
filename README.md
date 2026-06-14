# Spellbound.Modifiers

A composable, modifier-driven stats system for Unity games. PoE-style semantics — stats are computed at read time from a
base value plus an ordered chain of modifiers, and modifiers can be added or removed at any time, on any target, by any
source.

**Pre-1.0.** No backward-compatibility contract yet. When clarity demands renaming or restructuring, it happens.

---

## Philosophy

**A behaviour is a pure capability.** It knows HOW to do one thing — fire a projectile, hold a resource pool, receive
damage. It owns the stats that govern THAT thing. It does not know when it runs, what triggers it, or what comes next.

**A skill is a composition, not an orchestrator.** Just a `ModifiableObject` that owns behaviours and wires them
together in `Initialize()`. No magic, no orchestration layer hiding inside.

**The game orchestrates.** Triggers, cooldowns, scheduling, networking, save/load, talent trees, loot tables — all
game-side. The library cannot anticipate every game's trigger model and refuses to ship one.

**Modifiers reach anything.** Characters, items, props, projectiles, terrain — anything implementing `ICanBeModified`.
If a talent / gear / buff might plausibly want to alter a behaviour, that behaviour lives on the target as a modifiable
thing, not as a hardcoded service.

---

## Mental model

| Role               | What it is                                                                                                                                                       | What it doesn't know                                                                                         |
|--------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------|
| `SbBehaviour`      | A pure capability. Owns its own stats for the thing it does. PoE math lives here.                                                                                | When it runs, what triggers it, what's next.                                                                 |
| `SbModifier`       | Mutates a target via `Apply(target)` / `Remove(target)`. Carries a `UniqueId` so removal targets the exact applied instance.                                     | Anything about the target's other modifiers; reaches in through `IHasBehaviours` / `IHasEvents` and adjusts. |
| `ModifiableObject` | A composed target. Owns a `BehaviourContainer` + `EventContainer`. A "skill" in this lib is just a `ModifiableObject` whose `Initialize()` wires its behaviours. | What gameplay system owns or triggers it.                                                                    |
| The game           | Wires triggers, cooldowns, schedules, networking, save/load, scenes. Composes behaviours into skills, talents into trees, drops into loot tables.                | Library internals — that's why those internals exist.                                                        |

Power users implement the contracts (`IModifier`, `IHasUniqueId`, `IHasBehaviours`, `IHasEvents`) directly. `SbModifier`
and `ModifiableObject` are the 80% path, not the only path.

---

## Math

PoE-order, deterministic, fixed-point. For any stat:

```
final = (base + Σflat) × (1 + Σincreased) × Π(1 + more)
```

…unless any `Override` modifier is present, in which case the **first** `Override` wins and ignores everything else — a CI-style "Maximum Life becomes 1" is not displaced by overrides applied after it.

All values stored as scaled `int` (default scale = 10000 → four decimal places). Determinism matters for network sync,
replay, and save-load round-trips. Reads are dirty-flagged — `GetValue` only recalculates when modifiers actually
changed.

---

## Authoring shapes

Designers compose modifiers from two primitives:

- **`Affix`** — anonymous stat-flavor data. "+9 armor", "+25% increased life". Inline via `[SerializeReference]` on
  items / talents / pools. No identity, no asset, no display name; tooltips format directly from the referenced stat.
  The lib ships `Affix` as an abstract base; the consumer ships a concrete subclass that implements `Apply` / `Remove`
  with their preferred routing (which `SbBehaviour` receives the modifier).
- **`Trait`** + **`TraitRef`** — named, registered identities the player recognizes. "Iron Will", "Thick Hide", "Fire
  Resistant". A `Trait` is a `ScriptableObject` (display data + embedded `SbModifier` effect); a `TraitRef` is the
  inline `SbModifier` wrapper that lets a `[SerializeReference] List<SbModifier>` hold a reference to one. Tiered
  variants (`iron_will_t1`, `t2`, `t3`) are separate assets that share a C# class but tune its parameters.

The split exists because one-SO-per-affix bloats unmanageably at scale (~thousands of anonymous stat tweaks × multiple
item slots). Inline data for the anonymous mass; assets only for the named identities a player will actually see.

Drop generation rides **`ModifierPool`** assets — weighted slot lists that sample fresh `Affix` / `TraitRef` instances
at drop time and emit them through `ModifierCodec` for inventory / save / wire encoding.

---

## What this library deliberately omits

- **Trigger systems.** No cooldown manager, no input bindings, no scheduler. Game owns this.
- **Damage formulas / mitigation policies.** Damage math is `SbBehaviour` subclasses in the consumer, not baked-in
  services. A hardcoded "DamageCalculator" would lock out conversions, redirections, and per-type custom rules — the
  calculator is the composition of behaviours on the target.
- **Resource pools as a primary type.** Pools are derived from stats; their `Max` is a live read from a referenced stat
  so modifier-driven changes to the max flow through automatically.
- **Networking, ECS hooks, rendering.** None. The library depends only on `Spellbound.Core`. Anything that ties to a
  specific runtime stack (PurrNet, Unity Entities, URP) belongs in the consumer.

---

## Install

Unity package manifest:

```json
"com.spellboundstudios.modifiers": "<git url>"
```

Depends only on `com.spellboundstudios.core`.

---

## Status

Pre-1.0. **Architectural strength beats API stability** until 1.0. Breaking changes happen when the architecture demands
them; see `CHANGELOG.md`.
