# SPEC — UI: Aba Sistema + Fluxo de Título (New Game / Continue / Sair)

> **Spec ID:** `fable_56_spec_system_tab_title_flow`
> **Status:** A implementar
> **Wave:** FABLE Batch 10
> **Priority:** P2
> **Type:** Runtime / UI / Integration
> **Domain:** UI / Save
> **Parallelizable:** CONDITIONAL (lock do painel único F14 e do SaveManager)
> **Parallel group:** fable_batch10_ui
> **Can run with:** F54, F55, F58, F59, F60 (locks disjuntos — exceto binding de volume F58, aditivo)
> **Must not run with:** F14, F20, F38, F45 (painel único de abas), F13 (SaveManager)
> **Repo lock scope:** painel único de abas (F14), SaveManager (chamadas públicas), boot/título, ModalManager/input routing
> **Depends on:**
> - F14 (E44 — painel único 8 abas, RuntimeUiBuilder + UiFocusController)
> - F13 (executada — SaveManager/save debt closure)
> **Blocks:** N/A
> **Scope:** conteúdo da aba Sistema (Salvar/Carregar/Volume placeholder/Sair) + tela de título programática mínima + New Game limpando estado.
> **Out of scope:** múltiplos slots e autosave (PENDÊNCIA HUMANA — Fase 0 registra), options completas, rebind de teclas, cutscene/logo de abertura.

required_adrs: [ADR-0013-input-keyboard-mouse-only-v1.md, ADR-0014-single-difficulty-v1.md]
required_game_rules: [ui_modal_rules.md, save_rules.md]

---

# /speckit.specify

## Contexto

HUD_LAYOUT_SCENES §4 (decisão Q11.2) fixa o painel único com OITO abas — "Inventário ·
Equipamento · Skills · Quests · Bestiário · Calendário · Mapa · Sistema" — construído no
padrão F14 (RuntimeUiBuilder + UiFocusController, 100% navegável por teclado, Tab cicla,
Esc fecha). Sete abas têm dono em specs; a aba **Sistema** é a única órfã de spec — e é
também onde vivem as operações de meta-jogo: salvar, carregar, volume e sair.

SAVE_LOAD_FULL_STATE §20 lista como PENDÊNCIA ABERTA DE DECISÃO HUMANA: "Definir se
haverá save slots múltiplos / Definir se haverá autosave / Definir backup visível /
Definir UI futura de save/load". O estado real do código: schema v5, JSON, caminho fixo
`saves/slot_1.json`, escrita segura com .tmp. Ou seja: slot único JÁ é o contrato de
fato — esta spec NÃO pode decidir slots/autosave sozinha; a Fase 0 DEVE registrar a
pergunta ao humano e implementar apenas o v1 mínimo (slot único) até a resposta.

Também não existe fluxo de título: o jogo abre direto em cena de gameplay. UI_UX_FULL
GAMEPLAY e UI_UX_MENU_SCREEN_FLOWS (que VENCEM em regra de UX) pedem fluxos explícitos
de pause/options/save-load/quit. Esta spec substitui o placeholder
`spec_ui_menu_systems_final` do registro de specs.

## Problema

Sem a aba Sistema, salvar/carregar dependem de atalhos debug e o painel de 8 abas fica
incompleto (7/8); sem tela de título, não há New Game real (estado de save anterior vaza
para "novo jogo") nem Continue explícito, e "sair do jogo" mata o processo sem fluxo. O
risco específico: implementar slots/autosave por conta própria violaria a pendência
humana declarada no SAVE_LOAD §20; e um New Game que não limpa estado produz o pior bug
de save possível (mundo novo com progresso velho).

## Objetivo

