using System.Collections.Generic;
using CindarsHope.Combat;

namespace CindarsHope.Combat.Bestiary
{
    // fable_33 — Band 7 VOID / Pedra Negra (levels 86-101). Numbers from CAVE_BESTIARY_CATALOG §23/§24.
    // Note: VOID has TWO gate bosses (gate 90 Void Herald, gate 100 Draconic Elder).
    public static partial class CanonicalBestiaryCatalog
    {
        private static IEnumerable<BestiaryCreatureDef> BandVoid()
        {
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_void_husk", DisplayName = "Void Husk",
                Band = 7, MinLevel = 86, MaxLevel = 94, Family = "Aberration",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.GroundChase, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 260, Damage = 40, Defense = 6, Xp = 260, SpoilerTier = 1,
                PrimaryDropItemId = "item_blackstone_corrupted_shard", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_aberration",
                Notes = "Garras; Corruption por proximidade (10%/s adjacente); andam em grupos. Fraco Radiant; aqui Purify ja nao salva ninguem. Drops: blackstone shards.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_veilkin_blademaster", DisplayName = "Veilkin Blademaster",
                Band = 7, MinLevel = 87, MaxLevel = 96, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.CircleStrafe, MoveSecondary = EnemyMovementType.PhaseShortBlink,
                Hp = 320, Damage = 46, Defense = 7, Xp = 300, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_fiber", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "RENOMEADO para Veilkin. Flurry (4 golpes); Mirror Parry (reflete 1 projetil); duelista - castiga padroes repetidos (mesmo golpe 3x = punicao). Fraco AoE/Stagger. Drops: master blade part, veil cloth x2.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_starfall_remnant", DisplayName = "Starfall Remnant",
                Band = 7, MinLevel = 88, MaxLevel = 98, Family = "Elemental",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Tank,
                MovePrimary = EnemyMovementType.FloatingSlow, MoveSecondary = EnemyMovementType.CasterKeepAway,
                Hp = 420, Damage = 44, Defense = 12, Xp = 320, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_star_iron", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_elemental",
                Notes = "Meteor Shard Rain (area telegrafada); Gravity Well (puxa 1.5s). Combo Purify->ShockOverloaded e a fraqueza desenhada. Peca central da revelacao Q1.4. Drops: stabilized blackstone, star iron.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_silence_warden", DisplayName = "Silence Warden",
                Band = 7, MinLevel = 90, MaxLevel = 100, Family = "Construct",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Guard,
                MovePrimary = EnemyMovementType.ProtectAnchor, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 500, Damage = 42, Defense = 14, Xp = 360, SpoilerTier = 1,
                IsNonAggressive = true,
                PrimaryDropItemId = "item_material_bromecian_alloy", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_construct",
                Notes = "[condicionalmente NAO-AGRESSIVA] Construct NYMIRIANO (nao corrompido). Judgement Sweep; Sealing Pulse (silencia magia 2.5s). PARA de atacar se o jogador carrega Agua Viva (quest scq_warden_offering, liga Ato 3). Fraco Lightning. Drops: nymirian engraving (peca de lore Ato 3).",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_dread_choir", DisplayName = "Dread Choir",
                Band = 7, MinLevel = 92, MaxLevel = 101, Family = "Undead",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.FloatingOrbit, MoveSecondary = EnemyMovementType.CasterKeepAway,
                Hp = 180, Damage = 38, Defense = 4, Xp = 150, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_night_essence", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_undead",
                Notes = "Trio (stats por membro). Doom Verse (AoE Fear+Corruption, cast 2.0s); cada membro morto enfraquece o acorde (-33% dano). Fraco Radiant; interrupt quebra o verso. Drops: choir mask, night essence x2.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_heralds_hand", DisplayName = "Herald's Hand",
                Band = 7, MinLevel = 94, MaxLevel = 100, Family = "Aberration",
                Size = BestiarySizeClass.Huge, Role = EnemyRole.MiniBoss,
                MovePrimary = EnemyMovementType.ChargeLine, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 1100, Damage = 48, Defense = 10, Xp = 650, SpoilerTier = 2, IsMiniBoss = true,
                PrimaryDropItemId = "item_essence_void", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_aberration",
                Notes = "Miniboss. Investidas + agarrao que arrasta o jogador para fora da zona segura; janela apos cada agarrao. Fraco Radiant. Drops: tabela de miniboss + essence_void.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_gravelborn_twins", DisplayName = "Gravelborn Twins",
                Band = 7, MinLevel = 96, MaxLevel = 100, Family = "Elemental",
                Size = BestiarySizeClass.Large, Role = EnemyRole.MiniBoss,
                MovePrimary = EnemyMovementType.TankSlowPush, MoveSecondary = EnemyMovementType.KiteRanged,
                Hp = 900, Damage = 44, Defense = 12, Xp = 600, SpoilerTier = 2, IsMiniBoss = true,
                PrimaryDropItemId = "item_material_star_iron", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_elemental",
                Notes = "Miniboss DUPLO (Q2.4). Par complementar (um TankSlowPush, um KiteRanged). Enquanto AMBOS vivem regeneram 1% HP/s; matar um e demorar >10s no outro = enrage (+40%). Drops: twin cores (par de acessorios unicos).",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_void_herald", DisplayName = "Void Herald",
                Band = 7, MinLevel = 90, MaxLevel = 90, Family = "Aberration",
                Size = BestiarySizeClass.Huge, Role = EnemyRole.Boss,
                MovePrimary = EnemyMovementType.BossArenaControl, MoveSecondary = EnemyMovementType.BossPhaseShift,
                Hp = 4800, Damage = 46, Defense = 12, Xp = 3000, SpoilerTier = 3, IsBoss = true,
                PrimaryDropItemId = "item_material_stabilized_blackstone", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_aberration",
                Notes = "Boss gate 90. F1 lancas do vazio + Husks; F2 (66%) inverte a arena (ConfusionLite zonal); F3 (33%) abre fendas (cair = dano + reposicao). Fraco Radiant/Purify. Mecanicas = F05 (dormante). Drops first-kill: scroll Purify maior + stabilized blackstone x3.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_draconic_elder", DisplayName = "Draconic Elder",
                Band = 7, MinLevel = 100, MaxLevel = 100, Family = "Dragon",
                Size = BestiarySizeClass.Huge, Role = EnemyRole.Boss,
                MovePrimary = EnemyMovementType.BossPhaseShift, MoveSecondary = EnemyMovementType.FloatingOrbit,
                Hp = 6500, Damage = 52, Defense = 14, Xp = 4500, SpoilerTier = 3, IsBoss = true,
                PrimaryDropItemId = "item_material_wyrmling_scale", PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_dragon",
                Notes = "Boss gate 100 (carcereiro voluntario do 101). F1 terra (garra/cauda/breath); F2 voo (FloatingOrbit + mergulhos); F3 pousa exausto em ciclos (CoreExposed 5s) + chama Wyrmlings. Fraco Ice nas asas/Radiant no peito. Mecanicas = F05 (dormante). Drops first-kill: elder scale set (armadura final).",
            };

