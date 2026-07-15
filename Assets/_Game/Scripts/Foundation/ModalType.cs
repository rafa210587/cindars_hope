// arch: quebra do par mutuo Core|UI (2026-07-15) — enum puro (sem dependencia de engine) movido de
// CindarsHope.UI.Modal para Foundation, para que Core.Bootstrap.GameBootstrap exponha o port
// IModalRuntime sem nomear CindarsHope.UI.Modal diretamente. Valores e ordem preservados (o enum
// serializa como int em cenas/prefabs).
namespace CindarsHope.Foundation
{
    public enum ModalType
    {
        None,
        Dialogue,
        QuestOffer,
        ShopMenu,
        Buy,
        Sell,
        Crafting,
        Inventory,
        CorpseRecovery,
        AnyaFountain,
        SkillTree,
        CharacterEquipment,
        Pause,
        Death,
        CaveCheckpoint,
        QuestLog,
        // fable_56: confirmacoes da aba Sistema / titulo (Carregar, Sair, recuperacao de backup).
        SystemConfirm
    }
}
