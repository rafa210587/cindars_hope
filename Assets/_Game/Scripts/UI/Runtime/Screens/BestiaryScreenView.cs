using System.Collections.Generic;
using CindarsHope.Combat.Bestiary;
using CindarsHope.Core.Events;
using CindarsHope.Enemy;
using CindarsHope.Foundation;
using CindarsHope.UI.Modal;
using UnityEngine;

namespace CindarsHope.UI.Runtime.Screens
{
    /// <summary>
    /// fable_45 — thin adapter for the Bestiary CODEX tab (GameplayScreenTab.Bestiary = 5) of the
    /// single F14 panel, replacing the "em breve" placeholder. Mirrors the precedent
    /// <see cref="CalendarDayDetailScreenView"/>: opens via the panel's ModalManager (blocks gameplay),
    /// navigates by keyboard with <see cref="UiFocusController"/> (F14), closes on Esc/back.
    ///
    /// ALL data + visibility decisions are the pure <see cref="BestiaryCodexProjection"/> (which asks
    /// F21 <c>IsVisible</c> — the single knowledge source). The view never reads the catalog directly
    /// for content beyond passing it to the projection, so it can never leak a spoiler. It subscribes
    /// to <see cref="BestiaryKnowledgeUnlockedEvent"/> while open to rebind reactively, and
    /// unsubscribes on close (no save state — UI does not persist).
    ///
    /// No GameObject.Find / FindObjectOfType: the host (panel bootstrap) injects the
    /// <see cref="EnemyKnowledgeService"/> via <see cref="Configure"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BestiaryScreenView : MonoBehaviour
    {
        public const string FocusList = "bestiary.list";
        public const string FocusFilter = "bestiary.filter";
        public const string FocusClose = "bestiary.close";

        private ModalManager _modalManager;
        private EnemyKnowledgeService _knowledge;
        private IReadOnlyList<BestiaryCreatureDef> _entries;

        private readonly UiFocusController _focus = new UiFocusController();
        private BestiaryCodexModel _model;
        private BestiaryFichaModel _activeFicha;
        private int _selectedEntryIndex = -1;
        private bool _isOpen;
        private bool _subscribed;

        private BestiaryCodexProjection.CodexFilter _filter = BestiaryCodexProjection.CodexFilter.All;
        private string _familyFilter = string.Empty;

        public bool IsOpen => _isOpen;
        public BestiaryCodexModel Model => _model;
        public BestiaryFichaModel ActiveFicha => _activeFicha;
        public UiFocusController Focus => _focus;
        public BestiaryCodexProjection.CodexFilter Filter => _filter;

        /// <summary>Host wiring: knowledge service (F21) + catalog (F33, defaults to the canonical list).</summary>
        public void Configure(
            ModalManager modalManager,
            EnemyKnowledgeService knowledge,
            IReadOnlyList<BestiaryCreatureDef> entries = null)
        {
            _modalManager = modalManager;
            _knowledge = knowledge;
            _entries = entries ?? CanonicalBestiaryCatalog.All;
        }

        /// <summary>
        /// Open the codex tab. Pushes the panel's ModalType (blocks gameplay/clock). Builds the list
        /// model and subscribes for reactive rebind. Returns false if another modal is active.
        /// </summary>
        public bool Open(ModalType panelModalType)
        {
            if (_isOpen)
            {
                return false;
            }

            if (_modalManager != null && !_modalManager.PushModal(panelModalType))
            {
                return false;
            }

            _isOpen = true;
            Subscribe();
            Rebind();
            return true;
        }

        /// <summary>Esc/back: closes the tab, pops the modal, unsubscribes, clears the ficha.</summary>
        public bool Close(ModalType panelModalType)
        {
            if (!_isOpen)
            {
                return false;
            }

            _modalManager?.TryPopIfCurrent(panelModalType);
            Unsubscribe();
            _focus.Clear();
            _activeFicha = null;
            _selectedEntryIndex = -1;
            _isOpen = false;
            return true;
        }

        /// <summary>
        /// Rebuilds the list model from current knowledge (called on open and on every
        /// <see cref="BestiaryKnowledgeUnlockedEvent"/>). Preserves the active ficha by re-resolving
        /// it from the rebuilt model, so newly unlocked categories (e.g. drops after 5 kills) appear
        /// without the player re-opening the tab (CA-2 rebind).
        /// </summary>
        public void Rebind()
        {
            _model = BestiaryCodexProjection.Build(_entries, _knowledge, _filter, _familyFilter);
            _focus.SetElements(BuildFocusOrder(_model));

            if (_activeFicha != null && !string.IsNullOrWhiteSpace(_activeFicha.EnemyId))
            {
                _activeFicha = BuildFicha(_activeFicha.EnemyId);
            }
        }

        /// <summary>Cycles the v1 filter (All → Seen → Defeated → All); rebinds the list.</summary>
        public void CycleFilter()
        {
            switch (_filter)
            {
                case BestiaryCodexProjection.CodexFilter.All:
                    _filter = BestiaryCodexProjection.CodexFilter.Seen;
                    break;
                case BestiaryCodexProjection.CodexFilter.Seen:
                    _filter = BestiaryCodexProjection.CodexFilter.Defeated;
                    break;
                default:
                    _filter = BestiaryCodexProjection.CodexFilter.All;
                    break;
            }

            _selectedEntryIndex = -1;
            _activeFicha = null;
            Rebind();
        }

        /// <summary>Select a creature by id and build its ficha (the right panel). False if not visible.</summary>
        public bool SelectEntry(string enemyId)
        {
            var ficha = BuildFicha(enemyId);
            if (ficha == null)
            {
                return false;
            }

            _activeFicha = ficha;
            return true;
        }

        /// <summary>
        /// Route one navigation input. Cancel closes; Confirm opens the ficha for the focused list
        /// row (when focus is on the list). Movement walks the flat list of visible entries.
        /// </summary>
        public UiFocusController.FocusResult HandleInput(
            UiFocusController.FocusInput input, ModalType panelModalType)
        {
            var flat = FlattenEntries(_model);

            if (input == UiFocusController.FocusInput.Next)
            {
                _selectedEntryIndex = flat.Count == 0 ? -1 : (_selectedEntryIndex + 1) % flat.Count;
            }
            else if (input == UiFocusController.FocusInput.Previous)
            {
                _selectedEntryIndex = flat.Count == 0 ? -1 : (_selectedEntryIndex - 1 + flat.Count) % flat.Count;
            }

            var result = _focus.Apply(input);
            if (result == UiFocusController.FocusResult.Cancelled)
            {
                Close(panelModalType);
            }
            else if (result == UiFocusController.FocusResult.Confirmed
                && _selectedEntryIndex >= 0 && _selectedEntryIndex < flat.Count)
            {
                SelectEntry(flat[_selectedEntryIndex].EnemyId);
            }

            return result;
        }

        private BestiaryFichaModel BuildFicha(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId) || _entries == null)
            {
                return null;
            }

