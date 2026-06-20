using System.Collections.Generic;
using CindarsHope.Combat.Bestiary;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Enemy;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Items;
using CindarsHope.Items.Runtime;
using CindarsHope.NPC.Friendship;
using CindarsHope.Player;
using CindarsHope.Player.Conditions;
using UnityEngine;

namespace CindarsHope.NPC.Services
{
    /// <summary>
    /// fable_25 — bridge de RUNTIME dos serviços únicos de NPC (singleton DontDestroyOnLoad, padrão
    /// GiftGivingService/PlayerConditionService). Liga o <see cref="NpcServiceExecutor"/> puro aos
    /// sistemas reais (F26 amizade, QuestFlag, Player/Inventory para custos; F21/F16/F12/F31 + hooks de
    /// reparo/contrato para efeitos), publica o <see cref="NpcServiceAccess.ExecutorProvider"/> para o
    /// NpcShopController, consome DayStartedEvent (entrega de encomenda + re-alimentação do pasto) e
    /// EnemyKilledEvent (conclusão do contrato), e expõe Capture/Restore para o SaveManager.
    ///
    /// Arquitetura (ADR-0007): comunicação de gameplay só via GameEventBus; refs resolvidas por
    /// GameBootstrap.Instance / *.Instance; sem GameObject.Find em gameplay (o bootstrap usa
    /// FindAnyObjectByType só para evitar duplicata — padrão do projeto). NÃO reimplementa nenhum efeito.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NpcServiceRuntime : MonoBehaviour, INpcServiceContext, INpcServiceEffects
    {
        private static NpcServiceRuntime _instance;
        public static NpcServiceRuntime Instance => _instance;

        private readonly NpcServicesPendingState _pending = new NpcServicesPendingState();
        private NpcServiceExecutor _executor;
        private int _currentDay = 1;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _executor = new NpcServiceExecutor(_pending, this, this)
            {
                CurrentDayProvider = () => _currentDay
            };
            NpcServiceAccess.ExecutorProvider = () => _executor;
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                if (NpcServiceAccess.ExecutorProvider != null) NpcServiceAccess.ExecutorProvider = null;
            }
        }

        // ── Event hooks ──────────────────────────────────────────────────────────────────────────

        private void OnDayStarted(DayStartedEvent evt)
        {
            _currentDay = evt.DayNumber;

            // Encomendas vencidas (Yael) → adiciona ao inventário.
            var due = _executor.OnDayStarted(evt.DayNumber);
            var inventory = ResolveInventory();
            if (inventory != null)
            {
                foreach (var bookId in due)
                {
                    if (!string.IsNullOrEmpty(bookId) && inventory.AddItem(bookId, 1))
                    {
                        GameEventBus.Publish(new PlayerActionFeedbackEvent("Sua encomenda de livro chegou."));
                    }
                }
            }

            // Pasto premium ativo (Eiran) → re-alimenta os animais hoje (hook F12).
            if (_executor.IsPremiumPastureActive(evt.DayNumber))
            {
                var registry = Farm.Animals.FarmAnimalRegistry.Instance;
                if (registry != null) registry.FeedAllLiveAnimals();
            }
        }

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (string.IsNullOrEmpty(evt.EnemyId)) return;
            if (_executor.OnEnemyKilled(evt.EnemyId))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Contrato de caca concluido! Recompensa em dobro."));
            }
        }

        // ── INpcServiceContext (gates + custos) ────────────────────────────────────────────────────

        public bool FriendshipAtLeast(string npcId, int level)
        {
            var fs = FriendshipService.Instance;
            return fs != null && fs.IsAtLeast(npcId, level);
        }

        public bool FlagIsSet(string flagId)
        {
            // Fail-closed: sem serviço de flag runtime-acessível desta camada, o gate de flag fica
            // fechado (nenhum dos 8 serviços usa flag por padrão; previsto para extensões futuras).
            return false;
        }

        public int CurrentGold()
        {
            var pm = ResolvePlayer();
            return pm != null ? pm.CurrentGold : 0;
        }

        public bool SpendGold(int amount)
        {
            var pm = ResolvePlayer();
            return pm != null && pm.TrySpendGold(amount);
        }

        public bool HasItem(string itemId, int amount)
        {
            var inv = ResolveInventory();
            return inv != null && inv.HasItem(itemId, amount);
        }

        public bool RemoveItem(string itemId, int amount)
        {
            var inv = ResolveInventory();
            return inv != null && inv.RemoveItem(itemId, amount);
        }

        // ── INpcServiceEffects (despacho aos sistemas-alvo; nada reimplementado) ─────────────────────

        public bool TryCreatureAnalysis()
        {
            // F21: consome 1 parte de monstro do inventário e concede a categoria faltante mais valiosa
            // na criatura mais relevante ainda não documentada. Idempotência herdada do GrantKnowledge.
            var bestiary = ResolveBestiary();
            var inventory = ResolveInventory();
            if (bestiary == null || inventory == null) return false;

            string monsterPartId = FindMonsterPartItemId(inventory);
            if (string.IsNullOrEmpty(monsterPartId)) return false;

            if (!TryResolveAnalysisGrant(bestiary, out var enemyId, out var category)) return false;

            if (!inventory.RemoveItem(monsterPartId, 1)) return false;

            bool granted = bestiary.Knowledge.GrantKnowledge(enemyId, category, "thalindra_analysis");
            if (!granted)
            {
                // Idempotente: nada novo concedido ⇒ devolve a parte (não cobra). Mantém honestidade.
                inventory.AddItem(monsterPartId, 1);
                return false;
            }

            return true;
        }

        public bool ApplyRepairDiscount(float discountFraction)
        {
            NpcRepairDiscountState.ArmDiscount(discountFraction);
            return true;
        }

        public bool ApplyThermalBath()
        {
            // F16: remove a fadiga acumulada (threshold volta a Rested). O buff "+10% stamina regen até
            // dormir" depende de um status Rested da F01 que ainda não existe como modificador de regen
            // (débito documentado); o efeito mecânico primário (remover fadiga) é real.
            var conditions = PlayerConditionService.Instance;
            if (conditions == null) return false;
            conditions.SetFatigue(0f);
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Banho termal: voce se sente descansado."));
            return true;
        }

        public bool ApplyPremiumPasture(int days)
        {
            var registry = Farm.Animals.FarmAnimalRegistry.Instance;
            if (registry == null) return false;
            registry.FeedAllLiveAnimals(); // alimenta hoje; dias seguintes via OnDayStarted enquanto ativo
            return true;
        }

        public string ResolveOrderedBookItemId()
        {
            // Livro de conhecimento da banda à escolha. Resolve um id de livro real do ItemDatabase; se
            // o item não existir no banco, retorna null ⇒ EffectUnavailable (não cobra). Convenção do
            // catálogo de itens (F32): "item_book_*". Sem banco, fail-closed.
            var boot = GameBootstrap.Instance;
            var db = boot != null ? boot.ItemDatabase : null;
            if (db == null) return null;

            foreach (var candidate in CandidateBookItemIds)
            {
                if (db.TryGetById(candidate, out _)) return candidate;
            }

            return null;
        }

        public string ResolveHuntContractTarget()
        {
            // Alvo elite da banda atual. Resolve o primeiro inimigo não-chefe do catálogo canônico; sem
            // catálogo, usa o fallback estável. (O padrão board F34 não existe; o contrato vive na seção
            // de pendências desta spec + conclusão por EnemyKilledEvent.)
            foreach (var def in CanonicalBestiaryCatalog.All)
            {
                if (!def.IsBoss && !string.IsNullOrEmpty(def.EnemyId))
                {
                    return def.EnemyId;
                }
            }

            return FallbackHuntTargetId;
        }

        public bool RegisterHuntContract(string targetEnemyId, int rewardMultiplier)
        {
            // O contrato é registrado na seção de pendências (persistido) pelo executor após este retorno;
            // aqui só publicamos o anúncio. A recompensa 2× é concedida na conclusão (OnEnemyKilled) pela
            // tabela de loot do alvo — sem reimplementar loot aqui.
            GameEventBus.Publish(new PlayerActionFeedbackEvent(
                $"Contrato de caca: abata {targetEnemyId} (recompensa {rewardMultiplier}x)."));
            return true;
        }

        public bool ApplyDailyDish()
        {
            // Prato do dia: comida grátis com buff aleatório do dia. O buff alimentar (F01/consumível) é
            // o mesmo caminho de comida do jogo; sem um item-comida dedicado wired, publicamos o feedback
            // e o limite 1×/dia é garantido pelo estado de pendências. Buff alimentar concreto: débito UI.
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Prato do dia servido: voce se sente revigorado."));
            return true;
        }

        public bool TryIdentifyRelic()
        {
            // F31: revela 1 trinket não-identificado via o MESMO serviço do scroll_identify (1:1 swap).
            var inventory = ResolveInventory();
            if (inventory == null) return false;
            if (inventory.GetAmount(MagicItemCatalog.UnidentifiedTrinketId) <= 0) return false;

            var adapter = new InventorySwapAdapter(inventory, ResolveRevealContext);
            var service = new ItemIdentificationService(adapter);
            var result = service.IdentifyItem(MagicItemCatalog.UnidentifiedTrinketId);
            return result.Success;
        }

        // ── Save round-trip (wired pelo SaveManager) ────────────────────────────────────────────────

        public NpcServicesSaveData CaptureSaveData() => _pending.Capture();

        public void RestoreFromSaveData(NpcServicesSaveData data)
        {
            _pending.Restore(data);
            Debug.Log($"[NpcServiceRuntime] Restored NPC services pending state " +
                      $"(orders={_pending.PendingBookOrderCount}, dish_day={_pending.DailyDishLastUsedDay}).", this);
        }

        public void SetCurrentDay(int day) => _currentDay = day;

        // ── Resolução de dependências (refs estáveis; sem busca global de gameplay) ──────────────────

        private static InventoryManager ResolveInventory()
        {
            var boot = GameBootstrap.Instance;
            return boot != null ? boot.InventoryManager : null;
        }

        private static PlayerManager ResolvePlayer()
        {
            var boot = GameBootstrap.Instance;
            return boot != null ? boot.PlayerManager : null;
        }

        private static BestiaryManager ResolveBestiary()
        {
            var boot = GameBootstrap.Instance;
            return boot != null ? boot.BestiaryManager : null;
        }

        private (string runSeed, int level, string salt) ResolveRevealContext()
        {
            var run = Cave.Runtime.CaveRunManager.Instance;
            if (run == null)
            {
                return (string.Empty, 1, MagicItemCatalog.UnidentifiedTrinketId);
            }

            return (run.CaveRunSeed, run.CurrentCaveLevel, MagicItemCatalog.UnidentifiedTrinketId);
        }

        private static string FindMonsterPartItemId(InventoryManager inventory)
        {
            foreach (var kvp in inventory.Items)
            {
                if (kvp.Value <= 0) continue;
                if (inventory.TryGetItemData(kvp.Key, out ItemDataSO data)
                    && data != null
                    && data.Category == ItemCategory.MonsterDrop)
                {
                    return kvp.Key;
                }
            }

            return null;
        }

        /// <summary>
        /// Resolve (criatura, categoria) a conceder: a primeira criatura ainda não totalmente documentada
        /// com uma categoria-núcleo faltante. "Mais valiosa" = a primeira faltante na ordem
        /// Identity→Behavior→ElementVulnerability→DropsCommon (núcleo que define K4).
        /// </summary>
        private static bool TryResolveAnalysisGrant(
            BestiaryManager bestiary, out string enemyId, out BestiaryKnowledgeCategory category)
        {
            enemyId = null;
            category = BestiaryKnowledgeCategory.Identity;

            var coreOrder = new[]
            {
                BestiaryKnowledgeCategory.Identity,
                BestiaryKnowledgeCategory.BehaviorSummary,
                BestiaryKnowledgeCategory.ElementVulnerability,
                BestiaryKnowledgeCategory.DropsCommon
            };

            foreach (var entry in bestiary.GetAllEntries())
            {
                if (entry == null || string.IsNullOrEmpty(entry.EnemyId)) continue;
                foreach (var cat in coreOrder)
                {
                    if (!bestiary.Knowledge.IsUnlocked(entry.EnemyId, cat))
                    {
                        enemyId = entry.EnemyId;
                        category = cat;
                        return true;
                    }
                }
            }

            return false;
        }

        // Convenção do catálogo de itens (F32) p/ livros de conhecimento; resolvidos contra o ItemDatabase.
        private static readonly string[] CandidateBookItemIds =
        {
            "item_book_knowledge_band1",
            "item_book_bestiary_lore",
            "item_consumable_scroll_identify"
        };

        private const string FallbackHuntTargetId = "enemy_ruin_warden";
    }

    /// <summary>fable_25 — garante o bridge em runtime (padrão GiftGivingRuntimeBootstrap).</summary>
    public static class NpcServiceRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (Object.FindAnyObjectByType<NpcServiceRuntime>() != null)
            {
                return;
            }

            var go = new GameObject("NpcServiceRuntime");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<NpcServiceRuntime>();
            Debug.Log("[NpcServiceRuntimeBootstrap] NpcServiceRuntime instanciado via bootstrap.");
        }
    }
}
