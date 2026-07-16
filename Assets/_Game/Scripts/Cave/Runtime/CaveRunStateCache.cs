using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// Cache cross-cena do <see cref="CaveRuntimeState"/> ativo. Antes vivia em
    /// CindarsHope.Core.Bootstrap.GameBootstrap (campo + Set/Take/Cached); movido para o lado
    /// Cave (arch: corte do par mutuo Cave|Core, 2026-07-15) como static holder puro — statics
    /// sobrevivem a scene load, mesma semantica do cache anterior no bootstrap.
    /// </summary>
    public static class CaveRunStateCache
    {
        private static CaveRuntimeState _cached;

        public static CaveRuntimeState Cached => _cached;

        public static void Set(CaveRuntimeState state)
        {
            _cached = state;
            if (state != null)
            {
                Debug.Log($"CaveRunStateCache: cached CaveRunState. RunSeed={state.CaveRunSeed}, Level={state.CurrentCaveLevel}");
            }
        }

        public static CaveRuntimeState Take()
        {
            var state = _cached;
            _cached = null;
            if (state != null)
            {
                Debug.Log($"CaveRunStateCache: restored CaveRunState from cache. RunSeed={state.CaveRunSeed}, Level={state.CurrentCaveLevel}");
            }
            return state;
        }
    }
}
