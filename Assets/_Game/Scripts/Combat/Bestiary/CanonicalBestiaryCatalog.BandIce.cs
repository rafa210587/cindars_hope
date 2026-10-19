using System.Collections.Generic;
using CindarsHope.Combat;

namespace CindarsHope.Combat.Bestiary
{
    // fable_33 — Band 3 ICE (levels 26-40). Numbers from CAVE_BESTIARY_CATALOG §11/§12.
    public static partial class CanonicalBestiaryCatalog
    {
        private static IEnumerable<BestiaryCreatureDef> BandIce()
        {
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_undead_shambler", DisplayName = "Undead Shambler",
                Band = 3, MinLevel = 26, MaxLevel = 33, Family = "Undead",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Tank,
                MovePrimary = EnemyMovementType.TankSlowPush, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 60, Damage = 10, Defense = 2, Xp = 30, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_rot_gland", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_undead",
                Notes = "Nunca recua/teme; levanta 1x a 25% HP a menos que queimado. Fraco Fire/Silver/Radiant; imune Poison/Bleed/Fear; Ice -50%. Drops: bone, grave dust.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_frost_wisp", DisplayName = "Frost Wisp",
                Band = 3, MinLevel = 26, MaxLevel = 33, Family = "Elemental",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.FloatingSlow, MoveSecondary = EnemyMovementType.FloatingOrbit,
                Hp = 30, Damage = 9, Defense = 0, Xp = 28, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_frost_core", PrimaryDamageTypeId = "ice",
                VulnerabilityMatrixProfileId = "vulnmatrix_elemental",
                Notes = "Chill Touch (Chill 1.5s); aura fria (stamina regen -20% raio 2); persegue o jogador com MENOS stamina; atravessa paredes finas. Fraco Fire x2; imune Ice/Chill. Drops: frost core, essence_ice.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_veilkin_skirmisher", DisplayName = "Veilkin Skirmisher",
                Band = 3, MinLevel = 26, MaxLevel = 32, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.CircleStrafe, MoveSecondary = EnemyMovementType.PhaseShortBlink,
                Hp = 52, Damage = 12, Defense = 3, Xp = 36, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_fiber", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "RENOMEADO para Veilkin. Veil Step (blink 2 tiles); orbita e pica; blinka ao ser focado. Fraco Light/Fire; resiste Shadow. Drops: veil cloth, silvered dagger part.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_mirrorfin_shoal", DisplayName = "Mirrorfin Shoal",
                Band = 3, MinLevel = 27, MaxLevel = 34, Family = "Beast",
                Size = BestiarySizeClass.Tiny, Role = EnemyRole.Swarm,
                MovePrimary = EnemyMovementType.SwarmErratic, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 12, Damage = 3, Defense = 0, Xp = 4, SpoilerTier = 0,
                IsAquatic = true,
                PrimaryDropItemId = "item_fish_mirrorfin", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_beast",
                Notes = "[AQUATICA] Cardume de 6 (stats por peixe); Frenzy Nibble (Bleed leve); atraidos por Bleed no jogador; inofensivos fora d'agua. Drops: mirrorfin (peixe raro).",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_orc_berserker", DisplayName = "Orc Berserker",
                Band = 3, MinLevel = 27, MaxLevel = 34, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.GroundChase, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 70, Damage = 15, Defense = 1, Xp = 40, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_iron_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Frenzy Chain (3 golpes); <30% HP ignora posture (tell: olhos brilham); foca quem o feriu. Fraco Ice/Chill; nunca bloqueia. Drops: tusk, rage gland.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_gravedelver_warder", DisplayName = "Gravedelver Warder",
                Band = 3, MinLevel = 28, MaxLevel = 35, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Guard,
                MovePrimary = EnemyMovementType.GuardStationary, MoveSecondary = EnemyMovementType.TankSlowPush,
                Hp = 80, Damage = 11, Defense = 5, Xp = 42, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_iron_ore", PrimaryDamageTypeId = "physical",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "RENOMEADO para Gravedelver. Hold the Line (bloqueio frontal 70%); ancora corredores; avanca 1 tile/3s. Exige GuardBreak ou flanqueio. Drops: tower shield part, iron.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_cultist_zealot", DisplayName = "Cultist Zealot",
                Band = 3, MinLevel = 29, MaxLevel = 36, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.CasterKeepAway, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 56, Damage = 13, Defense = 2, Xp = 40, SpoilerTier = 0,
                PrimaryDropItemId = "item_blackstone_corrupted_shard", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Blackstone Bolt (Corruption 10%); Dark Ward (escudo 20HP aliado); sacrifica Shamblers p/ curar (tell 1.2s). Fraco interrupt; Radiant x1.5. Drops: blackstone shard, ritual knife.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_rime_stalker", DisplayName = "Rime Stalker",
                Band = 3, MinLevel = 30, MaxLevel = 38, Family = "Beast",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.CircleStrafe, MoveSecondary = EnemyMovementType.Leaper,
                Hp = 58, Damage = 14, Defense = 2, Xp = 44, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_white_pelt", PrimaryDamageTypeId = "ice",
                VulnerabilityMatrixProfileId = "vulnmatrix_beast",
                Notes = "Frostbite Lunge (Chill 25%) so pelo flanco/costas; caca em par espelhado; se encarado, circula. Fraco Fire; resiste Ice. Drops: white pelt, fang.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_glacier_maw", DisplayName = "Glacier Maw",
                Band = 3, MinLevel = 36, MaxLevel = 40, Family = "Beast",
                Size = BestiarySizeClass.Large, Role = EnemyRole.MiniBoss,
                MovePrimary = EnemyMovementType.ChargeLine, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 280, Damage = 18, Defense = 4, Xp = 160, SpoilerTier = 2, IsMiniBoss = true,
                PrimaryDropItemId = "item_material_frost_core", PrimaryDamageTypeId = "ice",
                VulnerabilityMatrixProfileId = "vulnmatrix_beast",
                Notes = "Miniboss. Avalanche Charge (tell 1.2s, derruba pilares hazard); blindado; janela 3s ao bater na parede. Drops: glacier hide, essence_ice x2.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_veilkin_witch", DisplayName = "Veilkin Witch",
                Band = 3, MinLevel = 38, MaxLevel = 40, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.MiniBoss,
                MovePrimary = EnemyMovementType.CasterKeepAway, MoveSecondary = EnemyMovementType.PhaseShortBlink,
                Hp = 240, Damage = 16, Defense = 3, Xp = 150, SpoilerTier = 2, IsMiniBoss = true,
                PrimaryDropItemId = "item_blackstone_corrupted_shard", PrimaryDamageTypeId = "arcane",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "RENOMEADA para Veilkin. Miniboss. Mirror Veil (2 copias 1HP); Hex (-15% dano jogador); janela 2.5s quando Mirror Veil quebra. Drops: veil grimoire (scrolls Ozzra), essence_ice.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_rimelock_colossus", DisplayName = "Rimelock Colossus",
                Band = 3, MinLevel = 30, MaxLevel = 30, Family = "Elemental",
                Size = BestiarySizeClass.Huge, Role = EnemyRole.Boss,
                MovePrimary = EnemyMovementType.BossArenaControl, MoveSecondary = EnemyMovementType.BossPhaseShift,
                Hp = 700, Damage = 16, Defense = 6, Xp = 420, SpoilerTier = 3, IsBoss = true,
                PrimaryDropItemId = "item_material_frost_core", PrimaryDamageTypeId = "ice",
                VulnerabilityMatrixProfileId = "vulnmatrix_elemental",
                Notes = "Boss gate 30. F1 punhos + pilares; F2 (66%) IceSlick global + 2 Frost Wisps; F3 (33%) pilar quebrado expoe nucleo (Hammer/charged +50%). Fraco Fire; imune Ice/Chill/Stun. Mecanicas de fase = F05 (dormante). Drops first-kill: receita Frost Oil + Anel de Brasa.",
            };

