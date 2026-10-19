using System;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Farm
{
    /// <summary>
    /// Grade de tiles araveis da fazenda. Mapeia coordenadas de tile (TileX,TileY) para instancias
    /// de FarmPlotLogic (pura C#). Conversao world-position <-> tile via FarmScaleContract.TileSizePixels
    /// e os bounds da fazenda. Fornece API IsTillable consumivel pela Spec A.
    /// </summary>
    public class FarmTileGrid
    {
        // Dicionario principal: tiles arados/plantados. Tiles nao-arados nao existem aqui.
        private readonly Dictionary<Vector2Int, FarmPlotLogic> _tilledTiles = new Dictionary<Vector2Int, FarmPlotLogic>();

        // Zonas nao-araveis e estufa (injetado no construtor para que o grid possa consultar).
        private readonly FarmNonArableZones _nonArableZones;

        // Bounds da area aravel da fazenda em coordenadas de tile.
        private int _originTileX;
        private int _originTileY;
        private int _widthTiles;
        private int _heightTiles;

        // Origem do mundo (canto inferior esquerdo da fazenda) em pixels/unidades do Unity.
        private float _worldOriginX;
        private float _worldOriginY;

        /// <summary>
        /// Tamanho do tile em unidades do Unity (reusa FarmScaleContract.TileSizePixels / pixels-por-unidade).
        /// Por padrao assume pixels-per-unit = 32, portanto 1 tile = 1 unidade.
        /// </summary>
        public float TileSizeUnits { get; private set; }

        /// <summary>
        /// Expoe as zonas nao-araveis para que o FarmSceneRuntimeBootstrap (Spec A) possa
        /// registrar footprints de construcoes/agua/montanha e interior de estufa.
        /// </summary>
        public FarmNonArableZones NonArableZones => _nonArableZones;

        public FarmTileGrid(FarmNonArableZones nonArableZones, float tileSizeUnits = 1f)
        {
            _nonArableZones = nonArableZones ?? throw new ArgumentNullException(nameof(nonArableZones));
            TileSizeUnits = tileSizeUnits > 0f ? tileSizeUnits : 1f;
        }

        /// <summary>
        /// Configura os bounds da fazenda em coordenadas de tile e a origem no espaco do mundo.
        /// Deve ser chamado pelo gerador de cena (Spec A) ou por um RuntimeBootstrap.
        /// </summary>
        public void SetBounds(int originTileX, int originTileY, int widthTiles, int heightTiles,
            float worldOriginX = 0f, float worldOriginY = 0f)
        {
            _originTileX = originTileX;
            _originTileY = originTileY;
            _widthTiles = widthTiles;
            _heightTiles = heightTiles;
            _worldOriginX = worldOriginX;
            _worldOriginY = worldOriginY;
        }

        // ── Conversao world <-> tile ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Converte posicao no mundo (Unity units) para coordenadas de tile.
        /// </summary>
        public Vector2Int WorldToTile(float worldX, float worldY)
        {
            var tileX = Mathf.FloorToInt((worldX - _worldOriginX) / TileSizeUnits) + _originTileX;
            var tileY = Mathf.FloorToInt((worldY - _worldOriginY) / TileSizeUnits) + _originTileY;
            return new Vector2Int(tileX, tileY);
        }

        /// <summary>
        /// Converte coordenadas de tile para a posicao do centro do tile no mundo (Unity units).
        /// </summary>
        public Vector2 TileToWorldCenter(int tileX, int tileY)
        {
            var localX = (tileX - _originTileX) * TileSizeUnits + TileSizeUnits * 0.5f;
            var localY = (tileY - _originTileY) * TileSizeUnits + TileSizeUnits * 0.5f;
            return new Vector2(_worldOriginX + localX, _worldOriginY + localY);
        }

        // ── IsTillable (API publica consumida pela Spec A) ───────────────────────────────────────

        /// <summary>
        /// Retorna true se o tile pode ser arado: dentro dos bounds, nao bloqueado por construcao/
        /// agua/montanha. Interior de estufa e aravel (retorna true).
        /// </summary>
        public bool IsTillable(int tileX, int tileY)
        {
            if (!IsWithinBounds(tileX, tileY))
            {
                return false;
            }

            return !_nonArableZones.IsBlocked(tileX, tileY);
        }

        /// <summary>
        /// Retorna true se a posicao no mundo esta dentro de um tile aravel.
        /// </summary>
        public bool IsTillableAtWorldPosition(float worldX, float worldY)
        {
            var tile = WorldToTile(worldX, worldY);
            return IsTillable(tile.x, tile.y);
        }

        // ── Estado dos tiles arados ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Retorna true se o tile esta arado (FarmPlotLogic existe para ele).
        /// </summary>
        public bool IsTilled(int tileX, int tileY)
        {
            return _tilledTiles.ContainsKey(new Vector2Int(tileX, tileY));
        }

        /// <summary>
        /// Cria um FarmPlotLogic para o tile. Retorna false se o tile nao e aravel ou ja esta arado.
        /// </summary>
        public bool TryRegisterTilledTile(int tileX, int tileY, out FarmPlotLogic logic)
        {
            logic = null;
            if (!IsTillable(tileX, tileY))
            {
                return false;
            }

            var key = new Vector2Int(tileX, tileY);
            if (_tilledTiles.TryGetValue(key, out logic))
            {
                // Ja existe — retorna o existente como out, mas false para indicar que nao criou.
                return false;
            }

            logic = new FarmPlotLogic();
            _tilledTiles[key] = logic;
            return true;
        }

        /// <summary>
        /// Obtem o FarmPlotLogic de um tile ja arado. Retorna null se o tile nao esta arado.
        /// </summary>
        public FarmPlotLogic GetTilledLogic(int tileX, int tileY)
        {
            _tilledTiles.TryGetValue(new Vector2Int(tileX, tileY), out var logic);
            return logic;
        }

        /// <summary>
        /// Obtem ou cria um FarmPlotLogic para o tile (para restore de save).
        /// NAO verifica IsTillable — usado exclusivamente pelo save restore.
        /// </summary>
        public FarmPlotLogic GetOrCreateTileLogicForRestore(int tileX, int tileY)
        {
            var key = new Vector2Int(tileX, tileY);
            if (!_tilledTiles.TryGetValue(key, out var logic))
            {
                logic = new FarmPlotLogic();
                _tilledTiles[key] = logic;
            }

            return logic;
        }

        /// <summary>
        /// Remove o FarmPlotLogic de um tile (volta para Raw). Retorna false se nao estava arado.
        /// </summary>
        public bool RemoveTilledTile(int tileX, int tileY)
        {
            return _tilledTiles.Remove(new Vector2Int(tileX, tileY));
        }

        // ── Iteracao ─────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Itera sobre todos os tiles arados. Usado por ProcessDayAllTiles e Capture.
        /// </summary>
        public IEnumerable<KeyValuePair<Vector2Int, FarmPlotLogic>> AllTilledTiles => _tilledTiles;

        /// <summary>
        /// Numero de tiles atualmente arados (para diagnostico/testes).
        /// </summary>
        public int TilledCount => _tilledTiles.Count;

        // ── Helpers ──────────────────────────────────────────────────────────────────────────────

        private bool IsWithinBounds(int tileX, int tileY)
        {
            if (_widthTiles <= 0 || _heightTiles <= 0)
            {
                // Bounds nao configurados: aceita qualquer tile (modo permissivo para testes).
                return true;
            }

            return tileX >= _originTileX && tileX < _originTileX + _widthTiles
                && tileY >= _originTileY && tileY < _originTileY + _heightTiles;
        }

        /// <summary>
        /// Limpa todos os tiles arados (usado em reset de cena).
        /// </summary>
        public void Clear()
        {
            _tilledTiles.Clear();
        }
    }
}
