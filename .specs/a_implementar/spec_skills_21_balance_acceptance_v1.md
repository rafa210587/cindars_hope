# Skills — fase mecânica 9: aceitação comparativa de equilíbrio

> **Spec ID:** spec_skills_21_balance_acceptance_v1  
> **Status:** APROVADA — autorização humana de 2026-09-10; matriz AC01 revisada com autorização humana de 2026-09-12
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Validation+Tuning / Skills+Combat+Economy / P0  
> **Parallelizable:** NO; **Depends on:** spec_skills_20_survival_crafting_capstones_v1  
> **Blocks:** spec de UI da árvore  
> **Validation:** EditMode determinístico + PlayMode gráfico na Cave/Farm + revisão de game design
> **Refinamento da matriz:** `docs/refinements/a_implementar/ref_skills_phase21_feasible_acceptance_matrix_v1.md`
> **Ordem de execucao:** 21A → 21B → 21C → 21D → 21E
> **Depende de:** `spec_skills_20_survival_crafting_capstones_v1`
> **Bloqueia:** spec de UI da árvore

required_adrs: [ADR-0010]
required_game_rules: [skill_tree_rules.md, quest_rules.md, combat_rules.md, economy_rules.md, cave_rules.md]

# /speckit.specify

## Spec

Aceitar ou ajustar o catálogo completo por comparação entre builds reais. Fórmulas unitárias são pré-condição;
equilíbrio só passa com telemetria reproduzível e inspeção em jogo.

- **AC01:** executar uma matriz de **ações pré-capstone compráveis em L20 R1/R3** e de **nove
  arquétipos completos nos estágios em que seus capstones são legais**, com equipamento coerente.
  L20 usa Corte Giratório (`skill_melee_whirl_cut`) para melee, Tiro Carregado
  (`skill_ranged_charged_shot`) como base ranged, Prisão de Gelo (`skill_magic_ice_bind`) para o
  caminho Anya e Chama Breve (`skill_magic_chama_breve`) para o caminho Senya. Alihana/Senya/Nyx
  podem ter escolhas secundárias pré-capstone, mas **não contam como três variantes lunares
  ativas** em L20; seus efeitos de capstone devem ser marcados indisponíveis nesse estágio.
  No baseline auditado, Tiro Carregado tinha `DamagePerRank=0` no asset e no disparo runtime:
  R1/R3 compráveis não apresentavam progressão de efeito. O amendment T21E abaixo autoriza
  provisoriamente `DamagePerRank=2` e sua aplicação única após a carga. A fixture deve medir
  o valor real de R1/R3 sem inventar dano; não aceitar o equilíbrio só pela diferença de rank.
  L50 testa capstone R1 de Kanthor/Kaand, Alihana/Senya/Nyx e Anya/Senya somente após os Atos
  1 e 2 concederem seus pontos canônicos. Capstone R3 entra em cenário adicional L54 ou posterior,
  com os mesmos Atos; o runner comprova primeiro que orçamento, pré-requisitos e rank são legais.
  O rank R1/R3 da ação L20 não deve ser confundido com rank de capstone. Telisandra e Thoren
  permanecem **bloqueados**, não aprovados, enquanto `survival.sinal_retirada` e
  `crafting.marca_eficiencia` estiverem `NotYetExecutable`; a aceitação global exige fechar
  essas duas builds em runs Farm/Town/Cave pertinentes.
- **AC02:** TTK permanece nas bandas canônicas do `TelemetryTargetEvaluator`: Common 3–6 s, Elite 12–20 s,
  Miniboss 45–90 s, Boss 120–240 s. Nenhuma build supera outra de mesmo estágio em single target por mais
  de 25% na mediana sem pagar risco/recurso equivalente.
- **AC03:** registrar dano causado/recebido, stamina/mana inicial-final, casts recusados, whiffs, acertos por
  alvo e uptime de controle. Common/Elite/Boss não podem permanecer sob controle forte por mais de
  65/40/20% da janela observada.
- **AC04:** cura/sustain não permite loop infinito sem provisões; uma run curta e uma longa preservam decisão
  entre consumo, retorno e risco. Load/reaggro/respec não rearmam usos únicos.
