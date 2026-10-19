# Dimensões de auditoria

Escolha as dimensões afetadas pelo pedido. Auditoria integral cobre as aplicáveis; não exige
ler todas as referências de todas as skills sem relação com o problema.

## Descoberta
- Compare pastas com SKILL.md, commands, rules e agents com seções correspondentes de
  `.claude/HARNESS_INDEX.md`; confirme link do roteador e IDs sem colisões.
- Consulte o formato efetivo do catálogo antes de extrair tabelas. Não reutilize regex antiga
  que presumia tudo em CLAUDE.md. Nome listado não prova que uma sessão antiga recarregou o agente.
- Confira frontmatter funcional, paths e triggers: a descrição distingue uso de arte estática,
  animação, cena, runtime, revisão e validação quando houver workflows próximos?

## Referências e cópias
- Resolva links relativos a partir do arquivo de origem; valide também IDs skill/rule/agent
  na fonte. Uma referência citada deve existir e ser acessível no destino publicado.
- Siga referências transitivas do escopo auditado, incluindo detalhes em references/.
- Detecte paths antigos e scripts inexistentes pelo filesystem; não marque toda menção de
  IMPLEMENTATION_STATUS como obsoleta: contexto de closeout pode legitimamente gravá-lo.
- Verifique cópias com o gerador vigente, diferenças esperadas e idempotência após fontes iguais.
  Não corrija o arquivo gerado manualmente.

## Carga e consistência
- Meça bytes/caracteres/linhas quando útil para comparar entrada e detalhes carregados por cenário.
- Entrada extensa sinaliza oportunidade de revisão; entrada curta com dependências obrigatórias
  enormes também pode falhar. Não usar 150/130 linhas como aprovação/reprovação automática.
- Excluir requisitos por presença/ausência de cabeçalho: julgue se limites e saída são claros.
- Diferenciar contagem contratual (ex.: grade 5x5 do consumidor NPC) de contagem volátil
  de testes/validators ou resultado PASS hardcoded. Só a segunda é drift por natureza.

## Capacidade, autorização e validação
- Ferramentas/modelos não devem ser inventados. Alias não demonstra capacidade de ver imagem
  ou custo. Instrução de geração deve identificar resultado real, limites de retry e evidência.
- Delegação deve resolver fatia independente ou review útil, preservando ownership e dirty;
  implementador pode fazer testes pequenos. Auditor não corrige os próprios findings.
- Pedido de auditoria não autoriza correções por si só; edição já autorizada não requer nova pergunta.
- Teste comportamento de scripts modificados: erro deve produzir falha real, repetição com
  mesmos inputs deve respeitar contrato, resultado não pode estar predefinido no checklist.
- Não exigir Unity em alteração exclusiva de harness. Ao afetar regra de validação, conferir que
  a instrução conserva evidência, falhas e escopo sem afrouxar gates de runtime.

## Cenários para mudanças de roteamento
Bug simples; refactor de responsabilidade; sprite estático; ciclo animado; montagem de cena;
validação sem edição de gameplay. Para cada cenário, registre entrypoint, referências necessárias,
limites de ação e evidência esperada. Escolha os cenários pertinentes, sem execução de efeitos
externos ou geração paga apenas para testar instruções.

## Journey scenarios for routing changes

Review these decisions against actual entries; do not execute a hypothetical gameplay change.
Use only applicable rows for a small edit. A broad routing change reviews all six.

| Request | Expected entry/read | Evidence and escalation |
|---|---|---|
| Fix a typo in existing text | `/bugfix`, target/context; localization only when relevant | Diff/placeholder/key inspection; no new spec, test suite, report or commit. |
| Fix an isolated calculation defect | `/bugfix`, owner and relevant callers/test | Focused behavioral regression or concrete limitation; no wave protocol or separate reviewer by default. |
| Add a pure gameplay rule | Approved spec, `spec-execution`, applicable domain contract | Relevant deterministic behavior/boundary tests; pure logic alone does not require PlayMode. |
| Change an inventory/save transaction | Approved contract/spec, save/transaction skills | Atomicity, compatibility, failure and retry checks; independent review by risk, even for one line. |
| Repair scene transition wiring | Existing contract, bootstrap/lifecycle skills | Actual integration and state/subscription preservation; escalate beyond simple bugfix if contracts change. |
| Review a sprite's transparency | `visual-asset-review`, image inspection reference | Inspect pixels/alpha; no animation generation or runtime refactor; unavailable image is NOT RUN. |

For each row, record entry, necessary reads, action boundary and expected evidence in the
existing spec/task result. Also record undesired work avoided and unresolved ambiguity.
This is a decision review, not proof a live model will select correctly. To claim improvement
in model efficiency, compare representative real tasks with equivalent inputs/model/tool access:
actual usage if available, retries, findings, repeated reads/checks and acceptance. No fixed
token percentage, agent quota or universal template count is an acceptance criterion.
