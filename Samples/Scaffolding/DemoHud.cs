// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Samples {
    [RequireComponent(typeof(UIDocument))]
    public sealed class DemoHud : MonoBehaviour {
        [SerializeField] private CombatDemo demo;

        private VisualElement _root;
        private VisualElement _leftPanel;
        private VisualElement _rightPanel;
        private Label _status;
        private VisualElement _quadHost;
        private VisualElement _buffQuadContent;
        private bool _quadBuilt;
        private string _lastLevelIcons;
        private ItemPanel _itemPanel;

        public VisualElement CircuitHost { get; private set; }

        private ModifiableItem _flameHelmet;
        private ModifiableItem _fireDamage;
        private ModifiableItem _chaosDamage;
        private ModifiableItem _projectiles;
        private ModifiableItem _circular;
        private ModifiableItem _ignite;
        private ModifiableItem _split;
        private ModifiableItem _empower;
        private ModifiableItem _lifeSteal;
        private ModifiableItem _chaosNoBypass;
        private ModifiableItem _chaosHalfBypass;
        private ModifiableItem _fireResist;
        private ModifiableItem _coldResist;
        private ModifiableItem _lightningResist;
        private ModifiableItem _armor;
        private ModifiableItem _reflectFire;

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
            BuildLeftPanel();
            BuildRightPanel();
        }

        private void OnDisable() {
            _itemPanel?.Dispose();
            _itemPanel = null;
        }

        private void LateUpdate() {
            _status.text = $"Enemies alive: {demo.AliveCount}";

            if (!_quadBuilt && demo.Pool != null) {
                BuildQuad();
                _quadBuilt = true;
            }

            if (_quadBuilt && demo.Level != null && !ReferenceEquals(demo.Level.RolledIcons, _lastLevelIcons)) {
                _lastLevelIcons = demo.Level.RolledIcons;
                FillBuffQuad();
            }
        }

        #region Panels

        private void BuildLeftPanel() {
            _leftPanel?.RemoveFromHierarchy();

            var panel = MakePanel();
            _leftPanel = panel;
            panel.style.left = 12;
            panel.style.top = 12;
            panel.style.width = 240;
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

            if (demo.Player != null) {
                panel.Add(Section("ITEM — STAFF"));
                _itemPanel?.Dispose();
                _itemPanel = new ItemPanel(demo.Player.Modifiable);
                panel.Add(_itemPanel.Root);
            }

            panel.Add(Section("FIREBALL MODIFIERS"));
            panel.Add(ToggleButton("+100% More Fire Damage", ToggleFireDamage));
            panel.Add(ToggleButton("+20 Chaos Damage", ToggleChaosDamage));
            panel.Add(ToggleButton("+2 Projectiles", ToggleProjectiles));
            panel.Add(ToggleButton("Circular Nova", ToggleCircular));
            panel.Add(ToggleButton("Ignite", ToggleIgnite));
            panel.Add(ToggleButton("Split On Hit", ToggleSplit));
            panel.Add(ToggleButton("Empower On Kill", ToggleEmpower));
            panel.Add(ToggleButton("Life Steal", ToggleLifeSteal));
        }

        private void BuildRightPanel() {
            _rightPanel?.RemoveFromHierarchy();

            var panel = MakePanel();
            _rightPanel = panel;
            panel.style.right = 12;
            panel.style.top = 12;
            panel.style.minWidth = 240;
            panel.style.maxWidth = 560;
            _root.Add(panel);

            var title = new Label("ENEMIES");
            title.style.color = Color.white;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 8;
            panel.Add(title);

            panel.Add(Section("DAMAGE CIRCUIT"));

            CircuitHost = new VisualElement();
            panel.Add(CircuitHost);

            panel.Add(Section("LEVEL"));

            _quadHost = new VisualElement();
            _quadHost.style.marginBottom = 4;
            panel.Add(_quadHost);

            panel.Add(ActionButton("Reroll Level Modifiers", () => demo.RerollLevel()));

            panel.Add(Section("ENEMY MODIFIERS"));
            panel.Add(ToggleButton("+40 Fire Resistance", ToggleFireResist));
            panel.Add(ToggleButton("+40 Cold Resistance", ToggleColdResist));
            panel.Add(ToggleButton("+40 Lightning Resistance", ToggleLightningResist));
            panel.Add(ToggleButton("+20 Armor", ToggleArmor));
            panel.Add(ToggleButton("Reflect Fire (25%)", ToggleReflectFire));
            panel.Add(ToggleButton("Chaos Cannot Bypass Shield", ToggleChaosNoBypass));
            panel.Add(ToggleButton("-50% Chaos Shield Bypass", ToggleChaosHalfBypass));

            panel.Add(Section("SCENE"));
            panel.Add(LabeledSlider("Inner Ring", 1, 20, demo.InnerCount, v => demo.SetInnerCount((int)v)));
            panel.Add(LabeledSlider("Outer Ring", 1, 30, demo.OuterCount, v => demo.SetOuterCount((int)v)));
            panel.Add(LabeledSlider("Radius Jitter", 0, 3, 0, demo.SetRadiusJitter));
            panel.Add(LabeledToggle("Enemies Move", demo.EnemiesMove, v => demo.EnemiesMove = v));
            panel.Add(ActionButton("Respawn All", () => demo.RespawnAll()));
            panel.Add(LabeledToggle("Auto Respawn", demo.AutoRespawn, v => demo.AutoRespawn = v));
        }

        private static VisualElement MakePanel() {
            var panel = new VisualElement();
            panel.style.position = Position.Absolute;
            panel.style.paddingLeft = 12;
            panel.style.paddingRight = 12;
            panel.style.paddingTop = 10;
            panel.style.paddingBottom = 12;
            panel.style.backgroundColor = new Color(0.06f, 0.06f, 0.09f, 0.92f);
            Round(panel);

            return panel;
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

        private void BuildQuad() {
            _quadHost.Clear();

            var topRow = QuadRow();
            var bottomRow = QuadRow();
            _quadHost.Add(topRow);
            _quadHost.Add(bottomRow);

            topRow.Add(QuadCell("II · BUFFS", "inherited from the level — top-left of the nameplate",
                out _buffQuadContent));

            topRow.Add(QuadCell("I · MODIFIERS", "rolled per enemy at spawn — top-right of the nameplate",
                out var modifierContent));

            foreach (var entry in demo.Pool.Entries) {
                if (entry.candidate == null)
                    continue;

                modifierContent.Add(LegendRow(
                    CombatColors.ForModifier(entry.candidate.Hash),
                    entry.candidate.DisplayName,
                    entry.candidate.Description));
            }

            bottomRow.Add(QuadCell("III · DEBUFFS", "harmful effects — bottom-left of the nameplate",
                out var debuffContent));

            var ignited = ModifierRegistry.GetDefinition("sample_ignited");

            if (ignited != null) {
                debuffContent.Add(LegendRow(CombatColors.ForModifier(ignited.Hash), ignited.DisplayName,
                    ignited.Description));
            }

            bottomRow.Add(QuadCell("IV · RESERVED", "bottom-right of the nameplate", out var reservedContent));
            var reserved = new Label("—");
            reserved.style.color = new Color(0.4f, 0.44f, 0.52f);
            reserved.style.fontSize = 10;
            reservedContent.Add(reserved);

            FillBuffQuad();
        }

        private void FillBuffQuad() {
            if (_buffQuadContent == null)
                return;

            _buffQuadContent.Clear();

            var rolled = demo.Level != null ? demo.Level.Rolled : null;

            if (rolled != null) {
                for (var i = 0; i < rolled.Count; i++) {
                    var definition = ModifierRegistry.GetDefinition(rolled[i].modifierHash);

                    _buffQuadContent.Add(LegendRow(
                        CombatColors.ForModifier(rolled[i].modifierHash),
                        definition != null ? definition.DisplayName : $"#{rolled[i].modifierHash}",
                        definition != null ? definition.Description : ""));
                }
            }

            var hardened = ModifierRegistry.GetDefinition("sample_hardened");

            if (hardened != null) {
                _buffQuadContent.Add(LegendRow(CombatColors.ForModifier(hardened.Hash), hardened.DisplayName,
                    hardened.Description));
            }

            if (_buffQuadContent.childCount == 0) {
                var none = new Label("—");
                none.style.color = new Color(0.4f, 0.44f, 0.52f);
                none.style.fontSize = 10;
                _buffQuadContent.Add(none);
            }
        }

        private static VisualElement QuadRow() {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            return row;
        }

        private static VisualElement QuadCell(string header, string description, out VisualElement content) {
            var cell = new VisualElement();
            cell.style.flexGrow = 1;
            cell.style.flexBasis = 0;
            cell.style.marginLeft = 2;
            cell.style.marginRight = 2;
            cell.style.marginBottom = 4;
            cell.style.paddingLeft = 6;
            cell.style.paddingRight = 6;
            cell.style.paddingTop = 4;
            cell.style.paddingBottom = 6;
            cell.style.borderTopWidth = 1;
            cell.style.borderBottomWidth = 1;
            cell.style.borderLeftWidth = 1;
            cell.style.borderRightWidth = 1;
            var borderColor = new Color(0.3f, 0.34f, 0.42f);
            cell.style.borderTopColor = borderColor;
            cell.style.borderBottomColor = borderColor;
            cell.style.borderLeftColor = borderColor;
            cell.style.borderRightColor = borderColor;
            Round(cell);

            var headerLabel = new Label(header);
            headerLabel.style.color = new Color(0.6f, 0.66f, 0.78f);
            headerLabel.style.fontSize = 10;
            headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            headerLabel.style.marginBottom = 2;
            cell.Add(headerLabel);

            var descriptionLabel = new Label(description);
            descriptionLabel.style.color = new Color(0.45f, 0.5f, 0.6f);
            descriptionLabel.style.fontSize = 9;
            descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
            descriptionLabel.style.marginBottom = 4;
            cell.Add(descriptionLabel);

            content = new VisualElement();
            cell.Add(content);

            return cell;
        }

        private static VisualElement LegendRow(Color color, string name, string description) {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 2;

            var square = new VisualElement();
            square.style.width = 10;
            square.style.height = 10;
            square.style.marginRight = 6;
            square.style.flexShrink = 0;
            square.style.backgroundColor = color;
            row.Add(square);

            var label = new Label($"<b>{name}</b>  <color=#9AA0B0>{description}</color>");
            label.style.color = Color.white;
            label.style.fontSize = 10;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.flexShrink = 1;
            row.Add(label);

            return row;
        }

        #endregion Panels

        #region Item toggles

        private bool ToggleFlameHelmet() =>
                ToggleOnPlayer(ref _flameHelmet, () => new StatItem(
                    (DemoStats.Armor, ContributionType.Flat, 5f),
                    (DemoStats.FireDamage, ContributionType.Flat, 5f)));

        private bool ToggleFireDamage() =>
                ToggleOnFireball(ref _fireDamage,
                    () => new StatItem((DemoStats.FireDamage, ContributionType.More, 1f)));

        private bool ToggleChaosDamage() =>
                ToggleOnFireball(ref _chaosDamage,
                    () => new StatItem((DemoStats.ChaosDamage, ContributionType.Flat, 20f)));

        private bool ToggleProjectiles() =>
                ToggleOnFireball(ref _projectiles,
                    () => new StatItem((DemoStats.ProjectileCount, ContributionType.Flat, 2f)));

        private bool ToggleCircular() =>
                ToggleOnFireball(ref _circular,
                    () => new StatItem((DemoStats.ProjectilePattern, ContributionType.Override, 1f)));

        private bool ToggleIgnite() =>
                ToggleOnFireball(ref _ignite, () => new StatItem(
                    (DemoStats.IgniteChance, ContributionType.Flat, 1f),
                    (DemoStats.IgniteDuration, ContributionType.Flat, 3f)));

        private bool ToggleSplit() =>
                ToggleOnFireball(ref _split, () => new StatItem((DemoStats.SplitOnHit, ContributionType.Flat, 1f)));

        private bool ToggleEmpower() =>
                ToggleOnFireball(ref _empower,
                    () => new StatItem((DemoStats.EmpowerOnKill, ContributionType.Flat, 1f)));

        private bool ToggleLifeSteal() =>
                ToggleOnFireball(ref _lifeSteal,
                    () => new StatItem((DemoStats.LifeSteal, ContributionType.Flat, 0.3f)));

        private bool ToggleFireResist() =>
                ToggleOnEnemies(ref _fireResist,
                    () => new StatItem((DemoStats.FireResistance, ContributionType.Flat, 40f)));

        private bool ToggleColdResist() =>
                ToggleOnEnemies(ref _coldResist,
                    () => new StatItem((DemoStats.ColdResistance, ContributionType.Flat, 40f)));

        private bool ToggleLightningResist() =>
                ToggleOnEnemies(ref _lightningResist,
                    () => new StatItem((DemoStats.LightningResistance, ContributionType.Flat, 40f)));

        private bool ToggleArmor() =>
                ToggleOnEnemies(ref _armor, () => new StatItem((DemoStats.Armor, ContributionType.Flat, 20f)));

        private bool ToggleReflectFire() => ToggleOnEnemies(ref _reflectFire, () => new ReflectFireItem());

        private bool ToggleChaosNoBypass() =>
                ToggleOnEnemies(ref _chaosNoBypass,
                    () => new StatItem((DemoStats.ChaosBypassesShield, ContributionType.Override, 0f)));

        private bool ToggleChaosHalfBypass() =>
                ToggleOnEnemies(ref _chaosHalfBypass,
                    () => new StatItem((DemoStats.ChaosBypassesShield, ContributionType.Flat, -50f)));

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