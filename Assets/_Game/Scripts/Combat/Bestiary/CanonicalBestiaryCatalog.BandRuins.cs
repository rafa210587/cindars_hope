using System.Collections.Generic;
using CindarsHope.Combat;

namespace CindarsHope.Combat.Bestiary
{
    // fable_33 — Band 5 RUINS (levels 56-70). Numbers from CAVE_BESTIARY_CATALOG §17/§18.
    // Note: RUINS has TWO gate bosses (gate 60 Gravedelver Artificer Lord, gate 70 Draconic Guardian).
    public static partial class CanonicalBestiaryCatalog
    {
        private static IEnumerable<BestiaryCreatureDef> BandRuins()
        {
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_gnome_tinkerer", DisplayName = "Gnome Tinkerer",
                Band = 5, MinLevel = 56, MaxLevel = 62, Family = "Humanoid",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Ranged,
                MovePrimary = EnemyMovementType.RetreatAndCall, MoveSecondary = EnemyMovementType.KiteRanged,
                Hp = 76, Damage = 18, Defense = 3, Xp = 78, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_gears", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Deploy Turret (30HP, max 1); recua montando engenhocas; conserta constructs (+10% HP/s). Fraco rush; torreta fraca Lightning. Drops: gears x2, copper, turret core.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_ninrorin_phantom", DisplayName = "Ninrorin Phantom",
                Band = 5, MinLevel = 56, MaxLevel = 63, Family = "Undead",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.PhaseShortBlink, MoveSecondary = EnemyMovementType.CasterKeepAway,
                Hp = 90, Damage = 22, Defense = 2, Xp = 85, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_phantom_essence", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_undead",
                Notes = "Wail (cone, Fear 1s); Phase Strike; atravessa paredes; evita luz forte 2s. Physical -50%; fraco Radiant/Arcane/Silver. Drops: phantom essence, memory shard.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_hoardmaw", DisplayName = "Hoardmaw",
                Band = 5, MinLevel = 56, MaxLevel = 70, Family = "Aberration",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Guard,
                MovePrimary = EnemyMovementType.TreasureIdleAmbush, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 130, Damage = 26, Defense = 4, Xp = 110, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_void_ichor", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_aberration",
                Notes = "Mimic. Snap Bite (agarra 0.8s se 'abre o bau'); max 1 por nivel; se ignorado foge engolindo 1 item do chao. Fraco Fire; tell: o bau respira e nunca tem cadeado. Drops: tudo que engoliu + roll de tesouro real.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_construct_sentry", DisplayName = "Construct Sentry",
                Band = 5, MinLevel = 57, MaxLevel = 65, Family = "Construct",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Guard,
                MovePrimary = EnemyMovementType.GuardStationary, MoveSecondary = EnemyMovementType.GroundPatrol,
                Hp = 150, Damage = 20, Defense = 9, Xp = 90, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_bromecian_alloy", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_construct",
                Notes = "Overcharge Beam (linha, tell 1.2s); patrulha rota fixa; ativa ao cruzar o feixe. Fraco Hammer/Lightning (ShockOverloaded janela 3s); imune Poison/Bleed/Fear; posture x2. Drops: bromecian alloy, gears.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_night_haunt", DisplayName = "Night Haunt",
                Band = 5, MinLevel = 58, MaxLevel = 66, Family = "Undead",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.CircleStrafe, MoveSecondary = EnemyMovementType.PhaseShortBlink,
                Hp = 84, Damage = 24, Defense = 1, Xp = 95, SpoilerTier = 0,
                IsNocturnal = true,
                PrimaryDropItemId = "item_material_night_essence", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_undead",
                Notes = "[NOTURNA] Dread Touch (Fear 1.2s) - fica visivel 1.5s ao atacar (unica janela); apaga tochas (tell sonoro); caca quem se isola. Fraco Radiant; luz o revela. Drops: night essence, moth dust x2.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_ruin_warden", DisplayName = "Ruin Warden",
                Band = 5, MinLevel = 60, MaxLevel = 68, Family = "Construct",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Guard,
                MovePrimary = EnemyMovementType.ProtectAnchor, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 220, Damage = 23, Defense = 10, Xp = 120, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_warden_core", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_construct",
                Notes = "Sweep (180); Anchor Pulse (puxa 2 tiles p/ nucleo); nunca abandona a ancora; desativa se nucleo da sala recebe Purify. Nucleo nas COSTAS (flanqueio). Drops: warden core, alloy x2.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_corrupted_orc_champion", DisplayName = "Corrupted Orc Champion",
                Band = 5, MinLevel = 62, MaxLevel = 70, Family = "Humanoid",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Elite,
                MovePrimary = EnemyMovementType.GroundChase, MoveSecondary = EnemyMovementType.ChargeLine,
                Hp = 260, Damage = 28, Defense = 6, Xp = 140, SpoilerTier = 1,
                PrimaryDropItemId = "item_blackstone_corrupted_shard", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Elite. Blackstone Blade (Corruption 15%); Enraged Slam; pedra no peito BRILHA antes de cada slam (tell). Posture alta; fraco Radiant/Purify. Drops: corrupted tusk, blackstone shard x2.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_gnome_wargolem", DisplayName = "Gnome Wargolem",
                Band = 5, MinLevel = 66, MaxLevel = 70, Family = "Construct",
                Size = BestiarySizeClass.Huge, Role = EnemyRole.MiniBoss,
                MovePrimary = EnemyMovementType.TankSlowPush, MoveSecondary = EnemyMovementType.ChargeLine,
                Hp = 520, Damage = 30, Defense = 12, Xp = 280, SpoilerTier = 2, IsMiniBoss = true,
                PrimaryDropItemId = "item_material_turret_core", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_construct",
                Notes = "Miniboss. Steam Charge; Cannon Volley (3 projeteis arco); superaquece cada 25s = ShockOverloaded 4s (janela). Blindagem maxima. Drops: wargolem boiler (peca Nimble), alloy x3, essence_arcane.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_undead_lich_acolyte", DisplayName = "Lich Acolyte",
                Band = 5, MinLevel = 68, MaxLevel = 70, Family = "Undead",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.MiniBoss,
                MovePrimary = EnemyMovementType.CasterKeepAway, MoveSecondary = EnemyMovementType.PhaseShortBlink,
                Hp = 420, Damage = 26, Defense = 4, Xp = 260, SpoilerTier = 2, IsMiniBoss = true,
                PrimaryDropItemId = "item_material_phantom_essence", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_undead",
                Notes = "Miniboss. Raise Bones (3 Shamblers); Death Ward (nega 1 golpe fatal 1x). Fraco Radiant/Silver/Fire; destruir o filacterio menor remove o Ward. Drops: acolyte phylactery shard, grimoire page.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_gravedelver_artificer_lord", DisplayName = "Gravedelver Artificer Lord",
                Band = 5, MinLevel = 60, MaxLevel = 60, Family = "Humanoid",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Boss,
                MovePrimary = EnemyMovementType.BossArenaControl, MoveSecondary = EnemyMovementType.BossPhaseShift,
                Hp = 1500, Damage = 28, Defense = 10, Xp = 900, SpoilerTier = 3, IsBoss = true,
                PrimaryDropItemId = "item_material_bromecian_alloy", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "RENOMEADO (ex-Duergar). Boss gate 60. F1 martelo + 2 torretas; F2 (66%) exo-armadura a vapor (posture x2); F3 (33%) sobrecarga - arena eletrifica + exo ShockOverloaded por ciclo. Fraco Lightning. Mecanicas = F05 (dormante). Drops first-kill: receita Mithril Work.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_draconic_guardian", DisplayName = "Draconic Guardian",
                Band = 5, MinLevel = 70, MaxLevel = 70, Family = "Dragon-kin",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Boss,
                MovePrimary = EnemyMovementType.BossPhaseShift, MoveSecondary = EnemyMovementType.GuardStationary,
                Hp = 1900, Damage = 32, Defense = 9, Xp = 1100, SpoilerTier = 3, IsBoss = true,
                PrimaryDropItemId = "item_material_wyrmling_scale", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_dragon",
                Notes = "Boss gate 70. F1 lanca+escudo (GuardBreak); F2 alterna posturas (tells por cor); F3 invoca 2 Wyrmlings. Fraco Spear/Impale nas asas (WingExposed). Mecanicas = F05 (dormante). Drops: guardian scale, dragon ember scale.",
            };