- **AC05:** Marca de Eficiência prova break-even por rank em 1/2/4/6 ações; Thoren e passivas econômicas
  permanecem abaixo de 20% da renda diária mediana e nenhum ciclo buy→respec→sell gera lucro.
- **AC06:** toda mudança de tuning volta ao `SkillActionSO`/nó/perfil autorado, regenera assets e repete a
  matriz afetada. O relatório separa PASS, ajuste aplicado e risco residual.
- **AC07:** uma sessão gráfica comprova origem no avatar, telegraph/tempo legível, reação do inimigo e ausência
  de travamento nas três cenas; captura sem graphics device não conta como validação visual.

# /speckit.plan

Reusar `CombatTelemetryService`, `CombatTelemetrySession`, `TelemetryTargetEvaluator` e os cenários de Cave
existentes. Criar somente um runner/projection de skills se a telemetria atual não carregar `actionId`, rank,
variant e fase. Fixtures usam seeds fixas; resultados vão para
`docs/validation/skills_sdd_v1/balance/PHASE_09_BALANCE_ACCEPTANCE_REPORT.md` e JSON/CSV bruto em `Logs/`.

O runner não deve atribuir pontos arbitrários nem declarar variante ativa por ID autorado. Para L50/L54,
usar os IDs de `QuestMainActsIds.Finales` e `QuestService.TryAwardActSkillPoint` como
**setup controlado de recompensa do estágio**, sem mutar a Fonte ou publicar um fim de Ato falso;
verificar que `RewardedMainActIds` foi salvo/restaurado sem duplicação,
e então comprar pela API de
`SkillTreeManager`. Registrar por linha pontos base, bônus de Atos, gastos por árvore, rank da ação,
rank/variante de capstone, item **equipado**, slot ativo e motivo de indisponibilidade. Em L20,
comparar só as ações realmente disponíveis; não inflar a amostra duplicando Tiro Carregado como
três variantes lunares. Essa fixture de conclusão testa orçamento e persistência da recompensa,
mas não comprova toda a cadeia de quests até os finais dos Atos; a aceitação da disponibilidade
real em L50 exige essa validação separada. Se o setup não conseguir registrar os dois Atos sem
duplicação, marcar a linha `NOT RUN`. Critérios de TTK e controle só passam após combate com
inimigos reais.

Não criar inimigos artificiais para substituir a matriz viva. Um fixture puro pode validar fórmulas, mas pelo
menos um Common, Elite e Boss reais devem participar da aceitação. Alterações acima de 10% em dano/custo/CD
exigem nova revisão de design; menores podem ser aplicadas e registradas no relatório.

### Plano técnico após audit de reuse

1. **21A — observabilidade:** estender `CombatTelemetrySession`, `CombatTelemetryReport` e
   `CombatTelemetryService` para guardar buildId, nível, rank, variant, actionId, resultado da ação,
   acertos por alvo e janelas de controle. Reusar `PlayerOffensiveActionCommittedEvent`,
   `PlayerActionFeedbackEvent` e os eventos de dano/status; só ampliar um evento quando os dados não
   existirem em nenhuma fonte atual. DTOs novos são aditivos e preservam leitura dos relatórios antigos.
2. **21B — matriz determinística:** criar um projection/runner de aceitação que seleciona assets reais
   das ações pré-capstone L20, dos sete capstones de combate R1 L50 com Atos 1+2 e R3 L54+,
   e dos alvos naturais da Cave. O probe de 2026-09-13 mostrou que os três IDs de Roster
   materializados em L20/L50/L54 retornam `Unknown` no `BestiaryTelemetryClassLookup`.
   Para aceitação por instância, resolver a classe do mesmo `EnemyDataSO` usado pela Cave,
   combinado ao `EliteAffix` do `CaveEnemySpawnPlanEntry` materializado; registrar
   `classSource=EnemyDataSO+SpawnPlan`, `EnemyInstanceId` e afixo. Usar Boss > Miniboss >
   Elite > Common, incluindo afixo nomeado. `entry.IsElite` é apenas diagnóstico: o
   `EnemySpawnResolver` atual o define em todos os membros de packs quando `AllowElite=true`,
   mas `EnemyBrain` não o usa para mecânica Elite. Um mesmo `EnemyId` pode ter instâncias
   Common e Elite na seed; escolher a instância pela classe efetiva. Asset/entry ausente ou classe divergente é
   `NOT RUN`. O lookup legado do bestiário continua nos relatórios existentes; nenhuma
   classe é preenchida manualmente pelo ID do manifesto. Materializar Telisandra/Thoren somente quando seus pré-requisitos
   forem executáveis; até lá registrar bloqueio explícito. Fixar seed, nível, pontos, progresso
   dos Atos e equipamento **aplicado**, produzindo uma linha por cenário. O runner calcula mediana,
   TTK, dano recebido, custo de recurso, recusas, whiffs, hits/target e uptime de controle, sem
   duplicar as fórmulas do gameplay.
   O PlayMode revelou que o fallback de `QuestRuntimeBootstrap.InitializeWhenReady` inicializava
   `QuestService` com adapter de progressão nulo, embora `PlayerProgressionManager.Instance`
   estivesse materializado; os Atos eram marcados recompensados sem conceder pontos. Corrigir o
   fallback para reter adapters disponíveis e fazer `QuestService.TryAwardActSkillPoint` falhar
   sem adapter, preservando a recompensa não consumida. Validar idempotência e save após o reparo.
