using System.Collections.Generic;
using CindarsHope.Combat;

namespace CindarsHope.Combat.Bestiary
{
    // fable_33 — PARTE I: The Four of level 101 (CAVE_BESTIARY_CATALOG §25). Assets enter here;
    // their SPECIAL mechanics stay DORMANT (documented in Notes) until F05 / the endgame spec.
    // SpoilerTier 4 = the Four. Band 7 (the 101 finale lives in the Void band's CaveScene).
    public static partial class CanonicalBestiaryCatalog
    {
        private static IEnumerable<BestiaryCreatureDef> BandFinalFour()
        {
            yield return new BestiaryCreatureDef
            {
                EnemyId = "boss_vel_karaum", DisplayName = "Vel-Karaum, Warden of the Arch",
                Band = 7, MinLevel = 101, MaxLevel = 101, Family = "Construct",
                Size = BestiarySizeClass.Huge, Role = EnemyRole.Boss,
                MovePrimary = EnemyMovementType.BossArenaControl, MoveSecondary = EnemyMovementType.BossPhaseShift,
                Hp = 7000, Damage = 50, Defense = 16, Xp = 6000, SpoilerTier = 4, IsBoss = true,
                PrimaryDropItemId = "item_material_stabilized_blackstone", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_construct",
                Notes = "Final 1/4. Construct de Elyndor, guardiao do Arco Estelar. Luta de feixes/plataformas/paciencia geometrica. Fraco contra ShockOverloaded. DORMANTE: orquestracao do encontro (sequencia + recuperacao controlada do 101) = spec futura de endgame.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "boss_cindrathel", DisplayName = "Cindrathel, the Broken Remembrance",
                Band = 7, MinLevel = 101, MaxLevel = 101, Family = "Spirit",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Boss,
                MovePrimary = EnemyMovementType.BossPhaseShift, MoveSecondary = EnemyMovementType.CircleStrafe,
                Hp = 5500, Damage = 48, Defense = 10, Xp = 6000, SpoilerTier = 4, IsBoss = true,
                PrimaryDropItemId = "item_fruto_mana", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_aberration",
                Notes = "Final 2/4. Eco de Cindar corrompido; visual x2.5. DORMANTE: ESPELHA a BUILD do jogador (classe/skills/estilo inferidos) - mecanica especial fora dos 22 Moves, fica dormante ate a spec de endgame. Recompensa: fragmento REAL da memoria de Cindar.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "boss_archivist_of_silence", DisplayName = "The Archivist of Silence",
                Band = 7, MinLevel = 101, MaxLevel = 101, Family = "Aberration",
                Size = BestiarySizeClass.Huge, Role = EnemyRole.Boss,
                MovePrimary = EnemyMovementType.BossArenaControl, MoveSecondary = EnemyMovementType.BossPhaseShift,
                Hp = 9000, Damage = 54, Defense = 14, Xp = 8000, SpoilerTier = 4, IsBoss = true,
                PrimaryDropItemId = "item_material_stabilized_blackstone", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_aberration",
                Notes = "Final 3/4. Antagonista canonico da main quest. DORMANTE: luta de INFORMACAO - remove elementos do HUD por fase (HP/minimapa/numeros somem); janelas abertas ao completar a Litania invertida. HUD-stripping = mecanica especial, dormante ate a spec de endgame.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "boss_ithryndor", DisplayName = "Ithryndor, the Buried Dawn",
                Band = 7, MinLevel = 101, MaxLevel = 101, Family = "Dragon",
                Size = BestiarySizeClass.Gargantuan, Role = EnemyRole.Boss,
                MovePrimary = EnemyMovementType.BossPhaseShift, MoveSecondary = EnemyMovementType.BossArenaControl,
                Hp = 14000, Damage = 60, Defense = 18, Xp = 12000, SpoilerTier = 4, IsBoss = true,
                PrimaryDropItemId = "item_material_wyrmling_scale", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_dragon",
                Notes = "Final 4/4 (segredo final). Dragao Ancestral adormecido sob a Fonte; carcereiro VOLUNTARIO do fragmento de Esperanca; Gargantuan 4x4. DORMANTE: luta condicional a escolha final (Proteger=aliado/nao luta; Selar=luta cerimonial parcial; Usar=luta completa 4 fases). Ithryndor scripted finale (orquestracao) = spec futura de endgame.",
            };
        }
    }
}
