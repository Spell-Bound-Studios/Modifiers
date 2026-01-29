// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// A simple projectile example script.
    /// </summary>
    /// <remarks>
    /// This script is intended to show the user how to implement a specific ITrigger and then invoke it with the
    /// appropriate payload struct.
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