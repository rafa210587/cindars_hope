using CindarsHope.Inventory.Data;
using CindarsHope.Combat.Weapon;

namespace CindarsHope.Combat
{
    /// <summary>Resultado do dispatch de caminho de ataque do player.</summary>
    public enum AttackPath
    {
        Melee,
        Bow,
        Spell,
        WandSpell,
        BlockedBowHand,
        BlockedNonWeapon,
        BlockedWeaponNotResolved
    }

    /// <summary>Decisoes determinísticas de ataque e dodge do player — sem dependência de UnityEngine.</summary>
    public sealed class PlayerAttackCore
    {
        // --- Dodge state ---
        private float _lastDodgeTime = float.MinValue;
        private float _dodgeEndTime = float.MinValue;
        private bool _isDodging;

        // --- Attack cooldown per slot ---
        private float _lastLeftHandAttackTime = float.MinValue;
        private float _lastRightHandAttackTime = float.MinValue;

        /// <summary>True enquanto o dodge ainda está em andamento.</summary>
        public bool IsDodging => _isDodging;

        // ---- Dodge ----

        /// <summary>Retorna true se o dodge pode ser iniciado (cooldown expirado e stamina disponível).</summary>
        /// <param name="currentTime">Tempo atual (Time.time).</param>
        /// <param name="cooldownSeconds">Cooldown configurado no MonoBehaviour.</param>
        /// <param name="hasEnoughStamina">Resultado da checagem de stamina feita pelo adapter.</param>
        public bool CanDodge(float currentTime, float cooldownSeconds, bool hasEnoughStamina)
        {
            if (currentTime < _lastDodgeTime + cooldownSeconds) return false;
            return hasEnoughStamina;
        }

        /// <summary>Registra o inicio do dodge. Deve ser chamado APÓS CanDodge retornar true.</summary>
        /// <param name="currentTime">Tempo atual (Time.time).</param>
        /// <param name="durationSeconds">Duração do dodge em segundos.</param>
        public void BeginDodge(float currentTime, float durationSeconds)
        {
            _isDodging = true;
            _lastDodgeTime = currentTime;
            _dodgeEndTime = currentTime + durationSeconds;
        }

        /// <summary>
        /// Atualiza o estado do dodge. Retorna true no frame em que o dodge termina (transicao ended).
        /// Após retornar true, IsDodging fica false.
        /// </summary>
        /// <param name="currentTime">Tempo atual (Time.time).</param>
        public bool UpdateDodge(float currentTime)
        {
            if (_isDodging && currentTime >= _dodgeEndTime)
            {
                _isDodging = false;
                return true; // dodge ended this frame
            }
            return false;
        }

        // ---- Attack cooldown por slot ----

        /// <summary>Retorna true se o slot pode atacar (cooldown expirado).</summary>
        /// <param name="slot">LeftHand ou RightHand.</param>
        /// <param name="currentTime">Tempo atual (Time.time).</param>
        /// <param name="cooldownSeconds">Cooldown calculado para este ataque.</param>
        public bool CanAttackSlot(Foundation.EquipmentSlot slot, float currentTime, float cooldownSeconds)
        {
            float lastTime = GetLastAttackTime(slot);
            return currentTime >= lastTime + cooldownSeconds;
        }

        /// <summary>Retorna o timestamp do ultimo ataque registrado para o slot (float.MinValue se nunca atacou).</summary>
        public float GetLastAttackTime(Foundation.EquipmentSlot slot)
        {
            return slot == Foundation.EquipmentSlot.LeftHand
                ? _lastLeftHandAttackTime
                : _lastRightHandAttackTime;
        }

        /// <summary>Registra o momento do ultimo ataque para um slot.</summary>
        /// <param name="slot">LeftHand ou RightHand.</param>
        /// <param name="currentTime">Tempo atual (Time.time).</param>
        public void RecordAttack(Foundation.EquipmentSlot slot, float currentTime)
        {
            if (slot == Foundation.EquipmentSlot.LeftHand)
                _lastLeftHandAttackTime = currentTime;
            else
                _lastRightHandAttackTime = currentTime;
        }

        // ---- Attack path dispatch ----

        /// <summary>
        /// Resolve o caminho de ataque a partir dos dados do item equipado, sem acessar Unity.
        /// </summary>
        /// <param name="itemData">ItemDataSO do item equipado (pode ser null = slot vazio).</param>
        /// <param name="bowCheck">WeaponDataSO do item (quando Category==Weapon e WeaponId preenchido). Pode ser null.</param>
        /// <param name="wandCheck">WeaponDataSO da arma quando há SpellId. Pode ser null.</param>
        public AttackPath ResolveAttackPath(ItemDataSO itemData, WeaponDataSO bowCheck, WeaponDataSO wandCheck)
        {
            if (itemData == null)
                return AttackPath.Melee; // slot vazio => unarmed => melee

            if (itemData.Category == ItemCategory.Ammo)
                return AttackPath.Bow;

            if (itemData.Category == ItemCategory.Magic)
                return AttackPath.Spell;

            if (itemData.Category == ItemCategory.Weapon
                && !string.IsNullOrEmpty(itemData.SpellId)
                && wandCheck != null
                && wandCheck.Type == WeaponType.Wand)
            {
                return AttackPath.WandSpell;
            }

            if (itemData.Category == ItemCategory.Weapon
                && !string.IsNullOrEmpty(itemData.WeaponId)
                && bowCheck != null
                && bowCheck.Type == WeaponType.Bow)
            {
                return AttackPath.BlockedBowHand;
            }

            if (itemData.Category != ItemCategory.Weapon)
                return AttackPath.BlockedNonWeapon;

            return AttackPath.Melee;
        }

        // ---- Bloqueio de ataque por dodge ----

        /// <summary>Retorna true se o ataque deve ser bloqueado porque o dodge está ativo.</summary>
        public bool IsAttackBlockedByDodge() => _isDodging;
    }
}
