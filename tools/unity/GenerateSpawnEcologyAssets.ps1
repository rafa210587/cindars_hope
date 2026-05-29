# Generates Unity YAML .asset files for EnemySpawnProfileSO, EnemySpawnPackSO, EnemyFactionLockSO
# Data mirrors CreateEnemySpawnEcologyData.cs — edit both in sync.
# Run from repo root: .\tools\unity\GenerateSpawnEcologyAssets.ps1
param([string]$ProjectRoot = ".")
$ErrorActionPreference = "Stop"

$root        = (Resolve-Path $ProjectRoot).Path
$profilesDir = "$root\Assets\_Game\Data\EnemySpawn\Profiles"
$packsDir    = "$root\Assets\_Game\Data\EnemySpawn\Packs"
$locksDir    = "$root\Assets\_Game\Data\EnemySpawn\FactionLocks"

$profileGuid = "c767e8d45998432ea8309677fd010b44"  # EnemySpawnProfileSO.cs guid
$packGuid    = "00919f09a9984916ae0d06ad019a979b"  # EnemySpawnPackSO.cs guid
$lockGuid    = "ef6d9326e8e3497e8daf5235c6038716"  # EnemyFactionLockSO.cs guid

foreach ($d in @($profilesDir, $packsDir, $locksDir)) {
    if (-not (Test-Path $d)) { New-Item -ItemType Directory -Path $d | Out-Null }
}

# EnemySizeClass: Tiny=0 Small=1 Medium=2 Large=3 Huge=4 Boss=5
# EnemyRoomSizeClass: Corridor=0 Small=1 Medium=2 Large=3 Arena=4

function New-FlatGuid { return [System.Guid]::NewGuid().ToString("N") }

function Write-Meta {
    param($assetPath, $guid)
    $meta = @"
fileFormatVersion: 2
guid: $guid
NativeFormatImporter:
  externalObjects: {}
  mainObjectFileID: 11400000
  userData:
  assetBundleName:
  assetBundleVariant:
"@
    [System.IO.File]::WriteAllText("$assetPath.meta", $meta, [System.Text.Encoding]::UTF8)
}

function Title-Case {
    param($s)
    return ($s.Split('_') | Where-Object { $_ } | ForEach-Object {
        $_.Substring(0,1).ToUpper() + $_.Substring(1)
    }) -join ' '
}

function Write-StringArray {
    param($label, [string[]]$items)
    if ($null -eq $items -or $items.Count -eq 0) { return "  ${label}: []" }
    $lines = "  ${label}:"
    foreach ($item in $items) { $lines += "`n  - $item" }
    return $lines
}

function Write-Profile {
    param($dir, $scriptGuid, $enemyId, $minLvl, $maxLvl, $biome, $faction,
          $lockId, $gateId, $weight, $maxCount, [int]$sizeInt,
          [string[]]$packIds, [string[]]$allowedTags, [string[]]$deniedTags)
    $assetPath = "$dir\$enemyId.asset"
    if (Test-Path $assetPath) { return 0 }
    $guid      = New-FlatGuid
    $display   = Title-Case ($enemyId -replace '^enemy_', '')
    $minRoom   = @(0,0,1,2,3,4)[$sizeInt]
    $canElite  = if ($sizeInt -ge 2 -and $sizeInt -lt 5) { 1 } else { 0 }
    $profileId = "spawn_$enemyId"

    $biomeYaml   = Write-StringArray "BiomeTags"    @($biome)
    $envYaml     = Write-StringArray "EnvironmentTags" @($biome)
    $allowedYaml = Write-StringArray "AllowedRoomTags" $allowedTags
    $deniedYaml  = Write-StringArray "DeniedRoomTags"  $deniedTags
    $packYaml    = Write-StringArray "PackIds"         $packIds

    $body = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: $scriptGuid, type: 3}
  m_Name: $enemyId
  m_EditorClassIdentifier:
  SpawnProfileId: $profileId
  DisplayName: $display
  EnemyId: $enemyId
  CaveLevelMin: $minLvl
  CaveLevelMax: $maxLvl
