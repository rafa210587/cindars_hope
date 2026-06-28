using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.DebugTools
{
    /// <summary>
    /// Overlay de debug (tecla F7): desenha um retângulo translúcido EXATAMENTE sobre cada
    /// <see cref="Collider2D"/> da cena — vermelho = sólido (bloqueia), ciano = trigger. Serve para
    /// enxergar "blocos invisíveis" (colliders sem visual ou com visual desalinhado) e diagnosticar
    /// o que atrapalha o movimento. Dev-only; o uso de FindObjects é permitido aqui por ser uma
    /// ferramenta de debug (não é comunicação de gameplay — ver rule unity-architecture).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CollisionDebugOverlay : MonoBehaviour
    {
        [SerializeField] private KeyCode _toggleKey = KeyCode.F7;
        [SerializeField] private KeyCode _zoomKey = KeyCode.F8;

        // Tamanho ortográfico no super zoom-out (mostra os ~56×48 tiles da cidade inteira de uma vez).
        private const float ZoomOutOrthoSize = 28f;

        // Recria o overlay periodicamente enquanto ligado, para acompanhar colliders que se movem (NPCs).
        private const float RebuildInterval = 0.3f;

        private bool _on;
        private float _timer;
        private readonly List<GameObject> _markers = new List<GameObject>();
        private static Sprite s_sprite;

        private bool _zoomed;
        private float _savedOrthoSize;

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                _on = !_on;
                if (_on) Rebuild(); else Clear();
                Debug.Log($"[CollisionDebugOverlay] {(_on ? "LIGADO" : "DESLIGADO")} (F7). " +
                          "Vermelho = collider sólido (bloqueia) · Ciano = trigger.");
            }

            if (Input.GetKeyDown(_zoomKey))
            {
                ToggleZoom();
            }

            if (_on)
            {
                _timer += Time.unscaledDeltaTime;
                if (_timer >= RebuildInterval)
                {
                    _timer = 0f;
                    Rebuild();
                }
            }
        }

        // Super zoom-out: alterna o orthographicSize da câmera principal entre o valor de jogo e um
        // valor largo que mostra a cidade inteira. O CameraFollow2D só mexe na POSIÇÃO, então o zoom
        // persiste até desligar.
        private void ToggleZoom()
        {
            var cam = UnityEngine.Camera.main;
            if (cam == null || !cam.orthographic)
            {
                Debug.LogWarning("[CollisionDebugOverlay] Câmera ortográfica principal não encontrada para o zoom (F8).");
                return;
            }

            if (!_zoomed)
            {
                _savedOrthoSize = cam.orthographicSize;
                cam.orthographicSize = ZoomOutOrthoSize;
                _zoomed = true;
                Debug.Log("[CollisionDebugOverlay] Super zoom-out LIGADO (F8) — cidade inteira.");
            }
            else
            {
                cam.orthographicSize = _savedOrthoSize > 0.01f ? _savedOrthoSize : 8f;
                _zoomed = false;
                Debug.Log("[CollisionDebugOverlay] Zoom restaurado (F8).");
            }
        }

        private void OnDisable() => Clear();

        private void Rebuild()
        {
            Clear();
            var colliders = Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Exclude);
            foreach (var c in colliders)
            {
                if (c == null)
                {
                    continue;
                }

                var b = c.bounds;
                if (b.size.x < 0.001f && b.size.y < 0.001f)
                {
                    continue; // colliders degenerados (ex.: alguns triggers de UI)
                }

                var go = new GameObject("CollDbg");
                go.transform.SetParent(transform, false);
                go.transform.position = new Vector3(b.center.x, b.center.y, 0f);
                go.transform.localScale = new Vector3(Mathf.Max(0.05f, b.size.x), Mathf.Max(0.05f, b.size.y), 1f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Marker();
                sr.color = c.isTrigger ? new Color(0.2f, 0.8f, 1f, 0.28f) : new Color(1f, 0.25f, 0.2f, 0.42f);
                sr.sortingOrder = 32000; // por cima de tudo
                _markers.Add(go);
            }
        }

        private void Clear()
        {
            for (int i = 0; i < _markers.Count; i++)
            {
                if (_markers[i] != null)
                {
                    Destroy(_markers[i]);
                }
            }

            _markers.Clear();
        }

        // Sprite branco 1x1 unidade (pixelsPerUnit = largura da textura), para localScale = bounds.size
        // render do tamanho exato do collider.
        private static Sprite Marker()
        {
            if (s_sprite == null)
            {
                var t = Texture2D.whiteTexture;
                s_sprite = Sprite.Create(t, new Rect(0f, 0f, t.width, t.height), new Vector2(0.5f, 0.5f), t.width);
                s_sprite.name = "CollDbgWhite";
            }

            return s_sprite;
        }
    }
}
