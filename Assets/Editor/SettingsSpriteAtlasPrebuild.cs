// [UNITY-SKILL:SPRITEATLAS]
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace MementoMori.EditorTools
{
    /// <summary>Packs modular UI sprites for every build without changing their individual references.</summary>
    public sealed class SettingsSpriteAtlasPrebuild : IPreprocessBuildWithReport
    {
        private const string AtlasPath = "Assets/UI/SettingsAndTutorial.spriteatlasv2";
        private static readonly string[] Sources =
        {
            "Assets/UI/Resources/SettingsCogV1.png",
            "Assets/UI/Resources/SettingsFrameV1.png",
            "Assets/UI/Resources/SettingsTabFrameV1.png",
            "Assets/UI/Resources/SettingsSliderTrackV1.png",
            "Assets/UI/Resources/SettingsSliderThumbV1.png",
            "Assets/UI/Resources/SettingsPurplePaperV1.png",
            "Assets/UI/Resources/TutorialFrameV1.png"
        };

        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report) => EnsureAtlas();

        public static void EnsureAtlas()
        {
            EditorSettings.spritePackerMode = SpritePackerMode.SpriteAtlasV2;
            if (EditorSettings.spritePackerMode == SpritePackerMode.Disabled)
                throw new InvalidOperationException("Sprite packing remains disabled.");

            var sprites = new UnityEngine.Object[Sources.Length];
            for (var i = 0; i < Sources.Length; i++)
            {
                var sourceImporter = AssetImporter.GetAtPath(Sources[i]) as TextureImporter;
                if (sourceImporter == null) throw new InvalidOperationException("Missing UI texture importer: " + Sources[i]);
                var limit = Sources[i].Contains("FrameV1") && !Sources[i].Contains("TabFrame") ? 1024
                    : Sources[i].Contains("Thumb") ? 128 : Sources[i].Contains("Cog") ? 256 : 512;
                if (sourceImporter.maxTextureSize != limit)
                {
                    sourceImporter.maxTextureSize = limit;
                    sourceImporter.SaveAndReimport();
                }
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(Sources[i]);
                if (sprites[i] == null) throw new InvalidOperationException("Missing UI sprite: " + Sources[i]);
            }

            var asset = File.Exists(AtlasPath) ? SpriteAtlasAsset.Load(AtlasPath) : new SpriteAtlasAsset();
            asset.Remove(sprites);
            asset.Add(sprites);
            SpriteAtlasAsset.Save(asset, AtlasPath);
            AssetDatabase.ImportAsset(AtlasPath);
            var importer = AssetImporter.GetAtPath(AtlasPath) as SpriteAtlasImporter;
            if (importer == null) throw new InvalidOperationException("Sprite atlas importer unavailable.");
            var texture = importer.textureSettings;
            texture.filterMode = FilterMode.Point;
            texture.generateMipMaps = false;
            importer.textureSettings = texture;
            var packing = importer.packingSettings;
            packing.padding = 2;
            packing.enableRotation = false;
            packing.enableTightPacking = false;
            importer.packingSettings = packing;
            importer.includeInBuild = true;
            importer.SaveAndReimport();
            AssetDatabase.SaveAssets();
            Debug.Log("[SettingsAtlas] Built-in atlas ready; packer=" + EditorSettings.spritePackerMode
                + ", sprites=" + Sources.Length + ", path=" + AtlasPath);
        }
    }
}
