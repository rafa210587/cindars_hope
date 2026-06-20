namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_50 — publicado quando o resolver de pesca v2 conclui uma captura (após o minigame de
    /// timing). Complementa <see cref="FishCaughtEvent"/> (que carrega item/quantidade/posição para o
    /// HUD legado) adicionando a tabela de origem, a raridade e a nota do minigame, para HUD/quests/
    /// bestiário que precisam diferenciar Perfect/Good e raro/comum.
    ///
    /// NÃO substitui FishCaughtEvent — ambos são publicados na captura bem-sucedida.
    /// Sem referências Unity (tipos simples + IDs), conforme regra do GameEventBus.
    /// </summary>
    public readonly struct FishCatchResolvedEvent
    {
        /// <summary>Id estável do spot (farm spotId ou FishingSpotId do snapshot de caverna).</summary>
        public string SpotId { get; }

        /// <summary>Id estável da <c>FishingTableSO</c> consultada.</summary>
        public string TableId { get; }

        /// <summary>Item de peixe entregue (ex.: item_fish_mirrorfin).</summary>
        public string ItemId { get; }

        /// <summary>Raridade resolvida (0=Common,1=Uncommon,2=Rare,3=Epic; valor já com bônus de Perfect).</summary>
        public int Rarity { get; }

        /// <summary>Qualidade base resolvida (0..3; valor já com bônus de Perfect).</summary>
        public int Quality { get; }

        /// <summary>Nota do minigame: 0=Miss, 1=Good, 2=Perfect.</summary>
        public int TimingGrade { get; }

        public FishCatchResolvedEvent(string spotId, string tableId, string itemId, int rarity, int quality, int timingGrade)
        {
            SpotId = spotId ?? string.Empty;
            TableId = tableId ?? string.Empty;
            ItemId = itemId ?? string.Empty;
            Rarity = rarity;
            Quality = quality;
            TimingGrade = timingGrade;
        }
    }
}
