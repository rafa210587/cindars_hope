using UnityEngine;
using UnityEngine.SceneManagement;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Save;
using CindarsHope.UI.SystemTab;
using CindarsHope.World.Scenes;

namespace CindarsHope.UI.Title
{
    /// <summary>
    /// fable_56: adapter MonoBehaviour FINO da tela de titulo minima (padrao F14). A logica
    /// (Continue condicional, recuperacao de backup) vive em <see cref="TitleFlowViewModel"/>.
    /// Este controller resolve dependencias via GameBootstrap (sem GameObject.Find): New Game
    /// chama <see cref="NewGameStateResetService.ResetAll"/> (ponto unico) e carrega a cena
    /// inicial SEM deletar slot_1.json (CA-3); Continue usa o mesmo caminho de load da aba
    /// (SaveManager.LoadGame, CA-4); Sair fecha o jogo.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TitleScreenController : MonoBehaviour
    {
        [SerializeField] private string _initialSceneName = SceneId.Farm;

        private TitleFlowViewModel _viewModel;
        private SaveFileProbe _probe;
        private SaveManager _saveManager;
        private readonly NewGameStateResetService _resetService = new NewGameStateResetService();

        public TitleFlowViewModel ViewModel => _viewModel;
        public NewGameStateResetService ResetService => _resetService;

        private void Awake()
        {
            SystemTabController.EnsureSettingsBound();
            var bootstrap = GameBootstrap.Instance;
            _saveManager = bootstrap != null ? bootstrap.SaveManager : null;
            _probe = new SaveFileProbe(_saveManager);
            _viewModel = new TitleFlowViewModel(_probe);
        }

        /// <summary>Registra uma superficie de reset de estado (chamado pelo wiring de runtime).</summary>
        public void RegisterResetSurface(IResettableGameState surface) => _resetService.Register(surface);

        /// <summary>
        /// New Game: limpa o estado em memoria (ponto unico) e entra na cena inicial. NUNCA
        /// deleta o arquivo de save anterior — ele so e sobrescrito no primeiro Salvar.
        /// </summary>
        public void StartNewGame()
        {
            _resetService.ResetAll();
            LoadInitialScene();
        }

        /// <summary>Continue: mesmo caminho de load da aba Sistema (CA-4). So habilitado com save.</summary>
        public SystemLoadOutcome Continue()
        {
            if (_viewModel == null || !_viewModel.CanContinue)
            {
                return SystemLoadOutcome.NoOp;
            }

            var loadOk = _saveManager != null && _saveManager.LoadGame();
            _probe.LastLoadFailed = !loadOk;
            return _viewModel.ResolveContinue();
        }

        /// <summary>Sair para o titulo (de dentro do jogo): descarta estado via o mesmo ResetAll.</summary>
        public void DiscardToTitle()
        {
            _resetService.ResetAll();
        }

        /// <summary>Sair do jogo (Application.Quit; stop no editor).</summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void LoadInitialScene()
        {
            if (string.IsNullOrEmpty(_initialSceneName))
            {
                _initialSceneName = SceneId.Farm;
            }

            SceneManager.LoadScene(_initialSceneName);
        }
    }
}
