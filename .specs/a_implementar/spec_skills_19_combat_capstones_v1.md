# Skills — fase mecânica 8: capstones de combate

> **Spec ID:** spec_skills_19_combat_capstones_v1  
> **Status:** APROVADA — autorização humana de 2026-09-10; refinada após audits técnico e de balanceamento  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Runtime+Save / Skills+Combat / P0  
> **Parallelizable:** por subfase; **Depends on:** spec_skills_18_crafting_economy_passives_v1  
> **Blocks:** spec_skills_20_survival_crafting_capstones_v1 e UI  
> **Validation:** Unity compile + EditMode + PlayMode por subfase + reauditoria independente

# /speckit.specify

## Decisões de equilíbrio

Os valores aprovados são preservados. Rank aumenta somente o resultado; frequência, janela e
condição não ficam mais fáceis. Nenhum capstone reage a dano secundário, DoT, reflexo ou proc
gerado por outro capstone.

- **Kanthor — Julgamento de Aço:** perfect block ou posture break causado pelo player ativa por
  8 s dano melee direto +10/12/15%, redução de knockback recebido 15/20/25% e redução somente da
  duração de Stun 12/16/20%. Eventos com o mesmo `ResolutionId` ativam uma vez. Novo gatilho
  refresca 8 s e rearma no máximo uma carga. O próximo melee direto confirmado com
  `FinalDamage > 0` consome a carga e cura `ceil(MaxHP × 2/3/3%)`; no R3, se o player já estava
  com HP cheio antes da recompensa, restaura 10 STA em vez de curar.
- **Kaand — Fúria de Aço:** posture break causado pelo player ativa por 8 s dano melee direto
  +18/24/30% e critical damage +15/20/25%. Refresca, não empilha e não cura.
- **Foco Lunar:** dura 6/8/10 s sobre o primeiro alvo atingido por Marked Prey em cada encounter.
  Esse primeiro-alvo fica consumido no encounter mesmo se morrer, a marca expirar, houver load
  ou respec. Alihana dá +8/12/16% range a novos lançamentos ranged enquanto o alvo vive e +15%
  projectile speed no R3. Senya causa dano elemental secundário de 8/12/16% do hit ranged
  primário usando o `DamageType` da última magia ofensiva confirmada; sem histórico, não há dano
  extra. O secundário não causa status, posture, crit, reação ou recursão. Nyx dá +8/12/15 pontos
  percentuais de crit e +15% critical damage no R3 enquanto não existir outro hostil vivo do
  encounter a até 2,5 tiles do alvo; o bônus desliga e religa sem pausar a duração. O único roll
  crítico ranged ocorre no impacto.
- **Confluência:** soma MP líquido somente de magias confirmadas numa janela deslizante de 6 s.
  O primeiro commit fixa o snapshot de MaxMP; threshold é `ceil(snapshot × 35%)`. Reserva
  cancelada/reembolsada vale zero. O cast que cruza o threshold arma, mas não autoconsome.
  A semente armada dura 10 s (Anya) ou 8 s (Senya); classe errada não consome. Consumo inicia
  lockout de 25 s; gastos durante lockout não contam.
- **Anya:** consome apenas magia `Spiritual`; custo final positivo é
  `ceil(custo_base × (1−30/40/50%))`, output espiritual efetivo (cura/barreira/purificação) recebe
  +20/28/35%, nunca RestoreMP. O eco de stamina é `floor(MP pós-desconto × 20/28/35%)`, entregue
  ao longo de 1/2/4 s.
- **Senya:** consome apenas magia `Offensive`; antes do debit exige custo normal mais surcharge
  `ceil(custo_base × 5/8/10%)`. Instâncias diretas do cast recebem dano +20/28/35% e o único roll
  crítico recebe +8/12/15 pontos percentuais. Não existe canal `overload`; crit é o contrato vivo.

## Contratos

- `DamageRequest`/`DamageResult` carregam, com defaults neutros, `SourceKind`, source/target
  instance IDs, `ActionToken`, crítico, primário/secundário e gates de capstone/status/reação.
  `EnemyHealth` fixa seu `TargetInstanceId` antes do cálculo. `DamageAppliedEvent` pós-HP é a
  confirmação de consumo de carga.
- Perfect block e posture break compartilham `ResolutionId`; posture informa instance ID,
  source e se foi causado pelo player. Consumidores legados continuam compatíveis.
- Bônus temporários são consultas síncronas anteriores ao cálculo, por providers Foundation;
  EventBus comunica ativação, expiração e feedback.
- Stability consulta um provider no caminho único de knockback do player. Stagger resistance
  consulta o mesmo contrato apenas para duração de Stun.
- O encounter existente é generalizado, sem segundo tracker: aggro inicia, reforços entram,
  `all_dead` ou leash quiet por 8 s resolve, em Cave ou fora dela. Started inclui o primeiro
  `EnemyInstanceId`; snapshots vivos usam IDs e posições determinísticas.
- Projectile carrega contexto de ação e resolve modificador/crit no impacto. O antigo roll de
  crit descartado no lançamento é removido.
