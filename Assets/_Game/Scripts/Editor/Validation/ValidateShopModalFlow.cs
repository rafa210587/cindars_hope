#if UNITY_EDITOR
using System;
using System.Reflection;
using CindarsHope.Foundation;
using CindarsHope.NPC;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Shop;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateShopModalFlow
    {
        public static void Validate()
        {
            var root = new GameObject("[SPEC17F] Modal Validation");
            root.SetActive(false);
            try
            {
                var modalManager = root.AddComponent<ModalManager>();
                modalManager.Initialize();
                var buyPanel = CreatePanel<BuyPanel>(root.transform, modalManager);
                var sellPanel = CreatePanel<SellPanel>(root.transform, modalManager);
                var shopMenu = root.AddComponent<ShopMenuModal>();

                Require(modalManager.PushModal(ModalType.Sell), "Cannot seed Sell modal.");
                buyPanel.Hide();
                Require(modalManager.CurrentModal == ModalType.Sell, "BuyPanel.Hide removed or altered active Sell modal.");
                sellPanel.Hide();
                Require(modalManager.CurrentModal == ModalType.None, "SellPanel.Hide did not remove active Sell modal.");

                Require(modalManager.PushModal(ModalType.Buy), "Cannot seed Buy modal.");
                sellPanel.Hide();
                Require(modalManager.CurrentModal == ModalType.Buy, "SellPanel.Hide removed or altered active Buy modal.");
                buyPanel.Hide();
                Require(modalManager.CurrentModal == ModalType.None, "BuyPanel.Hide did not remove active Buy modal.");

                ValidateControllerClose(root.transform, modalManager, buyPanel, sellPanel, shopMenu, ModalType.Sell);
                ValidateControllerClose(root.transform, modalManager, buyPanel, sellPanel, shopMenu, ModalType.Buy);
                Debug.Log("[SPEC17F] Shop modal flow validation passed.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static T CreatePanel<T>(Transform parent, ModalManager modalManager) where T : Component
        {
            var panelObject = new GameObject(typeof(T).Name, typeof(CanvasGroup));
            panelObject.transform.SetParent(parent, false);
            var panel = panelObject.AddComponent<T>();
            typeof(T).GetMethod("Initialize")?.Invoke(panel, new object[] { null, null, null, null, modalManager });
            return panel;
        }

        private static void ValidateControllerClose(Transform parent, ModalManager modalManager, BuyPanel buyPanel, SellPanel sellPanel, ShopMenuModal shopMenu, ModalType activeModal)
        {
            modalManager.ClearAllModals();
            Require(modalManager.PushModal(activeModal), $"Cannot seed {activeModal} for NpcShopController.");
            var npcObject = new GameObject($"NPC_{activeModal}");
            npcObject.SetActive(false);
            npcObject.transform.SetParent(parent, false);
            var controller = npcObject.AddComponent<NpcShopController>();
            var npcData = ScriptableObject.CreateInstance<NpcDataSO>();
            npcData.NpcId = $"validator_{activeModal}";
            SetField(controller, "_modalManager", modalManager);
            SetField(controller, "_buyPanel", buyPanel);
            SetField(controller, "_sellPanel", sellPanel);
            SetField(controller, "_shopMenuModal", shopMenu);
            SetField(controller, "_npcData", npcData);
            SetField(controller, "_isInteracting", true);
            SetField(controller, "_isClosing", false);

            typeof(NpcShopController).GetMethod("BeginCloseInteraction", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(controller, null);
            Require(modalManager.CurrentModal == ModalType.None, $"NpcShopController did not close active {activeModal} cleanly.");
            UnityEngine.Object.DestroyImmediate(npcData);
        }

        private static void SetField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                throw new InvalidOperationException($"[SPEC17F] Missing field '{name}' on '{target.GetType().Name}'.");
            }

            field.SetValue(target, value);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException($"[SPEC17F] {message}");
            }
        }
    }
}
#endif
