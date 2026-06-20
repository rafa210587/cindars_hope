using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.World.Events
{
    /// <summary>
    /// fable_37 — barraca temporária de festival (interactable). Criada/destruída pelo WorldEventService
    /// no início/fim do festival (sem prefab/asset — placeholder runtime). Interação simples (NÃO é
    /// minigame): comida grátis, "jogo de pesca" simplificado (item aleatório determinístico por dia +
    /// índice) ou brinde. Concede 1 item por interação e marca-se como usada (anti-exploit: o festival
    /// dura 1 dia e a barraca some no dia seguinte — sem loja infinita).
    ///
    /// Recursos via GameBootstrap.Instance.InventoryManager (ref injetada, não busca global de cena).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FestivalStallInteractable : MonoBehaviour, IInteractable
    {
        public enum StallKind
        {
            FreeFood = 0,
            FishingGame = 1,
            Gift = 2
        }

        // Pool determinístico simples do "jogo de pesca" (itens base garantidos no catálogo).
        private static readonly string[] FishingRewards =
        {
            "item_material_stone",
            "item_material_wood",
            "item_fish_smallfry"
        };

        private const string FreeFoodItemId = "item_dish_festival_snack";

        private StallKind _kind;
        private string _festivalId = string.Empty;
        private string _giftItemId = string.Empty;
        private bool _used;

        public void Configure(StallKind kind, string festivalId, string giftItemId)
        {
            _kind = kind;
            _festivalId = festivalId ?? string.Empty;
            _giftItemId = giftItemId ?? string.Empty;
            _used = false;
        }

        public string InteractionPrompt
        {
            get
            {
                if (_used) return "Barraca (já usada)";
                switch (_kind)
                {
                    case StallKind.FreeFood: return "Comida grátis";
                    case StallKind.FishingGame: return "Jogo de pesca";
                    case StallKind.Gift: return "Pegar brinde";
                    default: return "Barraca de festival";
                }
            }
        }

        public bool CanInteract(GameObject interactor) => !_used;

        public void Interact(GameObject interactor)
        {
            if (_used)
            {
                Publish("A barraca já foi aproveitada hoje.");
                return;
            }

            var inventory = GameBootstrap.Instance != null ? GameBootstrap.Instance.InventoryManager : null;
            if (inventory == null)
            {
                Publish("Sem inventário disponível para a barraca.");
                return;
            }

            var itemId = ResolveRewardItemId();
            if (string.IsNullOrEmpty(itemId))
            {
                Publish("A barraca não tem nada agora.");
                return;
            }

            if (!inventory.AddItem(itemId, 1))
            {
                Publish("Inventário cheio — não foi possível receber o item da barraca.");
                return;
            }

            _used = true;
            Publish($"Você recebeu '{itemId}' na barraca do festival.");
        }

        private string ResolveRewardItemId()
        {
            switch (_kind)
            {
                case StallKind.FreeFood:
                    return FreeFoodItemId;
                case StallKind.Gift:
                    return string.IsNullOrEmpty(_giftItemId) ? FreeFoodItemId : _giftItemId;
                case StallKind.FishingGame:
                    // Determinístico por festival + posição (sem Random/GUID): mesma barraca dá o mesmo item.
                    var hash = WorldEventResolver.StableHash($"{_festivalId}|fishing|{transform.position.x:0.0}|{transform.position.y:0.0}");
                    var index = (int)((uint)hash % (uint)FishingRewards.Length);
                    return FishingRewards[index];
                default:
                    return string.Empty;
            }
        }

        private void Publish(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent(message));
            }
        }
    }
}