Ao final desta spec, a aba Sistema do painel único (F14) deve oferecer: Salvar (slot
único, feedback de sucesso/erro), Carregar (com confirmação — progresso não salvo se
perde), Volume (slider placeholder persistido localmente — consumido pelo AudioManager
F58 se existir), e Sair para o Título (com confirmação). Deve existir uma tela de título
programática mínima (padrão de construção F14): New Game (limpa estado em memória e
inicia run nova SEM apagar o arquivo de save até o primeiro save explícito — decisão
segura), Continue (visível só se `slot_1.json` existe; carrega e entra), e Sair (fecha o
jogo). A Fase 0 registra a pendência de slots/autosave como pergunta humana no report.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/ui_ux/HUD_LAYOUT_SCENES_DIRECTION_v1.0.md (§4 — oito abas, padrão F14)
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md (pause/save-load/quit)
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md (fluxos — vence em UX)
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md (§20 pendências humanas)
.claude/rules/testing-quality-gate.md
.claude/skills/ui-modal-stack/SKILL.md
.claude/skills/save-load-pattern/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- painel único de abas (F14 — RuntimeUiBuilder/UiFocusController; aba Sistema é a 8ª);
- SaveManager (schema v5, slot_1.json, .tmp seguro, migrations) — chamadas públicas;
- ModalManager/ModalBase + input routing (GameplayInputRouter, InputFocus contracts);
- PauseMenuController (auditar: relação com a aba Sistema — não duplicar caminhos);
- SceneTransitionRouter (transições entre cenas);
- GameLoadedEvent / GameSavedEvent (feedback);
- NotificationToastController (feedback de salvar).
Não existe:
- conteúdo da aba Sistema; tela de título; fluxo New Game/Continue; confirmações de
  carregar/sair; volume persistido.
Auditar Fase 0:
- OBRIGATÓRIO: registrar a pendência SAVE_LOAD §20 (slots/autosave) como pergunta ao
  humano no report — implementar SOMENTE slot único até decisão;
- como o painel F14 registra abas (API de registro de conteúdo por aba);
- PauseMenuController: o que ele já oferece (resume/quit?) — a aba Sistema absorve ou
  coexiste? (decidir pelo menor caminho; documentar);
- boot atual: qual cena inicia e onde GameBootstrap injeta — ponto de entrada do título
  (cena própria via gerador OU overlay de boot na cena inicial — decidir na Fase 0);
- o que constitui "limpar estado" de New Game (managers com estado estático/runtime —
  inventário, tempo, quests, flags) — mapear superfícies de reset.
```

## Engineering stories

```text
Como jogador, quero salvar e carregar pela aba Sistema com confirmação, para controlar
  meu progresso sem teclas debug.
Como jogador, quero abrir o jogo numa tela de título com New Game/Continue, para começar
  do zero ou retomar com intenção explícita.
Como sistema de save, quero New Game limpando o estado em memória, para um jogo novo
  nunca herdar progresso do save anterior.
Como F58 (áudio), quero um valor de volume persistido e exposto, para o AudioManager
  consumir sem acoplamento.
```

## Escopo

```text
Inclui:
- aba Sistema no painel F14 com 4 entradas navegáveis por teclado:
  - Salvar: SaveManager.Save (slot único) + toast de sucesso/erro (GameSavedEvent);
  - Carregar: confirmação modal ("progresso não salvo será perdido") → load + transição;
    desabilitada se não há arquivo;
  - Volume: slider 0-100 placeholder, persistido FORA do save de gameplay
    (PlayerPrefs — UI state não é gameplay state, SAVE_LOAD §19); exposto via
    GameAudioSettings estático p/ F58;
  - Sair para o Título: confirmação modal → volta ao título (estado de gameplay
    descartado com aviso);
- tela de título programática mínima (decisão Fase 0: cena própria via gerador OU
  overlay de boot — padrão de construção F14, sem YAML manual):
  - New Game: limpa estado em memória (reset das superfícies mapeadas na Fase 0) e
    inicia na cena inicial; NÃO apaga slot_1.json até o primeiro save explícito;
  - Continue: visível/habilitado só se o save existe; carrega e entra;
  - Sair: Application.Quit (e stop no editor);
- registro da pendência humana (slots/autosave — SAVE_LOAD §20) no execution report;
- EditMode tests: lógica de disponibilidade (Continue/Carregar com/sem arquivo —
  abstração de IO testável), reset de New Game (superfícies limpas), persistência do
  volume, confirmações exigidas (state machine do fluxo).
