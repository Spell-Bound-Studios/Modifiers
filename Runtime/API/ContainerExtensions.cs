namespace Spellbound.Modifiers {
    /// <summary>
    /// General helper methods that wrap capability in a single location.
    /// </summary>
    public static class ContainerExtensions {
        private static StatDatabase _database;
    
        public static void SetDatabase(StatDatabase database) {
            _database = database;
        }
        
        #region Stat Definition Extensions
        
        public static string GetFormattedValue(this StatContainer container, string statName) {
            var value = container.GetValue(statName);
        
            if (_database == null)
                return value.ToString("F0");
        
            var definition = _database.GetDefinition(statName);
            return definition != null 
                ? definition.FormatValue(value) 
                : value.ToString("F0");
        }
    
        public static StatDefinition GetDefinition(this StatContainer container, string statName) {
            return _database?.GetDefinition(statName);
        }
        
        #endregion
        
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