using System;
using System.Collections.Generic;
using CindarsHope.Cave.Generation;
using CindarsHope.Equipment;
using UnityEngine;

namespace CindarsHope.Cave.Traps
{
    /// <summary>
    /// fable_60 — consumidor REAL do efeito de detecção de armadilhas do amuleto de Nyx (F23, CA-6).
    ///
    /// O efeito era uma flag DORMANTE (sem consumidor) em fable_23. Aqui ele ganha um leitor: o raio
    /// (em células) vem do ponto único <see cref="AccessoryEffectRouter.TrapDetectionRadiusSource"/>
    /// (não-stack, agregado pelos slots de acessório). Com o efeito ativo (raio &gt; 0), armadilhas
    /// dentro do raio do player revelam o telegraph (<c>TrapDetectedEvent</c>); sem o efeito (raio 0),
    /// nada muda — a detecção fica inerte e o jogo segue como antes.
    ///
    /// O núcleo (<see cref="ResolveDetectedTraps"/>) é PURO: recebe o raio explicitamente, então um
    /// teste pode ligar/desligar a flag sinteticamente sem cena nem acessórios (CA-6).
    /// </summary>
    public static class TrapDetectionService
    {
        /// <summary>Raio de detecção ativo (células), lido do ponto único da F23. 0 = sem detecção.</summary>
        public static float ActiveDetectionRadius()
        {
            return AccessoryEffectRouter.TrapDetectionRadiusSource?.Invoke() ?? 0f;
        }

        /// <summary>true quando há QUALQUER detecção de armadilha ativa (efeito F23 não-dormente).</summary>
        public static bool IsDetectionActive()
        {
            return ActiveDetectionRadius() > 0f;
        }

        /// <summary>
        /// Subconjunto de armadilhas (do plano) cuja célula está dentro de <paramref name="radiusCells"/>
        /// da <paramref name="playerCell"/> (distância Manhattan). PURO e determinístico — independe de
        /// cena. radius &lt;= 0 => lista vazia (flag desligada). Só considera armadilhas ainda armadas
        /// (estados Triggered/Disarmed não interessam ao aviso de detecção).
        /// </summary>
        public static List<CaveTrapPlacement> ResolveDetectedTraps(
            CaveTrapPlan plan,
            Vector2Int playerCell,
            float radiusCells,
            IReadOnlyDictionary<string, TrapState> stateById = null)
        {
            var detected = new List<CaveTrapPlacement>();
            if (plan == null || plan.Traps == null || radiusCells <= 0f)
            {
                return detected;
            }

            var radius = Mathf.FloorToInt(radiusCells);
            foreach (var trap in plan.Traps)
            {
                if (trap == null)
                {
                    continue;
                }

                if (stateById != null
                    && stateById.TryGetValue(trap.TrapInstanceId, out var state)
                    && state != TrapState.Armed)
                {
                    continue; // já disparada/desarmada — sem aviso de detecção
                }

                var distance = Math.Abs(trap.Cell.x - playerCell.x) + Math.Abs(trap.Cell.y - playerCell.y);
                if (distance <= radius)
                {
                    detected.Add(trap);
                }
            }

            return detected;
        }
    }
}
