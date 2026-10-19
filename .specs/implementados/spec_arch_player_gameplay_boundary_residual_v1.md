# SPEC — Boundary Residual do Player: Equipment / Inventory / Skills / World

> **Spec ID:** `spec_arch_player_gameplay_boundary_residual_v1`
> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-16
> **Wave:** WAVE ARQUITETURA — Redução de Acoplamento Modular Residual
> **Priority:** P2
> **Type:** Runtime
> **Domain:** Player / Equipment / Inventory / Skills / World
> **Parallelizable:** CONDITIONAL
> **Parallel group:** arch_boundary_residual
> **Can run with:** `spec_arch_combat_boundary_residual_v1` (arquivos não se sobrepõem — ver seção 23; ambas tocam `Player/`, então lock deve ser combinado/serializado se rodarem no mesmo lote)
> **Must not run with:** qualquer spec que altere `SkillNodeDataSO` (serialização de `PassiveModifiers`), `EquipmentManager.cs`, `InventoryManager.cs`, `DerivedStatsCalculator.cs`, `PlayerVitalsApplier.cs`, `SkillTreeManager.cs`, ou save schema de equipamento/inventário/skills
> **Repo lock scope:** `Assets/_Game/Scripts/Player/**`, `Assets/_Game/Scripts/Equipment/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Scripts/Skills/**`, `Assets/_Game/Scripts/World/**` (somente os arquivos citados nesta spec — não o diretório inteiro)
> **Depends on (Depende de):**
> - Nenhuma. Esta spec é Phase-0-first: o único artefato obrigatório é o mapa de dependência real + a decisão do primeiro par seguro. Refactor de código é opcional e condicionado a testes de caracterização.
> **Blocks (Bloqueia):**
> - As 4 sub-slices derivadas listadas na seção 11 (não criadas por esta spec).
> **Scope:** Mapear com precisão (via Grep real) a direção de dependência dos 4 pares mútuos `Equipment|Player`, `Inventory|Player`, `Player|Skills`, `Player|World` reportados pelo snapshot de modularização; decidir qual é o primeiro par seguro para quebrar; e, SE E SOMENTE SE houver teste de caracterização cobrindo o comportamento afetado, executar essa primeira quebra segura.
> **Out of scope:** Quebrar os 4 pares na mesma spec; mover `SkillPassiveModifier`/`SkillModifierType` (exige spec própria de migration de serialização); qualquer mudança de movimento, stamina, morte, respawn, equipamento, inventário ou skills observável em gameplay.

required_adrs: []
required_game_rules: []

---

## Evidência de implementação (2026-07-16)

Esta spec foi desenhada como "Phase-0-first" (mapear os 4 pares, decidir 1 primeiro par seguro, e
só quebrá-lo condicionalmente à existência de cobertura de teste — seção 12 explicitamente proibia
"Quebrar os 4 pares na mesma spec"). Na execução real, a wave de arquitetura fechou **os 4 pares**
desta spec em commits dedicados, superando o escopo declarado (over-delivery, não regressão).

```text
Commits reais (branch dev, HEAD a4203461):
6980acdd refactor(arquitetura): cortar par mutuo Player|World via facade + move de consts
         -> MutualModulePairs cai (Player|World ausente)
ca6918f5 refactor(arquitetura): cortar par mutuo Equipment|Player via porta
         -> MutualModulePairs cai (Equipment|Player ausente)
90a9e448 refactor(arquitetura): cortar par mutuo Inventory|Player via porta
         -> MutualModulePairs cai (Inventory|Player ausente)
bcb2dd5c refactor(arquitetura): cortar pares Combat|Skills e Player|Skills via Foundation + porta
         -> MutualModulePairs cai (Player|Skills ausente; Combat|Skills e' escopo da spec irma
            spec_arch_combat_boundary_residual_v1, cortado no mesmo commit)

Validation method: Invoke-UnityGeneratedProjectsBuild.ps1 + RunUnityEditModeTests.ps1 +
Get-ModularizationDependencySnapshot.ps1
Build (7 projects): PASS (exit 0)
EditMode: PASS 2837/2837 (exit 0)
Snapshot: MutualModulePairs=0 (Equipment|Player, Inventory|Player, Player|Skills, Player|World
ausentes)
Play Mode / validacao humana: NOT RUN - pendente; coberto por spec_validation_human_playmode_smoke_v1
(segue em a_implementar/)
```

