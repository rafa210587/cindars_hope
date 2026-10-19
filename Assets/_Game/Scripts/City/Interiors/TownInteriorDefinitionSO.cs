using System;
using UnityEngine;

namespace CindarsHope.City.Interiors
{
    public enum TownInteriorEntryMode { Portal = 0, GuardPostAnnex = 1, AnimalYardAnnex = 2 }
    public enum TownInteriorOwnershipKind { Household = 0, Institutional = 1 }
    public enum TownInteriorPropRole { SolidScenery = 0, FloorDecoration = 1, ExistingInteraction = 2 }

    [Serializable]
    public sealed class TownInteriorAccessDefinition
    {
        public int ResidentialVisitStartHour = 6, ResidentialVisitEndHour = 22;
        public int CivicStartHour, CivicEndHour;
        public bool PublicLobbyAlwaysOpen, CommercialEntryFollowsProviderSchedule;
        public bool HasResidentialVisit, ResidentsBypassEntryHours;
    }

    [Serializable]
    public sealed class TownInteriorRoomDefinition
    {
        public string RoomId, Purpose;
        public Rect LocalFloorBounds;
        public string[] AuthorizedResidentNpcIds = Array.Empty<string>();
    }

    [Serializable]
    public sealed class TownInteriorPropDefinition
    {
        public string PropId, RoomId, AssetKey, Purpose, ExistingInteractionId;
        public TownInteriorPropRole Role;
        public Vector2 LocalPosition;
    }

    /// <summary>Town content only. No save fields or runtime scene references.</summary>
    public sealed class TownInteriorDefinitionSO : ScriptableObject
    {
        public string HouseId, TemplateId, ResponsibleNpcId;
        public TownInteriorEntryMode EntryMode;
        public TownInteriorOwnershipKind OwnershipKind;
        public string[] ResidentNpcIds = Array.Empty<string>();
        public string[] PrimaryIndoorProviderNpcIds = Array.Empty<string>();
        public string[] PrimaryOutdoorProviderNpcIds = Array.Empty<string>();
        public string[] ExistingServiceIds = Array.Empty<string>();
        public Vector2 UsableFloorSize;
        public string InteriorPrefabKey, EntryAnchorId, ReturnAnchorId;
        public TownInteriorAccessDefinition Access = new TownInteriorAccessDefinition();
        public TownInteriorRoomDefinition[] Rooms = Array.Empty<TownInteriorRoomDefinition>();
        public TownInteriorPropDefinition[] Props = Array.Empty<TownInteriorPropDefinition>();
    }
}
