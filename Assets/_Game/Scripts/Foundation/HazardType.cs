// arch: quebra do ciclo Core|Player (spec_arch_core_player_cycle_reduction_v37) — enum puro (sem
// dependencia de engine) movido de CindarsHope.Player para Foundation, decisão explícita de
// arquitetura.
namespace CindarsHope.Foundation
{
    public enum HazardType
    {
        None,
        Heat,
        Cold,
        Toxic,
        Radiation
    }
}
