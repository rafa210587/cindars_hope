# SPEC — Inimigos: Unificação da Escala Visual no Eixo do Player

> **Spec ID:** `fable_79_spec_enemy_scale_player_relative_unification`
> **Status:** A implementar
> **Wave:** FABLE Batch 6
> **Priority:** P1
> **Type:** Runtime / Data
> **Domain:** Cave / Combat / Scale
> **Parallelizable:** NO (escala compartilhada por todo o roster)
> **Parallel group:** N/A
> **Can run with:** specs que não tocam materialização da cave nem escala
> **Must not run with:** F80 (roster — usa o resolver desta spec), F33 (gerador de bestiário)
> **Repo lock scope:** `Combat/EnemyScaleResolver.cs`, `Cave/Runtime/CaveRuntimeMaterializer.cs` (bloco de escala), `Combat/Data/EnemySizeProfileSO.cs`
> **Depends on:** F33 (BestiarySizeClass nas fichas), sistema de escala de mundo (VisualScaleProfile)
> **Blocks:** F80 (roster +40 — todas as fichas dependem do eixo unificado), F81 (packs)
> **Scope:** fazer a escala visual dos inimigos da caverna derivar SEMPRE do tamanho do player (eixo relativo único), eliminando a divergência entre `EnemySizeProfileSO.SpriteScale` (absoluto) e `BestiarySizeClass` (relativo).
> **Out of scope:** autoração de novas fichas (F80); mudança de física de colisão além do necessário; arte/sprites.

required_adrs: [ADR-0005]
required_game_rules: [cave_rules.md, combat_rules.md]

---

# /speckit.specify

## Contexto

O projeto tem três sistemas de escala que divergem:

1. **Mundo (Farm/Town)** — `VisualScaleProfileSO` aplica escala **relativa ao player**:
   `VisualScale = PlayerReferenceScale (2.0) × playerRelative`. Medium = 1.0× player,
   Large = 1.5×, Huge = 2.0×. Correto e centrado no personagem.
2. **Caverna (combate)** — `CaveRuntimeMaterializer` aplica `EnemySizeProfileSO.SpriteScale`,
   valores **absolutos** (Tiny 0.65 … Boss 2.20) que **não** referenciam o player.
3. **Bestiário** — `EnemyDataSO.VisualScale` baked de `BestiarySizeClass × multiplicador`
   (miniboss ×1.5, boss ×2.5; até 10.0× para Gargantuan boss). É **descartado** no runtime
   da caverna porque o `SpriteScale` do `EnemySizeProfileSO` tem precedência.

Resultado concreto: um boss Gargantuan que a ficha descreve como 10× aparece na caverna a
2.2×. Os tamanhos da caverna **não refletem o tamanho do player**, contradizendo o requisito
de design ("tamanhos sempre relativos ao personagem do jogador").

