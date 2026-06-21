using System;
using System.Collections.Generic;

namespace CindarsHope.Save
{
    /// <summary>
    /// fable_62 — secao aditiva de save dos hints de onboarding ja vistos (1x/save). Somente tipos
    /// simples (List&lt;string&gt; de ids estaveis), conforme ADR-0006 / save-dto-simple-types-only.
    /// Zero referencias Unity. Ausente em save legado = lista vazia = todos os hints elegiveis de
    /// novo (seguro, sem migration). Owner runtime: OnboardingHintService (padrao provider F07/F10).
    /// </summary>
    [Serializable]
    public class OnboardingHintsSaveData
    {
        public List<string> SeenHintIds = new List<string>();
    }
}
