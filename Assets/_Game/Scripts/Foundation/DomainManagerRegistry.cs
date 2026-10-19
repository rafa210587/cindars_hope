// arch: infraestrutura para quebra de ciclo Core|Inventory (spec_arch_core_inventory_cycle_reduction_v36)
// — registry generico puro (C# puro, sem dependencia de engine) que permite a managers de dominio
// (ex.: InventoryManager) se anunciarem sem exigir static Instance/Active (proibido para InventoryManager
// pela regra de ratchet GlobalInventoryAccess) e sem exigir que Core mantenha referencia direta ao tipo
// do dominio.
namespace CindarsHope.Foundation
{
    public static class DomainManagerRegistry
    {
        private static readonly System.Collections.Generic.Dictionary<System.Type, object> Managers = new();

        public static void Register<T>(T instance) where T : class
        {
            if (instance == null)
            {
                throw new System.ArgumentNullException(nameof(instance));
            }

            Managers[typeof(T)] = instance;
        }

        public static void Unregister<T>(T instance) where T : class
        {
            if (instance == null)
            {
                return;
            }

            var type = typeof(T);
            if (Managers.TryGetValue(type, out var registered) && ReferenceEquals(registered, instance))
            {
                Managers.Remove(type);
            }
        }

        public static void Unregister<T>() where T : class => Managers.Remove(typeof(T));

        public static T Get<T>() where T : class => Managers.TryGetValue(typeof(T), out var v) ? (T)v : null;
    }
}
