# SPEC 13A — Enemy Taxonomy, Profiles and Contracts
## Validation Record — 2026-05-27

---

## 1. Resumo do implementado

SPEC 13A finaliza os contratos de dados e perfis para a taxonomia canônica de inimigos de Vaalara/Dornecia. Não cria os 40 EnemyDataSO individuais (SPEC 13B) nem os action sets (13C), brain runtime (13D), bestiary save (13E) ou spawn resolver (13F).

---

## 2. Arquivos alterados

### Scripts (runtime)

| Arquivo | Alteração |
|---|---|
| `Assets/_Game/Scripts/Combat/EnemyDataSO.cs` | Adicionado `LoreTagline` (TextArea) acima de `Description` |
| `Assets/_Game/Scripts/Combat/Data/EnemySizeProfileSO.cs` | Adicionado campo `MinimumRoomSize` (int, default 6) com OnValidate clamp ≥ 4 |
| `Assets/_Game/Scripts/Combat/Data/EnemyVulnerabilityProfileSO.cs` | Adicionados 4 valores ao enum `VulnerabilityTriggerMode`: `AfterProjectileVolley`, `AfterShieldDrop`, `AfterBlinkArrival`, `AfterEnragePulse` |

### Scripts (editor)

| Arquivo | Alteração |
|---|---|
| `Assets/_Game/Scripts/Editor/EnemyTaxonomy/CreateDefaultEnemyProfiles.cs` | **CRIADO** — menu `CindarsHope > SPEC 13 > Create Default Enemy Profiles`; gera 16 factions, 6 size profiles, 10 movement profiles, 10 vulnerability profiles |
| `Assets/_Game/Scripts/Editor/Validation/ValidateSpec13EnemyTaxonomyProfiles.cs` | **CRIADO** — menu `CindarsHope > Validation > Validate SPEC 13A - Enemy Taxonomy` |

### Csproj

| Arquivo | Alteração |
|---|---|
| `Assembly-CSharp-Editor.csproj` | Adicionadas 2 entradas `Compile Include` para os novos scripts de editor |

---

## 3. Verificação de contratos existentes (sem alteração necessária)

| Contrato | Estado | Observação |
|---|---|---|
| `EnemyRole` enum | OK — completo | Chaser, Guard, Ranged, Caster, Burrower, Swarm, Tank, Elite, MiniBoss, Boss — Phase **não existe** |
| `EnemyMovementType` enum | OK — completo | GroundChase, GroundPatrol, GuardStationary, KiteRanged, CasterKeepAway, BurrowAmbush, SwarmErratic, TankSlowPush, PhaseShortBlink, Leaper — Flying ausente |
| `EnemySizeClass` enum | OK — completo | Tiny, Small, Medium, Large, Huge, Boss |
| `EnemyMovementProfileSO` | OK — campos suficientes | MoveSpeed, DetectionRange, LeashRange, AttackRange, PreferredDistance, WanderRadius, CanBurrow, CanPhaseShortBlink, CanLeap, CanFly=false, DecisionTickSeconds |
| `EnemyFactionSO` | OK — campos suficientes | factionId, DisplayName, FactionColor, Description |
| `EnemyDataSO` | OK + LoreTagline adicionado | Retrocompatível — todos os campos de SPEC 13 presentes |
| `EnemyDatabaseSO` | OK — sem alteração | DataRegistrySO<EnemyDataSO> |
| `EnemyBrain` | OK — sem alteração | Referencia EnemyMovementProfileSO por ID (sem referência direta) |

---

## 4. Profiles definidos

### Factions técnicas (16)

| ID | Display |
|---|---|
| faction_beast | Beast |
| faction_fungal | Fungal |
| faction_goblin | Goblin |
| faction_kobold | Kobold |
| faction_orc | Orc |
| faction_duergar | Duergar |
| faction_drow | Drow |
| faction_gnome | Gnome |
| faction_ninrorin | Ninrorin |
| faction_undead | Undead |
| faction_cultist | Cultist |
| faction_elemental | Elemental |
| faction_construct | Construct |
| faction_abyssal | Abyssal |
| faction_corrupted | Corrupted |
| faction_draconic | Draconic |

### Size profiles (6)

| ID | SizeClass | SpriteScale | ColliderRadius | PathingRadius | MinimumRoomSize |
|---|---|---|---|---|---|
| size_tiny | Tiny | 0.65 | 0.22 | 0.20 | 4 |
| size_small | Small | 0.85 | 0.32 | 0.30 | 5 |
| size_medium | Medium | 1.00 | 0.45 | 0.45 | 6 |
| size_large | Large | 1.35 | 0.65 | 0.65 | 8 |
| size_huge | Huge | 1.80 | 0.95 | 0.95 | 12 |
| size_boss | Boss | 2.20 | 1.20 | 1.20 | 16 |

### Movement profiles (10)

