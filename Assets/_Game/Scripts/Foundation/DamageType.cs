// arch: quebra do par mutuo Combat|Player (2026-07-16) — enum puro (sem dependencia de engine)
// movido de CindarsHope.Combat para Foundation (molde ModalType/SkillModifierType), para que
// Player (PlayerVitalsApplier/PlayerCombatController/PlayerStatusReceiver etc.) e Combat leiam
// o mesmo tipo sem que Player precise nomear CindarsHope.Combat. Valores e ordem preservados
// (o enum serializa como int em cenas/prefabs/saves).
namespace CindarsHope.Foundation
{
    public enum DamageType
    {
        Physical,
        Fire,
        Ice,
        Toxic,
        Lightning,
        Arcane,
        True
    }
}
