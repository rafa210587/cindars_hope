using CindarsHope.Cave.Generation;

namespace CindarsHope.Core.Events
{
    public sealed class CaveRuntimeMaterializationCompleteEvent
    {
        public readonly CaveGeneratedLevel GeneratedLevel;

        public CaveRuntimeMaterializationCompleteEvent(CaveGeneratedLevel generatedLevel)
        {
            GeneratedLevel = generatedLevel;
        }
    }
}
