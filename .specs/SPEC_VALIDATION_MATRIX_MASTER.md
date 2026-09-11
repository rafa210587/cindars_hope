# Cindar's Hope — Validation Matrix Master

> Matriz canônica de seleção, evidência e status. Atualizada pela manutenção autorizada
> `spec_validation_efficiency_harness_v1`. Não é spec implementável.
> Commands/skills apontam para esta fonte; não mantêm matrizes paralelas.

## Seleção por escopo e risco

Uma entrega coesa exige os gates que comprovam seus critérios e riscos. Um gate sobre
inputs pertinentes equivalentes pode ser reutilizado após revisão, inclusive se executado
por subagent. Não há obrigação de compilar por agente, fase ou repetição de closeout.

| Mudança | Durante a edição | Evidência de fechamento aplicável | Dispensável sem outro risco |
|---|---|---|---|
| Docs/governança | Diff, links e regras documentais afetadas | Docs consolidado; global se schema/índice/validador mudar | C#, Unity, PlayMode |
| Comentários C# | Diff e checks de fonte afetados | Evidência anterior se equivalência semântica comprovada | Testes comportamentais apenas para comentários |
| C# puro | Testes do domínio/consumidores; .NET se útil para feedback | Compile e testes do assembly pertinente; Unity/EditMode para código Unity | Compile-only se Test Runner já comprova compile; PlayMode sem integração |
| Runtime/MonoBehaviour | Testes de lógica afetada | Unity compile/testes; PlayMode para lifecycle/cena/input/física/wiring | Validadores de assets sem relação |
| Save/schema/providers | Fixtures de compatibilidade, defaults, round-trip, falhas | Integração de captura/restore se wiring/ordem mudou | Smokes de domínios não afetados |
| Cave runtime/procedural | Seed, snapshot, replay, revisita e regras afetadas | Fluxo real quando transição/materialização/save mudou | Repetir todos os smokes por spec |
| Event contracts | Subscribers/lifetime/payload e consumidores afetados | Compile + contratos; integração quando publisher/lifecycle mudou | Testes que apenas atribuem campos |
| UI/menu/HUD | Projection/políticas e regressão de foco/input | PlayMode ou cenário humano executado conforme UI/lifecycle | Cenário humano duplicado por spec |
| Scene/prefab/SO/import | Validator de dados/import/cena pertinente | Unity e smoke/inspeção visual conforme wiring | .NET se C# não mudou |
| Harness/PowerShell | Parse + contratos do mecanismo alterado | Contratos pertinentes; paridade após alteração canônica | Unity sem impacto em C#/assets |
| asmdef/package/settings/bootstrap | Dependências/compilação e testes afetados | EditMode completo + composição PlayMode; Player quando target/build exigir | Repetição exata de evidência |
| Integração ampla/release | Fluxos integrados | Gates globais pertinentes uma vez; suíte completa; humano/Player conforme entrega | Release build a cada edit |

Docs entram quando seus inputs mudam, incluindo reports/links. Critérios da spec podem
exigir evidência adicional concreta. Não dispensar compile/teste por conveniência.
Não criar teste novo para setter, lista, rename ou exemplo que apenas espelha implementação;
reusar testes existentes pertinentes é válido. Ausência de proteção real continua risco.

## Gates do tooling

`tools/docs/run_strict_validation.ps1`:

- Sem `-Gates`: GLOBAL, seis gates (corruption, docs, build, architecture, diff, quality).
- Com `-Gates docs,quality` ou array in-process: SCOPED, somente os IDs explícitos.
- `-ScopePath` exige Gates explícitos; JSON de tarefa com arrays não vazios
  `changedFiles` e `allowedPaths` de paths exatos relativos ao repo; `reportPaths` opcional.
  Entradas null não são paths; reports explícitos precisam existir como arquivos.
  Diff/quality inspecionam esses arquivos. Reviewer confere autorização, completude e diff.
  O manifesto não concede permissão de editar Unity YAML nem substitui regras de assets.
- Exit 0 + `GLOBAL_PASS` comprova o conjunto global. Exit 0 + `SCOPED_PASS` comprova
  somente a seleção; não significa que ela seja suficiente para a spec.
- Docs global ainda audita a documentação inteira. Seleção não oculta falhas antigas.
  Falha/throw/arquivo ausente/entrada inválida permanece FAIL, sem bypass de legado.
- Strict não executa todos os testes Unity/Player/humanos: anexar seus resultados pertinentes.

