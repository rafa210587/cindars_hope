# Execution Report — spec_content_enemy_status_kit_ids_v1

> **Status da spec:** BUILD_VALIDATED (fallback dotnet) — `UNITY_VALIDATED` e `EditMode` **NOT RUN /
> DEFERRED_TO_HUMAN** (batchmode Unity não executado; ver seção de bloqueios).

## Fase 0 — Auditoria (re-executada)

Comando (PowerShell/Bash, resultado idêntico):

```text
grep -rEn "status_(slow|burn|bleed|chill|poison|confuse|root)_minor|status_(haste|shield|frenzy|regen|guard)" \
  Assets/_Game/Data/Enemies/Actions/
→ 43 linhas / 43 arquivos .asset distintos
```

Contagem por ID (confirma exatamente a tabela do §9 da spec):

```text
status_slow_minor    10   status_haste     5   status_bleed_minor  2
status_burn_minor     9   status_frenzy    3   status_shield       2
status_chill_minor    5   status_poison_minor 3   status_root_minor 1
status_confuse_minor  1   status_regen     1   status_guard        1
```

**Schema real de `StatusEffectSO`** (`Assets/_Game/Scripts/Combat/StatusEffect/StatusEffectSO.cs`) —
**diverge** do pseudo-schema do §16.1 da spec (que assumia `Kind/Magnitude/DurationSeconds/MaxStacks`,
campos que **não existem**). Campos reais:

```csharp
public string Id;
public string DisplayName;
[TextArea] public string Description;
public StatusEffectType Type;          // enum, não "Kind" genérico
public int DurationTurns;              // não "DurationSeconds"
public int DamagePerTurn;
public Color VisualColor;
[Range(0f,2f)] public float MoveSpeedMultiplier;
public float BehaviorOverrideSeconds;
[Range(1f,5f)] public float DurabilityWearMultiplier;
// Não existe campo "MaxStacks" em lugar nenhum do schema.
```

`StatusEffectDatabaseSO` é só `DataRegistrySO<StatusEffectSO>` (sem campo/const próprio).

**Gerador localizado:** `Assets/_Game/Scripts/Editor/Combat/GenerateCanonicalStatusEffects.cs` —
`GenerateCanonicalStatusEffects.Generate()`. Já registrado como `RunStep` em
`CindarsHopeMenu.cs` (linhas 94-95 e ~360) dentro de **`CindarsHope/Inicializar Projeto`** e
**`CindarsHope/Reparar e Reconstruir`** — nenhum `[MenuItem]` novo foi necessário (rule
`editor-generation-orchestration` satisfeita por reuso).

**Decisão Fase 0: Caminho A** (adicionar as 12 definições no gerador) — confirmado viável e
executado. Caminho B não avaliado (não necessário).

## Fase 1 — Dados (implementado)

Arquivo: `Assets/_Game/Scripts/Editor/Combat/GenerateCanonicalStatusEffects.cs` (modificado, não
criado — dentro de `Assets/_Game/Scripts/Editor/**`, permitido).

- 12 IDs como `public const string` (rule id-stability, prefixo `status_`):
  `StatusSlowMinor, StatusBurnMinor, StatusChillMinor, StatusPoisonMinor, StatusBleedMinor,
  StatusConfuseMinor, StatusRootMinor, StatusHaste, StatusShield, StatusFrenzy, StatusRegen,
  StatusGuard`.
- Tabela `MinorAndBuffDefs` (Id, ParentId ou `null`, nome PT-BR).
- Novo passo em `Generate()`, após o bloco dos 13 status canônicos: para cada def, `AssetDatabase`
  load-or-create idempotente (mesmo padrão do bloco existente) + `ApplyMinorOrBuffFields`, depois
  adicionados ao `assets` passado para `RegisterInDatabase` (`AddOrUpdate` idempotente já existente —
  cobre a edge case "ID colide com existente").
- Fórmula adaptada ao schema real (§20 da spec era pseudo-código sobre um schema que não existe):
  - `DurationTurns` = igual ao do pai (minor não encurta duração — só reduz o efeito).
  - `DamagePerTurn` = `round(pai.DamagePerTurn * 0.60)` para burn/poison/bleed minor (0 nos demais,
    pois o pai já é 0).
  - `MoveSpeedMultiplier` = `1 - (1 - pai.MoveSpeedMultiplier) * 0.60` (reduz a penalidade de
    velocidade em 60%) para chill/slow/root minor.
  - `BehaviorOverrideSeconds` = `pai.BehaviorOverrideSeconds * 0.60` (confuse_minor).
  - Consts nomeadas (no-magic-balance-values): `MinorEffectScale = 0.60f`,
    `FallbackMinorDamagePerTurn = 1`, `FallbackMinorDurationTurns = 3` (só disparam se um `_minor`
    referenciar pai ausente — não ocorre hoje, os 7 pais existem), `BuffDurationTurns = 5` (para os 5
    buffs sem pai).
