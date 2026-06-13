# SPEC — UI: Minimapa v1 (Town/Farm estático + Cave fog-of-war)

> **Spec ID:** `fable_38_spec_minimap_v1_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 8
> **Priority:** P2
> **Type:** UI / Runtime
> **Domain:** UI / World
> **Parallelizable:** NO (HUD canvas lock — última do plano)
> **Parallel group:** N/A (lock do HUD canvas)
> **Can run with:** N/A
> **Must not run with:** F14, F20 (mesmo canvas/builder)
> **Repo lock scope:** GameplayHudCanvas, RuntimeUiBuilder
> **Depends on:**
> - F14 (builder/canvas), F20 (posição sob o relógio), F09 (tamanhos de nível)
> **Blocks:** N/A
> **Scope:** minimapa HUD (decisão Q11.1: entra na v1) + aba Mapa do painel único.
> **Out of scope:** mapa-múndi, marcadores custom do player, render de sprites reais (shapes/cores).

required_adrs: [ADR-0005]
required_game_rules: [ui_rules.md, cave_rules.md]

---

# /speckit.specify

## Contexto

Decisão Q11.1: o minimapa entra na v1, em local claro. HUD_LAYOUT_SCENES §2 fixa a
posição: canto superior direito, sob o widget de relógio/data/clima/lua (F20), ancorado
em % da tela (regra-mãe da direction: nenhuma posição em pixels absolutos). A direction
§minimapa define o comportamento por cena: na caverna, fog-of-war por células visitadas
que persiste por nível DENTRO da run (contrato stable-run — revisitar mostra o que já
viu); em Town/Farm, planta fixa simplificada sem fog. O minimapa NÃO mostra inimigos
(a leitura de combate pertence ao palco).

Objetivo: widget 160×160px (referência — ancorado em %) com render procedural barato
(Texture2D pintada por célula, sem segunda câmera): Town/Farm = layout estático
(posições conhecidas dos geradores: prédios/NPCs/player); Cave = fog-of-war por células
visitadas (grid do nível atual), entrada/saída/escada quando descobertas, player sempre
centrado; e a aba Mapa do painel único (F14) renderiza a versão expandida.

## Problema

Sem minimapa, o jogador navega a caverna procedural às cegas (voltar à escada ou ao
mercador errante exige memorização) e a decisão Q11.1 fica descumprida. Se o fog for
implementado com save global próprio, viola o contrato FASE9F/ADR-0005 (o estado de
nível do stable-run é o dono do que persiste dentro da run) e cria estado duplicado.
Se o render for por câmera secundária ou atualizado por frame, o custo é
desproporcional para uma UI de leitura.

## Objetivo

Ao final desta spec, o projeto deve ter um MinimapWidget no HUD (Town/Farm estático,
Cave com fog-of-war intra-run guardado no estado de nível do stable-run), com update
throttled a 4×/s, toggle pela tecla M, hook da lantern_of_true_sight (raio 6→9) e a aba
Mapa (F14) expandida — sem criar segunda câmera nem save global novo.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/ui_ux/HUD_LAYOUT_SCENES_DIRECTION_v1.0.md (§2, §minimapa)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§11)
.claude/rules/cave-stable-run.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- GameplayHudCanvas (canvas do HUD — F14);
- RuntimeUiBuilder + aba Mapa placeholder do painel único (F14);
- grid do nível de caverna (CaveLevel layout determinístico — stable-run ADR-0005);
- posições de cena conhecidas pelos geradores (prédios/NPCs/board/entrada da caverna);
- tamanhos de nível 42/55/65 (F09 — o renderer deve cobrir os 3);
- estado de nível do stable-run (dono da persistência intra-run).
Não existe:
- minimapa (widget, renderer);
- fog-of-war (revelação por células visitadas);
- render procedural de células.
Auditar Fase 0:
- como obter o grid do nível atual (API do cave runtime) e o ponto correto do estado de
  nível existente para guardar células reveladas (contrato FASE9F).
```

## Engineering stories

```text
Como jogador na caverna, quero ver as células que já visitei, para navegar de volta à
  escada sem memorizar o layout.
Como contrato stable-run (ADR-0005), quero o fog guardado no estado de nível existente,
  para revisitar um nível e ver exatamente o que já revelei — sem save global novo.
Como HUD, quero render procedural throttled (4×/s), para o minimapa não custar frame.
Como jogador na cidade, quero a planta fixa com NPCs marcados, para achar quem procuro.
```

## Escopo

