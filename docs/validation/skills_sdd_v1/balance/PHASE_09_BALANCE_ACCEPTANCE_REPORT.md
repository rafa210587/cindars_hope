# Fase 21 — aceitação comparativa de equilíbrio das skills

**Spec:** `spec_skills_21_balance_acceptance_v1`  
**Data da evidência:** 2026-09-13
**Revisão da matriz:** AC01/plan/tasks atualizados em 2026-09-12 após aprovação humana;
o PlayMode de compra r6 valida orçamento, compra e equipamento da matriz revista.

**Resultado global:** **NÃO ACEITO — falha histórica AC05 corrigida no recorte de venda direta; reteste global e demais critérios pendentes**

Em 2026-09-13, a implementação 21F ativou save v6 com lotes de procedência. A suíte
EditMode focal T1–T6 passou **200/200** (`Logs/skills21f-t1-t6-editmode-r10.xml/.log`,
scan PASS, exit 0), inclusive morte após gastar a última flecha e com o arco inicial. PlayMode de
arbitragem Farm e painel de venda materializado passaram **1/1** cada
(`Logs/skills21f-existing-arbitrage-playmode-r2.xml` e
`Logs/skills21f-sell-panel-playmode-r2.xml`). O novo PlayMode de compra,
SellPoint, envio, save/load e pagamento único passou 1/1
(`Logs/skills21f-economy-playmode-r5.xml`); a cadeia registrada de madeira e a
forja com job de 4s passaram 1/1 (`Logs/skills21f-craft-chains-playmode-r3.xml`).
Cinco janelas agrícolas pós-21F
L20/L50/L54 passaram 1/1 cada, com scanner PASS: mediana composta D3–D5
90g base, 90g R1 e 91g R3; nesta rotina, R3 acrescentou 1,11%. Os XMLs novos
estão em `Logs/skills21f-income-*-r1.xml`. A compra em Farm é uma sessão
controlada; Town→Farm natural, cadeias completas e renda global `<20%` seguem
sem demonstração. A [matriz 21F](PHASE_21F_T7_INCOME_AND_TRANSFORMATION_MATRIX.md)
separa os recortes aprovados das lacunas.

Em 2026-09-13, o PlayMode da Farm confirmou a arbitragem entre a compra de pão do Renko com
`crafting_shop_sense` R3 (17g) e o `SellPoint` materializado após respec do estado isolado (20g). O teste de contrato falha
por **+3g em uma unidade**. A [proposta de reconciliação de canais](../../../refinements/a_implementar/ref_skills_phase21_cross_channel_arbitrage_v1.md)
delimita o problema. Em 2026-09-13, o humano escolheu procedência dos itens comprados por
lote; o draft 21E de teto universal por SKU ficou superado e a spec 21F assume o desenho.
T1–T6 introduziram lotes no inventário, compra com procedência, revenda limitada,
transporte, transformação, save v6 e projeção da venda. O diagnóstico histórico de
+3g foi superado para o pão comprado no SellPoint controlado, mas a rota natural
Town→Farm e a renda global ainda não foram retestadas. A comparação agrícola
acima não substitui a cesta integral de fontes e gastos.

O draft 21F recebeu revisão independente dos contratos de compra/venda atômicas,
save v5/v6, corrupção por domínio, encomenda ativa de Yael e custo zero. Os P1/P2
apontados nessa revisão foram corrigidos na spec; a regra econômica registra a
decisão como futura, sem afirmar que o jogo já a aplica. A enumeração dos callers,
owners e testes foi fechada; o Depth Gate independente passou estaticamente e a
spec foi promovida a `.specs/a_implementar/`. O checkpoint inicial EditMode T1–T4
foi 98/98 (`Logs/skills21f-t3-t4-editmode-r1.xml/.log`); a evidência vigente é
200/200 em r9 e os PlayModes listados acima. O gate documental de qualidade foi
`SCOPED_PASS`; nenhum desses recortes comprova a renda global ou encerra AC05.

Auditoria estática posterior encontrou outro bloqueio de AC01/AC05: o catálogo liga
`crafting_shop_sense` a `crafting_pack_order`, mas este último está deliberadamente
`NotYetExecutable` pela spec 18, e a compra de pré-requisito dormente é rejeitada.
Logo o desconto R3 do diagnóstico inicial foi injetado em estado isolado e, naquela
rodada, **não** se comprovou que o jogador conseguia comprar Senso de Mercado.
O reparo e a compra legal posterior estão registrados abaixo; a rota Town→Farm
com a build legal ainda precisa ser repetida.
O grafo completo contém outras duas flags obsoletas: `survival.isca_improvisada` e
`crafting.irrigador_portatil` estão dormentes apesar de specs 15/16 e executores
de ação funcionais. Isca bloqueia também `survival.instinto_sobrevivencia`.
Antes do reparo, dos 66 nós canônicos três estavam dormentes e dois descendentes
funcionais inacessíveis. O gerador estreito `GenerateRepairedSkillNodesOnly` rodou no
Unity 6000.5.7f1 (`Logs/skills-phase21-graph-assets-20260913.log`, saída normal) e
alterou somente os assets de Isca, Irrigador e Senso: as duas flags passaram a 0,
Senso passou a requerer Olho de Material, e Mochila continuou dormente.
Comando: Unity `-batchmode -nographics -quit -executeMethod CindarsHope.Editor.Skills.GenerateCanonicalSkillCatalog.GenerateRepairedSkillNodesOnly`; processo encerrou com `Exiting batchmode successfully now!`, sem `error CS` no log, e 3/3 assets esperados foram encontrados.
Compra seguida de execução da ação na mesma jornada ainda está **NOT RUN**.

O PlayMode `[Explicit]` `Phase21SkillGraphReachabilityPlayModeTests` passou **1/1**
(`Logs/skills-phase21-graph-playmode-20260913.xml/.log`, scan PASS): Isca L20,
Instinto/Irrigador L50 e Senso foco compra R1 em L50/R3 em L54 foram comprados
legalmente, equipados quando ação, salvos/restaurados; Mochila foi recusada.
Esse teste não usa as ações nem percorre UI/Town. A revisão independente deu
**WARNING sem P1/P2**: o XML/log registrou um aviso transitório
`ActiveSkillSlotsHudView Guard violation: FORBIDDEN_SKILL_ID` ao restaurar
estado entre estágios; o PlayMode verde não atesta HUD estável nessa transição.
A janela de renda alta passou
**4/4 em processos separados** (`Logs/skills-phase21-high-income-l50-base-results.xml`,
`...l50-r1-results.xml`, `...l54-base-results.xml`, `...l54-r3-results.xml`,
todos com scan PASS). Três canteiros escalonados D0–D5, venda no SellPoint da
Farm e compra de sementes no Renko em sessão inicializada na Farm produziram:

| Janela controlada D3–D5 | Pontos | Semente | Mediana líquida transacional | Metas diárias | Mediana composta |
|---|---:|---:|---:|---:|---:|
| L50 base | 0 | 5g | 55g/dia | 35g/dia | 90g/dia |
| L50 Senso R1 + Thoren R1 | 27/27 | 5g | 55g/dia | 35g/dia | 90g/dia |
| L54 base | 0 | 5g | 55g/dia | 35g/dia | 90g/dia |
| L54 Senso R3 + Thoren R3 | 29/29 | 4g | 56g/dia | 35g/dia | 91g/dia |

O incremento nessa rotina é 0% em L50 e 1/90 = 1,11% em L54. `Thoren` não foi
acionado nessa janela e o teste usa pontes internas de FarmPlot, sem jornada natural
Town/UI ou renda total de crafting/combat/quest/rollover. O teto global `<20%`
segue **NOT RUN**. JSON/CSV por cenário em `Logs/skills-phase21-high-income-l*-three-plots.*`;
14 hashes dos inputs foram registrados **após** os testes em
`Logs/skills-phase21-graph-income-inputs-20260913.csv`, não no instante da execução.

