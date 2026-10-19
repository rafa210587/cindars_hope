# Handoff para Claude — gerar specs do trabalho restante

Data de referência: 2026-07-13  
Branch esperada: `dev`  
Objetivo deste documento: orientar o Claude a escrever specs detalhadas, fechadas e executáveis para o que ainda falta fazer no projeto antes de codar novas mudanças.

## Estado final (2026-07-16) — as 7 specs geradas por este handoff foram implementadas

As 7 specs de corte (`spec_arch_core_boundary_residual_v1`, `spec_arch_save_ownership_residual_v1`,
`spec_arch_ui_boundary_residual_v1`, `spec_arch_npc_quest_boundary_residual_v1`,
`spec_arch_player_gameplay_boundary_residual_v1`, `spec_arch_combat_boundary_residual_v1`,
`spec_arch_cave_integration_boundary_residual_v1`) geradas a partir deste handoff foram implementadas
e movidas para `.specs/implementados/`. Snapshot confirmado: `MutualModulePairs=0` (HEAD `a4203461`,
branch `dev`). Build 7/7 exit 0; EditMode 2837/2837 exit 0. Apenas
`spec_validation_human_playmode_smoke_v1` (Play Mode humano) segue pendente em
`.specs/a_implementar/`. O restante deste documento é o plano original, preservado como registro.

Este documento não é uma spec executável. Ele é um prompt/plano para transformar débitos conhecidos em specs pequenas, verificáveis e seguras.

## 1. Estado atual que deve ser validado no disco

Antes de escrever qualquer spec, não confie neste documento como prova única. Revalide:

```powershell
git fetch origin dev
git branch --show-current
git status --branch --short
git log --oneline -10
git rev-list --left-right --count origin/dev...dev
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
```

Baseline verificado pelo Codex em 2026-07-13:

```text
Branch=dev
Status=clean
Ahead origin/dev=6 commits
RuntimeModuleEdges=241
MutualModulePairs=25
UsingOnlyModuleEdges=213
UsingOnlyMutualModulePairs=17
```

Commits locais relevantes ainda não publicados no momento desta escrita:

```text
66361f17 chore(runtime): atualizar wiring de bootstraps
da83db93 feat(enemy): adicionar sprites de animacao gerados
a037a672 feat(cave): consolidar polimento visual runtime
1ea7bf88 refactor(arquitetura): reduzir acoplamento modular residual
c09fa288 refactor(arquitetura): reduzir pares reais core craft e inventory magic
e9b8c5bc chore(arquitetura): corrigir snapshot e hardening de registries
```

Se esses commits não existirem, se a branch não for `dev`, ou se a working tree estiver suja por trabalho humano/concorrente, pare e reporte antes de gerar specs.

## 2. Documentos que devem ser lidos antes das specs

Leia estes arquivos antes de escrever novas specs:

