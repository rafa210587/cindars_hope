# SPEC — Perfis de Arte por Bioma da Cave (dados + materialização visual + evento de bioma)

> **Spec ID:** `spec_cave_biome_art_profiles_runtime`
> **Status:** A implementar
> **Wave:** CAVE_VISUALS — camada de apresentação por bioma
> **Priority:** P1
> **Type:** Runtime / Data / Tooling
> **Domain:** Cave
> **Parallelizable:** NO
> **Parallel group:** N/A
> **Can run with:** specs docs-only
> **Must not run with:** qualquer spec que edite `Assets/_Game/Scripts/Cave/**` ou `CindarsHopeMenu.cs` (ex.: fable_78, spec_codex_13 se pendente)
> **Repo lock scope:** `Assets/_Game/Scripts/Cave/**`, `Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs`, `Assets/_Game/Data/Cave/**`
> **Ordem de execucao:** após o lote CODEX_CONVERGENCE (CX09 — reuso de CaveLayoutStableHash); antes do lote 2 de arte por bioma
> **Depende de:**
> - `docs/design/gameplay/cave/CAVE_BIOME_VISUAL_REFERENCE.md` (direção visual validada 2026-07-03)
> - ADR-0005 (cave stable run) — invariante, não bloqueia
> **Bloqueia:**
> - Lote 2 de arte da cave (tilesets/props por bioma) — o wiring deles usa os profiles desta spec
> - Spec futura de música por bioma (consumidora do evento criado aqui)
> **Scope:** criar a camada data-driven de apresentação por bioma (profiles SO + resolução de sprite/tile determinística nos materializers + passo de geração/validação nos 3 comandos canônicos + evento de mudança de bioma), com fallback integral para os placeholders atuais quando não houver arte.
> **Out of scope:** produzir arte nova; sistema de audio/música; alterar geração procedural/layout; Tilemap de FarmScene/TownScene.

required_adrs: [ADR-0005]
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

Os biomas da cave estão completos mecanicamente (8 biomas em `CaveBiomeRegistrySO`, layout por
banda em `CaveBiomeLayoutProfile`, 10 traps com pool por bioma, 3 hazards de tile, baú de tesouro
e baú falso), mas **não existe camada visual**: chão/parede não usam Tilemap, e trap/baú/hazard
são pintados com `SpriteRenderer.color` hardcoded. A direção visual por bioma foi validada com o
humano e documentada em `docs/design/gameplay/cave/CAVE_BIOME_VISUAL_REFERENCE.md` (paletas,
elementos, keyarts de referência em `art/world_gpt/raw/cave_guides/`).

Esta spec cria o **contrato de dados e o caminho de renderização** para que a arte do lote 2
(tiles/props por bioma) seja plugada sem tocar código de novo. Ela deve rodar HOJE, sem a arte
final: todo campo de sprite/tile é opcional e o fallback é o comportamento placeholder atual.

O que fica para specs futuras: produção da arte por bioma, sistema de música (consome o evento
criado aqui), decor/elementos ambientais do fable_78.

## 6. Problema

Sem uma camada data-driven de apresentação: (1) cada sprite novo de cave exigiria editar
materializer C# (string solta, como hoje `WorldSpriteLibrary.Prop("rock_ore_0")` nos scene
creators); (2) o lote 2 de arte não tem onde ser plugado e viraria wiring manual de YAML;
(3) "cada bioma parecer diferente" (requisito humano) não tem mecanismo — todos os níveis
renderizam igual, mudando só a cor; (4) música por bioma não tem gatilho de troca.

## 7. Objetivo

Ao final desta spec, o projeto deve ter 8 `CaveBiomeArtProfileSO` gerados e resolvidos por banda,
materializers de cave que consultam o profile para tile/sprite (com fallback aos placeholders
atuais), um passo idempotente em `Inicializar Projeto` + validator em `Validar Projeto`, e um
`CaveBiomeChangedEvent` publicado na troca de banda — sem alterar nenhuma decisão de layout,
spawn, loot ou snapshot (stable-run intacto).

