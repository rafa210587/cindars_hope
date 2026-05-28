# SPEC 13C — Enemy Actions / Action Sets
## Validation Record — 2026-05-27

---

## 1. Resumo do implementado

SPEC 13C cria o sistema data-driven de `EnemyActionSO` e `EnemyActionSetSO` para os 40 inimigos do roster canônico. Estende os contratos existentes com campos opcionais, cria 8 telegraph profiles, ~70 EnemyActionSO e 40 EnemyActionSetSO via editor script. Não implementa EnemyBrain runtime (SPEC 13D), Bestiary runtime (SPEC 13E) nem SpawnResolver (SPEC 13F).

---

## 2. Arquivos alterados

### Scripts (runtime)

| Arquivo | Alteração |
|---|---|
| `Assets/_Game/Scripts/Combat/Data/EnemyActionSO.cs` | Adicionados campos: `StatusApplyChance` (float), `VulnerabilityWindowTrigger` (enum), `MinRange` (float), `MaxTargets` (int), `RequiresLineOfSight` (bool), `IsInterruptible` (bool). Corrigido clamp de `Range` para `≥0` em vez de `≥0.5` para SelfBuff. Retrocompatível. |
| `Assets/_Game/Scripts/Combat/Data/EnemyActionSetSO.cs` | Adicionados campos: `FallbackActionId` (string), `RoleTags` (string[]), `Notes` (string, Multiline). Retrocompatível. |

### Scripts (editor)

| Arquivo | Alteração |
|---|---|
| `Assets/_Game/Scripts/Editor/EnemyTaxonomy/CreateEnemyActionsAndSets.cs` | **CRIADO** — menu `CindarsHope > SPEC 13 > Create Enemy Actions and Sets`; gera 8 telegraph profiles, ~70 EnemyActionSO e 40 EnemyActionSetSO em `Assets/_Game/Data/Enemies/`; idempotente |
| `Assets/_Game/Scripts/Editor/Validation/ValidateSpec13EnemyActions.cs` | **CRIADO** — menu `CindarsHope > Validation > Validate SPEC 13C - Enemy Actions` |

### Csproj

| Arquivo | Alteração |
|---|---|
| `Assembly-CSharp-Editor.csproj` | Adicionadas 2 entradas `Compile Include` |

---

## 3. Telegraph profiles criados (8)

| TelegraphProfileId | Cor | BlinkFreq | Windup |
|---|---|---|---|
| telegraph_fast_melee | Amarelo (1,0.9,0) | 0.08s | 0.3s |
| telegraph_heavy_melee | Laranja (1,0.4,0) | 0.15s | 0.6s |
| telegraph_ranged_projectile | Ciano (0,0.9,1) | 0.10s | 0.4s |
| telegraph_caster_spell | Roxo (0.8,0,1) | 0.12s | 0.7s |
| telegraph_area_pulse | Vermelho-laranja (1,0.3,0.1) | 0.10s | 0.5s |
| telegraph_burrow_emerge | Marrom (0.6,0.35,0.1) | 0.08s | 0.35s |
| telegraph_leap | Verde (0.2,0.9,0.2) | 0.08s | 0.4s |
| telegraph_phase | Azul-branco (0.5,0.8,1) | 0.06s | 0.3s |

---

## 4. Action sets criados (40)