- `.specs/SPEC_SOURCE_OF_TRUTH.md`
- `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
- `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
- `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
- `.specs/SPEC_REGISTRY_TO_IMPLEMENT.md`
- `.specs/SPEC_REGISTRY_IMPLEMENTED.md`
- `docs/architecture/CLAUDE_MODULARIZATION_REMAINING_PLAN.md`
- `docs/architecture/CLAUDE_MODULARIZATION_HANDOFF_20260712.md`
- `docs/architecture/RUNTIME_BOOTSTRAP_OWNERSHIP_V5.md`
- `.claude/rules/RULES.md`

Também leia as specs recentemente adicionadas/alteradas relacionadas a Cave e Enemy antes de propor qualquer spec que toque nesses módulos.

## 3. Regras de autoria das specs

Cada spec deve:

1. Ter escopo pequeno e uma fronteira técnica clara.
2. Declarar explicitamente o que não pode mudar:
   - saves existentes;
   - IDs estáveis;
   - prefabs/cenas sem necessidade;
   - balanceamento;
   - quantidades de itens;
   - diálogos;
   - comportamento de combate;
   - fluxo de input/UI.
3. Ter critérios de aceite mensuráveis.
4. Ter gates de validação com comandos reais.
5. Ter rollback seguro por commit.
6. Não declarar “modularização concluída” enquanto `MutualModulePairs` for maior que zero.
7. Separar specs de código headless de specs que exigem PlayMode/humano.
8. Ser escrita para preservar o código atual como fonte canônica, não para forçar specs antigas sobre o código.

## 4. Pares mútuos restantes a transformar em specs

Snapshot real em 2026-07-13:

```text
Cave|Combat
Cave|Core
Cave|Enemy
Cave|SceneManagement
Cave|UI
Combat|Core
Combat|Enemy
Combat|Inventory
Combat|Player
Combat|Skills
Core|Inventory
Core|Player
Core|Save
Core|Skills
Core|UI
Equipment|Player
Farm|Save
Inventory|Player
NPC|Quests
NPC|UI
Player|Skills
Player|UI
Player|World
Quests|Save
UI|World
```

Não gere uma única spec gigante para todos. Gere specs por família de risco.

## 5. Specs que devem ser criadas

### Spec A — Core boundary residual

Objetivo: reduzir os pares `Core|Inventory`, `Core|Player`, `Core|Save`, `Core|Skills`, `Core|UI` sem alterar gameplay.

Direção esperada:

- mapear cada referência `Core -> domínio` e `domínio -> Core`;
- preferir ports pequenos em `Foundation` ou contratos já existentes;
- remover dependências do `GameBootstrap` apenas quando a cena/wiring continuar validável;
- manter `GameRuntimeCompositionRoot` e installers como owner do wiring;
- não mover lógica de gameplay para `Core`;
- não criar service locator genérico novo se um registry/port específico resolver.

Aceite mínimo:

```text
Snapshot antes/depois documentado
MutualModulePairs deve diminuir ou, se não diminuir, a spec deve explicar por que o corte foi abortado
Nenhum par novo pode aparecer
Build Unity-generated 7/7 deve passar
EditMode relevante deve passar
```

Gates:

```powershell
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
& .\tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1
& .\tools\unity\RunUnityEditModeTests.ps1 -ResultsPath "TestResults\spec-core-boundary-editmode.xml" -LogFile "Logs\spec-core-boundary.log"
```

Observação: esta spec deve ser dividida em sublotes. Um par por commit.

### Spec B — Save ownership residual

Objetivo: reduzir `Farm|Save` e `Quests|Save` com ownership claro de seção de save.

Direção esperada:

- verificar se o estado pertence ao domínio ou ao `SaveManager`;
- preferir `ISaveSectionProvider` por módulo;
- manter DTOs simples e compatíveis com `JsonUtility`;
- não renomear campo serializado salvo sem migration explícita;
- criar testes de roundtrip e fallback para save antigo quando aplicável.

Aceite mínimo:

```text
SaveManager não deve ganhar nova lógica de domínio
Cada seção deve ter owner único
Roundtrip de save deve passar
Snapshots de arquitetura documentados
```

Gates:

```powershell
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
& .\tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1
& .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Save"
```

### Spec C — UI boundary residual

Objetivo: reduzir `NPC|UI`, `Player|UI`, `UI|World` e avaliar `Core|UI` se ainda existir após a Spec A.

Direção esperada:

- separar modal/input visual de domínio;
- usar ports/interfaces para disponibilidade, interação e estado modal;
- não mover decisões de gameplay para controllers de UI;
- manter `GameplayInputRouter` como dono de roteamento global, se aplicável;
- preservar comportamento de `OnGUI`/Canvas já existente.

Risco: médio/alto. Esta spec precisa de PlayMode ou smoke humano porque regressões podem ser visuais/interativas.

Aceite mínimo:

```text
Nenhum modal deve abrir/fechar em duplicidade
Input bloqueado por modal deve continuar correto
Interações com portas, corpse, NPC e painéis devem continuar funcionando
Build e EditMode passam
PlayMode/smoke humano descrito e executado quando houver wiring visual
```

Gates:

```powershell
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
& .\tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1
& .\tools\unity\RunUnityEditModeTests.ps1 -ResultsPath "TestResults\spec-ui-boundary-editmode.xml" -LogFile "Logs\spec-ui-boundary.log"
```

Smoke humano obrigatório a documentar:

- abrir/fechar inventário;
- abrir painel de skills;
- abrir modal de crafting;
- conversar com NPC;
- interagir com portas;
- recuperar corpse, se fluxo disponível.

### Spec D — Player/gameplay boundary residual

Objetivo: reduzir ou documentar `Equipment|Player`, `Inventory|Player`, `Player|Skills`, `Player|World`.

Direção esperada:

- identificar dependências que são dados puros versus dependências de runtime;
- mover tipos puros para `Foundation` somente quando não afeta GUID/serialized fields;
- usar serviços/ports específicos para runtime;
- não alterar movimento, stamina, morte, respawn, equipamento, inventário ou skills sem teste de caracterização.

Risco: alto. Não codar direto sem specs de caracterização.

Specs derivadas recomendadas:

1. `spec_player_equipment_boundary_runtime.md`
2. `spec_player_inventory_boundary_runtime.md`
3. `spec_player_skills_boundary_runtime.md`
4. `spec_player_world_boundary_runtime.md`

Cada uma deve começar por testes de caracterização ou documentação de comportamento atual.

### Spec E — Combat boundary residual

Objetivo: planejar redução segura de `Combat|Core`, `Combat|Enemy`, `Combat|Inventory`, `Combat|Player`, `Combat|Skills`.

Direção esperada:

- não tentar quebrar todos os pares em uma rodada;
- mapear primeiro dano, cooldown, target, ameaça, skill/passives e drop/reward;
- separar cálculo puro de adapters MonoBehaviour quando possível;
- manter comportamento de inimigos e combate idêntico;
- exigir PlayMode/smoke humano para combate real.

Aceite mínimo para a spec:

```text
Lista das dependências por direção
Decisão explícita do primeiro par seguro a atacar
Testes de caracterização antes de refatorar
Sem mudança de dano, cooldown, alcance, drops ou IA
```

Gates:

```powershell
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
& .\tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1
& .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Combat"
```

Se não houver cobertura suficiente, a primeira spec deve ser de cobertura, não de refactor.

### Spec F — Cave integration boundary residual

Objetivo: tratar `Cave|Combat`, `Cave|Core`, `Cave|Enemy`, `Cave|SceneManagement`, `Cave|UI` depois do polimento visual recente.

Direção esperada:

- não desfazer `spec_cave_visual_polish_runtime`;
- validar assets, scene wiring e generators antes de refatorar;
- manter geração/materialização da Cave idêntica visualmente;
- evitar novos `Resources.Load`;
- se precisar de runtime config, preferir referência serializada ou installer explícito;
- separar specs de arte/visual de specs de arquitetura.

Aceite mínimo:

```text
Nenhum asset de Cave removido sem substituição
CaveScene e CreateMvpCaveScene permanecem coerentes
ArchitectureRatchetTests não podem regredir Resources.Load
Build/EditMode passam
Smoke visual humano documentado
```

Gates:

```powershell
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
& .\tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1
& .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Cave"
```

### Spec G — NPC/Quest boundary residual

Objetivo: reduzir `NPC|Quests` sem quebrar papéis, rotinas, diálogos, entregas de quest e lojas.

Direção esperada:

- caracterizar comportamento atual dos NPCs antes de refatorar;
- manter horários, papéis, lojas, rotas e disponibilidade;
- extrair contrato de quest interaction se houver dependência direta indevida;
- não mexer em texto/IDs de quest sem migration.

Aceite mínimo:

```text
NPCs continuam com papel e rotina esperados
Quest offer/turn-in continuam funcionando
Nenhum ID de quest muda
Build/EditMode passam
Smoke humano de conversa/quest documentado
```

Gates:

```powershell
& .\tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1
& .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.NPC"
& .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Quests"
```

### Spec H — Validação humana e cenas

Objetivo: fechar o gap entre “código compila/testa” e “jogo funciona visualmente”.

Esta spec deve consolidar smoke tests humanos para:

- TownScene;
- FarmScene;
- CaveScene;
- inventário;
- crafting;
- loja;
- conversa com NPC;
- quest offer/turn-in;
- combate básico;
- morte/respawn;
- save/load.

Aceite mínimo:

```text
Checklist humano com passos claros
Resultado esperado por passo
Arquivo de relatório em docs/validation/playmode/
Nenhuma spec de arquitetura sensível pode ser marcada implementada sem este smoke quando mexer em UI/cena/runtime visual
```

## 6. Ordem recomendada de geração das specs

1. Spec H — Validação humana e cenas.
2. Spec A — Core boundary residual.
3. Spec B — Save ownership residual.
4. Spec C — UI boundary residual.
5. Spec G — NPC/Quest boundary residual.
6. Spec D — Player/gameplay boundary residual.
7. Spec E — Combat boundary residual.
8. Spec F — Cave integration boundary residual.

Motivo: primeiro criar a rede de validação; depois atacar fronteiras menos visuais; por fim mexer em sistemas com maior chance de regressão perceptível.

## 7. Formato esperado dos arquivos gerados

Criar specs em `.specs/a_implementar/` com nomes claros, por exemplo:

```text
.specs/a_implementar/spec_validation_human_playmode_smoke_v1.md
.specs/a_implementar/spec_arch_core_boundary_residual_v1.md
.specs/a_implementar/spec_arch_save_ownership_residual_v1.md
.specs/a_implementar/spec_arch_ui_boundary_residual_v1.md
.specs/a_implementar/spec_arch_npc_quest_boundary_residual_v1.md
.specs/a_implementar/spec_arch_player_gameplay_boundary_residual_v1.md
.specs/a_implementar/spec_arch_combat_boundary_residual_v1.md
.specs/a_implementar/spec_arch_cave_integration_boundary_residual_v1.md
```

Atualizar somente os registros necessários:

- `.specs/SPEC_REGISTRY_TO_IMPLEMENT.md`
- `.specs/SPEC_INDEX.md`, se o padrão atual do repo exigir.

Não mover specs para `.specs/implementados/` até que o código correspondente tenha sido implementado e validado.

## 8. Prompt direto para Claude

Use este prompt quando for pedir para o Claude escrever as specs:

```text
Você vai trabalhar no projeto Unity/C# Cindar's Hope, branch dev.

