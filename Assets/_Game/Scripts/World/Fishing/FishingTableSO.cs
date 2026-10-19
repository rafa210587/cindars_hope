using System;
using System.Collections.Generic;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.World.Fishing
{
    /// <summary>
    /// fable_50 — UMA fonte de verdade da captura de pesca, por contexto (bioma/estação/clima/hora).
    /// Consumida POR fazenda E lagos de caverna através do <see cref="FishingCatchResolver"/>.
    ///
    /// NÃO é um LootTableSO: LootTableSO rola com UnityEngine.Random (não determinístico) e não filtra
    /// por estação/clima/hora. FishingTableSO carrega só tipos simples + IDs estáveis (ADR-0006);
    /// o roll ponderado e determinístico vive no resolver puro, nunca aqui.
    /// </summary>
    [CreateAssetMenu(fileName = "FishingTable", menuName = "CindarsHope/World/Fishing Table")]
    public sealed class FishingTableSO : ScriptableObject, IIdentifiedData
    {
        [SerializeField] private string _tableId;
        [SerializeField] private string _displayName;
        [SerializeField] private List<FishingTableEntry> _entries = new List<FishingTableEntry>();

        public string TableId
        {
            get => _tableId;
            set => _tableId = value;
        }

        public string DisplayName
        {
            get => _displayName;
            set => _displayName = value;
        }

        public IReadOnlyList<FishingTableEntry> Entries => _entries;

        string IIdentifiedData.Id => _tableId;

        public void SetEntries(List<FishingTableEntry> entries)
        {
            _entries = entries ?? new List<FishingTableEntry>();
        }
    }

    /// <summary>
    /// fable_50 — entrada ponderada de uma <see cref="FishingTableSO"/>. Tipos simples + IDs.
    ///
    /// Filtros (aplicados ANTES do roll ponderado, no resolver):
    /// - <see cref="Seasons"/>: estações permitidas (tokens "Primavera/Verao/Outono/Inverno" ou aliases
    ///   em inglês). Vazio = todas as estações.
    /// - <see cref="Weathers"/>: climas permitidos ("Clear/Cloudy/Rainy/Stormy"). Vazio = todos.
    /// - <see cref="NightOnly"/>: quando true, só elegível à noite (hora fora de [DayStartHour..DayEndHour)).
    /// </summary>
    [Serializable]
    public sealed class FishingTableEntry
    {
        [Tooltip("Item de peixe entregue (ex.: item_fish_mirrorfin). Deve existir no catálogo (F32).")]
        public string ItemId;

        [Tooltip("Peso para o sorteio ponderado. <=0 ignora a entrada.")]
        public int Weight = 1;

        [Tooltip("Raridade base: 0=Common,1=Uncommon,2=Rare,3=Epic. Perfect sobe 1 passo.")]
        public int Rarity = 0;

        [Tooltip("Qualidade base: 0..3. Perfect sobe 1 passo.")]
        public int BaseQuality = 0;

        [Tooltip("Estações permitidas. Vazio = todas. Aceita Primavera/Verao/Outono/Inverno ou aliases EN.")]
        public string[] Seasons = Array.Empty<string>();

        [Tooltip("Climas permitidos. Vazio = todos. Aceita Clear/Cloudy/Rainy/Stormy.")]
        public string[] Weathers = Array.Empty<string>();

        [Tooltip("Quando true, só é elegível à noite (evento raro como Mirrorfin/Lake Lurker no acude).")]
        public bool NightOnly;
    }
}
