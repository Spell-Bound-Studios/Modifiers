// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Scaffolding: draws the live damage circuit of an enemy's <see cref="PipelineBehaviour"/> in UI Toolkit.
    /// The top-level sequence is numbered boxes (1..n) flowing left-to-right with arrows; a parallel tier stacks
    /// its stages inside its box. Stage colors come from <see cref="CombatColors"/> so the diagram and the
    /// floating numbers read together. Rebuilt only when the circuit's shape changes, so stages visibly appear
    /// and leave as modifiers reshape it. Needs a <see cref="UIDocument"/> + Panel Settings, like the HUD.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class CircuitView : MonoBehaviour {
        private VisualElement _panel;
        private Label _title;
        private VisualElement _diagram;
        private EnemyController _enemy;
        private string _signature;

        private void OnEnable() {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _panel?.RemoveFromHierarchy();

            _panel = new VisualElement();
            _panel.style.position = Position.Absolute;
            _panel.style.right = 12;
            _panel.style.top = 12;
            _panel.style.paddingLeft = 12;
            _panel.style.paddingRight = 12;
            _panel.style.paddingTop = 10;
            _panel.style.paddingBottom = 12;
            _panel.style.backgroundColor = new Color(0.05f, 0.05f, 0.08f, 0.9f);
            Round(_panel, 6);
            root.Add(_panel);

            _title = new Label("DAMAGE CIRCUIT");
            _title.style.color = Color.white;
            _title.style.unityFontStyleAndWeight = FontStyle.Bold;
            _title.style.fontSize = 12;
            _title.style.marginBottom = 8;
            _panel.Add(_title);

            _diagram = new VisualElement();
            _diagram.style.flexDirection = FlexDirection.Row;
            _diagram.style.alignItems = Align.Center;
            _panel.Add(_diagram);
        }

        private void LateUpdate() {
            var root = ResolveCircuit();

            if (root == null) {
                _panel.style.display = DisplayStyle.None;

                return;
            }

            _panel.style.display = DisplayStyle.Flex;
            _title.text = $"DAMAGE CIRCUIT — {_enemy.name}";

            var signature = Signature(root);

            if (signature == _signature)
                return;

            _signature = signature;
            Rebuild(root);
        }

        private PipelineNode<DamageContext> ResolveCircuit() {
            if (_enemy == null)
                _enemy = FindAnyObjectByType<EnemyController>();

            return _enemy == null
                    ? null
                    : _enemy.Behaviours.GetBehaviour<PipelineBehaviour>()?.Root;
        }

        private void Rebuild(PipelineNode<DamageContext> root) {
            _diagram.Clear();

            if (root is GroupNode<DamageContext> sequence && sequence.Kind == PipelineGroupKind.Sequence) {
                for (var i = 0; i < sequence.Children.Count; i++) {
                    _diagram.Add(StepBox(i + 1, sequence.Children[i]));

                    if (i < sequence.Children.Count - 1)
                        _diagram.Add(Arrow());
                }

                return;
            }

            _diagram.Add(RenderNode(root));
        }

        private VisualElement StepBox(int number, PipelineNode<DamageContext> node) {
            var box = new VisualElement();
            box.style.flexDirection = FlexDirection.Column;
            box.style.alignItems = Align.Center;
            box.style.paddingLeft = 6;
            box.style.paddingRight = 6;
            box.style.paddingTop = 4;
            box.style.paddingBottom = 6;
            Border(box, new Color(0.4f, 0.45f, 0.55f));
            Round(box, 5);

            var parallel = node is GroupNode<DamageContext> group && group.Kind == PipelineGroupKind.Parallel;

            var header = new Label(parallel ? $"{number}   ∥" : number.ToString());
            header.style.color = new Color(0.6f, 0.66f, 0.78f);
            header.style.fontSize = 10;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 4;
            box.Add(header);

            box.Add(RenderNode(node));

            return box;
        }

        private VisualElement RenderNode(PipelineNode<DamageContext> node) {
            if (node is not GroupNode<DamageContext> group)
                return StageBox(node.Id);

            var container = new VisualElement();
            container.style.flexDirection = group.Kind == PipelineGroupKind.Parallel
                    ? FlexDirection.Column
                    : FlexDirection.Row;
            container.style.alignItems = Align.Center;

            for (var i = 0; i < group.Children.Count; i++) {
                container.Add(RenderNode(group.Children[i]));

                if (group.Kind == PipelineGroupKind.Sequence && i < group.Children.Count - 1)
                    container.Add(Arrow());
            }

            return container;
        }

        private static Label StageBox(string id) {
            var background = CombatColors.ForNode(id);

            var box = new Label(id) {
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

        private static Label Arrow() {
            var arrow = new Label("→");
            arrow.style.color = Color.white;
            arrow.style.fontSize = 18;
            arrow.style.marginLeft = 4;
            arrow.style.marginRight = 4;

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

        private static string Signature(PipelineNode<DamageContext> node) {
            if (node is not GroupNode<DamageContext> group)
                return node.Id;

            var result = $"{group.Kind}:{node.Id}(";

            foreach (var child in group.Children)
                result += Signature(child) + ",";

            return result + ")";
        }
    }
}
