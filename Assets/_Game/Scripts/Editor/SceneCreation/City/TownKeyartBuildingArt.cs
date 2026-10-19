using CindarsHope.Editor.Art;
using UnityEngine;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>Maps isolated building art to the existing walk-in shell and its physical doorway.</summary>
    public static class TownKeyartBuildingArt
    {
        public static Sprite Resolve(string houseName)
        {
            switch (houseName)
            {
                case "House_Fishery": return WorldSpriteLibrary.Building("town_watermill_native_scale1_v03_facade");
                case "House_Bakery": return WorldSpriteLibrary.Building("town_bakery_native_scale1_v03_facade");
                case "House_AlchemyLab": return WorldSpriteLibrary.Building("town_alchemy_native_v06_facade");
                case "House_Residential_4": return WorldSpriteLibrary.Building("town_courtyard_native_architecture_facade");
                case "House_CarvalhoTorto":
                case "House_Tovin":
                case "House_Manor":
                case "House_Archive": return WorldSpriteLibrary.Building("town_house_hipped_stone_native_v01_facade");
                case "House_Residential_1":
                case "House_Dagna":
                case "House_Pip":
                case "House_Workshop": return WorldSpriteLibrary.Building("town_house_l_dormer_native_v01_facade");
                case "House_AnimalYard": return WorldSpriteLibrary.Building("town_animal_shed_keyart");
                case "House_Temple":
                case "House_Inn":
                case "House_Blacksmith": return null;
                default:
                    // District hierarchy is intentional and stable, not a name-hash colour accident.
                    string color = houseName == "House_Residential_2" || houseName == "House_Residential_3" ? "blue" :
                        houseName == "House_Chamber" || houseName == "House_Archive" || houseName == "House_Workshop" ||
                        houseName == "House_CarvalhoTorto" || houseName == "House_Tovin" || houseName == "House_Pip" ? "green" : "red";
                    return WorldSpriteLibrary.Building("town_house_" + color + "_gable_keyart");
            }
        }

        // Approved Town-only enlargement relative to revision-04-final; never applied to actors or map.
        public const float VisualEnlargement = 1.0f;

        public static float VisibleWidth(string houseName,Vector2 lotSize) =>
            Revision04VisibleWidth(houseName,lotSize) * VisualEnlargement;

        public static float Revision04VisibleWidth(string houseName,Vector2 lotSize)
        {
            switch(houseName)
            {
                case "House_Temple": case "TownHallBuilding": return 24;
                case "House_Fishery": return 8;
                case "House_Bakery": return 10;
                case "House_AlchemyLab": return 9;
                case "House_Residential_4": return 8;
                case "House_AnimalYard": return 7;
                case "House_Inn": case "House_Blacksmith": return lotSize.x*1.05f;
                default: return Mathf.Min(10.5f,lotSize.x);
            }
        }

        public static void Fit(SpriteRenderer renderer, string houseName, Sprite sprite, Vector2 localDoor, Vector2 lotSize)
        {
            Rect occupied = TownKeyartSceneArt.OpaqueBounds(sprite);
            float width = VisibleWidth(houseName,lotSize);
            Vector2 pixelDoor;
            switch (sprite.name)
            {
                case "town_house_red_gable_keyart": pixelDoor = new Vector2(282,598); break;
                case "town_house_blue_gable_keyart": pixelDoor = new Vector2(282,598); break;
                case "town_house_green_gable_keyart": pixelDoor = new Vector2(282,598); break;
                case "town_watermill_native_scale1_v03_facade": pixelDoor = new Vector2(79, 193); break;
                case "town_bakery_native_scale1_v03_facade": pixelDoor = new Vector2(182, 390); break;
                case "town_alchemy_native_v06_facade": pixelDoor = new Vector2(165,381); break;
                case "town_courtyard_native_architecture_facade": pixelDoor = new Vector2(60,121); break;
                case "town_house_hipped_stone_native_v01_facade":
                case "town_house_l_dormer_native_v01_facade": pixelDoor = new Vector2(56,121); break;
                case "town_animal_shed_keyart":
                    // The small shelter is an annex inside an open yard; the functional door is the yard gate.
                    pixelDoor = new Vector2(55,118);
                    localDoor = new Vector2(1f,1f);
                    break;
                case "temple": pixelDoor = new Vector2(615, 1040); break;
                case "town_hall": pixelDoor = new Vector2(562, 1045); break;
                default:
                    // Assets without a measured door use front support until their author provides it.
                    pixelDoor = new Vector2(sprite.pivot.x + occupied.center.x * sprite.pixelsPerUnit,
                        sprite.rect.height - sprite.pivot.y - occupied.yMin * sprite.pixelsPerUnit);
                    break;
            }
            bool nativeScale1 = sprite.name == "town_bakery_native_scale1_v03_facade" ||
                                sprite.name == "town_watermill_native_scale1_v03_facade" ||
                                sprite.name == "town_alchemy_native_v06_facade";
            float scale = nativeScale1 ? 1f : width / occupied.width;
            Vector2 spriteDoor = new Vector2(pixelDoor.x - sprite.pivot.x,
                sprite.rect.height - pixelDoor.y - sprite.pivot.y) / sprite.pixelsPerUnit;
            renderer.sprite = sprite;
            renderer.color = Color.white;
            renderer.drawMode = SpriteDrawMode.Simple;
            renderer.transform.localScale = new Vector3(scale, scale, 1f);
            renderer.transform.localPosition = localDoor - spriteDoor * scale;
        }

        public static bool TryGetModularParts(string houseName, out Sprite roof, out Sprite doorLeaf)
        {
            roof = null; doorLeaf = null;
            string stem = houseName == "House_Bakery" ? "town_bakery_native_scale1_v03" :
                houseName == "House_Fishery" ? "town_watermill_native_scale1_v03" :
                houseName == "House_AlchemyLab" ? "town_alchemy_native_v06" :
                houseName == "House_Residential_4" ? "town_courtyard_native_architecture" :
                houseName == "House_CarvalhoTorto" || houseName == "House_Tovin" || houseName == "House_Manor" || houseName == "House_Archive" ? "town_house_hipped_stone_native_v01" :
                houseName == "House_Residential_1" || houseName == "House_Dagna" || houseName == "House_Pip" || houseName == "House_Workshop" ? "town_house_l_dormer_native_v01" : null;
            if (stem == null) return false;
            roof = WorldSpriteLibrary.Building(stem + "_roof");
            doorLeaf = WorldSpriteLibrary.Building(stem + "_door_leaf");
            return roof != null && doorLeaf != null;
        }
    }
}
