# Auditoria do projeto e refatoração SOLID / AI

Data: 2026-09-08. Branch: `dev`. Pedido humano: avaliar o projeto inteiro, melhorar
qualidade/modularização/desacoplamento/clean code/SOLID e uso de AI, avaliar frontmatter
por classe e refatorar com patterns onde houver necessidade real.

**Resultado: auditoria da superfície própria, harness fortalecido e duas extrações de
responsabilidade verificadas. Não representa certificação SOLID de todo o jogo nem
aceitação humana final. Gates globais ainda têm falhas preexistentes.**

Continuação: [melhorias executadas](QUALITY_IMPROVEMENTS_EXECUTION.md) atualiza as
extrações de inventário/UI, organização das validações, cortes de testes e evidência.

## 1. Escopo e método

- Inventário de todos os C# próprios em `Assets/_Game/Scripts`, incluindo untracked;
  testes enumerados separadamente. Não contar Library/Packages/arte como código autoral.
- Varredura sintática geral e revisão semântica dos hotspots e diffs. Não houve leitura
  semântica exaustiva de cada classe nem execução manual de todos os fluxos de gameplay.
- Revisões independentes de arquitetura/harness; baseline antes de editar runtime;
  testes de caracterização antes/depois, revisão estática, compile, EditMode e composição.
- Working tree já continha trabalho de farm/keyart, arte/cenas e RULES.md. Nenhum desses
  edits foi revertido ou incluído como entrega desta refatoração.

## 2. Onde estamos

| Medida no baseline | Valor / interpretação |
|---|---|
| Arquivos Scripts | 1457; 1294 runtime excluindo Editor |
| Arquivos de teste | 239; Scripts+Tests = 1696 |
| Arquivos runtime >500 linhas | 31; triagem, não reprovação automática de SRP |
| Arquivos runtime >1000 linhas | 2; partials exigem somar arquivos da mesma classe |
| SaveManager | 880 + 391 linhas nos dois partials; 1271 no total |
| InventoryManager | 937 linhas físicas antes da extração |
| Foundation/Gameplay | assemblies com noEngineReferences; base útil já implementada |
| Grafo textual | 234 arestas, zero pares mútuos; não verifica ciclos de comprimento maior |

A base arquitetural é aproveitável: composition root/installers, ports, providers de
save, policies/projections, testes de compatibilidade e event bus existentes. Runtime
ainda agrupa a maior parte dos domínios. Ausência de dependência nominal reversa não
prova DIP: casts, `object` e service locator podem manter acoplamento em runtime.

Contagens do ratchet no baseline: 62 declarações de singleton, 33 buscas globais,
15 auto-bootstraps, 214 inputs diretos, 21 ResourcesLoad, quatro escritas de timeScale
e seis scene loads diretos. São matches do scanner, inclusive suas limitações; seu
PASS significa **dívida rastreada não aumentou**, não ausência de dívida.

## 3. Pontos de atenção e tratamento

| Prioridade | Evidência / consequência | Tratamento nesta sessão |
|---|---|---|
| P1 | Guard runtime ignorava apply_patch; mesmo conteúdo rejeitado como Edit passava como patch | Parser compartilhado e quatro guards adaptados, contratos executados explicitamente |
| P1 | Strict misturava stdout e exit code; docs nonzero virava legado sem prova | Processos isolados, fail-fast e FAIL verdadeiro; ratchet integrado |
| P1 | Skill save ensinava `items[i].Id = i + 1` em OnValidate | Exemplo removido; IDs estáveis e providers reais, sem mudar saves existentes |
| P1 | InventoryManager reúne algoritmo, catálogo, bootstrap e eventos | Split/move/merge/swap extraídos; aggregate e efeitos preservados no owner |
| P2 | SaveManager reúne IO, JSON/migrations/providers e cena | Escrita movida para SaveBackupService existente; recuperação/schema intactos |
| P2 | Quest ID literal repetido em três consumidores | Referências à const QuestRuntimeIds.SupplyQuestId |
| P2 | Gerador hardcodava hooks, omitindo sync-harness-and-tracing | Derivação de settings canônico, testes de paridade/idempotência; config pessoal preservada |
| P2 | Agente ensinava auto-bootstrap e reprovação por 50 linhas | Composition root e review de razões de mudança/contratos; sem limite arbitrário de LOC |
| P2 | CURRENT_STATE tinha 545 linhas históricas no contexto padrão | 78 linhas, original preservado byte a byte em docs/archive e indexado |
| P2 | UI ainda executa ações de inventário/uso/drop | Dívida identificada; precisa de costura de aplicação com retorno síncrono e smoke de UX |
| P2 | Ports com object/casts e registry global escondem dependências | Dívida identificada; evoluir contratos por consumidor sem inventar interface por classe |
| P2 | Global searches em bootstraps, alguns repetidos até 120 frames | Dívida identificada; corrigir por domínio com teste de ownership e troca de cena |

