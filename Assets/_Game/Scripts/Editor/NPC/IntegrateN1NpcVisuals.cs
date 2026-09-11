using System;
using System.Collections.Generic;
using CindarsHope.NPC;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.NPC
{
    /// <summary>Scoped N-1 asset integration for the two previously missing NPC visuals.</summary>
    public static class IntegrateN1NpcVisuals
    {
        private const string DataRoot = "Assets/_Game/Data/NPCs";
        private const string WalkRoot = "Assets/_Game/Resources/NpcWalkSprites";
        private const string BodyRoot = "Assets/_Game/Resources/NpcSprites";

        public static void Execute()
        {
            Integrate("npc_corvus", "Npc_Corvus.asset");
            Integrate("npc_vaalara_wanderer_01", "Npc_Vaalara_Wanderer_01.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("[IntegrateN1NpcVisuals] PASS: 2 NPCs integrados; folhas 5x5 e bases configuradas.");
        }

        private static void Integrate(string npcId, string dataFile)
        {
            string walkPath = $"{WalkRoot}/{npcId}_walk.png";
            string bodyPath = $"{BodyRoot}/{npcId}.png";
            ConfigureWalk(walkPath, npcId);
            ConfigureBody(bodyPath);

            var data = AssetDatabase.LoadAssetAtPath<NpcDataSO>($"{DataRoot}/{dataFile}");
            var body = AssetDatabase.LoadAssetAtPath<Sprite>(bodyPath);
            if (data == null || body == null)
                throw new InvalidOperationException($"N-1 asset ausente: {npcId} data={data != null} body={body != null}");

            var serialized = new SerializedObject(data);
            serialized.FindProperty("BodySprite").objectReferenceValue = body;
            serialized.FindProperty("WalkAnimResourcesPath").stringValue = $"NpcWalkSprites/{npcId}_walk";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
        }

        private static void ConfigureBody(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException($"TextureImporter ausente: {path}");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 234f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePivot = new Vector2(0.5f, 0f);
            importer.maxTextureSize = 256;
            importer.SaveAndReimport();
        }

        private static void ConfigureWalk(string path, string npcId)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException($"TextureImporter ausente: {path}");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 234f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 8192;
            importer.SaveAndReimport();

            // Reacquire after the first import pass; Unity invalidates the importer wrapper on reimport.
            importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException($"TextureImporter perdido apos reimport: {path}");

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null || texture.width != 750 || texture.height != 750)
                throw new InvalidOperationException($"Folha N-1 deve ser 750x750: {path}");

            var metas = new List<SpriteMetaData>(25);
            for (int row = 0; row < 5; row++)
            {
                int y = 4 - row;
                for (int col = 0; col < 5; col++)
                {
                    metas.Add(new SpriteMetaData
                    {
                        name = $"{npcId}_walk_r{row}_c{col}",
                        rect = new Rect(col * 150, y * 150, 150, 150),
                        alignment = (int)SpriteAlignment.BottomCenter,
                        pivot = new Vector2(0.5f, 0f),
                    });
                }
            }
#pragma warning disable CS0618
            importer.spritesheet = metas.ToArray();
#pragma warning restore CS0618
            var textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);
            textureSettings.spriteMeshType = SpriteMeshType.FullRect;
            textureSettings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(textureSettings);
            importer.SaveAndReimport();
        }
    }
}
