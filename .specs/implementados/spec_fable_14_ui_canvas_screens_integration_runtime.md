# SPEC — UI: Integração Canvas das Telas Core (Inventário, Equipamento, Skill Tree, Quest Log)

> **Spec ID:** `fable_14_spec_ui_canvas_screens_integration_runtime`
> **Status:** A implementar
> **Wave:** FABLE — Gap Closure Bloco F (fundação)
> **Priority:** P1
> **Type:** UI / Integration
> **Domain:** UI
> **Parallelizable:** NO
> **Parallel group:** fable_bloco_F
> **Can run with:** N/A
> **Must not run with:** fable_10, fable_11 (geradores de cena compartilhados), qualquer spec de HUD
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, geradores de cena (bloco de UI), ModalManager
> **Depends on:**
> - WAVE 04 (viewmodels CONTRACT_ONLY), WAVE 11 (projections), WI-23 (views headless + GameplayHudCanvas)
> **Blocks:** promoção das specs 04_spec_ui_* restantes da fila
> **Scope:** substituir os 4 painéis IMGUI de debug por telas Canvas reais construídas pelo padrão programático do ShopCanvas.
> **Out of scope:** todas as demais telas (crafting/calendar/fonte/detail drawers), gamepad, arte final, localização.

required_adrs: [ADR-0013-input-keyboard-mouse-only-v1.md, ADR-0014-single-difficulty-v1.md]
required_game_rules: [ui_modal_rules.md]

---

# /speckit.specify

## Contexto

`UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md` define os fluxos de tela; a WAVE 04 entregou 11
viewmodels CONTRACT_ONLY; a WAVE 11 entregou projections; a WI-23 entregou
`GameplayHudCanvas` + 6 views headless. Mas as telas reais de gameplay continuam IMGUI de
debug: `InventoryPanelController` (tecla I), `CharacterEquipmentPanelController` (K/L),
`SkillTreeGameplayPanelController` (U) e `QuestLogPanelController` (J) — todos
`RuntimeInitializeOnLoadMethod` singletons com `OnGUI`. O único padrão Canvas validado em
produção é o ShopCanvas (DialogueModal/ShopMenuModal/BuyPanel/SellPanel) construído
programaticamente pelo `CreateMvpTownScene.CreateShopUi` — sem prefabs, compatível com a
proibição de editar YAML.

## Problema

IMGUI não escala (sem foco, sem navegação, sem tooltip posicionado, fontes fixas), viola a
direction de focus order/empty states, e o jogo "parece debug" — o maior gap visível do
playable slice. As 11 specs 04_ não saem de CONTRACT_ONLY enquanto não existir um caminho
Canvas validado para telas modais de gameplay.

## Objetivo

Ao final desta spec, deve existir um `GameplayScreensCanvasBootstrap`
(RuntimeInitializeOnLoadMethod, DontDestroyOnLoad — padrão WI-23) que constrói em código as
4 telas Canvas (Inventário, Equipamento, Skill Tree, Quest Log) consumindo os viewmodels da
WAVE 04/11, com as MESMAS teclas/ModalTypes atuais, focus order navegável por teclado,
empty states explícitos e os controllers IMGUI desativados por flag (fallback preservado).

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
.claude/skills/ui-modal-stack/SKILL.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- ModalManager (stack, HasActiveModal, ModalTypes: Inventory/CharacterEquipment/SkillTree/QuestLog)
- IMGUI controllers (4) com binding completo aos managers — fonte da lógica a portar
- Viewmodels WAVE 04 (auditar Fase 0: quais dos 11 cobrem as 4 telas; ex.
  InventoryGridViewModel, EquipmentCompareViewModel, SkillTreeViewModel, QuestLogProjection W11)
- Padrão construtor Canvas: CreateMvpTownScene.CreateShopUi (CreatePanel/CreateText/
  CreateButton/CreateScrollableContainer) — EXTRAIR para utilitário compartilhado
