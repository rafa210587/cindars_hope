using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Farm.Lots
{
    /// <summary>
    /// fable_41 — handler de use-item para as escrituras (deed_lot_*). Registrado por-item no
    /// <see cref="ItemUseManager"/> existente (mesmo pipeline do MagicItemUseHandler), de modo que
    /// a escritura flui pelo caminho de consumo padrão (que remove o item e publica ItemUsedEvent
    /// quando este handler retorna true). NÃO cria um segundo padrão de use-item.
    ///
    /// Retornar false = "não consumir" (lote já é Owned, ou serviço ausente) — a escritura
    /// permanece no inventário e nada é duplicado (idempotência ponta-a-ponta).
    /// </summary>
    public sealed class DeedUseHandler : ItemUseHandler
    {
        public override bool CanUseItem(string itemId, int amount)
        {
            return FarmLotCatalog.IsDeedItem(itemId) && amount > 0;
        }

        public override bool TryUseItem(string itemId, int amount, GameObject user)
        {
            if (!FarmLotCatalog.IsDeedItem(itemId))
            {
                return false;
            }

            var service = FarmLotService.Instance;
            if (service == null)
            {
                Debug.LogWarning($"[DeedUseHandler] FarmLotService ausente; escritura '{itemId}' não consumida.");
                return false;
            }

            // UseDeedByItemId é idempotente: lote já Owned ⇒ false ⇒ escritura não é consumida.
            return service.UseDeedByItemId(itemId);
        }
    }
}
