using CindarsHope.Combat;
using CindarsHope.Core.Time;
using CindarsHope.Gameplay.Input;
using CindarsHope.Skills.Runtime.Effects;
using CindarsHope.UI.Routing;
using UnityEngine;

namespace CindarsHope.Composition
{
    public enum RuntimeCompositionState
    {
        NotInstalled = 0,
        Installing = 1,
        Ready = 2
    }

    /// <summary>
    /// Ponto único para instalar serviços runtime que já foram retirados de auto-bootstraps.
    /// Sistemas legados continuam funcionando até serem migrados explicitamente para esta raiz.
    /// </summary>
    [DefaultExecutionOrder(-32000)]
    [DisallowMultipleComponent]
    public sealed class GameRuntimeCompositionRoot : MonoBehaviour
    {
        private static GameRuntimeCompositionRoot _instance;

        public static GameRuntimeCompositionRoot Instance
        {
            get
            {
                NormalizeDestroyedInstance();
                return _instance;
            }
        }

        public static RuntimeCompositionState State { get; private set; }
        public static bool IsReady => Instance != null && State == RuntimeCompositionState.Ready;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            _instance = null;
            State = RuntimeCompositionState.NotInstalled;
            GameplayInputBlocker.Reset();
            EnsureExists();
        }

        private void Start()
        {
            NpcRuntimeInstaller.Install(transform);
            WorldRuntimeInstaller.Install(transform);
        }

        public static GameRuntimeCompositionRoot EnsureExists()
        {
            NormalizeDestroyedInstance();
            if (_instance != null)
            {
                return _instance;
            }

            var rootObject = new GameObject(nameof(GameRuntimeCompositionRoot));
            var root = rootObject.AddComponent<GameRuntimeCompositionRoot>();
            root.InitializeAsRoot();
            return root;
        }

        private void Awake()
        {
            InitializeAsRoot();
        }

        private void InitializeAsRoot()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }

            InstallRuntimeServices();
        }

        private void OnDestroy()
        {
            if (!object.ReferenceEquals(_instance, this))
            {
                return;
            }

            State = RuntimeCompositionState.NotInstalled;
            CraftingRuntimeInstaller.Uninstall();
            _instance = null;
        }

        private void OnEnable()
        {
            if (object.ReferenceEquals(_instance, this) && State == RuntimeCompositionState.Ready)
                CraftingRuntimeInstaller.Install();
        }

        private void OnDisable()
        {
            if (object.ReferenceEquals(_instance, this))
                CraftingRuntimeInstaller.Uninstall();
        }

        private static void NormalizeDestroyedInstance()
        {
            if (!object.ReferenceEquals(_instance, null) && _instance == null)
            {
                CraftingRuntimeInstaller.Uninstall();
                _instance = null;
                State = RuntimeCompositionState.NotInstalled;
            }
        }

        private static void InstallRuntimeServices()
        {
            if (State == RuntimeCompositionState.Ready || State == RuntimeCompositionState.Installing)
            {
                return;
            }

            State = RuntimeCompositionState.Installing;
            GameTimeScaleCoordinator.Reset();
            CombatStateTrackerBootstrap.Install();
            GameplayInputRouter.Install(_instance != null ? _instance.transform : null);
            ActiveSkillExecutionController.Install();
            CraftingRuntimeInstaller.Install();
            CaveRuntimeInstaller.Install(_instance != null ? _instance.transform : null);
            State = RuntimeCompositionState.Ready;
        }
    }
}
