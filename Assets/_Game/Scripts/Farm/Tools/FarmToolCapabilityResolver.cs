using System.Collections.Generic;

namespace CindarsHope.Farm.Tools
{
    public class FarmToolCapabilityResolver
    {
        private readonly Dictionary<string, FarmToolDefinition> _defs;

        public FarmToolCapabilityResolver(Dictionary<string, FarmToolDefinition> defs)
        {
            _defs = defs ?? new Dictionary<string, FarmToolDefinition>();
        }

        public bool CanAccessResourceNode(string toolId, FarmToolTier requiredTier)
        {
            if (!_defs.TryGetValue(toolId, out var def)) return false;
            return def.Tier >= requiredTier;
        }

        public float GetStaminaModifier(string toolId)
        {
            if (!_defs.TryGetValue(toolId, out var def)) return 1.0f;
            return def.BaseStaminaCostModifier;
        }

        public int GetAreaOfEffect(string toolId)
        {
            if (!_defs.TryGetValue(toolId, out var def)) return 1;
            return def.AreaOfEffect;
        }

        public bool IsEndgameReserved(string toolId)
        {
            if (!_defs.TryGetValue(toolId, out var def)) return false;
            return def.IsEndgameReserved;
        }
    }
}
