using System;
using System.Collections.Generic;

namespace CindarsHope.Loot
{
    /// <summary>
    /// fable_06 — Resolvedor PURO (sem Unity, sem UnityEngine.Random) que rola uma
    /// <see cref="LootTableSO"/> de forma DETERMINÍSTICA por seed (ADR-0005 / cave-stable-run).
    ///
    /// Contrato central:
    ///   <c>Roll(table, seed)</c> sempre produz o MESMO resultado para o mesmo (table, seed).
    /// O seed de cada inimigo é derivado de <c>CaveRunSeed</c> + <c>EnemyInstanceId</c> via
    /// <see cref="BuildLootSeed"/>, de modo que revisitar a MESMA instância na MESMA run produz
    /// o MESMO loot (até ser coletado); runs diferentes (seed diferente) produzem distribuições
    /// diferentes. Sem GUID/timestamp na rolagem.
    ///
    /// NÃO é um segundo sistema de loot: consome a <see cref="LootTableSO"/> existente. O caminho
    /// legado (dropItemId fixo) continua no <c>EnemyDropSpawner</c> quando não há tabela.
    /// </summary>
    public static class EnemyLootResolver
    {
        /// <summary>
        /// Salt estável usado ao compor o seed de loot a partir do run seed + instance id.
        /// Constante (não timestamp/GUID) — exigência do ADR-0005.
        /// </summary>
        public const string LootSalt = "loot";

        /// <summary>
        /// Hash determinístico FNV-1a (mesma implementação de CaveEnemySpawnPlanner.StableHash).
        /// Reimplementado aqui para manter o resolver PURO (sem dependência de Cave.Runtime),
        /// porém com paridade bit-a-bit garantida pelos testes.
        /// </summary>
        public static int StableHash(string value)
        {
            unchecked
            {
                const int fnvOffset = (int)2166136261;
                const int fnvPrime = 16777619;
                var hash = fnvOffset;
                var s = value ?? string.Empty;
                for (int i = 0; i < s.Length; i++)
                {
                    hash ^= s[i];
                    hash *= fnvPrime;
                }
                return hash == int.MinValue ? 0 : hash;
            }
        }

        /// <summary>
        /// Compõe o seed de loot estável de um inimigo:
        /// StableHash("{caveRunSeed}|{enemyInstanceId}|loot").
        /// Mesma run + mesma instância => mesmo seed => mesmo loot.
        /// </summary>
        public static int BuildLootSeed(string caveRunSeed, string enemyInstanceId)
        {
            return StableHash($"{caveRunSeed}|{enemyInstanceId}|{LootSalt}");
        }

        /// <summary>
        /// Rola a tabela de forma determinística. Ordem:
        ///   1. GuaranteedEntries (sempre, respeitando MinAmount..MaxAmount);
        ///   2. UMA escolha ponderada entre Entries (respeitando Weight e DropChance da entrada);
        ///   3. Essência da banda (se EssenceItemId definido) com a chance comum/elite.
        /// Entradas inválidas (id vazio / weight &lt;= 0) são ignoradas com aviso no warnings list.
        /// </summary>
        /// <param name="table">Tabela a rolar (pode ser null → resultado vazio).</param>
        /// <param name="seed">Seed determinístico (use <see cref="BuildLootSeed"/>).</param>
        /// <param name="isElite">Inimigo elite (aumenta a chance de essência).</param>
        /// <param name="isMinibossOrBoss">Miniboss/boss (essência garantida).</param>
        /// <param name="warnings">Lista opcional para coletar avisos de entradas inválidas.</param>
        public static List<LootDrop> Roll(
            LootTableSO table,
            int seed,
            bool isElite = false,
            bool isMinibossOrBoss = false,
            List<string> warnings = null)
        {
            var result = new List<LootDrop>();
            if (table == null)
            {
                warnings?.Add("EnemyLootResolver.Roll: table is null.");
                return result;
            }

            // System.Random é determinístico para um dado seed (independente de plataforma p/ a
            // sequência de Next()), diferente de UnityEngine.Random (estado global compartilhado).
            var rng = new Random(seed);

            // 1) Drops garantidos (material comum da família).
            RollGuaranteed(table, table.GuaranteedEntries, rng, result, warnings);

            // 2) Uma escolha ponderada entre as entradas principais.
            RollOneWeighted(table, table.Entries, rng, result, warnings);

            // 3) Essência elemental da banda (canon: 8% comum / +25% elite / 100% miniboss/boss).
            RollEssence(table, rng, isElite, isMinibossOrBoss, result);

            return result;
        }

