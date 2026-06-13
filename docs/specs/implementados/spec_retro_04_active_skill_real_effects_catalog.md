# Retro-Spec 04 — Catálogo Real de Efeitos de Skills Ativas (executores de combate/utilidade)

> **Spec ID:** `spec_retro_04_active_skill_real_effects_catalog`
> **Status:** RETRO_DOCUMENTED (código implementado em 2026-06; spec escrita a posteriori para reconstrutibilidade)
> **Tipo:** Retro-spec
> **Domínio:** Skills / Combat runtime
> **Código que documenta:**
> - `Assets/_Game/Scripts/Skills/Runtime/Effects/MeleeStrikeSkillEffectExecutor.cs`
> - `Assets/_Game/Scripts/Skills/Runtime/Effects/ProjectileSkillEffectExecutor.cs`
> - `Assets/_Game/Scripts/Skills/Runtime/Effects/SelfRestoreSkillEffectExecutor.cs`
> - `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs` (registrations + pipeline)
> **Evidência de execução:** `docs/validation/wave_gameplay_expansion_2026_06_12_execution_report.md`; base WAVE_INTEGRATION_11 (`docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECTS_GAMEPLAY_REPORT.md`, `WAVE_INTEGRATION_11_SKILL_ACTION_BALANCE_ADDENDUM.md`)
> **Supersedida/complementada por:** `fable_29` (migração para catálogo canônico de skills com EffectId em dados, substituindo o mapeamento estático), `fable_01` (status effects canônicos para `status_bleed`/`status_chill`/`status_poison`), `fable_02`/`fable_27` (posture).

---

# /speckit.specify

## Contexto

WAVE_INTEGRATION_11 criou a ponte slot→efeito (`ActiveSkillExecutionController` + `SkillEffectRegistry`) com a maioria dos efeitos feedback-only. A slice 2026-06-12 substituiu os placeholders por **21 executores reais** (7 melee, 9 projéteis, 1 gadget ofensivo, 3 restauros) usando 3 classes de executor parametrizadas (strategy pattern), mantendo 9 efeitos feedback-only com justificativa (sistema-alvo inexistente). Cooldown passou a ser por skill via `SkillEffectResult.CooldownSeconds`.

## Comportamento implementado

### 1. Pipeline de execução (`ActiveSkillExecutionController`)

- Singleton `DontDestroyOnLoad` auto-criado por `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`.
- Input: teclas `1–4` → slots ativos `0–3`. Bloqueado quando `ModalManager.HasActiveModal`.
- Fluxo: input → resolve skill action equipada no `SkillTreeManager.State` (aceita nodeId OU skillActionId no slot; exige `SkillCategory.EquippableSkill` comprada com `UnlockedSkillActionId`) → mapeia `SkillActionToEffectId` (dicionário estático, ~35 entradas) → resolve executor no `SkillEffectRegistry` → resolve target (`SkillTargetResolver`) → `Execute(context)` → cooldown por slot (`result.CooldownSeconds > 0 ? result.CooldownSeconds : 1.5`) → feedback via `PlayerActionFeedbackEvent`.
- Falhas de resolução publicam feedback explicativo ("slot vazio", "skill não comprada", "sem efeito implementado (Deferred)" etc.) e log estruturado.
- `TryGetEffectIdForValidation(skillActionId, out effectId)` — superfície para validadores editor.

### 2. `MeleeStrikeSkillEffectExecutor` (arco/círculo ao redor do caster)

Parâmetros por instância: `effectId, displayName, baseDamage, range, arcDegrees, staminaCost, lungeDistance=0, knockbackForce=3, cooldownSeconds=3, postureDamageMultiplier=1`.

