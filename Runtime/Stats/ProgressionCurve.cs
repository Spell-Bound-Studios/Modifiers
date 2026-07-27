// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using Spellbound.Core.Logging;
using Spellbound.Core.Registries;
using UnityEngine;

namespace Spellbound.Modifiers {
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Progression Curve")]
    public class ProgressionCurve : HashedScriptableObject {
        [SerializeField, Tooltip("Cost to advance one step. Element 0 is the cost from step 0 to step 1.")]
        private List<float> stepCosts = new();

        private float[] _thresholds;

        public IReadOnlyList<float> StepCosts => stepCosts;

        public int StepCount => stepCosts?.Count ?? 0;

        public void Define(params float[] costs) {
            stepCosts = costs != null ? new List<float>(costs) : new List<float>();
            _thresholds = null;
        }

        public float CostFor(int step) {
            if (step < 0 || step >= StepCount)
                return float.PositiveInfinity;

            return Mathf.Max(0f, stepCosts[step]);
        }

        public float ThresholdFor(int step) {
            if (step <= 0)
                return 0f;

            var thresholds = Thresholds();

            return step <= thresholds.Length ? thresholds[step - 1] : float.PositiveInfinity;
        }

        public int StepFor(float total) {
            var thresholds = Thresholds();
            var low = 0;
            var high = thresholds.Length;

            while (low < high) {
                var mid = (low + high) / 2;

                if (thresholds[mid] <= total)
                    low = mid + 1;
                else
                    high = mid;
            }

            return low;
        }

        private float[] Thresholds() {
            if (_thresholds != null && _thresholds.Length == StepCount)
                return _thresholds;

            var thresholds = new float[StepCount];
            var running = 0f;

            for (var i = 0; i < StepCount; i++) {
                var cost = stepCosts[i];

                if (cost < 0f) {
                    Log.Error($"ProgressionCurve '{name}' has a negative cost at step {i}; treated as 0.");
                    cost = 0f;
                }

                running += cost;
                thresholds[i] = running;
            }

            _thresholds = thresholds;

            return thresholds;
        }

#if UNITY_EDITOR
        protected override void OnValidateAsset() => _thresholds = null;
#endif
    }
}
