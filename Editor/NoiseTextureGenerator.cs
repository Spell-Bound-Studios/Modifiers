using System.IO;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Stats.Editor
{
    public class NoiseTextureGenerator : EditorWindow
    {
        [MenuItem("Tools/Generate Noise Texture")]
        private static void GenerateNoise()
        {
            const int size = 512;
            var noise = new Texture2D(size, size, TextureFormat.RGBA32, false);

            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
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

            Debug.Log("Noise texture created at Assets/BeamNoise.png");
        }
    }
}