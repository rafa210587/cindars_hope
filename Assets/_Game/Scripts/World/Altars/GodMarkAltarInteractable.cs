using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Localization;
using UnityEngine;

namespace CindarsHope.World.Altars
{
    /// <summary>
    /// fable_68 — altar/Marca interagível ("orar"). Adapter FINO: lê o id da Marca (serializado),
    /// delega TODA a regra ao <see cref="GodMarkRuntime"/> (que delega ao <see cref="GodMarkService"/>
    /// puro) e dá feedback ao jogador via <see cref="PlayerActionFeedbackEvent"/> (GameEventBus — nenhuma
    /// chamada direta MonoBehaviour↔MonoBehaviour). Texto via <see cref="LocalizationService"/> (id→string;
    /// ausente ⇒ o próprio id, fallback visível). Marcas de caverna são posicionadas pela geração da F22
    /// (id resolvido por <see cref="GodMarkCaveResolver"/>); marcas fixas de cidade/fazenda têm o id setado
    /// na cena (via editor script autorizado — fora do escopo de código desta spec).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GodMarkAltarInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _markId = GodMarkCatalog.FinanCoin;

        /// <summary>Id da Marca que este altar representa.</summary>
        public string MarkId => _markId;

        public string InteractionPrompt
        {
            get
            {
                if (GodMarkCatalog.TryGetById(_markId, out var def) && def.IsLoreOnly)
                {
                    return "Contemplar";
                }

                return "Orar";
            }
        }

        public bool CanInteract(GameObject interactor) => !string.IsNullOrEmpty(_markId);

        public void Interact(GameObject interactor)
        {
            var result = GodMarkRuntime.Pray(_markId);
            var message = BuildFeedback(result);
            GameEventBus.Publish(new PlayerActionFeedbackEvent(message, 3f));
        }

        private string BuildFeedback(GodMarkPrayResult result)
        {
            GodMarkCatalog.TryGetById(_markId, out var def);
            var markName = def != null ? LocalizationService.Get(def.DisplayNameKey) : _markId;

            switch (result)
            {
                case GodMarkPrayResult.Granted:
                    return $"{markName}: bencao concedida ate o amanhecer.";
                case GodMarkPrayResult.AlreadyPrayedToday:
                    return $"{markName}: ja oraste hoje.";
                case GodMarkPrayResult.ConditionNotMet:
                    return $"{markName}: o momento nao e propicio.";
                case GodMarkPrayResult.OfferingMissing:
                    return $"{markName}: e preciso uma oferenda.";
                case GodMarkPrayResult.LoreOnly:
                    return def != null ? LocalizationService.Get(def.LoreKey) : markName;
                default:
                    return $"{markName}.";
            }
        }

#if UNITY_EDITOR
        public void EditorSetMarkId(string markId)
        {
            _markId = markId;
        }
#endif
    }
}
