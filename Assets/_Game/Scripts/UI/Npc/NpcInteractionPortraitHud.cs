using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.NPC;
using CindarsHope.NPC.Friendship;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.Npc
{
    /// <summary>
    /// Painel lateral mostrado ao conversar com um NPC: retrato (placeholder tintado por feição),
    /// nome, raça e papel, e uma barra de afinidade vermelho→amarelo→verde (opinião -100..+100).
    ///
    /// Self-wiring por eventos: aparece em <see cref="NpcInteractionStartedEvent"/>, some em
    /// <see cref="NpcInteractionEndedEvent"/>, e atualiza ao vivo em <see cref="NpcOpinionChangedEvent"/>.
    /// Lê o nome/raça/papel do <see cref="NpcTownRosterRegistry"/> e a opinião do
    /// <see cref="FriendshipService"/>. A feição vem do <see cref="NpcExpressionResolver"/> (puro).
    /// Display-only: não bloqueia raycasts (não rouba cliques do diálogo).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NpcInteractionPortraitHud : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _panel;
        [SerializeField] private Image _portrait;       // placeholder; trocar por sprite por NPC/feição depois
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _raceText;
        [SerializeField] private Text _roleText;
        [SerializeField] private Text _expressionCaption;
        [SerializeField] private Image _affinityFill;   // Image.type = Filled (Horizontal)

        // Tons de placeholder por feição (até existirem sprites de rosto reais).
        private static readonly Color HatredTint = new Color(0.62f, 0.14f, 0.14f);
        private static readonly Color DisdainTint = new Color(0.40f, 0.45f, 0.55f);
        private static readonly Color NormalTint = new Color(0.70f, 0.70f, 0.72f);
        private static readonly Color HappinessTint = new Color(0.95f, 0.80f, 0.35f);
        private static readonly Color LoveTint = new Color(0.95f, 0.52f, 0.66f);

        // Gradiente da barra: vermelho (ódio) → amarelo (neutro) → verde (amor).
        private static readonly Color BarRed = new Color(0.85f, 0.20f, 0.20f);
        private static readonly Color BarYellow = new Color(0.95f, 0.85f, 0.25f);
        private static readonly Color BarGreen = new Color(0.35f, 0.80f, 0.35f);

        private string _activeNpcId;

        private void OnEnable()
        {
            GameEventBus.Subscribe<NpcInteractionStartedEvent>(OnInteractionStarted);
            GameEventBus.Subscribe<NpcInteractionEndedEvent>(OnInteractionEnded);
            GameEventBus.Subscribe<NpcOpinionChangedEvent>(OnOpinionChanged);
            HidePanel();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<NpcInteractionStartedEvent>(OnInteractionStarted);
            GameEventBus.Unsubscribe<NpcInteractionEndedEvent>(OnInteractionEnded);
            GameEventBus.Unsubscribe<NpcOpinionChangedEvent>(OnOpinionChanged);
        }

        private void OnInteractionStarted(NpcInteractionStartedEvent evt)
        {
            if (string.IsNullOrEmpty(evt.NpcId))
            {
                return;
            }

            _activeNpcId = evt.NpcId;

            string displayName = evt.NpcId;
            string race = "—";
            string role = "—";
            if (NpcTownRosterRegistry.TryGet(evt.NpcId, out var entry) && entry != null)
            {
                if (!string.IsNullOrEmpty(entry.DisplayName)) displayName = entry.DisplayName;
                if (!string.IsNullOrEmpty(entry.Race)) race = entry.Race;
                if (!string.IsNullOrEmpty(entry.Role)) role = entry.Role;
            }

            if (_nameText != null) _nameText.text = displayName;
            if (_raceText != null) _raceText.text = race;
            if (_roleText != null) _roleText.text = role;

            int opinion = FriendshipService.Instance != null ? FriendshipService.Instance.GetOpinion(evt.NpcId) : 0;
            ApplyOpinionVisuals(opinion);
            ShowPanel();
        }

        private void OnInteractionEnded(NpcInteractionEndedEvent evt)
        {
            if (_activeNpcId == evt.NpcId)
            {
                _activeNpcId = null;
                HidePanel();
            }
        }

        private void OnOpinionChanged(NpcOpinionChangedEvent evt)
        {
            if (!string.IsNullOrEmpty(_activeNpcId) && _activeNpcId == evt.NpcId)
            {
                ApplyOpinionVisuals(evt.Opinion);
            }
        }

        private void ApplyOpinionVisuals(int opinion)
        {
            var expression = NpcExpressionResolver.Resolve(opinion);

            if (_portrait != null)
            {
                _portrait.color = TintFor(expression);
            }

            if (_expressionCaption != null)
            {
                _expressionCaption.text = NpcExpressionResolver.LabelPtBr(expression);
            }

            // t: -100 → 0, 0 → 0.5, +100 → 1.
            float t = Mathf.Clamp01((opinion - FriendshipState.OpinionMin) /
                                    (float)(FriendshipState.OpinionMax - FriendshipState.OpinionMin));
            if (_affinityFill != null)
            {
                _affinityFill.fillAmount = t;
                _affinityFill.color = BarColorFor(t);
            }
        }

        private static Color TintFor(NpcExpression expression)
        {
            switch (expression)
            {
                case NpcExpression.Hatred: return HatredTint;
                case NpcExpression.Disdain: return DisdainTint;
                case NpcExpression.Happiness: return HappinessTint;
                case NpcExpression.Love: return LoveTint;
                default: return NormalTint;
            }
        }

        private static Color BarColorFor(float t)
        {
            return t < 0.5f
                ? Color.Lerp(BarRed, BarYellow, t * 2f)
                : Color.Lerp(BarYellow, BarGreen, (t - 0.5f) * 2f);
        }

        private void ShowPanel()
        {
            if (_panel == null) return;
            _panel.alpha = 1f;
            _panel.interactable = false;   // display-only
            _panel.blocksRaycasts = false; // não rouba cliques do diálogo
        }

        private void HidePanel()
        {
            if (_panel == null) return;
            _panel.alpha = 0f;
            _panel.interactable = false;
            _panel.blocksRaycasts = false;
        }
    }
}
