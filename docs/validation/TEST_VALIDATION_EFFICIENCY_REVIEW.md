# Revisão de eficiência dos testes e validações

Data: 2026-09-08 (UTC). Natureza: auditoria e proposta; não altera os gates vigentes.

Nota posterior: recomendações foram aplicadas na continuação autorizada. Consultar
[execução das melhorias](QUALITY_IMPROVEMENTS_EXECUTION.md) para regras vigentes,
cortes efetivos e evidência; os trechos abaixo descrevem o estado no momento da revisão.

## Conclusão

Existe excesso de repetição e de exigências indiscriminadas no workflow. O número de testes, isoladamente, não é o problema: a execução registrada de 2.904 casos EditMode levou 8,25 segundos dentro da suíte. Recomendo reduzir lançamentos do Unity, builds repetidos e verificações documentais fora do escopo antes de reduzir cobertura comportamental.

Há oito candidatos claros à remoção por apenas testar atribuição de propriedades ou `List<T>`. Outros grupos devem ser consolidados ou corrigidos. Os verificadores de Unity também têm falsos resultados positivos: simplificar a rotina depende de preservar um resultado confiável, não apenas de executar menos comandos.

Nenhum teste, regra, runner ou asset foi alterado nesta revisão. As recomendações abaixo ainda não estão implementadas.

## Evidência e limites

- Inventário textual dos 240 arquivos de testes; amostragem semântica de UI, Save, arquitetura, escala de inimigos, Inventory e das quatro falhas de farm registradas.
- Inspeção dos runners PowerShell, orquestradores de Editor, regras, skills, comandos e matriz de validação.
- Reutilizados XMLs e logs de `TestResults/solid-ai/` e os logs originais em `C:/Users/Rafa/AppData/Local/Temp/cindars-architecture-baseline-20260908/`.
- Não houve nova execução de Unity, build ou suíte. Não foi feita auditoria individual de todos os 2.904 casos nem mutation testing.
- As medições descrevem a execução anterior. O workspace contém alterações em andamento; elas não certificam o estado atual.

| Execução registrada | Casos | Tempo da suíte no XML | Intervalo observado do log original |
|---|---:|---:|---:|
| EditMode baseline | 2.888 | 10,87 s | 21,58 s |
| Caracterização antes do refactor | 20 | 0,29 s | 14,87 s |
| EditMode focado depois do refactor | 36 | 0,36 s | 28,69 s |
| EditMode completo depois do refactor | 2.904 | 8,25 s | 25,86 s |
| PlayMode de composição | 2 | 14,85 s | 30,74 s |

O intervalo do log usa `CreationTimeUtc → LastWriteTimeUtc`; não é um cronômetro exato do processo. Cache, importação e recompilação diferem entre rodadas. Portanto, não se pode concluir que testes focados sejam intrinsecamente mais lentos que a suíte completa, nem projetar uma porcentagem exata de economia.

No EditMode completo, 87,7% dos casos levaram menos de 1 ms e 98,5% menos de 10 ms. Os 49 casos dos dois arquivos de UI com candidatos à remoção somam aproximadamente 0,0111 s. Removê-los melhora clareza e manutenção, mas pouco altera o tempo de execução.

Os sete builds posteriores ao restore somaram 14,42 s reportados pelo MSBuild. Outra rodada no `root-build.log` somou 15,50 s, excluindo restore e overhead dos processos. Repetir uma rodada apenas para conferir a narração do executor é evitável quando os artefatos e os insumos efetivamente validados são verificáveis.

## Cortes e reduções recomendados no workflow

