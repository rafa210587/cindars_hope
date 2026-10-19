using System;
using UnityEngine;

namespace CindarsHope.City.Interiors
{
    public sealed class TownInteriorCatalogSO : ScriptableObject
    {
        public TownInteriorDefinitionSO[] Definitions = Array.Empty<TownInteriorDefinitionSO>();
        // Yael and Zrix remain outdoors without inventing a home or provider inside a house.
        public string[] UnhousedOutdoorServiceIds = Array.Empty<string>();
        public string[] UnhousedOutdoorProviderNpcIds = Array.Empty<string>();

        public bool TryGet(string houseId, out TownInteriorDefinitionSO definition)
        {
            if (!string.IsNullOrEmpty(houseId) && Definitions != null)
                foreach (var candidate in Definitions)
                    if (candidate != null && string.Equals(candidate.HouseId, houseId, StringComparison.Ordinal))
                    { definition = candidate; return true; }
            definition = null;
            return false;
        }
    }
}
