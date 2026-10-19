# SPEC — Physics Layers + ContactFilter2D para Queries de Combate

> **Spec ID:** `spec_codex_13_physics_layers_contact_filter`
> **Status:** Implementado e BUILD_VALIDATED, DEFERRED_TO_FINAL_HUMAN_VALIDATION (docs-migration 2026-08-12)
> **Wave:** WAVE CODEX CONVERGENCE — Honestidade de Validação (Lote 2)
> **Priority:** P2
> **Type:** Runtime / Editor / Data
> **Domain:** Physics / Combat / Enemy
> **Parallelizable:** NO
> **Parallel group:** N/A
> **Can run with:** Nenhuma — lock central em ProjectSettings/TagManager e nos 3 pontos de query de combate
> **Must not run with:** spec_codex_09, spec_codex_10, spec_codex_11, spec_codex_12 (não há colisão de arquivo, mas por ser a spec de maior risco do lote, recomenda-se não paralelizar execução humana de revisão)
> **Repo lock scope:** `ProjectSettings/TagManager.asset` (via editor script idempotente, não edição manual de YAML), `Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs` (novo RunStep), novo arquivo gerador em `Assets/_Game/Scripts/Editor/**`, `Assets/_Game/Scripts/Enemy/EnemyBrain.cs`, `Assets/_Game/Scripts/Enemy/EnemyMovementExecutor.cs`, `Assets/_Game/Scripts/Combat/PlayerAttackController.cs`, `Assets/_Game/Scripts/Combat/PlayerAttackController.Attacks.cs`, `Assets/_Game/Scripts/Combat/SpellCastService.cs`
> **Depends on (Depende de):**
> - Nenhuma spec deste lote. Depende da regra `editor-generation-orchestration` (3 comandos canônicos) e da skill `editor-tooling-orchestration`.
> **Blocks (Bloqueia):**
> - Nenhuma outra spec do lote `codex_convergence_lote2`.
> **Scope:** Criar 7 physics layers (Player, Enemy, NPC, WorldSolid, Interactable, Projectile, Hazard) via editor script idempotente registrado como `RunStep` em `CindarsHopeMenu.InicializarProjeto`; atribuir layers via os geradores de cena existentes; usar `ContactFilter2D`/`LayerMask` + buffers pré-alocados reutilizáveis nas 3 queries de combate hoje sem mask e com alocação por ataque; wire do `_obstacleLayerMask` do `EnemyBrain` (hoje `0`) para `WorldSolid`.
> **Out of scope:** Os 9 layers do wishlist original (reduzido a 7 — mínimo útil); mudar `AttackHitDetector`/lógica de dano em si (só a query e o mask); editar `ProjectSettings/TagManager.asset` manualmente (deve ser via editor script); adicionar layers via `[MenuItem]` avulso (proibido pela rule `editor-generation-orchestration`).

required_adrs: []
required_game_rules: []

---

## Evidência de implementação (docs-migration 2026-08-12)

```text
Status: Implementado e BUILD_VALIDATED (código) — PENDING_HUMAN_UNITY_ACTION_AND_PLAYMODE
  (materialização dos physics layers + Play Mode)
Execution report: docs/validation/spec_codex_13_physics_layers_contact_filter_execution_report.md (2026-07-03)
Human test scenario: docs/validation/playmode/spec_codex_13_physics_layers_contact_filter_human_test_scenario.md
Re-verificação nesta sessão (Grep/Bash no disco):
  - Assets/_Game/Scripts/Core/Physics/GameplayLayerNames.cs existe.
  - Assets/_Game/Scripts/Editor/Physics/GenerateGameplayPhysicsLayers.cs existe.
  - Assets/_Game/Tests/EditMode/Physics/GameplayPhysicsLayersTests.cs existe.
  - EnemyBrain.cs: `SetObstacleLayerMask(LayerMask)` público confirmado; chamado por
    CaveEnemyMaterializer.cs.
Build: dotnet build PASS (ambos assemblies).
Play Mode / materialização humana (`CindarsHope/Inicializar Projeto`): PENDENTE — comportamento
  runtime é idêntico ao anterior até essa ação humana (fallback seguro, sem regressão); evidência
  automatizada de código é considerada suficiente para esta baixa de docs-migration, com o passo
  humano documentado como follow-up explícito no execution report.
```

