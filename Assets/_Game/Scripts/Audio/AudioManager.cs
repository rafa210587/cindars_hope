using UnityEngine;

namespace CindarsHope.Audio
{
    /// <summary>
    /// fable_58 — host de áudio do jogo (CA-1). MonoBehaviour FINO:
    /// - canais SFX/Música com volume Master + por canal (0..100);
    /// - pool de vozes SFX (round-robin, sem Instantiate por som);
    /// - canal Música com dois AudioSources para crossfade (EMENDA V3);
    /// - clipes placeholder procedurais via <see cref="ProceduralSfxFactory"/>;
    /// - fallback silencioso + log de wiring 1× (nunca exceção, nunca bloquear gameplay).
    ///
    /// Volume: consome GameAudioSettings (F56) SE existir; como F56 ainda NÃO rodou
    /// (auditoria Fase 0), usa defaults locais e expõe SetMasterVolume/SetChannelVolume
    /// para binding posterior (contrato documentado no report).
    ///
    /// Instalado pelo GameRuntimeCompositionRoot no Start() (pós-cena, AfterSceneLoad-equivalente;
    /// era self-bootstrap por RuntimeInitializeOnLoadMethod) — não exige regeneração de cena.
    ///
    /// AudioListener: as cenas geradas (CaveScene etc.) podem NÃO ter um AudioListener
    /// na câmera, o que faz o Unity logar "There are no audio listeners in the scene"
    /// por frame. Para evitar editar .unity, o AudioManager garante EXATAMENTE UM
    /// AudioListener: se nenhum existir na cena, adiciona um ao próprio GameObject
    /// (DontDestroyOnLoad), que sobrevive à troca de cena e fica único.
    ///
    /// Toda comunicação é por GameEventBus (via SfxEventBridge); nenhum sistema de
    /// gameplay referencia este manager diretamente.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AudioManager : MonoBehaviour
    {
        public const int DefaultSfxVoiceCount = 12;

        private static AudioManager _instance;
        public static AudioManager Instance => _instance;

        private int _masterVolume = AudioGainCalculator.DefaultMasterVolume;
        private int _sfxVolume = AudioGainCalculator.DefaultSfxVolume;
        private int _musicVolume = AudioGainCalculator.DefaultMusicVolume;

        private readonly ProceduralSfxFactory _factory = new ProceduralSfxFactory();

        private AudioSource[] _sfxVoices;
        private int _nextVoice;

        // Crossfade de música: dois sources em reuso (ativo + entrando).
        private AudioSource _musicA;
        private AudioSource _musicB;
        private bool _musicAIsActive = true;
        private MusicState _currentMusicState = MusicState.Calmo;

        private bool _crossfading;
        private float _crossfadeElapsed;
        private float _crossfadeDuration = MusicCrossfade.DefaultDurationSeconds;

        private bool _missingClipLogged;
        private bool _audioListenerCreatedLogged;

        // --------------------------------------------------------------- bootstrap

        public static void Install(Transform owner)
        {
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject("AudioManager");
            go.transform.SetParent(owner);
            DontDestroyOnLoad(go);
            go.AddComponent<AudioManager>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            Initialize();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Configura pool de vozes, sources de música e gera clipes placeholder.
        /// Idempotente e seguro de chamar em EditMode-criado (mas o uso normal é via Awake).
        /// </summary>
        public void Initialize()
        {
            TryAdoptAudioSettings();
            EnsureAudioListener();
            EnsureSfxVoices(DefaultSfxVoiceCount);
            EnsureMusicSources();
            _factory.GenerateAll();
        }

        /// <summary>
        /// Garante EXATAMENTE UM AudioListener no jogo. Se nenhuma cena fornecer um
        /// (caso das cenas geradas — CaveScene etc.), adiciona um ao próprio GameObject
        /// (DontDestroyOnLoad), eliminando o warning "There are no audio listeners in the
        /// scene" logado por frame. FindAnyObjectByType é o idiom de bootstrap sancionado
        /// (não é gameplay runtime). Idempotente.
        /// </summary>
        private void EnsureAudioListener()
        {
            if (GetComponent<AudioListener>() != null)
            {
                return;
            }

            if (Object.FindAnyObjectByType<AudioListener>() != null)
            {
                return; // alguma câmera/cena já fornece o listener
            }

            gameObject.AddComponent<AudioListener>();

            if (!_audioListenerCreatedLogged)
            {
                _audioListenerCreatedLogged = true;
                Debug.Log("[Audio] AudioListener ausente na cena; AudioManager criou um (DontDestroyOnLoad).");
            }
        }

        private void EnsureSfxVoices(int count)
        {
            if (_sfxVoices != null && _sfxVoices.Length == count)
            {
                return;
            }

            _sfxVoices = new AudioSource[count];
            for (int i = 0; i < count; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                src.spatialBlend = 0f; // 2D flat (v1 sem spatial)
                _sfxVoices[i] = src;
            }
        }

        private void EnsureMusicSources()
        {
            if (_musicA != null && _musicB != null)
            {
                return;
            }

            _musicA = gameObject.AddComponent<AudioSource>();
            _musicB = gameObject.AddComponent<AudioSource>();
            ConfigureMusicSource(_musicA);
            ConfigureMusicSource(_musicB);
        }

        private static void ConfigureMusicSource(AudioSource src)
        {
            src.playOnAwake = false;
            src.loop = true;
            src.spatialBlend = 0f;
            src.volume = 0f;
        }

        private void TryAdoptAudioSettings()
        {
            // F56 (GameAudioSettings) ainda não existe no projeto (auditoria Fase 0).
            // Quando existir, o binding chamará SetMasterVolume/SetChannelVolume.
            // Aqui mantemos os defaults locais conservadores.
        }

        // --------------------------------------------------------------- volume API

        /// <summary>Define o volume master (0..100) — ponto de binding da F56.</summary>
        public void SetMasterVolume(int volume0To100)
        {
            _masterVolume = AudioGainCalculator.ClampVolume(volume0To100);
            ApplyMusicGain();
        }

        /// <summary>Define o volume de um canal (0..100) — ponto de binding da F56.</summary>
        public void SetChannelVolume(AudioChannel channel, int volume0To100)
        {
            int clamped = AudioGainCalculator.ClampVolume(volume0To100);
            if (channel == AudioChannel.Music)
            {
                _musicVolume = clamped;
                ApplyMusicGain();
            }
            else
            {
                _sfxVolume = clamped;
            }
        }

        public int MasterVolume => _masterVolume;
        public int SfxVolume => _sfxVolume;
        public int MusicVolume => _musicVolume;

        /// <summary>Ganho linear [0,1] efetivo de um canal (Master × canal). Testável.</summary>
        public float GetChannelGain(AudioChannel channel)
        {
            int channelVolume = channel == AudioChannel.Music ? _musicVolume : _sfxVolume;
            return AudioGainCalculator.ComputeGain(_masterVolume, channelVolume);
        }

        // --------------------------------------------------------------- SFX API

        /// <summary>
        /// Toca o SFX placeholder da categoria via uma voz do pool. Nunca lança:
        /// categoria None/sem clipe = silêncio + 1 log de wiring. Único call site
        /// real é o <see cref="SfxEventBridge"/>.
        /// </summary>
        public void PlaySfx(SfxCategory category)
        {
            if (category == SfxCategory.None)
            {
                return;
            }

            var clip = _factory.GetSfxClip(category);
            if (clip == null)
            {
                LogMissingClipOnce(category);
                return; // silêncio — gameplay nunca bloqueia
            }

            var voice = NextSfxVoice();
            if (voice == null)
            {
                return;
            }

            voice.clip = clip;
            voice.volume = GetChannelGain(AudioChannel.Sfx);
            voice.Play();
        }

        private AudioSource NextSfxVoice()
        {
            if (_sfxVoices == null || _sfxVoices.Length == 0)
            {
                return null;
            }

            var voice = _sfxVoices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _sfxVoices.Length;
            return voice;
        }

        private void LogMissingClipOnce(SfxCategory category)
        {
            if (_missingClipLogged)
            {
                return;
            }

            _missingClipLogged = true;
            Debug.LogWarning($"[Audio] Sem clipe placeholder para categoria '{SfxCategoryIds.ToStableId(category)}'. Silêncio (gameplay não é bloqueado). Wiring da fase de arte pendente.");
        }

        // --------------------------------------------------------------- Música / crossfade

        /// <summary>
        /// Troca a faixa-placeholder para o estado pedido com crossfade curto.
        /// Idempotente: mesmo estado não reinicia. Nunca lança.
        /// </summary>
        public void PlayMusic(MusicState state)
        {
            if (state == _currentMusicState && (_musicA != null && (_musicA.isPlaying || _musicB != null && _musicB.isPlaying)))
            {
                return;
            }

            var nextClip = _factory.GetMusicClip(state);
            _currentMusicState = state;

            EnsureMusicSources();
            if (_musicA == null || _musicB == null)
            {
                return;
            }

            var incoming = _musicAIsActive ? _musicB : _musicA;
            incoming.clip = nextClip; // null = silêncio para esse estado (fallback)
            incoming.volume = 0f;
            if (nextClip != null)
            {
                incoming.Play();
            }

            _musicAIsActive = !_musicAIsActive;
            _crossfading = true;
            _crossfadeElapsed = 0f;
        }

        /// <summary>Para a música (fade de saída via crossfade para silêncio).</summary>
        public void StopMusic()
        {
            if (_musicA != null)
            {
                _musicA.Stop();
                _musicA.volume = 0f;
            }

            if (_musicB != null)
            {
                _musicB.Stop();
                _musicB.volume = 0f;
            }

            _crossfading = false;
        }

        public MusicState CurrentMusicState => _currentMusicState;

        /// <summary>Número de vozes SFX no pool (diagnóstico/teste).</summary>
        public int SfxVoiceCount => _sfxVoices?.Length ?? 0;

        private void Update()
        {
            if (!_crossfading)
            {
                return;
            }

            _crossfadeElapsed += Time.unscaledDeltaTime;
            ApplyMusicGain();

            if (MusicCrossfade.IsComplete(MusicCrossfade.Progress(_crossfadeElapsed, _crossfadeDuration)))
            {
                _crossfading = false;
                // Para a faixa que saiu para liberar a voz.
                var outgoing = _musicAIsActive ? _musicB : _musicA;
                if (outgoing != null)
                {
                    outgoing.Stop();
                    outgoing.volume = 0f;
                }
            }
        }

        private void ApplyMusicGain()
        {
            if (_musicA == null || _musicB == null)
            {
                return;
            }

            float channelGain = GetChannelGain(AudioChannel.Music);
            var active = _musicAIsActive ? _musicA : _musicB;
            var inactive = _musicAIsActive ? _musicB : _musicA;

            if (_crossfading)
            {
                float progress = MusicCrossfade.Progress(_crossfadeElapsed, _crossfadeDuration);
                // 'active' é o que está ENTRANDO (trocamos a flag em PlayMusic).
                active.volume = channelGain * MusicCrossfade.FadeInFactor(progress);
                inactive.volume = channelGain * MusicCrossfade.FadeOutFactor(progress);
            }
            else
            {
                active.volume = channelGain;
                inactive.volume = 0f;
            }
        }

    }
}