- Custo: `StaminaManager.TrySpendStamina(staminaCost)` — falha → `InsufficientStamina` (sem cooldown).
- Lunge opcional: move o corpo `facing * lungeDistance` (Rigidbody2D.MovePosition quando existe).
- Hit: `Physics2D.OverlapCircleAll` em `posição + facing * range*0.4` com raio `range`; filtra `EnemyHealth` vivo; quando `arcDegrees < 360`, exige `Angle(facing, toEnemy) <= arc/2`.
- Dano: `DamageRequest(enemyId, baseDamage)` físico com knockback; posture damage `baseDamage * postureDamageMultiplier` via `EnemyPostureState` (F02 — quebra-guarda usa 3×).
- Retorno: sucesso com feedback "acertou N inimigo(s)" / "nenhum inimigo no alcance", `costSpent`, `cooldownStarted` + cooldown da instância.

### 3. `ProjectileSkillEffectExecutor` (projéteis runtime em leque)

Parâmetros: `effectId, displayName, baseDamage, speed, range, damageType, resourceCost, projectileCount=1, spreadDegrees=0, maxHitsPerProjectile=1, cooldownSeconds=4, statusEffectId=null, statusApplyChance=0`.

- Custo por tipo de dano: `Physical` → stamina; demais → mana (`ManaManager.TrySpendMana`).
- Direção: `PlayerController.LastFacingDirection`; leque centrado no facing com passo `spread/(count-1)`.
- Spawn via `ProjectileSpawnService` com `prefab: null` (fallback procedural — retro-spec 01), `VisualStyle = SkillBolt`, `knockbackForce = 2.5`, `spawnOffset = 0.5`, `MaxHits = maxHitsPerProjectile` (pierce).
- Status effect resolvido por id no `StatusEffectDatabase` do bootstrap (hook pronto; aplicação depende dos IDs existirem — fable_01).
- 0 projéteis spawnados → `ProjectileSpawnFailed` (sem cooldown).

### 4. `SelfRestoreSkillEffectExecutor` (restauros de emergência)

Parâmetros: `effectId, displayName, restoreHp, restoreStamina, restoreMana, cooldownSeconds=30`.

- Aplica `PlayerManager.RestoreHP` / `StaminaManager.AddStamina` / `ManaManager.RestoreMana` conforme valores > 0; nada restaurado → `NothingToRestore`.
- `costSpent: false` (restauro é gratuito; o cooldown longo é o balanceador).

### 5. Catálogo REAL registrado (RegisterCombatExecutors)

**Melee (7) — custo em stamina:**

| EffectId | Nome | Dano | Range | Arco | Stamina | Lunge | Knockback | CD | Posture× |
|---|---|---|---|---|---|---|---|---|---|
| `combat.melee.offhand_cut` | Corte com a Mao Inversa | 8 | 1.2 | 140° | 10 | — | 3 | 2.5s | 1 |
| `combat.melee.whirl_cut` | Corte Giratorio | 10 | 1.7 | 360° | 22 | — | 3 | 6s | 1 |
| `combat.melee.leap_attack` | Ataque Saltante | 14 | 1.4 | 120° | 25 | 2.2 | 3 | 7s | 1 |
| `combat.melee.battle_dash` | Avanco de Batalha | 8 | 1.2 | 100° | 20 | 3.0 | 3 | 5s | 1 |
| `melee.avanco_aco` | Avanco de Aco | 12 | 1.3 | 110° | 22 | 2.5 | 3 | 6s | 1 |
| `melee.grito_desafio` | Grito de Desafio | 6 | 2.2 | 360° | 18 | — | 6 | 8s | 1 |
| `melee.investida_quebra_guarda` | Investida Quebra-Guarda | 16 | 1.3 | 90° | 26 | 2.0 | 3 | 9s | **3** |

**Projéteis físicos (4) — custo em stamina:**

| EffectId | Nome | Dano | Vel | Range | Custo | Extras | CD |
|---|---|---|---|---|---|---|---|
| `combat.ranged.charged_shot` | Tiro Carregado | 20 | 12 | 9 | 20 | — | 6s |
| `combat.ranged.line_piercer` | Perfurador em Linha | 12 | 14 | 10 | 18 | pierce 5 | 7s |
| `combat.ranged.multishot_fan` | Leque de Flechas | 8 | 11 | 7 | 24 | 3 proj / 28° | 8s |
| `combat.ranged.bleeding_arrow` | Flecha Lacerante | 14 | 12 | 8 | 16 | status `status_bleed` | 6s |

