// arch: infraestrutura para quebra de ciclo Core|Inventory (spec_arch_core_inventory_cycle_reduction_v36)
// — registry generico puro (C# puro, sem dependencia de engine) que permite a managers de dominio
// (ex.: InventoryManager) se anunciarem sem exigir static Instance/Active (proibido para InventoryManager
// pela regra de ratchet GlobalInventoryAccess) e sem exigir que Core mantenha referencia direta ao tipo
// do dominio.
namespace CindarsHope.Foundation
{
    public static class DomainManagerRegistry
    {
        static readonly System.Collections.Generic.Dictionary<System.Type, object> _m = new();

        public static void Register<T>(T instance) where T : class => _m[typeof(T)] = instance;

        public static void Unregister<T>() where T : class => _m.Remove(typeof(T));

        public static T Get<T>() where T : class => _m.TryGetValue(typeof(T), out var v) ? (T)v : null;
    }
}
