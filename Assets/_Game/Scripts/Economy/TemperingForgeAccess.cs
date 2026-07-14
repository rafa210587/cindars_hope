using System;

namespace CindarsHope.Economy
{
    /// <summary>
    /// fable_22 — fachada estática de acesso à forja de têmpera do Brumdar, no padrão de acessor
    /// único já usado por EquipmentManager (RepairEfficiencyProvider.EffectiveRepairAmountSource) e por
    /// <see cref="WeaponInfusionRegistry.Active"/>. Mantém o NpcShopController desacoplado de
    /// QuestFlagService/MainProgression: o gate é um predicado injetado (CA-3), default FECHADO.
    ///
    /// O fluxo de seleção de arma/elemento/tier + confirmação (UI) é DIFERIDO para wiring de cena/
    /// Play Mode; o dono liga <see cref="GateResolver"/> e <see cref="OpenForgeUi"/> ao integrar a
    /// opção real. Aqui ficam apenas o ponto de gate testável e o entry-point opcional.
    /// </summary>
    public static class TemperingForgeAccess
    {
        /// <summary>
        /// Predicado do gate narrativo (sq_brumdar_3_done + Ato 1). Default => FECHADO (fail-closed):
        /// a opção "Temperar" não aparece até o dono ligar o resolver real.
        /// </summary>
        public static Func<bool> GateResolver { get; set; }

        /// <summary>
        /// Entry-point opcional da UI de têmpera (abrir picker/confirmação). Default null => o
        /// chamador apenas sinaliza disponibilidade (toast). Ligado no wiring de cena/Play Mode.
        /// </summary>
        public static Action OpenForgeUi { get; set; }

        /// <summary>True se o gate está aberto (CA-3). Fail-closed quando não há resolver.</summary>
        public static bool IsGateOpen()
        {
            try
            {
                return GateResolver != null && GateResolver();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>True se há UI de forja ligada para abrir (caso contrário, só sinaliza).</summary>
        public static bool HasForgeUi => OpenForgeUi != null;
    }
}
