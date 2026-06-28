using System.Collections.Generic;
using CindarsHope.Combat;

namespace CindarsHope.Combat.Bestiary
{
    // fable_33 — Band 4 FIRE (levels 41-55). Numbers from CAVE_BESTIARY_CATALOG §14/§15.
    public static partial class CanonicalBestiaryCatalog
    {
        private static IEnumerable<BestiaryCreatureDef> BandFire()
        {
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_earth_elemental_minor", DisplayName = "Minor Earth Elemental",
                Band = 4, MinLevel = 41, MaxLevel = 47, Family = "Elemental",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Tank,
                MovePrimary = EnemyMovementType.TankSlowPush, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 110, Damage = 16, Defense = 7, Xp = 60, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_iron_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_elemental",
                Notes = "Boulder Fist (posture x2); Tremor (raio 1.5); quebra paredes finas. Fraco Hammer/Lightning; imune Poison/Bleed/Burn. Drops: stone core, iron ore x2.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_ember_hound", DisplayName = "Ember Hound",
                Band = 4, MinLevel = 41, MaxLevel = 48, Family = "Beast",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.PackFlanker, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 64, Damage = 15, Defense = 2, Xp = 48, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_ember_fang", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_beast",
                Notes = "Burning Bite (Burn 20%); matilha de 3; ao morrer incha e explode (raio 1, tell). Imune Fire/Burn; fraco Ice x1.5/agua. Drops: ember fang, essence_fire.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_cave_burrower_elite", DisplayName = "Cave Burrower Elite",
                Band = 4, MinLevel = 42, MaxLevel = 50, Family = "Insect",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Elite,
                MovePrimary = EnemyMovementType.BurrowAmbush, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 130, Damage = 18, Defense = 4, Xp = 80, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_magma_chitin", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_insect",
                Notes = "Elite. Erupcao + Magma Wake (rastro lava 2s); blindado; janela 2.5s pos-erupcao. Drops: magma chitin, essence_fire.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_cinder_shade", DisplayName = "Cinder Shade",
                Band = 4, MinLevel = 44, MaxLevel = 52, Family = "Undead",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.HazardLure, MoveSecondary = EnemyMovementType.FloatingSlow,
                Hp = 56, Damage = 14, Defense = 1, Xp = 52, SpoilerTier = 0,
                IsNocturnal = true,
                PrimaryDropItemId = "item_material_shade_ash", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_undead",
                Notes = "[NOTURNA +20% pico de Nyx] Ash Grasp (puxa 1 tile p/ hazard); recua por cima de hazards (flutua); poe lava entre voces. Fraco Radiant/Silver; imune Burn. Drops: shade ash, grave dust.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_veilkin_pyromancer", DisplayName = "Veilkin Pyromancer",
                Band = 4, MinLevel = 45, MaxLevel = 53, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.CasterKeepAway, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 70, Damage = 19, Defense = 2, Xp = 62, SpoilerTier = 0,
                PrimaryDropItemId = "item_essence_fire", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "RENOMEADO (ex-Drow). Flame Lance (linha); Fire Wall (3 tiles/4s); corta rotas e recasta atras. Fraco interrupt/Ice. Drops: flame focus part, essence_fire.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_corrupted_vine_horror", DisplayName = "Corrupted Vine Horror",
                Band = 4, MinLevel = 46, MaxLevel = 54, Family = "Plant",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Tank,
                MovePrimary = EnemyMovementType.ProtectAnchor, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 140, Damage = 17, Defense = 3, Xp = 75, SpoilerTier = 0,
                PrimaryDropItemId = "item_blackstone_corrupted_shard", PrimaryDamageTypeId = "poison",
                VulnerabilityMatrixProfileId = "vulnmatrix_plant",
                Notes = "Lash (3 tiles); Constrict (Root 1.5s); ancorado a nucleo de pedra negra (destrutivel). Fraco Fire/Axe; Purify abre CriticalWindow. Drops: corrupted vine, blackstone shard.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_abyssal_hound", DisplayName = "Abyssal Hound",
                Band = 4, MinLevel = 47, MaxLevel = 55, Family = "Beast",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.GroundChase, MoveSecondary = EnemyMovementType.PhaseShortBlink,
                Hp = 86, Damage = 20, Defense = 3, Xp = 70, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_abyssal_fang", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_beast",
                Notes = "Shadow Maw; blink curto quando perde o alvo; caca em dupla. Fraco Radiant; resiste Shadow. Drops: abyssal fang, shade ash.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_ashwing_matriarch", DisplayName = "Ashwing Matriarch",
                Band = 4, MinLevel = 50, MaxLevel = 55, Family = "Beast",
                Size = BestiarySizeClass.Large, Role = EnemyRole.MiniBoss,
                MovePrimary = EnemyMovementType.FloatingOrbit, MoveSecondary = EnemyMovementType.Leaper,
                Hp = 300, Damage = 20, Defense = 3, Xp = 210, SpoilerTier = 2, IsMiniBoss = true,
                PrimaryDropItemId = "item_material_sinew", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_beast",
                Notes = "Miniboss. Dive Talon (mergulho telegrafado); Ash Storm (cegueira 1s area); janela 3s pos-mergulho. Ninho tem ovo frio (quest). Drops: ashwing feather (flecha rara), essence_fire.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_forge_tyrant_vask", DisplayName = "Forge-Tyrant Vask",
                Band = 4, MinLevel = 52, MaxLevel = 55, Family = "Humanoid",
                Size = BestiarySizeClass.Large, Role = EnemyRole.MiniBoss,
                MovePrimary = EnemyMovementType.TankSlowPush, MoveSecondary = EnemyMovementType.ChargeLine,
                Hp = 380, Damage = 22, Defense = 8, Xp = 220, SpoilerTier = 2, IsMiniBoss = true,
                PrimaryDropItemId = "item_material_bromecian_alloy", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Miniboss (ex-Gravedelver). Molten Hammer (AoE 2 - errar aplica ArmorCracked NELE = janela); Forge Slam. Blindado; fraco Lightning/posture. Drops: vask's hammer head (upgrade Brumdar), essence_fire x2.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_cindershard_wyrm", DisplayName = "Cindershard Wyrm",
                Band = 4, MinLevel = 50, MaxLevel = 50, Family = "Dragon",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Boss,
                MovePrimary = EnemyMovementType.BossArenaControl, MoveSecondary = EnemyMovementType.BossPhaseShift,
                Hp = 1100, Damage = 24, Defense = 8, Xp = 700, SpoilerTier = 3, IsBoss = true,
                PrimaryDropItemId = "item_material_wyrmling_scale", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_dragon",
                Notes = "Boss gate 50 (o dragao pequeno). F1 garras/cauda; F2 (66%) Ember Breath cone + voo curto; F3 (33%) pousa exausta cada 20s, CoreExposed 4s. Fraca Ice; imune Fire/Burn; 1o rugido aplica Fear. Mecanicas = F05 (dormante). Drops first-kill: dragon ember scale + receita scroll Fire Wall.",
            };