Manifesto SHA-256 da rodada de Tiro Carregado: `Logs/skills-phase21-charged-hashes-20260913.csv`
(9/9 inputs presentes: runtime, catálogo, gerador, asset e testes). Os manifestos anteriores de
59 e 51 inputs permanecem respectivamente em `Logs/skills-phase21-input-hashes-20260913.csv` e
`Logs/skills-phase21-input-hashes-20260912.csv` como snapshots das rodadas anteriores; seus hashes
não identificam o ajuste de Tiro Carregado. Os caminhos em `Logs/` são evidência local,
fora do save do jogador.

O catálogo ainda não tem medição comparativa de builds reais. As verificações abaixo comprovam
contratos locais, resolução de assets e um fluxo de ação em PlayMode; nenhuma delas demonstra as
medianas de TTK, o teto de controle ou a sessão gráfica exigidos pela spec. A fase 22 (Canvas da
árvore) continua bloqueada pelo gate de equilíbrio.

## Acceptance criteria extracted

A spec revista exige ações pré-capstone R1/R3 em L20, capstones R1 em L50 após Atos 1+2 e R3
em L54+; também exige TTK/comparação de medianas, observabilidade de combate, sustain/economia,
tuning rastreável e validação gráfica Farm/Town/Cave. O estado de cada critério está abaixo.

## Validation

