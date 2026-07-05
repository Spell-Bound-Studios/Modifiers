// Copyright 2026 Spellbound Studio Inc.

using System;
using TMPro;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    public sealed class HealthBar : MonoBehaviour {
        private const float Width = 1.4f;
        private const float Height = 0.18f;
        private const float ThinHeight = 0.1f;
        private const float ShieldY = 0.14f;
        private const float ManaY = -0.14f;

        private Func<float> _current;
        private Func<float> _max;
        private Func<float> _shieldCurrent;
        private Func<float> _shieldMax;
        private Func<float> _manaCurrent;
        private Func<float> _manaMax;
        private Func<string> _modifiers;
        private Func<string> _buffs;
        private Func<string> _debuffs;

        private SpriteRenderer _fill;
        private SpriteRenderer _shieldBackground;
        private SpriteRenderer _shieldFill;
        private SpriteRenderer _manaFill;
        private TextMeshPro _label;
        private TextMeshPro _modifierLabel;
        private TextMeshPro _buffLabel;
        private TextMeshPro _debuffLabel;
        private Camera _camera;

        public void Bind(Func<float> current, Func<float> max) {
            _current = current;
            _max = max;
            _camera = Camera.main;
            Build();
        }

        public void BindShield(Func<float> current, Func<float> max) {
            _shieldCurrent = current;
            _shieldMax = max;

            var sprite = Square();

            var background = Child("ShieldBackground", new Vector3(0f, ShieldY, 0f),
                new Vector3(Width, ThinHeight, 1f));
            _shieldBackground = background.AddComponent<SpriteRenderer>();
            _shieldBackground.sprite = sprite;
            _shieldBackground.color = new Color(0f, 0f, 0f, 0.7f);
            _shieldBackground.sortingOrder = 103;

            var fill = Child("ShieldFill", new Vector3(-Width / 2f, ShieldY, -0.01f),
                new Vector3(Width, ThinHeight * 0.8f, 1f));
            _shieldFill = fill.AddComponent<SpriteRenderer>();
            _shieldFill.sprite = sprite;
            _shieldFill.color = CombatColors.Absorb;
            _shieldFill.sortingOrder = 104;
        }

        public void BindMana(Func<float> current, Func<float> max) {
            _manaCurrent = current;
            _manaMax = max;

            var sprite = Square();

            var background = Child("ManaBackground", new Vector3(0f, ManaY, 0f), new Vector3(Width, ThinHeight, 1f));
            var backgroundRenderer = background.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = sprite;
            backgroundRenderer.color = new Color(0f, 0f, 0f, 0.7f);
            backgroundRenderer.sortingOrder = 103;

            var fill = Child("ManaFill", new Vector3(-Width / 2f, ManaY, -0.01f),
                new Vector3(Width, ThinHeight * 0.8f, 1f));
            _manaFill = fill.AddComponent<SpriteRenderer>();
            _manaFill.sprite = sprite;
            _manaFill.color = new Color(0.25f, 0.5f, 1f);
            _manaFill.sortingOrder = 104;
        }

        public void BindStatus(Func<string> modifiers, Func<string> buffs, Func<string> debuffs) {
            _modifiers = modifiers;
            _buffs = buffs;
            _debuffs = debuffs;

            _buffLabel = MakeStatusLabel("Buffs", new Vector3(-Width / 2f + 2f, 0.52f, 0f),
                TextAlignmentOptions.Left);

            _modifierLabel = MakeStatusLabel("Modifiers", new Vector3(Width / 2f - 2f, 0.52f, 0f),
                TextAlignmentOptions.Right);

            _debuffLabel = MakeStatusLabel("Debuffs", new Vector3(-Width / 2f + 2f, -0.34f, 0f),
                TextAlignmentOptions.Left);
        }

        private TextMeshPro MakeStatusLabel(string childName, Vector3 position, TextAlignmentOptions alignment) {
            var obj = Child(childName, position, Vector3.one);
            var label = obj.AddComponent<TextMeshPro>();
            label.fontSize = 2.2f;
            label.alignment = alignment;
            label.color = Color.white;
            label.sortingOrder = 105;
            label.GetComponent<RectTransform>().sizeDelta = new Vector2(4f, 0.4f);

            return label;
        }

        private void Build() {
            var sprite = Square();

            var background = Child("Background", Vector3.zero, new Vector3(Width, Height, 1f));
            var backgroundRenderer = background.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = sprite;
            backgroundRenderer.color = new Color(0f, 0f, 0f, 0.7f);
            backgroundRenderer.sortingOrder = 100;

            var fill = Child("Fill", new Vector3(-Width / 2f, 0f, -0.01f), new Vector3(Width, Height * 0.8f, 1f));
            _fill = fill.AddComponent<SpriteRenderer>();
            _fill.sprite = sprite;
            _fill.color = Color.green;
            _fill.sortingOrder = 101;

            var labelObject = Child("Label", new Vector3(0f, 0.32f, 0f), Vector3.one);
            _label = labelObject.AddComponent<TextMeshPro>();
            _label.fontSize = 2.5f;
            _label.alignment = TextAlignmentOptions.Center;
            _label.color = Color.white;
            _label.sortingOrder = 102;
            _label.GetComponent<RectTransform>().sizeDelta = new Vector2(4f, 1.5f);
        }

        private void LateUpdate() {
            if (_camera == null)
                _camera = Camera.main;

            if (_camera != null)
                transform.rotation = _camera.transform.rotation;

            if (_current == null || _max == null)
                return;

            var max = _max();
            var percent = max > 0f ? Mathf.Clamp01(_current() / max) : 0f;

            Resize(_fill, percent);

            _fill.color = percent > 0.5f
                    ? Color.Lerp(Color.yellow, Color.green, (percent - 0.5f) * 2f)
                    : Color.Lerp(Color.red, Color.yellow, percent * 2f);

            _label.text = $"{_current():F0} / {max:F0}";

            UpdateShield();
            UpdateMana();
            UpdateStatus();
        }

        private void UpdateShield() {
            if (_shieldFill == null)
                return;

            var current = _shieldCurrent();
            var shielded = current > 0f;

            _shieldBackground.enabled = shielded;
            _shieldFill.enabled = shielded;

            if (!shielded)
                return;

            var max = _shieldMax();
            Resize(_shieldFill, max > 0f ? Mathf.Clamp01(current / max) : 0f);
        }

        private void UpdateMana() {
            if (_manaFill == null)
                return;

            var max = _manaMax();
            Resize(_manaFill, max > 0f ? Mathf.Clamp01(_manaCurrent() / max) : 0f);
        }

        private void UpdateStatus() {
            if (_modifierLabel == null)
                return;

            _modifierLabel.text = _modifiers?.Invoke() ?? "";
            _buffLabel.text = _buffs?.Invoke() ?? "";
            _debuffLabel.text = _debuffs?.Invoke() ?? "";
        }

        private static void Resize(SpriteRenderer fill, float percent) {
            var scale = fill.transform.localScale;
            scale.x = Width * percent;
            fill.transform.localScale = scale;

            var position = fill.transform.localPosition;
            position.x = -Width / 2f + Width * percent / 2f;
            fill.transform.localPosition = position;
        }

        private GameObject Child(string childName, Vector3 localPosition, Vector3 localScale) {
            var obj = new GameObject(childName);
            obj.transform.SetParent(transform, false);
            obj.transform.localPosition = localPosition;
            obj.transform.localScale = localScale;

            return obj;
        }

        private static Sprite Square() {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}