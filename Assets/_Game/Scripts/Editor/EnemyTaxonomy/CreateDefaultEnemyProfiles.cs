using UnityEngine;
using UnityEditor;

namespace CindarsHope.Editor.EnemyTaxonomy
{
    /// <summary>
    /// Creates default ScriptableObject assets for SPEC 13A: factions, size profiles,
    /// movement profiles and vulnerability profiles.
    /// Run via: CindarsHope > SPEC 13 > Create Default Enemy Profiles
    /// </summary>
    public static class CreateDefaultEnemyProfiles
    {
        private const string FactionsPath      = "Assets/_Game/Data/Enemies/Factions";
        private const string SizeProfilesPath  = "Assets/_Game/Data/Enemies/SizeProfiles";
        private const string MovementPath      = "Assets/_Game/Data/Enemies/MovementProfiles";
        private const string VulnPath          = "Assets/_Game/Data/Enemies/VulnerabilityProfiles";

        public static void CreateAll()
        {
            EnsureDirectory(FactionsPath);
            EnsureDirectory(SizeProfilesPath);
            EnsureDirectory(MovementPath);
            EnsureDirectory(VulnPath);

            CreateFactions();
            CreateSizeProfiles();
            CreateMovementProfiles();
            CreateVulnerabilityProfiles();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SPEC 13A] Default enemy profiles created successfully.");
        }

        // ──────────────────────────────────────────────────────────────────────
        // Factions (16 technical factions)
        // ──────────────────────────────────────────────────────────────────────
        private static void CreateFactions()
        {
            CreateFaction("faction_beast",     "Beast",       "Wild predatory animals and magical beasts native to Dornecia.",          new Color(0.60f, 0.35f, 0.10f));
            CreateFaction("faction_fungal",    "Fungal",      "Spore-colonies and mycelium-infused creatures from deep cave biomes.",    new Color(0.45f, 0.75f, 0.35f));
            CreateFaction("faction_goblin",    "Goblin",      "Scavenging goblinoids who raid surface settlements and cave entrances.",  new Color(0.30f, 0.65f, 0.20f));
            CreateFaction("faction_kobold",    "Kobold",      "Cunning trap-setters who guard deep veins of ore and gem caches.",       new Color(0.65f, 0.40f, 0.15f));
            CreateFaction("faction_orc",       "Orc",         "Warband raiders with strict hierarchy; hostile to most surface folk.",   new Color(0.55f, 0.25f, 0.10f));
            CreateFaction("faction_duergar",   "Duergar",     "Grim deep-dwarves who enslave other cave-dwellers for labour.",          new Color(0.40f, 0.40f, 0.45f));
            CreateFaction("faction_drow",      "Drow",        "Matriarchal spider-kin from the deepest cave bands.",                    new Color(0.20f, 0.10f, 0.35f));
            CreateFaction("faction_gnome",     "Gnome",       "Mechanical tinkerers; neutral until provoked or their traps triggered.", new Color(0.60f, 0.80f, 0.55f));
            CreateFaction("faction_ninrorin",  "Ninrorin",    "Planar refugees anchored to Vaalara by Dornecian ley lines.",            new Color(0.70f, 0.50f, 0.85f));
            CreateFaction("faction_undead",    "Undead",      "Animated remains reactivated by cave energy or necromantic residue.",    new Color(0.75f, 0.75f, 0.80f));
            CreateFaction("faction_cultist",   "Cultist",     "Human and humanoid zealots serving dark entities beneath Dornecia.",     new Color(0.25f, 0.10f, 0.10f));
            CreateFaction("faction_elemental", "Elemental",   "Manifestations of cave element energy: stone, magma and void.",          new Color(0.85f, 0.55f, 0.15f));
            CreateFaction("faction_construct", "Construct",   "Autonomous golems and mechanical sentinels left by lost civilisations.",  new Color(0.50f, 0.55f, 0.60f));
            CreateFaction("faction_abyssal",   "Abyssal",     "Entities from the abyss bleeding through fractured cave walls.",         new Color(0.40f, 0.05f, 0.50f));
            CreateFaction("faction_corrupted", "Corrupted",   "Once-normal creatures warped by Dornecian corruption energy.",           new Color(0.20f, 0.55f, 0.20f));
            CreateFaction("faction_draconic",  "Draconic",    "Distant kin of dragons; territorial and hoard-protective.",              new Color(0.85f, 0.70f, 0.10f));
        }

