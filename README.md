# Spellbound.Modifiers

A Unity library for games where everything can be modified. An entity exposes stats; anything in the game — an item, a talent, a buff, the level it spawned in — contributes to those stats under a removable source id, and a single query resolves the truth with Path-of-Exile-grade algebra (flat, increased, more, override). Modifiers are authored once as rollable definitions, rolled into packable facts that survive saving and networking, and applied anywhere: a "Thick Hide" can be an item affix, a racial perk, a five-second buff, or a level-wide mutation without changing a line of code. Entities chain (gear modifies the player, skills read through the caster), timed modifiers expire against a clock you own, and event circuits let content restructure behavior — like damage mitigation — at runtime. All math runs on deterministic fixed-point integers behind a float-facing API, steady-state allocation-free.

## Installation

Requires Unity 6000.0+ and [Spellbound.Core](https://github.com/Spell-Bound-Studios/Core). Add both to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.spellboundstudios.core": "git@github.com:Spell-Bound-Studios/Core.git",
    "com.spellboundstudios.modifiers": "git@github.com:Spell-Bound-Studios/Modifiers.git"
  }
}
```

Or via **Window → Package Manager → + → Add package from git URL** using the same URLs. A playable combat demo lives in `Samples/`, and `CHANGELOG.md` tracks the API.
