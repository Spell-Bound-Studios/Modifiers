// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    public class StatDemo : MonoBehaviour {
        [SerializeField] private StatDatabase statDatabase;

        private void Start() => statDatabase.RegisterAll(true);
    }
}