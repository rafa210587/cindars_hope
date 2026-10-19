namespace CindarsHope.Crafting
{
    /// <summary>
    /// fable_49 — indireção PURA entre o CraftingManager e o RecipeUnlockService, para o gating de craft
    /// de tier alto sem dependência dura do static no fluxo de craft. Decisão de segurança: uma receita
    /// COM slug de unlock exigido é tratada como BLOQUEADA quando não há serviço ativo (fail-closed) —
    /// receitas de tier alto "começam bloqueadas". Receita SEM slug nunca passa por aqui (sempre liberada).
    ///
    /// Permite override em teste (Resolver) para validar bloqueado/desbloqueado sem cena.
    /// </summary>
    public static class CraftingRecipeGate
    {
        /// <summary>Override de teste: dado o slug, devolve se está desbloqueado. Null => usa o serviço ativo.</summary>
        public static System.Func<string, bool> Resolver { get; set; }

        public static bool IsRecipeUnlocked(string requiredRecipeUnlockId)
        {
            if (string.IsNullOrWhiteSpace(requiredRecipeUnlockId)) return true; // sem gating

            if (Resolver != null) return Resolver(requiredRecipeUnlockId);

            var service = RecipeUnlockService.Active;
            if (service == null) return false; // fail-closed: gated sem serviço => bloqueado
            return service.IsUnlocked(requiredRecipeUnlockId);
        }
    }
}
