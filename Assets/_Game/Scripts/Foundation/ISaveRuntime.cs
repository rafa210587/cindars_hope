// arch: quebra do par mutuo Core|Save (2026-07-15) — porta puro (sem dependencia de outros
// modulos) que permite a GameBootstrap.SaveManager (Core) expor save/load/hotbar aos consumidores
// existentes (UI, SceneManagement, Editor) sem nomear CindarsHope.Save.SaveManager. SaveManager
// implementa esta interface com as mesmas assinaturas (implementacao implicita, sem mudanca de
// logica). Os metodos Rebind*/Initialize/Shutdown de SaveManager NAO entram aqui porque suas
// assinaturas tomam tipos concretos de outros modulos (Player, Inventory, Cave, Farm) — coloca-los
// nesta porta criaria pares mutuos novos Foundation|Player/Inventory/Core (esses modulos ja
// referenciam Foundation). Consumidores que precisam desses membros resolvem o SaveManager
// concreto por outra via (cast local, fora de Core) ou, no caso do proprio GameBootstrap, via
// IGameBootstrapRuntimeService (Core.Bootstrap.GameBootstrapRuntimeContext).
namespace CindarsHope.Foundation
{
    public interface ISaveRuntime
    {
        bool SaveGame();
        bool LoadGame();
        HotbarState HotbarState { get; }
    }
}
