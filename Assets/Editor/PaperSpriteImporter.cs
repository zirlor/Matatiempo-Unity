using System.IO;
using UnityEditor;
using UnityEngine;

namespace Matatiempo.EditorTools
{
    public static class PaperSpriteImporter
    {
        private const string PaperStyleResources = "Assets/Resources/Sprites/UI/PaperStyle";
        private const string PaperStyleSource = "Assets/Sprites/UI/PaperStyle";

        [MenuItem("Matatiempo/Configurar Sprites Paper UI")]
        public static void ConfigurePaperSprites()
        {
            int count = ConfigureFolder(PaperStyleResources);
            count += ConfigureFolder(PaperStyleSource);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Matatiempo] {count} sprites paper configurados como Sprite.");
        }

        private static int ConfigureFolder(string folder)
        {
            int count = 0;
            string[] files = Directory.GetFiles(folder, "*.png");
            for (int i = 0; i < files.Length; i++)
            {
                string path = files[i].Replace('\\', '/');
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
                count++;
            }
            return count;
        }
    }
}
