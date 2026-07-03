# SPEC — Guards de Debug Tooling em Build de Produção

> **Spec ID:** `spec_codex_11_debug_build_guards`
> **Status:** A implementar
> **Wave:** WAVE CODEX CONVERGENCE — Honestidade de Validação (Lote 2)
> **Priority:** P1
> **Type:** Runtime
> **Domain:** DebugTools / Cave / Build
> **Parallelizable:** YES
> **Parallel group:** codex_convergence_lote2
> **Can run with:** spec_codex_09, spec_codex_10, spec_codex_12, spec_codex_13
> **Must not run with:** N/A
> **Repo lock scope:** `Assets/_Game/Scripts/DebugTools/CollisionDebugOverlayBootstrap.cs`, `Assets/_Game/Scripts/Cave/Runtime/CaveDebugLevelSkipController.cs`
> **Depends on (Depende de):**
> - Nenhuma spec deste lote.
> **Blocks (Bloqueia):**
> - Nenhuma outra spec do lote `codex_convergence_lote2`.
> **Scope:** Impedir que ferramentas de debug (overlay de colisão auto-instanciado, skip de nível da cave via tecla) fiquem ativas em builds de produção (Standalone Release sem `DEVELOPMENT_BUILD`), guardando-as com `#if UNITY_EDITOR || DEVELOPMENT_BUILD` e/ou default seguro (`false`).
> **Out of scope:** Guardar o `DebugHud` IMGUI usado como HUD real do slice atual (é gameplay-facing, não debug tooling — decisão explícita de não tocar); redesenhar o sistema de debug tooling do projeto; remover qualquer ferramenta de debug (só adicionar guard/default seguro).

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

Achados verificados nesta sessão:

- `Assets/_Game/Scripts/DebugTools/CollisionDebugOverlayBootstrap.cs` (25 linhas): `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]` na linha 11, **sem nenhum guard de compilação** — roda em qualquer build, inclusive Release final. O método usa `Object.FindAnyObjectByType<CollisionDebugOverlay>()` (L14) para checar se já existe antes de instanciar — não é `FindObjectOfType` proibido pela rule (`FindAnyObjectByType` é uma API diferente, mas o uso aqui é justamente para evitar duplicar um sistema DebugTools dormente, então é aceitável como exceção documentada de DebugTools; não expandir esse padrão para código de gameplay).
- `Assets/_Game/Scripts/Cave/Runtime/CaveDebugLevelSkipController.cs`: `[SerializeField] private bool _enableDebugLevelSkip = true;` (L11) — **default `true`**, ou seja, qualquer objeto de cena com este componente já nasce habilitado a pular níveis via tecla `P` ou `F2` (L12-13, L19-20) a menos que alguém desmarque manualmente no inspector. Não há guard de compilação (`#if UNITY_EDITOR || DEVELOPMENT_BUILD`) ao redor da lógica de skip.

## 6. Problema

Em um build de Standalone Release (sem `DEVELOPMENT_BUILD`), o jogador final poderia: (a) ter o overlay de debug de colisão instanciado automaticamente em toda cena (custo de performance + visual indesejado se ativado), e (b) pular níveis inteiros da cave apertando `P` ou `F2`, quebrando completamente a progressão e o stable-run contract observável pelo jogador. Nenhuma dessas ferramentas deveria estar acessível fora de Editor/Development Build.

## 7. Objetivo

Ao final desta spec, `CollisionDebugOverlayBootstrap` só executa (o `[RuntimeInitializeOnLoadMethod]` é efetivamente um no-op) fora de `UNITY_EDITOR`/`DEVELOPMENT_BUILD`, e `CaveDebugLevelSkipController` tem `_enableDebugLevelSkip` com default `false` e a lógica de skip guardada por `#if UNITY_EDITOR || DEVELOPMENT_BUILD` — nenhuma das duas ferramentas fica ativa em build de produção.

