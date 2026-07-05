using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.UI.Modal;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.Runtime
{
    /// <summary>
    /// fable_38 — adapter FINO (MonoBehaviour) do minimapa no GameplayHudCanvas, sob o widget de
    /// relógio (F20) no canto superior-direito (Rule 3 de ui_rules.md / HUD_LAYOUT_SCENES §2;
    /// ancorado em % da tela — nunca pixels absolutos). Toda a matemática vive no
    /// <see cref="MinimapRenderer"/> puro; esta View só:
    ///  - chama o renderer no máximo 4×/s (throttle por acumulador — CA-4, NÃO por frame);
    ///  - reusa um único buffer Color[] + uma Texture2D (sem alloc por update — risco de GC);
    ///  - alterna visibilidade com a tecla M, respeitando o modal guard (sem mover o player com
    ///    modal aberto; o minimapa é overlay não-modal — ui_rules Rule 1/7);
    ///  - é alimentado por uma <see cref="MinimapGridSource"/> injetada (sem GameObject.Find).
    ///
    /// O binding visual final (RawImage/Texture2D em scene/prefab) fica DEFERIDO para a validação
    /// final (DEFERRED_UI_VISUAL), como o widget F20. Se um RawImage for atribuído, a textura é
    /// aplicada; caso contrário a projeção segue disponível (Buffer/Source) para o binding.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MinimapWidget : MonoBehaviour
    {
        private static readonly ProfilerMarker RenderMarker =
            new ProfilerMarker("CindarsHope.UI.Minimap.Render");

        [Header("Binding visual (DEFERRED — opcional ate o wiring no Editor)")]
        [SerializeField] private RawImage _target;

        [Header("Render")]
        [Tooltip("Lado do buffer em pixels (160x160 referencia da spec; potencia de 2 nao exigida).")]
        [SerializeField] private int _pixelDim = 160;
        [Tooltip("Pixels por celula do grid (zoom). bufferCells = pixelDim / pixelsPerCell.")]
        [SerializeField] private int _pixelsPerCell = 4;

        // Throttle: 4 updates por segundo (CA-4).
        public const float UpdatesPerSecond = 4f;
        private const float UpdateInterval = 1f / UpdatesPerSecond;

        private readonly MinimapRenderer _renderer = new MinimapRenderer();
        private Color[] _buffer;
        private Texture2D _texture;
        private float _accumulator;

        private MinimapGridSource _source;
        private ModalManager _modalManager;

        public bool IsVisible { get; private set; } = true;

        /// <summary>Projeção atual (lida pelo binding visual e por testes de adapter).</summary>
        public MinimapGridSource Source => _source;

        /// <summary>Buffer de cor reutilizado (lido pelo binding visual / testes).</summary>
        public Color[] Buffer => _buffer;

        public int PixelDim => _pixelDim;
        public int PixelsPerCell => _pixelsPerCell;

        /// <summary>Injeção explícita da fonte do mapa (chamada pelo bootstrap/builder do HUD).</summary>
        public void SetSource(MinimapGridSource source)
        {
            _source = source;
            // Força um render no próximo tick.
            _accumulator = UpdateInterval;
        }

        private void Awake()
        {
            EnsureBuffer();
        }

        private void Update()
        {
            HandleToggleInput();

            if (!IsVisible || _source == null)
            {
                return;
            }

            _accumulator += Time.unscaledDeltaTime;
            if (_accumulator < UpdateInterval)
            {
                return;
            }

            _accumulator = 0f;
            RenderNow();
        }

        /// <summary>
        /// M alterna o minimapa. Respeita o modal guard: enquanto um modal está aberto, M não
        /// alterna o overlay (o painel único — incl. a aba Mapa — é a leitura modal naquele
        /// momento). Overlay não-modal nunca bloqueia movimento (ui_rules Rule 1).
        /// </summary>
        private void HandleToggleInput()
        {
            if (!Input.GetKeyDown(KeyCode.M))
            {
                return;
            }

            if (IsAnyModalOpen())
            {
                return;
            }

            IsVisible = !IsVisible;
            if (_target != null)
            {
                _target.enabled = IsVisible;
            }
        }

        private bool IsAnyModalOpen()
        {
            if (_modalManager == null)
            {
                var bootstrap = GameBootstrap.Instance;
                if (bootstrap != null)
                {
                    _modalManager = bootstrap.ModalManager;
                }
            }

            return _modalManager != null && _modalManager.HasActiveModal;
        }

        /// <summary>Renderiza imediatamente para a textura (também útil para a aba Mapa expandida).</summary>
        public void RenderNow()
        {
            using var profilerScope = RenderMarker.Auto();

            if (_source == null)
            {
                return;
            }

            EnsureBuffer();
            _renderer.Render(_source, _buffer, _pixelDim, _pixelsPerCell);

            if (_texture != null)
            {
                _texture.SetPixels(_buffer);
                _texture.Apply(false);
                if (_target != null && _target.texture != _texture)
                {
                    _target.texture = _texture;
                }
            }
        }

        private void EnsureBuffer()
        {
            if (_pixelDim < 1)
            {
                _pixelDim = 1;
            }

            if (_pixelsPerCell < 1)
            {
                _pixelsPerCell = 1;
            }

            int needed = _pixelDim * _pixelDim;
            if (_buffer == null || _buffer.Length != needed)
            {
                _buffer = new Color[needed];
            }

            if (_texture == null || _texture.width != _pixelDim || _texture.height != _pixelDim)
            {
                _texture = new Texture2D(_pixelDim, _pixelDim, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
            }
        }

        private void OnDestroy()
        {
            if (_texture != null)
            {
                Destroy(_texture);
                _texture = null;
            }
        }
    }
}
