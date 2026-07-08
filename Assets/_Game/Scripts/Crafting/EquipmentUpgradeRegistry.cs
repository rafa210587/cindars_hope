using System.Collections.Generic;
using CindarsHope.Foundation;

namespace CindarsHope.Crafting
{
    /// <summary>
    /// fable_49 — upgrade {level, focus} por instância de equipamento (chave = itemInstanceId), no MESMO
    /// padrão aditivo da têmpera (WeaponInfusionRegistry) e da durabilidade. Invariante §35: um foco por
    /// nível e teto +3. Capturado/restaurado de forma aditiva no DTO de equipment (sem refs Unity, sem
    /// migration; ausência de entrada = item sem upgrade, level 0). NUNCA persiste derivados (§45): o
    /// recálculo de stats/durabilidade é feito no load pelo fluxo existente lendo este registro.
    /// </summary>
    public sealed class EquipmentUpgradeRegistry
    {
        private readonly Dictionary<string, EquipmentUpgrade> _upgrades = new Dictionary<string, EquipmentUpgrade>();

        /// <summary>Instância ativa para leitura única em runtime (registrada pelo dono).</summary>
        public static EquipmentUpgradeRegistry Active { get; set; }

        public int Count => _upgrades.Count;

        /// <summary>Upgrade da instância, ou <see cref="EquipmentUpgrade.None"/> (level 0) se não houver.</summary>
        public EquipmentUpgrade Get(string itemInstanceId)
        {
            if (string.IsNullOrWhiteSpace(itemInstanceId)) return EquipmentUpgrade.None;
            return _upgrades.TryGetValue(itemInstanceId, out var upgrade) ? upgrade : EquipmentUpgrade.None;
        }

        public int GetLevel(string itemInstanceId) => Get(itemInstanceId).Level;

        public bool HasUpgrade(string itemInstanceId) => Get(itemInstanceId).IsActive;

        /// <summary>
        /// Grava o upgrade da instância. Level 0 / foco None remove a entrada. Invariante: um único par
        /// {level, focus} por instância (o serviço valida teto/foco antes de chamar).
        /// </summary>
        public void Set(string itemInstanceId, EquipmentUpgrade upgrade)
        {
            if (string.IsNullOrWhiteSpace(itemInstanceId)) return;
            if (!upgrade.IsActive)
            {
                _upgrades.Remove(itemInstanceId);
                return;
            }
            _upgrades[itemInstanceId] = upgrade;
        }

        public void Clear(string itemInstanceId)
        {
            if (string.IsNullOrWhiteSpace(itemInstanceId)) return;
            _upgrades.Remove(itemInstanceId);
        }

        public void ClearAll()
        {
            _upgrades.Clear();
        }

        // ─── Save (aditivo no DTO de equipment) ─────────────────────────────────────────────────

        /// <summary>Snapshot serializável dos upgrades (defaults vazios => save legado sem upgrade).</summary>
        public List<EquipmentUpgradeSaveData> CaptureSaveData()
        {
            var list = new List<EquipmentUpgradeSaveData>(_upgrades.Count);
            foreach (var kvp in _upgrades)
            {
                if (!kvp.Value.IsActive) continue;
                list.Add(new EquipmentUpgradeSaveData
                {
                    ItemInstanceId = kvp.Key,
                    UpgradeLevel = kvp.Value.Level,
                    UpgradeFocus = HighTierGearCanonFocus.ToStableString(kvp.Value.Focus)
                });
            }
            return list;
        }

        /// <summary>
        /// Restaura de um snapshot. Lista null/vazia (save legado) => registro vazio, sem migration.
        /// Entradas inválidas (level fora de 1-3 / foco desconhecido) são ignoradas com segurança.
        /// </summary>
        public void RestoreFromSaveData(List<EquipmentUpgradeSaveData> data)
        {
            _upgrades.Clear();
            if (data == null) return;
            foreach (var entry in data)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemInstanceId)) continue;
                if (!HighTierGearCanon.IsValidUpgradeLevel(entry.UpgradeLevel)) continue;
                var focus = HighTierGearCanonFocus.ParseFocus(entry.UpgradeFocus);
                if (focus == UpgradeFocus.None) continue;
                _upgrades[entry.ItemInstanceId] = new EquipmentUpgrade(entry.UpgradeLevel, focus);
            }
        }
    }
}