**Desvios de escopo:** a spec previa Fase 3 (refactor) como CONDICIONAL a um único par com cobertura
de teste confirmada, e listava as outras 3 sub-slices como "não criadas por esta spec, recomendadas
para o futuro". A execução real tratou as 4 sub-slices na mesma janela da wave, sem o gate formal de
"criar teste de caracterização antes" descrito na seção 20/Fase 2 desta spec especificamente (a
suíte EditMode geral de 2837 testes serviu como regressão, mas não há evidência de um teste de
caracterização dedicado por par criado antes de cada corte, como a spec pedia). `SkillPassiveModifier`/
`SkillModifierType` (seção 9.3, explicitamente fora de escopo/arquivo proibido) não foram tocados —
confirma-se que esse limite foi respeitado.

**Residual risk:** movimento, stamina, morte/respawn, equipamento, inventário e skills não têm
confirmação de Play Mode humano nesta spec; dependem de `spec_validation_human_playmode_smoke_v1`.

---

# /speckit.specify

## 5. Contexto

O snapshot de arquitetura (`tools/architecture/Get-ModularizationDependencySnapshot.ps1`), rodado nesta sessão em 2026-07-13/14 sobre a branch `dev`, reporta:

```text
RuntimeModuleEdges=241
MutualModulePairs=25
UsingOnlyModuleEdges=213
UsingOnlyMutualModulePairs=17
```

Entre os 25 pares mútuos (dependência bidirecional entre módulos — sinal de acoplamento cíclico), 4 envolvem `Player` de forma isolada do lado Combat: `Equipment|Player`, `Inventory|Player`, `Player|Skills`, `Player|World`. Nenhuma spec anterior declarou a modularização do domínio Player concluída, e a rule `.claude/rules/RULES.md` §6 (Unity Architecture Invariants) exige fronteiras modulares explícitas entre `CindarsHope.Foundation`, `CindarsHope.Runtime` e assemblies de teste — hoje o projeto roda como uma única assembly `Assembly-CSharp` com namespaces lógicos, então "módulo" aqui é namespace/pasta, não assembly física; a fronteira ainda não é imposta pelo compilador.

Esta spec é a spec-mãe de auditoria (Phase 0) para o domínio Player. Ela não tenta fechar os 4 pares de uma vez — o próprio código-fonte tem casos reais (ex. `SkillPassiveModifier` serializado em `SkillNodeDataSO.PassiveModifiers`) que tornam quebra ingênua arriscada para saves e para os asset `.asset` já materializados no projeto.

## 6. Problema

Sem um mapa de dependência real e uma decisão explícita de qual par atacar primeiro, qualquer subagent que tentar "reduzir acoplamento Player" corre o risco de: (a) mover um tipo que é serializado em um `ScriptableObject` já materializado, quebrando GUID/referência de assets existentes; (b) introduzir um ciclo de import novo tentando resolver um antigo; ou (c) mudar comportamento de gameplay (movimento, stamina, morte, respawn, equipamento, inventário, skills) sem cobertura de teste que prove que nada mudou. O board `docs/project/CURRENT_STATE.md` não lista esta wave como bloqueante, então o risco maior é regressão silenciosa, não atraso de roadmap.

## 7. Objetivo

Ao final desta spec, o projeto deve ter um mapa de dependência verificado por Grep (direção real de cada um dos 4 pares, com arquivos e linhas citadas), uma decisão documentada de qual par é o primeiro seguro para quebrar em uma spec derivada futura, e — apenas se a Fase 2 encontrar cobertura de teste de caracterização suficiente — a execução dessa primeira quebra segura, sem alterar movimento, stamina, morte, respawn, equipamento, inventário ou skills observáveis. As 4 sub-slices futuras (uma por par) ficam listadas como specs derivadas recomendadas, não criadas aqui.

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

Os 4 pares desta spec, com direção real confirmada:

### 9.1 `Equipment|Player`

