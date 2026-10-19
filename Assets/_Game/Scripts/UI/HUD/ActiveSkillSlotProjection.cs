namespace CindarsHope.UI.HUD
{
    /// <summary>
    /// fable_71 — projeção pura (C#, sem UnityEngine) do estado de um active skill slot para a HUD.
    /// Decide usabilidade e motivo de bloqueio a partir de inputs de runtime, para ser testável em EditMode.
    /// Nota: bloqueio por custo (NoMp/NoStamina) é deferido — o custo vive privado nos executores;
    /// expô-lo é um follow-up. Por ora o bloqueio cobre Empty e Cooldown.
    /// </summary>
    public static class ActiveSkillSlotProjection
    {
        public static void Project(bool isEquipped, float cooldownRemaining, ActiveSkillSlotViewModel vm)
        {
            if (vm == null) return;

            vm.IsEquipped = isEquipped;
            vm.IsUnlocked = isEquipped;
            vm.Cooldown = cooldownRemaining > 0f ? cooldownRemaining : 0f;

            if (!isEquipped)
            {
                vm.IsUsableInContext = false;
                vm.BlockedReason = ActiveSlotBlockedReason.Empty;
            }
            else if (cooldownRemaining > 0f)
            {
                vm.IsUsableInContext = false;
                vm.BlockedReason = ActiveSlotBlockedReason.Cooldown;
            }
            else
            {
                vm.IsUsableInContext = true;
                vm.BlockedReason = ActiveSlotBlockedReason.None;
            }
        }
    }
}
