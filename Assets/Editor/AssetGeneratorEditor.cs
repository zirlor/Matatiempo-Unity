using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Matatiempo.EditorTools
{
    public static class AssetGeneratorEditor
    {
        private const string SpriteRoot = "Assets/Resources/Sprites";
        private const string AudioRoot = "Assets/Resources/Audio/SFX";
        private const int SampleRate = 44100;

        private static readonly Color Wood = new Color(0.62f, 0.45f, 0.30f, 1f);
        private static readonly Color WoodDark = new Color(0.42f, 0.30f, 0.20f, 1f);
        private static readonly Color Figure = new Color(0.60f, 0.42f, 0.27f, 1f);
        private static readonly Color FigureOutline = new Color(0.28f, 0.20f, 0.12f, 1f);
        private static readonly Color KeyFill = new Color(0.965f, 0.918f, 0.780f, 1f);
        private static readonly Color KeyPressedFill = new Color(0.92f, 0.82f, 0.62f, 1f);

        [MenuItem("Tools/Matatiempo/Generate Game Assets")]
        public static void GenerateAll()
        {
            GenerateSprites();
            GenerateSfx();
            AssetDatabase.Refresh();
            Debug.Log("[Matatiempo] Sprites y SFX generados en Resources/Sprites y Resources/Audio/SFX.");
        }

        private static void GenerateSprites()
        {
            Directory.CreateDirectory(SpriteRoot);
            AssetDatabase.Refresh();

            SaveSprite("Hangman/base.png", Capsule(160, 16, 8, 8, 152, 8, 5f, Wood, WoodDark));
            SaveSprite("Hangman/pole.png", Capsule(16, 96, 8, 4, 8, 92, 5f, Wood, WoodDark));
            SaveSprite("Hangman/beam.png", Capsule(116, 16, 6, 8, 110, 8, 5f, Wood, WoodDark));
            SaveSprite("Hangman/rope.png", Capsule(8, 28, 4, 4, 4, 24, 3f, Figure, FigureOutline));
            SaveSprite("Hangman/head.png", Head(24, 24, 8f, Figure, FigureOutline));
            SaveSprite("Hangman/body.png", Capsule(16, 40, 8, 6, 8, 34, 5f, Figure, FigureOutline));
            SaveSprite("Hangman/arm.png", Capsule(24, 12, 4, 6, 20, 6, 4f, Figure, FigureOutline));
            SaveSprite("Hangman/leg.png", Capsule(24, 12, 4, 6, 20, 6, 4f, Figure, FigureOutline));

            SaveSprite("UI/background.png", Background(32, 32));
            SaveSprite("UI/panel.png", RoundedRect(64, 64, 16f, new Color(0.976f, 0.949f, 0.878f, 0.92f), new Color(0.45f, 0.33f, 0.22f, 0.60f), true));
            SaveSprite("UI/key_normal.png", RoundedRect(96, 96, 14f, KeyFill, Primary(), true));
            SaveSprite("UI/key_pressed.png", RoundedRect(96, 96, 14f, KeyPressedFill, Primary(), true));
        }

        private static void GenerateSfx()
        {
            Directory.CreateDirectory(AudioRoot);

            WriteWav("click", SynthClick());
            WriteWav("correct", SynthCorrect());
            WriteWav("wrong", SynthWrong());
            WriteWav("win", SynthWin());
            WriteWav("lose", SynthLose());
            WriteWav("levelup", SynthLevelUp());
            WriteWav("tick", SynthTick());
            WriteWav("start", SynthStart());
        }

        private static Color Primary()
        {
            return new Color(0.93f, 0.62f, 0.16f, 1f);
        }

        private static void SaveSprite(string relativePath, Texture2D texture)
        {
            Directory.CreateDirectory(SpriteRoot);
            string path = Path.Combine(SpriteRoot, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            path = path.Replace('\\', '/');
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        private static void WriteWav(string name, float[] samples)
        {
            Directory.CreateDirectory(AudioRoot);
            string path = Path.Combine(AudioRoot, name + ".wav");
            File.WriteAllBytes(path, streamToBytes(samples));
        }

        private static byte[] streamToBytes(float[] samples)
        {
            using (var stream = new MemoryStream(samples.Length * 2 + 44))
            using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + samples.Length * 2);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));

                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(SampleRate);
                writer.Write(SampleRate * 2);
                writer.Write((short)2);
                writer.Write((short)16);

                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(samples.Length * 2);
                for (int i = 0; i < samples.Length; i++)
                {
                    short sample = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, (int)(samples[i] * short.MaxValue)));
                    writer.Write(sample);
                }
                writer.Flush();
                return stream.ToArray();
            }
        }

        private static Texture2D Render(int width, int height, Func<float, float, Color> pixel)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var colors = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colors[y * width + x] = pixel(x + 0.5f, y + 0.5f);
                }
            }
            texture.SetPixels(colors);
            texture.Apply(false);
            return texture;
        }

        private static Texture2D Capsule(int width, int height, float ax, float ay, float bx, float by, float radius, Color fill, Color outline)
        {
            return Render(width, height, (px, py) =>
            {
                float d = DistanceToSegment(px, py, ax, ay, bx, by);
                if (d <= radius - 1f)
                {
                    return fill;
                }
                if (d <= radius + 1f)
                {
                    return outline;
                }
                return Color.clear;
            });
        }

        private static Texture2D Head(int width, int height, float radius, Color fill, Color outline)
        {
            float cx = width * 0.5f;
            float cy = height * 0.5f;
            return Render(width, height, (px, py) =>
            {
                float d = (float)Math.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));
                if (d <= radius - 1f)
                {
                    return fill;
                }
                if (d <= radius + 1.5f)
                {
                    return outline;
                }
                return Color.clear;
            });
        }

        private static Texture2D Background(int width, int height)
        {
            Color top = new Color(0.984f, 0.969f, 0.918f, 1f);
            Color bottom = new Color(0.941f, 0.878f, 0.737f, 1f);
            return Render(width, height, (px, py) =>
            {
                float t = py / height;
                float grain = (((int)px * 31 + (int)py * 17) % 7 - 3f) * 0.008f;
                Color baseColor = Color.Lerp(top, bottom, t);
                return new Color(
                    Mathf.Clamp01(baseColor.r + grain),
                    Mathf.Clamp01(baseColor.g + grain),
                    Mathf.Clamp01(baseColor.b + grain),
                    1f);
            });
        }

        private static Texture2D RoundedRect(int width, int height, float radius, Color fill, Color outline, bool verticalGradient)
        {
            Color light = Color.Lerp(fill, Color.white, 0.12f);
            return Render(width, height, (px, py) =>
            {
                float outside = RoundedSdf(px, py, width, height, radius);
                if (outside <= 0f)
                {
                    return verticalGradient ? Color.Lerp(light, fill, py / height) : fill;
                }
                if (outside <= 2.5f)
                {
                    return outline;
                }
                return Color.clear;
            });
        }

        private static float RoundedSdf(float px, float py, int width, int height, float radius)
        {
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;
            float qx = Mathf.Max(Mathf.Abs(px - halfW) - (halfW - radius), 0f);
            float qy = Mathf.Max(Mathf.Abs(py - halfH) - (halfH - radius), 0f);
            return Mathf.Sqrt(qx * qx + qy * qy) - radius;
        }

        private static float DistanceToSegment(float px, float py, float ax, float ay, float bx, float by)
        {
            float dx = bx - ax;
            float dy = by - ay;
            float lengthSq = dx * dx + dy * dy;
            float t = lengthSq > 0f ? Mathf.Clamp01(((px - ax) * dx + (py - ay) * dy) / lengthSq) : 0f;
            float ex = ax + t * dx;
            float ey = ay + t * dy;
            float vx = px - ex;
            float vy = py - ey;
            return Mathf.Sqrt(vx * vx + vy * vy);
        }

        private static float[] SynthClick()
        {
            int length = (int)(0.04f * SampleRate);
            var samples = new float[length];
            var rng = new System.Random(1234);
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)SampleRate;
                float env = (float)Math.Exp(-120.0 * t);
                samples[i] = (float)((rng.NextDouble() * 2.0 - 1.0) * env * 0.55);
            }
            return samples;
        }

        private static float[] SynthCorrect()
        {
            return ToneSequence(new[] { 523.25, 659.25 }, 0.12, true, 0.5);
        }

        private static float[] SynthWrong()
        {
            return ToneSequence(new[] { 196.0, 185.0 }, 0.16, false, 0.5);
        }

        private static float[] SynthWin()
        {
            return ToneSequence(new[] { 523.25, 659.25, 783.99, 1046.5 }, 0.14, true, 0.5);
        }

        private static float[] SynthLose()
        {
            return ToneSequence(new[] { 392.0, 311.13, 246.94 }, 0.18, true, 0.45);
        }

        private static float[] SynthLevelUp()
        {
            return ToneSequence(new[] { 523.25, 783.99, 1046.5 }, 0.09, true, 0.5);
        }

        private static float[] SynthTick()
        {
            return ToneSequence(new[] { 1200.0 }, 0.03, true, 0.3);
        }

        private static float[] SynthStart()
        {
            return ToneSequence(new[] { 392.0, 523.25, 659.25 }, 0.10, true, 0.45);
        }

        private static float[] ToneSequence(double[] frequencies, double noteLength, bool sine, double gain)
        {
            int noteSamples = (int)(noteLength * SampleRate);
            var samples = new float[noteSamples * frequencies.Length];
            for (int n = 0; n < frequencies.Length; n++)
            {
                double frequency = frequencies[n];
                for (int i = 0; i < noteSamples; i++)
                {
                    double t = i / (double)SampleRate;
                    double phase = 2.0 * Math.PI * frequency * t;
                    double wave = sine ? Math.Sin(phase) : Math.Sign(Math.Sin(phase)) * 0.7 + Math.Sin(phase) * 0.3;
                    double env = Math.Exp(-6.0 * t / noteLength);
                    samples[n * noteSamples + i] = (float)(wave * env * gain);
                }
            }
            return samples;
        }
    }
}