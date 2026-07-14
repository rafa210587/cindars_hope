namespace CindarsHope.Craft
{
    /// <summary>
    /// Port local do módulo Craft para abrir uma estação em uma UI concreta sem depender do módulo UI.
    /// </summary>
    public interface ICraftingStationModal
    {
        void Open(CraftingStation station);
    }
}
