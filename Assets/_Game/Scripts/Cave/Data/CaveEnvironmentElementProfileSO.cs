using System;
using System.Collections.Generic;
using CindarsHope.Cave.Ecosystem;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Cave.Data
{
    /// <summary>
    /// fable_78 — perfil temático de elementos ambientais de UM bioma/banda (seção 16.1).
    /// Define quais tipos de elemento aparecem, com que peso relativo, se a banda tem água (habilita
    /// criaturas aquáticas), e quais ore tiers/mineáveis são permitidos. Id estável (rule id-stability):
    /// "cave_elem_profile_&lt;biome&gt;". Membro de <see cref="CaveEnvironmentElementDatabaseSO"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "CaveEnvironmentElementProfile", menuName = "CindarsHope/Cave/Environment Element Profile")]
    public sealed class CaveEnvironmentElementProfileSO : ScriptableObject, IIdentifiedData
    {
        [Serializable]
        public struct ElementEntry
        {
            public CaveEnvironmentElementKind Kind;
            [Min(0f)] public float Weight;

            /// <summary>Id do ResourceNodeDataSO quando Kind == MineableNode; vazio caso contrário.</summary>
            public string MineNodeDataId;
        }

        [SerializeField] private string _id = "cave_elem_profile_stone";
        [SerializeField] private string _biomeId = "biome_cave_stone";

        [Tooltip("Banda de bioma 0..6 (Stone, Fungal, Ice, Fire, Ruins, Deep, Void).")]
        [SerializeField, Range(0, 6)] private int _band = 0;

        [SerializeField] private bool _hasWater;
        [SerializeField] private ElementEntry[] _entries = Array.Empty<ElementEntry>();

        [Tooltip("Ore tiers permitidos nesta banda (referência temática; numérica final = fable_59).")]
        [SerializeField] private string[] _allowedOreTiers = Array.Empty<string>();

        public string Id => _id;
        public string BiomeId => _biomeId;
        public int Band => _band;
        public bool HasWater => _hasWater;
        public IReadOnlyList<ElementEntry> Entries => _entries;
        public IReadOnlyList<string> AllowedOreTiers => _allowedOreTiers;

        string IIdentifiedData.Id => _id;

        private void OnValidate()
        {
            _band = Mathf.Clamp(_band, 0, CaveEcosystemBalanceSO.BandCount - 1);
            if (_entries == null)
            {
                return;
            }

            for (var i = 0; i < _entries.Length; i++)
            {
                if (_entries[i].Weight < 0f)
                {
                    _entries[i].Weight = 0f;
                }
            }
        }
    }
}
