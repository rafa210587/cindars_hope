using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CindarsHope.World.Rendering
{
    /// <summary>Authored sprite frames played by Unity's Tilemap renderer without per-cell behaviours.</summary>
    public sealed class SpriteAnimationTileSO : TileBase
    {
        [SerializeField] private Sprite[] _frames = Array.Empty<Sprite>();
        [SerializeField, Min(0.01f)] private float _framesPerSecond = 5f;

        public void Configure(Sprite[] frames, float framesPerSecond)
        {
            if (frames == null || frames.Length < 2)
                throw new ArgumentException("A tile loop requires at least two authored frames.", nameof(frames));
            if (framesPerSecond <= 0f || float.IsNaN(framesPerSecond) || float.IsInfinity(framesPerSecond))
                throw new ArgumentOutOfRangeException(nameof(framesPerSecond));
            for (var i = 0; i < frames.Length; i++)
                if (frames[i] == null) throw new ArgumentException("Tile animation frame is missing.", nameof(frames));
            _frames = (Sprite[])frames.Clone();
            _framesPerSecond = framesPerSecond;
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = _frames != null && _frames.Length > 0 ? _frames[0] : null;
            tileData.color = Color.white;
            tileData.transform = Matrix4x4.identity;
            tileData.flags = TileFlags.None;
            tileData.colliderType = Tile.ColliderType.None;
        }

        public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap,
            ref TileAnimationData tileAnimationData)
        {
            if (_frames == null || _frames.Length < 2) return false;
            tileAnimationData.animatedSprites = _frames;
            // Authoring sets the Tilemap animationFrameRate to1; this is the asset's actual FPS.
            tileAnimationData.animationSpeed = _framesPerSecond;
            tileAnimationData.animationStartTime = 0f;
            return true;
        }
    }
}
