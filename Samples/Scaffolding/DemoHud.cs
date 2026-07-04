// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample HUD: a UI Toolkit control panel that equips fireball + enemy items and drives the scene.
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

        private ModifiableItem _flameHelmet;
        private ModifiableItem _fireDamage;
        private ModifiableItem _chaosDamage;
        private ModifiableItem _projectiles;
        private ModifiableItem _ignite;
        private ModifiableItem _chaosNoBypass;
        private ModifiableItem _chaosHalfBypass;
        private ModifiableItem _fireResist;
        private ModifiableItem _coldResist;
        private ModifiableItem _lightningResist;
        private ModifiableItem _armor;
        private ModifiableItem _reflectFire;
        private FireballItem _circular;
        private FireballItem _split;
        private FireballItem _empower;
        private FireballItem _lifeSteal;

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

            panel.Add(Section("PLAYER EQUIPMENT"));
            panel.Add(ToggleButton("Flame Helmet (+5 Armor/Fire)", ToggleFlameHelmet));

            panel.Add(Section("FIREBALL MODIFIERS"));
            panel.Add(ToggleButton("+100% More Fire Damage", ToggleFireDamage));
            panel.Add(ToggleButton("+20 Chaos Damage", ToggleChaosDamage));
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
            panel.Add(ToggleButton("Chaos Cannot Bypass Shield", ToggleChaosNoBypass));
            panel.Add(ToggleButton("-50% Chaos Shield Bypass", ToggleChaosHalfBypass));
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

        #endregion Panel

        #region Item toggles

        private bool ToggleFlameHelmet() =>
                ToggleOnPlayer(ref _flameHelmet, () => new StatItem(
                    (DemoStats.Armor, ModifierType.Flat, 5f),
                    (DemoStats.FireDamage, ModifierType.Flat, 5f)));

        private bool ToggleFireDamage() =>
                ToggleOnFireball(ref _fireDamage, () => new StatItem((DemoStats.FireDamage, ModifierType.More, 1f)));

        private bool ToggleChaosDamage() =>
                ToggleOnFireball(ref _chaosDamage, () => new StatItem((DemoStats.ChaosDamage, ModifierType.Flat, 20f)));

        private bool ToggleProjectiles() =>
                ToggleOnFireball(ref _projectiles, () => new StatItem((DemoStats.ProjectileCount, ModifierType.Flat, 2f)));

        private bool ToggleIgnite() =>
                ToggleOnFireball(ref _ignite, () => new StatItem(
                    (DemoStats.IgniteChance, ModifierType.Flat, 1f),
                    (DemoStats.IgniteDuration, ModifierType.Flat, 3f)));

        private bool ToggleCircular() => ToggleCapability(ref _circular, () => new CircularNovaItem());

        private bool ToggleSplit() => ToggleCapability(ref _split, () => new SplitOnHitItem());

        private bool ToggleEmpower() => ToggleCapability(ref _empower, () => new EmpowerOnKillItem());

        private bool ToggleLifeSteal() => ToggleCapability(ref _lifeSteal, () => new LifeStealItem());

        private bool ToggleFireResist() =>
                ToggleOnEnemies(ref _fireResist, () => new StatItem((DemoStats.FireResistance, ModifierType.Flat, 40f)));

        private bool ToggleColdResist() =>
                ToggleOnEnemies(ref _coldResist, () => new StatItem((DemoStats.ColdResistance, ModifierType.Flat, 40f)));

        private bool ToggleLightningResist() =>
                ToggleOnEnemies(ref _lightningResist, () => new StatItem((DemoStats.LightningResistance, ModifierType.Flat, 40f)));

        private bool ToggleArmor() =>
                ToggleOnEnemies(ref _armor, () => new StatItem((DemoStats.Armor, ModifierType.Flat, 20f)));

        private bool ToggleReflectFire() => ToggleOnEnemies(ref _reflectFire, () => new ReflectFireItem());

        private bool ToggleChaosNoBypass() =>
                ToggleOnEnemies(ref _chaosNoBypass, () => new StatItem((DemoStats.ChaosBypassesShield, ModifierType.Override, 0f)));

        private bool ToggleChaosHalfBypass() =>
                ToggleOnEnemies(ref _chaosHalfBypass, () => new StatItem((DemoStats.ChaosBypassesShield, ModifierType.Flat, -50f)));

        private bool ToggleOnPlayer(ref ModifiableItem item, Func<ModifiableItem> create) {
            if (item == null) {
                item = create();
                item.Equip(demo.Player.Modifiable);
            }
            else {
                item.Unequip(demo.Player.Modifiable);
                item = null;
            }

            return item != null;
        }

        private bool ToggleOnFireball(ref ModifiableItem item, Func<ModifiableItem> create) {
            if (item == null) {
                item = create();
                item.Equip(demo.Player.Fireball);
            }
            else {
                item.Unequip(demo.Player.Fireball);
                item = null;
            }

            return item != null;
        }

        private bool ToggleCapability(ref FireballItem item, Func<FireballItem> create) {
            if (item == null) {
                item = create();
                item.Equip(demo.Player.Fireball);
            }
            else {
                item.Unequip(demo.Player.Fireball);
                item = null;
            }

            return item != null;
        }

        private bool ToggleOnEnemies(ref ModifiableItem item, Func<ModifiableItem> create) {
            if (item == null) {
                item = create();

                foreach (var enemy in demo.Enemies)
                    item.Equip(enemy.Modifiable);
            }
            else {
                foreach (var enemy in demo.Enemies)
                    item.Unequip(enemy.Modifiable);

                item = null;
            }

            return item != null;
        }

        #endregion Item toggles
    }
}