            // ── fable_80: +6 Ruins ──────────────────────────────────────────────────────────
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_rune_sentry_mk2", DisplayName = "Rune Sentry Mk.II",
                Band = 5, MinLevel = 56, MaxLevel = 63, Family = "Construct",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Ranged,
                MovePrimary = EnemyMovementType.GuardStationary, MoveSecondary = EnemyMovementType.KiteRanged,
                Hp = 85, Damage = 12, Defense = 5, Xp = 60, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_bromecian_alloy", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_construct",
                Notes = "Motivacao: sentinela de Elyndor que nunca soube que o imperio caiu; varredura de feixe runico. Razao no pack: torre de fogo de cobertura; ciclo de scan de 3s seguido de burst de feixe. Fraqueza: relampago/EMP desliga 2s.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_mirror_golem", DisplayName = "Mirror Golem",
                Band = 5, MinLevel = 58, MaxLevel = 65, Family = "Construct",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Tank,
                MovePrimary = EnemyMovementType.ProtectAnchor, MoveSecondary = EnemyMovementType.TankSlowPush,
                Hp = 160, Damage = 14, Defense = 7, Xp = 80, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_bromecian_alloy", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_construct",
                Notes = "Motivacao: guarda os saloes espelhados do Arquivo de Bromecia; reflete projeteis. Razao no pack: tanque-ancora anti-ranged; empurrao apos refletir. Fraqueza: contundente quebra o espelho (perde reflexo apos 3 hits contundentes).",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_gravedelver_runepriest", DisplayName = "Gravedelver Runepriest",
                Band = 5, MinLevel = 60, MaxLevel = 67, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.CasterKeepAway, MoveSecondary = EnemyMovementType.RetreatAndCall,
                Hp = 65, Damage = 13, Defense = 2, Xp = 75, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_iron_ore", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Motivacao: exilado Gravedelver que barganha com as maquinas antigas de Elyndor. Razao no pack: conjurador-lider tecnico; barreira runica absorve 1 hit; raio arcano penetra armadura. Fraqueza: silencio quebra a barreira e exige reativacao.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_ninrorin_echo_warrior", DisplayName = "Ninrorin Echo Warrior",
                Band = 5, MinLevel = 57, MaxLevel = 64, Family = "Undead",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.CircleStrafe, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 72, Damage = 13, Defense = 3, Xp = 66, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_night_essence", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_undead",
                Notes = "Motivacao: eco-memoria dos guerreiros Ninrorin que serviram antes do fim de Bromecia. Razao no pack: flanqueador espectral; flurry de lamina-fantasma ignora 1 ponto de defesa. Fraqueza: radiante desintegra o eco em 2 hits.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_chromatic_hoardling", DisplayName = "Chromatic Hoardling",
                Band = 5, MinLevel = 56, MaxLevel = 65, Family = "Aberration",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Burrower,
                MovePrimary = EnemyMovementType.TreasureIdleAmbush, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 55, Damage = 16, Defense = 2, Xp = 70, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_bromecian_alloy", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_aberration",
                Notes = "Motivacao: atrai os gananciosos fingindo ser um bau-mimico de tesouro. Razao no pack: armadilha solitaria junto a tesouro; bote de surpresa aplica stagger. Fraqueza: qualquer dano apos revelado — sem resistencias.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_runic_warbeast", DisplayName = "Runic Warbeast",
                Band = 5, MinLevel = 62, MaxLevel = 69, Family = "Beast",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.ChargeLine, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 180, Damage = 18, Defense = 5, Xp = 95, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_sinew", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_beast",
                Notes = "Motivacao: fera de guerra bromeciana domesticada que voltou ao selvagem; runa de carga e gravada nos chifres. Razao no pack: bruto de carga; investida runica atravessa 3 tiles e aplica knockback forte. Fraqueza: relampago desativa a runa de carga temporariamente.",
            };
        }
    }
}
