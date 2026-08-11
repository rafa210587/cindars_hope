using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Foundation.Transactions;
using CindarsHope.Player.Data;
using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class PlayerManager : MonoBehaviour, IWalletTransactionPort, IPlayerRuntime
    {
        // arch: quebra do ciclo Core|Player (spec_arch_core_player_cycle_reduction_v37,
        // 2026-07-15) — PlayerManager se anuncia via DomainManagerRegistry (nao um Instance/
        // Active estatico proprio, proibido pela regra de ratchet GlobalGoldAccess) para o
        // GameBootstrap parar de segurar referencia serializada direta a este tipo. Registrado tanto
        // sob o tipo concreto (compat) quanto sob a porta IPlayerRuntime (Foundation), que
        // GameBootstrap usa para resolver/operar sem nomear CindarsHope.Player.
        // arch/bugfix (2026-08-11): guarda de duplicata no self-registro do DomainManagerRegistry.
        // Cada cena (Farm/Town/Cave) traz seu proprio GameBootstrap+PlayerManager; o GameBootstrap e
        // singleton (DontDestroyOnLoad + destroi a duplicata em Awake). Sem esta guarda, o PlayerManager
        // da cena recem-carregada (na duplicata) sobrescrevia em Awake o registro do PlayerManager
        // persistente e, ao ser destruido junto com a duplicata, removia a si mesmo do registry
        // (Unregister e identity-checked), deixando IPlayerRuntime nulo — e o NpcShopController._playerManager
        // falhava no Interact ("required reference is null"). Mesma protecao que os managers de Instance
        // estatica ja tem (molde CraftingManager/ShopManager): a duplicata nao registra nem desregistra
        // o sobrevivente.
        private bool _ownsRegistration;

        private void Awake()
        {
            var existing = DomainManagerRegistry.Get<IPlayerRuntime>();
            if (existing is UnityEngine.Object existingObject && existingObject != null && !ReferenceEquals(existing, this))
            {
                _ownsRegistration = false;
                return;
            }

            DomainManagerRegistry.Register(this);
            DomainManagerRegistry.Register<IPlayerRuntime>(this);
            _ownsRegistration = true;
        }

        private void OnDestroy()
        {
            if (!_ownsRegistration)
            {
                return;
            }

            DomainManagerRegistry.Unregister(this);
            DomainManagerRegistry.Unregister<IPlayerRuntime>(this);
        }

        public bool IsInitialized { get; private set; }
        public int CurrentGold { get; private set; }
        public int CurrentHP { get; private set; }
        public int MaxHP { get; private set; }
        public int Strength { get; set; } = 0;
        int IWalletTransactionPort.Balance => CurrentGold;

        bool IWalletTransactionPort.TryDebit(int amount) => TrySpendGold(amount);

        void IWalletTransactionPort.Credit(int amount) => AddGold(amount);

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;
        }

        // arch: quebra do par mutuo Core|Player (2026-07-15) — implementacao explicita da porta
        // IPlayerRuntime.Initialize(object): GameBootstrap so tem um ScriptableObject generico (para
        // nao nomear PlayerDataSO), entao a porta recebe object e o cast para o tipo concreto
        // acontece aqui, dentro do modulo Player.
        void IPlayerRuntime.Initialize(object playerData) => Initialize(playerData as PlayerDataSO);

        // arch: quebra do par mutuo Core|Player (2026-07-15) — fabrica do CorpseRecoveryManager
        // (Player.Death, mesmo modulo). GameBootstrap.InitializeDeathSystem() construia este objeto
        // via 'new CindarsHope.Player.Death.CorpseRecoveryManager(...)' diretamente; isso nomeava
        // CindarsHope.Player em Core. Agora GameBootstrap chama esta fabrica via IPlayerRuntime e
        // guarda o retorno como 'object' (CorpseRecoveryManager e uma classe C# pura, nao
        // MonoBehaviour; consumidores fora de Core castam localmente).
        object IPlayerRuntime.CreateCorpseRecoveryManager(IInventoryRuntime inventoryManager, IEquipmentRuntime equipmentRuntime)
            => new CindarsHope.Player.Death.CorpseRecoveryManager(this, inventoryManager, equipmentRuntime);

        public void Initialize(PlayerDataSO playerData)
        {
            Initialize();

            if (playerData == null)
            {
                Debug.LogWarning("PlayerManager cannot initialize starting state because PlayerDataSO is missing.", this);
                return;
            }

            var previousGold = CurrentGold;
            CurrentGold = Mathf.Max(0, playerData.StartingGold);
            MaxHP = Mathf.Max(1, playerData.BaseHP);
            CurrentHP = MaxHP;

            var delta = CurrentGold - previousGold;
            if (delta != 0 || CurrentGold != 0)
            {
                GameEventBus.Publish(new GoldChangedEvent(delta == 0 ? CurrentGold : delta, CurrentGold));
            }

            GameEventBus.Publish(new HPChangedEvent(CurrentHP, CurrentHP, MaxHP));
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            IsInitialized = false;
        }

        public void SetGold(int newGold)
        {
            newGold = Mathf.Max(0, newGold);
            if (CurrentGold == newGold)
            {
                return;
            }

            var delta = newGold - CurrentGold;
            CurrentGold = newGold;
            GameEventBus.Publish(new GoldChangedEvent(delta, CurrentGold));
        }

        public bool TrySpendGold(int amount)
        {
            if (amount <= 0 || CurrentGold < amount)
            {
                return false;
            }

            SetGold(CurrentGold - amount);
            return true;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            SetGold(CurrentGold + amount);
        }

        // F18: máximo derivado (equipment/passivas) preservando a proporção corrente (floor 1).
        public void SetMaxHP(int newMaxHP, bool preserveRatio = true)
        {
            newMaxHP = Mathf.Max(1, newMaxHP);
            if (newMaxHP == MaxHP)
            {
                return;
            }

            var ratio = MaxHP > 0 ? (float)CurrentHP / MaxHP : 1f;
            MaxHP = newMaxHP;
            var newCurrent = preserveRatio ? Mathf.Max(1, Mathf.RoundToInt(newMaxHP * ratio)) : Mathf.Min(CurrentHP, newMaxHP);
            SetHP(newCurrent);
        }

        public void SetHP(int newHP)
        {
            MaxHP = Mathf.Max(1, MaxHP);
            newHP = Mathf.Clamp(newHP, 0, MaxHP);
            if (CurrentHP == newHP)
            {
                return;
            }

            var delta = newHP - CurrentHP;
            CurrentHP = newHP;
            GameEventBus.Publish(new HPChangedEvent(delta, CurrentHP, MaxHP));
        }

        public void DamageHP(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            SetHP(CurrentHP - amount);
        }

        public void RestoreHP(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            SetHP(CurrentHP + amount);
        }

        // Estado bruto (HP/MaxHP/Gold) para persistência: o DTO de save (PlayerSaveData) é montado
        // e desmontado pelo caller (PlayerSectionProvider em CindarsHope.Save), que já referencia
        // este namespace. PlayerManager não referencia CindarsHope.Save (ver
        // docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md — corte do par Player|Save).
        public void RestoreState(int maxHP, int currentHP, int gold)
        {
            var previousGold = CurrentGold;
            var previousHp = CurrentHP;

            MaxHP = Mathf.Max(1, maxHP);
            CurrentHP = Mathf.Clamp(currentHP, 0, MaxHP);
            CurrentGold = Mathf.Max(0, gold);

            var goldDelta = CurrentGold - previousGold;
            if (goldDelta != 0)
            {
                GameEventBus.Publish(new GoldChangedEvent(goldDelta, CurrentGold));
            }

            var hpDelta = CurrentHP - previousHp;
            GameEventBus.Publish(new HPChangedEvent(hpDelta, CurrentHP, MaxHP));
        }
    }
}
