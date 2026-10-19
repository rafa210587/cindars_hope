// arch: quebra do par mutuo Core|Player (2026-07-15) — porta pura (C# puro, sem dependencia da
// engine — Foundation nao pode referenciar a engine, ver ArchitectureRatchetTests) que
// permite a GameBootstrap e consumidores de gameplay resolverem e operarem os recursos essenciais
// do player sem nomear CindarsHope.Player.
// PlayerManager se anuncia via DomainManagerRegistry.Register<IPlayerRuntime>(this) (molde
// IEquipmentRuntime/ISkillTreeRuntime). Cobre o lifecycle chamado por GameBootstrap, a superfície
// mínima de HP para recompensas e a fábrica de CorpseRecoveryManager (antes construída via
// 'new' e agora delega para dentro do modulo Player, pois CorpseRecoveryManager e Player.Death —
// mesmo modulo de PlayerManager). Initialize(object) recebe o PlayerDataSO como object (nao o tipo
// de asset da engine) — o implementador casta internamente.
// A porta também expõe somente HP e restauração necessários por recompensas entre sistemas.
namespace CindarsHope.Foundation
{
    public interface IPlayerRuntime
    {
        int CurrentHP { get; }
        int MaxHP { get; }

        void Initialize();
        void Initialize(object playerData);
        void Shutdown();
        void RestoreHP(int amount);

        // Fabrica: constroi o CorpseRecoveryManager (Player.Death, mesmo modulo do PlayerManager) sem
        // que Core precise nomear o tipo concreto. Retorno 'object' pois CorpseRecoveryManager e uma
        // classe C# pura (nao MonoBehaviour) e Core so precisa segurar/expor a referencia.
        object CreateCorpseRecoveryManager(IInventoryRuntime inventoryManager, IEquipmentRuntime equipmentRuntime);
    }
}
