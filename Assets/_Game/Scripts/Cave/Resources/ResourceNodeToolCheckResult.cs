namespace CindarsHope.Cave.Resources
{
    public readonly struct ResourceNodeToolCheckResult
    {
        public readonly bool CanHarvest;
        public readonly bool ShouldFallback;
        public readonly string Message;

        public ResourceNodeToolCheckResult(bool canHarvest, bool shouldFallback, string message)
        {
            CanHarvest = canHarvest;
            ShouldFallback = shouldFallback;
            Message = message ?? string.Empty;
        }
    }
}
