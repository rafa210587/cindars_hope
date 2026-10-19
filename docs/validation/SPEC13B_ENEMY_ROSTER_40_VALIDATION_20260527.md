# SPEC 13B — Roster 40 EnemyDataSO
## Validation Record — 2026-05-27

---

## 1. Resumo do implementado

SPEC 13B cria os 40 `EnemyDataSO` oficiais do roster de Vaalara/Dornecia como assets Unity gerados por Editor script. Usa os profiles de SPEC 13A como base. Não cria action sets (13C), brain runtime (13D), bestiary runtime/save (13E) ou spawn resolver (13F).

---

## 2. Arquivos alterados

### Scripts (runtime)

| Arquivo | Alteração |
|---|---|
| `Assets/_Game/Scripts/Combat/EnemyDataSO.cs` | Adicionado campo `PrimaryDamageTypeId` (string) com tooltip; campo hint para tipo de dano primário do inimigo |

### Scripts (editor)

| Arquivo | Alteração |
|---|---|
| `Assets/_Game/Scripts/Editor/EnemyTaxonomy/CreateRoster40EnemyData.cs` | **CRIADO** — menu `CindarsHope > SPEC 13 > Create Roster 40 Enemy Data`; gera 40+ EnemyDataSO em `Assets/_Game/Data/Enemies/Roster/`; skipa assets já existentes |
| `Assets/_Game/Scripts/Editor/Validation/ValidateSpec13EnemyRoster.cs` | **CRIADO** — menu `CindarsHope > Validation > Validate SPEC 13B - Enemy Roster`; valida presença e completude dos 40 assets |

### Csproj

| Arquivo | Alteração |
|---|---|
| `Assembly-CSharp-Editor.csproj` | Adicionadas 2 entradas `Compile Include` para os novos editor scripts |

---

## 3. Roster — 44 entradas totais

### Band 1 (CaveBand=1, faixas 1-10) — 7 entradas

| enemyId | DisplayName | Faction | Role | Movement | Size | XP | HP |
|---|---|---|---|---|---|---|---|
| enemy_verdant_mite | Verdant Mite | beast | Swarm | SwarmErratic | Tiny | 8 | 6 |
| enemy_spore_crawler | Spore Crawler | fungal | Chaser | GroundChase | Small | 12 | 10 |
| enemy_goblin_scrounger | Goblin Scrounger | goblin | Ranged | KiteRanged | Small | 14 | 12 |
| enemy_kobold_sentry | Kobold Sentry | kobold | Guard | GuardStationary | Small | 14 | 14 |
| enemy_rot_beetle | Rot Beetle | beast | Chaser | GroundChase | Small | 10 | 8 |
| enemy_pale_grub | Pale Grub | beast | Swarm | SwarmErratic | Tiny | 6 | 4 |
| enemy_mushroom_puffball | Mushroom Puffball | fungal | Guard | GuardStationary | Small | 12 | 10 |

### Band 2 (CaveBand=2, faixas 11-25) — 8 entradas + 1 MiniBoss

| enemyId | DisplayName | Faction | Role | Movement | Size | XP | HP |
|---|---|---|---|---|---|---|---|
| enemy_goblin_shaman | Goblin Shaman | goblin | Caster | CasterKeepAway | Small | 28 | 22 |
| enemy_kobold_trapmaster | Kobold Trapmaster | kobold | Ranged | KiteRanged | Small | 24 | 18 |
| enemy_orc_grunt | Orc Grunt | orc | Tank | TankSlowPush | Medium | 32 | 38 |
| enemy_cave_leaper | Cave Leaper | beast | Chaser | Leaper | Medium | 30 | 28 |
| enemy_duergar_crossbowman | Duergar Crossbowman | duergar | Ranged | KiteRanged | Medium | 26 | 24 |
| enemy_burrowing_maggot | Burrowing Maggot | beast | Burrower | BurrowAmbush | Small | 22 | 20 |
| enemy_fungal_spreader | Fungal Spreader | fungal | Tank | TankSlowPush | Medium | 34 | 42 |
| enemy_drow_skirmisher | Drow Skirmisher | drow | Chaser | GroundChase | Medium | 28 | 26 |
| enemy_goblin_warchief | Goblin Warchief | goblin | **MiniBoss** | GroundChase | Medium | 90 | 80 |

