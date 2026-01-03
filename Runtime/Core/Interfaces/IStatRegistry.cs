// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Stats {
    /// <summary>
    /// Implement this if you want to create a registration source for stats, tags, or custom data.
    /// Our bootstrap will call RegisterAll() during initialization, or you can call it yourself with your
    /// own implementation.
    /// </summary>
    public interface IStatRegistry {
        /// <summary>
        /// Register all entries in this registry at runtime.
        /// Called during game initialization.
        /// </summary>
        /// <param name="verbose">If true, log detailed information about what was registered</param>
        void RegisterAll(bool verbose);
    }
}