A precedência atual vive em `CaveRuntimeMaterializer.cs` (bloco "Scale: prefer
SizeProfile.SpriteScale, fallback to EnemyDataSO.VisualScale").

## Problema

Sem unificar o eixo, qualquer expansão de roster (F80) herda o conflito: minibosses/bosses
ficam visualmente subdimensionados na caverna, a variedade de tamanho fica achatada (todos
os comuns colapsam em 1.0/2.0) e o número que o bestiário/codex mostra (F45) não corresponde
ao que o jogador vê em campo. É uma incoerência de design e uma fonte de bug de leitura de
combate (hitbox vs. sprite).

## Objetivo

Ao final desta spec, toda escala visual de inimigo da caverna deriva de uma **fórmula única
relativa ao player** — `EnemyScaleResolver.ResolveVisualScale(sizeClass, role)` =
`PlayerReferenceScale × playerRelative(sizeClass) × roleMultiplier(role)` — aplicada no
`CaveRuntimeMaterializer`. O `EnemySizeProfileSO` deixa de ser fonte de escala visual e passa
a carregar **apenas dados de física** (collider, footprint, pathing, sala mínima, knockback).
O número exibido no codex (F45) e o tamanho em campo passam a coincidir. EditMode tests
travam a relação ("Medium = player; Huge = 2× player; Boss Gargantuan = N× player"). O
stable-run da caverna é preservado.

## Fontes obrigatórias lidas

```text
Assets/_Game/Scripts/Combat/EnemyScaleResolver.cs (resolver atual)
Assets/_Game/Scripts/Combat/Data/EnemySizeProfileSO.cs
Assets/_Game/Scripts/Combat/EnemyDataSO.cs (BestiarySizeClass + VisualScale)
Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs (bloco de escala)
Assets/_Game/Scripts/Editor/ScaleSystem/CreateDefaultScaleAssets.cs (PlayerReferenceScale=2.0, ratios)
Assets/_Game/Scripts/Editor/Enemies/GenerateCanonicalBestiary.cs (mapeamento baseScale + multiplicadores)
.claude/rules/cave-stable-run.md
.claude/rules/no-magic-balance-values.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- PlayerReferenceScale = 2.0 e ratios playerRelative (CreateDefaultScaleAssets);
- BestiarySizeClass {Tiny,Small,Medium,Large,Huge,Gargantuan} em EnemyDataSO;
- multiplicadores de função miniboss ×1.5 / boss ×2.5 (GenerateCanonicalBestiary);
- EnemySizeProfileSO com ColliderRadius/FootprintCells/PathingRadius/MinRoomSize/Knockback;
- EnemyScaleResolver (já existe — ponto de extensão, NÃO criar resolver paralelo);
- CaveRuntimeMaterializer com bloco de escala + log.
Não existe:
- fórmula única player-relative consumida pelo materializer;
- separação explícita "EnemySizeProfileSO = física, não escala visual".
Auditar Fase 0:
- valores exatos de playerRelative por size class e onde estão (const vs asset);
- todos os call sites que leem SpriteScale para escala visual;
- se algum prefab de inimigo da cave tem VisualScaleApplicator que conflite.
```

## Engineering stories

```text
Como jogador, quero que um inimigo "Grande" pareça grande EM RELAÇÃO a mim sempre,
  na caverna como no mundo, para que a leitura de ameaça seja consistente.
Como designer, quero um único número (size class + função) que determine o tamanho,
  para não ajustar dois sistemas que discordam.
Como F45 (codex), quero que o tamanho mostrado seja o tamanho real em campo.
Como stable-run, quero que a escala derive de dados determinísticos (size class/role),
  sem reroll ao revisitar.
```

## Escopo

```text
Inclui:
- EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass, EnemyRole/flags miniboss/boss):
  PlayerReferenceScale × playerRelative(sizeClass) × roleMultiplier — fonte única;
- ratios playerRelative para as 6 size classes (incl. Gargantuan, hoje ausente do mundo)
  expostos como tabela nomeada (rule no-magic-balance-values), não literais soltos;
- roleMultiplier: comum 1.0, miniboss 1.5, boss 2.5 (alinhado ao gerador);
- CaveRuntimeMaterializer: trocar a precedência atual por ResolveVisualScale (escala
  visual NÃO vem mais de SpriteScale);
- EnemySizeProfileSO: SpriteScale marcado [Obsolete]/documentado como NÃO autoritativo
  para escala visual (mantido só por compat de física se algum campo derivar dele) —
  física (collider/footprint/pathing/minroom/knockback) continua vindo do profile;
- log de wiring claro (size class, role, escala final, player ref) no materializer;
- EditMode tests da fórmula e da relação com o player.
```

## Fora de escopo

```text
Não inclui:
- autoração de novas criaturas (F80);
- regerar assets de bestiário (gerador idempotente roda no closeout — evidência apenas);
- mudar a física de colisão (collider/footprint) — só a ESCALA VISUAL é unificada;
- VisualScaleProfile do mundo (já correto — só serve de referência);
- arte/sprites/animações.
```

## Regras de não duplicação

```text
NÃO criar segundo resolver de escala — estender EnemyScaleResolver existente.
NÃO inventar nova fonte de PlayerReferenceScale — reusar a const/asset existente (2.0).
NÃO usar literais de escala soltos — tabela nomeada (no-magic-balance-values).
NÃO mexer no VisualScaleApplicator do mundo (escopo de mundo, já correto).
```

## Critérios de aceite

### CA-1 Fonte única player-relative
- A escala visual de qualquer inimigo da caverna provém só de
  `EnemyScaleResolver.ResolveVisualScale(...)`; nenhum call site usa `SpriteScale` como
  escala visual.
- Evidência: grep sem ocorrências de SpriteScale como escala visual + teste.

### CA-2 Relação com o player travada
- Medium comum = PlayerReferenceScale (mesmo tamanho do player); Large = 1.5×; Huge = 2×;
  miniboss/boss aplicam ×1.5/×2.5 sobre a base; Gargantuan boss > Huge boss.
- Evidência: EditMode test com asserts numéricos por classe/função.

### CA-3 Física preservada
- Collider/footprint/pathing/minroom/knockback continuam vindo do EnemySizeProfileSO
  (sem regressão de colisão/pathing).
- Evidência: testes existentes de EnemyScaleResolver/física PASS; diff não altera leitura de física.

### CA-4 Codex coincide com campo
- O valor de tamanho exibível (F21/F45) e o aplicado no transform são o mesmo número.
- Evidência: teste que compara o valor exposto ao codex com ResolveVisualScale.

### CA-5 Stable-run intacto
- Replay validator do cave PASS; mesma composição/posições; escala determinística por size/role.
- Evidência: execução do replay validator registrada no report.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Combat/
  EnemyScaleResolver.cs        (ResolveVisualScale + tabela playerRelative/roleMultiplier)
  Data/EnemySizeProfileSO.cs   (SpriteScale documentado NÃO-autoritativo p/ escala visual)
Assets/_Game/Scripts/Cave/Runtime/
  CaveRuntimeMaterializer.cs   (bloco de escala consome ResolveVisualScale + log)
Assets/_Game/Tests/EditMode/Combat/
  EnemyScaleResolverPlayerRelativeTests.cs (NOVO)
docs/validation/
  fable_79_spec_enemy_scale_player_relative_unification_execution_report.md
```

## Contratos

### Data contracts
Tabela nomeada `PlayerRelativeScale` por `BestiarySizeClass` (Tiny … Gargantuan) e
`RoleScaleMultiplier` (comum 1.0 / miniboss 1.5 / boss 2.5). `PlayerReferenceScale`
reusado da fonte existente (2.0). Nenhum campo de save muda.

### Runtime contracts
`EnemyScaleResolver.ResolveVisualScale(BestiarySizeClass size, bool isMiniBoss, bool isBoss)`
→ float (puro, determinístico, testável). `CaveRuntimeMaterializer` chama esse método e
aplica em `transform.localScale`; física segue lendo `EnemySizeProfileSO`.

### Save contracts
N/A — escala recomputada de dados determinísticos. Stable-run preservado.

### UI contracts
F21/F45 (codex) passam a expor o número de `ResolveVisualScale` (coincidência campo/codex).
Nenhuma tela nova.

## Sistemas afetados

```text
Enemy scale (núcleo) | Cave materialization | Bestiary/codex (leitura) | Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Combat/EnemyScaleResolver.cs
Assets/_Game/Scripts/Combat/Data/EnemySizeProfileSO.cs (doc/atributo, sem remover campo)
Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs (bloco de escala)
Assets/_Game/Tests/EditMode/Combat/**
docs/validation/**
csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset (edição manual de YAML)
Packages/** ; ProjectSettings/**
VisualScaleApplicator / sistema de escala do mundo (consumir referência, não alterar)
SaveManager / GameSaveData
Geradores de bestiário (rodar no closeout para evidência; não reescrever a lógica aqui)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Mapear ratios playerRelative, todos os call sites de SpriteScale como escala visual e
prefabs de cave com VisualScaleApplicator conflitante. Ler stable-run.

### Fase 1 — Resolver
ResolveVisualScale + tabelas nomeadas. EditMode tests da fórmula e relação com player.

### Fase 2 — Materializer
Trocar precedência (SpriteScale → ResolveVisualScale) + log de wiring. Física intocada.

### Fase 3 — Codex coincide
Garantir que F21/F45 leem o mesmo número (ajuste de leitura, sem nova UI).

### Fase 4 — Validação
Replay validator; testes; csproj; run_strict_validation; gerar bestiário p/ evidência; report.
```

## Paralelização

- Parallelizable: NO — Must not run with F80/F33 (mesma cadeia de escala/materialização).
- Lock: EnemyScaleResolver, CaveRuntimeMaterializer (bloco de escala), EnemySizeProfileSO.
- Reason: F80 autora fichas que dependem desta fórmula; rodar junto geraria contrato instável.

## Impacto em save/load

```text
Changes save schema? NO | Adds section? NO | Migration? NO | Persists Unity refs? NO
Escala recomputada de size class/role (determinística). Stable-run intacto.
```

## Impacto em eventos

```text
Adds events: NO | Changes events: NO | Unsubscribe pattern: N/A
```

## Impacto em UI/Unity

```text
Changes UI: leitura do codex passa a usar ResolveVisualScale (sem nova tela)
Changes scenes/prefabs: NO | Changes assets: regerar bestiário no closeout (evidência)
Requires Play Mode final validation: YES (conferência visual campo vs codex)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: prefab de cave com VisualScaleApplicator sobrepondo a escala do materializer.
Mitigação: auditoria Fase 0; materializer aplica por último; documentar precedência.
Risco: física dessincronizar do visual (collider grande, sprite pequeno).
Mitigação: física segue do profile; teste comparando ordem de grandeza visual×collider.
Risco: quebra de stable-run.
Mitigação: escala 100% determinística por size/role; replay validator (CA-5).
Risco: Gargantuan ausente da tabela do mundo → ratio indefinido.
Mitigação: definir ratio explícito de Gargantuan na tabela nomeada.
```

## Rollback

```text
Reverter o bloco do materializer restaura precedência por SpriteScale (assets intactos).
ResolveVisualScale é aditivo; remover o call site volta ao comportamento anterior.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar ratios, call sites de SpriteScale (escala visual) e prefabs com applicator.
- [ ] T002 — ResolveVisualScale + tabelas nomeadas (playerRelative, roleMultiplier) + tests.
- [ ] T003 — Materializer consome ResolveVisualScale + log; física intocada.
- [ ] T004 — Coincidência codex/campo (F21/F45 leem o mesmo número).
- [ ] T005 — Replay validator; gerar bestiário (evidência); csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (fórmula de escala)
- Requires EditMode tests: YES (fórmula + relação com player + coincidência codex)
- Requires PlayMode/human scenario: YES (conferência visual campo vs codex)
- Requires regression test: YES (física/colisão + replay stable-run)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano comparando 3 size classes

## Definition of Done

```text
Escala visual de inimigo da cave 100% via ResolveVisualScale (player-relative);
SpriteScale não é mais fonte de escala visual; física preservada; codex coincide com campo;
stable-run intacto (replay PASS). Builds 0E; run_strict_validation exit 0; report criado.
```

## Anti-regressão

```text
Física (collider/footprint/pathing) inalterada.
Mesmo CaveRunSeed → mesma escala/composição ao revisitar.
Nenhum literal de escala solto (no-magic-balance-values).
Nenhum GameObject.Find em runtime; comunicação só via GameEventBus.
```
