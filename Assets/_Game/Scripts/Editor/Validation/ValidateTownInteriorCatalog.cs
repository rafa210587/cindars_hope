using System;
using System.Linq;
using CindarsHope.City.Interiors;
using CindarsHope.Editor.SceneCreation;
using CindarsHope.NPC.Services;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateTownInteriorCatalog
    {
        [MenuItem("CindarsHope/Validation/Validate Town Interior Catalog")]
        public static void Validate()
        {
            int errors=0,warnings=0;
            var draft=GenerateTownInteriorCatalog.BuildDefinitions();
            try
            {
                try{GenerateTownInteriorCatalog.ValidateManifest(draft);}
                catch(Exception error){Fail("authored manifest: "+error.Message,ref errors);}
                try{GenerateTownInteriorCatalog.ValidateProviderAssets(draft);}
                catch(Exception error){Fail("provider source: "+error.Message,ref errors);}
                var catalog=AssetDatabase.LoadAssetAtPath<TownInteriorCatalogSO>(GenerateTownInteriorCatalog.CatalogPath);
                if(catalog==null)Fail("catalog missing; run CindarsHope/Generate/Data/Town Interior Catalog",ref errors);
                var guids=AssetDatabase.FindAssets("t:TownInteriorDefinitionSO",new[]{GenerateTownInteriorCatalog.DataFolder});
                if(guids.Length!=24)Fail("expected 24 materialized definitions, found "+guids.Length,ref errors);
                var paths=guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
                var assetIds=paths.Select(path=>AssetDatabase.LoadAssetAtPath<TownInteriorDefinitionSO>(path)?.HouseId).ToArray();
                if(assetIds.Any(string.IsNullOrEmpty)||assetIds.Distinct(StringComparer.Ordinal).Count()!=assetIds.Length)
                    Fail("empty or duplicate materialized HouseId",ref errors);
                foreach(var expected in draft)
                {
                    string path=GenerateTownInteriorCatalog.PathFor(expected.HouseId);
                    var actual=AssetDatabase.LoadAssetAtPath<TownInteriorDefinitionSO>(path);
                    if(actual==null){Fail(expected.HouseId+": missing asset at "+path,ref errors);continue;}
                    if(AssetDatabase.AssetPathToGUID(path)==string.Empty)Fail(expected.HouseId+": missing GUID",ref errors);
                    if(actual.HouseId!=expected.HouseId)Fail(expected.HouseId+": asset HouseId mismatch",ref errors);
                    if(JsonUtility.ToJson(actual)!=JsonUtility.ToJson(expected))
                        Fail(expected.HouseId+": content diverges from approved programme or ownership",ref errors);
                    if(catalog==null||!catalog.TryGet(expected.HouseId,out var indexed)||indexed!=actual)
                        Fail(expected.HouseId+": orphan or wrong catalog reference",ref errors);
                }
                if(catalog!=null)
                {
                    if(catalog.Definitions==null||catalog.Definitions.Length!=24||catalog.Definitions.Any(d=>d==null)||
                        catalog.Definitions.Select(d=>d.HouseId).Distinct(StringComparer.Ordinal).Count()!=24)
                        Fail("catalog registry has missing, duplicate or dangling references",ref errors);
                    if(catalog.UnhousedOutdoorServiceIds==null||
                        !catalog.UnhousedOutdoorServiceIds.SequenceEqual(GenerateTownInteriorCatalog.UnhousedOutdoorServiceIds))
                        Fail("unhoused Yael/Zrix service metadata diverges",ref errors);
                    if(catalog.UnhousedOutdoorProviderNpcIds==null||
                        !catalog.UnhousedOutdoorProviderNpcIds.SequenceEqual(GenerateTownInteriorCatalog.UnhousedOutdoorProviderNpcIds))
                        Fail("unhoused Yael/Zrix external provider metadata diverges",ref errors);
                }
                foreach(string path in paths)
                    if(!draft.Any(d=>GenerateTownInteriorCatalog.PathFor(d.HouseId)==path))
                        Fail("orphan definition asset "+path,ref errors);
                int houses=catalog?.Definitions?.Length??0;
                int residences=catalog?.Definitions?.Count(d=>d!=null&&d.ResidentNpcIds!=null&&d.ResidentNpcIds.Length>0)??0;
                int residents=catalog?.Definitions?.Sum(d=>d?.ResidentNpcIds?.Length??0)??0;
                int indoor=catalog?.Definitions?.Sum(d=>d?.PrimaryIndoorProviderNpcIds?.Length??0)??0;
                int outdoorProviders=(catalog?.Definitions?.Sum(d=>d?.PrimaryOutdoorProviderNpcIds?.Length??0)??0)+
                    (catalog?.UnhousedOutdoorProviderNpcIds?.Length??0);
                int personalBeds=catalog?.Definitions?.Sum(d=>d?.Props?.Count(p=>p!=null&&p.PropId.Contains("/bed_npc_"))??0)??0;
                int guestBeds=catalog?.Definitions?.Sum(d=>d?.Props?.Count(p=>p!=null&&p.PropId.Contains("/GuestBed_Inn"))??0)??0;
                if(houses!=24||residences!=21||residents!=23||indoor!=13||outdoorProviders!=10||personalBeds!=23||guestBeds!=2||
                    GenerateTownInteriorCatalog.CanonicalOutdoorHomeNpcIds.Length!=5||NpcServiceCatalog.ServiceCount!=8)
                    Fail($"census houses={houses} residences={residences} residents={residents} indoor={indoor} outdoorProviders={outdoorProviders} beds={personalBeds}+{guestBeds}",ref errors);
                string verdict=errors==0?"PASS":"FAIL";
                Debug.Log($"TOWN_INTERIOR_CATALOG: {verdict} houses={houses} residences={residences} residents={residents} outdoor=5 indoorProviders={indoor} outdoorProviders={outdoorProviders} personalBeds={personalBeds} guestBeds={guestBeds} errors={errors} warnings={warnings}");
                if(errors>0)throw new InvalidOperationException("Town Interior Catalog failed with "+errors+" error(s).");
            }
            finally{foreach(var definition in draft)UnityEngine.Object.DestroyImmediate(definition);}
        }

        private static void Fail(string detail,ref int errors)
        {errors++;Debug.LogError("[TownInteriorCatalog] "+detail);}
    }
}