## 8. Fontes obrigatórias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.claude/skills/observability-and-logging/SKILL.md
.claude/rules/unity-architecture.md
```

## 9. Estado atual do repo (Phase 0 — auditado nesta sessão)

- `CollisionDebugOverlayBootstrap.cs` inteiro (25 linhas): namespace `CindarsHope.DebugTools`, classe estática, um único método `Init()` com `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]`. Sem guard. Cria `GameObject("CollisionDebugOverlay")`, `DontDestroyOnLoad`, adiciona `CollisionDebugOverlay` component.
- `CaveDebugLevelSkipController.cs`: `_enableDebugLevelSkip` (L11, default `true`), `_nextGateKey = KeyCode.P` (L12), `_alternateNextGateKey = KeyCode.F2` (L13), e campos duplicados/legados `_nextLevelKey`/`_alternateNextLevelKey` (L19-20, `[HideInInspector]`, mesmos valores — Fase 0 de execução deve confirmar se estes campos legados ainda são lidos em algum lugar ou são vestigiais; não remover sem confirmar, fora do escopo mínimo desta spec que é só guard + default). Linha 37 loga o estado no Awake/Start (`enabled=`). Linha 44 usa `if (!_enableDebugLevelSkip) return;` como guard de execução (dentro de algum método de update/input, a confirmar). Linha 74 tem uma segunda checagem: `if (!_enableDebugLevelSkip || !_showDebugSkipButton || SceneManager.GetActiveScene().name != "CaveScene") return;` (provavelmente controla desenho de UI/botão de debug). Linha 170 expõe `public bool IsDebugSkipEnabled => _enableDebugLevelSkip;` — Fase 0 deve confirmar consumidores deste getter antes de mudar o default, para não quebrar um consumidor que assume `true`.
- Varredura Phase-0 adicional por outros `*Debug*` runtime sem guard (Grep de `RuntimeInitializeOnLoadMethod` no projeto): existem vários usos legítimos do idiom (`AudioManager`, `CombatStateTracker`, `SfxEventBridge`, etc.) que são sistemas de produção reais (não debug), então não entram no escopo desta spec — a spec só cobre os 2 arquivos nomeados no scope. Fase 0 de execução deve, ainda assim, rodar `Grep -r "RuntimeInitializeOnLoadMethod" Assets/_Game/Scripts/DebugTools/` e `Assets/_Game/Scripts/Cave/` para confirmar que não há um terceiro arquivo de debug tooling sem guard que também deveria ser coberto — se encontrar, **listar no relatório como achado adicional**, mas não expandir o escopo de edição sem atualizar o repo lock scope e justificar.
- `DebugHud` (IMGUI, usado como HUD do slice atual conforme apontado no pedido do usuário): **NÃO deve ser guardado ou alterado nesta spec** — é gameplay-facing (HUD real), não uma ferramenta de debug descartável. Apenas registrar sua existência no relatório como nota, sem tocar no código.

## 10. User stories / engineering stories

```text
Como jogador de um build final, não quero ter acesso a ferramentas de debug que quebram a progressão (skip de nível) ou poluem a cena (overlay de colisão).
Como desenvolvedor, quero que ferramentas de debug continuem funcionando normalmente no Editor e em Development Builds.
```

## 11. Escopo

Inclui:
- Guardar o corpo de `CollisionDebugOverlayBootstrap.Init()` com `#if UNITY_EDITOR || DEVELOPMENT_BUILD` ... `#endif` (ou guardar a classe/atributo inteiro, decisão de Fase 0 conforme o que for mais idiomático no projeto — Grep por outros exemplos de guard `#if UNITY_EDITOR || DEVELOPMENT_BUILD` já existentes no código para seguir o padrão local).
- Trocar o default de `_enableDebugLevelSkip` em `CaveDebugLevelSkipController` de `true` para `false`.
- Guardar a lógica de skip (o corpo do método que lê `_nextGateKey`/`_alternateNextGateKey` e executa o skip, e o desenho do botão de debug em L74) com `#if UNITY_EDITOR || DEVELOPMENT_BUILD` — de forma que, mesmo que alguém habilite `_enableDebugLevelSkip = true` manualmente em um build de produção (ex.: editando um asset/prefab), a lógica ainda não executa fora de Editor/Development Build. Dupla proteção: default seguro + guard de compilação.
- Trocar `Object.FindAnyObjectByType<CollisionDebugOverlay>()` (L14) pelo padrão permitido: dado que este é código sob `DebugTools/` (exceção documentada na rule `unity-architecture` para ferramentas de debug/editor), confirmar se a exceção já cobre isto ou se deve-se documentar explicitamente como "exceção DebugTools" no comentário do arquivo — não é obrigatório trocar por serialized ref (o padrão de auto-bootstrap dormente depende de detectar se já existe uma instância), mas documentar a decisão explicitamente no código (comentário) e no relatório.
- Varredura Grep documentada (sem expandir escopo de edição) por outros `*Debug*` sem guard.

