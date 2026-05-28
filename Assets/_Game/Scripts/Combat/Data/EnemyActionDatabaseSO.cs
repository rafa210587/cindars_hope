using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Combat
{
    [CreateAssetMenu(fileName = "EnemyActionDatabase", menuName = "CindarsHope/Combat/Enemy Action Database")]
    public class EnemyActionDatabaseSO : DataRegistrySO<EnemyActionSO>
    {
    }
}
