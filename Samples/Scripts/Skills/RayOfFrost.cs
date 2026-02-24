using System;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    [Serializable]
    public sealed class RayOfFrost : ModifiableObject {
        public override string Name => "Ray of Frost";
        
        [SerializeField] public GameObject beamVisualPrefab;
        
        private BeamBehaviour _beam;
        private ColdBehaviour _cold;
        private DurationBehaviour _duration;

        private Vector3 _lastPosition;
        private Vector3 _lastDirection;
        
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
            
            Events.Add<PositionalPayload>("channel", OnChannel);
            Events.Add<TargetedPayload>("hit", OnHit);
        }
        
        public void StartChannel(Vector3 position, Vector3 direction) {
            if (IsChanneling) 
                return;
            
            IsChanneling = true;
            _lastPosition = position;
            _lastDirection = direction;
            
            _beam.StartVisual(position, direction);
            
            // Initial fire to set beam length
            var result = _beam.Fire(new PositionalPayload(this, position, direction));
            _beam.UpdateVisual(position, direction, result.Distance, result.HitPoint, result.DidHit);
        }
        
        public void UpdateChannel(Vector3 position, Vector3 direction) {
            if (!IsChanneling) 
                return;
            
            _lastPosition = position;
            _lastDirection = direction;
            
            Events.Invoke("channel", new PositionalPayload(this, position, direction));
        }
        
        public void StopChannel() {
            if (!IsChanneling) 
                return;
            
            IsChanneling = false;
            _beam.StopVisual();
        }
        
        private void OnChannel(PositionalPayload payload) {
            var result = _beam.Fire(payload);
            
            _beam.UpdateVisual(payload.Position, payload.Direction, result.Distance, result.HitPoint, result.DidHit);
            
            foreach (var hit in result.Hits) {
                Events.Invoke("hit", new TargetedPayload(this, hit, hit.transform.position));
            }
        }
        
        private void OnHit(TargetedPayload payload) {
            var damageResult = _cold.DealColdDamage(payload);
            Events.Invoke("damage", damageResult);
            
            _cold.TryChill(payload, _duration.GetChillDuration());
        }
    }
}