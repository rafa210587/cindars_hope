# SPEC — Boundary Residual do Combat: Core / Enemy / Inventory / Player / Skills

> **Spec ID:** `spec_arch_combat_boundary_residual_v1`
> **Status:** A implementar
> **Wave:** WAVE ARQUITETURA — Redução de Acoplamento Modular Residual
> **Priority:** P2
> **Type:** Runtime
> **Domain:** Combat / Core / Enemy / Inventory / Player / Skills
> **Parallelizable:** CONDITIONAL
> **Parallel group:** arch_boundary_residual
> **Can run with:** `spec_arch_player_gameplay_boundary_residual_v1` (arquivos não se sobrepõem — ver seção 22; ambas tocam `Player/`, então lock deve ser combinado/serializado se rodarem no mesmo lote)
> **Must not run with:** qualquer spec que altere dano, cooldown, alcance, drops, IA de inimigo, ou save schema de combate/inventário; qualquer spec que toque `EnemyBrain.cs`, `EnemyDataSO.cs`, `WeaponDataSO.cs`, `SpellDataSO.cs`, `StatusEffectSO.cs`
> **Repo lock scope:** `Assets/_Game/Scripts/Combat/**`, `Assets/_Game/Scripts/Core/Data/**`, `Assets/_Game/Scripts/Core/Events/**`, `Assets/_Game/Scripts/Enemy/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Scripts/Player/**`, `Assets/_Game/Scripts/Skills/**` (somente os arquivos citados nesta spec — não os diretórios inteiros)
> **Depends on (Depende de):**
> - Nenhuma. Esta spec é Phase-0-first: mapa de dependência real + decisão do primeiro par seguro. Refactor de código é condicionado a cobertura de teste ou é adiado por completo para uma spec de cobertura.
> **Blocks (Bloqueia):**
> - Sub-slices derivadas futuras, uma por par (não criadas por esta spec — nomeadas na seção 11 se aplicável).
> **Scope:** Mapear (via Grep real) a direção de dependência dos 5 pares mútuos `Combat|Core`, `Combat|Enemy`, `Combat|Inventory`, `Combat|Player`, `Combat|Skills` reportados pelo snapshot de modularização; separar por responsabilidade (dano, cooldown, target, ameaça, skill/passives, drop/reward); decidir o primeiro par seguro; e, se e só se houver cobertura de teste, executar essa quebra mínima. Se não houver cobertura suficiente para nenhum par, a spec entrega uma spec derivada de COBERTURA como próximo passo obrigatório, não um refactor.
> **Out of scope:** Quebrar os 5 pares na mesma spec; qualquer mudança de dano, cooldown, alcance, drops ou IA observável; criar assembly definitions físicas.

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

O mesmo snapshot de arquitetura citado em `spec_arch_player_gameplay_boundary_residual_v1` (rodado nesta sessão, 2026-07-13/14, branch `dev`) reporta 25 pares mútuos totais, dos quais 5 envolvem `Combat`: `Combat|Core`, `Combat|Enemy`, `Combat|Inventory`, `Combat|Player`, `Combat|Skills`. `Combat` é o domínio de maior superfície do projeto — a busca por `using CindarsHope.Core` dentro de `Assets/_Game/Scripts/Combat/` retornou 45 arquivos nesta sessão, cobrindo praticamente todo o subsistema (weapon, spell, status effect, enemy data, projectile, telemetria, feel/juice). Isso torna Combat o domínio de maior risco de regressão silenciosa do lote de redução de acoplamento — é também o sistema mais gameplay-crítico do jogo (dano, cooldown, drops, IA).

Diferente da spec irmã de Player, aqui o volume de arquivos por par é grande o suficiente para que tentar quebrar mais de um par na mesma rodada seja inviável de revisar com segurança. Esta spec, portanto, prioriza o mapa por responsabilidade sobre a execução de refactor.

## 6. Problema

