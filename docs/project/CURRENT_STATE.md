# Current State — Cindar's Hope

> Contexto primário de execução. Snapshot auditado em 2026-09-08; branch `dev`.
> Ler isto, AGENTS/CLAUDE e a spec alvo. Histórico completo preservado em
> [snapshot anterior](../archive/CURRENT_STATE_before_SOLID_AI_2026_09_08.md); não carregar por padrão.

## Onde estamos

- RPG + farm sim 2D em Unity 6000.5.7f1 / C#. Fonte de specs: `.specs/`.
- A modularização anterior permanece: Foundation e Gameplay puros, Runtime, Editor e testes
  em assemblies explícitas; composition root/installers, providers de save e projections existentes.
- A maior parte dos domínios ainda compartilha Runtime. Zero pares mútuos no scanner textual
  não demonstra ausência de ciclos gerais nem cumprimento de SOLID.
- MVP possui implementação extensa; aceitação humana final de gameplay continua pendente.
- Working tree inclui trabalho anterior de farm/keyart/arte/cenas. Preservar esses edits.

## Validação atual

| Gate | Evidência desta auditoria |
|---|---|
| .NET | 7/7 projetos PASS, uma invocação de grafo, exit 0, 22 warnings |
| Unity compile | Comprovado pelo Test Runner e batch posterior; não executado compile-only redundante |
| EditMode | 2911/2915; mesmas quatro falhas de farm do baseline 2900/2904 |
| Fixtures alteradas nesta continuação | 126/126 PASS, extraídos da suíte completa |
| PlayMode automatizado de composição | 2/2 PASS; não equivale a aceitação visual/humana |
| Ratchet arquitetural | baseline permite dívida; PASS significa que ela não aumentou |
| Docs/strict global | Sem GLOBAL_PASS; docs47 diagnósticos, removidos26 falsos positivos do detector |
| Menu real de validação | FAIL: wiring de2 picaretas, Fireball e4 gates/4 anchors canônicos |
| PlayMode humano completo | NOT RUN nesta auditoria |

- Falhas de farm: ranges WaterEdge, escalas ponte/árvore e bridge center arável.
  Não corrigir/reverter esse trabalho concorrente incidentalmente.
- [Melhorias e evidência atual](../validation/QUALITY_IMPROVEMENTS_EXECUTION.md) contém escopo,
  riscos, comandos e prioridades. SCOPED_PASS comprova somente os gates explicitamente selecionados.
- Quality global também sinaliza três cenas que já estavam dirty antes desta tarefa.

## Trabalho SOLID_AI desta sessão

- Rule `solid-and-ai-context`, skill `solid-refactoring` e agentes existentes orientam SRP,
  OCP, LSP, ISP e DIP por contratos; nenhuma quota de interfaces, patterns ou linhas.
- Add/remove/capacidade e split/move/merge/swap em `InventorySlotOperations`; fachada mantém
  catálogo, aggregate/eventos e restore. UI compartilha prontidão e preserva ID após consumo.
- Escrita segura de save concentrada no `SaveBackupService` existente; schema e recuperação intactos.
- Três consumidores usam `QuestRuntimeIds.SupplyQuestId` canônico.
- Contexto por classe via `pwsh -File tools/architecture/Get-ClassContext.ps1 -Type InventoryManager`.
  XML curto para intenção/invariantes; não inserir frontmatter manual repetitivo em massa.
- Matriz canônica de gates; runners/validadores sem falso PASS; 17 testes redundantes cortados,
  3 scans no owner PowerShell; saves inválidos com estado real. Tooling95+gerador30 contratos PASS.
- Specs `spec_solid_inventory_save_refactor_v1`, `spec_solid_class_context_v1` e
  `spec_solid_ai_harness_hardening_v1` permanecem em `a_implementar` enquanto gates estiverem pendentes.

## Próximas ações

1. Reconciliar falhas farm/keyart e wiring de assets/gates apontados no batch; preservar cena concorrente.
2. Resolver a dívida de docs/strict indicada pelo report, sem afrouxar os gates.
3. Executar `spec_validation_human_playmode_smoke_v1`: Town/Farm/Cave, inventário, crafting,
   loja, NPC/quest, combate, morte/respawn, save/load; registrar evidência humana.
4. Refactors seguintes exigem costura/teste: demais ações de UI, ports tipados no
   lugar de casts/service locator e bootstraps legados. Prioridades detalhadas no audit.

## Regras de continuidade

- Não reexecutar specs encerradas/absorvidas nem fases já concluídas. Spec vence roadmap/refinement.
  Conflito entre spec e CURRENT_STATE exige reconciliação explícita.
- `.specs/a_implementar/features_futuras/` permanece fora de automações, salvo decisão humana.
- Cave stable run: preservar snapshots por nível/seed; ForwardExit/BackExit não regeneram conteúdo.
  Ler os documentos FASE9F exigidos em AGENTS antes de qualquer alteração nessa área.
- Preservar APIs públicas, GUIDs, campos serializados e IDs/schema de save durante refactors.
- Wiring pelo composition root/installers; GameEventBus para comunicação de gameplay entre sistemas;
  queries/helpers locais podem ser dependências explícitas. UI nova usa Canvas/projection.
