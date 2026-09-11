---
required_adrs: []
required_game_rules: []
---

# Recarga compartilhada por habilidade — fatia D01

> **Spec ID:** spec_skills_03_shared_cooldowns_v1
> **Status:** PLAYMODE_VALIDATED — aceitação final da wave pendente
> **Wave:** SKILLS_SDD_V1
> **Type:** Runtime
> **Priority:** P1
> **Domain:** Skills
> **Depende de:** spec_skills_01_avatar_execucao_v1
> **Bloqueia:** UI de habilidades com recarga correta
> **Ordem de execucao:** após fundação; antes do Canvas
> **Repo lock scope:** ActiveSkillExecutionController.cs, SkillCooldownTracker.cs e testes do tracker

# /speckit.specify

O usuário escolheu explicitamente recarga compartilhada pela habilidade, inclusive após troca de slot.
Esta fatia executa somente D01 do lote; tuning/ranks/novas mecânicas do rascunho 03 continuam separados.
Baseline: ActiveSkillExecutionController mantém dois arrays float[4], consulta recarga por slot e só
decrementa depois do guard de modal. Isso permite copiar/trocar ação e congela recarga com mundo rodando.

- AC01: cópias da mesma actionId mostram/consomem a mesma recarga; trocar slot não a reinicia.
- AC02: ações distintas não compartilham recarga entre si.
- AC03: recarga acompanha tempo de gameplay; modal com mundo rodando não a congela, pausa real congela.
- AC04: índices inválidos/vazios retornam zero sem lançar; APIs públicas da HUD continuam compatíveis.

# /speckit.plan

CRIAR Assets/_Game/Scripts/Skills/SkillCooldownTracker.cs, C# puro, sem manager/global state:
`void Start(string actionId, float now, float duration)`;
`float Remaining(string actionId, float now)`; `float Total(string actionId)`.
Armazenar prazo absoluto e duração por ID, usando StringComparer.Ordinal. Ignorar ID vazio,
valores não finitos ou duração não positiva; nunca produzir NaN para fill da UI.

MODIFICAR ActiveSkillExecutionController: substituir arrays privados pelo tracker; getters resolvem
actionId atual a partir de SkillTreeManager.State e alias legado nodeId; usar Time.time como relógio
escalado. TryExecuteSlot consulta chave resolvida, Start só após resultado de sucesso existente.
Preservar números e política de sucesso desta fatia. Não persistir relógio absoluto em save.
Mantém recarga em transições da sessão no controller persistente; restart de processo continua sem
persistência como no baseline. Persistir cooldown entre sessões requer outro contrato.

# /speckit.tasks

- [x] T01 / AC01–03: implementar tracker de prazos por ID e relógio injetado.
- [x] T02 / AC01–04: ligar execução e getters da HUD ao mesmo tracker, removendo decremento por slot.
- [x] T03 / AC01–04: testes determinísticos de duplicatas/troca, independência, prazo exato/pausa e entradas inválidas.
- [x] T04 / AC01–04: Unity compile/EditMode e cenário integrado com slots; registrar evidência e revisão independente.

Testes propostos em Assets/_Game/Tests/EditMode/Skills/SkillCooldownTrackerTests.cs. Esperado:
ação A disparada em t=10 por 6 s tem restante 4 em t=12 em qualquer slot; B tem zero; t=16 retorna zero.
Manter t constante (pausa) mantém restante; avançar t sob modal reduz restante. Esses são resultados
esperados, não evidência executada. Reusar skills unity-validation e non-regression-review.

## Evidência

131/131 EditMode integrado e controller real nas três cenas PASS. Ver [relatório](../../docs/validation/skills_sdd_v1/execution/FOUNDATION_REPORT.md). Não promove a spec03 original de dados/rank/readiness.


Ordem de execucao: junto da fundação de avatar, antes da UI.
Depende de: D01 aprovada pelo usuário.
Bloqueia: aceitação de recarga compartilhada na UI e equilíbrio integrado.