## 8. Fontes obrigatórias lidas

```text
docs/design/gameplay/cave/CAVE_BIOME_VISUAL_REFERENCE.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.claude/rules/cave-stable-run.md
.claude/rules/editor-generation-orchestration.md
.claude/rules/testing-quality-gate.md
.claude/skills/tilemap-world-rendering/SKILL.md
.claude/skills/registry-catalog-pattern/SKILL.md
.claude/skills/event-catalog-and-tracing/SKILL.md
```

## 9. Estado atual do repo

```text
EXISTE (não recriar):
- CaveBiomeRegistrySO (Assets/_Game/Scripts/Cave/Data/CaveBiomeRegistrySO.cs) — 8 biomas, níveis 1-100.
- CaveBiomeLayoutProfile (Cave/Generation/) — perfis de layout por banda, hardcoded (NÃO mexer).
- CaveHazardKind (ToxicPool/IceSlick/FallingRock) + planners determinísticos.
- TrapDefinition (10 traps, PoolForBiome) + CaveTrapPlanner + TrapBehaviour + CaveTrapMaterializer.
- TreasureChestInteractable + FalseChestTrap — placeholders via SpriteRenderer.color.
- CaveBandScaling.BandForLevel — resolução nível→banda.
- GameEventBus + CaveLevelEnteredEvent (auditar assinatura exata na Phase 0).
- CindarsHopeMenu.cs — orquestrador dos 3 comandos canônicos (RunStep best-effort).
- WorldSpriteLibrary (Editor) — Load por categoria/nome em Assets/_Game/Art/Generated/World/.
- Arte pronta reutilizável: rock_ore_0..5 em Art/Generated/World/props; 20 tiles ground_* +
  6 TileAssets de grama em Art/Generated/World/tiles + _TileAssets.
- Keyarts/folhas de referência (NÃO são assets de jogo): art/world_gpt/raw/cave_guides/*.png.

NÃO EXISTE:
- Qualquer tile de cave (chão/parede por bioma); sprites de trap/baú/hazard/saída dedicados.
- SO de arte por bioma; evento de mudança de bioma; Tilemap na materialização da cave.

AUDITAR NA PHASE 0 (não assumir):
- Como o materializer atual instancia chão/células (se há grid visual hoje ou só objetos);
- Assinatura de CaveLevelEnteredEvent e onde é publicado;
- Se CaveBiomeRegistrySO é asset gerado (gerador existente em Inicializar Projeto) ou só código;
- Nomes exatos dos materializers de exit/checkpoint/boss gate.
Estado real precisa ser auditado na Phase 0 antes de implementação. Não recriar sistema
existente sem confirmar ausência no repo.
```

## 10. Engineering stories

```text
Como materializer da cave, quero resolver "banda → profile de arte → tile/sprite" numa chamada,
  para nunca hardcodar sprite ou cor por tipo de objeto.
Como pipeline de arte, quero que o lote 2 de tiles vire asset plugado num profile por um passo
  idempotente de Inicializar Projeto, sem editar C#.
Como sistema de música futuro, quero um CaveBiomeChangedEvent com biomeId/banda para trocar
  track sem acoplar audio à cave.
Como guardião do stable-run, quero que variação visual use hash determinístico por célula e
  nunca influencie layout, spawn, loot ou snapshot.
```

## 11. Escopo

