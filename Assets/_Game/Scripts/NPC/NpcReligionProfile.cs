using System.Collections.Generic;

namespace CindarsHope.NPC
{
    // Religion profile for NPC — supports dialogue/reputation/festival behavior.
    // Canon: Kanthor is the public main temple. Anya has no active public cult service.
    public class NpcReligionProfile
    {
        public string DeityWorshipped { get; set; }
        public List<string> DeitySympathy { get; set; } = new List<string>();
        public List<string> DeityDisliked { get; set; } = new List<string>();
        // Whether this NPC participates in public Kanthor temple events
        public bool ParticipatesInKanthorFestivals { get; set; } = false;
        // Private Anya sympathy is allowed as narrative tag; active public altar is blocked by canon
        public bool HasPrivateAnyaSympathy { get; set; } = false;
    }
}
