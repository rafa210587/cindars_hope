---
required_adrs: []
required_game_rules: []
---

# Vínculo das habilidades ao avatar e aplicação dos efeitos

> Spec ID: spec_skills_01_avatar_execucao_v1
> Status: PLAYMODE_VALIDATED — fundação técnica; aceitação final da wave pendente
> Wave: SKILLS_SDD_V1 | Prioridade: P0 | Domínio: Skills
> Revisão: 2 — 2026-09-10
> Depende de: nenhuma mudança de design
> Execução: agente primário autorizado após limite do Spark; revisão independente
> Evidência: [relatório](../../docs/validation/skills_sdd_v1/execution/FOUNDATION_REPORT.md)

## Refinamento incorporado

Vaalara continua um RPG de ação classless, com cinco árvores e quatro slots. A influência de D&D orienta papéis e ferramentas; esta correção não introduz classes, d20 ou descanso. Farm, Town e Cave preservam seus próprios loops. IDs e números de dano/custo existentes permanecem. Nenhuma mudança procedural de cave.

O caster era derivado do PlayerManager em _Bootstrap, separado do avatar. Quatro projéteis declaravam um status com probabilidade zero. O avanço físico e o golpe podiam acontecer em posições diferentes; colliders múltiplos permitiam dano repetido.

# /speckit.specify

## Spec

- AC01: Farm/Town/Cave usam o avatar ativo como origem, inclusive após substituição na mesma cena.
- AC02: avatar inválido, recurso obrigatório ausente/insuficiente, índice inválido e modal impedem execução sem débito ou nova recarga.
- AC03: cada receptor recebe um hit por golpe; avanço respeita parede, aplica hit após a física e libera sua própria interrupção no disable.
- AC04: Flecha Lacerante, Prisão de Gelo, Rajada Gélida e Nuvem Tóxica configuram chance explícita 1, conforme o addendum existente.
- AC05: disable/unload remove referências antigas; re-enable conserva acesso ao controller persistente.

Fora do escopo: UI Canvas, arte final, rank scaling, novos efeitos e rebalanceamento. O nome de algumas habilidades ainda promete mecânicas adicionais; corrigir isso pertence às specs seguintes, sem alegar conclusão deste lote inteiro.

# /speckit.plan

## Plan e ownership

Modificar somente:

- Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs
- Assets/_Game/Scripts/Skills/Runtime/Effects/MeleeStrikeSkillEffectExecutor.cs
- Assets/_Game/Scripts/Skills/Runtime/Effects/ProjectileSkillEffectExecutor.cs
- Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutorCatalog.cs
- Assets/_Game/Scripts/Player/Movement/PlayerMovementDisplacementResolver.cs

Criar testes:

- Assets/_Game/Tests/EditMode/Skills/SkillExecutionFoundationTests.cs
- Assets/_Game/Tests/PlayMode/Composition/SkillExecutionFoundationPlayModeTests.cs

Reusar PlayerController.ActiveInstance; atualizar BindAvatar antes de executar e limpar no disable/unload. Não criar busca global ou novo manager. Reusar PlayerMovementDisplacementResolver.TryDisplace com conclusão após o último passo físico. Exigir collider/Rigidbody para lunge; preservar duração equivalente existente de 0,22 s. Deduplicar receptores por EnemyHealth. Resolver status antes de pagar; recusar managers obrigatórios ausentes. Recarga é responsabilidade da [decisão D01](spec_skills_03_shared_cooldowns_v1.md), que substitui a antiga recarga por slot.

# /speckit.tasks

## Tasks

| Estado | Task | Critérios |
|---|---|---|
| [x] | T01: bind por avatar ativo, refresh na execução, limpeza e re-enable | AC01, AC02, AC05 |
| [x] | T02: deduplicação de hits e avanço pelo resolver existente | AC03 |
| [x] | T03: preflight de recursos/status e chance explícita no catálogo | AC02, AC04 |
| [x] | T04: regressões EditMode e PlayMode escritas; rodada inicial executada | AC01–05 |
| [x] | T05: repetir cenário físico com bootstrap de gameplay e validar capturas atuais | AC03–05 |
| [x] | T06: obter revisão integrada e fechar todos os gates antes de promoção | AC01–05 |

## Validação e limites

EditMode integrado: 131/131 PASS. PlayMode: 4/4 cenários de fundação mais 1/1 cenário cobrindo os quatro status reais do catálogo PASS. Revisor independente sem bloqueadores técnicos. Histórico das falhas intermediárias preservado no relatório.

Capturas 1280×720 nas três cenas inspecionadas: projétil funcional, arte provisória. A câmera isolada não prova UI overlay; a apresentação de Cave tem problemas visuais fora desta fatia. Sem aceite visual/humano final.

Mantida na fila para fechamento da wave. As specs de Canvas, pixel art e equilíbrio continuam pendentes.

## Dependências de execução

Ordem de execucao: fundação serial antes de dados/readiness e UI.
Depende de: contratos atuais de avatar, progressão e save, já existentes; nenhuma decisão de capstone.
Bloqueia: fechamento integrado de UI e equilíbrio do lote SKILLS_SDD_V1.