        private static void CreateFaction(string id, string displayName, string description, Color color)
        {
            var path = $"{FactionsPath}/{id}.asset";
            if (AssetDatabase.LoadAssetAtPath<Combat.EnemyFactionSO>(path) != null) return;

            var asset = ScriptableObject.CreateInstance<Combat.EnemyFactionSO>();
            asset.factionId   = id;
            asset.DisplayName = displayName;
            asset.Description = description;
            asset.FactionColor = color;
            AssetDatabase.CreateAsset(asset, path);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Size profiles (6 classes)
        // ──────────────────────────────────────────────────────────────────────
        private static void CreateSizeProfiles()
        {
            CreateSize("size_tiny",   Combat.EnemySizeClass.Tiny,   0.65f, 0.22f, 1,  0.20f, 0.45f, 1.30f, 4);
            CreateSize("size_small",  Combat.EnemySizeClass.Small,  0.85f, 0.32f, 1,  0.30f, 0.65f, 1.10f, 5);
            CreateSize("size_medium", Combat.EnemySizeClass.Medium, 1.00f, 0.45f, 1,  0.45f, 0.90f, 1.00f, 6);
            CreateSize("size_large",  Combat.EnemySizeClass.Large,  1.35f, 0.65f, 2,  0.65f, 1.25f, 0.75f, 8);
            CreateSize("size_huge",   Combat.EnemySizeClass.Huge,   1.80f, 0.95f, 3,  0.95f, 1.70f, 0.50f, 12);
            CreateSize("size_boss",   Combat.EnemySizeClass.Boss,   2.20f, 1.20f, 4,  1.20f, 2.10f, 0.35f, 16);
        }

        private static void CreateSize(string id, Combat.EnemySizeClass cls,
            float spriteScale, float colRadius, int footprint,
            float pathRadius, float dmgOffsetY, float knockbackMult, int minRoom)
        {
            var path = $"{SizeProfilesPath}/{id}.asset";
            if (AssetDatabase.LoadAssetAtPath<Combat.EnemySizeProfileSO>(path) != null) return;

            var asset = ScriptableObject.CreateInstance<Combat.EnemySizeProfileSO>();
            asset.SizeProfileId      = id;
            asset.SizeClass          = cls;
            asset.SpriteScale        = spriteScale;
            asset.ColliderRadius     = colRadius;
            asset.FootprintCells     = footprint;
            asset.PathingRadius      = pathRadius;
            asset.DamageNumberOffset = new Vector2(0f, dmgOffsetY);
            asset.KnockbackMultiplier = knockbackMult;
            asset.MinimumRoomSize    = minRoom;
            AssetDatabase.CreateAsset(asset, path);
        }

        // ──────────────────────────────────────────────────────────────────────
        // MoveSpeed tiers (tuning 2026-06-24 — reduzidos para ritmo jogavel)
        // LENTO  : 0.9  — tanks, sentinelas, bosses de arena, iscas, flutuantes lentos, mimicos
        // NORMAL : 1.4  — chasers base, patrulhas, kite, casters, lideres, boss phase
        // RAPIDO : 2.0  — swarm, leaper, blinker, flanker, orbit, charge, strafe, retreat
        // Razao: 1.4 tiles/s e ritmo de perseguicao confortavel; 2.0 ja parece rapido para mobs ageis.
        // Valores anteriores: 1.3 / 2.0 / 2.8 (tuning 2026-06).
        // ──────────────────────────────────────────────────────────────────────
        private const float SpeedSlow   = 0.9f;
        private const float SpeedNormal = 1.4f;
        private const float SpeedFast   = 2.0f;

        // ──────────────────────────────────────────────────────────────────────
        // Movement profiles — 10 originais (fable_13) + 12 novos (fable_24)
        // UpsertMovement: cria o asset se ausente; atualiza campos se já existe.
        // ──────────────────────────────────────────────────────────────────────
        private static void CreateMovementProfiles()
        {
            // ── fable_13: 10 originais ───────────────────────────────────────
            // Kiting corrigido (TUNING 2026-06):
            //   kite_ranged:     PreferredDistance 5.5 -> 3.5  (caves estreitas)
            //   caster_keep_away: PreferredDistance 6.0 -> 4.0 (caves estreitas)
            // id, type, speed, detect, leash, attackRange, preferred, wander, tick,
            //   burrow, blink, leap, fly, repositionDash
            UpsertMovement("movement_ground_chase",     Combat.EnemyMovementType.GroundChase,     SpeedNormal, 9f,  28f, 1.2f, 0.9f, 4f, 0.25f, false, false, false, false, false);
            UpsertMovement("movement_ground_patrol",    Combat.EnemyMovementType.GroundPatrol,    SpeedNormal, 7f,  20f, 1.2f, 0.9f, 8f, 0.35f, false, false, false, false, false);
            UpsertMovement("movement_guard_stationary", Combat.EnemyMovementType.GuardStationary, SpeedSlow,   6f,  10f, 1.5f, 1.0f, 1f, 0.40f, false, false, false, false, false);
            UpsertMovement("movement_kite_ranged",      Combat.EnemyMovementType.KiteRanged,      SpeedNormal, 10f, 25f, 7.0f, 3.5f, 5f, 0.30f, false, false, false, false, false); // preferred: 5.5->3.5
            UpsertMovement("movement_caster_keep_away", Combat.EnemyMovementType.CasterKeepAway,  SpeedNormal, 9f,  22f, 8.0f, 4.0f, 4f, 0.40f, false, false, false, false, false); // preferred: 6.0->4.0
            UpsertMovement("movement_burrow_ambush",    Combat.EnemyMovementType.BurrowAmbush,    SpeedNormal, 8f,  20f, 1.0f, 0.5f, 3f, 0.30f, true,  false, false, false, false);
            // SPEC 14A-FIX7: 3.0 -> 2.2 (Tiny swarm impossiveis de acertar); SpeedFast mantém diferenciacao
            UpsertMovement("movement_swarm_erratic",    Combat.EnemyMovementType.SwarmErratic,    SpeedFast,   7f,  18f, 0.8f, 0.4f, 3f, 0.20f, false, false, false, false, false);
            UpsertMovement("movement_tank_slow_push",   Combat.EnemyMovementType.TankSlowPush,    SpeedSlow,   8f,  20f, 1.8f, 1.2f, 2f, 0.45f, false, false, false, false, false);
            UpsertMovement("movement_phase_short_blink",Combat.EnemyMovementType.PhaseShortBlink, SpeedFast,   10f, 24f, 1.2f, 0.9f, 4f, 0.30f, false, true,  false, false, true);  // blink + repositionDash
            UpsertMovement("movement_leaper",           Combat.EnemyMovementType.Leaper,          SpeedFast,   9f,  22f, 3.5f, 2.5f, 5f, 0.30f, false, false, true,  false, false);

            // ── fable_24: 12 novos arquétipos ───────────────────────────────
            // PackFlanker: flanqueador rapido, abre espaço para o lider
            UpsertMovement("movement_pack_flanker",      Combat.EnemyMovementType.PackFlanker,      SpeedFast,   10f, 25f, 1.2f, 1.0f, 5f, 0.25f, false, false, false, false, true);  // repositionDash: reposiciona p/ flanquear
            // PackLeader: lider de bando, movimento normal mas coordena
            UpsertMovement("movement_pack_leader",       Combat.EnemyMovementType.PackLeader,       SpeedNormal, 10f, 28f, 1.5f, 1.0f, 4f, 0.30f, false, false, false, false, false);
            // RetreatAndCall: recua, chama reforcos, depois volta — agil
            UpsertMovement("movement_retreat_and_call",  Combat.EnemyMovementType.RetreatAndCall,   SpeedFast,   9f,  22f, 1.2f, 4.0f, 6f, 0.25f, false, false, false, false, false);
            // FloatingSlow: voador lento, flutua e drena — CanFly obrigatorio
            UpsertMovement("movement_floating_slow",     Combat.EnemyMovementType.FloatingSlow,     SpeedSlow,   10f, 25f, 2.0f, 2.5f, 4f, 0.35f, false, false, false, true,  false);
            // FloatingOrbit: voador agil que orbita o alvo — CanFly obrigatorio
            UpsertMovement("movement_floating_orbit",    Combat.EnemyMovementType.FloatingOrbit,    SpeedFast,   10f, 25f, 2.5f, 3.0f, 4f, 0.25f, false, false, false, true,  false);
            // CircleStrafe: combat strafe, reposiciona constantemente — assassino
            UpsertMovement("movement_circle_strafe",     Combat.EnemyMovementType.CircleStrafe,     SpeedFast,   9f,  22f, 2.0f, 2.5f, 3f, 0.20f, false, false, false, false, true);  // repositionDash
            // ChargeLine: base normal; investida multiplica velocidade no EnemyBrain
            UpsertMovement("movement_charge_line",       Combat.EnemyMovementType.ChargeLine,       SpeedNormal, 10f, 28f, 1.5f, 3.0f, 3f, 0.35f, false, false, false, false, false);
            // TreasureIdleAmbush: mimico parado ate ser ativado
            UpsertMovement("movement_treasure_idle_ambush", Combat.EnemyMovementType.TreasureIdleAmbush, SpeedSlow, 8f, 20f, 1.5f, 0.5f, 1f, 0.45f, false, false, false, false, false);
            // HazardLure: isca que atrai antes de atacar
            UpsertMovement("movement_hazard_lure",       Combat.EnemyMovementType.HazardLure,       SpeedNormal, 12f, 25f, 2.0f, 1.5f, 3f, 0.40f, false, false, false, false, false);
            // ProtectAnchor: sentinela lento que guarda um ponto fixo
            UpsertMovement("movement_protect_anchor",    Combat.EnemyMovementType.ProtectAnchor,    SpeedSlow,   8f,  15f, 1.8f, 1.0f, 2f, 0.45f, false, false, false, false, false);
            // BossArenaControl: boss lento que controla o espaco da arena
            UpsertMovement("movement_boss_arena_control",Combat.EnemyMovementType.BossArenaControl, SpeedSlow,   15f, 35f, 2.5f, 3.0f, 5f, 0.50f, false, false, false, false, false);
            // BossPhaseShift: boss que muda de fase, velocidade normal base
            UpsertMovement("movement_boss_phase_shift",  Combat.EnemyMovementType.BossPhaseShift,   SpeedNormal, 15f, 35f, 2.0f, 2.5f, 5f, 0.40f, false, false, false, false, false);
        }

        /// <summary>
        /// Cria o asset de MovementProfile se nao existir; atualiza todos os campos se ja existir.
        /// Garante que regeneracoes do gerador sempre apliquem os valores do codigo (upsert idempotente).
        /// </summary>
        private static void UpsertMovement(string id, Combat.EnemyMovementType type,
            float speed, float detect, float leash, float attackRange,
            float preferred, float wander, float tick,
            bool burrow, bool blink, bool leap, bool fly, bool repositionDash)
        {
            var path  = $"{MovementPath}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<Combat.EnemyMovementProfileSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<Combat.EnemyMovementProfileSO>();
                AssetDatabase.CreateAsset(asset, path);
            }

            // Upsert via SerializedObject para respeitar undo/dirty corretamente
            var so = new SerializedObject(asset);
            so.FindProperty("MovementProfileId").stringValue   = id;
            so.FindProperty("MovementType").enumValueIndex     = (int)type;
            so.FindProperty("MoveSpeed").floatValue            = speed;
            so.FindProperty("DetectionRange").floatValue       = detect;
            so.FindProperty("LeashRange").floatValue           = leash;
            so.FindProperty("AttackRange").floatValue          = attackRange;
            so.FindProperty("PreferredDistance").floatValue    = preferred;
            so.FindProperty("WanderRadius").floatValue         = wander;
            so.FindProperty("DecisionTickSeconds").floatValue  = tick;
            so.FindProperty("CanBurrow").boolValue             = burrow;
            so.FindProperty("CanPhaseShortBlink").boolValue    = blink;
            so.FindProperty("CanLeap").boolValue               = leap;
            so.FindProperty("CanFly").boolValue                = fly;
            so.FindProperty("RepositionDashEnabled").boolValue = repositionDash;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Vulnerability profiles (10 canonical profiles)
        // ──────────────────────────────────────────────────────────────────────
        private static void CreateVulnerabilityProfiles()
        {
            // id, triggerMode, windowSec, multiplier, cooldownSec
            CreateVuln("vuln_swarm_after_bite",     Combat.VulnerabilityTriggerMode.AfterAttackRecover,    0.8f, 1.4f,  8f);
            CreateVuln("vuln_chaser_charge",        Combat.VulnerabilityTriggerMode.DuringChargeWindup,    1.0f, 1.6f, 10f);
            CreateVuln("vuln_ranged_after_volley",  Combat.VulnerabilityTriggerMode.AfterProjectileVolley, 1.2f, 1.5f, 12f);
            CreateVuln("vuln_caster_after_cast",    Combat.VulnerabilityTriggerMode.AfterCast,             1.5f, 1.7f, 14f);
            CreateVuln("vuln_burrow_emerge",        Combat.VulnerabilityTriggerMode.AfterBurrowEmerges,    1.0f, 1.8f, 12f);
            CreateVuln("vuln_guard_shield_drop",    Combat.VulnerabilityTriggerMode.AfterShieldDrop,       1.3f, 1.6f, 15f);
            CreateVuln("vuln_tank_recover",         Combat.VulnerabilityTriggerMode.AfterAttackRecover,    1.5f, 1.5f, 16f);
            CreateVuln("vuln_phase_arrival",        Combat.VulnerabilityTriggerMode.AfterBlinkArrival,     0.9f, 1.7f, 10f);
            CreateVuln("vuln_leaper_landing",       Combat.VulnerabilityTriggerMode.AfterAttackRecover,    0.7f, 1.6f,  9f);
            CreateVuln("vuln_corrupted_enrage_pulse",Combat.VulnerabilityTriggerMode.AfterEnragePulse,     2.0f, 1.9f, 20f);
        }

        private static void CreateVuln(string id, Combat.VulnerabilityTriggerMode trigger,
            float window, float multiplier, float cooldown)
        {
            var path = $"{VulnPath}/{id}.asset";
            if (AssetDatabase.LoadAssetAtPath<Combat.EnemyVulnerabilityProfileSO>(path) != null) return;

            var asset = ScriptableObject.CreateInstance<Combat.EnemyVulnerabilityProfileSO>();
            asset.VulnerabilityProfileId = id;
            asset.TriggerMode            = trigger;
            asset.WindowDurationSeconds  = window;
            asset.Multiplier             = multiplier;
            asset.CooldownSeconds        = cooldown;
            AssetDatabase.CreateAsset(asset, path);
        }

        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parts = path.Split('/');
                var current = parts[0];
                for (var i = 1; i < parts.Length; i++)
                {
                    var next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
        }
    }
}
