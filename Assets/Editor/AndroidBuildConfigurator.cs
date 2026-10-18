using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;

namespace Matatiempo.EditorTools
{
    public static class AndroidBuildConfigurator
    {
        private const string BundleIdentifier = "com.polixgamestudio.matatiempo";
        private const string IconFolder = "Assets/Sprites/AppIcon";
        private const int IconSize = 512;

        [MenuItem("Tools/Matatiempo/Configure Android Build")]
        public static void ConfigureAndroidBuild()
        {
            NamedBuildTarget platform = NamedBuildTarget.Android;

            PlayerSettings.SetApplicationIdentifier(platform, BundleIdentifier);
            PlayerSettings.productName = "Matatiempo: El Ahorcado";
            PlayerSettings.companyName = "Polix Game Studio";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.SetScriptingBackend(platform, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;

            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.SplashScreen.backgroundColor = new Color(0.16f, 0.17f, 0.24f, 1f);
            PlayerSettings.SplashScreen.animationMode = PlayerSettings.SplashScreen.AnimationMode.Static;

            Texture2D background = SaveIconTexture("icon_background.png", RenderBackgroundPixel);
            Texture2D foreground = SaveIconTexture("icon_foreground.png", RenderForegroundPixel);

            PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(platform, AndroidPlatformIconKind.Adaptive);
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i].SetTextures(new[] { background, foreground });
            }
            PlayerSettings.SetPlatformIcons(platform, AndroidPlatformIconKind.Adaptive, icons);

            AssetDatabase.SaveAssets();
            Debug.Log("[Matatiempo] Android configurado: " + BundleIdentifier + " | IL2CPP ARM64 | minSdk 26 | splash e icono adaptativo generados.");
        }

        private static Texture2D SaveIconTexture(string fileName, Func<int, int, Color> paint)
        {
            var texture = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false);
            var pixels = new Color[IconSize * IconSize];
            for (int y = 0; y < IconSize; y++)
            {
                for (int x = 0; x < IconSize; x++)
                {
                    pixels[y * IconSize + x] = paint(x, y);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply(false);
            if (!Directory.Exists(IconFolder))
            {
                Directory.CreateDirectory(IconFolder);
            }
            string path = IconFolder + "/" + fileName;
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Color RenderBackgroundPixel(int x, int y)
        {
            Color top = new Color(1f, 0.72f, 0.30f, 1f);
            Color bottom = new Color(0.78f, 0.42f, 0.05f, 1f);
            float v = y / (float)(IconSize - 1);
            return Color.Lerp(top, bottom, Mathf.Clamp01(v));
        }

        private static Color RenderForegroundPixel(int x, int y)
        {
            float u = x / (float)(IconSize - 1);
            float v = y / (float)(IconSize - 1);
            float alpha = Mathf.Clamp01(0.5f - GlyphDistance(u, v) * IconSize);
            return new Color(0.98f, 0.95f, 0.88f, alpha);
        }

        private static float GlyphDistance(float u, float v)
        {
            float d = float.MaxValue;
            d = Mathf.Min(d, DistToSegment(u, v, 0.25f, 0.85f, 0.25f, 0.20f));
            d = Mathf.Min(d, DistToSegment(u, v, 0.75f, 0.85f, 0.75f, 0.20f));
            d = Mathf.Min(d, DistToSegment(u, v, 0.31f, 0.85f, 0.50f, 0.30f));
            d = Mathf.Min(d, DistToSegment(u, v, 0.69f, 0.85f, 0.50f, 0.30f));
            return d - 0.045f;
        }

        private static float DistToSegment(float px, float py, float ax, float ay, float bx, float by)
        {
            float abx = bx - ax;
            float aby = by - ay;
            float t = ((px - ax) * abx + (py - ay) * aby) / (abx * abx + aby * aby);
            t = Mathf.Clamp01(t);
            float cx = ax + abx * t;
            float cy = ay + aby * t;
            float dx = px - cx;
            float dy = py - cy;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }
    }
}
