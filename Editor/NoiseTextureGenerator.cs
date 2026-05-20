// Copyright 2026 Spellbound Studio Inc.

using System.IO;
using Spellbound.Core.Logging;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Modifiers.Editor {
    /// <summary>
    /// One-off menu item that bakes a 512x512 Perlin-noise PNG to <c>Assets/BeamNoise.png</c>. The output is
    /// the noise texture used by the beam visual shader in the samples.
    /// </summary>
    /// <remarks>
    /// SUPERFLUOUS for the library itself — this is a sample-only Editor tool that has nothing to do with
    /// stats, modifiers, behaviours, or any other lib concern. It also writes to the OUTER game project's
    /// <c>Assets/</c> root, which means dropping the package into a clean project produces a file outside the
    /// package. Move it to <c>Samples/Editor/</c> (and gate it behind a Samples Editor asmdef) or delete it
    /// once the beam sample no longer needs the regeneration step.
    /// </remarks>
    public class NoiseTextureGenerator : EditorWindow {
        [MenuItem("Tools/Generate Noise Texture")]
        private static void GenerateNoise() {
            const int size = 512;
            var noise = new Texture2D(size, size, TextureFormat.RGBA32, false);

            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++) {
                var xCoord = (float)x / size * 5f;
                var yCoord = (float)y / size * 5f;
                var sample = Mathf.PerlinNoise(xCoord, yCoord);
                noise.SetPixel(x, y, new Color(sample, sample, sample, 1f));
            }

            noise.Apply();

            var bytes = noise.EncodeToPNG();
            var path = Application.dataPath + "/BeamNoise.png";
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();

            Log.Info("Noise texture created at Assets/BeamNoise.png");
        }
    }
}