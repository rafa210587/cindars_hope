// fable_84 — Script utilitario ONE-SHOT para renomear a pasta de sprites de ataque.
// EXECUCAO DEFERIDA: rode no Unity Editor via CindarsHope/Reparar e Reconstruir,
// ou chame RenameAttackSpriteFolder.Execute() diretamente via menu de dev.
// Nao faz nada se attack_sword/ ja existir (idempotente).
// NUNCA renomear via File.Move direto no sistema de arquivos — preservar GUIDs exige AssetDatabase.
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    public static class RenameAttackSpriteFolder
    {
        private const string SourcePath = "Assets/_Game/Resources/PlayerSprites/attack";
        private const string DestPath   = "Assets/_Game/Resources/PlayerSprites/attack_sword";

        /// <summary>
        /// Renomeia a pasta de sprites de ataque de espada para attack_sword/.
        /// Idempotente: se attack_sword/ ja existir ou attack/ nao existir, nao faz nada.
        /// DEVE ser chamado com o Unity Editor aberto (usa AssetDatabase).
        /// </summary>
        public static void Execute()
        {
            if (!AssetDatabase.IsValidFolder(SourcePath))
            {
                if (AssetDatabase.IsValidFolder(DestPath))
                    Debug.Log("[RenameAttackSpriteFolder] Pasta attack_sword/ ja existe. Nada a fazer.");
                else
                    Debug.LogWarning("[RenameAttackSpriteFolder] Pasta attack/ nao encontrada. Nenhuma acao.");
                return;
            }

            string error = AssetDatabase.MoveAsset(SourcePath, DestPath);
            if (string.IsNullOrEmpty(error))
            {
                AssetDatabase.Refresh();
                Debug.Log("[RenameAttackSpriteFolder] Renomeado: attack/ -> attack_sword/ com sucesso.");
            }
            else
            {
                Debug.LogError($"[RenameAttackSpriteFolder] Falha ao renomear: {error}");
            }
        }
    }
}
