using System;
using System.Collections;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    public sealed class EnemyTarget : MonoBehaviour, IHasStats {
        [Header("Stats")]
        [SerializeField] private float baseHealth = 100f;
        [SerializeField] private float baseMana = 50f;
        
        [Header("Visual")]
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color ignitedColor = Color.red;
        [SerializeField] private Color chilledColor = new(0.2f, 0.4f, 0.8f);
        [SerializeField] private Color deadColor = Color.gray;
        
        [Header("Health Display")]
        [SerializeField] private Vector3 healthBarOffset = new(0, 2f, 0);
        
        [Header("DoT Settings")]
        [SerializeField] private float dotTickRate = 0.5f;
        
        private StatContainer _stats;
        private Coroutine _igniteCoroutine;
        private Coroutine _chillCoroutine;
        private EnemyHealthDisplay _healthDisplay;
        
        private float _currentHealth;
        private float _currentMana;
        
        public StatContainer Stats => _stats;
        public bool IsIgnited { get; private set; }
        public bool IsChilled { get; private set; }
        public bool IsDead { get; private set; }
        
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _stats.GetValue("health");
        public float CurrentMana => _currentMana;
        public float MaxMana => _stats.GetValue("mana");
        
        public event Action<EnemyTarget> OnDeath;
        public event Action<EnemyTarget, float, string> OnDamageTaken;

        private void Awake() {
            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();

            if (targetRenderer != null)
                targetRenderer.material.color = defaultColor;

            gameObject.tag = "Enemy";
            
            InitializeStats();
            CreateHealthDisplay();
        }
        
        private void InitializeStats() {
            _stats = new StatContainer();
            _stats.SetBase("health", baseHealth);
            _stats.SetBase("mana", baseMana);
            
            _currentHealth = MaxHealth;
            _currentMana = MaxMana;
        }
        
        private void CreateHealthDisplay() {
            var displayObj = new GameObject("HealthDisplay");
            displayObj.transform.SetParent(transform);
            displayObj.transform.localPosition = healthBarOffset;
            
            _healthDisplay = displayObj.AddComponent<EnemyHealthDisplay>();
            _healthDisplay.Initialize(this);
        }
        
        private void Update() {
            if (_healthDisplay != null && Camera.main != null)
                _healthDisplay.transform.rotation = Camera.main.transform.rotation;
        }

        public void TakeDamage(float damage, string damageType) {
            if (IsDead)
                return;
            
            _currentHealth -= damage;
            
            Debug.Log($"[{gameObject.name}] Took {damage:F1} {damageType} damage! ({_currentHealth:F0}/{MaxHealth:F0})");
            
            OnDamageTaken?.Invoke(this, damage, damageType);
            _healthDisplay?.UpdateDisplay();
            
            if (_currentHealth <= 0) {
                _currentHealth = 0;
                Die();
            }
        }
        
        public void Heal(float amount) {
            if (IsDead)
                return;
            
            _currentHealth = Mathf.Min(_currentHealth + amount, MaxHealth);
            _healthDisplay?.UpdateDisplay();
        }
        
        private void Die() {
            IsDead = true;
            
            if (_igniteCoroutine != null)
                StopCoroutine(_igniteCoroutine);
            
            if (_chillCoroutine != null)
                StopCoroutine(_chillCoroutine);
            
            IsIgnited = false;
            IsChilled = false;
            
            if (targetRenderer != null)
                targetRenderer.material.color = deadColor;
            
            Debug.Log($"[{gameObject.name}] DIED!");
            
            OnDeath?.Invoke(this);
            
            gameObject.SetActive(false);
        }
        
        public void Respawn() {
            gameObject.SetActive(true);
            
            IsDead = false;
            _currentHealth = MaxHealth;
            _currentMana = MaxMana;
            
            if (targetRenderer != null)
                targetRenderer.material.color = defaultColor;
            
            _healthDisplay?.UpdateDisplay();
            
            Debug.Log($"[{gameObject.name}] Respawned!");
        }

        public void ApplyIgnite(float duration, float damagePerSecond) {
            if (IsDead)
                return;
            
            if (_igniteCoroutine != null)
                StopCoroutine(_igniteCoroutine);

            _igniteCoroutine = StartCoroutine(IgniteRoutine(duration, damagePerSecond));
        }

        private IEnumerator IgniteRoutine(float duration, float damagePerSecond) {
            IsIgnited = true;
            UpdateColor();
            
            var damagePerTick = damagePerSecond * dotTickRate;
            var elapsed = 0f;

            Debug.Log($"[{gameObject.name}] IGNITED for {duration}s! ({damagePerSecond:F1} dps)");

            while (elapsed < duration && !IsDead) {
                yield return new WaitForSeconds(dotTickRate);
                elapsed += dotTickRate;
                
                TakeDamage(damagePerTick, "fire (ignite)");
            }

            IsIgnited = false;
            UpdateColor();

            if (!IsDead)
                Debug.Log($"[{gameObject.name}] Ignite expired.");
            
            _igniteCoroutine = null;
        }

        public void ApplyChill(float duration) {
            if (IsDead)
                return;
            
            if (_chillCoroutine != null)
                StopCoroutine(_chillCoroutine);

            _chillCoroutine = StartCoroutine(ChillRoutine(duration));
        }

        private IEnumerator ChillRoutine(float duration) {
            IsChilled = true;
            UpdateColor();

            Debug.Log($"[{gameObject.name}] CHILLED for {duration}s!");

            yield return new WaitForSeconds(duration);

            IsChilled = false;
            UpdateColor();

            if (!IsDead)
                Debug.Log($"[{gameObject.name}] Chill expired.");
            
            _chillCoroutine = null;
        }
        
        private void UpdateColor() {
            if (targetRenderer == null || IsDead)
                return;
            
            if (IsIgnited)
                targetRenderer.material.color = ignitedColor;
            else if (IsChilled)
                targetRenderer.material.color = chilledColor;
            else
                targetRenderer.material.color = defaultColor;
        }
    }
}