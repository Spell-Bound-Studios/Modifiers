// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample enemy: the standard take-hit pipeline plus a shield pool granted into Mitigate ahead of the
    /// resists, so absorption eats first. TakeHit mitigates the caller's packet in place and reports outcomes
    /// — the killing blow, reflected amounts — on the context's consequence list. Recolor, ignite DoT, and
    /// death/respawn make it visible; the health bar and floating numbers live elsewhere.
    /// </summary>
    public sealed class EnemyController : MonoBehaviour {
        [Header("Visual"), SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color ignitedColor = new(1f, 0.4f, 0.1f);
        [SerializeField] private Color deadColor = Color.gray;

        private Coroutine _igniteRoutine;
        private readonly CircuitContext _igniteContext = new();
        private readonly List<StatAndValue> _ignitePacket = new(1);

        public event Action<EnemyController> OnDeath;

        public Modifiable Modifiable { get; } = new();

        public float MaxHealth => Modifiable.GetValue(DemoStats.Health);
        public float CurrentHealth { get; private set; }

        public float MaxShield => Modifiable.GetValue(DemoStats.Shield);
        public float CurrentShield { get; private set; }

        public bool IsDead => CurrentHealth <= 0f;

        private void Awake() {
            var stats = Modifiable.Stats;
            stats.SetBase(DemoStats.Health, 100f);
            stats.SetBase(DemoStats.Shield, 50f);
            stats.SetBase(DemoStats.Armor, 10f);
            stats.SetBase(DemoStats.FireResistance, 20f);
            stats.SetBase(DemoStats.ColdResistance, 20f);
            stats.SetBase(DemoStats.LightningResistance, 20f);
            stats.SetBase(DemoStats.ChaosBypassesShield, 100f);

            var circuit = DemoCircuits.BuildTakeHit(Modifiable, Damage);
            circuit.TryGetStage(DemoStages.Mitigate, out var mitigate);
            mitigate.Add(new AbsorptionLeaf(Absorb, DemoStats.ChaosDamage, DemoStats.ChaosBypassesShield),
                    DemoCircuits.ShieldPriority);

            CurrentHealth = MaxHealth;
            CurrentShield = MaxShield;
            stats.Changed += OnStatChanged;

            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();

            if (targetRenderer != null)
                targetRenderer.material.color = defaultColor;

            gameObject.tag = "Enemy";

            CreateHealthBar();
        }

        /// <summary>
        /// Run a caller-owned context through the take-hit circuit. The packet is mitigated in place; outcomes
        /// (the killing blow, reflected amounts) are reported on the context's consequence list.
        /// </summary>
        public void TakeHit(CircuitContext ctx) {
            if (IsDead) {
                ctx.Packet?.Clear();

                return;
            }

            Modifiable.Run(DemoEvents.TakeHit, ctx);

            PopNumbers(ctx.Packet);

            if (IsDead) {
                Die();
                ctx.Note(DemoConsequences.KillingBlow, 1f);
            }
        }

        public void Damage(float amount) => CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);

        public float Absorb(float amount) {
            var taken = Mathf.Min(CurrentShield, amount);
            CurrentShield -= taken;

            return taken;
        }

        public void ApplyIgnite(float damagePerSecond, float duration) {
            if (IsDead)
                return;

            if (_igniteRoutine != null)
                StopCoroutine(_igniteRoutine);

            if (targetRenderer != null)
                targetRenderer.material.color = ignitedColor;

            _igniteRoutine = StartCoroutine(IgniteRoutine(damagePerSecond, duration));
        }

        public void Respawn() {
            StopIgnite();
            gameObject.SetActive(true);
            CurrentHealth = MaxHealth;
            CurrentShield = MaxShield;

            if (targetRenderer != null)
                targetRenderer.material.color = defaultColor;
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
            if (_igniteRoutine == null)
                return;

            StopCoroutine(_igniteRoutine);
            _igniteRoutine = null;
        }

        private void OnStatChanged(StatId stat) {
            if (stat == DemoStats.Health)
                CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
            else if (stat == DemoStats.Shield)
                CurrentShield = Mathf.Min(CurrentShield, MaxShield);
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
