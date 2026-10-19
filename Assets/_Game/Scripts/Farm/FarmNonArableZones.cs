using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Farm
{
    /// <summary>
    /// Registo de footprints nao-araveis (construcoes, agua, montanha) e flag de estufa.
    /// Alimentado pela Spec A (gerador de cena) via API publica. Pure C# — sem UnityEngine exceto Vector2Int.
    /// </summary>
    public class FarmNonArableZones
    {
        // Conjunto de tiles bloqueados por construcao/agua/montanha (coordenadas absolutas de tile).
        private readonly HashSet<Vector2Int> _blockedTiles = new HashSet<Vector2Int>();

        // Conjunto de tiles que estao dentro de estufa (araveis mesmo dentro de uma estrutura).
        private readonly HashSet<Vector2Int> _greenhouseTiles = new HashSet<Vector2Int>();

        /// <summary>
        /// Marca um tile como nao-aravel (construcao, agua, montanha).
        /// </summary>
        public void RegisterBlockedTile(int tileX, int tileY)
        {
            _blockedTiles.Add(new Vector2Int(tileX, tileY));
        }

        /// <summary>
        /// Remove o bloqueio de um tile (ex.: construcao demolida).
        /// </summary>
        public void UnregisterBlockedTile(int tileX, int tileY)
        {
            _blockedTiles.Remove(new Vector2Int(tileX, tileY));
        }

        /// <summary>
        /// Remove o bloqueio de um footprint retangular de tiles (ex.: ponte sobre rio).
        /// </summary>
        public void UnregisterBlockedRect(int originX, int originY, int width, int height)
        {
            for (var y = originY; y < originY + height; y++)
            {
                for (var x = originX; x < originX + width; x++)
                {
                    _blockedTiles.Remove(new Vector2Int(x, y));
                }
            }
        }

        /// <summary>
        /// Marca um footprint retangular de tiles como nao-araveis.
        /// </summary>
        public void RegisterBlockedRect(int originX, int originY, int width, int height)
        {
            for (var y = originY; y < originY + height; y++)
            {
                for (var x = originX; x < originX + width; x++)
                {
                    _blockedTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        /// <summary>
        /// Registra um tile como interior de estufa (aravel mesmo sendo "interior").
        /// </summary>
        public void RegisterGreenhouseTile(int tileX, int tileY)
        {
            _greenhouseTiles.Add(new Vector2Int(tileX, tileY));
        }

        /// <summary>
        /// Registra um footprint retangular de estufa como aravel.
        /// </summary>
        public void RegisterGreenhouseRect(int originX, int originY, int width, int height)
        {
            for (var y = originY; y < originY + height; y++)
            {
                for (var x = originX; x < originX + width; x++)
                {
                    _greenhouseTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        /// <summary>
        /// Remove a marcacao de estufa de um tile.
        /// </summary>
        public void UnregisterGreenhouseTile(int tileX, int tileY)
        {
            _greenhouseTiles.Remove(new Vector2Int(tileX, tileY));
        }

        /// <summary>
        /// Retorna true se o tile e um interior de estufa (aravel especial).
        /// </summary>
        public bool IsGreenhouseTile(int tileX, int tileY)
        {
            return _greenhouseTiles.Contains(new Vector2Int(tileX, tileY));
        }

        /// <summary>
        /// Retorna true se o tile esta bloqueado por construcao/agua/montanha.
        /// Tiles de estufa NAO sao bloqueados mesmo que tambem sejam registrados como blocked.
        /// </summary>
        public bool IsBlocked(int tileX, int tileY)
        {
            var key = new Vector2Int(tileX, tileY);
            // Estufa tem prioridade: mesmo que registrado como blocked, e aravel.
            if (_greenhouseTiles.Contains(key))
            {
                return false;
            }

            return _blockedTiles.Contains(key);
        }

        /// <summary>
        /// Limpa todos os registros (usado em testes ou ao recarregar a cena).
        /// </summary>
        public void Clear()
        {
            _blockedTiles.Clear();
            _greenhouseTiles.Clear();
        }

        /// <summary>
        /// Numero de tiles bloqueados registrados (para diagnostico/testes).
        /// </summary>
        public int BlockedCount => _blockedTiles.Count;

        /// <summary>
        /// Numero de tiles de estufa registrados (para diagnostico/testes).
        /// </summary>
        public int GreenhouseCount => _greenhouseTiles.Count;
    }
}
