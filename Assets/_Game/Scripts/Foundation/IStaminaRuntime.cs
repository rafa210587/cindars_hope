// arch: quebra do par mutuo Core|Player (2026-07-15) — porta pura que permite a GameBootstrap (Core)
// chamar lifecycle e aplicar recompensas de stamina sem nomear CindarsHope.Player. O campo serializado
// GameBootstrap._staminaManager passa a ser MonoBehaviour (molde ModalManager/SaveManager); esta
// porta e resolvida via cast local (_staminaManager as IStaminaRuntime). A superfície permanece
// limitada a lifecycle, leitura do recurso e crédito; débito continua no owner concreto.
namespace CindarsHope.Foundation
{
    public interface IStaminaRuntime
    {
        int CurrentStamina { get; }
        int MaxStamina { get; }

        void Initialize();
        void Shutdown();
        void AddStamina(int amount);
    }
}
