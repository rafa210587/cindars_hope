namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_48 — publicado quando o arco esgota a pilha de munição equipada e o
    /// <c>BowArrowAttackService</c> auto-equipa a próxima flecha compatível do inventário (ordem
    /// canônica do catálogo). HUD/hotbar podem escutar para feedback (toast) sem UI dedicada.
    ///
    /// Evento aditivo (ADR-0007): DTO puro, sem refs Unity. <see cref="NewAmmoId"/> vazio quando
    /// não havia nenhuma munição compatível (a troca falhou; o bloqueio NoArrowsInInventory segue).
    /// </summary>
    public class AmmoAutoSelectedEvent
    {
        public string PreviousAmmoId { get; }
        public string NewAmmoId { get; }

        /// <summary>True quando uma nova munição compatível foi efetivamente equipada.</summary>
        public bool HasNewAmmo => !string.IsNullOrEmpty(NewAmmoId);

        public AmmoAutoSelectedEvent(string previousAmmoId, string newAmmoId)
        {
            PreviousAmmoId = previousAmmoId ?? string.Empty;
            NewAmmoId = newAmmoId ?? string.Empty;
        }
    }
}
