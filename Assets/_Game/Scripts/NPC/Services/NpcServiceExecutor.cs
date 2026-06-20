using System;

namespace CindarsHope.NPC.Services
{
    /// <summary>fable_25 — resultado tipado da execução de um serviço (para feedback do caller).</summary>
    public enum NpcServiceOutcome
    {
        Success = 0,
        /// <summary>Gate de amizade/flag não atendido (opção estava desabilitada).</summary>
        GateClosed = 1,
        /// <summary>Ouro insuficiente para o custo.</summary>
        InsufficientGold = 2,
        /// <summary>Item exigido ausente do inventário (ex.: parte de monstro p/ análise).</summary>
        MissingItem = 3,
        /// <summary>Limite temporal atingido (prato do dia já usado; contrato da semana ativo).</summary>
        TemporallyUnavailable = 4,
        /// <summary>Sistema-alvo do efeito ausente/indisponível em runtime (efeito não aplicado).</summary>
        EffectUnavailable = 5,
        /// <summary>Pré-condição inválida (serviço nulo, dependência ausente).</summary>
        Failed = 6
    }

    public readonly struct NpcServiceResult
    {
        public readonly NpcServiceOutcome Outcome;
        public readonly string ServiceId;
        public readonly string Message;
        public bool Success => Outcome == NpcServiceOutcome.Success;

        public NpcServiceResult(NpcServiceOutcome outcome, string serviceId, string message)
        {
            Outcome = outcome;
            ServiceId = serviceId;
            Message = message ?? string.Empty;
        }
    }

    /// <summary>
    /// fable_25 — porta dos EFEITOS dos serviços. Cada método chama UM sistema-alvo já existente
    /// (F21/F16/F12/F31, hooks de reparo/pasto/contrato). Implementado pelo runtime
    /// (<see cref="NpcServiceAccess"/> bridge) e por fakes nos testes — mantém o executor puro e os
    /// efeitos NÃO reimplementados (regra de não duplicação). Retorno bool = efeito aplicado com sucesso.
    /// </summary>
    public interface INpcServiceEffects
    {
        /// <summary>Análise (F21): consome 1 parte de monstro e concede a categoria faltante mais valiosa.
        /// Retorna false se não houver parte disponível ou nada a conceder.</summary>
        bool TryCreatureAnalysis();

        /// <summary>Reparo com desconto (hook F49): aplica/arma o desconto de −30% para o próximo reparo.</summary>
        bool ApplyRepairDiscount(float discountFraction);

        /// <summary>Banho termal (F16/F01): remove a fadiga acumulada e aplica o buff Rested.</summary>
        bool ApplyThermalBath();

        /// <summary>Pasto premium (F12): alimenta os animais automaticamente por N dias.</summary>
        bool ApplyPremiumPasture(int days);

        /// <summary>Resolve o id do livro de conhecimento da encomenda (banda à escolha). null = sem livro.</summary>
        string ResolveOrderedBookItemId();

        /// <summary>Resolve o alvo elite da banda atual p/ o contrato de caça. null = sem alvo.</summary>
        string ResolveHuntContractTarget();

        /// <summary>Registra o contrato de caça (quest dinâmica via board F34 OU flag). Retorna false se nem flag deu.</summary>
        bool RegisterHuntContract(string targetEnemyId, int rewardMultiplier);

        /// <summary>Prato do dia: aplica a comida com buff aleatório do dia. Retorna false se indisponível.</summary>
        bool ApplyDailyDish();

        /// <summary>Identificação (F31): revela 1 item unidentified_* do inventário. false se não houver.</summary>
        bool TryIdentifyRelic();
    }

    /// <summary>
    /// fable_25 — adaptadores de gates/custos do executor (mockáveis). Amizade via F26; flag via
    /// QuestFlagService; ouro/itens via Player/Inventory. Defaults fail-closed quando não ligados.
    /// </summary>
    public interface INpcServiceContext
    {
        bool FriendshipAtLeast(string npcId, int level);
        bool FlagIsSet(string flagId);
        int CurrentGold();
        bool SpendGold(int amount);
        bool HasItem(string itemId, int amount);
        bool RemoveItem(string itemId, int amount);
    }

    /// <summary>
    /// fable_25 — executor PURO: valida gates (amizade/flag) → valida limites temporais → valida/consome
    /// custos (ouro/item) → despacha o efeito pela interface do sistema-alvo. Acoplamento controlado:
    /// nada é chamado diretamente; tudo via <see cref="INpcServiceContext"/> /
    /// <see cref="INpcServiceEffects"/> (mockáveis). NÃO reimplementa nenhum efeito.
    /// </summary>
    public sealed class NpcServiceExecutor
    {
        private readonly NpcServicesPendingState _pending;
        private readonly INpcServiceContext _context;
        private readonly INpcServiceEffects _effects;