Evidências das dívidas remanescentes: `UI/InventoryPanelController.cs` (SplitSlot,
TryMoveOrMergeSlot, TryUseItem, DropItem); `Foundation/IPlayerRuntime.cs` e
`Foundation/DomainManagerRegistry.cs`; `Core/Bootstrap/GameBootstrap.cs`;
`Player/Movement/PlayerMovementActionRuntimeBootstrap.cs`; `NPC/Schedule/NpcScheduleRuntimeBootstrap.cs`;
`World/Scenes/PlayerSpawnResolver.cs`. Caminhos relativos a `Assets/_Game/Scripts`.

`TownNpcDialogueLibrary` é conteúdo e `EnemyBrain` já tem colaboradores especializados:
não foram fatiados por tamanho. DTOs de save amostrados não exibiram refs de objetos Unity;
alguns conservam Vector2/Vector2Int (valores, não refs), dívida de independência da engine
que exige compatibilidade e, no caso da cave, o contrato FASE9F.

## 4. Regras e SOLID aplicados

- Rule `.claude/rules/solid-and-ai-context.md`: SRP por razões de mudança; OCP onde há variação;
  LSP por pré/pós-condições e falhas; ISP por consumidor; DIP por dependência explícita.
- Skill `.claude/skills/solid-refactoring/SKILL.md`: reuse → caracterização → extração coesa →
  testes/review. Atualizados architecture-reviewer, non-regression-auditor e spec-implementer;
  nenhum novo agente redundante.
- Patterns usados: adapter Unity sobre operações determinísticas no inventário;
  reutilização do serviço de IO existente no save. Não foi criada interface por classe,
  framework genérico ou partial cosmético para reduzir contagem de linhas.
- Hooks são heurísticos e têm contratos exercitados em processos explícitos. Este relatório
  não afirma que cada ferramenta desta sessão disparou hooks automaticamente.
- Configuração local de modelo/effort/approval não foi alterada pelo gerador.

## 5. Frontmatter e contexto de classe

Decisão comunicada: **índice gerado consultável + XML útil**, em vez de cabeçalho manual
obrigatório em milhares de classes. Pergunta opcional ficou sem resposta durante o trabalho;
essa é a recomendação adotada, não uma preferência explicitamente confirmada pelo humano.

Fatos extraíveis não devem ser mantidos duas vezes. Intenção, invariantes e ownership
cabem em summary/remarks curtos. Comentários ausentes não foram preenchidos por inferência
de nomes. Não há medição experimental de economia de tokens; menos texto de entrada
não substitui ler implementação e consumidores relevantes.

`tools/architecture/Get-ClassContext.ps1` usa Roslyn do PowerShell 7, sem pacote instalado.
Consulta por Type/Module, XML, partials, generics, assembly e membros opt-in. Exporta JSONL
reproduzível e recusa índice incompleto por parse error. O default é resumo, não dump.

Pós-slice: 1698 arquivos próprios com testes, 2633 declarações e 2618 tipos distintos na
configuração corrente; 1120 declarações já têm summary. Cobertura sintática inclui
classes/records/structs/interfaces/enums; não delegates, branches condicionais inativos,
asmref ou resolução semântica de chamadas. Detalhes: `docs/architecture/AI_CODE_CONTEXT.md`.

## 6. Refatoração e invariantes verificados

**Inventário:** operações puras sobre os slots existentes; nenhum estado ou catálogo duplicado.
SlotIndex permanece posição; conteúdo/binding move junto; totais conservados; merge respeita
capacidade; refusas não mutam/publicam. Aggregate é reconstruído antes dos mesmos eventos,
com payload, ordem e quantidade preservados. Split de equipado mantém comportamento legado.
Add/remove/restore continuam no InventoryManager; a classe inteira não virou adapter fino.

**Save:** corpo da escrita preservado em SaveBackupService.WriteTextSafely, com dois callers
atualizados. File.Replace, .tmp/.backup, fallback, erro e IO existentes; nenhuma mudança de
schema v5, providers, migrations, paths ou scene flow. Testes acessam a escrita diretamente;
somente recovery privado continua sob reflection. Fallback sem File.Replace conserva backup,
mas não garante atomicidade contra crash — risco existente, agora documentado corretamente.

**IDs:** NpcController, NpcShopController e QuestGiverInteractable usam a const canônica
existente com o mesmo valor. Sem migração de identidade ou novo catálogo.

## 7. Evidência de validação

Artefatos locais copiados para `TestResults/solid-ai/`, junto de comandos reproduzíveis
nos reports de baseline/pós-refactor. Nenhum commit foi criado; nenhum push/merge/PR.