        private static void RollGuaranteed(
            LootTableSO table, LootTableEntry[] entries, Random rng, List<LootDrop> result, List<string> warnings)
        {
            if (entries == null) return;
            foreach (var entry in entries)
            {
                if (!IsValid(entry, table, warnings, "guaranteed")) continue;
                // DropChance respeitado mesmo em "garantido" só se < 1 (default 1 = sempre).
                if (entry.DropChance < 1f && rng.NextDouble() > entry.DropChance) continue;
                int amount = RollAmount(entry, rng);
                if (amount > 0) AddOrMerge(result, entry.ItemId, amount);
            }
        }

        private static void RollOneWeighted(
            LootTableSO table, LootTableEntry[] entries, Random rng, List<LootDrop> result, List<string> warnings)
        {
            if (entries == null || entries.Length == 0) return;

            int totalWeight = 0;
            foreach (var entry in entries)
            {
                if (!IsValid(entry, table, warnings, "weighted")) continue;
                totalWeight += Math.Max(0, entry.Weight);
            }
            if (totalWeight <= 0) return;

            int roll = rng.Next(1, totalWeight + 1);
            int cursor = 0;
            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId) || entry.Weight <= 0) continue;
                cursor += entry.Weight;
                if (roll > cursor) continue;

                // Entrada selecionada — aplicar DropChance (raros podem "falhar" e não dropar nada).
                if (entry.DropChance < 1f && rng.NextDouble() > entry.DropChance) return;
                int amount = RollAmount(entry, rng);
                if (amount > 0) AddOrMerge(result, entry.ItemId, amount);
                return;
            }
        }

        private static void RollEssence(
            LootTableSO table, Random rng, bool isElite, bool isMinibossOrBoss, List<LootDrop> result)
        {
            if (string.IsNullOrWhiteSpace(table.EssenceItemId)) return;

            float chance;
            if (isMinibossOrBoss)
            {
                chance = 1f;
            }
            else
            {
                chance = table.EssenceCommonChance;
                if (isElite) chance += table.EssenceEliteBonusChance;
            }

            if (chance >= 1f || rng.NextDouble() <= chance)
            {
                AddOrMerge(result, table.EssenceItemId, 1);
            }
        }

        private static int RollAmount(LootTableEntry entry, Random rng)
        {
            int min = Math.Max(1, entry.MinAmount);
            int max = Math.Max(min, entry.MaxAmount);
            return rng.Next(min, max + 1);
        }

        private static bool IsValid(LootTableEntry entry, LootTableSO table, List<string> warnings, string bucket)
        {
            if (entry == null)
            {
                warnings?.Add($"{table.name}: null {bucket} entry ignored.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(entry.ItemId))
            {
                warnings?.Add($"{table.name}: {bucket} entry with empty ItemId ignored.");
                return false;
            }
            if (entry.Weight <= 0)
            {
                warnings?.Add($"{table.name}/{entry.ItemId}: {bucket} entry Weight<=0 ignored.");
                return false;
            }
            return true;
        }

        private static void AddOrMerge(List<LootDrop> result, string itemId, int amount)
        {
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i].ItemId == itemId)
                {
                    result[i] = new LootDrop(itemId, result[i].Amount + amount);
                    return;
                }
            }
            result.Add(new LootDrop(itemId, amount));
        }
    }

    /// <summary>Resultado imutável de uma rolagem: par (itemId, amount).</summary>
    public readonly struct LootDrop
    {
        public readonly string ItemId;
        public readonly int Amount;

        public LootDrop(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }

    /// <summary>
    /// fable_06 — PONTO DE EXTENSÃO NOMEADO para modificar o ouro dropado por inimigos.
    ///
    /// A spec pede: "se a F29 tiver hook IGoldDropModifier, consuma-o; senão, deixe ponto de
    /// extensão nomeado." Auditoria (Phase 0): o hook NÃO existe em código, mas é citado como
    /// conceito canônico em <c>docs/game_rules/economy_rules.md</c> (§"economy hooks
    /// (IGoldDropModifier, etc.)"). Esta interface fixa o contrato esperado; nenhum sistema o
    /// implementa ainda (ouro de inimigo comum não está no escopo desta spec — drops vão ao
    /// inventário). Quando a economia de ouro de drop for ligada, implemente-a e injete no
    /// caminho de drop. Não há fan-out de eventos aqui — apenas a forma do contrato.
    /// </summary>
    public interface IGoldDropModifier
    {
        /// <summary>Ajusta o ouro base de um drop de inimigo. Implementações devem ser puras/determinísticas.</summary>
        int ModifyGoldDrop(string enemyId, int baseGold);
    }
}
