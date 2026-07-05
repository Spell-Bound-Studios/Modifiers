// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Samples {
    public sealed class CircuitView : MonoBehaviour {
        private static readonly Dictionary<uint, string> StageLabels = new() {
            { DemoStages.Convert, "CONVERT" },
            { DemoStages.Mitigate, "MITIGATE" },
            { DemoStages.Apply, "APPLY" },
            { DemoStages.React, "REACT" }
        };

        private Label _subtitle;
        private VisualElement _diagram;
        private EnemyController _enemy;
        private string _signature;

        private void LateUpdate() {
            if (_diagram == null && !TryAttach())
                return;

            var circuit = ResolveCircuit();

            if (circuit == null) {
                _subtitle.style.display = DisplayStyle.None;
                _diagram.style.display = DisplayStyle.None;

                return;
            }

            _subtitle.style.display = DisplayStyle.Flex;
            _diagram.style.display = DisplayStyle.Flex;
            _subtitle.text = _enemy.name;

            var signature = Signature(circuit);

            if (signature == _signature)
                return;

            _signature = signature;
            Rebuild(circuit);
        }

        private bool TryAttach() {
            var hud = FindAnyObjectByType<DemoHud>();
            var host = hud != null ? hud.CircuitHost : null;

            if (host == null)
                return false;

            _subtitle = new Label("");
            _subtitle.style.color = new Color(0.6f, 0.66f, 0.78f);
            _subtitle.style.fontSize = 10;
            _subtitle.style.marginBottom = 4;
            host.Add(_subtitle);

            _diagram = new VisualElement();
            _diagram.style.flexDirection = FlexDirection.Row;
            _diagram.style.flexWrap = Wrap.Wrap;
            _diagram.style.alignItems = Align.FlexStart;
            host.Add(_diagram);

            return true;
        }

        private Circuit ResolveCircuit() {
            if (_enemy == null)
                _enemy = FindAnyObjectByType<EnemyController>();

            return _enemy == null ? null : _enemy.Modifiable.CircuitFor(DemoEvents.TakeHit);
        }

        private void Rebuild(Circuit circuit) {
            _diagram.Clear();

            var stages = circuit.Stages;

            for (var i = 0; i < stages.Count; i++) {
                if (i > 0)
                    _diagram.Add(Arrow());

                _diagram.Add(StepBox(i + 1, LabelForStage(stages[i]), stages[i]));
            }
        }

        private static string LabelForStage(Stage stage) =>
                StageLabels.TryGetValue(stage.Id, out var label) ? label : $"#{stage.Id}";

        private static VisualElement StepBox(int number, string label, Stage stage) {
            var box = new VisualElement();
            box.style.flexDirection = FlexDirection.Column;
            box.style.alignItems = Align.Center;
            box.style.paddingLeft = 6;
            box.style.paddingRight = 6;
            box.style.paddingTop = 4;
            box.style.paddingBottom = 6;
            box.style.marginBottom = 4;
            Border(box, new Color(0.4f, 0.45f, 0.55f));
            Round(box, 5);

            var header = new Label($"{number}   {label}");
            header.style.color = new Color(0.6f, 0.66f, 0.78f);
            header.style.fontSize = 10;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 4;
            box.Add(header);

            var children = stage.Children;

            if (children.Count == 0) {
                var empty = new Label("—");
                empty.style.color = new Color(0.4f, 0.44f, 0.52f);
                empty.style.fontSize = 11;
                empty.style.unityTextAlign = TextAnchor.MiddleCenter;
                empty.style.minWidth = 80;
                box.Add(empty);

                return box;
            }

            foreach (var child in children)
                box.Add(LeafBox(LabelFor(child)));

            return box;
        }

        private static Label LeafBox(string label) {
            var background = CombatColors.ForNode(label);

            var box = new Label(label) {
                style = {
                    backgroundColor = background,
                    color = TextOn(background),
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 11,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 5,
                    paddingBottom = 5,
                    marginTop = 2,
                    marginBottom = 2,
                    minWidth = 80
                }
            };

            Round(box, 4);

            return box;
        }

        private static string LabelFor(Node node) {
            var label = node.ToString();

            return label == node.GetType().FullName ? node.GetType().Name : label;
        }

        private static Label Arrow() {
            var arrow = new Label("→");
            arrow.style.color = Color.white;
            arrow.style.fontSize = 18;
            arrow.style.marginLeft = 4;
            arrow.style.marginRight = 4;
            arrow.style.alignSelf = Align.FlexStart;
            arrow.style.marginTop = 6;

            return arrow;
        }

        private static void Border(VisualElement element, Color color) {
            element.style.borderTopWidth = 1;
            element.style.borderBottomWidth = 1;
            element.style.borderLeftWidth = 1;
            element.style.borderRightWidth = 1;
            element.style.borderTopColor = color;
            element.style.borderBottomColor = color;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
        }

        private static void Round(VisualElement element, float radius) {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }

        private static Color TextOn(Color background) {
            var luminance = 0.299f * background.r + 0.587f * background.g + 0.114f * background.b;

            return luminance > 0.6f ? new Color(0.1f, 0.1f, 0.1f) : Color.white;
        }

        private static string Signature(Circuit circuit) {
            var sb = new StringBuilder(96);
            var stages = circuit.Stages;

            for (var s = 0; s < stages.Count; s++) {
                var stage = stages[s];
                sb.Append(stage.Id).Append('(');

                var children = stage.Children;

                for (var i = 0; i < children.Count; i++)
                    sb.Append(LabelFor(children[i])).Append(',');

                sb.Append(')');
            }

            return sb.ToString();
        }
    }
}