        /// <summary>Provedor do dia/semana ABSOLUTOS do calendário (injetado; default dia 1).</summary>
        public Func<int> CurrentDayProvider { get; set; } = () => 1;

        public NpcServiceExecutor(
            NpcServicesPendingState pending,
            INpcServiceContext context,
            INpcServiceEffects effects)
        {
            _pending = pending ?? new NpcServicesPendingState();
            _context = context;
            _effects = effects;
        }

        private int CurrentDay()
        {
            try { return CurrentDayProvider != null ? CurrentDayProvider() : 1; }
            catch { return 1; }
        }

        private int CurrentWeek() => CurrentDay() / 7;

        /// <summary>Avalia a opção (gates estáticos) p/ o Conversar — delega ao gate evaluator.</summary>
        public NpcServiceOptionModel BuildOption(NpcServiceDefinition def)
        {
            return NpcServiceGateEvaluator.Evaluate(
                def,
                _context != null ? (Func<string, int, bool>)_context.FriendshipAtLeast : null,
                _context != null ? (Func<string, bool>)_context.FlagIsSet : null);
        }

        /// <summary>
        /// Executa o serviço <paramref name="serviceId"/>: revalida gate → limite temporal → custo →
        /// efeito. Idempotência herdada do sistema-alvo (ex.: GrantKnowledge da F21). Não consome custo
        /// se o gate/efeito falhar.
        /// </summary>
        public NpcServiceResult TryExecute(string serviceId)
        {
            var def = NpcServiceCatalog.GetById(serviceId);
            if (def == null || _context == null || _effects == null)
            {
                return new NpcServiceResult(NpcServiceOutcome.Failed, serviceId, "Servico indisponivel.");
            }

            // 1) Gate estático (amizade/flag). Revalida no servidor mesmo que a UI já tenha desabilitado.
            var option = BuildOption(def);
            if (!option.Enabled)
            {
                return new NpcServiceResult(NpcServiceOutcome.GateClosed, serviceId, option.DisabledReason);
            }

            // 2) Limites temporais por efeito (prato do dia / contrato da semana). Não cobram custo.
            int day = CurrentDay();
            if (def.Effect == NpcServiceEffectType.DailyDish && !_pending.CanUseDailyDish(day))
            {
                return new NpcServiceResult(NpcServiceOutcome.TemporallyUnavailable, serviceId,
                    "O prato do dia ja foi servido hoje.");
            }

            if (def.Effect == NpcServiceEffectType.HuntContract && !_pending.CanIssueHuntContract(CurrentWeek()))
            {
                return new NpcServiceResult(NpcServiceOutcome.TemporallyUnavailable, serviceId,
                    "Ja ha um contrato de caca ativo nesta semana.");
            }

            // 3) Pré-validação de custo (sem consumir ainda): ouro + item exigido.
            if (def.GoldCost > 0 && _context.CurrentGold() < def.GoldCost)
            {
                return new NpcServiceResult(NpcServiceOutcome.InsufficientGold, serviceId,
                    $"Ouro insuficiente ({def.GoldCost}g).");
            }

            if (def.HasItemCost && !_context.HasItem(def.RequiredItemId, def.RequiredItemAmount))
            {
                return new NpcServiceResult(NpcServiceOutcome.MissingItem, serviceId,
                    "Item necessario ausente.");
            }

            // 4) Despacho do efeito (pode falhar por indisponibilidade do alvo — NÃO cobra nesse caso).
            return Dispatch(def, day);
        }

