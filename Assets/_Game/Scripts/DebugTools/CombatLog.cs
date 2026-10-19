namespace CindarsHope.DebugTools
{
    /// <summary>
    /// Gate central para os logs informativos verbosos de combate (e geração/materialização
    /// de caverna, que reusam o mesmo canal). Default OFF para não floodar o Console.
    ///
    /// Uso: substitua os <c>Debug.Log($"CombatLog: ...")</c> crus por
    /// <c>CombatLog.Log($"CombatLog: ...", this)</c>. O prefixo "CombatLog:" continua
    /// DENTRO da mensagem; este helper apenas gateia a emissão.
    ///
    /// NÃO use para warnings/errors reais — esses (Debug.LogWarning/LogError) continuam
    /// sempre visíveis, sem gate.
    ///
    /// Para ligar em debug: <c>CombatLog.Verbose = true;</c> (ex.: via console de dev ou
    /// um breakpoint). Permanece false em uso normal.
    /// </summary>
    public static class CombatLog
    {
        /// <summary>Quando false (default), <see cref="Log"/> não emite nada.</summary>
        public static bool Verbose = false;

        public static void Log(string message, UnityEngine.Object context = null)
        {
            if (!Verbose)
            {
                return;
            }

            if (context != null)
            {
                UnityEngine.Debug.Log(message, context);
            }
            else
            {
                UnityEngine.Debug.Log(message);
            }
        }
    }
}
