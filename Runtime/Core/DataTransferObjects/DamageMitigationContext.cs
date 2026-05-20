// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// Slice-based context for the damage-mitigation pipeline. The <see cref="State"/> slice holds the
    /// target's current defensive stats (armor, resistances, anything the strategies need to read).
    /// The <see cref="Delta"/> slice holds the damage entries about to be applied — stages mutate this
    /// in place to enact mitigation. After the pipeline runs, the caller (<c>ApplyDelta</c>) sums the
    /// remaining delta and applies it to life.
    /// </summary>
    public readonly struct DamageMitigationContext {
        public readonly StatSlice State;
        public readonly StatSlice Delta;

        public DamageMitigationContext(StatSlice state, StatSlice delta) {
            State = state;
            Delta = delta;
        }
    }
}