```

## Fora de escopo

```text
Não inclui:
- múltiplos slots, autosave, backup visível (PENDÊNCIA HUMANA — SAVE_LOAD §20);
- options completas (gráficos, rebind de teclas, idioma);
- arte/logo/música de título (placeholder programático);
- delete de save pela UI (decisão junto com slots);
- pause menu rework (PauseMenuController auditado, não reescrito sem necessidade).
```

## Regras de não duplicação

```text
Não criar segundo painel/stack de UI — aba Sistema entra no painel F14 existente.
Não criar segundo caminho de save/load — somente chamadas públicas do SaveManager.
Não duplicar quit/resume do PauseMenuController — Fase 0 decide absorver ou delegar.
Não persistir volume no save de gameplay — UI state separado (SAVE_LOAD §19).
Não criar segundo router de transição — SceneTransitionRouter existente.
```

## Critérios de aceite

### CA-1 Aba Sistema completa e navegável

- A 8ª aba do painel oferece Salvar/Carregar/Volume/Sair, 100% navegável por teclado
  (padrão F14), com Esc fechando o painel e input de gameplay bloqueado enquanto aberto.
- Evidência: EditMode tests da projection/estado + cenário humano do lote.

### CA-2 Salvar/Carregar com confirmação

- Salvar persiste e dá feedback; Carregar exige confirmação e restaura o estado salvo;
  Carregar desabilitado sem arquivo.
- Evidência: EditMode tests (disponibilidade, state machine de confirmação) + cenário
  humano salvar→alterar→carregar.

### CA-3 New Game limpa estado

- Após jogar e voltar ao título, New Game inicia mundo novo SEM herdar inventário,
  ouro, tempo, quests ou flags da sessão anterior; o arquivo de save anterior permanece
  intacto até o primeiro save explícito.
- Evidência: EditMode test de reset das superfícies mapeadas + cenário humano.

### CA-4 Continue condicional

- Continue só aparece/habilita com save existente; carregar pelo título equivale ao
  Carregar da aba (mesmo caminho de código).
- Evidência: EditMode test com IO abstraído (com/sem arquivo).

### CA-5 Pendência humana registrada

- O execution report contém a pergunta de slots/autosave (SAVE_LOAD §20) marcada como
  decisão humana pendente, e nenhuma das duas features foi implementada.
- Evidência: seção do report + diff sem código de slots/autosave.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/UI/System/
  SystemTabController.cs        (NOVO — conteúdo da aba Sistema no painel F14)
  SystemTabViewModel.cs         (NOVO — projection testável: disponibilidade/confirm)
  GameAudioSettings.cs          (NOVO — volume 0-100 via PlayerPrefs; API p/ F58)
Assets/_Game/Scripts/UI/Title/
  TitleScreenController.cs      (NOVO — New Game/Continue/Sair; padrão F14)
  NewGameStateResetService.cs   (NOVO — reset das superfícies mapeadas na Fase 0)
Assets/_Game/Scripts/Editor/SceneCreation/
  CreateTitleScene.cs           (NOVO — SE a Fase 0 decidir cena própria; senão overlay
                                 no boot da cena inicial)
Assets/_Game/Tests/EditMode/UI/SystemTabTitleFlowTests.cs (NOVO)
docs/validation/fable_56_spec_system_tab_title_flow_execution_report.md
```

## Contratos

### Data contracts

- `GameAudioSettings`: MasterVolume (0-100, default 80) em PlayerPrefs
  (`ch_volume_master`) — fora do GameSaveData.
- Sem mudança no schema de save (slot único existente; nenhum campo novo).

### Runtime contracts

- `SystemTabViewModel`: estados {CanSave, CanLoad, ConfirmPending(action)} — lógica
  pura testável; IO de existência de save abstraído (ISaveFileProbe).
- `TitleScreenController`: Continue → mesmo caminho de load da aba; New Game →
  NewGameStateResetService.ResetAll() → cena inicial; nunca deleta slot_1.json.
- `NewGameStateResetService`: ponto único de reset (superfícies mapeadas na Fase 0);
  idempotente; usado SÓ no fluxo New Game/voltar ao título.
