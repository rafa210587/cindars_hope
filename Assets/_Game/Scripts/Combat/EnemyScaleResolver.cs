using UnityEngine;

namespace CindarsHope.Combat
{
    /// <summary>
    /// Pure scale math shared by every enemy spawn path (CaveRuntimeMaterializer, CaveEnemySpawner,
    /// CaveBossSpawner). Extracted so the rules are EditMode-testable and live in ONE place instead of being
    /// duplicated (and drifting) across MonoBehaviours.
    /// </summary>
    public static class EnemyScaleResolver
    {
        // ── fable_79: Eixo player-relative unificado ──────────────────────────────────────────
        //
        // O player é a régua de todo o jogo. VisualScale = PlayerReferenceScale × playerRelative(size) × role.
        // PlayerReferenceScale = 2.0 (mesma constante de CreateDefaultScaleAssets.PlayerReferenceScale).
        // Os ratios abaixo alinham com os VisualScaleProfileSO do mundo (Farm/Town) para que a leitura
        // de ameaça seja consistente em qualquer cena. Gargantuan (ausente do mundo) recebe ratio 3.0
        // para ficar acima de Huge (2.0) sem ultrapassar o boss Huge (2.0 × 2.5 = 5.0).
        //
        // Referência: CreateDefaultScaleAssets.cs — EnemyMedium=1.00, EnemyLarge=1.50, EnemyHuge=2.00.
        //
        // rule: no-magic-balance-values — todos os literais de balance estão aqui como consts nomeadas,
        // nunca soltos no call site.

        /// <summary>Régua global: 1 unidade de player = este valor de escala Unity.</summary>
        public const float PlayerReferenceScale = 2.0f;

        /// <summary>
        /// Tabela player-relative por <see cref="BestiarySizeClass"/>.
        /// Multiplicar por <see cref="PlayerReferenceScale"/> dá a escala Unity absoluta de um inimigo comum.
        /// </summary>
        public static class PlayerRelativeScale
        {
            public const float Tiny       = 0.50f;
            public const float Small      = 0.75f;
            public const float Medium     = 1.00f; // mesmo tamanho que o player
            public const float Large      = 1.50f;
            public const float Huge       = 2.00f; // 2× o player
            public const float Gargantuan = 3.00f; // 3× o player (acima de Huge, abaixo de boss Gargantuan)
        }

        /// <summary>
        /// Multiplicadores de função aplicados sobre a escala base (size × player-relative).
        /// Alinhados ao gerador canônico (GenerateCanonicalBestiary §2.1).
        /// </summary>
        public static class RoleScaleMultiplier
        {
            public const float Common   = 1.0f;
            public const float MiniBoss = 1.5f;
            public const float Boss     = 2.5f;
        }

        // ─────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Fórmula canônica de escala visual de inimigo da caverna (fable_79).
        /// Resultado = <see cref="PlayerReferenceScale"/> × playerRelative(size) × roleMultiplier.
        /// Puro e determinístico — seguro para EditMode tests e para o stable-run (ADR-0005).
        /// </summary>
        /// <param name="size">Classe de tamanho natural do inimigo.</param>
        /// <param name="isMiniBoss">Inimigo é miniboss (role × 1.5)?</param>
        /// <param name="isBoss">Inimigo é boss (role × 2.5)? Prevalece sobre isMiniBoss.</param>
        /// <returns>Escala Unity a aplicar em transform.localScale (uniform X=Y).</returns>
        public static float ResolveVisualScale(BestiarySizeClass size, bool isMiniBoss, bool isBoss)
        {
            float relative = PlayerRelativeRatioFor(size);
            float role     = isBoss     ? RoleScaleMultiplier.Boss
                           : isMiniBoss ? RoleScaleMultiplier.MiniBoss
                           :              RoleScaleMultiplier.Common;
            return PlayerReferenceScale * relative * role;
        }

        /// <summary>Retorna o ratio player-relative para uma size class (sem multiplicador de role).</summary>
        public static float PlayerRelativeRatioFor(BestiarySizeClass size)
        {
            switch (size)
            {
                case BestiarySizeClass.Tiny:       return PlayerRelativeScale.Tiny;
                case BestiarySizeClass.Small:      return PlayerRelativeScale.Small;
                case BestiarySizeClass.Large:      return PlayerRelativeScale.Large;
                case BestiarySizeClass.Huge:       return PlayerRelativeScale.Huge;
                case BestiarySizeClass.Gargantuan: return PlayerRelativeScale.Gargantuan;
                default:                           return PlayerRelativeScale.Medium; // Medium
            }
        }

        // ── Legacy / boss config path (CaveBossSpawner) ──────────────────────────────────────

        /// <summary>
        /// Escala visual de um boss via config explícita (CaveBossSpawner legacy path).
        /// Preferir <see cref="ResolveVisualScale"/> para novos call sites (fable_79).
        /// The bestiary generator bakes the natural size × boss multiplier into
        /// <paramref name="dataVisualScale"/>, so a Gargantuan boss reads ~7.5 and a Medium boss ~2.5 — that
        /// override wins. Only when the data still carries the EnemyDataSO default (1.0) do we fall back to the
        /// flat global config scale. The result is floored at <paramref name="bossMin"/> so a boss never reads
        /// sub-boss; the upper end is intentionally NOT clamped to <paramref name="bossMax"/>, otherwise large
        /// bosses get shrunk back to the global ceiling.
        /// </summary>
        public static float ResolveBossScale(float dataVisualScale, float configBossScale, float bossMin, float bossMax)
        {
            var fallback = Mathf.Clamp(configBossScale, bossMin, bossMax);
            return dataVisualScale > 1.01f ? Mathf.Max(dataVisualScale, bossMin) : fallback;
        }

        /// <summary>
        /// Physical collider radius for a natural size class. Single source of truth for the legacy spawner and
        /// the materializer's fallback (which resolves the same numbers from a SizeProfileId string).
        /// </summary>
        public static float ColliderRadiusFor(BestiarySizeClass size)
        {
            switch (size)
            {
                case BestiarySizeClass.Tiny: return 0.25f;
                case BestiarySizeClass.Small: return 0.35f;
                case BestiarySizeClass.Large: return 0.65f;
                case BestiarySizeClass.Huge: return 0.95f;
                case BestiarySizeClass.Gargantuan: return 1.2f;
                default: return 0.45f; // Medium
            }
        }
    }
}