- Ler ADRs e game_rules listados na spec, por demanda: [decisões](DECISION_LOG.md),
  [game rules](../game_rules/GAME_RULES_INDEX.md). Decisões humanas FABLE v1/v2/v3 continuam
  vinculantes nos documentos originais referenciados pelo snapshot histórico, sem duplicá-las aqui.
- Não promover specs nem declarar aceitação humana com base apenas em build/teste automatizado.

## Referências sob demanda

- Farm keyart reconstruction (2026-09-10): [v4 report](../validation/farm_keyart_v4/REPORT.md),
  stage12 actual Unity capture and18-view gameplay PASS; scoped EditMode81/81 PASS. This supersedes
  the older farm evidence only for those selected fixtures; no global pass or95% visual acceptance.
  Source-derived sprites,76×50 western extension and corrected dock/greenhouse physics are integrated.
  Remaining visual material work is active in `spec_farm_enclosed_valley_keyart_v2`; preserve Town work.
  Door/interior and6-frame water are integrated:37timed Unity images,24named solid checks,4local accesses,
  78prop/furniture soil checks PASS. Player tag wiring corrected. Stage13 fixes exterior stair occlusion;
  its motion probe FAILS walking-frame direction. The subsequent animator candidate is not approved.
  [Latest visual comparison](../validation/farm_keyart_v4/progress-review.html) shows actual stage13/history.
  Round14 is integrated:14/14 decoration/navigation tests PASS,71 authored tree transforms preserved.
  Visual review accepts local planting only; path/composition objectives remain unmet. Water contour and
  interior finishing remain pending. Controlled-body tests do not certify input or full transitions.

- [Arquitetura implementada](../architecture/MODULARIZATION_IMPLEMENTED_ARCHITECTURE_GUIDE.md).
- [Navegação de código para AI](../architecture/AI_CODE_CONTEXT.md).
- [Índice de documentação](DOCUMENT_INDEX.md).
- `PROJECT_LOG.md` e `docs/IMPLEMENTATION_STATUS.md`: histórico/auditoria, não leitura padrão.

## Fundação de habilidades — 2026-09-10

Avatar/efeitos, transações/save v3 e cooldown por actionId corrigidos. 131/131 EditMode integrados e cinco cenários PlayMode PASS; revisão independente sem bloqueadores. Arte provisória, Canvas e equilíbrio global pendentes. [Evidência e limites](../validation/skills_sdd_v1/execution/FOUNDATION_REPORT.md).


Farm v15: seven nonblocking ground-cover accents, dock+10%, southern spacing and native ambient clips integrated.33/33scoped tests and18/18PlayMode views/routes PASS. See farm_keyart_v4/REPORT.md; fish20s runtime observation/human approval pending.

Farm ambient follow-up supersedes prior fish-observation pending:41s realPlayModePASS, two complete jumps20.000061s apart; source/savesunchanged. docs/validation/farm_keyart_v4/ambient_runtime15/index.html. Human visual acceptance pending.

Farm v16: pacote spec/plan/tasks criado e fontes aplicadas (pesca na ponta, grupo da fonte,4bases, grama314/estrada64). FarmScene v16 gerada e salva; geração e53/53EditMode PASS. PlayModeD PASS:16contatos, pesca na ponta aceita e centro/entrada recusados,5frames. Aceitação residual de circulação/respawn/arte pendente. Veja contact_v16/REPORT.md para limites e resultado vigente.



Farm v16 follow-up:contato do rochedo oeste corrigido;10/10navegação e19checksPlayPASS, revisão visual localPASS. Ver contact_v16/REPORT.md e index.html.

Farm v17:popa do barco reconstruída emAseprite e integrada; geraçãoUnity exit0 e revisão visualPASS. Outros props semamputação confirmada no conjunto examinado; emenda degrama no gate leste pendente. docs/validation/farm_keyart_v4/boat_v17/index.html.

Farm v18 piloto integrado: barco4poses5s e galinha19frames com movimento seguro, interação e restore.62/62EditMode +PlayC60sPASS, artevisívelrevisada. Vaca/ovelha/cabra semciclosnovos; fullherd/humano pendentes. docs/validation/farm_keyart_v4/motion_v18/index.html eREPORT.md.

Farm v19 integrado: vaca/ovelha/cabra com 19 poses cada e perfis por ID; cordeiro ausente corrigido no catálogo gerado. 65/65 EditMode e Play B com quatro animais por 60s PASS, interação/restore/limites verificados. Proporções revisadas, 71 árvores preservadas. Aceitação humana/input pendente. Evidência: docs/validation/farm_keyart_v4/herd_v19/REPORT.md e index.html.


Farm v20 integrado: +27 arbustos baixos e54 tufos decorativos, Ground/4 semcolliders; árvores eacessos preservados. GeraçãoUnity exit0 e15/15testesdecoração/navegação PASS. Comparativo/evidência: docs/validation/farm_keyart_v4/groundcover_v20/index.html eREPORT.md. Aceitaçãovisualhumana pendente.