| Prioridade | Excesso observado | Proposta | Condição para preservar confiança |
|---|---|---|---|
| Alta | Worker compila, compila por fase e o orquestrador compila novamente | Uma execução válida por conjunto de alterações; o revisor inspeciona evidências e diff | Reexecutar se mudarem código, dependências, defines, ferramenta ou escopo; evidência ausente/incompatível exige execução |
| Alta | `validate_docs` separado e novamente dentro de `run_strict_validation` | Uma validação documental por conjunto de insumos | Editar os próprios documentos depois invalida a evidência documental, sem invalidar automaticamente builds |
| Alta | Compile-only antes/depois de Test Runner no mesmo código | Usar a compilação realizada pelo Test Runner como evidência de compilação do Editor | Exigir execução real e recente, identificar assemblies/configuração e preservar diagnóstico de compile; não cobre build Player/IL2CPP |
| Alta | Full docs/build/Unity exigidos sem distinguir tipo de mudança | Selecionar por escopo e risco, conforme matriz proposta abaixo | Gate global de integração continua existindo; diagnóstico antigo não vira PASS por ser antigo |
| Média | Restore de sete projetos e build dos sete em processos separados | Avaliar build pelo grafo da solução, uma invocação canônica | Preservar os sete projetos; medir e validar a substituição antes de alterar o runner |
| Média | Scanner de arquitetura em PowerShell e novamente no EditMode | Uma implementação canônica dos checks de fonte | Manter checks que inspecionam assemblies realmente compiladas; garantir chamada obrigatória e exit code |
| Média | PlayMode e cenário humano para refactor puro, sem risco novo de integração | Condicionar a lifecycle, cena, input, física, serialização, UX e wiring | Refactor ainda exige evidência comportamental pertinente; código existente coberto pode reutilizar seus testes |
| Média | Cada spec exige um checklist humano que repete o mesmo fluxo | Um cenário de integração por fluxo/wave, com vínculo aos critérios afetados | Cenário escrito não equivale a teste executado; preservar pendências humanas de visual, input e feel |
| Baixa | Scanners de documentação avaliam repetidamente relatórios históricos | Escopo alterado no ciclo local; auditoria global no fechamento de integração/harness | Mudança em schema, índice ou validador amplia o conjunto afetado; baseline permanece visível |

Não considero desperdício a caracterização antes e depois de um refactor relevante: ela compara dois comportamentos. Também não proponho eliminar a suíte completa final de uma integração ampla. Como a execução dos casos é rápida, criar agora outro framework de testes só para sair do Unity provavelmente adicionaria manutenção sem atacar o maior custo observado.

## Oito testes candidatos à remoção

Os nomes são da implementação inspecionada. Nos casos sem cobertura equivalente examinada, a remoção elimina uma falsa impressão de cobertura; não prova que a funcionalidade esteja protegida.

| Arquivo em `Assets/_Game/Tests/EditMode/` | Teste | Motivo e proteção pertinente |
|---|---|---|
| `UI/InventoryEquipmentTooltipTests.cs:28` | `InventoryItemViewModel_QuestItem_Fields` | Atribui `IsQuestItem=true` e lê true. Manter o guard que realmente impede vender/dropar quest item. |
| `UI/InventoryEquipmentTooltipTests.cs:178` | `EquipmentComparison_Warnings_Settable` | Adiciona string à lista e verifica Contains. Se a produção de warnings mudar, exercitar o produtor. |
| `UI/InventoryEquipmentTooltipTests.cs:195` | `ItemTooltipViewModel_QuestKeyWarnings_Settable` | Adiciona warning e verifica Count. A política de visibilidade é um contrato separado e deve permanecer. |
| `UI/MenuProjectionTests.cs:86` | `Crafting_CanCraft_FalseWhenMissingMaterials` | O próprio teste define `CanCraft=false`; não consulta materiais. A proteção real deve acionar a regra de crafting. |
| `UI/MenuProjectionTests.cs:105` | `SkillTree_HasFiveInitialTabs` | O teste adiciona cinco tabs e conta cinco. Se a quantidade for contrato vigente, testar a projeção/catálogo que constrói as tabs. |
| `UI/MenuProjectionTests.cs:121` | `SkillTree_RespecHidden_WhenNotUnlocked` | Define `RespecAvailable=false`; não avalia unlock nem ocultação. |
| `UI/MenuProjectionTests.cs:128` | `SkillTree_LockedNode_HasLockReason` | Atribui estado e motivo. Não executa prerequisites para produzir lock/reason. |
| `UI/MenuProjectionTests.cs:145` | `QuestLog_MenuState_CanSwitchTabs` | Atribui `SelectedTab` e lê a propriedade. Navegação real deve ser coberta pelo fluxo de UI. |

