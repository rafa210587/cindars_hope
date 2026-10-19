namespace CindarsHope.Foundation
{
    /// <summary>
    /// arch: hook neutro para a durabilidade efetiva restaurada por reparo. Permite que o modulo
    /// Equipment aplique o bonus de eficiencia de reparo derivado (F18/fable_47) sem nomear o modulo
    /// Player. O Player registra a Source (que compoe o bonus atual com a formula pura no lado Player);
    /// Equipment apenas invoca. Sem Source registrada, o reparo usa o valor base (bonus tratado como
    /// zero) — mesmo fallback do codigo anterior.
    /// </summary>
    public static class RepairEfficiencyProvider
    {
        /// <summary>base restore -&gt; effective restore, com o bonus de eficiencia de reparo aplicado.</summary>
        public static System.Func<int, int> EffectiveRepairAmountSource;
    }
}
