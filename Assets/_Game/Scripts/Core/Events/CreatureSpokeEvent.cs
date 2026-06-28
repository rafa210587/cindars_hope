using UnityEngine;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Uma criatura da caverna falou uma fala ambiente curta. Cosmético: nunca afeta combate,
    /// loot ou o contrato de stable-run da caverna. Publicado pelo CreatureChatterController;
    /// o CreatureSpeechBubbleDisplayer escuta e desenha o balão (estilo HQ) na posição.
    /// </summary>
    public readonly struct CreatureSpokeEvent
    {
        public readonly string EnemyId;
        public readonly string Line;
        public readonly Vector3 WorldPosition;

        public CreatureSpokeEvent(string enemyId, string line, Vector3 worldPosition)
        {
            EnemyId = enemyId ?? string.Empty;
            Line = line ?? string.Empty;
            WorldPosition = worldPosition;
        }
    }
}
