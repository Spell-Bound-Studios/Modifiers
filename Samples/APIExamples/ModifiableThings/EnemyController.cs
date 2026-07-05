// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    public sealed class EnemyController : MonoBehaviour {
        [Header("Visual"), SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color ignitedColor = new(1f, 0.4f, 0.1f);
        [SerializeField] private Color deadColor = Color.gray;

        private Coroutine _igniteRoutine;
        private readonly CircuitContext _igniteContext = new();
        private readonly List<StatAndValue> _ignitePacket = new(1);

        private readonly List<RolledModifier> _rolled = new();
        private ModifierPool _pool;
        private System.Random _rng;
        private LevelController _level;
        private float _lastDamageTime;

        public event Action<EnemyController> OnDeath;

        public Modifiable Modifiable { get; } = new();

        public float MaxHealth => Modifiable.GetValue(DemoStats.Health);
        public float CurrentHealth { get; private set; }

        public float MaxShield => Modifiable.GetValue(DemoStats.Shield);
        public float CurrentShield { get; private set; }

        public float MaxMana => Modifiable.GetValue(DemoStats.Mana);
        public float CurrentMana { get; private set; }

        private static ModifierDefinition _hardened;
        private static ModifierDefinition Hardened => _hardened ??= ModifierRegistry.GetDefinition("sample_hardened");

        private static ModifierDefinition _ignited;
        private static ModifierDefinition Ignited => _ignited ??= ModifierRegistry.GetDefinition("sample_ignited");

        private string _ownBuffIcons = "";
        private string _debuffIcons = "";
        private string _combinedBuffIcons = "";
        private string _combinedLevelPart;

        public TimedModifierSet Buffs { get; private set; }
        public TimedModifierSet Debuffs { get; private set; }

        public bool IsDead => CurrentHealth <= 0f;
        public bool IsIgnited { get; private set; }
        public string ModifierIcons { get; private set; } = "";

        private void Awake() {
            var stats = Modifiable.Stats;
            stats.SetBase(DemoStats.Health, 100f);
            stats.SetBase(DemoStats.Shield, 50f);
            stats.SetBase(DemoStats.Mana, 30f);
            stats.SetBase(DemoStats.ShieldRegen, 5f);
            stats.SetBase(DemoStats.ShieldRegenDelay, 4f);
            stats.SetBase(DemoStats.Armor, 10f);
            stats.SetBase(DemoStats.FireResistance, 20f);
            stats.SetBase(DemoStats.ColdResistance, 20f);
            stats.SetBase(DemoStats.LightningResistance, 20f);
            stats.SetBase(DemoStats.ChaosBypassesShield, 100f);

            var circuit = DemoCircuits.BuildTakeHit(Modifiable, Damage);
            circuit.TryGetStage(DemoStages.Mitigate, out var mitigate);
            mitigate.Add(new AbsorptionLeaf(Absorb, DemoStats.ChaosDamage, DemoStats.ChaosBypassesShield),
                    DemoCircuits.ShieldPriority);
            circuit.TryGetStage(DemoStages.React, out var react);
            react.Add(new KillingBlowLeaf(() => IsDead));

            Buffs = new TimedModifierSet(Modifiable);
            Debuffs = new TimedModifierSet(Modifiable);
            Buffs.Changed += OnBuffsChanged;
            Debuffs.Changed += OnDebuffsChanged;

            CurrentHealth = MaxHealth;
            CurrentShield = MaxShield;
            CurrentMana = MaxMana;
            _lastDamageTime = Time.time;
            stats.Changed += OnStatChanged;

            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();

            if (targetRenderer != null)
                targetRenderer.material.color = defaultColor;

            gameObject.tag = "Enemy";

            CreateHealthBar();
        }

        private void Update() {
            Buffs.Tick(Time.deltaTime);
            Debuffs.Tick(Time.deltaTime);

            if (IsDead || CurrentShield >= MaxShield)
                return;

            if (Time.time - _lastDamageTime < Modifiable.GetValue(DemoStats.ShieldRegenDelay))
                return;

            CurrentShield = Mathf.Min(MaxShield,
                    CurrentShield + Modifiable.GetValue(DemoStats.ShieldRegen) * Time.deltaTime);
        }

        public void Configure(ModifierPool pool, System.Random rng, LevelController level) {
            _pool = pool;
            _rng = rng;
            _level = level;
            Modifiable.Parent = level != null ? level.Modifiable : null;

            RollModifiers();

            CurrentHealth = MaxHealth;
            CurrentShield = MaxShield;
            CurrentMana = MaxMana;
        }

        public void TakeHit(CircuitContext ctx) {
            if (IsDead) {
                ctx.Packet?.Clear();

                return;
            }

            _lastDamageTime = Time.time;
            Modifiable.Run(DemoEvents.TakeHit, ctx);

            PopNumbers(ctx.Packet);

            if (IsDead)
                Die();
        }

        public void Damage(float amount) => CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);

        public float Absorb(float amount) {
            var taken = Mathf.Min(CurrentShield, amount);
            CurrentShield -= taken;

            if (taken > 0f && CurrentShield <= 0f)
                ApplyHardened();

            return taken;
        }

        private void ApplyHardened() {
            if (_rng == null || Hardened == null)
                return;

            Buffs.Apply(Hardened.Roll(_rng, (uint)_rng.Next(1, int.MaxValue)), 5f);
        }

        public void ApplyIgnite(float damagePerSecond, float duration) {
            if (IsDead)
                return;

            if (_igniteRoutine != null)
                StopCoroutine(_igniteRoutine);

            IsIgnited = true;

            if (Ignited != null && _rng != null)
                Debuffs.Apply(Ignited.Roll(_rng, (uint)_rng.Next(1, int.MaxValue)), duration);

            if (targetRenderer != null)
                targetRenderer.material.color = ignitedColor;

            _igniteRoutine = StartCoroutine(IgniteRoutine(damagePerSecond, duration));
        }

        public void Respawn() {
            StopIgnite();
            Buffs.Clear();
            Debuffs.Clear();
            gameObject.SetActive(true);
            RollModifiers();
            CurrentHealth = MaxHealth;
            CurrentShield = MaxShield;
            CurrentMana = MaxMana;
            _lastDamageTime = Time.time;

            if (targetRenderer != null)
                targetRenderer.material.color = defaultColor;
        }

        private void RollModifiers() {
            foreach (var modifier in _rolled)
                modifier.RemoveFrom(Modifiable);

            _rolled.Clear();

            if (_pool != null && _rng != null) {
                var starRoll = _rng.Next(100);
                var stars = starRoll < 50 ? 0 : starRoll < 85 ? 1 : 2;
                _rolled.AddRange(_pool.Roll(stars, _rng));

                foreach (var modifier in _rolled)
                    modifier.TryApplyTo(Modifiable);
            }

            ModifierIcons = CombatColors.ModifierIcons(_rolled);
        }

        private IEnumerator IgniteRoutine(float damagePerSecond, float duration) {
            const float tick = 0.5f;
            var elapsed = 0f;

            while (elapsed < duration && !IsDead) {
                yield return new WaitForSeconds(tick);

                elapsed += tick;

                _igniteContext.Clear();
                _ignitePacket.Clear();
                _ignitePacket.Add(new StatAndValue(DemoStats.FireDamage, damagePerSecond * tick));
                _igniteContext.Packet = _ignitePacket;
                TakeHit(_igniteContext);
            }

            IsIgnited = false;

            if (!IsDead && targetRenderer != null)
                targetRenderer.material.color = defaultColor;

            _igniteRoutine = null;
        }

        private void Die() {
            StopIgnite();

            if (targetRenderer != null)
                targetRenderer.material.color = deadColor;

            OnDeath?.Invoke(this);
            gameObject.SetActive(false);
        }

        private void StopIgnite() {
            IsIgnited = false;

            if (Ignited != null)
                Debuffs.Dispel(Ignited.Hash);

            if (_igniteRoutine == null)
                return;

            StopCoroutine(_igniteRoutine);
            _igniteRoutine = null;
        }

        private void OnBuffsChanged() {
            _ownBuffIcons = CombatColors.ModifierIcons(Buffs.Active);
            _combinedLevelPart = null;
        }

        private void OnDebuffsChanged() => _debuffIcons = CombatColors.ModifierIcons(Debuffs.Active);

        private string BuffIcons() {
            var level = _level != null ? _level.RolledIcons : "";

            if (!ReferenceEquals(level, _combinedLevelPart)) {
                _combinedLevelPart = level;
                _combinedBuffIcons = level + _ownBuffIcons;
            }

            return _combinedBuffIcons;
        }

        private void OnStatChanged(StatId stat) {
            if (stat == DemoStats.Health)
                CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
            else if (stat == DemoStats.Shield)
                CurrentShield = Mathf.Min(CurrentShield, MaxShield);
            else if (stat == DemoStats.Mana)
                CurrentMana = Mathf.Min(CurrentMana, MaxMana);
        }

        private void PopNumbers(List<StatAndValue> damage) {
            var offset = 0f;

            foreach (var entry in damage) {
                if (entry.amount <= 0.5f)
                    continue;

                var position = transform.position + Vector3.up * 2.6f + Vector3.right * offset;
                CombatText.Pop(position, entry.amount, CombatColors.ForDamage(entry.statHash));
                offset += 0.45f;
            }
        }

        private void CreateHealthBar() {
            var bar = new GameObject("HealthBar");
            bar.transform.SetParent(transform);
            bar.transform.localPosition = new Vector3(0f, 2.2f, 0f);

            var healthBar = bar.AddComponent<HealthBar>();
            healthBar.Bind(() => CurrentHealth, () => MaxHealth);
            healthBar.BindShield(() => CurrentShield, () => MaxShield);
            healthBar.BindMana(() => CurrentMana, () => MaxMana);
            healthBar.BindStatus(
                () => ModifierIcons,
                BuffIcons,
                () => _debuffIcons);
        }

        [ContextMenu("Take Test Hit")]
        private void TakeTestHit() => TakeHit(new CircuitContext {
            Packet = new List<StatAndValue> {
                new(DemoStats.PhysicalDamage, 40f),
                new(DemoStats.FireDamage, 40f),
                new(DemoStats.ColdDamage, 40f),
                new(DemoStats.LightningDamage, 40f),
                new(DemoStats.ChaosDamage, 40f)
            }
        });
    }
}
