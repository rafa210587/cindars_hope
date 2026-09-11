# /audit-harness

Audita descoberta, referências e decisões do harness .claude, com findings proporcionais ao impacto.

**Arguments:** `$ARGUMENTS` — opcional: artefato, `skills`, `agents`, `rules` ou outro escopo concreto;
sem filtro, auditar as dimensões aplicáveis ao harness.

## Quando usar
- Após mudanças de roteamento ou batch de artefatos; para investigar recurso não descoberto ou instrução contraditória.

## Procedimento
1. Usar `harness-audit`; selecionar dimensões e cenários pertinentes ao escopo.
2. Conferir descoberta pelo `.claude/HARNESS_INDEX.md`, referências transitivas e cópias publicadas.
3. Priorizar findings por risco: instrução incorreta, referência quebrada, perda de segurança/evidência
   ou misrouting. Tamanho de arquivo é sinal para análise, não reprovação por limite arbitrário.
4. Entregar localização, evidência, impacto e correção mínima. Auditoria isolada não edita;
   correções já autorizadas são executadas no ownership permitido sem nova pergunta.

## Não fazer automaticamente
- Deletar artefatos, promover specs, alterar gameplay ou ampliar o escopo da autorização.
- Tratar formato correto, contagem de linhas ou palavra PASS como prova de comportamento.

## Saída esperada
Scope, checks realizados, findings priorizados e limitações. Quando houver autorização de edição,
incluir correções feitas, validação e arquivos alterados; caso contrário, apresentar ações recomendadas.
