using System;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Events;

namespace CindarsHope.Enemy
{
    public class EnemySpawnResolver
    {
        private readonly IReadOnlyList<EnemySpawnProfileSO> _profiles;
        private readonly IReadOnlyList<EnemySpawnPackSO> _packs;
        private readonly IReadOnlyList<EnemyFactionLockSO> _factionLocks;

        public EnemySpawnResolver(
            IEnumerable<EnemySpawnProfileSO> profiles,
            IEnumerable<EnemySpawnPackSO> packs = null,
            IEnumerable<EnemyFactionLockSO> factionLocks = null)
        {
            _profiles = profiles?.Where(p => p != null).ToList() ?? new List<EnemySpawnProfileSO>();
            _packs = packs?.Where(p => p != null).ToList() ?? new List<EnemySpawnPackSO>();
            _factionLocks = factionLocks?.Where(l => l != null).ToList() ?? new List<EnemyFactionLockSO>();
        }

        public EnemySpawnResult Resolve(EnemySpawnRequest request)
        {
            request = (request ?? new EnemySpawnRequest()).Normalize();
            var result = new EnemySpawnResult { RequestSummary = request.BuildSummary() };
            var random = new Random(request.Seed);

            if (request.MaxEnemies <= 0)
            {
                result.Warnings.Add("EnemySpawnResolver: request MaxEnemies is 0; no enemies can be selected.");
                PublishWarnings(request, result);
                return result;
            }

            var profileMap = _profiles
                .Where(p => p.IsEnabled && !string.IsNullOrWhiteSpace(p.EnemyId))
                .GroupBy(p => p.EnemyId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.Weight).First());

            var validPacks = BuildValidPacks(request, profileMap, result);
            if (validPacks.Count > 0)
            {
                var selectedPack = PickWeighted(validPacks, p => p.Weight, random);
                result.SelectedPackId = selectedPack.PackId;
                AddPackSelections(selectedPack, request, profileMap, random, result);
                result.IsValid = result.SelectedEnemies.Count > 0;
                if (!result.IsValid)
                {
                    result.Warnings.Add($"EnemySpawnResolver: pack '{selectedPack.PackId}' selected but produced 0 enemies.");
                    result.Warnings.Add(BuildDiagnosticSummary(request));
                }
                PublishResult(request, result);
                return result;
            }

            var validProfiles = BuildValidProfiles(request, result);
            if (validProfiles.Count == 0)
            {
                result.Warnings.Add("EnemySpawnResolver: no spawn pack or individual candidate matched the request.");
                result.Warnings.Add(BuildDiagnosticSummary(request));
                PublishWarnings(request, result);
                return result;
            }

            AddIndividualSelections(validProfiles, request, random, result);
            result.IsValid = result.SelectedEnemies.Count > 0;
            if (!result.IsValid)
            {
                result.Warnings.Add("EnemySpawnResolver: profiles passed filters but produced 0 enemies.");
                result.Warnings.Add(BuildDiagnosticSummary(request));
            }
            PublishResult(request, result);
            return result;
        }

        private List<EnemySpawnPackSO> BuildValidPacks(
            EnemySpawnRequest request,
            IReadOnlyDictionary<string, EnemySpawnProfileSO> profileMap,
            EnemySpawnResult result)
        {
            var valid = new List<EnemySpawnPackSO>();
            foreach (var pack in _packs)
            {
                if (TryRejectPack(pack, request, profileMap, out var reason))
                {
                    result.RejectedCandidates.Add(new EnemySpawnRejectedCandidate(pack?.PackId ?? string.Empty, reason));
                    continue;
                }

                valid.Add(pack);
            }

            return valid;
        }

        private List<EnemySpawnProfileSO> BuildValidProfiles(EnemySpawnRequest request, EnemySpawnResult result)
        {
            var valid = new List<EnemySpawnProfileSO>();
            foreach (var profile in _profiles)
            {
                if (TryRejectProfile(profile, request, out var reason))
                {
                    result.RejectedCandidates.Add(new EnemySpawnRejectedCandidate(profile?.EnemyId ?? string.Empty, reason));
                    continue;
                }

                valid.Add(profile);
            }

            return valid;
        }

