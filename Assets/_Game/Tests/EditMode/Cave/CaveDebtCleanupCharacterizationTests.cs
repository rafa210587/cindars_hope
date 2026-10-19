using CindarsHope.Cave.Death;
using CindarsHope.Combat;
using CindarsHope.Player.Death;
using CindarsHope.Player.Movement;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// fable_66 — testes de CARACTERIZAÇÃO que pinam o comportamento dos débitos ANTES e DEPOIS do
    /// fechamento (CA-5). Cobrem: carimbos de morte reais (dia/hora/layout), regra de replace de
    /// corpse, pipeline central de dano (que substitui o PlayerHitEvent aposentado) e a regra pura
    /// de mitigação de block. Todos puros — sem MonoBehaviour, sem Play Mode.
    /// </summary>
    public class CaveDebtCleanupCharacterizationTests
    {
        // ---------------------------------------------------------------- carimbos de morte (CA-1)

        [Test]
        public void ResolveGameDay_UsesRealTimeOwner_NotHardcodedOne()
        {
            // ANTES: GetCurrentGameDay() retornava 1 fixo. DEPOIS: repassa o dia real do TimeManager.
            Assert.AreEqual(7, CaveCorpseStamp.ResolveGameDay(7), "Dia real do owner deve ser preservado.");
            Assert.AreEqual(42, CaveCorpseStamp.ResolveGameDay(42));
        }

        [Test]
        public void ResolveGameDay_FloorsToOne_WhenOwnerZeroOrNegative()
        {
            // Owner ausente/zerado degrada para o dia inicial 1 (nunca grava 0 corrompido).
            Assert.AreEqual(1, CaveCorpseStamp.ResolveGameDay(0));
            Assert.AreEqual(1, CaveCorpseStamp.ResolveGameDay(-5));
        }

        [Test]
        public void ResolveGameTime_NormalizesIntoDayRange_NeverNegative()
        {
            Assert.AreEqual(0f, CaveCorpseStamp.ResolveGameTime(0f), 0.001f, "Sem relógio intra-dia ainda: 0 é válido.");
            Assert.AreEqual(0f, CaveCorpseStamp.ResolveGameTime(-3f), 0.001f, "Negativo degrada para 0.");
            Assert.AreEqual(13.5f, CaveCorpseStamp.ResolveGameTime(13.5f), 0.001f);
            Assert.AreEqual(1f, CaveCorpseStamp.ResolveGameTime(25f), 0.001f, "25h normaliza para 1h.");
        }

        [Test]
        public void ResolveLayoutHash_IsDeterministicPerSeedAndLevel()
        {
            // ADR-0005: revisitar o MESMO nível na MESMA run produz o MESMO hash (sem GUID/timestamp).
            var a = CaveCorpseStamp.ResolveLayoutHash("world", "run", 3);
            var b = CaveCorpseStamp.ResolveLayoutHash("world", "run", 3);
            Assert.AreEqual(a, b, "Mesmo seed+nível ⇒ mesmo hash (estável).");
            Assert.IsNotEmpty(a, "Run ativa ⇒ hash não vazio (antes: sempre vazio).");
        }

        [Test]
        public void ResolveLayoutHash_DiffersPerLevel_AndIsEmptyWhenNoRun()
        {
            var level1 = CaveCorpseStamp.ResolveLayoutHash("world", "run", 1);
            var level2 = CaveCorpseStamp.ResolveLayoutHash("world", "run", 2);
            Assert.AreNotEqual(level1, level2, "Níveis diferentes ⇒ hashes diferentes.");
            Assert.AreEqual(string.Empty, CaveCorpseStamp.ResolveLayoutHash(string.Empty, string.Empty, 1),
                "Sem run (seeds vazias) ⇒ sem hash.");
        }

        // ---------------------------------------------------------------- replace de corpse (CA-1)

        [Test]
        public void CorpseReplace_OnlyWhenNewIdDiffers()
        {
            // Caracteriza o contrato que CorpseRecoveryManager.SetActiveCorpse aplica (e que o
            // ReplaceActiveCorpse morto do resolver NÃO precisava reimplementar).
            Assert.IsTrue(CorpseReplaceDecision.ShouldReplace("corpse_old", "corpse_new"),
                "Id diferente ⇒ substitui + publica CorpseReplacedEvent.");
            Assert.IsFalse(CorpseReplaceDecision.ShouldReplace("corpse_x", "corpse_x"),
                "Mesmo id ⇒ não substitui.");
            Assert.IsFalse(CorpseReplaceDecision.ShouldReplace(null, "corpse_new"),
                "Sem corpse anterior ⇒ não há replace (apenas set).");
        }

        // ---------------------------------------------------- pipeline central de dano (CA-2)

        [Test]
        public void PlayerDamagePipeline_StillReducesByDefense_AfterPlayerHitEventRetired()
        {
            // O PlayerHitEvent foi aposentado; o dano ao player flui pelo caminho central
            // PlayerDamageReceiver (chamado direto por EnemyBrain/contato/projétil). A fórmula
            // documentada continua: final = max(1, raw - defense - resist).
            Assert.AreEqual(7, PlayerDamageReceiver.CalculateReducedDamage(10, 3), "10 - 3 = 7.");
            Assert.AreEqual(1, PlayerDamageReceiver.CalculateReducedDamage(2, 9), "Floor 1 (dano nunca zera com raw > 0).");
            Assert.AreEqual(0, PlayerDamageReceiver.CalculateReducedDamage(0, 5), "Raw 0 ⇒ 0.");
            Assert.AreEqual(4, PlayerDamageReceiver.CalculateReducedDamage(10, 3, 3), "Resistência também reduz.");
        }

        // ------------------------------------------------------------ block / stamina (CA-3)

        [Test]
        public void BlockMitigation_HalvesDamage_FloorOne_Unchanged()
        {
            // Comportamento de block COM stamina wired preservado byte-a-byte (pinado).
            Assert.AreEqual(5, BlockTimingRules.MitigateNormalBlock(10));
            Assert.AreEqual(1, BlockTimingRules.MitigateNormalBlock(1), "Floor 1.");
            Assert.AreEqual(0, BlockTimingRules.MitigateNormalBlock(0));
        }
    }
}