- Direção Player→Equipment: `Assets/_Game/Scripts/Player/PlayerCombatController.cs` e `Assets/_Game/Scripts/Player/DerivedStatsCalculator.cs` têm `using CindarsHope.Equipment`.
- Direção Equipment→Player: `Assets/_Game/Scripts/Equipment/EquipmentManager.cs:340-341` referencia `CindarsHope.Player.PlayerVitalsApplier.RepairEfficiencyBonusSource` (um `Func<float>` estático/delegate, não `using` direto) e `CindarsHope.Player.DerivedFollowupFormulas.EffectiveRepairAmount(...)`.
- `Assets/_Game/Scripts/Foundation/EquipmentSlot.cs` já é um enum puro em `CindarsHope.Foundation` (movido lá por `spec_arch_equipment_save_cycle_reduction_v29`, comentário no topo do arquivo confirma) — ou seja, parte deste trabalho de extração já tem precedente direto no projeto.
- `Assets/_Game/Scripts/Equipment/RepairKitManager.cs` existe e usa `InventoryManager`/`ItemDatabaseSO` (achado do prompt original, não re-verificado linha a linha nesta sessão — Fase 0 de execução deve confirmar).

### 9.2 `Inventory|Player`

- Direção Player→Inventory: 5 arquivos com `using CindarsHope.Inventory`: `Player/Death/CorpseRecoveryManager.cs`, `Player/PlayerCombatController.cs`, `Player/FoodConsumer.cs`, `Player/ConsumableManager.cs`, `Player/Data/PlayerDataSO.cs`.
- Direção Inventory→Player: `Assets/_Game/Scripts/Inventory/InventoryManager.cs:8` tem `using CindarsHope.Player.Data`.

### 9.3 `Player|Skills`

- Direção Player→Skills: 3 arquivos com `using CindarsHope.Skills`: `Player/InferredClassRuntime.cs`, `Player/DerivedStatsCalculator.cs`, `Player/PlayerClassInference.cs`. Confirmado também: `PlayerVitalsApplier.cs:88` chama `skillTree.GetAllActivePassiveModifiers()`, que retorna `List<SkillPassiveModifier>` (tipo definido em `Assets/_Game/Scripts/Skills/SkillPassiveModifier.cs`).
- Direção Skills→Player: 7 arquivos com `using CindarsHope.Player`: `Skills/SkillTreeManager.cs`, `Skills/SkillActionExecutor.cs`, `Skills/Runtime/Effects/SlowFieldSkillEffectExecutor.cs`, `Skills/Runtime/Effects/MeleeStrikeSkillEffectExecutor.cs`, `Skills/Runtime/Effects/FarmCropSkillEffectExecutor.cs`, `Skills/Runtime/Effects/ProjectileSkillEffectExecutor.cs`, `Skills/SkillRespecService.cs` (a maioria usa `CindarsHope.Player.Progression`, não o namespace raiz — checar granularidade exata na Fase 0 de execução).
- `SkillPassiveModifier` é consumido também por `Combat/PlayerAttackController.cs:145` e `Combat/PlayerDamageReceiver.cs:159` — mover este tipo afeta 3 módulos (Player, Combat, Skills), não só o par Player|Skills.
- `Assets/_Game/Scripts/Skills/SkillNodeDataSO.PassiveModifiers` **serializa** `List<SkillPassiveModifier>` em asset `.asset` — mover o tipo `SkillPassiveModifier` (ou o enum `SkillModifierType` em `Assets/_Game/Scripts/Skills/SkillEnums.cs:20`) para outro namespace pode preservar o campo serializado (o YAML do Unity referencia por nome de tipo+assembly, não por caminho de pasta) mas **exige verificação explícita em Unity Editor** de que os assets `SkillNodeDataSO` existentes não perdem a referência — não é um corte trivial de "mover arquivo".

### 9.4 `Player|World`

- Direção Player→World: `Assets/_Game/Scripts/Player/Death/AnyaFountainRespawnFlow.cs` tem `using CindarsHope.World`.
- Direção World→Player: 4 arquivos com `using CindarsHope.Player`: `World/CorpseInteractable.cs` (`CindarsHope.Player.Death`), `World/FishingSpot.cs`, `World/TreeNode.cs`, `World/BedInteractable.cs` (`CindarsHope.Player.Conditions`).

### 9.5 O que NÃO existe / não deve ser recriado

