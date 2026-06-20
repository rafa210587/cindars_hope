using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Cave.Traps
{
    /// <summary>
    /// fable_60 — consumidor RUNTIME do efeito de detecção do amuleto de Nyx (F23, CA-6). Anexado pelo
    /// materializer ao parent das armadilhas, com as armadilhas materializadas + o transform do player
    /// injetados (sem busca global de cena). A cada ~0.4s, se o efeito de detecção estiver ativo
    /// (<see cref="TrapDetectionService.IsDetectionActive"/>), revela o telegraph das armadilhas no
    /// raio (<see cref="TrapBehaviour.RevealByDetection"/> / <see cref="FalseChestTrap.RevealByDetection"/>).
    ///
    /// Sem o efeito (raio 0), o componente fica inerte: nada é revelado, e a flag permanece dormente
    /// como antes. A escala da grade vem do tamanho do mundo (1 célula = 1 unidade no GridToWorld).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrapDetectionRuntime : MonoBehaviour
    {
        private const float PollSeconds = 0.4f;

        private readonly List<TrapBehaviour> _traps = new List<TrapBehaviour>();
        private readonly List<FalseChestTrap> _falseChests = new List<FalseChestTrap>();
        private Transform _player;
        private float _nextPollAt;

        public void Configure(Transform player)
        {
            _player = player;
            _nextPollAt = 0f;
        }

        public void Register(TrapBehaviour trap)
        {
            if (trap != null)
            {
                _traps.Add(trap);
            }
        }

        public void Register(FalseChestTrap falseChest)
        {
            if (falseChest != null)
            {
                _falseChests.Add(falseChest);
            }
        }

        private void Update()
        {
            if (Time.time < _nextPollAt)
            {
                return;
            }

            _nextPollAt = Time.time + PollSeconds;

            var radius = TrapDetectionService.ActiveDetectionRadius();
            if (radius <= 0f || _player == null)
            {
                return; // efeito dormente (F23 inativo) — nada a revelar
            }

            var playerPos = _player.position;

            for (var i = 0; i < _traps.Count; i++)
            {
                var trap = _traps[i];
                if (trap == null || trap.State != TrapState.Armed || trap.IsDetected)
                {
                    continue;
                }

                if (WithinRadius(trap.transform.position, playerPos, radius))
                {
                    trap.RevealByDetection();
                }
            }

            for (var i = 0; i < _falseChests.Count; i++)
            {
                var chest = _falseChests[i];
                if (chest == null || chest.State != TrapState.Armed || chest.IsDetected)
                {
                    continue;
                }

                if (WithinRadius(chest.transform.position, playerPos, radius))
                {
                    chest.RevealByDetection();
                }
            }
        }

        private static bool WithinRadius(Vector3 trapPos, Vector3 playerPos, float radiusCells)
        {
            // 1 célula = 1 unidade de mundo (GridToWorld). Distância Manhattan em unidades.
            var dx = Mathf.Abs(trapPos.x - playerPos.x);
            var dy = Mathf.Abs(trapPos.y - playerPos.y);
            return (dx + dy) <= radiusCells;
        }
    }
}
