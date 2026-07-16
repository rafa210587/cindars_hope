// arch: quebra do par mutuo Core|Player (2026-07-15) — porta pura (C# puro, sem dependencia da
// engine — Foundation nao pode referenciar a engine, ver ArchitectureRatchetTests) que
// permite a GameBootstrap (Core) resolver e operar o PlayerManager sem nomear CindarsHope.Player.
// PlayerManager se anuncia via DomainManagerRegistry.Register<IPlayerRuntime>(this) (molde
// IEquipmentRuntime/ISkillTreeRuntime). Cobre apenas os membros efetivamente chamados por
// GameBootstrap (lifecycle) mais a fabrica de CorpseRecoveryManager (que GameBootstrap construia via
// 'new' e agora delega para dentro do modulo Player, pois CorpseRecoveryManager e Player.Death —
// mesmo modulo de PlayerManager). Initialize(object) recebe o PlayerDataSO como object (nao o tipo
// de asset da engine) — o implementador casta internamente.
// Consumidores fora de Core que precisem da API completa (AddGold, DamageHP, etc.) resolvem o tipo
// concreto por cast local (ex.: bootstrap.PlayerManager as PlayerManager), permitido pois esses
// modulos ja podem nomear CindarsHope.Player.
namespace CindarsHope.Foundation
{
    public interface IPlayerRuntime
    {
        void Initialize();
        void Initialize(object playerData);
        void Shutdown();

        // Fabrica: constroi o CorpseRecoveryManager (Player.Death, mesmo modulo do PlayerManager) sem
        // que Core precise nomear o tipo concreto. Retorno 'object' pois CorpseRecoveryManager e uma
        // classe C# pura (nao MonoBehaviour) e Core so precisa segurar/expor a referencia.
        object CreateCorpseRecoveryManager(IInventoryRuntime inventoryManager, IEquipmentRuntime equipmentRuntime);
    }
}
