# SPEC — Redução Residual de Acoplamento UI (NPC|UI, Player|UI, UI|World)

> **Spec ID:** `spec_arch_ui_boundary_residual_v1`
> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-16
> **Wave:** WAVE ARCH — Redução Residual de Acoplamento Modular
> **Priority:** P2
> **Type:** Integration
> **Domain:** UI
> **Parallelizable:** CONDITIONAL
> **Parallel group:** arch_boundary_residual
> **Can run with:** specs que não tocam `Player/ManaManager.cs`, `World/CorpseInteractable.cs`, `UI/Death/CorpseRecoveryUIController.cs`, `NPC/NpcController.cs`, `NPC/NpcShopController.cs`, `UI/Modal/**`
> **Must not run with:** `spec_arch_npc_quest_boundary_residual_v1` (lock em `NPC/NpcController.cs`), qualquer spec residual de `Core|UI` boundary ainda ativa (overlap potencial em `UI/Modal/ModalManager.cs`), qualquer spec que altere `ModalManager` ou o fluxo de `GameplayInputRouter`
> **Repo lock scope:** `Assets/_Game/Scripts/Player/ManaManager.cs`, `Assets/_Game/Scripts/World/CorpseInteractable.cs`, `Assets/_Game/Scripts/UI/Death/CorpseRecoveryUIController.cs`, `Assets/_Game/Scripts/NPC/NpcController.cs`, `Assets/_Game/Scripts/NPC/NpcShopController.cs`, `Assets/_Game/Scripts/UI/Modal/**`
> **Depends on (Depende de):**
> - `.claude/rules/RULES.md` (invariantes de arquitetura Unity, item 2 — comunicação de gameplay só via GameEventBus)
> - Precedente vivo `Craft/CraftingPoint.cs` + `Craft/ICraftingStationModal.cs` (corte `Craft|UI` já existente no repo)
> **Blocks (Bloqueia):**
> - Nenhuma spec desta wave depende diretamente desta; reduz drift acumulado do `MutualModulePairs` de `UI` antes de novas waves de UI.
> **Scope:** Reduzir o acoplamento direto entre `Player`/`NPC`/`World` e `UI` nos 3 pontos identificados nesta sessão, substituindo o campo serializado concreto por uma interface consumida pelo domínio, seguindo o mesmo padrão adapter já usado em `CraftingPoint` (`ICraftingStationModal`). Preservar 100% do comportamento visual/interativo observável.
> **Out of scope:** Redesenhar o `ModalManager`; mudar o `GameplayInputRouter`; mudar texto/UX de qualquer modal; resolver `Core|UI` boundary (spec separada, se ainda pendente após a auditoria dela); balance; arte.

required_adrs: []
required_game_rules: []

---

## Evidência de implementação (2026-07-16)

Os 3 pares-alvo (`NPC|UI`, `Player|UI`, `UI|World`) saíram de `MutualModulePairs`, usando exatamente
o padrão adapter prescrito (`[SerializeField] MonoBehaviour` + `is IInterface`, molde
`CraftingPoint`/`ICraftingStationModal`) — confirmado por leitura direta dos 3 commits. Sem desvio de
técnica.

```text
Commits reais (branch dev, HEAD a4203461):
2014a11b refactor(arquitetura): cortar par mutuo Player|UI via porta
         -> ManaManager consome IModalStateProvider; MutualModulePairs 24 -> 23
5cc9f6bd refactor(arquitetura): cortar par mutuo UI|World via porta
         -> CorpseInteractable consome ICorpseRecoveryPresenter (World, nao Foundation,
            por depender de UnityEngine); MutualModulePairs 23 -> 22
692af532 refactor(arquitetura): cortar par mutuo NPC|UI via presenter ports
         -> NpcController/NpcShopController consomem INpcDialoguePresenter/INpcShopMenuPresenter/
            INpcBuyPanel/INpcSellPanel/IModalRuntime; MutualModulePairs 11 -> 10

Validation method: Invoke-UnityGeneratedProjectsBuild.ps1 + RunUnityEditModeTests.ps1 +
Get-ModularizationDependencySnapshot.ps1
Build (7 projects): PASS (exit 0)
EditMode: PASS 2837/2837 (exit 0)
Snapshot: MutualModulePairs=0 (NPC|UI, Player|UI, UI|World ausentes)
Play Mode / validacao humana: NOT RUN - pendente; coberto por spec_validation_human_playmode_smoke_v1
(segue em a_implementar/); o commit 692af532 documenta explicitamente "PlayMode/smoke de
dialogo+loja diferido ao playtest final"
```