| Gate | Resultado | Evidência |
|---|---|---|
| Unity compile + EditMode focado | **PASS, 98/98** | `Logs/skills-phase21-final-editmode-r6-20260912.xml` e `.log`; filtro `CanonicalCatalogTests;EfficiencyMarkSkillTests;Phase21;QuestSourcesTests;CombatTelemetry`; Unity 6000.5.7f1; exit 0. O gerador Editor concluiu 66 nós/31 ações em `Logs/skills-support-catalog-regenerate-r2-20260912.log`; apenas os assets dos dois nós mudaram. |
| Regressão dos Atos/quest sources | **PASS, 43/43** | `Logs/skills-phase21-quest-regression-20260912.xml` e `.log`; filtro `MainActsTests;QuestSourcesTests`; exit 0 após o reparo do adapter. |
| PlayMode da ação Fagulha | **PASS, 1/1** | `Logs/skills-phase21-outcome-playmode-r2-20260912.xml` e `.log`; FarmScene, mana insuficiente → `Refused`, execução → um `Committed`, recarga → `Refused`; `-nographics`; exit 0 |
| PlayMode de manifesto, alvos e Marca | **PASS, 3/3 como probe/ação; TTK NÃO EXECUTADO** | `Logs/skills-phase21-support-target-playmode-r4-20260912.xml` e `.log`; CSV/JSON em `Logs/phase21_combat_probe/phase21_combat_target_probe_20260913_010342.*`. Cinco combinações de alvo/estágio materializadas por ID de instância, mas todas `NOT RUN` para TTK: classe `Unknown` no lookup do bestiário. Marca R1 recusou sem estação e aplicou redução de STA numa estação real da Farm. |
| PlayMode da classe por instância no roster | **PASS, 1/1 como diagnóstico; classe/TTK ainda não aceitos** | `Logs/skills-phase21-class-probe-r7-20260912.xml` e `.log`; CSV/JSON em `Logs/phase21_combat_probe/phase21_combat_target_probe_20260913_012758.*`. O plano marcou as 62 entradas `IsElite`, inclusive inimigos comuns sem afixo. A auditoria do runtime mostrou que esse flag afeta o tint e o snapshot, mas não a mecânica Elite. A projeção r7, que usava `entry.IsElite` como classe, foi invalidada e substituída pelo r8. |
| PlayMode da classe efetiva por instância | **PASS, 1/1 como probe; TTK NÃO EXECUTADO** | `Logs/skills-phase21-class-probe-r8-20260912.xml` e `.log`; CSV/JSON em `Logs/phase21_combat_probe/phase21_combat_target_probe_20260913_013649.*`. `EnemyDataSO+EliteAffix` encontrou 52 Common/10 Elite entre 62 entradas do plano; seis linhas naturais do manifesto (Common/Elite em L20/L50/L54) ficaram `CLASS_READY` com `EnemyInstanceId` ativo. Miniboss/Boss seguem `NOT RUN`; 1/1 é sucesso do probe, não de AC02. |
| PlayMode de impacto real de Corte Giratório | **PASS, 1/1; TTK NÃO EXECUTADO** | `Logs/skills-phase21-common-whirl-hit-r3-20260912.xml` e `.log` registraram o primeiro acerto. O teste foi reforçado no integrado r2 abaixo: compra R1 pela API, espada de ferro equipada, Spore Imp Common natural L20 confirmado pelo asset e pelo `EnemyBrain` ativo, um commit e um `DamageAppliedEvent` com `ActionToken=skill_melee_whirl_cut:1`/`EnemyInstanceId` correto. O JSON final `Logs/phase21_combat_probe/phase21_common_whirl_hit_20260913_014846_393.json` e `.csv` confirma 262→252 HP, 10 de dano da skill e zero dano externo. Primeiras duas tentativas falharam por reposicionamento da fixture antes do passo de física; r3 aplicou a posição após o passo. |
| PlayMode integrado compra + impacto | **PASS, 2/2; TTK NÃO EXECUTADO** | `Logs/skills-phase21-purchase-hit-integrated-r2-20260912.xml` e `.log`; executou o teste de 8/8 ações L20 + 18/18 capstones + 22/22 armas e o impacto isolado de Corte Giratório na mesma classe de teste, exigindo perda de HP igual ao dano atribuído à skill e nenhum dano externo. |
| Probe exploratório de combate completo L20 Common | **INVALID, sem kill/TTK** | `Logs/skills-phase21-ttk-common-r1-20260912.xml` e `.log`: 2 hits da skill + 2 da espada, alvo 262→206 HP, jogador 100→0 em 25,36 s; a perseguição da fixture não acompanhou o alvo de forma confiável. O r2 ficou sem XML após pausa na morte e foi encerrado; r3 em `Logs/skills-phase21-ttk-common-r3-20260912.xml`/`.log` encerrou com alvo 262→242, jogador 100→0, 2 hits da skill e 0 da espada. `DeathSystemBootstrap` registrou erro porque a cena vazia do Test Runner esgotou seu bind antes da Farm. Após corrigir o setup, r4 (`Logs/skills-phase21-ttk-common-r4-20260912.xml`/`.log`; `Logs/phase21_combat_probe/phase21_common_whirl_r1_ttk_20260913_022049_438.json`/`.csv`) fez o movimento pelo `PlayerController`, mas terminou com jogador 100→0 em 30,82 s, alvo 262→242, 2 hits da skill, 0 da espada, 12 whiffs e nenhum kill. A revisão dos eventos atribuiu 68/112 de dano recebido a arqueiros externos ao alvo; a luta não era isolada. O sistema de morte inicializou/processou o evento; o driver roteirizado segue inadequado para AC02. |
| Regressão da fixture de compra/impacto após o probe | **2 PASS, 1 SKIPPED; 0 FAIL** | `Logs/skills-phase21-purchase-final-r1-20260912.xml` e `.log` são o run final após isolamento de HP/STA/mana/caches, espera em tempo real e retorno do probe TTK a `Ignore`. Carregar Farm de forma síncrona no setup fez `DeathSystemBootstrap` inicializar antes da Cave, sem timeout da cena vazia. O teste de TTK permanece exploratório até existir driver de movimento/navegação fiel; compra e hit real seguem verdes. Não contar o skip como cobertura TTK. |
| Porta de input de movimento/arma | **PASS, 2/2 PlayMode; AC02 ainda pendente** | `Logs/skills-phase21-input-reader-r2-20260913.xml`/`.log`, Unity 6000.5.7f1, exit 0. A fixture usa o player real da Farm; `PlayerController.Update` lê direção e mantém `MovementLocked`, `PlayerAttackController.Update` reconhece Q down/up e bloqueia novo windup com modal. O adapter padrão delega ao Legacy Input. O primeiro r1 falhou por `PlayerBlockController` sem StaminaManager em player artificial na cena vazia; a correção usa a composição real. Não há teste isolado de stun nem bordas reais do teclado do adapter padrão. |
| Probe TTK com input jogável, r5–r8 | **INVALID, sem kill/TTK** | `Logs/skills-phase21-ttk-common-r5-20260913.xml` a `r8` e JSON/CSV homônimos em `Logs/phase21_combat_probe/`: os comandos E down/up passaram por `PlayerAttackController.Update` e houve 11–12 commits da espada, mas zero hits; jogador morreu, alvo 262→242 HP por duas skills. No r5, 104/114 de dano recebido vieram de outros tipos de inimigo. No r8, o jogador continuava com `MoveInput=(1,0)` e o alvo estava a ~1,82 unidade; o collider principal estava a ~1,15 do centro da espada de raio 1,0. A consulta sem máscara enxergava o trigger do alvo, a máscara `Enemy` não encontrava collider no alcance; raycast físico encontrou `cave_elem_20_58_41_DecorBlocking` entre jogador e alvo, na célula do próprio spawn. Assim, a instância/seed não é um encontro isolado e navegável para esta rotação. Nenhum valor de alcance/dano foi alterado. |
| Triagem de seeds para TTK, r9–r10 | **INVALID/NOT RUN, sem kill/TTK** | `Logs/skills-phase21-ttk-common-r9-20260913.xml`/`.log` selecionou `phase21_seed_l20_open_01`, rejeitando antes o Common preso no decor da seed original. No alvo aberto, a mesma espada teve **4 hits** e a skill 1 hit; HP 262→154, mas o jogador morreu em 5,4 s com 85/101 de dano recebido de outros tipos de inimigo. Isso confirma que E down/up, resolução da arma e colisão funcionam quando o alvo é alcançável, sem medir equilíbrio. A triagem r10 (`Logs/skills-phase21-ttk-common-r10-20260913.xml`/`.log`, JSON `Logs/phase21_combat_probe/phase21_common_whirl_r1_ttk_20260913_032529_080.json`) exigiu nenhum terceiro inimigo a ≤10 unidades e falhou fechada: nas oito seeds fixas, o seletor conservador não aprovou um Common com corredor inicial sem decor bloqueante próximo (±1 célula) e sem outro inimigo a até 10 unidades. Isso não prova que todos os encontros sejam inalcançáveis. A fixture exploratória voltou a `Ignore` após o diagnóstico; não contam os tempos de derrota como TTK. |
| Probe de layout antes do r11 | **HIPÓTESE geométrica, não aceitação** | Probe .NET temporário, sem alterar o repositório, reproduziu a posição conhecida da seed original e avaliou 34 layouts em ~2,6 s. `phase21_seed_l20_open_10` tem spawns índices 5/6 em (10,21)/(10,59), a pelo menos 13,6/22,2 unidades dos demais **24 primeiros** spawns. O probe não resolveu packs, afixos, decor ou colisores; não garante Spore Imp Common nem isolamento completo. A Cave real foi testada no r11 abaixo; esta triagem isolada não classificava inimigos nem decor. |
| Cave real na seed candidata, r11 | **NOT RUN para TTK; seleção rejeitada** | `Logs/skills-phase21-ttk-common-r11-20260913.xml`/`.log` e JSON `Logs/phase21_combat_probe/phase21_common_whirl_r1_ttk_20260913_035258_638.json`: o teste exploratório executou 1 caso e falhou fechado antes de medir combate. Em `phase21_seed_l20_open_10`, um Common Spore Imp ficou junto a `cave_elem_20_52_28_DecorBlocking`; outros quatro candidatos tinham inimigo a 2,28–5,12 unidades do ponto de ataque. O layout puro não predizia packs/decor. A fixture voltou a `Ignore`; os nove seeds testados não são prova de inexistência de encontro válido. Próxima proposta é pré-limpeza por ataques reais com HP do alvo intacto, a revisar antes de implementar. |
| Revisão de design da pré-limpeza | **SOUND_WITH_AMENDMENTS; AC02 ainda pendente** | Duelo após limpar terceiros por combate real pode diagnosticar fórmulas, mas muda risco e custo do encontro natural; não é evidência única de equilíbrio. Runs por rank devem partir de seed nova pareada, registrar tentativas derrotadas e custo completo da preparação, sem teleporte/reset de recursos/CD, e só medir alvo natural de HP cheio sem interferência. A fixture atual reposiciona o jogador/recompõe STA; `PlayerDamagedEvent.SourceId` fornece tipo, não instância. Aceite AC02 também requer encontro completo, classe Common/Elite/Boss e comparação de risco do pack. |
| Regressão final de input, compra e hit após triagem | **PASS, 4/4 executados; 1 SKIPPED; 0 FAIL** | `Logs/skills-phase21-input-purchase-final-r1-20260913.xml`/`.log`, Unity 6000.5.7f1, exit 0. Duas bordas de input/movimento e os testes de compra matriz + impacto isolado passaram juntos. O único skip é o TTK exploratório; não conta como cobertura AC02. |
| Geração restrita do Tiro Carregado | **PASS no Editor** | `Logs/skills-phase21-charged-action-generate-r3-20260913.log`, Unity 6000.5.7f1, exit 0; `GenerateChargedShotActionOnly` atualizou só `SkillAction_skill_ranged_charged_shot.asset` de `DamagePerRank=0` para 2. r1 foi impedido por CS0165 temporário no loader Town em edição concorrente; r2 expôs referência inválida do gerador ao assembly Gameplay, corrigida antes do r3. O catálogo já contém o mesmo valor 2. |
| Disparo Carregado R1/R2/R3, carga mínima/máxima | **PASS, 2/2 PlayMode focado** | `Logs/skills-phase21-charged-playmode-r2-20260913.xml`/`.log`, Unity 6000.5.7f1, exit 0, log scan PASS; nove inputs em `Logs/skills-phase21-charged-hashes-20260913.csv`. O teste existente confirma cancelamento precoce/dano/stagger sem custo/CD e disparo cheio com 26 STA; o novo compra ranks por API em L20, mede projétil real 8/10/12 no tap e 20/22/24 na carga máxima, custo 12/26 STA, um commit/rank, sem mutar o asset. r1 falhou corretamente porque a fixture restaurava R3 sem os 5 pontos gastos exigidos para cap dinâmico; r2 compra pré-requisitos e ranks legalmente. Não é acerto em inimigo nem TTK comparativo. |
| EditMode focado de Disparo Carregado | **PASS, 7/7** | `Logs/skills-phase21-charged-editmode-r2-20260913.xml`/`.log`; `RunUnityEditModeTests.ps1 -TestFilter CindarsHope.Tests.EditMode.Skills.SkillRangedIdentityTests`, Unity 6000.5.7f1, exit 0, log scan PASS. r1 retornou `NOT RUN`/exit 2 enquanto uma instância gráfica do Editor mantinha o lock; após sua saída, r2 executou. `dotnet build CindarsHope.Tests.EditMode.csproj -v:q -clp:ErrorsOnly` também passou com 0 erros/42 avisos. |
| Regressão PlayMode após isolamento das fixtures | **PASS, 4/4** | `Logs/skills-phase21-charged-input-final-r2-20260913.xml`/`.log`, Unity 6000.5.7f1, exit 0 e log scan PASS: 2 testes Charged Shot + 2 de input, com skills/progressão/STA restaurados após a fixture. O r1 anterior ficou sem XML por CS1061 em `SceneFadeOverlayBootstrap.cs` durante integração Town; após freeze e compilação Town verde, r2 executou sem falhas. |
| Regressão final de compra/impacto e estado do TTK | **PASS, 2/2 executados; TTK SKIPPED** | `Logs/skills-phase21-purchase-hit-final-r4-20260913.xml`/`.log`, Unity 6000.5.7f1, exit 0, log scan PASS; oito inputs em `Logs/skills-phase21-purchase-r4-hashes-20260913.csv`. Compra legal e acerto real de Corte Giratório passaram. A execução completa da classe em `Logs/skills-phase21-purchase-final-r3-20260913.xml` teve 2 PASS, 1 SKIP, 0 FAIL; o scanner classifica o agregado `Skipped:Ignored` como evidência inválida para PASS, por isso o r4 filtrou só os dois casos executáveis. O TTK exploratório está explicitamente suspenso após r11, sem evidência de mediana. |
| PlayMode compra real de builds + survival | **PASS, 6/6 testes; 8/8 L20 + 18/18 capstones + 22/22 armas; suporte 4/4** | `Logs/skills-phase21-support-survival-playmode-r2-20260912.xml` e `.log`; Unity exit 0, orçamento e recompensas de Atos pela API, compra/rank/slot/save dos nove perfis e regressão de Sinal/Último Fôlego/Campo. Não é TTK nem quests completas. |
| EditMode de economia/sustain | **PASS, 10/10** | `Logs/skills-phase21-economy-editmode-r2-20260913.xml`/`.log`; Unity 6000.5.7f1, filtro `Phase21EconomySustainTests`, exit 0, log scan PASS. Verifica break-even R1/R3 em 1/2/4/6 gastos e flags de uso único após save/load/respec. A primeira rodada falhou 10/12 porque dois testes de transação em EditMode não tinham o bootstrap exigido por `SellableItemPolicy`; foram removidos, pois o ciclo real pertence ao PlayMode. Não é renda observada nem run longa. |
| PlayMode de suporte R3 e Sinal | **PASS, 3/3 como execução diagnóstica** | `Logs/skills-phase21-support-playmode-r2-20260913.xml`/`.log`; Unity 6000.5.7f1, filtro `Phase21SupportActionExecutionPlayModeTests`, exit 0, log scan PASS. Marca R1 recusou sem estação e aplicou efeito numa estação Farm; R3 foi comprado/equipado por slot, persistiu no DTO, com seis gastos reais de `StaminaManager` e break-even após quatro ações-base. Quatro gastos adicionais devolvem ao coordenador persistente o crédito fracionário anterior da fixture. Sinal de Retirada foi comprado/equipado em L20, ativado por slot numa Cave materializada e aplicou multiplicadores direcionais 0,70/1,12. O teste publicou aggro controlado usando ID de inimigo natural; não é run espontânea de sustain ou validação gráfica. |
| PlayMode existente dos capstones Telisandra/Thoren | **PASS, 9/9 como contratos de execução** | `Logs/skills-phase21-support-capstones-playmode-20260913.xml`/`.log`; filtros `LivingForgeCapstonePlayModeTests;CavebornCapstonePlayModeTests`, Unity 6000.5.7f1, exit 0, log scan PASS; inputs em `Logs/skills-phase21-support-capstones-hashes-20260913.csv` (6/6). Verifica heal/buff/uso único do Caveborn e qualidade, potência, reserva, cancelamento e rollover da Forja Viva. Essas fixtures usam rank/objetos controlados, sem unir compra legal, Farm/Town/Cave e run longa numa mesma amostra. |
| PlayMode integrado de Thoren na forja Farm, rodada final | **PASS, 1/1 L50 R1 + 1/1 L54 R3; diagnóstico T05** | `Logs/skills-phase21-thoren-craft-l50-r1-r7-results.xml` e `Logs/skills-phase21-thoren-craft-l54-r3-r3-results.xml`, processos batch separados Unity 6000.5.7f1, scans de log PASS; JSON/CSV `Logs/skills-phase21-thoren-craft-l50-r1-quality.*` e `...l54-r3-save-common.*`; manifestos `Logs/skills-phase21-thoren-craft-input-sha-{before-final,after-final}.csv` iguais em 19/19 insumos. Compra legal de 27/29 pontos incluindo Atos 1+2, forja materializada `farm_forge_01`, receita real de espada de ferro. R1 consumiu 2 minérios e gerou uma variante q1; preço transacional e delta da carteira no Brumdar são 30g para base e q1, sem prêmio de venda ou meta de colheita por espada. R3 poupou 1 dos 2 minérios; o DTO de carga serializado e restaurado em uma instância nova manteve o dia consumido, sem round-trip de SaveManager. A fixture não restaura carga/metas/sessão ShopManager e exige o processo batch dedicado usado. Receita custa 0 STA, portanto não exercita Marca; não há janela de renda L50/L54, Town/UI natural ou run longa. |
| Bugfix da meta `daily_goal_sell_first_crop` em venda NPC | **PASS, EditMode 22/22 + integração L50 pós-fix** | `Logs/farm_daily_goal_shopsell_editmode_20260913.xml/.log` e `Logs/farm_daily_goal_shopsell_build_20260913.log`: build C# de 7 projetos sem erro; scan Unity PASS. `ShopSell` só avança a meta quando o `ItemId` registrado é `ItemCategory.Crop`; L50 r7 vendeu espada base e q1 por 30g cada, sem os 20g indevidos. `SellPoint`, `SellAll` e `shipping` ainda publicam transações agregadas sem IDs por item e podem concluir a meta sem colheita. É gap residual de contrato de evento, não AC05 resolvido. |
| PlayMode de arbitragem entre canais | **FAIL, 0/1 — defeito AC05 confirmado** | `Logs/skills-phase21-economy-explicit-r2-20260913.xml`/`.log`; Unity 6000.5.7f1, filtro pelo nome completo do teste `[Explicit]`, exit 2. Compra real via `ShopManager` de pão do asset `Shop_General_Store` a 17g com Senso R3; após `FullRespec` do estado isolado de preço, `SellPoint.Interact` instanciado na FarmScene paga 20g. Assert `finalGold<=buyPrice` falha; o componente de venda é runtime real com managers isolados, não o objeto materializado da cena. `[Explicit]` deixa o diagnóstico fora da execução sem filtro; filtros que selecionem a classe ou o método o executam e falham (repetido com filtro de classe em `Logs/skills-phase21-economy-default-filter-20260913.xml`). A fixture exige `ShopManager` inicialmente sem sessões e restaura seu estado vazio; não publica respec global de uma árvore sintética. O scanner rejeita corretamente a execução falha. |
| PlayMode refinado com assets materializados | **FAIL, 0/1 — defeito AC05 reproduzido** | `Logs/skills-phase21-renko-materialized-economy-playmode-20260913.xml`/`.log`; Unity 6000.5.7f1, exit 2. O asset `Shop_Renko` está referenciado em `TownScene.unity`, e o teste o compra pelo `ShopManager` por 17g; vende pelo único `SellPoint` materializado em `FarmScene.unity`, religado a managers isolados, por 20g após respec do estado isolado. GUIDs de loja e componente foram conferidos nas cenas. O teste ainda inicializa a sessão do Renko na Farm e não dirige UI/portas de Town nem compra a build legal da árvore. O PlayMode anterior com `Shop_General_Store` permanece como histórico, mas seu asset não está referenciado em cena. |
| PlayMode de payout agrícola controlado, r3 | **PASS, 1/1 para transação; mediana diária NOT RUN** | `Logs/skills-phase21-observed-income-r3-payout-results.xml`/`-unity.log`, Unity 6000.5.7f1, exit 0, log scan PASS; JSON/CSV `Logs/skills-phase21-observed-income-l20-controlled-payout.*`. Três trigos do yield base autorado foram injetados no inventário isolado e vendidos no SellPoint da Farm por +36g; uma meta de venda creditou +20g à carteira global separada (50→70). Não houve plantio/dias/colheita. O processo batch foi dedicado e o teste restaurou carteira/progressão/metas ao final. |
| PlayMode de um ciclo de trigo no FarmPlot, r3 | **PASS, 1/1 como diagnóstico controlado; mediana diária NOT RUN** | `Logs/skills-phase21-observed-income-r3-cycle-results.xml`/`-unity.log`, Unity 6000.5.7f1, exit 0, log scan PASS; JSON/CSV `Logs/skills-phase21-observed-income-l20-farmplot-cycle.*`; 13/13 hashes em `Logs/skills-phase21-observed-income-r3-input-hashes-20260913.csv`. Em processo batch próprio, `plot_200` materializado: semente do asset Renko comprada por 5g via sessão `ShopManager` na Farm; plantio/rega/colheita reais via ponte interna, sem menu/input nem viagem Town; D1→D4, 5 trigos por evento/inventário, SellPoint +60g. Carteira da transação 5→0→60: **+55g isolados**, excluindo ferramentas e recompensas; carteira global 50→85: **+35g** de duas metas diárias autoradas, registradas separadamente. O teste restaura carteira/HP/progressão/metas; DayStarted avançou quest/clima sem rollback integral. A rodada r2 combinada 2/2 fica como diagnóstico histórico, superada por estes dois processos independentes. Não é renda total nem aceite AC05. |
| PlayMode de janela escalonada L20, r2 | **PASS, 1/1 para mediana controlada L20; AC05 FAIL** | `Logs/skills-phase21-steady-income-l20-r2-20260913.xml/.log`, Unity 6000.5.7f1, exit 0, log scan PASS; JSON/CSV `Logs/skills-phase21-steady-income-l20-three-plots.*` e 17 hashes pós-r2 em `Logs/skills-phase21-steady-income-l20-r2-20260913-inputs.sha256`. Seis linhas D0–D5 em três `FarmPlot` materializados: nos três dias estáveis, venda 60g, semente −5g, metas diárias +35g globais, medianas transacional 55g e composta 90g/dia. Auditoria independente conferiu XML, eventos, carteira, CSV e hashes 17/17; rollover fica fora de cada linha. Sessão Renko iniciada na Farm e ponte interna sem Town/UI; L50/L54 foram medidos depois na mesma janela, mas o teto global `<20%` segue NOT RUN. |
| Auditoria estática de preços e referências Town | **76/91 pares com margem potencial no catálogo; 56/70 quotes neutros nas lojas ligadas** | `powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File Logs/skills-phase21-price-audit-20260913.ps1` gerou `Logs/skills-phase21-price-audit-20260913.csv`: 28 lojas, 91 pares, 27 SKUs vendáveis, oito cenários SellAll por estoque/lote e projeção de teto por SKU. A conferência de referências encontrou 23/28 lojas ligadas à Town, com 70 pares vendáveis e 27 SKUs; 56/70 cotações de compra R3 → SellAll neutro têm margem. A General Store teria até 88g por reposição diária teórica, mas não está referenciada em cena; Renko está. No cenário hipotético com piso 1,20, 0/90 lotes vendáveis lucram na venda neutra, 86/90 só com Finan, 90/90 com Finan+CropSurge. O teto por SKU **calculado sobre todo o catálogo, sem filtro de acesso**, reduziria a venda neutra de 24/27 SKUs e a cesta hipotética de estoque da General Store de 558g a 470g. A conferência de referências e o CSV não provam entrada real em cada loja, renda observada nem transações desses pares. |
| Matriz diagnóstica cross-channel EditMode | **1 PASS; 5 FAIL esperados por defeito AC05** | `Assets/_Game/Tests/EditMode/Skills/Phase21CrossChannelPriceTests.cs` usa assets reais para ranks R1–R3, override, unidade/lote/estoque, loja, SellAll neutro/Finan/Âmbar/CropSurge/combinado, SellPoint e Shipping. Unity 6000.5.7f1: desconto efetivo do pão 1/1 PASS em `Logs/skills-phase21-crosschannel-discount-editmode-20260913.xml`/`.log` (exit 0, scan PASS); matriz `[Explicit]` 0/5, 5 FAIL em `Logs/skills-phase21-crosschannel-matrix-editmode-20260913.xml`/`.log` (Unity exit 2), com 792 cotações item/rank/lote por cenário e 1872–2032 cotações lucrativas por cenário somando canais. A falha é de assert econômico, sem `error CS`; o scanner rejeita a suíte falha. É matriz de quotes/asset, não transação de compra/venda. |
| Revisão independente de política econômica | **NEEDS_DESIGN_PASS; sem asset alterado** | O parecer cruzou resolução de evento, bônus em lote, 28 SKUs únicos e cestas de prateleira. Com piso 1,20 e estoque diário completo, projeção encontra 86/90 pares vendáveis com margem usando só Finan e 90/90 com Finan + CropSurge; estes são limites estáticos, não PlayMode. Oito pães custariam 160g e renderiam 227g com Finan+Lua+CropSurge. Cesta de uma unidade de cada SKU da General Store iria de 81g a 98g (+21%); pão+kit passaria de 45g a 54g, superando os 50g iniciais. O refinamento/spec 21E guardam alternativas e o requisito de renda observada por estágio. |
| Identidade dos inputs da rodada final 2026-09-13 | **PASS, 10/10 hashes novos; histórico 20/20 preservado** | `Logs/skills-phase21-crosschannel-input-hashes-20260913.csv` identifica os testes novos, executores de preço/venda, `Shop_Renko`, pão e script/CSV ampliados. O manifesto anterior `Logs/skills-phase21-economy-support-hashes-r3-20260913.csv` conserva os inputs da rodada inicial, agora supersedida nos pontos de diagnóstico alterados. Hash não substitui XML, log ou inspeção visual. |
| Reparação da paridade Elite | **PASS no asset e no teste** | `Logs/skills-phase21-elite-repair-20260912.log`; Editor API reparou quatro `EnemyDataSO.IsElite`; teste `Phase21EliteAssetParityTests` passou depois da reparação |
| Smoke gráfico Farm/Town/Cave | **PASS, 1/1 como materialização/câmera; aceitação visual PENDENTE** | `Logs/skills-phase21-manifest-graphic-r7-20260912.xml` e `.log`; Direct3D 11, suíte combinada 2/2, exit 0; capturas em `Logs/skills-phase21-graphic-captures-r7-20260912/`. |
| Docs global (gate `docs`) | **FAIL — dívida de schema do repositório** | `tools/docs/run_strict_validation.ps1 -Gates docs`, exit 1 após a revisão; spec 21 sem finding próprio, mas specs antigas têm prefixo inválido e várias ativas ainda não possuem `required_adrs`/`required_game_rules` (incluindo fases 22–26). |
| Qualidade dos docs alterados (gate `quality` scoped) | **PASS** | `tools/docs/run_strict_validation.ps1 -Gates quality -ScopePath Logs/skills-phase21-amendment-quality-scope.json`; exit 0, spec 21, refinamento, report e HTML. Não equivale a `docs` global. |
| Qualidade dos novos docs econômicos (gate `quality` scoped) | **SCOPED_PASS** | `tools/docs/run_strict_validation.ps1 -Gates quality -ScopePath Logs/skills-phase21-economy-docs-quality-scope.json`; exit 0 após edição de T3, refinamento, draft histórico 21E, spec executável 21F e este relatório. 21F está em execução com T1–T3 concluídas no recorte; o gate não é `docs` global. |

