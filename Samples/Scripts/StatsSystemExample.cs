using UnityEngine;

namespace Spellbound.Stats.Samples
{
    /// <summary>
    ///     Demonstrates basic stat registration and modifier creation.
    ///     This is the foundation - we register stats once at startup, then reference them by ID.
    /// </summary>
    public class StatSystemExample : MonoBehaviour
    {
        private void Start()
        {
            InitializeStats();
            CreateSomeModifiers();
        }

        /// <summary>
        ///     Register all stats we'll use in our game.
        ///     Call this once at startup - registration is idempotent so it's safe to call multiple times.
        /// </summary>
        private void InitializeStats()
        {
            // Core combat stats
            var physDmg = StatRegistry.Register("physical_damage");
            var fireDmg = StatRegistry.Register("fire_damage");
            var attackSpeed = StatRegistry.Register("attack_speed");

            // Defensive stats
            var maxLife = StatRegistry.Register("max_life");
            var fireRes = StatRegistry.Register("fire_resistance");

            Debug.Log($"Registered {physDmg} as physical_damage");
            Debug.Log($"Can lookup later: {StatRegistry.GetId("physical_damage")}");
        }

        /// <summary>
        ///     Example of creating different modifier types.
        ///     In a real game, these would come from items, passives, buffs, etc.
        /// </summary>
        private void CreateSomeModifiers()
        {
            var physDmgId = StatRegistry.GetId("physical_damage");
            var attackSpeedId = StatRegistry.GetId("attack_speed");

            // Flat modifier: "+10 to physical damage"
            var flatMod = new StatModifier(physDmgId, ModifierType.Flat, 10f);

            // Increased modifier: "30% increased physical damage"
            var increasedMod = new StatModifier(physDmgId, ModifierType.Increased, 30f);

            // More modifier: "40% more attack speed"
            var moreMod = new StatModifier(attackSpeedId, ModifierType.More, 40f);

            Debug.Log($"Created flat modifier: +{flatMod.Value} to {StatRegistry.GetName(flatMod.StatId)}");
            Debug.Log(
                $"Created increased modifier: {increasedMod.Value}% increased {StatRegistry.GetName(increasedMod.StatId)}");
            Debug.Log($"Created more modifier: {moreMod.Value}% more {StatRegistry.GetName(moreMod.StatId)}");

            // TODO: We don't have a container to hold these yet or a calculator to apply them
            // That's our next step!
        }
    }
}