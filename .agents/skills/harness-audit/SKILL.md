---
name: harness-audit
description: Audita descoberta, referências, limites e comportamento do harness .claude. Usar em /audit-harness, após mudanças de roteamento ou quando skills e agentes parecem indisponíveis ou contraditórios.
---

# Skill: Auditoria de Harness

HARNESS_INDEX é o catálogo fonte; CLAUDE mantém o acesso inicial e o gerador publica as cópias.

**Regra central: comprovar descoberta e decisões do workflow; tamanho sozinho não determina qualidade.**

## Quando usar
- Auditar um artefato, batch ou mudança do harness e seu efeito no roteamento.

## Checklist essencial
- [ ] IDs disponíveis no filesystem e catálogo, sem duplicatas ou triggers ambíguos.
- [ ] Links e referências transitivas resolvem na fonte e cópias afetadas.
- [ ] Entrada não carrega todos os detalhes por padrão.
- [ ] Capacidade, autorização e resultados de validação descritos sem pressupostos falsos.
- [ ] Cenários reais e regressões de scripts examinados conforme o escopo.

## Procedimento
1. Identifique escopo e baseline; confira [catálogo](../../../.claude/HARNESS_INDEX.md) sem abrir todo o corpus.
2. Para os checks concretos, leia [dimensões de auditoria](references/audit-dimensions.md).
3. Resolva links por arquivo/ID; rastreie referências necessárias além da entrada inicial.
4. Exercite pedidos representativos proporcionais à mudança, avaliando leitura selecionada,
   ownership, ação permitida e evidência exigida. Não testar apenas presença de palavras.
5. Reporte findings por risco e impacto com arquivo/linha e correção mínima.

## Limites de edição
Pedido apenas de auditoria entrega findings, sem mudanças. Se a tarefa já autoriza correções,
execute no ownership autorizado sem nova confirmação; não ampliar para artefatos alheios.

## Quando NÃO usar
- Auditar gameplay/runtime: `non-regression-review`.
- Criar artefato novo: `harness-authoring` e seus critérios de validação.

## Quando parar e reportar
Sem acesso ao gerador/artefato necessário, registrar check não executado e risco residual.
Ausência de imagem/tool não pode virar PASS por análise textual.

## Saída esperada
Scope, checks realizados, findings, ações autorizadas executadas e pendências.
Métricas de bytes/caracteres são proxy de contexto; não demonstram economia real de tokens/faturamento.
