using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>Town candidate carve-out runs after the shared generated-sprite defaults.</summary>
    public sealed class TownKeyartSpriteImporter : AssetPostprocessor
    {
        // Exact final exports owned by this pass. In particular, locations/town_hall is shared.
        private static readonly HashSet<string> OwnedPaths = new HashSet<string>(System.StringComparer.Ordinal)
        {
            "Assets/_Game/Art/Generated/World/building/town_house_red_gable_keyart.png",
            "Assets/_Game/Art/Generated/World/building/town_house_blue_gable_keyart.png",
            "Assets/_Game/Art/Generated/World/building/town_house_green_gable_keyart.png",
            "Assets/_Game/Art/Generated/World/building/town_watermill_keyart.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_keyart.png",
            "Assets/_Game/Art/Generated/World/building/town_bakery_keyart.png",
            "Assets/_Game/Art/Generated/World/building/town_watermill_native_architecture.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_architecture.png",
            "Assets/_Game/Art/Generated/World/building/town_bakery_native_architecture.png",
            "Assets/_Game/Art/Generated/World/building/town_house_courtyard_gable_native.png",
            "Assets/_Game/Art/Generated/World/building/town_house_hipped_stone_native_v01.png",
            "Assets/_Game/Art/Generated/World/building/town_house_l_dormer_native_v01.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_repixel_v02.png",
            "Assets/_Game/Art/Generated/World/building/town_bakery_native_scale1_v03.png",
            "Assets/_Game/Art/Generated/World/building/town_watermill_native_scale1_v03.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_scale1_v03.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_pixel_v04.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_v06.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_v06_facade.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_v06_roof.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_v06_door_leaf.png",
            "Assets/_Game/Art/Generated/World/building/town_bakery_native_architecture_facade.png",
            "Assets/_Game/Art/Generated/World/building/town_bakery_native_architecture_roof.png",
            "Assets/_Game/Art/Generated/World/building/town_bakery_native_architecture_door_leaf.png",
            "Assets/_Game/Art/Generated/World/building/town_watermill_native_architecture_facade.png",
            "Assets/_Game/Art/Generated/World/building/town_watermill_native_architecture_roof.png",
            "Assets/_Game/Art/Generated/World/building/town_watermill_native_architecture_door_leaf.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_architecture_facade.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_architecture_roof.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_architecture_door_leaf.png",
            "Assets/_Game/Art/Generated/World/building/town_courtyard_native_architecture_facade.png",
            "Assets/_Game/Art/Generated/World/building/town_courtyard_native_architecture_roof.png",
            "Assets/_Game/Art/Generated/World/building/town_courtyard_native_architecture_door_leaf.png",
            "Assets/_Game/Art/Generated/World/building/town_house_hipped_stone_native_v01_facade.png",
            "Assets/_Game/Art/Generated/World/building/town_house_hipped_stone_native_v01_roof.png",
            "Assets/_Game/Art/Generated/World/building/town_house_hipped_stone_native_v01_door_leaf.png",
            "Assets/_Game/Art/Generated/World/building/town_house_l_dormer_native_v01_facade.png",
            "Assets/_Game/Art/Generated/World/building/town_house_l_dormer_native_v01_roof.png",
            "Assets/_Game/Art/Generated/World/building/town_house_l_dormer_native_v01_door_leaf.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_repixel_v02_facade.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_repixel_v02_roof.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_repixel_v02_door_leaf.png",
            "Assets/_Game/Art/Generated/World/building/town_bakery_native_scale1_v03_facade.png",
            "Assets/_Game/Art/Generated/World/building/town_bakery_native_scale1_v03_roof.png",
            "Assets/_Game/Art/Generated/World/building/town_bakery_native_scale1_v03_door_leaf.png",
            "Assets/_Game/Art/Generated/World/building/town_watermill_native_scale1_v03_facade.png",
            "Assets/_Game/Art/Generated/World/building/town_watermill_native_scale1_v03_roof.png",
            "Assets/_Game/Art/Generated/World/building/town_watermill_native_scale1_v03_door_leaf.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_scale1_v03_facade.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_scale1_v03_roof.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_scale1_v03_door_leaf.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_pixel_v04_facade.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_pixel_v04_roof.png",
            "Assets/_Game/Art/Generated/World/building/town_alchemy_native_pixel_v04_door_leaf.png",
            "Assets/_Game/Art/Generated/World/building/town_animal_shed_keyart.png",
            "Assets/_Game/Art/Generated/World/building/town_hay_shed_keyart.png",
            "Assets/_Game/Art/Generated/World/props/town_statue_fountain_keyart.png",
            "Assets/_Game/Art/Generated/World/props/town_plaza_arc_nw_keyart.png",
            "Assets/_Game/Art/Generated/World/props/town_plaza_arc_ne_keyart.png",
            "Assets/_Game/Art/Generated/World/props/town_plaza_arc_sw_keyart.png",
            "Assets/_Game/Art/Generated/World/props/town_plaza_arc_se_keyart.png",
            "Assets/_Game/Art/Generated/World/props/town_market_stall_blue_keyart.png",
            "Assets/_Game/Art/Generated/World/props/town_market_stall_red_keyart.png",
            "Assets/_Game/Art/Generated/World/props/town_market_stall_green_keyart.png",
            "Assets/_Game/Art/Generated/World/props/town_gate_pillar_keyart.png",
            "Assets/_Game/Art/Generated/World/props/town_low_wall_curve_keyart.png",
            "Assets/_Game/Art/Generated/World/props/town_cobble_keyart.png",
            "Assets/_Game/Art/Generated/World/building/town_door_frame_00.png",
            "Assets/_Game/Art/Generated/World/building/town_door_frame_01.png",
            "Assets/_Game/Art/Generated/World/building/town_door_frame_02.png",
            "Assets/_Game/Art/Generated/World/building/town_door_frame_03.png",
            "Assets/_Game/Art/Generated/World/building/town_door_frame_04.png"
        };

        public static bool IsOwnedAsset(string path) => OwnedPaths.Contains(path.Replace('\\','/'));

        public static void ApplyOwnedImportSettings()
        {
            foreach (string path in OwnedPaths)
            {
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
                settings.spritePixelsPerUnit = path.EndsWith("town_cobble_keyart.png") ? 64 : 32;
                settings.spriteAlignment = (int)(path.EndsWith("town_cobble_keyart.png") ? SpriteAlignment.Center : SpriteAlignment.BottomCenter);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                foreach (string platform in new[] { "DefaultTexturePlatform", "Standalone", "WebGL", "Android", "iPhone" })
                {
                    var platformSettings = importer.GetPlatformTextureSettings(platform);
                    platformSettings.name = platform;
                    platformSettings.overridden = true;
                    platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
                    platformSettings.crunchedCompression = false;
                    importer.SetPlatformTextureSettings(platformSettings);
                }
                importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log($"[TownKeyartSpriteImporter] PASS owned imports configured: {OwnedPaths.Count}");
        }

        public override int GetPostprocessOrder() => 1000;
        private void OnPreprocessTexture()
        {
            string path=assetPath.Replace('\\','/');
            if(!IsOwnedAsset(path))return;
            var importer=(TextureImporter)assetImporter;
            bool tile=path.EndsWith("town_cobble_keyart.png");
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled=false;importer.isReadable=true;
            foreach (string platform in new[] { "DefaultTexturePlatform", "Standalone", "WebGL", "Android", "iPhone" })
            {
                var platformSettings = importer.GetPlatformTextureSettings(platform);
                platformSettings.name = platform;
                platformSettings.overridden = true;
                platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
                platformSettings.crunchedCompression = false;
                importer.SetPlatformTextureSettings(platformSettings);
            }
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
            settings.spritePixelsPerUnit=tile?64:32;
            settings.spriteAlignment=(int)(tile?SpriteAlignment.Center:SpriteAlignment.BottomCenter);
            settings.spriteMeshType=SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
        }
    }
}
