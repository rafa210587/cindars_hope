using System;
using System.Collections.Generic;

namespace CindarsHope.Farm
{
    // arch: movido de CindarsHope.Save (SaveData.cs) para CindarsHope.Farm — quebra do ciclo mutuo
    // Farm|Save (spec_arch_farm_save_cycle_reduction). DTO [Serializable], serializado por campo:
    // move e transparente para save files existentes (mesmo padrao de InventorySaveData/
    // NpcManagerSaveData). Ver campo GameSaveData.Farm em Save/SaveData.cs.
    [Serializable]
    public class FarmSaveData
    {
        public List<FarmPlotSaveData> Plots = new List<FarmPlotSaveData>();
        public List<CindarsHope.World.TreeSaveData> Trees = new List<CindarsHope.World.TreeSaveData>();

        // fable_55: jobs de processamento ativos (queijaria/barril). Campo ADITIVO na seção farm;
        // só IDs/ints (ADR-0006). Ausente em save legado = sem jobs (CA-5). Dono: FarmProcessingStationService.
        public CindarsHope.Farm.Processing.FarmProcessingSaveData Processing = new CindarsHope.Farm.Processing.FarmProcessingSaveData();

        // fable_54: batch de envio pendente da caixa de shipping (venda overnight). Campo ADITIVO na
        // seção farm; só ids/ints/floats (ADR-0006). Ausente em save legado = lista vazia = nenhum
        // batch pendente (CA-4). Dono: ShippingBinRuntimeService.
        public CindarsHope.Farm.Shipping.PendingShippingSaveData PendingShipping = new CindarsHope.Farm.Shipping.PendingShippingSaveData();

        // fable_54: estado dos pontos de forrageio do dia. Campo ADITIVO na seção farm; só ids/ints
        // (ADR-0006). Ausente em save legado = lista vazia = spawns regeneram no próximo DayStarted
        // (CA-4). Dono: FarmForageRuntimeService.
        public CindarsHope.Farm.Forage.ForageSpawnsSaveData ForageSpawns = new CindarsHope.Farm.Forage.ForageSpawnsSaveData();
    }
}