- GameplayHudBootstrap/GameplayHudCanvas (WI-23 — padrão de bootstrap de canvas runtime)
- GameplayInputRouter (teclas I/K/U/J)
Não existe:
- telas Canvas de gameplay; focus order por teclado; utilitário compartilhado de construção
```

## Engineering stories

```text
Como jogador, quero inventário em grade com seleção por setas/tab, tooltip e ação de equipar/usar.
Como jogador, quero skill tree visual com detail antes de gastar ponto (regra canônica).
Como ModalManager, quero as novas telas no mesmo stack com Esc fechando o topo.
Como projeto, quero um construtor de UI compartilhado para as próximas 7 telas das specs 04_.
```

## Escopo

```text
Inclui:
- RuntimeUiBuilder (NOVO utilitário estático): extrair/generalizar CreatePanel/CreateText/
  CreateButton/CreateScrollableContainer/CreateInputField do CreateMvpTownScene (que passa a
  consumi-lo — refactor sem mudança de comportamento) + CreateGrid/CreateFocusOutline;
- GameplayScreensCanvasBootstrap (NOVO): canvas sorting 30, EventSystem reuse, DontDestroyOnLoad;
- 4 telas Canvas (NOVAS): InventoryScreenView (grade 5xN, tooltip, usar/equipar/largar com
  confirmação), EquipmentScreenView (slots + atributos derivados + comparação não-equipa-por-hover),
  SkillTreeScreenView (5 árvores em abas, node detail drawer ANTES de comprar, confirmação de
  SkillPoint, active slots R/T/Y/G), QuestLogScreenView (lista + detail com objetivos visíveis
  conforme spoiler rules WAVE 09);
- UiFocusController (NOVO): ordem de foco linear por tela, setas/Tab/Enter/Esc, outline visual;
- mesmas teclas e ModalTypes: abrir empurra modal, Esc fecha topo, gameplay input bloqueado
  (contrato atual preservado);
- empty states explícitos ("Inventario vazio.", "Nenhuma quest ativa.");
- IMGUI controllers atrás de flag estática LegacyImguiPanelsEnabled=false (rollback trivial);
- EditMode tests: construção das 4 telas (instanciação headless), focus order determinístico,
  binding viewmodel→view com dados sintéticos, empty states.
```

## Fora de escopo

```text
Não inclui: crafting/shop/calendar/fonte/dialogue (shop já é Canvas; demais nas specs 04_);
gamepad (04_spec futura); arte/tema final; animações; localização; HUD (WI-23 cobre).
```

## Regras de não duplicação

```text
Não criar segundo ModalManager/stack — push/pop no existente.
Não criar segundo EventSystem se houver um na cena.
Não duplicar lógica de negócio dos IMGUI controllers — portar consumindo os MESMOS managers/viewmodels.
Não criar prefabs — construção programática (padrão ShopCanvas).
```

## Critérios de aceite

### CA-1 Quatro telas Canvas funcionais
- I/K/U/J abrem telas Canvas com dados reais; Esc fecha; gameplay bloqueado; IMGUI desativado.
- Evidência: testes de construção/binding + cenário humano.

### CA-2 Focus order e regras canônicas
- Navegação completa por teclado em cada tela; skill node SEMPRE mostra detail antes de
  comprar; equipment compare nunca equipa por foco/hover; confirmação para largar item.
- Evidência: testes do UiFocusController + checklist humano por regra.

### CA-3 Construtor compartilhado sem regressão
- CreateMvpTownScene consome RuntimeUiBuilder e gera ShopCanvas idêntico (diff de hierarquia).
- Evidência: teste editor comparando hierarquia gerada antes/depois (nomes/componentes).

### CA-4 Empty states
- Inventário vazio, sem skill points e sem quests exibem mensagens explícitas.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Runtime/
  RuntimeUiBuilder.cs                (NOVO)
  GameplayScreensCanvasBootstrap.cs  (NOVO)
  UiFocusController.cs               (NOVO)
  Screens/{InventoryScreenView,EquipmentScreenView,SkillTreeScreenView,QuestLogScreenView}.cs
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs (consumir builder)
Assets/_Game/Scripts/UI/<controllers IMGUI> (flag de desativação)
Assets/_Game/Tests/EditMode/UI/{RuntimeUiBuilderTests,UiFocusControllerTests,GameplayScreensTests}.cs
```

## Contratos

