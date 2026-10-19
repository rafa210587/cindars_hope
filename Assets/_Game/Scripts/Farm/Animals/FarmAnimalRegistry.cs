using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Farm.Animals
{
    /// <summary>
    /// fable_12 — serviço central de animais de fazenda. Faz o spawn/track por abrigo, consome
    /// <see cref="DayStartedEvent"/> (reset fed → produz se alimentado ontem), aplica MORTE PERMANENTE
    /// por negligência (EMENDA 5.1-A, N=7), e captura/restaura a seção de save.
    ///
    /// REUSO (sem sistema paralelo): capacidade = <see cref="AnimalHousingCapacityState"/>; estado por
    /// animal = <see cref="AnimalInstanceState"/>; semântica de fome/saúde = a MESMA do
    /// AnimalDailyProcessor (Hungry@1, Unavailable@3) estendida só com Dead@N. Produto/qualidade base
    /// permanecem em AnimalProductDefinition/Resolver; a qualidade por dias consecutivos de
    /// alimentação (§18) é resolvida via <see cref="FarmAnimalCatalog.ResolveProductItemId"/>.
    ///
    /// Comunicação só via GameEventBus. Sem GameObject.Find/FindObjectOfType (registro de runtimes é
    /// feito por chamada explícita de <see cref="RegisterRuntime"/> a partir do próprio animal).
    /// </summary>
    [DisallowMultipleComponent]
    public class FarmAnimalRegistry : MonoBehaviour
    {
        public enum ReleaseResult
        {
            Success = 0,
            HousingNotFound = 1,
            HousingFull = 2,
            UnknownAnimal = 3
        }

        public enum FeedResult
        {
            Success = 0,
            AnimalNotFound = 1,
            AnimalDead = 2,
            AlreadyFed = 3
        }

        public enum CollectResult
        {
            Success = 0,
            AnimalNotFound = 1,
            AnimalDead = 2,
            ProductNotReady = 3
        }

        private static FarmAnimalRegistry _instance;
        public static FarmAnimalRegistry Instance => _instance;

        private readonly Dictionary<string, AnimalDataSO> _definitions = new Dictionary<string, AnimalDataSO>();
        private readonly Dictionary<string, AnimalInstanceState> _animals = new Dictionary<string, AnimalInstanceState>();
        private readonly Dictionary<string, AnimalHousingCapacityState> _housings = new Dictionary<string, AnimalHousingCapacityState>();

        // Campos de domínio que pertencem a esta camada de integração (não ao state puro WAVE 05).
        private readonly Dictionary<string, int> _consecutiveFedDays = new Dictionary<string, int>();
        private readonly Dictionary<string, FarmAnimalRuntime> _runtimes = new Dictionary<string, FarmAnimalRuntime>();
        private readonly Dictionary<string, int> _instanceCounterByHousing = new Dictionary<string, int>();

        private int _currentDay = 1;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            RegisterCanonicalDefinitions();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void RegisterCanonicalDefinitions()
        {
            // Definições canônicas existem mesmo sem assets (testes/headless). Se houver AnimalDataSO
            // em cena/bootstrap, RegisterDefinition os adiciona/atualiza por cima.
        }

        public void RegisterDefinition(AnimalDataSO definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.AnimalId))
            {
                return;
            }
            _definitions[definition.AnimalId] = definition;
        }

        /// <summary>Registra/atualiza um abrigo (Coop/Barn) e sua capacidade.</summary>
        public void RegisterHousing(string housingId, AnimalHousingBuildingType type, int capacity)
        {
            if (string.IsNullOrWhiteSpace(housingId))
            {
                return;
            }

            if (!_housings.ContainsKey(housingId))
            {
                _housings[housingId] = new AnimalHousingCapacityState(housingId, type, Mathf.Max(1, capacity));
            }
        }

        /// <summary>O animal-runtime se registra para receber notificações de estado (sem Find).</summary>
        public void RegisterRuntime(string animalInstanceId, FarmAnimalRuntime runtime)
        {
            if (string.IsNullOrWhiteSpace(animalInstanceId) || runtime == null)
            {
                return;
            }
            _runtimes[animalInstanceId] = runtime;
        }

        public void UnregisterRuntime(string animalInstanceId)
        {
            if (!string.IsNullOrWhiteSpace(animalInstanceId))
            {
                _runtimes.Remove(animalInstanceId);
            }
        }

        public AnimalHousingCapacityState GetHousing(string housingId)
        {
            if (string.IsNullOrWhiteSpace(housingId))
            {
                return null;
            }
            _housings.TryGetValue(housingId, out var housing);
            return housing;
        }

        public AnimalInstanceState GetAnimal(string animalInstanceId)
        {
            if (string.IsNullOrWhiteSpace(animalInstanceId))
            {
                return null;
            }
            _animals.TryGetValue(animalInstanceId, out var animal);
            return animal;
        }

        /// <summary>
        /// fable_25 (hook pontual do Pasto Premium do Eiran): alimenta TODOS os animais vivos ainda não
        /// alimentados hoje, reutilizando a regra de <see cref="Feed"/> (sem duplicar a lógica de fome).
        /// Retorna quantos foram efetivamente alimentados nesta chamada. Animais mortos e já alimentados
        /// são ignorados. Não consome ração de inventário (o pasto é o serviço pago do NPC).
        /// </summary>
        public int FeedAllLiveAnimals()
        {
            int fed = 0;
            foreach (var animal in _animals.Values)
            {
                if (animal == null || animal.HealthState == AnimalHealthState.Dead || animal.FedToday)
                {
                    continue;
                }

                if (Feed(animal.AnimalInstanceId) == FeedResult.Success)
                {
                    fed++;
                }
            }

            return fed;
        }

        public int GetConsecutiveFedDays(string animalInstanceId)
        {
            if (!string.IsNullOrWhiteSpace(animalInstanceId) && _consecutiveFedDays.TryGetValue(animalInstanceId, out var days))
            {
                return days;
            }
            return 0;
        }

        public IReadOnlyList<AnimalInstanceState> GetByHousing(string housingId)
        {
            var list = new List<AnimalInstanceState>();
            if (string.IsNullOrWhiteSpace(housingId) || !_housings.TryGetValue(housingId, out var housing))
            {
                return list;
            }

            foreach (var id in housing.AnimalInstanceIds)
            {
                if (_animals.TryGetValue(id, out var animal))
                {
                    list.Add(animal);
                }
            }
            return list;
        }

        /// <summary>
        /// Gera o próximo AnimalInstanceId determinístico para um abrigo: animal_&lt;housing&gt;_&lt;index&gt;.
        /// </summary>
        public string BuildNextInstanceId(string housingId, string animalDataId)
        {
            _instanceCounterByHousing.TryGetValue(housingId, out var count);
            string candidate;
            do
            {
                candidate = $"animal_{housingId}_{count}";
                count++;
            }
            while (_animals.ContainsKey(candidate));

            _instanceCounterByHousing[housingId] = count;
            return candidate;
        }

        /// <summary>
        /// Solta um filhote no abrigo: valida espécie/abrigo, capacidade e registra o animal vivo.
        /// Retorna o id da nova instância em <paramref name="newInstanceId"/> quando bem-sucedido.
        /// </summary>
        public ReleaseResult TryRelease(string animalDataId, string housingId, out string newInstanceId)
        {
            newInstanceId = null;

            if (string.IsNullOrWhiteSpace(animalDataId) || !_definitions.TryGetValue(animalDataId, out var def))
            {
                return ReleaseResult.UnknownAnimal;
            }

            if (string.IsNullOrWhiteSpace(housingId) || !_housings.TryGetValue(housingId, out var housing))
            {
                return ReleaseResult.HousingNotFound;
            }

            if (def.HousingType != housing.BuildingType)
            {
                // Abrigo errado para a espécie (ex.: vaca no galinheiro) — tratado como abrigo inválido.
                return ReleaseResult.HousingNotFound;
            }

            if (!housing.CanAddAnimal())
            {
                return ReleaseResult.HousingFull;
            }

            var instanceId = BuildNextInstanceId(housingId, animalDataId);
            var state = new AnimalInstanceState(instanceId, animalDataId, def.DisplayName, housingId);
            _animals[instanceId] = state;
            _consecutiveFedDays[instanceId] = 0;
            housing.AddAnimal(instanceId);

            newInstanceId = instanceId;
            return ReleaseResult.Success;
        }

        /// <summary>Alimenta um animal (consome ração externamente). Marca FedToday e cura fome.</summary>
        public FeedResult Feed(string animalInstanceId)
        {
            if (!_animals.TryGetValue(animalInstanceId, out var animal))
            {
                return FeedResult.AnimalNotFound;
            }

            if (animal.HealthState == AnimalHealthState.Dead)
            {
                return FeedResult.AnimalDead;
            }

            if (animal.FedToday)
            {
                return FeedResult.AlreadyFed;
            }

            animal.MarkFed(_currentDay);
            // Recuperação leve: animal "doente/crítico" (Unavailable) volta a ficar disponível ao ser
            // alimentado — mas a morte (Dead) é terminal e nunca chega aqui.
            if (animal.HealthState == AnimalHealthState.Unavailable)
            {
                animal.HealthState = AnimalHealthState.Recovering;
            }
            return FeedResult.Success;
        }

        /// <summary>
        /// Coleta o produto pronto de um animal: resolve o item de qualidade (§18) e retorna em
        /// <paramref name="productItemId"/> / <paramref name="quantity"/>. Idempotente: ProducedToday
        /// é consumido para impedir dupla coleta no mesmo dia.
        /// </summary>
        public CollectResult Collect(string animalInstanceId, out string productItemId, out int quantity)
        {
            productItemId = null;
            quantity = 0;

            if (!_animals.TryGetValue(animalInstanceId, out var animal))
            {
                return CollectResult.AnimalNotFound;
            }

            if (animal.HealthState == AnimalHealthState.Dead)
            {
                return CollectResult.AnimalDead;
            }

            if (!animal.ProductReady)
            {
                return CollectResult.ProductNotReady;
            }

            if (!_definitions.TryGetValue(animal.AnimalDataId, out var def))
            {
                return CollectResult.ProductNotReady;
            }

            int consecutive = GetConsecutiveFedDays(animalInstanceId);
            productItemId = FarmAnimalCatalog.ResolveProductItemId(def.ProductItemId, consecutive);
            quantity = def.ProductQuantity;

            // Idempotência: consumir prontidão e registrar o dia de produção.
            animal.ProductReady = false;
            animal.LastProductDay = _currentDay;

            GameEventBus.Publish(new AnimalProductCollectedEvent(
                animalInstanceId, animal.AnimalDataId, productItemId, quantity));

            return CollectResult.Success;
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            ProcessDayTransition(evt.DayNumber);
        }

        /// <summary>
        /// Avança um dia para todos os animais: animal alimentado ontem fica com produto pronto;
        /// animal não alimentado escala fome e, em N dias consecutivos, MORRE permanentemente
        /// (EMENDA 5.1-A). Publica avisos progressivos de saúde e o evento de morte.
        /// </summary>
        public void ProcessDayTransition(int dayNumber)
        {
            _currentDay = dayNumber;

            foreach (var kvp in _animals)
            {
                var animal = kvp.Value;
                if (animal.HealthState == AnimalHealthState.Dead)
                {
                    continue;
                }

                if (!_definitions.TryGetValue(animal.AnimalDataId, out var def))
                {
                    continue;
                }

                var previousHealth = animal.HealthState;
                bool fedYesterday = animal.FedToday;

                if (fedYesterday)
                {
                    _consecutiveFedDays.TryGetValue(animal.AnimalInstanceId, out var streak);
                    _consecutiveFedDays[animal.AnimalInstanceId] = streak + 1;
                    animal.DaysWithoutFood = 0;
                    if (animal.HealthState == AnimalHealthState.Hungry || animal.HealthState == AnimalHealthState.Recovering)
                    {
                        animal.HealthState = AnimalHealthState.Healthy;
                    }

                    // Produto pronto se cadência satisfeita.
                    int daysSinceProduct = animal.LastProductDay >= 0
                        ? dayNumber - animal.LastProductDay
                        : def.ProductIntervalDays;
                    animal.ProductReady = animal.HealthState == AnimalHealthState.Healthy
                        && daysSinceProduct >= def.ProductIntervalDays;
                }
                else
                {
                    _consecutiveFedDays[animal.AnimalInstanceId] = 0;
                    animal.DaysWithoutFood++;
                    animal.ProductReady = false;

                    if (animal.DaysWithoutFood >= def.NeglectDeathDays)
                    {
                        animal.HealthState = AnimalHealthState.Dead;
                        RemoveFromHousing(animal);
                        NotifyRuntimeHealth(animal);
                        GameEventBus.Publish(new AnimalDiedEvent(
                            animal.AnimalInstanceId, animal.AnimalDataId, animal.HomeBuildingId));
                        GameEventBus.Publish(new PlayerActionFeedbackEvent(
                            $"{def.DisplayName} morreu de fome.", 3f));
                        continue;
                    }

                    if (animal.DaysWithoutFood >= 3)
                    {
                        animal.HealthState = AnimalHealthState.Unavailable; // crítico/doente
                    }
                    else
                    {
                        animal.HealthState = AnimalHealthState.Hungry;
                    }
                }

                // Resetar o estado de alimentação para o novo dia.
                animal.FedToday = false;

                if (animal.HealthState != previousHealth)
                {
                    NotifyRuntimeHealth(animal);
                    GameEventBus.Publish(new AnimalHealthChangedEvent(
                        animal.AnimalInstanceId, animal.AnimalDataId, (int)animal.HealthState, animal.DaysWithoutFood));

                    if (animal.HealthState == AnimalHealthState.Unavailable)
                    {
                        GameEventBus.Publish(new PlayerActionFeedbackEvent(
                            $"{def.DisplayName} esta doente por falta de cuidado!", 3f));
                    }
                }
            }
        }

        private void RemoveFromHousing(AnimalInstanceState animal)
        {
            if (animal != null && _housings.TryGetValue(animal.HomeBuildingId, out var housing))
            {
                housing.RemoveAnimal(animal.AnimalInstanceId);
            }
        }

        private void NotifyRuntimeHealth(AnimalInstanceState animal)
        {
            if (animal != null && _runtimes.TryGetValue(animal.AnimalInstanceId, out var runtime) && runtime != null)
            {
                runtime.OnHealthStateChanged(animal.HealthState);
            }
        }

        // ───────────────────────────── Save / Load ─────────────────────────────

        public FarmAnimalsSaveData CaptureSaveData()
        {
            var data = new FarmAnimalsSaveData();
            foreach (var kvp in _animals)
            {
                var animal = kvp.Value;
                data.Animals.Add(new FarmAnimalRecord
                {
                    AnimalInstanceId = animal.AnimalInstanceId,
                    AnimalDataId = animal.AnimalDataId,
                    HousingId = animal.HomeBuildingId,
                    FedToday = animal.FedToday,
                    ProducedToday = !animal.ProductReady && animal.LastProductDay == _currentDay,
                    DaysOwned = 0,
                    DaysWithoutFood = animal.DaysWithoutFood,
                    ConsecutiveFedDays = GetConsecutiveFedDays(animal.AnimalInstanceId),
                    HealthState = (int)animal.HealthState,
                    LastProductDay = animal.LastProductDay
                });
            }
            return data;
        }

        /// <summary>
        /// Restaura a seção. <paramref name="saveData"/> nulo (save legado) = mantém zero animais.
        /// Reconstrói instâncias, housing membership e o contador determinístico de IDs.
        /// </summary>
        public void RestoreFromSaveData(FarmAnimalsSaveData saveData)
        {
            _animals.Clear();
            _consecutiveFedDays.Clear();
            _instanceCounterByHousing.Clear();
            foreach (var housing in _housings.Values)
            {
                housing.AnimalInstanceIds.Clear();
            }

            if (saveData == null || saveData.Animals == null)
            {
                return;
            }

            foreach (var record in saveData.Animals)
            {
                if (record == null || string.IsNullOrWhiteSpace(record.AnimalInstanceId))
                {
                    continue;
                }

                string displayName = _definitions.TryGetValue(record.AnimalDataId, out var def)
                    ? def.DisplayName
                    : record.AnimalDataId;

                var state = new AnimalInstanceState(
                    record.AnimalInstanceId, record.AnimalDataId, displayName, record.HousingId)
                {
                    FedToday = record.FedToday,
                    DaysWithoutFood = record.DaysWithoutFood,
                    HealthState = (AnimalHealthState)record.HealthState,
                    LastProductDay = record.LastProductDay,
                    // ProductReady é recomputado no próximo DayStarted; restauramos como false para
                    // evitar coleta fantasma logo após o load (idempotência de produto).
                    ProductReady = false
                };

                _animals[record.AnimalInstanceId] = state;
                _consecutiveFedDays[record.AnimalInstanceId] = record.ConsecutiveFedDays;

                if (state.HealthState != AnimalHealthState.Dead
                    && _housings.TryGetValue(record.HousingId, out var housing)
                    && !housing.AnimalInstanceIds.Contains(record.AnimalInstanceId))
                {
                    housing.AnimalInstanceIds.Add(record.AnimalInstanceId);
                }

                // Manter o contador de IDs à frente do índice já usado para preservar unicidade.
                int parsedIndex = ParseInstanceIndex(record.AnimalInstanceId, record.HousingId);
                if (parsedIndex >= 0)
                {
                    _instanceCounterByHousing.TryGetValue(record.HousingId, out var current);
                    if (parsedIndex + 1 > current)
                    {
                        _instanceCounterByHousing[record.HousingId] = parsedIndex + 1;
                    }
                }
            }
        }

        private static int ParseInstanceIndex(string instanceId, string housingId)
        {
            string prefix = $"animal_{housingId}_";
            if (!string.IsNullOrEmpty(instanceId) && instanceId.StartsWith(prefix))
            {
                string suffix = instanceId.Substring(prefix.Length);
                if (int.TryParse(suffix, out var index))
                {
                    return index;
                }
            }
            return -1;
        }
    }
}
