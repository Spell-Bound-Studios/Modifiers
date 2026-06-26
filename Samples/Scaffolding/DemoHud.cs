// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample HUD: a UI Toolkit control panel that equips fireball + enemy modifiers and drives the scene.
    /// Health bars and combat numbers are world-space, anchored to the entities — not part of this panel.
    /// Needs a <see cref="UIDocument"/> with a Panel Settings asset; resolves its <see cref="CombatDemo"/> itself.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class DemoHud : MonoBehaviour {
        [SerializeField] private CombatDemo demo;

        private VisualElement _root;
        private VisualElement _leftPanel;
        private VisualElement _enemyPanel;
        private Label _status;

        private IncreasedFireDamageModifier _fireDamageMod;
        private AddedProjectileCountModifier _projectilesMod;
        private CircularPatternModifier _circularMod;
        private IgniteModifier _igniteMod;
        private SplitOnHitModifier _splitMod;
        private EmpowerOnKillModifier _empowerMod;
        private LifeStealModifier _lifeStealMod;
        private FireResistanceModifier _fireResistMod;
        private ColdResistanceModifier _coldResistMod;
        private LightningResistanceModifier _lightningResistMod;
        private ArmorModifier _armorMod;
        private ReflectFireModifier _reflectFireMod;

        private void OnEnable() {
            if (demo == null)
                demo = GetComponent<CombatDemo>();

            if (demo == null)
                demo = FindAnyObjectByType<CombatDemo>();

            if (demo == null) {
                Debug.LogError($"[DemoHud] No {nameof(CombatDemo)} on this object or in the scene; disabling.");
                enabled = false;

                return;
            }

            _root = GetComponent<UIDocument>().rootVisualElement;
            BuildPanel();
            BuildEnemyPanel();
        }

        private void LateUpdate() => _status.text = $"Enemies alive: {demo.AliveCount}";

        #region Panel

        private void BuildPanel() {
            _leftPanel?.RemoveFromHierarchy();

            var panel = new VisualElement();
            _leftPanel = panel;
            panel.style.position = Position.Absolute;
            panel.style.left = 12;
            panel.style.top = 12;
            panel.style.width = 240;
            panel.style.paddingLeft = 12;
            panel.style.paddingRight = 12;
            panel.style.paddingTop = 10;
            panel.style.paddingBottom = 12;
            panel.style.backgroundColor = new Color(0.06f, 0.06f, 0.09f, 0.92f);
            Round(panel);
            _root.Add(panel);

            _status = new Label("Enemies alive: -");
            _status.style.color = Color.white;
            _status.style.unityFontStyleAndWeight = FontStyle.Bold;
            _status.style.marginBottom = 8;
            panel.Add(_status);

            panel.Add(Section("COMBAT"));
            panel.Add(ActionButton("Cast Fireball", () => demo.Cast()));

            panel.Add(Section("FIREBALL MODIFIERS"));
            panel.Add(ToggleButton("+100% Fire Damage", ToggleFireDamage));
            panel.Add(ToggleButton("+2 Projectiles", ToggleProjectiles));
            panel.Add(ToggleButton("Circular Nova", ToggleCircular));
            panel.Add(ToggleButton("Ignite", ToggleIgnite));
            panel.Add(ToggleButton("Split On Hit", ToggleSplit));
            panel.Add(ToggleButton("Empower On Kill", ToggleEmpower));
            panel.Add(ToggleButton("Life Steal", ToggleLifeSteal));

            panel.Add(Section("SCENE"));
            panel.Add(LabeledSlider("Inner Ring", 1, 20, demo.InnerCount, v => demo.SetInnerCount((int)v)));
            panel.Add(LabeledSlider("Outer Ring", 1, 30, demo.OuterCount, v => demo.SetOuterCount((int)v)));
            panel.Add(LabeledSlider("Radius Jitter", 0, 3, 0, demo.SetRadiusJitter));
            panel.Add(LabeledToggle("Enemies Move", demo.EnemiesMove, v => demo.EnemiesMove = v));
            panel.Add(ActionButton("Respawn All", () => demo.RespawnAll()));
            panel.Add(LabeledToggle("Auto Respawn", demo.AutoRespawn, v => demo.AutoRespawn = v));
        }

        private void BuildEnemyPanel() {
            _enemyPanel?.RemoveFromHierarchy();

            var panel = new VisualElement();
            _enemyPanel = panel;
            panel.style.position = Position.Absolute;
            panel.style.right = 12;
            panel.style.top = 230;
            panel.style.width = 220;
            panel.style.paddingLeft = 12;
            panel.style.paddingRight = 12;
            panel.style.paddingTop = 10;
            panel.style.paddingBottom = 12;
            panel.style.backgroundColor = new Color(0.06f, 0.06f, 0.09f, 0.92f);
            Round(panel);
            _root.Add(panel);

            var title = new Label("ENEMY");
            title.style.color = Color.white;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 8;
            panel.Add(title);

            panel.Add(Section("MODIFIERS"));
            panel.Add(ToggleButton("+40 Fire Resistance", ToggleFireResist));
            panel.Add(ToggleButton("+40 Cold Resistance", ToggleColdResist));
            panel.Add(ToggleButton("+40 Lightning Resistance", ToggleLightningResist));
            panel.Add(ToggleButton("+20 Armor", ToggleArmor));
            panel.Add(ToggleButton("Reflect Fire (25%)", ToggleReflectFire));
        }

        private static Label Section(string text) {
            var label = new Label(text);
            label.style.color = new Color(0.55f, 0.6f, 0.75f);
            label.style.fontSize = 10;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginTop = 10;
            label.style.marginBottom = 4;

            return label;
        }

        private static Button ActionButton(string text, Action onClick) {
            var button = new Button(onClick) { text = text };
            StyleButton(button);
            button.style.backgroundColor = new Color(0.2f, 0.3f, 0.5f);

            return button;
        }

        private Button ToggleButton(string text, Func<bool> onToggle) {
            Button button = null;
            button = new Button(() => Recolor(button, onToggle())) { text = text };
            StyleButton(button);
            Recolor(button, false);

            return button;
        }

        private static void StyleButton(Button button) {
            button.style.height = 26;
            button.style.marginTop = 0;
            button.style.marginBottom = 4;
            button.style.marginLeft = 0;
            button.style.marginRight = 0;
            button.style.color = Color.white;
            Round(button);
        }

        private static void Recolor(Button button, bool on) =>
                button.style.backgroundColor = on ? new Color(0.2f, 0.55f, 0.25f) : new Color(0.22f, 0.22f, 0.27f);

        private static Slider LabeledSlider(string label, float min, float max, float value, Action<float> onChange) {
            var slider = new Slider(label, min, max) { value = value };
            slider.style.color = Color.white;
            slider.style.marginBottom = 4;
            slider.RegisterValueChangedCallback(e => onChange(e.newValue));

            return slider;
        }

        private static Toggle LabeledToggle(string label, bool value, Action<bool> onChange) {
            var toggle = new Toggle(label) { value = value };
            toggle.style.color = Color.white;
            toggle.style.marginBottom = 4;
            toggle.RegisterValueChangedCallback(e => onChange(e.newValue));

            return toggle;
        }

        private static void Round(VisualElement element) {
            element.style.borderTopLeftRadius = 5;
            element.style.borderTopRightRadius = 5;
            element.style.borderBottomLeftRadius = 5;
            element.style.borderBottomRightRadius = 5;
        }

        #endregion

        #region Modifier toggles

        private bool ToggleFireDamage() => Toggle(ref _fireDamageMod, demo.Player.Fireball);
        private bool ToggleProjectiles() => Toggle(ref _projectilesMod, demo.Player.Fireball);
        private bool ToggleCircular() => Toggle(ref _circularMod, demo.Player.Fireball);
        private bool ToggleIgnite() => Toggle(ref _igniteMod, demo.Player.Fireball);
        private bool ToggleSplit() => Toggle(ref _splitMod, demo.Player.Fireball);
        private bool ToggleEmpower() => Toggle(ref _empowerMod, demo.Player.Fireball);
        private bool ToggleLifeSteal() => Toggle(ref _lifeStealMod, demo.Player.Fireball);

        private static bool Toggle<T>(ref T modifier, ICanBeModified target) where T : SbModifier, new() {
            if (modifier == null) {
                modifier = new T();
                modifier.Apply(target);
            }
            else {
                modifier.Remove(target);
                modifier = default;
            }

            return modifier != null;
        }

        private bool ToggleFireResist() => ToggleEnemyMod(ref _fireResistMod);
        private bool ToggleColdResist() => ToggleEnemyMod(ref _coldResistMod);
        private bool ToggleLightningResist() => ToggleEnemyMod(ref _lightningResistMod);
        private bool ToggleArmor() => ToggleEnemyMod(ref _armorMod);
        private bool ToggleReflectFire() => ToggleEnemyMod(ref _reflectFireMod);

        private bool ToggleEnemyMod<T>(ref T modifier) where T : SbModifier, new() {
            if (modifier == null) {
                modifier = new T();

                foreach (var enemy in demo.Enemies)
                    modifier.Apply(enemy);
            }
            else {
                foreach (var enemy in demo.Enemies)
                    modifier.Remove(enemy);

                modifier = default;
            }

            return modifier != null;
        }

        #endregion
    }
}