            foreach (var def in _entries)
            {
                if (def.EnemyId == enemyId)
                {
                    return BestiaryCodexProjection.BuildFicha(def, _knowledge);
                }
            }

            return null;
        }

        private void OnKnowledgeUnlocked(BestiaryKnowledgeUnlockedEvent evt)
        {
            if (_isOpen)
            {
                Rebind();
            }
        }

        private void Subscribe()
        {
            if (_subscribed)
            {
                return;
            }

            CindarsHope.Core.GameEventBus.Subscribe<BestiaryKnowledgeUnlockedEvent>(OnKnowledgeUnlocked);
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
            {
                return;
            }

            CindarsHope.Core.GameEventBus.Unsubscribe<BestiaryKnowledgeUnlockedEvent>(OnKnowledgeUnlocked);
            _subscribed = false;
        }

        private void OnDisable()
        {
            // Safety: never leak the subscription if the GameObject is torn down while open.
            Unsubscribe();
        }

        /// <summary>Focus order: list (when non-empty) → filter → close. Empty codex skips the list.</summary>
        public static IReadOnlyList<string> BuildFocusOrder(BestiaryCodexModel model)
        {
            var order = new List<string>();
            if (model != null && !model.IsEmpty && model.Groups.Count > 0)
            {
                order.Add(FocusList);
            }

            order.Add(FocusFilter);
            order.Add(FocusClose);
            return order;
        }

        private static List<BestiaryCodexListItem> FlattenEntries(BestiaryCodexModel model)
        {
            var flat = new List<BestiaryCodexListItem>();
            if (model == null)
            {
                return flat;
            }

            foreach (var group in model.Groups)
            {
                flat.AddRange(group.Entries);
            }

            return flat;
        }
    }
}