Sem separar por responsabilidade (dano vs. cooldown vs. target vs. ameaça vs. skill/passives vs. drop/reward), qualquer tentativa de "reduzir Combat|X" tende a tocar arquivos com múltiplas responsabilidades misturadas (ex. `PlayerAttackController.cs` toca Core, Enemy, Inventory, Player e Skills simultaneamente), aumentando o raio de impacto de qualquer edição. Combat não tem, hoje, garantia documentada de cobertura de teste de caracterização suficiente para todas as fórmulas de dano/cooldown/target — se a Fase 0 confirmar essa lacuna, tentar refatorar sem antes fechá-la é o maior risco concreto desta spec.

## 7. Objetivo

Ao final desta spec, o projeto deve ter: (a) o mapa de dependência dos 5 pares por direção, verificado por Grep; (b) um mapa por responsabilidade (dano, cooldown, target, ameaça, skill/passives, drop/reward) apontando quais arquivos cobrem qual responsabilidade em cada par; (c) uma decisão explícita do primeiro par seguro a atacar; (d) se a cobertura de teste for insuficiente para esse par, a PRIMEIRA spec derivada recomendada deve ser de COBERTURA (caracterização), não de refactor — isso deve estar declarado explicitamente no execution report; (e) apenas se cobertura já existir, a execução condicional da quebra mínima desse par.

