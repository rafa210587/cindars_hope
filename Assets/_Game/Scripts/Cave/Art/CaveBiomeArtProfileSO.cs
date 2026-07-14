using System.Collections.Generic;
using CindarsHope.Cave.Generation;
using CindarsHope.Core.Data;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CindarsHope.Cave.Art
{
    /// <summary>
    /// spec_cave_biome_art_profiles_runtime (CV01) — contrato de dados data-driven da apresentação
    /// visual de UM bioma da caverna (1 asset por bioma do <see cref="CaveBiomeRegistrySO"/>, 8 no
    /// total). TODO campo é OPCIONAL: profile vazio = fallback integral aos placeholders atuais nos
    /// materializers (CaveBiomeArtResolver retorna false para tudo). Preenchido pelo gerador de
    /// editor (GenerateCaveBiomeArtProfiles) a partir da convenção de pasta
    /// Art/Generated/World/cave/&lt;biomeId&gt;/ quando a arte existir — nunca por edição manual de YAML.
    ///
    /// Nenhuma referência aqui influencia layout/spawn/loot/snapshot (cave-stable-run/ADR-0005):
    /// este asset é puramente de apresentação, consumido pelo CaveBiomeArtResolver (C# puro).
    /// </summary>
    [CreateAssetMenu(fileName = "CaveBiomeArtProfile", menuName = "CindarsHope/Cave/Biome Art Profile")]
    public sealed class CaveBiomeArtProfileSO : ScriptableObject, IIdentifiedData
    {
        [Header("Identidade (deve casar com CaveBiomeRegistrySO)")]
        [SerializeField] private string _biomeId;
        [SerializeField] private int _bandId = 1;

        [Header("Terreno (Tilemap) — todos opcionais")]
        [SerializeField] private TileBase[] _floorTiles = System.Array.Empty<TileBase>();
        [SerializeField] private TileBase _floorDetailTile;
        [SerializeField] private TileBase _wallFaceTile;
        [SerializeField] private TileBase _wallTopTile;

        [Header("Hazards de tile (CaveHazardKind) — opcionais")]
        [SerializeField] private Sprite _toxicPoolSprite;
        [SerializeField] private Sprite _iceSlickSprite;
        [SerializeField] private Sprite _fallingRockSprite;

        [Header("Armadilhas — mapa trapId -> sprite (opcional, entradas ausentes = fallback)")]
        [SerializeField] private TrapSpriteEntry[] _trapSprites = System.Array.Empty<TrapSpriteEntry>();

        [Header("Baú de tesouro / baú falso — opcionais")]
        [SerializeField] private Sprite _chestClosedSprite;
        [SerializeField] private Sprite _chestOpenSprite;
        [SerializeField] private Sprite _falseChestRevealedSprite;

        [Header("Saídas — opcionais")]
        [SerializeField] private Sprite _exitDownSprite;
        [SerializeField] private Sprite _exitUpSprite;

        [Header("Ambiente — dados only nesta spec (sem consumo de luz/audio)")]
        [SerializeField] private Color _ambientLightColor = Color.white;
        [SerializeField] private float _ambientLightIntensity = 1f;
        [SerializeField] private string _musicTrackId = string.Empty;

        [Header("Decor ambiental (fable_78) — pools por Kind, opcionais (DEPRECATED: mantidos só para leitura legada; CV03 usa os pools por CONTEXTO abaixo)")]
        [Tooltip("spec_cave_decor_placement_runtime (CV02): pool de sprites para CaveEnvironmentElementKind.DecorNonBlocking. Pick determinístico por hash estável da posição no CaveBiomeArtResolver.")]
        [SerializeField] private Sprite[] _decorNonBlockingSprites = System.Array.Empty<Sprite>();
        [Tooltip("spec_cave_decor_placement_runtime (CV02): pool de sprites para CaveEnvironmentElementKind.DecorBlocking.")]
        [SerializeField] private Sprite[] _decorBlockingSprites = System.Array.Empty<Sprite>();

        [Header("Borda de rocha (spec_cave_visual_polish_runtime, CV04) — overlay determinístico, opcional")]
        [Tooltip("4 peças em ORDEM FIXA: [0]=top (parede com sul walkable), [1]=side (parede com leste/oeste " +
                 "walkable, espelhado em X para o lado oeste), [2]=cornerA (sul+leste), [3]=cornerB " +
                 "(sul+oeste). Array com tamanho != 4 é tratado como ausente (fallback: sem overlay).")]
        [SerializeField] private Sprite[] _wallEdgeSprites = System.Array.Empty<Sprite>();

        [Header("Cascalho de chão denso (CV04) — pool flat, pick determinístico por hash da célula")]
        [SerializeField] private Sprite[] _groundScatterSprites = System.Array.Empty<Sprite>();

        [Header("Decor de superfície de parede (CV04) — musgo/vegetação na base de parede")]
        [SerializeField] private Sprite[] _wallSurfaceSprites = System.Array.Empty<Sprite>();

        [Header("Feixe de luz fake perto da entrada (CV04) — opcional")]
        [SerializeField] private Sprite _lightShaftSprite;

        [Header("Decor ambiental por CONTEXTO (spec_cave_decor_composition_runtime, CV03) — opcionais")]
        [Tooltip("CV03: pool de sprites para CaveDecorPlacementContext.CeilingHang (decor de teto — estalactites). Pick determinístico por hash estável da posição no CaveBiomeArtResolver.")]
        [SerializeField] private Sprite[] _ceilingSprites = System.Array.Empty<Sprite>();
        [Tooltip("CV03: pool de sprites para CaveDecorPlacementContext.WallHug (decor encostado em parede — carrinho, entulho, minério de parede).")]
        [SerializeField] private Sprite[] _wallHugSprites = System.Array.Empty<Sprite>();
        [Tooltip("CV03: pool de sprites para CaveDecorPlacementContext.FloorCluster (decor de chão aberto, colocado em clusters — cogumelo, picareta, tábua, poça).")]
        [SerializeField] private Sprite[] _floorClusterSprites = System.Array.Empty<Sprite>();
        [Tooltip("CV03: pool de sprites para decor BLOQUEANTE (ocupa colisão) — usado independente do contexto de célula onde caiu (hoje só chão/parede, nunca teto).")]
        [SerializeField] private Sprite[] _blockingSprites = System.Array.Empty<Sprite>();

        public string Id
        {
            get => _biomeId;
            set => _biomeId = value;
        }

        public string BiomeId => _biomeId;
        public int BandId => _bandId;

        public TileBase[] FloorTiles => _floorTiles ?? System.Array.Empty<TileBase>();
        public TileBase FloorDetailTile => _floorDetailTile;
        public TileBase WallFaceTile => _wallFaceTile;
        public TileBase WallTopTile => _wallTopTile;

        public Sprite ToxicPoolSprite => _toxicPoolSprite;
        public Sprite IceSlickSprite => _iceSlickSprite;
        public Sprite FallingRockSprite => _fallingRockSprite;

        public TrapSpriteEntry[] TrapSprites => _trapSprites ?? System.Array.Empty<TrapSpriteEntry>();

        public Sprite ChestClosedSprite => _chestClosedSprite;
        public Sprite ChestOpenSprite => _chestOpenSprite;
        public Sprite FalseChestRevealedSprite => _falseChestRevealedSprite;

        public Sprite ExitDownSprite => _exitDownSprite;
        public Sprite ExitUpSprite => _exitUpSprite;

        public Color AmbientLightColor => _ambientLightColor;
        public float AmbientLightIntensity => _ambientLightIntensity;
        public string MusicTrackId => _musicTrackId ?? string.Empty;

        public IReadOnlyList<Sprite> DecorNonBlockingSprites => _decorNonBlockingSprites ?? System.Array.Empty<Sprite>();
        public IReadOnlyList<Sprite> DecorBlockingSprites => _decorBlockingSprites ?? System.Array.Empty<Sprite>();

        public IReadOnlyList<Sprite> CeilingSprites => _ceilingSprites ?? System.Array.Empty<Sprite>();
        public IReadOnlyList<Sprite> WallHugSprites => _wallHugSprites ?? System.Array.Empty<Sprite>();
        public IReadOnlyList<Sprite> FloorClusterSprites => _floorClusterSprites ?? System.Array.Empty<Sprite>();
        public IReadOnlyList<Sprite> BlockingSprites => _blockingSprites ?? System.Array.Empty<Sprite>();

        public IReadOnlyList<Sprite> WallEdgeSprites => _wallEdgeSprites ?? System.Array.Empty<Sprite>();
        public IReadOnlyList<Sprite> GroundScatterSprites => _groundScatterSprites ?? System.Array.Empty<Sprite>();
        public IReadOnlyList<Sprite> WallSurfaceSprites => _wallSurfaceSprites ?? System.Array.Empty<Sprite>();
        public Sprite LightShaftSprite => _lightShaftSprite;

        /// <summary>Setter editor-only usado pelo gerador (GenerateCaveBiomeArtProfiles) para popular
        /// o profile a partir da convenção de pasta. Não usar em runtime.</summary>
        public void EditorSetBandId(int bandId) => _bandId = bandId;

        public void EditorSetFloorTiles(TileBase[] tiles) => _floorTiles = tiles ?? System.Array.Empty<TileBase>();
        public void EditorSetFloorDetailTile(TileBase tile) => _floorDetailTile = tile;
        public void EditorSetWallTiles(TileBase face, TileBase top)
        {
            _wallFaceTile = face;
            _wallTopTile = top;
        }

        /// <summary>Setter editor-only (lote 2 de arte) — sprites de hazard de tile
        /// (CaveHazardKind), populados pelo gerador a partir da convenção de pasta.</summary>
        public void EditorSetHazardSprites(Sprite toxicPool, Sprite iceSlick, Sprite fallingRock)
        {
            _toxicPoolSprite = toxicPool;
            _iceSlickSprite = iceSlick;
            _fallingRockSprite = fallingRock;
        }

        /// <summary>Setter editor-only (lote 2 de arte) — sprites de baú de tesouro / baú falso.</summary>
        public void EditorSetChestSprites(Sprite closed, Sprite open, Sprite falseRevealed)
        {
            _chestClosedSprite = closed;
            _chestOpenSprite = open;
            _falseChestRevealedSprite = falseRevealed;
        }

        /// <summary>Setter editor-only (lote 2 de arte) — sprites de saída (forward/back exit).</summary>
        public void EditorSetExitSprites(Sprite exitDown, Sprite exitUp)
        {
            _exitDownSprite = exitDown;
            _exitUpSprite = exitUp;
        }

        /// <summary>Setter editor-only (spec_cave_decor_placement_runtime, CV02) — pools de decor
        /// ambiental do fable_78, usados pelo CaveEnvironmentElementMaterializer via
        /// CaveBiomeArtResolver.TryGetDecorSprite. Não usar em runtime.</summary>
        public void EditorSetDecorSprites(Sprite[] nonBlocking, Sprite[] blocking)
        {
            _decorNonBlockingSprites = nonBlocking ?? System.Array.Empty<Sprite>();
            _decorBlockingSprites = blocking ?? System.Array.Empty<Sprite>();
        }

        /// <summary>Setter editor-only (spec_cave_decor_composition_runtime, CV03) — pools de decor por
        /// CONTEXTO (teto/wall-hug/chão/bloqueante), usados pelo CaveEnvironmentElementMaterializer via
        /// CaveBiomeArtResolver.TryGetDecorSprite(context). Não usar em runtime.</summary>
        public void EditorSetContextDecorSprites(Sprite[] ceiling, Sprite[] wallHug, Sprite[] floorCluster, Sprite[] blocking)
        {
            _ceilingSprites = ceiling ?? System.Array.Empty<Sprite>();
            _wallHugSprites = wallHug ?? System.Array.Empty<Sprite>();
            _floorClusterSprites = floorCluster ?? System.Array.Empty<Sprite>();
            _blockingSprites = blocking ?? System.Array.Empty<Sprite>();
        }

        /// <summary>Setter editor-only (spec_cave_visual_polish_runtime, CV04) — 4 peças de overlay de
        /// borda de rocha em ORDEM FIXA (top/side/cornerA/cornerB). Não usar em runtime.</summary>
        public void EditorSetWallEdgeSprites(Sprite top, Sprite side, Sprite cornerA, Sprite cornerB)
        {
            _wallEdgeSprites = new[] { top, side, cornerA, cornerB };
        }

        /// <summary>Setter editor-only (CV04) — pool flat de cascalho/litter denso de chão. Não usar em runtime.</summary>
        public void EditorSetGroundScatterSprites(Sprite[] sprites)
        {
            _groundScatterSprites = sprites ?? System.Array.Empty<Sprite>();
        }

        /// <summary>Setter editor-only (CV04) — pool de musgo/vegetação de base de parede. Não usar em runtime.</summary>
        public void EditorSetWallSurfaceSprites(Sprite[] sprites)
        {
            _wallSurfaceSprites = sprites ?? System.Array.Empty<Sprite>();
        }

        /// <summary>Setter editor-only (CV04) — sprite único do feixe de luz fake perto da entrada. Não usar em runtime.</summary>
        public void EditorSetLightShaftSprite(Sprite lightShaft)
        {
            _lightShaftSprite = lightShaft;
        }

        [System.Serializable]
        public struct TrapSpriteEntry
        {
            public string TrapId;
            public Sprite Sprite;
        }
    }
}