A primeira rodada EditMode teve 19/20: `enemy_cave_burrower_elite` estava marcado como comum no
`EnemyDataSO`. O reparo pelo gerador atualizou esse asset e mais três fichas com a mesma
divergência (`enemy_corrupted_orc_champion`, `enemy_veilkin_voidassassin`,
`enemy_veilkin_voidknight`). A repetição focada passou 20/20; a rodada integrada posterior passou
45/45. O primeiro PlayMode da Fagulha foi interrompido por um diagnóstico do
`DeathSystemBootstrap` na cena vazia do Test Runner; a repetição consumiu somente essa mensagem
esperada e passou 1/1.

Uma tentativa posterior de EditMode (`Logs/skills-phase21-final-editmode-r2-20260912.log`) foi
barrada por `CS0118` em `TownAllEntriesPlayModeProbe.cs`, arquivo de outra frente simultânea.
A linha foi corrigida; a rodada EditMode r4 passou 46/46. O primeiro PlayMode de compra
falhou por seleção recursiva de filler na própria fixture, sem implicar ciclo nos nós do jogo.
A revisão corrigida foi executada em r4: usa XP/pontos canônicos e só compra fillers aceitos pela
API real. Ela também falhou, agora por gates efetivos de progressão/tier.

Depois da revisão de AC01, os PlayModes da nova fixture r1/r2 falharam: 7/8 ações L20 compradas
(Tiro Carregado R3 travou no rank cap Tier 1) e 0/14 capstones. O log r2 revelou que
`QuestRuntimeBootstrap` expirou a espera de inventory com `Progression adapter: NULL`, mesmo
com `PlayerProgressionManager` presente no teste. `QuestService` gravava os IDs dos Atos e
logava “+1 skill point granted” sem conceder os pontos. O fallback foi corrigido para preservar
o adapter de progressão disponível, e a recompensa passou a falhar sem consumi-la caso ele
falte; `QuestSourcesTests` cobre esse caso. A fixture também passou a abrir o rank cap com
compras legais de Tier 1 e a verificar o save idempotente dos bônus. O PlayMode r6 passou
8/8 L20, 14/14 capstones e 22/22 equipamentos, com `Progression adapter: WIRED`;
o EditMode posterior passou 71/71, incluindo a guarda de recompensa.