        private void AddPackSelections(
            EnemySpawnPackSO pack,
            EnemySpawnRequest request,
            IReadOnlyDictionary<string, EnemySpawnProfileSO> profileMap,
            Random random,
            EnemySpawnResult result)
        {
            int remaining = Math.Min(request.MaxEnemies, pack.MaxTotalEnemies);
            foreach (var rawEntry in pack.Entries ?? Array.Empty<EnemySpawnPackEntry>())
            {
                if (remaining <= 0)
                    break;

                var entry = NormalizeCopy(rawEntry);
                if (string.IsNullOrWhiteSpace(entry.EnemyId))
                    continue;

                if (!string.IsNullOrWhiteSpace(entry.RequiresUnlockedFactionLock) &&
                    !ContainsId(request.UnlockedFactionLockIds, entry.RequiresUnlockedFactionLock))
                {
                    result.RejectedCandidates.Add(new EnemySpawnRejectedCandidate(entry.EnemyId, $"Pack entry requires faction lock '{entry.RequiresUnlockedFactionLock}'."));
                    continue;
                }

                if (!profileMap.TryGetValue(entry.EnemyId, out var profile))
                {
                    result.RejectedCandidates.Add(new EnemySpawnRejectedCandidate(entry.EnemyId, "Pack entry has no matching EnemySpawnProfileSO."));
                    continue;
                }

                if (TryRejectProfile(profile, request, out var profileRejectReason))
                {
                    result.RejectedCandidates.Add(new EnemySpawnRejectedCandidate(entry.EnemyId, profileRejectReason));
                    continue;
                }

                int min = Math.Min(entry.MinCount, remaining);
                int max = Math.Min(entry.MaxCount, remaining);
                if (max <= 0 || (!entry.IsRequired && entry.Weight <= 0))
                    continue;

                int count = entry.IsRequired
                    ? random.Next(min, max + 1)
                    : random.Next(0, entry.Weight + 1) > 0 ? random.Next(Math.Max(1, min), max + 1) : 0;

                if (count <= 0)
                    continue;

                result.SelectedEnemies.Add(ToSelection(profile, count, pack.PackId, request.AllowElite));
                remaining -= count;
            }
        }

        private void AddIndividualSelections(
            List<EnemySpawnProfileSO> profiles,
            EnemySpawnRequest request,
            Random random,
            EnemySpawnResult result)
        {
            int remaining = request.MaxEnemies;
            int guard = 0;
            while (remaining > 0 && profiles.Count > 0 && guard < 128)
            {
                guard++;
                var profile = PickWeighted(profiles, p => p.Weight, random);
                int count = Math.Min(profile.MaxCountPerRoom, remaining);
                if (count <= 0)
                    break;

                result.SelectedEnemies.Add(ToSelection(profile, count, string.Empty, request.AllowElite && profile.CanSpawnAsElite));
                remaining -= count;
                profiles.Remove(profile);
            }
        }