$biomeYaml
$envYaml
  FactionId: $faction
  FactionLockId: $lockId
  RequiredBossGateProgress: $gateId
  Weight: $weight
  MaxCountPerRoom: $maxCount
  CanSpawnAsElite: $canElite
  MinimumRoomSizeForSizeClass: $minRoom
  SizeClass: $sizeInt
$allowedYaml
$deniedYaml
$packYaml
  IsEnabled: 1
"@
    [System.IO.File]::WriteAllText($assetPath, $body, [System.Text.Encoding]::UTF8)
    Write-Meta $assetPath $guid
    return 1
}

function Write-PackAsset {
    param($dir, $scriptGuid, $packId, $minLvl, $maxLvl, $biome,
          [string[]]$factions, [int]$maxTotal, [int]$minRoom, $entries)
    $assetPath = "$dir\$packId.asset"
    if (Test-Path $assetPath) { return 0 }
    $guid    = New-FlatGuid
    $display = Title-Case ($packId -replace '^pack_', '')

    $factionsYaml = Write-StringArray "RequiredFactionIds" $factions
    $biomeYaml    = Write-StringArray "BiomeTags"          @($biome)
    $envYaml      = Write-StringArray "EnvironmentTags"    @($biome)

    $entriesYaml = "  Entries:"
    foreach ($e in $entries) {
        $req = if ($e.req) { 1 } else { 0 }
        $entriesYaml += "`n  - EnemyId: $($e.id)`n    MinCount: $($e.min)`n    MaxCount: $($e.max)`n    Weight: 1`n    IsRequired: $req`n    RequiresUnlockedFactionLock: "
    }

    $body = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: $scriptGuid, type: 3}
  m_Name: $packId
  m_EditorClassIdentifier:
  PackId: $packId
  DisplayName: $display
  CaveLevelMin: $minLvl
  CaveLevelMax: $maxLvl
$biomeYaml
$envYaml
$factionsYaml
$entriesYaml
  Weight: 1
  MinimumRoomSize: $minRoom
  MaxTotalEnemies: $maxTotal
  IsEnabled: 1
"@
    [System.IO.File]::WriteAllText($assetPath, $body, [System.Text.Encoding]::UTF8)
    Write-Meta $assetPath $guid
    return 1
}

function Write-LockAsset {
    param($dir, $scriptGuid, $lockId, [bool]$isDefault, $bossGateId, [int]$minLvl,
          [string[]]$factions, [string[]]$packIds)
    $assetPath = "$dir\$lockId.asset"
    if (Test-Path $assetPath) { return 0 }
    $guid       = New-FlatGuid
    $display    = Title-Case ($lockId -replace '^lock_', '')
    $defaultInt = if ($isDefault) { 1 } else { 0 }

    $factionsYaml = Write-StringArray "UnlocksFactionIds" $factions
    $packsYaml    = Write-StringArray "UnlocksPackIds"    $packIds

    $body = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: $scriptGuid, type: 3}
  m_Name: $lockId
  m_EditorClassIdentifier:
  FactionLockId: $lockId
  DisplayName: $display
  RequiredBossGateId: $bossGateId
  RequiredStoryFlagId:
  RequiredCaveLevelMin: $minLvl
$factionsYaml
$packsYaml
  IsUnlockedByDefault: $defaultInt
"@
    [System.IO.File]::WriteAllText($assetPath, $body, [System.Text.Encoding]::UTF8)
    Write-Meta $assetPath $guid
    return 1
}

function E { param($id,$min,$max,$req=$true); @{id=$id;min=$min;max=$max;req=$req} }

