# Revisão de autoria SDD — 2026-09-09

Escopo: três novas skills, extensão de spec-authoring, roteamento/documentação e nove rascunhos de
specs de habilidades. Nenhum arquivo de gameplay, scene, prefab ou sprite foi alterado por esta entrega.
O workspace já continha mudanças de outras tarefas; o escopo desta revisão não equivale ao diff inteiro.

## Revisão primária do trabalho delegado

Codex CLI executou `gpt-5.3-codex-spark`, esforço high, modo read-only, com fontes anexadas e sem ferramentas
no subprocesso: rascunhos save/UI e revisão de amostras do fluxo SDD. Spark não alterou o repositório.
Logs temporários identificam o modelo; o relatório primário registra decisões, não aceita resumos como prova.

| Sugestão/achado Spark | Decisão primária |
|---|---|
| Fallback SkillTreeManager.Instance no provider | Rejeitado: manter injeção explícita e diagnosticar ausência |
| Novo catálogo de ícones | Rejeitado: SkillNodeDataSO.Icon/SkillActionSO.Icon já existem |
| UI deixa variante pendente | Rejeitado: completar escolha/cancelamento com sobrecarga existente |
| Equipar bloqueado por readiness de uso | Rejeitado: equipar e executar têm regras distintas |
| HashSet em memória prova migração idempotente | Rejeitado: exigir comparação de cargas completas e save normalizado |
| Phase 0 de save pouco explícita | Aceito: acrescentadas chamadas/linhas e shape atual do DTO |
| TryPurchase e ValidatePurchase seriam nomes incompatíveis | Rejeitado: um é existente, outro extração proposta; texto esclarecido |
| 12+3→15→14 seria aprovação de balanceamento sem prova | Rejeitado: fixture de conservação de pontos, explicitamente não medida |
| Política de desconhecidos sem critério próprio | Aceito: AC05 e tabela versionada obrigatória antes da migração |
| Esc/bindings/lifecycle pouco explícitos nos testes | Aceito: cenários e task de OnEnable/OnDisable acrescentados |
| Gerador/prefab Canvas ainda sem ownership fechado | Mantido como pendência explícita; impede promoção da 06 |

## Cenários de uso das skills — revisão de instruções

| Entrada | Rota esperada e conferida na leitura |
|---|---|
| Ideia de magia sem cenário | refinement-authoring: evidência, alternativas e vínculo com mundo |
| Requisitos definidos, arquitetura por escolher | spec-planning: reuso e contratos, sem reinventar requisitos |
| Plan atualizado, tasks antigas | spec-task-authoring: apontar drift e atualizar tarefas afetadas |
| Spec legada completa | Mantém validade; não exige migração ou quatro arquivos novos |
| Alteração pequena | Profundidade proporcional; sem quota de testes/agentes |
| Proposta de balanceamento aberta | Continua proposta; não vira decisão ao escrever task |
| Pedido só de planejar | Não executa código; rascunhos fora da fila |

Esta tabela é revisão estática de cenários, não medição de disparo automático em nova sessão.
As cópias Codex foram geradas da fonte canônica; novas skills são descobertas conforme o carregamento do host.

## Verificações executadas e limites

- Generator Codex: 76 skills, 16 commands, 11 agentes, 23 rules; 0 problemas de frontmatter e 0 falhas de cópia.
- Test-CodexHarnessGeneration.ps1: PASS, 36 assertions em repositório temporário isolado.
- A geração também sincronizou `.codex/hooks.json` com as configurações canônicas já existentes; esta entrega não editou `.claude/settings.json` nem a lógica dos hooks.
- Conferência local: PASS — 9 specs, 56 tasks, 36 critérios, 108 links locais; sem AC órfão, task duplicada, ciclo de dependência, link quebrado ou spec ausente do índice.
- Segunda geração Codex: 0 arquivos alterados (idempotência).
- Strict validation, gate quality com manifesto do escopo: SCOPED_PASS; não é aprovação global.
- validate_docs.ps1 global: FAIL, 67 diagnósticos em outros documentos do workspace; nenhum cita este lote. Exemplos: marcadores/dependency headers de specs de farm/town e prefixos de quatro specs implementadas. Não foram corrigidos fora do escopo.
- SPEC_INDEX regenerado: 454 entradas; as nove novas specs permanecem fora da fila executável.
- Unity validation: NOT RUN. Reason: autoria documental/harness sem alterações de runtime. Command attempted: nenhum. Residual risk: specs ainda não demonstram comportamento em Unity.

## Prontidão do lote

Nove documentos foram redigidos com Spec/Plan/Tasks; são rascunhos revisáveis, não nove features prontas
para execução. A 00 concentra decisões D01–D07; 04 precisa fechar consumidores; 05 deve gerar filhas;
06 precisa fechar gerador/prefab; 07 precisa produzir/revisar a amostra visual. Demais pendências constam
nas próprias specs. Não esconder esses itens por um status “ready”.

O modelo anterior identifica riscos; não há aprovação global de equilíbrio. Nenhuma imagem foi gerada
nesta entrega. A spec 07 descreve como obter e aceitar pixel art real.
