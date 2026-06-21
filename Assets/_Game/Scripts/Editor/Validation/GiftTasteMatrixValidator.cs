#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using CindarsHope.NPC;
using CindarsHope.NPC.Gifting;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Validation
{
    /// <summary>
    /// fable_72 — validador de completude da matriz de gostos de presente (estilo F30:
    /// ValidateItemAndShopData / ValidateShopPriceData). Audita a autoria EM CÓDIGO (GiftTasteMatrixData)
    /// contra o roster canônico (NpcTownRosterRegistry) e o vocabulário de tags (§3).
    ///
    /// ERROS (return false / bloqueiam): NPC do roster sem GiftPreferences na matriz; NPC sem >=1 hated;
    /// tag gift_* citada por um NPC fora do vocabulário canônico (AllGiftTags).
    ///
    /// WARNINGS (não bloqueiam — gating honesto do débito A4): LovedItemId citado que ainda não tem
    /// tags gift_* mapeadas (item do A4 não materializado); item mapeado sem tag conhecida. Enquanto o
    /// A4 não materializar itens/tags, o runtime cai no fallback neutral — não declaramos o gosto por
    /// NPC "validado" sem itens/tags reais.
    /// </summary>
    public static class GiftTasteMatrixValidator
    {
        public static bool Run()
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            ValidateNpcCoverage(errors, warnings);
            ValidateTagVocabulary(errors);
            ValidateMappedItemTags(warnings);

            var sb = new StringBuilder();
            sb.AppendLine($"[GiftTasteMatrixValidator] NPCs mapeados: {CountMapped()}; " +
                          $"itens com tags gift_*: {CountTaggedItems()}; " +
                          $"erros: {errors.Count}; warnings (débito A4): {warnings.Count}.");

            foreach (var w in warnings)
            {
                sb.AppendLine($"  WARNING: {w}");
            }

            if (errors.Count == 0)
            {
                sb.AppendLine("  OK — completude estrutural da matriz validada (0 erros).");
                Debug.Log(sb.ToString());
                return true;
            }

            foreach (var e in errors)
            {
                sb.AppendLine($"  ERROR: {e}");
            }
            Debug.LogError(sb.ToString());
            return false;
        }

        private static void ValidateNpcCoverage(List<string> errors, List<string> warnings)
        {
            foreach (var entry in NpcTownRosterRegistry.AllEntries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.NpcId)) continue;

                // NPCs Legacy (ambiente/lore, ex.: npc_vaalara_wanderer_01 = "Lore wanderer") NÃO fazem
                // parte da matriz canônica de relacionamento (§4 cobre os 23 NPCs do roster v1.1; o
                // peregrino é LegacyRetained, sem amizade/presente por design). Cobrá-los era falso-positivo.
                if (entry.PriorityTier == NpcTownRosterRegistry.NpcPriorityTier.Legacy) continue;

                var prefs = GiftTasteMatrixData.TryGetPreferences(entry.NpcId);
                if (prefs == null)
                {
                    errors.Add($"NPC '{entry.NpcId}' do roster sem GiftPreferences na matriz §4.");
                    continue;
                }

                if (prefs.HatedItemTags == null || prefs.HatedItemTags.Count == 0)
                {
                    errors.Add($"NPC '{entry.NpcId}' sem nenhuma tag 'hated' (matriz exige >=1).");
                }

                bool hasAnyPositive =
                    (prefs.LovedItemIds != null && prefs.LovedItemIds.Count > 0) ||
                    (prefs.LikedItemTags != null && prefs.LikedItemTags.Count > 0);
                if (!hasAnyPositive)
                {
                    warnings.Add($"NPC '{entry.NpcId}' sem loved/liked — só reage neutro/negativo (revisar §4).");
                }

                // LovedItemId citado mas ainda sem tag gift_* mapeada = débito A4 (WARNING).
                if (prefs.LovedItemIds != null)
                {
                    foreach (var id in prefs.LovedItemIds)
                    {
                        if (!GiftTasteMatrixData.IsMappedGiftItem(id))
                        {
                            warnings.Add(
                                $"NPC '{entry.NpcId}': LovedItemId '{id}' ainda não tem tags gift_* " +
                                "mapeadas (débito A4 — fallback neutral até materializar).");
                        }
                    }
                }
            }
        }

        private static void ValidateTagVocabulary(List<string> errors)
        {
            var vocab = new HashSet<string>(GiftTasteMatrixData.AllGiftTags);
            foreach (var npcId in GiftTasteMatrixData.MappedNpcIds)
            {
                var prefs = GiftTasteMatrixData.TryGetPreferences(npcId);
                if (prefs == null) continue;
                CheckTags(npcId, "liked", prefs.LikedItemTags, vocab, errors);
                CheckTags(npcId, "disliked", prefs.DislikedItemTags, vocab, errors);
                CheckTags(npcId, "hated", prefs.HatedItemTags, vocab, errors);
                CheckTags(npcId, "neutral", prefs.NeutralItemTags, vocab, errors);
            }
        }

        private static void CheckTags(
            string npcId, string bucket, List<string> tags, HashSet<string> vocab, List<string> errors)
        {
            if (tags == null) return;
            foreach (var t in tags)
            {
                if (string.IsNullOrEmpty(t)) continue;
                if (!vocab.Contains(t))
                {
                    errors.Add($"NPC '{npcId}' ({bucket}): tag '{t}' fora do vocabulário gift_* (§3).");
                }
            }
        }

        private static void ValidateMappedItemTags(List<string> warnings)
        {
            var vocab = new HashSet<string>(GiftTasteMatrixData.AllGiftTags);
            foreach (var itemId in GiftTasteMatrixData.TaggedItemIds)
            {
                var tags = GiftTasteMatrixData.GetItemGiftTags(itemId);
                if (tags == null || tags.Count == 0)
                {
                    warnings.Add($"Item '{itemId}' mapeado sem nenhuma tag gift_* (revisar §3).");
                    continue;
                }
                foreach (var t in tags)
                {
                    if (!vocab.Contains(t))
                    {
                        warnings.Add($"Item '{itemId}': tag '{t}' fora do vocabulário gift_* (§3).");
                    }
                }
            }
        }

        private static int CountMapped()
        {
            int n = 0;
            foreach (var _ in GiftTasteMatrixData.MappedNpcIds) n++;
            return n;
        }

        private static int CountTaggedItems()
        {
            int n = 0;
            foreach (var _ in GiftTasteMatrixData.TaggedItemIds) n++;
            return n;
        }
    }
}
#endif
