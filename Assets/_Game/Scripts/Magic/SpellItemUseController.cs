using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using UnityEngine;

namespace CindarsHope.Magic
{
    /// <summary>
    /// fable_07 — MonoBehaviour fina que dirige o <see cref="SpellItemUseHandler"/> a partir de input,
    /// no espírito do FoodConsumer. Resolve dependências via <see cref="GameBootstrap.Instance"/>
    /// (sem busca global de cena), respeita o ModalManager (não usa item com modal aberto) e mantém
    /// toda a regra no handler/estado (testáveis). NÃO implementa um segundo caminho de cast.
    /// </summary>
    [DisallowMultipleComponent]
    public class SpellItemUseController : MonoBehaviour
    {
        [SerializeField] private KeyCode _useScrollKey = KeyCode.J;

        // Adapter do inventário real para a abstração testável do handler.
        private sealed class InventoryUseAdapter : SpellItemUseHandler.IInventoryUse
        {
            private readonly InventoryManager _inventory;

            public InventoryUseAdapter(InventoryManager inventory)
            {
                _inventory = inventory;
            }

            public bool HasItem(string itemId, int amount = 1) => _inventory.HasItem(itemId, amount);

            public bool TryGetItemData(string itemId, out ItemDataSO itemData) =>
                _inventory.TryGetItemData(itemId, out itemData);

            public bool RemoveItem(string itemId, int amount) => _inventory.RemoveItem(itemId, amount);
        }

        private void Update()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap?.ModalManager != null && bootstrap.ModalManager.HasActiveModal)
            {
                return;
            }

            if (Input.GetKeyDown(_useScrollKey))
            {
                TryUseFirstUsableSpellItem();
            }
        }

        /// <summary>
        /// Procura no inventário o primeiro item de magia "usável" (scroll/tome) e o usa.
        /// Mantém o input simples (sem UI de seleção, deferida para F14); a regra mora no handler.
        /// </summary>
        public void TryUseFirstUsableSpellItem()
        {
            var bootstrap = GameBootstrap.Instance;
            var spellbook = PlayerSpellbook.Instance;
            var inventory = bootstrap?.InventoryManager;
            if (spellbook == null || inventory == null)
            {
                Debug.LogWarning("SpellItemUseController: PlayerSpellbook ou InventoryManager ausente; uso ignorado.", this);
                return;
            }

            var skillTree = bootstrap.SkillTreeManager;
            var handler = new SpellItemUseHandler(
                new InventoryUseAdapter(inventory),
                spellbook.State,
                isSkillNodeUnlocked: skillTree != null ? skillTree.IsNodeUnlocked : (System.Func<string, bool>)null,
                castSpell: null, // CastScroll runtime depende do contexto de cast do player (Play Mode, diferido).
                onLearned: (spellId, source) => spellbook.PublishLearned(spellId, source));

            string itemId = FindFirstUsableSpellItemId(inventory);
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.Log("SpellItemUseController: nenhum item de magia usável no inventário.", this);
                return;
            }

            var result = handler.UseItem(itemId);
            PublishFeedback(result, itemId);
        }

        private static string FindFirstUsableSpellItemId(InventoryManager inventory)
        {
            var slots = inventory.Slots;
            if (slots == null)
            {
                return null;
            }

            foreach (var slot in slots)
            {
                if (slot == null || string.IsNullOrEmpty(slot.ItemId))
                {
                    continue;
                }

                if (!inventory.TryGetItemData(slot.ItemId, out var data) || data == null)
                {
                    continue;
                }

                if (data.SpellSource == SpellSourceType.LearnableScroll
                    || data.SpellSource == SpellSourceType.CastScroll
                    || data.SpellSource == SpellSourceType.Tome)
                {
                    return slot.ItemId;
                }
            }

            return null;
        }

        private void PublishFeedback(SpellItemUseHandler.UseResult result, string itemId)
        {
            switch (result)
            {
                case SpellItemUseHandler.UseResult.Learned:
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("Magia aprendida!"));
                    break;
                case SpellItemUseHandler.UseResult.TomeStudied:
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("Voce estuda o tomo..."));
                    break;
                case SpellItemUseHandler.UseResult.PrerequisiteNotMet:
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("Dominio nao desbloqueado."));
                    break;
                case SpellItemUseHandler.UseResult.AlreadyKnown:
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("Magia ja conhecida."));
                    break;
                case SpellItemUseHandler.UseResult.Cast:
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("Pergaminho conjurado!"));
                    break;
                default:
                    Debug.Log($"SpellItemUseController: uso de '{itemId}' resultou em {result}.", this);
                    break;
            }
        }
    }
}