## Spec Compliance Matrix

| Critério | Estado | Evidência que ainda falta |
|---|---|---|
| AC01 — L20 pré-capstone; L50 R1; L54+ R3 | **PARTIAL** | PlayMode comprou/fez round-trip de 8/8 ações L20, 18/18 capstones e quatro linhas de suporte, aplicou 22/22 armas nas builds de combate e salvou recompensas dos Atos pela API. Tiro Carregado R1/R2/R3 comprova dano bruto por rank em seis disparos reais; Marca R1/R3 e Sinal R1 têm cast/efeito diagnóstico; Caveborn/Forja Viva passaram 9/9 em fixtures controladas. O reparo de grafo passou PlayMode 1/1: Isca L20, Instinto/Irrigador L50, Senso R1 L50/R3 L54 e Mochila ainda recusada. Falta compra→ação na mesma jornada, cadeia integral de quests, efeitos de capstone associados à compra legal na mesma run, execução/TTK por build e Boss de gate por estágio. |
| AC02 — TTK e comparação de 25% | **PARTIAL** | Bandas e mediana têm testes determinísticos; o probe r8 confirmou seis linhas naturais Common/Elite por `EnemyInstanceId` e classe `EnemyDataSO+EliteAffix`. O probe exploratório gravou tempos de hits, mas terminou em derrota do jogador sem kill; ainda não há TTK válido, séries por build/rank/alvo nem comparação de risco/recurso equivalente. |
| AC03 — recursos, recusas, whiffs, hits/target, controle | **PARTIAL** | `SkillActionResolvedEvent` e serialização de recusas/commits funcionam; Marca R1 recusou sem estação e gastou 6 STA no commit real. Corte Giratório R1 teve um hit correlacionado por `ActionToken` e `EnemyInstanceId` em alvo Common. Faltam séries por build, custo/whiff/controle fortes em execução comparativa. `ControlWindowUptimePercent=-1` sem observação; jamais interpretar como 0% medido. |
| AC04 — sustain em runs curta/longa | **PARTIAL** | Save/load/respec e uso único têm testes de estado; Sinal R1 ativou um efeito direcional em Cave real com aggro controlado. Thoren R1/R3 foi comprado legalmente e fabricou na forja Farm; o DTO de carga R3 serializado manteve o dia consumido ao restaurar em instância nova, sem SaveManager completo. Faltam runs naturais curta/longa e contabilidade de provisões. |
| AC05 — economia abaixo de 20% e sem arbitragem | **FAIL / PARTIAL para renda L20/L50/L54 controlada; teto global NOT RUN** | Marca R1/R3 teve execução e break-even de stamina. O PlayMode controlado comprou pão por 17g e vendeu por 20g após respec; o grafo já permite comprar Senso, mas Town→Farm→venda com essa build ainda não foi executado. A auditoria estática encontrou 76/91 pares potencialmente lucrativos no catálogo e 56/70 quotes neutros com margem entre pares vendáveis das 23 lojas ligadas à Town; cotações não são transações. Três canteiros deram mediana composta de 90g/dia na base L20/L50/L54, 90g/dia em L50 R1 e 91g/dia em L54 R3; essa janela não usa Thoren nem mede fontes externas. Thoren R1 q1 não teve prêmio de venda e R3 poupou um minério em um craft, mas a renda global e ganho incremental conjunto `<20%` permanecem NOT RUN. |
| AC06 — tuning e regeneração | **PARTIAL** | A revisão independente de design propôs `DamagePerRank=2` para Tiro Carregado; o gerador restrito alterou só o asset correspondente. EditMode 7/7 e PlayMode focado 2/2 confirmaram seis disparos R1/R2/R3. Ainda faltam acertos em inimigo e comparação real de TTK para aceitar o equilíbrio; a reparação Elite corrige classificação de asset, não tuning. |
| AC07 — inspeção gráfica | **PARTIAL** | Farm, Town e Cave renderizam com Direct3D 11; Cave foi aberta pela entrada canônica da Farm e materializou tiles/inimigos. Revisão independente dos PNGs r6/r7 confirmou figura humana central legível em Town, corrigindo a leitura anterior; a imagem isolada não prova sua identidade de Player. As capturas de câmera não mostram uso de skill, telegraph ou reação; HUD/Canvas fica fora do enquadramento técnico e não foi validado. Faixa escura da Cave persiste após 2 s adicionais. |

