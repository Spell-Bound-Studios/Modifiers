// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample projectile: travels in a straight line at a set speed, dies after <c>maxDistance</c>, and on
    /// trigger-enter with a tagged target raises <see cref="ITriggersTargetedEvent.OnTargetHit"/> with a
    /// <see cref="TargetedPayload"/>. Whoever spawned it (typically a <c>ProjectileBehaviour</c>) wires the
    /// handler that turns the hit into damage / status / split / whatever.
    /// </summary>
    /// <remarks>
    /// Sample-only. A production projectile would use pooling, layer-mask filters instead of tag string
    /// compares, deterministic motion (no per-frame floats), and probably no per-instance allocation.
    /// </remarks>
    public sealed class SimpleProjectile : MonoBehaviour, ITriggersTargetedEvent {
        [SerializeField] private float maxDistance = 20f;
        [SerializeField] private string targetTag = "Enemy";

        public GameObject ExcludedTarget { get; set; }

        public Vector3 Direction { get; set; }
        public float Speed { get; set; }
        public Action<TargetedPayload> OnTargetHit { get; set; }

        private float _distanceTraveled;

        private void Update() {
            var movement = Direction * (Speed * Time.deltaTime);
            transform.position += movement;
            _distanceTraveled += movement.magnitude;

            if (_distanceTraveled >= maxDistance)
                Destroy(gameObject);

            if (Direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(Direction);
        }

        private void OnTriggerEnter(Collider other) {
            if (!other.CompareTag(targetTag))
                return;

            if (other.gameObject == ExcludedTarget)
                return;

            OnTargetHit?.Invoke(new TargetedPayload(null, other.gameObject, transform.position, this));
            Destroy(gameObject);
        }
    }
}