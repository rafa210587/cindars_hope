# Fundação das habilidades — implementação e validação

Data: 2026-09-10. Status da fatia: SCOPED_PASS técnico; lote completo ainda PARTIAL.
Specs: [01](../../../../.specs/a_implementar/spec_skills_01_avatar_execucao_v1.md), [02](../../../../.specs/a_implementar/spec_skills_02_pontos_respec_save_v1.md), [D01](../../../../.specs/a_implementar/spec_skills_03_shared_cooldowns_v1.md).
Sem commit, push, alteração manual de cenas/YAML ou promoção das specs de UI/arte.

## Resultado

Habilidades usam o avatar vivo e a mesma recarga por actionId entre slots. Compra/rank/respec preservam o ledger de pontos e anunciam sucesso depois do commit; o ouro real já está atualizado quando o respec é anunciado. Save de skills v3 preserva custos não unitários e a progressão clona o snapshot de entrada. Um save de skills incompatível é verificado antes de qualquer restauração ou troca de cena.

Avanço reutiliza a física existente, respeita parede e aplica o golpe após o movimento. Cada receptor recebe um hit, mesmo com múltiplos colliders. Quatro ações aplicam os status existentes, com chance explícita 1. Recurso obrigatório ausente/insuficiente recusa execução.

Spark reforçou a skill de validação e seu agente. Os executores Spark de gameplay atingiram o limite antes de gravar código. O usuário autorizou o agente primário a implementar; toda implementação de gameplay desta entrega foi feita pelo primário. Um agente independente revisou o diff, os XMLs e as capturas; findings foram corrigidos e conferidos.

## Acceptance criteria extracted

Os critérios abaixo correspondem às três specs ativas; requisitos de UI/arte/equilíbrio estão fora desta fatia.

## Spec Compliance Matrix

| Critério | Evidência | Resultado |
|---|---|---|
| 01 AC01/AC05: avatar nas três cenas, substituição, disable/re-enable e singleton | GameplayScenes_BindAvatarAndRefreshAfterSameSceneReplacement | PASS |
| 01 AC02: mana zero, modal/clique, índice inválido, ausência de recurso/status | PlayMode integrado + SkillExecutionFoundationTests | PASS no escopo |
| 01 AC03: multicollider, parede, interrupção e hit após física | EditMode multicollider + dois cenários físicos PlayMode com PlayerController real | PASS |
| 01 AC04: quatro status do catálogo funcionam com o database da cena | CatalogStatusEffects_ApplyAllFourUsingSceneDatabaseAndRealCollision | PASS |
| 02 AC01: compra/rank custo real, queries sem mutação, falha/reentrância | SkillPointTransactionTests | PASS |
| 02 AC02/AC03: 12+3→15→14, ouro insuficiente, carteira visível no evento | SkillPointTransactionTests | PASS |
| 02 AC04/AC05: clone/round-trip, v3/legado/duplicatas, restituição, versão/overflow | SkillPointTransactionTests + fixtures Save | PASS |
| D01 AC01–04: alias/cópia/troca, independência, modal, pausa, índices | SkillCooldownTrackerTests + controller real nas três cenas | PASS |

O preflight de SaveManager foi verificado pela invocação real de ApplySaveData com seção incompatível: até a primeira seção (dia) permanece intacta. A entrada pública de arquivo e o ramo de troca de cena foram revisados estaticamente; não foram exercitados gravando/carregando o save pessoal do usuário.

## Existing systems audit

Reusados PlayerController.ActiveInstance, PlayerMovementDisplacementResolver, SkillTreeManager/State/PurchaseService/RespecService, providers existentes, GameEventBus, ProjectileSpawnService e StatusEffectDatabase. Nenhum novo manager ou catálogo paralelo. SkillCooldownTracker é estado C# puro interno ao controller, com relógio fornecido pelo caller. Recarga usa Time.time escalado, sem persistir prazo absoluto em disco.

Os callers FonteInteractable e AnyaFountainMenu preservam seus gates; somente coordenam o commit de ouro antes do evento. Sem alteração de capstones, dano/custos, curvas de nível ou comportamento procedural de cave.

## Validation

Unity: 6000.5.7f1. Um owner por vez. Wrappers verificaram XML com casos positivos, exit e logs; não houve compile-only redundante.

| Rodada | Artefato | Resultado |
|---|---|---|
| Baseline anterior ao runtime | [XML](baseline-editmode.xml) | 28/28 PASS; não prova correção |
| Skills após fundação | [XML](foundation-editmode-r4.xml), [log](foundation-editmode-r4.log) | 46/46 PASS |
| Skills + Save + ProgressionCurve | [XML](foundation-integrated-editmode.xml), [log](foundation-integrated-editmode.log) | 131/131 PASS, exit 0 |
| Física + controller/cenas + status 0/1 | [XML](foundation-playmode-r5.xml), [log](foundation-playmode-r5.log) | 4/4 PASS, exit 0 |
| Quatro status do catálogo e database real | [XML](catalog-status-playmode.xml), [log](catalog-status-playmode.log) | 1/1 PASS, exit 0 |
| Ratchet arquitetural | [log](architecture.log) | PASS; não aumentou dívida rastreada |
| Harness gerado | Test-CodexHarnessGeneration.ps1, executado após alteração canônica | 36 assertions PASS na entrega da skill |

