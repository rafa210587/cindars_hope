namespace CindarsHope.Cave.Runtime
{
    public interface IVisitedLevelSnapshot
    {
        int CaveLevel { get; }
        string SnapshotId { get; }
        bool IsValid();
    }
}
