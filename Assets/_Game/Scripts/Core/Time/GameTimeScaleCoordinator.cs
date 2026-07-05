using System;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Core.Time
{
    /// <summary>
    /// Coordena pausas concorrentes por token. A escala anterior só é restaurada quando o último
    /// proprietário libera seu token, impedindo que uma tela ou efeito desfaça a pausa de outro.
    /// </summary>
    public static class GameTimeScaleCoordinator
    {
        private static readonly HashSet<int> ActiveTokens = new HashSet<int>();
        private static int _nextTokenId;
        private static float _restoreTimeScale = 1f;

        public static bool IsPaused => ActiveTokens.Count > 0;
        public static int ActiveTokenCount => ActiveTokens.Count;

        public static IDisposable AcquirePause()
        {
            if (ActiveTokens.Count == 0)
            {
                _restoreTimeScale = UnityEngine.Time.timeScale <= 0f ? 1f : UnityEngine.Time.timeScale;
                UnityEngine.Time.timeScale = 0f;
            }

            int tokenId = ++_nextTokenId;
            ActiveTokens.Add(tokenId);
            return new PauseToken(tokenId);
        }

        public static void Reset()
        {
            ActiveTokens.Clear();
            _nextTokenId = 0;
            UnityEngine.Time.timeScale = _restoreTimeScale <= 0f ? 1f : _restoreTimeScale;
            _restoreTimeScale = 1f;
        }

        private static void Release(int tokenId)
        {
            if (!ActiveTokens.Remove(tokenId) || ActiveTokens.Count > 0)
            {
                return;
            }

            UnityEngine.Time.timeScale = _restoreTimeScale <= 0f ? 1f : _restoreTimeScale;
            _restoreTimeScale = 1f;
        }

        private sealed class PauseToken : IDisposable
        {
            private readonly int _tokenId;
            private bool _disposed;

            public PauseToken(int tokenId)
            {
                _tokenId = tokenId;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                Release(_tokenId);
            }
        }
    }
}