Os cinco cenários PlayMode são distribuídos em duas rodadas: a última adicionou somente o cenário do catálogo; os quatro anteriores não foram alterados após r5. A evidência EditMode cobre o runtime vigente; depois dessa rodada só se acrescentou o cenário PlayMode. Identificação dos inputs: [manifesto](SOURCE_INPUTS.json). Não equivale a execução global de todos os testes do projeto.

Comandos:
- tools/unity/RunUnityEditModeTests.ps1 com TestFilter CindarsHope.Tests.EditMode.Skills;CindarsHope.Tests.EditMode.Save;CindarsHope.Tests.EditMode.Player.ProgressionCurveTests, outputs foundation-integrated-editmode e TimeoutSeconds 600.
- Runner PlayMode equivalente baseado em UnityValidation.Common.ps1: -batchmode -projectPath <repo> -runTests -testPlatform PlayMode -testFilter CindarsHope.Tests.PlayMode.Composition.SkillExecutionFoundationPlayModeTests -testResults <xml> -logFile <log>. Rodada r5 com gráficos e CINDARS_SKILL_CAPTURE_DIR apontando captures.
- Teste adicional: mesmo runner, filtro terminado em .CatalogStatusEffects_ApplyAllFourUsingSceneDatabaseAndRealCollision; capturas desativadas.

### Falhas intermediárias, preservadas

- foundation-editmode: seis falhas de fixture EditMode sem Awake; setup corrigido, r2 41/41.
- foundation-editmode-r3: erro de implementação no posicionamento inicial do preflight; corrigido, r4 46/46.
- foundation-playmode e r4: cenário de parede interrompido pelo timeout de DeathSystemBootstrap enquanto a primeira cena carregava a partir da cena vazia de testes. O setup inicial passou a carregar Farm sincronamente; as transições efetivamente testadas continuam assíncronas. Nenhum erro foi suprimido. r5 4/4.
- foundation-playmode-r2: seis erros concorrentes TownCityLayout/TownKeyartGeometry impediram compile. Esses arquivos não foram corrigidos por esta tarefa; o trabalho concorrente liberou as rodadas seguintes.
- foundation-playmode-r3: qualificação de UnityEngine.Camera ausente no teste de captura; corrigida antes de r4/r5.

## Inspeção visual real

[Farm](captures/FarmScene-fire-spark.png), [Town](captures/TownScene-fire-spark.png), [Cave](captures/CaveScene-fire-spark.png): 1280×720, câmera de gameplay, imediatamente após cast. Agente primário e revisor independente inspecionaram a evidência; não há aceite humano.

Avatar e projétil aparecem próximos no mundo. O projétil ainda é um disco laranja plano: funcional, visual provisório. Cave apresenta região esbranquiçada e corte horizontal de iluminação; a causa não foi determinada nesta fatia. Essas imagens não aprovam apresentação final.

Camera.Render não demonstra UI ScreenSpaceOverlay, navegação por teclado/gamepad, animação completa ou impacto visual. As specs 06/07 permanecem pendentes.

## Revisão independente e limites

O revisor encontrou e verificou correções para: evento antes do ouro real; preflight tardio do save; falta de cobertura do PlayerController no teste físico; duplicação do controller ao reativar instância antiga. Parecer final: sem bloqueadores técnicos restantes na fundação revisada. Isso não aprova a arquitetura inteira do baseline.

Não há validação de equilíbrio global, builds completas, novas mecânicas ou arte final. Cooldown não persiste entre processos, como no baseline. A probabilidade 1 dos quatro status restaura a aplicação já descrita, não representa uma conclusão sobre sua força relativa.

## Honest status rationale

01 e D01 têm comportamento PlayMode comprovado. 02 possui validação Unity/EditMode integrada de transações/save. Mantidas na fila até fechamento de wave; não promover UI/arte por associação.

Próxima fatia: fechar contratos de dados/rank/readiness da 03 original, depois Canvas da 06 e pixel art da 07; mecânicas ativas precisam de specs filhas por família. D02 (capstone 3 vs 5 ranks) continua sem resposta; não foi alterada. As demais decisões de equilíbrio seguem no refinamento, sem mudanças arbitrárias.

Cenário de aceitação final: [roteiro](HUMAN_SCENARIO.md). Um roteiro escrito não constitui execução humana.



## Gates documentais de fechamento

tools/docs/run_strict_validation.ps1 com gates diff,quality e scope.json: SCOPED_PASS. Ratchet arquitetural PASS, sem aumento de dívida rastreada. Docs global: FAIL com 59 diagnósticos em outros documentos; zero diagnósticos referindo as specs/relatório de skills desta fatia. Ver docs-global.log e scoped-quality.log. Nenhuma alegação de GLOBAL_PASS.

As marcações de Spec/Plan/Tasks, dependências e frontmatter das três specs ativas foram corrigidas após a primeira checagem documental. Índice regenerado. Fechamento técnico registrado; specs mantidas na fila até aceitação da wave. Nenhum teste Unity foi repetido por edição de documentação.
