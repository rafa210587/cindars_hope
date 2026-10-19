// arch: quebra do par mutuo Combat|Player (2026-07-16) — enum puro extraido de
// Combat/EnemyDataSO.cs para Foundation (molde ModalType/SkillModifierType/DamageType), para que
// Player/Progression/PlayerProgressionRules.cs calcule XP de inimigo sem nomear CindarsHope.Combat.
// Valores e ordem preservados (o enum serializa como int em cenas/prefabs).
namespace CindarsHope.Foundation
{
    public enum EnemyDifficulty
    {
        VeryEasy,
        Easy,
        Normal,
        Hard,
        Elite,
        MiniBoss,
        Boss
    }
}
