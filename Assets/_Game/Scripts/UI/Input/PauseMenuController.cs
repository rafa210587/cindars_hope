using System;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Core.Time;
using CindarsHope.Foundation;
using CindarsHope.Localization;
using CindarsHope.UI.SystemTab;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace CindarsHope.UI.Routing
{
    [DisallowMultipleComponent]
    public sealed class PauseMenuController : MonoBehaviour
    {
        private IModalRuntime _modal;
        private IDisposable _pause;
        private GameObject _view;
        private SystemTabController _system;
        private GameObject _resumeButton;
        private GameObject _previousSelection;
        private EventSystem _ownedEventSystem;
        public bool IsOpen => _pause != null;
        public static void Install(Transform host)
        {
            if (host != null && host.GetComponent<PauseMenuController>() == null)
                host.gameObject.AddComponent<PauseMenuController>();
        }
        private void OnEnable()
        {
            GameEventBus.Subscribe<PauseOpenedEvent>(OnOpen);
            GameEventBus.Subscribe<ModalCloseRequestedEvent>(OnClose);
        }
        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PauseOpenedEvent>(OnOpen);
            GameEventBus.Unsubscribe<ModalCloseRequestedEvent>(OnClose);
            Close();
        }
        private void OnOpen(PauseOpenedEvent evt) => Open(GameBootstrap.Instance?.ModalManager);
        private void OnClose(ModalCloseRequestedEvent evt) { if (evt.Targets(ModalType.Pause) && IsOpen) Close(); }
        public bool Open(IModalRuntime modal)
        {
            if (IsOpen) return true;
            if (modal == null || !modal.PushModal(ModalType.Pause)) return false;
            _modal = modal;
            _pause = GameTimeScaleCoordinator.AcquirePause();
            if (_system == null) _system = GetComponent<SystemTabController>() ?? gameObject.AddComponent<SystemTabController>();
            if (_view == null) BuildView();
            _previousSelection = EventSystem.current?.currentSelectedGameObject;
            if (_ownedEventSystem != null)
                _ownedEventSystem.gameObject.SetActive(EventSystem.current == null || EventSystem.current == _ownedEventSystem);
            _view.SetActive(true);
            if (EventSystem.current == null)
            {
                var input = new GameObject("PauseEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                input.transform.SetParent(_view.transform, false);
                _ownedEventSystem = input.GetComponent<EventSystem>();
            }
            EventSystem.current?.SetSelectedGameObject(_resumeButton);
            return true;
        }
        public void Close()
        {
            if (!IsOpen) return;
            var selected = EventSystem.current?.currentSelectedGameObject;
            if (selected != null && _view != null && selected.transform.IsChildOf(_view.transform))
                EventSystem.current.SetSelectedGameObject(null);
            if (_view != null) _view.SetActive(false);
            if (_previousSelection != null && _previousSelection.activeInHierarchy)
                EventSystem.current?.SetSelectedGameObject(_previousSelection);
            _previousSelection = null;
            _modal?.TryPopIfCurrent(ModalType.Pause);
            _modal = null;
            _pause.Dispose();
            _pause = null;
        }
        private void BuildView()
        {
            _view = new GameObject("PauseCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _view.transform.SetParent(transform, false);
            var canvas = _view.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5100;
            var scaler = _view.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            var shade = Element("Backdrop", _view.transform, Vector2.zero, Vector2.zero);
            shade.anchorMin = Vector2.zero;
            shade.anchorMax = Vector2.one;
            shade.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.65f);
            var panel = Element("Panel", _view.transform, Vector2.zero, new Vector2(520, 420));
            panel.gameObject.AddComponent<Image>().color = new Color(0.075f, 0.085f, 0.11f, 1);
            Label(panel, "ui.pause.title", new Vector2(0, 160), 26);
            Label(panel, "ui.pause.hint", new Vector2(0, 115), 17);
            VolumeRow(panel, "ui.settings.master", 55, () => GameAudioSettings.MasterVolume, _system.SetMasterVolume);
            VolumeRow(panel, "ui.settings.music", 5, () => GameAudioSettings.MusicVolume, _system.SetMusicVolume);
            VolumeRow(panel, "ui.settings.sfx", -45, () => GameAudioSettings.SfxVolume, _system.SetSfxVolume);
            Button(panel, "ui.settings.fullscreen", -100, () => _system.SetFullscreen(!Screen.fullScreen));
            _resumeButton = Button(panel, "ui.pause.resume", -160, Close);
        }
        private static GameObject Button(Transform parent, string key, float y, UnityEngine.Events.UnityAction action)
        {
            var rect = Element("Button", parent, new Vector2(0, y), new Vector2(300, 40));
            rect.gameObject.AddComponent<Image>().color = new Color(0.22f, 0.30f, 0.36f, 1);
            rect.gameObject.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(action);
            Label(rect, key, Vector2.zero, 18);
            return rect.gameObject;
        }
        private static void VolumeRow(Transform parent, string key, float y, Func<int> read, Action<int> write)
        {
            var label = Label(parent, key, new Vector2(0, y), 18);
            void Refresh() => label.text = $"{LocalizationService.Get(key)}: {read()}%";
            Refresh();
            foreach (var direction in new[] { -1, 1 })
            {
                var rect = Element("Adjust", parent, new Vector2(direction * 205, y), new Vector2(40, 36));
                rect.gameObject.AddComponent<Image>().color = new Color(0.22f, 0.30f, 0.36f, 1);
                rect.gameObject.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(() => { write(read() + direction * 10); Refresh(); });
                var symbol = Label(rect, direction < 0 ? "-" : "+", Vector2.zero, 24);
                symbol.rectTransform.sizeDelta = new Vector2(36, 36);
            }
        }
        private static RectTransform Element(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }
        private static Text Label(Transform parent, string key, Vector2 position, int size)
        {
            var text = Element("Label", parent, position, new Vector2(400, 40)).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.text = LocalizationService.Get(key);
            return text;
        }
    }
}
