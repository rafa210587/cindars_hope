# Habilidades de Vaalara — lote SDD v1

**Entrega de planejamento, revisão 1.** Nove specs com Spec, Plan, Tasks, critérios e testes esperados.
Os rascunhos originais permanecem como histórico de planejamento. O usuário autorizou depois a implementação; versões executáveis 01/02 e a decisão D01 estão em a_implementar. Ver [execução e evidência](../../../docs/validation/skills_sdd_v1/execution/FOUNDATION_REPORT.md).

O fluxo é **refinamento → spec → plan → tasks → revisão de consistência → implementação → validação**.
Separar as responsabilidades não exige três cópias da informação: cada spec deste lote contém as três
seções, na mesma revisão. O índice atual varre arquivos Markdown; manter tudo no documento canônico
evita contar plan/tasks como features diferentes.

## Ordem e dependências

| ID | Spec | Depende de | Tasks |
|---|---|---|---|
| 00 | [Reconciliação de contratos e decisões das habilidades](spec_skills_00_decisoes_catalogo_v1.md) | — | 5 |
| 01 | [Vínculo de habilidades ao avatar e aplicação real dos efeitos](spec_skills_01_avatar_execucao_v1.md) | — | 6 |
| 02 | [Integridade de pontos, compras, respec e restauração](spec_skills_02_pontos_respec_save_v1.md) | — | 6 |
| 03 | [Dados de execução, ranks e estado consultável das habilidades](spec_skills_03_dados_rank_readiness_v1.md) | 00, 01, 02 | 6 |
| 04 | [Consumidores de passivas e gatilhos reais de capstone](spec_skills_04_passivas_capstones_v1.md) | 00, 02, 03 | 7 |
| 05 | [Identidade mecânica das ações ativas e restaurações justificadas](spec_skills_05_mecanicas_ativas_v1.md) | 00, 01, 03 | 7 |
| 06 | [Tela de habilidades em Canvas com compra, rank, variante e slots](spec_skills_06_ui_canvas_skills_v1.md) | 02, 03 | 7 |
| 07 | [Pixel art da tela e feedback visual das habilidades](spec_skills_07_pixelart_feedback_v1.md) | 03, 06 | 6 |
| 08 | [Equilíbrio reproduzível e aceitação integrada das builds](spec_skills_08_equilibrio_aceitacao_v1.md) | 00, 01, 02, 03, 04, 05, 06, 07 | 6 |

As specs 00, 01 e 02 não dependem umas das outras para planejar. A execução é serial por padrão,
porque várias fatias compartilham SkillTreeManager, catálogo e controller. Delegar somente após
fixar ownership e contratos. A spec 05 é guarda-chuva: precisa gerar filhas por família antes de
entrar na fila. A 08 espera essas filhas implementadas, não apenas documentação concluída.

## O que está decidido e o que continua aberto

Corrigir avatar errado, evitar perda/duplicação de pontos, mostrar rank/variante e ligar arte aos
SOs existentes são objetivos sustentados pela auditoria. A implementação ainda exige fechar os
detalhes de contrato indicados em cada plano.

D01 foi aprovada explicitamente: recarga compartilhada por actionId, preservada ao trocar/copiar slots. A fatia D01 foi implementada e validada; isso não conclui a spec03 de dados/readiness. Não há aprovação para alterar ranks de capstone, permissões de skills
dormentes, regeneração, descontos ou gatilhos. A spec 00 organiza essas decisões D01–D07.
As propostas de Contra-ataque/Purify restauram direções existentes; Contenção de Circuito fica
como hipótese condicional, fora de implementação. A spec 07 cobre produção real de pixel art,
mas ainda não contém sprites produzidos ou uma amostra visual aceita.

## Como executar corretamente depois

1. Fechar as pendências de decisão e de ownership do plano da fatia escolhida.
2. Conferir o código atual e a revisão conjunta de Spec/Plan/Tasks; resolver drift.
3. Passar o Depth Gate e registrar aprovação aplicável, então mover a fatia pronta para
   `.specs/a_implementar/` com seus links/dependências atualizados. A posição na pasta não é aprovação.
4. Executar por `/execute-spec-strict`, marcar tasks somente com evidência e fechar por `/finish-spec`.

Não executar o lote inteiro por wildcard. Os rascunhos ficam fora da fila justamente para evitar
que decisões pendentes sejam tomadas incidentalmente durante o código.

## Base e revisão

- [Auditoria de código](../../../docs/validation/SKILLS_CAPABILITIES_AUDIT_2026_09_09.md).
- [Refinamento técnico/UI](../../../docs/refinements/a_implementar/ref_skills_capabilities_gameplay_ui_pixelart_v1.md).
- [Refinamento Vaalara/equilíbrio](../../../docs/refinements/a_implementar/ref_vaalara_skills_design_and_balance_v2.md).
- [Modelo analítico e limites](../../../docs/validation/skills_balance_v1/BALANCE_REVIEW.md).
- [Revisão do lote e das skills](../../../docs/validation/skills_sdd_v1/SDD_AUTHORING_REVIEW.md).

Spark apoiou rascunhos de contratos de save/UI e revisão de amostras. A revisão primária rejeitou
fallback de singleton, catálogo de ícones paralelo e UI de variante incompleta; uma sugestão do
revisor que confundia valor esperado de teste com aprovação de balanceamento também foi rejeitada.

