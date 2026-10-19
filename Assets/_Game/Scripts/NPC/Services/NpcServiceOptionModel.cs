namespace CindarsHope.NPC.Services
{
    /// <summary>
    /// fable_25 — modelo PURO de uma opção de serviço no Conversar (CA-4 descoberta > ocultação).
    /// A opção NUNCA some: quando bloqueada, <see cref="Enabled"/> é false e <see cref="DisabledReason"/>
    /// carrega o requisito visível ("Amizade 3 necessaria"). O NpcShopController renderiza o rótulo
    /// (habilitado) ou rótulo + motivo (desabilitado); ao selecionar uma opção desabilitada, mostra o
    /// motivo por toast em vez de executar. EditMode-testável (sem Unity).
    /// </summary>
    public readonly struct NpcServiceOptionModel
    {
        public readonly string ServiceId;
        public readonly string NpcId;

        /// <summary>Rótulo final exibido (inclui o motivo entre parênteses quando desabilitado).</summary>
        public readonly string Label;

        /// <summary>Rótulo base sem o motivo (útil para testes/telemetria).</summary>
        public readonly string BaseLabel;

        public readonly bool Enabled;

        /// <summary>Motivo do bloqueio (vazio quando habilitado). Ex.: "Amizade 3 necessaria".</summary>
        public readonly string DisabledReason;

        public NpcServiceOptionModel(
            string serviceId, string npcId, string baseLabel, bool enabled, string disabledReason)
        {
            ServiceId = serviceId;
            NpcId = npcId;
            BaseLabel = baseLabel;
            Enabled = enabled;
            DisabledReason = disabledReason ?? string.Empty;
            Label = enabled || string.IsNullOrEmpty(DisabledReason)
                ? baseLabel
                : $"{baseLabel} ({DisabledReason})";
        }
    }
}
