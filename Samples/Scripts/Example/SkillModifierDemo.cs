// Copyright 2025 Spellbound Studio Inc.

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Spellbound.Stats.Samples {
    public sealed class SkillModifierDemo : MonoBehaviour {
        [Header("Skill Setup")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private ModdedCollection rayOfFrostCollection;
        
        [Header("Scene Setup")]
        [SerializeField] private Transform player;
        [SerializeField] private GameObject enemyPrefab;
        
        [Header("Ring Defaults")]
        [SerializeField] private float innerRingDistance = 5f;
        [SerializeField] private float outerRingDistance = 10f;
        [SerializeField] private bool autoRespawn = true;
        
        [Header("Enemy Movement")]
        [SerializeField] private float moveSpeed = 2f;
        
        [Header("UI Setup")]
        [SerializeField] private Canvas canvas;
        
        [Header("Button Colors")]
        [SerializeField] private Color defaultButtonColor = Color.white;
        [SerializeField] private Color activeButtonColor = Color.green;
        
        private Fireball _fireball;
        private RayOfFrost _rayOfFrost;
        
        private AddedProjectileCountModifier _projectileCountMod;
        private CircularProjectileModifier _circularMod;
        private IncreasedDurationModifier _durationMod;
        private SplittingProjectileModifier _splitMod;
        
        private List<EnemyTarget> _innerEnemies = new();
        private List<EnemyTarget> _outerEnemies = new();
        
        private int _innerEnemyCount = 7;
        private int _outerEnemyCount = 14;
        private float _radiusJitter;
        private bool _enemiesMove;
        
        private TMP_Text _statusText;
        private TMP_Text _innerCountLabel;
        private TMP_Text _outerCountLabel;
        private TMP_Text _radiusJitterLabel;
        private Button _toggleProjectileCountButton;
        private Button _toggleCircularButton;
        private Button _toggleDurationButton;
        private Button _toggleSplitButton;
        
        private Button _rayOfFrostButton;
        private bool _isChannelingRayOfFrost;
        
        private void Start() {
            CreateUI();
            SpawnAllEnemies();
            InitializeSkills();
            UpdateStatusText();
            UpdateButtonColors();
        }
        
        private void Update() {
            if (_enemiesMove)
                MoveEnemiesAroundPlayer();
            
            HandleRayOfFrostChannel();
        }
        
        private void HandleRayOfFrostChannel() {
            if (_rayOfFrost == null)
                return;
            
            if (_isChannelingRayOfFrost) {
                _rayOfFrost.UpdateChannel(player.position, player.forward);
            }
        }
        
        private void CreateUI() {
            if (canvas == null) {
                var canvasObj = new GameObject("DemoCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(canvas.transform, false);
            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0.5f);
            panelRect.anchorMax = new Vector2(0, 0.5f);
            panelRect.pivot = new Vector2(0, 0.5f);
            panelRect.anchoredPosition = new Vector2(20, 0);
            panelRect.sizeDelta = new Vector2(300, 0);
            
            var verticalLayout = panelObj.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 8;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.padding = new RectOffset(10, 10, 10, 10);
            
            var sizeFitter = panelObj.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            var panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);
            
            _statusText = CreateText(panelObj.transform, "", 12);
            _statusText.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 180);
            
            CreateText(panelObj.transform, "═══ FIREBALL (Code) ═══", 12);
            CreateButton(panelObj.transform, "Cast Fireball", CastFireball);
            _toggleProjectileCountButton = CreateButton(panelObj.transform, "+6 Projectiles", ToggleProjectileCount);
            _toggleCircularButton = CreateButton(panelObj.transform, "Circular Pattern", ToggleCircular);
            _toggleDurationButton = CreateButton(panelObj.transform, "+50% Duration", ToggleDuration);
            _toggleSplitButton = CreateButton(panelObj.transform, "Split On Hit", ToggleSplit);
            
            CreateText(panelObj.transform, "═══ RAY OF FROST (SO) ═══", 12);
            _rayOfFrostButton = CreateHoldButton(panelObj.transform, "Channel Ray of Frost", OnRayOfFrostDown, OnRayOfFrostUp);
            
            CreateText(panelObj.transform, "═══ SCENE CONTROLS ═══", 12);
            
            _innerCountLabel = CreateText(panelObj.transform, $"Inner Ring Count: {_innerEnemyCount}", 12);
            CreateSlider(panelObj.transform, 1, 20, _innerEnemyCount, UpdateInnerEnemyCount);
            
            _outerCountLabel = CreateText(panelObj.transform, $"Outer Ring Count: {_outerEnemyCount}", 12);
            CreateSlider(panelObj.transform, 1, 30, _outerEnemyCount, UpdateOuterEnemyCount);
            
            _radiusJitterLabel = CreateText(panelObj.transform, $"Radius Jitter: {_radiusJitter:F1}", 12);
            CreateSlider(panelObj.transform, 0f, 3f, _radiusJitter, UpdateRadiusJitter);
            
            CreateToggle(panelObj.transform, "Enemies Move", _enemiesMove, e => _enemiesMove = e);
            
            CreateText(panelObj.transform, "═══ ENEMY CONTROLS ═══", 12);
            CreateButton(panelObj.transform, "Respawn All Enemies", RespawnAllEnemies);
            CreateToggle(panelObj.transform, "Auto Respawn (3s)", false, v => autoRespawn = v);
        }
        
        private TMP_Text CreateText(Transform parent, string text, int fontSize) {
            var obj = new GameObject("Text");
            obj.transform.SetParent(parent, false);
            
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280, 25);
            
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            
            return tmp;
        }
        
        private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick) {
            var obj = new GameObject("Button");
            obj.transform.SetParent(parent, false);
            
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280, 35);
            
            var image = obj.AddComponent<Image>();
            image.color = defaultButtonColor;
            
            var button = obj.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 14;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            
            return button;
        }
        
        private Button CreateHoldButton(Transform parent, string label, UnityEngine.Events.UnityAction onDown, UnityEngine.Events.UnityAction onUp) {
            var obj = new GameObject("HoldButton");
            obj.transform.SetParent(parent, false);
            
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280, 35);
            
            var image = obj.AddComponent<Image>();
            image.color = new Color(0.6f, 0.8f, 1f);
            
            var button = obj.AddComponent<Button>();
            button.targetGraphic = image;
            
            var eventTrigger = obj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            
            var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
            pointerDown.callback.AddListener(_ => onDown());
            eventTrigger.triggers.Add(pointerDown);
            
            var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
            pointerUp.callback.AddListener(_ => onUp());
            eventTrigger.triggers.Add(pointerUp);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 14;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            
            return button;
        }
        
        private void CreateSlider(Transform parent, float min, float max, float value, UnityEngine.Events.UnityAction<float> onValueChanged) {
            var obj = new GameObject("Slider");
            obj.transform.SetParent(parent, false);
            
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280, 20);
            
            var slider = obj.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            slider.onValueChanged.AddListener(onValueChanged);
            
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(obj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.25f);
            bgRect.anchorMax = new Vector2(1, 0.75f);
            bgRect.sizeDelta = Vector2.zero;
            var bgImage = bgObj.AddComponent<Image>();
            bgImage.color = Color.gray;
            
            var fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(obj.transform, false);
            var fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.sizeDelta = Vector2.zero;
            
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            var fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.sizeDelta = Vector2.zero;
            var fillImage = fillObj.AddComponent<Image>();
            fillImage.color = Color.green;
            
            slider.fillRect = fillRect;
            
            var handleAreaObj = new GameObject("Handle Slide Area");
            handleAreaObj.transform.SetParent(obj.transform, false);
            var handleAreaRect = handleAreaObj.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.sizeDelta = Vector2.zero;
            
            var handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(handleAreaObj.transform, false);
            var handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);
            var handleImage = handleObj.AddComponent<Image>();
            handleImage.color = Color.white;
            
            slider.handleRect = handleRect;
        }
        
        private void CreateToggle(Transform parent, string label, bool isOn, UnityEngine.Events.UnityAction<bool> onValueChanged) {
            var obj = new GameObject("Toggle");
            obj.transform.SetParent(parent, false);
            
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280, 30);
            
            var layout = obj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(obj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(25, 25);
            var bgImage = bgObj.AddComponent<Image>();
            bgImage.color = Color.white;
            
            var checkObj = new GameObject("Checkmark");
            checkObj.transform.SetParent(bgObj.transform, false);
            var checkRect = checkObj.AddComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.sizeDelta = new Vector2(-6, -6);
            checkRect.anchoredPosition = Vector2.zero;
            var checkImage = checkObj.AddComponent<Image>();
            checkImage.color = Color.green;
            
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(obj.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(200, 30);
            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 14;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            
            var toggle = obj.AddComponent<Toggle>();
            toggle.isOn = isOn;
            toggle.graphic = checkImage;
            toggle.targetGraphic = bgImage;
            toggle.onValueChanged.AddListener(onValueChanged);
        }
        
        private void UpdateInnerEnemyCount(float value) {
            var newCount = Mathf.RoundToInt(value);
            if (newCount == _innerEnemyCount) return;
            
            _innerEnemyCount = newCount;
            _innerCountLabel.text = $"Inner Ring Count: {_innerEnemyCount}";
            RespawnRing(ref _innerEnemies, _innerEnemyCount, innerRingDistance, "Inner");
        }
        
        private void UpdateOuterEnemyCount(float value) {
            var newCount = Mathf.RoundToInt(value);
            if (newCount == _outerEnemyCount) return;
            
            _outerEnemyCount = newCount;
            _outerCountLabel.text = $"Outer Ring Count: {_outerEnemyCount}";
            RespawnRing(ref _outerEnemies, _outerEnemyCount, outerRingDistance, "Outer");
        }
        
        private void UpdateRadiusJitter(float value) {
            _radiusJitter = value;
            _radiusJitterLabel.text = $"Radius Jitter: {_radiusJitter:F1}";
            ApplyJitterToRings();
        }
        
        private void SpawnAllEnemies() {
            if (enemyPrefab == null) {
                Debug.LogError("Enemy prefab not assigned!");
                return;
            }
            
            if (player == null) {
                Debug.LogError("Player transform not assigned!");
                return;
            }
            
            SpawnRing(ref _innerEnemies, _innerEnemyCount, innerRingDistance, "Inner");
            SpawnRing(ref _outerEnemies, _outerEnemyCount, outerRingDistance, "Outer");
            
            Debug.Log($"[Demo] Spawned {_innerEnemyCount} inner enemies (radius: {innerRingDistance})");
            Debug.Log($"[Demo] Spawned {_outerEnemyCount} outer enemies (radius: {outerRingDistance})");
        }
        
        private void RespawnAllEnemies() {
            foreach (var enemy in _innerEnemies)
                if (enemy != null && enemy.IsDead)
                    enemy.Respawn();
    
            foreach (var enemy in _outerEnemies)
                if (enemy != null && enemy.IsDead)
                    enemy.Respawn();
        }
        
        private void SpawnRing(ref List<EnemyTarget> enemies, int count, float distance, string prefix) {
            enemies.Clear();
            var playerPos = player.position;
    
            for (var i = 0; i < count; i++) {
                var angle = (360f / count) * i * Mathf.Deg2Rad;
                var jitteredDistance = distance + Random.Range(-_radiusJitter, _radiusJitter);
                var offset = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * jitteredDistance;
                var spawnPos = playerPos + offset;
        
                var enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                enemyObj.name = $"{prefix}_Enemy_{i + 1}";
                enemyObj.transform.LookAt(new Vector3(playerPos.x, enemyObj.transform.position.y, playerPos.z));
        
                var enemy = enemyObj.GetComponent<EnemyTarget>();
                enemy.OnDeath += OnEnemyDeath;
                enemies.Add(enemy);
            }
        }

        private void OnEnemyDeath(EnemyTarget enemy) {
            if (autoRespawn)
                StartCoroutine(RespawnAfterDelay(enemy, 3f));
        }

        private IEnumerator RespawnAfterDelay(EnemyTarget enemy, float delay) {
            yield return new WaitForSeconds(delay);
            if (enemy != null)
                enemy.Respawn();
        }
        
        private void RespawnRing(ref List<EnemyTarget> enemies, int count, float distance, string prefix) {
            foreach (var enemy in enemies) {
                if (enemy != null)
                    Destroy(enemy.gameObject);
            }
            
            SpawnRing(ref enemies, count, distance, prefix);
        }
        
        private void ApplyJitterToRings() {
            ApplyJitterToRing(_innerEnemies, innerRingDistance);
            ApplyJitterToRing(_outerEnemies, outerRingDistance);
        }
        
        private void ApplyJitterToRing(List<EnemyTarget> enemies, float baseDistance) {
            if (enemies == null) return;
            
            var playerPos = player.position;
            var count = enemies.Count;
            
            for (var i = 0; i < count; i++) {
                if (enemies[i] == null) continue;
                
                var angle = (360f / count) * i * Mathf.Deg2Rad;
                var jitteredDistance = baseDistance + Random.Range(-_radiusJitter, _radiusJitter);
                var offset = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * jitteredDistance;
                
                enemies[i].transform.position = playerPos + offset;
                enemies[i].transform.LookAt(new Vector3(playerPos.x, enemies[i].transform.position.y, playerPos.z));
            }
        }
        
        private void MoveEnemiesAroundPlayer() {
            var playerPos = player.position;
            var rotationAmount = moveSpeed * Time.deltaTime;
            
            RotateRing(_innerEnemies, playerPos, rotationAmount);
            RotateRing(_outerEnemies, playerPos, -rotationAmount);
        }
        
        private void RotateRing(List<EnemyTarget> enemies, Vector3 center, float rotationAmount) {
            if (enemies == null) return;
            
            foreach (var enemy in enemies) {
                if (enemy == null) continue;
                
                var direction = enemy.transform.position - center;
                var currentDistance = direction.magnitude;
                direction.y = 0;
                direction = Quaternion.AngleAxis(rotationAmount, Vector3.up) * direction;
                
                enemy.transform.position = center + direction.normalized * currentDistance;
                enemy.transform.LookAt(new Vector3(center.x, enemy.transform.position.y, center.z));
            }
        }
        
        private void InitializeSkills() {
            _fireball = new Fireball {
                projectilePrefab = projectilePrefab
            };
            _fireball.Initialize();
            Debug.Log($"[Demo] Fireball initialized: {_fireball.Name}");

            if (rayOfFrostCollection == null) 
                return;
            
            var instance = rayOfFrostCollection.CreateInstance();
            _rayOfFrost = instance as RayOfFrost;
                
            if (_rayOfFrost != null)
                Debug.Log($"[Demo] Ray of Frost initialized from SO: {_rayOfFrost.Name}");
            else
                Debug.LogWarning("[Demo] RayOfFrost collection did not create a RayOfFrost instance");
        }
        
        private void CastFireball() {
            if (_fireball == null) 
                return;
            
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("CASTING FIREBALL");
            Debug.Log("═══════════════════════════════════════");
            
            _fireball.Cast(player.position, player.forward);
        }
        
        private void OnRayOfFrostDown() {
            if (_rayOfFrost == null)
                return;
            
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("CHANNELING RAY OF FROST");
            Debug.Log("═══════════════════════════════════════");
            
            _isChannelingRayOfFrost = true;
            _rayOfFrost.StartChannel(player.position, player.forward);
        }
        
        private void OnRayOfFrostUp() {
            if (_rayOfFrost == null)
                return;
            
            Debug.Log("RAY OF FROST STOPPED");
            
            _isChannelingRayOfFrost = false;
            _rayOfFrost.StopChannel();
        }
        
        #region Toggle Modifiers
        
        private void ToggleProjectileCount() {
            if (_projectileCountMod == null) {
                _projectileCountMod = new AddedProjectileCountModifier();
                _projectileCountMod.Apply(_fireball);
                Debug.Log("[Demo] Added: +6 Projectile Count");
            } else {
                _projectileCountMod.Remove(_fireball);
                _projectileCountMod = null;
                Debug.Log("[Demo] Removed: +6 Projectile Count");
            }
            
            UpdateStatusText();
            UpdateButtonColors();
        }
        
        private void ToggleCircular() {
            if (_circularMod == null) {
                _circularMod = new CircularProjectileModifier();
                _circularMod.Apply(_fireball);
                Debug.Log("[Demo] Added: Circular Projectile Pattern");
            } else {
                _circularMod.Remove(_fireball);
                _circularMod = null;
                Debug.Log("[Demo] Removed: Circular Projectile Pattern");
            }
            
            UpdateStatusText();
            UpdateButtonColors();
        }
        
        private void ToggleDuration() {
            if (_durationMod == null) {
                _durationMod = new IncreasedDurationModifier();
                _durationMod.Apply(_fireball);
                Debug.Log("[Demo] Added: +50% Increased Duration");
            } else {
                _durationMod.Remove(_fireball);
                _durationMod = null;
                Debug.Log("[Demo] Removed: +50% Increased Duration");
            }
            
            UpdateStatusText();
            UpdateButtonColors();
        }
        
        private void ToggleSplit() {
            if (_splitMod == null) {
                _splitMod = new SplittingProjectileModifier();
                _splitMod.Apply(_fireball);
                Debug.Log("[Demo] Added: Split On Hit");
            } else {
                _splitMod.Remove(_fireball);
                _splitMod = null;
                Debug.Log("[Demo] Removed: Split On Hit");
            }
            
            UpdateStatusText();
            UpdateButtonColors();
        }
        
        #endregion
        
        private void UpdateButtonColors() {
            SetButtonColor(_toggleProjectileCountButton, _projectileCountMod != null);
            SetButtonColor(_toggleCircularButton, _circularMod != null);
            SetButtonColor(_toggleDurationButton, _durationMod != null);
            SetButtonColor(_toggleSplitButton, _splitMod != null);
        }
        
        private void SetButtonColor(Button button, bool isActive) {
            if (button == null) return;
            
            var colors = button.colors;
            colors.normalColor = isActive ? activeButtonColor : defaultButtonColor;
            colors.highlightedColor = isActive ? activeButtonColor : defaultButtonColor;
            colors.selectedColor = isActive ? activeButtonColor : defaultButtonColor;
            button.colors = colors;
        }
        
        private void UpdateStatusText() {
            if (_statusText == null || _fireball == null) return;
            
            var projectile = _fireball.Behaviours.GetBehaviour<ProjectileBehaviour>();
            var fire = _fireball.Behaviours.GetBehaviour<FireBehaviour>();
            var duration = _fireball.Behaviours.GetBehaviour<DurationBehaviour>();
            
            var count = (int)projectile.Stats.GetValue("projectile_count");
            var speed = projectile.Stats.GetValue("projectile_speed");
            var damage = fire.Stats.GetValue("fire_damage");
            var igniteChance = fire.Stats.GetValue("ignite_chance");
            var igniteDuration = duration.GetIgniteDuration();
            var pattern = _circularMod != null ? "Circular" : "Forward";
            
            _statusText.text = $@"═══ FIREBALL STATS ═══
Projectiles: {count}
Speed: {speed}
Pattern: {pattern}
Fire Damage: {damage}
Ignite Chance: {igniteChance}%
Ignite Duration: {igniteDuration}s

═══ ACTIVE MODIFIERS ═══
[{(_projectileCountMod != null ? "X" : " ")}] +6 Projectile Count
[{(_circularMod != null ? "X" : " ")}] Circular Pattern
[{(_durationMod != null ? "X" : " ")}] +50% Duration
[{(_splitMod != null ? "X" : " ")}] Split On Hit";
        }
    }
}