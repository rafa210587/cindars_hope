// arch: quebra do ciclo Core|UI (spec_arch_core_ui_cycle_reduction_v38) — port C# puro, independente
// de engine, que permite a Core.GameTimeManager consultar se ha um modal ativo (para pausar o tick de
// tempo) sem referenciar CindarsHope.UI.Modal diretamente. ModalManager (em CindarsHope.UI.Modal)
// implementa esta interface e se anuncia via DomainManagerRegistry.Register<IModalStateProvider>(this)
// em Awake.
namespace CindarsHope.Foundation
{
    public interface IModalStateProvider
    {
        bool HasActiveModal { get; }
    }
}
