using System.Collections.Generic;
using CindarsHope.Combat;

namespace CindarsHope.Combat.Bestiary
{
    // fable_33 — Band 6 DEEP (levels 71-85). Numbers from CAVE_BESTIARY_CATALOG §20/§21.
    public static partial class CanonicalBestiaryCatalog
    {
        private static IEnumerable<BestiaryCreatureDef> BandDeep()
        {
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_blackstone_thrall", DisplayName = "Blackstone Thrall",
                Band = 6, MinLevel = 71, MaxLevel = 80, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.GroundChase, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 150, Damage = 28, Defense = 4, Xp = 130, SpoilerTier = 1,
                PrimaryDropItemId = "item_blackstone_corrupted_shard", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Claw Frenzy; ao morrer perto de outros explode em Corruption (cadeia). Fraco Radiant; Purify o SALVA (vira aldeao resgatado - quest Ato 3). Drops: blackstone shard x2.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_draconic_wyrmling", DisplayName = "Draconic Wyrmling",
                Band = 6, MinLevel = 71, MaxLevel = 78, Family = "Dragon",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Ranged,
                MovePrimary = EnemyMovementType.FloatingOrbit, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 180, Damage = 30, Defense = 6, Xp = 160, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_wyrmling_scale", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_dragon",
                Notes = "Spark Breath (cone curto); Tail Snap. Fraco Ice; janela 2s pos-breath. Onde ha dois ha um ninho. Drops: wyrmling scale, essencia variada.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_abyssal_lurker", DisplayName = "Abyssal Lurker",
                Band = 6, MinLevel = 72, MaxLevel = 80, Family = "Aberration",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Burrower,
                MovePrimary = EnemyMovementType.TreasureIdleAmbush, MoveSecondary = EnemyMovementType.ChargeLine,
                Hp = 240, Damage = 34, Defense = 5, Xp = 180, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_lurker_eye", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_aberration",
                Notes = "Tentacle Grab (Root 1s, puxa 2 tiles); Maw Crush; finge ser cenario, ataca quando o jogador passa rente. Fraco Fire/Radiant. Drops: lurker eye, void ichor.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_deep_angler", DisplayName = "Deep Angler",
                Band = 6, MinLevel = 73, MaxLevel = 82, Family = "Aberration",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.HazardLure, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 200, Damage = 36, Defense = 4, Xp = 175, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_void_ichor", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_aberration",
                Notes = "A 'luz' imita loot a distancia (pisca em ritmo errado); Bite devastador sobre quem cai na isca; recua mantendo a isca entre voces. Fraco AoE (revela silhueta). Drops: angler lamp (UPGRADE de lanterna!), void ichor.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_earth_elemental_greater", DisplayName = "Greater Earth Elemental",
                Band = 6, MinLevel = 74, MaxLevel = 83, Family = "Elemental",
                Size = BestiarySizeClass.Huge, Role = EnemyRole.Tank,
                MovePrimary = EnemyMovementType.TankSlowPush, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 480, Damage = 34, Defense = 14, Xp = 240, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_iron_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_elemental",
                Notes = "Quake (arena treme, tell 1.4s); Boulder Throw. Imune Physical comum ate quebrar posture; fraco Hammer charged/Lightning. Drops: greater core, gems.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_whisper_of_veyraath", DisplayName = "Whisper of Veyraath",
                Band = 6, MinLevel = 76, MaxLevel = 85, Family = "Aberration",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.CasterKeepAway, MoveSecondary = EnemyMovementType.PhaseShortBlink,
                Hp = 170, Damage = 32, Defense = 3, Xp = 200, SpoilerTier = 1,
                PrimaryDropItemId = "item_essence_void", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_aberration",
                Notes = "Unmaking Bolt (DurabilityStress 20%); Litany of Silence (silencia skills 2s); ouvi-lo 10s acumulados aplica Fear automatico. Fraco Radiant/interrupt. Lore da Pedra Negra (Q1.4). Drops: void ichor, essence_void.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_orc_warlord", DisplayName = "Orc Warlord",
                Band = 6, MinLevel = 80, MaxLevel = 85, Family = "Humanoid",
                Size = BestiarySizeClass.Large, Role = EnemyRole.MiniBoss,
                MovePrimary = EnemyMovementType.ChargeLine, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 720, Damage = 40, Defense = 9, Xp = 400, SpoilerTier = 2, IsMiniBoss = true,
                PrimaryDropItemId = "item_material_iron_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Miniboss. Warlord Charge (atravessa colunas); Execute (dano DOBRADO se jogador <30% HP); janela 3.5s quando a investida erra. Drops: warlord cleaver (arma unica), essencias x2.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_goblin_warchief", DisplayName = "Goblin Warchief",
                Band = 6, MinLevel = 82, MaxLevel = 85, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.MiniBoss,
                MovePrimary = EnemyMovementType.PackLeader, MoveSecondary = EnemyMovementType.RetreatAndCall,
                Hp = 600, Damage = 34, Defense = 8, Xp = 380, SpoilerTier = 2, IsMiniBoss = true,
                IsNonAggressive = true,
                PrimaryDropItemId = "item_material_iron_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "[parcialmente NAO-AGRESSIVA] Miniboss. Command Volley (pack atira em unissono); Duel Challenge (1v1 ritual - pack PARA se aceito). Vencer o duelo sem pack morrer => quest scq_goblin_truce (banda neutra 1 run). Drops: warchief crest (quest), gold x5.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_abyssal_gatekeeper", DisplayName = "Abyssal Gatekeeper",
                Band = 6, MinLevel = 80, MaxLevel = 80, Family = "Aberration",
                Size = BestiarySizeClass.Huge, Role = EnemyRole.Boss,
                MovePrimary = EnemyMovementType.BossArenaControl, MoveSecondary = EnemyMovementType.BossPhaseShift,
                Hp = 3000, Damage = 38, Defense = 10, Xp = 1800, SpoilerTier = 3, IsBoss = true,
                PrimaryDropItemId = "item_material_void_ichor", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_aberration",
                Notes = "Boss gate 80. F1 tentaculos por zona; F2 (66%) ciclo de portais (teleporta entre 3 plataformas); F3 (33%) devora a arena pelas bordas (zona segura encolhe). Fraco Radiant; os olhos que abrem sao CoreExposed. Mecanicas = F05 (dormante). Drops first-kill: scroll Purify maior + stabilized blackstone x3.",
            };

