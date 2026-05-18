using UnityEngine;

namespace CindarsHope.Combat
{
    [DisallowMultipleComponent]
    public class PlayerAttackController : MonoBehaviour
    {
        [SerializeField] private KeyCode _attackKey = KeyCode.J;
        [SerializeField] private int _punchDamage = 1;
        [SerializeField] private float _punchRange = 0.8f;
        [SerializeField] private float _attackCooldownSeconds = 0.4f;

        private float _lastAttackTime;

        private void Update()
        {
            if (Input.GetKeyDown(_attackKey))
            {
                Punch();
            }
        }

        private void Punch()
        {
            if (Time.time < _lastAttackTime + _attackCooldownSeconds)
            {
                return;
            }

            var hitColliders = Physics2D.OverlapCircleAll(transform.position, _punchRange);
            bool hitAny = false;

            foreach (var collider in hitColliders)
            {
                if (collider.gameObject == gameObject)
                {
                    continue;
                }

                var enemyHealth = collider.GetComponentInParent<EnemyHealth>();
                if (enemyHealth == null)
                {
                    enemyHealth = collider.GetComponent<EnemyHealth>();
                }

                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(_punchDamage);
                    Debug.Log($"PlayerAttackController: punch hit enemy {enemyHealth.gameObject.name} for {_punchDamage} damage.");

                    var knockback = enemyHealth.GetComponent<KnockbackController>();
                    if (knockback != null)
                    {
                        Vector2 direction = (enemyHealth.transform.position - transform.position).normalized;
                        knockback.ApplyKnockback(direction, 2.5f);
                    }

                    hitAny = true;
                }
            }

            if (!hitAny)
            {
                Debug.Log("PlayerAttackController: punch missed.");
            }

            _lastAttackTime = Time.time;
        }
    }
}