        private bool TryRejectPack(
            EnemySpawnPackSO pack,
            EnemySpawnRequest request,
            IReadOnlyDictionary<string, EnemySpawnProfileSO> profileMap,
            out string reason)
        {
            reason = string.Empty;
            if (pack == null)
                return Reject("Pack is null.", out reason);
            if (!pack.IsEnabled)
                return Reject("Pack is disabled.", out reason);
            if (string.IsNullOrWhiteSpace(pack.PackId))
                return Reject("PackId is empty.", out reason);
            if (pack.Weight <= 0)
                return Reject("Pack weight is 0.", out reason);
            if (request.CaveLevel < pack.CaveLevelMin || request.CaveLevel > pack.CaveLevelMax)
                return Reject($"Cave level {request.CaveLevel} is outside pack range {pack.CaveLevelMin}-{pack.CaveLevelMax}.", out reason);
            if (!TagsMatch(request.BiomeTags, pack.BiomeTags))
                return Reject("Biome tags do not match pack.", out reason);
            if (!TagsMatch(request.EnvironmentTags, pack.EnvironmentTags))
                return Reject("Environment tags do not match pack.", out reason);
            if (!IsRoomAtLeast(request.RoomSizeClass, pack.MinimumRoomSize))
                return Reject($"Room size {request.RoomSizeClass} is smaller than pack minimum {pack.MinimumRoomSize}.", out reason);
            if (pack.Entries == null || pack.Entries.Length == 0)
                return Reject("Pack has no entries.", out reason);

            foreach (var factionId in pack.RequiredFactionIds ?? Array.Empty<string>())
            {
                if (IsFactionLocked(factionId, request))
                    return Reject($"Required faction '{factionId}' is locked.", out reason);
            }

            bool hasResolvableRequired = false;
            foreach (var entry in pack.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.EnemyId))
                    continue;
                if (!profileMap.TryGetValue(entry.EnemyId, out var profile))
                    continue;
                if (TryRejectProfile(profile, request, out _))
                    continue;
                if (entry.IsRequired)
                    hasResolvableRequired = true;
            }

            if (!hasResolvableRequired)
                return Reject("Pack has no resolvable required entry.", out reason);

            int minimumRequired = pack.Entries.Where(e => e != null && e.IsRequired).Sum(e => Math.Max(0, e.MinCount));
            if (minimumRequired > request.MaxEnemies)
                return Reject($"Pack minimum required count {minimumRequired} exceeds request MaxEnemies {request.MaxEnemies}.", out reason);
            if (minimumRequired > pack.MaxTotalEnemies)
                return Reject($"Pack minimum required count {minimumRequired} exceeds MaxTotalEnemies {pack.MaxTotalEnemies}.", out reason);

            return false;
        }

        private bool TryRejectProfile(EnemySpawnProfileSO profile, EnemySpawnRequest request, out string reason)
        {
            reason = string.Empty;
            if (profile == null)
                return Reject("Profile is null.", out reason);
            if (!profile.IsEnabled)
                return Reject("Profile is disabled.", out reason);
            if (string.IsNullOrWhiteSpace(profile.SpawnProfileId))
                return Reject("SpawnProfileId is empty.", out reason);
            if (string.IsNullOrWhiteSpace(profile.EnemyId))
                return Reject("EnemyId is empty.", out reason);
            if (profile.Weight <= 0)
                return Reject("Profile weight is 0.", out reason);
            if (request.CaveLevel < profile.CaveLevelMin || request.CaveLevel > profile.CaveLevelMax)
                return Reject($"Cave level {request.CaveLevel} is outside profile range {profile.CaveLevelMin}-{profile.CaveLevelMax}.", out reason);
            if (!request.AllowElite && profile.CanSpawnAsElite)
                return Reject("Elite profile rejected because request does not allow elites.", out reason);
            if (!TagsMatch(request.BiomeTags, profile.BiomeTags))
                return Reject("Biome tags do not match profile.", out reason);
            if (!TagsMatch(request.EnvironmentTags, profile.EnvironmentTags))
                return Reject("Environment tags do not match profile.", out reason);
            if (!string.IsNullOrWhiteSpace(profile.FactionLockId) && !ContainsId(request.UnlockedFactionLockIds, profile.FactionLockId))
                return Reject($"FactionLockId '{profile.FactionLockId}' is locked.", out reason);
            if (IsFactionLocked(profile.FactionId, request))
                return Reject($"Faction '{profile.FactionId}' is locked.", out reason);
            if (!string.IsNullOrWhiteSpace(profile.RequiredBossGateProgress) && !ContainsId(request.BossGateProgressIds, profile.RequiredBossGateProgress))
                return Reject($"Required boss gate progress '{profile.RequiredBossGateProgress}' is missing.", out reason);
            if (!IsRoomAllowed(profile, request))
                return Reject($"Room size/tags reject profile. Room={request.RoomSizeClass}, Size={profile.SizeClass}.", out reason);

            return false;
        }

        private bool IsFactionLocked(string factionId, EnemySpawnRequest request)
        {
            if (string.IsNullOrWhiteSpace(factionId))
                return false;

            foreach (var factionLock in _factionLocks)
            {
                if (factionLock == null || factionLock.IsUnlockedByDefault)
                    continue;
                if (!ContainsId(factionLock.UnlocksFactionIds, factionId))
                    continue;
                if (!string.IsNullOrWhiteSpace(factionLock.FactionLockId) && ContainsId(request.UnlockedFactionLockIds, factionLock.FactionLockId))
                    continue;
                if (!string.IsNullOrWhiteSpace(factionLock.RequiredBossGateId) && ContainsId(request.BossGateProgressIds, factionLock.RequiredBossGateId))
                    continue;
                if (factionLock.RequiredCaveLevelMin > 0 && request.CaveLevel >= factionLock.RequiredCaveLevelMin)
                    continue;

                return true;
            }

            return false;
        }

        private static bool IsRoomAllowed(EnemySpawnProfileSO profile, EnemySpawnRequest request)
        {
            if (!IsRoomAtLeast(request.RoomSizeClass, profile.MinimumRoomSizeForSizeClass))
                return false;
            if (!IsRoomAllowedForSizeClass(request.RoomSizeClass, profile.SizeClass))
                return false;
            if (HasAnyTag(request.RoomTags, profile.DeniedRoomTags))
                return false;
            if ((profile.AllowedRoomTags?.Length ?? 0) > 0 && !HasAnyTag(request.RoomTags, profile.AllowedRoomTags))
                return false;

            return true;
        }

        public static bool IsRoomAllowedForSizeClass(EnemyRoomSizeClass roomSize, EnemySizeClass sizeClass)
        {
            return sizeClass switch
            {
                EnemySizeClass.Tiny => roomSize >= EnemyRoomSizeClass.Corridor,
                EnemySizeClass.Small => roomSize >= EnemyRoomSizeClass.Corridor,
                EnemySizeClass.Medium => roomSize >= EnemyRoomSizeClass.Small,
                EnemySizeClass.Large => roomSize >= EnemyRoomSizeClass.Medium,
                EnemySizeClass.Huge => roomSize >= EnemyRoomSizeClass.Large,
                EnemySizeClass.Boss => roomSize >= EnemyRoomSizeClass.Arena,
                _ => false
            };
        }

        private static bool IsRoomAtLeast(EnemyRoomSizeClass actual, EnemyRoomSizeClass required)
        {
            return actual >= required;
        }

        private static bool TagsMatch(IReadOnlyCollection<string> requestedTags, string[] candidateTags)
        {
            if (requestedTags == null || requestedTags.Count == 0)
                return true;
            if (candidateTags == null || candidateTags.Length == 0)
                return false;
            return requestedTags.Any(tag => ContainsId(candidateTags, tag));
        }

        private static bool HasAnyTag(IReadOnlyCollection<string> source, string[] tags)
        {
            if (source == null || tags == null || tags.Length == 0)
                return false;
            return tags.Any(tag => ContainsId(source, tag));
        }

        private static bool ContainsId(IEnumerable<string> values, string id)
        {
            if (string.IsNullOrWhiteSpace(id) || values == null)
                return false;
            return values.Any(v => string.Equals(v, id, StringComparison.OrdinalIgnoreCase));
        }

        private static T PickWeighted<T>(IReadOnlyList<T> items, Func<T, int> weightSelector, Random random)
        {
            int total = Math.Max(1, items.Sum(i => Math.Max(0, weightSelector(i))));
            int roll = random.Next(0, total);
            int cursor = 0;
            foreach (var item in items)
            {
                cursor += Math.Max(0, weightSelector(item));
                if (roll < cursor)
                    return item;
            }

            return items[items.Count - 1];
        }

        private static EnemySpawnSelection ToSelection(EnemySpawnProfileSO profile, int count, string packId, bool isElite)
        {
            return new EnemySpawnSelection
            {
                EnemyId = profile.EnemyId,
                Count = Math.Max(1, count),
                SpawnProfileId = profile.SpawnProfileId,
                PackId = packId ?? string.Empty,
                SizeClass = profile.SizeClass.ToString(),
                IsElite = isElite
            };
        }

        private static EnemySpawnPackEntry NormalizeCopy(EnemySpawnPackEntry source)
        {
            var copy = new EnemySpawnPackEntry
            {
                EnemyId = source?.EnemyId ?? string.Empty,
                MinCount = source?.MinCount ?? 0,
                MaxCount = source?.MaxCount ?? 0,
                Weight = source?.Weight ?? 0,
                IsRequired = source?.IsRequired ?? false,
                RequiresUnlockedFactionLock = source?.RequiresUnlockedFactionLock ?? string.Empty
            };
            copy.Normalize();
            return copy;
        }

        private string BuildDiagnosticSummary(EnemySpawnRequest request)
        {
            // Per-step profile filter counts (SPEC 14A-FIX6 — reveal which rule actually rejects)
            bool Enabled(EnemySpawnProfileSO p) => p != null && p.IsEnabled && !string.IsNullOrWhiteSpace(p.EnemyId) && p.Weight > 0;
            bool MatchLevel(EnemySpawnProfileSO p) => Enabled(p) && request.CaveLevel >= p.CaveLevelMin && request.CaveLevel <= p.CaveLevelMax;
            bool MatchBiome(EnemySpawnProfileSO p) => MatchLevel(p) && TagsMatch(request.BiomeTags, p.BiomeTags);
            bool MatchEnv(EnemySpawnProfileSO p) => MatchBiome(p) && TagsMatch(request.EnvironmentTags, p.EnvironmentTags);
            bool MatchFactionLock(EnemySpawnProfileSO p) => MatchEnv(p) && (string.IsNullOrWhiteSpace(p.FactionLockId) || ContainsId(request.UnlockedFactionLockIds, p.FactionLockId));
            bool MatchFaction(EnemySpawnProfileSO p) => MatchFactionLock(p) && !IsFactionLocked(p.FactionId, request);
            bool MatchBossGate(EnemySpawnProfileSO p) => MatchFaction(p) && (string.IsNullOrWhiteSpace(p.RequiredBossGateProgress) || ContainsId(request.BossGateProgressIds, p.RequiredBossGateProgress));
            bool MatchRoom(EnemySpawnProfileSO p) => MatchBossGate(p) && IsRoomAllowed(p, request);

            int profilesAfterLevel        = _profiles.Count(MatchLevel);
            int profilesAfterBiome        = _profiles.Count(MatchBiome);
            int profilesAfterEnvironment  = _profiles.Count(MatchEnv);
            int profilesAfterFactionLock  = _profiles.Count(MatchFactionLock);
            int profilesAfterFaction      = _profiles.Count(MatchFaction);
            int profilesAfterBossGate     = _profiles.Count(MatchBossGate);
            int profilesAfterRoom         = _profiles.Count(MatchRoom);

            // Per-step pack filter counts
            bool PackEnabled(EnemySpawnPackSO p) => p != null && p.IsEnabled && !string.IsNullOrWhiteSpace(p.PackId) && p.Weight > 0;
            bool PackMatchLevel(EnemySpawnPackSO p) => PackEnabled(p) && request.CaveLevel >= p.CaveLevelMin && request.CaveLevel <= p.CaveLevelMax;
            bool PackMatchBiome(EnemySpawnPackSO p) => PackMatchLevel(p) && TagsMatch(request.BiomeTags, p.BiomeTags);
            bool PackMatchEnv(EnemySpawnPackSO p) => PackMatchBiome(p) && TagsMatch(request.EnvironmentTags, p.EnvironmentTags);
            bool PackMatchRoom(EnemySpawnPackSO p) => PackMatchEnv(p) && IsRoomAtLeast(request.RoomSizeClass, p.MinimumRoomSize);
            bool PackMatchFaction(EnemySpawnPackSO p)
            {
                if (!PackMatchRoom(p)) return false;
                foreach (var f in p.RequiredFactionIds ?? Array.Empty<string>())
                    if (IsFactionLocked(f, request)) return false;
                return true;
            }

            int packsAfterLevel       = _packs.Count(PackMatchLevel);
            int packsAfterBiome       = _packs.Count(PackMatchBiome);
            int packsAfterEnvironment = _packs.Count(PackMatchEnv);
            int packsAfterRoom        = _packs.Count(PackMatchRoom);
            int packsAfterFaction     = _packs.Count(PackMatchFaction);

            // Top rejection reasons for profiles that pass biome (most actionable: shows what's stopping things)
            var rejectionReasons = new Dictionary<string, int>();
            foreach (var profile in _profiles.Where(MatchEnv))
            {
                if (TryRejectProfile(profile, request, out var reason))
                {
                    if (!rejectionReasons.ContainsKey(reason)) rejectionReasons[reason] = 0;
                    rejectionReasons[reason]++;
                }
            }
            var topRejections = string.Join(" | ", rejectionReasons.OrderByDescending(kv => kv.Value).Take(3).Select(kv => $"{kv.Value}x:'{kv.Key}'"));

            var biomeTags = request.BiomeTags != null ? string.Join(",", request.BiomeTags) : "";
            var envTags = request.EnvironmentTags != null ? string.Join(",", request.EnvironmentTags) : "";
            var unlockedLocks = string.Join(",", request.UnlockedFactionLockIds ?? new List<string>());
            var bossGates = string.Join(",", request.BossGateProgressIds ?? new List<string>());

            return $"EnemySpawnResolver diagnostic: CaveLevel={request.CaveLevel}, " +
                   $"BiomeTags=[{biomeTags}], EnvTags=[{envTags}], RoomSize={request.RoomSizeClass}, " +
                   $"ProfilesTotal={_profiles.Count}, ProfilesAfterLevel={profilesAfterLevel}, ProfilesAfterBiome={profilesAfterBiome}, " +
                   $"ProfilesAfterEnvironment={profilesAfterEnvironment}, ProfilesAfterFactionLock={profilesAfterFactionLock}, " +
                   $"ProfilesAfterFaction={profilesAfterFaction}, ProfilesAfterRequiredBossGate={profilesAfterBossGate}, " +
                   $"ProfilesAfterRoom={profilesAfterRoom}, " +
                   $"PacksTotal={_packs.Count}, PacksAfterLevel={packsAfterLevel}, PacksAfterBiome={packsAfterBiome}, " +
                   $"PacksAfterEnvironment={packsAfterEnvironment}, PacksAfterRoom={packsAfterRoom}, PacksAfterFaction={packsAfterFaction}, " +
                   $"UnlockedLocks=[{unlockedLocks}], BossGates=[{bossGates}], " +
                   $"TopRejectedProfiles=[{topRejections}]";
        }

        private static bool Reject(string message, out string reason)
        {
            reason = message;
            return true;
        }

        private static void PublishResult(EnemySpawnRequest request, EnemySpawnResult result)
        {
            if (!string.IsNullOrWhiteSpace(result.SelectedPackId))
            {
                GameEventBus.Publish(new EnemySpawnPackSelectedEvent(
                    request.CaveLevel,
                    request.BiomeTags?.ToArray() ?? Array.Empty<string>(),
                    result.SelectedPackId,
                    result.SelectedEnemies.Select(e => e.EnemyId).ToArray()));
            }

            GameEventBus.Publish(new EnemySpawnResolvedEvent(
                request.CaveLevel,
                request.BiomeTags?.ToArray() ?? Array.Empty<string>(),
                result.SelectedPackId,
                result.SelectedEnemies.Select(e => e.EnemyId).ToArray(),
                result.Warnings.ToArray()));

            PublishWarnings(request, result);
        }

        private static void PublishWarnings(EnemySpawnRequest request, EnemySpawnResult result)
        {
            if (result.Warnings == null || result.Warnings.Count == 0)
                return;

            GameEventBus.Publish(new EnemySpawnResolverWarningEvent(
                request.CaveLevel,
                request.BiomeTags?.ToArray() ?? Array.Empty<string>(),
                result.Warnings.ToArray()));
        }
    }
}