3. **21C — survival/economia:** usar os estados reais de Telisandra/Thoren e pricing/crafting existentes
   para rodadas curta/longa e save/load/respec. A fixture de renda mediana deve derivar seus valores de
   atividades canônicas do estágio e registrar a composição; nenhum número de renda é inventado no teste.
4. **21D — runtime real:** executar Common, Elite e Boss reais e uma sessão gráfica Farm/Town/Cave. Os
   mesmos identificadores de build/cenário usados no runner devem aparecer nos JSON/CSV e no relatório.
   O TTK da matriz usa eventos de dano/kill filtrados por `EnemyInstanceId`, nunca o agregado
   apenas por tipo do `CombatTelemetrySession`; a classe da projection é verificada contra
   o plano de spawn e o asset real antes de chamar `TelemetryTargetEvaluator`. O primeiro corte
   comparável usa a mesma rotação real de skill e arma equipada para Corte Giratório R1/R3 contra
   as instâncias Common e Elite/Vampiric de `enemy_spore_imp` na seed L20. O cronômetro começa
   no primeiro dano do jogador naquela instância e termina apenas no `EnemyKilledEvent` da mesma
   instância. Registrar HP inicial cheio, golpes, dano externo, recurso, whiffs, recusas e motivo
   de timeout/perda do alvo; registrar também tempo de engajamento desde o primeiro comando,
   pois carga/windup precedem o primeiro dano e afetam comparações entre disciplinas. Morte
   artificial por `EnemyHealth.TakeDamage` e proxy de ataque
   desarmado não qualificam a amostra. Quatro kills dão uma comparação inicial, não a mediana
   AC02: repetir cada célula com seed/rotação equivalentes antes de avaliar bandas e delta de 25%.
   Os probes com perseguição por `Rigidbody2D.MovePosition` (r1/r3) e pelo `MoveInput` do
   `PlayerController` (r4) morreram antes do kill, com zero acertos da espada em r3/r4; não usar
   seus tempos como TTK. O r4 sofreu dano de outros inimigos, sobretudo arqueiros, portanto não
   representa combate isolado. Antes de ampliar a matriz, introduzir uma pequena porta de leitura
   das teclas já usadas por `PlayerController` e `PlayerAttackController`, com adapter padrão para
   `UnityEngine.Input` e um reader controlável apenas na fixture. O driver deve passar pelos
   `Update`/`FixedUpdate` reais, pelo guard de modal/stun, pela física, pelo equipamento e pela
   carga/windup da arma; atribuir `MoveInput` por reflection ou chamar um ataque proxy não serve.
   Confirmar em teste que o adapter padrão mantém as bordas `GetKeyDown`/`GetKeyUp` e que o reader
   controlado não ignora bloqueios de gameplay. Por tentativa de espada, registrar posição e
   orientação do jogador, distância/posição do alvo, estado da ação, candidatos do overlap e
   identificadores de qualquer dano recebido/de terceiros. Selecionar uma instância real com
   oportunidade de engajamento isolado; se outro inimigo interferir, a instância perder HP de
   fonte externa, a arma não conectar ou o jogador morrer, marcar a linha `INVALID` com causa,
   nunca converter tempo de timeout em TTK. Uma medição mecânica `target_locked` pode diagnosticar
   fórmulas, mas não é aceitação de combate real.
   O primeiro seed L20 não serve como célula comparativa: no r8 o Spore Imp Common da célula
   58,41 ficou atrás de `cave_elem_20_58_41_DecorBlocking`; a espada teve 12 commits/zero hits,
   embora o reader real tenha produzido movimento e E down/up. Não aumentar o alcance da arma
   para compensar esse bloqueio. O próximo seed fixo deve registrar a posição do mesmo tipo de
   alvo, a distância de outros inimigos e a ausência de prop sólido no corredor de aproximação
   antes de iniciar o cronômetro; falhar fechado quando o encontro não for navegável. Como
   `PlayerDamagedEvent.SourceId` identifica o tipo, não a instância, uma amostra com qualquer
   dano recebido fica `INVALID` até a origem individual ser comprovada.
   O seletor conservador da triagem r10 não aprovou um Spore Imp Common nas oito seeds fixas:
   rejeitou decor bloqueante próximo (±1 célula) e qualquer outro inimigo vivo a até 10 unidades
   do ponto inicial. Isso não prova que todos os encontros eram inalcançáveis. A seed r9 com ponto
   aberto provou quatro hits da espada, porém morreu com 85/101 de dano vindo de outros tipos.
   Suspender esse teste exploratório na suíte regular e não promover AC02. A próxima proposta
   para mediana precisa de um encontro natural isolável ou de uma política de limpeza prévia
   usando somente ações reais, com HP do alvo intacto e sem interferência durante a janela;
   definir e revisar essa política antes de adicionar mais seeds às cegas.
   Um probe puro do gerador de layout reproduziu a célula 58,41 da seed conhecida e encontrou
   pontos geometricamente afastados entre os primeiros 24 spawns na nova seed fixa
   `phase21_seed_l20_open_10`, sobretudo os índices 5/6. Ele **não** carregou todos os spawns,
   packs, afixos nem decor. O r11 testou essa seed na Cave real: o Common da célula 52,28
   tinha `DecorBlocking` junto ao spawn e os outros quatro candidatos tinham inimigos a
   2,28–5,12 unidades do ponto de ataque. Não há amostra TTK nessa seed. A alternativa
   de pré-limpeza foi revista por game design como **duelo condicionado diagnóstico**, não como
   única evidência de AC02. Em runs novas pareadas por seed/rank, terceiros só podem ser derrotados
   por movimento, input e ataques reais, sem teleporte, `Destroy`, despawn, save/reload, espera
   artificial de CD ou restauração de HP/STA/mana entre preparação e duelo. Registrar inclusive
   preparações que terminem em derrota, tempo, dano, kills e recursos gastos. Confirmar mesmo
   `EnemyInstanceId` natural com HP máximo/sem status anterior, classe/afixo, posição/aggro,
   recursos/CD e ausência de interferência antes da janela; qualquer dano externo invalida.
   A fixture r11 ainda reposiciona o jogador e recompõe STA, portanto não implementa essa
   política. Para aceitar AC02, comparar também encontros naturais completos em seeds pareadas,
   com risco/custo do pack inteiro e TTK por instância; corrigir a ambiguidade de
   `PlayerDamagedEvent.SourceId` (tipo, não instância) ou manter amostras com dano recebido como
   inválidas. Corrigir overlap de decor em novos runs é candidato separado, preservando snapshots.
