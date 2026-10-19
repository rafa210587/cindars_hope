// arch: quebra do par mutuo Core|Player (2026-07-15) — porta pura que permite a GameBootstrap (Core)
// chamar Initialize do ManaManager sem nomear CindarsHope.Player. O campo serializado
// GameBootstrap._manaManager passa a ser MonoBehaviour (molde ModalManager/SaveManager); esta porta
// e resolvida via cast local (_manaManager as IManaRuntime) so para a chamada de lifecycle e para o
// self-heal em EnsureCombatRuntimeReferences. Consumidores fora de Core que precisem da API completa
// (TrySpendMana, RestoreMana, etc.) resolvem o tipo concreto por cast local
// (ex.: bootstrap.ManaManager as ManaManager).
namespace CindarsHope.Foundation
{
    public interface IManaRuntime
    {
        void Initialize();
    }
}
