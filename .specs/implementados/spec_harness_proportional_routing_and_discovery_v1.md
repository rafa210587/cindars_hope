# Harness — jornadas proporcionais e descoberta verificável

> Spec ID: `spec_harness_proportional_routing_and_discovery_v1`
> Status: ACCEPTED — implementada e validada no escopo em 2026-09-27; autorização humana nesta sessão.
> Type / Domain / Priority: Tooling + Governance / Harness / P1
> Wave: manutenção do harness existente; não cria macro-wave.
> Parallelizable: CONDITIONAL — tooling e instruções em arquivos separados; geração e closeout pelo owner.
> Repo lock scope: arquivos abaixo; preservar os quatro hooks untracked na raiz.
> Ordem de execucao: inventário → jornadas/contratos → implementação → validação/revisão → closeout.
> Depende de: harness vigente em `28b842cb` e tooling em `bc16a206`, já no checkout.
> Bloqueia: nenhum gameplay existente; não promove specs de gameplay.
> Validation level: contratos PowerShell, auditoria estrutural, cenários de decisão e paridade gerada.
> Executor: Codex; root integra, revisores somente leitura.

required_adrs: []
required_game_rules: []

# /speckit.specify

## Objetivo e Phase 0

Reduzir a chance de selecionar instruções erradas, repetir leitura/validação ou exigir uma jornada
de feature para corrigir um defeito local. Reforçar prevenção arquitetural sem novos agents,
skills, rules, commands ou hooks. O freeze permite correções dos existentes; ferramentas de
verificação desta spec não constituem novos artefatos dessas categorias.

Phase 0 de 2026-09-27: `Get-ChangeSummary.ps1` encontrou apenas quatro arquivos untracked
(`post-checkout`, `post-commit`, `post-merge`, `pre-push`), fora do escopo. O `/bugfix` ainda
exige docs/build por extensão e commit automático; o agente encaminha todo fix a `/validate-spec`
e obriga atualização de estado/backlog. O catálogo tem 80 skills, 16 commands, 11 agents e
25 rules; o gerador testa cópias, mas não oferece auditoria completa de descoberta/referências.
Base SOLID/testes/patterns da rodada anterior permanece; não reimplementar esses contratos.

## Critérios de aceite

- **AC01 — jornada simples:** correção localizada de comportamento existente ou ajuste de texto
  segue descrição/reprodução → menor fix → evidência pertinente → resposta curta. Não exige nova
  spec, plano separado, subagent, relatório, commit ou suite global. Uma feature continua exigindo
  spec aprovada. Risco de save, IDs, cross-system, lifecycle ou mudança intencional de contrato
  amplia análise/revisão/testes mesmo num diff pequeno; não vira dispensa por tamanho.
- **AC02 — descoberta verificável:** inventariar todos os artefatos canônicos, verificar catálogo,
  identidade, links Markdown locais explícitos fora de exemplos de código, referências tipadas
  `(skill: id)`/`(rule: id)`/`(agent: id)` e scripts locais referidos por settings hooks. Inputs
  ausentes/malformados retornam falha; findings ordenados, contagens completas e truncamento explícito.
  Não concluir qualidade semântica, execução de hooks ou aderência SOLID a partir desse PASS.
- **AC03 — progressive disclosure:** entrada distingue jornada e gatilho; detalhes/templates/exemplos
  são condicionais e ligados. Corrigir conflitos encontrados nas rotas auditadas. Não impor template
  ou few-shot a cada skill, nem carregar todo o catálogo de riscos para corrigir um typo.
- **AC04 — exemplos e cenários:** no recurso existente, exemplos contrastam fix local e mudança
  crítica pequena. Exercitar seis pedidos: typo, bug local, regra pura, transação/save, integração
  entre cenas e sprite/visual. Registrar seleção, leituras, evidência e escalada; revisão de decisão
  não é benchmark de comportamento real de modelos nem teste de gameplay.
- **AC05 — contrato dos verificadores:** fixtures válidas passam; catálogo órfão, referência quebrada,
  identidade duplicada/inválida, hook local ausente e entrada inexistente falham com exit não zero.
  Saída determinística para inputs iguais; exemplos/placeholders não geram falso finding.
- **AC06 — integração honesta:** gerar cópias, comprovar paridade/idempotência e revisar independentemente.
  Medir escopo e caracteres carregáveis, sem converter isso em tokens/custo. Não modificar runtime,
  baselines arquiteturais, permissões ou afirmar resolução dos 15 apontamentos do ratchet.

# /speckit.plan

## Ownership e arquivos

