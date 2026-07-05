// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Modifiers.Samples {
    public sealed class Fireball : Modifiable {
        private static readonly Color FireColor = new(1f, 0.45f, 0.1f);
        private static readonly Color EmpoweredColor = new(0.3f, 1f, 0.4f);

        public GameObject ProjectilePrefab { get; set; }
        public PlayerController Caster { get; set; }
        public Action<float> OnLifeSteal { get; set; }

        private readonly CircuitContext _hitContext = new();
        private readonly List<StatAndValue> _hitPacket = new(4);
        private readonly List<StatAndValue> _reflectPacket = new(1);

        public Fireball() {
            Stats.SetBase(DemoStats.ProjectileCount, 1f);
            Stats.SetBase(DemoStats.ProjectileSpeed, 14f);
            Stats.SetBase(DemoStats.FireDamage, 30f);
            Stats.SetBase(DemoStats.KillingBlow, 0f);

            Stats.Changed += OnOwnStatChanged;
        }

        public int Count => Mathf.Max(1, (int)GetValue(DemoStats.ProjectileCount));
        public float Speed => GetValue(DemoStats.ProjectileSpeed);
        public float FireDamage => GetValue(DemoStats.FireDamage);
        public float Banked => GetValue(DemoStats.KillingBlow);

        public void OnExecute(Vector3 origin, Vector3 forward) {
            if (ProjectilePrefab == null)
                return;

            var empowered = TrySpendEmpowerment();
            var damage = BuildDamage(empowered);
            var color = empowered ? EmpoweredColor : FireColor;

            foreach (var direction in WorldDirections(forward, Count))
                SpawnProjectile(origin, direction, damage, color, canSplit: true, excluded: null);
        }

        private void OnOwnStatChanged(StatId stat) {
            if (stat == DemoStats.EmpowerOnKill && GetValue(DemoStats.EmpowerOnKill) <= 0f)
                Stats.SetBase(DemoStats.KillingBlow, 0f);
        }

        private List<StatAndValue> BuildDamage(bool empowered) {
            var multiplier = empowered ? 2f : 1f;
            var damage = new List<StatAndValue> { new(DemoStats.FireDamage, FireDamage * multiplier) };
            var chaos = GetValue(DemoStats.ChaosDamage);

            if (chaos > 0f)
                damage.Add(new StatAndValue(DemoStats.ChaosDamage, chaos * multiplier));

            return damage;
        }

        private bool TrySpendEmpowerment() {
            if (GetValue(DemoStats.EmpowerOnKill) <= 0f || Banked < 1f)
                return false;

            Stats.SetBase(DemoStats.KillingBlow, Banked - 1f);

            return true;
        }

        private void Hit(EnemyController enemy, FireballProjectile projectile, List<StatAndValue> damage) {
            if (enemy == null || enemy.IsDead)
                return;

            _hitContext.Clear();
            _hitPacket.Clear();
            _hitPacket.AddRange(damage);
            _hitContext.Packet = _hitPacket;

            enemy.TakeHit(_hitContext);

            Receive(_hitContext.Consequence);

            var heal = ComputeHeal(_hitContext.Packet);

            if (heal > 0f)
                OnLifeSteal?.Invoke(heal);

            TryIgnite(enemy);
            ReturnReflected(_hitContext.Consequence);

            if (projectile.CanSplit && GetValue(DemoStats.SplitOnHit) > 0f) {
                foreach (var direction in SplitDirections(projectile.Direction, 3, 30f))
                    SpawnProjectile(projectile.transform.position, direction, damage, projectile.Tint,
                            canSplit: false, excluded: enemy.gameObject);
            }
        }

        private void Receive(List<StatAndValue> consequence) {
            if (consequence == null || GetValue(DemoStats.EmpowerOnKill) <= 0f)
                return;

            foreach (var entry in consequence) {
                if (entry.statHash == DemoConsequences.KillingBlow)
                    Stats.SetBase(DemoStats.KillingBlow, Banked + entry.amount);
            }
        }

        private float ComputeHeal(List<StatAndValue> packet) {
            var fraction = GetValue(DemoStats.LifeSteal);

            if (fraction <= 0f || packet == null)
                return 0f;

            var total = 0f;

            for (var i = 0; i < packet.Count; i++)
                total += packet[i].amount;

            return total * fraction;
        }

        private void TryIgnite(EnemyController enemy) {
            var chance = GetValue(DemoStats.IgniteChance);

            if (chance <= 0f || UnityEngine.Random.value > chance)
                return;

            enemy.ApplyIgnite(FireDamage * 0.3f, GetValue(DemoStats.IgniteDuration));
        }

        private void ReturnReflected(List<StatAndValue> consequence) {
            if (consequence == null || Caster == null)
                return;

            var reflected = 0f;

            foreach (var entry in consequence) {
                if (entry.statHash == DemoConsequences.ReflectedFire)
                    reflected += entry.amount;
            }

            if (reflected <= 0f)
                return;

            _reflectPacket.Clear();
            _reflectPacket.Add(new StatAndValue(DemoStats.FireDamage, reflected));
            Caster.TakeHit(_reflectPacket);
        }

        private void SpawnProjectile(
                Vector3 origin, Vector3 direction, List<StatAndValue> damage, Color color, bool canSplit,
                GameObject excluded) {
            var obj = Object.Instantiate(ProjectilePrefab, origin, Quaternion.identity);
            var projectile = obj.GetComponent<FireballProjectile>();

            if (projectile == null) {
                Object.Destroy(obj);

                return;
            }

            projectile.Direction = direction;
            projectile.Speed = Speed;
            projectile.CanSplit = canSplit;
            projectile.Excluded = excluded;
            projectile.SetColor(color);
            projectile.OnHit = (enemy, hitProjectile) => Hit(enemy, hitProjectile, damage);
        }

        private Vector3[] WorldDirections(Vector3 forward, int count) {
            var pattern = (int)GetValue(DemoStats.ProjectilePattern);
            var local = pattern == 1 ? Circle(count) : ForwardSpread(count);
            var rotation = Quaternion.LookRotation(forward == Vector3.zero ? Vector3.forward : forward);
            var world = new Vector3[local.Length];

            for (var i = 0; i < local.Length; i++)
                world[i] = rotation * local[i];

            return world;
        }

        private static Vector3[] Circle(int count) {
            var directions = new Vector3[count];

            for (var i = 0; i < count; i++)
                directions[i] = Quaternion.AngleAxis(360f / count * i, Vector3.up) * Vector3.forward;

            return directions;
        }

        private static Vector3[] ForwardSpread(int count) {
            var directions = new Vector3[count];

            if (count == 1) {
                directions[0] = Vector3.forward;

                return directions;
            }

            const float spread = 15f;
            var start = -spread * (count - 1) / 2f;

            for (var i = 0; i < count; i++)
                directions[i] = Quaternion.AngleAxis(start + i * spread, Vector3.up) * Vector3.forward;

            return directions;
        }

        private static Vector3[] SplitDirections(Vector3 baseDirection, int count, float angleBetween) {
            var directions = new Vector3[count];
            var start = -angleBetween * (count - 1) / 2f;

            for (var i = 0; i < count; i++)
                directions[i] = Quaternion.AngleAxis(start + i * angleBetween, Vector3.up) * baseDirection;

            return directions;
        }
    }
}