Usar `Invoke-UnityGeneratedProjectsBuild.ps1` quando .NET for aplicável. Ele descobre
os projetos da solução; não usar assemblies históricas fixas. .NET não prova compile
Unity/import/lifecycle. Test Runner válido pode comprovar compile Editor e comportamento
numa execução; validators de assets e Player são resultados distintos.
Wrappers Unity fazem o scan pertinente; não executar scanner novamente sobre o mesmo log.
O gate architecture (`Test-ArchitectureRatchet.ps1`) é o owner canônico dos checks
de dívida de fonte, GUIDs serializados e nomes de assemblies predefinidas em Scripts+Tests.
Executar uma vez sobre os inputs pertinentes; EditMode conserva checks de assemblies
realmente compiladas. Baselines/regras ausentes, vazias ou inválidas não produzem PASS.

## Contratos comportamentais que permanecem

| Domínio | Evidência a manter conforme alteração |
|---|---|
| Save | Round-trip com estado real; legado/missing sections; IDs desconhecidos; DTO sem Unity refs; restore order; backup/atomicidade/recovery |
| Quest/rewards | Condições e triggers; idempotência; flags/progresso/save; anti-softlock e visibilidade quando afetados |
| Inventário/economia/equipment | Quantidades/capacidade, falha sem mutação parcial, eventos, pricing/stock/durability e transações |
| Cave | Mesmo seed/nível revisitado usa snapshot; ForwardExit/BackExit preservam conteúdo; novo run muda conforme contrato |
| UI | Open/close/Esc, foco e bloqueio de movimento/combate com modal; estados vazio/erro |
| Combat/skills | Fórmulas, cooldowns, status, custos/gates e integração de feedback quando alterada |
| EventBus | Publish/subscribe, lifetime/unsubscribe, reentrância e alocação quando pertinentes |

Caracterização antes/depois de refactor relevante compara comportamentos e não é
redundância. Não reduzir amostras estatísticas só por serem as mais lentas.
Bugfix exige regressão ou justificativa concreta e risco residual; um compile não basta.

## Reuso verificável de evidência

Registrar comando/argumentos, ferramenta/versão, configuração/defines, escopo, exit,
log/XML e contagem/resultados. Associar aos inputs executados: código, testes,
referências, scripts e assets/settings aplicáveis. HEAD/data do log sozinhos não bastam.
Reviewer inspeciona artefatos e diff; reexecuta se inputs mudam, evidência falta/diverge
ou surge risco novo. Não confiar no JSON legado de run-required-validations.
Resultado anterior de código pode sobreviver à edição de report; docs precisa refletir
a edição. Sem cache automático: equivalência é justificada e registrada no report.

## Status e promoção

| Status | Significado |
|---|---|
| CODE_COMPLETE | Implementação feita; validações ainda podem estar pendentes |
| BUILD_VALIDATED | Critérios centrais implementados, report e compile/gates pertinentes PASS; modo/escopo explícitos |
| UNITY_VALIDATED | Níveis Unity compile/validators exigidos passaram; não prova gameplay completo |
| PLAYMODE_VALIDATED | Cenário automatizado ou humano requerido realmente passou |
| DEFERRED_TO_FINAL_HUMAN_VALIDATION | Cenário de integração documentado para fim de wave; humano ainda não executado |
| ACCEPTED | Todos os requisitos obrigatórios satisfeitos, ou exceção humana explícita documentada |
| PARTIAL | Entrega/evidência incompleta, com motivo e risco |
| BLOCKED | Requisito não pode prosseguir sem dependência/correção/decisão |

Estados operacionais como CONTRACT_ONLY, NEEDS_REWORK e BLOCKED_BY_DEPENDENCY_PENDING
não constituem aprovação. Fase pendente não é PASS. Falhas globais continuam visíveis
mesmo com scoped PASS; não promover status histórico automaticamente pela nova matriz.
Docs-only pode concluir com docs/gates aplicáveis PASS, sem inventar build Unity.
Promoção física da spec segue /finish-spec e o protocolo da wave.

Validação humana pode ser agrupada por fluxo/wave; cada critério deve apontar ao cenário.
Cenário escrito não é execução nem ACCEPTED. Não pedir humano por spec se o fluxo
está previsto no checklist final e não há requisito explícito de execução imediata.

## Report mínimo

Seções: Acceptance criteria extracted, Existing systems audit, Spec Compliance Matrix,
Validation, Honest status rationale. Acrescentar arquivos/escopo, comando/inputs/artefatos,
modo GLOBAL/SCOPED, resultados e pendências. Para reutilizar testes existentes:

```text
Existing tests executed: YES
Test command: comando real executado
Test evidence: path real de XML/log
Test result: PASS ou FAIL, conforme artefato
```

Essas linhas são declaração para revisão, não prova mecânica do resultado.
Não alegar economias não medidas nem transformar baseline FAIL em aprovação global.

## Referências

`SPEC_WAVE_EXECUTION_PROTOCOL.md`; `SPEC_IMPLEMENTABLE_TEMPLATE.md`;
`.claude/rules/validation-truth.md`; `.claude/skills/spec-execution/SKILL.md`;
`docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`.
