namespace CindarsHope.NPC.Services
{
    /// <summary>
    /// fable_25 — definição PURA de um serviço único de NPC (id, npcId, gates, custo, efeito). Sem
    /// Unity refs: é EditMode-testável e usada tanto pelo <see cref="NpcServiceCatalog"/> quanto pelo
    /// <see cref="NpcServiceExecutor"/>. Os IDs de NPC são os do registry canônico
    /// (<see cref="NpcTownRosterRegistry"/>); os preços são os canônicos do escopo da spec/ITEM_CATALOG.
    /// </summary>
    public sealed class NpcServiceDefinition
    {
        /// <summary>ID estável do serviço (ex.: "service_thalindra_analysis").</summary>
        public string ServiceId;

        /// <summary>ID canônico do NPC provedor (ex.: "npc_thalindra"). Join key do roster/diálogo.</summary>
        public string NpcId;

        /// <summary>Rótulo base da opção no Conversar (via LocalizationService quando disponível).</summary>
        public string DisplayLabel;

        /// <summary>Efeito mecânico tipado, despachado ao sistema-alvo pelo executor.</summary>
        public NpcServiceEffectType Effect;

        // ── Gates (descoberta > ocultação: a opção aparece DESABILITADA com o motivo) ──────────────

        /// <summary>Nível mínimo de amizade (F26). 0 = sem gate de amizade.</summary>
        public int FriendshipMin;

        /// <summary>Flag de quest necessária (QuestFlagService). null/empty = sem gate de flag.</summary>
        public string RequiredQuestFlag;

        // ── Custo ──────────────────────────────────────────────────────────────────────────────────

        /// <summary>Custo em ouro (sink de economia). 0 = grátis.</summary>
        public int GoldCost;

        /// <summary>Item consumido junto ao custo (ex.: "1 parte de monstro"). null/empty = nenhum.</summary>
        public string RequiredItemId;

        /// <summary>Quantidade do item consumido (>=1 quando RequiredItemId está definido).</summary>
        public int RequiredItemAmount;

        public bool HasFriendshipGate => FriendshipMin > 0;
        public bool HasFlagGate => !string.IsNullOrEmpty(RequiredQuestFlag);
        public bool HasItemCost => !string.IsNullOrEmpty(RequiredItemId) && RequiredItemAmount > 0;
    }
}