**Desvios de técnica:** nenhum — os 3 cortes seguem o precedente `CraftingPoint` exatamente como
prescrito (campo serializado vira `MonoBehaviour`, cast para interface no ponto de uso, referência
de cena preservada sem regen).

**Residual risk:** smoke humano obrigatório desta spec (seção 30 — abrir inventário/skills, crafting,
diálogo, corpse, shop) não foi executado; fica coberto por `spec_validation_human_playmode_smoke_v1`.

---

# /speckit.specify

## 5. Contexto

Esta spec reduz drift acumulado de acoplamento modular na fronteira `UI`, medido pelo snapshot de dependências do repo (ver baseline abaixo). O padrão de corte já existe e está em produção no domínio `Craft` (`CraftingPoint` + `ICraftingStationModal`): o campo serializado permanece um `MonoBehaviour` (serializável no Inspector) e o domínio consumidor faz `_campo is IInterface` antes de chamar o contrato — sem `GetComponent`/busca de cena, sem quebrar wiring existente nas cenas geradas pelos `SceneCreation/Create*Scene.cs`. Esta spec generaliza o mesmo padrão para os 3 casos residuais mapeados nesta auditoria: `Player.ManaManager` → `UI.Modal.ModalManager`, `World.CorpseInteractable` → `UI.Death.CorpseRecoveryUIController`, `NPC.NpcController`/`NPC.NpcShopController` → `UI.Modal.ModalManager`/`UI.Dialogue.DialogueModal`.

Baseline de dependência modular (Branch=dev, Snapshot 2026-07-13, gerado por `tools/architecture/Get-ModularizationDependencySnapshot.ps1`):

```text
RuntimeModuleEdges=241
MutualModulePairs=25
UsingOnlyModuleEdges=213
UsingOnlyMutualModulePairs=17
```

Nenhuma spec desta wave declara "modularização concluída" enquanto `MutualModulePairs > 0`. Esta spec reduz o número, não o zera.

## 6. Problema

Sem os pontos abaixo isolados atrás de contratos, qualquer refactor futuro em `UI.Modal`, `UI.Death` ou `UI.Dialogue` propaga acoplamento direto para `Player`, `World` e `NPC` — obrigando recompile/retest cruzado mesmo quando a mudança é puramente visual, e mantendo `NPC|UI`/`Player|UI`/`UI|World` como pares mútuos no snapshot de arquitetura, o que trava a meta de `MutualModulePairs == 0` desta wave.

## 7. Objetivo

Ao final desta spec, `ManaManager`, `CorpseInteractable`, `NpcController` e `NpcShopController` consomem a UI correspondente via interface (não o tipo concreto `MonoBehaviour` importado do namespace `UI.*` além do necessário para o campo serializado), preservando o comportamento visual/interativo atual, sem alterar `ModalManager`, `GameplayInputRouter`, cenas ou prefabs além do necessário para religar os campos (se algum wiring de cena precisar de ajuste, deve ser feito via os `Create*Scene.cs` editor scripts existentes, nunca YAML manual).

