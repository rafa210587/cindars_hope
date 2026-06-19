using System;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using CindarsHope.Player;

namespace CindarsHope.Economy
{
    /// <summary>
    /// fable_22 — adapter da porta de inventário sobre o <see cref="InventoryManager"/> real.
    /// Encapsula HasItem/RemoveItem/AddItem (sem expor o manager ao serviço puro).
    /// </summary>
    public sealed class InventoryManagerTemperingAdapter : ITemperingInventory
    {
        private readonly InventoryManager _inventory;

        public InventoryManagerTemperingAdapter(InventoryManager inventory)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        }

        public bool HasItem(string itemId, int amount) => _inventory.HasItem(itemId, amount);
        public bool RemoveItem(string itemId, int amount) => _inventory.RemoveItem(itemId, amount);
        public bool AddItem(string itemId, int amount) => _inventory.AddItem(itemId, amount);
    }

    /// <summary>
    /// fable_22 — adapter da porta de carteira sobre o <see cref="PlayerManager"/> real.
    /// </summary>
    public sealed class PlayerManagerTemperingWallet : ITemperingWallet
    {
        private readonly PlayerManager _player;

        public PlayerManagerTemperingWallet(PlayerManager player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public int CurrentGold => _player.CurrentGold;
        public bool TrySpendGold(int amount) => _player.TrySpendGold(amount);
    }

    /// <summary>
    /// fable_22 — classificador por ID de instância (heurística estável de nomes, no mesmo espírito
    /// de EquipmentManager.InferToolTypeFromId). Ferramentas: hoe/axe/pickaxe/watering/fishing/sickle.
    /// Demais armas (sword/spear/bow/staff/dagger/hammer/wand/weapon_) contam como arma.
    /// Pode ser substituído por um classificador baseado em ItemDatabase no wiring real.
    /// </summary>
    public sealed class IdHeuristicItemClassifier : ITemperingItemClassifier
    {
        public bool IsToolInstance(string itemInstanceId)
        {
            if (string.IsNullOrWhiteSpace(itemInstanceId)) return false;
            var id = itemInstanceId.ToLowerInvariant();
            return id.Contains("hoe") || id.Contains("axe") || id.Contains("pickaxe")
                || id.Contains("watering") || id.Contains("fishing") || id.Contains("sickle");
        }

        public bool IsWeaponInstance(string itemInstanceId)
        {
            if (string.IsNullOrWhiteSpace(itemInstanceId)) return false;
            // axe é ambíguo (machado = ferramenta); aqui tratamos ferramentas como NÃO-arma.
            if (IsToolInstance(itemInstanceId)) return false;
            var id = itemInstanceId.ToLowerInvariant();
            return id.Contains("sword") || id.Contains("spear") || id.Contains("bow")
                || id.Contains("staff") || id.Contains("dagger") || id.Contains("hammer")
                || id.Contains("wand") || id.Contains("weapon");
        }
    }

    /// <summary>
    /// fable_22 — fábrica do <see cref="TemperingService"/> com adapters reais e o callback que
    /// publica <see cref="WeaponTemperedEvent"/> + toast (PlayerActionFeedbackEvent) ao temperar.
    /// Usada pelo wiring de cena/Play Mode (DIFERIDO) para construir a forja com managers vivos.
    /// </summary>
    public static class TemperingServiceFactory
    {
        public static TemperingService Create(
            WeaponInfusionRegistry registry,
            InventoryManager inventory,
            PlayerManager player,
            ITemperingItemClassifier classifier = null)
        {
            return new TemperingService(
                registry,
                new InventoryManagerTemperingAdapter(inventory),
                new PlayerManagerTemperingWallet(player),
                classifier ?? new IdHeuristicItemClassifier(),
                PublishTemperedFeedback);
        }

        private static void PublishTemperedFeedback(WeaponInfusion infusion, string instanceId)
        {
            string element = TemperingCanon.ToStableString(infusion.Element);
            GameEventBus.Publish(new WeaponTemperedEvent(instanceId, element, infusion.Tier));
            GameEventBus.Publish(new PlayerActionFeedbackEvent(
                $"Arma temperada: {element} T{infusion.Tier}."));
        }
    }
}
