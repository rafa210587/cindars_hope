using System;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using UnityEngine;

namespace CindarsHope.Magic
{
    /// <summary>
    /// fable_07 — ponto ÚNICO de uso de itens de magia (scroll/tome), no espírito do FoodConsumer:
    /// resolve o item no inventário, decide pela fonte (<see cref="SpellSourceType"/>), aplica o efeito
    /// e consome via InventoryManager. NÃO cria um segundo caminho de cast: o CastScroll delega ao
    /// callback de cast (ligado ao SpellCastService no bootstrap). NÃO cria storage paralelo de spells:
    /// usa o PlayerSpellbook como estado e o SpellDatabase como catálogo.
    ///
    /// É um serviço C# puro (sem MonoBehaviour, sem busca global de cena), o que permite EditMode tests da
    /// lógica de fonte/consumo/pré-requisito injetando fakes.
    /// </summary>
    public class SpellItemUseHandler
    {
        /// <summary>Abstração mínima do inventário para uso/consumo (testável sem cena).</summary>
        public interface IInventoryUse
        {
            bool HasItem(string itemId, int amount = 1);
            bool TryGetItemData(string itemId, out ItemDataSO itemData);
            bool RemoveItem(string itemId, int amount);
        }

        /// <summary>Resultado determinístico do uso de um item de magia.</summary>
        public enum UseResult
        {
            NotFound,
            NotASpellItem,
            PrerequisiteNotMet,
            AlreadyKnown,
            Learned,
            TomeStudied,
            Cast,
            CastFailed,
            ConsumeFailed,
            MissingDependency
        }

        private readonly IInventoryUse _inventory;
        private readonly SpellbookState _spellbook;
        private readonly Func<string, bool> _isSkillNodeUnlocked;
        private readonly Func<string, bool> _castSpell;
        private readonly Action<string, SpellSourceType> _onLearned;

        /// <param name="inventory">Inventário (uso/consumo).</param>
        /// <param name="spellbook">Estado do grimório (determinístico).</param>
        /// <param name="isSkillNodeUnlocked">Pré-requisito de domínio (SkillTreeManager.IsNodeUnlocked). Null = sem checagem.</param>
        /// <param name="castSpell">Cast de um spellId via SpellCastService (CastScroll). Null = CastScroll indisponível.</param>
        /// <param name="onLearned">Callback de aprendizado (para publicar SpellLearnedEvent fora da lógica pura). Opcional.</param>
        public SpellItemUseHandler(
            IInventoryUse inventory,
            SpellbookState spellbook,
            Func<string, bool> isSkillNodeUnlocked = null,
            Func<string, bool> castSpell = null,
            Action<string, SpellSourceType> onLearned = null)
        {
            _inventory = inventory;
            _spellbook = spellbook;
            _isSkillNodeUnlocked = isSkillNodeUnlocked;
            _castSpell = castSpell;
            _onLearned = onLearned;
        }

        /// <summary>
        /// Usa o item de magia identificado por itemId. Sequência por fonte:
        /// - LearnableScroll: valida pré-requisito → aprende (idempotente) → consome.
        /// - CastScroll: casta via callback → consome (não aprende).
        /// - Tome: incrementa estudo; ao concluir aprende; consome em todo uso.
        /// EquippedItem/None/NpcTeaching/FonteStory não são "usáveis" por aqui.
        /// </summary>
        public UseResult UseItem(string itemId)
        {
            if (_inventory == null || _spellbook == null)
            {
                return UseResult.MissingDependency;
            }

            if (string.IsNullOrWhiteSpace(itemId) || !_inventory.HasItem(itemId))
            {
                return UseResult.NotFound;
            }

            if (!_inventory.TryGetItemData(itemId, out var itemData) || itemData == null)
            {
                return UseResult.NotFound;
            }

            switch (itemData.SpellSource)
            {
                case SpellSourceType.LearnableScroll:
                    return UseLearnableScroll(itemId, itemData);
                case SpellSourceType.CastScroll:
                    return UseCastScroll(itemId, itemData);
                case SpellSourceType.Tome:
                    return UseTome(itemId, itemData);
                default:
                    return UseResult.NotASpellItem;
            }
        }

        private UseResult UseLearnableScroll(string itemId, ItemDataSO itemData)
        {
            string spellId = itemData.TaughtSpellId;
            if (string.IsNullOrWhiteSpace(spellId))
            {
                return UseResult.NotASpellItem;
            }

            // Pré-requisito de domínio na skill tree (se exigido pelo item).
            if (!string.IsNullOrWhiteSpace(itemData.RequiredSkillNodeId)
                && _isSkillNodeUnlocked != null
                && !_isSkillNodeUnlocked(itemData.RequiredSkillNodeId))
            {
                return UseResult.PrerequisiteNotMet;
            }

            if (_spellbook.IsKnown(spellId))
            {
                // Já conhecida: não consome (evita desperdiçar o scroll).
                return UseResult.AlreadyKnown;
            }

            if (!_spellbook.TryLearn(spellId, SpellSourceType.LearnableScroll))
            {
                return UseResult.AlreadyKnown;
            }

            if (!_inventory.RemoveItem(itemId, 1))
            {
                return UseResult.ConsumeFailed;
            }

            _onLearned?.Invoke(spellId, SpellSourceType.LearnableScroll);
            return UseResult.Learned;
        }

        private UseResult UseCastScroll(string itemId, ItemDataSO itemData)
        {
            string spellId = itemData.TaughtSpellId;
            if (string.IsNullOrWhiteSpace(spellId))
            {
                return UseResult.NotASpellItem;
            }

            if (_castSpell == null)
            {
                return UseResult.MissingDependency;
            }

            if (!_castSpell(spellId))
            {
                return UseResult.CastFailed;
            }

            if (!_inventory.RemoveItem(itemId, 1))
            {
                return UseResult.ConsumeFailed;
            }

            return UseResult.Cast;
        }

        private UseResult UseTome(string itemId, ItemDataSO itemData)
        {
            string spellId = itemData.TaughtSpellId;
            if (string.IsNullOrWhiteSpace(spellId))
            {
                return UseResult.NotASpellItem;
            }

            if (_spellbook.IsKnown(spellId))
            {
                return UseResult.AlreadyKnown;
            }

            int required = itemData.TomeUsesRequired;
            bool learnedNow = _spellbook.RegisterTomeUse(spellId, required, out bool firstTimeKnown);

            // Tomo é reutilizável (estudo); consumir 1 cópia por sessão de estudo.
            if (!_inventory.RemoveItem(itemId, 1))
            {
                return UseResult.ConsumeFailed;
            }

            if (learnedNow && firstTimeKnown)
            {
                _onLearned?.Invoke(spellId, SpellSourceType.Tome);
                return UseResult.Learned;
            }

            return UseResult.TomeStudied;
        }
    }
}
