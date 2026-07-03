using CindarsHope.Core.Physics;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Physics
{
    /// <summary>
    /// spec_codex_13: smoke test de resolucao de physics layer por NOME com fallback
    /// seguro. Nao assume que os 7 layers de gameplay ja foram materializados no
    /// TagManager deste ambiente de teste (isso depende de rodar
    /// CindarsHope/Inicializar Projeto no Unity Editor, uma acao humana) — cobre a
    /// logica pura e deterministica de resolucao/fallback, que e o que o
    /// Testing Quality Gate exige aqui.
    /// </summary>
    public class GameplayPhysicsLayersTests
    {
        [Test]
        public void GetMaskSafe_UnknownLayerName_ReturnsZeroMask()
        {
            var mask = GameplayLayerNames.GetMaskSafe("Layer_Definitely_Not_Registered_Codex13");
            Assert.AreEqual(0, mask.value, "Layer inexistente deve resolver para mask 0 (nenhum filtro), nunca lancar excecao.");
        }

        [Test]
        public void GetLayerIndexSafe_UnknownLayerName_ReturnsNegativeOne()
        {
            int layer = GameplayLayerNames.GetLayerIndexSafe("Layer_Definitely_Not_Registered_Codex13");
            Assert.AreEqual(-1, layer, "Layer inexistente deve resolver para -1 (LayerMask.NameToLayer contract), nunca lancar excecao.");
        }

        [Test]
        public void TryAssignRuntimeLayer_UnknownLayerName_LeavesGameObjectOnDefaultLayer()
        {
            var go = new GameObject("Codex13_TestObject");
            try
            {
                int layerBefore = go.layer;
                GameplayLayerNames.TryAssignRuntimeLayer(go, "Layer_Definitely_Not_Registered_Codex13");
                Assert.AreEqual(layerBefore, go.layer, "Sem o layer materializado, o GameObject nao deve ser alterado (fallback seguro).");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void TryAssignRuntimeLayer_NullGameObject_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => GameplayLayerNames.TryAssignRuntimeLayer(null, GameplayLayerNames.Enemy));
        }

        [Test]
        public void AllLayerNames_AreNonEmptyAndStable()
        {
            // fable_00 / id-stability: os 7 nomes canonicos nao podem ser strings vazias/duplicadas
            // (contrato usado por editor + runtime como fonte unica de verdade).
            var names = new[]
            {
                GameplayLayerNames.Player,
                GameplayLayerNames.Enemy,
                GameplayLayerNames.Npc,
                GameplayLayerNames.WorldSolid,
                GameplayLayerNames.Interactable,
                GameplayLayerNames.Projectile,
                GameplayLayerNames.Hazard,
            };

            Assert.AreEqual(7, names.Length, "spec_codex_13: exatamente 7 layers minimos (YAGNI — nao os 9 do wishlist original).");
            foreach (var name in names)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(name), "Nome de layer nao pode ser vazio.");
            }

            var distinct = new System.Collections.Generic.HashSet<string>(names);
            Assert.AreEqual(names.Length, distinct.Count, "Nomes de layer nao podem se repetir.");
        }
    }
}