            // ── fable_80: +6 Fire ───────────────────────────────────────────────────────────
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_magma_slug", DisplayName = "Magma Slug",
                Band = 4, MinLevel = 41, MaxLevel = 48, Family = "Elemental",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Tank,
                MovePrimary = EnemyMovementType.TankSlowPush, MoveSecondary = EnemyMovementType.GuardStationary,
                Hp = 130, Damage = 10, Defense = 5, Xp = 55, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_magma_chitin", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_elemental",
                Notes = "Motivacao: pastador lento de fornalha que bloqueia caminhos em busca de mineral. Razao no pack: controlador de espaco; trilha de magma cria hazard tile que dura 8s. Fraqueza: agua/gelo aplica slow e cancela a trilha.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_ember_scorpion", DisplayName = "Ember Scorpion",
                Band = 4, MinLevel = 43, MaxLevel = 50, Family = "Beast",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.ChargeLine, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 75, Damage = 14, Defense = 2, Xp = 60, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_ember_fang", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_beast",
                Notes = "Motivacao: territorial perto de veios de magma; protege a ninhada. Razao no pack: perseguidor de carga em packs de fogo; ferrão aplica Burn 3s. Fraqueza: gelo paralisa a carga e expoe o abdomen.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_sulfur_wyrmling", DisplayName = "Sulfur Wyrmling",
                Band = 4, MinLevel = 45, MaxLevel = 53, Family = "Dragon",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Ranged,
                MovePrimary = EnemyMovementType.KiteRanged, MoveSecondary = EnemyMovementType.FloatingSlow,
                Hp = 65, Damage = 13, Defense = 2, Xp = 68, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_wyrmling_scale", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_dragon",
                Notes = "Motivacao: ninhada menor do Cindershard; cone de sopro sulfurico aplica Poison+Burn. Razao no pack: atirador que mantem distancia, lider de mini-ninho; guarda o adulto. Fraqueza: gelo/agua fecha o cone e para o voo.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_emberroot_horror", DisplayName = "Emberroot Horror",
                Band = 4, MinLevel = 44, MaxLevel = 52, Family = "Plant",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Guard,
                MovePrimary = EnemyMovementType.ProtectAnchor, MoveSecondary = EnemyMovementType.TankSlowPush,
                Hp = 120, Damage = 11, Defense = 4, Xp = 58, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_sinew", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_plant",
                Notes = "Motivacao: crescimento corrompido alimentado por lava e Pedra Negra; defende a fenda de onde cresce. Razao no pack: ancora de hazard; chicote de vinha flamejante alcanca 3 tiles. Fraqueza: agua extingue as vinhas temporariamente.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_veilkin_pyrecaller", DisplayName = "Veilkin Pyrecaller",
                Band = 4, MinLevel = 46, MaxLevel = 54, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.CasterKeepAway, MoveSecondary = EnemyMovementType.RetreatAndCall,
                Hp = 55, Damage = 15, Defense = 1, Xp = 72, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_shade_ash", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Motivacao: culto do Veu de Kaand do fogo; convoca brasa para homenagear o deus do conflito. Razao no pack: conjurador-lider; bola de fogo + chuva de brasas cobre a retirada. Fraqueza: silencio/agua cancela o canal de chuva.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_steam_golem_proto", DisplayName = "Steam Golem Prototype",
                Band = 4, MinLevel = 48, MaxLevel = 56, Family = "Construct",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Guard,
                MovePrimary = EnemyMovementType.BossArenaControl, MoveSecondary = EnemyMovementType.TankSlowPush,
                Hp = 160, Damage = 16, Defense = 6, Xp = 90, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_bromecian_alloy", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_construct",
                Notes = "Motivacao: unidade de forja bromeciana defeituosa que patrulha a camara sem ordens; jato de vapor e pancada de pistao. Razao no pack: guarda pesado de camara (proto-boss primitivo — sem fases de gate); leash de arena. Fraqueza: relampago/EMP desliga a caldeira 3s (CoreExposed).",
            };
        }
    }
}
