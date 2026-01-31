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
        
        public List<GameObject> Fire(PositionalPayload payload) {
            var hits = new List<GameObject>();
            
            var beamRange = Stats.GetValue("beam_range");
            var beamWidth = Stats.GetValue("beam_width");
            
            if (beamWidth <= 0.01f) {
                if (!Physics.Raycast(payload.Position, payload.Direction, out var hit, beamRange, targetMask))
                    return hits;
                if (hit.collider.CompareTag("Enemy"))
                    hits.Add(hit.collider.gameObject);
            } else {
                var hitCount = Physics.SphereCastNonAlloc(payload.Position, beamWidth, payload.Direction, _hitBuffer, beamRange, targetMask);
                for (var i = 0; i < hitCount; i++) {
                    if (_hitBuffer[i].collider.CompareTag("Enemy"))
                        hits.Add(_hitBuffer[i].collider.gameObject);
                }
            }
            
            return hits;
        }
        
        public void StartVisual(Vector3 position, Vector3 direction) {
            if (BeamVisualPrefab == null || _activeBeamVisual != null) 
                return;
            
            _activeBeamVisual = UnityEngine.Object.Instantiate(BeamVisualPrefab, position, Quaternion.LookRotation(direction));
        }
        
        public void UpdateVisual(Vector3 position, Vector3 direction, float length) {
            if (_activeBeamVisual == null) 
                return;
    
            _activeBeamVisual.transform.position = position;
            _activeBeamVisual.transform.rotation = Quaternion.LookRotation(direction);
    
            // Scale Z to match beam length
            var scale = _activeBeamVisual.transform.localScale;
            scale.z = length;
            _activeBeamVisual.transform.localScale = scale;
        }
        
        public void StopVisual() {
            if (_activeBeamVisual == null) 
                return;
            
            UnityEngine.Object.Destroy(_activeBeamVisual);
            _activeBeamVisual = null;
        }
        
        protected override StatContainer InitializeStats() {
            var stats = new StatContainer();
            stats.SetBase("beam_range", range);
            stats.SetBase("beam_width", width);
            return stats;
        }
    }
}