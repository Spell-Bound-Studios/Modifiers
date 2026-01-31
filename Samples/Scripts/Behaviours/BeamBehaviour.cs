using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    [Serializable]
    public sealed class BeamBehaviour : SbBehaviour {
        [SerializeField] private float range = 15f;
        [SerializeField] private float width = 0.5f;
        [SerializeField] private LayerMask targetMask = -1;
        
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[32];
        
        public GameObject BeamVisualPrefab { get; set; }
        
        private GameObject _activeBeamVisual;
        private Transform _beamCore;
        private Transform _beamCoreB;
        private Transform _beamGlow;
        private ParticleSystem _impactParticles;
        
        public float GetRange() => Stats.GetValue("beam_range");
        
        public BeamHitResult Fire(PositionalPayload payload) {
            var hits = new List<GameObject>();
            
            var beamRange = Stats.GetValue("beam_range");
            var beamWidth = Stats.GetValue("beam_width");
            
            float hitDistance = beamRange;
            Vector3 hitPoint = payload.Position + payload.Direction * beamRange;
            bool didHit = false;
            
            if (beamWidth <= 0.01f) {
                if (Physics.Raycast(payload.Position, payload.Direction, out var hit, beamRange, targetMask)) {
                    if (hit.collider.CompareTag("Enemy")) {
                        hits.Add(hit.collider.gameObject);
                        hitDistance = hit.distance;
                        hitPoint = hit.point;
                        didHit = true;
                    }
                }
            } else {
                var hitCount = Physics.SphereCastNonAlloc(payload.Position, beamWidth, payload.Direction, _hitBuffer, beamRange, targetMask);
                
                float closestDistance = beamRange;
                
                for (var i = 0; i < hitCount; i++) {
                    if (_hitBuffer[i].collider.CompareTag("Enemy")) {
                        hits.Add(_hitBuffer[i].collider.gameObject);
                        
                        if (_hitBuffer[i].distance < closestDistance) {
                            closestDistance = _hitBuffer[i].distance;
                            hitPoint = _hitBuffer[i].point;
                        }
                        
                        didHit = true;
                    }
                }
                
                if (didHit)
                    hitDistance = closestDistance;
            }
            
            return new BeamHitResult(hits, hitDistance, hitPoint, didHit);
        }
        
        public void StartVisual(Vector3 position, Vector3 direction) {
            if (BeamVisualPrefab == null || _activeBeamVisual != null) 
                return;
            
            _activeBeamVisual = UnityEngine.Object.Instantiate(BeamVisualPrefab, position, Quaternion.LookRotation(direction));
            
            _beamCore = _activeBeamVisual.transform.GetChild(0);
            _beamCoreB = _activeBeamVisual.transform.GetChild(1);
            _beamGlow = _activeBeamVisual.transform.GetChild(2);
            _impactParticles = _activeBeamVisual.transform.GetChild(3).GetComponent<ParticleSystem>();
            
            _impactParticles.Stop();
        }
        
        public void UpdateVisual(Vector3 position, Vector3 direction, float length, Vector3 hitPoint, bool isHitting) {
            if (_activeBeamVisual == null) 
                return;
            
            _activeBeamVisual.transform.position = position;
            _activeBeamVisual.transform.rotation = Quaternion.LookRotation(direction);
            
            SetBeamLength(_beamCore, length);
            SetBeamLength(_beamCoreB, length);
            SetBeamLength(_beamGlow, length);
            
            if (isHitting) {
                _impactParticles.transform.position = hitPoint;
                
                if (!_impactParticles.isPlaying)
                    _impactParticles.Play();
            } else {
                if (_impactParticles.isPlaying)
                    _impactParticles.Stop();
            }
        }
        
        private void SetBeamLength(Transform beam, float length) {
            var scale = beam.localScale;
            scale.y = length;
            beam.localScale = scale;
        }
        
        public void StopVisual() {
            if (_activeBeamVisual == null) 
                return;
            
            UnityEngine.Object.Destroy(_activeBeamVisual);
            _activeBeamVisual = null;
            _beamCore = null;
            _beamCoreB = null;
            _beamGlow = null;
            _impactParticles = null;
        }
        
        protected override StatContainer InitializeStats() {
            var stats = new StatContainer();
            stats.SetBase("beam_range", range);
            stats.SetBase("beam_width", width);
            return stats;
        }
    }
    
    public readonly struct BeamHitResult {
        public readonly List<GameObject> Hits;
        public readonly float Distance;
        public readonly Vector3 HitPoint;
        public readonly bool DidHit;
        
        public BeamHitResult(List<GameObject> hits, float distance, Vector3 hitPoint, bool didHit) {
            Hits = hits;
            Distance = distance;
            HitPoint = hitPoint;
            DidHit = didHit;
        }
    }
}