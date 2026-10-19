namespace CindarsHope.Farm.Lots
{
    /// <summary>
    /// fable_41 — definição imutável de um lote de expansão (dado puro, sem Unity).
    /// Liga o lote à sua escritura (item KeyItem F32) e ao preço de venda na prefeitura/Veska.
    ///
    /// Decisão 5.3 (EMENDA v2): a expansão é gated APENAS por dinheiro/recursos — NENHUM gate
    /// de caverna/fragmento. Por isso não há campo de marco de caverna aqui.
    /// </summary>
    public sealed class FarmLotDefinition
    {
        public string LotId { get; }

        /// <summary>Item KeyItem (catálogo F32) cuja posse/uso destrava o lote.</summary>
        public string DeedItemId { get; }

        /// <summary>Preço de COMPRA da escritura no ponto de venda (sink de ouro 5.3).</summary>
        public int DeedPrice { get; }

        /// <summary>Texto da placa enquanto o lote está Locked.</summary>
        public string DisplayName { get; }

        public FarmLotDefinition(string lotId, string deedItemId, int deedPrice, string displayName)
        {
            LotId = lotId;
            DeedItemId = deedItemId;
            DeedPrice = deedPrice;
            DisplayName = displayName;
        }
    }
}
