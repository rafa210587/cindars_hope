using UnityEngine;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// fable_83 — Pathing leve: desvio local de obstaculo via whiskers/raycasts curtos.
    /// Logica pura de decisao (Steer) testavel em EditMode sem scene.
    /// A aplicacao de Raycast com a layer real acontece no EnemyBrain (MoveChase); os
    /// helpers de decisao aqui so operam sobre direcoes e resultados de hit passados pelo chamador.
    /// Sem navmesh/A*; determinisico; histerese para evitar jitter.
    /// </summary>
    public static class EnemyLocalAvoidance
    {
        // ── Tuning (no-magic-balance-values) ────────────────────────────────────────────────────

        /// <summary>Comprimento dos whiskers em tiles.</summary>
        public const float WhiskerLength = 1.4f;

        /// <summary>Angulo de abertura dos whiskers laterais em relacao a direcao desejada.</summary>
        public const float WhiskerAngleDeg = 35f;

        /// <summary>
        /// Multiplicador de velocidade durante o contorno.
        /// Mantido acima de 0.5 para que o inimigo nao trave completamente.
        /// </summary>
        public const float AvoidanceSpeedMultiplier = 0.8f;

        /// <summary>
        /// Histerese: o desvio so termina quando o caminho central fica livre por esta distancia.
        /// Previne jitter quando o inimigo oscila na borda do obstaculo.
        /// </summary>
        public const float HysteresisExitDistance = 0.4f;

        // ── Estrutura de resultado ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Resultado da decisao de desvio; passado de volta para o EnemyBrain aplicar no rb.linearVelocity.
        /// </summary>
        public readonly struct AvoidanceResult
        {
            /// <summary>True se desvio esta ativo este frame.</summary>
            public readonly bool IsAvoiding;

            /// <summary>Direcao corrigida (normalizada) para aplicar. Vector2.zero = sem movimento.</summary>
            public readonly Vector2 Direction;

            public AvoidanceResult(bool isAvoiding, Vector2 direction)
            {
                IsAvoiding = isAvoiding;
                Direction = direction;
            }
        }

        // ── Calculo de posicoes dos whiskers ─────────────────────────────────────────────────────

        /// <summary>
        /// Retorna as direcoes dos 3 whiskers: central, esquerda, direita.
        /// Puramente geometrico — sem raycast aqui.
        /// </summary>
        public static void GetWhiskerDirections(
            Vector2 desiredDirection,
            out Vector2 centerDir,
            out Vector2 leftDir,
            out Vector2 rightDir)
        {
            centerDir = desiredDirection.normalized;
            float angleRad = WhiskerAngleDeg * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);

            // Rotacao no sentido horario (direita)
            rightDir = new Vector2(
                centerDir.x * cos + centerDir.y * sin,
                -centerDir.x * sin + centerDir.y * cos).normalized;

            // Rotacao no sentido anti-horario (esquerda)
            leftDir = new Vector2(
                centerDir.x * cos - centerDir.y * sin,
                centerDir.x * sin + centerDir.y * cos).normalized;
        }

        /// <summary>
        /// Decide a direcao de desvio com base em quais whiskers acertaram obstaculo.
        /// <paramref name="centerHit"/>, <paramref name="leftHit"/>, <paramref name="rightHit"/>:
        /// true se o whisker correspondente acertou geometria de colisao.
        /// <paramref name="currentlyAvoiding"/>: histerese — true se o inimigo ja estava desviando.
        /// Retorna o resultado de direcao corrigida para este tick.
        /// </summary>
        public static AvoidanceResult Steer(
            Vector2 desiredDirection,
            bool centerHit,
            bool leftHit,
            bool rightHit,
            bool currentlyAvoiding)
        {
            // Histerese: se ja estava desviando e o caminho central ainda esta bloqueado, mantem.
            bool shouldAvoid = centerHit || (currentlyAvoiding && centerHit);

            if (!centerHit && !leftHit && !rightHit)
            {
                // Caminho completamente livre — andar direto.
                return new AvoidanceResult(false, desiredDirection.normalized);
            }

            if (!centerHit)
            {
                // Obstaculo apenas nos whiskers laterais — ir em frente com leve redução.
                return new AvoidanceResult(false, desiredDirection.normalized);
            }

            // Caminho central bloqueado: escolher lado menos obstruido.
            // Se um lado esta livre, desviar para ele. Se ambos obstruidos, tentar contorno perpendicular.
            if (!rightHit && leftHit)
            {
                // Lado direito livre — virar para direita
                Vector2 rightContour = new Vector2(desiredDirection.y, -desiredDirection.x).normalized;
                return new AvoidanceResult(true, (desiredDirection + rightContour).normalized);
            }

            if (!leftHit && rightHit)
            {
                // Lado esquerdo livre — virar para esquerda
                Vector2 leftContour = new Vector2(-desiredDirection.y, desiredDirection.x).normalized;
                return new AvoidanceResult(true, (desiredDirection + leftContour).normalized);
            }

            // Ambos os lados bloqueados ou ambos livres com centro bloqueado.
            // Tenta contorno pela direita (determinisico; sem Random).
            {
                Vector2 rightContour = new Vector2(desiredDirection.y, -desiredDirection.x).normalized;
                return new AvoidanceResult(true, rightContour);
            }
        }

        // ── Integracao com Physics2D (chamada pelo EnemyBrain, nao testada em EditMode) ────────────

        /// <summary>
        /// Executa os 3 raycasts de Physics2D e chama Steer com os resultados.
        /// O EnemyBrain chama este metodo em MoveChase/MoveChargeLine quando nao esta
        /// em leap/dash/evasao. A layer de colisao e passada via parametro (sem hardcode).
        /// </summary>
        public static AvoidanceResult SteerWithRaycast(
            Vector2 origin,
            Vector2 desiredDirection,
            LayerMask obstacleLayer,
            bool currentlyAvoiding,
            float whiskerLength = WhiskerLength)
        {
            GetWhiskerDirections(desiredDirection, out var centerDir, out var leftDir, out var rightDir);

            bool centerHit = Physics2D.Raycast(origin, centerDir, whiskerLength, obstacleLayer).collider != null;
            bool leftHit   = Physics2D.Raycast(origin, leftDir,   whiskerLength, obstacleLayer).collider != null;
            bool rightHit  = Physics2D.Raycast(origin, rightDir,  whiskerLength, obstacleLayer).collider != null;

            return Steer(desiredDirection, centerHit, leftHit, rightHit, currentlyAvoiding);
        }
    }
}