CREATE: `tools/codex/Test-HarnessDiscovery.ps1` e `tools/codex/Test-HarnessDiscoveryContracts.ps1`.
MODIFY (root): `CLAUDE.md`, `.claude/HARNESS_INDEX.md`, `.claude/commands/bugfix.md`,
`.claude/agents/bugfix-investigator.md`, `.claude/commands/audit-harness.md`,
`.claude/skills/harness-audit/SKILL.md`, `.claude/skills/harness-audit/references/audit-dimensions.md`,
`.claude/HARNESS_AUTHORING_STANDARD.md`, `.claude/skills/harness-authoring/assets/templates/skill-entry.md`,
`.claude/skills/harness-authoring/references/examples/conditional-art-review.md`,
`.claude/skills/spec-execution/SKILL.md`, `.claude/skills/spec-authoring/SKILL.md`, `tools/codex/README.md`.
Copias derivadas desses arquivos em `.agents/` e `.codex/`, bloco gerado de `AGENTS.md`, esta spec
(incluindo promoção para `implementados`) e `.specs/SPEC_INDEX.json/.md` são permitidos.
Reparos de referências adicionais exigem listar paths nesta spec antes do edit.

Emenda Phase 0: também MODIFY `AGENTS.md` somente políticas de leitura (invariantes intactas),
`.claude/rules/context-reading-policy.md`, `.claude/skills/implementation-closeout/SKILL.md`,
`.claude/skills/gameplay-test-scenario/SKILL.md`, `.claude/skills/quest-authoring/SKILL.md`,
`.specs/SPEC_VALIDATION_MATRIX_MASTER.md`. Motivos: spec obrigatória por encaminhamento no bugfix,
closeout/trigger desproporcional, três IDs de rules inexistentes e evidência exigindo commit.
Também permitido reparar a referência de regra inexistente em
`.claude/skills/time-calendar-weather/references/domain-contracts.md`, descoberta pelo checker.
Emenda pelos cenários: MODIFY `.claude/skills/runtime-bootstrap-pattern/SKILL.md` e
`.claude/skills/bootstrap-wiring/SKILL.md` (auto-bootstrap/Resources como padrão de criação
contradizem composição e ratchet); `.claude/skills/localization-authoring/SKILL.md` e
`.claude/skills/visual-asset-review/SKILL.md` (checks condicionais para typo e inspeção de alpha).

FORBIDDEN: `Assets/**`, `Packages/**`, `ProjectSettings/**`, baselines de arquitetura, hooks raiz,
alteração de permissões/semântica de hooks, exclusão de artefatos, novas features e nova plataforma
de roteamento. Sem push/PR/merge. Não adicionar relatório em `docs/validation` para este harness.

## Contratos propostos

`Test-HarnessDiscovery.ps1 -ProjectRoot <repo> [-MaxFindings 20] [-OutputPath <json>]`:
somente leitura da fonte; escrita opcional do JSON fora de Assets, nunca sobre input.
PowerShell 7. JSON com schemaVersion, counts por tipo, findings (code/path/message), totalFindings,
omittedFindings e result PASS/FAIL; exit 0 somente sem findings. Ordenar por path/code/message.
Sem network, Unity, chamadas de modelo, geração ou scanner de gameplay. Links externos são
inventariados/ignorados pela checagem local; existência não equivale a conteúdo correto.

Reusar o catálogo e as estruturas do gerador; não construir registry concorrente. Remover blocos
fenced antes de verificar links/exemplos; aceitar parâmetros `{{...}}` só em templates/exemplos.
Resolver links relativamente ao arquivo fonte; checar arquivos canônicos, sem seguir symlinks
para fora do repo. Referências de command como skills geradas são válidas quando o command existe.
Fixtures isoladas em temp usam o checker real; limpeza valida o caminho antes de remoção recursiva.

Jornadas em `CLAUDE.md` e `/bugfix`: selecionar por mudança de comportamento e risco, não LOC.
Plano/spec novos somente quando necessários ao contrato; testes podem ser escritos pelo owner.
`harness-audit` chama o checker e faz revisão contextual dos cenários; não confunde os dois níveis.
Templates têm placeholders sem resultados predefinidos; exemplos são hipotéticos e opt-in.

## Falhas e limites

Não ampliar baseline ou ocultar finding para obter PASS. Referência histórica não executável deve
ser rotulada ou corrigida, não resolvida com arquivo falso. Fonte/cópia divergente exige gerar a cópia;
zero arquivos auditados é erro. Freeze não impede ferramentas desta spec, mas continua impedindo
novas skills/rules/agentes/commands/hooks. Problema global alheio permanece documentado sem bloquear
artificialmente este escopo. Contagem de bytes e revisão de prompts não provam economia faturada.

