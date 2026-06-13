using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.MainProgression;
using UnityEngine;

namespace CindarsHope.Fonte
{
    /// <summary>
    /// Host runtime do estado da Fonte de Anya (FonteAnyaSection WAVE 10, antes órfão) e da
    /// MainProgressionSection (fragmentos). Recarga de Água Viva no DayStarted; concessão
    /// 1×/dia gated pelo FonteFunctionUnlockService. F10 chamará IntegrateFragment.
    /// </summary>
    [DisallowMultipleComponent]
    public class FonteRuntimeService : MonoBehaviour
    {
        public const string LivingWaterItemId = "item_consumable_agua_viva";

        private static FonteRuntimeService _instance;

        private readonly FonteFunctionUnlockService _unlockService = new FonteFunctionUnlockService();
        private readonly MainProgressionService _progressionService = new MainProgressionService();
        private FonteAnyaSection _section = new FonteAnyaSection();
        private MainProgressionSection _progression = new MainProgressionSection();
        private int _lastGrantDay = -1;
        private int _currentDay = 1;

        public static FonteRuntimeService Instance => _instance;

        public FonteAnyaSection Section => _section;
        public MainProgressionSection Progression => _progression;
        public int LastGrantDay => _lastGrantDay;
        public int CurrentDay => _currentDay;

        public static bool CanGrantToday(int lastGrantDay, int today)
        {
            return lastGrantDay != today;
        }

        /// <summary>Integra um fragmento (chamado por reward de quest — F10) e destrava funções.</summary>
        public bool IntegrateFragment(MainFragmentType fragmentType)
        {
            var result = _progressionService.TryIntegrateFragment(_progression, fragmentType, _currentDay, false);
            if (!result.Success)
            {
                Debug.LogWarning($"FonteRuntimeService: fragmento {fragmentType} recusado ({result.FailureReason}).");
                return false;
            }

            RefreshUnlocks();
            return true;
        }

        /// <summary>Reavalia destravas a partir dos fragmentos integrados (honesto — service WAVE 10 decide).</summary>
        public void RefreshUnlocks()
        {
            _unlockService.TryUnlock(_section, FonteFunction.ReturnPoint, _progression);

            var water = _unlockService.TryUnlock(_section, FonteFunction.LimitedLivingWater, _progression);
            if (water.Success && !_section.LivingWater.Unlocked)
            {
                _section.LivingWater.Unlocked = true;
                _section.LivingWater.CurrentCharges = _section.LivingWater.MaxCharges;
                _section.LivingWater.InventoryItemId = LivingWaterItemId;
            }

            var respec = _unlockService.TryUnlock(_section, FonteFunction.Respec, _progression);
            if (respec.Success)
            {
                _section.Respec.Unlocked = true;
            }
        }

        /// <summary>Coleta 1 frasco de Água Viva (1×/dia, consome carga). Idempotente por dia.</summary>
        public FonteUseResult TryCollectLivingWater()
        {
            if (!CanGrantToday(_lastGrantDay, _currentDay))
            {
                return FonteUseResult.Fail("ALREADY_GRANTED_TODAY");
            }

            var result = _unlockService.EvaluateUseRequest(_section, new FonteUseRequest
            {
                RequestedFunction = FonteFunction.LimitedLivingWater,
                CurrentDay = _currentDay,
                AvailableLivingWaterCharges = _section.LivingWater.CurrentCharges
            });

            if (!result.Success)
            {
                return result;
            }

            var inventory = GameBootstrap.Instance != null ? GameBootstrap.Instance.InventoryManager : null;
            if (inventory == null || !inventory.AddItem(LivingWaterItemId, 1))
            {
                return FonteUseResult.Fail("INVENTORY_UNAVAILABLE_OR_FULL");
            }

            _section.LivingWater.CurrentCharges -= result.LivingWaterChargesConsumed;
            _lastGrantDay = _currentDay;
            _section.LastUseRecords.Add(new FonteUseRecord
            {
                Function = FonteFunction.LimitedLivingWater,
                UsedAtDay = _currentDay
            });

            GameEventBus.Publish(new PlayerActionFeedbackEvent("Voce recolheu um frasco de Agua Viva."));
            return result;
        }

        public void RestoreFromSave(int fonteState, System.Collections.Generic.List<int> unlockedFunctions,
            bool livingWaterUnlocked, int livingWaterCharges, int lastGrantDay, System.Collections.Generic.List<int> integratedFragments)
        {
            _section = new FonteAnyaSection { FonteState = (FonteState)fonteState };
            if (unlockedFunctions != null)
            {
                foreach (var fn in unlockedFunctions)
                {
                    _section.UnlockedFunctions.Add((FonteFunction)fn);
                }
            }

            _section.LivingWater.Unlocked = livingWaterUnlocked;
            _section.LivingWater.CurrentCharges = Mathf.Max(0, livingWaterCharges);
            _section.LivingWater.InventoryItemId = LivingWaterItemId;
            _lastGrantDay = lastGrantDay;

            _progression = new MainProgressionSection();
            if (integratedFragments != null)
            {
                foreach (var fragment in integratedFragments)
                {
                    _progression.FragmentStates.Add(new FragmentStateRecord
                    {
                        FragmentType = (MainFragmentType)fragment,
                        AcquisitionState = FragmentAcquisitionState.Integrated
                    });
                }
            }

            RefreshUnlocks();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            RefreshUnlocks();
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

        private void OnDayStarted(DayStartedEvent evt)
        {
            _currentDay = evt.DayNumber;

            // Recarga: +1 carga por dia até o máximo (política limitada canônica).
            if (_section.LivingWater.Unlocked && _section.LivingWater.CurrentCharges < _section.LivingWater.MaxCharges)
            {
                _section.LivingWater.CurrentCharges++;
                _section.LivingWater.LastRechargeDay = _currentDay;
            }
        }
    }

    /// <summary>Garante o host da Fonte em runtime (padrão bootstrap do projeto).</summary>
    public static class FonteRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (Object.FindAnyObjectByType<FonteRuntimeService>() != null)
            {
                return;
            }

            var go = new GameObject("FonteRuntimeService");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<FonteRuntimeService>();
            Debug.Log("[FonteRuntimeBootstrap] FonteRuntimeService instanciado via bootstrap.");
        }
    }
}