- Sair para o Título: descarta estado em memória via o mesmo ResetAll (ponto único).

### Event contracts

- Consome: GameSavedEvent/GameLoadedEvent (feedback). Sem evento novo obrigatório;
  se necessário, TitleFlowStateChangedEvent (documentar). Tudo via GameEventBus.

### Save contracts

- Nenhum campo novo. Chamadas públicas do SaveManager apenas (Save/Load/Exists).
- Volume em PlayerPrefs (UI state ≠ gameplay state — SAVE_LOAD §19).

### UI contracts

- Aba Sistema registrada no painel F14 (API de abas existente); confirmações via
  ModalManager (stack, Esc volta um nível, input de gameplay bloqueado);
  foco/navegação via UiFocusController; empty/disabled states explícitos.

## Sistemas afetados

```text
Painel único F14 (8ª aba registrada)
SaveManager (consumido — sem mudança interna)
ModalManager/input routing (confirmações)
Boot/título (fluxo de entrada novo)
PauseMenuController (auditado — absorvido ou delegado)
F58 áudio (consumidor futuro do GameAudioSettings)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/UI/System/** (novos)
Assets/_Game/Scripts/UI/Title/** (novos)
registro de abas do painel F14 (diff aditivo — arquivo auditado na Fase 0)
Assets/_Game/Scripts/UI/Pause/PauseMenuController.cs (diff mínimo SE Fase 0 decidir delegar)
Assets/_Game/Scripts/Editor/SceneCreation/CreateTitleScene.cs (novo, SE cena própria)
ponto de boot (diff mínimo p/ entrar no título — arquivo auditado na Fase 0)
Assets/_Game/Tests/EditMode/UI/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML (cena SÓ via gerador)
Packages/** ; ProjectSettings/** (inclusive build settings — se a cena de título exigir
  registro em EditorBuildSettings, fazê-lo VIA script editor com evidência, nunca YAML)
SaveManager internals (apenas chamadas públicas; zero mudança de schema)
implementação de slots múltiplos/autosave (pendência humana)
outras abas do painel F14
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria e pendência humana
Registrar pergunta slots/autosave (SAVE_LOAD §20) no report; API de abas do F14;
PauseMenuController (absorver × delegar); ponto de boot e decisão cena própria ×
overlay; mapa de superfícies de reset do New Game.

### Fase 1 — Aba Sistema
SystemTabViewModel + SystemTabController + GameAudioSettings + confirmações via
ModalManager + testes (disponibilidade/confirmação/volume).

### Fase 2 — Título e New Game
TitleScreenController + NewGameStateResetService + (CreateTitleScene OU overlay) +
Continue condicional + testes (reset/IO abstraído).

### Fase 3 — Fechamento
Sair para o Título ligado ao ResetAll; regeneração/evidência se cena nova; csproj;
run_strict_validation; execution report com a pendência humana registrada.
```

## Paralelização

- Parallelizable: CONDITIONAL.
- Parallel group: fable_batch10_ui.
- Can run with: F54, F55, F59, F60; F58 (binding de volume é aditivo — coordenar).
- Must not run with: F14, F20, F38, F45 (painel de abas), F13 (SaveManager).
- Shared files/systems that require lock: registro de abas F14, SaveManager (consumo),
  boot/título, ModalManager.
- Reason: estende o painel compartilhado F14 e o fluxo de boot — superfícies que as
  demais specs de UI também tocam.

## Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
Regra crítica: New Game NÃO deleta slot_1.json (preservar até save explícito);
volume em PlayerPrefs, fora do GameSaveData.
```

## Impacto em eventos

```text
Adds events: NO (preferência; se necessário, TitleFlowStateChangedEvent documentado)
Changes existing events: NO
Requires unsubscribe pattern: YES (controllers assinam GameSavedEvent/GameLoadedEvent)
```

## Impacto em UI/Unity

```text
Changes UI: YES — 8ª aba + tela de título (programáticas, padrão F14)
Changes scenes: POSSIBLE — cena de título via gerador (decisão Fase 0; evidência)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES (lote final)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: implementar slots/autosave sem decisão humana (violação SAVE_LOAD §20).
Mitigação: Fase 0 registra a pergunta; escopo trava em slot único; report evidencia.

