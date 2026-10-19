# Cobertura integrada das correções de playtest

> Spec ID: spec_playtest_integrated_coverage_v1
> Status: PARTIAL em 2026-09-27; AC01 PASS, PlayMode headless composição 2/2 PASS, AC02/03 NOT RUN, docs PASS, architecture global FAIL. Sem promoção.
> Type: Validation; Domain: Farm/Town/Cave/UI; Priority: P2; Executor: root.
> Parallelizable: documentação sim; Editor/testes não (root é único owner).
> Repo lock scope: este arquivo, CURRENT_STATE e relatório integrado.
> Ordem de execucao: final do pacote
> Depende de: specs playtest de save, death, UI, fishing e world readability
> Bloqueia: alegação de aceite integrado

required_adrs: []
required_game_rules: []

# /speckit.specify
Refinamento D21: transformar cobertura anterior parcial em evidência rastreável por fluxo. Phase0:
`docs/validation/playmode/PLAYMODE_SMOKE_CHECKLIST_MASTER.md` já existe e é o guarda-chuva canônico;
não criar outro framework de teste nem promover spec humana por execução de agente.
Os relatórios EXPLORATORY de 27/09 mostram cenário inicial/falhas e não validam correções novas.

# /speckit.plan
Reusar Test Runner Unity, runners `tools/unity`, validação strict, APIs Editor dos owners e checklist
master. Evidência do agente em `docs/validation/spec_playtest_integrated_coverage_v1/REPORT.md`
(máximo 60 linhas), logs/XML em TestResults/Logs. Campos: inputs/versão, cenário, preparação,
ação, esperado, observado, status, artefato; distinguir preparação controlada de caminho normal.
Não editar produto para fazer teste passar; falhas voltam à spec proprietária. Nenhum save real
do usuário; pasta temporária e confirmação do path antes de Play Mode. Preservar cenas dirty.

# /speckit.tasks
- [x] AC01: executar testes relevantes de cada owner em rodada integrada, XML com total positivo,
  falhas diagnosticadas; registrar filtros e exit. Nenhum PASS com zero casos ou arquivo velho.
- [ ] AC02: rota A boot→pausa/retomar→inventário/equipamento→loja→craft→save→load, verificando
  resultado visível e foco. Load restaura estado salvo sem duplicação/perda.
- [ ] AC03: rota B cultivo→pesca→Cave→combate→derrota→fonte→corpo; comparar estados e IDs,
  registrar limitações de input contínuo e screenshots apenas conforme matriz/keyart.
- [x] AC04: listar cobertura restante de quests/skills/atributos/estações/consumíveis/bosses/áudio/
  gamepad sem declarar sistema todo testado por painel aberto; estados NOT RUN permanecem explícitos.
- [x] AC05: revisão independente dos contratos de risco, strict no checkpoint e finish-spec por
  resultado real; nenhum status ACCEPTED do pacote com AC02/03 pendente.

Gate único de aceite: AC01–05 conjuntamente com evidência verificável. Compile isolado não substitui
rotas. Se infraestrutura ou decisão impedir parte, registrar PARTIAL com próximo cenário concreto;
continuar trabalho independente autorizado. Nenhum novo teste tautológico para checklist documental.
