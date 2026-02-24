namespace Spellbound.Modifiers {
    /// <summary>
    /// Responsible for fixed point integer precision.
    /// </summary>
    /// <remarks>
    /// If the user needs to do complex calculations outside the library then they can leverage the calls below.
    /// var internalDamage = StatSettings.ToInternal(damage);
    /// var internalResist = StatSettings.ToInternal(resistance);
    /// var result = internalDamage * (StatSettings.Precision - internalResist) / StatSettings.Precision;
    /// var finalDamage = StatSettings.ToExternal(result);
    /// </remarks>
    public static class StatSettings {
        /// <summary>
        /// Internal scale factor. Default 10000 = 4 decimal places.
        /// This provides float-like precision with deterministic int math.
        /// </summary>
        public static int Precision { get; private set; } = 10000;
        
        /// <summary>
        /// Set decimal precision. Call once at startup before any stats are created.
        /// 2 = hundredths (100 scale), 4 = ten-thousandths (10000 scale, default)
        /// </summary>
        public static void SetDecimalPrecision(int decimalPlaces) {
            Precision = (int)System.Math.Pow(10, decimalPlaces);
        }
        
        public static int ToInternal(float value) => (int)(value * Precision);
        public static float ToExternal(int value) => (float)value / Precision;
    }
}