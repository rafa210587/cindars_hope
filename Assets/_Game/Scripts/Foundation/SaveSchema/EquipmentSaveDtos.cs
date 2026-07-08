using System;
using System.Collections.Generic;

namespace CindarsHope.Foundation
{
    // arch: quebra do ciclo mutuo Equipment|Save (spec_arch_equipment_save_cycle_reduction_v29) —
    // DTOs puros (sem refs Unity) movidos de CindarsHope.Save para o schema de save em Foundation,
    // decisao explicita de arquitetura (mesma tecnica de EconomySaveDtos/HotbarSaveData/QuestSource).
    // JsonUtility serializa por NOME DE CAMPO, nao por namespace: o JSON no disco fica identico, sem
    // migration. EquipmentSlotSaveData depende do enum EquipmentSlot (tambem movido para Foundation).
    [Serializable]
    public class EquipmentSaveData
    {
        public string EquippedToolId;
        public List<EquipmentSlotSaveData> Slots = new List<EquipmentSlotSaveData>();
        // fable_22 (aditivo): infusões de têmpera por instância de arma/ferramenta. Default VAZIO =>
        // saves legados carregam sem têmpera, sem migration. Strings/ints simples (sem refs Unity).
        public List<WeaponInfusionSaveData> Infusions = new List<WeaponInfusionSaveData>();
        // fable_49 (aditivo): upgrade focado (+1/+2/+3) por instância de equipamento. Default VAZIO =>
        // saves legados carregam sem upgrade (level 0), sem migration. Tipos simples (sem refs Unity).
        public List<EquipmentUpgradeSaveData> Upgrades = new List<EquipmentUpgradeSaveData>();
        // fable_49 (aditivo): receitas de tier alto APRENDIDas via first-kill (slugs estáveis). Default
        // VAZIO => save legado sem receitas, sem migration. Persistido aqui (mesmo owner do equipment
        // save) para não tocar o SaveManager core; consultado pelo gating de craft.
        public List<string> UnlockedRecipeIds = new List<string>();
    }

    [Serializable]
    public class EquipmentSlotSaveData
    {
        public EquipmentSlot SlotType;
        public string ItemInstanceId;
    }

    // fable_49: entrada aditiva de upgrade (itemInstanceId → level/focus). Tipos simples, sem refs
    // Unity, compatível com JsonUtility. Ausência da entrada = item sem upgrade (level 0). Derivados
    // (dano/durabilidade) NUNCA persistidos (§45) — recalculados no load a partir deste registro.
    [Serializable]
    public class EquipmentUpgradeSaveData
    {
        public string ItemInstanceId;
        public int UpgradeLevel;   // 0 = sem; 1..3
        public string UpgradeFocus; // estável: damage/durability/weight/stamina/block
    }

    [Serializable]
    public class EquipmentDurabilitySaveData
    {
        public List<DurabilityEntryData> EquipmentDurabilities = new List<DurabilityEntryData>();
    }

    [Serializable]
    public class DurabilityEntryData
    {
        public string ItemInstanceId;
        public int CurrentDurability;
        public int MaxDurability;
    }
}