| # | ActionSetId | Enemy | Band | # Actions | ActionTypes |
|---|---|---|---|---|---|
| 1 | actionset_enemy_cave_mite | Cave Mite | 1 | 1 | Melee |
| 2 | actionset_enemy_stone_rat | Stone Rat | 1 | 1 | Melee |
| 3 | actionset_enemy_cave_bat | Cave Bat | 1 | 1 | Ranged |
| 4 | actionset_enemy_goblin_grashnaar_scavenger | Goblin Scavenger | 1 | 2 | Ranged + Melee |
| 5 | actionset_enemy_kobold_scout | Kobold Scout | 1 | 2 | Ranged + Melee |
| 6 | actionset_enemy_mossling | Mossling | 1 | 2 | AreaPulse + Melee |
| 7 | actionset_enemy_cracked_bone | Cracked Bone | 1 | 1 | Melee |
| 8 | actionset_enemy_blackroot_sprout | Blackroot Sprout | 1 | 1 | Ranged |
| 9 | actionset_enemy_spore_imp | Spore Imp | 2 | 2 | AreaPulse + Cast |
| 10 | actionset_enemy_rootsnare | Rootsnare | 2 | 1 | Burrow |
| 11 | actionset_enemy_hollow_stagling | Hollow Stagling | 2 | 1 | Leap |
| 12 | actionset_enemy_goblin_urudakh_trapper | Goblin Trapper | 2 | 2 | Ranged + Ranged |
| 13 | actionset_enemy_thorn_archer | Thorn Archer | 2 | 1 | Ranged |
| 14 | actionset_enemy_orc_nyx_stalker | Orc Nyx Stalker | 2 | 2 | Burrow + Melee |
| 15 | actionset_enemy_mycobulwark | Mycobulwark | 2 | 2 | AreaPulse + Melee |
| 16 | actionset_enemy_nyx_moth | Nyx Moth | 2 | 2 | AreaPulse + Cast |
| 17 | actionset_enemy_frost_gnawer | Frost Gnawer | 3 | 1 | Melee |
| 18 | actionset_enemy_duergar_frostdelver | Duergar Frostdelver | 3 | 2 | Melee + Ranged |
| 19 | actionset_enemy_duergar_shieldbreaker | Duergar Shieldbreaker | 3 | 2 | Melee + AreaPulse |
| 20 | actionset_enemy_icebound_sentinel | Icebound Sentinel | 3 | 2 | SelfBuff + Melee |
| 21 | actionset_enemy_glassbone | Glassbone | 3 | 1 | Ranged |
| 22 | actionset_enemy_cold_cult_acolyte | Cold Cult Acolyte | 3 | 1 | Cast |
| 23 | actionset_enemy_crystal_leaper | Crystal Leaper | 3 | 1 | Leap |
| 24 | actionset_enemy_frost_wailer | Frost Wailer | 3 | 2 | AreaPulse + Cast |
| 25 | actionset_enemy_ember_tick | Ember Tick | 4 | 2 | Melee + AreaPulse(hook) |
| 26 | actionset_enemy_ash_crawler | Ash Crawler | 4 | 1 | Melee |
| 27 | actionset_enemy_orc_kaand_berserker | Orc Kaand Berserker | 4 | 2 | Melee + AreaPulse |
| 28 | actionset_enemy_orc_kaand_ashcaller | Orc Kaand Ashcaller | 4 | 2 | Cast + AreaPulse |
| 29 | actionset_enemy_lava_bulwark | Lava Bulwark | 4 | 2 | Melee + AreaPulse |
| 30 | actionset_enemy_cinder_spitter | Cinder Spitter | 4 | 1 | Ranged |
| 31 | actionset_enemy_scorched_cultist | Scorched Cultist | 4 | 2 | Cast + AreaPulse |
| 32 | actionset_enemy_furnace_warden | Furnace Warden | 4 | 2 | AreaPulse + Melee |
| 33 | actionset_enemy_rune_shard | Rune Shard | 5 | 1 | Ranged |
| 34 | actionset_enemy_clockwork_guard | Clockwork Guard | 5 | 1 | Melee |
| 35 | actionset_enemy_gnome_gem_madcap | Gnome Gem Madcap | 5 | 2 | Cast + AreaPulse |
| 36 | actionset_enemy_gnomorin_rune_tinker | Gnomorin Rune Tinker | 5 | 2 | Ranged + AreaPulse |
| 37 | actionset_enemy_sealed_knight | Sealed Knight | 5 | 2 | Melee + AreaPulse |
| 38 | actionset_enemy_mirror_adept | Mirror Adept | 5 | 2 | Cast + Melee(hook) |
| 39 | actionset_enemy_puzzle_golem | Puzzle Golem | 5 | 2 | AreaPulse + Melee |
| 40 | actionset_enemy_oathless_shade | Oathless Shade | 5 | 2 | Cast + AreaPulse |

**Total de EnemyActionSO: 71** (contando action_ember_tick_death_pop e action_mirror_adept_short_blink_strike como hooks futuros com cooldown=999 e notas explícitas).

---

## 5. Hooks futuros (SPEC 13D)

| ActionId | Razão | Resolução MVP | Ativação SPEC 13D |
|---|---|---|---|
| action_ember_tick_death_pop | Death-trigger; não existe mecanismo de morte-como-gatilho ainda | cooldown=999 (inativo em combate normal) | EnemyBrain death event routing |
| action_mirror_adept_short_blink_strike | Blink runtime requer PhaseShortBlink no EnemyBrain | Resolve como MeleeAttack no range atual | EnemyBrain phase/blink state |
| action_oathless_shade_shadow_step | Blink de deslocamento sem teleport runtime | Resolve como CastProjectile no range atual | EnemyBrain phase/blink state |

---

## 6. Conflito de roster: SPEC 13B vs 13C

**Situação:** SPEC 13B criou 44 EnemyDataSO com IDs alternativos (enemy_verdant_mite, enemy_spore_crawler, etc.) que **não pertencem ao roster canônico** definido pelo spec oficial (`spec_enemy_ai_roster_bestiary_faction_locks_runtime.md`) e pelo prompt da SPEC 13C.