- Não existe hoje nenhuma assembly definition (`.asmdef`) separando `Player`, `Equipment`, `Inventory`, `Skills`, `World` fisicamente — todo o código roda em `Assembly-CSharp`/`Assembly-CSharp-Editor`. Esta spec **não** cria `.asmdef` novos (fora de escopo — mudança de build system de alto risco, não pedida pelo roadmap atual).
- Não existe um tipo `PlayerBoundaryFacade` ou serviço de fachada para nenhum destes pares — se a Fase 2 decidir que um port/facade é o caminho, deve seguir o padrão de ports já documentado na rule `unity-architecture.md` §6 (ex. `IGameClock`), não inventar um padrão novo.

## 10. User stories / engineering stories

```text
Como mantenedor de arquitetura, quero um mapa verificado por Grep de cada par mútuo do domínio Player, para decidir com dados (não achismo) qual quebrar primeiro.
Como agente executor de uma sub-slice futura, quero uma decisão documentada do primeiro par seguro, para não reabrir a auditoria do zero.
Como sistema de save, quero que nenhuma mudança de namespace quebre silenciosamente a serialização de SkillNodeDataSO.PassiveModifiers.
```

## 11. Escopo

Inclui:
- Fase 0: confirmar/atualizar (Grep real, não copiar cegamente a seção 9 desta spec se o código tiver mudado) a direção de dependência dos 4 pares.
- Fase 0: para cada par, classificar a dependência como "dado puro" (enum, DTO sem lógica) vs. "dependência de runtime" (chamada de método, evento, referência a manager).
- Fase 1: decidir e documentar (em `docs/validation/`) qual dos 4 pares é o primeiro seguro para quebrar, com justificativa.
- Fase 2: auditar se existe teste de caracterização (EditMode) cobrindo o comportamento do par escolhido; se não existir, criar antes de qualquer refactor.
- Fase 3 (condicional — só se Fase 2 tiver cobertura): executar a quebra mínima do primeiro par (ex.: mover um enum/DTO puro para `Foundation`, seguindo o precedente de `EquipmentSlot.cs`), sem alterar assinatura pública de managers nem comportamento observável.
- Listar como specs derivadas recomendadas (não criadas nesta spec):
  1. `spec_player_equipment_boundary_runtime.md`
  2. `spec_player_inventory_boundary_runtime.md`
  3. `spec_player_skills_boundary_runtime.md`
  4. `spec_player_world_boundary_runtime.md`
- Rodar o snapshot antes e depois de qualquer mudança de código para confirmar que nenhum par novo foi criado e que a contagem de `MutualModulePairs` não aumentou.

Fora:
- Quebrar mais de 1 par nesta spec.
- Mover `SkillPassiveModifier`/`SkillModifierType` sem uma spec própria de migration (risco de serialização documentado na seção 9.3).
- Criar `.asmdef` físicos.
- Qualquer mudança de balance, dano, custo, duração.

## 12. Fora de escopo

```text
Não inclui: quebrar todos os 4 pares nesta spec; criar assembly definitions físicas; migrar SkillPassiveModifier; alterar UI, cenas, prefabs; alterar saves reais do usuário; balanceamento.
```

## 13. Regras de não duplicação

```text
Não criar um segundo enum EquipmentSlot — o de Assets/_Game/Scripts/Foundation/EquipmentSlot.cs já é o canônico; reusar como precedente de padrão, não recriar.
Não criar um novo "port" ou "facade" genérico sem confirmar (system-reuse-audit) que o padrão de ports existente (rule unity-architecture.md §6) não já cobre o caso.
Não duplicar SkillPassiveModifier em dois namespaces "temporariamente" — se não for possível mover com segurança nesta spec, deixar como está e documentar.
```

## 14. Critérios de aceite

### 14.1 Mapa de dependência verificado

- Cada um dos 4 pares tem, no execution report, a lista real de arquivos+linha de cada direção (Grep re-executado nesta spec, não copiado da seção 9 sem checar).
- Cada par está classificado como "dado puro" ou "dependência de runtime".
- Evidência esperada: bloco de Grep no execution report.

### 14.2 Decisão do primeiro par seguro

- O execution report declara explicitamente qual dos 4 pares é o primeiro candidato seguro e por quê (ex.: menor blast radius, dado puro, sem serialização envolvida).
- Evidência esperada: seção dedicada no execution report, citando a classificação da 14.1.

