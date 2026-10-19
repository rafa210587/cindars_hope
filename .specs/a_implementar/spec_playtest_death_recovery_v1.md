# SPEC — correção de derrota, lágrima e retorno à Fonte

Spec ID: `spec_playtest_death_recovery_v1`. Data: 2026-09-27. Prioridade P1.
Status: UNITY_VALIDATED em 2026-09-27 (EditMode integrado 166/166 e regressão final 35/35); rota de derrota jogada pendente. Sem promoção.
Tipo: Runtime / integração / manutenção. Wave: pacote playtest, D02–D03.
Executor: recovery; validação Unity integrada pelo coordenador. Paralelizável com save/UI fora de Death.
Depends: domínio existente de inventário/corpo; D01 para smoke integrado save/load. Blocks: aceite do loop de derrota.
Ordem de execucao: antes da cobertura integrada.
Depende de: contrato existente de inventário e corpo.
Bloqueia: aceite da rota de derrota no pacote integrado.
required_adrs: []
required_game_rules: []

# /speckit.specify

## Phase 0 e contrato reconciliado
`CaveDeathResolver` transfere inventário completo ao corpo antes da escolha; UI consulta inventário vazio.
`AnyaRespawnService` move o GameObject de PlayerManager, localizado no bootstrap, em vez do avatar.
`AnyaFountainRespawnFlow` solicita `spawn_farm_default` ausente e conclui em sceneLoaded antes do spawn em Start.
Auditoria: `rg` nos owners, SceneSpawnInstaller/PlayerSpawnResolver, CreateMvpFarmScene e testes de death.
FASE9J determina perda de inventário/equipamento/ouro e XP; preservar. Documento death_anya_corpse_rules
diverge dessas regras e do runtime (recovery pago, sem XP); não aplicar essas regras antigas incidentemente.
Não existe contrato de propriedade de lágrima nas specs anteriores: esta correção explicita a unidade elegível
do corpo criado NESTA morte; consumi-la não recupera os outros itens nem desfaz penalidades. Fora da Cave,
consumir inventário atual. Corpo anterior nunca financia outra morte. Uma decisão aceita somente uma ação.
FASE9F: path de amendment removido; lidos audit e refinement consolidado ref_cave_stable_run_replay_pr170_192.
Nenhuma geração/seed/snapshot alterada. Captura da posição do corpo usa avatar ativo.

## Escopo e contratos
Reusar CaveDeathResolver, CorpseRecoveryManager, DeathSystemBootstrap, AnyaRespawnService e UI existente.
Criar `DeathRevivalDecision`: Begin(string corpseId), EligibleTearCount, TryRevive(), Close().
Recebe owners existentes e delegates de contagem/consumo de inventário; não cria manager/singleton.
Modificar CorpseRecoveryManager com contagem/consumo de lágrima somente se corpseId coincide e ativo;
consumo remove uma unidade e respectivo origin lot, antes de curar. Sem novo DTO/schema/ID de item.
Criar eventos DeathDecisionReadyEvent(scene, count), DeathReviveRequestedEvent,
DeathReviveCompletedEvent. Bootstrap resolve penalidades, abre decisão e publica Ready; UI só projeta/pede.
TryRevive valida decisão+HP morto+fonte elegível, fecha decisão antes da mutação para impedir reentrância,
consome uma unidade, cura HP cheio e publica conclusão. Falha não cura nem fecha tela.
AnyaRespawnService publica PlayerRespawnedEvent para avatar; PlayerController sincroniza Rigidbody e Transform.
Flow carrega Farm por anchor serializado existente `farm_from_cave` e conclui na referência AnyaFountain depois de Start.

## Arquivos permitidos / proibidos
Player/Death/{DeathRevivalDecision,CorpseRecoveryManager,AnyaRespawnService,AnyaFountainRespawnFlow}.cs;
Cave/Death/{CaveDeathResolver,DeathSystemBootstrap}.cs; UI/Death/DeathScreenCanvasController.cs;
Player/PlayerController.cs (somente subscriber/handler respawn); Core/Events/Death*Event.cs;
Tests/EditMode/Player/DeathRecoveryRegressionTests.cs; respectivos .meta.
Proibido editar cenas/YAML, dados de balanceamento, save schema, geradores/procedural e inventário externo.

## Plano e gates
# /speckit.plan
# /speckit.tasks
1. Autoridade/consumo no domínio (event-bus-pattern, save-load-pattern para conservação dos lots).
2. Wiring Bootstrap→Ready→UI e Request→decisão→Completed; listeners removidos em OnDisable.
3. Corrigir colocação avatar e ordem de load; fallback mantém recuperação de HP sem coordenadas inventadas.
4. EditMode `DeathRecoveryRegressionTests`: 0/1/2 lágrimas, clique repetido, corpo antigo, duas mortes,
   conservação origin lots e recuperação após consumo, posição avatar diferente do manager.
   Integração `DeathEvent_ProjectsPostTransferTearsRegardlessOfUiSubscriptionOrder` publica morte com
   Canvas inscrito antes/depois do owner, verifica transferência, count=2 e request→HP=100/corpo=1.
5. Reusar PurchasedOriginDeathEventOrderTests, DeathScreenCanvasTests, PlayerDeathDecisionTests e AnyaFountainActiveInstancesTests.
DoD automático: filtros acima com failed=0 no XML Unity. DoD integração: Cave→derrota→revive e nova
derrota→Fonte→corpo; 1 consumo, avatar na referência, controle/câmera coerentes, recovery sem duplicação.
Roundtrip do corpo já tem DeathSectionProvider; incluir estado pós-consumo no teste novo.

## Falhas e closeout
Sem corpo/run: inventário elegível, nunca corpo antigo. Body substituído: recusar consume sem mutação.
Evento repetido/click repetido: decisão fechada recusa. Inventário falha: sem HP ganho e decisão continua.
Cena errada/load recusado: fallback documentado, sem alegar chegada à Fonte. Sem avatar: não mover bootstrap.
Save durante modal morto não é novo estado suportado; roundtrip validado é corpo após decisão.
Status de execução e evidência serão registrados pelo coordenador; não promover sem XML e smoke aplicável.

## Execução da fatia
Código e 12 casos da fixture nova escritos; `git diff --check` PASS nos arquivos desta fatia.
Unity validation: NOT RUN pelo executor desta fatia (rodada única integrada pertence ao coordenador).
Residual risk: Unity compile/integration ainda requerem XML e Play Mode integrado; sem aceite humano.
Revisão independente solicitada ao coordenador. Nenhuma cena, schema, seed ou save do usuário editado.
