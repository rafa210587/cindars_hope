using System.Collections.Generic;

namespace CindarsHope.Crafting
{
    /// <summary>
    /// fable_49 — armazém PURO de receitas APRENDIDAS por slug estável (ex.: recipe_unlock_mithril_work).
    /// Receitas de tier alto começam BLOQUEADAS; o boss do gate as ensina via first-kill. Idempotente:
    /// re-aprender o mesmo slug é no-op (re-kill não duplica). Sem refs Unity; capturado/restaurado de
    /// forma aditiva no save (lista de strings; ausência = nada aprendido, save legado intacto).
    ///
    /// O CraftingService consulta <see cref="IsUnlocked"/> antes de aceitar o craft de uma receita
    /// gated. O hook de combate chama <see cref="Unlock"/> — se F06/F33 ainda não publicar o evento de
    /// first-kill no momento, o serviço continua expondo Unlock(id) e o hook fica como integração
    /// documentada (nunca unlock automático silencioso).
    /// </summary>
    public sealed class RecipeUnlockService
    {
        private readonly HashSet<string> _unlocked = new HashSet<string>();

        /// <summary>Instância ativa para consulta única do gating de craft (registrada pelo dono em runtime).</summary>
        public static RecipeUnlockService Active { get; set; }

        public int Count => _unlocked.Count;

        /// <summary>Receita disponível? Slug vazio = "sem gating" => sempre disponível (receitas atuais inalteradas).</summary>
        public bool IsUnlocked(string recipeUnlockId)
        {
            if (string.IsNullOrWhiteSpace(recipeUnlockId)) return true;
            return _unlocked.Contains(Normalize(recipeUnlockId));
        }

        /// <summary>Aprende a receita (idempotente). Retorna true só na PRIMEIRA vez (para 1× de evento/toast).</summary>
        public bool Unlock(string recipeUnlockId)
        {
            if (string.IsNullOrWhiteSpace(recipeUnlockId)) return false;
            return _unlocked.Add(Normalize(recipeUnlockId));
        }

        public void Clear(string recipeUnlockId)
        {
            if (string.IsNullOrWhiteSpace(recipeUnlockId)) return;
            _unlocked.Remove(Normalize(recipeUnlockId));
        }

        public void ClearAll()
        {
            _unlocked.Clear();
        }

        // ─── Save (aditivo: lista de slugs) ─────────────────────────────────────────────────────

        /// <summary>Snapshot serializável (lista vazia => save legado sem receitas aprendidas).</summary>
        public List<string> CaptureUnlockedIds()
        {
            return new List<string>(_unlocked);
        }

        /// <summary>Restaura de um snapshot. Lista null/vazia (save legado) => registro vazio, sem migration.</summary>
        public void RestoreUnlockedIds(List<string> data)
        {
            _unlocked.Clear();
            if (data == null) return;
            foreach (var id in data)
            {
                if (string.IsNullOrWhiteSpace(id)) continue;
                _unlocked.Add(Normalize(id));
            }
        }

        private static string Normalize(string id) => id.Trim().ToLowerInvariant();
    }
}