            // ── fable_80: +5 Void ───────────────────────────────────────────────────────────
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_void_tendril_watcher", DisplayName = "Void Tendril Watcher",
                Band = 7, MinLevel = 86, MaxLevel = 92, Family = "Aberration",
                Size = BestiarySizeClass.Huge, Role = EnemyRole.Guard,
                MovePrimary = EnemyMovementType.ProtectAnchor, MoveSecondary = EnemyMovementType.TankSlowPush,
                Hp = 280, Damage = 25, Defense = 8, Xp = 150, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_void_ichor", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_aberration",
                Notes = "Motivacao: fragmento do olhar de Veyraath; feixe-olho e campo de tentaculos protegem o ponto de ancora. Razao no pack: ancora-controlador de camara; so radiante puro faz dano real (purify falha neste). Fraqueza: radiante concentrado no olho central (CoreExposed) por 2s.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_reality_render", DisplayName = "Reality Render",
                Band = 7, MinLevel = 88, MaxLevel = 95, Family = "Aberration",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.ChargeLine, MoveSecondary = EnemyMovementType.PhaseShortBlink,
                Hp = 220, Damage = 28, Defense = 6, Xp = 140, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_void_ichor", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_aberration",
                Notes = "Motivacao: desfaz o que toca; bote de rasgo-de-fase aplica Silence instantaneo. Razao no pack: perseguidor de pressao de endgame; combina carga e blink para nao deixar respiro. Fraqueza: radiante e a unica escola de dano que nao e absorvida.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_veilkin_voidknight", DisplayName = "Veilkin Void-Knight",
                Band = 7, MinLevel = 90, MaxLevel = 97, Family = "Humanoid",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Elite,
                MovePrimary = EnemyMovementType.CircleStrafe, MoveSecondary = EnemyMovementType.ChargeLine,
                Hp = 260, Damage = 30, Defense = 9, Xp = 160, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_stabilized_blackstone", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Motivacao: evolucao final dos traidores do Veu; combo de montante-vazio aplica Blackstone Stain. Razao no pack: elite duelista que circula e carrega em alternancia. Fraqueza: radiante e a unica fraqueza — impede o combo apos hit.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_sealed_observer", DisplayName = "Sealed Observer",
                Band = 7, MinLevel = 86, MaxLevel = 94, Family = "Construct",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Guard,
                MovePrimary = EnemyMovementType.GuardStationary, MoveSecondary = EnemyMovementType.ProtectAnchor,
                Hp = 300, Damage = 22, Defense = 10, Xp = 120, SpoilerTier = 1,
                IsNonAggressive = true,
                PrimaryDropItemId = "item_material_stabilized_blackstone", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_construct",
                Notes = "[NAO-AGRESSIVA] Motivacao: vigia feito por Anya que ainda mantem o posto; pulso de julgamento poupa quem nao perturba o arquivo. Razao no pack: guarda neutro/condicional; imune a corrupcao; ataca so se agredido ou se jogador porta item corrupto. Fraqueza: sem fraqueza elementar; vulneravel apenas a armas de Anya (lore — F05 dormante).",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_dread_chorister", DisplayName = "Dread Chorister",
                Band = 7, MinLevel = 87, MaxLevel = 95, Family = "Undead",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.FloatingOrbit, MoveSecondary = EnemyMovementType.CasterKeepAway,
                Hp = 150, Damage = 20, Defense = 3, Xp = 130, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_night_essence", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_undead",
                Notes = "Motivacao: canta a litania de Veyraath; hino do medo aplica oscilacao de HUD (simulando medo). Razao no pack: conjurador de apoio de packs do vazio; resiste silencio mas sofre dano triplo de radiante. Fraqueza: radiante — unica escola de dano real.",
            };
        }
    }
}
