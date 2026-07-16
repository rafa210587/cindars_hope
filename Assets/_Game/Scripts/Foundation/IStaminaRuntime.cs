// arch: quebra do par mutuo Core|Player (2026-07-15) — porta pura que permite a GameBootstrap (Core)
// chamar Initialize/Shutdown do StaminaManager sem nomear CindarsHope.Player. O campo serializado
// GameBootstrap._staminaManager passa a ser MonoBehaviour (molde ModalManager/SaveManager); esta
// porta e resolvida via cast local (_staminaManager as IStaminaRuntime) so para as chamadas de
// lifecycle. Consumidores fora de Core que precisem da API completa (TrySpendStamina, AddStamina,
// etc.) resolvem o tipo concreto por cast local (ex.: bootstrap.StaminaManager as StaminaManager).
namespace CindarsHope.Foundation
{
    public interface IStaminaRuntime
    {
        void Initialize();
        void Shutdown();
    }
}