# ── 40 Profiles ───────────────────────────────────────────────────────────────
$profileCount = 0
# stone / levels 1-10
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_cave_mite"                  1 10  "stone" "beast"             "" ""                   4 4 0 @("pack_stone_fauna_basic")       @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_stone_rat"                  1 10  "stone" "beast"             "" ""                   3 3 1 @("pack_stone_fauna_basic")       @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_cave_bat"                   1 10  "stone" "beast"             "" ""                   2 2 1 @()                               @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_goblin_grashnaar_scavenger" 1 10  "stone" "goblin"            "" ""                   3 3 1 @("pack_grashnaar_kobold_scouts","pack_urudakh_trappers") @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_kobold_scout"               1 10  "stone" "kobold"            "" ""                   2 2 1 @("pack_grashnaar_kobold_scouts") @() @()
# fungal / levels 1-25
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_mossling"                   1 25  "fungal" "fungal"           "" ""                   2 2 1 @("pack_blackroot_growth","pack_fungal_colony") @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_cracked_bone"               1 10  "stone" "undead_weak"       "" ""                   2 2 1 @()                               @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_blackroot_sprout"           1 25  "fungal" "fungal"           "" ""                   1 1 2 @("pack_blackroot_growth")         @() @()
# fungal / levels 11-25
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_spore_imp"                 11 25  "fungal" "fungal"           "" ""                   2 2 1 @("pack_fungal_colony")            @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_rootsnare"                 11 25  "fungal" "fungal"           "" ""                   1 1 2 @("pack_fungal_colony")            @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_hollow_stagling"           11 25  "fungal" "beast"            "" ""                   1 1 2 @()                               @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_goblin_urudakh_trapper"    11 25  "fungal" "goblin"           "" ""                   1 1 1 @("pack_urudakh_trappers")         @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_thorn_archer"              11 25  "fungal" "goblin"           "" ""                   1 1 1 @("pack_urudakh_trappers")         @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_orc_nyx_stalker"           11 25  "fungal" "orc_nyx"          "" ""                   1 1 2 @("pack_nyx_ambush")              @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_mycobulwark"               11 25  "fungal" "fungal"           "" ""                   1 1 3 @()                               @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_nyx_moth"                  11 25  "fungal" "beast"            "" ""                   2 2 1 @("pack_nyx_ambush")              @() @()
# ice / levels 26-40
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_frost_gnawer"              26 40  "ice"   "beast"             "lock_after_gate_15" "boss_gate_level_15" 2 2 1 @("pack_frozen_beasts")  @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_duergar_frostdelver"       26 40  "ice"   "duergar"           "lock_after_gate_15" "boss_gate_level_15" 2 2 2 @("pack_duergar_patrol") @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_duergar_shieldbreaker"     26 40  "ice"   "duergar"           "lock_after_gate_15" "boss_gate_level_15" 1 1 2 @("pack_duergar_patrol") @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_icebound_sentinel"         26 40  "ice"   "ice_cult"          "lock_after_gate_15" "boss_gate_level_15" 1 1 3 @("pack_ice_guardians")  @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_glassbone"                 26 40  "ice"   "undead_stronger"   "lock_after_gate_15" "boss_gate_level_15" 1 1 2 @("pack_frozen_beasts")  @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_cold_cult_acolyte"         26 40  "ice"   "ice_cult"          "lock_after_gate_15" "boss_gate_level_15" 1 1 2 @("pack_ice_guardians")  @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_crystal_leaper"            26 40  "ice"   "beast"             "lock_after_gate_15" "boss_gate_level_15" 1 1 2 @()                      @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_frost_wailer"              26 40  "ice"   "undead_stronger"   "lock_after_gate_15" "boss_gate_level_15" 1 1 2 @()                      @() @()
# fire / levels 41-55
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_ember_tick"                41 55  "fire"  "elemental_fire"    "lock_after_gate_30" "boss_gate_level_30" 5 5 0 @("pack_ember_swarm")    @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_ash_crawler"               41 55  "fire"  "beast"             "lock_after_gate_30" "boss_gate_level_30" 1 1 2 @("pack_ember_swarm")    @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_orc_kaand_berserker"       41 55  "fire"  "orc_kaand"         "lock_after_gate_30" "boss_gate_level_30" 1 1 2 @("pack_kaand_warband")  @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_orc_kaand_ashcaller"       41 55  "fire"  "orc_kaand"         "lock_after_gate_30" "boss_gate_level_30" 1 1 2 @("pack_kaand_warband")  @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_lava_bulwark"              41 55  "fire"  "elemental_fire"    "lock_after_gate_30" "boss_gate_level_30" 1 1 4 @("pack_lava_guard")     @() @("corridor")
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_cinder_spitter"            41 55  "fire"  "elemental_fire"    "lock_after_gate_30" "boss_gate_level_30" 2 2 1 @("pack_lava_guard")     @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_scorched_cultist"          41 55  "fire"  "furnace_construct" "lock_after_gate_30" "boss_gate_level_30" 1 1 2 @("pack_furnace_guard")  @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_furnace_warden"            41 55  "fire"  "furnace_construct" "lock_after_gate_30" "boss_gate_level_30" 1 1 3 @("pack_furnace_guard")  @() @()
# ruins / levels 56-70
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_rune_shard"                56 70  "ruins" "stronger_construct" "lock_after_gate_45" "boss_gate_level_45" 4 4 1 @("pack_rune_shards")          @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_clockwork_guard"           56 70  "ruins" "stronger_construct" "lock_after_gate_45" "boss_gate_level_45" 1 1 2 @("pack_rune_shards")          @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_gnome_gem_madcap"          56 70  "ruins" "gnome_ruins"        "lock_after_gate_45" "boss_gate_level_45" 1 1 1 @("pack_gnome_ruin_tinkerers") @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_gnomorin_rune_tinker"      56 70  "ruins" "gnome_ruins"        "lock_after_gate_45" "boss_gate_level_45" 2 2 1 @("pack_gnome_ruin_tinkerers") @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_sealed_knight"             56 70  "ruins" "oathless_undead"    "lock_after_gate_45" "boss_gate_level_45" 1 1 3 @("pack_oathless_dead")        @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_mirror_adept"              56 70  "ruins" "gnome_ruins"        "lock_after_gate_45" "boss_gate_level_45" 1 1 2 @("pack_puzzle_guardians")     @() @()
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_puzzle_golem"              56 70  "ruins" "stronger_construct" "lock_after_gate_45" "boss_gate_level_45" 1 1 4 @("pack_puzzle_guardians")     @() @("corridor")
$profileCount += Write-Profile $profilesDir $profileGuid "enemy_oathless_shade"            56 70  "ruins" "oathless_undead"    "lock_after_gate_45" "boss_gate_level_45" 1 1 2 @("pack_oathless_dead")        @() @()

