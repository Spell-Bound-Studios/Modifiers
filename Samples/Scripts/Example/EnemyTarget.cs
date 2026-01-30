// Copyright 2025 Spellbound Studio Inc.

using System.Collections;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Simple enemy target that can be hit and ignited.
    /// </summary>
    /// <remarks>
    /// This is purely an example script and scaffolding to highlight the library. Nothing in this script is really
    /// highlighting the libraries capabilities.
    /// </remarks>
    public sealed class EnemyTarget : MonoBehaviour {
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color ignitedColor = Color.red;
        [SerializeField] private Color chilledColor = Color.darkBlue;

        private Coroutine _igniteCoroutine;
        private Coroutine _chillCoroutine;

        public bool IsIgnited { get; private set; }
        public bool IsChilled { get; private set; }

        private void Awake() {
            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();

            if (targetRenderer != null)
                targetRenderer.material.color = defaultColor;

            gameObject.tag = "Enemy";
        }

        public void TakeDamage(float damage, string damageType) =>
                Debug.Log($"[{gameObject.name}] Took {damage} {damageType} damage!");

        public void ApplyIgnite(float duration) {
            if (_igniteCoroutine != null)
                StopCoroutine(_igniteCoroutine);

            _igniteCoroutine = StartCoroutine(IgniteRoutine(duration));
        }

        private IEnumerator IgniteRoutine(float duration) {
            IsIgnited = true;

            if (targetRenderer != null)
                targetRenderer.material.color = ignitedColor;

            Debug.Log($"[{gameObject.name}] IGNITED for {duration}s!");

            yield return new WaitForSeconds(duration);

            IsIgnited = false;

            if (targetRenderer != null)
                targetRenderer.material.color = defaultColor;

            Debug.Log($"[{gameObject.name}] Ignite expired.");
            _igniteCoroutine = null;
        }

        public void ApplyChill(float duration) {
            if (_chillCoroutine != null)
                StopCoroutine(_chillCoroutine);

            _chillCoroutine = StartCoroutine(ChillRoutine(duration));
        }

        private IEnumerator ChillRoutine(float duration) {
            IsChilled = true;
            
            if (targetRenderer != null)
                targetRenderer.material.color = chilledColor;

            Debug.Log($"[{gameObject.name}] CHILLED for {duration}s!");

            yield return new WaitForSeconds(duration);

            IsChilled = false;

            if (targetRenderer != null)
                targetRenderer.material.color = defaultColor;

            Debug.Log($"[{gameObject.name}] Chill expired.");
            _chillCoroutine = null;
        }
    }
}