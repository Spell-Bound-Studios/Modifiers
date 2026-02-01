using TMPro;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    public class EnemyHealthDisplay : MonoBehaviour {
        private EnemyTarget _enemy;
        private TextMeshPro _healthText;
        private SpriteRenderer _healthBarBackground;
        private SpriteRenderer _healthBarFill;
        
        private const float BarWidth = 1.2f;
        private const float BarHeight = 0.15f;
        
        public void Initialize(EnemyTarget enemy) {
            _enemy = enemy;
            
            CreateHealthBar();
            CreateHealthText();
            UpdateDisplay();
        }
        
        private void CreateHealthBar() {
            // Background
            var bgObj = new GameObject("HealthBarBg");
            bgObj.transform.SetParent(transform);
            bgObj.transform.localPosition = Vector3.zero;
            bgObj.transform.localScale = new Vector3(BarWidth, BarHeight, 1f);
            
            _healthBarBackground = bgObj.AddComponent<SpriteRenderer>();
            _healthBarBackground.sprite = CreateSquareSprite();
            _healthBarBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            _healthBarBackground.sortingOrder = 100;
            
            // Fill
            var fillObj = new GameObject("HealthBarFill");
            fillObj.transform.SetParent(transform);
            fillObj.transform.localPosition = new Vector3(-BarWidth / 2f, 0, -0.01f);
            fillObj.transform.localScale = new Vector3(BarWidth, BarHeight * 0.8f, 1f);
            
            _healthBarFill = fillObj.AddComponent<SpriteRenderer>();
            _healthBarFill.sprite = CreateSquareSprite();
            _healthBarFill.color = Color.green;
            _healthBarFill.sortingOrder = 101;
        }
        
        private void CreateHealthText() {
            var textObj = new GameObject("HealthText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = new Vector3(0, 0.2f, 0);
            
            _healthText = textObj.AddComponent<TextMeshPro>();
            _healthText.fontSize = 2f;
            _healthText.alignment = TextAlignmentOptions.Center;
            _healthText.color = Color.white;
            _healthText.sortingOrder = 102;
        }
        
        private Sprite CreateSquareSprite() {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
        
        public void UpdateDisplay() {
            if (_enemy == null)
                return;
            
            var healthPercent = _enemy.CurrentHealth / _enemy.MaxHealth;
            healthPercent = Mathf.Clamp01(healthPercent);
            
            // Update fill scale and position
            var fillScale = _healthBarFill.transform.localScale;
            fillScale.x = BarWidth * healthPercent;
            _healthBarFill.transform.localScale = fillScale;
            
            // Anchor fill to left side
            var fillPos = _healthBarFill.transform.localPosition;
            fillPos.x = -BarWidth / 2f + (fillScale.x / 2f);
            _healthBarFill.transform.localPosition = fillPos;
            
            // Update color based on health
            _healthBarFill.color = healthPercent > 0.5f 
                ? Color.Lerp(Color.yellow, Color.green, (healthPercent - 0.5f) * 2f)
                : Color.Lerp(Color.red, Color.yellow, healthPercent * 2f);
            
            // Update text
            _healthText.text = $"{_enemy.CurrentHealth:F0} / {_enemy.MaxHealth:F0}";
            
            // Hide if dead
            gameObject.SetActive(!_enemy.IsDead);
        }
    }
}