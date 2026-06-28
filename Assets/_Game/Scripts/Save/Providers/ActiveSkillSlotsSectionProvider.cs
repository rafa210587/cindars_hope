using CindarsHope.Skills;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save dos slots de habilidades ativas (teclas R/T/Y/G). Fonte do estado:
    /// <see cref="ActiveSkillSlots"/> injetado via constructor. Fallback: todos os slots vazios
    /// quando o componente não está disponível.
    /// </summary>
    public class ActiveSkillSlotsSectionProvider : ISaveSectionProvider
    {
        private readonly ActiveSkillSlots _activeSkillSlots;

        public ActiveSkillSlotsSectionProvider(ActiveSkillSlots activeSkillSlots)
        {
            _activeSkillSlots = activeSkillSlots;
        }

        public string ProviderId => "active_skill_slots";

        public object Capture(GameSaveData existingSaveData)
        {
            var data = new ActiveSkillSlotsSaveData();
            if (_activeSkillSlots != null)
            {
                var slot0 = _activeSkillSlots.GetSlot(0);
                var slot1 = _activeSkillSlots.GetSlot(1);
                var slot2 = _activeSkillSlots.GetSlot(2);
                var slot3 = _activeSkillSlots.GetSlot(3);

                data.SlotRSkillActionId = slot0?.SkillActionId ?? string.Empty;
                data.SlotTSkillActionId = slot1?.SkillActionId ?? string.Empty;
                data.SlotYSkillActionId = slot2?.SkillActionId ?? string.Empty;
                data.SlotGSkillActionId = slot3?.SkillActionId ?? string.Empty;
            }
            return data;
        }

        public void Restore(object sectionData)
        {
            if (_activeSkillSlots == null)
            {
                return;
            }

            var data = sectionData as ActiveSkillSlotsSaveData;
            if (data == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(data.SlotRSkillActionId))
            {
                _activeSkillSlots.SetSkillInSlot(0, data.SlotRSkillActionId, 0f);
            }

            if (!string.IsNullOrEmpty(data.SlotTSkillActionId))
            {
                _activeSkillSlots.SetSkillInSlot(1, data.SlotTSkillActionId, 0f);
            }

            if (!string.IsNullOrEmpty(data.SlotYSkillActionId))
            {
                _activeSkillSlots.SetSkillInSlot(2, data.SlotYSkillActionId, 0f);
            }

            if (!string.IsNullOrEmpty(data.SlotGSkillActionId))
            {
                _activeSkillSlots.SetSkillInSlot(3, data.SlotGSkillActionId, 0f);
            }
        }
    }
}
