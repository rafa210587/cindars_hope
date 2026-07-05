# Baseline da Fase 0 — Rework Modular

Data de captura: 2026-07-05

Branch: `dev`

Commit de origem: `aed4f35c`

Status: `VALIDATED_PENDING_PUBLICATION`

## 1. Escopo e interpretação

Este baseline protege o estado anterior à modularização. Ele não declara que a dívida existente é
correta. O ratchet aceita os limites medidos por arquivo, permite redução e falha quando uma
ocorrência nova aparece ou quando um arquivo ultrapassa seu limite registrado.

Nenhum arquivo runtime, cena, prefab, asset ou `ProjectSettings` foi alterado para produzir esta
fase. Os únicos arquivos C# novos pertencem a `Assets/_Game/Tests/EditMode/`.

## 2. Superfície de código

| Métrica | Baseline |
|---|---:|
| Scripts em `Assets/_Game/Scripts` | 1.342 |
| Scripts runtime, excluindo `Editor/` | 1.187 |
| Scripts em `Editor/` | 155 |
| Assemblies `.asmdef` | 0 |
| Campos `[SerializeField]` em runtime | 829 |
| Usos de `FormerlySerializedAs` em runtime | 5 |

## 3. Ratchets arquiteturais

| Regra | Ocorrências aceitas no baseline |
|---|---:|
| `RuntimeInitializeOnLoadMethod` | 63 |
| declarações estáticas `Instance`/`Active`/`ActiveInstance` | 54 |
| buscas Unity globais | 37 |
| `Resources.Load`/`LoadAll` | 22 |
| leitura direta por `Input.Get*` | 207 |
| escrita direta de `Time.timeScale` | 6 |
| `SceneManager.LoadScene*` direto | 6 |
| queries `Physics2D.*All` | 2 |
| acesso global direto a `InventoryManager` | 0 |
| acesso global direto a managers de ouro/player | 0 |

Fontes executáveis:

- regras: `tools/architecture/architecture-ratchet-rules.tsv`;
- limites por arquivo: `tools/architecture/architecture-ratchet-baseline.tsv`;
- CLI: `tools/architecture/Test-ArchitectureRatchet.ps1`;
- EditMode: `Assets/_Game/Tests/EditMode/Architecture/Editor/ArchitectureRatchetTests.cs`.

O baseline não deve ser aumentado automaticamente. Qualquer aumento exige decisão arquitetural
explícita, justificativa e atualização deste documento e do handoff.

## 4. Cenas serializadas sensíveis

As três cenas abaixo são as únicas cenas de jogo habilitadas em
`ProjectSettings/EditorBuildSettings.asset`.

| Cena | GUID | Bytes | refs de script | GUIDs de script únicos | SHA-256 inicial |
|---|---|---:|---:|---:|---|
| `FarmScene.unity` | `fe632812dfa3466fb7e7d05679caaff9` | 4.840.914 | 262 | 77 | `c281ae07b1a2f19d820a08a51df38f9672360dbd5ded9da6b0adafb0b4f654b7` |
| `TownScene.unity` | `28d3fe07fc6000e44b3559a8352f0c66` | 27.339.562 | 447 | 72 | `535fc2abd0bd8cfcc556d23719a0263582749d0f573746a937cf5fe276fe294f` |
| `CaveScene.unity` | `5252d3db051ecd04aa0740f571e5419c` | 53.407 | 41 | 40 | `d66ce351a6fdcf092f4fcda6e9410aac7e595d588ab5dbfc9201e9152af2f6fc` |

Os hashes são evidência do snapshot, não um bloqueio permanente: mudanças legítimas de cena podem
alterá-los. Paths e GUIDs dos arquivos mais sensíveis são protegidos por
`tools/architecture/serialized-guid-baseline.tsv` e pelo teste EditMode.

`Assets/_Recovery/*.unity` e cenas de exemplo do TextMesh Pro não fazem parte da superfície de jogo.

## 5. Arquivos serializados e de composição críticos

O baseline de GUID cobre 16 entradas:

- três cenas de build;
- três geradores canônicos de cena;
- `GameBootstrap`;
- contratos/orquestrador de save;
- estado de inventário e player;
- `PlayerController`;
- router de transição;
- installers runtime de Farm, Town e Cave.

Também são sensíveis, mesmo sem GUID próprio no baseline:

