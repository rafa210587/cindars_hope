# Cindar's Hope — SpecKit Architecture Reorg Specs v3.0

Pacote de specs declarativas para reorganização incremental do projeto, com controle explícito de paralelização, drift e alucinação.

Esta versão v3.0 reforça:

- leitura obrigatória de fontes reais antes de alterar código;
- escopo permitido/proibido por spec;
- stop conditions explícitas;
- modo sequencial seguro e modo multi-branch paralelo;
- matriz de dependência por wave/spec;
- relatórios de execução isolados para subagents;
- consolidação documental só por orchestrator quando houver paralelização.

## Regras globais anti-drift

1. Não inferir estado do projeto por memória ou conversa anterior.
2. Não criar sistema paralelo se existir sistema vigente adaptável.
3. Não remover código, asset, prefab, scene object ou docs antigas sem spec explicitamente autorizando remoção.
4. Não editar fora do escopo permitido da spec.
5. Não editar `docs_old/**`, `specs/**` nem `spec/**`.
6. Não recriar `specs/` ou `spec/` na raiz.
7. Não declarar Unity/Play Mode/prefab import como validado se não executou Unity.
8. Não usar `GameObject.Find()` nem `FindObjectOfType()`.
9. Não serializar Unity refs em DTOs de save.
10. Se uma validação falhar, corrigir a causa antes de seguir.
11. Se precisar de arquivo fora de escopo, parar e registrar bloqueio.
12. Se encontrar divergência entre spec e repo, o repo vence; atualizar relatório e parar se a mudança deixar de ser segura.

## Modo de execução recomendado

### Modo A — Sequencial seguro

Use quando estiver em uma única branch:

```text
SPEC_00 -> SPEC_01 -> SPEC_02 -> SPEC_03 -> SPEC_04 -> SPEC_05 -> SPEC_06 -> SPEC_07 -> SPEC_08 -> SPEC_09 -> SPEC_10 -> SPEC_11 -> SPEC_12
```

### Modo B — Multi-branch com subagents

Use apenas se aceitar merge controlado.

Regras:

1. Cada spec paralela roda em branch própria.
2. Subagents paralelos não editam `PROJECT_LOG.md` nem `docs/IMPLEMENTATION_STATUS.md`.
3. Subagents paralelos escrevem somente relatório em `docs/validation/<SPEC_ID>_execution_report.md`.
4. O orchestrator mergeia branches e atualiza documentação consolidada.
5. Se dois subagents alterarem `.csproj`, `GameBootstrap`, `SaveManager`, `PlayerAttackController`, `ProjectileBehaviour`, prefab ou registry asset, considerar conflito esperado e mergear sequencialmente.

## Grafo de execução

```text
SPEC_00 Strategy
  -> SPEC_01 Wave0A Validator Foundation
      -> [SPEC_02 Wave0B Projectile Validator]
      -> [SPEC_03 Wave0C Combat Database Validators]
  -> SPEC_04 Wave1 Legacy Combat Quarantine
  -> SPEC_05 Wave2A Combat Service Extraction
  -> SPEC_06 Wave2B Projectile Spawn Service
  -> SPEC_07 Wave2C Bow/Arrow/Spell Services
  -> SPEC_08 Wave3 Item Equipment Contracts
  -> SPEC_09 Wave4 Status Effect Runtime
  -> SPEC_10 Wave5 Save Providers
  -> SPEC_11 Wave6 Bootstrap Installers
  -> SPEC_12 Wave7 Closeout Validation
```

## Matriz de paralelização validada

| Spec | Wave | Paralelização | Pode executar junto com | Condição | Motivo |
|---|---|---|---|---|---|
| SPEC_00 | Strategy | Não | Nenhuma | Primeira sempre | Define regras do pacote |
| SPEC_01 | 0A | Não | Nenhuma | Depois da 00 | Fundação usada por validators |
| SPEC_02 | 0B | Sim, limitada | SPEC_03 | Branch própria; sem editar PROJECT_LOG/IMPLEMENTATION_STATUS | Domínio projectile/prefab isolável |
| SPEC_03 | 0C | Sim, limitada | SPEC_02 | Branch própria; sem editar PROJECT_LOG/IMPLEMENTATION_STATUS | Domínio databases isolável |
| SPEC_04 | 1 | Não | Nenhuma | Depois dos relatórios 02/03 | Quarentena depende dos achados |
| SPEC_05 | 2A | Não | Nenhuma | Depois da 04 | Toca PlayerAttackController |
| SPEC_06 | 2B | Não | Nenhuma | Depois da 05 | Depende da extração de combate |
| SPEC_07 | 2C | Não | Nenhuma | Depois da 06 | Regras bow/arrow/fireball críticas |
| SPEC_08 | 3 | Não | Nenhuma | Depois da 07 | Contratos afetam item/equipment/combat |
| SPEC_09 | 4 | Parcial só análise | SPEC_10 análise, não implementação | Implementação depois da 08 | Status pode afetar save/combat |
| SPEC_10 | 5 | Parcial só análise | SPEC_09 análise, não implementação | Implementação depois da 08/09 | SaveManager central |
| SPEC_11 | 6 | Não | Nenhuma | Depois da 10 | GameBootstrap central |
| SPEC_12 | 7 | Não | Nenhuma | Última sempre | Consolidação final |

## Subagents recomendados

```text
architecture-reviewer       -> SPEC_00 e SPEC_12
validator-foundation-agent  -> SPEC_01
projectile-validator-agent  -> SPEC_02
combat-data-validator-agent -> SPEC_03
legacy-quarantine-agent     -> SPEC_04
combat-refactor-agent       -> SPEC_05, SPEC_06, SPEC_07
data-contract-agent         -> SPEC_08
status-runtime-agent        -> SPEC_09
save-architecture-agent     -> SPEC_10
bootstrap-installer-agent   -> SPEC_11
docs-curator                -> consolidação PROJECT_LOG/IMPLEMENTATION_STATUS
non-regression-auditor      -> revisão final de cada merge
```

## Política de relatórios

Toda spec deve gerar:

```text
docs/validation/<SPEC_ID>_execution_report.md
```

Em modo sequencial, também atualizar:

```text
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md se status oficial mudar
```

Em modo paralelo, só o orchestrator atualiza `PROJECT_LOG.md` e `docs/IMPLEMENTATION_STATUS.md` depois dos merges.

## Validação mínima por spec

```powershell
dotnet restore .\Assembly-CSharp.csproj
dotnet restore .\Assembly-CSharp-Editor.csproj
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
tools/docs/validate_docs.ps1
```

Se alterou runtime Unity, prefab, scene, ScriptableObject, Editor menu ou validator:

```powershell
tools/unity/RunUnityCompileValidation.ps1
tools/unity/ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

Se Unity não rodar, registrar como NOT RUN. Não escrever PASS.
