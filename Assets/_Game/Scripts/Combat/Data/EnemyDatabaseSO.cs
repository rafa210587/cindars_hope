using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Combat.Data
{
    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "CindarsHope/Combat/Enemy Database")]
    public sealed class EnemyDatabaseSO : DataRegistrySO<EnemyDataSO>
    {
    }
}