5. **21E — tuning:** qualquer ajuste fica no `SkillActionSO`, nó ou perfil autorado. Regerar assets e
   repetir somente a matriz afetada; delta acima de 10% volta para revisão de game design.
   Exceção de correção de execução: Tiro Carregado ignorava rank no caminho de carga
   (`ActiveSkillExecutionController` zerava `DamagePerRank` no clone). A revisão independente de
   game design de 2026-09-13 propôs `DamagePerRank=2`, coerente com Corte Giratório/Tiro Triplo:
   a carga continua interpolando o dano-base 8→20; `ResolveRank` adiciona +2 por rank acima de R1
   uma única vez após a carga. Assim R1=8→20, R2=10→22, R3=12→24. Tempo 0,20–1,10 s, alcance
   5→9, postura, custo 12→26 STA e CD 6 s não mudam. O runtime preserva `DamagePerRank` do asset
   no clone, sem rank no perfil de carga, e a geração do asset deve ser restrita à ação afetada.
   Esse envelope é **tuning provisório**, não aceite de AC02: R3 ganha 20% no disparo máximo,
   mas 50% no mínimo. Provar casts reais por rank, custo/cancelamento, asset intacto e comparação
   contra outras ações ranged/TTK antes de promover equilíbrio; o delta acima de 10% recebeu
   revisão de design, mas continua sem mediana de combate real.

