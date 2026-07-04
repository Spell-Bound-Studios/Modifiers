// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using Spellbound.Core.Hashing;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample player: a MonoBehaviour composing a <see cref="Modifiable"/>. Base stats are set in Awake, the
    /// take-hit circuit is built once with all four stages pre-defined (an empty stage costs nothing, and
    /// pre-defining them gives later modifiers somewhere to land), and the sapphire ring shows the equip /
    /// unequip shape: contributions under one instance-derived source id, stripped with RemoveSource.
    /// </summary>
    public sealed class PlayerController : MonoBehaviour {
        [SerializeField] private GameObject projectilePrefab;

        private readonly Modifiable _modifiable = new();
        private float _currentHealth;
        private uint _ringSourceId;
        private Fireball _fireball;
        private readonly CircuitContext _takeHitContext = new();

        public Modifiable Modifiable => _modifiable;
        public Fireball Fireball => _fireball ??= BuildFireball();
        public float MaxHealth => _modifiable.GetValue(DemoStats.Health);
        public float CurrentHealth => _currentHealth;

        private void Awake() {
            var stats = _modifiable.Stats;
            stats.SetBase(DemoStats.Health, 50f);
            stats.SetBase(DemoStats.Armor, 10f);
            stats.SetBase(DemoStats.FireResistance, 20f);
            stats.SetBase(DemoStats.ColdResistance, 20f);
            stats.SetBase(DemoStats.LightningResistance, 20f);

            DemoCircuits.BuildTakeHit(_modifiable, Damage);

            _currentHealth = MaxHealth;
            stats.Changed += OnStatChanged;

            CreateHealthBar();
        }

        public void CastFireball() => Fireball.OnExecute(transform.position + transform.forward, transform.forward);

        public void TakeHit(List<StatAndValue> damage) {
            _takeHitContext.Clear();
            _takeHitContext.Packet = damage;
            _modifiable.Run(DemoEvents.TakeHit, _takeHitContext);
        }

        public void Damage(float amount) => _currentHealth = Mathf.Max(0f, _currentHealth - amount);

        public void Heal(float amount) => _currentHealth = Mathf.Min(MaxHealth, _currentHealth + amount);

        public void EquipSapphireRing() {
            if (_ringSourceId != 0)
                return;

            _ringSourceId = StableHash.Fnv1A32($"sapphire_ring_{GetInstanceID()}");
            _modifiable.Stats.AddModifier(DemoStats.ColdResistance, ModifierType.Flat, 30f, _ringSourceId);
            _modifiable.Stats.AddModifier(DemoStats.Health, ModifierType.Increased, 0.2f, _ringSourceId);
        }

        public void UnequipSapphireRing() {
            if (_ringSourceId == 0)
                return;

            _modifiable.RemoveSource(_ringSourceId);
            _ringSourceId = 0;
        }

        private void OnStatChanged(StatId stat) {
            if (stat == DemoStats.Health)
                _currentHealth = Mathf.Min(_currentHealth, MaxHealth);
        }

        private Fireball BuildFireball() =>
                new() {
                    Parent = _modifiable,
                    ProjectilePrefab = projectilePrefab,
                    Caster = this,
                    OnLifeSteal = Heal
                };

        private void CreateHealthBar() {
            var bar = new GameObject("HealthBar");
            bar.transform.SetParent(transform);
            bar.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            bar.AddComponent<HealthBar>().Bind(() => CurrentHealth, () => MaxHealth);
        }
    }
}
