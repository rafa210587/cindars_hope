using System.Collections.Generic;
using CindarsHope.Combat;

namespace CindarsHope.Combat.Bestiary
{
    // fable_33 — Band 1 STONE (levels 1-10). Numbers from CAVE_BESTIARY_CATALOG §5/§6.
    // Drops map the catalog's loose names to existing item_* ids (fable_32) for the F30 cross-ref.
    public static partial class CanonicalBestiaryCatalog
    {
        private static IEnumerable<BestiaryCreatureDef> BandStone()
        {
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_verdant_mite", DisplayName = "Verdant Mite",
                Band = 1, MinLevel = 1, MaxLevel = 4, Family = "Insect",
                Size = BestiarySizeClass.Tiny, Role = EnemyRole.Swarm,
                MovePrimary = EnemyMovementType.SwarmErratic, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 8, Damage = 1, Defense = 0, Xp = 4, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_chitin", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_insect",
                Notes = "Swarms 4-8; recua isolado; ignora alvo parado >3s. Fraco Fire/AoE; imune Bleed. Drops: chitin, fiber.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_pale_grub", DisplayName = "Pale Grub",
                Band = 1, MinLevel = 1, MaxLevel = 4, Family = "Insect",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Swarm,
                MovePrimary = EnemyMovementType.GroundPatrol, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 10, Damage = 1, Defense = 0, Xp = 3, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_grub_meat", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_insect",
                Notes = "Quase cega; 3+ juntas atraem Cave Leapers (isca natural). Drops: grub meat, slime.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_spore_crawler", DisplayName = "Spore Crawler",
                Band = 1, MinLevel = 1, MaxLevel = 5, Family = "Plant",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.GroundChase, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 14, Damage = 2, Defense = 0, Xp = 6, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_spores", PrimaryDamageTypeId = "poison",
                VulnerabilityMatrixProfileId = "vulnmatrix_plant",
                Notes = "Spore Puff ao morrer (Poison 10%, raio 1). Fraco Fire/Axe; imune Poison. Drops: spores, fiber.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_goblin_scrounger", DisplayName = "Goblin Scrounger",
                Band = 1, MinLevel = 2, MaxLevel = 6, Family = "Humanoid",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.PackFlanker, MoveSecondary = EnemyMovementType.RetreatAndCall,
                Hp = 16, Damage = 3, Defense = 1, Xp = 8, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_copper_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Pocket Sand (ConfusionLite 1s). Foge <30% HP carregando loot (dropa se morto). Drops: gold pouch, copper ore.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_kobold_sentry", DisplayName = "Kobold Sentry",
                Band = 1, MinLevel = 2, MaxLevel = 6, Family = "Humanoid",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Ranged,
                MovePrimary = EnemyMovementType.KiteRanged, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 12, Damage = 3, Defense = 0, Xp = 8, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_copper_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Yelp alerta o pack 1x/combate; mantem 4-5 tiles. Fraco burst melee. Drops: stones, sling fiber.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_mushroom_puffball", DisplayName = "Mushroom Puffball",
                Band = 1, MinLevel = 2, MaxLevel = 6, Family = "Plant",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Guard,
                MovePrimary = EnemyMovementType.GuardStationary, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 12, Damage = 0, Defense = 0, Xp = 6, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_glowcap", PrimaryDamageTypeId = "poison",
                VulnerabilityMatrixProfileId = "vulnmatrix_plant",
                Notes = "Estacionario; Spore Cloud (Poison 25%/3s, raio 1.5). Fraco Fire; ranged trivializa. Drops: spores, glowcap.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_rot_beetle", DisplayName = "Rot Beetle",
                Band = 1, MinLevel = 3, MaxLevel = 7, Family = "Insect",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Tank,
                MovePrimary = EnemyMovementType.TankSlowPush, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 24, Damage = 2, Defense = 2, Xp = 9, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_chitin_plate", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_insect",
                Notes = "Mandible Crush (posture +50%); ignora knockback leve; vira-se devagar (flanqueavel). Resiste Physical 25%; fraco Hammer/Toxic. Drops: chitin plate, rot gland.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_grimfang_packleader", DisplayName = "Grimfang Packleader",
                Band = 1, MinLevel = 4, MaxLevel = 8, Family = "Beast",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.PackLeader, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 30, Damage = 4, Defense = 1, Xp = 18, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_white_pelt", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_beast",
                Notes = "Lidera 3-5 bestas; Howl buffa +15% vel; 50% debanda se ele morre. Fraco Fire. Drops: fang, hide, loot_beast.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_lake_lurker", DisplayName = "Lake Lurker",
                Band = 1, MinLevel = 5, MaxLevel = 9, Family = "Beast",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Burrower,
                MovePrimary = EnemyMovementType.TreasureIdleAmbush, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 26, Damage = 5, Defense = 1, Xp = 16, SpoilerTier = 1,
                IsAquatic = true,
                PrimaryDropItemId = "item_fish_pale", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_beast",
                Notes = "[AQUATICA] Submerso ate aproximacao (tell: ondulacao); nao persegue >4 tiles da agua. Fraco Fire fora d'agua. Drops: fish_pale, slick hide.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_burrow_matron", DisplayName = "Burrow Matron",
                Band = 1, MinLevel = 8, MaxLevel = 10, Family = "Insect",
                Size = BestiarySizeClass.Large, Role = EnemyRole.MiniBoss,
                MovePrimary = EnemyMovementType.BurrowAmbush, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 90, Damage = 6, Defense = 2, Xp = 60, SpoilerTier = 2, IsMiniBoss = true,
                PrimaryDropItemId = "item_material_chitin_plate", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_insect",
                Notes = "Miniboss. Eruption (tell 1.0s); Brood Call (3 Pale Grubs a 50% HP); vulneravel 2s pos-erupcao. Resiste Physical 25%; fraco Toxic/charged. Drops: roll extra + chitin plate x3.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_scrounger_king", DisplayName = "Old Scrounger King",
                Band = 1, MinLevel = 8, MaxLevel = 10, Family = "Humanoid",
                Size = BestiarySizeClass.Small, Role = EnemyRole.MiniBoss,
                MovePrimary = EnemyMovementType.CircleStrafe, MoveSecondary = EnemyMovementType.PhaseShortBlink,
                Hp = 70, Damage = 5, Defense = 2, Xp = 55, SpoilerTier = 2, IsMiniBoss = true,
                IsNonAggressive = true,
                PrimaryDropItemId = "item_material_copper_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "[NAO-AGRESSIVA] Miniboss. Oferece quest scq_scrounger_bargain; vira miniboss so se atacado. Knife Flurry + Smoke Bomb. Fraco AoE. Drops: gold x3 + 1 anel aleatorio.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_cave_mite_queen", DisplayName = "Cave Mite Queen",
                Band = 1, MinLevel = 10, MaxLevel = 10, Family = "Insect",
                Size = BestiarySizeClass.Huge, Role = EnemyRole.Boss,
                MovePrimary = EnemyMovementType.BossArenaControl, MoveSecondary = EnemyMovementType.BossPhaseShift,
                Hp = 220, Damage = 7, Defense = 2, Xp = 150, SpoilerTier = 3, IsBoss = true,
                PrimaryDropItemId = "item_material_chitin_plate", PrimaryDamageTypeId = "poison",
                VulnerabilityMatrixProfileId = "vulnmatrix_insect",
                Notes = "Boss gate 10. F1 spawn mites + investidas; F2 (66%) Acid Spray cone; F3 (33%) frenesi +30% vel. Fraco Fire; imune ConfusionLite. Janela 2.5s pos-Acid. Mecanicas de fase = F05 (dormante ate la). Drops first-kill: receita cozinha + essencia da banda.",
            };