`InventoryItemViewModel_HasRequiredFields` merece reformulação, não remoção automática: além de setters, verifica um default que pode ser um contrato legítimo.

## Consolidar ou substituir

- **ArchitectureRatchetTests.cs:31,71,110:** regex de dívida, GUIDs e busca por predefined assembly se sobrepõem a scripts em `tools/architecture/`. A fixture inteira levou 1,458 s; o maior scanner, 1,188 s. Escolher uma execução canônica, sem apagar as invariantes.
- **ArchitectureRatchetTests.cs:141,336:** conservar as verificações de assembly e ausência de referências compiladas a Unity. Revisar a lista exata de arquivos e a busca textual por `UnityEngine`, que também encontra comentários. `noEngineReferences=true` já impõe parte do contrato no compilador.
- **Save/SaveSectionProviderTests.cs:45,52,131,138:** testes de DTO nulo/tipo incorreto usam manager nulo; o provider retorna antes de avaliar o DTO. Consolidar os casos de manager ausente e exercitar DTO inválido com manager válido e estado sentinela. O registry com stubs não substitui os providers reais.
- **Combat/EnemyScaleResolverPlayerRelativeTests.cs:172:** há sobreposição entre tabela parametrizada e casos individuais de tamanhos/roles. Consolidar a matriz preservando precedência boss/miniboss e collider. Em `:128`, chamar a mesma função duas vezes não prova integração entre codex e materializer.
- **UI/Input/InputFocusModalRoutingTests.cs:** parametrizar a matriz foco × ação pode reduzir código de teste. Manter os casos que impedem movimento/combate/hotbar com modal aberto e as exceções de Debug/Gameplay.
- **UI/Editor/HudPhaseOneTests.cs:12:** proibir qualquer método `Update` não demonstra ausência de polling inútil. Preferir contrato de refresh/subscription ou gate arquitetural apropriado; um `Update` legítimo não deve falhar só pelo nome.
- **NPC/BirthdayInnTests.cs:226:** proibir dois nomes de classe não prova que Friendship seja o único dono da reputação. Consolidar a decisão ADR-0017 no gate arquitetural/fluxo real. Esse teste levou 0,287 s.

## Proteções que devem permanecer

- Save compatibility/migrations, gravação atômica, backup, recuperação e round-trip: são contratos distintos.
- Transações de compra/inventário, preservação de quantidades e eventos, idempotência de rewards.
- Núcleo de `InventorySlotOperationsTests` e fachada de `InventorySlotMoveMergeTests`: regras locais versus integração de catálogo/aggregates/eventos. Evitar copiar toda a matriz pura para a fachada.
- Semântica/lifetime de EventBus e testes de alocação: verificam riscos diferentes.
- Cave seed/snapshot/replay e amostragem de distribuição: não diminuir amostras apenas porque são os testes mais lentos. Um caso estatístico de 200 níveis levou cerca de 1 s.
- Dois PlayMode de composição: persistência/ownership do root e carregamento das cenas reais sem missing scripts. Não substituem aceitação humana de gameplay e visual.

## Falhas de farm: reconciliar, não apagar para obter verde

A execução anterior teve 2.900 passes e quatro falhas, iguais à baseline. Isso não certifica que o working tree atual ainda produza o mesmo resultado.

