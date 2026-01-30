using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    [Serializable]
    public sealed class BeamBehaviour : SbBehaviour {
        [SerializeField] private float range = 20f;
        [SerializeField] private float width = 0.5f;
        [SerializeField] private LayerMask targetMask = -1;
        
        public GameObject BeamVisualPrefab { get; set; }
        
        public List<GameObject> Fire(PositionalPayload payload) {
            var hits = new List<GameObject>();
            
            var beamRange = Stats.GetValue("beam_range");
            var beamWidth = Stats.GetValue("beam_width");
            
            if (beamWidth <= 0.01f) {
                if (Physics.Raycast(payload.Position, payload.Direction, out var hit, beamRange, targetMask))
                    hits.Add(hit.collider.gameObject);
            } else {
                var results = new RaycastHit[] { };
                var size = Physics.SphereCastNonAlloc(
                    payload.Position, 
                    beamWidth, 
                    payload.Direction, 
                    results,
                    beamRange, 
                    targetMask);
                
                foreach (var hit in results)
                    hits.Add(hit.collider.gameObject);
            }
            
            if (BeamVisualPrefab != null) {
                var visual = UnityEngine.Object.Instantiate(BeamVisualPrefab, payload.Position, Quaternion.LookRotation(payload.Direction));
            }
            
            return hits;
        }
        
        protected override StatContainer InitializeStats() {
            var stats = new StatContainer();
            stats.SetBase("beam_range", range);
            stats.SetBase("beam_width", width);
            return stats;
        }
    }
}