# ── 17 Packs ──────────────────────────────────────────────────────────────────
$packCount = 0
$packCount += Write-PackAsset $packsDir $packGuid "pack_stone_fauna_basic"       1  10 "stone"  @()                                                      4 0 @(E "enemy_cave_mite" 2 4; E "enemy_stone_rat" 1 2)
$packCount += Write-PackAsset $packsDir $packGuid "pack_grashnaar_kobold_scouts" 1  10 "stone"  @()                                                      5 0 @(E "enemy_goblin_grashnaar_scavenger" 1 3; E "enemy_kobold_scout" 1 2)
$packCount += Write-PackAsset $packsDir $packGuid "pack_blackroot_growth"        1  10 "fungal" @()                                                      3 0 @(E "enemy_mossling" 1 2; E "enemy_blackroot_sprout" 1 1)
$packCount += Write-PackAsset $packsDir $packGuid "pack_fungal_colony"          11  25 "fungal" @()                                                      5 0 @(E "enemy_spore_imp" 1 2; E "enemy_mossling" 1 2; E "enemy_rootsnare" 0 1 $false)
$packCount += Write-PackAsset $packsDir $packGuid "pack_urudakh_trappers"       11  25 "fungal" @()                                                      4 0 @(E "enemy_goblin_urudakh_trapper" 1 1; E "enemy_thorn_archer" 1 1; E "enemy_goblin_grashnaar_scavenger" 1 2)
$packCount += Write-PackAsset $packsDir $packGuid "pack_nyx_ambush"             11  25 "fungal" @()                                                      3 0 @(E "enemy_orc_nyx_stalker" 1 1; E "enemy_nyx_moth" 1 2)
$packCount += Write-PackAsset $packsDir $packGuid "pack_frozen_beasts"          26  40 "ice"    @()                                                      3 0 @(E "enemy_frost_gnawer" 2 2; E "enemy_glassbone" 1 1)
$packCount += Write-PackAsset $packsDir $packGuid "pack_duergar_patrol"         26  40 "ice"    @()                                                      3 1 @(E "enemy_duergar_frostdelver" 1 2; E "enemy_duergar_shieldbreaker" 0 1 $false)
$packCount += Write-PackAsset $packsDir $packGuid "pack_ice_guardians"          26  40 "ice"    @()                                                      2 1 @(E "enemy_icebound_sentinel" 1 1; E "enemy_cold_cult_acolyte" 1 1)
$packCount += Write-PackAsset $packsDir $packGuid "pack_ember_swarm"            41  55 "fire"   @()                                                      6 0 @(E "enemy_ember_tick" 3 5; E "enemy_ash_crawler" 1 1)
$packCount += Write-PackAsset $packsDir $packGuid "pack_kaand_warband"          41  55 "fire"   @()                                                      2 1 @(E "enemy_orc_kaand_berserker" 1 1; E "enemy_orc_kaand_ashcaller" 1 1)
$packCount += Write-PackAsset $packsDir $packGuid "pack_lava_guard"             41  55 "fire"   @()                                                      3 2 @(E "enemy_lava_bulwark" 1 1; E "enemy_cinder_spitter" 1 2)
$packCount += Write-PackAsset $packsDir $packGuid "pack_furnace_guard"          41  55 "fire"   @()                                                      2 1 @(E "enemy_furnace_warden" 1 1; E "enemy_scorched_cultist" 1 1)
$packCount += Write-PackAsset $packsDir $packGuid "pack_rune_shards"            56  70 "ruins"  @()                                                      5 0 @(E "enemy_rune_shard" 2 4; E "enemy_clockwork_guard" 1 1)
$packCount += Write-PackAsset $packsDir $packGuid "pack_gnome_ruin_tinkerers"   56  70 "ruins"  @()                                                      3 0 @(E "enemy_gnome_gem_madcap" 1 1; E "enemy_gnomorin_rune_tinker" 1 2)
$packCount += Write-PackAsset $packsDir $packGuid "pack_oathless_dead"          56  70 "ruins"  @()                                                      2 1 @(E "enemy_sealed_knight" 1 1; E "enemy_oathless_shade" 1 1)
$packCount += Write-PackAsset $packsDir $packGuid "pack_puzzle_guardians"       56  70 "ruins"  @()                                                      2 2 @(E "enemy_puzzle_golem" 1 1; E "enemy_mirror_adept" 1 1)

