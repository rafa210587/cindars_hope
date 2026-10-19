using System;

namespace CindarsHope.Farm
{
    /// <summary>
    /// fable_55 — regra PURA de validação de estação no plantio (ponto único; sem Unity).
    ///
    /// Política v1 (diff mínimo, regressão segura):
    /// - Semente SEM season tags ⇒ plantável o ano todo (comportamento atual preservado: as
    ///   sementes/itens existentes não declaram SeasonTags, então o gate é no-op para elas).
    /// - Estação atual desconhecida (sem calendário) ⇒ permitido (não bloquear sem informação).
    /// - Semente COM tags e estação atual fora delas ⇒ RECUSADO, exceto se o canteiro pode
    ///   sobrepor estação (estufa, via GreenhouseContextProvider.CanOverrideSeason) ⇒ permitido.
    ///
    /// O casamento de tags é case-insensitive e aceita tanto os nomes do enum Season
    /// (Primavera/Verao/Outono/Inverno) quanto aliases comuns em inglês (Spring/Summer/Autumn-Fall/
    /// Winter), para robustez frente à autoria das tags.
    /// </summary>
    public static class FarmSeasonGate
    {
        public static bool IsPlantingAllowed(string[] seasonTags, string currentSeason, bool canOverrideSeason)
        {
            // Estufa: ignora estação sempre.
            if (canOverrideSeason)
                return true;

            // Sem restrição de estação na semente ⇒ ano todo.
            if (seasonTags == null || seasonTags.Length == 0)
                return true;

            // Sem informação de estação ⇒ não bloquear.
            if (string.IsNullOrWhiteSpace(currentSeason))
                return true;

            foreach (var tag in seasonTags)
            {
                if (SeasonMatches(tag, currentSeason))
                    return true;
            }

            return false;
        }

        private static bool SeasonMatches(string tag, string currentSeason)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return false;

            var t = tag.Trim();
            if (string.Equals(t, currentSeason, StringComparison.OrdinalIgnoreCase))
                return true;

            // Normaliza ambos os lados para uma chave canônica e compara.
            return NormalizeSeasonKey(t) == NormalizeSeasonKey(currentSeason);
        }

        private static int NormalizeSeasonKey(string season)
        {
            if (string.IsNullOrWhiteSpace(season))
                return -1;

            switch (season.Trim().ToLowerInvariant())
            {
                case "primavera":
                case "spring":
                    return 0;
                case "verao":
                case "verão":
                case "summer":
                    return 1;
                case "outono":
                case "autumn":
                case "fall":
                    return 2;
                case "inverno":
                case "winter":
                    return 3;
                default:
                    return -1;
            }
        }
    }
}
