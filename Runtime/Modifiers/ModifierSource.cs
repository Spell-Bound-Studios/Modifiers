// Copyright 2026 Spellbound Studio Inc.

using System.Threading;

namespace Spellbound.Modifiers {
    public static class ModifierSource {
        private static int _next;

        public static uint Next() => (uint)Interlocked.Increment(ref _next);
    }
}