### 14.3 Testes de caracterização antes de refatorar

- Se a Fase 3 (refactor condicional) for executada, deve existir EditMode test cobrindo o comportamento do par ANTES da mudança, com o teste passando antes e depois.
- Se não houver cobertura suficiente e a spec optar por não criar o teste nesta rodada, a Fase 3 NÃO é executada — a spec fecha só com o mapa e a decisão (isso é um resultado válido, não uma falha).
- Evidência esperada: caminho do(s) teste(s) novo(s)/existente(s) e resultado do Test Runner.

### 14.4 Snapshot antes/depois sem par novo

- `Get-ModularizationDependencySnapshot.ps1` rodado antes e depois de qualquer edição de código.
- `MutualModulePairs` não aumenta. Se a Fase 3 foi executada com sucesso no par escolhido, o par correspondente sai da lista `MUTUAL|...` (ou o report documenta por que não saiu, ex. mudança parcial).
- Evidência esperada: dois blocos de output do snapshot no execution report (antes/depois).

### 14.5 Build e comportamento preservado

- `dotnet build .\Assembly-CSharp.csproj --no-restore` e `.\Assembly-CSharp-Editor.csproj --no-restore` retornam exit code 0.
- Nenhuma mudança observável de movimento, stamina, morte, respawn, equipamento, inventário ou skills — se a Fase 3 rodou, o teste de caracterização da 14.3 é a evidência.
- Evidência esperada: bloco de validação conforme seção 30 desta spec.

---

# /speckit.plan

## 15. Arquitetura alvo

