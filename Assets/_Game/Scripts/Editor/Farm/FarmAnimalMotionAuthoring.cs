#if UNITY_EDITOR
using System;
using System.Linq;
using CindarsHope.Farm.Animals;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Farm
{
    /// <summary>Imports reviewed animal strips and authors profiles keyed by stable animal IDs.</summary>
    public static class FarmAnimalMotionAuthoring
    {
        public const string ChickenSheetPath =
            "Assets/_Game/Art/Generated/World/animals/chicken_motion_v18/chicken_v18.png";
        public const string ChickenProfilePath =
            "Assets/_Game/Data/Animals/AnimalMotionProfile_animal_chicken.asset";

        public const string CowProfilePath = "Assets/_Game/Data/Animals/AnimalMotionProfile_animal_cow.asset";
        public const string SheepProfilePath = "Assets/_Game/Data/Animals/AnimalMotionProfile_animal_sheep.asset";
        public const string GoatProfilePath = "Assets/_Game/Data/Animals/AnimalMotionProfile_animal_goat.asset";

        private readonly struct SheetDefinition
        {
            public readonly string AnimalId, SheetPath, ProfilePath, Prefix;
            public readonly int FrameSize;
            public readonly Vector2 Pivot, BodySize;
            public readonly float Speed, StepDistance;
            public bool IsChicken => AnimalId == FarmAnimalCatalog.AnimalChicken;

            public SheetDefinition(string animalId, string sheetPath, string profilePath, string prefix,
                int frameSize, Vector2 pivot, Vector2 bodySize, float speed, float stepDistance)
            {
                AnimalId = animalId; SheetPath = sheetPath; ProfilePath = profilePath; Prefix = prefix;
                FrameSize = frameSize; Pivot = pivot; BodySize = bodySize;
                Speed = speed; StepDistance = stepDistance;
            }
        }

        private static readonly SheetDefinition[] Sheets =
        {
            new SheetDefinition(FarmAnimalCatalog.AnimalChicken, ChickenSheetPath, ChickenProfilePath,
                "chicken_v18", 32, new Vector2(16f / 32f, 6f / 32f), new Vector2(.55f, .42f), .7f, .11f),
            new SheetDefinition(FarmAnimalCatalog.AnimalCow, "Assets/_Game/Art/Generated/World/animals/herd_motion_v19/cow_v19.png",
                CowProfilePath, "cow_v19", 48, new Vector2(.5f, 8f / 48f), new Vector2(.9f, .55f), .5f, .09f),
            new SheetDefinition(FarmAnimalCatalog.AnimalSheep, "Assets/_Game/Art/Generated/World/animals/herd_motion_v19/sheep_v19.png",
                SheepProfilePath, "sheep_v19", 48, new Vector2(.5f, 8f / 48f), new Vector2(.7f, .45f), .6f, .096f),
            new SheetDefinition(FarmAnimalCatalog.AnimalGoat, "Assets/_Game/Art/Generated/World/animals/herd_motion_v19/goat_v19.png",
                GoatProfilePath, "goat_v19", 48, new Vector2(.5f, 8f / 48f), new Vector2(.7f, .45f), .6f, .096f)
        };
        private const int FrameCount = 19;
        private const float PixelsPerUnit = 32f;

        private static readonly (string tag, int from, int to)[] Sequences =
        {
            ("idle", 0, 1),
            ("walk_down", 2, 5),
            ("walk_up", 6, 9),
            ("walk_side", 10, 13),
            ("peck", 14, 16),
            ("rest", 17, 18)
        };

        public static void Generate()
        {
            foreach (var sheet in Sheets) Generate(sheet);
            AssetDatabase.SaveAssets();
        }

        private static void Generate(SheetDefinition sheet)
        {
            var importer = AssetImporter.GetAtPath(sheet.SheetPath) as TextureImporter;
            if (importer == null)
                throw new InvalidOperationException($"Animal motion sheet not found: {sheet.SheetPath}");

            ConfigureAndSlice(importer, sheet);
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(sheet.SheetPath)
                .OfType<Sprite>()
                .OrderBy(ParseFrameIndex)
                .ToArray();
            if (sprites.Length != FrameCount)
                throw new InvalidOperationException($"Expected {FrameCount} {sheet.AnimalId} sprites, imported {sprites.Length}.");

            EnsureFolder("Assets/_Game/Data", "Animals");
            var profile = AssetDatabase.LoadAssetAtPath<AnimalMotionProfileSO>(sheet.ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<AnimalMotionProfileSO>();
                AssetDatabase.CreateAsset(profile, sheet.ProfilePath);
            }

            var settings = new AnimalMotionSettings(
                moveSpeed: sheet.Speed,
                wanderRadius: 1.15f,
                idleMin: 0.8f,
                idleMax: 2.5f,
                peckDuration: 0.72f,
                restDuration: 2.4f,
                walkChance: 0.58f,
                peckChance: 0.27f,
                maxTargetAttempts: 6,
                targetTolerance: 0.04f);
            profile.Configure(
                sheet.AnimalId,
                settings,
                bodySize: sheet.BodySize,
                boundsSkin: 0.03f,
                obstacleMask: ~0,
                interactionFreezeSeconds: 0.45f,
                distancePerWalkFrame: sheet.StepDistance,
                idleFrameSeconds: 0.4f,
                peckFrameSeconds: sheet.IsChicken ? 0.18f : 0.22f,
                restFrameSeconds: sheet.IsChicken ? 0.6f : 0.65f,
                idle: Slice(sprites, 0, 2),
                walkDown: Slice(sprites, 2, 4),
                walkUp: Slice(sprites, 6, 4),
                walkSide: Slice(sprites, 10, 4),
                peck: Slice(sprites, 14, 3),
                rest: Slice(sprites, 17, 2));
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            Debug.Log($"[FarmAnimalMotionAuthoring] PASS: {FrameCount} frames + {sheet.ProfilePath}");
        }

        private static void ConfigureAndSlice(TextureImporter importer, SheetDefinition sheet)
        {
            int frameSize = sheet.FrameSize;
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(sheet.SheetPath);
            if (texture == null || texture.width != frameSize * FrameCount || texture.height != frameSize)
                throw new InvalidOperationException($"{sheet.AnimalId} sheet must contain 19 horizontal {frameSize}x{frameSize} frames.");

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.textureType = TextureImporterType.Sprite;
            settings.spriteMode = (int)SpriteImportMode.Multiple;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.filterMode = FilterMode.Point;
            settings.mipmapEnabled = false;
            settings.readable = false;
            importer.SetTextureSettings(settings);

            var metas = new SpriteMetaData[FrameCount];
            int index = 0;
            foreach (var sequence in Sequences)
            {
                for (int frame = sequence.from; frame <= sequence.to; frame++)
                {
                    metas[index++] = new SpriteMetaData
                    {
                        name = $"{sheet.Prefix}_{frame:00}_{(!sheet.IsChicken && sequence.tag == "peck" ? "graze" : sequence.tag)}",
                        rect = new Rect(frame * frameSize, 0, frameSize, frameSize),
                        alignment = (int)SpriteAlignment.Custom,
                        pivot = sheet.Pivot,
                        border = Vector4.zero
                    };
                }
            }
            if (index != FrameCount) throw new InvalidOperationException("Chicken sequence contract does not total 19 frames.");
#pragma warning disable CS0618
            importer.spritesheet = metas;
#pragma warning restore CS0618
            importer.SaveAndReimport();
        }

        private static int ParseFrameIndex(Sprite sprite)
        {
            string[] parts = sprite.name.Split('_');
            return parts.Length >= 3 && int.TryParse(parts[2], out int index) ? index : int.MaxValue;
        }

        private static Sprite[] Slice(Sprite[] sprites, int start, int count)
        {
            var result = new Sprite[count];
            Array.Copy(sprites, start, result, 0, count);
            return result;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
