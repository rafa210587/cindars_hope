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

            if (activeScene.name == FarmSceneName)
            {
                if (_itemPickupRegistry != null)
                {
                    worldSaveData.Pickups = _itemPickupRegistry.CaptureSaveData();
                }
                else if (existingSaveData?.World?.Pickups != null)
                {
                    worldSaveData.Pickups = existingSaveData.World.Pickups;
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
                    worldSaveData.Pickups = existingSaveData.World.Pickups;
                    worldSaveData.Trees = existingSaveData.World.Trees;
                }
            }

            return worldSaveData;
        }

        public void Restore(object sectionData)
        {
            var data = sectionData as WorldSaveData;

            if (_itemPickupRegistry != null && data != null)
            {
                _itemPickupRegistry.RestoreFromSaveData(data.Pickups);
            }

            if (_treeRegistry != null && data != null)
            {
                var treeFarmSaveData = new FarmSaveData
                {
                    Trees = data.Trees ?? new List<TreeSaveData>()
                };
                _treeRegistry.RestoreFromSaveData(treeFarmSaveData);
            }
        }
    }
}