            // ── fable_80: +5 Stone ──────────────────────────────────────────────────────────
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_glimmer_centipede", DisplayName = "Glimmering Centipede",
                Band = 1, MinLevel = 1, MaxLevel = 4, Family = "Insect",
                Size = BestiarySizeClass.Tiny, Role = EnemyRole.Swarm,
                MovePrimary = EnemyMovementType.SwarmErratic, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 6, Damage = 1, Defense = 0, Xp = 3, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_chitin", PrimaryDamageTypeId = "poison",
                VulnerabilityMatrixProfileId = "vulnmatrix_insect",
                Notes = "Motivacao: caca atraida por sua propria bioluminescencia que atrai presas cegas. Razao no pack: enxame de fundo que preenche packs de abertura do bioma Stone. Fraqueza: fogo/AoE liquida o enxame rapido.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_stone_burrower", DisplayName = "Stone Burrower",
                Band = 1, MinLevel = 2, MaxLevel = 6, Family = "Beast",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Burrower,
                MovePrimary = EnemyMovementType.BurrowAmbush, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 18, Damage = 3, Defense = 1, Xp = 10, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_chitin_plate", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_beast",
                Notes = "Motivacao: embosca em gargalos de pedra com investida vertical surpresa. Razao no pack: isca/abre-pack que pune avanco descuidado; tell de 0.5s no chao antes do salto. Fraqueza: golpe contundente quando emergido.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_roost_cave_bat", DisplayName = "Roost Cave Bat",
                Band = 1, MinLevel = 1, MaxLevel = 5, Family = "Beast",
                Size = BestiarySizeClass.Tiny, Role = EnemyRole.Swarm,
                MovePrimary = EnemyMovementType.FloatingSlow, MoveSecondary = EnemyMovementType.SwarmErratic,
                Hp = 5, Damage = 1, Defense = 0, Xp = 3, SpoilerTier = 0,
                IsNocturnal = true,
                PrimaryDropItemId = "item_material_sinew", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_beast",
                Notes = "[NOTURNA] Motivacao: colonia de teto que defende seu roost durante a noite. Razao no pack: enxame aereo de Nyx ativo a noite, desorientador por guincho (aplica ConfusionLite 0.5s). Fraqueza: radiante/luz revela e dispersa.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_bandit_scavenger", DisplayName = "Bandit Scavenger",
                Band = 1, MinLevel = 3, MaxLevel = 7, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.GroundChase, MoveSecondary = EnemyMovementType.RetreatAndCall,
                Hp = 22, Damage = 3, Defense = 1, Xp = 12, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_copper_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Motivacao: gente quebrada que fugiu para as cavernas e saqueia recem-chegados desesperadamente. Razao no pack: lider humano improvisado de packs Stone; chama reforcos ao recuar. Fraqueza: qualquer elemento magico — sem resistencias.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_cracked_golem_shard", DisplayName = "Cracked Golem Shard",
                Band = 1, MinLevel = 4, MaxLevel = 8, Family = "Construct",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Guard,
                MovePrimary = EnemyMovementType.GuardStationary, MoveSecondary = EnemyMovementType.TankSlowPush,
                Hp = 28, Damage = 4, Defense = 3, Xp = 14, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_iron_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_construct",
                Notes = "Motivacao: reliquia bromeciana partida que ainda guarda escombros sem saber que o imperio caiu. Razao no pack: ancora estacionaria perto de tesouro inicial; imune a corrupcao. Fraqueza: relampago/EMP desativa temporariamente.",
            };
        }
    }
}