Para AC05, `EconomyBalanceConfigSO` expõe metas por hora, enquanto catálogos/lojas expõem
preços e ofertas; nenhum fornece a **mediana de renda diária por estágio** exigida para comparar
Thoren/passivas com o teto de 20%. Há uma amostra controlada L20, mas a comparação entre
estágios e builds continua **NOT RUN**; não se infere aceite de teto a partir de uma amostra.
O primeiro PlayMode controlado executou somente o pagamento de três trigos (+36g); o segundo
acionou plantio/rega/colheita reais via ponte interna e confirmou cinco trigos após três dias.
O menu de `FarmPlot` depende de `Input.GetKeyDown` e não foi dirigido. O ciclo de um plot
também disparou metas diárias com ouro/XP na carteira global e eventos DayStarted que avançam
quest/clima; sua carteira isolada não representa renda total. Ainda faltam agenda de atividades,
área, insumos e despesas por estágio e observação de janelas estáveis L50/L54.

**Protocolo AC05 executado parcialmente em L20:** três dos dez canteiros de trigo foram
escalonados. D0–D2 registraram compra no Renko, plantio, regas, STA e despesas;
D3–D5 registraram três colheitas, venda no SellPoint e replantio. O PlayMode dedicado
r2 (`Logs/skills-phase21-steady-income-l20-r2-20260913.xml/.log`) passou 1/1 e o
scan de log passou; hashes de 17 insumos capturados após o r2 estão em
`Logs/skills-phase21-steady-income-l20-r2-20260913-inputs.sha256`. O JSON/CSV
`Logs/skills-phase21-steady-income-l20-three-plots.*` mostra, em cada dia D3–D5,
cinco trigos `Excellent`, venda por 60g, semente seguinte por 5g e líquido
transacional isolado de 55g. Duas metas diárias pagaram 35g à carteira global
em cada dia, conferidos contra seus IDs/eventos: mediana composta controlada
de 90g/dia. A fixture usa pontes internas de `FarmPlot`, sessão Renko inicializada
na Farm e `SellPoint.Interact`; não dirige Town/UI. Ela encerra a linha antes de
`AdvanceDay`, portanto possíveis ganhos no rollover não integram os 90g e o valor
não é renda global exaustiva. Senso R3 não é comprável em L20.
As janelas pareadas L50/L54 foram executadas depois, conforme tabela no início deste
relatório, mas não medem todas as fontes. `docs/game_rules/farm_rules.md`
descreve três trigos/50g e rega não implementada, divergindo do `+2 Excellent`
e da rega exigida no comportamento atual; a regra precisa de reconciliação.
AC05 continua **FAIL**, com comparação global L50/L54 **NOT RUN**.
A auditoria de viabilidade delimitou o valor da janela de trigo em níveis altos:
Thoren/Forja Viva afeta craft real, não colheita ou venda;
Marca exige ativação em estação; Senso `sell` atua no NPC shop, não no SellPoint;
Olho de Material tem bônus RNG de 5%/rank e três colheitas são amostra fraca.
Thoren R1 em L50 e R3 em L54, combinados a Senso R1/R3 após o reparo do grafo,
foram comprados com 27/27 e 29/29 pontos e deram 90→90 e 90→91g/dia na janela
agrícola. A próxima medição de **aceite econômico global** precisa de runs
pareadas com craft, stamina, ingredientes e canais de venda representativos.

