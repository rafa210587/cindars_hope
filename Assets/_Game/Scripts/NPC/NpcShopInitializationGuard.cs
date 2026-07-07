using System;
using UnityObject = UnityEngine.Object;

namespace CindarsHope.NPC
{
    internal static class NpcShopInitializationGuard
    {
        public static bool ValidateRequiredReferences(
            UnityObject npcData,
            UnityObject shopManager,
            UnityObject playerManager,
            UnityObject inventoryManager,
            UnityObject itemDatabase,
            UnityObject modalManager,
            UnityObject dialogueModal,
            UnityObject shopMenuModal,
            UnityObject buyPanel,
            UnityObject sellPanel,
            Action<string, string> logMissingReference)
        {
            if (!ValidateReference(npcData, "_npcData", logMissingReference)) return false;
            if (!ValidateReference(shopManager, "_shopManager", logMissingReference)) return false;
            if (!ValidateReference(playerManager, "_playerManager", logMissingReference)) return false;
            if (!ValidateReference(inventoryManager, "_inventoryManager", logMissingReference)) return false;
            if (!ValidateReference(itemDatabase, "_itemDatabase", logMissingReference)) return false;
            if (!ValidateReference(modalManager, "_modalManager", logMissingReference)) return false;
            if (!ValidateReference(dialogueModal, "_dialogueModal", logMissingReference)) return false;
            if (!ValidateReference(shopMenuModal, "_shopMenuModal", logMissingReference)) return false;
            if (!ValidateReference(buyPanel, "_buyPanel", logMissingReference)) return false;
            if (!ValidateReference(sellPanel, "_sellPanel", logMissingReference)) return false;

            return true;
        }

        private static bool ValidateReference(
            UnityObject reference,
            string fieldName,
            Action<string, string> logMissingReference)
        {
            if (reference != null)
            {
                return true;
            }

            logMissingReference?.Invoke(fieldName, "required reference is null.");
            return false;
        }
    }
}
