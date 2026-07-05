using CindarsHope.NPC;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.NPC
{
    /// <summary>
    /// Validador somente-leitura: para todo NpcDataSO com WalkAnimResourcesPath preenchido, confere
    /// que os 25 sprites fatiados (5 frames x 5 direcoes) realmente existem em Resources. NAO gera,
    /// NAO repara, NAO muta assets. Registrado como RunStep em CindarsHopeMenu.ValidarProjeto.
    /// </summary>
    public static class ValidateNpcWalkAnimations
    {
        private const string NpcDataRoot = "Assets/_Game/Data/NPCs";
        private const int ExpectedSpriteCount = 25;

        public static void Validate()
        {
            var guids = AssetDatabase.FindAssets("t:NpcDataSO", new[] { NpcDataRoot });
            int checkedCount = 0;
            int errors = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var npcData = AssetDatabase.LoadAssetAtPath<NpcDataSO>(path);
                if (npcData == null || string.IsNullOrEmpty(npcData.WalkAnimResourcesPath))
                {
                    continue; // NPC ainda sem anim de caminhada gerada — nao e erro.
                }

                checkedCount++;
                var sprites = Resources.LoadAll<Sprite>(npcData.WalkAnimResourcesPath);
                if (sprites == null || sprites.Length != ExpectedSpriteCount)
                {
                    Debug.LogError($"[ValidateNpcWalkAnimations] {npcData.NpcId}: esperado {ExpectedSpriteCount} " +
                                    $"sprites em Resources/{npcData.WalkAnimResourcesPath}, encontrado " +
                                    $"{(sprites != null ? sprites.Length : 0)}.");
                    errors++;
                }
            }

            Debug.Log($"[ValidateNpcWalkAnimations] Concluido. NPCs checados: {checkedCount} | Erros: {errors}.");
        }
    }
}