Sua tarefa agora NÃO é codar. Sua tarefa é gerar specs implementáveis, detalhadas e seguras para o trabalho restante de modularização, validação humana e redução de acoplamento.

Antes de escrever qualquer spec, valide o estado real no disco/git:

git fetch origin dev
git branch --show-current
git status --branch --short
git log --oneline -10
git rev-list --left-right --count origin/dev...dev
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1

Leia obrigatoriamente:
- docs/architecture/CLAUDE_SPEC_AUTHORING_REMAINING_WORK_20260713.md
- docs/architecture/CLAUDE_MODULARIZATION_REMAINING_PLAN.md
- docs/architecture/CLAUDE_MODULARIZATION_HANDOFF_20260712.md
- docs/architecture/RUNTIME_BOOTSTRAP_OWNERSHIP_V5.md
- .specs/SPEC_SOURCE_OF_TRUTH.md
- .specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- .specs/SPEC_VALIDATION_MATRIX_MASTER.md
- .specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- .claude/rules/RULES.md

Gere specs novas em .specs/a_implementar/ para:
1. validação humana/playmode smoke;
2. Core boundary residual;
3. Save ownership residual;
4. UI boundary residual;
5. NPC/Quest boundary residual;
6. Player/gameplay boundary residual;
7. Combat boundary residual;
8. Cave integration boundary residual.

Não implemente código nesta etapa.
Não altere gameplay, saves, IDs, cenas, prefabs, balanceamento ou diálogos.
Não declare modularização concluída enquanto MutualModulePairs for maior que zero.
Cada spec deve conter escopo, não-escopo, critérios de aceite, plano por etapas, comandos de validação, rollback e riscos.
Atualize os registros de specs necessários.
Ao final, reporte arquivos criados/alterados, snapshot usado como baseline e qualquer divergência encontrada.
```

## 9. Critério de conclusão desta etapa

Esta etapa só está completa quando:

- todas as specs acima existirem em `.specs/a_implementar/`;
- os registros apontarem para elas;
- cada spec tiver critérios de aceite e gates reais;
- nenhuma spec prometer conclusão ampla sem validação;
- o Claude reportar o baseline usado e divergências, se houver.