**Roster canônico SPEC 13C** usa IDs como: enemy_cave_mite, enemy_stone_rat, enemy_mossling, etc. — alinhados ao spec oficial e ao pre-existing asset `enemy_cave_mite.asset`.

**Impacto:**
- Os 40 EnemyActionSetSO de SPEC 13C referenciam os IDs canônicos.
- Os EnemyDataSO do roster canônico ainda não existem como assets Unity (exceto `enemy_cave_mite.asset` e `enemy_meteor_ooze_king.asset` que são pre-existentes).
- O validator de SPEC 13C emite WARN (não ERROR) para EnemyDataSO canônicos ausentes.

**Reconciliação recomendada (próximo recorte):**
1. Criar EnemyDataSO para os 40 IDs canônicos usando um editor script separado (ou atualizar o script de SPEC 13B).
2. Deprezar/remover o roster alternativo da SPEC 13B (enemy_verdant_mite etc.) ou renomeá-lo como "roster alternativo/legado".
3. Wiring: setar `ActionSetId` em cada EnemyDataSO canônico para o ActionSetId correspondente.

---

## 7. Validações executadas

| Validação | Resultado |
|---|---|
| `dotnet build Assembly-CSharp.csproj` | 0 erros, 1 aviso pre-existente (EnemyBrain._movementProfile CS0649) |
| `dotnet build Assembly-CSharp-Editor.csproj` | 0 erros, 3 avisos (CS0649 em campos opcionais de ActionEntry) |
| `tools/docs/validate_docs.ps1` | PASSED |
| DamageType separado de StatusApplicationIds | Confirmado por estrutura de campos |
| Toda action ofensiva tem TelegraphProfileId no script | Confirmado pela tabela de dados |
| EnemyActionType.SelfBuff existente | Confirmado — icebound_sentinel usa SelfBuff com Range=0 |

---

## 8. Validações não executadas

| Validação | Motivo |
|---|---|
| `CindarsHope > SPEC 13 > Create Enemy Actions and Sets` | Requer Unity Editor — gera os .asset files |
| `CindarsHope > Validation > Validate SPEC 13C - Enemy Actions` | Requer Unity Editor |
| `tools/unity/RunUnityCompileValidation.ps1` | Requer Unity Editor aberto |
| Play Mode humano | Fora do escopo de 13C — runtime de IA é SPEC 13D |

---

## 9. Riscos residuais

| Risco | Severidade | Mitigação |
|---|---|---|
| EnemyDataSO canônicos ausentes — ActionSetId não wired | Médio | Reconciliação de roster recomendada antes de SPEC 13D |
| Roster alternativo (13B) pode confundir spawn resolver | Médio | EnemySpawnResolver resolve por EnemyDatabaseSO registry; não inclui roster alternativo se não adicionado |
| action_ember_tick_death_pop inativo (cooldown=999) | Baixo | Inativo em combate normal; ativação em SPEC 13D |
| Blink runtime não implementado para mirror_adept/oathless_shade | Baixo | Resolve como melee/cast até SPEC 13D; dados prontos |
| status_confuse_minor pode não existir em StatusEffectManager | Baixo | StatusApplicationIds são strings; runtime lida graciosamente com ID ausente até SPEC 13D implementar |

---

## 10. Confirmação de escopo

| Item | Status |
|---|---|
| SPEC 13A — taxonomy, profiles, contracts | **FECHADO** |
| SPEC 13B — Roster 40 EnemyDataSO | **FECHADO em código** (roster alternativo — reconciliação pendente) |
| SPEC 13C — EnemyActionSO/action sets | **FECHADO em código** — assets gerados no Unity Editor |
| SPEC 13D — EnemyBrain runtime MVP | **NÃO implementado** |
| SPEC 13E — Bestiary runtime/save | **NÃO implementado** |
| SPEC 13F — SpawnResolver ecologia/faction locks | **NÃO implementado** |

---

## 11. Próximo recorte recomendado

**SPEC 13D — EnemyBrain Runtime MVP**

Implementar a state machine completa de EnemyBrain com:
- Seleção de ação por ActionSetId (lookup por ID no runtime)
- Execução data-driven de EnemyActionSO (windup → resolve → recover)
- Integração com DamageCalculator (SPEC 11) para criar DamageRequest
- Integração com EnemyTelegraphController (já existente) via TelegraphProfileId
- Lógica de burrow/blink/leap como estados especiais
- Ativação de VulnerabilityWindowTrigger em EnemyHealth/TargetVulnerabilityState
- Ativação de death_pop em morte para ember_tick
