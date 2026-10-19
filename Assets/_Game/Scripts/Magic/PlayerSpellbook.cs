using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Magic
{
    /// <summary>
    /// fable_07 — estado de conhecimento arcano do jogador.
    /// Direction MAGIC_LEARNING_UNLOCKS_SOURCES: a skill tree libera o DOMÍNIO; a FONTE libera a SPELL.
    /// Mantém knownSpellIds permanentes (persistidos), contadores de tomo e o conjunto transitório de
    /// magias concedidas por item equipado (NÃO persistido, NÃO entra em knownSpellIds).
    ///
    /// É um MonoBehaviour singleton (padrão F13/F16/F17: static Instance + Capture/Restore para a seção
    /// de save), mas TODA a lógica determinística vive em <see cref="SpellbookState"/> (C# puro), o que
    /// permite EditMode tests sem cena. Comunicação de gameplay só via GameEventBus (SpellLearnedEvent).
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerSpellbook : MonoBehaviour
    {
        private static PlayerSpellbook _instance;

        public static PlayerSpellbook Instance => _instance;

        private readonly SpellbookState _state = new SpellbookState();

        /// <summary>Estado determinístico (testável sem cena).</summary>
        public SpellbookState State => _state;

        /// <summary>Cópia somente-leitura das magias aprendidas permanentemente.</summary>
        public IReadOnlyCollection<string> KnownSpellIds => _state.KnownSpellIds;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Aprende uma magia permanentemente. Idempotente: re-aprender retorna false (já conhecida)
        /// e não duplica nem republica o evento. Publica SpellLearnedEvent no primeiro aprendizado.
        /// </summary>
        public bool TryLearn(string spellId, SpellSourceType source)
        {
            if (!_state.TryLearn(spellId, source))
            {
                return false;
            }

            GameEventBus.Publish(new SpellLearnedEvent(spellId, source.ToString()));
            Debug.Log($"SpellbookLog: SpellLearned. SpellId={spellId}, Source={source}", this);
            return true;
        }

        /// <summary>
        /// Publica SpellLearnedEvent SEM alterar o estado. Usado quando a mutação já ocorreu no
        /// SpellbookState (ex.: via SpellItemUseHandler operando sobre State) e só falta o evento.
        /// </summary>
        public void PublishLearned(string spellId, SpellSourceType source)
        {
            if (string.IsNullOrWhiteSpace(spellId))
            {
                return;
            }

            GameEventBus.Publish(new SpellLearnedEvent(spellId, source.ToString()));
            Debug.Log($"SpellbookLog: SpellLearned. SpellId={spellId}, Source={source}", this);
        }

        /// <summary>
        /// Castabilidade: conhecida permanentemente OU provida por item atualmente equipado.
        /// Não considera mana/cooldown (isso é do SpellCastService); é só estado de conhecimento.
        /// </summary>
        public bool CanCast(string spellId) => _state.CanCast(spellId);

        /// <summary>True se a magia foi aprendida permanentemente (ignora item equipado).</summary>
        public bool IsKnown(string spellId) => _state.IsKnown(spellId);

        /// <summary>Conjunto de magias concedidas por itens equipados (transitório, não persistido).</summary>
        public IReadOnlyCollection<string> GetGrantedByEquipment() => _state.GrantedByEquipment;

        /// <summary>
        /// Registra a magia concedida por um item equipado (ex.: wand/foco). Não adiciona conhecimento.
        /// </summary>
        public void SetEquipmentGrantedSpell(string spellId) => _state.AddEquipmentGrant(spellId);

        /// <summary>Remove a concessão de magia ao desequipar.</summary>
        public void ClearEquipmentGrantedSpell(string spellId) => _state.RemoveEquipmentGrant(spellId);

        /// <summary>Limpa todas as concessões por equipamento (ex.: troca total de loadout).</summary>
        public void ClearAllEquipmentGrants() => _state.ClearEquipmentGrants();

        /// <summary>
        /// Incrementa o contador de estudo de um tomo. Quando atinge usesRequired, aprende a magia
        /// (TryLearn com fonte Tome) e retorna true. Antes disso retorna false (ainda estudando).
        /// usesRequired &lt;= 1 aprende imediatamente.
        /// </summary>
        public bool RegisterTomeUse(string spellId, int usesRequired)
        {
            bool learnedNow = _state.RegisterTomeUse(spellId, usesRequired, out bool firstTimeKnown);
            if (learnedNow && firstTimeKnown)
            {
                GameEventBus.Publish(new SpellLearnedEvent(spellId, SpellSourceType.Tome.ToString()));
                Debug.Log($"SpellbookLog: SpellLearned. SpellId={spellId}, Source={SpellSourceType.Tome}", this);
            }
            return learnedNow;
        }

        /// <summary>Usos acumulados de estudo de um tomo (0 se nunca estudado ou já aprendido).</summary>
        public int GetTomeUses(string spellId) => _state.GetTomeUses(spellId);

        /// <summary>
        /// fable_07 — API de story unlock publicada para os hooks da WAVE 10 (Fonte de Anya).
        /// Concede uma magia permanentemente com fonte FonteStory. Não implementa a quest.
        /// </summary>
        public bool FonteStoryUnlock(string spellId) => TryLearn(spellId, SpellSourceType.FonteStory);

        // ---------------------------------------------------------------- save section (F13/F17 pattern)

        /// <summary>Captura o estado persistente do grimório (knownSpellIds + progresso de tomo).</summary>
        public SpellbookSaveData CaptureSaveData() => _state.CaptureSaveData();

        /// <summary>
        /// Restaura o grimório a partir do save. Null/seção ausente = grimório vazio (saves legados).
        /// IDs inválidos não são filtrados aqui (catálogo resolve no cast); a normalização ignora
        /// entradas vazias. Idempotente: re-restaurar substitui o estado, sem duplicar.
        /// </summary>
        public void RestoreFromSaveData(SpellbookSaveData saveData) => _state.RestoreFromSaveData(saveData);
    }
}
