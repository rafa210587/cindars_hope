using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CindarsHope.City.Interiors;
using CindarsHope.City.Services;
using CindarsHope.NPC.Services;
using CindarsHope.Economy;
using CindarsHope.NPC;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>Authored S01 content only; never creates rooms, NPCs, doors, scenes or save state.</summary>
    public static class GenerateTownInteriorCatalog
    {
        public const string DataFolder = "Assets/_Game/Data/City/Interiors";
        public const string CatalogPath = DataFolder + "/TownInteriorCatalog.asset";

        // House|floor|custodian|residents|indoor shops|outdoor shops|entry|template|south|centre|north.
        // Every row is traced to the approved S01 H01-H24 programme; no runtime balance numbers live here.
        private static readonly string[] Rows =
        {
            "House_Temple|18,16|npc_corvus|npc_corvus|npc_corvus||Portal|temple|Vestibule and axial nave, four benches and Kanthor altar|East consultation and offerings counter with materials cabinet|Sacristy archive and Corvus bedroom with pantry",
            "House_Chamber|14,12|npc_velorin||||Portal|civic|Reception and waiting with two seats and notice board|Council table with four chairs; Velorin office desk|Administrative archive with two shelves",
            "House_Prison|14,12|npc_alaric||||Portal|civic|Visitor register and bench|Equipment store; central corridor; two barred cells with fixed benches|Service storage; cells are not resident bedrooms",
            "House_Manor|16,14|npc_velorin|npc_velorin|||Portal|residence|Visiting hall, sofa, two chairs and table|Study and library; separate domestic dining and kitchen|Velorin bedroom, washstand and pantry",
            "House_Registry|16,12|npc_mara|npc_mara|npc_mara||Portal|shop_home|Records counter, two waiting seats and form shelves|Authentication table, private archive and independent passage|Mara bedroom, domestic kitchen, meal table and washstand",
            "House_Archive|16,14|npc_thalindra|npc_thalindra|npc_thalindra||Portal|shop_home|Consultation counter and two seats|Two lateral book rows, research table and analysis bench|Thalindra bedroom, pantry and washstand apart from collection",
            "House_MarketHall|16,12|||||Portal|civic|Wide covered entry and exit|Two display tables and accessible Station_Workbench|Scenic stock and notice board; no exclusive seller",
            "House_Bakery|16,12|npc_mella|npc_mella|npc_mella||Portal|shop_home|Bread counter and two seats|Preparation table, oven, flour and shelves; oven is scenery|Mella bedroom, domestic pantry, meal table and washstand",
            "House_Inn|24,18|npc_orlan|npc_orlan,npc_gruta|npc_orlan,npc_gruta||Portal|inn|Orlan reception, Gruta bar, three tables and six seats|Cooking station, thermal bath, pantry and three-unit corridor|Separate Orlan and Gruta rooms and two six-unit guest rooms",
            "House_Fishery|14,12|npc_sael|npc_sael||npc_sael|Portal|work_home|Nets, rod rack and cleaning table|Basket store and scenic mill mechanism outside circulation|Sael bedroom, domestic pantry and washstand; wheel and dock exterior",
            "House_Blacksmith|18,14|npc_brumdar|npc_brumdar|npc_brumdar||Portal|shop_home|Tool service counter and display|Station_Forge, anvil, ore storage and marked hot zone|Brumdar bedroom, domestic kitchen and tool reserve away from fire",
            "House_AlchemyLab|16,14|npc_ozzra|npc_ozzra|npc_ozzra||Portal|shop_home|Service counter and display|Station_Alchemy, bench, reagents cabinet and decorative vials|Ozzra bedroom, pantry and washstand away from reagents",
            "House_Workshop|16,12|npc_nimble|npc_nimble|npc_nimble||Portal|shop_home|Order counter and desk|Station_Carpentry, bench, scenic saws and wood store|Nimble bedroom, pantry, meal table and washstand",
            "House_Tannery|18,14|npc_hess|npc_hess|npc_hess||Portal|shop_home|Leather service counter and barrel display|Racks, work table, vats and protected tool passage|Hess bedroom, pantry and washstand behind a divider",
            "House_Residential_4|14,12|npc_savra|npc_savra||npc_savra|Portal|residence|Visiting room and meal table|Domestic preparation and dried herb niche, not another alchemy station|Savra bedroom, Dragonborn bed, personal storage and washstand",
            "House_CarvalhoTorto|18,14|npc_hund|npc_hund,npc_gurd||npc_hund,npc_gurd|Portal|shared_home|Four-place family meal table, not four residents|Kitchen, washstand and separate guard/work tool storage|Two distinct Orc bedrooms with personal beds and chests",
            "House_Residential_1|14,12|npc_sylveth|npc_sylveth||npc_sylveth|Portal|residence|Reception and meal table|Separate seed preparation and domestic pantry, labelled seed boxes|Sylveth bedroom, clothing store and washstand",
            "House_Dagna|12,10|npc_dagna|npc_dagna||npc_dagna|Portal|residence|Meal room|Domestic pantry, washstand and mining gear niche|Dagna bedroom, Dwarf bed, wardrobe and scenic stone samples",
            "House_Residential_2|16,12|npc_renko|npc_renko|npc_renko||Portal|shop_home|Shop counter, three lateral displays and clear central axis|Scenic stock and separate domestic pantry|Renko bedroom, Smallfolk bed, wardrobe and washstand",
            "House_Pip|12,10|npc_pip|npc_pip||npc_pip|Portal|residence|Letter sorting table, bags and visiting bench|Pantry, washstand and personal chest|Pip bedroom and Smallfolk bed; no postal system",
            "House_Residential_3|16,12|npc_mirela|npc_mirela|npc_mirela||Portal|shop_home|Service counter and scenic mannequin|Station_Sewing, cutting table and fabric rolls|Mirela bedroom, pantry and washstand",
            "House_Tovin|14,12|npc_tovin|npc_tovin|npc_tovin||Portal|shop_home|License desk, two visitor seats and board|Personal archive, pantry and washstand|Tovin bedroom, Smallfolk bed and wardrobe",
            "House_GateKeeper|12,10|npc_alaric|npc_alaric|||GuardPostAnnex|guard_annex|Private annex meal room behind the existing cutaway post|Pantry, washstand, map and equipment|Alaric bedroom, wardrobe and one replacement bed",
            "House_AnimalYard|14,12|npc_eiran|npc_eiran||npc_eiran|AnimalYardAnnex|yard_annex|Annex support and meal room; open corral remains outside|Pantry, washstand and clean equipment|Eiran bedroom and wardrobe; no active animals inside"
        };

        // Frozen from TownNpcHomes, not inferred from a nearest building or NPC profession.
        public static readonly IReadOnlyDictionary<string,string> CanonicalHomes = new Dictionary<string,string>(StringComparer.Ordinal)
        {
            ["npc_corvus"]="House_Temple",["npc_velorin"]="House_Manor",["npc_orlan"]="House_Inn",["npc_gruta"]="House_Inn",
            ["npc_gurd"]="House_CarvalhoTorto",["npc_hund"]="House_CarvalhoTorto",["npc_mara"]="House_Registry",
            ["npc_tovin"]="House_Tovin",["npc_brumdar"]="House_Blacksmith",["npc_dagna"]="House_Dagna",
            ["npc_thalindra"]="House_Archive",["npc_nimble"]="House_Workshop",["npc_ozzra"]="House_AlchemyLab",
            ["npc_eiran"]="House_AnimalYard",["npc_alaric"]="House_GateKeeper",["npc_pip"]="House_Pip",
            ["npc_sylveth"]="House_Residential_1",["npc_renko"]="House_Residential_2",["npc_mirela"]="House_Residential_3",
            ["npc_savra"]="House_Residential_4",["npc_sael"]="House_Fishery",["npc_mella"]="House_Bakery",
            ["npc_hess"]="House_Tannery"
        };
        public static readonly string[] CanonicalOutdoorHomeNpcIds = {"npc_maelor","npc_yael","npc_liora","npc_zrix","npc_tibbet"};
        public static readonly string[] UnhousedOutdoorServiceIds =
            {NpcServiceCatalog.BookOrderServiceId,NpcServiceCatalog.HuntContractServiceId};
        public static readonly string[] UnhousedOutdoorProviderNpcIds = {"npc_yael","npc_zrix"};
        public static readonly IReadOnlyDictionary<string,string> CanonicalProviderVenues = new Dictionary<string,string>(StringComparer.Ordinal)
        {
            ["npc_corvus"]="House_Temple|I",["npc_mara"]="House_Registry|I",["npc_thalindra"]="House_Archive|I",
            ["npc_mella"]="House_Bakery|I",["npc_orlan"]="House_Inn|I",["npc_gruta"]="House_Inn|I",
            ["npc_brumdar"]="House_Blacksmith|I",["npc_ozzra"]="House_AlchemyLab|I",["npc_nimble"]="House_Workshop|I",
            ["npc_hess"]="House_Tannery|I",["npc_renko"]="House_Residential_2|I",["npc_mirela"]="House_Residential_3|I",
            ["npc_tovin"]="House_Tovin|I",["npc_sael"]="House_Fishery|O",["npc_savra"]="House_Residential_4|O",
            ["npc_hund"]="House_CarvalhoTorto|O",["npc_gurd"]="House_CarvalhoTorto|O",
            ["npc_sylveth"]="House_Residential_1|O",["npc_dagna"]="House_Dagna|O",["npc_pip"]="House_Pip|O",
            ["npc_eiran"]="House_AnimalYard|O",["npc_yael"]="outdoor_night_market|O",["npc_zrix"]="outdoor_cave_mouth|O"
        };

        // Room local bounds are floor-space envelopes (x,y,width,height), not exterior lot or sprite dimensions.
        // Distinct bedrooms and service zones are required; S02/S04 later establish final wall/furniture collision.
        private static readonly IReadOnlyDictionary<string,string> RoomPlans = new Dictionary<string,string>(StringComparer.Ordinal)
        {
            ["House_Temple"]="vestibule:-9,-8,18,3;nave:-9,-5,12,7;consultation:3,-5,6,7;sacristy:-9,2,6,6;pantry:-3,2,4,6;bedroom:1,2,8,6",
            ["House_Chamber"]="reception:-7,-6,14,3;council:-7,-3,10,6;office:3,-3,4,6;archive:-7,3,14,3",
            ["House_Prison"]="reception:-7,-6,14,3;equipment:-7,-3,4,6;corridor:-3,-3,3,6;cell_01:0,-3,3.5,6;cell_02:3.5,-3,3.5,6;service:-7,3,14,3",
            ["House_Manor"]="visiting:-8,-7,16,4;study:-8,-3,8,5;domestic:0,-3,8,5;bedroom:-8,2,10,5;washroom:2,2,6,5",
            ["House_Registry"]="reception:-8,-6,16,5;domestic:-8,-1,5,7;bedroom:-3,-1,5,7;archive:2,-1,6,7",
            ["House_Archive"]="reception:-8,-7,16,4;research:-8,-3,16,5;bedroom:-8,2,8,5;domestic:0,2,8,5",
            ["House_MarketHall"]="entrance:-8,-6,16,3;display:-8,-3,10,6;workbench:2,-3,6,6;stock:-8,3,16,3",
            ["House_Bakery"]="shop:-8,-6,16,4;bakery:-8,-2,16,4;bedroom:-8,2,8,4;domestic:0,2,8,4",
            ["House_Inn"]="salon:-12,-9,24,6;kitchen:-12,-3,8,6;bath:-4,-3,6,6;corridor:2,-3,3,6;pantry:5,-3,7,6;guest_01:-12,3,6,6;guest_02:-6,3,6,6;orlan_bedroom:0,3,6,6;gruta_bedroom:6,3,6,6",
            ["House_Fishery"]="workfront:-7,-6,14,4;millwork:-7,-2,14,4;bedroom:-7,2,7,4;domestic:0,2,7,4",
            ["House_Blacksmith"]="reception:-9,-7,18,4;forge:-9,-3,18,5;bedroom:-9,2,6,5;domestic:-3,2,6,5;reserve:3,2,6,5",
            ["House_AlchemyLab"]="reception:-8,-7,16,4;lab:-8,-3,16,5;bedroom:-8,2,8,5;domestic:0,2,8,5",
            ["House_Workshop"]="reception:-8,-6,16,4;workshop:-8,-2,16,4;bedroom:-8,2,8,4;domestic:0,2,8,4",
            ["House_Tannery"]="reception:-9,-7,18,4;tannery:-9,-3,18,5;bedroom:-9,2,9,5;domestic:0,2,9,5",
            ["House_Residential_4"]="visiting:-7,-6,14,4;domestic:-7,-2,14,4;bedroom:-7,2,14,4",
            ["House_CarvalhoTorto"]="visiting:-9,-7,18,4;domestic:-9,-3,10,5;equipment:1,-3,8,5;hund_bedroom:-9,2,9,5;gurd_bedroom:0,2,9,5",
            ["House_Residential_1"]="visiting:-7,-6,14,4;seedwork:-7,-2,7,4;domestic:0,-2,7,4;bedroom:-7,2,14,4",
            ["House_Dagna"]="visiting:-6,-5,12,3;domestic:-6,-2,12,3;bedroom:-6,1,12,4",
            ["House_Residential_2"]="shop:-8,-6,16,5;stock:-8,-1,8,3;domestic:0,-1,8,3;bedroom:-8,2,16,4",
            ["House_Pip"]="sorting:-6,-5,12,3;domestic:-6,-2,12,3;bedroom:-6,1,12,4",
            ["House_Residential_3"]="shop:-8,-6,16,4;atelier:-8,-2,16,4;bedroom:-8,2,8,4;domestic:0,2,8,4",
            ["House_Tovin"]="office:-7,-6,14,4;domestic:-7,-2,14,4;bedroom:-7,2,14,4",
            ["House_GateKeeper"]="visiting:-6,-5,12,3;domestic:-6,-2,12,3;bedroom:-6,1,12,4",
            ["House_AnimalYard"]="support:-7,-6,14,4;domestic:-7,-2,14,4;bedroom:-7,2,14,4"
        };

        // Individual physical props; role D is decorative floor/wall detail, otherwise solid scenery.
        private static readonly IReadOnlyDictionary<string,string> PropPlans = new Dictionary<string,string>(StringComparer.Ordinal)
        {
            ["House_Temple"]="nave:bench_01,bench_02,bench_03,bench_04,kanthor_altar,decor_niche_w,decor_niche_e;consultation:materials_cabinet,consultation_table;sacristy:archive_shelf,offerings_store",
            ["House_Chamber"]="reception:waiting_seat_01,waiting_seat_02,decor_notice_board;council:council_table,council_chair_01,council_chair_02,council_chair_03,council_chair_04;office:velorin_desk,decor_seal;archive:archive_shelf_01,archive_shelf_02",
            ["House_Prison"]="reception:register_table,visitor_bench;equipment:uniform_chest,decor_weapon_rack;cell_01:cell_bench_01,decor_cell_wash_01;cell_02:cell_bench_02,decor_cell_wash_02;service:service_store,barred_gate_01,barred_gate_02",
            ["House_Manor"]="visiting:sofa,chair_01,chair_02,visiting_table;study:study_desk,library_shelf,decor_map;domestic:dining_table,chair_dining_01,chair_dining_02,stove,pantry;washroom:washstand,linen_store",
            ["House_Registry"]="reception:waiting_seat_01,waiting_seat_02,form_shelf,authentication_table;archive:closed_archive_shelf_01,closed_archive_shelf_02",
            ["House_Archive"]="reception:consultation_seat_01,consultation_seat_02;research:book_shelf_w,book_shelf_e,research_table,analysis_bench,decor_scrolls,decor_specimens",
            ["House_MarketHall"]="display:display_table_01,display_table_02,decor_goods_01,decor_goods_02;stock:stock_shelf,decor_notice_board",
            ["House_Bakery"]="shop:customer_seat_01,customer_seat_02,bread_display;bakery:preparation_table,oven,flour_store,bread_shelf",
            ["House_Inn"]="salon:table_01,table_02,table_03,seat_01,seat_02,seat_03,seat_04,seat_05,seat_06;pantry:food_store;bath:bath_tub,privacy_screen;guest_01:guest_storage_01,guest_lamp_01;guest_02:guest_storage_02,guest_lamp_02;orlan_bedroom:orlan_lamp;gruta_bedroom:gruta_lamp",
            ["House_Fishery"]="workfront:net_rack,rod_rack,cleaning_table;millwork:basket_store,decor_mill_mechanism",
            ["House_Blacksmith"]="reception:tool_display,decor_tools;forge:anvil,ore_store,decor_heat_zone;reserve:tool_reserve",
            ["House_AlchemyLab"]="reception:display_shelf;lab:workbench,reagent_cabinet,decor_vials_01,decor_vials_02",
            ["House_Workshop"]="reception:order_desk;workshop:bench,wood_store,decor_saws",
            ["House_Tannery"]="reception:leather_display,decor_barrels;tannery:hide_rack,work_table,vat_01,vat_02",
            ["House_Residential_4"]="visiting:visiting_table;domestic:herb_niche,decor_dried_herbs",
            ["House_CarvalhoTorto"]="visiting:family_table,seat_01,seat_02,seat_03,seat_04;equipment:guard_rack,work_tool_store;hund_bedroom:hund_lamp;gurd_bedroom:gurd_lamp",
            ["House_Residential_1"]="visiting:visiting_table;seedwork:seed_table,seed_store,decor_seed_labels",
            ["House_Dagna"]="visiting:meal_table;domestic:mining_gear_niche,decor_stone_samples",
            ["House_Residential_2"]="shop:display_01,display_02,display_03;stock:stock_shelf",
            ["House_Pip"]="sorting:letter_sorting_table,visitor_bench,decor_route_bags;domestic:personal_chest",
            ["House_Residential_3"]="shop:decor_mannequin;atelier:cutting_table,fabric_roll_01,fabric_roll_02",
            ["House_Tovin"]="office:visitor_seat_01,visitor_seat_02,decor_license_board;domestic:personal_archive",
            ["House_GateKeeper"]="visiting:meal_table;domestic:decor_gate_map,equipment_rack",
            ["House_AnimalYard"]="support:meal_table;domestic:clean_equipment_store,feed_store"
        };

        [MenuItem("CindarsHope/Generate/Data/Town Interior Catalog")]
        public static void Generate()
        {
            var draft=BuildDefinitions();
            try
            {
                ValidateManifest(draft);
                ValidateProviderAssets(draft);
                // Complete preflight before any CreateAsset/CopySerialized/SaveAssets call.
                foreach(var definition in draft)
                {
                    string path=PathFor(definition.HouseId);
                    var main=AssetDatabase.LoadMainAssetAtPath(path);
                    if(main!=null && !(main is TownInteriorDefinitionSO))throw new InvalidOperationException("Wrong asset type at "+path);
                    if(main is TownInteriorDefinitionSO existing && existing.HouseId!=definition.HouseId)
                        throw new InvalidOperationException("Stable HouseId conflict at "+path+": "+existing.HouseId);
                }
                var catalogMain=AssetDatabase.LoadMainAssetAtPath(CatalogPath);
                if(catalogMain!=null && !(catalogMain is TownInteriorCatalogSO))throw new InvalidOperationException("Wrong catalog asset type at "+CatalogPath);
                EnsureFolder();
                var materialized=new TownInteriorDefinitionSO[draft.Length];int created=0,updated=0;
                for(int i=0;i<draft.Length;i++)
                {
                    var path=PathFor(draft[i].HouseId);
                    var existing=AssetDatabase.LoadAssetAtPath<TownInteriorDefinitionSO>(path);
                    if(existing==null)
                    {
                        existing=ScriptableObject.CreateInstance<TownInteriorDefinitionSO>();
                        EditorUtility.CopySerialized(draft[i],existing);
                        AssetDatabase.CreateAsset(existing,path);created++;
                    }
                    else if(JsonUtility.ToJson(existing)!=JsonUtility.ToJson(draft[i]))
                    {EditorUtility.CopySerialized(draft[i],existing);EditorUtility.SetDirty(existing);updated++;}
                    materialized[i]=existing;
                }
                var catalog=AssetDatabase.LoadAssetAtPath<TownInteriorCatalogSO>(CatalogPath);
                if(catalog==null)
                {
                    catalog=ScriptableObject.CreateInstance<TownInteriorCatalogSO>();catalog.Definitions=materialized;
                    catalog.UnhousedOutdoorServiceIds=UnhousedOutdoorServiceIds.ToArray();
                    catalog.UnhousedOutdoorProviderNpcIds=UnhousedOutdoorProviderNpcIds.ToArray();
                    AssetDatabase.CreateAsset(catalog,CatalogPath);created++;
                }
                else if(catalog.Definitions==null||!catalog.Definitions.SequenceEqual(materialized)||
                    catalog.UnhousedOutdoorServiceIds==null||!catalog.UnhousedOutdoorServiceIds.SequenceEqual(UnhousedOutdoorServiceIds)||
                    catalog.UnhousedOutdoorProviderNpcIds==null||!catalog.UnhousedOutdoorProviderNpcIds.SequenceEqual(UnhousedOutdoorProviderNpcIds))
                {
                    catalog.Definitions=materialized;catalog.UnhousedOutdoorServiceIds=UnhousedOutdoorServiceIds.ToArray();
                    catalog.UnhousedOutdoorProviderNpcIds=UnhousedOutdoorProviderNpcIds.ToArray();
                    EditorUtility.SetDirty(catalog);updated++;
                }
                if(created+updated>0)AssetDatabase.SaveAssets();
                Debug.Log($"TOWN_INTERIOR_GENERATOR: PASS houses=24 created={created} updated={updated} unchanged={25-created-updated}");
            }
            finally{foreach(var definition in draft)UnityEngine.Object.DestroyImmediate(definition);}
        }

        public static TownInteriorDefinitionSO[] BuildDefinitions()
        {
            var result=new List<TownInteriorDefinitionSO>(24);
            foreach(string row in Rows)result.Add(Parse(row));
            return result.ToArray();
        }

        private static TownInteriorDefinitionSO Parse(string row)
        {
            string[] parts=row.Split('|');
            if(parts.Length!=11)throw new InvalidOperationException("S01 programme row has "+parts.Length+" fields: "+row);
            string[] size=parts[1].Split(',');
            var definition=ScriptableObject.CreateInstance<TownInteriorDefinitionSO>();
            definition.HouseId=parts[0];definition.TemplateId=TemplateFor(parts[0]);definition.ResponsibleNpcId=parts[2];
            definition.ResidentNpcIds=Ids(parts[3]);definition.PrimaryIndoorProviderNpcIds=Ids(parts[4]);
            definition.PrimaryOutdoorProviderNpcIds=Ids(parts[5]);
            definition.ExistingServiceIds=NpcServiceCatalog.All.Where(service=>
                definition.PrimaryIndoorProviderNpcIds.Contains(service.NpcId)||definition.PrimaryOutdoorProviderNpcIds.Contains(service.NpcId))
                .Select(service=>service.ServiceId).ToArray();
            if(definition.HouseId=="House_Registry")definition.ExistingServiceIds=definition.ExistingServiceIds.Concat(
                new[]{CityServiceCatalog.FarmRegistryService.ServiceId}).ToArray();
            if(definition.HouseId=="House_Tovin")definition.ExistingServiceIds=definition.ExistingServiceIds.Concat(
                new[]{CityServiceCatalog.MarketStallService.ServiceId}).ToArray();
            definition.EntryMode=(TownInteriorEntryMode)Enum.Parse(typeof(TownInteriorEntryMode),parts[6]);
            definition.OwnershipKind=definition.ResidentNpcIds.Length==0?TownInteriorOwnershipKind.Institutional:TownInteriorOwnershipKind.Household;
            definition.UsableFloorSize=new Vector2(float.Parse(size[0],System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(size[1],System.Globalization.CultureInfo.InvariantCulture));
            definition.InteriorPrefabKey="Assets/_Game/Prefabs/City/Interiors/"+definition.HouseId+".prefab";
            definition.EntryAnchorId="town_entry_"+definition.HouseId;definition.ReturnAnchorId="town_return_"+definition.HouseId;
            definition.Access=AccessFor(definition);
            var rooms=new List<TownInteriorRoomDefinition>();var props=new List<TownInteriorPropDefinition>();
            string[] roomSpecs=RoomPlans[definition.HouseId].Split(';');
            foreach(string spec in roomSpecs)
            {
                string[] pair=spec.Split(':');string[] values=pair[1].Split(',');
                var bounds=new Rect(ParseFloat(values[0]),ParseFloat(values[1]),ParseFloat(values[2]),ParseFloat(values[3]));
                string purpose=pair[0].Replace('_',' ')+" — "+parts[bounds.center.y< -definition.UsableFloorSize.y/6f?8:
                    bounds.center.y>definition.UsableFloorSize.y/6f?10:9];
                string[] authorized=pair[0].Contains("bedroom")?definition.ResidentNpcIds.Where(npc=>
                    pair[0]=="bedroom"||pair[0].StartsWith(npc.Substring(4)+"_",StringComparison.Ordinal)).ToArray():Array.Empty<string>();
                AddRoom(rooms,pair[0],purpose,bounds,authorized);
            }
            definition.Rooms=rooms.ToArray();
            foreach(string group in PropPlans[definition.HouseId].Split(';'))
            {
                string[] pair=group.Split(':');
                foreach(string prop in pair[1].Split(','))
                    AddProp(definition,props,pair[0],prop,prop.StartsWith("decor_",StringComparison.Ordinal)?TownInteriorPropRole.FloorDecoration:
                        TownInteriorPropRole.SolidScenery,prop.Replace('_',' ')+" for "+pair[0].Replace('_',' '));
            }
            if(definition.ResidentNpcIds.Length>0)
            {
                string domestic=definition.Rooms.Any(r=>r.RoomId=="domestic")?"domestic":
                    definition.Rooms.Any(r=>r.RoomId=="pantry")?"pantry":null;
                if(domestic==null)throw new InvalidOperationException(definition.HouseId+": resident needs a separate domestic or pantry zone.");
                AddProp(definition,props,domestic,"domestic_pantry",TownInteriorPropRole.SolidScenery,"Separate domestic cooking and food storage");
                AddProp(definition,props,domestic,"meal_table",TownInteriorPropRole.SolidScenery,"Resident meal surface and seat");
                AddProp(definition,props,domestic,"washstand",TownInteriorPropRole.SolidScenery,"Private washing and screen");
                foreach(string resident in definition.ResidentNpcIds)
                {
                    string bedroom=definition.Rooms.First(r=>r.RoomId=="bedroom"||r.RoomId==resident.Substring(4)+"_bedroom").RoomId;
                    AddProp(definition,props,bedroom,"bed_"+resident,TownInteriorPropRole.SolidScenery,"Personal bed for "+resident);
                    AddProp(definition,props,bedroom,"storage_"+resident,TownInteriorPropRole.SolidScenery,"Personal storage for "+resident);
                }
            }
            foreach(string npc in definition.PrimaryIndoorProviderNpcIds)
                AddProp(definition,props,ProviderRoomFor(definition.HouseId),"counter_"+npc,TownInteriorPropRole.ExistingInteraction,
                    "Existing shop of "+npc,"shop_"+npc.Substring(4));
            AddExistingInteractions(definition,props);
            definition.Props=props.ToArray();
            return definition;
        }

        private static TownInteriorAccessDefinition AccessFor(TownInteriorDefinitionSO definition)
        {
            var access=new TownInteriorAccessDefinition
            {HasResidentialVisit=definition.ResidentNpcIds.Length>0,ResidentsBypassEntryHours=definition.ResidentNpcIds.Length>0,
             CommercialEntryFollowsProviderSchedule=definition.PrimaryIndoorProviderNpcIds.Length>0};
            switch(definition.HouseId)
            {
                case "House_Temple":access.CivicStartHour=6;access.CivicEndHour=22;access.CommercialEntryFollowsProviderSchedule=false;break;
                case "House_Chamber":case "House_Prison":access.CivicStartHour=9;access.CivicEndHour=18;break;
                case "House_MarketHall":access.CivicStartHour=6;access.CivicEndHour=22;break;
                case "House_Inn":access.PublicLobbyAlwaysOpen=true;access.CommercialEntryFollowsProviderSchedule=false;break;
            }
            return access;
        }

        private static void AddExistingInteractions(TownInteriorDefinitionSO definition,List<TownInteriorPropDefinition> props)
        {
            string house=definition.HouseId;
            string station=house=="House_Inn"?"Station_CookingStation":house=="House_Blacksmith"?"Station_Forge":
                house=="House_AlchemyLab"?"Station_Alchemy":house=="House_Workshop"?"Station_Carpentry":
                house=="House_Residential_3"?"Station_Sewing":house=="House_MarketHall"?"Station_Workbench":null;
            if(station!=null)AddProp(definition,props,StationRoomFor(house),station,TownInteriorPropRole.ExistingInteraction,
                "Existing crafting station", "station_"+house.ToLowerInvariant());
            if(house=="House_Inn")
            {
                AddProp(definition,props,"guest_01","GuestBed_Inn",TownInteriorPropRole.ExistingInteraction,"Existing paid guest bed","GuestBed_Inn");
                AddProp(definition,props,"guest_02","GuestBed_Inn_02",TownInteriorPropRole.ExistingInteraction,"Second paid guest bed using existing InnBedPaymentGate","GuestBed_Inn_02");
            }
            if(house=="House_Registry")AddProp(definition,props,"reception","farm_registry",TownInteriorPropRole.ExistingInteraction,
                "Existing Mara farm registry service; contract_farm_registry remains its effect",CityServiceCatalog.FarmRegistryService.ServiceId);
            if(house=="House_Tovin")AddProp(definition,props,"office","market_license",TownInteriorPropRole.ExistingInteraction,
                "Existing Tovin market stall service; license_market_stall remains its effect",CityServiceCatalog.MarketStallService.ServiceId);
            foreach(string service in definition.ExistingServiceIds)
                if(definition.PrimaryIndoorProviderNpcIds.Any(npc=>NpcServiceCatalog.GetById(service)?.NpcId==npc))
                    AddProp(definition,props,service==NpcServiceCatalog.ThermalBathServiceId?"bath":StationRoomFor(house),service,
                        TownInteriorPropRole.ExistingInteraction,"Existing NPC service; no new effect",service);
        }

        private static string TemplateFor(string house)
        {
            switch(house)
            {
                case "House_Temple":return "temple_residence";
                case "House_Chamber":return "civic_office";
                case "House_Prison":return "detention";
                case "House_Manor":return "manor";
                case "House_Registry":case "House_Archive":case "House_Tovin":return "archive_residence";
                case "House_MarketHall":return "covered_market";
                case "House_Bakery":case "House_Residential_2":case "House_Residential_3":return "shop_residence";
                case "House_Inn":return "inn_tavern";
                case "House_Blacksmith":case "House_AlchemyLab":case "House_Workshop":case "House_Tannery":case "House_Fishery":return "industrial_residence";
                case "House_CarvalhoTorto":return "home_shared";
                case "House_GateKeeper":case "House_AnimalYard":return "annex_home";
                default:return "home_single";
            }
        }

        private static string ProviderRoomFor(string house)
        {
            switch(house)
            {
                case "House_Temple":return "consultation";
                case "House_Inn":return "salon";
                case "House_Tovin":return "office";
                default:return house=="House_Bakery"||house=="House_Residential_2"||house=="House_Residential_3"?"shop":"reception";
            }
        }

        private static string StationRoomFor(string house)
        {
            switch(house)
            {
                case "House_Inn":return "kitchen";
                case "House_Blacksmith":return "forge";
                case "House_AlchemyLab":return "lab";
                case "House_Workshop":return "workshop";
                case "House_Residential_3":return "atelier";
                case "House_MarketHall":return "workbench";
                case "House_Archive":return "research";
                default:return "reception";
            }
        }

        private static float ParseFloat(string value)=>float.Parse(value,System.Globalization.CultureInfo.InvariantCulture);

        private static void AddRoom(List<TownInteriorRoomDefinition> rooms,string id,string purpose,Rect bounds,string[] residents=null) =>
            rooms.Add(new TownInteriorRoomDefinition{RoomId=id,Purpose=purpose,LocalFloorBounds=bounds,
                AuthorizedResidentNpcIds=residents??Array.Empty<string>()});

        private static void AddProp(TownInteriorDefinitionSO definition,List<TownInteriorPropDefinition> props,string room,string id,
            TownInteriorPropRole role,string purpose,string interactionId=null)
        {
            var bounds=definition.Rooms.Single(r=>r.RoomId==room).LocalFloorBounds;
            int slot=props.Count(prop=>prop.RoomId==room);
            float x=bounds.xMin+1f+(slot%4)*(bounds.width-2f)/3f;
            float y=Mathf.Min(bounds.yMax-.5f,bounds.yMin+1f+(slot/4)*1.1f);
            props.Add(new TownInteriorPropDefinition{PropId=definition.HouseId+"/"+room+"/"+id,RoomId=room,
                AssetKey="candidate:town_interior/"+definition.TemplateId+"/"+id,Purpose=purpose,
                ExistingInteractionId=interactionId??string.Empty,Role=role,LocalPosition=new Vector2(x,y)});
        }

        private static string[] Ids(string value)=>string.IsNullOrEmpty(value)?Array.Empty<string>():value.Split(',');
        public static string PathFor(string houseId)=>DataFolder+"/"+houseId+"_Interior.asset";
        public static void ValidateProviderAssets(IReadOnlyList<TownInteriorDefinitionSO> definitions)
        {
            var specs=ReadSourceNpcSpecs();
            var sourceProviders=new HashSet<string>(specs.Where(pair=>!string.IsNullOrWhiteSpace(pair.Value.ShopDataPath))
                .Select(pair=>pair.Key),StringComparer.Ordinal);
            if(!sourceProviders.SetEquals(CanonicalProviderVenues.Keys))
                throw new InvalidOperationException("RefinedCanonicalTownNpcSpecs shop provider set changed.");
            foreach(string npc in CanonicalProviderVenues.Keys)
            {
                var spec=specs[npc];
                var shop=AssetDatabase.LoadAssetAtPath<ShopDataSO>(spec.ShopDataPath);
                var npcData=AssetDatabase.LoadAssetAtPath<NpcDataSO>(spec.NpcDataPath);
                if(shop==null||npcData==null||shop.Id!="shop_"+npc.Substring(4)||shop.NpcId!=npc||
                    npcData.NpcId!=npc||npcData.ShopId!=shop.Id)
                    throw new InvalidOperationException("Provider "+npc+" disagrees with source ShopDataPath/NpcDataSO.ShopId at "+spec.ShopDataPath);
            }
        }

        public static void ValidateSourceContracts(IReadOnlyList<TownInteriorDefinitionSO> definitions)
        {
            // S01 intentionally snapshots private scene authoring. Reflect that source, so a future
            // TownNpcHomes/RefinedCanonicalTownNpcSpecs edit fails before an asset can be rewritten.
            const BindingFlags flags=BindingFlags.Static|BindingFlags.NonPublic;
            var field=typeof(CreateMvpTownScene).GetField("TownNpcHomes",flags);
            if(!(field?.GetValue(null) is IDictionary source))
                throw new InvalidOperationException("TownNpcHomes source contract unavailable.");
            var sourceHomes=new Dictionary<string,string>(StringComparer.Ordinal);
            var outdoor=new HashSet<string>(StringComparer.Ordinal);
            foreach(DictionaryEntry item in source)
            {
                string npc=(string)item.Key;
                Type homeType=item.Value.GetType();
                var indoorField=homeType.GetField("IsIndoor",BindingFlags.Public|BindingFlags.Instance);
                var houseField=homeType.GetField("HouseName",BindingFlags.Public|BindingFlags.Instance);
                if(indoorField==null||houseField==null)
                    throw new InvalidOperationException("TownNpcHomes HomeAssignment contract changed.");
                if((bool)indoorField.GetValue(item.Value))sourceHomes.Add(npc,(string)houseField.GetValue(item.Value));
                else outdoor.Add(npc);
            }
            if(sourceHomes.Count!=CanonicalHomes.Count||outdoor.Count!=CanonicalOutdoorHomeNpcIds.Length||
                !outdoor.SetEquals(CanonicalOutdoorHomeNpcIds))
                throw new InvalidOperationException("TownNpcHomes indoor/outdoor census drifted.");
            foreach(var pair in CanonicalHomes)
                if(!sourceHomes.TryGetValue(pair.Key,out string canonicalHouse)||canonicalHouse!=pair.Value)
                    throw new InvalidOperationException("TownNpcHomes home drift for "+pair.Key);
            foreach(var definition in definitions)
            {
                if(definition==null)throw new InvalidOperationException("S01 contains a null definition.");
                foreach(string npc in definition.ResidentNpcIds.Concat(definition.PrimaryIndoorProviderNpcIds)
                    .Concat(definition.PrimaryOutdoorProviderNpcIds))
                    if(!sourceHomes.TryGetValue(npc,out string providerHouse)||providerHouse!=definition.HouseId)
                        throw new InvalidOperationException(definition.HouseId+": source TownNpcHomes disagrees for "+npc);
            }
            var specs=ReadSourceNpcSpecs();
            foreach(string npc in CanonicalProviderVenues.Keys)
                if(!specs.TryGetValue(npc,out var spec)||string.IsNullOrWhiteSpace(spec.ShopDataPath))
                    throw new InvalidOperationException("RefinedCanonicalTownNpcSpecs missing shop for "+npc);
            foreach(string npc in UnhousedOutdoorProviderNpcIds)
                if(!outdoor.Contains(npc))
                    throw new InvalidOperationException("TownNpcHomes no longer keeps "+npc+" outdoors.");
        }

        private static Dictionary<string,(string NpcDataPath,string ShopDataPath)> ReadSourceNpcSpecs()
        {
            var field=typeof(CreateMvpTownScene).GetField("RefinedCanonicalTownNpcSpecs",BindingFlags.Static|BindingFlags.NonPublic);
            if(!(field?.GetValue(null) is Array source))
                throw new InvalidOperationException("RefinedCanonicalTownNpcSpecs source contract unavailable.");
            var specs=new Dictionary<string,(string NpcDataPath,string ShopDataPath)>(StringComparer.Ordinal);
            foreach(object spec in source)
            {
                Type type=spec.GetType();
                var npc=type.GetProperty("NpcId",BindingFlags.Public|BindingFlags.Instance);
                var data=type.GetProperty("NpcDataPath",BindingFlags.Public|BindingFlags.Instance);
                var shop=type.GetProperty("ShopDataPath",BindingFlags.Public|BindingFlags.Instance);
                if(npc==null||data==null||shop==null)
                    throw new InvalidOperationException("RefinedCanonicalTownNpcSpecs member contract changed.");
                specs.Add((string)npc.GetValue(spec),((string)data.GetValue(spec),(string)shop.GetValue(spec)));
            }
            return specs;
        }
        private static void EnsureFolder()
        {
            if(!AssetDatabase.IsValidFolder("Assets/_Game/Data/City"))AssetDatabase.CreateFolder("Assets/_Game/Data","City");
            if(!AssetDatabase.IsValidFolder(DataFolder))AssetDatabase.CreateFolder("Assets/_Game/Data/City","Interiors");
        }

        public static void ValidateManifest(IReadOnlyList<TownInteriorDefinitionSO> definitions)
        {
            if(definitions==null||definitions.Count!=24)throw new InvalidOperationException("S01 requires exactly 24 HouseId records.");
            ValidateSourceContracts(definitions);
            var expected=new HashSet<string>(TownCityLayout.AllBuildings.Select(l=>l.Name),StringComparer.Ordinal);
            var knownNpc=new HashSet<string>(TownCityLayout.AllNpcPlaces.Select(n=>n.NpcId),StringComparer.Ordinal);
            var houses=new HashSet<string>(StringComparer.Ordinal);
            var residents=new HashSet<string>(StringComparer.Ordinal);
            var providers=new HashSet<string>(StringComparer.Ordinal);
            int beds=0,guests=0,indoor=0,outdoor=0;var serviceIds=new HashSet<string>(StringComparer.Ordinal);
            foreach(var definition in definitions)
            {
                if(definition==null||string.IsNullOrEmpty(definition.HouseId)||!expected.Contains(definition.HouseId)||!houses.Add(definition.HouseId))
                    throw new InvalidOperationException("Unknown or duplicate HouseId: "+definition?.HouseId);
                string house=definition.HouseId;
                if(definition.UsableFloorSize.x<=0||definition.UsableFloorSize.y<=0||definition.Rooms==null||definition.Rooms.Length<3||definition.Props==null)
                    throw new InvalidOperationException(house+": incomplete spatial programme.");
                if(definition.TemplateId!=TemplateFor(house)||definition.InteriorPrefabKey!="Assets/_Game/Prefabs/City/Interiors/"+house+".prefab"||
                    definition.EntryAnchorId!="town_entry_"+house||definition.ReturnAnchorId!="town_return_"+house)
                    throw new InvalidOperationException(house+": template, prefab key or stable anchor mismatch.");
                if(string.IsNullOrEmpty(definition.ResponsibleNpcId) && house!="House_MarketHall")
                    throw new InvalidOperationException(house+": missing custodian/owner.");
                if(!string.IsNullOrEmpty(definition.ResponsibleNpcId)&&!knownNpc.Contains(definition.ResponsibleNpcId))
                    throw new InvalidOperationException(house+": unknown responsible NPC "+definition.ResponsibleNpcId);
                foreach(string npc in definition.ResidentNpcIds)
                    if(!knownNpc.Contains(npc)||!residents.Add(npc)||!CanonicalHomes.TryGetValue(npc,out var home)||home!=house)
                        throw new InvalidOperationException(house+": unknown, duplicate or misplaced resident "+npc);
                foreach(string npc in definition.PrimaryIndoorProviderNpcIds)
                    if(!knownNpc.Contains(npc)||!providers.Add(npc)||!CanonicalProviderVenues.TryGetValue(npc,out var indoorVenue)||indoorVenue!=house+"|I")
                        throw new InvalidOperationException(house+": unknown, duplicate or misplaced indoor provider "+npc);
                foreach(string npc in definition.PrimaryOutdoorProviderNpcIds)
                    if(!knownNpc.Contains(npc)||!providers.Add(npc)||!CanonicalProviderVenues.TryGetValue(npc,out var outdoorVenue)||outdoorVenue!=house+"|O")
                        throw new InvalidOperationException(house+": unknown, duplicate or misplaced outdoor provider "+npc);
                indoor+=definition.PrimaryIndoorProviderNpcIds.Length;outdoor+=definition.PrimaryOutdoorProviderNpcIds.Length;
                foreach(string serviceId in definition.ExistingServiceIds)
                    if(NpcServiceCatalog.GetById(serviceId)==null&&serviceId!=CityServiceCatalog.FarmRegistryService.ServiceId&&
                       serviceId!=CityServiceCatalog.MarketStallService.ServiceId || !serviceIds.Add(serviceId))
                        throw new InvalidOperationException(house+": unknown or duplicate service "+serviceId);
                var expectedServices=NpcServiceCatalog.All.Where(service=>
                    definition.PrimaryIndoorProviderNpcIds.Contains(service.NpcId)||definition.PrimaryOutdoorProviderNpcIds.Contains(service.NpcId))
                    .Select(service=>service.ServiceId).ToList();
                if(house=="House_Registry")expectedServices.Add(CityServiceCatalog.FarmRegistryService.ServiceId);
                if(house=="House_Tovin")expectedServices.Add(CityServiceCatalog.MarketStallService.ServiceId);
                if(!definition.ExistingServiceIds.OrderBy(id=>id).SequenceEqual(expectedServices.OrderBy(id=>id)))
                    throw new InvalidOperationException(house+": canonical service mapping changed.");
                if((house=="House_Chamber"||house=="House_Prison"||house=="House_MarketHall")&&definition.ResidentNpcIds.Length>0)
                    throw new InvalidOperationException(house+": institutional building cannot invent residents.");
                var roomIds=new HashSet<string>(StringComparer.Ordinal);foreach(var room in definition.Rooms)
                {
                    if(room==null||string.IsNullOrEmpty(room.RoomId)||!roomIds.Add(room.RoomId)||room.LocalFloorBounds.width<=0||room.LocalFloorBounds.height<=0||
                        string.IsNullOrWhiteSpace(room.Purpose)||room.LocalFloorBounds.xMin < -definition.UsableFloorSize.x*.5f-.001f||
                        room.LocalFloorBounds.xMax > definition.UsableFloorSize.x*.5f+.001f||
                        room.LocalFloorBounds.yMin < -definition.UsableFloorSize.y*.5f-.001f||
                        room.LocalFloorBounds.yMax > definition.UsableFloorSize.y*.5f+.001f)
                        throw new InvalidOperationException(house+": invalid, off-floor or duplicate room.");
                }
                string[] expectedRoomSpecs=RoomPlans[house].Split(';');
                if(roomIds.Count!=expectedRoomSpecs.Length)throw new InvalidOperationException(house+": mandatory room missing.");
                foreach(string spec in expectedRoomSpecs)
                {
                    string[] fields=spec.Split(':');string[] values=fields[1].Split(',');
                    var bounds=new Rect(ParseFloat(values[0]),ParseFloat(values[1]),ParseFloat(values[2]),ParseFloat(values[3]));
                    var room=definition.Rooms.FirstOrDefault(r=>r.RoomId==fields[0]);
                    if(room==null||room.LocalFloorBounds!=bounds)throw new InvalidOperationException(house+": mandatory room geometry changed: "+fields[0]);
                }
                var propIds=new HashSet<string>(StringComparer.Ordinal);
                foreach(var prop in definition.Props)
                {
                    if(prop==null||!propIds.Add(prop.PropId)||!prop.PropId.StartsWith(house+"/",StringComparison.Ordinal)||
                        !roomIds.Contains(prop.RoomId)||string.IsNullOrEmpty(prop.Purpose)||string.IsNullOrEmpty(prop.AssetKey)||
                        (!prop.AssetKey.StartsWith("candidate:",StringComparison.Ordinal)&&!prop.AssetKey.StartsWith("asset:",StringComparison.Ordinal))||
                        (prop.Role==TownInteriorPropRole.ExistingInteraction && string.IsNullOrEmpty(prop.ExistingInteractionId)))
                        throw new InvalidOperationException(house+": invalid prop "+prop?.PropId);
                    var owningRoom=definition.Rooms.Single(r=>r.RoomId==prop.RoomId);
                    if(!owningRoom.LocalFloorBounds.Contains(prop.LocalPosition))
                        throw new InvalidOperationException(house+": prop lies outside room: "+prop.PropId);
                    if(definition.Props.Any(other=>other!=prop&&other!=null&&other.RoomId==prop.RoomId&&
                        (other.LocalPosition-prop.LocalPosition).sqrMagnitude<.01f))
                        throw new InvalidOperationException(house+": duplicate prop placement in "+prop.RoomId);
                    if(prop.PropId.Contains("/bed_npc_"))beds++;
                    if(prop.PropId.EndsWith("/GuestBed_Inn",StringComparison.Ordinal)||prop.PropId.EndsWith("/GuestBed_Inn_02",StringComparison.Ordinal))guests++;
                }
                foreach(string group in PropPlans[house].Split(';'))
                {
                    string[] pair=group.Split(':');
                    foreach(string id in pair[1].Split(','))
                        if(!propIds.Contains(house+"/"+pair[0]+"/"+id))
                            throw new InvalidOperationException(house+": mandatory individual prop missing: "+id);
                }
                foreach(string resident in definition.ResidentNpcIds)
                    if(definition.Props.Count(p=>p.PropId.EndsWith("/bed_"+resident,StringComparison.Ordinal))!=1||
                        definition.Props.Count(p=>p.PropId.EndsWith("/storage_"+resident,StringComparison.Ordinal))!=1)
                        throw new InvalidOperationException(house+": resident bed/storage missing: "+resident);
                foreach(string npc in definition.PrimaryIndoorProviderNpcIds)
                    if(definition.Props.Count(p=>p.PropId.EndsWith("/counter_"+npc,StringComparison.Ordinal)&&
                        p.Role==TownInteriorPropRole.ExistingInteraction&&p.ExistingInteractionId=="shop_"+npc.Substring(4))!=1)
                        throw new InvalidOperationException(house+": existing shop counter binding missing: "+npc);
                foreach(string service in definition.ExistingServiceIds)
                    if(service==CityServiceCatalog.FarmRegistryService.ServiceId||service==CityServiceCatalog.MarketStallService.ServiceId||
                        definition.PrimaryIndoorProviderNpcIds.Any(npc=>NpcServiceCatalog.GetById(service)?.NpcId==npc))
                        if(definition.Props.Count(p=>p.Role==TownInteriorPropRole.ExistingInteraction&&p.ExistingInteractionId==service)!=1)
                            throw new InvalidOperationException(house+": existing service interaction missing: "+service);
                string station=house=="House_Inn"?"Station_CookingStation":house=="House_Blacksmith"?"Station_Forge":
                    house=="House_AlchemyLab"?"Station_Alchemy":house=="House_Workshop"?"Station_Carpentry":
                    house=="House_Residential_3"?"Station_Sewing":house=="House_MarketHall"?"Station_Workbench":null;
                if(station!=null&&definition.Props.Count(p=>p.PropId.EndsWith("/"+station,StringComparison.Ordinal)&&
                    p.Role==TownInteriorPropRole.ExistingInteraction&&p.ExistingInteractionId=="station_"+house.ToLowerInvariant())!=1)
                    throw new InvalidOperationException(house+": existing station binding missing: "+station);
                if(house=="House_Inn"&&(!propIds.Contains(house+"/guest_01/GuestBed_Inn")||
                    !propIds.Contains(house+"/guest_02/GuestBed_Inn_02")))
                    throw new InvalidOperationException(house+": two distinct guest rooms/beds required.");
                if(house=="House_Temple"&&(definition.Access.CivicStartHour!=6||definition.Access.CivicEndHour!=22||
                    definition.Access.CommercialEntryFollowsProviderSchedule))
                    throw new InvalidOperationException(house+": public Temple entry cannot follow Corvus shop schedule.");
                if(house=="House_Inn"&&(!definition.Access.PublicLobbyAlwaysOpen||definition.Access.CommercialEntryFollowsProviderSchedule))
                    throw new InvalidOperationException(house+": Inn public lobby must remain open independently of shop hours.");
                if(definition.PrimaryIndoorProviderNpcIds.Length>0&&house!="House_Inn"&&house!="House_Temple"&&
                    !definition.Access.CommercialEntryFollowsProviderSchedule)
                    throw new InvalidOperationException(house+": commercial entry must follow provider schedule.");
            }
            foreach(string npc in UnhousedOutdoorProviderNpcIds)
                if(!knownNpc.Contains(npc)||!providers.Add(npc)||!CanonicalProviderVenues.ContainsKey(npc))
                    throw new InvalidOperationException("S01 unknown or duplicate unhoused outdoor provider "+npc);
            outdoor+=UnhousedOutdoorProviderNpcIds.Length;
            foreach(string service in UnhousedOutdoorServiceIds)
                if(NpcServiceCatalog.GetById(service)==null||!serviceIds.Add(service))
                    throw new InvalidOperationException("S01 unknown or duplicate unhoused outdoor service "+service);
            if(houses.Count!=expected.Count||residents.Count!=CanonicalHomes.Count||providers.Count!=23||indoor!=13||outdoor!=10||beds!=23||guests!=2||
                serviceIds.Count!=NpcServiceCatalog.ServiceCount+2||
                CanonicalOutdoorHomeNpcIds.Any(residents.Contains))
                throw new InvalidOperationException($"S01 census mismatch houses={houses.Count} residents={residents.Count} providers={providers.Count} indoor={indoor} outdoor={outdoor} beds={beds} guests={guests}");
        }
    }
}
