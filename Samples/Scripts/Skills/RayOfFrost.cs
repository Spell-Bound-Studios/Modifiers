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

        public bool IsChanneling { get; private set; }

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
            
            Events.Add<PositionalPayload>("cast", OnChannel);
            Events.Add<TargetedPayload>("hit", OnHit);
        }
        
        public void StartChannel(Vector3 position, Vector3 direction) {
            if (IsChanneling) 
                return;
            
            IsChanneling = true;
            _beam.StartVisual(position, direction);
        }
        
        public void UpdateChannel(Vector3 position, Vector3 direction) {
            if (!IsChanneling) 
                return;
            // TODO: Temporary beam length.
            _beam.UpdateVisual(position, direction, 5);
            Events.Invoke("channel", new PositionalPayload(this, position, direction));
        }
        
        public void StopChannel() {
            if (!IsChanneling) 
                return;
            
            IsChanneling = false;
            _beam.StopVisual();
        }
        
        private void OnChannel(PositionalPayload payload) {
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