---

# /speckit.specify

## 5. Contexto

Achados verificados nesta sessão:

- `ProjectSettings/TagManager.asset` só tem os layers built-in do Unity (Default, TransparentFX, IgnoreRaycast, Water, UI) — nenhum layer de gameplay foi criado.
- `Assets/_Game/Scripts/Enemy/EnemyBrain.cs:87`: `[SerializeField] private LayerMask _obstacleLayerMask = 0; // configurar no prefab; 0 = sem raycast` — o comentário já reconhece que precisa ser configurado, mas nenhum prefab faz isso.
- `Assets/_Game/Scripts/Enemy/EnemyMovementExecutor.cs:332`: pula lógica de avoidance quando a mask é `0` — ou seja, inimigos hoje não fazem obstacle avoidance real via física.
- `Assets/_Game/Scripts/Combat/PlayerAttackController.cs:237-241`: `QueryEnemyPositions` aloca `new List<Vector2>()` (L239) e `new HashSet<EnemyHealth>()` (L241) a cada chamada — sem `LayerMask`, sem buffer reutilizável.
- `Assets/_Game/Scripts/Combat/PlayerAttackController.Attacks.cs:245`: mesmo padrão de alocação por ataque (a confirmar exato em Fase 0).
- `Assets/_Game/Scripts/Combat/SpellCastService.cs:318`: aloca um novo `HashSet` por cast, mesmo padrão.

Nenhuma matriz de colisão está configurada (`Physics2D.SetLayerCollisionMask` ou matriz do `ProjectSettings/Physics2DSettings`), porque não há layers para configurar ainda.

## 6. Problema

Sem layers de gameplay, todo raycast/overlap de combate e de movimento de inimigo é feito sem filtro (mask `0` = nada, ou sem mask = tudo), e a alocação de coleções por ataque/cast é churn de GC em hot path de combate (múltiplos inimigos, múltiplos ataques por segundo). O `_obstacleLayerMask` do `EnemyBrain` está documentado como "configurar no prefab" mas nenhum prefab faz isso — obstacle avoidance de inimigo está efetivamente desligado.

## 7. Objetivo

Ao final desta spec: (1) existem 7 physics layers criados via editor script idempotente e registrado nos 3 comandos canônicos; (2) os prefabs relevantes (Player, Enemy, NPC, chão/parede sólida, interactables, projéteis, hazards) têm layer atribuído via os geradores de cena existentes; (3) as 3 queries de combate usam `ContactFilter2D` com `LayerMask` apropriado e buffers pré-alocados reutilizáveis (arrays, sem `new` por ataque); (4) `EnemyBrain._obstacleLayerMask` é wireado para `WorldSolid` via gerador; (5) a matriz de colisão relevante está configurada via código idempotente (preferencialmente `Physics2D.SetLayerCollisionMask` em runtime bootstrap ou editor script, evitando edição direta de `ProjectSettings/Physics2DSettings.asset`, que exigiria aprovação humana por hook).