- `SpellDiscipline { None, Spiritual, Offensive }` é campo autorado em `SpellDataSO` e
  `SkillActionSO`; não é inferido por shape. `None` nunca arma ou consome Confluência.
- Toda magia elegível, incluindo active magic skills, usa prepare/reserve/commit/cancel comum.
  Senya cobra o total atomicamente; Anya reduz antes do debit; commit ocorre somente após
  resolução bem-sucedida.
- Save persiste somente tipos simples: variante/rank, timers restantes, ledger da janela,
  primeiro alvo consumido, marca/target/token e lockout. Restore não publica trigger.
- Respec remove buff, marca ativa, arm e ledger sem reembolso; preserva lockout já iniciado e o
  primeiro-alvo consumido do encounter. Buffs nunca empilham.

## Fora de escopo

Telisandra/Thoren, UI, arte, um sistema global novo de posture do player e mudança global de
control DR. O aceite visual ocorre depois das fases de skill tree, loadout e animação.

# /speckit.plan

Executar quatro subfases estritas:

1. **19A — identidade e transações:** campos aditivos de dano, causal token de block/posture,
   provider de controle, classificação autorada de magia, prepare/commit/cancel e contexto de
   projectile. Somente contratos e characterization tests; nenhuma recompensa de capstone.
2. **19B — melee:** estado/save, Kanthor/Kaand, providers temporários, dedupe, heal/STA,
   knockback/Stun e PlayMode do perfect block que também quebra posture.
3. **19C — ranged:** encounter generalizado, primeiro alvo, snapshots de isolamento, crit no
   impacto e três luas com gates antirrecursão.
4. **19D — magic:** janela/arm/lockout, paridade entre SpellCastService e active magic skills,
   custo atômico, output/eco e cancelamento/reembolso.

Ownership inclui os arquivos originalmente listados e, após audit de reuse:

- `Assets/_Game/Scripts/Combat/DamageRequest.cs`, `DamageResult.cs`, `EnemyHealth.cs`
- `Assets/_Game/Scripts/Combat/Events/StatusAndDamageEvents.cs`
- `Assets/_Game/Scripts/Core/Events/CombatPostureEvents.cs`, `SurvivalEncounterEvents.cs`
- `Assets/_Game/Scripts/Combat/PlayerDamageReceiver.cs`, `EnemyPostureState.cs`
- `Assets/_Game/Scripts/Combat/KnockbackController.cs`, `StatusEffect/PlayerStatusReceiver.cs`
- `Assets/_Game/Scripts/Combat/Weapon/ProjectileSpawnRequest.cs`, `ProjectileSpawnService.cs`,
  `ProjectileBehaviour.cs`, `BowArrowAttackService.cs`
- `Assets/_Game/Scripts/Combat/Magic/SpellDataSO.cs`, `SpellCastService.cs` e executors magic
- `Assets/_Game/Scripts/Skills/SkillActionSO.cs`, `SkillEffectAggregator.cs`
- `Assets/_Game/Scripts/Skills/Runtime/CombatCapstone*.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/SurvivalSkillState.cs` e coordinator (reuse/generalização)
- `Assets/_Game/Scripts/Save/SaveData.cs`, `SaveManager.cs` e provider novo
- testes EditMode/save/PlayMode próprios de cada subfase

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [x] | T19A | Contratos de identidade, causalidade, projectile, controle e magia transacional | fundação |
| [x] | T19A-V | Compile + characterization EditMode/PlayMode `failed=0` | fundação |
| [x] | T19B | Kanthor/Kaand, estado/save e consumers | melee |
| [x] | T19B-V | EditMode/save + PlayMode de dedupe, refresh, hit e resistência | melee |
| [x] | T19C | Encounter geral, primeiro alvo, impacto/crit e três luas | ranged |
| [x] | T19C-V | PlayMode de reforço, morte, load, expiração e isolamento | ranged |
| [x] | T19D | Confluência em spell e active skill com transação única | magic |
| [x] | T19D-V | PlayMode de threshold, classe, MP insuficiente, interrupção e load | magic |
| [x] | T19-Z | Geração idempotente, regressão integrada, reauditoria e closeout | todos |

## Matriz mínima de testes

- PB que reflete e quebra posture ativa Kanthor uma vez; eventos distintos refrescam sem stack.
- Heal só após melee primário com dano; miss/DoT/secondary não consomem; HP cheio no R3 dá STA.
- Kaand exige break causado pelo player e expira em 8 s.
- Primeiro Marked Prey por encounter persiste morto/load/respec; reforço não troca alvo.
- Alihana cessa ao alvo morrer; Senya sem histórico não procede; secondary não recursa; Nyx
  alterna por isolamento e faz um único roll no impacto.
- Gasto abaixo/igual/acima do threshold, dois casts no mesmo frame e janela de 6 s.
- O cast que arma não consome; classe errada não consome; semente expira; lockout resta no load.
- Anya nunca cria mana/stamina líquida por RestoreMP; Senya falha atomicamente sem surcharge;
  interrupted/refunded e failed resolution não entram no ledger.
