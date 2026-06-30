# SPEC — Caverna: Armadilhas por Bioma/Tier (subset v1 determinístico)

> **Spec ID:** `fable_60_spec_cave_traps_by_biome_tier`
> **Status:** A implementar
> **Wave:** FABLE Batch 10
> **Priority:** P2
> **Type:** Runtime / Integration
> **Domain:** Cave / Combat
> **Parallelizable:** CONDITIONAL (lock da cadeia de geração da caverna)
> **Parallel group:** fable_batch10_cave
> **Can run with:** F54, F55, F56, F57, F58, F59 (locks disjuntos)
> **Must not run with:** F05, F09, F44 (geração/snapshot/save da caverna)
> **Repo lock scope:** CaveProceduralGenerator/CaveRuntimeMaterializer (pontos de extensão), Cave/Traps/** (novo), VisitedLevelSnapshot/CaveSnapshotService (campos aditivos)
> **Depends on:**
> - F09 (E27 — biomas/layout variety: bioma por nível resolvido)
> - F33 (E21 — bestiário: enemy_hoardmaw p/ trap_false_chest)
> - F01 (executada — status effects canônicos: Poison/Slow/Stun/Chill/Burn)
> **Blocks:** N/A (consumidor do efeito dormante de detecção do amuleto Nyx — F23)
> **Scope:** 10 armadilhas canônicas geradas deterministicamente por bioma/tier com telegraph, dano/status pelos caminhos existentes, desarme por interação e estado estável na revisita.
> **Out of scope:** os demais ~10 tipos do catálogo, pet detection, traps scripted dos níveis 100/101, perda permanente de item, puzzle traps.

required_adrs: [ADR-0005]
required_game_rules: [cave_rules.md, combat_rules.md, event_rules.md]

---

# /speckit.specify

## Contexto

CAVE_LEVEL_GENERATION PARTE H define o sistema completo de armadilhas: 20 tipos
canônicos (§23, tabela TrapId), quantidade por tamanho de nível (§24: Small 0-2 …
Huge 5-10) e por tier (§25: T1 0-2 … T7 4-8), distribuição por bioma (§26) e
counterplay obrigatório (§27: telegraph visual, som/partícula antes de ativar, desarme,
rota alternativa). A própria direction recomenda a spec
`spec_cave_trap_generation_by_biome_tier` — este arquivo é ela, em subset v1.

O subset escolhido (10 tipos, cobrindo todos os biomas v1 do §26):
`trap_spike_floor` (espinhos), `trap_clockwork_dart` (dardos), `trap_spore_pod` (gás
tóxico Poison/Slow), `trap_ice_plate` (gelo/Chill), `trap_loose_rocks` (queda de
pedras), `trap_ember_vent` (fogo/Burn), `trap_rune_lock_pulse` (runa arcana Stun/Slow),
`trap_false_chest` (baú falso → spawna **Hoardmaw**, `enemy_hoardmaw`, CAVE_BESTIARY:
"a razão de se bater no baú antes de abrir"), `trap_root_snare` (raiz/root curto) e
`trap_frost_burst_rune` (estouro frio com windup).

Regras vinculantes herdadas: ADR-0005/cave-stable-run — dentro do mesmo CaveRunSeed, a
revisita de um nível NÃO pode rerolar composição/posições/IDs nem estado (armadilha
desarmada continua desarmada); seeds determinísticos
(CaveWorldSeed + CaveRunSeed + CaveLevel + salt), sem GUID/timestamp. E a F23 deixou um
efeito DORMANTE esperando consumidor: o amuleto de Nyx com "detecção de trap → flag
dormante documentada" — esta spec o liga (detecção revela telegraph à distância).

## Problema

A caverna não tem nenhum risco fora do combate direto: exploração é caminhar e abrir
baú sem leitura, o que contradiz a função das traps (§22: "criar risco fora do combate,
premiar atenção visual, proteger tesouros, reforçar identidade do bioma"). O baú falso —
o contrato de tensão de tesouro do bestiário (Hoardmaw) — não existe. E qualquer
implementação que rolar traps com Random ou rerolar na revisita quebra o contrato
FASE9F (stable run), o invariante mais protegido do projeto.

## Objetivo

Ao final desta spec, cada nível da caverna deve gerar 0-N armadilhas do subset v1 —
quantidade pela tabela §24/§25 (clampada ao subset), tipos pelo pool do bioma §26,
posições em células válidas fora do caminho crítico de entrada/saída — tudo derivado de
seed determinístico (ADR-0005); cada trap tem telegraph visual (e SFX se F58 presente)
antes/durante ativação, aplica dano pelo pipeline existente e status pelos efeitos
canônicos F01; pode ser desarmada por interação (chance por tier de ferramenta);
trap_false_chest aparenta baú e spawna Hoardmaw ao abrir; armadilhas
ativadas/desarmadas persistem no snapshot do nível (revisita não reseta); e o efeito de
detecção do amuleto Nyx (F23) revela traps num raio.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md (PARTE H §22-27)
docs/design/gameplay/cave/CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md (Hoardmaw)
docs/decisions/ADR-0005-cave-stable-run-and-replay.md
.specs/a_implementar/fable/fable_23_spec_accessories_relics_runtime.md (flag dormante)
.claude/rules/cave-stable-run.md
.claude/rules/testing-quality-gate.md
.claude/skills/cave-stable-run-guard/SKILL.md
.claude/skills/rng-and-determinism/SKILL.md
.claude/skills/event-bus-pattern/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- CaveProceduralGenerator / CaveGeneratedLevel / CaveRoom (layout determinístico);
- CaveRuntimeMaterializer (materialização do nível — ponto de extensão das traps);
- CaveBiomeResolver / CaveBiomeDataSO (bioma por nível — F09);
- VisitedLevelSnapshot / CaveSnapshotService / CaveSnapshotCacheManager (estado estável
  da revisita — traps entram AQUI, padrão de inimigos/recursos);
- pipeline de dano (DamageAppliedEvent/PlayerDamagedEvent) e StatusEffectService (F01:
  Poison/Slow/Stun/Chill/Burn/Root);
- spawn de inimigos por ID (CaveEnemySpawner — Hoardmaw entra pelo caminho existente);
- IInteractable (desarme/baú); FarmToolCapabilityResolver (tier de ferramenta — auditar
  reuso p/ chance de desarme);
- efeito dormante de detecção do amuleto Nyx (F23 — flag documentada).
Não existe:
- qualquer código de trap; campo de traps no snapshot; consumidor da flag de detecção.
Auditar Fase 0:
- ponto exato de extensão no CaveRuntimeMaterializer (onde inimigos/recursos
  materializam — traps seguem o MESMO padrão);
- shape do VisitedLevelSnapshot (como inimigos/nós persistem — traps idem, aditivo);
- células válidas/caminho crítico (CavePlayerPathConfinement — traps nunca bloqueiam
  a rota entrada→saída de forma intransponível, §22);
- API real da flag de detecção F23 (nome/local da flag dormante);
- como o jogador "interage para desarmar" (prompt existente) e qual ferramenta/tier
  alimenta a chance (proposta: base 50% + 15%/tier acima do mínimo — documentar).
```

## Engineering stories

```text
Como jogador, quero ler telegraphs e desviar/desarmar armadilhas, para a exploração
  ter risco com counterplay (nunca dano inevitável sem aviso).
Como jogador, quero bater no baú antes de abrir, porque o baú pode ser um Hoardmaw.
Como stable run, quero traps derivadas de seed e estado persistido no snapshot, para
  a revisita do nível ser idêntica (ADR-0005).
Como amuleto de Nyx (F23), quero um consumidor real do efeito de detecção, para a
  relíquia deixar de ser flag dormante.
```

## Escopo

```text
Inclui:
- TrapDefinition (catálogo em código dos 10 tipos v1): trapId, displayName, categoria
  de efeito (dano direto / status / spawn), statusId (F01), dano base por tier,
  telegraphSeconds, disarmable (bool — false p/ false_chest), biomas (§26);
- CaveTrapPlanner (classe pura determinística): (worldSeed, runSeed, level, salt
  "traps") → quantidade pela tabela §24/§25 (clampada ao subset) + tipos do pool do
  bioma (§26) + posições em células válidas (fora do caminho crítico e das âncoras de
  spawn) + trapInstanceIds ESTÁVEIS (hash posicional — sem GUID/timestamp);
- materialização no CaveRuntimeMaterializer (mesmo padrão de inimigos/recursos):
  TrapBehaviour (trigger por proximidade/pisada → telegraph (telegraphSeconds, visual
  via SpriteRenderer/cor; SFX via evento p/ F58) → ativação: dano pelo pipeline
  existente + status via StatusEffectService (F01);
- desarme: IInteractable no estado armado-detectado (prompt "Desarmar") — chance por
  tier de ferramenta (proposta auditada na Fase 0); sucesso = Disarmed permanente;
  falha = ativação imediata (risco/recompensa §22);
- trap_false_chest: aparenta baú (visual de baú do projeto); "abrir" = spawna
  enemy_hoardmaw (caminho de spawn existente) e remove o falso baú; nunca desarmável,
  mas detectável;
- detecção (consumidor F23): com o efeito do amuleto Nyx ativo, traps num raio N
  revelam o telegraph (visual de aviso) antes do trigger;
- snapshot: campos aditivos no VisitedLevelSnapshot — traps {trapInstanceId, trapId,
  cell, state ∈ {Armed, Triggered, Disarmed}}; revisita restaura exatamente
  (Triggered/Disarmed não rearmam dentro do mesmo CaveRunSeed);
- eventos: TrapTriggeredEvent, TrapDisarmedEvent, TrapDetectedEvent (NOVOS — HUD/
  toast/F58 escutam);
- EditMode tests: determinismo do planner (mesmo seed/level = mesmo plano; levels
  distintos = planos distintos), quantidade dentro das faixas §24/§25, pool por bioma
  §26 respeitado, posições fora do caminho crítico, snapshot round-trip de estados,
  chance de desarme por tier (bordas), false_chest → spawn de hoardmaw 1×, IDs
  estáveis (sem GUID).
```

## Fora de escopo

```text
Não inclui:
- os demais ~10 tipos do §23 (rust_cloud, tripwire_bones, lava_crack, mirror_alarm,
  shadow_lantern, void_gravity_snare, blackstone_growth, corrupt_mana_leak,
  collapsing_bridge, anya_echo_seal — follow-up v2);
- traps scripted dos níveis 100/101 (§24 — specs de endgame);
- pet/companion detection (§27 — sistema futuro);
- perda permanente de item (proibido sem spec própria — §22);
- puzzle traps / rota alternativa gerada (layout é da F09);
- arte final (telegraph/trap em placeholder do padrão visual da caverna).
```

## Regras de não duplicação

```text
Não criar segundo caminho de dano/status — pipeline existente + StatusEffectService F01.
Não criar segundo spawner — Hoardmaw entra pelo spawn de inimigos por ID existente.
Não criar segundo snapshot — campos ADITIVOS no VisitedLevelSnapshot/CaveSnapshotService.
Não criar segunda fonte de bioma — CaveBiomeResolver (F09) é a fonte.
Não criar RNG próprio — seed derivado ADR-0005 (StableHash), zero UnityEngine.Random/
GUID/timestamp em conteúdo estável.
```

## Critérios de aceite

### CA-1 Geração determinística por bioma/tier

- Mesmo (worldSeed, runSeed, level) = mesmo plano de traps (tipos/posições/IDs);
  quantidades dentro das faixas §24/§25; tipos sempre do pool do bioma §26; posições
  fora do caminho crítico.
- Evidência: EditMode tests do planner (determinismo/faixas/pool/posições).

### CA-2 Telegraph e efeito com counterplay

- Trap ativa só após telegraph (telegraphSeconds > 0); dano entra pelo pipeline
  existente e status pelos canônicos F01; nenhum tipo do subset causa dano inevitável
  sem aviso.
- Evidência: EditMode tests da máquina de estados (Armed→Telegraph→Triggered) +
  catálogo (todo tipo tem telegraph > 0).

### CA-3 Desarme por ferramenta

- Interagir com trap detectada rola chance por tier de ferramenta (determinística por
  seed da tentativa); sucesso = Disarmed permanente; falha = ativação.
- Evidência: EditMode tests de chance nas bordas + estado resultante.

### CA-4 Baú falso spawna Hoardmaw

- trap_false_chest aparenta baú; abrir spawna enemy_hoardmaw exatamente 1× pelo
  caminho de spawn existente e remove o falso baú (estado Triggered no snapshot).
- Evidência: EditMode test do trigger (spawn request 1×; reentrada não duplica).

### CA-5 Revisita estável (ADR-0005)

- Sair e revisitar o nível no mesmo CaveRunSeed restaura traps idênticas
  (composição/posições/IDs) com estados preservados (Triggered/Disarmed não rearmam).
- Evidência: EditMode test de round-trip do snapshot + plano re-derivado idêntico.

### CA-6 Detecção do amuleto Nyx

- Com o efeito F23 ativo, traps num raio N exibem o aviso de detecção
  (TrapDetectedEvent); sem o efeito, nada muda (flag deixa de ser dormante).
- Evidência: EditMode test com flag sintética ON/OFF.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/Traps/
  TrapDefinition.cs        (NOVO — catálogo em código dos 10 tipos v1)
  TrapState.cs             (NOVO — enum {Armed, Telegraphing, Triggered, Disarmed})
  CaveTrapPlanner.cs       (NOVO — classe pura: seed → plano {tipo, célula, id})
  TrapBehaviour.cs         (NOVO — MonoBehaviour: trigger/telegraph/efeito/desarme)
  FalseChestTrap.cs        (NOVO — variante baú: abrir → spawn hoardmaw)
  TrapDisarmResolver.cs    (NOVO — classe pura: chance por tier, determinística)
  TrapDetectionService.cs  (NOVO — consumidor da flag F23; raio de detecção)
Assets/_Game/Scripts/Core/Events/CaveTrapEvents.cs (NOVO — Triggered/Disarmed/Detected)
CaveRuntimeMaterializer    (ponto de extensão — materializa o plano, padrão inimigos)
VisitedLevelSnapshot/CaveSnapshotService (campos aditivos de traps)
Assets/_Game/Tests/EditMode/Cave/CaveTrapsTests.cs (NOVO)
docs/validation/fable_60_spec_cave_traps_by_biome_tier_execution_report.md
```

## Contratos

### Data contracts

- `TrapDefinition`: 10 entradas estáticas — trapId canônico §23, efeito (dano direto
  e/ou statusId F01: spore_pod→Poison+Slow, ice_plate→Chill, ember_vent→Burn,
  rune_lock_pulse→Stun/Slow, root_snare→Root, frost_burst_rune→Chill+dano), dano base
  por tier (% do hit de comum da banda — BALANCE §7 como teto: nunca acima de hit de
  elite), telegraphSeconds, disarmable, biomas §26.
- `trapInstanceId` estável: hash de (runSeed, level, célula, trapId) — sem GUID.

### Runtime contracts

- `CaveTrapPlanner.Plan(worldSeed, runSeed, level, bioma, tamanho)`: determinístico,
  puro; faixas §24/§25 clampadas ao subset; células válidas fornecidas pelo layout
  (excluindo caminho crítico/âncoras).
- `TrapBehaviour`: Armed → (proximidade/pisada) → Telegraphing (telegraphSeconds,
  visual) → Triggered (dano via pipeline + status via StatusEffectService F01) →
  estado final persistido. Desarme via IInteractable quando detectada/visível.
- `TrapDisarmResolver.Roll(toolTier, trapId, seedDaTentativa)`: determinístico por
  seed; proposta base 50% +15%/tier (Fase 0 confirma com a régua).
- `FalseChestTrap`: interação "abrir" → spawn request de enemy_hoardmaw pelo spawner
  existente, 1× (estado Triggered).
- `TrapDetectionService`: lê a flag/efeito F23 (API auditada) e publica
  TrapDetectedEvent para traps no raio.

### Event contracts

- `TrapTriggeredEvent`, `TrapDisarmedEvent`, `TrapDetectedEvent` (NOVOS — trapId/
  trapInstanceId; HUD/toast/F58 escutam). Dano/status pelos eventos existentes.

### Save contracts

- Campos ADITIVOS no VisitedLevelSnapshot: lista de traps {trapInstanceId, trapId,
  cell(x,y), state} — tipos simples/IDs apenas; snapshot legado sem o campo = traps
  re-derivadas Armed (plano determinístico garante composição idêntica).
- Seed NUNCA muda em ForwardExit/BackExit (regra cave-stable-run).

### UI contracts

- Prompt de desarme via InteractionPrompt existente; avisos via toast/eventos.
  Telegraph é visual de cena (placeholder). Sem tela nova.

## Sistemas afetados

```text
Cave generation/materialização (ponto de extensão)
Snapshot/replay da caverna (campos aditivos — superfície ADR-0005)
Status effects F01 (consumo) e pipeline de dano (consumo)
Spawn de inimigos (Hoardmaw por ID)
Acessórios F23 (efeito dormante ganha consumidor)
Event bus (3 eventos novos)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Cave/Traps/** (tudo novo)
Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs (extensão aditiva)
Assets/_Game/Scripts/Cave/Runtime/{VisitedLevelSnapshot, CaveSnapshotService}.cs
  (campos aditivos de traps)
Assets/_Game/Scripts/Core/Events/CaveTrapEvents.cs (novo)
ponto de leitura do efeito F23 (consumo — arquivo auditado na Fase 0)
Assets/_Game/Tests/EditMode/Cave/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML
Packages/** ; ProjectSettings/**
CaveProceduralGenerator (layout é da F09 — traps usam células prontas, não mudam salas)
StatusEffectService/pipeline de dano (consumo apenas — zero mudança)
CaveEnemySpawner internals (spawn por ID via API existente)
lógica de seed/run (CaveRunManager — LER seeds, jamais alterá-los)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria (cave-stable-run-guard OBRIGATÓRIA)
Ler docs do FASE9F citados na regra cave-stable-run; ponto de extensão do
materializer; shape do snapshot; células válidas/caminho crítico; API da flag F23;
ferramenta/tier p/ desarme; caminho de spawn por enemyId.

### Fase 1 — Planner determinístico
TrapDefinition (10 tipos) + CaveTrapPlanner (faixas §24/§25, pool §26, células, IDs
estáveis) + testes de determinismo/faixas/pool/posições.

### Fase 2 — Comportamento
TrapBehaviour (estados/telegraph/dano/status F01) + TrapDisarmResolver + eventos +
materialização no CaveRuntimeMaterializer + testes da máquina de estados/desarme.

### Fase 3 — Baú falso e detecção
FalseChestTrap (spawn hoardmaw 1×) + TrapDetectionService (flag F23) + testes.

### Fase 4 — Snapshot e fechamento
Campos aditivos no snapshot + round-trip de revisita + CaveReplayValidator conferido;
csproj; run_strict_validation; execution report (com a cláusula stable-run).
```

## Paralelização

- Parallelizable: CONDITIONAL.
- Parallel group: fable_batch10_cave.
- Can run with: F54, F55, F56, F57, F58, F59 (locks disjuntos).
- Must not run with: F05, F09, F44 (geração/snapshot/save da caverna).
- Shared files/systems that require lock: CaveRuntimeMaterializer,
  VisitedLevelSnapshot/CaveSnapshotService, cadeia de seeds.
- Reason: estende a superfície mais protegida do projeto (stable run ADR-0005) —
  serial dentro do grupo cave, após F09/F33 resolvidas.

## Impacto em save/load

```text
Does this change save schema? YES — campos aditivos no snapshot de nível visitado
(traps: id/tipo/célula/estado — tipos simples)
Does this add a save section? NO (snapshot existente)
Does this require migration? NO (snapshot legado sem traps = re-derivação Armed —
composição idêntica garantida pelo plano determinístico)
Does this persist Unity references? NO (IDs/ints)
Regra ADR-0005: revisita no mesmo CaveRunSeed restaura composição/posições/IDs/estado;
seed só muda em new game/KO/regeneração debug.
```

## Impacto em eventos

```text
Adds events: YES — TrapTriggeredEvent, TrapDisarmedEvent, TrapDetectedEvent
Changes existing events: NO
Requires unsubscribe pattern: YES (detecção/HUD assinam; unsubscribe simétrico)
```

## Impacto em UI/Unity

```text
Changes UI: prompts/toasts via fluxos existentes (sem tela nova)
Changes scenes: NO (materialização runtime na CaveScene, como inimigos/recursos)
Changes prefabs: NO (visuais placeholder programáticos, padrão da caverna)
Changes ScriptableObjects/assets: NO (catálogo em código no v1)
Requires Play Mode final validation: YES (lote final)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: quebrar o stable run (reroll na revisita, IDs instáveis).
Mitigação: planner puro re-derivável + IDs por hash posicional + estado no snapshot +
testes de round-trip; CaveReplayValidator conferido no fechamento; skill
cave-stable-run-guard na Fase 0.

Risco: trap bloquear o caminho crítico (progresso aleatoriamente impedido — §22).
Mitigação: células do plano excluem rota entrada→saída e âncoras; teste de posições.

Risco: dano de trap injusto (one-shot sem aviso).
Mitigação: telegraph > 0 obrigatório no catálogo (teste) + dano teto pelo hit de
elite da banda (BALANCE §7).

Risco: false_chest duplicar Hoardmaw (re-trigger na revisita).
Mitigação: estado Triggered persistido + teste de reentrada.

Risco: flag F23 com API diferente da documentada (efeito dormante).
Mitigação: Fase 0 audita; se a flag não existir ainda (F23 não executada), detecção
entra atrás de interface fina com consumidor noop + gap documentado (não bloquear).

Risco: chance de desarme com Random (drift entre máquinas).
Mitigação: TrapDisarmResolver determinístico por seed da tentativa; teste de bordas.
```

## Rollback

```text
Planner retorna plano vazio (flag) = caverna volta exatamente ao estado atual;
campos aditivos do snapshot são inofensivos vazios; eventos sem consumidores são
inertes. Não apagar save real do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: cave-stable-run-guard + auditorias (materializer, snapshot,
        células/caminho crítico, flag F23, spawn por ID, ferramenta de desarme).
- [ ] T002 — TrapDefinition (10) + CaveTrapPlanner determinístico + testes
        (determinismo/faixas §24-25/pool §26/posições/IDs estáveis).
- [ ] T003 — TrapBehaviour (telegraph/dano/status F01) + TrapDisarmResolver + eventos
        + materialização + testes (máquina de estados/desarme).
- [ ] T004 — FalseChestTrap (hoardmaw 1×) + TrapDetectionService (F23) + testes.
- [ ] T005 — Snapshot aditivo + round-trip de revisita + CaveReplayValidator; csproj;
        run_strict_validation; execution report com cláusula stable-run.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (planner, desarme, estados, snapshot — núcleo da spec)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final)
- Requires regression test: YES (stable run: revisita de nível SEM traps legadas
  intacta; inimigos/recursos/boss não afetados)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano pisando/desarmando
  uma trap, abrindo um baú falso (Hoardmaw) e revisitando o nível com estado preservado

## Definition of Done

```text
10 traps canônicas geradas deterministicamente (faixas §24/§25, pools §26, posições
válidas, IDs estáveis sem GUID); telegraph obrigatório antes de todo efeito; dano/status
pelos caminhos existentes (F01); desarme determinístico por tier; baú falso spawna
enemy_hoardmaw 1×; detecção do amuleto Nyx consumida (flag F23 deixa de ser dormante);
revisita estável (snapshot aditivo, ADR-0005); builds 0E; run_strict_validation exit 0;
execution report com cláusula stable-run.
```

## Anti-regressão

```text
Contrato FASE9F intacto: mesma run = mesma caverna (inimigos/recursos/boss/traps);
ForwardExit/BackExit jamais mudam CaveRunSeed.
Zero UnityEngine.Random/GUID/timestamp em conteúdo estável (IDs por hash).
Caminho crítico entrada→saída nunca bloqueado de forma intransponível.
Nenhum dano inevitável sem telegraph (regra §22 + teste de catálogo).
Pipeline de dano/StatusEffectService/spawner sem mudança de contrato (consumo apenas).
Snapshot legado carrega sem erro (traps re-derivadas Armed).
Zero GameObject.Find em runtime; eventos só via GameEventBus.
```
