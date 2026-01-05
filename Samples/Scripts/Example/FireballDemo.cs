// Copyright 2025 Spellbound Studio Inc.

using Spellbound.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Demo controller showing the modifier system in action.
    /// </summary>
    public class FireballDemo : MonoBehaviour {
        [Header("Skill Setup")]
        [SerializeField] private ObjectPreset fireballSkill;
        [SerializeField] private GameObject projectilePrefab;
        
        [Header("Scene Setup")]
        [SerializeField] private Transform player;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private int enemyCount = 7;
        [SerializeField] private float enemyDistance = 5f;
        
        [Header("UI Buttons")]
        [SerializeField] private Button castButton;
        [SerializeField] private Button toggleProjectileCountButton;
        [SerializeField] private Button toggleCircularButton;
        [SerializeField] private Button toggleDurationButton;
        
        [Header("UI Status")]
        [SerializeField] private TMP_Text statusText;
        
        [Header("Button Colors")]
        [SerializeField] private Color defaultButtonColor = Color.white;
        [SerializeField] private Color activeButtonColor = Color.green;
        
        private Skill _fireball;
        private EnemyTarget[] _enemies;
        
        // Modifier instances (null when not applied)
        private AddProjectileModifier _projectileCountMod;
        private CircularProjectileModifier _circularMod;
        private IncreasedDurationModifier _durationMod;
        
        private void Start() {
            SpawnEnemies();
            InitializeSkill();
            SetupButtons();
            UpdateStatusText();
            UpdateButtonColors();
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
            
            _enemies = new EnemyTarget[enemyCount];
            var playerPos = player.position;
            
            for (var i = 0; i < enemyCount; i++) {
                Vector3 spawnPos;
                
                if (enemyCount == 1) {
                    // Single enemy directly in front
                    spawnPos = playerPos + Vector3.forward * enemyDistance;
                } else {
                    // Evenly distribute in a circle
                    // Sin for X, Cos for Z means i=0 is forward (+Z)
                    var angle = (360f / enemyCount) * i * Mathf.Deg2Rad;
                    var offset = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * enemyDistance;
                    spawnPos = playerPos + offset;
                }
                
                var enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                enemyObj.name = $"Enemy_{i + 1}";
                
                // Face the player
                enemyObj.transform.LookAt(new Vector3(playerPos.x, enemyObj.transform.position.y, playerPos.z));
                
                _enemies[i] = enemyObj.GetComponent<EnemyTarget>();
                
                if (_enemies[i] == null)
                    Debug.LogError($"Enemy prefab missing EnemyTarget component!");
            }
            
            Debug.Log($"[Demo] Spawned {enemyCount} enemies in a circle (radius: {enemyDistance})");
        }
        
        private void InitializeSkill() {
            if (!fireballSkill.TryGetModule<SkillModule>(out var skillModule)) {
                Debug.LogError("No SkillModule found on Fireball preset!");
                return;
            }
            
            _fireball = skillModule.CreateSkill();
            
            // Wire up projectile spawning
            var projectileBehaviour = _fireball.GetBehaviour<ProjectileBehaviour>();
            if (projectileBehaviour != null) {
                projectileBehaviour.ProjectilePrefab = projectilePrefab;
                projectileBehaviour.OnProjectileHit = OnProjectileHit;
            }
            
            Debug.Log($"[Demo] Fireball skill initialized: {_fireball.Name}");
        }
        
        private void SetupButtons() {
            castButton?.onClick.AddListener(CastFireball);
            toggleProjectileCountButton?.onClick.AddListener(ToggleProjectileCount);
            toggleCircularButton?.onClick.AddListener(ToggleCircular);
            toggleDurationButton?.onClick.AddListener(ToggleDuration);
        }
        
        private void CastFireball() {
            if (_fireball == null) return;
            
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("CASTING FIREBALL");
            Debug.Log("═══════════════════════════════════════");
            
            _fireball.Events.Trigger(new CastEvent(
                _fireball,
                player.position,
                player.forward
            ));
        }
        
        private void OnProjectileHit(GameObject target, Vector3 position) {
            _fireball.Events.Trigger(new HitEvent(
                _fireball,
                _fireball.GetBehaviour<ProjectileBehaviour>(),
                target,
                position
            ));
        }
        
        #region Toggle Modifiers
        
        private void ToggleProjectileCount() {
            if (_projectileCountMod == null) {
                _projectileCountMod = new AddProjectileModifier();
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
        
        #endregion
        
        private void UpdateButtonColors() {
            SetButtonColor(toggleProjectileCountButton, _projectileCountMod != null);
            SetButtonColor(toggleCircularButton, _circularMod != null);
            SetButtonColor(toggleDurationButton, _durationMod != null);
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
            if (statusText == null || _fireball == null) return;
            
            var projectile = _fireball.GetBehaviour<ProjectileBehaviour>();
            var fire = _fireball.GetBehaviour<FireBehaviour>();
            var duration = _fireball.GetBehaviour<DurationBehaviour>();
            
            var count = (int)projectile.Stats.GetValue(StatRegistry.GetId("projectile_count"));
            var speed = projectile.Stats.GetValue(StatRegistry.GetId("projectile_speed"));
            var damage = fire.Stats.GetValue(StatRegistry.GetId("fire_damage"));
            var igniteChance = fire.Stats.GetValue(StatRegistry.GetId("ignite_chance"));
            var igniteDuration = duration.Stats.GetValue(StatRegistry.GetId("ignite_duration"));
            var pattern = _circularMod != null ? "Circular" : "Forward";
            
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
[{(_durationMod != null ? "X" : " ")}] +50% Duration";
        }
    }
}