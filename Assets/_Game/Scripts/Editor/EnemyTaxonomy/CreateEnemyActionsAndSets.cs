using System.Collections.Generic;
using CindarsHope.Combat;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.EnemyTaxonomy
{
    /// <summary>
    /// SPEC 13C — Creates EnemyTelegraphProfileSO, EnemyActionSO and EnemyActionSetSO assets
    /// for the 40-enemy canonical roster.
    /// Run via: CindarsHope > SPEC 13 > Create Enemy Actions and Sets
    /// Idempotent: skips assets that already exist at the target path.
    /// </summary>
    public static class CreateEnemyActionsAndSets
    {
        private const string TelegraphFolder  = "Assets/_Game/Data/Enemies/TelegraphProfiles";
        private const string ActionsFolder    = "Assets/_Game/Data/Enemies/Actions";
        private const string ActionSetsFolder = "Assets/_Game/Data/Enemies/ActionSets";

        // ── Data structures ──────────────────────────────────────────────────────

        private struct TelegraphEntry
        {
            public string Id;
            public Color  BlinkColor;
            public float  BlinkFrequency;
            public float  WindupSeconds;
        }

        private struct ActionEntry
        {
            public string          Id, DisplayName;
            public EnemyActionType ActionType;
            public string          DamageTypeId;
            public int             BaseDamage;
            public float           Range, AreaRadius, MinRange;
            public float           Cooldown, Windup, Recover;
            public float           ProjectileSpeed;
            public string[]        StatusIds;
            public float           StatusChance;
            public string          TelegraphId;
            public bool            TriggersVuln;
            public VulnerabilityTriggerMode VulnTrigger;
            public bool            IsInterruptible;
            public bool            RequiresLos;
            public int             MaxTargets;
        }

        private struct ActionSetEntry
        {
            public string   Id, DisplayName, FallbackId;
            public string[] ActionIds, RoleTags;
            public string   Notes;
        }

        // ── Entry point ──────────────────────────────────────────────────────────

        [MenuItem("CindarsHope/Archive/SPEC 13/Create Enemy Actions and Sets")]
        public static void CreateAll()
        {
            EnsureFolder("Assets/_Game/Data/Enemies");
            EnsureFolder(TelegraphFolder);
            EnsureFolder(ActionsFolder);
            EnsureFolder(ActionSetsFolder);

            int created = 0, skipped = 0;

            foreach (var t in BuildTelegraphProfiles())
            {
                string path = $"{TelegraphFolder}/{t.Id}.asset";
                if (AssetDatabase.LoadAssetAtPath<EnemyTelegraphProfileSO>(path) != null) { skipped++; continue; }
                var so = ScriptableObject.CreateInstance<EnemyTelegraphProfileSO>();
                so.TelegraphProfileId = t.Id;
                so.BlinkColor         = t.BlinkColor;
                so.BlinkFrequency     = t.BlinkFrequency;
                so.WindupSeconds      = t.WindupSeconds;
                AssetDatabase.CreateAsset(so, path);
                created++;
            }

            foreach (var a in BuildActions())
            {
                string path = $"{ActionsFolder}/{a.Id}.asset";
                if (AssetDatabase.LoadAssetAtPath<EnemyActionSO>(path) != null) { skipped++; continue; }
                var so = ScriptableObject.CreateInstance<EnemyActionSO>();
                so.ActionId                  = a.Id;
                so.DisplayName               = a.DisplayName;
                so.ActionType                = a.ActionType;
                so.DamageType                = a.DamageTypeId;
                so.BaseDamage                = a.BaseDamage;
                so.Range                     = a.Range;
                so.AreaRadius                = a.AreaRadius;
                so.MinRange                  = a.MinRange;
                so.CooldownSeconds           = a.Cooldown;
                so.WindupSeconds             = a.Windup;
                so.RecoverSeconds            = a.Recover;
                so.ProjectileSpeed           = a.ProjectileSpeed;
                so.StatusApplicationIds      = a.StatusIds ?? new string[0];
                so.StatusApplyChance         = a.StatusChance > 0f ? a.StatusChance : 1.0f;
                so.TelegraphProfileId        = a.TelegraphId;
                so.TriggersVulnerabilityWindow  = a.TriggersVuln;
                so.VulnerabilityWindowTrigger   = a.VulnTrigger;
                so.IsInterruptible           = a.IsInterruptible;
                so.RequiresLineOfSight       = a.RequiresLos;
                so.MaxTargets               = a.MaxTargets > 0 ? a.MaxTargets : 1;
                AssetDatabase.CreateAsset(so, path);
                created++;
            }

            foreach (var s in BuildActionSets())
            {
                string path = $"{ActionSetsFolder}/{s.Id}.asset";
                if (AssetDatabase.LoadAssetAtPath<EnemyActionSetSO>(path) != null) { skipped++; continue; }
                var so = ScriptableObject.CreateInstance<EnemyActionSetSO>();
                so.ActionSetId      = s.Id;
                so.DisplayName      = s.DisplayName;
                so.ActionIds        = s.ActionIds ?? new string[0];
                so.FallbackActionId = s.FallbackId ?? (s.ActionIds?.Length > 0 ? s.ActionIds[0] : "");
                so.RoleTags         = s.RoleTags ?? new string[0];
                so.Notes            = s.Notes ?? "";
                AssetDatabase.CreateAsset(so, path);
                created++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SPEC 13C] Actions/ActionSets creation done. Created: {created}, Skipped (existing): {skipped}.");
        }

        // ── Telegraph profiles ───────────────────────────────────────────────────

        private static List<TelegraphEntry> BuildTelegraphProfiles() => new List<TelegraphEntry>
        {
            new TelegraphEntry { Id = "telegraph_fast_melee",       BlinkColor = new Color(1f, 0.9f, 0f),    BlinkFrequency = 0.08f, WindupSeconds = 0.3f  },
            new TelegraphEntry { Id = "telegraph_heavy_melee",      BlinkColor = new Color(1f, 0.4f, 0f),    BlinkFrequency = 0.15f, WindupSeconds = 0.6f  },
            new TelegraphEntry { Id = "telegraph_ranged_projectile",BlinkColor = new Color(0f, 0.9f, 1f),    BlinkFrequency = 0.10f, WindupSeconds = 0.4f  },
            new TelegraphEntry { Id = "telegraph_caster_spell",     BlinkColor = new Color(0.8f, 0f, 1f),    BlinkFrequency = 0.12f, WindupSeconds = 0.7f  },
            new TelegraphEntry { Id = "telegraph_area_pulse",       BlinkColor = new Color(1f, 0.3f, 0.1f),  BlinkFrequency = 0.10f, WindupSeconds = 0.5f  },
            new TelegraphEntry { Id = "telegraph_burrow_emerge",    BlinkColor = new Color(0.6f, 0.35f, 0.1f),BlinkFrequency= 0.08f, WindupSeconds = 0.35f },
            new TelegraphEntry { Id = "telegraph_leap",             BlinkColor = new Color(0.2f, 0.9f, 0.2f),BlinkFrequency = 0.08f, WindupSeconds = 0.4f  },
            new TelegraphEntry { Id = "telegraph_phase",            BlinkColor = new Color(0.5f, 0.8f, 1f),  BlinkFrequency = 0.06f, WindupSeconds = 0.3f  },
        };

        // ── Actions ──────────────────────────────────────────────────────────────

        private static List<ActionEntry> BuildActions()
        {
            var M = EnemyActionType.MeleeAttack;
            var R = EnemyActionType.RangedProjectile;
            var C = EnemyActionType.CastProjectile;
            var A = EnemyActionType.AreaPulse;
            var S = EnemyActionType.SelfBuff;
            var B = EnemyActionType.BurrowStrike;
            var L = EnemyActionType.LeapStrike;

            var TFM  = "telegraph_fast_melee";
            var THM  = "telegraph_heavy_melee";
            var TRP  = "telegraph_ranged_projectile";
            var TCS  = "telegraph_caster_spell";
            var TAP  = "telegraph_area_pulse";
            var TBE  = "telegraph_burrow_emerge";
            var TLP  = "telegraph_leap";
            var TPH  = "telegraph_phase";

            var AAR  = VulnerabilityTriggerMode.AfterAttackRecover;
            var DCW  = VulnerabilityTriggerMode.DuringChargeWindup;
            var ABE  = VulnerabilityTriggerMode.AfterBurrowEmerges;
            var AC   = VulnerabilityTriggerMode.AfterCast;
            var APV  = VulnerabilityTriggerMode.AfterProjectileVolley;
            var ASD  = VulnerabilityTriggerMode.AfterShieldDrop;
            var ABA  = VulnerabilityTriggerMode.AfterBlinkArrival;
            var AEP  = VulnerabilityTriggerMode.AfterEnragePulse;

            return new List<ActionEntry>
            {
                // ── BAND 1 ─────────────────────────────────────────────────────────

                // 1. enemy_cave_mite
                new ActionEntry { Id="action_cave_mite_bite", DisplayName="Bite",
                    ActionType=M, DamageTypeId="physical", BaseDamage=3,
                    Range=0.6f, Cooldown=1.5f, Windup=0.25f, Recover=0.3f,
                    TelegraphId=TFM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=true, MaxTargets=1 },

                // 2. enemy_stone_rat
                new ActionEntry { Id="action_stone_rat_lunge", DisplayName="Stone Lunge",
                    ActionType=M, DamageTypeId="physical", BaseDamage=4,
                    Range=1.0f, Cooldown=2.0f, Windup=0.4f, Recover=0.3f,
                    TelegraphId=TFM, TriggersVuln=true, VulnTrigger=DCW, IsInterruptible=true, MaxTargets=1 },

                // 3. enemy_cave_bat
                new ActionEntry { Id="action_cave_bat_screech_dart", DisplayName="Screech Dart",
                    ActionType=R, DamageTypeId="physical", BaseDamage=3,
                    Range=3.0f, Cooldown=2.0f, Windup=0.35f, Recover=0.3f, ProjectileSpeed=5.0f,
                    StatusIds=new[]{"status_bleed_minor"}, StatusChance=0.6f,
                    TelegraphId=TRP, TriggersVuln=true, VulnTrigger=APV, IsInterruptible=true, MaxTargets=1 },

                // 4. enemy_goblin_grashnaar_scavenger
                new ActionEntry { Id="action_grashnaar_throw_scrap", DisplayName="Throw Scrap",
                    ActionType=R, DamageTypeId="physical", BaseDamage=4,
                    Range=3.0f, Cooldown=2.5f, Windup=0.4f, Recover=0.4f, ProjectileSpeed=4.5f,
                    TelegraphId=TRP, TriggersVuln=false, IsInterruptible=true, MaxTargets=1 },
                new ActionEntry { Id="action_grashnaar_knife_jab", DisplayName="Knife Jab",
                    ActionType=M, DamageTypeId="physical", BaseDamage=5,
                    Range=0.8f, Cooldown=1.8f, Windup=0.3f, Recover=0.3f,
                    TelegraphId=TFM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=true, MaxTargets=1 },

                // 5. enemy_kobold_scout
                new ActionEntry { Id="action_kobold_pebble_shot", DisplayName="Pebble Shot",
                    ActionType=R, DamageTypeId="physical", BaseDamage=3,
                    Range=3.5f, Cooldown=2.0f, Windup=0.35f, Recover=0.3f, ProjectileSpeed=5.0f,
                    TelegraphId=TRP, TriggersVuln=true, VulnTrigger=APV, IsInterruptible=true, MaxTargets=1 },
                new ActionEntry { Id="action_kobold_quick_stab", DisplayName="Quick Stab",
                    ActionType=M, DamageTypeId="physical", BaseDamage=4,
                    Range=0.7f, Cooldown=1.5f, Windup=0.2f, Recover=0.25f,
                    TelegraphId=TFM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=true, MaxTargets=1 },

                // 6. enemy_mossling
                new ActionEntry { Id="action_mossling_spore_puff", DisplayName="Spore Puff",
                    ActionType=A, DamageTypeId="toxic", BaseDamage=4,
                    Range=1.5f, AreaRadius=1.0f, Cooldown=3.0f, Windup=0.5f, Recover=0.5f,
                    StatusIds=new[]{"status_poison_minor"}, StatusChance=0.8f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=false, MaxTargets=3 },
                new ActionEntry { Id="action_mossling_body_bump", DisplayName="Body Bump",
                    ActionType=M, DamageTypeId="physical", BaseDamage=3,
                    Range=0.7f, Cooldown=2.0f, Windup=0.35f, Recover=0.35f,
                    TelegraphId=TFM, TriggersVuln=false, IsInterruptible=true, MaxTargets=1 },

                // 7. enemy_cracked_bone
                new ActionEntry { Id="action_cracked_bone_bone_swing", DisplayName="Bone Swing",
                    ActionType=M, DamageTypeId="physical", BaseDamage=6,
                    Range=0.9f, Cooldown=2.0f, Windup=0.45f, Recover=0.4f,
                    TelegraphId=TFM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=true, MaxTargets=1 },

                // 8. enemy_blackroot_sprout
                new ActionEntry { Id="action_blackroot_thorn_shot", DisplayName="Thorn Shot",
                    ActionType=R, DamageTypeId="toxic", BaseDamage=4,
                    Range=3.0f, Cooldown=2.5f, Windup=0.4f, Recover=0.35f, ProjectileSpeed=4.0f,
                    StatusIds=new[]{"status_slow_minor"}, StatusChance=0.7f,
                    TelegraphId=TRP, TriggersVuln=true, VulnTrigger=APV, IsInterruptible=true, MaxTargets=1 },

                // ── BAND 2 ─────────────────────────────────────────────────────────

                // 9. enemy_spore_imp
                new ActionEntry { Id="action_spore_imp_toxic_cloud", DisplayName="Toxic Cloud",
                    ActionType=A, DamageTypeId="toxic", BaseDamage=7,
                    Range=1.5f, AreaRadius=1.2f, Cooldown=3.5f, Windup=0.55f, Recover=0.5f,
                    StatusIds=new[]{"status_poison"}, StatusChance=0.9f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=false, MaxTargets=4 },
                new ActionEntry { Id="action_spore_imp_spore_bolt", DisplayName="Spore Bolt",
                    ActionType=C, DamageTypeId="toxic", BaseDamage=5,
                    Range=3.5f, Cooldown=2.0f, Windup=0.5f, Recover=0.4f, ProjectileSpeed=4.0f,
                    StatusIds=new[]{"status_poison_minor"}, StatusChance=0.8f,
                    TelegraphId=TCS, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=true, MaxTargets=1 },

                // 10. enemy_rootsnare
                new ActionEntry { Id="action_rootsnare_emerge_grab", DisplayName="Emerge Grab",
                    ActionType=B, DamageTypeId="physical", BaseDamage=8,
                    Range=1.0f, Cooldown=4.0f, Windup=0.6f, Recover=0.6f,
                    StatusIds=new[]{"status_root_minor"}, StatusChance=0.85f,
                    TelegraphId=TBE, TriggersVuln=true, VulnTrigger=ABE, IsInterruptible=false, MaxTargets=1 },

                // 11. enemy_hollow_stagling
                new ActionEntry { Id="action_hollow_stagling_horn_leap", DisplayName="Horn Leap",
                    ActionType=L, DamageTypeId="physical", BaseDamage=9,
                    Range=2.5f, Cooldown=3.0f, Windup=0.45f, Recover=0.5f,
                    TelegraphId=TLP, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=false, MaxTargets=1 },

                // 12. enemy_goblin_urudakh_trapper
                new ActionEntry { Id="action_urudakh_trip_snare", DisplayName="Trip Snare",
                    ActionType=R, DamageTypeId="physical", BaseDamage=5,
                    Range=3.0f, Cooldown=3.0f, Windup=0.4f, Recover=0.4f, ProjectileSpeed=4.0f,
                    StatusIds=new[]{"status_slow_minor"}, StatusChance=0.85f,
                    TelegraphId=TRP, TriggersVuln=true, VulnTrigger=APV, IsInterruptible=true, MaxTargets=1 },
                new ActionEntry { Id="action_urudakh_scrap_throw", DisplayName="Scrap Throw",
                    ActionType=R, DamageTypeId="physical", BaseDamage=6,
                    Range=3.5f, Cooldown=2.0f, Windup=0.35f, Recover=0.35f, ProjectileSpeed=5.0f,
                    TelegraphId=TRP, TriggersVuln=false, IsInterruptible=true, MaxTargets=1 },

                // 13. enemy_thorn_archer
                new ActionEntry { Id="action_thorn_archer_bleeding_arrow", DisplayName="Bleeding Arrow",
                    ActionType=R, DamageTypeId="physical", BaseDamage=8,
                    Range=4.0f, Cooldown=2.5f, Windup=0.45f, Recover=0.4f, ProjectileSpeed=6.0f,
                    StatusIds=new[]{"status_bleed"}, StatusChance=0.75f,
                    TelegraphId=TRP, TriggersVuln=true, VulnTrigger=APV, IsInterruptible=true, MaxTargets=1 },

                // 14. enemy_orc_nyx_stalker
                new ActionEntry { Id="action_orc_nyx_shadow_emerge", DisplayName="Shadow Emerge",
                    ActionType=B, DamageTypeId="arcane", BaseDamage=9,
                    Range=1.0f, Cooldown=4.0f, Windup=0.55f, Recover=0.55f,
                    StatusIds=new[]{"status_slow_minor"}, StatusChance=0.7f,
                    TelegraphId=TBE, TriggersVuln=true, VulnTrigger=ABE, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_orc_nyx_claw", DisplayName="Shadow Claw",
                    ActionType=M, DamageTypeId="physical", BaseDamage=8,
                    Range=0.9f, Cooldown=2.0f, Windup=0.35f, Recover=0.4f,
                    TelegraphId=TFM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=true, MaxTargets=1 },

                // 15. enemy_mycobulwark
                new ActionEntry { Id="action_mycobulwark_spore_burst", DisplayName="Spore Burst",
                    ActionType=A, DamageTypeId="toxic", BaseDamage=10,
                    Range=1.8f, AreaRadius=1.4f, Cooldown=4.0f, Windup=0.6f, Recover=0.6f,
                    StatusIds=new[]{"status_poison"}, StatusChance=0.9f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=false, MaxTargets=4 },
                new ActionEntry { Id="action_mycobulwark_heavy_bash", DisplayName="Mycelium Bash",
                    ActionType=M, DamageTypeId="physical", BaseDamage=12,
                    Range=1.0f, Cooldown=2.5f, Windup=0.55f, Recover=0.5f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=false, MaxTargets=1 },

                // 16. enemy_nyx_moth
                new ActionEntry { Id="action_nyx_moth_lunar_dust", DisplayName="Lunar Dust",
                    ActionType=A, DamageTypeId="arcane", BaseDamage=8,
                    Range=2.0f, AreaRadius=1.5f, Cooldown=4.0f, Windup=0.55f, Recover=0.5f,
                    StatusIds=new[]{"status_slow"}, StatusChance=0.8f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=false, MaxTargets=4 },
                new ActionEntry { Id="action_nyx_moth_arcane_flutter", DisplayName="Arcane Flutter",
                    ActionType=C, DamageTypeId="arcane", BaseDamage=6,
                    Range=3.5f, Cooldown=2.5f, Windup=0.5f, Recover=0.4f, ProjectileSpeed=4.5f,
                    StatusIds=new[]{"status_slow_minor"}, StatusChance=0.7f,
                    TelegraphId=TCS, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=true, MaxTargets=1 },

                // ── BAND 3 ─────────────────────────────────────────────────────────

                // 17. enemy_frost_gnawer
                new ActionEntry { Id="action_frost_gnawer_cold_bite", DisplayName="Cold Bite",
                    ActionType=M, DamageTypeId="ice", BaseDamage=10,
                    Range=0.8f, Cooldown=2.0f, Windup=0.35f, Recover=0.35f,
                    StatusIds=new[]{"status_chill_minor"}, StatusChance=0.75f,
                    TelegraphId=TFM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=true, MaxTargets=1 },

                // 18. enemy_duergar_frostdelver
                new ActionEntry { Id="action_duergar_frostdelver_pick_swing", DisplayName="Frost Pick Swing",
                    ActionType=M, DamageTypeId="physical", BaseDamage=12,
                    Range=1.0f, Cooldown=2.0f, Windup=0.4f, Recover=0.4f,
                    StatusIds=new[]{"status_chill_minor"}, StatusChance=0.6f,
                    TelegraphId=TFM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=true, MaxTargets=1 },
                new ActionEntry { Id="action_duergar_frostdelver_ice_chip", DisplayName="Ice Chip",
                    ActionType=R, DamageTypeId="ice", BaseDamage=8,
                    Range=3.0f, Cooldown=2.5f, Windup=0.4f, Recover=0.35f, ProjectileSpeed=5.0f,
                    StatusIds=new[]{"status_chill_minor"}, StatusChance=0.7f,
                    TelegraphId=TRP, TriggersVuln=true, VulnTrigger=APV, IsInterruptible=true, MaxTargets=1 },

                // 19. enemy_duergar_shieldbreaker
                new ActionEntry { Id="action_duergar_shieldbreaker_heavy_crack", DisplayName="Heavy Crack",
                    ActionType=M, DamageTypeId="physical", BaseDamage=14,
                    Range=1.0f, Cooldown=2.5f, Windup=0.55f, Recover=0.5f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_duergar_shieldbreaker_guard_break", DisplayName="Guard Break",
                    ActionType=A, DamageTypeId="physical", BaseDamage=10,
                    Range=1.5f, AreaRadius=1.0f, Cooldown=4.0f, Windup=0.6f, Recover=0.55f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=3 },

                // 20. enemy_icebound_sentinel
                new ActionEntry { Id="action_icebound_sentinel_glacial_guard", DisplayName="Glacial Guard",
                    ActionType=S, DamageTypeId="ice", BaseDamage=0,
                    Range=0f, Cooldown=8.0f, Windup=0.4f, Recover=0.4f,
                    TelegraphId=TCS, TriggersVuln=false, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_icebound_sentinel_ice_slam", DisplayName="Ice Slam",
                    ActionType=M, DamageTypeId="ice", BaseDamage=16,
                    Range=1.2f, Cooldown=3.0f, Windup=0.65f, Recover=0.55f,
                    StatusIds=new[]{"status_chill"}, StatusChance=0.85f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=1 },

                // 21. enemy_glassbone
                new ActionEntry { Id="action_glassbone_shard_throw", DisplayName="Bone Shard Throw",
                    ActionType=R, DamageTypeId="ice", BaseDamage=11,
                    Range=4.0f, Cooldown=2.5f, Windup=0.4f, Recover=0.4f, ProjectileSpeed=5.5f,
                    StatusIds=new[]{"status_bleed_minor"}, StatusChance=0.65f,
                    TelegraphId=TRP, TriggersVuln=true, VulnTrigger=APV, IsInterruptible=true, MaxTargets=1 },

                // 22. enemy_cold_cult_acolyte
                new ActionEntry { Id="action_cold_cult_acolyte_frost_ray", DisplayName="Frost Ray",
                    ActionType=C, DamageTypeId="ice", BaseDamage=14,
                    Range=4.5f, Cooldown=3.0f, Windup=0.65f, Recover=0.5f, ProjectileSpeed=5.5f,
                    StatusIds=new[]{"status_chill"}, StatusChance=0.85f,
                    TelegraphId=TCS, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=false, MaxTargets=1 },

                // 23. enemy_crystal_leaper
                new ActionEntry { Id="action_crystal_leaper_shatter_jump", DisplayName="Shatter Jump",
                    ActionType=L, DamageTypeId="ice", BaseDamage=13,
                    Range=2.5f, Cooldown=3.0f, Windup=0.45f, Recover=0.5f,
                    StatusIds=new[]{"status_chill_minor"}, StatusChance=0.7f,
                    TelegraphId=TLP, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=false, MaxTargets=1 },

                // 24. enemy_frost_wailer
                new ActionEntry { Id="action_frost_wailer_freezing_cry", DisplayName="Freezing Cry",
                    ActionType=A, DamageTypeId="ice", BaseDamage=12,
                    Range=2.0f, AreaRadius=2.0f, Cooldown=4.5f, Windup=0.65f, Recover=0.6f,
                    StatusIds=new[]{"status_slow"}, StatusChance=0.9f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=false, MaxTargets=5 },
                new ActionEntry { Id="action_frost_wailer_cold_bolt", DisplayName="Cold Bolt",
                    ActionType=C, DamageTypeId="ice", BaseDamage=10,
                    Range=4.0f, Cooldown=2.5f, Windup=0.55f, Recover=0.45f, ProjectileSpeed=5.0f,
                    StatusIds=new[]{"status_chill_minor"}, StatusChance=0.75f,
                    TelegraphId=TCS, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=true, MaxTargets=1 },

                // ── BAND 4 ─────────────────────────────────────────────────────────

                // 25. enemy_ember_tick
                new ActionEntry { Id="action_ember_tick_burning_bite", DisplayName="Burning Bite",
                    ActionType=M, DamageTypeId="fire", BaseDamage=10,
                    Range=0.6f, Cooldown=1.5f, Windup=0.25f, Recover=0.3f,
                    StatusIds=new[]{"status_burn_minor"}, StatusChance=0.7f,
                    TelegraphId=TFM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=true, MaxTargets=1 },
                new ActionEntry { Id="action_ember_tick_death_pop", DisplayName="Death Pop (Hook)",
                    ActionType=A, DamageTypeId="fire", BaseDamage=12,
                    Range=0f, AreaRadius=1.0f, Cooldown=999f, Windup=0.2f, Recover=0.5f,
                    StatusIds=new[]{"status_burn_minor"}, StatusChance=0.8f,
                    TelegraphId=TAP, TriggersVuln=false, IsInterruptible=false, MaxTargets=3,
                    // Death-trigger activation is SPEC 13D; cooldown=999 keeps it inactive in normal combat
                    },

                // 26. enemy_ash_crawler
                new ActionEntry { Id="action_ash_crawler_hot_lunge", DisplayName="Hot Lunge",
                    ActionType=M, DamageTypeId="fire", BaseDamage=12,
                    Range=1.0f, Cooldown=2.0f, Windup=0.35f, Recover=0.35f,
                    StatusIds=new[]{"status_burn_minor"}, StatusChance=0.65f,
                    TelegraphId=TFM, TriggersVuln=true, VulnTrigger=DCW, IsInterruptible=true, MaxTargets=1 },

                // 27. enemy_orc_kaand_berserker
                new ActionEntry { Id="action_orc_kaand_rage_charge", DisplayName="Rage Charge",
                    ActionType=M, DamageTypeId="physical", BaseDamage=16,
                    Range=1.5f, Cooldown=2.5f, Windup=0.55f, Recover=0.5f,
                    StatusIds=new[]{"status_bleed"}, StatusChance=0.75f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=DCW, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_orc_kaand_ground_roar", DisplayName="Ground Roar",
                    ActionType=A, DamageTypeId="physical", BaseDamage=8,
                    Range=1.5f, AreaRadius=2.0f, Cooldown=5.0f, Windup=0.6f, Recover=0.55f,
                    TelegraphId=TAP, TriggersVuln=false, IsInterruptible=false, MaxTargets=5 },

                // 28. enemy_orc_kaand_ashcaller
                new ActionEntry { Id="action_orc_kaand_ash_bolt", DisplayName="Ash Bolt",
                    ActionType=C, DamageTypeId="fire", BaseDamage=14,
                    Range=4.5f, Cooldown=3.0f, Windup=0.65f, Recover=0.5f, ProjectileSpeed=5.0f,
                    StatusIds=new[]{"status_burn"}, StatusChance=0.8f,
                    TelegraphId=TCS, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_orc_kaand_heat_pulse", DisplayName="Heat Pulse",
                    ActionType=A, DamageTypeId="fire", BaseDamage=10,
                    Range=1.8f, AreaRadius=1.5f, Cooldown=4.0f, Windup=0.55f, Recover=0.5f,
                    StatusIds=new[]{"status_burn_minor"}, StatusChance=0.75f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=false, MaxTargets=4 },

                // 29. enemy_lava_bulwark
                new ActionEntry { Id="action_lava_bulwark_molten_bash", DisplayName="Molten Bash",
                    ActionType=M, DamageTypeId="fire", BaseDamage=18,
                    Range=1.2f, Cooldown=2.5f, Windup=0.65f, Recover=0.6f,
                    StatusIds=new[]{"status_burn"}, StatusChance=0.8f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_lava_bulwark_heat_wave", DisplayName="Heat Wave",
                    ActionType=A, DamageTypeId="fire", BaseDamage=12,
                    Range=2.0f, AreaRadius=2.0f, Cooldown=5.0f, Windup=0.7f, Recover=0.65f,
                    StatusIds=new[]{"status_burn_minor"}, StatusChance=0.7f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=false, MaxTargets=5 },

                // 30. enemy_cinder_spitter
                new ActionEntry { Id="action_cinder_spitter_ember_spit", DisplayName="Ember Spit",
                    ActionType=R, DamageTypeId="fire", BaseDamage=12,
                    Range=4.0f, Cooldown=2.0f, Windup=0.4f, Recover=0.4f, ProjectileSpeed=5.5f,
                    StatusIds=new[]{"status_burn_minor"}, StatusChance=0.7f,
                    TelegraphId=TRP, TriggersVuln=true, VulnTrigger=APV, IsInterruptible=true, MaxTargets=1 },

                // 31. enemy_scorched_cultist
                new ActionEntry { Id="action_scorched_cultist_fire_seal", DisplayName="Fire Seal",
                    ActionType=C, DamageTypeId="fire", BaseDamage=16,
                    Range=4.5f, Cooldown=3.5f, Windup=0.7f, Recover=0.55f, ProjectileSpeed=4.5f,
                    StatusIds=new[]{"status_burn"}, StatusChance=0.85f,
                    TelegraphId=TCS, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_scorched_cultist_ember_circle", DisplayName="Ember Circle",
                    ActionType=A, DamageTypeId="fire", BaseDamage=10,
                    Range=2.0f, AreaRadius=1.8f, Cooldown=4.5f, Windup=0.6f, Recover=0.55f,
                    StatusIds=new[]{"status_burn_minor"}, StatusChance=0.75f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=false, MaxTargets=4 },

                // 32. enemy_furnace_warden
                new ActionEntry { Id="action_furnace_warden_thermal_pulse", DisplayName="Thermal Pulse",
                    ActionType=A, DamageTypeId="fire", BaseDamage=18,
                    Range=2.5f, AreaRadius=2.5f, Cooldown=5.5f, Windup=0.75f, Recover=0.65f,
                    StatusIds=new[]{"status_burn"}, StatusChance=0.9f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=5 },
                new ActionEntry { Id="action_furnace_warden_heavy_guard_hit", DisplayName="Heavy Guard Hit",
                    ActionType=M, DamageTypeId="physical", BaseDamage=20,
                    Range=1.2f, Cooldown=3.0f, Windup=0.7f, Recover=0.6f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=1 },

                // ── BAND 5 ─────────────────────────────────────────────────────────

                // 33. enemy_rune_shard
                new ActionEntry { Id="action_rune_shard_arcane_splinter", DisplayName="Arcane Splinter",
                    ActionType=R, DamageTypeId="arcane", BaseDamage=15,
                    Range=4.0f, Cooldown=2.0f, Windup=0.4f, Recover=0.4f, ProjectileSpeed=6.0f,
                    TelegraphId=TRP, TriggersVuln=true, VulnTrigger=APV, IsInterruptible=true, MaxTargets=1 },

                // 34. enemy_clockwork_guard
                new ActionEntry { Id="action_clockwork_guard_rhythmic_strike", DisplayName="Rhythmic Strike",
                    ActionType=M, DamageTypeId="physical", BaseDamage=18,
                    Range=1.0f, Cooldown=2.0f, Windup=0.5f, Recover=0.5f,
                    TelegraphId=TFM, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=1 },

                // 35. enemy_gnome_gem_madcap
                new ActionEntry { Id="action_gnome_gem_madcap_prism_bolt", DisplayName="Prism Bolt",
                    ActionType=C, DamageTypeId="arcane", BaseDamage=16,
                    Range=4.5f, Cooldown=3.0f, Windup=0.65f, Recover=0.5f, ProjectileSpeed=5.5f,
                    StatusIds=new[]{"status_confuse_minor"}, StatusChance=0.65f,
                    TelegraphId=TCS, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_gnome_gem_madcap_gem_burst", DisplayName="Gem Burst",
                    ActionType=A, DamageTypeId="arcane", BaseDamage=12,
                    Range=2.0f, AreaRadius=2.0f, Cooldown=4.5f, Windup=0.6f, Recover=0.55f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=false, MaxTargets=5 },

                // 36. enemy_gnomorin_rune_tinker
                new ActionEntry { Id="action_gnomorin_rune_tinker_rune_dart", DisplayName="Rune Dart",
                    ActionType=R, DamageTypeId="arcane", BaseDamage=14,
                    Range=4.0f, Cooldown=2.5f, Windup=0.45f, Recover=0.4f, ProjectileSpeed=5.5f,
                    StatusIds=new[]{"status_slow_minor"}, StatusChance=0.7f,
                    TelegraphId=TRP, TriggersVuln=true, VulnTrigger=APV, IsInterruptible=true, MaxTargets=1 },
                new ActionEntry { Id="action_gnomorin_rune_tinker_pressure_glyph", DisplayName="Pressure Glyph",
                    ActionType=A, DamageTypeId="arcane", BaseDamage=10,
                    Range=2.0f, AreaRadius=1.5f, Cooldown=4.0f, Windup=0.55f, Recover=0.5f,
                    StatusIds=new[]{"status_slow_minor"}, StatusChance=0.7f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=false, MaxTargets=4 },

                // 37. enemy_sealed_knight
                new ActionEntry { Id="action_sealed_knight_bound_slash", DisplayName="Bound Slash",
                    ActionType=M, DamageTypeId="physical", BaseDamage=22,
                    Range=1.2f, Cooldown=2.5f, Windup=0.6f, Recover=0.55f,
                    StatusIds=new[]{"status_bleed"}, StatusChance=0.75f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_sealed_knight_oath_slam", DisplayName="Oath Slam",
                    ActionType=A, DamageTypeId="arcane", BaseDamage=16,
                    Range=2.0f, AreaRadius=1.8f, Cooldown=5.0f, Windup=0.7f, Recover=0.6f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=4 },

                // 38. enemy_mirror_adept
                new ActionEntry { Id="action_mirror_adept_reflective_bolt", DisplayName="Reflective Bolt",
                    ActionType=C, DamageTypeId="arcane", BaseDamage=18,
                    Range=5.0f, Cooldown=3.0f, Windup=0.65f, Recover=0.5f, ProjectileSpeed=5.5f,
                    StatusIds=new[]{"status_slow"}, StatusChance=0.75f,
                    TelegraphId=TCS, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_mirror_adept_short_blink_strike", DisplayName="Blink Strike (Hook)",
                    ActionType=M, DamageTypeId="arcane", BaseDamage=14,
                    Range=1.0f, Cooldown=2.5f, Windup=0.35f, Recover=0.4f,
                    // Note: blink runtime is SPEC 13D; this action uses melee resolution until then
                    TelegraphId=TPH, TriggersVuln=true, VulnTrigger=ABA, IsInterruptible=false, MaxTargets=1 },

                // 39. enemy_puzzle_golem
                new ActionEntry { Id="action_puzzle_golem_pressure_zone", DisplayName="Pressure Zone",
                    ActionType=A, DamageTypeId="physical", BaseDamage=16,
                    Range=2.0f, AreaRadius=2.0f, Cooldown=5.0f, Windup=0.7f, Recover=0.65f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=5 },
                new ActionEntry { Id="action_puzzle_golem_weighted_slam", DisplayName="Weighted Slam",
                    ActionType=M, DamageTypeId="physical", BaseDamage=22,
                    Range=1.2f, Cooldown=3.0f, Windup=0.7f, Recover=0.65f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=1 },

                // 40. enemy_oathless_shade
                new ActionEntry { Id="action_oathless_shade_shadow_step", DisplayName="Shadow Step",
                    ActionType=C, DamageTypeId="arcane", BaseDamage=18,
                    Range=4.5f, Cooldown=3.0f, Windup=0.5f, Recover=0.45f, ProjectileSpeed=5.0f,
                    StatusIds=new[]{"status_slow"}, StatusChance=0.8f,
                    // Note: full blink resolution is SPEC 13D
                    TelegraphId=TPH, TriggersVuln=true, VulnTrigger=ABA, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_oathless_shade_oathless_cry", DisplayName="Oathless Cry",
                    ActionType=A, DamageTypeId="arcane", BaseDamage=14,
                    Range=2.5f, AreaRadius=2.5f, Cooldown=5.0f, Windup=0.65f, Recover=0.6f,
                    StatusIds=new[]{"status_slow_minor"}, StatusChance=0.8f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=false, MaxTargets=5 },

                // ── BAND 6 – DEEP (71-85) ──────────────────────────────────────────

                // 41. enemy_drow_shadowblade
                new ActionEntry { Id="action_drow_shadowblade_shadow_strike", DisplayName="Shadow Strike",
                    ActionType=M, DamageTypeId="physical", BaseDamage=24,
                    Range=1.0f, Cooldown=2.0f, Windup=0.4f, Recover=0.35f,
                    TelegraphId=TFM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=true, MaxTargets=1 },
                new ActionEntry { Id="action_drow_shadowblade_void_slash", DisplayName="Void Slash",
                    ActionType=M, DamageTypeId="arcane", BaseDamage=20,
                    Range=1.2f, Cooldown=3.0f, Windup=0.5f, Recover=0.4f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=DCW, IsInterruptible=false, MaxTargets=1 },

                // 42. enemy_drow_arcane_adept
                new ActionEntry { Id="action_drow_arcane_adept_shadow_bolt", DisplayName="Shadow Bolt",
                    ActionType=C, DamageTypeId="arcane", BaseDamage=22,
                    Range=4.5f, Cooldown=2.5f, Windup=0.55f, Recover=0.5f, ProjectileSpeed=5.5f,
                    StatusIds=new[]{"status_slow_minor"}, StatusChance=0.7f,
                    TelegraphId=TCS, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=false, MaxTargets=1 },

                // 43. enemy_drow_shadow_warden
                new ActionEntry { Id="action_drow_shadow_warden_guardian_slam", DisplayName="Guardian Slam",
                    ActionType=M, DamageTypeId="physical", BaseDamage=30,
                    Range=1.2f, Cooldown=3.0f, Windup=0.65f, Recover=0.55f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_drow_shadow_warden_dark_shield_bash", DisplayName="Dark Shield Bash",
                    ActionType=A, DamageTypeId="arcane", BaseDamage=22,
                    Range=1.5f, AreaRadius=1.5f, Cooldown=5.0f, Windup=0.7f, Recover=0.6f,
                    StatusIds=new[]{"status_slow"}, StatusChance=0.75f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=3 },

                // 44. enemy_void_tick
                new ActionEntry { Id="action_void_tick_void_bite", DisplayName="Void Bite",
                    ActionType=M, DamageTypeId="arcane", BaseDamage=18,
                    Range=0.5f, Cooldown=1.5f, Windup=0.2f, Recover=0.25f,
                    TelegraphId=TFM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=true, MaxTargets=1 },

                // 45. enemy_abyssal_riftstalker
                new ActionEntry { Id="action_abyssal_riftstalker_rift_claw", DisplayName="Rift Claw",
                    ActionType=M, DamageTypeId="arcane", BaseDamage=24,
                    Range=1.0f, Cooldown=2.0f, Windup=0.4f, Recover=0.35f,
                    StatusIds=new[]{"status_slow_minor"}, StatusChance=0.6f,
                    TelegraphId=TFM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=true, MaxTargets=1 },

                // 46. enemy_shadow_sentinel
                new ActionEntry { Id="action_shadow_sentinel_void_slam", DisplayName="Void Slam",
                    ActionType=M, DamageTypeId="arcane", BaseDamage=32,
                    Range=1.3f, Cooldown=3.0f, Windup=0.7f, Recover=0.65f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_shadow_sentinel_abyssal_pulse", DisplayName="Abyssal Pulse",
                    ActionType=A, DamageTypeId="arcane", BaseDamage=24,
                    Range=2.0f, AreaRadius=2.0f, Cooldown=5.0f, Windup=0.75f, Recover=0.7f,
                    StatusIds=new[]{"status_slow"}, StatusChance=0.8f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=5 },

                // 47. enemy_void_spitter
                new ActionEntry { Id="action_void_spitter_void_bolt", DisplayName="Void Bolt",
                    ActionType=R, DamageTypeId="arcane", BaseDamage=20,
                    Range=4.5f, Cooldown=2.0f, Windup=0.4f, Recover=0.4f, ProjectileSpeed=5.5f,
                    StatusIds=new[]{"status_slow_minor"}, StatusChance=0.65f,
                    TelegraphId=TRP, TriggersVuln=true, VulnTrigger=APV, IsInterruptible=true, MaxTargets=1 },

                // 48. enemy_abyssal_void_reaver
                new ActionEntry { Id="action_abyssal_void_reaver_reaving_claw", DisplayName="Reaving Claw",
                    ActionType=M, DamageTypeId="arcane", BaseDamage=26,
                    Range=1.0f, Cooldown=2.5f, Windup=0.45f, Recover=0.4f,
                    TelegraphId=TFM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=true, MaxTargets=1 },

                // 49. enemy_ninrorin_phasewalker
                new ActionEntry { Id="action_ninrorin_phasewalker_phase_strike", DisplayName="Phase Strike",
                    ActionType=M, DamageTypeId="arcane", BaseDamage=24,
                    Range=1.0f, Cooldown=2.5f, Windup=0.35f, Recover=0.4f,
                    TelegraphId=TPH, TriggersVuln=true, VulnTrigger=ABA, IsInterruptible=false, MaxTargets=1 },

                // 50. enemy_ninrorin_echo_shade
                new ActionEntry { Id="action_ninrorin_echo_shade_echo_dart", DisplayName="Echo Dart",
                    ActionType=C, DamageTypeId="arcane", BaseDamage=20,
                    Range=4.0f, Cooldown=2.5f, Windup=0.5f, Recover=0.45f, ProjectileSpeed=5.0f,
                    TelegraphId=TCS, TriggersVuln=true, VulnTrigger=ABA, IsInterruptible=false, MaxTargets=1 },

                // ── BAND 7 – VOID (86-99) ──────────────────────────────────────────

                // 51. enemy_ninrorin_void_knight
                new ActionEntry { Id="action_ninrorin_void_knight_void_slash", DisplayName="Void Slash",
                    ActionType=M, DamageTypeId="arcane", BaseDamage=36,
                    Range=1.3f, Cooldown=2.5f, Windup=0.65f, Recover=0.55f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_ninrorin_void_knight_phase_guard", DisplayName="Phase Guard",
                    ActionType=A, DamageTypeId="arcane", BaseDamage=26,
                    Range=2.0f, AreaRadius=1.8f, Cooldown=5.0f, Windup=0.7f, Recover=0.65f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=4 },

                // 52. enemy_ninrorin_void_acolyte
                new ActionEntry { Id="action_ninrorin_void_acolyte_void_ray", DisplayName="Void Ray",
                    ActionType=C, DamageTypeId="arcane", BaseDamage=32,
                    Range=5.0f, Cooldown=3.0f, Windup=0.65f, Recover=0.55f, ProjectileSpeed=5.5f,
                    StatusIds=new[]{"status_slow"}, StatusChance=0.8f,
                    TelegraphId=TCS, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_ninrorin_void_acolyte_rift_pulse", DisplayName="Rift Pulse",
                    ActionType=A, DamageTypeId="arcane", BaseDamage=24,
                    Range=2.5f, AreaRadius=2.5f, Cooldown=5.5f, Windup=0.75f, Recover=0.7f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AC, IsInterruptible=false, MaxTargets=5 },

                // 53. enemy_corrupted_draconic_spawn
                new ActionEntry { Id="action_corrupted_draconic_spawn_corrupted_bite", DisplayName="Corrupted Bite",
                    ActionType=M, DamageTypeId="poison", BaseDamage=38,
                    Range=1.3f, Cooldown=2.5f, Windup=0.65f, Recover=0.6f,
                    StatusIds=new[]{"status_poison"}, StatusChance=0.8f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=DCW, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_corrupted_draconic_spawn_toxic_roar", DisplayName="Toxic Roar",
                    ActionType=A, DamageTypeId="poison", BaseDamage=28,
                    Range=2.5f, AreaRadius=2.5f, Cooldown=5.0f, Windup=0.75f, Recover=0.7f,
                    StatusIds=new[]{"status_poison_minor"}, StatusChance=0.9f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AEP, IsInterruptible=false, MaxTargets=5 },

                // 54. enemy_corrupted_lich_shard
                new ActionEntry { Id="action_corrupted_lich_shard_void_shard", DisplayName="Void Shard",
                    ActionType=R, DamageTypeId="arcane", BaseDamage=22,
                    Range=4.0f, Cooldown=2.0f, Windup=0.4f, Recover=0.35f, ProjectileSpeed=5.5f,
                    TelegraphId=TRP, TriggersVuln=true, VulnTrigger=APV, IsInterruptible=true, MaxTargets=1 },

                // 55. enemy_draconic_void_wyrm
                new ActionEntry { Id="action_draconic_void_wyrm_void_fire_breath", DisplayName="Void Fire Breath",
                    ActionType=A, DamageTypeId="fire", BaseDamage=42,
                    Range=3.0f, AreaRadius=1.5f, Cooldown=4.5f, Windup=0.75f, Recover=0.7f,
                    StatusIds=new[]{"status_burn"}, StatusChance=0.85f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=false, MaxTargets=4 },
                new ActionEntry { Id="action_draconic_void_wyrm_tail_swipe", DisplayName="Tail Swipe",
                    ActionType=M, DamageTypeId="physical", BaseDamage=36,
                    Range=1.5f, Cooldown=3.0f, Windup=0.65f, Recover=0.6f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=false, MaxTargets=2 },

                // 56. enemy_draconic_elder_kin
                new ActionEntry { Id="action_draconic_elder_kin_elder_breath", DisplayName="Elder Breath",
                    ActionType=A, DamageTypeId="fire", BaseDamage=44,
                    Range=3.5f, AreaRadius=2.0f, Cooldown=5.0f, Windup=0.8f, Recover=0.75f,
                    StatusIds=new[]{"status_burn"}, StatusChance=0.9f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AEP, IsInterruptible=false, MaxTargets=5 },
                new ActionEntry { Id="action_draconic_elder_kin_earth_crush", DisplayName="Earth Crush",
                    ActionType=M, DamageTypeId="physical", BaseDamage=40,
                    Range=1.5f, Cooldown=3.0f, Windup=0.7f, Recover=0.65f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=1 },

                // 57. enemy_corrupted_bone_knight
                new ActionEntry { Id="action_corrupted_bone_knight_bone_slash", DisplayName="Bone Slash",
                    ActionType=M, DamageTypeId="physical", BaseDamage=36,
                    Range=1.2f, Cooldown=2.5f, Windup=0.65f, Recover=0.55f,
                    StatusIds=new[]{"status_bleed"}, StatusChance=0.75f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_corrupted_bone_knight_void_shield_bash", DisplayName="Void Shield Bash",
                    ActionType=A, DamageTypeId="arcane", BaseDamage=26,
                    Range=1.8f, AreaRadius=1.5f, Cooldown=5.0f, Windup=0.7f, Recover=0.65f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=3 },

                // 58. enemy_ninrorin_void_sentinel
                new ActionEntry { Id="action_ninrorin_void_sentinel_sentinel_phase", DisplayName="Sentinel Phase",
                    ActionType=M, DamageTypeId="arcane", BaseDamage=38,
                    Range=1.3f, Cooldown=2.5f, Windup=0.65f, Recover=0.6f,
                    TelegraphId=TPH, TriggersVuln=true, VulnTrigger=ABA, IsInterruptible=false, MaxTargets=1 },
                new ActionEntry { Id="action_ninrorin_void_sentinel_void_pulse", DisplayName="Void Pulse",
                    ActionType=A, DamageTypeId="arcane", BaseDamage=28,
                    Range=2.5f, AreaRadius=2.0f, Cooldown=6.0f, Windup=0.8f, Recover=0.7f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=ASD, IsInterruptible=false, MaxTargets=5 },

                // 59. enemy_blackstone_wyvern
                new ActionEntry { Id="action_blackstone_wyvern_wyvern_crush", DisplayName="Wyvern Crush",
                    ActionType=M, DamageTypeId="physical", BaseDamage=50,
                    Range=1.8f, Cooldown=2.5f, Windup=0.75f, Recover=0.7f,
                    TelegraphId=THM, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=false, MaxTargets=2 },
                new ActionEntry { Id="action_blackstone_wyvern_shadow_flame", DisplayName="Shadow Flame",
                    ActionType=A, DamageTypeId="fire", BaseDamage=44,
                    Range=3.5f, AreaRadius=2.5f, Cooldown=5.0f, Windup=0.85f, Recover=0.8f,
                    StatusIds=new[]{"status_burn"}, StatusChance=0.9f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AEP, IsInterruptible=false, MaxTargets=5 },

                // 60. enemy_draconic_ashspitter
                new ActionEntry { Id="action_draconic_ashspitter_ash_spit", DisplayName="Ash Spit",
                    ActionType=R, DamageTypeId="fire", BaseDamage=30,
                    Range=4.5f, Cooldown=2.0f, Windup=0.45f, Recover=0.4f, ProjectileSpeed=5.5f,
                    StatusIds=new[]{"status_burn_minor"}, StatusChance=0.75f,
                    TelegraphId=TRP, TriggersVuln=true, VulnTrigger=APV, IsInterruptible=true, MaxTargets=1 },
                new ActionEntry { Id="action_draconic_ashspitter_ember_cloud", DisplayName="Ember Cloud",
                    ActionType=A, DamageTypeId="fire", BaseDamage=26,
                    Range=2.0f, AreaRadius=1.8f, Cooldown=4.5f, Windup=0.6f, Recover=0.55f,
                    StatusIds=new[]{"status_burn_minor"}, StatusChance=0.8f,
                    TelegraphId=TAP, TriggersVuln=true, VulnTrigger=AAR, IsInterruptible=false, MaxTargets=4 },
            };
        }

        // ── Action sets ──────────────────────────────────────────────────────────

        private static List<ActionSetEntry> BuildActionSets() => new List<ActionSetEntry>
        {
            new ActionSetEntry { Id="actionset_enemy_cave_mite",
                DisplayName="Cave Mite Actions",
                ActionIds=new[]{"action_cave_mite_bite"},
                FallbackId="action_cave_mite_bite",
                RoleTags=new[]{"Swarm","Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_stone_rat",
                DisplayName="Stone Rat Actions",
                ActionIds=new[]{"action_stone_rat_lunge"},
                FallbackId="action_stone_rat_lunge",
                RoleTags=new[]{"Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_cave_bat",
                DisplayName="Cave Bat Actions",
                ActionIds=new[]{"action_cave_bat_screech_dart"},
                FallbackId="action_cave_bat_screech_dart",
                RoleTags=new[]{"Ranged"} },

            new ActionSetEntry { Id="actionset_enemy_goblin_grashnaar_scavenger",
                DisplayName="Goblin Scavenger Actions",
                ActionIds=new[]{"action_grashnaar_throw_scrap","action_grashnaar_knife_jab"},
                FallbackId="action_grashnaar_knife_jab",
                RoleTags=new[]{"Ranged","Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_kobold_scout",
                DisplayName="Kobold Scout Actions",
                ActionIds=new[]{"action_kobold_pebble_shot","action_kobold_quick_stab"},
                FallbackId="action_kobold_quick_stab",
                RoleTags=new[]{"Ranged","Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_mossling",
                DisplayName="Mossling Actions",
                ActionIds=new[]{"action_mossling_spore_puff","action_mossling_body_bump"},
                FallbackId="action_mossling_body_bump",
                RoleTags=new[]{"Guard","Tank"} },

            new ActionSetEntry { Id="actionset_enemy_cracked_bone",
                DisplayName="Cracked Bone Actions",
                ActionIds=new[]{"action_cracked_bone_bone_swing"},
                FallbackId="action_cracked_bone_bone_swing",
                RoleTags=new[]{"Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_blackroot_sprout",
                DisplayName="Blackroot Sprout Actions",
                ActionIds=new[]{"action_blackroot_thorn_shot"},
                FallbackId="action_blackroot_thorn_shot",
                RoleTags=new[]{"Ranged","Guard"} },

            new ActionSetEntry { Id="actionset_enemy_spore_imp",
                DisplayName="Spore Imp Actions",
                ActionIds=new[]{"action_spore_imp_toxic_cloud","action_spore_imp_spore_bolt"},
                FallbackId="action_spore_imp_spore_bolt",
                RoleTags=new[]{"Caster","Swarm"} },

            new ActionSetEntry { Id="actionset_enemy_rootsnare",
                DisplayName="Rootsnare Actions",
                ActionIds=new[]{"action_rootsnare_emerge_grab"},
                FallbackId="action_rootsnare_emerge_grab",
                RoleTags=new[]{"Guard","Burrower"} },

            new ActionSetEntry { Id="actionset_enemy_hollow_stagling",
                DisplayName="Hollow Stagling Actions",
                ActionIds=new[]{"action_hollow_stagling_horn_leap"},
                FallbackId="action_hollow_stagling_horn_leap",
                RoleTags=new[]{"Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_goblin_urudakh_trapper",
                DisplayName="Goblin Trapper Actions",
                ActionIds=new[]{"action_urudakh_trip_snare","action_urudakh_scrap_throw"},
                FallbackId="action_urudakh_scrap_throw",
                RoleTags=new[]{"Ranged"} },

            new ActionSetEntry { Id="actionset_enemy_thorn_archer",
                DisplayName="Thorn Archer Actions",
                ActionIds=new[]{"action_thorn_archer_bleeding_arrow"},
                FallbackId="action_thorn_archer_bleeding_arrow",
                RoleTags=new[]{"Ranged"} },

            new ActionSetEntry { Id="actionset_enemy_orc_nyx_stalker",
                DisplayName="Orc Nyx Stalker Actions",
                ActionIds=new[]{"action_orc_nyx_shadow_emerge","action_orc_nyx_claw"},
                FallbackId="action_orc_nyx_claw",
                RoleTags=new[]{"Burrower","Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_mycobulwark",
                DisplayName="Mycobulwark Actions",
                ActionIds=new[]{"action_mycobulwark_spore_burst","action_mycobulwark_heavy_bash"},
                FallbackId="action_mycobulwark_heavy_bash",
                RoleTags=new[]{"Tank","Guard"} },

            new ActionSetEntry { Id="actionset_enemy_nyx_moth",
                DisplayName="Nyx Moth Actions",
                ActionIds=new[]{"action_nyx_moth_lunar_dust","action_nyx_moth_arcane_flutter"},
                FallbackId="action_nyx_moth_arcane_flutter",
                RoleTags=new[]{"Caster","Swarm"} },

            new ActionSetEntry { Id="actionset_enemy_frost_gnawer",
                DisplayName="Frost Gnawer Actions",
                ActionIds=new[]{"action_frost_gnawer_cold_bite"},
                FallbackId="action_frost_gnawer_cold_bite",
                RoleTags=new[]{"Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_duergar_frostdelver",
                DisplayName="Duergar Frostdelver Actions",
                ActionIds=new[]{"action_duergar_frostdelver_pick_swing","action_duergar_frostdelver_ice_chip"},
                FallbackId="action_duergar_frostdelver_pick_swing",
                RoleTags=new[]{"Ranged","Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_duergar_shieldbreaker",
                DisplayName="Duergar Shieldbreaker Actions",
                ActionIds=new[]{"action_duergar_shieldbreaker_heavy_crack","action_duergar_shieldbreaker_guard_break"},
                FallbackId="action_duergar_shieldbreaker_heavy_crack",
                RoleTags=new[]{"Guard","Tank"} },

            new ActionSetEntry { Id="actionset_enemy_icebound_sentinel",
                DisplayName="Icebound Sentinel Actions",
                ActionIds=new[]{"action_icebound_sentinel_glacial_guard","action_icebound_sentinel_ice_slam"},
                FallbackId="action_icebound_sentinel_ice_slam",
                RoleTags=new[]{"Guard","Tank"} },

            new ActionSetEntry { Id="actionset_enemy_glassbone",
                DisplayName="Glassbone Actions",
                ActionIds=new[]{"action_glassbone_shard_throw"},
                FallbackId="action_glassbone_shard_throw",
                RoleTags=new[]{"Ranged","Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_cold_cult_acolyte",
                DisplayName="Cold Cult Acolyte Actions",
                ActionIds=new[]{"action_cold_cult_acolyte_frost_ray"},
                FallbackId="action_cold_cult_acolyte_frost_ray",
                RoleTags=new[]{"Caster"} },

            new ActionSetEntry { Id="actionset_enemy_crystal_leaper",
                DisplayName="Crystal Leaper Actions",
                ActionIds=new[]{"action_crystal_leaper_shatter_jump"},
                FallbackId="action_crystal_leaper_shatter_jump",
                RoleTags=new[]{"Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_frost_wailer",
                DisplayName="Frost Wailer Actions",
                ActionIds=new[]{"action_frost_wailer_freezing_cry","action_frost_wailer_cold_bolt"},
                FallbackId="action_frost_wailer_cold_bolt",
                RoleTags=new[]{"Caster","Elite"} },

            new ActionSetEntry { Id="actionset_enemy_ember_tick",
                DisplayName="Ember Tick Actions",
                ActionIds=new[]{"action_ember_tick_burning_bite","action_ember_tick_death_pop"},
                FallbackId="action_ember_tick_burning_bite",
                RoleTags=new[]{"Swarm","Chaser"},
                Notes="action_ember_tick_death_pop has cooldown=999 — death-trigger hook for SPEC 13D" },

            new ActionSetEntry { Id="actionset_enemy_ash_crawler",
                DisplayName="Ash Crawler Actions",
                ActionIds=new[]{"action_ash_crawler_hot_lunge"},
                FallbackId="action_ash_crawler_hot_lunge",
                RoleTags=new[]{"Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_orc_kaand_berserker",
                DisplayName="Orc Kaand Berserker Actions",
                ActionIds=new[]{"action_orc_kaand_rage_charge","action_orc_kaand_ground_roar"},
                FallbackId="action_orc_kaand_rage_charge",
                RoleTags=new[]{"Chaser","Tank"} },

            new ActionSetEntry { Id="actionset_enemy_orc_kaand_ashcaller",
                DisplayName="Orc Kaand Ashcaller Actions",
                ActionIds=new[]{"action_orc_kaand_ash_bolt","action_orc_kaand_heat_pulse"},
                FallbackId="action_orc_kaand_ash_bolt",
                RoleTags=new[]{"Caster","Ranged"} },

            new ActionSetEntry { Id="actionset_enemy_lava_bulwark",
                DisplayName="Lava Bulwark Actions",
                ActionIds=new[]{"action_lava_bulwark_molten_bash","action_lava_bulwark_heat_wave"},
                FallbackId="action_lava_bulwark_molten_bash",
                RoleTags=new[]{"Tank","Guard"} },

            new ActionSetEntry { Id="actionset_enemy_cinder_spitter",
                DisplayName="Cinder Spitter Actions",
                ActionIds=new[]{"action_cinder_spitter_ember_spit"},
                FallbackId="action_cinder_spitter_ember_spit",
                RoleTags=new[]{"Ranged"} },

            new ActionSetEntry { Id="actionset_enemy_scorched_cultist",
                DisplayName="Scorched Cultist Actions",
                ActionIds=new[]{"action_scorched_cultist_fire_seal","action_scorched_cultist_ember_circle"},
                FallbackId="action_scorched_cultist_fire_seal",
                RoleTags=new[]{"Caster"} },

            new ActionSetEntry { Id="actionset_enemy_furnace_warden",
                DisplayName="Furnace Warden Actions",
                ActionIds=new[]{"action_furnace_warden_thermal_pulse","action_furnace_warden_heavy_guard_hit"},
                FallbackId="action_furnace_warden_heavy_guard_hit",
                RoleTags=new[]{"Guard","Elite"} },

            new ActionSetEntry { Id="actionset_enemy_rune_shard",
                DisplayName="Rune Shard Actions",
                ActionIds=new[]{"action_rune_shard_arcane_splinter"},
                FallbackId="action_rune_shard_arcane_splinter",
                RoleTags=new[]{"Swarm","Ranged"} },

            new ActionSetEntry { Id="actionset_enemy_clockwork_guard",
                DisplayName="Clockwork Guard Actions",
                ActionIds=new[]{"action_clockwork_guard_rhythmic_strike"},
                FallbackId="action_clockwork_guard_rhythmic_strike",
                RoleTags=new[]{"Guard","Tank"} },

            new ActionSetEntry { Id="actionset_enemy_gnome_gem_madcap",
                DisplayName="Gnome Gem Madcap Actions",
                ActionIds=new[]{"action_gnome_gem_madcap_prism_bolt","action_gnome_gem_madcap_gem_burst"},
                FallbackId="action_gnome_gem_madcap_prism_bolt",
                RoleTags=new[]{"Caster","Ranged"} },

            new ActionSetEntry { Id="actionset_enemy_gnomorin_rune_tinker",
                DisplayName="Gnomorin Rune Tinker Actions",
                ActionIds=new[]{"action_gnomorin_rune_tinker_rune_dart","action_gnomorin_rune_tinker_pressure_glyph"},
                FallbackId="action_gnomorin_rune_tinker_rune_dart",
                RoleTags=new[]{"Ranged","Caster"} },

            new ActionSetEntry { Id="actionset_enemy_sealed_knight",
                DisplayName="Sealed Knight Actions",
                ActionIds=new[]{"action_sealed_knight_bound_slash","action_sealed_knight_oath_slam"},
                FallbackId="action_sealed_knight_bound_slash",
                RoleTags=new[]{"Tank","Elite"} },

            new ActionSetEntry { Id="actionset_enemy_mirror_adept",
                DisplayName="Mirror Adept Actions",
                ActionIds=new[]{"action_mirror_adept_reflective_bolt","action_mirror_adept_short_blink_strike"},
                FallbackId="action_mirror_adept_reflective_bolt",
                RoleTags=new[]{"Caster"},
                Notes="action_mirror_adept_short_blink_strike — blink runtime is SPEC 13D; resolves as melee until then" },

            new ActionSetEntry { Id="actionset_enemy_puzzle_golem",
                DisplayName="Puzzle Golem Actions",
                ActionIds=new[]{"action_puzzle_golem_pressure_zone","action_puzzle_golem_weighted_slam"},
                FallbackId="action_puzzle_golem_weighted_slam",
                RoleTags=new[]{"Tank","Guard"} },

            new ActionSetEntry { Id="actionset_enemy_oathless_shade",
                DisplayName="Oathless Shade Actions",
                ActionIds=new[]{"action_oathless_shade_shadow_step","action_oathless_shade_oathless_cry"},
                FallbackId="action_oathless_shade_shadow_step",
                RoleTags=new[]{"Caster"},
                Notes="action_oathless_shade_shadow_step — full blink resolution is SPEC 13D" },

            // ── BAND 6 – DEEP (71-85) ────────────────────────────────────────────

            new ActionSetEntry { Id="actionset_enemy_drow_shadowblade",
                DisplayName="Drow Shadowblade Actions",
                ActionIds=new[]{"action_drow_shadowblade_shadow_strike","action_drow_shadowblade_void_slash"},
                FallbackId="action_drow_shadowblade_shadow_strike",
                RoleTags=new[]{"Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_drow_arcane_adept",
                DisplayName="Drow Arcane Adept Actions",
                ActionIds=new[]{"action_drow_arcane_adept_shadow_bolt"},
                FallbackId="action_drow_arcane_adept_shadow_bolt",
                RoleTags=new[]{"Caster"} },

            new ActionSetEntry { Id="actionset_enemy_drow_shadow_warden",
                DisplayName="Drow Shadow Warden Actions",
                ActionIds=new[]{"action_drow_shadow_warden_guardian_slam","action_drow_shadow_warden_dark_shield_bash"},
                FallbackId="action_drow_shadow_warden_guardian_slam",
                RoleTags=new[]{"Guard","Tank"} },

            new ActionSetEntry { Id="actionset_enemy_void_tick",
                DisplayName="Void Tick Actions",
                ActionIds=new[]{"action_void_tick_void_bite"},
                FallbackId="action_void_tick_void_bite",
                RoleTags=new[]{"Swarm","Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_abyssal_riftstalker",
                DisplayName="Abyssal Riftstalker Actions",
                ActionIds=new[]{"action_abyssal_riftstalker_rift_claw"},
                FallbackId="action_abyssal_riftstalker_rift_claw",
                RoleTags=new[]{"Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_shadow_sentinel",
                DisplayName="Shadow Sentinel Actions",
                ActionIds=new[]{"action_shadow_sentinel_void_slam","action_shadow_sentinel_abyssal_pulse"},
                FallbackId="action_shadow_sentinel_void_slam",
                RoleTags=new[]{"Guard","Tank"} },

            new ActionSetEntry { Id="actionset_enemy_void_spitter",
                DisplayName="Void Spitter Actions",
                ActionIds=new[]{"action_void_spitter_void_bolt"},
                FallbackId="action_void_spitter_void_bolt",
                RoleTags=new[]{"Ranged"} },

            new ActionSetEntry { Id="actionset_enemy_abyssal_void_reaver",
                DisplayName="Abyssal Void Reaver Actions",
                ActionIds=new[]{"action_abyssal_void_reaver_reaving_claw"},
                FallbackId="action_abyssal_void_reaver_reaving_claw",
                RoleTags=new[]{"Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_ninrorin_phasewalker",
                DisplayName="Ninrorin Phasewalker Actions",
                ActionIds=new[]{"action_ninrorin_phasewalker_phase_strike"},
                FallbackId="action_ninrorin_phasewalker_phase_strike",
                RoleTags=new[]{"Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_ninrorin_echo_shade",
                DisplayName="Ninrorin Echo Shade Actions",
                ActionIds=new[]{"action_ninrorin_echo_shade_echo_dart"},
                FallbackId="action_ninrorin_echo_shade_echo_dart",
                RoleTags=new[]{"Caster"} },

            // ── BAND 7 – VOID (86-99) ────────────────────────────────────────────

            new ActionSetEntry { Id="actionset_enemy_ninrorin_void_knight",
                DisplayName="Ninrorin Void Knight Actions",
                ActionIds=new[]{"action_ninrorin_void_knight_void_slash","action_ninrorin_void_knight_phase_guard"},
                FallbackId="action_ninrorin_void_knight_void_slash",
                RoleTags=new[]{"Guard","Tank"} },

            new ActionSetEntry { Id="actionset_enemy_ninrorin_void_acolyte",
                DisplayName="Ninrorin Void Acolyte Actions",
                ActionIds=new[]{"action_ninrorin_void_acolyte_void_ray","action_ninrorin_void_acolyte_rift_pulse"},
                FallbackId="action_ninrorin_void_acolyte_void_ray",
                RoleTags=new[]{"Caster","Elite"} },

            new ActionSetEntry { Id="actionset_enemy_corrupted_draconic_spawn",
                DisplayName="Corrupted Draconic Spawn Actions",
                ActionIds=new[]{"action_corrupted_draconic_spawn_corrupted_bite","action_corrupted_draconic_spawn_toxic_roar"},
                FallbackId="action_corrupted_draconic_spawn_corrupted_bite",
                RoleTags=new[]{"Tank","Elite"} },

            new ActionSetEntry { Id="actionset_enemy_corrupted_lich_shard",
                DisplayName="Corrupted Lich Shard Actions",
                ActionIds=new[]{"action_corrupted_lich_shard_void_shard"},
                FallbackId="action_corrupted_lich_shard_void_shard",
                RoleTags=new[]{"Swarm","Ranged"} },

            new ActionSetEntry { Id="actionset_enemy_draconic_void_wyrm",
                DisplayName="Draconic Void Wyrm Actions",
                ActionIds=new[]{"action_draconic_void_wyrm_void_fire_breath","action_draconic_void_wyrm_tail_swipe"},
                FallbackId="action_draconic_void_wyrm_void_fire_breath",
                RoleTags=new[]{"Tank","Chaser"} },

            new ActionSetEntry { Id="actionset_enemy_draconic_elder_kin",
                DisplayName="Draconic Elder Kin Actions",
                ActionIds=new[]{"action_draconic_elder_kin_elder_breath","action_draconic_elder_kin_earth_crush"},
                FallbackId="action_draconic_elder_kin_elder_breath",
                RoleTags=new[]{"Tank","Elite"} },

            new ActionSetEntry { Id="actionset_enemy_corrupted_bone_knight",
                DisplayName="Corrupted Bone Knight Actions",
                ActionIds=new[]{"action_corrupted_bone_knight_bone_slash","action_corrupted_bone_knight_void_shield_bash"},
                FallbackId="action_corrupted_bone_knight_bone_slash",
                RoleTags=new[]{"Guard","Tank"} },

            new ActionSetEntry { Id="actionset_enemy_ninrorin_void_sentinel",
                DisplayName="Ninrorin Void Sentinel Actions",
                ActionIds=new[]{"action_ninrorin_void_sentinel_sentinel_phase","action_ninrorin_void_sentinel_void_pulse"},
                FallbackId="action_ninrorin_void_sentinel_sentinel_phase",
                RoleTags=new[]{"Guard","Elite"} },

            new ActionSetEntry { Id="actionset_enemy_blackstone_wyvern",
                DisplayName="Blackstone Wyvern Actions",
                ActionIds=new[]{"action_blackstone_wyvern_wyvern_crush","action_blackstone_wyvern_shadow_flame"},
                FallbackId="action_blackstone_wyvern_wyvern_crush",
                RoleTags=new[]{"Tank","Elite"} },

            new ActionSetEntry { Id="actionset_enemy_draconic_ashspitter",
                DisplayName="Draconic Ashspitter Actions",
                ActionIds=new[]{"action_draconic_ashspitter_ash_spit","action_draconic_ashspitter_ember_cloud"},
                FallbackId="action_draconic_ashspitter_ash_spit",
                RoleTags=new[]{"Ranged","Chaser"} },
        };

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
