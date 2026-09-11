# Skills — fase mecânica 1B: timing, cancelamento e commit

> **Spec ID:** spec_skills_10b_action_timing_commit_v1  
> **Status:** IMPLEMENTADA E VALIDADA — 2026-09-10  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Runtime+Data / Skills / P0  
> **Parallelizable:** NO  
> **Depends on:** spec_skills_10_mechanics_data_rank_readiness_v1  
> **Blocks:** spec_skills_11_melee_equipment_shapes_v1 e todas as ações seguintes  
> **Validation:** Unity compile + EditMode state machine + PlayMode skills  
> **Executor:** Codex/subagent; root valida

# /speckit.specify

## Problema e resultado

Windup, active e recovery fazem parte do risco e do DPS das habilidades, mas o runtime atual aplica o efeito
imediatamente. Adiar esses tempos para a arte invalidaria o balanceamento e deixaria cancelamento/commit
implícitos. Esta fase cria a linha temporal mecânica antes das demais famílias, sem exigir sprites ou VFX.

- **R01/AC01:** `SkillActionSO` guarda `WindupSeconds`, `ActiveSeconds`, `RecoverySeconds` e `TimingProfileId`;
  M1 usa 0,10/0,10/0,20 e M2 usa 0,20/0,18/0,38.
- **R02/AC02:** uma state machine pura progride Idle→Windup→Active→Recovery→Idle com tempo escalado; zero
  preserva execução imediata de ações ainda não migradas.
- **R03/AC03:** readiness completo ocorre antes do windup; custo/efeito/cooldown ocorrem uma vez na transição
  de commit. Falha no commit volta a Idle sem cooldown.
- **R04/AC04:** morte, troca de cena, modal e dano antes do commit cancelam sem custo/cooldown. Após commit,
  cancelamento não restitui custo. Stagger dedicado será ligado quando existir evento canônico; dano é o sinal
  vigente e não se cria um evento fictício.
- **R05/AC05:** recovery bloqueia iniciar outra skill; whiff de melee é commit válido e mantém recovery.
- **R06/AC06:** fases publicam DTO pelo `GameEventBus` com actionId/fase/duração, sem Unity refs, para a etapa
  posterior de animação/VFX/SFX.

Fora: sprites, Animator, hit-stop, VFX/SFX, timing de Ranged/C1/C2/U1/O1 e mudanças de dano/custo.

# /speckit.plan

## Contratos e ownership

- MODIFICAR `Assets/_Game/Scripts/Skills/SkillActionSO.cs`.
- MODIFICAR `Assets/_Game/Scripts/Skills/DefaultSkillActionCatalog.cs` somente para M1/M2.
- MODIFICAR `Assets/_Game/Scripts/Editor/Skills/GenerateCanonicalSkillCatalog.cs` e regenerar os 31 actions.
- CRIAR `Assets/_Game/Scripts/Skills/Runtime/SkillCastTimeline.cs`: estado puro, tick determinístico e decisão
  `ShouldCommit/Completed/Cancelled`.
- CRIAR `Assets/_Game/Scripts/Skills/Runtime/Effects/IPreparableSkillEffectExecutor.cs` com
  `SkillEffectResult Validate(SkillEffectContext context)` sem mutação.
- MODIFICAR `MeleeStrikeSkillEffectExecutor` para implementar Validate e repetir a validação crítica no commit.
- MODIFICAR `ActiveSkillExecutionController` para manter no máximo um cast, snapshotar contexto/action/rank,
  avançar a timeline e executar no commit.
- CRIAR `SkillCastPhaseChangedEvent` em `Assets/_Game/Scripts/Core/Events/PlayerCombatEvents.cs` como DTO.
- CRIAR testes `SkillCastTimelineTests.cs` e ampliar PlayMode foundation.
- CRIAR relatório `docs/validation/skills_sdd_v1/mechanics/PHASE_01B_TIMING_COMMIT_REPORT.md`.

Não usar coroutine por executor nem números no `MonoBehaviour`. A timeline usa `Time.deltaTime` do bridge,
mas toda transição é calculada em C# puro. Troca de cena e `PlayerDiedEvent` cancelam por assinatura com
unsubscribe; modal/dano consultam eventos/estado vivo. A implementação não persiste cast parcial em save.

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [x] | T01 | Campos e profiles M1/M2 data-driven + gerador | AC01 |
| [x] | T02 | State machine pura e testes de fronteira/cancelamento | AC02–AC05 |
| [x] | T03 | Validate sem mutação para melee | AC03–AC05 |
| [x] | T04 | Integrar controller, commit único, eventos e lifecycle | AC03–AC06 |
| [x] | T05 | PlayMode: timing, whiff, cancelamento antes/depois do commit | AC01–AC06 |
| [x] | T06 | Gerar assets, Unity compile, EditMode, PlayMode e revisão independente | AC01–AC06 |

## Testes nomeados

- `Timeline_M1_CommitsOnceAtPointTen_AndCompletesAtPointForty`.
- `Timeline_CancelBeforeCommit_NeverRequestsCommit`.
- `Whirl_Whiff_CommitsCostCooldownAndRecovery`.
- `DamageDuringWindup_CancelsWithoutCommit`.
- `SceneChangeDuringWindup_CancelsWithoutCommit`.
- `InputDuringRecovery_DoesNotStartSecondSkill`.
