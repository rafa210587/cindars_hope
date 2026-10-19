using System.Collections.Generic;

namespace CindarsHope.NPC.Schedule
{
    [System.Serializable]
    public class NpcScheduleProfile
    {
        public string ScheduleId;
        public string NpcId;
        public List<NpcScheduleBlock> Blocks = new List<NpcScheduleBlock>();

        /// <summary>
        /// fable_11 (CA-1) — coarse archetype that drives per-hour Work/Social/Home/Night resolution
        /// (closes the WAVE25 TIME_BLOCK_DEBT). Default Shopkeeper; set from the roster/generator spec.
        /// </summary>
        public NpcScheduleArchetype Archetype = NpcScheduleArchetype.Shopkeeper;

        /// <summary>
        /// Build the canonical per-block anchor mapping for an NPC: each runtime block points at the
        /// stable anchor ID <c>npc_&lt;id&gt;_&lt;suffix&gt;</c> the generator emits (work/social/home).
        /// Home and Night both resolve to the home anchor (NPC is at/inside the house).
        /// </summary>
        public static NpcScheduleProfile CreateForArchetype(string npcId, NpcScheduleArchetype archetype)
        {
            var profile = new NpcScheduleProfile
            {
                ScheduleId = $"schedule_{npcId}",
                NpcId = npcId,
                Archetype = archetype,
                Blocks = new List<NpcScheduleBlock>
                {
                    BlockFor(npcId, NpcRuntimeBlock.Work),
                    BlockFor(npcId, NpcRuntimeBlock.Social),
                    BlockFor(npcId, NpcRuntimeBlock.Home),
                    BlockFor(npcId, NpcRuntimeBlock.Night)
                }
            };
            return profile;
        }

        private static NpcScheduleBlock BlockFor(string npcId, NpcRuntimeBlock block)
        {
            var suffix = NpcScheduleBlockResolver.AnchorSuffixForBlock(block);
            var available = block == NpcRuntimeBlock.Work || block == NpcRuntimeBlock.Social;
            return new NpcScheduleBlock
            {
                RuntimeBlock = block,
                AnchorId = $"{npcId}_{suffix}",
                ActivityLabel = block.ToString(),
                CanInteract = available
            };
        }

        /// <summary>Anchor ID this profile resolves to at the given hour.</summary>
        public string ResolveAnchorId(int hour)
        {
            var block = NpcScheduleBlockResolver.ResolveBlock(Archetype, hour);
            return $"{NpcId}_{NpcScheduleBlockResolver.AnchorSuffixForBlock(block)}";
        }
    }
}
