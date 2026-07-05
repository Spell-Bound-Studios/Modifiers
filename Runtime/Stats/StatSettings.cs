// Copyright 2026 Spellbound Studio Inc.

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
        private static bool _locked;

        /// <summary>
        /// Internal scale factor. Default 10000 = 4 decimal places.
        /// This provides float-like precision with deterministic int math.
        /// </summary>
        public static int Precision { get; private set; } = 10000;

        /// <summary>
        /// Set decimal precision. Call once at startup before any stats are created.
        /// 2 = hundredths (100 scale), 4 = ten-thousandths (10000 scale, default)
        /// Locked once any conversion has run: re-setting the current precision is a no-op, changing it throws.
        /// </summary>
        public static void SetDecimalPrecision(int decimalPlaces) {
            var precision = (int)System.Math.Pow(10, decimalPlaces);

            if (precision == Precision)
                return;

            if (_locked) {
                throw new System.InvalidOperationException(
                    $"Stat precision is locked at {Precision} because stat conversions have already run. " +
                    "Set it once at startup before any stats are created.");
            }

            Precision = precision;
        }

        public static int ToInternal(float value) {
            _locked = true;

            return (int)System.Math.Round(value * (double)Precision, System.MidpointRounding.AwayFromZero);
        }

        public static float ToExternal(int value) {
            _locked = true;

            return (float)value / Precision;
        }
    }
}