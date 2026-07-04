// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Modifiers {
    public sealed class CircuitContext {
        public List<StatAndValue> Packet;
        public List<StatAndValue> Consequence;
        public Modifiable Subject;
        public Modifiable Owner;

        public void Note(uint id, float amount) {
            Consequence ??= new List<StatAndValue>();
            Consequence.Add(new StatAndValue(id, amount));
        }

        public void Clear() {
            Packet = null;
            Consequence?.Clear();
            Subject = null;
            Owner = null;
        }
    }
}
