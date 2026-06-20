using UnityEngine;

namespace CindarsHope.Combat
{
    [DisallowMultipleComponent]
    public class KnockbackController : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private float _duration = 0.15f;

        // fable_23 — opt-in para a resistência a knockback de acessório (Charm de Stoneheart -50%).
        // FALSE por padrão: inimigos recebem este componente via AddComponent e NÃO devem sofrer a
        // resistência do jogador. Só a instância do JOGADOR habilita a flag (wiring de cena/prefab),
        // garantindo que o ponto único de resistência (ApplyKnockback) só afete o jogador.
        [SerializeField] private bool _appliesAccessoryResist;

        public bool AppliesAccessoryResist
        {
            get => _appliesAccessoryResist;
            set => _appliesAccessoryResist = value;
        }

        private float _remainingTime;
        private Vector2 _velocity;

        private void Awake()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody2D>();
            }
        }

        private void FixedUpdate()
        {
            if (_remainingTime <= 0)
            {
                return;
            }

            _remainingTime -= Time.fixedDeltaTime;

            if (_rigidbody != null)
            {
                _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);
            }
            else
            {
                transform.position += (Vector3)_velocity * Time.fixedDeltaTime;
            }
        }

        public void ApplyKnockback(Vector2 direction, float force)
        {
            if (force <= 0f)
            {
                return;
            }

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            // fable_23 — ponto ÚNICO do KnockbackResistModifier. Só o jogador (opt-in) consulta o
            // roteador; inimigos mantêm a força original. Sem acessório => força inalterada (helper
            // trata default 0 como neutro). Clamp01 na redução garante que nunca inverte a direção.
            if (_appliesAccessoryResist)
            {
                force = CindarsHope.Equipment.AccessoryEffectRouter.ApplyKnockbackResist(force);
                if (force <= 0f)
                {
                    return;
                }
            }

            direction.Normalize();
            _velocity = direction * force;
            _remainingTime = _duration;
            Debug.Log($"KnockbackController: applying knockback on '{name}', force={force}.");
        }

        public void ApplyKnockback(KnockbackRequest request)
        {
            ApplyKnockback(request.Direction, request.Force);
        }

        public bool IsKnockingBack => _remainingTime > 0;
    }
}
