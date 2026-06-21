using System.Collections.Generic;
using CindarsHope.Core.Bootstrap;
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
        /// <summary>Fonte de Defense (injetável p/ testes; default = passivas via bootstrap).</summary>
        public static System.Func<int> DefenseSource;

        /// <summary>F18: fonte de resistência por tipo de dano (setada pelo PlayerVitalsApplier).</summary>
        public static System.Func<DamageType, int> ResistanceSource;

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
        public static int ApplyDamage(PlayerManager playerManager, int rawDamage, string sourceId, DamageType damageType = DamageType.Physical, GameObject attacker = null)
        {
            if (playerManager == null || rawDamage <= 0)
            {
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

                rawDamage = Player.Movement.BlockTimingRules.MitigateNormalBlock(rawDamage);
            }

            var defense = ResolveDefense();
            var resistance = ResistanceSource != null ? ResistanceSource(damageType) : 0;
            var finalDamage = CalculateReducedDamage(rawDamage, defense, resistance);
            playerManager.DamageHP(finalDamage);
            CombatLog.Log($"CombatLog: PlayerDamageReceived. Source={sourceId}, Raw={rawDamage}, Defense={defense}, Resist={resistance}({damageType}), Final={finalDamage}");
            return finalDamage;
        }

        // F27: perfect block — dano 0, postura refletida no atacante melee, guard break canônico.
        private static void HandlePerfectBlock(int rawDamage, string sourceId, GameObject attacker)
        {
            CombatLog.Log($"CombatLog: PlayerPerfectBlock. Source={sourceId}, NegatedDamage={rawDamage}");
            Core.GameEventBus.Publish(new Core.Events.PlayerPerfectBlockEvent(sourceId, rawDamage));
            Core.GameEventBus.Publish(new Core.Events.PlayerActionFeedbackEvent("Perfect block!"));

            if (attacker == null)
            {
                return; // projéteis: sem reflexo (documentado na F27)
            }

            var posture = attacker.GetComponent<EnemyPostureState>();
            if (posture != null)
            {
                posture.ApplyPostureDamage(rawDamage * Player.Movement.BlockTimingRules.PostureReflectFraction);
            }

            // Inimigo em GuardHold tem a guarda quebrada imediatamente (interação canônica).
            var brain = attacker.GetComponent<CindarsHope.Enemy.EnemyBrain>();
            if (brain != null && brain.CurrentState == CindarsHope.Enemy.EnemyBrainState.GuardHold)
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

            var bootstrap = GameBootstrap.Instance;
            var skillTree = bootstrap != null ? bootstrap.SkillTreeManager : null;
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
}
