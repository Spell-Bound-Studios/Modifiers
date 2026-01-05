// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Simple projectile that travels and triggers hits.
    /// </summary>
    public class SimpleProjectile : MonoBehaviour {
        private const float MaxDistance = 20f;
        private Vector3 _direction;
        private float _distanceTraveled;
        private Action<GameObject, Vector3> _onHit;
        private float _speed;

        private void Update() {
            var movement = _direction * (_speed * Time.deltaTime);
            transform.position += movement;
            _distanceTraveled += movement.magnitude;

            if (_distanceTraveled >= MaxDistance)
                Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other) {
            // Don't compare tags in your game - I'm just using this as a simple example. :)
            if (!other.CompareTag("Enemy"))
                return;
            
            _onHit?.Invoke(other.gameObject, transform.position);
            Destroy(gameObject);
        }

        public void Initialize(Vector3 direction, float speed, Action<GameObject, Vector3> onHit) {
            _direction = direction.normalized;
            _speed = speed;
            _onHit = onHit;
            _distanceTraveled = 0f;

            // Orient projectile in travel direction
            if (_direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(_direction);
        }
    }
}