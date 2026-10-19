namespace CindarsHope.NPC
{
    // Functional gameplay classes for NPCs in Cindar's Hope.
    // These are NOT D&D classes. D&D mechanical class names (Cleric, Paladin, Fighter,
    // Wizard, Rogue, Bard, Druid, Ranger, Warlock, Sorcerer, Monk, Barbarian) are forbidden
    // as mechanical NPC classes.
    public enum FunctionalGameplayClass
    {
        None = 0,
        Plantador,
        Colhedor,
        Pescador,
        Lenhador,
        Minerador,
        Artesao,
        Explorador,
        Construtor,
        Tratador,
        Comerciante,
        Escriba,
        Curandeiro,
        Guardiao,
        Combatente,
        Pesquisador,
        Musico,
        Alquimista
    }

    public enum NpcGender { Unknown = 0, Masculine, Feminine, NonBinary }

    public enum NpcAgeBand { Child = 0, YoungAdult, Adult, MiddleAged, Elder }
}