## 8. Fontes obrigatórias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.claude/rules/RULES.md
.claude/rules/unity-architecture.md
.claude/rules/id-stability.md
.claude/skills/ui-modal-stack/SKILL.md
.claude/skills/testing-quality-gate.md (via .claude/skills/spec-execution/SKILL.md)
```

Para runtime/code specs, incluir também:

```text
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
```

## 9. Estado atual do repo (Phase 0 — auditado nesta sessão via Grep/Read direto)

Confirmado nesta sessão:

- `Assets/_Game/Scripts/Player/ManaManager.cs:21`: `[SerializeField] private ModalManager _modalManager;` — campo concreto de `CindarsHope.UI.Modal.ModalManager`.
- `Assets/_Game/Scripts/World/CorpseInteractable.cs:15`: `[SerializeField] private CorpseRecoveryUIController _uiController;` — campo concreto de `CindarsHope.UI.Death.CorpseRecoveryUIController`.
- `Assets/_Game/Scripts/NPC/NpcController.cs`: `[SerializeField] private DialogueModal _dialogueModal;` (linha 24) e `[SerializeField] private ModalManager _modalManager;` (linha 25), com `using CindarsHope.UI.Dialogue;` e `using CindarsHope.UI.Modal;` no topo do arquivo. O controller chama `_dialogueModal.Show(...)`, `.ShowWithChoices(...)`, `.Hide()`, assina `_dialogueModal.OnClose`/`.OnChoiceSelected` diretamente no tipo concreto.
- `Assets/_Game/Scripts/NPC/NpcShopController.cs`: `[SerializeField] private DialogueModal _dialogueModal;`, `[SerializeField] private ShopMenuModal _shopMenuModal;`, `[SerializeField] private ModalManager _modalManager;` — mesmo padrão de acoplamento direto, com leitura de `_modalManager.CurrentModal` (enum `ModalType`) usada para decisão de fluxo (linhas ~185, ~799).
- Precedente confirmado: `Assets/_Game/Scripts/Craft/CraftingPoint.cs:22` usa `[SerializeField] private MonoBehaviour _craftingModal;` e no `Interact()` faz `_craftingModal is ICraftingStationModal craftingModal` antes de chamar `craftingModal.Open(...)`, com fallback via `GameEventBus.Publish(new OpenCraftingStationRequestedEvent(...))` quando o campo não está plugado (`Assets/_Game/Scripts/Craft/ICraftingStationModal.cs`). Este é o modelo a replicar.
- `ModalManager` está em `Assets/_Game/Scripts/UI/Modal/ModalManager.cs` e expõe (a confirmar exato shape na Fase 0 de execução) `CurrentModal` (enum `ModalType`) e métodos de abrir/fechar/limpar (`ClearAllModals()` já referenciado por `NpcShopController.cs:844`). A spec deve extrair uma interface mínima (`IModalStateReader`/similar) cobrindo só o subset realmente consumido por `ManaManager`, `NpcController` e `NpcShopController` — não replicar a API pública inteira de `ModalManager` na interface se parte dela não for usada por esses consumidores.
- `Assets/_Game/Scripts/Foundation/IModalStateProvider.cs` já existe no repo — Fase 0 de execução DEVE ler este arquivo primeiro e confirmar se cobre total ou parcialmente a necessidade antes de criar qualquer interface nova (regra de não duplicação, seção 13).
- Este estado real precisa ser reconfirmado na Fase 0 de execução (a auditoria acima é a base, não substitui a leitura direta do código no momento de implementar) — não recriar sistema existente sem confirmar ausência no repo.

O que esta spec não deve recriar: `ModalManager`, `GameEventBus`, `GameplayInputRouter`, `IModalStateProvider` (se já cobrir o caso).

## 10. User stories / engineering stories

```text
Como sistema Player, quero consultar disponibilidade/estado de modal via interface, para não depender do tipo concreto UI.Modal.ModalManager.
Como CorpseInteractable, quero abrir a UI de recuperação de corpse via interface, para desacoplar World de UI.Death.
Como NpcController/NpcShopController, quero expor diálogo e menu via interface mínima, para reduzir o par mútuo NPC|UI sem alterar o fluxo de conversa existente.
```

## 11. Escopo

Inclui:
- Definir (ou reusar, se `IModalStateProvider` já cobrir) uma interface mínima para o subset de `ModalManager` consumido por `ManaManager`, `NpcController`, `NpcShopController`.
- Definir uma interface mínima para o subset de `CorpseRecoveryUIController` consumido por `CorpseInteractable`.
- Definir uma interface mínima para o subset de `DialogueModal` (e `ShopMenuModal`, no caso de `NpcShopController`) consumido por `NpcController`/`NpcShopController`.
- Trocar os campos serializados concretos pelo padrão `[SerializeField] private MonoBehaviour _campo;` + cast `is IInterface` no ponto de uso, igual a `CraftingPoint`.
- Ajustar wiring de cena (via `Editor/SceneCreation/Create*Scene.cs`, não YAML manual) se o tipo do campo mudar de forma que quebre o assign existente no Inspector/editor script.
- Re-executar os 3 editor generators/scene creators afetados e confirmar wiring ainda resolve.

Fora:
- Redesenho de `ModalManager`/`GameplayInputRouter`.
- Qualquer mudança em `Core|UI` boundary (spec separada).
- Texto, layout, ou UX de qualquer modal.
- Qualquer mudança em save/quest/economy.

## 12. Fora de escopo

```text
Não inclui: redesign de ModalManager; mudança de GameplayInputRouter; mudança de UX/texto/layout de modal; alteração de save schema; alteração de fluxo de quest.
```

## 13. Regras de não duplicação

```text
Não criar uma segunda interface de modal state se Foundation/IModalStateProvider.cs já cobrir o caso — extender/reusar primeiro.
Não recriar ModalManager, GameEventBus ou GameplayInputRouter.
Não duplicar o padrão adapter — reusar exatamente a forma já usada em CraftingPoint/ICraftingStationModal (MonoBehaviour serializado + cast `is IInterface`), não inventar uma variante nova (ex.: injeção via ScriptableObject) sem justificar por que o padrão existente não serve.
```

## 14. Critérios de aceite

### 14.1 ManaManager desacoplado de ModalManager concreto

- `ManaManager` não importa `CindarsHope.UI.Modal.ModalManager` como tipo do campo serializado consumido em lógica de domínio (o campo pode continuar `MonoBehaviour` serializado, com cast para interface no uso).
- Comportamento de bloqueio/disponibilidade de mana ligado a modal permanece idêntico ao atual.
- Evidência esperada: diff do arquivo + `dotnet build` PASS.

### 14.2 CorpseInteractable desacoplado de CorpseRecoveryUIController concreto

- `CorpseInteractable` consome a UI de recuperação via interface.
- Fluxo de recuperação de corpse (abrir UI ao interagir) funciona sem regressão.
- Evidência esperada: diff + smoke humano (ver seção Testing Quality Gate).

### 14.3 NpcController/NpcShopController desacoplados de DialogueModal/ShopMenuModal/ModalManager concretos

- Ambos os controllers consomem diálogo/shop/modal state via interface mínima.
- Diálogo abre/fecha, choices funcionam, shop abre (Buy/Sell/ShopMenu), `ClearAllModals()`-equivalente continua disponível via a interface ou via fallback documentado.
- Evidência esperada: diff + smoke humano.

### 14.4 Nenhuma regressão de modal stack

- Nenhum modal abre/fecha em duplicidade; input bloqueado por modal continua correto (via `ModalManager`/`GameplayInputRouter` inalterados).
- Build e EditMode passam.

## 15. Notas

```text
Esta spec não resolve Core|UI boundary — se essa auditoria ainda estiver pendente/ativa em paralelo, tratar como Must-not-run-with (lock em UI.Modal.ModalManager compartilhado) até uma das duas terminar.
```

---

# /speckit.plan

## 16. Arquitetura alvo

```text
Assets/_Game/Scripts/Foundation/
  IModalStateProvider.cs        (reusar/estender se cobrir o caso; senão, novo contrato mínimo no mesmo padrão)

