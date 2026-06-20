using System.Collections.Generic;
using CindarsHope.Items;

namespace CindarsHope.NPC.Friendship
{
    /// <summary>
    /// fable_26 (emenda 2026-06-13-V3 §2/§6) — classificação pura de um presente pelo gosto do NPC
    /// e mapeamento do nível de gosto para o delta de amizade. Sem Unity, sem estado: 100% testável.
    ///
    /// Porteiro: o item precisa carregar ItemTag.Giftable, senão NÃO é presenteável (caller recusa).
    /// Precedência (mais forte vence, do mais negativo ao mais positivo):
    ///     hated > disliked > loved(id) > liked(tag) > neutral(default).
    /// Deltas: loved +12, liked +6, neutral +2, disliked -2, hated -6.
    /// </summary>
    public static class GiftTasteClassifier
    {
        public const int DeltaLoved = 12;
        public const int DeltaLiked = 6;
        public const int DeltaNeutral = 2;
        public const int DeltaDisliked = -2;
        public const int DeltaHated = -6;

        /// <summary>
        /// Mapeia o nível de gosto para o delta de pontos de amizade.
        /// </summary>
        public static int DeltaFor(GiftTaste taste)
        {
            switch (taste)
            {
                case GiftTaste.Loved: return DeltaLoved;
                case GiftTaste.Liked: return DeltaLiked;
                case GiftTaste.Neutral: return DeltaNeutral;
                case GiftTaste.Disliked: return DeltaDisliked;
                case GiftTaste.Hated: return DeltaHated;
                default: return DeltaNeutral;
            }
        }

        /// <summary>
        /// True se o item pode ser presenteado (carrega ItemTag.Giftable). Item sem a tag
        /// é recusa silenciosa (0 ganho/perda) — o caller não deve aplicar delta.
        /// </summary>
        public static bool IsGiftable(ItemDefinition item)
        {
            return item != null && item.HasTag(ItemTag.Giftable);
        }

        /// <summary>
        /// Classifica o gosto do NPC pelo item, respeitando a precedência.
        /// preferences nula/vazia ⇒ fallback Neutral (sem exceção). Não verifica Giftable aqui
        /// (o caller faz o gate); esta função só decide o NÍVEL.
        /// </summary>
        public static GiftTaste Classify(NpcGiftPreferences preferences, ItemDefinition item)
        {
            if (item == null)
            {
                return GiftTaste.Neutral;
            }

            // Sem preferências populadas ⇒ default neutral (gating honesto da emenda §5).
            if (preferences == null)
            {
                return GiftTaste.Neutral;
            }

            var itemTags = item.LoreTags; // tags de gosto gift_* vivem em LoreTags do ItemDefinition

            // 1) hated (mais forte) — por tag.
            if (AnyTagMatches(preferences.HatedItemTags, itemTags))
            {
                return GiftTaste.Hated;
            }

            // 2) disliked — por tag.
            if (AnyTagMatches(preferences.DislikedItemTags, itemTags))
            {
                return GiftTaste.Disliked;
            }

            // 3) loved — por id de item (rejeição pessoal já venceu acima).
            if (ListContains(preferences.LovedItemIds, item.ItemId))
            {
                return GiftTaste.Loved;
            }

            // 4) liked — por tag.
            if (AnyTagMatches(preferences.LikedItemTags, itemTags))
            {
                return GiftTaste.Liked;
            }

            // 5) neutral (default) — tag neutral explícita ou qualquer Giftable não classificado.
            return GiftTaste.Neutral;
        }

        /// <summary>
        /// Conveniência: classifica e já devolve o delta. Não aplica gate Giftable.
        /// </summary>
        public static int DeltaForGift(NpcGiftPreferences preferences, ItemDefinition item)
        {
            return DeltaFor(Classify(preferences, item));
        }

        private static bool AnyTagMatches(List<string> prefTags, List<string> itemTags)
        {
            if (prefTags == null || prefTags.Count == 0 || itemTags == null || itemTags.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < prefTags.Count; i++)
            {
                var pref = prefTags[i];
                if (string.IsNullOrEmpty(pref))
                {
                    continue;
                }

                for (int j = 0; j < itemTags.Count; j++)
                {
                    if (string.Equals(pref, itemTags[j], System.StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool ListContains(List<string> list, string value)
        {
            if (list == null || list.Count == 0 || string.IsNullOrEmpty(value))
            {
                return false;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i], value, System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
