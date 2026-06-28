using UnityEngine;

namespace CindarsHope.Farm
{
    /// <summary>
    /// Farm level 1 layout contract v6 — disposicao Stardew-like com montanha, rio/ponte,
    /// bosque e base a leste. Lagoa 3x maior, fazenda ampliada para 64x44, 3 areas de expansao.
    ///
    /// Coordenadas em world units com ORIGEM CENTRADA (0,0). Norte = +Y.
    /// Tile = 1 Unity unit = 32px.
    /// Bounds v6: x in [-32, 32] (64 tiles de largura), y in [-22, 22] (44 tiles de altura).
    /// Atualizado em 2026-06-26 (spec_farm_scene_relayout_v4 §15.2 v6).
    /// </summary>
    public static class FarmLevel1LayoutContract
    {
        // ── Dimensoes v6 ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Farm level 1 width in tiles (64 tiles, origin-centered: x in [-32, 32]).
        /// </summary>
        public const float Level1WidthTiles = 64f;

        /// <summary>
        /// Farm level 1 height in tiles (44 tiles, origin-centered: y in [-22, 22]).
        /// </summary>
        public const float Level1HeightTiles = 44f;

        /// <summary>
        /// Limite minimo de X (canto esquerdo da fazenda).
        /// </summary>
        public const float MinX = -32f;

        /// <summary>
        /// Limite maximo de X (canto direito da fazenda).
        /// </summary>
        public const float MaxX = 32f;

        /// <summary>
        /// Limite minimo de Y (borda sul da fazenda).
        /// </summary>
        public const float MinY = -22f;

        /// <summary>
        /// Limite maximo de Y (borda norte da fazenda — montanha).
        /// </summary>
        public const float MaxY = 22f;

        // ── Ancora: NORTE — Montanha / Caverna / Minerio ─────────────────────────────────────────

        /// <summary>
        /// Y base da montanha (colisao intransponivel ao norte). Faixa y in [18,22].
        /// </summary>
        public const float MountainBaseY = 18f;

        /// <summary>
        /// Fixed anchor: Cave entrance location X (canto noroeste da montanha). v6: maior (~5x4).
        /// </summary>
        public const float CaveEntranceX = -26f;

        /// <summary>
        /// Fixed anchor: Cave entrance location Y. v6: embutida na montanha.
        /// </summary>
        public const float CaveEntranceY = 18.5f;

        /// <summary>
        /// Fixed anchor: Spawn ao sair da caverna (abaixo do canto NO).
        /// </summary>
        public const float SpawnFromCaveX = -23f;

        /// <summary>
        /// Fixed anchor: Spawn ao sair da caverna Y.
        /// </summary>
        public const float SpawnFromCaveY = 15f;

        // ── Ancora: LESTE — Base / Casa / Saida da Cidade ────────────────────────────────────────

        /// <summary>
        /// Fixed anchor: Portal da cidade (extrema direita, altura central) X. v6: (31,0).
        /// </summary>
        public const float CityExitX = 31f;

        /// <summary>
        /// Fixed anchor: Portal da cidade Y.
        /// </summary>
        public const float CityExitY = 0f;

        /// <summary>
        /// Fixed anchor: Spawn ao chegar da cidade X. v6: (29,0).
        /// </summary>
        public const float SpawnFromTownX = 29f;

        /// <summary>
        /// Fixed anchor: Spawn ao chegar da cidade Y.
        /// </summary>
        public const float SpawnFromTownY = 0f;

        /// <summary>
        /// Fixed anchor: Spawn padrao do jogador (em frente a casa, lado leste da ponte) X. v6: (14,2).
        /// </summary>
        public const float DefaultSpawnX = 14f;

        /// <summary>
        /// Fixed anchor: Spawn padrao do jogador Y.
        /// </summary>
        public const float DefaultSpawnY = 2f;

        /// <summary>
        /// Fixed anchor: Fonte de Anya (respawn + Agua Viva, clareira sul do bosque) X. v6: (-16,8).
        /// </summary>
        public const float FonteAnchorX = -16f;

        /// <summary>
        /// Fixed anchor: Fonte de Anya Y.
        /// </summary>
        public const float FonteAnchorY = 8f;

        /// <summary>
        /// House anchor X (centro da casa WALK-IN). v6: (24,8).
        /// </summary>
        public const float HouseStartX = 24f;

        /// <summary>
        /// House anchor Y.
        /// </summary>
        public const float HouseStartY = 8f;

        /// <summary>
        /// SellPoint anchor X. v6: (16,4).
        /// </summary>
        public const float SellPointStartX = 16f;

        /// <summary>
        /// SellPoint anchor Y.
        /// </summary>
        public const float SellPointStartY = 4f;

        /// <summary>
        /// Fixed anchor: Evolution Board (Quadro de Evolucoes) X. v6: (27,3).
        /// </summary>
        public const float EvolutionBoardX = 27f;

        /// <summary>
        /// Fixed anchor: Evolution Board Y.
        /// </summary>
        public const float EvolutionBoardY = 3f;

