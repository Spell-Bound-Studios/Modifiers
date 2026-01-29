// Copyright 2025 Spellbound Studio Inc.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Spellbound.Stats.Samples {
    public sealed class FireballDemo : MonoBehaviour {
        [Header("Skill Setup")]
        [SerializeField] private GameObject projectilePrefab;
        
        [Header("Scene Setup")]
        [SerializeField] private Transform player;
        [SerializeField] private GameObject enemyPrefab;
        
        [Header("Inner Ring")]
        [SerializeField] private int innerEnemyCount = 7;
        [SerializeField] private float innerRingDistance = 5f;
        
        [Header("Outer Ring")]
        [SerializeField] private int outerEnemyCount = 14;
        [SerializeField] private float outerRingDistance = 10f;
        [SerializeField] private float outerRingOffset = 0f;
        
        [Header("Enemy Movement")]
        [SerializeField] private bool enemiesMove = false;
        [SerializeField] private float moveSpeed = 2f;
        
        [Header("UI Buttons")]
        [SerializeField] private Button castButton;
        [SerializeField] private Button toggleProjectileCountButton;
        [SerializeField] private Button toggleCircularButton;
        [SerializeField] private Button toggleDurationButton;
        [SerializeField] private Button toggleSplitButton;
        
        [Header("UI Sliders")]
        [SerializeField] private Slider outerOffsetSlider;
        [SerializeField] private TMP_Text outerOffsetLabel;
        [SerializeField] private Toggle movementToggle;
        
        [Header("UI Status")]
        [SerializeField] private TMP_Text statusText;
        
        [Header("Button Colors")]
        [SerializeField] private Color defaultButtonColor = Color.white;
        [SerializeField] private Color activeButtonColor = Color.green;
        
        private FireballSkill _fireball;
        
        private AddedProjectileCountModifier _projectileCountMod;
        private CircularProjectileModifier _circularMod;
        private IncreasedDurationModifier _durationMod;
        private SplittingProjectileModifier _splitMod;
        
        private EnemyTarget[] _innerEnemies;
        private EnemyTarget[] _outerEnemies;
        
        private void Start() {
            SpawnEnemies();
            InitializeSkill();
            SetupButtons();
            SetupSliders();
            UpdateStatusText();
            UpdateButtonColors();
        }
        
        private void Update() {
            if (enemiesMove)
                MoveEnemiesAroundPlayer();
        }
        
        private void SpawnEnemies() {
            if (enemyPrefab == null) {
                Debug.LogError("Enemy prefab not assigned!");
                return;
            }
            
            if (player == null) {
                Debug.LogError("Player transform not assigned!");
                return;
            }
            
            _innerEnemies = SpawnRing(innerEnemyCount, innerRingDistance, 0f, "Inner");
            _outerEnemies = SpawnRing(outerEnemyCount, outerRingDistance, outerRingOffset, "Outer");
            
            Debug.Log($"[Demo] Spawned {innerEnemyCount} inner enemies (radius: {innerRingDistance})");
            Debug.Log($"[Demo] Spawned {outerEnemyCount} outer enemies (radius: {outerRingDistance})");
        }
        
        private EnemyTarget[] SpawnRing(int count, float distance, float angleOffset, string prefix) {
            var enemies = new EnemyTarget[count];
            var playerPos = player.position;
            
            for (var i = 0; i < count; i++) {
                var angle = ((360f / count) * i + angleOffset) * Mathf.Deg2Rad;
                var offset = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * distance;
                var spawnPos = playerPos + offset;
                
                var enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                enemyObj.name = $"{prefix}_Enemy_{i + 1}";
                enemyObj.transform.LookAt(new Vector3(playerPos.x, enemyObj.transform.position.y, playerPos.z));
                
                enemies[i] = enemyObj.GetComponent<EnemyTarget>();
            }
            
            return enemies;
        }
        
        private void MoveEnemiesAroundPlayer() {
            var playerPos = player.position;
            var rotationAmount = moveSpeed * Time.deltaTime;
            
            RotateRing(_innerEnemies, playerPos, innerRingDistance, rotationAmount);
            RotateRing(_outerEnemies, playerPos, outerRingDistance, -rotationAmount); // Opposite direction
        }
        
        private void RotateRing(EnemyTarget[] enemies, Vector3 center, float distance, float rotationAmount) {
            if (enemies == null) 
                return;
            
            foreach (var enemy in enemies) {
                if (enemy == null) 
                    continue;
                
                var direction = enemy.transform.position - center;
                direction.y = 0;
                direction = Quaternion.AngleAxis(rotationAmount, Vector3.up) * direction;
                
                enemy.transform.position = center + direction.normalized * distance;
                enemy.transform.LookAt(new Vector3(center.x, enemy.transform.position.y, center.z));
            }
        }
        
        private void UpdateOuterRingOffset(float newOffset) {
            outerRingOffset = newOffset;
            
            if (outerOffsetLabel != null)
                outerOffsetLabel.text = $"Outer Ring Offset: {newOffset:F1}°";
            
            RepositionOuterRing();
        }
        
        private void RepositionOuterRing() {
            if (_outerEnemies == null) return;
            
            var playerPos = player.position;
            
            for (var i = 0; i < _outerEnemies.Length; i++) {
                if (_outerEnemies[i] == null) continue;
                
                var angle = ((360f / outerEnemyCount) * i + outerRingOffset) * Mathf.Deg2Rad;
                var offset = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * outerRingDistance;
                
                _outerEnemies[i].transform.position = playerPos + offset;
                _outerEnemies[i].transform.LookAt(new Vector3(playerPos.x, _outerEnemies[i].transform.position.y, playerPos.z));
            }
        }
        
        private void InitializeSkill() {
            _fireball = new FireballSkill {
                ProjectilePrefab = projectilePrefab
            };
            _fireball.Initialize();
            
            Debug.Log($"[Demo] Fireball skill initialized: {_fireball.Name}");
        }
        
        private void SetupButtons() {
            castButton?.onClick.AddListener(CastFireball);
            toggleProjectileCountButton?.onClick.AddListener(ToggleProjectileCount);
            toggleCircularButton?.onClick.AddListener(ToggleCircular);
            toggleDurationButton?.onClick.AddListener(ToggleDuration);
            toggleSplitButton?.onClick.AddListener(ToggleSplit);
        }
        
        private void SetupSliders() {
            if (outerOffsetSlider != null) {
                outerOffsetSlider.minValue = 0f;
                outerOffsetSlider.maxValue = 360f / outerEnemyCount;
                outerOffsetSlider.value = outerRingOffset;
                outerOffsetSlider.onValueChanged.AddListener(UpdateOuterRingOffset);
            }
            
            if (movementToggle != null) {
                movementToggle.isOn = enemiesMove;
                movementToggle.onValueChanged.AddListener(enabled => enemiesMove = enabled);
            }
            
            UpdateOuterRingOffset(outerRingOffset);
        }
        
        private void CastFireball() {
            if (_fireball == null) 
                return;
            
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("CASTING FIREBALL");
            Debug.Log("═══════════════════════════════════════");
            
            _fireball.Cast(player.position, player.forward);
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
            SetButtonColor(toggleProjectileCountButton, _projectileCountMod != null);
            SetButtonColor(toggleCircularButton, _circularMod != null);
            SetButtonColor(toggleDurationButton, _durationMod != null);
            SetButtonColor(toggleSplitButton, _splitMod != null);
        }
        
        private void SetButtonColor(Button button, bool isActive) {
            if (button == null) 
                return;
            
            var colors = button.colors;
            
            colors.normalColor = isActive 
                ? activeButtonColor 
                : defaultButtonColor;
            
            colors.highlightedColor = isActive 
                ? activeButtonColor 
                : defaultButtonColor;
            
            colors.selectedColor = isActive 
                ? activeButtonColor 
                : defaultButtonColor;
            
            button.colors = colors;
        }
        
        private void UpdateStatusText() {
            if (statusText == null || _fireball == null) 
                return;
            
            var projectile = _fireball.Behaviours.GetBehaviour<ProjectileBehaviour>();
            var fire = _fireball.Behaviours.GetBehaviour<FireBehaviour>();
            var duration = _fireball.Behaviours.GetBehaviour<DurationBehaviour>();
            
            var count = (int)projectile.Stats.GetValue("projectile_count");
            var speed = projectile.Stats.GetValue("projectile_speed");
            var damage = fire.Stats.GetValue("fire_damage");
            var igniteChance = fire.Stats.GetValue("ignite_chance");
            var igniteDuration = duration.GetIgniteDuration();
            var pattern = _circularMod != null 
                ? "Circular" 
                : "Forward";
            
            statusText.text = $@"═══ FIREBALL STATS ═══
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
