using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>Measured native support bands, not alpha/canvas bounds. Town-only Editor data.</summary>
    public static class TownKeyartComponentSupportCatalog
    {
        public const string WorldRoot = "Assets/_Game/Art/Generated/World/";

        public sealed class Recipe
        {
            public readonly string Asset, Sha256, Category, Observation;
            public string Id => Asset;
            public readonly int Width, Height;
            // Native pixel edges, origin at top left. A rectangle is a projected floor support.
            public readonly Rect[] Supports;
            public readonly float PixelTolerance;
            public Recipe(string asset, int width, int height, string sha256, string category,
                string observation, float tolerance, params Rect[] supports)
            {
                Asset = WorldRoot + asset; Width = width; Height = height; Sha256 = sha256;
                Category = category; Observation = observation; PixelTolerance = tolerance; Supports = supports;
            }
        }

        public static readonly IReadOnlyList<Recipe> All = new[]
        {
            new Recipe("trees/tree_oak.png",477,531,"6A27D707D86078DF0D917D0A9456284E394286D4C244E3A6DD831F262D21D1D7",
                "TreeTrunk","Root fan only; foliage and elevated trunk are not floor barriers.",3,new Rect(154,466,194,65)),
            new Recipe("trees/tree_pine.png",338,549,"2AC9CF4A7A46C4C52D8082FF806D25680DA91BBA7EDB320338BC152ADDCD6425",
                "TreeTrunk","Lowest root fan; lower crown remains passable behind.",3,new Rect(110,505,120,44)),
            new Recipe("trees/tree_apple.png",423,531,"311AE6C8B8BEA1AB71D1A1D5C3E6B41B75504151AF5A0B44E9FCDA0F690F7D05",
                "TreeTrunk","Root fan only, excluding forked upper trunk and crown.",3,new Rect(139,472,148,59)),
            new Recipe("props/bench.png",84,64,"B512DBC3F7B5BC214D342F77063120128BE6F8B5B9D0EEAAFE88D9325A97A388",
                "Bench","Seat support and both feet, excluding backrest.",1,new Rect(4,42,76,22)),
            new Recipe("props/crate.png",61,64,"DDEE4A3CEF8D09276829615462010CA9E46745446BF2C38811C15F9CF271FB8D",
                "Crate","Lower floor support; upper lid is not the contact plane.",1,new Rect(2,42,57,22)),
            new Recipe("props/hay_bale.png",58,64,"038269E205DFECA2B2043A7EAA8D914A1FF29C0C4F3BE814671A55F171C14DBE",
                "HayBale","Lower compressed base of the round bale.",1,new Rect(4,43,49,20)),
            new Recipe("interior/bed.png",61,64,"753F48AF3B49522020891FAAB3028284772D4C997BD2534047D0D05ACEAE6DA6",
                "Bed","Lower mattress/bed frame and feet; headboard excluded.",1,new Rect(6,35,53,29)),
            new Recipe("interior/table.png",61,64,"E0EB8CCC347786D2A29993243251B68AA28387771CD409FEC693D3E6AB2FCB34",
                "Armchair","Filename mismatch: image is an armchair, not a table.",1,new Rect(5,41,51,23)),
            new Recipe("interior/cupboard.png",86,64,"A183858DB162E536D0CF2B6CF006F9C8D54E7BC0813308D46225606C5B1BBA6A",
                "FloorRug","Filename mismatch: image is a flat rug; explicitly no solid.",1),
            new Recipe("interior/kitchen_counter.png",98,64,"50E302CC95A9B763BBD82B03EE12BC09C806782F389EA0C0DF0CDF390F6D11FD",
                "Table","Table and legs; can be reused as a simple counter by explicit Town wiring.",1,new Rect(4,32,90,32)),
            new Recipe("interior/fireplace.png",59,64,"5AE85CEA8D17BF84D72C9C2508CDBD5F9CEBD7BA5355EFADE8491CE3641D955B",
                "Fireplace","Lower stone hearth, not chimney height.",1,new Rect(2,45,55,19)),
            new Recipe("interior/chair.png",49,64,"F9CD9EB45C7952C0D57758972ECF38753C5529EB3A890DCC379119312FDC9454",
                "Cupboard","Filename mismatch: tall cupboard; its bottom plinth is solid.",1,new Rect(1,47,47,17)),
            new Recipe("interior/rug.png",43,64,"7EF918A560CBA5F3C0E481B540814F8D83A554E74353D34CBDFA621DFA5B5056",
                "Chair","Filename mismatch: wooden chair; seat support and feet are solid.",1,new Rect(5,38,35,26)),
            new Recipe("props/forge.png",432,445,"EACE16E6804ECA527FD836282B01839ECD36693533FE65980096D874AF57253C",
                "Forge","Two distinct supports: stone hearth and barrel/anvil. Suspended tools excluded.",3,
                new Rect(13,349,240,84),new Rect(279,361,124,71)),
            new Recipe("props/workbench.png",466,398,"ACA77BC970EF3DF73030BB87197D5E24F1BA308E6D8C701B4DD235F964F8906F",
                "Workbench","Lower two feet and cross support; upright tools excluded.",3,new Rect(25,341,394,48)),
            new Recipe("props/cooking_station.png",406,411,"486FB1036B5A3006C6E5350063AAFECCC5D1837553ABA504F61439C4603A30B2",
                "CookingStation","Bottom stone range footprint, not cauldron or utensils.",3,new Rect(12,349,384,52))
        };

        public static Recipe Find(string path)
        {
            foreach (var recipe in All) if (recipe.Asset == path) return recipe;
            return null;
        }

        /// <summary>Deterministic, enumerable catalog identity source used by the Town census.</summary>
        public static IEnumerable<string> EligibleIds()
        {
            foreach (var recipe in All) yield return recipe.Id;
        }

        /// <summary>Reviewable Town mapping. Returns a proposal; does not load or change assets.</summary>
        public static string ProposedInteriorAsset(string objectName)
        {
            switch (objectName)
            {
                case "Bed": case "GuestBed_Inn": return WorldRoot + "interior/bed.png";
                case "Table": case "Furniture_Altar": case "Furniture_KitchenCounter":
                case "Furniture_ServiceCounter": return WorldRoot + "interior/kitchen_counter.png";
                case "Furniture_Shelf": case "Furniture_Cupboard": return WorldRoot + "interior/chair.png";
                case "Furniture_Rug": return WorldRoot + "interior/cupboard.png";
                case "Furniture_Pew": return WorldRoot + "props/bench.png";
                case "Furniture_Stove": return WorldRoot + "interior/fireplace.png";
                case "Station_Forge": return WorldRoot + "props/forge.png";
                case "Station_CookingStation": return WorldRoot + "props/cooking_station.png";
                // Town currently has no dedicated inspected alchemy/sewing prop source;
                // these stations use the measured native machine silhouettes until their
                // bespoke art is authored. The binding keeps the station IDs/colliders stable.
                case "Station_Alchemy": return WorldRoot + "props/cooking_station.png";
                case "Station_Sewing": return WorldRoot + "props/workbench.png";
                case "Station_Carpentry": case "Station_Workbench": return WorldRoot + "props/workbench.png";
                default: return null;
            }
        }
    }
}
