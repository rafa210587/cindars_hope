using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using CindarsHope.Enemy;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.EnemyTaxonomy
{
    public static class CreateBestiaryEntries40
    {
        private const string BestiaryFolder = "Assets/_Game/Data/Bestiary";

        public static void CreateEntries()
        {
            EnsureFolder(BestiaryFolder);

            var enemyMap = AssetDatabase.FindAssets("t:EnemyDataSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyDataSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.enemyId))
                .GroupBy(e => e.enemyId)
                .ToDictionary(g => g.Key, g => g.OrderBy(e => AssetDatabase.GetAssetPath(e)).First());

            int created = 0;
            int updated = 0;

            foreach (var enemyId in GetRequiredEnemyIds())
            {
                enemyMap.TryGetValue(enemyId, out var enemy);
                string entryId = !string.IsNullOrWhiteSpace(enemy?.BestiaryEntryId)
                    ? enemy.BestiaryEntryId
                    : $"bestiary_{enemyId.Replace("enemy_", string.Empty)}";

                string assetPath = $"{BestiaryFolder}/{entryId}.asset";
                var entry = AssetDatabase.LoadAssetAtPath<EnemyBestiaryEntrySO>(assetPath);
                if (entry == null)
                {
                    entry = ScriptableObject.CreateInstance<EnemyBestiaryEntrySO>();
                    AssetDatabase.CreateAsset(entry, assetPath);
                    created++;
                }
                else
                {
                    updated++;
                }

                Populate(entry, enemyId, entryId, enemy);
                EditorUtility.SetDirty(entry);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SPEC 13E] Bestiary entries complete. Created: {created}, Updated: {updated}, Total required: {RequiredEnemyIds.Length}.");
        }

        private static void Populate(EnemyBestiaryEntrySO entry, string enemyId, string entryId, EnemyDataSO enemy)
        {
            string displayName = !string.IsNullOrWhiteSpace(enemy?.DisplayName) ? enemy.DisplayName : ToTitle(enemyId.Replace("enemy_", string.Empty));
            string factionText = ToFactionText(enemy?.FactionId, enemyId);
            string habitat = BuildHabitat(enemy?.CaveBand ?? GuessCaveBand(enemyId), enemyId);
            string behavior = BuildBehavior(enemy?.PrimaryRole ?? GuessRole(enemyId), displayName);
            string vulnerabilityLocked = "Toda criatura de Vaalara deixa um ritmo escapar antes de atacar; observe o corpo antes de responder.";
            string vulnerabilityDiscovered = BuildVulnerabilityHint(enemy?.VulnerabilityProfileId, enemy?.PrimaryRole ?? GuessRole(enemyId));
            string drops = BuildDropsHint(enemy);

            entry.BestiaryEntryId = entryId;
            entry.EnemyId = enemyId;
            entry.DisplayName = displayName;
            entry.ShortDescription = BuildDescription(displayName, enemy?.LoreTagline, factionText, habitat);
            entry.HabitatText = habitat;
            entry.BehaviorHint = behavior;
            entry.VulnerabilityHintLocked = vulnerabilityLocked;
            entry.VulnerabilityHintDiscovered = vulnerabilityDiscovered;
            entry.KnownDropsHint = drops;
            entry.FactionText = factionText;
            entry.FirstSeenUnlockMode = BestiaryFirstSeenUnlockMode.SeenOrSpawned;
        }

        private static string BuildDescription(string displayName, string loreTagline, string factionText, string habitat)
        {
            if (!string.IsNullOrWhiteSpace(loreTagline))
            {
                return $"{displayName} carrega sinais de {factionText.ToLowerInvariant()} nas rotas de Vaalara. {loreTagline}";
            }

            return $"{displayName} e uma ameaca registrada nas patrulhas de Cindar's Hope. Costuma aparecer em {habitat.ToLowerInvariant()}";
        }

        private static string BuildHabitat(int caveBand, string enemyId)
        {
            if (enemyId.Contains("draconic") || enemyId.Contains("lava") || enemyId.Contains("ember") || enemyId.Contains("ash"))
                return "Encontrado em camadas quentes, fornalhas antigas e saloes marcados por fuligem.";
            if (enemyId.Contains("frost") || enemyId.Contains("ice"))
                return "Encontrado em galerias frias, corredores cristalizados e bolsos de ar gelado.";
            if (enemyId.Contains("fungal") || enemyId.Contains("spore") || enemyId.Contains("mushroom"))
                return "Encontrado em bolsos fungicos, troncos apodrecidos e salas umidas.";
            if (enemyId.Contains("void") || enemyId.Contains("phase") || enemyId.Contains("phantom"))
                return "Encontrado em ruinas instaveis, passagens silenciosas e fissuras de Nyx.";

            return caveBand switch
            {
                <= 1 => "Encontrado nas primeiras camadas da caverna, perto de raizes e pedra quebrada.",
                2 => "Encontrado em tuneis medios, postos improvisados e passagens de patrulha.",
                3 => "Encontrado em regioes profundas onde faccoes disputam territorio.",
                4 => "Encontrado em salas antigas, corredores selados e areas corrompidas.",
                _ => "Encontrado nas camadas mais perigosas, longe das rotas seguras de Cindar's Hope."
            };
        }

        private static string BuildBehavior(EnemyRole role, string displayName)
        {
            return role switch
            {
                EnemyRole.Swarm => $"{displayName} tenta cercar o alvo e punir movimentos atrasados.",
                EnemyRole.Guard => $"{displayName} segura posicao e abre brechas quando baixa a defesa.",
                EnemyRole.Ranged => $"{displayName} prefere distancia e perde ritmo apos sequencias de disparo.",
                EnemyRole.Caster => $"{displayName} prepara efeitos de area e fica exposto depois da conjuracao.",
                EnemyRole.Burrower => $"{displayName} some no terreno e reaparece buscando flancos.",
                EnemyRole.Tank => $"{displayName} avanca devagar, aguenta impacto e demora a recuperar golpes pesados.",
                EnemyRole.Boss => $"{displayName} alterna pressoes e exige leitura cuidadosa das janelas de ataque.",
                EnemyRole.MiniBoss => $"{displayName} combina padroes de elite com aberturas curtas.",
                _ => $"{displayName} persegue o alvo e pune jogadores parados."
            };
        }

        private static string BuildVulnerabilityHint(string vulnerabilityProfileId, EnemyRole role)
        {
            if (!string.IsNullOrWhiteSpace(vulnerabilityProfileId))
            {
                if (vulnerabilityProfileId.Contains("burrow")) return "Fica vulneravel logo depois de emergir do solo.";
                if (vulnerabilityProfileId.Contains("caster")) return "Fica vulneravel ao terminar uma conjuracao.";
                if (vulnerabilityProfileId.Contains("ranged")) return "Fica vulneravel depois de uma sequencia de projeteis.";
                if (vulnerabilityProfileId.Contains("guard")) return "Fica vulneravel quando a guarda cai.";
                if (vulnerabilityProfileId.Contains("phase")) return "Fica vulneravel depois de reaparecer.";
                if (vulnerabilityProfileId.Contains("leaper")) return "Fica vulneravel depois de aterrissar.";
                if (vulnerabilityProfileId.Contains("tank")) return "Fica vulneravel durante a recuperacao de golpes pesados.";
            }

            return role switch
            {
                EnemyRole.Caster => "Fica vulneravel ao terminar uma conjuracao.",
                EnemyRole.Ranged => "Fica vulneravel depois de disparar em sequencia.",
                EnemyRole.Guard => "Fica vulneravel quando baixa a guarda.",
                EnemyRole.Burrower => "Fica vulneravel logo depois de emergir.",
                _ => "Fica vulneravel durante a recuperacao dos ataques mais comprometidos."
            };
        }

        private static string BuildDropsHint(EnemyDataSO enemy)
        {
            if (enemy == null)
                return "Drops ainda dependem da tabela de loot final desta criatura.";

            if (!string.IsNullOrWhiteSpace(enemy.lootTableId))
                return $"Pode revelar itens da tabela {enemy.lootTableId}.";

            if (!string.IsNullOrWhiteSpace(enemy.dropItemId) && enemy.dropAmount > 0)
                return $"Pode deixar {enemy.dropItemId} quando derrotado.";

            return "Pode deixar materiais comuns da sua regiao.";
        }

        private static string ToFactionText(string factionId, string enemyId)
        {
            if (!string.IsNullOrWhiteSpace(factionId))
                return ToTitle(factionId.Replace("faction_", string.Empty));

            if (enemyId.Contains("orc")) return "Orcs de Kaand";
            if (enemyId.Contains("duergar")) return "Duergar das forjas frias";
            if (enemyId.Contains("goblin")) return "Bandos goblin";
            if (enemyId.Contains("kobold")) return "Patrulhas kobold";
            if (enemyId.Contains("drow")) return "Casas sombrias";
            if (enemyId.Contains("gnome")) return "Engenho gnomorin";
            if (enemyId.Contains("undead")) return "Mortos inquietos";
            if (enemyId.Contains("draconic")) return "Sangue draconico";
            return "Criaturas da caverna";
        }

        private static EnemyRole GuessRole(string enemyId)
        {
            if (enemyId.Contains("shaman") || enemyId.Contains("witch") || enemyId.Contains("cultist") || enemyId.Contains("lich")) return EnemyRole.Caster;
            if (enemyId.Contains("crossbow") || enemyId.Contains("archer") || enemyId.Contains("spitter")) return EnemyRole.Ranged;
            if (enemyId.Contains("sentry") || enemyId.Contains("guardian") || enemyId.Contains("knight") || enemyId.Contains("warder")) return EnemyRole.Guard;
            if (enemyId.Contains("burrow") || enemyId.Contains("lurker")) return EnemyRole.Burrower;
            if (enemyId.Contains("mite") || enemyId.Contains("grub")) return EnemyRole.Swarm;
            if (enemyId.Contains("warlord") || enemyId.Contains("queen") || enemyId.Contains("patriarch") || enemyId.Contains("herald") || enemyId.Contains("elder")) return EnemyRole.Boss;
            if (enemyId.Contains("elemental") || enemyId.Contains("wargolem") || enemyId.Contains("gatekeeper")) return EnemyRole.Tank;
            return EnemyRole.Chaser;
        }

        private static int GuessCaveBand(string enemyId)
        {
            if (enemyId.Contains("queen") || enemyId.Contains("patriarch") || enemyId.Contains("lord") || enemyId.Contains("herald") || enemyId.Contains("elder")) return 6;
            if (enemyId.Contains("greater") || enemyId.Contains("guardian") || enemyId.Contains("champion") || enemyId.Contains("lurker")) return 5;
            if (enemyId.Contains("witch") || enemyId.Contains("knight") || enemyId.Contains("construct") || enemyId.Contains("hound")) return 4;
            if (enemyId.Contains("berserker") || enemyId.Contains("warder") || enemyId.Contains("undead") || enemyId.Contains("phase")) return 3;
            if (enemyId.Contains("shaman") || enemyId.Contains("trapmaster") || enemyId.Contains("orc") || enemyId.Contains("leaper")) return 2;
            return 1;
        }

        private static string ToTitle(string value)
        {
            return string.Join(" ", value.Split('_').Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
        }

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

        private static readonly string[] RequiredEnemyIds =
        {
            "enemy_verdant_mite", "enemy_spore_crawler", "enemy_goblin_scrounger", "enemy_kobold_sentry",
            "enemy_rot_beetle", "enemy_pale_grub", "enemy_mushroom_puffball", "enemy_goblin_shaman",
            "enemy_kobold_trapmaster", "enemy_orc_grunt", "enemy_cave_leaper", "enemy_duergar_crossbowman",
            "enemy_burrowing_maggot", "enemy_fungal_spreader", "enemy_drow_skirmisher", "enemy_orc_berserker",
            "enemy_duergar_warder", "enemy_undead_shambler", "enemy_cultist_zealot", "enemy_gnome_tinkerer",
            "enemy_phase_stalker", "enemy_earth_elemental_minor", "enemy_cave_burrower_elite", "enemy_drow_witch",
            "enemy_undead_knight", "enemy_construct_sentry", "enemy_abyssal_hound", "enemy_corrupted_vine_horror",
            "enemy_ninrorin_phantom", "enemy_gnome_wargolem", "enemy_draconic_wyrmling", "enemy_abyssal_lurker",
            "enemy_corrupted_orc_champion", "enemy_earth_elemental_greater", "enemy_undead_lich_acolyte",
            "enemy_draconic_guardian", "enemy_goblin_warchief", "enemy_orc_warlord", "enemy_abyssal_gatekeeper",
            "enemy_cave_mite_queen", "enemy_fungal_patriarch", "enemy_duergar_artificer_lord", "enemy_void_herald",
            "enemy_draconic_elder",
        };

        private static string[] GetRequiredEnemyIds()
        {
            return CreateEnemySpawnEcologyData.BuildProfileDefinitions()
                .Select(p => p.EnemyId)
                .Distinct()
                .ToArray();
        }
    }
}
