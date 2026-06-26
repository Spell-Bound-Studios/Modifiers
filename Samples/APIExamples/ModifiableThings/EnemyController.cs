// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample enemy: a modifiable thing — a <see cref="MonoBehaviour"/> implementing <see cref="ICanBeModified"/>
    /// + <see cref="IHasBehaviours"/>, owning a <see cref="BehaviourContainer"/> of <see cref="ArmorBehaviour"/>,
    /// <see cref="ResistanceBehaviour"/>, <see cref="AbsorptionBehaviour"/>, <see cref="PipelineBehaviour"/>, and
    /// <see cref="ResourceBehaviour"/>. The pipeline runs incoming damage through absorption -> resistances ->
    /// armor -> life. Recolor + ignite DoT + death/respawn make it visible; the health bar + numbers live elsewhere.
    /// </summary>
    public sealed class EnemyController : MonoBehaviour, ICanBeModified, IHasBehaviours {
        [Header("Visual"), SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color ignitedColor = new(1f, 0.4f, 0.1f);
        [SerializeField] private Color deadColor = Color.gray;

        public BehaviourContainer Behaviours { get; } = new();

        private PipelineBehaviour _pipeline;
        private ResourceBehaviour _resource;
        private AbsorptionBehaviour _absorption;
        private Coroutine _igniteRoutine;

        private static uint? _killingBlowHash;
        private static uint KillingBlowHash => _killingBlowHash ??= StatRegistry.GetHash("killing_blow");

        private static uint? _fireHash;
        private static uint FireHash => _fireHash ??= StatRegistry.GetHash("fire_damage");

        public event Action<EnemyController> OnDeath;

        public float CurrentHealth => _resource.Current;
        public float MaxHealth => _resource.Max;
        public bool IsDead => _resource.IsDead;

        private void Awake() {
            Behaviours.Add(new ArmorBehaviour());
            Behaviours.Add(new ResistanceBehaviour());
            Behaviours.Add(new AbsorptionBehaviour());
            Behaviours.Add(new PipelineBehaviour());
            Behaviours.Add(new ResourceBehaviour());

            _pipeline = Behaviours.GetBehaviour<PipelineBehaviour>();
            _resource = Behaviours.GetBehaviour<ResourceBehaviour>();
            _absorption = Behaviours.GetBehaviour<AbsorptionBehaviour>();

            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();

            if (targetRenderer != null)
                targetRenderer.material.color = defaultColor;

            gameObject.tag = "Enemy";

            CreateHealthBar();
        }

        private void CreateHealthBar() {
            var bar = new GameObject("HealthBar");
            bar.transform.SetParent(transform);
            bar.transform.localPosition = new Vector3(0f, 2.2f, 0f);

            var healthBar = bar.AddComponent<HealthBar>();
            healthBar.Bind(() => CurrentHealth, () => MaxHealth);
            healthBar.BindShield(() => _absorption.Current, () => _absorption.Max);
        }

        /// <summary>
        /// Run a typed-damage list through the damage circuit (absorption -> resistances -> armor -> life), pop a
        /// floating number for what reached life, and return the consequence — a <c>killing_blow</c> if lethal.
        /// </summary>
        public List<StatAndValue> TakeHit(List<StatAndValue> damage, PlayerController attacker = null) {
            var consequence = new List<StatAndValue>();

            if (_resource.IsDead)
                return consequence;

            _pipeline.Mitigate(damage, Behaviours, attacker);

            PopNumbers(damage);

            // The consequence carries the actual, post-mitigation damage that reached life — already typed.
            consequence.AddRange(damage);

            if (_resource.IsDead) {
                Die();
                consequence.Add(new StatAndValue(KillingBlowHash, 1f));
            }

            return consequence;
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

        public void ApplyIgnite(float damagePerSecond, float duration) {
            if (IsDead)
                return;

            if (_igniteRoutine != null)
                StopCoroutine(_igniteRoutine);

            if (targetRenderer != null)
                targetRenderer.material.color = ignitedColor;

            _igniteRoutine = StartCoroutine(IgniteRoutine(damagePerSecond, duration));
        }

        private IEnumerator IgniteRoutine(float damagePerSecond, float duration) {
            const float tick = 0.5f;
            var elapsed = 0f;

            while (elapsed < duration && !IsDead) {
                yield return new WaitForSeconds(tick);

                elapsed += tick;
                TakeHit(new List<StatAndValue> { new(FireHash, damagePerSecond * tick) });
            }

            if (!IsDead && targetRenderer != null)
                targetRenderer.material.color = defaultColor;

            _igniteRoutine = null;
        }

        public void Respawn() {
            StopIgnite();
            gameObject.SetActive(true);
            _resource.Reset();
            _absorption.Reset();

            if (targetRenderer != null)
                targetRenderer.material.color = defaultColor;
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

        [ContextMenu("Take Test Hit")]
        private void TakeTestHit() => TakeHit(new List<StatAndValue> {
            new(StatRegistry.GetHash("physical_damage"), 40f),
            new(StatRegistry.GetHash("fire_damage"), 40f),
            new(StatRegistry.GetHash("cold_damage"), 40f),
            new(StatRegistry.GetHash("lightning_damage"), 40f)
        });
    }
}
