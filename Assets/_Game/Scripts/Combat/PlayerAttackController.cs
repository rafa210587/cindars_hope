using UnityEngine;

namespace CindarsHope.Combat
{
    [DisallowMultipleComponent]
    public class PlayerAttackController : MonoBehaviour
    {
        [SerializeField] private KeyCode _attackKey = KeyCode.J;
        [SerializeField] private Collider2D _attackCollider;
        [SerializeField] private float _attackDamage = 3f;
        [SerializeField] private float _attackRange = 1.5f;

        private void Update()
        {
            if (Input.GetKeyDown(_attackKey))
            {
                Attack();
            }
        }

        private void Attack()
        {
            var hitColliders = Physics2D.OverlapCircleAll(transform.position, _attackRange);

            foreach (var collider in hitColliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    var enemyHealth = collider.GetComponent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage((int)_attackDamage);
                    }
                }
            }
        }
    }
}
