namespace CindarsHope.Foundation
{
    /// <summary>
    /// arch: quebra do par mútuo Cave|Combat — porta pura (sem dependência de engine) que permite a
    /// EnemyHealth (Combat) consultar dados de boss e reportar sua morte via GetComponent no mesmo
    /// GameObject, sem nomear CindarsHope.Cave.Runtime.CaveBossDeathReporter. Implementada por
    /// CaveBossDeathReporter. Posição passada como 3 floats (não Vector3) para manter Foundation
    /// livre de tipos de engine.
    /// </summary>
    public interface ICaveBossReporter
    {
        string BossGateId { get; }
        int CaveLevel { get; }
        int CheckpointUnlockedOnDefeat { get; }
        void ReportDefeatedFromOwner(float deathPositionX, float deathPositionY, float deathPositionZ);
    }
}