Risco: New Game herdando estado (estáticos/singletons não resetados).
Mitigação: mapa de superfícies na Fase 0 + ponto único ResetAll + teste de reset.

Risco: New Game apagando save do jogador.
Mitigação: regra explícita — nunca deletar slot_1.json; teste garante arquivo intacto.

Risco: cena de título exigir EditorBuildSettings (ProjectSettings proibido).
Mitigação: decisão Fase 0 pende para overlay de boot se o registro de cena exigir
superfície proibida; se cena própria, registro só via script editor com evidência e
autorização (permissions.ask).

Risco: duplicar caminhos de quit/save com o PauseMenuController.
Mitigação: auditoria Fase 0 — absorver ou delegar, nunca dois caminhos.

Risco: Carregar sem confirmação destruir progresso não salvo.
Mitigação: confirmação modal obrigatória testada na state machine.
```

## Rollback

```text
Remover a aba (painel volta a 7 abas funcionais) e o fluxo de título (boot volta a
entrar direto em gameplay). Nenhum schema alterado; PlayerPrefs de volume inofensivo.
Não apagar save real do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: pendência humana (slots/autosave) registrada + auditorias (API de
        abas F14, PauseMenu, boot, mapa de reset).
- [ ] T002 — Aba Sistema: viewmodel + controller + volume (PlayerPrefs) + confirmações
        + testes.
- [ ] T003 — Título: controller + NewGameStateResetService + Continue condicional +
        (cena via gerador OU overlay) + testes.
- [ ] T004 — Sair para o Título + integração fim-a-fim; csproj; run_strict_validation;
        execution report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (disponibilidade, state machine de confirmação,
  reset de New Game, persistência de volume)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final — UI-heavy:
  open/close, Esc, input blocking, focus, confirmação, empty/disabled states)
- Requires regression test: YES (7 abas existentes + save/load atuais intactos)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano: título → New Game
  → jogar → salvar → título → Continue → estado restaurado

## Definition of Done

```text
Aba Sistema (Salvar/Carregar/Volume/Sair) navegável por teclado no painel F14 com
confirmações modais; título mínimo com New Game (estado limpo, save preservado),
Continue condicional e Sair; pendência humana de slots/autosave registrada e NÃO
implementada; zero mudança de schema; builds 0E; run_strict_validation exit 0; report.
```

## Anti-regressão

```text
As 7 abas existentes do painel F14 intactas (registro aditivo).
SaveManager sem mudança interna/schema; save legado carrega igual.
New Game jamais deleta slot_1.json; jamais herda estado em memória.
ModalManager stack/Esc/input blocking preservados (ui_modal_rules).
PauseMenuController sem caminho duplicado de save/quit.
Zero GameObject.Find em runtime; eventos só via GameEventBus.
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
1. CRIAÇÃO DE PERSONAGEM no New Game (1.1/1.2/1.3): nome (campo de texto), apresentação
   M/F/Neutro, tints placeholder de cabelo/pele. Persistir em PlayerSaveData (campos aditivos).
2. SLOTS (7.4 — fecha SAVE_LOAD §20): 3 SLOTS MANUAIS (não slot único) + BACKUP ROLLING do
   último save bom a cada gravação. A Fase 0 não precisa mais perguntar — decisão tomada.
3. Identidade do produto (7.1): productName "Cindar's Hope", versão 0.1.0; companyName a
   confirmar na Fase 0 (default provisório aceito).
```


---

## EMENDA 2026-06-13-V3 (Refinamento v3 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v3.0.md, decisões 4.8 e 4.4)