# /speckit.tasks

- [x] T1 (AC01–04): fechar inventário, emendar paths necessários e revisar consistência da spec.
- [x] T2 (AC02/05): implementar checker e fixtures independentes; rodar testes negativos e positivos.
- [x] T3 (AC01/03/04): aplicar jornadas, disclosure, template e exemplos nos arquivos existentes.
- [x] T4 (AC02–06): auditoria do repo, revisão de seis cenários, geração/paridade e revisão independente.
- [x] T5 (AC01–06): conferir gate composto e `/finish-spec`, registrar evidência nesta spec e promover se elegível.

## Gate único de aceitação

Decisão composta: contratos do checker PASS + auditoria estrutural do repo sem finding não resolvido
no escopo + `Test-CodexHarnessGeneration.ps1` PASS + paridade/idempotência + seis cenários com
seleção proporcional revisada + diff/quality SCOPED_PASS. Falhas obrigatórias não são informativas.
Unity não aplicável: não há mudança C#/assets do jogo. Docs globais são diagnóstico separado.

## Evidência e closeout

Execução em 2026-09-27. Gate composto: SCOPED_PASS; promoção YES. Nenhum runtime alterado.

| Evidência executada | Resultado |
|---|---|
| `pwsh -NoProfile -File tools/codex/Test-HarnessDiscoveryContracts.ps1` | PASS, 38 assertions, exit 0; fixtures positivas/negativas, determinismo e proteção de output |
| `pwsh -NoProfile -File tools/codex/Test-HarnessDiscovery.ps1` | PASS, exit 0; 80 skills, 16 commands, 11 agents, 25 rules, 21 hooks; 192 Markdown, 173 links locais, 199 referências tipadas, zero findings |
| `powershell -NoProfile -File tools/codex/Test-CodexHarnessGeneration.ps1` | PASS, 40 assertions, exit 0 |
| `Generate-CodexHarness.ps1` e comparação SHA256 | Exit 0; 193 arquivos gerados sem drift na segunda execução; skills sem diferenças da fonte |
| `run_strict_validation.ps1 -Gates diff,quality -ScopePath <manifest temporário>` | SCOPED_PASS, exit 0; manifesto dos paths autorizados, quatro hooks raiz excluídos |
| `git diff --check` | PASS, exit 0 |
| Revisão independente do checker | Sem P1/P2 pendente; corrigida coerção de arrays em command/type; fixture de múltiplos -File |
| `tools/docs/validate_docs.ps1` | FAIL, exit 1; 173 diagnósticos globais, nenhum desta spec; problemas legados de nomes/metadados em outras specs |

Cenários revisados contra entradas reais (PASS contextual, não execução de modelos/gameplay):

| Pedido | Rota/leitura mínima | Evidência e limite |
|---|---|---|
| Typo | bugfix + texto; localization se pertinente | Diff, chaves/placeholders; sem spec/suite/relatório |
| Bug local | bugfix + owner/callers/teste relevante | Regressão focal; owner pode testar, sem delegação obrigatória |
| Regra pura nova | spec aprovada + spec-execution + domínio | Testes de comportamento/limites; PlayMode só se integração afetada |
| Transação/save | contrato aprovado + save/transactions | Atomicidade, compatibilidade, falha/retry; revisão por risco mesmo com uma linha |
| Wiring entre cenas | contrato + bootstrap/lifecycle | Integração real e preservação de estado/subscriptions; contrato novo escala |
| Alpha de sprite | visual-asset-review + imagem | Pixels/alpha; referência artística somente para fidelidade |

Medição de caracteres com finais de linha normalizados, HEAD versus trabalho:
CLAUDE 3803→4830; bugfix 1905→2337; bugfix-investigator 3735→2716;
bootstrap-wiring 6592→3316; runtime-bootstrap-pattern 2823→2134.
Entradas cresceram para explicitar decisões; referências selecionadas encolheram.
Não é medição de tokens faturados nem benchmark de economia: exige tarefas reais comparáveis.

Unity: NOT APPLICABLE (somente harness/tooling/documentação). Não reexecutados ratchet/runtime;
os 15 apontamentos arquiteturais da rodada anterior não foram resolvidos por esta entrega.
Parser estrutural tem suporte delimitado a frontmatter/Markdown; não valida semântica completa,
anchors ou execução de hooks. Nenhuma certificação global de SOLID ou das 80 skills.
Sem commit/push/PR; fontes canônicas e cópias geradas entregues no working tree.