- `ProjectSettings/EditorBuildSettings.asset`;
- `ProjectSettings/TagManager.asset`;
- `ProjectSettings/ProjectSettings.asset`;
- `ProjectSettings/GraphicsSettings.asset`;
- qualquer `.unity`, `.prefab`, `.asset` e seu `.meta` correspondente;
- todos os campos serializados em `GameSaveData` e DTOs alcançáveis.

Mover script/asset Unity exige preservar seu `.meta`. Renomear campo serializado exige
`FormerlySerializedAs` ou migração explícita. YAML Unity não deve ser editado manualmente.

## 6. Contrato de save

| Item | Baseline |
|---|---:|
| Schema atual | 5 |
| Migrações sequenciais | 4 (`v1→v2→v3→v4→v5`) |
| Implementações `*SectionProvider.cs` | 31 |
| Fixtures versionadas | 5 (`v1` a `v5`) |

Fixtures:

- `tools/architecture/save-fixtures/save-v1.json`;
- `tools/architecture/save-fixtures/save-v2.json`;
- `tools/architecture/save-fixtures/save-v3.json`;
- `tools/architecture/save-fixtures/save-v4.json`;
- `tools/architecture/save-fixtures/save-v5.json`.

`SaveCompatibilityFixtureTests` exige que cada fixture legada migre até v5, que o fixture atual
faça round-trip e que o inventário v1 preserve sua quantidade durante a conversão para slots.

## 7. Comandos reproduzíveis

```powershell
& .\tools\architecture\Test-ArchitectureRatchet.ps1
dotnet build .\Assembly-CSharp.csproj
dotnet build .\Assembly-CSharp-Editor.csproj
```

O gate Unity deve executar ao menos:

- `CindarsHope.Tests.EditMode.Architecture.ArchitectureRatchetTests`;
- `CindarsHope.Tests.EditMode.Save.SaveCompatibilityFixtureTests`;
- a suíte EditMode completa para comparar com o baseline anterior.

## 8. Dívida conhecida fora do escopo

O baseline anterior encontrou 13 falhas EditMode: sete de layout da cidade, duas de
`ProjectValidationRunner` e quatro de catálogos de itens/flechas. A Fase 0 não altera os sistemas
relacionados e não converte essas falhas em PASS.

O validador de documentação também possuía falhas no lote publicado. O fechamento deve distinguir
falhas preexistentes de regressões introduzidas por esta fase.

## 9. Resultado da validação

| Gate | Resultado |
|---|---|
| Ratchet CLI | PASS, exit 0; dez regras nos limites e 16 GUIDs preservados |
| `Assembly-CSharp.csproj` | PASS, exit 0; 5 warnings preexistentes |
| `Assembly-CSharp-Editor.csproj` | PASS, exit 0; 7 warnings preexistentes |
| EditMode arquitetura/GUID | PASS, 2/2 |
| EditMode fixtures de save | PASS, 6/6 |
| EditMode completa | 58/71 PASS; as mesmas 13 falhas do baseline anterior |
| Comparação de falhas | zero falhas adicionadas, zero removidas |
| Docs validator | exit 1 pelas mesmas dívidas de specs/harness já registradas; nenhum erro nos documentos desta fase |
| PlayMode | não executado; fase altera somente testes, ferramentas e documentação |

Builds com `--no-restore` retornam `NETSDK1004` depois que o Unity encerra e limpa `Temp/obj`. A
repetição com restore habilitado retornou exit 0 nos dois projetos. Isso é registrado como condição
de ambiente, não como falha de compilação do código.

Evidências Unity:

- `TestResults/modularization-phase0-architecture.xml`;
- `TestResults/modularization-phase0-save.xml`;
- `TestResults/modularization-phase0-full-editmode.xml`.

## 10. Critério de fechamento

A Fase 0 só muda para `COMPLETE` quando:

1. CLI ratchet retorna exit 0 — **concluído**;
2. builds runtime/editor retornam exit 0 — **concluído**;
3. testes novos compilam e são executados no Unity Test Runner — **concluído**;
4. o resultado da suíte completa é comparado ao baseline anterior — **concluído**;
5. working tree contém apenas arquivos desta fase — **concluído**;
6. plano e handoff registram os resultados reais — **concluído**;
7. commits são publicados e `origin/dev...HEAD` termina em `0 0` — **pendente**.