        // ── Ancora: SUDESTE — Agua (acude, rio, ponte, lago) ─────────────────────────────────────

        /// <summary>
        /// Fixed anchor: Centro do lago v6 (~28x17, 3x maior). Centro (18,-12).
        /// </summary>
        public const float LakeCenterX = 18f;

        /// <summary>
        /// Fixed anchor: Centro do lago Y.
        /// </summary>
        public const float LakeCenterY = -12f;

        /// <summary>
        /// Fixed anchor: Largura do lago em tiles (~28x17, organico). v6: spans x[4,32].
        /// </summary>
        public const float LakeWidthTiles = 28f;

        /// <summary>
        /// Fixed anchor: Altura do lago em tiles. v6: spans y[-20.5,-3.5].
        /// </summary>
        public const float LakeHeightTiles = 17f;

        /// <summary>
        /// Fixed anchor: Centro da ponte (trilha farm_default<->centro) X. v6: (11,2).
        /// </summary>
        public const float BridgeCenterX = 11f;

        /// <summary>
        /// Fixed anchor: Centro da ponte Y.
        /// </summary>
        public const float BridgeCenterY = 2f;

        // ── Ancora: AREAS DE EXPANSAO RESERVADAS (v6) ───────────────────────────────────────────

        /// <summary>
        /// Expansao Norte — centro X. Nao-aravel ate desbloquear.
        /// </summary>
        public const float ExpNorthX = -6f;

        /// <summary>
        /// Expansao Norte — centro Y.
        /// </summary>
        public const float ExpNorthY = 17f;

        /// <summary>
        /// Expansao Oeste — centro X.
        /// </summary>
        public const float ExpWestX = -30f;

        /// <summary>
        /// Expansao Oeste — centro Y.
        /// </summary>
        public const float ExpWestY = -4f;

        /// <summary>
        /// Expansao Sul — centro X.
        /// </summary>
        public const float ExpSouthX = -16f;

        /// <summary>
        /// Expansao Sul — centro Y.
        /// </summary>
        public const float ExpSouthY = -18f;

        // ── Campos legados removidos / renomeados (stub de compatibilidade) ──────────────────────

        // NOTE: InitialFieldWidthTiles / InitialFieldHeightTiles / InitialFieldStartX /
        //       InitialFieldStartY nao se aplicam mais (solo aravel e por tile — Spec B).
        //       CaveEntranceX/Y e CityExitX/Y foram remapeados para world-units centradas acima.

        // ── Validacoes ───────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Valida que as dimensoes passadas correspondem ao contrato v6 (64x44).
        /// </summary>
        public static bool IsLevel1SizeValid(float widthTiles, float heightTiles)
        {
            return Mathf.Approximately(widthTiles, Level1WidthTiles) &&
                   Mathf.Approximately(heightTiles, Level1HeightTiles);
        }

        /// <summary>
        /// Valida que a Fonte de Anya esta dentro dos bounds.
        /// </summary>
        public static bool IsFonteInBounds()
        {
            return FonteAnchorX >= MinX && FonteAnchorX < MaxX &&
                   FonteAnchorY >= MinY && FonteAnchorY < MaxY;
        }

        /// <summary>
        /// Valida que o lago esta dentro dos bounds.
        /// </summary>
        public static bool IsLakeInBounds()
        {
            float lakeStartX = LakeCenterX - LakeWidthTiles / 2f;
            float lakeStartY = LakeCenterY - LakeHeightTiles / 2f;
            float lakeEndX = lakeStartX + LakeWidthTiles;
            float lakeEndY = lakeStartY + LakeHeightTiles;

            return lakeStartX >= MinX && lakeEndX <= MaxX &&
                   lakeStartY >= MinY && lakeEndY <= MaxY;
        }

        /// <summary>
        /// Valida que a entrada da caverna esta dentro dos bounds.
        /// </summary>
        public static bool IsCaveEntranceInBounds()
        {
            return CaveEntranceX >= MinX && CaveEntranceX < MaxX &&
                   CaveEntranceY >= MinY && CaveEntranceY < MaxY;
        }

        /// <summary>
        /// Valida que a saida da cidade esta dentro dos bounds.
        /// </summary>
        public static bool IsCityExitAccessible()
        {
            return CityExitX >= MinX && CityExitX < MaxX &&
                   CityExitY >= MinY && CityExitY < MaxY;
        }

        /// <summary>
        /// Valida que a montanha esta no topo (acima de MountainBaseY).
        /// </summary>
        public static bool IsMountainInBounds()
        {
            return MountainBaseY > 0f && MountainBaseY < MaxY;
        }

        /// <summary>
        /// Valida que a ponte esta dentro dos bounds.
        /// </summary>
        public static bool IsBridgeInBounds()
        {
            return BridgeCenterX >= MinX && BridgeCenterX < MaxX &&
                   BridgeCenterY >= MinY && BridgeCenterY < MaxY;
        }
    }
}
