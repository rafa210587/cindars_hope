using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Cave.Art
{
    /// <summary>
    /// Fix pós-Play-Mode 2026-07-04 (spec_cave_biome_art_profiles_runtime / CV01): fallback via
    /// Resources para o array serializado <c>CaveRuntimeMaterializer._biomeArtProfiles</c>. Cenas
    /// de CaveScene criadas ANTES desta spec têm o array vazio (nunca preenchido no Editor) e nada
    /// religava os 8 <see cref="CaveBiomeArtProfileSO"/> gerados — resolver ficava sempre vazio e o
    /// bioma nunca aparecia, em silêncio.
    ///
    /// Precedente: <c>CombatRuntimeDatabasesRegistrySO</c> (Assets/_Game/Resources/
    /// CombatRuntimeDatabasesRegistry.asset), carregado via <c>Resources.Load</c> em
    /// <c>CaveRuntimeMaterializer.EnsureCombatDatabasesBound</c>. Este registry segue o mesmo padrão:
    /// só é consultado quando o array serializado está vazio (serialized field continua sendo a
    /// fonte de verdade quando preenchido — nenhuma mudança de comportamento quando a cena já tem os
    /// profiles wireados manualmente).
    ///
    /// Não influencia layout/spawn/loot/snapshot (cave-stable-run/ADR-0005) — puramente uma lista de
    /// referências de apresentação visual, materializada por <see cref="GenerateCaveBiomeArtProfiles"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "CaveBiomeArtProfileRegistry", menuName = "CindarsHope/Cave/Biome Art Profile Registry")]
    public sealed class CaveBiomeArtProfileRegistrySO : ScriptableObject
    {
        [SerializeField] private List<CaveBiomeArtProfileSO> _profiles = new List<CaveBiomeArtProfileSO>();

        public List<CaveBiomeArtProfileSO> Profiles => _profiles;

        /// <summary>Setter editor-only usado pelo gerador (GenerateCaveBiomeArtProfiles) para popular
        /// o registry idempotentemente. Não usar em runtime.</summary>
        public void EditorSetProfiles(List<CaveBiomeArtProfileSO> profiles)
        {
            _profiles = profiles ?? new List<CaveBiomeArtProfileSO>();
        }
    }
}