```text
1. SAVE CORROMPIDO → RESTAURAÇÃO AUTOMÁTICA DO BACKUP ROLLING (decisão 4.8). Quando o load de um
   save falhar (arquivo corrompido/ilegível/schema irrecuperável), o fluxo de Carregar — tanto
   pela aba Sistema quanto pelo Continue do título — DEVE oferecer AUTOMATICAMENTE o backup
   rolling do último save bom, com mensagem clara ao jogador, ex.: "Save danificado — restaurar
   backup de <data>?".
   - Pré-requisito: o BACKUP ROLLING do último save bom já é decisão tomada na EMENDA 2026-06-12-D
     item 2 (3 slots manuais + backup rolling a cada gravação). Esta emenda define o COMPORTAMENTO
     DE RECUPERAÇÃO quando o save principal falha.
   - Fluxo:
     a) tentar carregar o save selecionado (slot);
     b) se o load falhar, NÃO travar nem cair em estado inconsistente: detectar a falha, manter o
        arquivo corrompido intacto (não sobrescrever), e apresentar uma confirmação modal
        oferecendo restaurar o backup rolling daquele slot, exibindo a DATA do backup;
     c) se o jogador aceitar: carregar a partir do backup rolling (mesmo caminho público do
        SaveManager) e entrar no jogo;
     d) se recusar (ou se NÃO houver backup): mensagem clara de que o save não pôde ser carregado;
        permanecer no título/aba Sistema sem destruir nada.
   - A detecção de "load falhou" e a existência/data do backup vêm do SaveManager (chamadas
     públicas / probe de arquivo abstraído — ISaveFileProbe já previsto na spec). NÃO reimplementar
     leitura de save; consumir o que o SaveManager expõe (estender a API pública do SaveManager
     SOMENTE se necessário para sinalizar "load falhou" e "backup disponível em <data>", sem mudar
     schema).
   - Anti-regressão: o save corrompido NUNCA é apagado nem sobrescrito automaticamente; a
     restauração do backup é sempre confirmada pelo jogador; nenhum schema novo.
   - Testing (EditMode): state machine de recuperação — {load OK} / {load falhou + backup existe →
     oferta de restauração} / {load falhou + sem backup → mensagem de erro, nada destruído};
     confirmação obrigatória antes de restaurar; IO abstraído (sucesso/corrompido/backup-presente/
     backup-ausente). A audição/feel fica no cenário humano do lote.
   NOVO CA:

   ### CA-6 Recuperação de save corrompido via backup rolling
   - Ao falhar o load (aba Sistema ou Continue do título), o jogo oferece automaticamente
     restaurar o backup rolling do último save bom, com a data exibida; aceitar restaura e entra;
     recusar/ausência de backup → mensagem clara, save corrompido preservado (não apagado).
   - Evidência: EditMode test da state machine de recuperação (com IO abstraído) + cenário humano
     (corromper save de teste → abrir → oferta de backup → restaurar).

2. ABA SISTEMA v1 = TRÊS VOLUMES + VÍDEO (decisão 4.4 — substitui o "slider de volume placeholder
   único"). O controle de áudio da aba Sistema passa a ser TRÊS volumes — Master / SFX / Música —
   persistidos (PlayerPrefs, UI state fora do GameSaveData), além de toggle Fullscreen/Windowed e
   dropdown de resolução. Nada além disso no v1 (sem dificuldade — ADR-0014; sem rebind — ADR-0013;
   sem idioma).
   - GameAudioSettings (já previsto nesta spec) passa a expor os TRÊS volumes (Master/SFX/Música),
     não apenas Master, para a F58 consumir por canal. Chaves PlayerPrefs sugeridas:
     ch_volume_master / ch_volume_sfx / ch_volume_music (defaults conservadores).
   - O toggle Fullscreen/Windowed e o dropdown de resolução também são UI state (PlayerPrefs /
     Screen API), fora do save de gameplay; aplicar via Screen.* sem tocar ProjectSettings.
   - Anti-regressão: nada disso entra no schema de save; o painel continua navegável por teclado
     (padrão F14); a borda com F14 (que reserva a aba) permanece — esta spec preenche o conteúdo.
   - Testing (EditMode): persistência e leitura dos três volumes + do modo de tela/resolução
     escolhidos (com PlayerPrefs/IO abstraído); aplicação do volume por canal validada na
     projection de ganho da F58 (não nesta spec). Feel no cenário humano.
```