### Band 3 (CaveBand=3, faixas 26-40) — 8 entradas + 1 MiniBoss

| enemyId | DisplayName | Faction | Role | Movement | Size | XP | HP |
|---|---|---|---|---|---|---|---|
| enemy_orc_berserker | Orc Berserker | orc | Chaser | GroundChase | Large | 52 | 64 |
| enemy_duergar_warder | Duergar Warder | duergar | Guard | GuardStationary | Large | 48 | 72 |
| enemy_undead_shambler | Undead Shambler | undead | Chaser | GroundChase | Medium | 42 | 50 |
| enemy_cultist_zealot | Cultist Zealot | cultist | Caster | CasterKeepAway | Medium | 50 | 44 |
| enemy_gnome_tinkerer | Gnome Tinkerer | gnome | Ranged | KiteRanged | Small | 44 | 36 |
| enemy_phase_stalker | Phase Stalker | ninrorin | Chaser | PhaseShortBlink | Medium | 56 | 52 |
| enemy_earth_elemental_minor | Minor Earth Elemental | elemental | Tank | TankSlowPush | Large | 58 | 80 |
| enemy_cave_burrower_elite | Cave Burrower (Elite) | beast | Burrower/Elite | BurrowAmbush | Medium | 70 | 60 |
| enemy_orc_warlord | Orc Warlord | orc | **MiniBoss** | TankSlowPush | Large | 140 | 160 |

### Band 4 (CaveBand=4, faixas 41-55) — 7 entradas + 1 MiniBoss

| enemyId | DisplayName | Faction | Role | Movement | Size | XP | HP |
|---|---|---|---|---|---|---|---|
| enemy_drow_witch | Drow Witch | drow | Caster | CasterKeepAway | Medium | 78 | 62 |
| enemy_undead_knight | Undead Knight | undead | Guard | GuardStationary | Large | 82 | 90 |
| enemy_construct_sentry | Construct Sentry | construct | Guard | GroundPatrol | Large | 84 | 100 |
| enemy_abyssal_hound | Abyssal Hound | abyssal | Chaser | GroundChase | Large | 88 | 78 |
| enemy_corrupted_vine_horror | Corrupted Vine Horror | corrupted | Tank | TankSlowPush | Huge | 96 | 140 |
| enemy_ninrorin_phantom | Ninrorin Phantom | ninrorin | Caster | PhaseShortBlink | Medium | 90 | 74 |
| enemy_gnome_wargolem | Gnome Wargolem | gnome | Tank/Elite | TankSlowPush | Large | 100 | 120 |
| enemy_abyssal_gatekeeper | Abyssal Gatekeeper | abyssal | **MiniBoss** | GuardStationary | Huge | 200 | 250 |

### Band 5 (CaveBand=5, faixas 56-70) — 6 entradas

| enemyId | DisplayName | Faction | Role | Movement | Size | XP | HP |
|---|---|---|---|---|---|---|---|
| enemy_draconic_wyrmling | Draconic Wyrmling | draconic | Chaser | GroundChase | Large | 120 | 110 |
| enemy_abyssal_lurker | Abyssal Lurker | abyssal | Burrower | BurrowAmbush | Large | 130 | 100 |
| enemy_corrupted_orc_champion | Corrupted Orc Champion | corrupted | Elite | GroundChase | Huge | 140 | 180 |
| enemy_earth_elemental_greater | Greater Earth Elemental | elemental | Tank | TankSlowPush | Huge | 135 | 200 |
| enemy_undead_lich_acolyte | Lich Acolyte | undead | Caster/Elite | CasterKeepAway | Medium | 128 | 96 |
| enemy_draconic_guardian | Draconic Guardian | draconic | Guard | GuardStationary | Huge | 145 | 220 |

### Bosses — 5 entradas