| ID | Type | MoveSpeed | CanFly |
|---|---|---|---|
| movement_ground_chase | GroundChase | 2.5 | false |
| movement_ground_patrol | GroundPatrol | 1.5 | false |
| movement_guard_stationary | GuardStationary | 0.8 | false |
| movement_kite_ranged | KiteRanged | 2.0 | false |
| movement_caster_keep_away | CasterKeepAway | 1.8 | false |
| movement_burrow_ambush | BurrowAmbush | 2.2 | false (CanBurrow=true) |
| movement_swarm_erratic | SwarmErratic | 3.0 | false |
| movement_tank_slow_push | TankSlowPush | 1.0 | false |
| movement_phase_short_blink | PhaseShortBlink | 2.0 | false (CanPhaseShortBlink=true) |
| movement_leaper | Leaper | 2.5 | false (CanLeap=true) |

### Vulnerability profiles (10)

| ID | TriggerMode | Window | Multiplier |
|---|---|---|---|
| vuln_swarm_after_bite | AfterAttackRecover | 0.8s | 1.4x |
| vuln_chaser_charge | DuringChargeWindup | 1.0s | 1.6x |
| vuln_ranged_after_volley | AfterProjectileVolley | 1.2s | 1.5x |
| vuln_caster_after_cast | AfterCast | 1.5s | 1.7x |
| vuln_burrow_emerge | AfterBurrowEmerges | 1.0s | 1.8x |
| vuln_guard_shield_drop | AfterShieldDrop | 1.3s | 1.6x |
| vuln_tank_recover | AfterAttackRecover | 1.5s | 1.5x |
| vuln_phase_arrival | AfterBlinkArrival | 0.9s | 1.7x |
| vuln_leaper_landing | AfterAttackRecover | 0.7s | 1.6x |
| vuln_corrupted_enrage_pulse | AfterEnragePulse | 2.0s | 1.9x |

---

## 5. Validações executadas

| Validação | Resultado |
|---|---|
| `dotnet build Assembly-CSharp.csproj` | 0 erros, 0 avisos |
| `dotnet build Assembly-CSharp-Editor.csproj` | 0 erros, 0 avisos |
| `tools/docs/validate_docs.ps1` | PASSED |
| `Phase` não existe como `EnemyRole` | Confirmado — enum não contém Phase |
| `PhaseShortBlink` existe como `EnemyMovementType` | Confirmado |
| `Flying` não existe como `EnemyMovementType` ativo | Confirmado |
| `CanFly = false` em todos os movement profiles criados | Confirmado pelo editor script |
| `EnemyDataSO` retrocompatível | Confirmado — nenhum campo renomeado/removido |

---

## 6. Validações não executadas

| Validação | Motivo |
|---|---|
| `tools/unity/RunUnityCompileValidation.ps1` | Requer Unity Editor aberto; não disponível neste ambiente de CI |
| `tools/unity/ScanUnityLogs.ps1` | Requer Unity Editor aberto |
| `CindarsHope > SPEC 13 > Create Default Enemy Profiles` | Requer Unity Editor — gera os .asset files para factions/size/movement/vuln |
| `CindarsHope > Validation > Validate SPEC 13A - Enemy Taxonomy` | Requer Unity Editor — valida assets criados |
| Play Mode humano | Fora do escopo de 13A — runtime de IA é SPEC 13D |

---

## 7. Riscos residuais

| Risco | Severidade | Mitigação |
|---|---|---|
| Assets de profiles não foram criados (precisam do menu Unity) | Médio | Editor script cria todos ao rodar menu `CindarsHope > SPEC 13 > Create Default Enemy Profiles` |
| VulnerabilityTriggerMode novos não testados em runtime | Baixo | SPEC 13D (EnemyBrain runtime) que os consumirá |
| EnemyBrain ainda referencia `_movementProfile` como `null` se asset não estiver wired | Baixo | Existente antes do 13A; fallback por null-check já presente em EnemyBrain |
| `EnemyDataSO.LoreTagline` opcional — dados antigos têm campo vazio | Mínimo | Retrocompatível; TextArea com string vazia é válido |

---

## 8. Confirmação de escopo

| Item | Status |
|---|---|
| SPEC 13A — taxonomy, profiles, contracts | **FECHADO em código** |
| SPEC 13B — roster 40 EnemyDataSO | **NÃO implementado** (aguarda próximo recorte) |
| SPEC 13C — EnemyActionSO/action sets completos | **NÃO implementado** |
| SPEC 13D — EnemyBrain runtime MVP | **NÃO implementado** |
| SPEC 13E — Bestiary runtime/save | **NÃO implementado** |
| SPEC 13F — SpawnResolver ecologia/faction locks | **NÃO implementado** |

---

## 9. Próximo recorte recomendado

**SPEC 13B — Roster 40 EnemyDataSO**

Criar os 40 EnemyDataSO individuais usando os profiles de 13A como base, distribuídos por faction, cave band e role. Cada entry precisa de `SizeProfileId`, `MovementProfileId`, `VulnerabilityProfileId`, `FactionId`, `XPReward`, `CaveBand`.
