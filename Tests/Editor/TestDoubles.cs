// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers.Editor.Tests {
    /// <summary>
    /// Bare modifiable target: an identity tag plus a behaviour container, nothing else.
    /// </summary>
    internal sealed class TestTarget : ICanBeModified, IHasBehaviours {
        public BehaviourContainer Behaviours { get; } = new();
    }

    /// <summary>
    /// Stat-owning behaviour seeded with raw hashes in its constructor — mirrors the game's
    /// ctor-seeding pattern without touching StatRegistry. Distinct from B because the container
    /// keys by concrete type.
    /// </summary>
    internal sealed class StatOwnerBehaviourA : SbBehaviour {
        public StatOwnerBehaviourA(params (uint hash, float baseValue)[] seeds) {
            foreach (var (hash, baseValue) in seeds)
                SetBase(hash, baseValue);
        }
    }

    /// <summary>
    /// Second stat-owning behaviour type; see <see cref="StatOwnerBehaviourA"/>.
    /// </summary>
    internal sealed class StatOwnerBehaviourB : SbBehaviour {
        public StatOwnerBehaviourB(params (uint hash, float baseValue)[] seeds) {
            foreach (var (hash, baseValue) in seeds)
                SetBase(hash, baseValue);
        }
    }

    /// <summary>
    /// Minimal stat-entry-shaped modifier: Apply routes one StatModifierEntry through the container
    /// walk under this instance's UniqueId; Remove sweeps that id. Pack/Unpack are deliberately empty —
    /// nothing in this suite exercises serialization.
    /// </summary>
    internal sealed class TestStatModifier : SbModifier {
        private readonly uint _statHash;
        private readonly ModifierType _type;
        private readonly float _value;

        public TestStatModifier(uint statHash, ModifierType type, float value) {
            _statHash = statHash;
            _type = type;
            _value = value;
        }

        public override void Apply(ICanBeModified target) {
            if (target is IHasBehaviours hasBehaviours)
                hasBehaviours.Behaviours.AddModifier(new StatModifierEntry(_statHash, _type, _value, UniqueId));
        }

        public override void Remove(ICanBeModified target) {
            if (target is IHasBehaviours hasBehaviours)
                hasBehaviours.Behaviours.RemoveModifierByUniqueId(UniqueId);
        }

        public override void Pack(ref Span<byte> buffer) { }

        public override void Unpack(ref ReadOnlySpan<byte> buffer) { }
    }
}