**Projéteis mágicos (6) — custo em mana:**

| EffectId | Nome | Tipo | Dano | Vel | Range | Mana | Extras | CD |
|---|---|---|---|---|---|---|---|---|
| `combat.magic.fire_spark` | Faisca de Fogo | Fire | 12 | 10 | 7 | 10 | — | 3s |
| `combat.magic.ice_bind` | Prisao de Gelo | Ice | 10 | 9 | 7 | 14 | status `status_chill` | 6s |
| `combat.magic.toxic_cloud` | Nuvem Toxica | Toxic | 8 | 7 | 6 | 18 | 3 proj / 40°, status `status_poison` | 8s |
| `combat.magic.lightning_chain` | Corrente Eletrica | Lightning | 12 | 16 | 9 | 20 | pierce 4 | 8s |
| `magic.chama_breve` | Chama Breve | Fire | 8 | 10 | 6 | 8 | — | 2.5s |
| `magic.rajada_gelida` | Rajada Gelida | Ice | 6 | 9 | 6 | 16 | 3 proj / 30°, status `status_chill` | 6s |

**Gadget de crafting (1):** `crafting.bomba_improvisada` — Bomba Improvisada, Toxic, dano 18, vel 8, range 5, custo 20, pierce 3, CD 12s.

**Restauros survival (3):**

| EffectId | Nome | HP | Stamina | Mana | CD |
|---|---|---|---|---|---|
| `survival.kit_emergencia` | Kit de Emergencia | +30 | — | — | 45s |
| `survival.instinto_sobrevivencia` | Instinto de Sobrevivencia | — | +50 | — | 30s |
| `survival.campo_seguro` | Campo Seguro | +15 | +25 | +15 | 60s |

**Farm (1, WAVE11 original):** `farm.crop.water_skill` via `FarmCropSkillEffectExecutor`.

### 6. Feedback-only justificados (9, `RegisterFeedbackExecutors`)

`combat.melee.block` (bloqueio já existe como ação de movimento Left Shift), `combat.ranged.marked_prey` (sistema de marcação pendente), `combat.magic.elemental_ward` (ward pendente), `combat.magic.slowing_sigils` (campo de lentidão pendente), `survival.sinal_retirada`, `survival.isca_improvisada`, `crafting.irrigador_portatil`, `crafting.mecanismo_campo`, `crafting.marca_eficiencia` — todos `FeedbackOnlySkillEffectExecutor` com mensagem explicando a pendência (`DEFERRED_RUNTIME_EFFECT` / `TODO_INTEGRATION_NOT_FINAL`).

## Critérios de aceite (verificáveis no código atual)

1. Os 21 efeitos reais da tabela estão registrados com exatamente os parâmetros listados; os 9 feedback-only têm mensagem de pendência.
2. Skills físicas gastam stamina; mágicas gastam mana; restauros não gastam recurso; recurso insuficiente falha SEM iniciar cooldown.
3. Cooldown por skill vem de `SkillEffectResult.CooldownSeconds` (fallback 1.5s) e é mantido por slot.
4. Melee respeita arco (`Angle <= arc/2` quando < 360°) e aplica posture damage (3× no quebra-guarda).
5. Projéteis de skill usam o fallback procedural (`prefab: null`, `SkillBolt`) e suportam pierce/spread.
6. Execução bloqueada com modal ativo; slot vazio/skill não comprada produz feedback e não executa.

---

# /speckit.plan

## Arquitetura real

