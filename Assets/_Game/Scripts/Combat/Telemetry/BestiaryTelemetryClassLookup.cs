using System.Collections.Generic;
using CindarsHope.Combat.Bestiary;

namespace CindarsHope.Combat.Telemetry
{
    /// <summary>
    /// fable_59 — adaptador que resolve enemyId → <see cref="TelemetryCreatureClass"/> usando o
    /// registry canônico do bestiário F33 (<see cref="CanonicalBestiaryCatalog"/>). NÃO cria tabela
    /// paralela de bandas/papéis: lê a fonte de verdade existente. Classe pura (sem Unity).
    ///
    /// Mapeamento de papel → classe de alvo §6-§7:
    ///   IsBoss            → Boss
    ///   IsMiniBoss        → Miniboss
    ///   Role == Elite     → Elite
    ///   caso contrário    → Common
    ///   enemyId desconhecido → Unknown (alvo NoTarget; vira gap nominal no relatório).
    /// </summary>
    public sealed class BestiaryTelemetryClassLookup : ICreatureClassLookup
    {
        private readonly Dictionary<string, TelemetryCreatureClass> _byEnemyId;

        public BestiaryTelemetryClassLookup()
            : this(CanonicalBestiaryCatalog.All)
        {
        }

        /// <summary>Construtor testável: aceita uma lista de fichas arbitrária.</summary>
        public BestiaryTelemetryClassLookup(IEnumerable<BestiaryCreatureDef> fichas)
        {
            _byEnemyId = new Dictionary<string, TelemetryCreatureClass>();
            if (fichas == null)
            {
                return;
            }

            foreach (var ficha in fichas)
            {
                if (string.IsNullOrEmpty(ficha.EnemyId))
                {
                    continue;
                }

                _byEnemyId[ficha.EnemyId] = ClassifyFicha(ficha);
            }
        }

        public TelemetryCreatureClass GetClass(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId))
            {
                return TelemetryCreatureClass.Unknown;
            }

            return _byEnemyId.TryGetValue(enemyId, out var creatureClass)
                ? creatureClass
                : TelemetryCreatureClass.Unknown;
        }

        /// <summary>Regra pura de classificação (papel/flags da ficha → classe de alvo).</summary>
        public static TelemetryCreatureClass ClassifyFicha(BestiaryCreatureDef ficha)
        {
            if (ficha.IsBoss || ficha.Role == EnemyRole.Boss)
            {
                return TelemetryCreatureClass.Boss;
            }

            if (ficha.IsMiniBoss || ficha.Role == EnemyRole.MiniBoss)
            {
                return TelemetryCreatureClass.Miniboss;
            }

            if (ficha.Role == EnemyRole.Elite)
            {
                return TelemetryCreatureClass.Elite;
            }

            return TelemetryCreatureClass.Common;
        }
    }
}
