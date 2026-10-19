using System.Collections.Generic;

namespace CindarsHope.World.Altars
{
    /// <summary>
    /// fable_68 — sorteio 100% DETERMINÍSTICO e PURO (sem Unity) de QUAL Marca de caverna (ou nenhuma)
    /// nasce num dado nível, dado o <c>CaveRunSeed</c> + nível. Usa SÓ o FNV-1a 32-bit canônico do
    /// projeto (mesmo algoritmo de <see cref="CindarsHope.Cave.Generation.CaveLayoutStableHash"/> /
    /// WorldEventResolver — replicado aqui para que World não dependa de Cave) — NUNCA GUID/timestamp/
    /// Random não-semeado (rule cave-stable-run / ADR-0005). Mesma entrada ⇒ mesma saída em revisita
    /// (CA-4): o nível mostra a MESMA Marca no MESMO lugar.
    ///
    /// NÃO cria um segundo spawner de special room: é um resolver puro que a integração da F22 consome
    /// para decidir o conteúdo "god_mark" do nível. A raridade (0-1 por nível) é parametrizável.
    /// </summary>
    public static class GodMarkCaveResolver
    {
        /// <summary>Salt fixo do domínio (separa este sorteio de qualquer outro com a mesma seed).</summary>
        public const string Salt = "god_mark";

        /// <summary>Denominador da chance de aparecer uma Marca por nível (1 em N). Default 6 ≈ 16% (raro).</summary>
        public const int DefaultRarityDenominator = 6;

        /// <summary>FNV-1a 32-bit — idêntico ao canônico do projeto.</summary>
        public static int StableHash(string value)
        {
            unchecked
            {
                const int fnvOffset = (int)2166136261;
                const int fnvPrime = 16777619;
                var hash = fnvOffset;
                foreach (var c in value ?? string.Empty)
                {
                    hash ^= c;
                    hash *= fnvPrime;
                }

                return hash == int.MinValue ? 0 : hash;
            }
        }

        /// <summary>
        /// Resolve a Marca de caverna do nível (ou null se nenhuma). Determinístico por
        /// (<paramref name="caveRunSeed"/>, <paramref name="caveLevel"/>). Só Marcas cuja banda de
        /// profundidade contém <paramref name="caveLevel"/> são elegíveis (Nyx 71-85, Tandra 11-25 etc.).
        /// </summary>
        public static GodMarkDefinition Resolve(
            string caveRunSeed, int caveLevel, int rarityDenominator = DefaultRarityDenominator)
        {
            if (caveLevel < 1) return null;
            if (rarityDenominator < 1) rarityDenominator = 1;

            // 1) Roll de presença (raro): mesma seed/nível ⇒ mesma decisão.
            var presenceKey = $"{Salt}|presence|{caveRunSeed ?? string.Empty}|{caveLevel}";
            var presenceRoll = (int)((uint)StableHash(presenceKey) % (uint)rarityDenominator);
            if (presenceRoll != 0) return null; // sem Marca neste nível

            // 2) Filtra elegíveis pela banda de profundidade.
            var eligible = new List<GodMarkDefinition>();
            foreach (var def in GodMarkCatalog.CaveMarks())
            {
                if (caveLevel >= def.CaveBandMin && caveLevel <= def.CaveBandMax)
                {
                    eligible.Add(def);
                }
            }

            if (eligible.Count == 0) return null;

            // 3) Escolhe entre os elegíveis (determinístico). Ordena por id para estabilidade total.
            eligible.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            var pickKey = $"{Salt}|pick|{caveRunSeed ?? string.Empty}|{caveLevel}";
            var pick = (int)((uint)StableHash(pickKey) % (uint)eligible.Count);
            return eligible[pick];
        }
    }
}
