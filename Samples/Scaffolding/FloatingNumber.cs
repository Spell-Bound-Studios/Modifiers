// Copyright 2026 Spellbound Studio Inc.

using TMPro;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// World-space combat number: billboards to the camera, rises, fades, and destroys itself. Spawned by
    /// <see cref="CombatText"/> at the hit position.
    /// </summary>
    public sealed class FloatingNumber : MonoBehaviour {
        private const float Life = 1.2f;
        private const float RiseSpeed = 1.6f;

        private TextMeshPro _text;
        private Camera _camera;
        private Color _color;
        private float _elapsed;

        public void Init(float amount, Color color) {
            _color = color;
            _camera = Camera.main;

            _text = gameObject.AddComponent<TextMeshPro>();
            _text.text = Mathf.RoundToInt(amount).ToString();
            _text.fontSize = 5;
            _text.color = color;
            _text.alignment = TextAlignmentOptions.Center;
            _text.GetComponent<RectTransform>().sizeDelta = new Vector2(4f, 2f);
        }

        private void Update() {
            _elapsed += Time.deltaTime;

            if (_elapsed >= Life) {
                Destroy(gameObject);

                return;
            }

            transform.position += Vector3.up * (RiseSpeed * Time.deltaTime);

            if (_camera != null)
                transform.rotation = _camera.transform.rotation;

            _color.a = 1f - _elapsed / Life;
            _text.color = _color;
        }
    }
}