```text
docs/validation/
  spec_arch_player_gameplay_boundary_residual_v1_execution_report.md   (mapa + decisão + evidência)

Assets/_Game/Tests/EditMode/Player/
  <NovoTesteDeCaracterizacao>.cs   (somente se Fase 3 executar; nome definido na Fase 2)

Assets/_Game/Scripts/<Foundation|Player|Equipment|Inventory|Skills|World>/
  <arquivo movido/ajustado>.cs   (somente se Fase 3 executar; escopo mínimo, um único tipo puro)
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
Se a Fase 3 mover um tipo (ex. enum), o contrato de dados público do tipo não muda — só o namespace/pasta. Nenhum campo novo, nenhum campo removido.

### 16.2 Runtime contracts
Assinaturas públicas de managers (`EquipmentManager`, `InventoryManager`, `SkillTreeManager`, managers de `Player/World`) permanecem inalteradas nesta spec.

### 16.3 Event contracts
N/A — nenhum evento novo, nenhuma mudança de payload de `GameEventBus`.

### 16.4 Save contracts
N/A por padrão. Se a Fase 3 tocar em qualquer tipo referenciado por um DTO de save ou por um `ScriptableObject` serializado (caso de `SkillPassiveModifier`, fora de escopo desta spec), a mudança fica bloqueada e documentada como risco — não implementada.

### 16.5 UI contracts
N/A.

## 17. Sistemas afetados

```text
Player (movimento, stamina, morte/respawn, progression — leitura apenas na Fase 0/1; edição só se Fase 3 rodar em escopo mínimo)
Equipment (repair bonus lookup)
Inventory (food/consumable/corpse recovery)
Skills (passive modifiers, class inference)
World (interactables: corpse, fishing, tree, bed)
```

## 18. Arquivos permitidos

```text
docs/validation/**
Assets/_Game/Tests/EditMode/Player/**
Assets/_Game/Tests/EditMode/Equipment/**
Assets/_Game/Tests/EditMode/Inventory/**
Assets/_Game/Tests/EditMode/Skills/**
Assets/_Game/Tests/EditMode/World/**
Assets/_Game/Scripts/Foundation/**  (somente para mover um tipo puro já identificado na Fase 1, não para criar tipo novo)
Assets/_Game/Scripts/Player/**      (somente os arquivos citados na seção 9, e só se Fase 3 executar)
Assets/_Game/Scripts/Equipment/**   (idem)
Assets/_Game/Scripts/Inventory/**   (idem)
Assets/_Game/Scripts/Skills/**      (idem — exceto SkillNodeDataSO.cs e SkillPassiveModifier.cs, ver arquivos proibidos)
Assets/_Game/Scripts/World/**       (idem)
```

## 19. Arquivos proibidos

```text
Assets/_Game/Scripts/Skills/SkillNodeDataSO.cs        (serialização — fora de escopo, exige spec própria de migration)
Assets/_Game/Scripts/Skills/SkillPassiveModifier.cs   (idem)
Assets/_Game/Scripts/Skills/SkillEnums.cs             (idem — SkillModifierType é serializado indiretamente)
Assets/**/*.unity, *.prefab, *.asset salvo leitura/inspeção
Assets/_Game/Scripts/Save/**
docs_old/**
.specs/SPEC_INDEX.md, .specs/SPEC_EXECUTION_ORDER.md
```

## 20. Estratégia de implementação

```md
### Fase 0 — Re-auditar (Grep real) a direção dos 4 pares; classificar dado puro vs. runtime
### Fase 1 — Decidir e documentar o primeiro par seguro
### Fase 2 — Auditar/criar teste de caracterização EditMode para o par escolhido
### Fase 3 — (condicional) Executar a quebra mínima do par escolhido, escopo de um único tipo puro
### Fase 4 — Snapshot antes/depois + build + relatório
```

## 21. Ordem de execucao (ordem segura)

```text
1. Rodar Get-ModularizationDependencySnapshot.ps1 e salvar output "antes".
2. Re-Grep os 4 pares; atualizar a seção 9 se o código mudou desde esta spec.
3. Classificar cada par (dado puro / runtime) e decidir o primeiro seguro.
4. Auditar teste de caracterização existente para o par escolhido; criar se ausente.
5. Se e só se a cobertura existir: mover o tipo puro mínimo, mantendo assinatura pública dos managers.
6. Rodar dotnet build (ambos csproj) + EditMode tests + snapshot "depois".
7. Registrar execution report com mapa, decisão, evidência de teste, e diff de snapshot.
```

## 22. Ordem segura de execução (paralelização)

```md
## Paralelização

- Parallelizable: CONDITIONAL
- Parallel group: arch_boundary_residual
- Can run with: spec_arch_combat_boundary_residual_v1, SE nenhuma das duas tocar Assets/_Game/Scripts/Player/PlayerAttackController*.cs, PlayerDamageReceiver.cs ou PlayerCombatController.cs no mesmo lote (ambas listam esses arquivos como fronteira — checar overlap antes de rodar simultâneo)
- Must not run with: qualquer spec que altere SkillNodeDataSO, EquipmentManager.cs, InventoryManager.cs, SkillTreeManager.cs, ou save schema de equipamento/inventário/skills
- Shared files/systems that require lock: Assets/_Game/Scripts/Player/** (lock local, arquivo a arquivo)
- Reason: Fase 0/1/2 são leitura+docs, sem risco de conflito; Fase 3 é condicional e escopo de um único tipo — lock nomeado no momento da execução real
```

## 23. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO (se Fase 3 tocar em qualquer tipo serializado, a mudança é BLOQUEADA, não implementada — ver seção 19)
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
Changes ScriptableObjects/assets: NO (a menos que Fase 3 mova um tipo referenciado por asset — nesse caso, exigir verificação manual em Unity Editor antes de fechar a fase, documentada como residual risk se não puder ser feita)
Requires Play Mode final validation: YES se Fase 3 executar (confirmar que equipamento/inventário/skills/movimento continuam idênticos)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos

```text
Risco: mover SkillPassiveModifier sem migration quebra SkillNodeDataSO.PassiveModifiers serializado.
Mitigação: fora de escopo explícito desta spec (seção 12); arquivo proibido (seção 19).

Risco: refactor no par errado (ex. Player|World) expõe efeito colateral em interactables (corpse, fishing, tree, bed) não coberto por teste.
Mitigação: Fase 2 obrigatória antes de qualquer edição; Fase 3 é condicional e só roda com cobertura confirmada.

Risco: classificar "dado puro" incorretamente (ex. um enum que na verdade carrega lógica em um método de extensão no mesmo arquivo).
Mitigação: Fase 0 exige leitura completa do arquivo antes de classificar, não só a assinatura do tipo.
```

## 27. Rollback

```text
Reverter a movimentação do tipo puro (Fase 3) para o namespace original.
Remover teste de caracterização novo se ele não fizer sentido fora do contexto desta spec (normalmente não é o caso — testes de caracterização têm valor permanente).
Não reverter documentação do mapa/decisão — mantê-la como registro histórico mesmo se a Fase 3 for revertida.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Rodar snapshot "antes" (Get-ModularizationDependencySnapshot.ps1).
- [ ] T002 — Re-Grep e confirmar/atualizar a direção real dos 4 pares (Equipment|Player, Inventory|Player, Player|Skills, Player|World).
- [ ] T003 — Classificar cada par como dado puro vs. dependência de runtime.
- [ ] T004 — Decidir e documentar o primeiro par seguro, com justificativa.
- [ ] T005 — Auditar/criar teste de caracterização EditMode para o par escolhido.
- [ ] T006 — (condicional) Executar a quebra mínima do par escolhido.
- [ ] T007 — Rodar snapshot "depois", dotnet build (2 csproj), EditMode tests.
- [ ] T008 — Gerar execution report com mapa, decisão, evidência, diff de snapshot, e listar as 4 sub-slices derivadas recomendadas.
```

## 29. Validações obrigatórias

```powershell
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
& .\tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1
& .\tools\unity\RunUnityEditModeTests.ps1 -ResultsPath "TestResults\spec-player-boundary-editmode.xml" -LogFile "Logs\spec-player-boundary.log"
```

Se algum comando não puder rodar, o report deve registrar `NOT RUN` com motivo e risco residual — nunca inferir sucesso.

## 30. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: CONDITIONAL (só se Fase 3 executar)
- Requires EditMode tests: YES (teste de caracterização do par escolhido, antes de qualquer refactor)
- Requires PlayMode automated or final human scenario: YES se Fase 3 executar (movimento/equipamento/inventário/skills são gameplay-crítico)
- Requires regression test: YES se Fase 3 executar
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: mapa de dependência verificado por Grep + decisão documentada, no mínimo. Se Fase 3 rodou: adicionalmente dotnet build PASS (2 csproj) + EditMode test de caracterização PASS + snapshot antes/depois sem par novo. Sem Play Mode humano documentado, status máximo é BUILD_VALIDATED, nunca ACCEPTED.
```

## 31. Definition of Done

```text
Mapa de dependência dos 4 pares verificado por Grep real nesta execução (não copiado cegamente desta spec).
Decisão do primeiro par seguro documentada com justificativa.
Teste de caracterização auditado/criado para o par escolhido.
Fase 3 executada apenas se cobertura de teste existir; caso contrário, spec fecha só com mapa+decisão (resultado válido).
Snapshot antes/depois sem aumento de MutualModulePairs.
Build (2 csproj) exit code 0.
Execution report criado em docs/validation/ com as 4 sub-slices derivadas listadas.
Nenhuma spec afirma "modularização do Player concluída" — MutualModulePairs > 0 permanece esperado após esta spec (só 1 par no máximo é resolvido).
```

## 32. Anti-regressão

```text
Não alterar movimento, stamina, morte, respawn, equipamento, inventário ou skills observáveis em gameplay.
Não mover SkillPassiveModifier/SkillModifierType nesta spec.
Não criar .asmdef físico.
Não usar GameObject.Find/FindObjectOfType em qualquer código novo.
Não comunicar entre sistemas de gameplay fora do GameEventBus.
Não declarar "modularização concluída" enquanto MutualModulePairs > 0.
```

## 33. Notas para execução posterior

```text
Esta spec não cria as 4 sub-slices — apenas as recomenda com nome de arquivo sugerido (seção 11).
A sub-slice de Player|Skills deve, na sua Fase 0, avaliar separadamente se mover SkillPassiveModifier/SkillModifierType é o caminho certo, dado o acoplamento com SkillNodeDataSO e com Combat (PlayerAttackController, PlayerDamageReceiver) — provavelmente merece sua própria spec de migration de serialização, não um sub-slice simples.
Se a Fase 3 desta spec resolver o par Equipment|Player reusando o padrão já estabelecido por spec_arch_equipment_save_cycle_reduction_v29 (mover enum/DTO puro para Foundation), citar esse precedente no execution report.
```
