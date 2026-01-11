// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    public class SimpleProjectile : MonoBehaviour {
        [SerializeField] private float maxDistance = 20f;
        [SerializeField] private string targetTag = "Enemy";
        
        public GameObject ExcludedTarget { get; set; }
        
        public Vector3 Direction { get; set; }
        public float Speed { get; set; }
        public Action<GameObject, Vector3> Payload { get; set; }
        
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
            
            Payload?.Invoke(other.gameObject, transform.position);
            Destroy(gameObject);
        }
    }
}