### UI contracts
Cada ScreenView: `Open()/Close()/Rebind()`; push/pop do ModalType correspondente;
consome viewmodel WAVE 04/11 correspondente (mapear na Fase 0).
### Runtime contracts — builder estático puro (retorna GameObjects configurados).
### Event contracts — N/A novos (assina eventos que os IMGUI já assinam).
### Save contracts — N/A (UI state não persiste — regra canônica).

## Sistemas afetados

```text
UI/Modal stack, Input routing, Inventory/Equipment/SkillTree/Quest managers (somente leitura+ações existentes), Town generator (refactor)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/UI/Runtime/** (novo namespace)
Assets/_Game/Scripts/UI/<4 controllers IMGUI> (flag apenas)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs (refactor p/ builder)
Assets/_Game/Tests/EditMode/UI/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity/*.prefab/*.asset; Packages/ProjectSettings; ModalManager.cs (consumir, não alterar);
managers de gameplay (somente APIs existentes); SaveManager
```

## Estratégia de implementação

```md
### Fase 0 — Mapear viewmodels WAVE 04/11 disponíveis por tela; auditar IMGUI controllers.
### Fase 1 — RuntimeUiBuilder extraído + refactor do ShopCanvas (equivalência testada).
### Fase 2 — Bootstrap + UiFocusController + InventoryScreenView (tela piloto completa).
### Fase 3 — Equipment/SkillTree/QuestLog views (regras canônicas).
### Fase 4 — Flag IMGUI off + testes + validação estrita + report.
```

## Paralelização

- Parallelizable: NO
- Reason: trava UI/modal compartilhados e o gerador da TownScene.

## Impacto em save/load

```text
Does this change save schema? NO (UI state não persiste — regra canônica)
```

## Impacto em eventos

```text
Adds events: NO | Changes existing: NO | Requires unsubscribe: YES (views)
```

## Impacto em UI/Unity

```text
Changes UI: YES (núcleo da spec) | Scenes: via gerador (refactor) | Prefabs/assets: NO
Requires Play Mode final validation: YES (obrigatório para UI)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: regressão no ShopCanvas pelo refactor. Mitigação: teste de equivalência de hierarquia + checklist de compra/venda no cenário final.
Risco: conflito de EventSystem duplicado. Mitigação: reuse-if-exists (padrão WI-23).
Risco: telas Canvas vivas durante transição de cena. Mitigação: DontDestroyOnLoad + Rebind em sceneLoaded (padrão ActiveSkillExecutionController).
```

## Rollback

```text
LegacyImguiPanelsEnabled=true restaura IMGUI; remover bootstrap desliga Canvas; refactor do
builder é equivalente (testado).
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Mapear viewmodels por tela + auditar IMGUI controllers.
- [ ] T002 — RuntimeUiBuilder + refactor ShopCanvas + teste de equivalência.
- [ ] T003 — Bootstrap + UiFocusController + testes.
- [ ] T004 — InventoryScreenView (piloto: grade/tooltip/ações/empty state).
- [ ] T005 — EquipmentScreenView (compare sem equipar por foco).
- [ ] T006 — SkillTreeScreenView (detail antes de comprar + confirmação + slots).
- [ ] T007 — QuestLogScreenView (spoiler rules).
- [ ] T008 — Flag IMGUI off; testes; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (focus order, bindings)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (mandatório para UI)
- Requires regression test: YES (ShopCanvas equivalente; modal guard)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + checklist humano das 4 telas com as regras canônicas

## Definition of Done

```text
4 telas Canvas com foco/empty states/regras canônicas; IMGUI atrás de flag; builder
compartilhado sem regressão de shop; builds 0E; report.
```

## Anti-regressão

```text
WASD bloqueado com modal aberto (regra). Esc fecha topo do stack. Shop/dialogue Canvas
intactos. Skill point nunca gasto sem confirmação. Quest log não revela oculto.
```

## Notas para execução posterior

```text
As 7 telas restantes das specs 04_ devem reusar RuntimeUiBuilder/UiFocusController.
Gamepad: 04_spec_ui_menu_gamepad_navigation_future.
Tema visual/arte: fase de arte.
```


---

## EMENDA 2026-06-12 (pós-canonização dos catálogos — VINCULANTE)

```text
1. PAINEL ÚNICO DE 8 ABAS (decisão Q11.2): Inventário/Equipamento/Skills/Quests/Bestiário/
   Calendário/Mapa/Sistema — Tab cicla, atalhos diretos abrem na aba; as 4 telas desta spec
   viram as 4 primeiras ABAS (não janelas separadas). Abas Bestiário/Calendário/Mapa nascem
   como placeholder "em breve" (F20/F38/F21 preenchem).
