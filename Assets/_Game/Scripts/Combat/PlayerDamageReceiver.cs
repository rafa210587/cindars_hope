using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using CindarsHope.Core.Events;
using CindarsHope.DebugTools;
using CindarsHope.Foundation;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Combat
{
    /// <summary>
    /// F03 — redutor CENTRAL de dano recebido pelo player. Todos os caminhos passam aqui por chamada
    /// DIRETA a ApplyDamage (fable_66: o antigo PlayerHitEvent foi aposentado — não havia publisher):
    /// melee inimigo (EnemyBrain), contato (EnemyContactDamage), projétil inimigo
    /// (EnemyProjectileBehaviour), armadilhas/hazards da caverna. Armadura: Defense derivado
    /// (equipment bônus + passivas) com floor 1.
    /// </summary>
    public static class PlayerDamageReceiver
    {
        private static long s_nextCombatResolutionId;

        /// <summary>Fonte de Defense (injetável p/ testes; default = passivas via bootstrap).</summary>
        public static System.Func<int> DefenseSource;

        // Graça de spawn: janela curta de invulnerabilidade logo após (re)entrar numa cena ou reviver.
        // Sem ela o player pode spawnar colado num inimigo restaurado do snapshot da caverna e morrer
        // de dano de contato antes de reagir (bug reportado: caverna->fazenda->caverna virava tela de
        // morte). Armada pelo PlayerDeathController em sceneLoaded e PlayerRespawnedEvent (via
        // SpawnGraceProvider).
        private static float s_spawnGraceUntil;

        /// <summary>Concede invulnerabilidade por <paramref name="seconds"/> a partir de agora.</summary>
        public static void GrantSpawnGrace(float seconds)
        {
            var until = Time.time + Mathf.Max(0f, seconds);
            if (until > s_spawnGraceUntil)
            {
                s_spawnGraceUntil = until;
            }
        }

        /// <summary>True enquanto a graça de spawn estiver ativa (todo dano recebido é ignorado).</summary>
        public static bool IsInSpawnGrace => Time.time < s_spawnGraceUntil;

        /// <summary>Fórmula central documentada: final = max(1, raw − Defense − resistência do tipo).</summary>
        public static int CalculateReducedDamage(int rawDamage, int defense, int resistance = 0)
        {
            if (rawDamage <= 0)
            {
                return 0;
            }

            return Mathf.Max(1, rawDamage - Mathf.Max(0, defense) - Mathf.Max(0, resistance));
        }

        /// <summary>Aplica dano reduzido ao player. Retorna o dano final aplicado.</summary>
        public static int ApplyDamage(PlayerManager playerManager, int rawDamage, string sourceId,
            DamageType damageType = DamageType.Physical, GameObject attacker = null,
            GameObject playerObject = null, bool isDamageOverTime = false)
        {
            if (playerManager == null || rawDamage <= 0)
            {
                return 0;
            }

            // Graça de spawn: ignora TODO dano por uma janela curta logo após (re)entrar numa cena ou
            // reviver — evita morrer instantaneamente por dano de contato ao spawnar colado num inimigo.
            if (IsInSpawnGrace)
            {
                CombatLog.Log($"CombatLog: PlayerDamageIgnoredSpawnGrace. Source={sourceId}, Raw={rawDamage}");
                return 0;
            }

            // fable_08: barreira arcana absorve ANTES de block/defesa/resistência (ordem documentada;
            // risco de dupla mitigação com Defense mitigado por teste). Dano totalmente absorvido = 0.
            rawDamage = Magic.PlayerBarrierState.AbsorbIncoming(rawDamage, Time.time);
            if (rawDamage <= 0)
            {
                CombatLog.Log($"CombatLog: PlayerDamageFullyAbsorbedByBarrier. Source={sourceId}");
                return 0;
            }

            // F27: block intercepta ANTES de defesa/resistência.
            var block = Player.Movement.PlayerBlockController.ActiveInstance;
            if (block != null && block.IsBlocking)
            {
                if (block.ResolveIncomingHitIsPerfect(Time.time))
                {
                    HandlePerfectBlock(rawDamage, sourceId, attacker);
                    return 0;
                }

                int incomingBlockDamage = rawDamage;
                rawDamage = Player.Movement.BlockTimingRules.MitigateNormalBlock(rawDamage);
                Core.GameEventBus.Publish(new PlayerNormalBlockEvent(
                    sourceId,
                    incomingBlockDamage,
                    rawDamage));
            }

            rawDamage = IncomingDamageModifierProvider.Resolve(
                rawDamage, damageType, Time.time, isDamageOverTime);
            rawDamage = CavebornCapstoneProvider.ResolveIncomingDamage(rawDamage, damageType);

            var defense = ResolveDefense();
            var resistanceSource = ResistanceProvider.Source;
            var resistance = resistanceSource != null ? resistanceSource(damageType) : 0;
            var finalDamage = CalculateReducedDamage(rawDamage, defense, resistance);
            // Flash antes do DamageHP: DamageHP pode disparar morte sincronamente, ocultando
            // o flash se acionado depois. playerObject = GO da cena (nao o Bootstrap GO).
            var flashTarget = playerObject ?? playerManager?.gameObject;
            var earlyFlash = flashTarget != null
                ? (flashTarget.GetComponentInChildren<HitFlashController>(true)
                   ?? flashTarget.GetComponentInParent<HitFlashController>())
                : null;
            if (earlyFlash != null) earlyFlash.Flash();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            PlayerDamageAuditLog.Record(Time.time, sourceId, damageType, rawDamage, finalDamage, playerManager.CurrentHP, playerManager.MaxHP);
#endif
            playerManager.DamageHP(finalDamage);
            CombatLog.Log($"CombatLog: PlayerDamageReceived. Source={sourceId}, Raw={rawDamage}, Defense={defense}, Resist={resistance}({damageType}), Final={finalDamage}");
            return finalDamage;
        }

        // F27: perfect block — dano 0, postura refletida no atacante melee, guard break canônico.
        private static void HandlePerfectBlock(int rawDamage, string sourceId, GameObject attacker)
        {
            string resolutionId = Interlocked.Increment(ref s_nextCombatResolutionId)
                .ToString(CultureInfo.InvariantCulture);
            CombatLog.Log($"CombatLog: PlayerPerfectBlock. Source={sourceId}, NegatedDamage={rawDamage}");
            Core.GameEventBus.Publish(new Core.Events.PlayerPerfectBlockEvent(sourceId, rawDamage, resolutionId));
            Core.GameEventBus.Publish(new Core.Events.PlayerActionFeedbackEvent("Perfect block!"));

            if (attacker == null)
            {
                return; // projéteis: sem reflexo (documentado na F27)
            }

            var posture = attacker.GetComponent<EnemyPostureState>();
            if (posture != null)
            {
                posture.ApplyPostureDamage(
                    rawDamage * Player.Movement.BlockTimingRules.PostureReflectFraction,
                    "perfect_block",
                    "player",
                    true,
                    resolutionId);
            }

            // Inimigo em GuardHold tem a guarda quebrada imediatamente (interação canônica).
            // arch: quebra do par mutuo Combat|Enemy — porta IEnemyBrainController em vez do tipo
            // concreto CindarsHope.Enemy.EnemyBrain/EnemyBrainState.
            var brain = attacker.GetComponent<IEnemyBrainController>();
            if (brain != null && brain.IsGuardHold)
            {
                brain.ApplyStun(1.0f);
            }
        }

        private static int ResolveDefense()
        {
            if (DefenseSource != null)
            {
                return DefenseSource();
            }

            // arch: quebra do par mutuo Combat|Skills (2026-07-15) — porta ISkillTreeRuntime em vez
            // do tipo concreto SkillTreeManager.
            var skillTree = DomainManagerRegistry.Get<ISkillTreeRuntime>();
            if (skillTree == null)
            {
                return 0;
            }

            // Defense derivado das passivas (equipment entra quando houver registry — F03 nota).
            var stats = DerivedStatsCalculator.Calculate(
                0, 0, 0, 0f, 0, 0f, 1f,
                equippedItems: null,
                passiveModifiers: skillTree.GetAllActivePassiveModifiers());
            return stats.Defense;
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// Dev-only: registra todo dano recebido pelo player e imprime um relatorio ao morrer.
    /// Janela deslizante de 5 minutos. Ativo em UNITY_EDITOR e DEVELOPMENT_BUILD.
    /// </summary>
    public static class PlayerDamageAuditLog
    {
        private const float WindowSeconds = 300f;

        private struct Entry
        {
            public float GameTime;
            public string SourceId;
            public DamageType DamageType;
            public int RawDamage;
            public int FinalDamage;
            public int HpBefore;
            public int MaxHP;
        }

        private static readonly List<Entry> s_entries = new List<Entry>();
        private static bool s_subscribed;

        /// <summary>Chamado por PlayerDamageReceiver antes de DamageHP. Ignora finalDamage <= 0.</summary>
        public static void Record(float gameTime, string sourceId, DamageType damageType,
            int rawDamage, int finalDamage, int hpBefore, int maxHp)
        {
            if (finalDamage <= 0) return;
            EnsureSubscribed();
            float cutoff = gameTime - WindowSeconds;
            s_entries.RemoveAll(e => e.GameTime < cutoff);
            s_entries.Add(new Entry
            {
                GameTime = gameTime,
                SourceId = sourceId ?? "unknown",
                DamageType = damageType,
                RawDamage = rawDamage,
                FinalDamage = finalDamage,
                HpBefore = hpBefore,
                MaxHP = maxHp,
            });
        }

        private static void EnsureSubscribed()
        {
            if (s_subscribed) return;
            s_subscribed = true;
            Core.GameEventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private static void OnPlayerDied(PlayerDiedEvent evt)
        {
            DumpReport(evt.SceneName);
        }

        private static void DumpReport(string sceneName)
        {
            var sb = new StringBuilder(2048);
            sb.AppendLine();
            sb.AppendLine("=== PLAYER DEATH AUDIT | cena: " + (sceneName ?? "?") + " | ultimos 5 min ===");
            sb.AppendLine("  #  |  +T     | Fonte                      | Tipo     | Raw | Final | HP");
            sb.AppendLine("-----|---------|----------------------------|----------|-----|-------|--------");

            if (s_entries.Count == 0)
            {
                sb.AppendLine("  (nenhum dano registrado na janela de 5 minutos)");
                Debug.Log(sb.ToString());
                return;
            }

            float t0 = s_entries[0].GameTime;
            int totalDamage = 0;
            Entry lastEntry = default;
            var sourceTotals = new Dictionary<string, int>();

            for (int i = 0; i < s_entries.Count; i++)
            {
                var e = s_entries[i];
                float rel = e.GameTime - t0;
                int mm = (int)(rel / 60f);
                int ss = (int)(rel % 60f);
                int hpAfter = Mathf.Max(0, e.HpBefore - e.FinalDamage);
                string src = e.SourceId.Length > 26 ? e.SourceId.Substring(0, 26) : e.SourceId;
                string typStr = e.DamageType.ToString();
                if (typStr.Length > 8) typStr = typStr.Substring(0, 8);
                sb.AppendLine(string.Format(" {0,3} | {1:00}:{2:00}  | {3,-26} | {4,-8} | {5,3} | {6,5} | {7}→{8}",
                    i + 1, mm, ss, src, typStr, e.RawDamage, e.FinalDamage, e.HpBefore, hpAfter));
                totalDamage += e.FinalDamage;
                lastEntry = e;
                if (!sourceTotals.ContainsKey(e.SourceId)) sourceTotals[e.SourceId] = 0;
                sourceTotals[e.SourceId] += e.FinalDamage;
            }

            string topSource = ""; int topDmg = 0;
            foreach (var kv in sourceTotals)
                if (kv.Value > topDmg) { topDmg = kv.Value; topSource = kv.Key; }

            int fatalHpAfter = Mathf.Max(0, lastEntry.HpBefore - lastEntry.FinalDamage);
            sb.AppendLine("-----|---------|----------------------------|----------|-----|-------|--------");
            sb.AppendLine("  Total: " + totalDamage + " dano em " + s_entries.Count + " hits  |  MaxHP: " + lastEntry.MaxHP);
            sb.AppendLine("  Fonte mais danosa: " + topSource + " (" + topDmg + " dmg total)");
            sb.AppendLine("  Golpe FATAL: " + lastEntry.SourceId + " (" + lastEntry.DamageType + ", " + lastEntry.FinalDamage + " dmg) — HP era " + lastEntry.HpBefore + " -> " + fatalHpAfter);
            sb.AppendLine("===");
            Debug.Log(sb.ToString());

            s_entries.Clear();
        }
    }
#endif
}
