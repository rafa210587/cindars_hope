namespace CindarsHope.Farm.Runtime
{
    /// <summary>
    /// fable_65 — canal de entrega de recompensa de meta diária (ouro + XP).
    /// Abstrai os canais existentes do player (PlayerManager.AddGold / PlayerProgressionManager.AddXp)
    /// para que a decisão de pagamento (TryClaimReward) seja testável em EditMode sem managers vivos.
    /// NÃO é um segundo canal de economia: a implementação de produção apenas roteia para os
    /// canais únicos já existentes.
    /// </summary>
    public interface IDailyGoalRewardSink
    {
        void Grant(int gold, int xp);
    }
}
