using System.Reflection;
using CindarsHope.Core.Respawn;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Player
{
    /// <summary>
    /// Achado de eficiencia (MODULARIZATION_PAIR_BREAK_MAP.md): AnyaFountainRespawnFlow lia
    /// FindObjectsByType(MonoBehaviour) sem filtro. Cobre o registro estatico
    /// AnyaFountain.ActiveInstances (auto-registro via OnEnable/OnDisable) que substituiu o scan.
    ///
    /// EditMode nao dispara Awake/OnEnable/OnDisable sincronamente fora do Play Mode; os metodos
    /// sao invocados via reflection para exercitar exatamente a logica de registro/desregistro que
    /// roda em runtime (mesma tecnica usada para validar callbacks Unity fora do Play Mode).
    /// </summary>
    public class AnyaFountainActiveInstancesTests
    {
        private static void InvokeLifecycle(AnyaFountain fountain, string methodName)
        {
            var method = typeof(AnyaFountain).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Metodo {methodName} nao encontrado via reflection.");
            method.Invoke(fountain, null);
        }

        [Test]
        public void OnEnable_RegistersInstance_InActiveInstances()
        {
            var go = new GameObject("fountain");
            try
            {
                var fountain = go.AddComponent<AnyaFountain>();
                InvokeLifecycle(fountain, "OnEnable");

                Assert.IsTrue(AnyaFountain.ActiveInstances.Contains(fountain));
            }
            finally
            {
                InvokeLifecycle(go.GetComponent<AnyaFountain>(), "OnDisable");
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnDisable_RemovesInstance_FromActiveInstances()
        {
            var go = new GameObject("fountain");
            try
            {
                var fountain = go.AddComponent<AnyaFountain>();
                InvokeLifecycle(fountain, "OnEnable");
                Assert.IsTrue(AnyaFountain.ActiveInstances.Contains(fountain));

                InvokeLifecycle(fountain, "OnDisable");

                Assert.IsFalse(AnyaFountain.ActiveInstances.Contains(fountain));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnEnable_DoesNotDuplicate_WhenCalledTwice()
        {
            var go = new GameObject("fountain");
            try
            {
                var fountain = go.AddComponent<AnyaFountain>();
                InvokeLifecycle(fountain, "OnEnable");
                InvokeLifecycle(fountain, "OnEnable");

                var count = 0;
                foreach (var instance in AnyaFountain.ActiveInstances)
                {
                    if (ReferenceEquals(instance, fountain))
                    {
                        count++;
                    }
                }

                Assert.AreEqual(1, count, "Fonte nao deve se registrar duas vezes em ActiveInstances.");
            }
            finally
            {
                InvokeLifecycle(go.GetComponent<AnyaFountain>(), "OnDisable");
                Object.DestroyImmediate(go);
            }
        }
    }
}
