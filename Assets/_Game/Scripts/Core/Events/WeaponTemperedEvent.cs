namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_22 — publicado quando uma arma é temperada (ou re-temperada) na forja do Brumdar.
    /// Consumido pelo toast existente (PlayerActionFeedbackEvent) — nenhum evento existente muda.
    /// </summary>
    public readonly struct WeaponTemperedEvent
    {
        public readonly string WeaponInstanceId;
        public readonly string Element; // estável: fire/ice/toxic/lightning/arcane/void
        public readonly int Tier;

        public WeaponTemperedEvent(string weaponInstanceId, string element, int tier)
        {
            WeaponInstanceId = weaponInstanceId ?? string.Empty;
            Element = element ?? string.Empty;
            Tier = tier;
        }
    }
}