```text
Inclui:
- MinimapWidget (RawImage + Texture2D procedural, update 4×/s — não por frame):
  células pintadas por tipo (chão/parede/água/hazard placeholder colors);
- fog-of-war na caverna: célula revelada num raio 6 do player; reveladas persistem
  DENTRO da run (CaveRunSeed) — guardado no estado de nível existente do stable-run
  (NÃO novo save global; revisitar nível mantém revelado — contrato FASE9F);
- Town/Farm: mapa estático completo (sem fog), pontos: player (branco), NPCs (amarelo),
  board/mural (azul), entrada da caverna (vermelho);
- Cave: player centrado, mapa rola; ícones: escada desc/sub quando célula revelada,
  mercador errante quando spawnado e revelado;
- toggle M: mostra/oculta widget; aba Mapa (F14) renderiza versão 2× com legenda;
- lantern_of_true_sight (F31): raio de revelação 6→9 (hook);
- EditMode tests: reveal por raio, persistência intra-run, mapeamento célula→cor,
  centragem/rolagem (matemática pura).
```

## Fora de escopo

```text
Não inclui:
- mapa-múndi (região de Dornécia — pós-v1);
- marcadores custom colocados pelo player;
- render de sprites reais (v1 usa shapes/cores placeholder);
- inimigos no minimapa (decisão da direction: leitura de combate é do palco);
- arte final do minimapa (fase de arte).
```

## Regras de não duplicação

```text
Não criar câmera segunda — render procedural via Texture2D, não RenderTexture de câmera.
Não criar save novo — o estado de nível do stable-run é o dono do fog intra-run.
Não criar segundo canvas/builder — GameplayHudCanvas + RuntimeUiBuilder (F14) são a base.
Não duplicar o grid do nível — ler a API do cave runtime, nunca reconstruir layout.
Não criar segundo sistema de toggle/atalho — usar o roteamento de input existente.
```

## Critérios de aceite

### CA-1 — Fog intra-run

- A caverna nasce escura no minimapa; andar revela células num raio 6; sair e voltar ao
  mesmo nível na MESMA run mantém o revelado (estado de nível do stable-run).
- Evidência: EditMode tests de reveal/persistência + cenário humano revisitando nível.

### CA-2 — Reset por nova run

- Nova run (KO/new game — troca de CaveRunSeed) reseta o fog, conforme contrato de seed
  do stable-run (ADR-0005).
- Evidência: EditMode test de reset com mudança de CaveRunSeed.

### CA-3 — Town/Farm estático + toggle + aba Mapa

- Town/Farm mostram layout completo sem fog com os pontos coloridos (player/NPCs/board/
  entrada da caverna); tecla M alterna o widget; a aba Mapa (F14) mostra a versão 2× com
  legenda.
- Evidência: testes de mapeamento célula→cor/pontos + cenário humano nas 3 cenas.

### CA-4 — Throttle

- O update do minimapa roda no máximo 4×/s (sem custo por frame).
- Evidência: EditMode test do throttle (acumulador de tempo determinístico).

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Runtime/
  MinimapWidget.cs                  (NOVO — RawImage/Texture2D, throttle, toggle M)
  MinimapRenderer.cs                (NOVO — renderer puro testável: célula→cor,
                                     centragem/rolagem, fog, ícones)
Assets/_Game/Scripts/Cave/ (fog no estado de nível existente do stable-run — aditivo)
Assets/_Game/Scripts/UI/ (aba Mapa F14 — versão expandida 2× com legenda)
Assets/_Game/Tests/EditMode/UI/
  MinimapTests.cs                   (NOVO)
docs/validation/
  fable_38_spec_minimap_v1_runtime_execution_report.md
