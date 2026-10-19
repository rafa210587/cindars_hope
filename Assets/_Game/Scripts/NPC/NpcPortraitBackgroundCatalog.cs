using System.Collections.Generic;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Mapeia cada NPC ao fundo (por papel/local) do painel de retrato. Nome do fundo = arquivo em
    /// Resources/NpcPortraitBackgrounds/&lt;nome&gt;.png. Retorna null se o NPC nao tiver fundo mapeado.
    /// </summary>
    public static class NpcPortraitBackgroundCatalog
    {
        private const string Praca = "bg_praca";
        private const string Mercado = "bg_mercado";
        private const string Taverna = "bg_taverna";
        private const string Oficina = "bg_oficina";
        private const string Beira = "bg_beira";

        private static readonly Dictionary<string, string> ByNpcId = new Dictionary<string, string>
        {
            { "npc_mara", Praca }, { "npc_alaric", Praca }, { "npc_velorin", Praca },
            { "npc_tovin", Praca }, { "npc_thalindra", Praca }, { "npc_hund", Praca },
            { "npc_renko", Mercado }, { "npc_pip", Mercado }, { "npc_mirela", Mercado }, { "npc_sylveth", Mercado },
            { "npc_gruta", Taverna }, { "npc_orlan", Taverna }, { "npc_mella", Taverna }, { "npc_liora", Taverna },
            { "npc_brumdar", Oficina }, { "npc_dagna", Oficina }, { "npc_nimble", Oficina },
            { "npc_gurd", Oficina }, { "npc_ozzra", Oficina }, { "npc_hess", Oficina },
            { "npc_eiran", Beira }, { "npc_savra", Beira }, { "npc_sael", Beira },
            { "npc_zrix", Beira }, { "npc_yael", Beira }, { "npc_maelor", Beira }, { "npc_tibbet", Beira },
        };

        public static string BackgroundFor(string npcId)
        {
            if (string.IsNullOrEmpty(npcId)) return null;
            return ByNpcId.TryGetValue(npcId, out var bg) ? bg : null;
        }
    }
}