O teto por SKU é apenas uma simulação de política sobre todas as 28 lojas: reduziria o valor
neutro de 24/27 SKUs vendáveis, inclusive peixe Sael 66→57g (−13,6%) e pão 20→17g (−15%).
Com bônus acumulados no quote de SellAll, o impacto pode superar 20%: semente de trigo
de Eiran em lote cinco teria 29→20g com Finan+Lua (−31%) ou 35→20g com
Finan+Lua+CropPriceSurge (−42,9%); cinco pães de Renko teriam 116→85g (−26,7%) e
142→85g (−40,1%). São projeções do CSV, não renda observada; o evento hoje bonifica
indevidamente não-cultivos. A política precisa de revisão de payout antes de runtime.
O teto `min(BuyR3)` ainda permite revenda empatada ao menor preço de compra,
contrariando a margem estrita de `docs/game_rules/economy_rules.md`. O spread
hipotético de 1g faria pão Renko 20→16g (−20%), semente de trigo 5→3g (−40%)
e os cinco SKUs vendáveis de Renko 480→380g por lote de estoque (−20,8%);
são quotes estáticos, não renda observada. A revisão independente classificou a
política 21E como **NEEDS_DESIGN_PASS**; a decisão humana posterior escolheu procedência
comprada e retirou 21E da fila de implementação. A revisão de design da procedência é
**SOUND_WITH_AMENDMENTS**, condicionada à preservação de lotes e à política para receitas
com insumos comprados; ainda não há validação Unity dessa mudança.
O inventário atual perde qualidade/raridade do pescado, e `ShippingBinInteractable` usa
`qualityTier=0`; uma futura integração com qualidade positiva teria seu prêmio anulado se
o teto por SKU fosse aplicado após esse prêmio sem exceção. Quotes condicionais de receitas
com tempo/STA autorados em zero mostram madeira 3→13g sob o teto hipotético, perch 12→grilled
fish 34g e quatro leather 52→armor 85g. Não houve craft nem venda executados dessas cadeias;
insumos, unlock, estação e canal ainda precisam de verificação. As medianas L50/L54 e
o impacto do teto por estágio seguem **NOT RUN**; esses cálculos não promovem AC05 nem o resultado global.

Para AC01/AC02, `BossGate_Level15.asset` e `BossGate_Level45.asset` — assim como os outros
quatro gates — apontam para `enemy_meteor_ooze_king`, cuja ficha está na banda 1. Os Boss de
Bestiary das bandas 2 e 4 resolvem no catálogo, mas não têm encontro canônico nesses gates.
O manifesto passou a classificá-los `CatalogOnly`; TTK de Boss por estágio não pode ser
extraído dessas entradas sem um cenário real ou decisão de conteúdo/wiring. Não trocamos
silenciosamente os bosses do jogo apenas para satisfazer a fixture.

O probe de Cave revelou outra divergência entre fontes: os IDs do manifesto original, escolhidos
no bestiário canônico, não aparecem nos seeds L20/L50/L54. Os spawns reais incluem
`enemy_spore_imp` (L20), `enemy_ash_crawler` (L50/L54) e
`enemy_furnace_warden` (Elite L50/L54), com cinco linhas presentes por `EnemyInstanceId`.
Esses três IDs estão no `EnemyDatabaseSO`/Roster, mas ausentes do
`CanonicalBestiaryCatalog`; `BestiaryTelemetryClassLookup` devolve `Unknown` para todos. O
probe r4 os marca `targetPresent=true` e **`NOT RUN` para TTK**, apesar de seu próprio teste
passar como observação de cena. O probe r7 resolveu o mesmo `EnemyDataSO` usado pela Cave,
mas classificou as 62 entradas como Elite ao incluir `entry.IsElite`. A auditoria encontrou
que `EnemySpawnResolver.AddPackSelections` copia `AllowElite=true` para esse flag em todos os
membros do pack, enquanto `EnemyBrain` só ativa mecânica Elite quando o asset é Elite ou a
instância tem `EliteAffix`. Assim a projeção r7 está **invalidada como classificação de combate**.
Spore Imp L20 tem uma instância Vampiric e outra sem afixo; o probe r8 confirmou a segunda
como Common e a primeira como Elite, ambas com `EnemyHealth` ativo. Ash Crawler Common e
Furnace Warden Elite também ficaram `CLASS_READY` em L50/L54. O lookup legado do bestiário
permanece `Unknown`;
`entry.IsElite` continua no diagnóstico para expor a divergência visual. Corrigir o resolvedor
da Cave fica em spec própria, pois esse flag participa do hash/snapshot de runs salvas (ADR-0005).
O r8 de TTK revelou outro defeito de composição: o Common da célula `(58,41)` coincide com
`DecorBlocking`, deixando a aproximação pela fixture fisicamente bloqueada. A proposta de
reparo para novas runs e preservação de snapshots antigos está em
`.specs/features_futuras/spec_cave_enemy_blocking_decor_overlap_v1.md`; não foi aprovada nem
implementada. A fixture de fase 21 deve escolher outro encontro natural verificável, sem
usar o teste de equilíbrio para alterar a geração da Cave.
A auditoria também não encontrou chamada runtime para o bônus de atributos de elite nomeado e
observou que o loot lê somente `EnemyDataSO.IsElite`; ambas são lacunas de balanceamento ainda
sem aceitação, não ajustes feitos nesta fase. Nenhuma banda TTK será aprovada enquanto a
projeção não registrar hits/HP/kill por
`EnemyInstanceId` e sua classe efetiva. A divergência entre as duas fontes continua explícita.

