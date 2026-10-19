using System.Collections.Generic;
using CindarsHope.Combat;

namespace CindarsHope.Combat.Bestiary
{
    // fable_33 — Band 2 FUNGAL (levels 11-25). Numbers from CAVE_BESTIARY_CATALOG §8/§9.
    public static partial class CanonicalBestiaryCatalog
    {
        private static IEnumerable<BestiaryCreatureDef> BandFungal()
        {
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_burrowing_maggot", DisplayName = "Burrowing Maggot",
                Band = 2, MinLevel = 11, MaxLevel = 16, Family = "Insect",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Burrower,
                MovePrimary = EnemyMovementType.BurrowAmbush, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 36, Damage = 6, Defense = 1, Xp = 18, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_grub_meat", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_insect",
                Notes = "So emerge sob o jogador; errar = 2s vulneravel. Drops: slime, grub meat.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_goblin_shaman", DisplayName = "Goblin Shaman",
                Band = 2, MinLevel = 11, MaxLevel = 16, Family = "Humanoid",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.CasterKeepAway, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 28, Damage = 6, Defense = 1, Xp = 20, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_glowcap", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Fica atras do pack; Mend cura aliado 15%; foge se ultimo. Fraco interrupt/dash. Drops: spark dust, glowcap.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_kobold_trapmaster", DisplayName = "Kobold Trapmaster",
                Band = 2, MinLevel = 11, MaxLevel = 17, Family = "Humanoid",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Ranged,
                MovePrimary = EnemyMovementType.RetreatAndCall, MoveSecondary = EnemyMovementType.KiteRanged,
                Hp = 24, Damage = 5, Defense = 1, Xp = 20, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_copper_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Recua plantando Snare Trap (Root 1.5s, max 2); chama reforco 1x. Drops: trap parts, darts.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_orc_grunt", DisplayName = "Orc Grunt",
                Band = 2, MinLevel = 12, MaxLevel = 18, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Tank,
                MovePrimary = EnemyMovementType.GroundChase, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 44, Damage = 8, Defense = 2, Xp = 24, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_iron_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Cleave arco 120; +15% dano 5s quando aliado morre (furia). Fraco Ice/slow. Drops: orc tusk, iron scrap.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_cave_leaper", DisplayName = "Cave Leaper",
                Band = 2, MinLevel = 12, MaxLevel = 18, Family = "Beast",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.Leaper, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 30, Damage = 7, Defense = 1, Xp = 22, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_sinew", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_beast",
                Notes = "Pounce 4 tiles (errar = 1.5s recovery vulneravel); reposiciona apos cada bote. Fraco ranged em salto. Drops: sinew, hide.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_gravedelver_crossbowman", DisplayName = "Gravedelver Crossbowman",
                Band = 2, MinLevel = 13, MaxLevel = 19, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Ranged,
                MovePrimary = EnemyMovementType.KiteRanged, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 34, Damage = 9, Defense = 3, Xp = 26, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_iron_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Gravedelver renomeado. Heavy Bolt (posture +30%); recarrega atras de cobertura. Blindado DEF3; fraco Lightning/flanqueio. Drops: bolts, iron ore, ale.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_fungal_spreader", DisplayName = "Fungal Spreader",
                Band = 2, MinLevel = 14, MaxLevel = 20, Family = "Plant",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.FloatingSlow, MoveSecondary = EnemyMovementType.FloatingOrbit,
                Hp = 40, Damage = 5, Defense = 1, Xp = 24, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_mycel_thread", PrimaryDamageTypeId = "poison",
                VulnerabilityMatrixProfileId = "vulnmatrix_plant",
                Notes = "Mycel Field (slow -25%/4s); zoneia rotas de fuga; evita fogo. Fraco Fire; imune Poison. Drops: spores x2, mycel thread.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_gloom_moth", DisplayName = "Gloom Moth",
                Band = 2, MinLevel = 15, MaxLevel = 22, Family = "Insect",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Swarm,
                MovePrimary = EnemyMovementType.SwarmErratic, MoveSecondary = EnemyMovementType.FloatingOrbit,
                Hp = 18, Damage = 4, Defense = 0, Xp = 14, SpoilerTier = 0,
                IsNocturnal = true,
                PrimaryDropItemId = "item_material_night_essence", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_insect",
                Notes = "[NOTURNA] Dust Wing (ConfusionLite 0.8s); orbita tochas/luz; enxame 5-9. Fraco Fire/Light. Drops: moth dust (reagente Ozzra).",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_packlord_ruvash", DisplayName = "Packlord Ruvash",
                Band = 2, MinLevel = 20, MaxLevel = 25, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.MiniBoss,
                MovePrimary = EnemyMovementType.PackLeader, MoveSecondary = EnemyMovementType.RetreatAndCall,
                Hp = 130, Damage = 10, Defense = 3, Xp = 85, SpoilerTier = 2, IsMiniBoss = true,
                PrimaryDropItemId = "item_material_iron_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Miniboss. Nunca luta sozinho; War Drum buffa 4 grunts; recua p/ gate se pack cai. Drops: orc warbanner (quest Hund), tabela elite.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_fungal_tyrant_sprout", DisplayName = "Fungal Tyrant Sprout",
                Band = 2, MinLevel = 22, MaxLevel = 25, Family = "Plant",
                Size = BestiarySizeClass.Large, Role = EnemyRole.MiniBoss,
                MovePrimary = EnemyMovementType.ProtectAnchor, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 160, Damage = 9, Defense = 2, Xp = 90, SpoilerTier = 2, IsMiniBoss = true,
                PrimaryDropItemId = "item_material_mycel_heart", PrimaryDamageTypeId = "poison",
                VulnerabilityMatrixProfileId = "vulnmatrix_plant",
                Notes = "Miniboss. Root Wave (Root 1s); Spore Bloom (raio 2, Poison). Fraco Fire; janela 3s pos-Root Wave. Drops: mycel heart, essence_toxic.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_fungal_patriarch", DisplayName = "Fungal Patriarch",
                Band = 2, MinLevel = 20, MaxLevel = 20, Family = "Plant",
                Size = BestiarySizeClass.Huge, Role = EnemyRole.Boss,
                MovePrimary = EnemyMovementType.BossArenaControl, MoveSecondary = EnemyMovementType.ProtectAnchor,
                Hp = 420, Damage = 11, Defense = 3, Xp = 280, SpoilerTier = 3, IsBoss = true,
                PrimaryDropItemId = "item_material_mycel_heart", PrimaryDamageTypeId = "poison",
                VulnerabilityMatrixProfileId = "vulnmatrix_plant",
                Notes = "Boss gate 20. F1 chicotadas + spawns; F2 (66%) Mycel Maze; F3 (33%) Burst Bloom, nucleo exposto 4s/ciclo. Fraco Fire (queimar anel abre CoreExposed). Mecanicas de fase = F05 (dormante). Drops first-kill: receita rara + essence_toxic x3.",
            };