| Teste | Decisão recomendada |
|---|---|
| `FarmDecorationPlannerTests.Plan_AllBiomesAreWithinDeclaredRanges` | Manter comparação da saída real com faixas; reconciliar densidade de WaterEdge com conteúdo aprovado. |
| `FarmSceneCompositionContractTests.BridgeVisualScale_IsReducedWithoutDefiningPhysicalFootprint` | Reavaliar o valor artístico exato histórico de Y=0,8 versus 1,4 observado. O teste não verifica a independência física prometida pelo nome; substituir pelo contrato aprovado de escala/footprint. |
| `FarmSceneCompositionContractTests.ScaleRanges_AcceptApprovedValuesAndRejectBridgeOutlier` | Manter aceitação/rejeição de ranges; reconciliar valores aprovados, incluindo escala de árvore. Não alterar expected apenas para passar. |
| `FarmSceneSpatialContractTests.BlockingFootprints_AreNonArable` | Manter proteção de áreas bloqueadas. A falha registrada está na expectativa final de ponte arável; reconciliar design. Retângulos montados no teste não provam wiring real da cena. |

## Corrigir a confiabilidade antes de depender de menos gates

1. **O menu Unity pode anunciar passos OK apesar de falhas.** `CindarsHopeMenu.cs:534` conta sucesso quando a `Action` não lança exception. Muitos validadores retornam report/boolean ou apenas logam erro. Em `:291`, a lista retornada por `ValidateTownShopCatalogIntegrity.Validate()` é descartada; `ValidateTownShopCatalogIntegrity.cs:40` já oferece `Run()` que reporta e lança em falha. Usar o contrato existente de relatório/resultado para agregar falhas reais, em vez de contar retorno normal como aprovação. Isso não exige criar outro framework paralelo.
2. **Scene transitions pode registrar ausência e terminar com PASS.** `ValidateSceneTransitions.cs:60,122,135` escreve `SCENE_NOT_FOUND`, `MISSING_GATE` e `MISSING_ANCHOR` no texto, sem adicioná-los a `errors`; `:155` pode emitir PASS. A busca global em `:77` também não restringe objetos à cena inspecionada. Essas verificações precisam de resultado estrutural e escopo por cena.
3. **Runner EditMode aceita evidência insuficiente.** `RunUnityEditModeTests.ps1` não elimina/identifica XML antigo, não exige casos executados, não verifica resultado agregado e conjunto esperado pelo filtro, nem possui timeout. Os caminhos exclusivos e a inspeção manual usados na execução anterior evitaram essas lacunas, mas devem virar garantia do runner. Detectar Unity já aberto para não iniciar batch concorrente.
4. **Versão do Unity está desatualizada nos defaults.** Os runners apontam para `6000.4.7f1`; `ProjectSettings/ProjectVersion.txt` declara `6000.5.7f1`. Resolver a versão do projeto evita tentativas e diagnósticos incorretos.
5. **O scanner genérico confunde falha esperada com erro não tratado.** `ScanUnityLogs.ps1` marca qualquer `Exception:` como crítica. O log completo anterior falhou apenas em `Exception: Test exception`, esperada por `GameEventBusTests` via `LogAssert.Expect`, com o teste passando. Usar XML/Test Framework para resultado dos testes e diagnóstico específico para compilação/crash. Não criar whitelist ampla de exceptions nem remover o único scan de um fluxo compile-only: o wrapper de compile não faz esse scan internamente.
6. **O gate documental confunde variáveis PowerShell com placeholders.** `validate_docs.ps1:185–192` inclui scripts e busca nomes genéricos de variáveis. Dos 73 diagnósticos da baseline, 26 são esses falsos matches. Corrigir seleção/sintaxe do detector; não renomear variáveis legítimas para satisfazer regex.
7. **O gate de escopo não identifica autorização nem autoria.** `check_spec_quality.ps1:29–57` trata toda alteração de cena/asset/ProjectSettings no `git status` como proibida. Isso mistura alterações previamente existentes e tarefas autorizadas. Validar contra o escopo concreto da tarefa, mantendo os guardrails de edição via Editor.
8. **“Arquivo de teste mudou” não prova cobertura.** `check_spec_diff_completeness.ps1` considera presença de qualquer teste/report no working tree e não reconhece explicitamente execução de testes existentes como evidência suficiente. Substituir essa heurística por vínculo critério → teste/resultado; reutilização válida não é testing deferred.
9. **O JSON legado não é cache confiável.** `.claude/hooks/run-required-validations.ps1` pode registrar FAIL de subprocesso sem atualizar o agregado e produzir overall PASS. Ele não está conectado ao Stop hook atual; `.claude/commands/review-non-regression.md:65–81` ainda orienta consultar seu JSON. Retirar essa referência do fluxo de evidência ou corrigir o produtor antes de reutilizá-lo.

