namespace CindarsHope.Cave.Death
{
    /// <summary>
    /// fable_66 — helper PURO (sem UnityEngine) que resolve os carimbos de morte de um corpse:
    /// dia/hora reais do owner de tempo e o hash de layout determinístico do cave runtime.
    /// Extraído do CaveDeathResolver para que o fechamento dos TODOs seja testável em EditMode.
    /// </summary>
    public static class CaveCorpseStamp
    {
        /// <summary>
        /// Dia real do owner de tempo (TimeManager). Floor 1: o jogo nunca tem dia &lt; 1, então um
        /// owner ausente/zerado degrada para o dia inicial (1) em vez de gravar 0 corrompido.
        /// </summary>
        public static int ResolveGameDay(int currentDayFromTimeOwner)
        {
            return currentDayFromTimeOwner < 1 ? 1 : currentDayFromTimeOwner;
        }

        /// <summary>
        /// Hora real intra-dia. O projeto ainda não tem relógio intra-dia canônico (TimeManager só
        /// expõe CurrentDay); o owner repassa 0 até existir. Normaliza para [0,24) e nunca negativo.
        /// </summary>
        public static float ResolveGameTime(float timeOfDayFromOwner)
        {
            if (timeOfDayFromOwner < 0f)
            {
                return 0f;
            }

            return timeOfDayFromOwner >= 24f ? timeOfDayFromOwner % 24f : timeOfDayFromOwner;
        }

        /// <summary>
        /// Hash de layout determinístico (ADR-0005 / cave-stable-run): MESMO algoritmo estável do resto
        /// da geração (CaveLayoutStableHash, FNV-1a). Derivado de worldSeed|runSeed|level|"layout" — sem
        /// GUID/timestamp, idêntico em revisita. Seeds vazias ⇒ string vazia (sem run ativa = sem hash).
        /// </summary>
        public static string ResolveLayoutHash(string caveWorldSeed, string caveRunSeed, int caveLevel)
        {
            if (string.IsNullOrEmpty(caveWorldSeed) && string.IsNullOrEmpty(caveRunSeed))
            {
                return string.Empty;
            }

            var level = caveLevel < 1 ? 1 : caveLevel;
            var hash = Generation.CaveLayoutStableHash.Compute(
                $"{caveWorldSeed}|{caveRunSeed}|{level}|layout");
            return hash.ToString("x8");
        }
    }
}
