// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample skill: a modifiable thing composing a <see cref="ProjectileBehaviour"/>, <see cref="FireBehaviour"/>,
    /// an <see cref="AwardBehaviour"/>, a <see cref="CastBehaviour"/>, and a <see cref="LifeStealBehaviour"/>.
    /// <see cref="OnExecute"/> spawns the projectiles; each strike deals fire damage through the enemy's pipeline,
    /// ignites it if enabled, banks a killing blow when empowerment is on, splits if enabled, and — reading the
    /// life damage the defender returns — heals the caster via <see cref="OnLifeSteal"/> when life steal is on.
    /// An empowered cast flies green instead of orange.
    /// </summary>
    public sealed class Fireball : ICanBeModified, IHasBehaviours {
        private static readonly Color FireColor = new(1f, 0.45f, 0.1f);
        private static readonly Color EmpoweredColor = new(0.3f, 1f, 0.4f);

        public BehaviourContainer Behaviours { get; } = new();

        public string Name => "Fireball";

        public GameObject ProjectilePrefab { get; set; }

        /// <summary>Invoked with the heal a life-stealing hit earned, so the caster can top up its own pool.</summary>
        public Action<float> OnLifeSteal { get; set; }

        /// <summary>The caster, passed to each target as the attacker so a reflect stage can fire damage back at it.</summary>
        public PlayerController Caster { get; set; }

        private ProjectileBehaviour _projectile;
        private FireBehaviour _fire;
        private AwardBehaviour _award;
        private CastBehaviour _cast;
        private LifeStealBehaviour _lifeSteal;

        public Fireball() {
            Behaviours.Add(new ProjectileBehaviour());
            Behaviours.Add(new FireBehaviour());
            Behaviours.Add(new AwardBehaviour());
            Behaviours.Add(new CastBehaviour());
            Behaviours.Add(new LifeStealBehaviour());

            _projectile = Behaviours.GetBehaviour<ProjectileBehaviour>();
            _fire = Behaviours.GetBehaviour<FireBehaviour>();
            _award = Behaviours.GetBehaviour<AwardBehaviour>();
            _cast = Behaviours.GetBehaviour<CastBehaviour>();
            _lifeSteal = Behaviours.GetBehaviour<LifeStealBehaviour>();
        }

        /// <summary>The cast goes off: spawn the projectiles along <paramref name="forward"/> from <paramref name="origin"/>.</summary>
        public void OnExecute(Vector3 origin, Vector3 forward) {
            if (ProjectilePrefab == null)
                return;

            var empowered = _award.TrySpendEmpowerment();
            var damage = _fire.BuildDamage(empowered);
            var color = empowered ? EmpoweredColor : FireColor;
            var directions = _projectile.WorldDirections(forward, _projectile.Count);

            foreach (var direction in directions)
                SpawnProjectile(origin, direction, damage, color, canSplit: true, excluded: null);
        }

        private void SpawnProjectile(
                Vector3 origin, Vector3 direction, List<StatAndValue> damage, Color color, bool canSplit, GameObject excluded) {
            var obj = Object.Instantiate(ProjectilePrefab, origin, Quaternion.identity);
            var projectile = obj.GetComponent<FireballProjectile>();

            if (projectile == null) {
                Object.Destroy(obj);

                return;
            }

            projectile.Direction = direction;
            projectile.Speed = _projectile.Speed;
            projectile.CanSplit = canSplit;
            projectile.Excluded = excluded;
            projectile.SetColor(color);
            projectile.OnHit = (enemy, hitProjectile) => Hit(enemy, hitProjectile, damage);
        }

        private void Hit(EnemyController enemy, FireballProjectile projectile, List<StatAndValue> damage) {
            if (enemy == null || enemy.IsDead)
                return;

            var consequence = enemy.TakeHit(Clone(damage), Caster);
            _award.Receive(consequence);

            var heal = _lifeSteal.ComputeHeal(consequence);

            if (heal > 0f)
                OnLifeSteal?.Invoke(heal);

            if (_fire.IgniteEnabled)
                enemy.ApplyIgnite(_fire.FireDamage * 0.3f, 3f);

            if (projectile.CanSplit && _projectile.SplitOnHit) {
                foreach (var direction in SplitDirections(projectile.Direction, 3, 30f))
                    SpawnProjectile(projectile.transform.position, direction, damage, projectile.Tint,
                            canSplit: false, excluded: enemy.gameObject);
            }
        }

        private static Vector3[] SplitDirections(Vector3 baseDirection, int count, float angleBetween) {
            var directions = new Vector3[count];
            var start = -angleBetween * (count - 1) / 2f;

            for (var i = 0; i < count; i++)
                directions[i] = Quaternion.AngleAxis(start + i * angleBetween, Vector3.up) * baseDirection;

            return directions;
        }

        private static List<StatAndValue> Clone(List<StatAndValue> damage) {
            var copy = new List<StatAndValue>(damage.Count);

            foreach (var entry in damage)
                copy.Add(entry);

            return copy;
        }
    }
}