## 8. Fontes obrigatórias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.claude/rules/editor-generation-orchestration.md
.claude/skills/editor-tooling-orchestration/SKILL.md
.claude/skills/enemy-ai-authoring/SKILL.md
.claude/rules/unity-assets.md
```

## 9. Estado atual do repo (Phase 0 — auditado nesta sessão)

- `Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs` é o orquestrador único com os 3 comandos canônicos (`Inicializar Projeto`, `Validar Projeto`, `Reparar e Reconstruir`) — qualquer gerador novo deve expor um método `public static` idempotente e ser registrado como `RunStep(...)` dentro de `InicializarProjeto`, na ordem de dependência correta (esta spec deve rodar antes dos geradores de cena que atribuem layer a objetos, e depois de qualquer gerador de prefab de inimigo/player/NPC existir).
- `EnemyBrain.cs:87` e `EnemyMovementExecutor.cs:332` confirmados (ver seção 6).
- 3 pontos de query sem `LayerMask`/`ContactFilter2D` e com alocação por chamada: `PlayerAttackController.cs:237-241` (`QueryEnemyPositions`), `PlayerAttackController.Attacks.cs:245` (a confirmar exato em Fase 0 — Grep antes de editar), `SpellCastService.cs:318`.
- Nenhuma matriz de colisão configurada hoje (layers não existem).
- Fase 0 de execução DEVE mapear, antes de qualquer edição: (a) todos os prefabs de Player/Enemy/NPC/projéteis/interactables/hazards existentes e qual gerador de cena os cria/instancia (para saber onde plugar a atribuição de layer); (b) todos os call sites reais das 3 queries e a assinatura exata dos métodos afetados; (c) se `Physics2D.SetLayerCollisionMask` pode ser chamado de um `RuntimeBootstrap`/`GameBootstrap` sem editar `ProjectSettings/Physics2DSettings.asset` diretamente (confirmar API — `Physics2D.SetLayerCollisionMask(layer1, layer2, bool)` é runtime API, não exige edição do asset de settings; usar isso é o caminho de menor risco, mas fixa a matriz só em runtime, não no editor — documentar essa limitação se for o caminho escolhido).

## 10. User stories / engineering stories

```text
Como sistema de combate, quero que queries de inimigo/dano usem um LayerMask real, para não varrer objetos irrelevantes e para permitir contact filtering correto no futuro (ex.: hazards vs inimigos).
Como sistema de IA de inimigo, quero que o obstacle avoidance realmente funcione contra WorldSolid, em vez de mask 0.
Como sistema de performance, quero que hot paths de combate não aloquem List/HashSet novos por ataque.
```

## 11. Escopo

Inclui:
- Criar 7 layers via `Physics2D`/`TagManager` API idempotente: `Player`, `Enemy`, `NPC`, `WorldSolid`, `Interactable`, `Projectile`, `Hazard` (usar `UnityEditorInternal.InternalEditorUtility.layers`/`SerializedObject` sobre `TagManager.asset` — API editor padrão para adicionar layers programaticamente, idempotente: checar se o layer já existe pelo nome antes de adicionar em outro slot).
- Registrar o gerador como `RunStep("Criar physics layers de gameplay", ...)` em `CindarsHopeMenu.InicializarProjeto`, posicionado ANTES de qualquer `RunStep` que atribua layer a prefab/objeto de cena (respeitar ordem de dependência da rule `editor-generation-orchestration`).
- Atribuir layer aos prefabs/objetos relevantes via os geradores de cena/prefab EXISTENTES (não criar um gerador novo de prefab do zero — estender os que já criam Player/Enemy/NPC/chão/interactables/projéteis/hazards para setar `gameObject.layer` após criar o objeto).
- Wire de `EnemyBrain._obstacleLayerMask` para `LayerMask.GetMask("WorldSolid")` via o gerador de prefab de inimigo existente (não hardcode no C# de runtime — setar no prefab via editor script, mantendo o campo serializado como está).
- Nas 3 queries de combate: trocar para `ContactFilter2D` configurado com o `LayerMask` apropriado (ex.: `Enemy` para queries de dano ao jogador atingir inimigos; documentar o mask exato por query na Fase 0) e usar buffers pré-alocados (arrays de tamanho fixo reutilizados entre chamadas, ex.: `Collider2D[] _resultsBuffer = new Collider2D[32];` como campo de instância, reusado via `Physics2D.OverlapCircleNonAlloc`/`Physics2D.OverlapCollider(ContactFilter2D, Collider2D[])` em vez de alocar `List`/`HashSet` novos por chamada).
- Configurar a matriz de colisão relevante (ex.: `Projectile` não colide com `Player` se for projétil inimigo destinado só a acertar `Player`... decisão de Fase 0 sobre quais pares fazem sentido, documentando cada decisão) via `Physics2D.SetLayerCollisionMask` chamado de um ponto de bootstrap idempotente (ex.: `GameBootstrap` ou um `RuntimeBootstrap` dedicado), já que a rule `unity-assets` e o hook `permissions.ask` exigem aprovação humana para editar `ProjectSettings/*.asset` diretamente — preferir código.
- EditMode smoke test: confirmar que os 7 layers existem após rodar o gerador (idempotente — rodar 2x não duplica).

Fora:
- Os 9 layers do wishlist original (usar só os 7 listados — YAGNI).
- Reescrever `AttackHitDetector` ou a lógica de dano em si.
- Editar `ProjectSettings/TagManager.asset` ou `Physics2DSettings.asset` manualmente (via Write/Edit direto) — só via editor script rodado (e mesmo assim, `permissions.ask` vai pedir aprovação humana por instância para a escrita do asset resultante; documentar isso no relatório).

## 12. Fora de escopo

```text
Não inclui: 9 layers completos do wishlist; reescrita de AttackHitDetector; layers para todo tipo de prop/decoração.
```

## 13. Regras de não duplicação

```text
Não criar um MenuItem avulso para o gerador de layers — registrar como RunStep em CindarsHopeMenu.InicializarProjeto (rule editor-generation-orchestration).
Não criar um segundo sistema de buffer de query — reusar o padrão de buffer pré-alocado consistente entre os 3 pontos (mesmo tamanho/convenção, se fizer sentido).
```

## 14. Critérios de aceite

### 14.1 7 layers criados idempotentemente

- Rodar o gerador (via `CindarsHope/Inicializar Projeto`) cria os 7 layers se ausentes; rodar de novo não duplica nem falha.
- Evidência esperada: log do RunStep + EditMode smoke test confirmando presença dos 7 nomes em `LayerMask.NameToLayer`.

### 14.2 Layers atribuídos nos prefabs relevantes

- Prefabs de Player/Enemy/NPC/chão-sólido/interactables/projéteis/hazards recebem o layer correspondente via os geradores de cena existentes (não manualmente).
- Evidência esperada: leitura do código do gerador estendido + confirmação de que `gameObject.layer` é setado.

### 14.3 EnemyBrain._obstacleLayerMask wireado

- `_obstacleLayerMask` deixa de ser `0` no prefab de inimigo gerado — passa a apontar para `WorldSolid`.
- `EnemyMovementExecutor` deixa de pular avoidance por mask vazio (comportamento observável muda: inimigos evitam obstáculos).

### 14.4 Queries de combate com ContactFilter2D + buffers reutilizáveis

- As 3 queries (`PlayerAttackController.QueryEnemyPositions`, o ponto em `PlayerAttackController.Attacks.cs`, `SpellCastService`) usam `ContactFilter2D` com `LayerMask` explícito e buffers pré-alocados (sem `new List`/`new HashSet` por chamada).
- Evidência esperada: leitura do código (zero `new List<...>()`/`new HashSet<...>()` dentro do hot path da query, substituídos por campo de instância reutilizado + `.Clear()`).

### 14.5 Build + smoke + Play Mode

- `dotnet build` PASS.
- EditMode smoke test dos 7 layers PASS.
- Cenário de Play Mode humano confirma: combate funciona sem regressão (dano ainda acerta inimigos corretamente), inimigo evita obstáculo visível.

---

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/Editor/Physics/
  GenerateGameplayPhysicsLayers.cs   (novo — cria os 7 layers idempotentemente, público, sem [MenuItem])

Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs
  (+ RunStep("Criar physics layers de gameplay", ...) em InicializarProjeto, antes dos geradores de prefab/cena que atribuem layer)

Assets/_Game/Scripts/Editor/SceneCreation/** (ou onde os geradores de prefab existentes vivem)
  (geradores existentes estendidos para setar gameObject.layer)

Assets/_Game/Scripts/Enemy/EnemyBrain.cs           (sem mudança de lógica — só o valor wireado no prefab via gerador)
Assets/_Game/Scripts/Enemy/EnemyMovementExecutor.cs (nenhuma mudança de lógica esperada — passa a receber mask != 0)
Assets/_Game/Scripts/Combat/PlayerAttackController.cs           (ContactFilter2D + buffer)
Assets/_Game/Scripts/Combat/PlayerAttackController.Attacks.cs   (ContactFilter2D + buffer)
Assets/_Game/Scripts/Combat/SpellCastService.cs                 (ContactFilter2D + buffer)

Assets/_Game/Tests/EditMode/Physics/
  GameplayPhysicsLayersTests.cs (novo — smoke test de presença e idempotência)
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
Nenhum SO novo. `ProjectSettings/TagManager.asset` ganha 7 entradas de layer (via editor API, não edição manual).

### 16.2 Runtime contracts
Assinaturas públicas das 3 queries de combate inalteradas (parâmetros de entrada/saída); só a implementação interna muda para usar `ContactFilter2D` + buffer.

### 16.3 Event contracts
N/A — nenhum evento novo.

### 16.4 Save contracts
N/A.

### 16.5 UI contracts
N/A.

## 17. Sistemas afetados

```text
Editor tooling (CindarsHopeMenu, novo gerador de layers)
Enemy (obstacle avoidance)
Combat (queries de ataque/spell)
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/Physics/GenerateGameplayPhysicsLayers.cs (novo)
Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs
Assets/_Game/Scripts/Editor/SceneCreation/** (extensão pontual dos geradores existentes para setar layer)
Assets/_Game/Scripts/Enemy/EnemyBrain.cs (só se necessário para expor a mask corretamente — preferir wiring via prefab, não via código)
Assets/_Game/Scripts/Enemy/EnemyMovementExecutor.cs (leitura/confirmação; edição só se algo além do mask precisar mudar)
Assets/_Game/Scripts/Combat/PlayerAttackController.cs
Assets/_Game/Scripts/Combat/PlayerAttackController.Attacks.cs
Assets/_Game/Scripts/Combat/SpellCastService.cs
Assets/_Game/Tests/EditMode/Physics/**
ProjectSettings/TagManager.asset (só via execução do editor script, nunca edição manual de YAML — hook permissions.ask pedirá aprovação humana)
docs/validation/**
```

## 19. Arquivos proibidos

```text
Assets/_Game/Scripts/Combat/AttackHitDetector.cs (leitura ok, edição não — lógica de dano fora de escopo)
ProjectSettings/Physics2DSettings.asset (edição manual proibida; se a matriz de colisão for necessária, preferir Physics2D.SetLayerCollisionMask via código runtime/editor)
Assets/**/*.unity, *.prefab (edição só via os geradores de cena/prefab existentes rodados, nunca YAML manual)
```

## 20. Estratégia de implementação

```md
### Fase 0 — Mapear prefabs/geradores existentes de Player/Enemy/NPC/chão/interactables/projéteis/hazards; mapear call sites exatos das 3 queries; confirmar viabilidade de Physics2D.SetLayerCollisionMask via bootstrap
### Fase 1 — Criar GenerateGameplayPhysicsLayers.cs (idempotente) + registrar RunStep em CindarsHopeMenu
### Fase 2 — Estender geradores existentes para atribuir layer aos prefabs relevantes + wire EnemyBrain._obstacleLayerMask
### Fase 3 — ContactFilter2D + buffers reutilizáveis nas 3 queries de combate
### Fase 4 — Configurar matriz de colisão relevante via Physics2D.SetLayerCollisionMask (bootstrap)
### Fase 5 — EditMode smoke test + dotnet build
### Fase 6 — Rodar CindarsHope/Inicializar Projeto (ou just o novo RunStep) e documentar evidência
### Fase 7 — Cenário de Play Mode humano (combate + obstacle avoidance) + relatório
```

## 21. Ordem de execucao (ordem segura)

```text
1. Mapear geradores de prefab existentes e call sites das 3 queries (Fase 0).
2. Criar o gerador de layers idempotente.
3. Registrar RunStep em CindarsHopeMenu, na posição correta da ordem de dependência.
4. Estender geradores de prefab para atribuir layer.
5. Wire EnemyBrain._obstacleLayerMask para WorldSolid via prefab.
6. Reescrever as 3 queries com ContactFilter2D + buffer.
7. Configurar matriz de colisão via Physics2D.SetLayerCollisionMask em bootstrap.
8. EditMode smoke test (presença + idempotência dos layers).
9. dotnet build.
10. Rodar o gerador via Unity Editor (evidência de log).
11. Cenário de Play Mode humano documentado.
12. Registrar relatório.
```

## 22. Paralelização

```md
- Parallelizable: NO
- Parallel group: N/A
- Can run with: nenhuma (spec de maior risco do lote; lock central em ProjectSettings/TagManager.asset e nos 3 arquivos de combate)
- Must not run with: recomenda-se não paralelizar revisão humana com as demais 4 specs do lote
- Shared files/systems that require lock: ProjectSettings/TagManager.asset, CindarsHopeMenu.cs, Combat/*, Enemy/EnemyBrain.cs
- Reason: única spec do lote que toca ProjectSettings e físicas — risco de regressão de combate/movement mais alto, não deve ser paralelizada com revisão de outras specs para reduzir superfície de erro simultâneo
```

## 23. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
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
Changes scenes: NO (layers atribuídos via regeneração dos prefabs pelos geradores existentes, não edição manual de .unity)
Changes prefabs: YES (layer atribuído; via gerador, com evidência de regeneração)
Changes ScriptableObjects/assets: YES (ProjectSettings/TagManager.asset ganha 7 layers, via editor script — aprovação humana via permissions.ask esperada)
Requires Play Mode final validation: YES (maior risco do lote — combate e movimento de inimigo mudam de comportamento observável)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION — mas recomenda-se validação assim que possível dado o risco, não só no fechamento de wave
```

## 26. Riscos técnicos

```text
Risco: adicionar layers pode deslocar índices de layer já usados implicitamente em algum lugar (raro, já que não há layers de gameplay hoje, mas confirmar).
Mitigação: usar API que insere no primeiro slot livre nomeado, sem reordenar layers built-in; Fase 0 confirma que nenhum código hoje assume um índice numérico de layer específico.

Risco: matriz de colisão mal configurada pode quebrar detecção de dano existente (regressão de combate).
Mitigação: configurar a matriz de forma conservadora (permitir todos os pares relevantes a colidir, só desabilitar pares claramente inúteis como Player-Player) e validar via cenário de Play Mode antes de considerar ACCEPTED.

Risco: EnemyMovementExecutor pode ter lógica downstream que assume mask 0 = "sem avoidance, sempre anda direto" e passar a se comportar diferente (ex.: preso em quina).
Mitigação: cenário de Play Mode humano explícito testando movimento de inimigo perto de obstáculos.

Risco: maior escopo de arquivos tocados do lote — maior chance de conflito se rodado em paralelo com humano revisando outras specs.
Mitigação: marcado NO-parallel; recomenda-se executar e validar isoladamente.
```

## 27. Rollback

```text
Reverter os 5 arquivos C# de combat/enemy.
Reverter/remover o gerador de layers e o RunStep em CindarsHopeMenu.
Reverter ProjectSettings/TagManager.asset ao estado anterior (via controle de versão, já que a mudança é gerada por script — git checkout do asset).
Reverter prefabs regenerados (via controle de versão).
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Mapear geradores de prefab existentes e call sites exatos das 3 queries de combate.
- [ ] T002 — Criar GenerateGameplayPhysicsLayers.cs idempotente (7 layers).
- [ ] T003 — Registrar RunStep em CindarsHopeMenu.InicializarProjeto na ordem correta.
- [ ] T004 — Estender geradores existentes para atribuir layer aos prefabs relevantes.
- [ ] T005 — Wire EnemyBrain._obstacleLayerMask para WorldSolid via prefab.
- [ ] T006 — Reescrever as 3 queries de combate com ContactFilter2D + buffers pré-alocados.
- [ ] T007 — Configurar matriz de colisão via Physics2D.SetLayerCollisionMask em bootstrap idempotente.
- [ ] T008 — EditMode smoke test (presença + idempotência dos 7 layers).
- [ ] T009 — dotnet build; rodar o gerador via Unity Editor (evidência de log).
- [ ] T010 — Cenário de Play Mode humano (combate sem regressão + obstacle avoidance visível); gerar execution report.
```

## 29. Validações obrigatórias

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\validate_docs.ps1
```

Unity Editor: rodar `CindarsHope/Inicializar Projeto` (ou o RunStep isolado) e capturar log. EditMode smoke test obrigatório.

## 30. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: YES (query com ContactFilter2D é lógica determinística de combate)
- Requires EditMode tests: YES (smoke test de presença/idempotência dos 7 layers; se praticável, teste de que o LayerMask correto é passado ao ContactFilter2D)
- Requires PlayMode automated or final human scenario: YES — OBRIGATÓRIO antes de ACCEPTED, não apenas deferred, dado o risco de regressão de combate/movimento identificado na própria spec
- Requires regression test: YES (combate deve continuar acertando inimigos corretamente; movimento de inimigo não pode travar)
- Human validation timing: IMMEDIATE_RECOMMENDED (não esperar o fechamento de wave, dado o risco P2-mas-arriscado desta spec específica)
- Minimum validation evidence for ACCEPTED: dotnet build PASS + EditMode smoke test PASS + log de execução do gerador de layers no Unity Editor + cenário de Play Mode humano documentado (combate normal, inimigo evitando obstáculo).
```

## 31. Definition of Done

```text
7 layers criados via editor script idempotente, registrado como RunStep em CindarsHopeMenu.
Prefabs relevantes com layer atribuído via geradores existentes.
EnemyBrain._obstacleLayerMask wireado para WorldSolid; obstacle avoidance funcional.
3 queries de combate usando ContactFilter2D + buffers reutilizáveis, sem alocação por ataque.
Matriz de colisão configurada de forma conservadora via Physics2D.SetLayerCollisionMask.
EditMode smoke test + dotnet build PASS + cenário de Play Mode humano documentado.
```

## 32. Anti-regressão

```text
Não reduzir a 9 layers do wishlist original — manter os 7 mínimos.
Não editar ProjectSettings/*.asset manualmente — só via script executado (evidência de log).
Não quebrar detecção de dano existente — validar via Play Mode antes de aceitar.
Não criar MenuItem avulso — usar RunStep em CindarsHopeMenu.
```

## 33. Notas para execução posterior

```text
Se a Fase 0 revelar que mais de 7 layers são estritamente necessários para algum caso já existente no código (ex.: um sistema que já espera um layer nomeado específico fora da lista), documentar e decidir na execução se expande a lista ou trata como spec futura — não expandir silenciosamente sem registrar a decisão.
Physics2D.SetLayerCollisionMask configurado via bootstrap runtime não persiste no ProjectSettings/Physics2DSettings.asset do editor — ou seja, a matriz só é efetiva quando o bootstrap roda (Play Mode/build). Documentar essa limitação; se o projeto preferir a matriz refletida também no editor fora de Play Mode, isso exigiria editar Physics2DSettings.asset diretamente, o que é fora de escopo desta spec (exigiria autorização humana explícita adicional).
```