            // ── fable_80: +6 Deep ───────────────────────────────────────────────────────────
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_void_brood_larva", DisplayName = "Void Brood Larva",
                Band = 6, MinLevel = 71, MaxLevel = 77, Family = "Aberration",
                Size = BestiarySizeClass.Tiny, Role = EnemyRole.Swarm,
                MovePrimary = EnemyMovementType.SwarmErratic, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 18, Damage = 7, Defense = 0, Xp = 35, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_void_ichor", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_aberration",
                Notes = "Motivacao: prole do ninho de Veyraath; mordida corrosiva degrada durabilidade do equipamento. Razao no pack: enxame que desgasta equipamento e cobre emboscadas do lider. Fraqueza: radiante apaga o enxame em AoE.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_mindbound_thrall", DisplayName = "Mindbound Thrall",
                Band = 6, MinLevel = 72, MaxLevel = 79, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.PackFlanker, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 75, Damage = 14, Defense = 3, Xp = 60, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_lurker_eye", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Motivacao: mortais escravizados pelo sussurro de Veyraath; golpe-fantoche erratico. Razao no pack: flanqueador que depende do mestre (PackFlanker); liberto se o lider morre (confuso por 3s). Fraqueza: radiante/purify quebra o controle mental.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_veilkin_voidassassin", DisplayName = "Veilkin Void-Assassin",
                Band = 6, MinLevel = 74, MaxLevel = 81, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Elite,
                MovePrimary = EnemyMovementType.PhaseShortBlink, MoveSecondary = EnemyMovementType.CircleStrafe,
                Hp = 90, Damage = 22, Defense = 2, Xp = 90, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_void_ichor", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Motivacao: elite do Veu que bebeu o silencio profundo; backstab por blink aplica Silence 2s. Razao no pack: assassino que reposiciona via blink; elite de pack. Fraqueza: revelar via AoE/luz bloqueia o blink por 3s.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_gloomspine_lurker", DisplayName = "Gloomspine Lurker",
                Band = 6, MinLevel = 73, MaxLevel = 80, Family = "Aberration",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Burrower,
                MovePrimary = EnemyMovementType.BurrowAmbush, MoveSecondary = EnemyMovementType.TankSlowPush,
                Hp = 180, Damage = 20, Defense = 5, Xp = 95, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_lurker_eye", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_aberration",
                Notes = "Motivacao: emboscador paciente do teto da Deep; arrasto de tentaculo e agarrar. Razao no pack: abre encontros de emboscada descendo do teto. Fraqueza: radiante revela e impede a descida; ataque de area apos o arrasto.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_corrupt_pseudowyrm", DisplayName = "Corrupted Pseudo-Wyrm",
                Band = 6, MinLevel = 76, MaxLevel = 83, Family = "Dragon",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.FloatingOrbit, MoveSecondary = EnemyMovementType.CasterKeepAway,
                Hp = 110, Damage = 18, Defense = 3, Xp = 88, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_wyrmling_scale", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_dragon",
                Notes = "Motivacao: prole draconica deformada por Pedra Negra; feixe de sopro-vazio corroe e aplica Blackstone Stain. Razao no pack: orbitador conjurador que marca alvos. Fraqueza: purify remove o Blackstone Stain e aumenta dano vs wyrm 50%.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_nyx_shade_elemental", DisplayName = "Nyx Shade",
                Band = 6, MinLevel = 75, MaxLevel = 82, Family = "Elemental",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Guard,
                MovePrimary = EnemyMovementType.HazardLure, MoveSecondary = EnemyMovementType.FloatingSlow,
                Hp = 80, Damage = 16, Defense = 2, Xp = 78, SpoilerTier = 1,
                IsNocturnal = true,
                PrimaryDropItemId = "item_material_night_essence", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_elemental",
                Notes = "[NOTURNA] Motivacao: sombra com vontade sob a lua oculta de Nyx; agarrar de sombra aplica cegueira 2s. Razao no pack: controlador que atrai para hazard tile com HazardLure. Fraqueza: luz/radiante desfaz a forma de sombra instantaneamente.",
            };
        }
    }
}