Ownership serial padrão, porque sessão, serviço e relatório formam um contrato compartilhado:

- MODIFY `Assets/_Game/Scripts/Combat/Telemetry/CombatTelemetrySession.cs`
- MODIFY `Assets/_Game/Scripts/Combat/Telemetry/CombatTelemetryReport.cs`
- MODIFY `Assets/_Game/Scripts/Combat/Telemetry/CombatTelemetryService.cs`
- MODIFY `Assets/_Game/Scripts/Combat/Telemetry/TelemetryTargetEvaluator.cs` somente se faltar uma
  regra de aceitação; não duplicar as bandas existentes
- MODIFY eventos existentes somente após provar ausência do dado na superfície atual
- MODIFY `Assets/_Game/Tests/EditMode/Combat/CombatTelemetryTests.cs` e
  `CombatTelemetryPhaseOneTests.cs`
- MODIFY `Assets/_Game/Scripts/Quests/Runtime/QuestRuntimeBootstrap.cs` somente no fallback de
  adapters após timeout, para preservar progressão quando disponível
- MODIFY `Assets/_Game/Scripts/Quests/Runtime/QuestService.cs` em `TryAwardActSkillPoint`,
  para não consumir um Ato se o adapter de progressão for nulo
- MODIFY `Assets/_Game/Tests/EditMode/Quests/QuestSourcesTests.cs` com teste de recompensa
  não consumida quando o adapter está ausente
- CREATE uma fixture PlayMode de aceitação da fase 21 e seu gerador de JSON/CSV em `Logs/`
- CREATE porta/adapter de polling de input no assembly Runtime, apenas para os comandos necessários
  ao driver de T04; MODIFY `Assets/_Game/Scripts/Player/PlayerController.cs` e
  `Assets/_Game/Scripts/Combat/PlayerAttackController.cs` somente para trocar a fonte dessas
  leituras, preservando o processamento e os guards existentes; adicionar teste de regressão das
  bordas e bloqueios. Confirmar os caminhos reais antes de editar.

Falha fechada: cenário sem build, nível, target class, asset real ou campo obrigatório é `NOT RUN` para
aquela linha e impede aceitação global; não preencher zero como se fosse observação válida.

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [ ] | T01 | 21A: adicionar build/action/rank/variant, outcome, hits/target e uptime de controle à telemetria; compatibilidade dos relatórios antigos | AC01–AC03 |
| [ ] | T02 | 21B: corrigir fallback de adapter de quest e recompensa consumida sem ponto; corrigir manifesto/runner para L20 pré-capstone, L50 capstone R1 com Atos 1+2 e L54+ R3; testar orçamento, compra, item equipado, slot e indisponibilidade sem variantes falsas; EditMode de envelopes, DR e medianas | AC01–AC03 |
| [ ] | T03 | 21C: fixture canônica de renda, break-even, sustain e save/load/respec, incluindo `<20%` por estágio | AC04–AC05 |
| [ ] | T04 | 21D: PlayMode Common/Elite/Boss reais com L20 pré-capstone e capstones legais L50/L54+, JSON/CSV bruto e marcação NOT RUN para Elite/Boss sem encontro canônico | AC01–AC03, AC06 |
| [ ] | T05 | 21D: após o reparo de catálogo que liberou os pré-requisitos de Telisandra/Thoren, validar seus efeitos em PlayMode survival/crafting curta/longa + load/respec, sem atribuir ação ofensiva fictícia | AC01, AC04–AC05 |
| [ ] | T06 | 21D: sessão gráfica Farm/Town/Cave e capturas com telegraph/reação/travamento | AC07 |
| [ ] | T07 | 21E: aplicar tuning necessário, regerar/repetir matriz e obter revisão independente | AC01–AC07 |
