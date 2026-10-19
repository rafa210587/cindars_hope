using System.Reflection;
using CindarsHope.Cave.Resources;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// Achado de eficiencia (MODULARIZATION_PAIR_BREAK_MAP.md): CaveLevelRuntimeController fazia
    /// FindObjectsByType(ResourceNode) a cada DayStartedEvent. Cobre o registro estatico
    /// ResourceNode.ActiveInstances (auto-registro via OnEnable/OnDisable) que substituiu o scan.
    ///
    /// EditMode nao dispara Awake/OnEnable/OnDisable sincronamente fora do Play Mode; os metodos
    /// sao invocados via reflection para exercitar exatamente a logica de registro/desregistro que
    /// roda em runtime (mesma tecnica usada para validar callbacks Unity fora do Play Mode).
    /// </summary>
    public class ResourceNodeActiveInstancesTests
    {
        private static void InvokeLifecycle(ResourceNode node, string methodName)
        {
            var method = typeof(ResourceNode).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Metodo {methodName} nao encontrado via reflection.");
            method.Invoke(node, null);
        }

        [Test]
        public void OnEnable_RegistersInstance_InActiveInstances()
        {
            var go = new GameObject("node");
            try
            {
                var node = go.AddComponent<ResourceNode>();
                InvokeLifecycle(node, "OnEnable");

                Assert.IsTrue(ResourceNode.ActiveInstances.Contains(node));
            }
            finally
            {
                InvokeLifecycle(go.GetComponent<ResourceNode>(), "OnDisable");
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnDisable_RemovesInstance_FromActiveInstances()
        {
            var go = new GameObject("node");
            try
            {
                var node = go.AddComponent<ResourceNode>();
                InvokeLifecycle(node, "OnEnable");
                Assert.IsTrue(ResourceNode.ActiveInstances.Contains(node));

                InvokeLifecycle(node, "OnDisable");

                Assert.IsFalse(ResourceNode.ActiveInstances.Contains(node));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnEnable_DoesNotDuplicate_WhenCalledTwice()
        {
            var go = new GameObject("node");
            try
            {
                var node = go.AddComponent<ResourceNode>();
                InvokeLifecycle(node, "OnEnable");
                InvokeLifecycle(node, "OnEnable");

                var count = 0;
                foreach (var instance in ResourceNode.ActiveInstances)
                {
                    if (ReferenceEquals(instance, node))
                    {
                        count++;
                    }
                }

                Assert.AreEqual(1, count, "Node nao deve se registrar duas vezes em ActiveInstances.");
            }
            finally
            {
                InvokeLifecycle(go.GetComponent<ResourceNode>(), "OnDisable");
                Object.DestroyImmediate(go);
            }
        }
    }
}
