using System.Collections.Generic;
using CindarsHope.Farm;
using CindarsHope.World;
using UnityEngine.SceneManagement;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save do mundo (pickups de itens e árvores no mapa). Captura apenas quando na
    /// FarmScene; fora dela preserva os dados existentes para evitar perda ao salvar de outra cena.
    /// As refs de <see cref="ItemPickupRegistry"/> e <see cref="TreeRegistry"/> são injetadas via
    /// constructor (bindings serializados do SaveManager, rebindados por cena).
    /// </summary>
    public class WorldSectionProvider : ISaveSectionProvider
    {
        private const string FarmSceneName = "FarmScene";

        private readonly ItemPickupRegistry _itemPickupRegistry;
        private readonly TreeRegistry _treeRegistry;

        public WorldSectionProvider(ItemPickupRegistry itemPickupRegistry, TreeRegistry treeRegistry)
        {
            _itemPickupRegistry = itemPickupRegistry;
            _treeRegistry = treeRegistry;
        }

        public string ProviderId => "world";

        public object Capture(GameSaveData existingSaveData)
        {
            var worldSaveData = new WorldSaveData();
            var activeScene = SceneManager.GetActiveScene();
            var previousPickups = existingSaveData?.World?.Pickups;
            var pickups = new List<ItemPickupSaveData>();

            if (activeScene.name == FarmSceneName)
            {
                if (_itemPickupRegistry != null)
                {
                    pickups.AddRange(_itemPickupRegistry.CaptureSaveData());
                }
                else if (previousPickups != null)
                {
                    foreach (var pickup in previousPickups)
                        if (pickup != null && !pickup.IsDynamic) pickups.Add(pickup);
                }

                if (_treeRegistry != null)
                {
                    worldSaveData.Trees = _treeRegistry.CaptureSaveData();
                }
                else if (existingSaveData?.World?.Trees != null)
                {
                    worldSaveData.Trees = existingSaveData.World.Trees;
                }
            }
            else
            {
                if (existingSaveData?.World != null)
                {
                    worldSaveData.Trees = existingSaveData.World.Trees;
                    if (previousPickups != null)
                        foreach (var pickup in previousPickups)
                            if (pickup != null && !pickup.IsDynamic) pickups.Add(pickup);
                }
            }

            // The persistent drop spawner retains all scene-owned dynamic pickups in memory.
            // Its snapshot replaces the previous dynamic list so re-saving cannot duplicate IDs.
            var spawner = ItemDropSpawner.Instance;
            if (spawner != null)
                pickups.AddRange(spawner.CaptureDynamicPickupSaveData());
            else if (previousPickups != null)
                foreach (var pickup in previousPickups)
                    if (pickup != null && pickup.IsDynamic)
                        pickups.Add(pickup);
            worldSaveData.Pickups = pickups;

            return worldSaveData;
        }

        public void Restore(object sectionData)
        {
            var data = sectionData as WorldSaveData;

            if (_itemPickupRegistry != null && data != null)
            {
                var staticPickups = new List<ItemPickupSaveData>();
                if (data.Pickups != null)
                    foreach (var pickup in data.Pickups)
                        if (pickup != null && !pickup.IsDynamic) staticPickups.Add(pickup);
                _itemPickupRegistry.RestoreFromSaveData(staticPickups);
            }

            if (data != null && ItemDropSpawner.Instance != null)
            {
                var dynamicPickups = new List<ItemPickupSaveData>();
                if (data.Pickups != null)
                    foreach (var pickup in data.Pickups)
                        if (pickup != null && pickup.IsDynamic)
                            dynamicPickups.Add(pickup);
                ItemDropSpawner.Instance.RestoreDynamicPickups(dynamicPickups);
            }

            if (_treeRegistry != null && data != null)
            {
                _treeRegistry.RestoreFromSaveData(data.Trees ?? new List<TreeSaveData>());
            }
        }
    }
}
