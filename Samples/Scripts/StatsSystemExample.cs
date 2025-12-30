using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Complete demonstration of the stat system showing:
    /// 1. Two players with identical base stats
    /// 2. Both receive the same numeric modifiers
    /// 3. Player 2 has Fireball equipped
    /// 4. Fireball is affected by player's global stats
    /// 5. Fireball receives behavioural modifiers (projectiles, on-kill, conversion)
    /// </summary>
    public class StatSystemExample : MonoBehaviour {
        private PlayerTemplate _playerOne;
        private PlayerTemplate _playerTwo;
        private FireballSkill _fireball;

        private void Awake() {
            InitializeRegistries();
            RegisterSkills();
        }

        private void Start() {
            CreatePlayers();
            ApplyPlayerModifiers();
            ShowPlayerResults();
            
            Debug.Log("\n" + new string('=', 80) + "\n");
            
            CreateAndModifyFireball();
            ShowFireballResults();
            SimulateCombat();
        }

        private void InitializeRegistries() {
            // Player stats
            StatRegistry.Register("strength");
            StatRegistry.Register("intelligence");
            StatRegistry.Register("max_life");
            StatRegistry.Register("max_mana");
            
            // Skill stats
            StatRegistry.Register("base_damage");
            StatRegistry.Register("cast_time");
            StatRegistry.Register("mana_cost");
            StatRegistry.Register("crit_chance");

            // Tags
            TagRegistry.Register("Fire");
            TagRegistry.Register("Cold");
            TagRegistry.Register("Spell");
            TagRegistry.Register("Projectile");

            Debug.Log("=== REGISTRIES INITIALIZED ===\n");
        }

        private void RegisterSkills() {
            SkillRegistry.Register<FireballSkill>();
        }

        /// <summary>
        /// Create two players with identical base stats.
        /// </summary>
        private void CreatePlayers() {
            _playerOne = new PlayerTemplate("Player One");
            _playerTwo = new PlayerTemplate("Player Two");

            // Identical base stats
            foreach (var player in new[] { _playerOne, _playerTwo }) {
                player.Stats.SetBase(StatRegistry.GetId("strength"), 20f);
                player.Stats.SetBase(StatRegistry.GetId("intelligence"), 30f);
                player.Stats.SetBase(StatRegistry.GetId("max_life"), 100f);
                player.Stats.SetBase(StatRegistry.GetId("max_mana"), 80f);
            }

            Debug.Log("=== PLAYERS CREATED WITH IDENTICAL BASE STATS ===");
            Debug.Log($"{_playerOne.Name}:\n{_playerOne.Stats.GetBaseStatList()}");
            Debug.Log($"\n{_playerTwo.Name}:\n{_playerTwo.Stats.GetBaseStatList()}\n");
        }

        /// <summary>
        /// Apply identical modifiers to both players.
        /// These will affect their stats AND any skills they use.
        /// </summary>
        private void ApplyPlayerModifiers() {
            foreach (var player in new[] { _playerOne, _playerTwo }) {
                // Modifier 1: +15 Intelligence (flat)
                var intSource = new SimpleModifierSource(1001, "+15 Intelligence");
                intSource.AddModifier(new NumericModifier(
                    modifierId: 1,
                    requiredTags: new HashSet<int>(),
                    statModifier: new StatModifier(
                        StatRegistry.GetId("intelligence"),
                        ModifierType.Flat,
                        15f,
                        sourceId: 1001
                    )
                ));
                player.AddModifierSource(intSource);

                // Modifier 2: 40% increased Spell Damage (global - affects ALL spells)
                var spellDamageSource = new SimpleModifierSource(1002, "40% increased Spell Damage");
                spellDamageSource.AddModifier(new NumericModifier(
                    modifierId: 2,
                    requiredTags: new HashSet<int> { TagRegistry.GetId("Spell") },
                    statModifier: new StatModifier(
                        StatRegistry.GetId("base_damage"),
                        ModifierType.Increased,
                        40f,
                        sourceId: 1002
                    )
                ));
                player.AddModifierSource(spellDamageSource);

                // Modifier 3: 25% more Fire Damage (global - affects ALL fire skills)
                var fireDamageSource = new SimpleModifierSource(1003, "25% more Fire Damage");
                fireDamageSource.AddModifier(new NumericModifier(
                    modifierId: 3,
                    requiredTags: new HashSet<int> { TagRegistry.GetId("Fire") },
                    statModifier: new StatModifier(
                        StatRegistry.GetId("base_damage"),
                        ModifierType.More,
                        25f,
                        sourceId: 1003
                    )
                ));
                player.AddModifierSource(fireDamageSource);
            }

            // Apply modifiers to player stats
            foreach (var player in new[] { _playerOne, _playerTwo }) {
                foreach (var modifier in player.GetAllModifiers()) {
                    modifier.Apply(player);
                }
            }

            Debug.Log("=== MODIFIERS APPLIED TO BOTH PLAYERS ===");
            Debug.Log("1. +15 Intelligence (Flat)");
            Debug.Log("2. 40% increased Spell Damage (Global)");
            Debug.Log("3. 25% more Fire Damage (Global)\n");
        }

        private void ShowPlayerResults() {
            Debug.Log("=== PLAYER ONE FINAL STATS ===");
            Debug.Log(_playerOne.Stats.GetCalculatedStatList());
            Debug.Log($"\n{_playerOne.Stats.GetModifierAnalysis(StatRegistry.GetId("intelligence"))}");

            Debug.Log("\n=== PLAYER TWO FINAL STATS ===");
            Debug.Log(_playerTwo.Stats.GetCalculatedStatList());
            Debug.Log($"\n{_playerTwo.Stats.GetModifierAnalysis(StatRegistry.GetId("intelligence"))}\n");
        }

        /// <summary>
        /// Player Two equips Fireball and it gets modified by:
        /// 1. Player's global stat modifiers (40% increased spell, 25% more fire)
        /// 2. Skill-specific behavioural modifiers
        /// </summary>
        private void CreateAndModifyFireball() {
            Debug.Log("=== PLAYER TWO EQUIPS FIREBALL ===\n");
            
            _fireball = (FireballSkill)SkillRegistry.Create("Fireball");

            Debug.Log("FIREBALL BASE STATS:");
            Debug.Log(_fireball.Stats.GetBaseStatList());
            Debug.Log($"\nFIREBALL BASE BEHAVIOURS:");
            Debug.Log($"Projectile Count: {_fireball.GetBehaviour<ProjectileBehaviour>().ProjectileCount}");
            Debug.Log($"Can Ignite: {_fireball.GetBehaviour<FireBehaviour>().CanIgnite}\n");

            // Apply player's global modifiers to Fireball
            Debug.Log("Applying Player Two's global modifiers to Fireball...");
            _fireball.ApplyModifiers(_playerTwo.GetAllModifiers());

            // Add skill-specific behavioural modifiers
            Debug.Log("\n=== ADDING SKILL-SPECIFIC MODIFIERS ===");
            
            // Modifier 1: Fire 2 Additional Projectiles
            var projSource = new SimpleModifierSource(2001, "+2 Projectiles");
            projSource.AddModifier(new AdditionalProjectilesModifier(
                modifierId: 4,
                requiredTags: new HashSet<int> { TagRegistry.GetId("Projectile") },
                additionalProjectiles: 2
            ));
            _fireball.ApplyModifiers(projSource.GetModifiers());

            // Modifier 2: Gain Frenzy Charge on Kill
            var frenzySource = new SimpleModifierSource(2002, "Frenzy on Kill");
            frenzySource.AddModifier(new OnKillFrenzyChargeModifier(
                modifierId: 5,
                requiredTags: new HashSet<int>()
            ));
            _fireball.ApplyModifiers(frenzySource.GetModifiers());

            // Modifier 3: 25% of Fire Damage converted to Cold
            var conversionSource = new SimpleModifierSource(2003, "25% Fire to Cold");
            conversionSource.AddModifier(new FireToColdConversionModifier(
                modifierId: 6,
                requiredTags: new HashSet<int> { TagRegistry.GetId("Fire") },
                conversionPercent: 25f
            ));
            _fireball.ApplyModifiers(conversionSource.GetModifiers());
        }

        private void ShowFireballResults() {
            Debug.Log("\n=== FIREBALL FINAL STATS ===");
            Debug.Log(_fireball.Stats.GetCalculatedStatList());

            Debug.Log("\n=== DAMAGE CALCULATION BREAKDOWN ===");
            Debug.Log(_fireball.Stats.GetModifierAnalysis(StatRegistry.GetId("base_damage")));
            Debug.Log($"\nExpected: 10 (base) * 1.4 (40% increased) * 1.25 (25% more) = {10f * 1.4f * 1.25f}");

            Debug.Log("\n=== FIREBALL BEHAVIOURS ===");
            if (_fireball.HasBehaviour<ProjectileBehaviour>()) {
                var proj = _fireball.GetBehaviour<ProjectileBehaviour>();
                Debug.Log($"Projectile Count: {proj.ProjectileCount} (was 1, now 3)");
            }
            
            if (_fireball.HasBehaviour<DamageConversionBehaviour>()) {
                var conv = _fireball.GetBehaviour<DamageConversionBehaviour>();
                Debug.Log($"Damage Conversion: {conv.ConversionPercent}% {conv.FromDamageType} → {conv.ToDamageType}");
            }

            Debug.Log($"\nFireball Tags: {string.Join(", ", GetTagNames(_fireball.Tags))}");
            Debug.Log("(Notice Cold tag was added by conversion modifier!)\n");
        }

        private void SimulateCombat() {
            Debug.Log("=== SIMULATING COMBAT ===");
            _fireball.Execute();

            Debug.Log("\nSimulating kills to trigger Frenzy Charge modifier...");

            if (!_fireball.HasBehaviour<ProjectileBehaviour>()) 
                return;

            var proj = _fireball.GetBehaviour<ProjectileBehaviour>();
            proj.TriggerKill("Enemy 1");
            proj.TriggerKill("Enemy 2");
            proj.TriggerKill("Enemy 3");
        }

        private List<string> GetTagNames(HashSet<int> tagIds) {
            var names = new List<string>();
            foreach (var id in tagIds)
                names.Add(TagRegistry.GetName(id));
            
            return names;
        }
    }
}