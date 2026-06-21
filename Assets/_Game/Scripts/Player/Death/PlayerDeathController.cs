using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Player.Death
{
    /// <summary>
    /// Escuta HPChangedEvent e dispara PlayerDiedEvent quando o HP chega a 0.
    ///
    /// Nasce sozinho via self-bootstrap estatico (idiom *RuntimeBootstrap do projeto):
    /// um GameObject DontDestroyOnLoad com singleton guard, sem depender de nenhum scene
    /// creator. Assim o sistema de morte sempre existe em runtime.
    ///
    /// O estado de morte rearma quando o HP volta > 0 (revive/cura), corrigindo o bug em que
    /// o jogador so morria uma vez por sessao. A cena da morte e capturada NO MOMENTO da morte
    /// (DontDestroyOnLoad persiste entre cenas, entao nao pode ser congelada no Start).
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerDeathController : MonoBehaviour
    {
        private static PlayerDeathController _instance;

        // Graça de spawn concedida ao (re)entrar numa cena ou reviver — evita morte instantanea por
        // dano de contato ao spawnar colado num inimigo (bug caverna->fazenda->caverna). Tecnico fixo.
        private const float SpawnGraceSeconds = 1.5f;

        // Opcional: se ja estiver setado por uma cena, ok; mas nao dependemos dele —
        // o HP autoritativo vem do proprio HPChangedEvent.
        [SerializeField] private PlayerManager _playerManager;

        private bool _isDead;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject("PlayerDeathController");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<PlayerDeathController>();
        }

        private void Awake()
        {
            // Singleton guard: se uma cena ja trouxe um PlayerDeathController, descarta o duplicado.
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            Debug.Log("[PlayerDeath] PlayerDeathController armado (escutando HPChangedEvent).");
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<HPChangedEvent>(OnHPChanged);
            GameEventBus.Subscribe<PlayerRespawnedEvent>(OnPlayerRespawned);
            SceneManager.sceneLoaded += OnSceneLoaded;
            // O player ja esta nesta cena no primeiro enable; concede graça inicial.
            PlayerDamageReceiver.GrantSpawnGrace(SpawnGraceSeconds);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<HPChangedEvent>(OnHPChanged);
            GameEventBus.Unsubscribe<PlayerRespawnedEvent>(OnPlayerRespawned);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // Ao (re)entrar numa cena, o player e reposicionado no anchor; conceder graça impede que um
        // inimigo restaurado do snapshot encostado no spawn mate o player antes de ele reagir.
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayerDamageReceiver.GrantSpawnGrace(SpawnGraceSeconds);
        }

        // Reviver (Lagrima da Deusa no lugar, ou respawn na Fonte) tambem ganha graça — o player pode
        // reviver perto do inimigo que o matou.
        private void OnPlayerRespawned(PlayerRespawnedEvent evt)
        {
            PlayerDamageReceiver.GrantSpawnGrace(SpawnGraceSeconds);
        }

        private void OnHPChanged(HPChangedEvent evt)
        {
            // Logica de decisao em C# puro (testavel em EditMode): decide se deve disparar morte
            // e qual o novo estado _isDead (rearma quando o HP volta > 0).
            var decision = PlayerDeathDecision.Evaluate(evt.CurrentHP, _isDead);
            _isDead = decision.NextIsDead;

            if (!decision.ShouldDie)
            {
                return;
            }

            // Cena capturada AGORA (a morte pode acontecer em qualquer cena; este objeto persiste).
            var sceneName = SceneManager.GetActiveScene().name;
            GameEventBus.Publish(new PlayerDiedEvent { SceneName = sceneName });
            Debug.Log($"[PlayerDeath] HP<=0 em '{sceneName}' -> PlayerDiedEvent publicado.");
        }
    }
}
