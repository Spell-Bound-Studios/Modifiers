// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;
using Spellbound.Core.Logging;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample player: a modifiable thing with the same defensive stack a target has — an <see cref="ArmorBehaviour"/>,
    /// a <see cref="PipelineBehaviour"/>, and a <see cref="ResourceBehaviour"/> at 50 health — plus the
    /// <see cref="Fireball"/> it casts. The fireball names the player as the attacker on every hit, so reflected
    /// damage comes back to <see cref="TakeHit"/> and runs through the player's own circuit; life steal heals it.
    /// All of that is built lazily, NOT in Awake — see <see cref="EnsureReady"/>.
    /// </summary>
    public sealed class PlayerController : MonoBehaviour, ICanBeModified, IHasBehaviours {
        [SerializeField] private GameObject projectilePrefab;

        public BehaviourContainer Behaviours { get; } = new();

        private Fireball _fireball;
        private ResourceBehaviour _resource;
        private PipelineBehaviour _pipeline;
        private bool _ready;

        public Fireball Fireball => _fireball ??= BuildFireball();

        public float CurrentHealth => Resource.Current;
        public float MaxHealth => Resource.Max;

        private ResourceBehaviour Resource {
            get {
                EnsureReady();

                return _resource;
            }
        }

        private void Awake() {
            Log.Debug($"[Reflect] Player Awake on #{GetInstanceID()} '{name}' @ {transform.position} — creating bar");
            CreateHealthBar();
        }

        public void CastFireball() => Fireball.OnExecute(transform.position + transform.forward, transform.forward);

        /// <summary>
        /// Take a hit through the player's own circuit — this is where reflected damage lands. No attacker is
        /// passed on, so the player can't reflect it back: one bounce, never a loop.
        /// </summary>
        public void TakeHit(List<StatAndValue> damage) {
            EnsureReady();

            var before = _resource.Current;
            _pipeline.Mitigate(damage, Behaviours);

            Log.Debug($"[Reflect] Player #{GetInstanceID()} '{name}' @ {transform.position} Max={MaxHealth} " +
                    $"TakeHit {damage.Count}: health {before} -> {_resource.Current}");
        }

        /// <summary>
        /// Build the defensive stack on first use, NOT in Awake. The demo's player is wired as a prefab-asset
        /// reference whose Awake never fires, so anything set up there silently never happens — which is exactly
        /// what stopped reflect (a null <see cref="Fireball.Caster"/>) and the Hurt button before it.
        /// </summary>
        private void EnsureReady() {
            if (_ready)
                return;

            _ready = true;
            Behaviours.Add(new ArmorBehaviour());
            _resource = new ResourceBehaviour();
            Behaviours.Add(_resource);
            _resource.SetBase("health", 50f);
            _pipeline = new PipelineBehaviour();
            Behaviours.Add(_pipeline);
        }

        private Fireball BuildFireball() {
            EnsureReady();

            return new Fireball {
                ProjectilePrefab = projectilePrefab,
                Caster = this,
                OnLifeSteal = amount => _resource.Heal(amount)
            };
        }

        private void CreateHealthBar() {
            var bar = new GameObject("HealthBar");
            bar.transform.SetParent(transform);
            bar.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            bar.AddComponent<HealthBar>().Bind(() => CurrentHealth, () => MaxHealth);
        }
    }
}