| Arquivo | Responsabilidade |
|---|---|
| `ActiveSkillExecutionController.cs` | Singleton runtime: input 1–4, resolução slot→skill→effect→executor, cooldowns por slot, feedback |
| `MeleeStrikeSkillEffectExecutor.cs` | Executor parametrizado de golpe em arco/círculo com lunge e posture |
| `ProjectileSkillEffectExecutor.cs` | Executor parametrizado de projéteis (leque/pierce/status, custo stamina-ou-mana) |
| `SelfRestoreSkillEffectExecutor.cs` | Executor parametrizado de restauro HP/stamina/mana |
| `SkillEffectRegistry` / `ISkillEffectExecutor` / `SkillEffectResult` / `SkillEffectContext` (WAVE11, reuso) | Contrato de registro/execução |
| `FeedbackOnlySkillEffectExecutor` (WAVE11, reuso) | Placeholder justificado |

## Contratos

- `ISkillEffectExecutor { EffectId, Category, TargetType, Execute(SkillEffectContext) : SkillEffectResult }` — uma instância configurada por EffectId (strategy sobre o registry).
- `SkillEffectResult.Succeeded(feedback, costSpent, cooldownStarted, cooldownSeconds)` / `Failed(reason, feedback)`.
- Mapeamento `SkillActionToEffectId` (estático): `skill_melee_*`, `skill_ranged_*`, `skill_magic_*`, `skill_survival_*`, `skill_crafting_*` → effect ids. **TODO_INTEGRATION_NOT_FINAL**: fable_29 move EffectId para `SkillActionSO`/`SkillNodeDataSO`.
- Dependências via `GameBootstrap.Instance`: StaminaManager, ManaManager, PlayerManager, SkillTreeManager, ModalManager, StatusEffectDatabase.
- Feedback: `PlayerActionFeedbackEvent` no GameEventBus; logs `CombatLog: SkillMeleeStrike|SkillProjectileFired|SkillSelfRestore`.

## Decisões e invariantes

- **Tiers de balance** (WAVE_INTEGRATION_11_SKILL_ACTION_BALANCE_ADDENDUM): golpes rápidos ~2–3s CD, golpes pesados 6–9s, restauros 30–60s.
- **Custo só é cobrado se a skill pode executar** (TrySpend antes do efeito); falha de spawn de projétil após custo é o único caso degenerado conhecido (aceito).
- **Sem dedução de custo via SkillDefinition** — custo é do executor (débito declarado no controller, resolvido por fable_29).
- **Nenhum sistema paralelo**: dano via `EnemyHealth.TakeDamage`/`DamageRequest`; projéteis via `ProjectileSpawnService`; comunicação via GameEventBus.

---

# /speckit.tasks

## Reconstrução (passos para refazer do zero)

1. Garantir contrato WAVE11 (`ISkillEffectExecutor`, `SkillEffectRegistry`, `SkillEffectResult` com `CooldownSeconds`, `SkillEffectContext`, `SkillTargetResolver`).
2. Implementar os 3 executores parametrizados com os comportamentos exatos (custo, arco, lunge, leque, pierce, status hook, restauros).
3. Registrar o catálogo completo da seção 5 no `Bootstrap()` do controller (valores exatos das tabelas) e os 9 feedback-only com mensagens de pendência.
4. Implementar o pipeline do controller: input 1–4, guard de modal, resolução tolerante do slot (nodeId ou actionId), validação comprada/equipável, cooldown por slot com fallback 1.5s.
5. Manter o mapeamento estático `SkillActionToEffectId` até fable_29 migrar EffectId para dados.

## Débitos conhecidos

- `status_bleed`/`status_chill`/`status_poison` dependem de IDs confirmados no `StatusEffectDatabase` (fable_01); o hook `statusEffectId` está pronto mas pode resolver null silenciosamente.
- Custo de stamina/mana não vem de `SkillDefinition` (TODO no controller; fable_29).
- Mapeamento skill→effect é hard-coded (fable_29 migra para dados).
- 9 efeitos feedback-only aguardam sistemas-alvo (marcação, wards, campos de lentidão, iscas, buffs de eficiência).
- Sem testes de caracterização dos executores (dependem de física/cena — cenário humano de Play Mode cobre).
- Input legado `UnityEngine.Input` (KeyCode) — migração para Input System fora de escopo.
