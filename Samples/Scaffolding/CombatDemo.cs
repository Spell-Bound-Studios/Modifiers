// Copyright 2026 Spellbound Studio Inc.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample simulation: spawns rings of <see cref="EnemyController"/>s around the player, orbits them, and
    /// respawns them. Pure stage — all UI lives in <see cref="DemoHud"/> (UI Toolkit). Exposes the player, the
    /// enemies, and the controls the HUD drives.
    /// </summary>
    public sealed class CombatDemo : MonoBehaviour {
        [Header("Scene"), SerializeField] private PlayerController player;
        [SerializeField] private GameObject enemyPrefab;

        [Header("Rings"), SerializeField] private float innerRingDistance = 5f;
        [SerializeField] private float outerRingDistance = 10f;
        [SerializeField] private bool autoRespawn = true;

        [Header("Movement"), SerializeField] private float moveSpeed = 2f;

        private readonly List<EnemyController> _innerEnemies = new();
        private readonly List<EnemyController> _outerEnemies = new();

        private int _innerEnemyCount = 7;
        private int _outerEnemyCount = 14;
        private float _radiusJitter;
        private bool _enemiesMove;

        public PlayerController Player => player;
        public int InnerCount => _innerEnemyCount;
        public int OuterCount => _outerEnemyCount;
        public bool EnemiesMove { get => _enemiesMove; set => _enemiesMove = value; }
        public bool AutoRespawn { get => autoRespawn; set => autoRespawn = value; }

        public IEnumerable<EnemyController> Enemies {
            get {
                foreach (var enemy in _innerEnemies)
                    if (enemy != null)
                        yield return enemy;

                foreach (var enemy in _outerEnemies)
                    if (enemy != null)
                        yield return enemy;
            }
        }

        public int AliveCount {
            get {
                var count = 0;

                foreach (var enemy in Enemies)
                    if (!enemy.IsDead)
                        count++;

                return count;
            }
        }

        private void Start() => SpawnAllEnemies();

        private void Update() {
            if (_enemiesMove)
                MoveEnemiesAroundPlayer();
        }

        public void Cast() => player.CastFireball();

        public void RespawnAll() {
            foreach (var enemy in _innerEnemies)
                if (enemy != null && enemy.IsDead)
                    enemy.Respawn();

            foreach (var enemy in _outerEnemies)
                if (enemy != null && enemy.IsDead)
                    enemy.Respawn();
        }

        public void SetInnerCount(int count) {
            if (count == _innerEnemyCount)
                return;

            _innerEnemyCount = count;
            RespawnRing(_innerEnemies, _innerEnemyCount, innerRingDistance, "Inner");
        }

        public void SetOuterCount(int count) {
            if (count == _outerEnemyCount)
                return;

            _outerEnemyCount = count;
            RespawnRing(_outerEnemies, _outerEnemyCount, outerRingDistance, "Outer");
        }

        public void SetRadiusJitter(float jitter) {
            _radiusJitter = jitter;
            ApplyJitterToRings();
        }

        #region Spawning

        private void SpawnAllEnemies() {
            if (enemyPrefab == null) {
                Debug.LogError("[CombatDemo] Enemy prefab not assigned!");

                return;
            }

            if (player == null) {
                Debug.LogError("[CombatDemo] Player not assigned!");

                return;
            }

            if (!player.gameObject.scene.IsValid()) {
                Debug.LogError($"[CombatDemo] 'player' ({player.name}) is a PREFAB ASSET, not a scene instance — " +
                        "its Awake never runs, it's invisible, and reflect/health silently no-op against it. " +
                        "Drag the player from the HIERARCHY into the Player field, not the prefab from the Project.");

                return;
            }

            SpawnRing(_innerEnemies, _innerEnemyCount, innerRingDistance, "Inner");
            SpawnRing(_outerEnemies, _outerEnemyCount, outerRingDistance, "Outer");
        }

        private void SpawnRing(List<EnemyController> enemies, int count, float distance, string prefix) {
            enemies.Clear();
            var playerPos = player.transform.position;

            for (var i = 0; i < count; i++) {
                var angle = 360f / count * i * Mathf.Deg2Rad;
                var jitteredDistance = distance + Random.Range(-_radiusJitter, _radiusJitter);
                var offset = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * jitteredDistance;
                var spawnPos = playerPos + offset;

                var enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                enemyObj.name = $"{prefix}_Enemy_{i + 1}";
                enemyObj.transform.LookAt(new Vector3(playerPos.x, enemyObj.transform.position.y, playerPos.z));

                var enemy = enemyObj.GetComponent<EnemyController>();

                if (enemy == null) {
                    Debug.LogError("[CombatDemo] enemyPrefab has no EnemyController component.");
                    Destroy(enemyObj);

                    continue;
                }

                enemy.OnDeath += OnEnemyDeath;
                enemies.Add(enemy);
            }
        }

        private void RespawnRing(List<EnemyController> enemies, int count, float distance, string prefix) {
            foreach (var enemy in enemies)
                if (enemy != null)
                    Destroy(enemy.gameObject);

            SpawnRing(enemies, count, distance, prefix);
        }

        private void OnEnemyDeath(EnemyController enemy) {
            if (autoRespawn)
                StartCoroutine(RespawnAfterDelay(enemy, 3f));
        }

        private IEnumerator RespawnAfterDelay(EnemyController enemy, float delay) {
            yield return new WaitForSeconds(delay);

            if (enemy != null)
                enemy.Respawn();
        }

        private void ApplyJitterToRings() {
            ApplyJitterToRing(_innerEnemies, innerRingDistance);
            ApplyJitterToRing(_outerEnemies, outerRingDistance);
        }

        private void ApplyJitterToRing(List<EnemyController> enemies, float baseDistance) {
            var playerPos = player.transform.position;
            var count = enemies.Count;

            for (var i = 0; i < count; i++) {
                if (enemies[i] == null)
                    continue;

                var angle = 360f / count * i * Mathf.Deg2Rad;
                var jitteredDistance = baseDistance + Random.Range(-_radiusJitter, _radiusJitter);
                var offset = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * jitteredDistance;

                enemies[i].transform.position = playerPos + offset;
                enemies[i].transform.LookAt(new Vector3(playerPos.x, enemies[i].transform.position.y, playerPos.z));
            }
        }

        #endregion

        #region Movement

        private void MoveEnemiesAroundPlayer() {
            var playerPos = player.transform.position;
            var rotationAmount = moveSpeed * Time.deltaTime;

            RotateRing(_innerEnemies, playerPos, rotationAmount);
            RotateRing(_outerEnemies, playerPos, -rotationAmount);
        }

        private static void RotateRing(List<EnemyController> enemies, Vector3 center, float rotationAmount) {
            foreach (var enemy in enemies) {
                if (enemy == null)
                    continue;

                var direction = enemy.transform.position - center;
                var currentDistance = direction.magnitude;
                direction.y = 0;
                direction = Quaternion.AngleAxis(rotationAmount, Vector3.up) * direction;

                enemy.transform.position = center + direction.normalized * currentDistance;
                enemy.transform.LookAt(new Vector3(center.x, enemy.transform.position.y, center.z));
            }
        }

        #endregion
    }
}
