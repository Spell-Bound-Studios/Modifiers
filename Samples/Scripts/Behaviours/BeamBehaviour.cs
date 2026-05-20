// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample behaviour: emits a beam — sphere-cast or thin raycast based on the <c>beam_width</c> stat,
    /// with range from <c>beam_range</c>. Knows HOW to fire and HOW to drive its visual; doesn't know when to
    /// fire, who can fire it, or what to do with the hits — those are the orchestrating skill's job.
    /// </summary>
    /// <remarks>
    /// Demonstrates the "behaviour owns its stats" pattern: range and width live on the behaviour's own
    /// <see cref="StatContainer"/>, so modifiers can boost them without the skill knowing the math.
    /// </remarks>
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

        public BeamBehaviour() {
            this.SetBase("beam_range", range);
            this.SetBase("beam_width", width);
        }

        public float GetRange() => GetValue("beam_range");
        public float GetWidth() => GetValue("beam_width");

        public BeamHitResult Fire(PositionalPayload payload) {
            var hits = new List<GameObject>();

            var beamRange = GetValue("beam_range");
            var beamWidth = GetValue("beam_width");

            var hitDistance = beamRange;
            var hitPoint = payload.Position + payload.Direction * beamRange;
            var didHit = false;

            if (beamWidth <= 0.01f) {
                if (Physics.Raycast(payload.Position, payload.Direction, out var hit, beamRange, targetMask)) {
                    if (hit.collider.CompareTag("Enemy")) {
                        hits.Add(hit.collider.gameObject);
                        hitDistance = hit.distance;
                        hitPoint = hit.point;
                        didHit = true;
                    }
                }
            }
            else {
                var hitCount = Physics.SphereCastNonAlloc(payload.Position, beamWidth, payload.Direction, _hitBuffer,
                    beamRange, targetMask);

                var closestDistance = beamRange;

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

            _activeBeamVisual =
                    UnityEngine.Object.Instantiate(BeamVisualPrefab, position, Quaternion.LookRotation(direction));

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
            }
            else {
                if (_impactParticles.isPlaying)
                    _impactParticles.Stop();
            }
        }

        private void SetBeamLength(Transform beam, float length) {
            var scale = beam.localScale;
            scale.y = length;
            beam.localScale = scale;

            beam.localPosition = new Vector3(0, 0, length * 0.5f);
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
        

    /// <summary>
    /// Return value of <see cref="BeamBehaviour.Fire"/>: the hit list, the effective distance the beam
    /// traveled, the hit point for visual anchoring, and whether anything was actually struck.
    /// </summary>
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