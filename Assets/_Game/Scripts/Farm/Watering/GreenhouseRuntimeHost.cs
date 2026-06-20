using UnityEngine;

namespace CindarsHope.Farm.Watering
{
    /// <summary>
    /// fable_55 — host de runtime que INSTANCIA o órfão <see cref="GreenhouseContextProvider"/>
    /// (contrato único de estufa — não recriar) e registra os canteiros da estufa via refs do
    /// gerador. Expõe o provider como singleton para o FarmPlot consultar override de estação e
    /// para a RainIrrigationIntegration excluir a estufa da chuva.
    ///
    /// v1: zona da estufa sempre presente → SetGreenhouseUnlocked(true) no Start (decisão Fase 0:
    /// zona pequena sempre presente, sem lote destravável). Sem estado persistido (zona fixa).
    /// Refs serializadas (ids dos canteiros) — sem GameObject.Find.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GreenhouseRuntimeHost : MonoBehaviour
    {
        [SerializeField] private string[] _greenhousePlotIds = new string[0];
        [SerializeField] private bool _unlockedByDefault = true;

        private readonly GreenhouseContextProvider _provider = new GreenhouseContextProvider();

        private static GreenhouseRuntimeHost _instance;
        public static GreenhouseRuntimeHost Instance => _instance;

        /// <summary>Provider canônico da estufa (consultado pelo FarmPlot e pela rega).</summary>
        public GreenhouseContextProvider Provider => _provider;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            RegisterConfiguredPlots();
            _provider.SetGreenhouseUnlocked(_unlockedByDefault);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>Configura os canteiros e o destravamento (chamado pelo gerador). Sem Find.</summary>
        public void Configure(string[] greenhousePlotIds, bool unlocked)
        {
            _greenhousePlotIds = greenhousePlotIds ?? new string[0];
            _unlockedByDefault = unlocked;
            RegisterConfiguredPlots();
            _provider.SetGreenhouseUnlocked(unlocked);
        }

        /// <summary>Override de estação: o canteiro da estufa aceita semente fora de estação.</summary>
        public bool CanOverrideSeason(string plotId) => _provider.CanOverrideSeason(plotId);

        /// <summary>Exclusão de chuva: a chuva não rega os canteiros da estufa.</summary>
        public bool IsRainExcluded(string plotId) => _provider.IsRainExcluded(plotId);

        private void RegisterConfiguredPlots()
        {
            if (_greenhousePlotIds == null)
                return;

            foreach (var plotId in _greenhousePlotIds)
            {
                _provider.RegisterGreenhousePlot(plotId);
            }
        }
    }
}