```

## Contratos

### Data contracts
Tabela célula→cor (chão/parede/água/hazard — placeholder colors) e tabela de ícones
(player, NPC, board, entrada da caverna, escadas, mercador errante) no renderer puro.

### Runtime contracts
MinimapRenderer é puro (recebe grid + células reveladas + posições → pinta buffer);
MinimapWidget aplica throttle 4×/s, reusa o buffer Texture2D (sem alloc por update),
centra o player na caverna (mapa rola) e usa mapa fixo em Town/Farm. Reveal: células num
raio 6 do player (9 com lantern_of_true_sight — hook F31). Fog lido/escrito EXCLUSIVAMENTE
no estado de nível do stable-run (ADR-0005). Sem GameObject.Find: referências via
builder/bootstrap.

### Event contracts
Nenhum evento novo. Consome eventos existentes de mudança de cena/nível para trocar a
fonte do mapa; toggle M via roteamento de input existente.

### Save contracts
N/A global — o fog persiste APENAS intra-run, dentro do estado de nível existente do
stable-run (mesmo dono, mesma vida útil: reset em new game/KO/regeneração debug).
Nenhuma seção de save nova; nenhuma migração.

### UI contracts
Widget 160×160px de referência ancorado em % (canto sup-direito, sob o relógio F20);
aba Mapa (F14) renderiza 2× com legenda; toggle M mostra/oculta; sem inimigos no mapa.

## Sistemas afetados

```text
HUD (GameplayHudCanvas — novo widget)
Painel único F14 (aba Mapa — versão expandida)
Cave runtime (fog no estado de nível do stable-run — aditivo)
Input (toggle M via roteamento existente)
Itens F31 (lantern_of_true_sight — hook de raio)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/UI/Runtime/** (novos: MinimapWidget, MinimapRenderer)
Assets/_Game/Scripts/UI/** (aba Mapa F14 — aditivo)
Assets/_Game/Scripts/Cave/** (fog no estado de nível — aditivo, contrato FASE9F)
Assets/_Game/Tests/EditMode/UI/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset manual (UI via RuntimeUiBuilder; nada de YAML)
Packages/**
ProjectSettings/**
Geração procedural da caverna (layout/seeds — ler, nunca alterar)
SaveManager / seções de save (fog NÃO é save global)
WeaponDatabase/itens além do hook da lantern (F31)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
API de grid do nível atual; ponto exato do estado de nível do stable-run para o fog;
roteamento de input para a tecla M; aba Mapa placeholder (F14).
### Fase 1 — Renderer puro
MinimapRenderer (célula→cor, centragem/rolagem, fog por raio, ícones) + MinimapTests
da matemática pura.
### Fase 2 — Widget HUD
MinimapWidget (RawImage + buffer reusado, throttle 4×/s, toggle M, posição sob o
relógio) + pontos de Town/Farm.
### Fase 3 — Fog intra-run + lantern
Persistência das células reveladas no estado de nível do stable-run + reset por troca de
CaveRunSeed + hook lantern 6→9; testes de contrato (CA-1/CA-2).
### Fase 4 — Aba Mapa e closeout
Versão 2× com legenda na aba Mapa (F14); csproj; run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Can run with: N/A
- Must not run with: F14, F20 (mesmo canvas/builder — esta é a última do plano no HUD)
- Shared files/systems that require lock: GameplayHudCanvas, RuntimeUiBuilder
- Reason: edita o canvas do HUD e o builder compartilhados com F14/F20; rodar em
  paralelo geraria conflito direto de arquivos de UI.

## Impacto em save/load

```text
Does this change save schema? NO (fog vive no estado de nível intra-run do stable-run)
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
Contrato FASE9F: revisitar nível na mesma run mantém revelado; nova run reseta.
```

## Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: YES (widget consome mudança de cena/nível)
```

## Impacto em UI/Unity

```text
Changes UI: YES (novo widget HUD + aba Mapa expandida)
Changes scenes: NO | Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: alloc de Texture2D por update (GC pressure).
Mitigação: buffer único reusado; níveis 65×65 cabem (4225 px) — pior caso conhecido.
Risco: fog criar save paralelo e violar o stable-run (ADR-0005).
Mitigação: persistência EXCLUSIVA no estado de nível existente + teste de contrato
  (revisita mantém, nova run reseta).
Risco: update por frame degradar performance do HUD.
Mitigação: throttle 4×/s testado (CA-4).
Risco: renderer acoplado a Unity impedir testes.
Mitigação: MinimapRenderer puro (entrada/saída determinística) — widget é casca fina.
Risco: conflito de input da tecla M com modais.
Mitigação: roteamento de input existente (M abre aba Mapa quando painel aberto — F14).
```

## Rollback

```text
Widget desligado (não construído pelo builder) = HUD atual sem minimapa.
Fog aditivo no estado de nível removível sem afetar o stable-run.
Nenhuma seção de save nova para limpar.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar API de grid/estado de nível (ponto do fog) + input M + aba Mapa.
- [ ] T002 — Renderer puro (célula→cor, centragem/rolagem, fog por raio) + testes.
- [ ] T003 — Widget HUD + throttle 4×/s + toggle M + ícones + pontos Town/Farm.
- [ ] T004 — Fog intra-run no estado de nível + reset por CaveRunSeed + lantern hook
        (6→9) + testes de contrato.
- [ ] T005 — Aba Mapa expandida (2× + legenda); csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (fog/render/throttle — matemática pura)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote)
- Requires regression test: YES (stable-run — revisita/reset de seed)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano com fog persistindo
  entre revisitas de nível na mesma run

## Definition of Done

```text
Minimapa v1 nas 3 cenas (Town/Farm estático, Cave fog-of-war raio 6, lantern 9);
fog respeita o stable-run (intra-run no estado de nível, reset por nova run);
toggle M; aba Mapa expandida; sem câmera extra; update throttled 4×/s.
Nenhum arquivo proibido alterado; Builds 0E; run_strict_validation exit 0; report.
```

## Anti-regressão

```text
Contrato stable-run (FASE9F/ADR-0005) intacto: nenhum reroll de layout/seed por causa
do minimapa; fog nunca vira save global.
HUD existente (F14/F20) sem regressão de layout/foco.
Sem GameObject.Find em runtime; refs via builder/bootstrap.
Sem custo por frame (throttle obrigatório); sem alloc por update.
Save schema intacto (nenhuma seção nova).
```