            // ── fable_80: +6 Ice ────────────────────────────────────────────────────────────
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_frostshard_wisp", DisplayName = "Frostshard Wisp",
                Band = 3, MinLevel = 26, MaxLevel = 32, Family = "Elemental",
                Size = BestiarySizeClass.Tiny, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.FloatingOrbit, MoveSecondary = EnemyMovementType.FloatingSlow,
                Hp = 18, Damage = 5, Defense = 0, Xp = 22, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_frost_core", PrimaryDamageTypeId = "ice",
                VulnerabilityMatrixProfileId = "vulnmatrix_elemental",
                Notes = "Motivacao: estilhaco da vontade do Rimelock; orbita e pune stamina. Razao no pack: orbitador de apoio que aplica dreno de stamina via raio de frio. Fraqueza: fogo dissolve em 2 hits; melee nao alcanca enquanto orbita.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_crystal_hound", DisplayName = "Crystal Hound",
                Band = 3, MinLevel = 28, MaxLevel = 36, Family = "Beast",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.PackLeader, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 58, Damage = 9, Defense = 2, Xp = 40, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_frost_core", PrimaryDamageTypeId = "ice",
                VulnerabilityMatrixProfileId = "vulnmatrix_beast",
                Notes = "Motivacao: caca em pares a servico do culto frio; lider inato do bando de feras de gelo. Razao no pack: PackLeader de feras; bote de presa-de-gelo aplica slow 1s. Fraqueza: contundente quebra a armadura cristalina.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_veilkin_iceblade", DisplayName = "Veilkin Iceblade",
                Band = 3, MinLevel = 29, MaxLevel = 37, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Chaser,
                MovePrimary = EnemyMovementType.CircleStrafe, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 52, Damage = 10, Defense = 3, Xp = 44, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_frost_core", PrimaryDamageTypeId = "ice",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Motivacao: duelista do Veu a servico do culto frio; usa ripostes apos bloqueio. Razao no pack: flanqueador tecnico que circula enquanto lider ataca de frente. Fraqueza: fogo cancela riposte window.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_coldcult_preacher", DisplayName = "Cold Cult Preacher",
                Band = 3, MinLevel = 30, MaxLevel = 38, Family = "Humanoid",
                Size = BestiarySizeClass.Medium, Role = EnemyRole.Caster,
                MovePrimary = EnemyMovementType.CasterKeepAway, MoveSecondary = EnemyMovementType.RetreatAndCall,
                Hp = 42, Damage = 9, Defense = 1, Xp = 48, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_frost_core", PrimaryDamageTypeId = "ice",
                VulnerabilityMatrixProfileId = "vulnmatrix_humanoid",
                Notes = "Motivacao: converte os perdidos a fome de Husinord; envia nova de frio e canto de chill. Razao no pack: conjurador-lider do culto que chama reforco ao fugir. Fraqueza: silencio interrompe o canto de chill e o buff do pack.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_frostbound_revenant", DisplayName = "Frostbound Revenant",
                Band = 3, MinLevel = 32, MaxLevel = 40, Family = "Undead",
                Size = BestiarySizeClass.Large, Role = EnemyRole.Tank,
                MovePrimary = EnemyMovementType.TankSlowPush, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 110, Damage = 12, Defense = 4, Xp = 60, SpoilerTier = 1,
                PrimaryDropItemId = "item_material_rot_gland", PrimaryDamageTypeId = "ice",
                VulnerabilityMatrixProfileId = "vulnmatrix_undead",
                Notes = "Motivacao: mineiros congelados em pleno labor que nunca param; talho congelado e ergue-se uma vez (50% HP). Razao no pack: tanque que regressa, nucleo de packs mortos. Fraqueza: fogo/radiante impede a ressurreicao.",
            };
            yield return new BestiaryCreatureDef
            {
                EnemyId = "enemy_glacier_tick", DisplayName = "Glacier Tick",
                Band = 3, MinLevel = 26, MaxLevel = 34, Family = "Insect",
                Size = BestiarySizeClass.Small, Role = EnemyRole.Swarm,
                MovePrimary = EnemyMovementType.SwarmErratic, MoveSecondary = EnemyMovementType.GroundChase,
                Hp = 14, Damage = 4, Defense = 0, Xp = 16, SpoilerTier = 0,
                PrimaryDropItemId = "item_material_chitin", PrimaryDamageTypeId = "ice",
                VulnerabilityMatrixProfileId = "vulnmatrix_insect",
                Notes = "Motivacao: parasita de enxame da banda de gelo; agarra e drena frostbite. Razao no pack: fodder que cobre conjuradores; drena stamina ao agarrar. Fraqueza: fogo mata grupo inteiro em AoE.",
            };
        }
    }
}
