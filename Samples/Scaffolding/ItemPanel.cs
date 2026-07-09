// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Item card + lifecycle buttons for the sample staff. Pure scaffolding — the API on display is
    /// <see cref="ItemDefinition"/> / <see cref="ItemInstance"/>; everything in this file is styling.
    /// Inline implicit lines render with no range (the roll belongs to the instance); named lines — implicit
    /// or crafted — resolve through their definition and show roll ranges (they trace back to it).
    /// </summary>
    public sealed class ItemPanel {
        private static readonly Color TitleColor = new(0.9f, 0.82f, 0.55f);
        private static readonly Color ImplicitColor = new(0.75f, 0.85f, 1f);
        private static readonly Color ModifierColor = new(0.62f, 0.78f, 0.58f);
        private static readonly Color DimColor = new(0.5f, 0.54f, 0.64f);

        private readonly Modifiable _wearer;
        private readonly System.Random _rng = new();
        private readonly ItemDefinition _definition;
        private readonly VisualElement _card;
        private readonly Button _equipButton;
        private ItemInstance _item;

        public VisualElement Root { get; } = new();

        public ItemPanel(Modifiable wearer) {
            _wearer = wearer;
            _definition = Resources.Load<ItemDefinition>("Item_Staff");

            if (_definition == null) {
                Root.Add(MakeLine("Item_Staff.asset not found in a Resources folder.", DimColor));

                return;
            }

            _card = MakeCard();
            Root.Add(_card);

            _equipButton = MakeButton("Equip", ToggleEquip);
            Root.Add(_equipButton);
            Root.Add(MakeButton("Add Random Modifier", AddModifier));
            Root.Add(MakeButton("Remove Random Modifier", RemoveModifier));
            Root.Add(MakeButton("Remove All Modifiers", RemoveAll));
            Root.Add(MakeButton("Drop New Staff", DropNewStaff));

            DropNewStaff();
        }

        public void Dispose() => _item?.Unequip();

        #region Actions

        private void DropNewStaff() {
            _item?.Unequip();
            _item = new ItemInstance(_definition, _rng);
            RefreshCard();
            RefreshEquipButton();
        }

        private void ToggleEquip() {
            if (_item.IsEquipped)
                _item.Unequip();
            else
                _item.Equip(_wearer);

            RefreshEquipButton();
        }

        private void AddModifier() {
            if (_item.AddRandomModifier(_rng))
                RefreshCard();
        }

        private void RemoveModifier() {
            if (_item.RemoveRandomModifier(_rng))
                RefreshCard();
        }

        private void RemoveAll() {
            if (_item.RemoveAllModifiers() > 0)
                RefreshCard();
        }

        #endregion Actions

        #region Card

        private void RefreshCard() {
            _card.Clear();

            var title = MakeLine(_definition.ItemName, TitleColor);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            _card.Add(title);

            if (!string.IsNullOrEmpty(_definition.Description))
                _card.Add(MakeLine($"<i>{_definition.Description}</i>", DimColor));

            var grants = _definition.Implicits.Grants;

            for (var i = 0; i < grants.Count; i++) {
                switch (grants[i]) {
                    case ContributionSpecification specification when specification.IsValid:
                        _card.Add(MakeLine(
                            DescribeSpecification(specification, _item.ImplicitRolls.baked, false),
                            ImplicitColor));

                        break;
                    case NamedModifierGrant named when named.IsValid: {
                        var name = MakeLine(named.Definition.DisplayName, ImplicitColor);
                        name.style.unityFontStyleAndWeight = FontStyle.Bold;
                        _card.Add(name);

                        var baked = FindImplicitRecord(named.Definition.Hash);
                        var specifications = named.Definition.Contributions;

                        for (var j = 0; j < specifications.Count; j++) {
                            _card.Add(MakeLine(DescribeSpecification(specifications[j], baked, true),
                                ImplicitColor));
                        }

                        break;
                    }
                }
            }

            _card.Add(MakeDivider());

            if (_item.Modifiers.Count == 0) {
                _card.Add(MakeLine("no modifiers", DimColor));

                return;
            }

            for (var i = 0; i < _item.Modifiers.Count; i++) {
                var rolled = _item.Modifiers[i];
                var definition = ModifierRegistry.GetDefinition(rolled.modifierHash);

                if (definition == null) {
                    _card.Add(MakeLine($"#{rolled.modifierHash}", DimColor));

                    continue;
                }

                var name = MakeLine(definition.DisplayName, ModifierColor);
                name.style.unityFontStyleAndWeight = FontStyle.Bold;
                _card.Add(name);

                var specifications = definition.Contributions;

                for (var j = 0; j < specifications.Count; j++)
                    _card.Add(MakeLine(DescribeSpecification(specifications[j], rolled.baked, true), ModifierColor));
            }
        }

        private void RefreshEquipButton() {
            _equipButton.text = _item.IsEquipped ? "Unequip" : "Equip";
            _equipButton.style.backgroundColor =
                    _item.IsEquipped ? new Color(0.2f, 0.55f, 0.25f) : new Color(0.22f, 0.22f, 0.27f);
        }

        private BakedRoll[] FindImplicitRecord(uint modifierHash) {
            var modifiers = _item.ImplicitRolls.modifiers;

            if (modifiers != null) {
                for (var i = 0; i < modifiers.Length; i++) {
                    if (modifiers[i].modifierHash == modifierHash)
                        return modifiers[i].baked;
                }
            }

            return null;
        }

        #endregion Card

        #region Formatting

        private static string DescribeSpecification(
            ContributionSpecification specification, IReadOnlyList<BakedRoll> baked, bool withRanges) {
            switch (specification) {
                case SingleStatContribution single: {
                    var text = $"+{ResolveValue(single.Amount, single.Stat, baked):0.#} {single.Stat.DisplayName}";

                    return withRanges ? WithRange(text, DescribeRange(single.Amount)) : text;
                }
                case StatBandContribution band: {
                    var low = ResolveValue(band.LowAmount, band.LowStat, baked);
                    var high = ResolveValue(band.HighAmount, band.HighStat, baked);
                    var text = $"{low:0.#}–{high:0.#} {band.LowStat.DisplayName}";

                    if (!withRanges)
                        return text;

                    var range = DescribeRange(band.LowAmount);
                    var highRange = DescribeRange(band.HighAmount);

                    if (highRange != null)
                        range = range == null ? highRange : $"{range}, {highRange}";

                    return WithRange(text, range);
                }
                case MultiStatContribution multi: {
                    var names = new List<string>(multi.Stats.Count);

                    foreach (var stat in multi.Stats) {
                        if (stat != null)
                            names.Add(stat.DisplayName);
                    }

                    var value = multi.Stats.Count > 0 ? ResolveValue(multi.Amount, multi.Stats[0], baked) : 0f;
                    var text = $"+{value:0.#} {string.Join(", ", names)}";

                    return withRanges ? WithRange(text, DescribeRange(multi.Amount)) : text;
                }
                default: {
                    var parts = new List<string>();

                    foreach (var (stat, _, amount) in specification.StatContributions) {
                        if (stat == null || amount == null)
                            continue;

                        var text = $"+{ResolveValue(amount, stat, baked):0.#} {stat.DisplayName}";
                        parts.Add(withRanges ? WithRange(text, DescribeRange(amount)) : text);
                    }

                    return parts.Count > 0 ? string.Join("\n", parts) : specification.ToString();
                }
            }
        }

        private static string WithRange(string text, string range) =>
                range == null ? text : $"{text}  <color=#808A9E>({range})</color>";

        private static string DescribeRange(Magnitude magnitude) =>
                magnitude is RolledMagnitude rolled ? $"{rolled.Min:0.#}–{rolled.Max:0.#}" : null;

        private static float ResolveValue(Magnitude magnitude, StatDefinition stat, IReadOnlyList<BakedRoll> baked) {
            if (magnitude is not ScalarMagnitude scalar)
                return 0f;

            if (!magnitude.Rolls)
                return scalar.Value(0f);

            if (baked != null) {
                for (var i = 0; i < baked.Count; i++) {
                    if (baked[i].statHash == stat.Hash)
                        return scalar.Value(baked[i].value);
                }
            }

            return 0f;
        }

        #endregion Formatting

        #region Styling

        private static VisualElement MakeCard() {
            var card = new VisualElement();
            card.style.backgroundColor = new Color(0.1f, 0.1f, 0.14f);
            card.style.paddingLeft = 8;
            card.style.paddingRight = 8;
            card.style.paddingTop = 6;
            card.style.paddingBottom = 6;
            card.style.marginBottom = 6;
            card.style.borderTopLeftRadius = 5;
            card.style.borderTopRightRadius = 5;
            card.style.borderBottomLeftRadius = 5;
            card.style.borderBottomRightRadius = 5;

            return card;
        }

        private static Label MakeLine(string text, Color color) {
            var label = new Label(text);
            label.style.color = color;
            label.style.fontSize = 11;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.marginBottom = 1;

            return label;
        }

        private static VisualElement MakeDivider() {
            var divider = new VisualElement();
            divider.style.height = 1;
            divider.style.marginTop = 4;
            divider.style.marginBottom = 4;
            divider.style.backgroundColor = new Color(0.3f, 0.34f, 0.42f);

            return divider;
        }

        private static Button MakeButton(string text, Action onClick) {
            var button = new Button(onClick) { text = text };
            button.style.height = 26;
            button.style.marginTop = 0;
            button.style.marginBottom = 4;
            button.style.marginLeft = 0;
            button.style.marginRight = 0;
            button.style.color = Color.white;
            button.style.backgroundColor = new Color(0.22f, 0.22f, 0.27f);
            button.style.borderTopLeftRadius = 5;
            button.style.borderTopRightRadius = 5;
            button.style.borderBottomLeftRadius = 5;
            button.style.borderBottomRightRadius = 5;

            return button;
        }

        #endregion Styling
    }
}