2. RESPONSIVIDADE (decisão Q6.5): TODA âncora em % via CanvasScaler; caixas de diálogo
   60-80% da largura; HUD_LAYOUT_SCENES §1-2 vence em posições.
3. Quest log com abas internas Main/Side/Contratos/Secretas (QUEST_CATALOG §14).
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
1. PAINEL DE 9 ABAS (7.7): adicionar aba SOCIAL (relacionamentos/amizades — consome F26;
   romance F46 quando existir). Ordem: Inventário/Equipamento/Skills/Quests/Social/Bestiário/
   Calendário/Mapa/Sistema.
2. BUSCA TEXTUAL no inventário (7.7-A): campo de filtro na aba Inventário.
```


---

## EMENDA 2026-06-13-V3 (Refinamento v3 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v3.0.md, decisões 4.1 e 4.4)

```text
1. PAUSA DO RELÓGIO DO DIA COM UI ABERTA (decisão 4.1 — padrão Stardew). O GameTimeManager
   PAUSA o relógio do dia sempre que QUALQUER modal/painel/diálogo/loja estiver aberto. A pausa
   deve ser controlada por UM GATE ÚNICO (não dois caminhos paralelos): o gate é alimentado pelo
   estado do ModalManager (HasActiveModal) — quando o stack tem ao menos um modal/painel/diálogo/
   loja, o relógio congela; ao esvaziar, retoma.
   - Esta decisão RESOLVE a duplicidade hoje existente entre MenuManager e PauseMenuController:
     em vez de cada um pausar o tempo por conta própria, ambos passam a contribuir para o MESMO
     gate único de pausa do GameTimeManager. Não criar um segundo mecanismo de pausa.
   - O painel único (esta spec / suas abas), o diálogo/loja (ShopCanvas) e as confirmações modais
     (incluindo as da F56) entram no mesmo gate. Abrir qualquer um congela o dia; fechar todos
     retoma.
   - Anti-regressão: com o painel/loja/diálogo aberto, o tempo de jogo não avança; ao fechar o
     topo do stack até esvaziar, o tempo retoma do ponto exato (sem salto).
   - Testing: cobrir em EditMode a lógica do gate (modal aberto → tempo pausado; stack vazio →
     tempo corre) com o ModalManager/contadores abstraídos onde necessário; o "feel" da pausa
     fica no cenário humano do lote.
   NOTA DE ESCOPO: a borda exata entre esta spec (que constrói o painel/abas) e a F56 (que liga o
   gate aos caminhos de save/quit) é decidida na Fase 0 — o GATE pertence ao GameTimeManager e é
   compartilhado; ambas as specs apenas o alimentam.

2. ABA SISTEMA v1 — CONTEÚDO CANÔNICO (decisão 4.4). A aba Sistema do painel (8ª/9ª aba) tem,
   no v1, EXATAMENTE:
   - volumes Master / SFX / Música, persistidos (UI state, fora do save de gameplay — consumidos
     pelo AudioManager da F58 via GameAudioSettings);
   - toggle Fullscreen / Windowed;
   - dropdown de resolução.
   Nada mais (sem dificuldade — ver ADR-0014; sem rebind de teclas — ver ADR-0013; sem idioma).
   - A IMPLEMENTAÇÃO do conteúdo da aba Sistema (Salvar/Carregar/Sair + estes controles de
     volume/vídeo) é OWNED PELA F56 (fable_56). Esta spec apenas reserva a aba e o ponto de
     registro no painel; F56 preenche. Os três volumes (Master/SFX/Música) substituem o "slider
     placeholder único" citado nas versões anteriores da F56 — ver emenda V3 da F56.
```
