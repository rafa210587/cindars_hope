using CindarsHope.Cave.Runtime;
using CindarsHope.Core.Bootstrap;
using UnityEngine;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save da run ativa da caverna. Fonte do estado: <see cref="CaveRunManager"/>
    /// acessado via <see cref="GameBootstrap.Instance"/> (padrão estabelecido em F13/CaptureCaveRunSaveData).
    /// Antes de capturar, força o refresh dos snapshots do nível corrente (HP dos inimigos, baús
    /// abertos, estado das armadilhas). Fallback: sem run ativa quando o bootstrap não está disponível.
    /// </summary>
    public class CaveRunSectionProvider : ISaveSectionProvider
    {
        public string ProviderId => "cave_run";

        public object Capture(GameSaveData existingSaveData)
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                return new CaveRunSaveData { HasActiveRun = false };
            }

            var runManager = bootstrap.CaveRunManager;
            if (runManager != null)
            {
                // Regrava HP dos inimigos do nível corrente antes de capturar.
                var levelController = runManager.GetComponent<Cave.CaveLevelRuntimeController>();
                if (levelController != null)
                {
                    levelController.RefreshCurrentSnapshotEnemyHp();
                    levelController.RefreshCurrentSnapshotOpenedChests(); // fable_09
                    levelController.RefreshCurrentSnapshotTrapStates();   // fable_60
                }

                return CaveRunSaveMapper.ToSaveData(runManager.State);
            }

            return CaveRunSaveMapper.ToSaveData(bootstrap.CachedCaveRunState);
        }

        public void Restore(object sectionData)
        {
            var data = sectionData as CaveRunSaveData;
            if (data == null || !data.HasActiveRun)
            {
                return;
            }

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                Debug.LogWarning("CaveRunSectionProvider: GameBootstrap não disponível; cave run não restaurada.");
                return;
            }

            var restoredRun = CaveRunSaveMapper.FromSaveData(data);
            if (restoredRun != null)
            {
                bootstrap.SetCachedCaveRunState(restoredRun);
                Debug.Log($"CaveRunSectionProvider: cave run restaurada (nível {restoredRun.CurrentCaveLevel}, seed {restoredRun.CaveRunSeed}).");
            }
        }
    }
}
