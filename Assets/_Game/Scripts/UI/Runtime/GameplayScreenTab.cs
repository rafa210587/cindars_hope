namespace CindarsHope.UI.Runtime
{
    /// <summary>
    /// fable_14 EMENDA 2026-06-12 + 2026-06-12-D: the single 9-tab gameplay panel.
    /// Order is binding: Inventory / Equipment / Skills / Quests / Social / Bestiary /
    /// Calendar / Map / System. Tab cycles between tabs; a direct shortcut (I/K/U/J/...)
    /// opens the panel already on its tab.
    ///
    /// Tabs whose owning spec has not landed yet (Social=F26, Bestiary=F20/F21,
    /// Calendar=F38, Map, System=F56) render as an explicit "em breve" placeholder; the
    /// four core tabs (Inventory/Equipment/Skills/Quests) are the live screens of this spec.
    /// </summary>
    public enum GameplayScreenTab
    {
        Inventory = 0,
        Equipment = 1,
        Skills = 2,
        Quests = 3,
        Social = 4,
        Bestiary = 5,
        Calendar = 6,
        Map = 7,
        System = 8
    }
}
