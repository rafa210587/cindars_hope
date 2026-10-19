// arch: quebra do par mutuo Core|Player (2026-07-15) — porta pura que permite a GameBootstrap (Core)
// chamar Initialize/Shutdown do StatusEffectManager sem nomear CindarsHope.Player. StatusEffectManager
// se anuncia via DomainManagerRegistry.Register<IStatusEffectRuntime>(this) (molde
// IEquipmentRuntime/ISkillTreeRuntime), alem do static Instance ja existente para consumidores fora de
// Core que precisem da API completa (TryAddEffect, HasEffect, etc.) via cast local.
namespace CindarsHope.Foundation
{
    public interface IStatusEffectRuntime
    {
        void Initialize();
        void Shutdown();
    }
}
