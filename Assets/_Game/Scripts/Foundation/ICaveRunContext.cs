namespace CindarsHope.Foundation
{
    /// <summary>
    /// Porta neutra para o contexto de run da cave (identidade opaca, seed + nivel corrente), consumida por
    /// codigo fora do modulo Cave sem nomea-lo. CaveRunManager registra a implementacao no
    /// DomainManagerRegistry. Consumidor le com fallback (seed vazio / nivel 0) quando ausente.
    /// </summary>
    public interface ICaveRunContext
    {
        string CaveRunId { get; }
        string CaveRunSeed { get; }
        int CurrentCaveLevel { get; }
    }
}
