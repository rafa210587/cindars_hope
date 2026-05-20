using CindarsHope.Cave.Generation;
using UnityEngine;

namespace CindarsHope.Cave
{
    public sealed class CaveDebugVisualizer : MonoBehaviour
    {
        [SerializeField] private bool _drawWalkableTiles = true;
        [SerializeField] private bool _drawWalls = true;
        [SerializeField] private bool _drawRooms = true;
        [SerializeField] private bool _drawSpawnPoints = true;
        [SerializeField] private bool _drawEntranceExit = true;
        [SerializeField] private Color _walkableTileColor = Color.green * 0.5f;
        [SerializeField] private Color _wallColor = Color.gray;
        [SerializeField] private Color _roomColor = Color.blue * 0.3f;
        [SerializeField] private Color _enemySpawnColor = Color.red;
        [SerializeField] private Color _resourceSpawnColor = Color.yellow;
        [SerializeField] private Color _entranceColor = Color.cyan;
        [SerializeField] private Color _exitColor = Color.magenta;

        private CaveLevelRuntimeController _caveController;

        private void Start()
        {
            _caveController = GetComponent<CaveLevelRuntimeController>();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || _caveController == null || _caveController.CurrentGeneratedLevel == null)
            {
                return;
            }

            var generated = _caveController.CurrentGeneratedLevel;

            if (_drawWalkableTiles)
            {
                DrawWalkableTiles(generated);
            }

            if (_drawWalls)
            {
                DrawWalls(generated);
            }

            if (_drawRooms)
            {
                DrawRooms(generated);
            }

            if (_drawSpawnPoints)
            {
                DrawSpawnPoints(generated);
            }

            if (_drawEntranceExit)
            {
                DrawEntranceExit(generated);
            }
        }

        private void DrawWalkableTiles(CaveGeneratedLevel generated)
        {
            Gizmos.color = _walkableTileColor;
            foreach (var tile in generated.WalkableTiles)
            {
                Gizmos.DrawCube(new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0), Vector3.one * 0.1f);
            }
        }

        private void DrawWalls(CaveGeneratedLevel generated)
        {
            Gizmos.color = _wallColor;
            foreach (var tile in generated.WallTiles)
            {
                Gizmos.DrawCube(new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0), Vector3.one * 0.08f);
            }
        }

        private void DrawRooms(CaveGeneratedLevel generated)
        {
            Gizmos.color = _roomColor;
            foreach (var room in generated.Rooms)
            {
                var center = new Vector3(room.Center.x, room.Center.y, 0);
                var size = new Vector3(room.Width, room.Height, 0);
                Gizmos.DrawWireCube(center, size);
            }
        }

        private void DrawSpawnPoints(CaveGeneratedLevel generated)
        {
            Gizmos.color = _enemySpawnColor;
            foreach (var point in generated.EnemySpawnPoints)
            {
                Gizmos.DrawSphere(new Vector3(point.Position.x, point.Position.y, 0), 0.2f);
            }

            Gizmos.color = _resourceSpawnColor;
            foreach (var point in generated.ResourceSpawnPoints)
            {
                Gizmos.DrawCube(new Vector3(point.Position.x, point.Position.y, 0), Vector3.one * 0.3f);
            }
        }

        private void DrawEntranceExit(CaveGeneratedLevel generated)
        {
            Gizmos.color = _entranceColor;
            Gizmos.DrawSphere(new Vector3(generated.Entrance.x + 0.5f, generated.Entrance.y + 0.5f, 0), 0.3f);

            Gizmos.color = _exitColor;
            Gizmos.DrawSphere(new Vector3(generated.Exit.x + 0.5f, generated.Exit.y + 0.5f, 0), 0.3f);
        }
    }
}
