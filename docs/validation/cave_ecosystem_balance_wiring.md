# Wiring gap: CaveEcosystemBalanceSO sem fallback de runtime

**Data:** 2026-07-10
**Escopo:** `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs`,
`Assets/_Game/Scripts/Editor/Cave/GenerateCaveEcosystemBalance.cs`

## Gap encontrado

`CaveRuntimeMaterializer._ecosystemBalance` (`[SerializeField] CaveEcosystemBalanceSO`) já tinha dois
irmãos (`_biomeArtProfiles` e `_environmentElementDatabase`) com fallback via `Resources.Load` quando
o `SerializeField` fica `fileID: 0` numa `CaveScene` recriada. `_ecosystemBalance` não tinha esse
fallback, e o asset `Assets/_Game/Data/Cave/CaveEcosystemBalance.asset` vive fora de `Resources/`.

Efeito: em cena recriada sem o campo ligado no Inspector, o roll de conflito inter-monstro (fable_78
SLICE 4), o status "Ferido", threat budget/densidade de inimigos e elementos ambientais por banda, e o
`FloorClusterDensityMultiplier` (spec_cave_decor_composition_runtime / CV03) caíam silenciosamente em
defaults hardcoded no código (via os fallbacks internos de cada consumidor), em vez dos valores do
asset de balance.

## Fix (2 partes, espelhando o padrão existente)

### 1. Runtime fallback — `CaveRuntimeMaterializer.cs`

- Novo `private CaveEcosystemBalanceSO ResolveEcosystemBalance()`, espelhando exatamente
  `ResolveEnvironmentElementDatabase()`: retorna `_ecosystemBalance` se != null (serialized field
  continua fonte de verdade); senão `Resources.Load<CaveEcosystemBalanceSO>("CaveEcosystemBalance")`;
  log one-shot `[Cave][Wiring]` de origem (serialized/Resources) ou `Debug.LogWarning` se nenhum dos
  dois resolver, com instrução para rodar `CindarsHope/Inicializar Projeto`.
- Novo campo cache `_resolvedEcosystemBalance` + guard one-shot `_ecosystemBalanceResolveAttempted`,
  resolvido uma vez em `EnsureCollaborators()` (antes dos demais colaboradores, mesmo padrão do
  `_biomeArtResolver`).
- Todos os usos internos de `_ecosystemBalance` (construtor de `CaveEnemyMaterializer`, construtor de
  `CaveEnvironmentElementMaterializer`, `ApplyConflict`/`WireConflictSide`) passaram a ler
  `_resolvedEcosystemBalance`.
- A propriedade pública `EcosystemBalance` (consumida por `CaveLevelRuntimeController.ApplyInterMonsterConflict`)
  agora chama `EnsureCollaborators()` antes de retornar `_resolvedEcosystemBalance` — garante resolução
  mesmo se acessada antes de qualquer `Materialize()` nesta sessão.
- Nenhuma lógica de consumo do balance foi alterada — só a fonte de onde o valor vem.

### 2. Gerador materializa cópia em Resources — `GenerateCaveEcosystemBalance.cs`

- `Generate()` continua idempotente: NÃO sobrescreve `Assets/_Game/Data/Cave/CaveEcosystemBalance.asset`
  se já existir (preserva tuning manual).
- Nova etapa `MaterializeResourcesAsset(...)`: cria (se ausente) ou atualiza (se já existir)
  `Assets/_Game/Resources/CaveEcosystemBalance.asset`, sincronizando os valores do asset de Data via
  `EditorUtility.CopySerialized` — mesmo padrão de "cópia idempotente de valores" usado por
  `GenerateCaveEnvironmentElementProfiles.MaterializeResourcesDatabase`, adaptado para um SO escalar (não
  uma lista de referências, então `CopySerialized` em vez de sincronizar um array `_items`). Roda em toda
  chamada de `Generate()`, então tuning manual feito no asset de Data se propaga para a cópia de
  Resources na próxima `CindarsHope/Inicializar Projeto`.
- `Generate()` já era um `RunStep` registrado em `CindarsHopeMenu.InicializarProjeto()` (linha ~176) —
  nenhuma mudança de orquestração necessária.

## Fora de escopo (conforme instrução)

- Wiring de `_biomeArtProfiles`/`_environmentElementDatabase` na cena — já cobertos por fallback,
  redundante.
- Campo de água/minério no profile (schema) — não tocado.
- Planner/snapshot/CV03 — não tocados (só a fonte do balance que os planners recebem).
- `.unity`/`.prefab` YAML — não editado.

## Validação

```text
Validation method: dotnet build manual (per-project)
Command 1: dotnet build .\Assembly-CSharp.csproj
Exit code: 0
Assembly-CSharp: PASS (0 erros, 0 avisos)

Command 2: dotnet build .\Assembly-CSharp-Editor.csproj
Exit code: 0
Assembly-CSharp-Editor: PASS (0 erros; 1101 avisos CS0436 pré-existentes de conflito de tipo
Assembly-CSharp vs CindarsHope.Editor, não relacionados a esta mudança — mesmo padrão de warnings já
presente antes deste diff)
```

Phase 2 (Unity batchmode / `CindarsHope/Validate/Combat/Validate Combat Databases`): NOT RUN — fora do
escopo desta tarefa (sem Unity Editor disponível nesta sessão de shell). Play Mode: NOT RUN.

## Ação humana pendente (Inspector wiring / geração)

```text
Inspector wiring required (human action in Unity Editor):
- Rodar CindarsHope/Inicializar Projeto 1x (materializa/atualiza
  Assets/_Game/Resources/CaveEcosystemBalance.asset a partir de
  Assets/_Game/Data/Cave/CaveEcosystemBalance.asset).
- Entrar em Play Mode e confirmar via console o log
  "[Cave] CaveRuntimeMaterializer: ecosystem balance resolved from ..." (fonte esperada:
  "Resources/CaveEcosystemBalance" em cena sem o campo _ecosystemBalance wireado no Inspector,
  ou "serialized field" se alguém já ligou manualmente).
- Opcional (preferível a depender do fallback): ligar
  Assets/_Game/Data/Cave/CaveEcosystemBalance.asset no campo _ecosystemBalance do
  CaveRuntimeMaterializer na CaveScene.
```

Residual risk: a cópia em Resources reflete o asset de Data no momento em que
`CindarsHope/Inicializar Projeto` roda. Se alguém tunar o asset de Data e não rodar o gerador de novo,
a cópia de Resources fica desatualizada até a próxima geração (mesmo risco residual documentado para
`CaveEnvironmentElementDatabase`/`CaveBiomeArtProfileRegistry`).
