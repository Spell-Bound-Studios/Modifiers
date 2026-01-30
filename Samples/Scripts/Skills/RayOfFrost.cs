using System;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    [Serializable]
    public sealed class RayOfFrost : ModifiableObject {
        public override string Name => "Ray of Frost";
        
        [SerializeField] public GameObject beamVisualPrefab;
        
        private BeamBehaviour _beam;
        private ColdBehaviour _cold;
        private DurationBehaviour _duration;
        
        public RayOfFrost() {
            Behaviours.Add(new BeamBehaviour());
            Behaviours.Add(new ColdBehaviour());
            Behaviours.Add(new DurationBehaviour());
        }
        
        public override void Initialize() {
            _beam = Behaviours.GetBehaviour<BeamBehaviour>();
            _cold = Behaviours.GetBehaviour<ColdBehaviour>();
            _duration = Behaviours.GetBehaviour<DurationBehaviour>();
            
            _beam.BeamVisualPrefab = beamVisualPrefab;
            
            Events.Add<PositionalPayload>("cast", OnCast);
            Events.Add<TargetedPayload>("hit", OnHit);
        }
        
        public void Cast(Vector3 position, Vector3 direction) =>
            Events.Invoke("cast", new PositionalPayload(this, position, direction));
        
        private void OnCast(PositionalPayload payload) {
            var hits = _beam.Fire(payload);
            
            foreach (var hit in hits)
                Events.Invoke("hit", new TargetedPayload(this, hit, hit.transform.position));
        }
        
        private void OnHit(TargetedPayload payload) {
            var damageResult = _cold.DealColdDamage(payload);
            Events.Invoke("damage", damageResult);
            
            _cold.TryChill(payload, _duration.GetChillDuration());
        }
    }
}