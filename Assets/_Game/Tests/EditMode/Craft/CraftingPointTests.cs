using CindarsHope.Core;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.Craft.Events;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Craft
{
    /// <summary>
    /// spec_city_artisan_stations — CraftingPoint em modo DESACOPLADO (sem refs de runtime/modal, usado
    /// pela cidade): apertar E publica OpenCraftingStationRequestedEvent com o id e o WorkshopType.
    /// (No modo com refs — usado pela fazenda — ele abre o modal direto; não coberto aqui por exigir cena.)
    /// </summary>
    public class CraftingPointTests
    {
        [Test]
        public void Interact_Decoupled_PublishesOpenEvent_WithIdAndType()
        {
            var go = new GameObject("station");
            var point = go.AddComponent<CraftingPoint>();
            point.Configure("station_house_blacksmith", WorkshopType.Forge);

            OpenCraftingStationRequestedEvent? captured = null;
            void Handler(OpenCraftingStationRequestedEvent e) => captured = e;
            GameEventBus.Subscribe<OpenCraftingStationRequestedEvent>(Handler);

            var interactor = new GameObject("player");
            try
            {
                point.Interact(interactor);
            }
            finally
            {
                GameEventBus.Unsubscribe<OpenCraftingStationRequestedEvent>(Handler);
                Object.DestroyImmediate(interactor);
                Object.DestroyImmediate(go);
            }

            Assert.IsTrue(captured.HasValue, "Estação desacoplada deve publicar o evento de abrir craft.");
            Assert.AreEqual("station_house_blacksmith", captured.Value.StationInstanceId);
            Assert.AreEqual(WorkshopType.Forge, captured.Value.WorkshopType);
        }

        [Test]
        public void CanInteract_IsFalse_WhenStationIdNotConfigured()
        {
            var go = new GameObject("station");
            var point = go.AddComponent<CraftingPoint>();
            var interactor = new GameObject("player");
            try
            {
                Assert.IsFalse(point.CanInteract(interactor), "Sem id configurado, a estação não deve ser interagível.");
            }
            finally
            {
                Object.DestroyImmediate(interactor);
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Interact_Decoupled_PublishesConfiguredType()
        {
            AssertPublishes(WorkshopType.Alchemy);
            AssertPublishes(WorkshopType.Sewing);
            AssertPublishes(WorkshopType.CookingStation);
        }

        private static void AssertPublishes(WorkshopType type)
        {
            var go = new GameObject("station");
            var point = go.AddComponent<CraftingPoint>();
            point.Configure($"station_{type}", type);

            WorkshopType? got = null;
            void Handler(OpenCraftingStationRequestedEvent e) => got = e.WorkshopType;
            GameEventBus.Subscribe<OpenCraftingStationRequestedEvent>(Handler);
            var interactor = new GameObject("player");
            try
            {
                point.Interact(interactor);
            }
            finally
            {
                GameEventBus.Unsubscribe<OpenCraftingStationRequestedEvent>(Handler);
                Object.DestroyImmediate(interactor);
                Object.DestroyImmediate(go);
            }

            Assert.AreEqual(type, got);
        }
    }
}