Fora:
- Tocar em `DebugHud` (gameplay HUD real do slice, fora de escopo por decisão explícita).
- Remover qualquer ferramenta de debug (só guard + default).
- Expandir para outros arquivos de debug não nomeados no repo lock scope, exceto para citar no relatório.

## 12. Fora de escopo

```text
Não inclui: DebugHud IMGUI; remoção de qualquer ferramenta de debug; refactor do sistema de debug tooling; novos debug commands.
```

## 13. Regras de não duplicação

```text
Não criar um novo sistema de guard de build — usar as diretivas de compilação padrão do Unity (#if UNITY_EDITOR, DEVELOPMENT_BUILD) já usadas em outros pontos do projeto (Grep antes de inventar convenção nova).
```

## 14. Critérios de aceite

### 14.1 CollisionDebugOverlayBootstrap guardado

- Fora de `UNITY_EDITOR`/`DEVELOPMENT_BUILD`, `Init()` não cria o overlay (é efetivamente no-op em build de produção).
- Dentro de Editor/Development Build, comportamento inalterado (overlay continua funcionando como hoje).
- Evidência esperada: leitura do código (guard presente e correto).

### 14.2 CaveDebugLevelSkipController seguro por padrão e por build

- `_enableDebugLevelSkip` tem default `false`.
- A lógica de skip (leitura de tecla + execução do skip + desenho do botão) está guardada por `#if UNITY_EDITOR || DEVELOPMENT_BUILD`, então mesmo com o campo setado para `true` manualmente, não executa em build de produção.
- Evidência esperada: leitura do código.

### 14.3 Sem regressão em Editor/Development Build

- `dotnet build` PASS.
- Comportamento de debug em Editor/Development Build permanece idêntico ao atual (overlay funcionando, skip de nível funcionando quando habilitado).

---

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/DebugTools/
  CollisionDebugOverlayBootstrap.cs   (guard de compilação adicionado)

Assets/_Game/Scripts/Cave/Runtime/
  CaveDebugLevelSkipController.cs     (default false + guard de compilação na lógica de skip)
```

Nenhum teste automatizado novo é esperado aqui — a lógica é puramente diretivas de compilação e um default de campo serializado, não lógica determinística testável em EditMode da forma usual. Ver Testing Quality Gate para o residual risk documentado.

## 16. Contratos, dados e eventos

### 16.1 Data contracts
Nenhum novo.

### 16.2 Runtime contracts
`CollisionDebugOverlayBootstrap.Init()` e a lógica interna de `CaveDebugLevelSkipController` — comportamento externo (fora de Editor/Dev Build) muda de "ativo" para "no-op"; dentro de Editor/Dev Build, inalterado.

### 16.3 Event contracts
N/A.

### 16.4 Save contracts
N/A.

### 16.5 UI contracts
N/A — `DebugHud` explicitamente fora de escopo.

## 17. Sistemas afetados

```text
DebugTools (overlay de colisão)
Cave (debug level skip)
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/DebugTools/CollisionDebugOverlayBootstrap.cs
Assets/_Game/Scripts/Cave/Runtime/CaveDebugLevelSkipController.cs
docs/validation/**
```

## 19. Arquivos proibidos

```text
Assets/_Game/Scripts/DebugTools/CollisionDebugOverlay.cs (o overlay em si — só o bootstrap muda)
Assets/_Game/Scripts/UI/**/DebugHud*.cs (explicitamente fora de escopo)
Assets/**/*.unity, *.prefab, *.asset
```

## 20. Estratégia de implementação

```md
### Fase 0 — Grep por outros *Debug* sem guard em DebugTools/ e Cave/; confirmar consumidores de IsDebugSkipEnabled; confirmar padrão local de #if UNITY_EDITOR || DEVELOPMENT_BUILD já usado no projeto
### Fase 1 — Guardar CollisionDebugOverlayBootstrap.Init()
### Fase 2 — Default false + guard na lógica de skip de CaveDebugLevelSkipController
### Fase 3 — dotnet build + relatório (citando achados adicionais da varredura, se houver)
```

## 21. Ordem de execucao (ordem segura)

```text
1. Grep de RuntimeInitializeOnLoadMethod em DebugTools/ e Cave/ para achados adicionais.
2. Confirmar consumidores de IsDebugSkipEnabled antes de mudar default.
3. Adicionar guard em CollisionDebugOverlayBootstrap.
4. Trocar default + guardar lógica de skip em CaveDebugLevelSkipController.
5. dotnet build.
6. Registrar relatório citando achados adicionais e a decisão sobre DebugHud (não tocado).
```

## 22. Paralelização

```md
- Parallelizable: YES
- Parallel group: codex_convergence_lote2
- Can run with: demais specs deste lote (arquivos não se sobrepõem)
- Must not run with: N/A
- Shared files/systems that require lock: DebugTools/CollisionDebugOverlayBootstrap.cs, Cave/Runtime/CaveDebugLevelSkipController.cs (lock local)
- Reason: escopo isolado a diretivas de compilação e um default de campo; sem contato com outros sistemas
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
Changes UI: NO (DebugHud explicitamente fora de escopo)
Changes scenes: NO
Changes prefabs: NO (default de campo serializado muda só para instâncias NOVAS/prefabs regenerados; instâncias já existentes em cena mantêm o valor serializado salvo — Fase 0 deve confirmar se algum prefab existente tem _enableDebugLevelSkip explicitamente serializado como true e documentar)
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES (confirmar que build de produção não tem overlay nem skip ativo; confirmar que Development Build mantém ambos funcionando)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos

