namespace CindarsHope.Foundation
{
    /// <summary>
    /// Port puro para aplicar modificadores de ouro de venda sem acoplar Economy a Equipment.
    /// </summary>
    public interface IGoldGainModifierRuntime
    {
        int ApplyGoldGain(int baseGold);
    }
}