```text
Inclui:
- CaveBiomeArtProfileSO (ScriptableObject) com campos OPCIONAIS: floorTiles (TileBase[]),
  wallTile (TileBase), floorDetailTiles, sprites por CaveHazardKind, sprites por trap id
  (lista trapId→Sprite), chestClosed/chestOpen/falseChestRevealed, exitDown/exitUp,
  ambientLightColor+intensity, musicTrackId (string, só dado — sem consumo nesta spec);
- resolução banda→profile: campo novo no CaveBiomeRegistrySO (ou tabela no próprio profile
  registry asset — decidir na Phase 0 pelo caminho de MENOR mudança) + fallback null-safe;
- CaveBiomeArtResolver (C# puro, testável): dado (bandId, kind/id) retorna sprite/tile ou null;
  variação de floor tile por hash determinístico FNV-1a de (worldSeed, runSeed, level, x, y)
  reusando CaveLayoutStableHash (CX09) — proibido System.Random/UnityEngine.Random;
- materialização: onde hoje há SpriteRenderer.color placeholder (trap, chest, false chest,
  hazard tile, exits), consultar o resolver; sprite null → manter EXATAMENTE o placeholder atual;
- chão/parede: se a Phase 0 confirmar que o chão hoje é renderizado por objetos/quàds, criar
  2 Tilemaps (Floor, Wall + TilemapCollider2D/CompositeCollider2D conforme skill
  tilemap-world-rendering) pintados a partir do grid do level plan JÁ GERADO; profile sem tiles
  → manter render atual (nada quebra sem arte);
- CaveBiomeChangedEvent { PreviousBiomeId, BiomeId, BandId, CaveLevel } publicado no fluxo de
  entrada de nível SOMENTE quando a banda difere da anterior (checar duplicação no catálogo de
  eventos antes de criar — skill event-catalog-and-tracing);
- editor: RunStep "Gerar perfis de arte de bioma da cave" em InicializarProjeto (cria/atualiza
  os 8 profiles em Assets/_Game/Data/Cave/Biomes/ via AssetDatabase, idempotente, preenchendo
  o que houver de arte importada por convenção de pasta Art/Generated/World/cave/<biomeId>/);
  RunStep de validação read-only em ValidarProjeto (ValidateCaveBiomeArtProfiles: 8 profiles
  existem, ids batem com registry, campos preenchidos vs. faltantes como WARNING, nunca ERROR
  por arte ausente);
- EditMode tests do resolver (fallback null, determinismo do hash de variação, mapeamento
  banda→profile, evento só em troca de banda — lógica pura).
```

## 12. Fora de escopo

```text
Não inclui:
- produzir/importar arte nova (lote 2 é outra entrega);
- sistema de audio/música (só o campo musicTrackId e o evento);
- decor/elementos ambientais e densidades (fable_78);
- mudar CaveBiomeLayoutProfile, planners, spawn, loot, snapshot ou qualquer decisão de conteúdo;
- reskin por bioma de inimigos (EnemySkinCatalog já cobre);
- Y-sort/iluminação 2D global; balance; validação humana imediata.
```

## 13. Regras de não duplicação

```text
Não criar segundo registry de biomas — estender/consultar CaveBiomeRegistrySO existente.
Não criar novo event bus, novo hash (reusar CaveLayoutStableHash), nem novo loader de sprite
  runtime paralelo ao que os materializers já usam (auditar na Phase 0 como carregam sprites hoje).
Não criar [MenuItem] avulso — só RunStep em CindarsHopeMenu (rule editor-generation-orchestration).
Não duplicar CaveLevelEnteredEvent — CaveBiomeChangedEvent só se não existir equivalente.
```

## 14. Critérios de aceite

### 14.1 Profiles e resolução

- 8 assets `CaveBiomeArtProfileSO` gerados (1 por bioma do registry), idempotente ao re-rodar.
- Resolver retorna null-safe fallback para TODO campo ausente; nenhum NullReference com
  profiles vazios (estado atual do repo, sem arte).
- Evidência: EditMode tests + `ValidateCaveBiomeArtProfiles` OK no log.

### 14.2 Materialização com fallback

- Com profiles vazios, a cave renderiza EXATAMENTE como hoje (placeholder colors) — zero
  regressão visual/funcional.
