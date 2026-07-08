using CindarsHope.Core.Data;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Inventory.Data
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "CindarsHope/Data/Item")]
    public class ItemDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;
        public ItemCategory Category;
        public ConsumableSubtype ConsumableSubtype;
        public int MaxStack = 1;
        public int BaseValue;
        public int HungerRestore;
        public int StaminaRestore;
        public string[] StatusEffectIds;
        public float BuffDurationSeconds;
        public bool IsEquippable;
        public int DurabilityRestoreAmount;
        public string WeaponId;
        public string SpellId;

        // SPEC_08: Item use contract fields — backward compatible; UseKind.None triggers fallback inference
        public ItemUseKind UseKind = ItemUseKind.None;
        public EquipmentSlot[] AllowedEquipmentSlots;
        public string AmmoType;
        public ItemUseKind RequiredPairedUseKind = ItemUseKind.None;

        // fable_07: fontes de aprendizado/desbloqueio de magia (campos aditivos, defaults neutros).
        // SpellSource=None mantém 100% o comportamento legado (magia = item equipado via SpellId).
        public CindarsHope.Magic.SpellSourceType SpellSource = CindarsHope.Magic.SpellSourceType.None;
        // Magia ensinada por LearnableScroll/Tome (knownSpellIds). Vazio para CastScroll/EquippedItem.
        public string TaughtSpellId;
        // Tome: número de usos (estudos) para concluir o aprendizado. <=1 aprende no 1º uso.
        public int TomeUsesRequired = 3;
        // LearnableScroll: nó da skill tree exigido como pré-requisito de domínio (vazio = sem pré-requisito).
        public string RequiredSkillNodeId;

        // fable_31: itens mágicos não-identificados (campos aditivos, defaults neutros — backward compatible).
        // IsUnidentified=false e IdentifiedItemId/PassiveFlag vazios mantêm 100% o comportamento legado.
        // Padrão PAR DE ITENS: o item "não-identificado" aponta para o item real via IdentifiedItemId;
        // identificar = swap 1:1 no inventário (sem metadata de instância, sem schema novo de save).
        public bool IsUnidentified;
        // ID do item real revelado ao identificar (o "par"). Só lido quando IsUnidentified=true.
        public string IdentifiedItemId;
        // Nome do hook contínuo (passiveFlag) ativado pela PRESENÇA do item no inventário (ItemPassiveTracker).
        // Vazio = item sem efeito passivo. Ex.: pendant->HUD HP, lantern->reveal, pouch->+slots, candle->luz.
        public string PassiveFlag;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            MaxStack = Mathf.Max(1, MaxStack);
            BaseValue = Mathf.Max(0, BaseValue);
            HungerRestore = Mathf.Max(0, HungerRestore);
            StaminaRestore = Mathf.Max(0, StaminaRestore);
            BuffDurationSeconds = Mathf.Max(0, BuffDurationSeconds);
            DurabilityRestoreAmount = Mathf.Max(0, DurabilityRestoreAmount);
        }
    }
}
