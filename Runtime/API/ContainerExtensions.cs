namespace Spellbound.Stats {
    /// <summary>
    /// General helper methods that wrap capability in a single location.
    /// </summary>
    public static class ContainerExtensions {
        #region Stat Container Extensions

        public static void SetBase(this StatContainer container, string statName, float value) {
            container.SetBase(StatRegistry.Register(statName), value);
        }
    
        public static float GetValue(this StatContainer container, string statName) {
            return container.GetValue(StatRegistry.Register(statName));
        }
    
        public static void AddFlat(this StatContainer container, string statName, float value, string uniqueId = null) {
            container.AddModifier(new StatModifier(
                StatRegistry.Register(statName),
                ModifierType.Flat,
                value,
                uniqueId
            ));
        }
    
        public static void AddIncreased(this StatContainer container, string statName, float percent, string uniqueId = null) {
            container.AddModifier(new StatModifier(
                StatRegistry.Register(statName),
                ModifierType.Increased,
                percent,
                uniqueId
            ));
        }
    
        public static void AddMore(this StatContainer container, string statName, float percent, string uniqueId = null) {
            container.AddModifier(new StatModifier(
                StatRegistry.Register(statName),
                ModifierType.More,
                percent,
                uniqueId
            ));
        }

        #endregion
    }
}