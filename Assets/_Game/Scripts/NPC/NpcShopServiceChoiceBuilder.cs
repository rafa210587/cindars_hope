using System.Collections.Generic;
using CindarsHope.NPC.Services;

namespace CindarsHope.NPC
{
    public static class NpcShopServiceChoiceBuilder
    {
        /// <summary>
        /// Builds one dialogue choice definition per unique NPC service. Disabled/gated services are
        /// intentionally preserved by NpcServiceAccess.BuildOptions so the dialogue can show honest
        /// requirements instead of hiding the option.
        /// </summary>
        public static List<NpcShopServiceChoiceDefinition> Build(string npcId)
        {
            var choices = new List<NpcShopServiceChoiceDefinition>();
            if (!NpcServiceAccess.HasServices(npcId))
            {
                return choices;
            }

            var options = NpcServiceAccess.BuildOptions(npcId);
            foreach (var option in options)
            {
                if (string.IsNullOrEmpty(option.ServiceId))
                {
                    continue;
                }

                choices.Add(new NpcShopServiceChoiceDefinition(option.Label, option.ServiceId));
            }

            return choices;
        }
    }
}