- Com um sprite/tile de teste preenchido manualmente num profile, o objeto correspondente usa o
  sprite (verificável em Play Mode final; para a spec, verificação estática + teste do resolver).
- Evidência: execution report com diff dos materializers mostrando fallback preservado.

### 14.3 Determinismo (stable-run)

- Variação de floor tile usa exclusivamente hash FNV-1a de (worldSeed|runSeed|level|x|y);
  revisitar o mesmo nível produz visual idêntico; NENHUM dado de snapshot/save novo.
- Evidência: EditMode test de determinismo (mesma seed → mesma sequência de índices).

### 14.4 Evento de bioma

- `CaveBiomeChangedEvent` publicado só quando a banda muda (incl. primeira entrada da run);
  documentado no catálogo de eventos.
- Evidência: EditMode test da lógica de decisão de troca + entrada no catálogo.

### 14.5 Builds e docs

- `dotnet build` runtime e editor exit 0; docs validation sem erros novos.

---

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/Art/
  CaveBiomeArtProfileSO.cs
  CaveBiomeArtResolver.cs          (C# puro)
  CaveBiomeChangedEvent.cs         (ou em Core/Events, seguir convenção auditada na Phase 0)
Assets/_Game/Scripts/Cave/Runtime|Traps/  (edits pontuais: consultar resolver + fallback)
Assets/_Game/Scripts/Editor/Cave/
  GenerateCaveBiomeArtProfiles.cs  (public static, sem MenuItem)
  ValidateCaveBiomeArtProfiles.cs  (read-only)
Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs  (2 RunStep novos)
Assets/_Game/Data/Cave/Biomes/     (8 .asset gerados por editor API)
Assets/_Game/Tests/EditMode/Cave/  (testes novos)
docs/validation/spec_cave_biome_art_profiles_execution_report.md
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
`CaveBiomeArtProfileSO`: string biomeId (deve casar com CaveBiomeRegistrySO); campos de
tile/sprite todos opcionais; musicTrackId string (sem consumo nesta spec).

### 16.2 Runtime contracts
`CaveBiomeArtResolver`: `TryGetFloorTile(band, cellHash, out TileBase)`,
`TryGetTrapSprite(band, trapId, out Sprite)`, `TryGetHazardSprite(band, kind, out Sprite)`,
`TryGetChestSprite(band, chestState, out Sprite)` — todos `bool` + out (padrão csharp-style).

### 16.3 Event contracts
`CaveBiomeChangedEvent { string PreviousBiomeId; string BiomeId; int BandId; int CaveLevel; }` —
publicado por quem já publica a entrada de nível (auditar na Phase 0). Sem mudança em eventos
existentes.

### 16.4 Save contracts
N/A — nada persiste. Visual é derivado por seed; snapshot inalterado (invariante do critério 14.3).

### 16.5 UI contracts
N/A — sem UI nesta spec.

## 17. Sistemas afetados

```text
Cave materialization (visual apenas)
Event bus (1 evento novo)
Editor tooling (2 RunSteps nos comandos canônicos)
Data assets (8 SO novos gerados)
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Cave/**            (novos em Cave/Art/; edits pontuais em materializers/interactables)
Assets/_Game/Scripts/Core/Events/**     (só se a convenção de eventos exigir o evento lá)
Assets/_Game/Scripts/Editor/Cave/**
Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs
Assets/_Game/Data/Cave/Biomes/**        (somente via editor generator, nunca YAML manual)
Assets/_Game/Tests/EditMode/Cave/**
docs/validation/**
docs/design/gameplay/cave/CAVE_BIOME_VISUAL_REFERENCE.md  (só atualizar seção de status)
.specs/** (registry/report desta spec)
```

## 19. Arquivos proibidos

```text
Assets/**/*.unity e Assets/**/*.prefab
Assets/**/*.asset por edição manual de YAML (geração só via AssetDatabase no generator)
Assets/_Game/Scripts/Cave/Generation/** que decida CONTEÚDO (layout/spawn/loot) — leitura ok
Packages/**  ProjectSettings/**  docs_old/**  docs/archive/**
Sistema de audio (qualquer arquivo)
```

## 20. Estratégia de implementação

### Fase 0 — Auditoria (obrigatória, gate de não-duplicação)
Mapear: como o chão é renderizado hoje; como materializers obtêm sprites; assinatura/publicador
de CaveLevelEnteredEvent; se CaveBiomeRegistrySO tem gerador; catálogo de eventos. Registrar no
report. Se descobrir sistema equivalente a profile de arte, PARAR e reportar.
### Fase 1 — Contratos/dados
SO + resolver + evento + testes do resolver (sem tocar materializers).
### Fase 2 — Runtime
Fallback-first: integrar resolver nos pontos de placeholder; Tilemaps só se Fase 0 confirmar
ausência de grid visual; nada muda com profiles vazios.
### Fase 3 — Editor + validação
Generator idempotente + validator + RunSteps; rodar builds, testes, docs validation.
### Fase 4 — Relatório
Execution report + atualização da seção de status no doc de direção visual.

## 21. Ordem segura de execução

```text
1. Fase 0 (audit) → 2. SO/resolver/evento + testes → 3. integração fallback-first nos
materializers → 4. Tilemap (se aplicável) → 5. generator/validator/RunSteps → 6. builds +
EditMode + docs validation → 7. report.
```

## 22. Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Can run with: specs docs-only
- Must not run with: fable_78; qualquer spec tocando Cave/** ou CindarsHopeMenu.cs
- Shared files/systems that require lock: `Assets/_Game/Scripts/Cave/**`, `CindarsHopeMenu.cs`
- Reason: edita materializers centrais da cave e o orquestrador de editor.

## 23. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO (nada persiste)
```

## 24. Impacto em eventos

```text
Adds events: YES — CaveBiomeChangedEvent (1)
Changes existing events: NO
Requires unsubscribe pattern: N/A nesta spec (sem subscriber novo; publicador segue padrão do publicador de nível)
```

## 25. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO (Tilemaps criados em runtime pelo materializer, não salvos em cena)
Changes prefabs: NO
Changes ScriptableObjects/assets: YES — 8 CaveBiomeArtProfileSO gerados via editor API
Requires Play Mode final validation: YES (visual da cave)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos

```text
Risco: variação visual acidentalmente ler estado mutável e quebrar stable-run.
  Mitigação: resolver é puro, entrada só (seeds, level, x, y); teste de determinismo; review
  contra .claude/rules/cave-stable-run.md (skill cave-stable-run-guard na execução).
Risco: regressão visual/funcional nos placeholders atuais.
  Mitigação: fallback-first (critério 14.2); diff mínimo nos materializers.
Risco: Tilemap em runtime conflitar com colisão existente das paredes.
  Mitigação: Fase 0 audita colisão atual; se houver colisão própria, Tilemap fica SEM collider
  nesta spec (só visual) e o collider vira nota para spec futura.
Risco: generator rodar sem arte e poluir profiles com refs quebradas.
  Mitigação: generator só preenche o que existir na convenção de pasta; ausência = campo null +
  WARNING no validator.
```

## 27. Rollback

```text
Remover Cave/Art/**, Editor/Cave/{Generate,Validate}CaveBiomeArtProfiles.cs, os 2 RunSteps e
Assets/_Game/Data/Cave/Biomes/*.asset; reverter edits pontuais dos materializers (fallback é o
código atual). Nenhum save/snapshot afetado.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Fase 0: auditar render atual do chão, fonte de sprites dos materializers,
        CaveLevelEnteredEvent (assinatura/publicador), gerador do CaveBiomeRegistrySO, catálogo de eventos.
- [ ] T002 — CaveBiomeArtProfileSO + resolução banda→profile (menor mudança possível no registry).
- [ ] T003 — CaveBiomeArtResolver (C# puro, TryGet*, hash FNV-1a reusando CaveLayoutStableHash).
- [ ] T004 — CaveBiomeChangedEvent + publicação na troca de banda + entrada no catálogo de eventos.
- [ ] T005 — Integração fallback-first: trap, chest, false chest, hazard tiles, exits.
- [ ] T006 — Tilemap floor/wall no materializer (condicional à Fase 0; sem collider se risco de conflito).
- [ ] T007 — GenerateCaveBiomeArtProfiles (idempotente, convenção Art/Generated/World/cave/<biomeId>/)
        + RunStep em InicializarProjeto.
- [ ] T008 — ValidateCaveBiomeArtProfiles (read-only) + RunStep em ValidarProjeto.
- [ ] T009 — EditMode tests: fallback, determinismo, banda→profile, decisão de troca de bioma.
- [ ] T010 — Builds + docs validation + execution report + seção de status no doc de direção.
- [ ] T011 — FATIA DE TESTE (autorizada pelo humano 2026-07-03): toggle dev-only de bioma fixo
        (forçar banda 1 / biome_stone_cavern em todos os níveis) para testar o pipeline visual —
        guard `#if UNITY_EDITOR || DEVELOPMENT_BUILD`, default OFF, mesmo padrão do CX11; afeta
        SOMENTE a resolução de ARTE (nunca layout/spawn/loot/snapshot); remover ou manter
        documentado como ferramenta Dev após o teste.
```

> **Emenda 2026-07-03 (humano):** prioridade de execução é a fatia de teste do bioma 1 —
> profile da Caverna de Pedra preenchido com a arte de teste (tiles seamless de chão/parede via
> pipeline `art/world_gpt`), toggle T011 ligável manualmente, validando as 3 camadas da
> arquitetura (doc de direção §3.9) de ponta a ponta antes dos outros 7 biomas.

## 29. Validações obrigatórias

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\validate_docs.ps1
.\tools\run_strict_validation.ps1
# Unity Test Runner EditMode — pendência acumulada padrão se Unity estiver bloqueado (registrar NOT RUN + motivo)
```

## 30. Testing Quality Gate

```md
- Changed deterministic logic: YES (resolver + decisão de troca de banda)
- Requires EditMode tests: YES (T009)
- Requires PlayMode automated or final human scenario: YES — cenário humano documentado no report
  (entrar na cave, descer até troca de banda, revisitar nível, conferir visual estável)
- Requires regression test: YES — fallback com profiles vazios (14.2)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: builds exit 0 + EditMode PASS + validator OK +
  Play Mode humano do cenário documentado
```

## 31. Definition of Done

```text
Tasks T001-T010 concluídas dentro dos arquivos permitidos; nenhum proibido tocado; profiles
gerados idempotentes; fallback provado por teste; evento catalogado; strict validation exit 0;
execution report criado; status máximo desta spec: BUILD_VALIDATED (Play Mode deferido).
```

## 32. Anti-regressão

```text
Não alterar layout/spawn/loot/snapshot da cave (stable-run ADR-0005).
Não usar Random/GetHashCode para variação visual — só hash estável (CX09).
Não adicionar campo em save/snapshot.
Não criar [MenuItem] avulso.
Não quebrar placeholders atuais quando profile estiver vazio.
Não renomear biomeIds existentes (id-stability).
```

## 33. Notas para execução posterior

```text
Lote 2 de arte: importar tiles/props em Art/Generated/World/cave/<biomeId>/ e re-rodar
Inicializar Projeto — profiles se preenchem sozinhos, sem código.
Spec futura de música: subscrever CaveBiomeChangedEvent e consumir musicTrackId (fable_58).
Decor/elementos por bioma: fable_78 pode adicionar campos ao profile (extensão, não refactor).
Reskin de baú bandas 5-7 e collider do Tilemap de parede: avaliar após o lote 2.
```