- **Buffs sem `Kind=Buff` no enum real:** `StatusEffectType` não tem um valor `Buff` dedicado.
  Adicioná-lo exigiria editar `StatusEffectSO.cs`, **fora dos "Arquivos permitidos" da spec** (só
  `StatusEffectDatabaseSO.cs` é permitido em `Combat/Data`, e `StatusEffectSO.cs` fica em
  `Combat/StatusEffect/`, um arquivo diferente e não listado). Decisão: reaproveitar
  `StatusEffectType.Vulnerable` — declarado no enum mas **sem nenhum uso real** (confirmado via grep:
  nenhum asset atribui `Type = StatusEffectType.Vulnerable` e nenhum `switch` em
  `StatusEffectSemantics` o trata; o `default` de todos os switches é neutro). Documentado no código
  como reuso deliberado, sem introduzir comportamento novo. Este é o único desvio de arquitetura desta
  execução — reportado explicitamente por não estar coberto pelos "Arquivos permitidos" literais.
- `StatusEffectDatabaseSO.cs` **não foi modificado** — não faltava const/acessor de ID (o registro é
  via `RegisterInDatabase`/`AddOrUpdate`-like merge já existente, reused).

## Fase 2 — Geração (`CindarsHope/Inicializar Projeto`)

**BLOCKED / DEFERRED_TO_HUMAN.** Requer Unity Editor batchmode. Verificado nesta execução que **não
há processo Unity rodando** (`Get-Process -Name "*Unity*"` e `tasklist | findstr unity` vazios), mas
`Inicializar Projeto` é uma operação **destrutiva** (recria as 3 cenas, rule
`editor-generation-orchestration`) e de longa duração — fora do escopo de execução autônoma segura
desta tarefa; a instrução de execução também pré-definiu este passo como
`DEFERRED_TO_HUMAN` sempre que batchmode for necessário. Passos exatos para o humano:

```powershell
Set-Location 'D:\Projetos\Cindars_Hope\cindars_hope'
# Nota: RunUnityCompileValidation.ps1 / RunUnityEditModeTests.ps1 têm default de
# Unity 6000.4.7f1; o Unity instalado é 6000.5.7f1 — passe -UnityEditorPath/-UnityPath.
& "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" `
  -batchmode -quit -nographics `
  -projectPath "D:\Projetos\Cindars_Hope\cindars_hope" `
  -executeMethod CindarsHope.Editor.CindarsHopeMenu.InicializarProjeto `
  -logFile ".\Logs\inicializar-projeto.log"
```

(Ou via Editor aberto: menu `CindarsHope/Inicializar Projeto`.) Depois de rodar, confirmar por `git
diff` que só mudaram `.asset` sob `Assets/_Game/Data/Combat/StatusEffects/` (mais o
`StatusEffectDatabase.asset`) — **não** cenas (edge case §23).

## Fase 3 — Testes/validação

### EditMode test — criado

`Assets/_Game/Tests/EditMode/Combat/EnemyKitStatusIdsTests.cs` (novo, dentro de
`Assets/_Game/Tests/EditMode/Combat/**`, permitido). 12 métodos `[Test]`, um assert dedicado por ID
(`StatusSlowMinor_Resolves` … `StatusGuard_Resolves`), via `StatusEffectDatabaseSO.TryGetById`.

**Compile (fallback dotnet) — PASS, exit 0:**

```text
dotnet build .\CindarsHope.Editor.csproj --no-restore        → Compilação com êxito. 0 Erro(s). EXITCODE: 0
dotnet build .\CindarsHope.Tests.EditMode.csproj --no-restore → Compilação com êxito. 0 Erro(s). EXITCODE: 0
```

Nota: `CindarsHope.Tests.EditMode.csproj` usa `EnableDefaultItems=false` (lista explícita de
`<Compile Include>`, regenerada pelo Unity ao abrir o projeto — não rastreado pelo git, confirmado via
`git status`). O arquivo novo não estava na lista; foi adicionado manualmente uma linha
`<Compile Include="Assets\_Game\Tests\EditMode\Combat\EnemyKitStatusIdsTests.cs" />` só para permitir
a validação de compile via dotnet nesta execução — o Unity vai regenerar esse csproj normalmente na
próxima vez que abrir o projeto (arquivo de build transitório, fora do controle de versão).