            // ── fable_80: +6 Fungal ─────────────────────────────────────────────────────────
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_rotcap_cluster", DisplayName = "Rotcap Cluster",
                Band = 2, MinLevel = 11, MaxLevel = 16, Family = "Plant",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Guard,
                MovePrimary = EnemyMovementType.GuardStationary, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 20, Damage = 2, Defense = 0, Xp = 14, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_spores", PrimaryDamageTypeId = "poison",
                VulnerabilityMatrixProfileId = "vulnmatrix_plant",
                Notes = "Motivacao: colonia que toma corredores do bioma fungico naturalmente. Razao no pack: hazard estacionario que cobre packs fungicos; estouro de esporos cria nuvem de veneno (raio 1.5). Fraqueza: fogo desintegra imediatamente.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_mycelial_warden", DisplayName = "Mycelial Warden",
                Band = 2, MinLevel = 14, MaxLevel = 20, Family = "Plant",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Tank,
                MovePrimary = EnemyMovementType.ProtectAnchor, MoveSecondary = EnemyMovementType.TankSlowPush,
                Hp = 80, Damage = 7, Defense = 3, Xp = 42, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_mycel_thread", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_plant",
                Notes = "Motivacao: guarda os leitos de esporo do Patriarca; regenera HP perto de esporos. Razao no pack: tanque-ancora do pack fungico; chicote de raiz atinge 2 tiles. Fraqueza: fogo cancela a regen e abre janela de dano extra.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_goblin_shredder", DisplayName = "Goblin Shredder",
                Band = 2, MinLevel = 11, MaxLevel = 17, Family = "Humanoid",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.PackFlanker, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 28, Damage = 5, Defense = 1, Xp = 20, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_copper_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Motivacao: flanqueador do bando que segue o tambor do Orc War-Drummer. Razao no pack: PackFlanker — so engaja enquanto lider vivo; investida de lamina dupla. Fraqueza: mata o lider e eles debandam.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_orc_drummer", DisplayName = "Orc War-Drummer",
                Band = 2, MinLevel = 13, MaxLevel = 19, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.RetreatAndCall, MoveSecondary = EnemyMovementType.GuardStationary,
                Hp = 45, Damage = 4, Defense = 2, Xp = 32, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_iron_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Motivacao: reune os bandos de Kaand com o tambor de guerra fungico. Razao no pack: lider de apoio que chama reforco (RetreatAndCall) e aplica frenesi +15% dano aliados. Fraqueza: silencio/atordoamento interrompe o tambor e paralisa o buff.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_cave_stalker_cat", DisplayName = "Cave Stalker",
                Band = 2, MinLevel = 15, MaxLevel = 22, Family = "Beast",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.Leaper, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 50, Damage = 8, Defense = 1, Xp = 38, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_sinew", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_beast",
                Notes = "Motivacao: emboscador solitario que disputa presas com goblins na escuridao. Razao no pack: elite ocasional fora de pack; bote de escuridao adiciona stagger. Fraqueza: luz o revela (perde bonus de surpresa); fogo.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_spore_amalgam", DisplayName = "Spore Amalgam",
                Band = 2, MinLevel = 17, MaxLevel = 24, Family = "Aberration",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Tank,
                MovePrimary = EnemyMovementType.TankSlowPush, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 70, Damage = 6, Defense = 2, Xp = 40, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_mycel_heart", PrimaryDamageTypeId = "poison",
                VulnerabilityMatrixProfileId = "vulnmatrix_aberration",
                Notes = "Motivacao: fusao sem mente de coisas mortas que transbordou do leito fungico; absorve materia organica. Razao no pack: bruto de pressao em packs fungicos densos; engolfa e aplica lentidao. Fraqueza: fogo/radiante.",
            };
        }
    }
}
