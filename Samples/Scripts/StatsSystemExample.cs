using UnityEngine;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Demonstrates basic stat registration and modifier creation.
    /// This is the foundation - we register stats once at startup, then reference them by ID.
    /// </summary>
    public class StatSystemExample : MonoBehaviour {
        private StatContainer _exampleRaceOrClassOne;
        private StatContainer _exampleRaceOrClassTwo;

        private void Awake() {
            // This is the baseline stats that will be found in your game. Go absolutely nuts here.
            InitializeStats();
        }
        
        private void Start() {
            // This is an example of how you might implement class or a race with unique base stats.
            CreatePlayerOneStats();
            CreatePlayerTwoStats();
            
            // These are the modifiers that will be found in your game.
            CreateModifiers();
            
            Debug.Log($"Player one base stats created: {_exampleRaceOrClassOne.GetBaseStatList()}");
            Debug.Log($"Player two base stats created: {_exampleRaceOrClassTwo.GetBaseStatList()}");

            ComparePlayerStats();
        }

        /// <summary>
        /// Register all stats we'll use in our game.
        /// </summary>
        private void InitializeStats() {
            StatRegistry.Register("max_life");
            StatRegistry.Register("max_mana");
            StatRegistry.Register("max_stamina");
        }
        
        private void CreatePlayerOneStats() {
            _exampleRaceOrClassOne = new StatContainer();
            
            // Retrieve the stat that you wish to establish a base with.
            var maxLifeId = StatRegistry.GetId("max_life");
            var maxManaId = StatRegistry.GetId("max_mana");
            var maxStaminaId = StatRegistry.GetId("max_stamina");
            
            _exampleRaceOrClassOne.SetBase(maxLifeId, 100f);
            _exampleRaceOrClassOne.SetBase(maxManaId, 20f);
            _exampleRaceOrClassOne.SetBase(maxStaminaId, 20f);
        }
        
        private void CreatePlayerTwoStats() {
            _exampleRaceOrClassTwo = new StatContainer();
            
            // Retrieve the stat that you wish to establish a base with.
            var maxLifeId = StatRegistry.GetId("max_life");
            var maxManaId = StatRegistry.GetId("max_mana");
            var maxStaminaId = StatRegistry.GetId("max_stamina");
            
            _exampleRaceOrClassTwo.SetBase(maxLifeId, 100f);
            _exampleRaceOrClassTwo.SetBase(maxManaId, 10f);
            _exampleRaceOrClassTwo.SetBase(maxStaminaId, 20f);
        }
        
        private void CreateModifiers() {
            var maxLifeId = StatRegistry.GetId("max_life");
            
            var moreLifeModifier = new StatModifier(maxLifeId, ModifierType.More, 2f);
            var increasedLifeModifier = new StatModifier(maxLifeId, ModifierType.Increased, 2f);
            
            // Player one stat modifiers
            _exampleRaceOrClassOne.AddModifier(moreLifeModifier);
            
            // Player two stat modifiers
            _exampleRaceOrClassTwo.AddModifier(increasedLifeModifier);
        }

        /// <summary>
        /// Simple Debug Log comparison of changes.
        /// </summary>
        private void ComparePlayerStats() {
            var healthId = StatRegistry.GetId("max_life");
        
            Debug.Log("=== Player One ===");
            Debug.Log(_exampleRaceOrClassOne.GetCalculatedStatList());
            Debug.Log("=== Player One Health Analysis ===");
            Debug.Log(_exampleRaceOrClassOne.GetModifierAnalysis(healthId));
            
            Debug.Log("=== Player Two ===");
            Debug.Log(_exampleRaceOrClassTwo.GetCalculatedStatList());
            Debug.Log("=== Player Two Health Analysis ===");
            Debug.Log(_exampleRaceOrClassTwo.GetModifierAnalysis(healthId));
        }
    }
}