**Execução real do EditMode (NUnit, 12 asserts) — NOT RUN / DEFERRED_TO_HUMAN**, pois depende dos
assets gerados na Fase 2 (sem eles, os 12 asserts falhariam por ausência real de dado, não por bug de
teste). Comando exato para o humano, **depois** da Fase 2:

```powershell
Set-Location 'D:\Projetos\Cindars_Hope\cindars_hope'
.\tools\unity\RunUnityEditModeTests.ps1 `
  -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" `
  -TestFilter "CindarsHope.Tests.EditMode.Combat.EnemyKitStatusIdsTests"
```

Critério: `Total: 12, Passed: 12, Failed: 0`, exit code 0.

### `CindarsHope/Validar Projeto` (`ValidateEnemyAttackKits`, `ValidateStatusEffectDatabase`)

**NOT RUN / DEFERRED_TO_HUMAN** — mesma dependência de batchmode + Fase 2. Comando exato:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" `
  -batchmode -quit -nographics `
  -projectPath "D:\Projetos\Cindars_Hope\cindars_hope" `
  -executeMethod CindarsHope.Editor.CindarsHopeMenu.ValidarProjeto `
  -logFile ".\Logs\validar-projeto.log"
```

Critério: log sem `[ERR]` de `EnemyAttackKits Validation` nem de `ValidateStatusEffectDatabase`
(0 erros, antes eram 43).

## Fase 4 — Relatório (este arquivo)

## Bloco de validação (rule validation-truth)

```text
Validation method: dotnet build (fallback C#) — CindarsHope.Editor.csproj, CindarsHope.Tests.EditMode.csproj
Exit code: 0 (ambos)
CindarsHope.Editor: PASS
CindarsHope.Tests.EditMode: PASS
Unity batchmode compile (RunUnityCompileValidation.ps1): NOT RUN — deferred (ver Fase 2/3)
EditMode NUnit (12 asserts, EnemyKitStatusIdsTests): NOT RUN / DEFERRED_TO_HUMAN — depende de asset generation (Fase 2)
Validar Projeto (ValidateEnemyAttackKits, ValidateStatusEffectDatabase): NOT RUN / DEFERRED_TO_HUMAN
Result artifact: nenhum (bloqueado); passos exatos documentados acima
```

## Critérios de aceite (§14) — status

- **14.1** (12 IDs resolvem) — teste escrito e compila; execução real **DEFERRED_TO_HUMAN**.
- **14.2** (validador de kits limpo) — **DEFERRED_TO_HUMAN** (depende de Fase 2 + batchmode).
- **14.3** (sem regressão) — `git diff` mostra só ADIÇÕES em
  `Assets/_Game/Scripts/Editor/Combat/GenerateCanonicalStatusEffects.cs` (114 linhas inseridas, 0
  removidas/alteradas) — nenhum ID existente renomeado/removido. `ValidateStatusEffectDatabase`
  em si **NOT RUN** (mesmo motivo).
- **14.4** (compila) — fallback dotnet PASS (exit 0); Unity batchmode compile **NOT RUN**.

## O que NÃO foi feito (explícito)

- `CindarsHope/Inicializar Projeto` **não foi executado** — os 12 `.asset` de status NÃO existem em
  disco ainda; `StatusEffectDatabase.asset` NÃO contém os 12 IDs ainda.
- `CindarsHope/Validar Projeto` **não foi executado** — não há evidência de que
  `ValidateEnemyAttackKits` caiu de 43 para 0 erros; residual risk documentado, não PASS.
- EditMode NUnit run real (via Unity) **não foi executado** — só a compilação C# do teste foi
  verificada.
- `Assets/_Game/Data/Enemies/Actions/**` **não foi tocado** (caminho B não usado, conforme default A).
- Nenhum commit, nenhum push.
- Nenhum sub-agente foi disparado; toda a execução foi feita diretamente nesta sessão.

## Arquivos alterados

```text
MODIFICADO: Assets/_Game/Scripts/Editor/Combat/GenerateCanonicalStatusEffects.cs   (+114 linhas)
CRIADO:     Assets/_Game/Tests/EditMode/Combat/EnemyKitStatusIdsTests.cs
CRIADO:     docs/validation/spec_content_enemy_status_kit_ids_v1_execution_report.md
(build artifact, não versionado) CindarsHope.Tests.EditMode.csproj — 1 linha de <Compile Include> adicionada p/ validar compile do teste novo; será regenerada pelo Unity
```