| Check | Antes / depois | Evidência |
|---|---|---|
| .NET assemblies | 7/7 PASS após restore em ambas rodadas | build-post-refactor-after-restore-exits.tsv; root-build.log |
| Unity compile | PASS, exit 0; scan PASS | post-refactor-unity-compile-wrapper.log e -scan.log |
| Caracterização anterior à extração | 20/20 PASS | pre-refactor-characterization.xml |
| Afetados + compatibilidade v1–v5 | 36/36 PASS | post-refactor-focused.xml |
| EditMode completo | 2884/2888 → 2900/2904; mesmas quatro falhas | editmode.xml e post-refactor-full.xml |
| PlayMode composição | 2/2 PASS, exit 0 | post-refactor-playmode-composition.xml |
| ClassContext | 15 contratos PASS | Test-ClassContext.ps1 |
| Guards / geração / strict | 77 / 30 / 23 asserts PASS, reexecutados pelo root | root-guards.log, root-harness-generation.log, root-strict-contracts.log |
| Docs / strict global | FAIL; 73 diagnósticos iguais ao baseline | root-docs.log, root-strict.log |
| PlayMode visual/humano | NOT RUN | scenario do slice e smoke geral pendentes |

As quatro falhas iguais antes/depois:

1. FarmDecorationPlannerTests.Plan_AllBiomesAreWithinDeclaredRanges.
2. FarmSceneCompositionContractTests.BridgeVisualScale_IsReducedWithoutDefiningPhysicalFootprint.
3. FarmSceneCompositionContractTests.ScaleRanges_AcceptApprovedValuesAndRejectBridgeOutlier
   (falha concreta no valor de árvore, apesar do nome mencionar ponte).
4. FarmSceneSpatialContractTests.BlockingFootprints_AreNonArable.

O scan bruto do log da suíte completa reporta uma Exception deliberada de GameEventBusTests,
esperada com LogAssert e teste aprovado; não foi convertida em erro novo de compile. Compile
separado e scan passaram. Warnings runtime/editor preexistentes permanecem.

### Efeitos do runner e preservação do workspace

PlayMode causou migração automática de ProjectSettings pelo Unity. O arquivo, originalmente
limpo, foi restaurado byte a byte do objeto LFS original; SHA256 conferido e diff vazio.
O SDK Microsoft GDK removeu `Assets/Resources/GDKEditionAutoGen` ao sair de PlayMode:
é intermediário autogerado com edition number, removido por GdkApiPlaymodeStateListener /
GdkEditionAssetGenerator e recriado normalmente pelo SDK em Play/build. Seus untracked não
tinham backup, portanto **não se afirma restauração desses arquivos temporários**.

## 8. Próxima refatoração e limites

1. Reconciliar testes de farm e dívida de docs com os responsáveis pelos edits em andamento.
2. Extrair ações de aplicação da UI preservando resposta síncrona, feedback e foco/modal.
3. Refinar ports que retornam object e casts em consumidores; injetar contratos realmente usados.
4. Retirar buscas globais nos bootstraps por domínio com characterization de lifecycle.
5. Complementar grafo textual com análise de ciclos gerais/símbolos antes de alegar DAG.

Esses itens são trabalho identificado, **não refactors implementados nesta sessão**. Alterações
de scene/wiring e save/cave exigem suas próprias evidências. A auditoria não justifica uma
reescrita em massa de todo o projeto nem promessa de ausência de regressões.

## 9. Closeout final

- Root reexecutou builds7/7, testes tooling77/30/23/15 e ratchet: PASS, exit0.
- Docs:73diagnósticos; comparação com baseline normalizando somente números de linha de
  scripts resultou em zero diferenças. São os mesmos problemas, sem novo erro de documentação.
- Diff completeness: exit0; quality global: exit1 pelas três cenas preexistentes. Nenhum
  warning dessas ferramentas referencia os três execution reports SOLID_AI após adequação.
- Manifesto de arquivos da entrega: `TestResults/solid-ai/change-manifest.txt`; inclui
  cópias geradas. RULES.md preserva também edits anteriores; keyart-scene-fidelity em Codex
  foi sincronizada da regra humana já existente, não criada como política por esta tarefa.
- Strict: exit1 em DOCS_VALIDATION_FAILURE, como deve ocorrer; etapas posteriores desta
  invocação não rodaram. Builds/ratchet foram verificados separadamente, não atribuídos ao strict.
- Geração real repetida pelo comando canônico PowerShell5.1: exit0 e bytes idênticos;
  PowerShell7 produz JSON de hooks semanticamente igual com serialização diferente.
  Preferências config mantidas byte-exatas, SHA256
  `6F369E1652B349C0B19AD365C038ADE22D65FA2BBD293C3C53FC4A39152A39C4`.
- Histórico CURRENT_STATE preservado SHA256
  `04C15372BADEB78C63FA299152D302EFC9887657B19896AC0E4C772DB60528FC`.
- Specs continuam em `a_implementar`; sem promoção/claim BUILD_VALIDATED ou aceitação humana.
  Nenhum commit criado. Falhas globais e refactors seguintes permanecem explicitamente pendentes.
