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
