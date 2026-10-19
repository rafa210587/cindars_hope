namespace CindarsHope.NPC
{
    public enum NpcShopTransactionReadinessAction
    {
        Ready,
        InitializeController,
        RecoverSession,
        Fail
    }

    public readonly struct NpcShopTransactionReadinessSnapshot
    {
        public readonly bool HasShopData;
        public readonly bool HasShopId;
        public readonly bool HasShopManager;
        public readonly bool HasItemDatabase;
        public readonly bool IsControllerReady;
        public readonly bool IsShopManagerInitialized;
        public readonly bool HasShopSession;
        public readonly bool IsPanelReady;

        public NpcShopTransactionReadinessSnapshot(
            bool hasShopData,
            bool hasShopId,
            bool hasShopManager,
            bool hasItemDatabase,
            bool isControllerReady,
            bool isShopManagerInitialized,
            bool hasShopSession,
            bool isPanelReady)
        {
            HasShopData = hasShopData;
            HasShopId = hasShopId;
            HasShopManager = hasShopManager;
            HasItemDatabase = hasItemDatabase;
            IsControllerReady = isControllerReady;
            IsShopManagerInitialized = isShopManagerInitialized;
            HasShopSession = hasShopSession;
            IsPanelReady = isPanelReady;
        }
    }

    public readonly struct NpcShopTransactionReadinessDecision
    {
        public readonly NpcShopTransactionReadinessAction Action;
        public readonly string FieldName;
        public readonly string Cause;

        public NpcShopTransactionReadinessDecision(
            NpcShopTransactionReadinessAction action,
            string fieldName = null,
            string cause = null)
        {
            Action = action;
            FieldName = fieldName;
            Cause = cause;
        }
    }

    /// <summary>Pure state machine for deciding the next transaction-readiness step.</summary>
    public static class NpcShopTransactionReadinessPolicy
    {
        public static NpcShopTransactionReadinessDecision Evaluate(
            NpcShopTransactionReadinessSnapshot snapshot,
            string panelFieldName)
        {
            if (!snapshot.HasShopData)
                return Fail("_shopData", "ShopDataSO is null.");
            if (!snapshot.HasShopId)
                return Fail("_shopData.Id", "ShopDataSO.Id is empty.");
            if (!snapshot.HasShopManager)
                return Fail("_shopManager", "ShopManager reference is null.");
            if (!snapshot.HasItemDatabase)
                return Fail("_itemDatabase", "ItemDatabaseSO reference is null.");
            if (!snapshot.IsControllerReady)
                return new NpcShopTransactionReadinessDecision(
                    NpcShopTransactionReadinessAction.InitializeController);
            if (!snapshot.IsShopManagerInitialized)
                return Fail("_shopManager.IsInitialized", "ShopManager exists but is not initialized.");
            if (!snapshot.HasShopSession)
                return new NpcShopTransactionReadinessDecision(
                    NpcShopTransactionReadinessAction.RecoverSession);
            if (!snapshot.IsPanelReady)
                return Fail(panelFieldName, "panel is not initialized with this NPC shop context.");
            return new NpcShopTransactionReadinessDecision(NpcShopTransactionReadinessAction.Ready);
        }

        private static NpcShopTransactionReadinessDecision Fail(string fieldName, string cause)
        {
            return new NpcShopTransactionReadinessDecision(
                NpcShopTransactionReadinessAction.Fail,
                fieldName,
                cause);
        }
    }
}
