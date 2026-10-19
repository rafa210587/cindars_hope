using System.Collections.Generic;

namespace CindarsHope.Core.Data
{
    public interface IDataRegistry<T> where T : IIdentifiedData
    {
        bool TryGetById(string id, out T data);
        T GetRequired(string id);
        IReadOnlyCollection<T> All { get; }
    }
}
