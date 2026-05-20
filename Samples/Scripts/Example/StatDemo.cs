// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample MonoBehaviour: calls <see cref="StatDatabase.RegisterAll"/> with strict validation on Start.
    /// The code-path equivalent of the drop-in <see cref="StatDatabaseLoader"/>, kept around because the
    /// README's getting-started snippets reference the two-line manual registration form.
    /// </summary>
    /// <remarks>
    /// REDUNDANT in practice — <see cref="StatDatabaseLoader"/> does the same job (with a Resources fallback,
    /// strict-validation toggle, and an earlier <c>DefaultExecutionOrder</c>) and is the documented "drop-in
    /// component" path. Keep this only as a documentation artifact for the code-first README example; if the
    /// README adopts the loader as the canonical path, delete this file.
    /// </remarks>
    public class StatDemo : MonoBehaviour {
        [SerializeField] private StatDatabase statDatabase;

        private void Start() => statDatabase.RegisterAll(true);
    }
}