## 8. Fontes obrigatórias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.claude/rules/RULES.md
.claude/rules/unity-architecture.md
.claude/rules/no-magic-balance-values.md
.claude/rules/gameplay-design-patterns.md
.claude/rules/code-minimalism-ladder.md
.claude/rules/id-stability.md
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/system-reuse-audit/SKILL.md
.claude/skills/monobehaviour-decomposition/SKILL.md
.claude/skills/enemy-ai-authoring/SKILL.md
.claude/skills/combat-data-wiring/SKILL.md
```

## 9. Estado atual do repo (Phase 0 — auditado nesta sessão via Grep real)

Snapshot completo (2026-07-13/14, branch `dev`):

```text
CSharpFiles=1632
RuntimeModuleEdges=241
MutualModulePairs=25
UsingOnlyModuleEdges=213
UsingOnlyMutualModulePairs=17
```

Os 5 pares desta spec, com direção real confirmada nesta sessão:

### 9.1 `Combat|Core`

- Direção Combat→Core: 45 arquivos em `Assets/_Game/Scripts/Combat/**` têm `using CindarsHope.Core` (lista completa no log de Grep desta sessão — cobre praticamente todo o domínio: `PlayerAttackController*.cs`, `PlayerDamageReceiver.cs`, `Weapon/*.cs`, `Magic/*.cs`, `Data/*.cs`, `StatusEffect/*.cs`, `Feel/*.cs`, `EnemyHealth.cs`, `EnemyDataSO.cs`, etc.).
- Direção Core→Combat: 7 arquivos: `Core/Data/CombatRuntimeDatabasesRegistrySO.cs`, `Core/Events/StatusAndDamageEvents.cs`, `Core/Events/EndgameEvents.cs`, `Core/Data/StatusEffectDatabaseSO.cs`, `Core/Data/SpellDatabaseSO.cs`, `Core/Data/WeaponDatabaseSO.cs`, `Core/Events/PlayerAttackedEvent.cs`.
- Responsabilidade dominante: Core→Combat é majoritariamente **eventos** (`GameEventBus` DTOs) e **registries de database** — não lógica de dano/cooldown em si. Combat→Core é o consumo desses eventos/registries por quase todo o subsistema. Este é provavelmente o par de MAIOR blast radius e MENOR risco de quebra de gameplay (eventos já são o canal correto pela rule `unity-architecture.md` §2) — candidato a "dependência esperada", não necessariamente algo a "quebrar".

### 9.2 `Combat|Enemy`

- Direção Combat→Enemy: 5 arquivos: `PlayerAttackController.Attacks.cs`, `PlayerDamageReceiver.cs`, `EnemyHealth.cs`, `EnemyPostureState.cs`, `StatusEffect/EnemyStatusRuntimeTicker.cs`.
- Direção Enemy→Combat: 21 arquivos: `BestiaryManager.cs`, `EnemyBrain.cs`, `EnemyDebugTelemetry.cs`, `EnemyBrainConfigurationPolicy.cs`, `EnemyBrainTuningResolver.cs`, `EnemyMovementStrategyRegistry.cs`, `EnemyActionExecutionStrategyRegistry.cs`, `EnemyMovementExecutor.cs`, `EnemyActionRunner.cs`, `EnemyActionSelectionStrategy.cs`, `EnemyHazardZoneRunner.cs`, `EnemyDecisionCore.cs`, `EnemyConflictHandler.cs`, `EnemyEvasionDecision.cs`, `Speech/CreatureChatterController.cs`, `EnemyVolatileExplosionRunner.cs`, `EnemyMoveLogic.cs`, `EnemyThreatState.cs`, `EnemySpawnResolver.cs`, `EnemySpawnProfileSO.cs`, `EnemyTelegraphController.cs`.
- Responsabilidade dominante: Enemy→Combat é majoritariamente **IA** (AI brain, movement, action selection, ameaça) consumindo dados de Combat (`EnemyDataSO`, `EnemyActionSO`, profiles). Combat→Enemy é dano/posture (`EnemyHealth`, `EnemyPostureState`) consumido pelo lado de ataque do player. Par de maior volume (21 arquivos de um lado) — candidato a **não** ser o primeiro par atacado.

### 9.3 `Combat|Inventory`

- Direção Combat→Inventory: 1 arquivo: `Assets/_Game/Scripts/Inventory/InventoryManager.cs` (nota: Grep foi rodado nas duas direções sob os respectivos diretórios; `Combat→Inventory` real fica em 9 arquivos de Combat que referenciam `CindarsHope.Inventory`: `PlayerAttackController.Attacks.cs`, `PlayerAttackController.cs`, `CombatActionContext.cs`, `BowArrowAttackService.cs`, `SpellCastService.cs`, `EquippedItemResolver.cs`, `PlayerAttackCore.cs`, `EnemyDropSpawner.cs`, `ArrowBallisticsResolver.cs`).
- Direção Inventory→Combat: `InventoryManager.cs` referencia `CindarsHope.Combat` (achado nesta sessão — confirmar linha exata na Fase 0 de execução).
- Responsabilidade dominante: Combat→Inventory é **resolução de item equipado** (`EquippedItemResolver`, arrow ballistics/ammo) e **drop de loot** (`EnemyDropSpawner`). Menor volume que Enemy — bom candidato a par pequeno e bem definido.

### 9.4 `Combat|Player`

- Direção Combat→Player: 4 arquivos: `PlayerAttackController.Attacks.cs`, `PlayerDamageReceiver.cs`, `PlayerAttackController.cs`, `PlayerCombatStatsProvider.cs`.
- Direção Player→Combat: 5 arquivos: `Player/Death/PlayerDeathController.cs`, `Player/PlayerCombatController.cs`, `Player/PlayerVitalsApplier.cs`, `Player/Movement/PlayerSprintController.cs`, `Player/Progression/PlayerProgressionRules.cs`.
- Responsabilidade dominante: **dano recebido pelo player, morte, stats derivados de combate, e vitals** — este é o par mais gameplay-crítico e o de menor volume (9 arquivos totais) entre os 5. Bom candidato a primeiro par SE houver cobertura de teste; caso contrário, o mais perigoso para tentar sem rede de segurança.

### 9.5 `Combat|Skills`

- Direção Combat→Skills: 4 arquivos: `PlayerAttackController.Attacks.cs`, `PlayerDamageReceiver.cs`, `PlayerAttackController.cs`, `PlayerCombatStatsProvider.cs` — mesmos arquivos do lado Combat→Player, reforçando que `PlayerAttackController`/`PlayerDamageReceiver`/`PlayerCombatStatsProvider` são hubs de múltiplas responsabilidades.
- Direção Skills→Combat: 7 arquivos: `SkillActionExecutor.cs`, `Runtime/Effects/ActiveSkillExecutorCatalog.cs`, `Runtime/Effects/SlowFieldSkillEffectExecutor.cs`, `Runtime/Effects/MeleeStrikeSkillEffectExecutor.cs`, `Runtime/Effects/SelfRestoreSkillEffectExecutor.cs`, `Runtime/Effects/ProjectileSkillEffectExecutor.cs`, `SkillActionSO.cs`.
- Responsabilidade dominante: **execução de active skills que causam dano/efeito de combate** (melee strike, projectile, slow field). `SkillPassiveModifier` (ver spec irmã de Player) também é consumido aqui via `PlayerAttackController.cs:145` e `PlayerDamageReceiver.cs:159` — quebrar este par sem coordenar com a spec de Player|Skills pode reintroduzir o mesmo tipo em dois lugares.

### 9.6 Achado transversal: hub files

`PlayerAttackController.cs` (+ `.Attacks.cs`), `PlayerDamageReceiver.cs` e `PlayerCombatStatsProvider.cs` aparecem em 4 dos 5 pares mapeados (`Combat|Enemy`, `Combat|Inventory`, `Combat|Player`, `Combat|Skills`). Isso não é coincidência de escopo — são os arquivos de maior responsabilidade concentrada em Combat. Nenhuma decomposição destes arquivos é escopo desta spec (ver skill `monobehaviour-decomposition` para uma spec futura dedicada, se o volume justificar), mas o execution report deve citar este achado como contexto para qualquer sub-slice futura.

### 9.7 O que NÃO existe / não deve ser recriado

- Não existe `.asmdef` separando estes módulos fisicamente — fora de escopo.
- Não existe hoje um relatório de cobertura de teste EditMode por responsabilidade de Combat (dano/cooldown/target/ameaça/skill/drop) — a Fase 0 de execução desta spec deve gerar esse inventário (quais `Assets/_Game/Tests/EditMode/Combat/**` já existem e o que cobrem), não assumir que existe.

## 10. User stories / engineering stories

```text
Como mantenedor de arquitetura, quero um mapa de dependência de Combat separado por responsabilidade (dano/cooldown/target/ameaça/skill/drop), para não tratar Combat como um bloco monolítico ao planejar redução de acoplamento.
Como agente de uma sub-slice futura, quero saber se cobertura de teste existe antes de tentar refatorar dano ou cooldown.
Como sistema de gameplay, quero que dano, cooldown, alcance, drops e IA permaneçam idênticos após qualquer mudança desta spec.
```

## 11. Escopo

Inclui:
- Fase 0: confirmar/atualizar (Grep real) a direção dos 5 pares.
- Fase 0: inventariar a cobertura de teste EditMode existente em `Assets/_Game/Tests/EditMode/Combat/**` (e pastas correlatas de Enemy/Skills/Player se tocarem combate) e classificar por responsabilidade (dano, cooldown, target, ameaça, skill/passives, drop/reward).
- Fase 1: decisão explícita do primeiro par seguro a atacar, priorizando o de menor blast radius com cobertura de teste existente (candidato inicial sugerido nesta spec: `Combat|Inventory`, por ter o menor volume de arquivos entre os pares não-Core — decisão final é da Fase 1 de execução, não fixada aqui).
- Fase 2: se a cobertura para o par escolhido for insuficiente, a spec entrega como PRIMEIRA recomendação uma spec derivada de COBERTURA (nome sugerido: `spec_combat_characterization_coverage_v1.md`), não uma spec de refactor.
- Fase 3 (condicional — só se cobertura já existir): executar a quebra mínima do par escolhido, sem alterar dano, cooldown, alcance, drops ou IA.
- Rodar snapshot antes/depois.

Fora:
- Quebrar mais de 1 par nesta spec.
- Decompor `PlayerAttackController`, `PlayerDamageReceiver` ou `PlayerCombatStatsProvider` (achado 9.6 é só contexto, não escopo).
- Qualquer mudança de dano, cooldown, alcance, drops ou IA de inimigo.
- Coordenar a movimentação de `SkillPassiveModifier` — isso pertence à spec irmã de Player|Skills; se `Combat|Skills` for o par escolhido e envolver esse tipo, a Fase 1 deve declarar a dependência cruzada e considerar não escolher esse par nesta rodada.

## 12. Fora de escopo

```text
Não inclui: quebrar todos os 5 pares nesta spec; decompor PlayerAttackController/PlayerDamageReceiver/PlayerCombatStatsProvider; alterar dano, cooldown, alcance, drops, IA; criar assembly definitions físicas; balanceamento.
```

## 13. Regras de não duplicação

```text
Não criar um segundo tipo para SkillPassiveModifier "temporariamente" — se o par Combat|Skills for escolhido e esbarrar nesse tipo, coordenar com a spec de Player|Skills em vez de duplicar.
Não criar um novo padrão de "port" de combate sem confirmar (system-reuse-audit) que os ports existentes (rule unity-architecture.md §6) não cobrem o caso.
Não criar uma segunda suite de testes de caracterização paralela se Assets/_Game/Tests/EditMode/Combat/** já cobrir a responsabilidade — estender, não duplicar.
```

## 14. Critérios de aceite

### 14.1 Mapa de dependência por direção

- Cada um dos 5 pares tem, no execution report, a lista real de arquivos de cada direção (Grep re-executado, não copiado cegamente da seção 9).
- Evidência esperada: bloco de Grep no execution report.

### 14.2 Mapa por responsabilidade

- Cada par tem seus arquivos classificados por responsabilidade dominante (dano, cooldown, target, ameaça, skill/passives, drop/reward) — pelo menos uma frase de responsabilidade por par, como nas subseções 9.1–9.5.
- Evidência esperada: seção dedicada no execution report.

### 14.3 Decisão do primeiro par seguro

- O execution report declara explicitamente qual par é o candidato e por quê, considerando blast radius (volume de arquivos) e presença/ausência de cobertura de teste.
- Evidência esperada: seção dedicada, referenciando 14.1/14.2.

### 14.4 Testes de caracterização antes de refatorar

- Se cobertura para o par escolhido não existir, o execution report declara explicitamente que a primeira spec derivada recomendada é de COBERTURA, e a Fase 3 (refactor) NÃO é executada nesta spec — isso é um resultado válido, não uma falha.
- Se cobertura existir e a Fase 3 rodar, o teste deve passar antes e depois da mudança.
- Evidência esperada: inventário de testes existentes + resultado do Test Runner se a Fase 3 rodou.

### 14.5 Sem mudança de dano, cooldown, alcance, drops ou IA

- Se a Fase 3 rodou, nenhuma constante de balance (dano, cooldown, alcance, drop rate) foi alterada — só a localização/namespace de código.
- Evidência esperada: diff mostra apenas mudança estrutural (using/namespace/localização de arquivo), nunca valor numérico.

### 14.6 Snapshot antes/depois sem par novo

- `MutualModulePairs` não aumenta; se a Fase 3 resolveu o par escolhido, ele sai da lista `MUTUAL|...` (ou o report documenta por que não saiu).
- Evidência esperada: dois blocos de output do snapshot.

## Aceite mínimo (verbatim)

```text
Lista das dependências por direção; decisão explícita do primeiro par seguro a atacar; testes de caracterização antes de refatorar; sem mudança de dano, cooldown, alcance, drops ou IA.
```

---

# /speckit.plan

## 15. Arquitetura alvo

```text
docs/validation/
  spec_arch_combat_boundary_residual_v1_execution_report.md   (mapa por direção + por responsabilidade + decisão + evidência)

Assets/_Game/Tests/EditMode/Combat/
  <NovoTesteDeCaracterizacao>.cs   (somente se Fase 3 executar; nome definido na Fase 2)

Assets/_Game/Scripts/<Combat|Core|Enemy|Inventory|Player|Skills>/
  <arquivo movido/ajustado>.cs     (somente se Fase 3 executar; escopo mínimo, um único tipo puro ou uma única interface de resolução)
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
Se a Fase 3 mover um tipo puro, o contrato de dados público não muda — só namespace/pasta.

### 16.2 Runtime contracts
Assinaturas públicas de `PlayerAttackController`, `PlayerDamageReceiver`, `PlayerCombatStatsProvider`, `EnemyHealth`, `EnemyBrain`, `InventoryManager`, `SkillActionExecutor` permanecem inalteradas nesta spec.

### 16.3 Event contracts
N/A por padrão — `Combat|Core` já é majoritariamente comunicação via `GameEventBus` (ver 9.1); esta spec não adiciona nem altera evento algum.

### 16.4 Save contracts
N/A. Se a Fase 3 tocar em qualquer tipo referenciado por DTO de save (ex. estado de equipamento/durabilidade, `EnemyDataSO` referenciado por save de bestiary), a mudança fica bloqueada e documentada como risco.

### 16.5 UI contracts
N/A — floating damage number display e HUD suppression (`Combat/FloatingDamageNumberDisplayer.cs`, `Combat/Feel/HudSuppressionBroadcaster.cs`) não são tocados por esta spec.

## 17. Sistemas afetados

```text
Combat (dano, weapon, spell, status effect, projectile, feel)
Core (eventos, registries de database)
Enemy (AI brain, movement, ameaça)
Inventory (item equipado, drop)
Player (dano recebido, morte, stats, progression)
Skills (active skill execution, passive modifiers)
```

## 18. Arquivos permitidos

```text
docs/validation/**
Assets/_Game/Tests/EditMode/Combat/**
Assets/_Game/Tests/EditMode/Enemy/**
Assets/_Game/Tests/EditMode/Inventory/**
Assets/_Game/Tests/EditMode/Player/**
Assets/_Game/Tests/EditMode/Skills/**
Assets/_Game/Scripts/Core/**       (somente os 7 arquivos citados na seção 9.1, leitura/ajuste mínimo, só se Fase 3 escolher Combat|Core)
Assets/_Game/Scripts/Combat/**     (somente os arquivos citados na seção 9, e só se Fase 3 executar)
Assets/_Game/Scripts/Enemy/**      (idem)
Assets/_Game/Scripts/Inventory/**  (idem)
Assets/_Game/Scripts/Player/**     (idem)
Assets/_Game/Scripts/Skills/**     (idem — exceto SkillNodeDataSO.cs/SkillPassiveModifier.cs/SkillEnums.cs, ver arquivos proibidos)
```

## 19. Arquivos proibidos

```text
Assets/_Game/Scripts/Skills/SkillNodeDataSO.cs        (serialização — coordenar com spec irmã de Player, não editar aqui)
Assets/_Game/Scripts/Skills/SkillPassiveModifier.cs   (idem)
Assets/_Game/Scripts/Skills/SkillEnums.cs             (idem)
Assets/_Game/Scripts/Combat/PlayerAttackController.cs, PlayerAttackController.Attacks.cs, PlayerDamageReceiver.cs, PlayerCombatStatsProvider.cs — LEITURA livre; EDIÇÃO só permitida se Fase 3 escolher explicitamente um desses arquivos como parte mínima do par selecionado, e a mudança for puramente de namespace/localização (nunca lógica de dano/cooldown)
Assets/_Game/Scripts/Combat/EnemyDataSO.cs, Assets/_Game/Scripts/Combat/Weapon/WeaponDataSO.cs, Assets/_Game/Scripts/Combat/Magic/SpellDataSO.cs, Assets/_Game/Scripts/Combat/StatusEffect/StatusEffectSO.cs (balance/dados de referência — não editar)
Assets/**/*.unity, *.prefab, *.asset salvo leitura/inspeção
Assets/_Game/Scripts/Save/**
docs_old/**
.specs/SPEC_INDEX.md, .specs/SPEC_EXECUTION_ORDER.md
```

## 20. Estratégia de implementação

```md
### Fase 0 — Re-auditar (Grep real) a direção dos 5 pares; inventariar cobertura de teste EditMode existente por responsabilidade
### Fase 1 — Decidir o primeiro par seguro (blast radius + cobertura de teste)
### Fase 2 — Se cobertura insuficiente: declarar spec derivada de COBERTURA como próximo passo; parar aqui
### Fase 3 — (condicional, só com cobertura confirmada) Executar a quebra mínima do par escolhido
### Fase 4 — Snapshot antes/depois + build + relatório
```

## 21. Ordem de execucao (ordem segura)

```text
1. Rodar Get-ModularizationDependencySnapshot.ps1 e salvar output "antes".
2. Re-Grep os 5 pares; atualizar a seção 9 se o código mudou.
3. Inventariar Assets/_Game/Tests/EditMode/Combat/** (e correlatos) por responsabilidade.
4. Decidir o primeiro par seguro; se cobertura ausente, declarar spec de cobertura como próximo passo e parar (Fase 3 não roda).
5. Se cobertura existir: mover o tipo/interface mínimo do par escolhido, mantendo assinaturas públicas.
6. Rodar dotnet build (2 csproj) + EditMode tests (filtro Combat) + snapshot "depois".
7. Registrar execution report com mapa por direção, mapa por responsabilidade, decisão, evidência de teste, diff de snapshot.
```

## 22. Paralelização

```md
## Paralelização

- Parallelizable: CONDITIONAL
- Parallel group: arch_boundary_residual
- Can run with: spec_arch_player_gameplay_boundary_residual_v1, SE nenhuma das duas tocar Assets/_Game/Scripts/Player/PlayerAttackController*.cs, PlayerDamageReceiver.cs, PlayerCombatController.cs, PlayerVitalsApplier.cs, DerivedStatsCalculator.cs no mesmo lote
- Must not run with: qualquer spec que altere EnemyBrain.cs, EnemyDataSO.cs, WeaponDataSO.cs, SpellDataSO.cs, StatusEffectSO.cs, ou dano/cooldown/alcance/drop
- Shared files/systems that require lock: Assets/_Game/Scripts/Combat/**, Assets/_Game/Scripts/Enemy/** (lock local, arquivo a arquivo)
- Reason: Fase 0/1/2 são leitura+docs, sem risco de conflito; Fase 3 é condicional e escopo de um único par — lock nomeado no momento da execução real
```

## 23. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO (se Fase 3 tocar tipo serializado, mudança é BLOQUEADA — ver seção 19)
Does this persist Unity references? N/A
```

## 24. Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO
```

## 25. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES se Fase 3 executar (combate é gameplay-crítico — cenário humano deve confirmar dano/cooldown/drop/IA idênticos)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos

```text
Risco: tratar Combat como bloco monolítico e tentar refatorar múltiplos pares de uma vez, dado que PlayerAttackController/PlayerDamageReceiver/PlayerCombatStatsProvider aparecem em 4 dos 5 pares.
Mitigação: escopo desta spec é 1 par por rodada; achado 9.6 documentado explicitamente para specs futuras.

Risco: cobertura de teste de combate (dano/cooldown/target) pode não existir hoje — refatorar sem rede de segurança em sistema gameplay-crítico.
Mitigação: Fase 2 obrigatória — se cobertura ausente, a spec entrega uma spec de COBERTURA como próximo passo em vez de refactor.

Risco: par Combat|Skills esbarra em SkillPassiveModifier, que também é escopo da spec irmã de Player|Skills — risco de trabalho duplicado ou conflitante.
Mitigação: Fase 1 deve checar essa dependência cruzada antes de escolher Combat|Skills como primeiro par; se escolhido, coordenar com a spec irmã antes de mover o tipo.
```

## 27. Rollback

```text
Reverter a movimentação do tipo/interface (Fase 3) para a localização original.
Manter o mapa de dependência e a decisão documentados mesmo se a Fase 3 for revertida (valor de registro histórico).
Não reverter testes de caracterização novos.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Rodar snapshot "antes".
- [ ] T002 — Re-Grep e confirmar/atualizar a direção real dos 5 pares.
- [ ] T003 — Classificar cada par por responsabilidade (dano/cooldown/target/ameaça/skill/drop).
- [ ] T004 — Inventariar cobertura de teste EditMode existente por responsabilidade.
- [ ] T005 — Decidir o primeiro par seguro; se cobertura ausente, declarar spec derivada de COBERTURA e parar.
- [ ] T006 — (condicional) Executar a quebra mínima do par escolhido.
- [ ] T007 — Rodar snapshot "depois", dotnet build (2 csproj), EditMode tests (filtro Combat).
- [ ] T008 — Gerar execution report com os dois mapas, decisão, evidência, diff de snapshot.
```

## 29. Validações obrigatórias

```powershell
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
& .\tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1
& .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Combat"
```

Se algum comando não puder rodar, o report deve registrar `NOT RUN` com motivo e risco residual — nunca inferir sucesso.

## 30. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: CONDITIONAL (só se Fase 3 executar)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES se Fase 3 executar (combate é gameplay-crítico)
- Requires regression test: YES se Fase 3 executar
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: mapa por direção + mapa por responsabilidade + decisão documentada, no mínimo. Se cobertura ausente para o par escolhido: spec derivada de COBERTURA declarada é o entregável, sem refactor. Se Fase 3 rodou: dotnet build PASS (2 csproj) + EditMode tests PASS + snapshot antes/depois sem par novo. Combat é gameplay-crítico: sem cenário de Play Mode humano documentado, status máximo é BUILD_VALIDATED, nunca ACCEPTED.
```

## 31. Definition of Done

```text
Mapa de dependência dos 5 pares verificado por Grep real nesta execução.
Mapa por responsabilidade (dano/cooldown/target/ameaça/skill/drop) documentado.
Decisão do primeiro par seguro documentada, considerando blast radius e cobertura de teste.
Se cobertura ausente: spec derivada de COBERTURA declarada como próximo passo obrigatório (não refactor).
Se Fase 3 executou: build (2 csproj) exit 0, EditMode tests PASS, snapshot sem par novo.
Execution report criado em docs/validation/.
Nenhuma spec afirma "modularização do Combat concluída".
```

## 32. Anti-regressão

```text
Não alterar dano, cooldown, alcance, drops ou IA de inimigo.
Não decompor PlayerAttackController/PlayerDamageReceiver/PlayerCombatStatsProvider nesta spec.
Não mover SkillPassiveModifier sem coordenar com a spec irmã de Player|Skills.
Não usar GameObject.Find/FindObjectOfType em qualquer código novo.
Não comunicar entre sistemas de gameplay fora do GameEventBus.
Não declarar "modularização do Combat concluída" enquanto MutualModulePairs > 0.
```

## 33. Notas para execução posterior

```text
Esta spec sugere Combat|Inventory como candidato inicial ao primeiro par (menor volume, achado 9.3), mas a decisão final é da Fase 1 de execução — não fixada aqui.
O achado 9.6 (hub files PlayerAttackController/PlayerDamageReceiver/PlayerCombatStatsProvider concentrando 4 dos 5 pares) deve ser citado em qualquer sub-slice futura como contexto de risco.
Se o par Combat|Skills for escolhido em uma rodada futura, coordenar explicitamente com spec_arch_player_gameplay_boundary_residual_v1 antes de tocar SkillPassiveModifier.
```