## Matriz proposta para uma rotina menor

Esta matriz é uma proposta de seleção; não é exceção silenciosa aos gates hoje vigentes. “Uma vez” significa sobre os mesmos insumos pertinentes e configuração, não simplesmente sobre o mesmo commit: alterações não commitadas também contam.

| Tipo de alteração | Durante o trabalho | Fechamento/integração pertinente | Dispensável sem outro risco |
|---|---|---|---|
| Texto/documentação | Diff, links e regras documentais afetadas | Uma validação documental consolidada; global se índices/schema/governança mudarem | Build C#, Unity, PlayMode |
| Comentários C# sem alteração de código | Diff e checks de fonte sensíveis aos comentários, se afetados | Evidência anterior de compile/testes continua aplicável quando os inputs semânticos não mudaram | Rebuild e suíte comportamental só para comentários |
| C# puro localizado | Testes do domínio e consumidores afetados, agrupados; build .NET se trouxer feedback mais rápido | Execução Unity/EditMode recente cobrindo assemblies pertinentes; suíte completa uma vez em integração ampla | Compile-only adicional sobre o mesmo código; PlayMode sem risco de integração |
| Runtime Unity/MonoBehaviour | EditMode pertinente | Compilação pelo Test Runner; PlayMode se lifecycle, input, física, cena ou wiring forem afetados | Todos os validadores de assets sem relação com a mudança |
| Save/transações/cave | Fixtures de compatibilidade, atomicidade, idempotência ou determinismo afetadas | EditMode completo em integração ampla; smoke do fluxo real se persistência/wiring/transição mudar | Repetir todos os smokes de outras features por spec |
| Cena/prefab/SO/sprites | Validador de dados/cena/import afetado | Unity e smoke/inspeção visual pertinente; compilação C# se também houver código alterado | Sete builds .NET por alteração exclusiva de asset |
| Scripts PowerShell/harness | Testes de contrato do script alterado; paridade quando `.claude/` mudar | Um resultado do conjunto afetado, inspecionado pelo revisor | Unity para script/documentação sem impacto em C# ou assets |
| Assembly/package/ProjectSettings/bootstrap | Checks de dependência e testes afetados | EditMode completo + PlayMode de composição; build Player quando target/build/pacotes exigirem | Deduplicar apenas repetições exatas; não trocar validação Player por compile Editor |
| Entrega jogável/release | Fluxos integrados | Smoke humano de UX/visual + build Standalone no target pertinente | Rodar release build a cada pequeno edit |

Uma execução Unity de testes não prova automaticamente todos os validadores de assets nem build Player. O relatório deve distinguir compile, comportamento, assets, PlayMode automatizado e teste humano, mesmo quando dois resultados compartilham uma única invocação.

## Contradições do harness que precisam ser resolvidas juntas

