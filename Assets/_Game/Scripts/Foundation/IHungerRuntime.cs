// arch: quebra do par mutuo Core|Player (2026-07-15) — porta pura (sem a engine — Foundation nao
// pode referenciar a engine, ver ArchitectureRatchetTests) que permite a GameBootstrap (Core)
// chamar Initialize do HungerManager sem nomear CindarsHope.Player. O campo serializado
// GameBootstrap._hungerManager passa a ser MonoBehaviour (molde ModalManager/SaveManager); esta porta
// e resolvida via cast local (_hungerManager as IHungerRuntime) so para a chamada de lifecycle.
// Initialize(object) recebe o PlayerDataSO como object (nao ScriptableObject); o implementador casta
// internamente. Consumidores fora de Core que precisem da API completa (RestoreHunger, CurrentHunger,
// etc.) resolvem o tipo concreto por cast local (ex.: bootstrap.HungerManager as HungerManager).
namespace CindarsHope.Foundation
{
    public interface IHungerRuntime
    {
        void Initialize(object playerData);
    }
}
