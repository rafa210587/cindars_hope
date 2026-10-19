using UnityEngine;

namespace CindarsHope.Cave.Generation
{
    public readonly struct CaveRoom
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;

        public CaveRoom(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Vector2Int Center => new Vector2Int(X + Width / 2, Y + Height / 2);
        public int MinX => X;
        public int MaxX => X + Width - 1;
        public int MinY => Y;
        public int MaxY => Y + Height - 1;

        public bool Overlaps(CaveRoom other, int padding)
        {
            return MinX - padding <= other.MaxX
                && MaxX + padding >= other.MinX
                && MinY - padding <= other.MaxY
                && MaxY + padding >= other.MinY;
        }

        public bool Contains(Vector2Int position)
        {
            return position.x >= MinX && position.x <= MaxX && position.y >= MinY && position.y <= MaxY;
        }
    }
}