- `.specs/SPEC_VALIDATION_MATRIX_MASTER.md:58–61` já dispensa builds para docs-only e condiciona documentação ao escopo. `validation-truth.md:3` e o strict universal voltam a exigir o conjunto completo. A matriz deve ser a fonte de seleção para os commands e skills.
- `execute-spec-strict.md:42–43` proíbe Unity Test Runner/PlayMode, enquanto exige testes determinísticos em `:173`; `spec-execution/SKILL.md:205` limita testes a EditMode apesar de exigir evidência de lifecycle em `:304`. Remover restrições universais que excluem PlayMode e testes de tooling legítimos.
- `finish-spec.md:67–69` permite cenário de wave, mas outras seções voltam a exigir cenário por spec. Consolidar a referência ao cenário adequado; documentação do cenário não equivale a execução nem a ACCEPTED.
- `validate-spec.md:45–55` e `subagent-results-not-evidence.md:8` referem-se a `Assembly-CSharp-Editor.csproj`, ausente no checkout atual. Referenciar a descoberta canônica de assemblies, sem repetir comandos históricos.
- `execute-spec-strict.md:235–254` documenta `EXPECTED_FAIL_LEGACY_ONLY` e `LAST_STRICT_VALIDATION_RESULT.json`, que o strict atual não produz. Alinhar o contrato documentado ao resultado real; não reutilizar esses exemplos como evidência.
- A própria `solid-refactoring/SKILL.md:56–63`, criada na rodada anterior, pode induzir build/ratchet separados e depois strict. Corrigir também as regras novas para pedir evidência pelo gate aplicável, sem duplicar sua execução.

## Regras para reutilizar uma execução

- Registrar comando, ferramenta/versão, configuração/defines, escopo, exit code, artefatos e contagem/resultado dos testes.
- Associar a evidência aos insumos efetivamente executados: código, testes, referências, scripts de validação e, quando aplicável, assets e settings. Apenas HEAD ou data do log não bastam no workspace compartilhado.
- O revisor inspeciona artefatos e conteúdo do diff. Reexecução é necessária quando a evidência não é verificável, quando os insumos pertinentes mudam ou quando surge uma preocupação nova.
- Resultado prévio de código pode continuar válido após editar um relatório. O resultado do gate documental precisa refletir a edição desse relatório.
- Manter FAIL, NOT RUN e falhas antigas visíveis. Baseline conhecida serve para identificar regressões novas; não transforma uma suíte global com falhas em aprovação global.

## Ordem recomendada de aplicação

1. Corrigir resultados de validadores/runners e eliminar referências a evidência legada não confiável.
2. Unificar a matriz de seleção e retirar builds/docs repetidos entre worker, revisor e closeout. Aproveitar a matriz existente; não criar mais uma camada obrigatória.
3. Consolidar scanners e invocações por grafo; medir o ganho usando tempo real de processo em futuras execuções necessárias.
4. Remover os oito testes tautológicos e reformular os casos que não chegam à condição anunciada. Preservar as proteções listadas acima.
5. Reconciliar os contratos de farm antes de mudar expectativas. Não usar exclusão de testes como correção de baseline.

## Arquivos centrais para a implementação futura

- `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`: fonte para consolidar seleção por risco.
- `.claude/rules/validation-truth.md`, `.claude/rules/subagent-results-not-evidence.md`.
- `.claude/skills/delegated-execution/SKILL.md`, `.claude/skills/spec-execution/SKILL.md`, `.claude/skills/unity-validation/SKILL.md`, `.claude/skills/implementation-closeout/SKILL.md`.
- `.claude/commands/validate-spec.md`, `.claude/commands/finish-spec.md`, `.claude/commands/review-non-regression.md` e instruções equivalentes que devem apontar à mesma matriz.
- `tools/docs/run_strict_validation.ps1`, `validate_docs.ps1`, `check_spec_quality.ps1`, `check_spec_diff_completeness.ps1`.
- `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`, `RunUnityCompileValidation.ps1`, `RunUnityEditModeTests.ps1`, `ScanUnityLogs.ps1`.
- `Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs` e validadores citados acima.

Após eventual alteração canônica em `.claude/`, regenerar a paridade Codex pelo gerador existente. A revisão atual criou somente este relatório; não fez essa migração nem declarou gates globais aprovados.