```text
Risco: prefabs/objetos de cena existentes podem ter _enableDebugLevelSkip serializado explicitamente como true (valor salvo sobrepõe o novo default do campo).
Mitigação: o guard de compilação (#if UNITY_EDITOR || DEVELOPMENT_BUILD) é a proteção real contra build de produção, independente do valor serializado; documentar isso no relatório como a defesa primária.

Risco: mudar o default pode afetar fluxo de QA/dev que dependia do skip estar sempre ligado sem configuração manual.
Mitigação: o comportamento em Editor/Development Build é preservado quando _enableDebugLevelSkip é explicitamente true (que continua sendo o caso em qualquer objeto de cena já configurado); só o DEFAULT de novas instâncias muda.
```

## 27. Rollback

```text
Reverter os guards de compilação e o default para true.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Grep de RuntimeInitializeOnLoadMethod sem guard em DebugTools/ e Cave/ (achados adicionais).
- [ ] T002 — Confirmar consumidores de IsDebugSkipEnabled.
- [ ] T003 — Guardar CollisionDebugOverlayBootstrap.Init() com #if UNITY_EDITOR || DEVELOPMENT_BUILD.
- [ ] T004 — Trocar default de _enableDebugLevelSkip para false + guardar lógica de skip com a mesma diretiva.
- [ ] T005 — dotnet build; gerar execution report citando achados adicionais e a decisão de não tocar DebugHud.
```

## 29. Validações obrigatórias

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\validate_docs.ps1
```

## 30. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: NO (diretivas de compilação + default de campo, não lógica testável em EditMode da forma usual)
- Requires EditMode tests: NO — residual risk documentado: a garantia real vem de revisão de código + validação de build de produção real (fora do escopo prático de EditMode, que roda sempre em contexto Editor)
- Requires PlayMode automated or final human scenario: YES (cenário humano: gerar um build de produção — ou simular via Development Build desabilitado — e confirmar ausência de overlay/skip; confirmar presença em Development Build)
- Requires regression test: NO (mudança aditiva de guard, não bugfix de lógica existente)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: dotnet build PASS + leitura de código confirmando guards corretos + cenário de Play Mode/build documentado.
```

## 31. Definition of Done

```text
CollisionDebugOverlayBootstrap.Init() guardado por #if UNITY_EDITOR || DEVELOPMENT_BUILD.
CaveDebugLevelSkipController com default false e lógica de skip guardada pela mesma diretiva.
Achados adicionais da varredura Phase-0 documentados no execution report.
DebugHud explicitamente não tocado, com nota no relatório.
```

## 32. Anti-regressão

```text
Não alterar comportamento em Editor/Development Build.
Não tocar em DebugHud.
Não remover CollisionDebugOverlay ou CaveDebugLevelSkipController — só guardar/ajustar default.
```

## 33. Notas para execução posterior

```text
Se a varredura Phase-0 encontrar outros arquivos *Debug* sem guard fora do repo lock scope desta spec, registrar como item de backlog/spec futura — não expandir esta spec sem atualizar o scope formalmente.
Campos legados _nextLevelKey/_alternateNextLevelKey ([HideInInspector]) não foram removidos — se confirmados vestigiais na Fase 0, considerar spec de limpeza futura (fora de escopo aqui).
```
