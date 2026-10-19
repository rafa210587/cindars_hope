using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Loot
{
    /// <summary>
    /// fable_06 — Registro de <see cref="LootTableSO"/> indexado por <c>TableId</c>
    /// (= <c>EnemyDataSO.lootTableId</c>). Segue o padrão <see cref="DataRegistrySO{T}"/> dos
    /// demais bancos (Item/Enemy/Vulnerability) — sem FindObjectOfType, resolução por id estável.
    /// O <c>EnemyDropSpawner</c> consome este banco para resolver a tabela de um inimigo morto.
    /// </summary>
    [CreateAssetMenu(fileName = "LootTableDatabase", menuName = "CindarsHope/Data/Loot Table Database")]
    public class LootTableDatabaseSO : DataRegistrySO<LootTableSO>
    {
    }
}
