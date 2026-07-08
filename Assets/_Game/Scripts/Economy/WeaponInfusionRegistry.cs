using System.Collections.Generic;
using CindarsHope.Foundation;

namespace CindarsHope.Economy
{
    /// <summary>
    /// fable_22 — armazém PURO de infusões por instância de arma (chave = itemInstanceId), no MESMO
    /// padrão da durabilidade (EquipmentDurabilityTracker é paralelo ao slot map). Invariante central
    /// da spec: NO MÁXIMO uma infusão por instância (1 elemento por arma). Captura/restaura via DTO
    /// aditivo de equipment (sem refs Unity, sem migration; ausência de infusão = arma sem têmpera).
    ///
    /// Exposto à leitura de combate por um acessor estático único (<see cref="Active"/>) — o ponto
    /// único do cálculo de dano lê a tag de gume daqui, sem busca global de cena.
    /// </summary>
    public sealed class WeaponInfusionRegistry
    {
        private readonly Dictionary<string, WeaponInfusion> _infusions = new Dictionary<string, WeaponInfusion>();

        /// <summary>
        /// Instância ativa para o ponto único de leitura de combate (registrada pelo dono em runtime).
        /// Null em teste puro / cenas sem têmpera => combate trata como sem infusão (neutro).
        /// </summary>
        public static WeaponInfusionRegistry Active { get; set; }

        public int Count => _infusions.Count;

        /// <summary>Infusão da instância, ou <see cref="WeaponInfusion.None"/> se não houver.</summary>
        public WeaponInfusion Get(string itemInstanceId)
        {
            if (string.IsNullOrWhiteSpace(itemInstanceId)) return WeaponInfusion.None;
            return _infusions.TryGetValue(itemInstanceId, out var infusion) ? infusion : WeaponInfusion.None;
        }

        public bool HasInfusion(string itemInstanceId)
        {
            return Get(itemInstanceId).IsActive;
        }

        /// <summary>
        /// Grava a infusão da instância. Invariante 1-elemento: SUBSTITUI qualquer infusão prévia
        /// (nunca coexistem duas). Infusão inativa (None/tier 0) remove a entrada.
        /// </summary>
        public void Set(string itemInstanceId, WeaponInfusion infusion)
        {
            if (string.IsNullOrWhiteSpace(itemInstanceId)) return;
            if (!infusion.IsActive)
            {
                _infusions.Remove(itemInstanceId);
                return;
            }
            _infusions[itemInstanceId] = infusion;
        }

        public void Clear(string itemInstanceId)
        {
            if (string.IsNullOrWhiteSpace(itemInstanceId)) return;
            _infusions.Remove(itemInstanceId);
        }

        public void ClearAll()
        {
            _infusions.Clear();
        }

        /// <summary>
        /// fable_22 CA-5 — provedor opcional da tag de óleo ATIVO na instância (futuro sistema de
        /// óleos temporários). Recebe o itemInstanceId e devolve a tag de gume do óleo, ou null se
        /// não houver óleo ativo. Quando presente, o óleo SUPRIME a têmpera pela duração.
        /// Default null => sem óleo => vale a têmpera.
        /// </summary>
        public System.Func<string, string> ActiveCoatingTagProvider { get; set; }

        /// <summary>
        /// Tag de gume canônica EFETIVA da instância para o matching F06: óleo ativo (se houver)
        /// SUPRIME a têmpera; senão a tag da infusão; null se nenhum dos dois. Ponto único que o
        /// combate consome.
        /// </summary>
        public string GetEdgeTag(string itemInstanceId)
        {
            var infusion = Get(itemInstanceId);
            string infusionTag = infusion.IsActive ? TemperingCanon.EdgeTag(infusion.Element) : null;
            string coatingTag = ActiveCoatingTagProvider != null ? ActiveCoatingTagProvider(itemInstanceId) : null;
            return TemperingCanon.ResolveActiveEdgeTag(infusionTag, coatingTag);
        }

        // ─── Save (aditivo no DTO de equipment) ──────────────────────────────────────────────────

        /// <summary>Snapshot serializável das infusões (defaults vazios => save legado sem têmpera).</summary>
        public List<WeaponInfusionSaveData> CaptureSaveData()
        {
            var list = new List<WeaponInfusionSaveData>(_infusions.Count);
            foreach (var kvp in _infusions)
            {
                if (!kvp.Value.IsActive) continue;
                list.Add(new WeaponInfusionSaveData
                {
                    ItemInstanceId = kvp.Key,
                    InfusionElement = TemperingCanon.ToStableString(kvp.Value.Element),
                    InfusionTier = kvp.Value.Tier
                });
            }
            return list;
        }

        /// <summary>
        /// Restaura de um snapshot. Lista null/vazia (save legado) => registro vazio, sem migration.
        /// Entradas inválidas (elemento desconhecido / tier fora de 1-2) são ignoradas com segurança.
        /// </summary>
        public void RestoreFromSaveData(List<WeaponInfusionSaveData> data)
        {
            _infusions.Clear();
            if (data == null) return;
            foreach (var entry in data)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemInstanceId)) continue;
                var element = TemperingCanon.ParseElement(entry.InfusionElement);
                if (element == TemperingElement.None) continue;
                if (!TemperingCanon.IsValidTier(entry.InfusionTier)) continue;
                _infusions[entry.ItemInstanceId] = new WeaponInfusion(element, entry.InfusionTier);
            }
        }
    }
}