Assets/_Game/Scripts/UI/Modal/
  ModalManager.cs                (implementa a(s) interface(s) consumida(s); sem mudança de comportamento público)

Assets/_Game/Scripts/UI/Dialogue/
  DialogueModal.cs                (implementa interface mínima consumida por NPC)

Assets/_Game/Scripts/UI/Death/
  CorpseRecoveryUIController.cs   (implementa interface mínima consumida por World)

Assets/_Game/Scripts/Player/
  ManaManager.cs                  (campo trocado para MonoBehaviour + cast is IInterface)

Assets/_Game/Scripts/World/
  CorpseInteractable.cs           (campo trocado para MonoBehaviour + cast is IInterface)

Assets/_Game/Scripts/NPC/
  NpcController.cs                (campos trocados para MonoBehaviour + cast is IInterface)
  NpcShopController.cs            (campos trocados para MonoBehaviour + cast is IInterface)

docs/validation/
  spec_arch_ui_boundary_residual_v1_execution_report.md
```

## 17. Contratos, dados e eventos

### 17.1 Data contracts
Nenhum novo save DTO. Possível interface nova em `Foundation` (contrato puro, sem Unity ref além de assinatura de método/propriedade).

### 17.2 Runtime contracts
Novas interfaces mínimas (nomes exatos a decidir na Fase 0, seguindo convenção `I<Substantivo>`), cobrindo apenas os membros realmente chamados pelos 3 consumidores auditados nesta spec.

### 17.3 Event contracts
N/A — nenhum evento novo. Fallback por evento (`GameEventBus`) só se a Fase 0 concluir que é necessário para algum caso sem ref direta disponível, seguindo o precedente de `OpenCraftingStationRequestedEvent`.

### 17.4 Save contracts
N/A — nenhum DTO novo, nenhuma mudança de save schema.

### 17.5 UI contracts
Ver 17.2 — as views (`ModalManager`, `DialogueModal`, `ShopMenuModal`, `CorpseRecoveryUIController`) passam a implementar a interface consumida por seus respectivos domínios, sem mudar comportamento visual público.

## 18. Sistemas afetados

```text
Player (ManaManager)
World (CorpseInteractable)
NPC (NpcController, NpcShopController)
UI (ModalManager, DialogueModal, ShopMenuModal, CorpseRecoveryUIController)
Foundation (contrato de interface)
Editor/SceneCreation (wiring, se necessário)
```

## 19. Arquivos permitidos

```text
Assets/_Game/Scripts/Foundation/IModalStateProvider.cs
Assets/_Game/Scripts/Foundation/**  (apenas se nova interface mínima for necessária)
Assets/_Game/Scripts/Player/ManaManager.cs
Assets/_Game/Scripts/World/CorpseInteractable.cs
Assets/_Game/Scripts/NPC/NpcController.cs
Assets/_Game/Scripts/NPC/NpcShopController.cs
Assets/_Game/Scripts/UI/Modal/ModalManager.cs
Assets/_Game/Scripts/UI/Dialogue/DialogueModal.cs
Assets/_Game/Scripts/UI/Shop/ShopMenuModal.cs
Assets/_Game/Scripts/UI/Death/CorpseRecoveryUIController.cs
Assets/_Game/Scripts/Editor/SceneCreation/**  (apenas wiring, se necessário)
Assets/_Game/Tests/EditMode/**
docs/validation/**
```

## 20. Arquivos proibidos

```text
Assets/**/*.unity, *.prefab, *.asset salvo autorização explícita
Assets/_Game/Scripts/UI/Input/GameplayInputRouter.cs
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Save/**
docs_old/**
docs/archive/**
Packages/**
ProjectSettings/**
```

## 21. Estratégia de implementação

```md
### Fase 0 — Auditoria: reler os 5 arquivos-alvo + Foundation/IModalStateProvider.cs; confirmar shape exato de ModalManager/DialogueModal/ShopMenuModal/CorpseRecoveryUIController; decidir se reusa IModalStateProvider ou cria interface mínima nova; mapear wiring de cena atual (Create*Scene.cs)
### Fase 1 — Definir/estender contrato(s) de interface em Foundation
### Fase 2 — Fazer ModalManager/DialogueModal/ShopMenuModal/CorpseRecoveryUIController implementarem a(s) interface(s)
### Fase 3 — Trocar campo serializado em ManaManager para MonoBehaviour + cast is IInterface
### Fase 4 — Trocar campo serializado em CorpseInteractable para MonoBehaviour + cast is IInterface
### Fase 5 — Trocar campos serializados em NpcController/NpcShopController para MonoBehaviour + cast is IInterface
### Fase 6 — Ajustar wiring em Create*Scene.cs se necessário; rodar CindarsHope/Validar Projeto
### Fase 7 — Build + EditMode + smoke humano documentado; relatório
```

## 22. Ordem de execucao (ordem segura)

```text
1. Ler CraftingPoint.cs + ICraftingStationModal.cs como referência de padrão.
2. Auditar shape real dos 4 tipos UI-alvo e Foundation/IModalStateProvider.cs.
3. Criar/estender interface(s) mínima(s).
4. Implementar interface(s) nos 4 tipos UI concretos (sem mudar API pública existente, só adicionar).
5. Trocar campo + cast em ManaManager.
6. Trocar campo + cast em CorpseInteractable.
7. Trocar campo + cast em NpcController.
8. Trocar campo + cast em NpcShopController.
9. Rodar CindarsHope/Validar Projeto (wiring check) se scene refs mudarem.
10. dotnet build (Assembly-CSharp e Assembly-CSharp-Editor).
11. Rodar EditMode tests relevantes.
12. Smoke humano documentado (ver Testing Quality Gate).
13. Rodar snapshot de dependência modular e registrar delta no relatório.
14. Registrar relatório.
```

## Paralelização

- Parallelizable: CONDITIONAL
- Parallel group: arch_boundary_residual
- Can run with:
  - specs que não tocam os arquivos do repo lock scope
- Must not run with:
  - `spec_arch_npc_quest_boundary_residual_v1` (lock em `NPC/NpcController.cs`)
  - spec residual de `Core|UI` boundary se ainda ativa (overlap em `UI/Modal/ModalManager.cs`)
- Shared files/systems that require lock:
  - `Assets/_Game/Scripts/Player/ManaManager.cs`
  - `Assets/_Game/Scripts/World/CorpseInteractable.cs`
  - `Assets/_Game/Scripts/UI/Death/CorpseRecoveryUIController.cs`
  - `Assets/_Game/Scripts/NPC/NpcController.cs`
  - `Assets/_Game/Scripts/NPC/NpcShopController.cs`
  - `Assets/_Game/Scripts/UI/Modal/ModalManager.cs`
- Reason:
  - Overlap direto de arquivo com `spec_arch_npc_quest_boundary_residual_v1` em `NpcController.cs`; overlap potencial de `ModalManager.cs` com qualquer spec de `Core|UI` boundary ainda em execução.

## 23. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? N/A — sem save DTO novo
```

## 24. Impacto em eventos

```text
Adds events: NO (salvo se Fase 0 concluir que fallback por evento é necessário para algum caso — documentar se ocorrer)
Changes existing events: NO
Requires unsubscribe pattern: NO (padrão de subscribe/unsubscribe de DialogueModal/ShopMenuModal já existente permanece igual, só o tipo do campo muda)
```

## 25. Impacto em UI/Unity

```text
Changes UI: YES (wiring interno, não visual/UX)
Changes scenes: CONDITIONAL (só se o tipo do campo serializado quebrar assign existente — via Create*Scene.cs, não YAML manual)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos

```text
Risco: trocar o tipo do campo serializado de concreto para MonoBehaviour pode desconectar a referência já plugada em cena/prefab (Unity reserializa por tipo).
Mitigação: preferir manter o campo como MonoBehaviour desde o início (igual CraftingPoint) e validar via CindarsHope/Validar Projeto + re-rodar os Create*Scene.cs geradores; nunca editar YAML manualmente para "consertar" a referência.

Risco: interface mínima nova pode divergir do contrato Foundation/IModalStateProvider.cs já existente, criando um segundo contrato paralelo.
Mitigação: Fase 0 obrigatoriamente lê IModalStateProvider.cs antes de criar qualquer interface nova; se cobrir o caso, reusar; se não, documentar por que uma interface separada é necessária.

Risco: regressão silenciosa em UX de modal (double-open, input não bloqueado) não pega no build/EditMode.
Mitigação: smoke humano obrigatório listado no Testing Quality Gate antes de ACCEPTED.
```

## 27. Rollback

```text
Reverter os campos serializados para o tipo concreto original nos 4 arquivos.
Remover interface(s) nova(s) criada(s) em Foundation se não usada em outro lugar.
Reverter wiring de cena se algum Create*Scene.cs foi ajustado.
Nenhum dado de save é afetado — rollback é puramente de código/wiring.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Auditar shape real de ModalManager, DialogueModal, ShopMenuModal, CorpseRecoveryUIController e Foundation/IModalStateProvider.cs.
- [ ] T002 — Criar/estender interface(s) mínima(s) em Foundation.
- [ ] T003 — Implementar interface(s) em ModalManager/DialogueModal/ShopMenuModal/CorpseRecoveryUIController.
- [ ] T004 — Trocar campo + cast em Player/ManaManager.cs.
- [ ] T005 — Trocar campo + cast em World/CorpseInteractable.cs.
- [ ] T006 — Trocar campo + cast em NPC/NpcController.cs.
- [ ] T007 — Trocar campo + cast em NPC/NpcShopController.cs.
- [ ] T008 — Ajustar wiring em Create*Scene.cs se necessário; rodar CindarsHope/Validar Projeto.
- [ ] T009 — dotnet build (Assembly-CSharp e Assembly-CSharp-Editor) + EditMode tests relevantes.
- [ ] T010 — Executar e documentar smoke humano (lista na seção 30).
- [ ] T011 — Rodar snapshot de dependência modular; registrar delta de MutualModulePairs no relatório.
- [ ] T012 — Gerar execution report.
```

## 29. Validações obrigatórias

```powershell
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
& .\tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1
& .\tools\unity\RunUnityEditModeTests.ps1 -ResultsPath "TestResults\spec-ui-boundary-editmode.xml" -LogFile "Logs\spec-ui-boundary.log"
.\tools\docs\validate_docs.ps1
```

Se algum comando não puder rodar, o report deve registrar `NOT RUN` com motivo e risco residual.

## 30. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: NO (wiring/contrato, não lógica de gameplay determinística)
- Requires EditMode tests: CONDITIONAL — se alguma lógica de decisão (ex.: `ResolveQuestInteractionMode`-like) for tocada incidentalmente; caso contrário, cobertura por regressão do fluxo existente é suficiente.
- Requires PlayMode automated or final human scenario: YES
- Requires regression test: YES (smoke humano cobre a regressão)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: dotnet build PASS + Unity compile validation PASS + smoke humano documentado com os passos abaixo executados e resultado registrado.
```

Smoke humano obrigatório (executar e documentar resultado por passo):

```text
1. Abrir inventário (tecla/hotbar correspondente) e fechar — confirmar abre/fecha sem duplicidade.
2. Abrir painel de skills e fechar.
3. Interagir com uma estação de crafting e abrir o modal de crafting (regressão do precedente CraftingPoint).
4. Conversar com um NPC (ex.: npc_thalindra) — abrir diálogo, escolher uma opção, fechar.
5. Interagir com uma porta/objeto interativo do mundo.
6. Se houver corpse disponível no fluxo de teste: interagir e confirmar UI de recuperação abre corretamente.
7. Abrir e fechar o menu de shop de um NPC vendedor (Buy/Sell/ShopMenu) — confirmar ManaManager e bloqueio de input por modal continuam corretos.
```

## 31. Definition of Done

```text
Spec implementada dentro dos arquivos permitidos.
Nenhum arquivo proibido alterado.
Docs validation executada ou NOT RUN com motivo.
dotnet build + Unity compile validation executados ou NOT RUN com motivo.
EditMode tests adicionados quando exigidos pela Fase 0.
Smoke humano executado e documentado (não apenas descrito).
Snapshot de dependência modular re-rodado e delta de MutualModulePairs/UsingOnlyMutualModulePairs registrado no relatório.
Execution report criado.
Sem claim de ACCEPTED sem evidência.
```

## 32. Anti-regressão

```text
Nenhum modal abre/fecha em duplicidade.
Input bloqueado por modal continua correto (GameplayInputRouter inalterado).
Interações com portas, corpse, NPC e painéis continuam funcionando.
Não alterar IDs existentes.
Não alterar evento público sem atualizar consumidores.
Não serializar referência Unity em save.
Não usar GameObject.Find/FindObjectOfType em runtime.
Não criar fluxo UI que deixe gameplay input ativo durante modal.
Não declarar "modularização concluída" — apenas registrar o delta de MutualModulePairs.
```

## 33. Notas para execução posterior

```text
Esta spec não implementa UI final nem resolve Core|UI boundary.
Esta spec não executa validação humana imediata — smoke humano faz parte do próprio fechamento desta spec (não é intermediário, é o critério de aceite final dela), mas não substitui uma validação humana de lote maior se o usuário pedir uma no final da wave.
Se Foundation/IModalStateProvider.cs cobrir menos do que o necessário, a extensão dele (não um contrato paralelo) deve ser preferida — documentar a decisão tomada na Fase 0 no relatório final.
```

## 34. Checklist final da spec pronta

```text
[x] Tem cabeçalho completo.
[x] Declara Parallelizable / Parallel group / locks.
[x] Declara fontes obrigatórias lidas.
[x] Declara estado atual do repo.
[x] Tem escopo pequeno.
[x] Tem fora de escopo explícito.
[x] Tem regras de não duplicação.
[x] Tem arquivos permitidos e proibidos.
[x] Tem contratos/dados/eventos/save/UI quando aplicável.
[x] Tem critérios de aceite verificáveis.
[x] Tem validações obrigatórias.
[x] Tem Testing Quality Gate.
[x] Marca validação humana como DEFERRED_TO_FINAL_VALIDATION quando aplicável.
[x] Tem riscos e rollback.
[x] Não pede execução humana intermediária.
[x] Não altera SPEC_EXECUTION_ORDER.md como se a spec já estivesse implementada.
```