# ── 7 Faction Locks ───────────────────────────────────────────────────────────
$lockCount = 0
$lockCount += Write-LockAsset $locksDir $lockGuid "lock_default_low_tier" $true  ""                   0  @("beast","fungal","goblin","kobold","undead_weak")    @("pack_stone_fauna_basic","pack_grashnaar_kobold_scouts","pack_blackroot_growth")
$lockCount += Write-LockAsset $locksDir $lockGuid "lock_after_gate_15"    $false "boss_gate_level_15" 16 @("duergar","ice_cult","undead_stronger")               @("pack_frozen_beasts","pack_duergar_patrol","pack_ice_guardians")
$lockCount += Write-LockAsset $locksDir $lockGuid "lock_after_gate_30"    $false "boss_gate_level_30" 31 @("orc_kaand","elemental_fire","furnace_construct")      @("pack_ember_swarm","pack_kaand_warband","pack_lava_guard","pack_furnace_guard")
$lockCount += Write-LockAsset $locksDir $lockGuid "lock_after_gate_45"    $false "boss_gate_level_45" 46 @("gnome_ruins","stronger_construct","oathless_undead") @("pack_rune_shards","pack_gnome_ruin_tinkerers","pack_oathless_dead","pack_puzzle_guardians")
$lockCount += Write-LockAsset $locksDir $lockGuid "lock_after_gate_60"    $false "boss_gate_level_60" 61 @("drow","abyssal","void")                              @("pack_drow_court")
$lockCount += Write-LockAsset $locksDir $lockGuid "lock_after_gate_75"    $false "boss_gate_level_75" 76 @("ninrorin","corrupted","draconic")                    @("pack_ninrorin_broken_echoes","pack_corrupted_draconic_nest")
$lockCount += Write-LockAsset $locksDir $lockGuid "lock_after_gate_90"    $false "boss_gate_level_90" 91 @("blackstone_wyvern")                                  @("pack_blackstone_wyvern_arena")

Write-Host "Done. Profiles: $profileCount | Packs: $packCount | Locks: $lockCount"
