using System.IO;
using UnityEditor;
using UnityEngine;

namespace TrucoVR.Editor
{
    public static class ConfigureSpanishCardSprites
    {
        [MenuItem("TrucoVR/Configuración/Convertir cartas JPG a Sprite")]
        public static void ConfigureCards()
        {
            string[] folders =
            {
                "Assets/_TrucoVR/Art/Textures/jpg",
                "Assets/_TrucoVR/Resources/SpanishCards"
            };

            int configuredCount = 0;

            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                    continue;

                string[] assets = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });

                foreach (string guid in assets)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                    if (!assetPath.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    if (importer == null)
                        continue;

                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 1525;
                    importer.mipmapEnabled = false;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.alphaIsTransparency = false;
                    importer.SaveAndReimport();
                    configuredCount++;
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[TrucoVR] Se configuraron {configuredCount} imágenes JPG como Sprite.");
        }
    }
}