        private NpcServiceResult Dispatch(NpcServiceDefinition def, int day)
        {
            switch (def.Effect)
            {
                case NpcServiceEffectType.CreatureAnalysis:
                    // O efeito F21 resolve a parte do monstro a consumir + a categoria faltante. Se nada
                    // houver a conceder/consumir, não cobra (idempotente). Custo de ouro só após sucesso.
                    if (!_effects.TryCreatureAnalysis())
                    {
                        return new NpcServiceResult(NpcServiceOutcome.MissingItem, def.ServiceId,
                            "Sem parte de monstro ou nada novo a analisar.");
                    }
                    return Charge(def, "Criatura analisada. Conhecimento revelado.");

                case NpcServiceEffectType.RepairDiscount:
                    if (!_effects.ApplyRepairDiscount(NpcServiceCatalog.RepairDiscountFraction))
                        return EffectUnavailable(def);
                    // Sem custo de ouro: o desconto se aplica ao reparo futuro (hook F49).
                    return Charge(def, "Desconto de reparo (-30%) garantido.");

                case NpcServiceEffectType.BookOrder:
                {
                    var bookId = _effects.ResolveOrderedBookItemId();
                    if (string.IsNullOrEmpty(bookId))
                        return EffectUnavailable(def);
                    var result = Charge(def, $"Livro encomendado. Chega no dia {day + NpcServiceCatalog.BookOrderDeliveryDays}.");
                    if (result.Success)
                        _pending.AddBookOrder(bookId, day + NpcServiceCatalog.BookOrderDeliveryDays);
                    return result;
                }

                case NpcServiceEffectType.ThermalBath:
                    if (!_effects.ApplyThermalBath())
                        return EffectUnavailable(def);
                    return Charge(def, "Banho termal: fadiga removida, descanso recuperado.");

                case NpcServiceEffectType.PremiumPasture:
                    // Alimenta hoje (efeito imediato F12); o host re-alimenta nos próximos dias enquanto
                    // o pasto estiver ativo (estado persistido nesta seção; consome DayStartedEvent).
                    if (!_effects.ApplyPremiumPasture(NpcServiceCatalog.PremiumPastureDays))
                        return EffectUnavailable(def);
                    _pending.ActivatePremiumPasture(day, NpcServiceCatalog.PremiumPastureDays);
                    return Charge(def, $"Pasto premium: animais alimentados por {NpcServiceCatalog.PremiumPastureDays} dias.");

                case NpcServiceEffectType.HuntContract:
                {
                    var target = _effects.ResolveHuntContractTarget();
                    if (string.IsNullOrEmpty(target))
                        return EffectUnavailable(def);
                    if (!_effects.RegisterHuntContract(target, NpcServiceCatalog.HuntContractRewardMultiplier))
                        return EffectUnavailable(def);
                    var result = Charge(def, "Contrato de caca emitido (recompensa 2x).");
                    if (result.Success)
                        _pending.IssueHuntContract(target, CurrentWeek());
                    return result;
                }

                case NpcServiceEffectType.DailyDish:
                    if (!_effects.ApplyDailyDish())
                        return EffectUnavailable(def);
                    _pending.MarkDailyDishUsed(day);
                    return Charge(def, "Prato do dia servido. Buff aplicado.");

                case NpcServiceEffectType.RelicIdentification:
                    if (!_effects.TryIdentifyRelic())
                        return new NpcServiceResult(NpcServiceOutcome.MissingItem, def.ServiceId,
                            "Nenhuma reliquia nao identificada no inventario.");
                    return Charge(def, "Reliquia identificada.");

                default:
                    return new NpcServiceResult(NpcServiceOutcome.Failed, def.ServiceId, "Efeito desconhecido.");
            }
        }

        /// <summary>Consome o custo (ouro + item) APÓS o efeito ter sucesso. Falha de débito = Failed.</summary>
        private NpcServiceResult Charge(NpcServiceDefinition def, string successMessage)
        {
            if (def.HasItemCost && !_context.RemoveItem(def.RequiredItemId, def.RequiredItemAmount))
            {
                return new NpcServiceResult(NpcServiceOutcome.MissingItem, def.ServiceId, "Item necessario ausente.");
            }

            if (def.GoldCost > 0 && !_context.SpendGold(def.GoldCost))
            {
                return new NpcServiceResult(NpcServiceOutcome.InsufficientGold, def.ServiceId,
                    $"Ouro insuficiente ({def.GoldCost}g).");
            }

            return new NpcServiceResult(NpcServiceOutcome.Success, def.ServiceId, successMessage);
        }

        private static NpcServiceResult EffectUnavailable(NpcServiceDefinition def)
        {
            return new NpcServiceResult(NpcServiceOutcome.EffectUnavailable, def.ServiceId,
                "Servico temporariamente indisponivel.");
        }

        // ── Hooks de evento de runtime (consumidos pelo host) ──────────────────────────────────────

        /// <summary>Avanço de dia: entrega encomendas vencidas. Retorna os ids de livro a adicionar.</summary>
        public System.Collections.Generic.List<string> OnDayStarted(int dayNumber)
        {
            CurrentDayProvider = () => dayNumber;
            return _pending.CollectDueBookOrders(dayNumber);
        }

        /// <summary>True se o pasto premium cobre o dia atual (host deve re-alimentar os animais).</summary>
        public bool IsPremiumPastureActive(int dayNumber) => _pending.IsPremiumPastureActive(dayNumber);

        /// <summary>Inimigo abatido: tenta concluir o contrato de caça. Retorna true se concluiu agora.</summary>
        public bool OnEnemyKilled(string enemyId) => _pending.TryCompleteHuntContract(enemyId);
    }
}
