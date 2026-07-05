// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample projectile: the physical representation of a cast fireball. Travels in a straight line, despawns
    /// after <c>maxDistance</c>, and on trigger-enter with an enemy invokes <see cref="OnHit"/>. Skips its
    /// <see cref="Excluded"/> object so a split child never re-hits the enemy it spawned from. Needs a trigger
    /// Collider and a kinematic Rigidbody to register hits.
    /// </summary>
    public sealed class FireballProjectile : MonoBehaviour {
        [SerializeField] private float maxDistance = 30f;
        [SerializeField] private string targetTag = "Enemy";

        public Vector3 Direction { get; set; }
        public float Speed { get; set; }
        public bool CanSplit { get; set; } = true;
        public GameObject Excluded { get; set; }
        public Color Tint { get; private set; } = Color.white;
        public Action<EnemyController, FireballProjectile> OnHit { get; set; }

        private float _traveled;
        private Renderer _renderer;

        public void SetColor(Color color) {
            Tint = color;

            if (_renderer == null)
                _renderer = GetComponentInChildren<Renderer>();

            if (_renderer != null)
                _renderer.material.color = color;
        }

        private void Update() {
            var step = Direction * (Speed * Time.deltaTime);
            transform.position += step;
            _traveled += step.magnitude;

            if (Direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(Direction);

            if (_traveled >= maxDistance)
                Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other) {
            if (!other.CompareTag(targetTag) || other.gameObject == Excluded)
                return;

            var enemy = other.GetComponent<EnemyController>();

            if (enemy == null)
                return;

            OnHit?.Invoke(enemy, this);
            Destroy(gameObject);
        }
    }
}