Para AC01, `PlayerProgressionRules.CalculateTotalSkillPointsAtLevel` concede um ponto a cada
dois níveis. `SkillTierRules` exige 26 pontos **já gastos na árvore** para Tier 5, onde estão
os capstones das builds. R1 custa pelo menos 27 pontos totais, R3 pelo menos 29. A base é
10 em L20 e 25 em L50; os Atos 1 e 2 podem acrescentar um ponto cada, segundo
`docs/game_rules/quest_rules.md`, logo L50 com ambos os atos permite R1, mas ainda não R3.
Sem atos, os limites inferiores são L54 e L58; com ambos, L50 e L54. A fixture da matriz
anterior usava apenas pontos por nível; a revista r6 simula os dois bônus pela API idempotente
de `QuestService`, sem afirmar que toda a cadeia de quests foi concluída. Mesmo com Ato 1, L20 tem no
máximo 11 pontos: Corte Giratório R1/R3 é alcançável, mas Flecha Sangrante requer pelo menos
13/15 pontos e Rajada Gélida 12/14. O PlayMode da matriz anterior r4 confirmou recusas de tier,
com 0/28 linhas ofensivas materializadas. A direção aprovada preservou a progressão e corrigiu
a matriz; o PlayMode r6 comprou e equipou as 22 linhas ofensivas legais. Nenhum ID preenchido
foi tratado como medição de equilíbrio ou como conclusão das quests.

`survival.sinal_retirada` e `crafting.marca_eficiencia`, pré-requisitos dos capstones de
Telisandra e Thoren, estavam marcados `NotYetExecutable` apesar dos executores existentes.
A correção da lista dormente em `DefaultSkillCatalog` e a regeneração Editor deixaram ambos os
assets com `NotYetExecutable: 0`; isso repara a configuração remanescente das specs aprovadas
15/16. O PlayMode de compra confirmou os dois capstones em R1/R3, mas cast/efeito por rank e
runs de sustain/economia ainda pertencem ao T05 da spec 21. Marca de
Eficiência reduz stamina de trabalho em estação, enquanto Forja Viva melhora output: os
efeitos não se confundem. Sem o reparo, essas duas builds não se tornariam completas nem em
L100 apenas por receber mais pontos. A revisão
independente recomenda preservar a curva e as identidades: em L20 comparar ações pré-capstone
alcançáveis por disciplina; em L50 testar capstone R1 com os Atos 1 e 2 concluídos; acrescentar
capstone R3 a partir de L54 como cenário próprio. A matriz de ações R1/R3 pode permanecer em
L20/L50 somente onde o nó é realmente comprável. Survival/crafting estavam bloqueados por
aqueles pré-requisitos; o reparo de catálogo acima liberou a compra, mas as runs de sustain/economia
seguem sem aceitação. O usuário aprovou essa direção
em 2026-09-12; AC01, plan e tasks da spec foram revisados sem alterar a progressão. O cálculo, as
alternativas e as decisões propostas estão em
`docs/refinements/a_implementar/ref_skills_phase21_feasible_acceptance_matrix_v1.md`.

A revisão de design encontrou um bloqueio de rank em Tiro Carregado: o asset autorado tinha
`DamagePerRank=0` e o controlador zerava esse campo no clone de execução. R1/R3 compravam a mesma
ação, mas só o tempo segurado variava dano/alcance/custo. A correção proposta preserva a curva de
carga 8→20 e adiciona +2 de dano por rank após a carga: R1 8→20, R2 10→22, R3 12→24. O ganho de
R3 é 20% no tiro totalmente carregado e 50% na carga mínima; a revisão independente julgou a
regra coerente com Corte Giratório e Tiro Triplo, mas **provisória até cast real e AC02**. O asset
será regenerado por método restrito para não reescrever outras skills. A mesma revisão encontrou
que o bônus de atributos Elite +25% existe na regra, mas não tem chamada runtime no materializador,
e que o loot do Spore Imp Vampiric usa `EnemyDataSO.IsElite=false`. São lacunas de classificação,
risco e recompensa a medir por instância, não justificativa para alterar HP ou dano por hipótese.

## Existing systems audit

O relatório JSON é aditivo (shape 2) e não sobrescreve outro arquivo com mesmo seed/nível/segundo.
`BuildId` e variante ficam vazios no serviço passivo até uma build real ser registrada; versão do app
e bioma não são substitutos. `EnemyDamagedEvent` identifica tipo de inimigo, portanto a contagem
por `enemyId` não é hits por instância e TTK pode misturar dois alvos do mesmo tipo simultâneos.
Vulnerabilidade não foi tratada como controle forte. A janela observada termina no flush; sem evento
de controle, a porcentagem é indisponível. Os testes sintéticos da sessão provam cálculo e shape,
não observação runtime dos campos ausentes.

O `QuestRuntimeBootstrap` agora espera um `PlayerProgressionManager` antes de criar `QuestService`:
um serviço sem adapter consumia os IDs dos Atos sem aplicar pontos. Em uma cena sem progressão, quests
ficam indisponíveis enquanto esse manager não aparecer. O fluxo Farm testado inicializou os adapters
de inventory, gold e progression (`WIRED`) e a regressão dos Atos passou 43/43; a disponibilidade
em cenas sem personagem ainda não foi exercitada e permanece risco de integração, não PASS global.

O smoke gráfico usa a câmera do mundo, sem sobrepor HUD/Canvas. A primeira captura da Cave ficou
preta porque o teste carregou a cena diretamente, sem iniciar a run. A versão final entra pela
`CaveEntranceInteractable` da Farm e exige nível/tile materializados; a Cave renderizou cenário e
inimigos. Nova revisão independente dos PNGs r6/r7 mostrou uma figura humana legível
na região central da trilha clara de Town; a imagem isolada não prova sua identidade
de Player, mas a afirmação anterior de que nenhuma figura aparecia era incorreta.
`Logs/skills-phase21-graphic-captures-r6-20260912/phase21-TownScene.png` e a r7
têm o mesmo SHA-256 `39A51FD1...73EB0`. A Cave mostra faixa horizontal escura abrupta por volta de y=408, também
presente nas capturas `Logs/skills-phase21-graphic-captures-r6-20260912/phase21-CaveScene.png`
e `Logs/skills-phase21-graphic-captures-r7-20260912/phase21-CaveScene.png`
após dois segundos adicionais, portanto não é só o primeiro
frame de chegada. O teste usa `camera.Render` em `RenderTexture` RGB24 e verifica
pixels não pretos e objetos; HUD/Canvas de sobreposição não aparece nessa captura,
o que não prova defeito do HUD no jogo. Uma sequência real com GameView/Canvas,
windup→impacto→reação→recuperação, ator/alvo e frames identificados em Farm/Town/Cave,
mais reparo e recaptura da faixa da Cave, são necessários para AC07.

## Honest status rationale

O próximo corte de TTK deve repetir uma rotação de skill mais arma equipada em quatro células
L20 (Corte Giratório R1/R3 × Spore Imp Common/Elite Vampiric), com registro por
`EnemyInstanceId` até `EnemyKilledEvent`. O acerto único de 10 em 262 HP comprova o pipeline,
mas não mede TTK. O `CombatTelemetrySession` agrega por `EnemyId` e mistura Common e Elite do
mesmo tipo; o runner precisa correlacionar eventos por instância. O caminho de Q/E no
`PlayerAttackController` aciona a arma equipada; o proxy de `PlayerCombatController.TryAttack`
é desarmado e não pode substituir essa rotação. Quatro kills ainda não fornecem mediana nem
aceitação global de AC02.

Construir a fixture de renda canônica e o runner que aplica builds reais a Common, Elite, Miniboss
e Boss; produzir JSON/CSV bruto com contexto completo; executar runs survival/crafting e sessão
gráfica. Só depois comparar medianas, ajustar `SkillActionSO`/nós/perfis autorados onde houver
falha e repetir as linhas afetadas. Manter T01–T07 abertos até evidência por critério.
