using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.SceneManagement
{
    /// <summary>Owns one additive content scene and one explicit prefab instance; never changes active Town.</summary>
    public sealed class TownInteriorSceneLoader
    {
        private Scene _ownedScene;
        private GameObject _ownedObject;
        private string _ownerToken;
        private AsyncOperation _pendingLoad;
        private bool _cancelRequested;
        private int _generation;
        private UnityEngine.Events.UnityAction<Scene, LoadSceneMode> _sceneLoadedHandler;

        public TownInteriorInstance Instance => _ownedObject != null ? _ownedObject.GetComponent<TownInteriorInstance>() : null;
        public bool IsLoaded => _ownedScene.IsValid() && _ownedScene.isLoaded && _ownedObject != null;
        public bool IsBusy => _pendingLoad != null || !string.IsNullOrEmpty(_ownerToken);

        public IEnumerator Load(string operationId, string sceneName, string houseId, GameObject prefab,
            Vector2 origin, Bounds townBounds, Action<bool, string> completed)
        {
            if (string.IsNullOrWhiteSpace(operationId) || string.IsNullOrWhiteSpace(sceneName) ||
                string.IsNullOrWhiteSpace(houseId) || prefab == null || !Finite(origin.x) || !Finite(origin.y) ||
                IsLoaded || !string.IsNullOrEmpty(_ownerToken) || SceneManager.GetSceneByName(sceneName).isLoaded)
            { completed?.Invoke(false, "Interior load arguments or ownership are invalid."); yield break; }

            _ownerToken = operationId;
            int generation = ++_generation;
            _cancelRequested = false;
            int seenScenes = 0;
            Scene loadedByThisOperation = default;
            void OnLoaded(Scene scene, LoadSceneMode mode)
            {
                if (!Owns(operationId, generation) || mode != LoadSceneMode.Additive || scene.name != sceneName) return;
                seenScenes++;
                loadedByThisOperation = scene;
                _ownedScene = seenScenes == 1 ? scene : default;
            }
            _sceneLoadedHandler = OnLoaded;
            SceneManager.sceneLoaded += _sceneLoadedHandler;
            try { _pendingLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive); }
            catch (Exception exception)
            {
                SceneManager.sceneLoaded -= _sceneLoadedHandler;
                _sceneLoadedHandler = null;
                _ownerToken = null; completed?.Invoke(false, exception.Message); yield break;
            }
            if (_pendingLoad == null)
            {
                SceneManager.sceneLoaded -= _sceneLoadedHandler;
                _sceneLoadedHandler = null;
                _ownerToken = null; completed?.Invoke(false, "Interior scene was not found in the Player build."); yield break;
            }
            yield return _pendingLoad;
            if (!Owns(operationId, generation)) yield break;
            _pendingLoad = null;
            SceneManager.sceneLoaded -= _sceneLoadedHandler;
            _sceneLoadedHandler = null;

            // The callback supplies the exact scene handle. A homonymous scene already present
            // or an ambiguous concurrent load is never claimed via GetSceneByName.
            if (seenScenes != 1 || !loadedByThisOperation.IsValid())
            { completed?.Invoke(false, "Interior scene ownership is ambiguous."); yield break; }
            _ownedScene = loadedByThisOperation;
            if (!_ownedScene.IsValid() || !_ownedScene.isLoaded)
            { completed?.Invoke(false, "Interior additive scene did not load."); yield break; }
            if (_cancelRequested) yield break;
            // The new scene must be content-only. Do not search active Town or steal its managers.
            foreach (var root in _ownedScene.GetRootGameObjects())
                if (root.GetComponent<TownInteriorInstance>() != null)
                { completed?.Invoke(false, "Interior scene already contains an instance marker."); yield break; }

            _ownedObject = UnityEngine.Object.Instantiate(prefab);
            SceneManager.MoveGameObjectToScene(_ownedObject, _ownedScene);
            _ownedObject.transform.position = new Vector3(origin.x, origin.y, 0f);
            var instance = Instance;
            string reason = string.Empty;
            if (instance == null || !instance.TryValidate(houseId, out reason))
            { completed?.Invoke(false, instance == null ? "Interior prefab has no root instance marker." : reason); yield break; }
            var bounds = instance.WorldBounds;
            if (!Finite(bounds.min.x) || !Finite(bounds.min.y) || !Finite(bounds.max.x) || !Finite(bounds.max.y) ||
                bounds.Intersects(townBounds))
            { completed?.Invoke(false, "Interior bounds are invalid or overlap the active Town."); yield break; }
            if (!_cancelRequested) completed?.Invoke(true, string.Empty);
        }

        public IEnumerator Unload(string operationId)
        {
            int generation = _generation;
            if (!Owns(operationId, generation)) yield break;
            CancelAndUnload(operationId);
            // Cancellation and normal exit share one cleanup; this waiter never mutates a later visit.
            while (Owns(operationId, generation)) yield return null;
        }

        /// <summary>Lifecycle abort when the coroutine host is disabled; no scene may be orphaned.</summary>
        public void CancelAndUnload(string operationId)
        {
            int generation = _generation;
            if (!Owns(operationId, generation) || _cancelRequested) return;
            _cancelRequested = true;
            void Release(AsyncOperation _)
            {
                if (!Owns(operationId, generation)) return;
                ++_generation; // Invalidates suspended Load continuations before allowing another visit.
                _pendingLoad = null;
                _ownedScene = default;
                _ownedObject = null;
                _ownerToken = null;
            }
            void Finish(AsyncOperation _)
            {
                if (!Owns(operationId, generation)) return;
                if (_sceneLoadedHandler != null)
                { SceneManager.sceneLoaded -= _sceneLoadedHandler; _sceneLoadedHandler = null; }
                if (_ownedScene.IsValid() && _ownedScene.isLoaded)
                {
                    var unload = SceneManager.UnloadSceneAsync(_ownedScene);
                    if (unload != null) { unload.completed += Release; return; }
                }
                Release(null);
            }
            if (_pendingLoad != null && !_pendingLoad.isDone) _pendingLoad.completed += Finish;
            else Finish(null);
        }

        private bool Owns(string operationId, int generation) =>
            !string.IsNullOrEmpty(operationId) && _ownerToken == operationId && _generation == generation;

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
