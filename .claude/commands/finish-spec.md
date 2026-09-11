# /finish-spec

Confere evidência e decide promoção sem repetir gates válidos.

**Arguments:** `$ARGUMENTS` — spec e execution report.

## Procedimento

1. Ler spec/report, CURRENT_STATE e SPEC_VALIDATION_MATRIX_MASTER; consultar protocolo
   da wave para promoção. Identificar requisitos aplicáveis e autorização da sessão.
2. Conferir critérios/integração e artefatos reais. Resumo de subagent sozinho não é prova;
   log/XML vigente verificado não exige reexecução automática.
3. Determinar o status pelo nível realmente comprovado. Scoped PASS não é global PASS;
   manter falhas globais/pendências visíveis e não promover por simples alteração de regra.
4. Para humano pendente, usar cenário por fluxo/wave em FINAL_HUMAN_VALIDATION_BY_WAVE
   ou arquivo por spec quando útil/exigido. Vincular critérios; não criar duplicata obrigatória.
   Cenário escrito corresponde a DEFERRED_TO_FINAL_HUMAN_VALIDATION, não a ACCEPTED.
5. Promover somente quando evidência e protocolo permitirem. Se não elegível, registrar
   o nível real e Remaining work; não alterar status para BUILD_VALIDATED por padrão.
6. Quando elegível e autorizado: mover spec/refinement, adicionar header de evidência,
   atualizar índices/status pertinentes e validar docs após esses edits.
   Compile/testes inalterados continuam válidos; não repetir Unity/build por mover .md.

## Saída esperada

Spec, status, promoção YES/NO com motivo, evidência, falhas/NOT RUN/risco e próxima ação.
Docs-only não exige build C#/Unity. Runtime exige comportamento/integração conforme matriz.

## Não fazer automaticamente

Não mover spec sem elegibilidade; não declarar humano PASS por cenário escrito;
não fazer push/PR/commit contra instruções da sessão; não apagar histórico.
