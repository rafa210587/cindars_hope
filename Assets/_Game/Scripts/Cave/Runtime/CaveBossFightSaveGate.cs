namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// fable_44 — política de save em boss fight (CA-3). Estado puro testável (sem Unity): rastreia se
    /// uma boss fight do nível está ATIVA e responde se o save manual é permitido. O
    /// <see cref="CindarsHope.Cave.CaveLevelRuntimeController"/> é dono de uma instância (set no spawn
    /// do boss; clear em derrota do boss, morte do player, saída do nível/caverna) e o SaveManager lê
    /// <c>CanSaveNow()</c> no ponto ÚNICO de save manual.
    ///
    /// O flag é idempotente e nunca fica órfão: TODOS os caminhos de fim chamam <see cref="EndBossFight"/>
    /// (derrota, morte, saída de nível, saída da caverna) — ver os testes em CaveMultiLevelSaveTests.
    /// Fora de boss fight, o save NUNCA é bloqueado por este gate.
    /// </summary>
    public sealed class CaveBossFightSaveGate
    {
        /// <summary>Mensagem de recusa no canal de feedback existente (GameSavedEvent.Message). Sem UI nova.</summary>
        public const string SaveBlockedReason = "Nao e possivel salvar agora.";

        private int _activeBossLevel = -1;

        /// <summary>Verdadeiro enquanto uma boss fight do nível está ativa (save manual bloqueado).</summary>
        public bool IsBossFightActive { get; private set; }

        /// <summary>Nível da boss fight ativa, ou -1 se nenhuma. Diagnóstico/escopo do clear por nível.</summary>
        public int ActiveBossLevel => _activeBossLevel;

        /// <summary>Marca a boss fight do nível como ativa (idempotente). Chamado quando um boss real spawna.</summary>
        public void BeginBossFight(int caveLevel)
        {
            IsBossFightActive = true;
            _activeBossLevel = caveLevel;
        }

        /// <summary>
        /// Limpa o flag (idempotente). Chamado em TODOS os caminhos de fim: derrota do boss, morte do
        /// player, saída do nível, saída da caverna. Limpar quando já inativo é no-op seguro.
        /// </summary>
        public void EndBossFight()
        {
            IsBossFightActive = false;
            _activeBossLevel = -1;
        }

        /// <summary>True se o save manual é permitido agora (i.e., nenhuma boss fight ativa).</summary>
        public bool CanSaveNow()
        {
            return !IsBossFightActive;
        }
    }
}