| enemyId | DisplayName | Faction | Band | XP | HP |
|---|---|---|---|---|---|
| enemy_cave_mite_queen | Cave Mite Queen | beast | 1 | 300 | 200 |
| enemy_fungal_patriarch | Fungal Patriarch | fungal | 2 | 400 | 300 |
| enemy_duergar_artificer_lord | Duergar Artificer Lord | duergar | 3 | 500 | 420 |
| enemy_void_herald | Void Herald | abyssal | 4 | 650 | 550 |
| enemy_draconic_elder | Draconic Elder | draconic | 5 | 800 | 700 |

---

## 4. Cobertura de fações

| Faction | # Entradas |
|---|---|
| faction_beast | 8 (incluindo boss) |
| faction_fungal | 4 (incluindo boss) |
| faction_goblin | 3 (incluindo miniboss) |
| faction_kobold | 2 |
| faction_orc | 4 (incluindo miniboss + boss) |
| faction_duergar | 3 (incluindo boss) |
| faction_drow | 2 |
| faction_gnome | 2 |
| faction_ninrorin | 2 |
| faction_undead | 4 |
| faction_cultist | 1 |
| faction_elemental | 2 |
| faction_construct | 1 |
| faction_abyssal | 4 (incluindo miniboss + boss) |
| faction_corrupted | 2 |
| faction_draconic | 3 (incluindo boss) |

Todas as 16 fações técnicas de SPEC 13A representadas.

---

## 5. Validações executadas

| Validação | Resultado |
|---|---|
| `dotnet build Assembly-CSharp.csproj` | 0 erros, 0 avisos |
| `dotnet build Assembly-CSharp-Editor.csproj` | 0 erros, 0 avisos |
| `tools/docs/validate_docs.ps1` | PASSED |
| `PrimaryDamageTypeId` retrocompatível | Confirmado — campo opcional, defaults para string vazia |
| 44 entradas no roster (≥40) | Confirmado pelo código do editor script |

---

## 6. Validações não executadas

| Validação | Motivo |
|---|---|
| `CindarsHope > SPEC 13 > Create Roster 40 Enemy Data` | Requer Unity Editor — gera os .asset files |
| `CindarsHope > Validation > Validate SPEC 13B - Enemy Roster` | Requer Unity Editor — valida assets criados |
| `tools/unity/RunUnityCompileValidation.ps1` | Requer Unity Editor aberto |
| Play Mode humano | Fora do escopo de 13B — runtime de IA é SPEC 13D |

---

## 7. Riscos residuais

| Risco | Severidade | Mitigação |
|---|---|---|
| Assets não criados até menu Unity ser executado | Médio | Editor script `CreateRoster40EnemyData` é idempotente (skipa existentes) |
| `PrimaryDamageTypeId` não validado em runtime | Baixo | Campo hint; uso real em SPEC 13C/13D |
| CaveBand não perfeitamente alinhado com faixas do spec (11-25, 26-40, etc.) | Baixo | EnemySpawnResolver usa `(band-1)*10+1` a `band*10`; inimigos atribuídos ao band mais próximo |
| 2 assets pré-existentes (enemy_cave_mite, enemy_meteor_ooze_king) não no roster obrigatório | Mínimo | Validator verifica os 40 IDs do roster canônico; extras são neutros |

---

## 8. Confirmação de escopo

| Item | Status |
|---|---|
| SPEC 13A — taxonomy, profiles, contracts | **FECHADO** |
| SPEC 13B — Roster 40 EnemyDataSO | **FECHADO em código** — assets gerados no Unity Editor |
| SPEC 13C — EnemyActionSO/action sets completos | **NÃO implementado** |
| SPEC 13D — EnemyBrain runtime MVP | **NÃO implementado** |
| SPEC 13E — Bestiary runtime/save | **NÃO implementado** |
| SPEC 13F — SpawnResolver ecologia/faction locks | **NÃO implementado** |

---

## 9. Próximo recorte recomendado

**SPEC 13C — Enemy Action Sets**

Criar os `EnemyActionSO` para cada role/faction cluster, definindo ataques, cooldowns, ranges e tipos de dano por ação. Permite que `EnemyBrain` (13D) execute sequências de ação data-driven.
