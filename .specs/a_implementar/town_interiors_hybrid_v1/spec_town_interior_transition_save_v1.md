# SPEC — transição orgânica, cena aditiva e save dos interiores

**Spec ID:** spec_town_interior_transition_save_v1 (S02)  
**Revisão:** 1.0 / 2026-09-12  
**Status:** PARTIAL — visita/save/autosave/recovery do piloto com evidência selecionada; falhas por fase, continuidade, Single integrado e Player pendentes. [Checkpoint S02](../../../docs/validation/town_interiors_hybrid_v1/S02.md).
**Wave:** TOWN_INTERIORS_HYBRID_V1 (lote local)  
**Type/Domain/Priority:** Runtime + Save + Integration / SceneManagement, Save, City / P0  
**Executor:** Claude ou Codex; implementador de integração, auditor read-only de save  
**Parallelizable:** NO; contratos e fontes de bootstrap/save sob um owner  
**Repo lock scope:** SceneManagement/World/Scenes, SaveManager/providers, PlayerController/InteractionSystem e installer Town citados em §18.  
**Ordem de execucao:** 2 de 4  
**Depende de:** [S01](spec_town_interior_catalog_v1.md), catálogo/amendment aprovados.  
**Bloqueia:** S03 e S04; ativação pública bloqueada até S03.  
**Scope:** travessia de Player, conteúdo aditivo, câmera/input/fade, localização/save/rollback.  
**Out of scope:** refatorar o router inteiro, mudar Farm/Cave, agenda/economia (S03), arte final (S04).  
**Validation level alvo:** PLAYMODE + save compatibility + Player build smoke; nesta autoria NOT RUN.

required_adrs: []
required_game_rules: [event_rules, city_rules]

# /speckit.specify

## 5–9. Objetivo e Phase 0 real

Entrar parece atravessar uma porta, não acionar teleporte ao conversar. O exterior mantém a fachada/telhado em casas Portal; o interior é outro espaço, com escala e owner preservados.

Fontes inspecionadas: `Assets/_Game/Scripts/World/Scenes/{SceneTransitionRouter,SceneTransitionRequest,SceneTransitionState,SceneFadeOverlayController,SceneFadeOverlayBootstrap}.cs`; `Assets/_Game/Scripts/SceneManagement/{SceneTransitionRouterAdapter,SceneSpawnInstaller,TownSceneRuntimeReferenceInstaller}.cs`; `Assets/_Game/Scripts/Save/{SaveData,SaveManager}.cs`; providers Player/Npcs; `CameraTransparencySort2D.cs`.

Hoje `Execute` faz load Single logo após started, guard limpo em sceneLoaded; fade depende de callbacks e fallback sceneLoaded. Request possui SourceSceneId/DestinationSceneId/TargetSpawnAnchorId/InitiatingGateId; state só PendingSpawnId. Adapter é fachada do mesmo router, não segundo sistema. SaveManager usa cena ativa e posição mundial; NpcsSectionProvider só captura em Town ativa. Nenhum `TownInteriorSession` ou save por HouseId encontrado. Estes são deltas novos, não wrappers a criar sobre “sistema pronto”.

Reconfirmar métodos e ciclo real de boot antes de execução. Skills obrigatórias de implementação: `system-reuse-audit`, `event-bus-pattern`, `save-section-provider`, `save-load-pattern`, `bootstrap-wiring`, `state-machine-design`, `input-gamepad-routing`, `unity-validation`. Não copiar padrão Cave nem tocar snapshots.

### Revisão independente de contratos — 2026-09-12

Auditor `gatekeeper_room_rebuild`, read-only; root revisou a proposta. Confirma §17/20 com estes seams obrigatórios, sem alteração de escopo:

- `SceneTransitionRequest.IsValid` deve validar por Kind antes de qualquer efeito do router; visita não preenche PendingSpawn nem publica eventos Single.
- Readiness é registro/sinal da instância Town após rebind e SaveManager inicializado, com timeout; dois frames de espera não provam readiness. Installer injeta `ITownInteriorLocationRuntime`, sem dependência concreta em Player.
- `SaveGame` captura localização committed uma única vez; provider e posição legada derivam desse mesmo snapshot congelado, nunca de duas consultas ao runtime.
- Load na mesma Town e load de outra cena convergem na continuação assíncrona de interior. `GameLoadedEvent` só após colocação/fade ou fallback seguro; saves sem seção conservam caminho legado.
- Para correlacionar o callback assíncrono, o port oferece também `TryRequestRestore(snapshot, operationId, out reason)`; SaveManager fornece token único e espera somente o Finished correspondente. A assinatura sem token permanece disponível, sem inferir sucesso pela casa coincidir.
- Lock tokenizado cobre movimento e InteractionSystem, sem liberar locks de modal/FarmActionMenu. Fade aditivo ignora callback/fallback Single e tokens antigos.

Revisão de contrato permite implementação após S01; não comprova implementação, testes ou aceite visual.

## 10. Arquitetura escolhida e limites

Uma cena `Assets/_Game/Scenes/TownInteriorScene.unity`, carregada Additive, contendo somente raiz de conteúdo e anchors. Um prefab de S01 selecionado por HouseId. TownScene continua **ativa**, Player/câmera/managers originais mantidos. Um interior carregado por vez. Nada de GameBootstrap, Player, EventSystem, TimeManager, ShopManager ou SaveManager no prefab/cena interior.

Raiz física do conteúdo em posição serializada reservada `(4096,4096,0)` para não sobrepor exterior. Isso é detalhe de apresentação, não HouseId: nenhum `y > …`, distância ou nome da cena decide residência/agenda. Converter posição local↔global exclusivamente pelo root. Préflight rejeita overlap de bounds com Town e coordenadas não finitas. Não teletransportar NPC/Player para qualquer Y por heurística.

O Player permanece objeto da Town; sua **localização lógica** é `Town exterior` ou `HouseId + anchor/local position`. Câmera original segue o mesmo Player, recebe bounds do interior e mantém escala; luz/chuva/áudio recebem evento de contexto interior e restauram estado ao sair. Não desabilitar simulação Town inteira para esconder geometria. Confiner/culling deve impedir que a viagem espacial seja vista.

## 11. Sequência orgânica e estados

`IdleExterior → Opening → ThresholdArmed → FadingOut → Loading → Placing → FadingIn → IdleInterior`. Caminho inverso usa a mesma máquina/ID de operação.

1. Player interage via input normal, usando `HouseDoorInteractable` e política aprovada. Porta fechada bloqueia, folha anima no sistema atual. Aproximar-se ou conversar não carrega conteúdo.
2. Só a travessia do **corpo físico** sobre o limiar, direção exterior→interior, com porta efetivamente aberta, inicia visita. Sensor de interação não é limiar. Guard contra múltiplos colliders/inputs; um request por travessia.
3. Nenhuma modal aberta, morte/respawn ou outra transição pendente. Se bloqueado, feedback localizado; corpo permanece no pocket da porta, não desaparece.
4. Lock de movimento/ação do Player por token; não `Time.timeScale=0`, não destruir PlayerController. NPCs/relógio continuam como no jogo; menu pausa preexistente deve ser respeitado.
5. SFX de porta existente, fade out de **0,20s unscaled**, até alpha≥0,99 **antes** de mover Player ou mostrar conteúdo. Valores em `TownInteriorTransitionConfigSO`, não balanceamento inline de MonoBehaviour.
6. Carregar conteúdo, validar HouseId/anchors/collider do spawn, câmera/bounds e contexto; tela permanece preta até conclusão. Alvo deve suportar corpo inteiro + 0,15u de folga em todos os lados e face direcionada para dentro.
7. Commit localização somente após colocação válida. Fade in **0,25s unscaled**, destravar ao concluir. Warm transition visa até 1s; medir cold/warm, não prometer prazo de hardware. Após 0,75s de carregamento, texto discreto “Entrando…” se ainda escuro. Após 10s sem conclusão: erro + rollback seguro, não mostrar meio cenário.
8. Retorno ao exterior usa `town_return_<HouseId>` fora da porta/trigger, virado para rua. Exigir sair do volume para rearme (debounce por crossing), não cooldown arbitrário que permita loop automático.

Guardar porta como apresentação exterior durante a visita é independente do acesso. Fechamento de horário **nunca bloqueia saída**. Ao sair, escolher pocket livre entre anchors de retorno aprovados; não mover/remover NPC que está no pocket. Se todos bloqueados, permanecer seguro no interior com opção de tentar novamente, sem tela preta indefinida.

## 12. Fades e coexistência

Reutilizar overlay/bootstrap existentes. Adicionar modo de operação correlacionado a `operationId` para visita aditiva: o fallback genérico `sceneLoaded` **não** pode iniciar fade in desse modo. Fade só termina após `TownInteriorPlacementReadyEvent`. Não publicar `SceneTransitionStarted/CompletedEvent` globais para uma visita aditiva, pois consumidores de Farm/Cave/quests podem interpretar mudança de mundo. Usar eventos específicos de localização.

`SceneTransitionRouter.Execute` preserva assinatura e comportamento Single para requests legados. Para o novo modo opt-in, valida dados, publica comando de visita e retorna “aceito”, não “concluído”. Apenas um executor Town injetado/registrado no boot processa o comando. O router não ganha dependência em City/Player/Save; contratos simples em Foundation. Não criar um segundo ScenePortal/Router global.

# /speckit.plan

## 13/15. Reuso e tipos novos propostos

CRIAR:

- `Foundation/TownInteriorVisitContracts.cs`: request/state/result e eventos específicos, sem Unity refs.
- `Foundation/ITownInteriorLocationRuntime.cs`: porta de snapshot/restore; sem City concreto no Save.
- `City/Interiors/TownInteriorTransitionConfigSO.cs`: timing/origem/limites de transição, dado injetado.
- `SceneManagement/TownInteriorTransitionStateMachine.cs`: regras puras de operação única, fases, cancelamento/timeout/commit; não chama Unity.
- `SceneManagement/TownInteriorSceneLoader.cs`: adaptador de load/unload aditivo e resolução de root/prefab explicitamente referenciado, sem gameplay.
- `SceneManagement/TownInteriorVisitController.cs`: ponte coroutine/eventos, utiliza máquina, loader, port de Player e overlay; não concentra políticas de owner/loja.
- `World/Scenes/TownInteriorThreshold.cs`: ponte de crossing físico/direção + identidade do endereço; publica request, não aplica posição por conta própria.
- `Save/TownInteriorLocationSaveData.cs` e `Save/Providers/TownInteriorLocationSectionProvider.cs`: seção aditiva de Player; contrato de runtime devolve snapshot simples convertido pelo provider.

MODIFICAR, apenas os pontos descritos: SceneTransitionRequest/Router/State, SceneFadeOverlayController; TownSceneRuntimeReferenceInstaller; SaveData/SaveManager; PlayerController/InteractionSystem para lock por evento/token; Camera/CameraFollow2D para aplicar/remover bounds recebidos por evento, preservando câmera/escala e snap existentes; generator CreateMvpTownScene para installer/anchors/threshold opt-in. A dependência SceneManagement→Player concreta continua proibida: mover/lock via eventos/port Foundation com implementação Player-owned, não cast no installer.

Detalhamento de integração (2026-09-13): `TownInteriorInstance` é marcador explícito no prefab, com identidade, anchors e bounds; não localiza managers. `ITownInteriorVisitAdmission` em Foundation permite ao router validar sem efeitos antes de publicar o comando. Loader retém ownership durante load pendente/cancelamento e descarrega somente o handle aditivo que possui. O limiar agrega colliders sólidos pelo Rigidbody2D e rearma somente quando todo o corpo sai. Esses pontos implementam TR01/02/04, sem ampliar rollout. `CameraFollow2D` não possuía clamp; a edição acima é necessária para o requisito de bounds da câmera original, não autoriza outra câmera.

## 16. Contratos propostos

```csharp
// Foundation — campos somente simples; constructors devem validar/normalizar IDs.
public enum TownVisitPhase { Idle, Opening, Armed, FadingOut, Loading, Placing, FadingIn, RollingBack }
public readonly struct TownInteriorVisitRequest {
    public readonly string OperationId, HouseId, EntryAnchorId, ReturnAnchorId;
    public readonly bool Exiting;
}
public readonly struct TownInteriorLocationSnapshot {
    public readonly string HouseId, AnchorId, ReturnAnchorId;
    public readonly float LocalX, LocalY, FacingX, FacingY, SafeTownX, SafeTownY;
}
public interface ITownInteriorLocationRuntime {
    bool IsTransitioning { get; }
    TownInteriorLocationSnapshot CaptureCommitted();
    bool TryRequestRestore(TownInteriorLocationSnapshot snapshot, out string reason);
}
// Cada evento contém OperationId; os eventos com localização também HouseId/AnchorId.
public readonly struct TownInteriorVisitRequestedEvent { public readonly TownInteriorVisitRequest Request; }
public readonly struct TownInteriorBlackoutReadyEvent { public readonly string OperationId; }
public readonly struct TownInteriorPlacementReadyEvent { public readonly string OperationId, HouseId; }
public readonly struct TownInteriorVisitFinishedEvent { public readonly string OperationId, HouseId; public readonly bool Success; public readonly string Reason; }
public readonly struct TownInteriorPlayerLockEvent { public readonly string OperationId; public readonly bool Locked; }
public readonly struct TownInteriorContextChangedEvent { public readonly string HouseId; public readonly bool IsInside; }
public readonly struct TownInteriorPlayerPlacementRequestedEvent {
    public readonly string OperationId;
    public readonly float WorldX, WorldY, FacingX, FacingY;
}
public readonly struct TownInteriorPlayerPlacementCompletedEvent {
    public readonly string OperationId; public readonly bool Success;
}
public readonly struct TownInteriorFadeFinishedEvent { public readonly string OperationId; }
// Máquina pura — dependências tempo/resultado são parâmetros, não globals Unity.
public sealed class TownInteriorTransitionStateMachine {
    public TownVisitPhase Phase { get; }
    public bool TryBegin(TownInteriorVisitRequest request, out string reason);
    public bool Advance(string operationId, TownVisitPhase expected, TownVisitPhase next);
    public void Fail(string operationId, string reason);
    public void Complete(string operationId);
}
```

`SceneTransitionRequest` recebe enum opt-in `SceneTransitionKind { Single = 0, TownInteriorVisit = 1 }`, HouseId e OperationId por overload novo; constructor antigo permanece Single. `SceneTransitionState.PendingSpawnId` não vira armazenamento global do endereço; estado de visita fica no port. Sem inventar ServiceLocator novo.

PlacementRequested/Completed são consumidos/publicados pelo adaptador **Player-owned**, que move Rigidbody2D/Transform e sincroniza física; o executor não chama PlayerController concreto. Usar `TownInteriorPlayerLockEvent` também em InteractionSystem e bloquear novas ações sem reativar input de uma modal anterior. `TownInteriorFadeFinishedEvent` confirma fade in ao executor; callbacks sempre comparados com OperationId. Eventos e snapshots pertencem a Foundation; SceneManagement não importa SaveDTO ou City concreto. O loader recebe referências de prefab/bounds já resolvidas pelo composition root. O SO de timing define FadeOutSeconds=.20, FadeInSeconds=.25, LoadingHintAfterSeconds=.75, TimeoutSeconds=10 e PresentationOrigin=(4096,4096), injetados, não globais.

Save proposto:

```csharp
[Serializable] public sealed class TownInteriorLocationSaveData {
    public int Version = 1;
    public string HouseId, AnchorId, ReturnAnchorId;
    public float LocalX, LocalY, FacingX, FacingY, SafeTownX, SafeTownY;
}
// Campo ADITIVO em GameSaveData:
public TownInteriorLocationSaveData TownInteriorLocation;
```

HouseId vazio/null = exterior. Não salvar Transform, Sprite, root prefab, Coroutine, operationId transitório, coordenada global 4096 ou cena aditiva como cena de mundo. Enquanto dentro, `CurrentSceneName` permanece TownScene; `Player.PlayerPosition` grava **pocket exterior seguro** para compatibilidade/fallback. Posição real interior fica exclusivamente nos floats locais da nova seção. Dados de NPC em S03, sem inventar lista paralela aqui.

## 17/20. Edições e pseudocódigo

1. `SceneTransitionRouter.Execute`: branch opt-in antes de `SetPendingSpawn` e eventos globais. Validar que Town runtime da visita está registrado, HouseId conhecido e sistema ocioso; publicar request correlacionado. Requests Single seguem caminho atual. O host também verifica Single começando para cancelar visita/limpar conteúdo com segurança; morte vence visita.
2. `SceneFadeOverlayController`: adicionar handlers de blackout/placement para modo visita. Token único impede callbacks antigos de interromper fade novo. Preservar fallback legado para Single e unsubscribe simétrico.
3. `TownInteriorVisitController`: ligar ports/scene root explicitamente no installer; nenhuma busca global. Guardar posição/câmera/contexto de origem para rollback; await blackout, loader, spawn check; liberar só a própria trava num finally.
4. `SaveManager.RegisterProviderDescriptors`: registrar provider `town_interior_location`; `SaveGame`: snapshot atômico do estado **committed**, substituir PlayerPosition pelo safe exterior quando HouseId não vazio. Durante transição, recusar save manual com feedback; autosave é adiado uma vez para Finished (não gravar mistura). Não alterar economia ou slots.
5. `SaveManager.ApplySaveData`/`LoadSceneAndApplySaveData`: restaurar relógio/serviços primeiro, manter Player no safe exterior sob fade/lock e emitir restore da localização **após** boot Town/provider pronto; só publicar finalização visual após callback da operação. Não restaurar posição 4096 primeiro e tentar corrigir depois. Save sem seção segue caminho legado.
6. Conteúdo inválido: fallback por ReturnAnchorId validado, depois spawn Town padrão registrado; registrar reason. Se nem o spawn seguro existe, interromper restore com erro recuperável/menu, nunca aceitar origem (0,0) por padrão.

```text
on crossing(request):
  validate idle, direction, open door, policy, modal/death state
  lease player lock; retain committed source snapshot
  request blackout(token); await matching black-ready
  await additive scene + selected prefab; validate house ID and candidate spawn
  place through Player-owned adapter; bind bounds/context; sync physics
  commit house/local location; emit placement-ready
  await fade-in; release own lock; finish
on any failure:
  ignore late callbacks of cancelled token
  restore original safe position/camera/context; unload only owned content
  fade in origin; release own lock; failure feedback
```

## 18/19. Arquivos permitidos/proibidos

Piloto técnico S02: `CreateMvpTownScene.PrepareTownInteriorInfrastructure` pode materializar somente a cena aditiva, config e `Assets/_Game/Prefabs/City/Interiors/House_Residential_2.prefab` (marker/bounds/anchors/saída da Renko conforme S01). O prefab técnico não é interior artístico aprovado nem rollout S04; preservar assets existentes e TownScene, mantendo ativação pública OFF. O teste de load/unload não comprova travessia da porta ou circulação com móveis.

Restore: permitido `SceneManagement/TownInteriorRestoreCoordinator.cs` para separar a seleção/coordenação da restauração do controlador de visita, sem importar Save/City/Player concretos. Direção atual vem de porta read-only `IPlayerFacingSource` em Foundation, implementada pelo PlayerController; installer injeta a leitura. Não cria outro runtime global.

Complementos necessários aos contratos acima: `SceneManagement/TownInteriorInstance.cs` (marcador do prefab), `Camera/CameraFollow2D.cs` (bounds na câmera existente), `World/Scenes/SceneFadeOverlayBootstrap.cs` (aviso e botão de recuperação no mesmo Canvas) e `Localization/LocalizationStringTable.cs` (somente `ui.town_interior.loading` e `ui.town_interior.visit_failed`, `ui.town_interior.recovery_blocked` e `ui.town_interior.retry`, feedback recuperável de TR04). Não autoriza nova câmera, Canvas raiz ou framework de localização.

Permitidos: arquivos CREATE/MODIFY acima; `Assets/_Game/Scenes/TownInteriorScene.unity` e meta pelo generator; `Assets/_Game/Data/City/Interiors/TownInteriorTransitionConfig.asset`; `Assets/_Game/Tests/EditMode/SceneManagement/TownInteriorTransitionTests.cs`, `Assets/_Game/Tests/EditMode/Save/TownInteriorLocationSaveTests.cs`, `Assets/_Game/Tests/PlayMode/Composition/TownInteriorVisitPlayModeTests.cs`; nova cena no build list somente pelo Editor API e inclusão pontual revisada. `.specs`/reports específicos.

Proibidos: cenas Farm/Cave, DTO Cave, Packages/asmdefs sem revisão separada, reordenar registry de providers não envolvidos, recriar managers ou alterar tarifas/estoques. TownScene só opt-in de piloto e só por generator; sem rollout S04 ou YAML manual. Mudança em Build Settings limitada à cena nova, sem remover cenas existentes.

## 14/22. Aceite binário / validação esperada

| AC | Teste/DoD futuro, não executado na autoria |
|---|---|
| TR01 | `OpeningWithoutCrossing_DoesNotLoadInterior`: 10 interações sem crossing → 0 loads; `MultiColliderCrossing_StartsOneVisit` → uma operação. |
| TR02 | `Visit_BlackBeforeMove_ReadyBeforeFadeIn`: timeline mostra alpha≥0,99 antes de trocar posição; zero frames em exterior visto de dentro; mesmo Player/câmera/managers InstanceId antes/depois. |
| TR03 | `Return_DoesNotRetriggerAndIgnoresClosedEntryHours`: retorno no anchor da casa correta, corpo livre, 0 reentradas até sair/recruzar; saída funciona após fechar. |
| TR04 | `LoadFailure_RollsBackAndReleasesOwnLock`, `LateCallback_CannotCommitCancelledVisit`: conteúdo ausente/timeout/cancelamento recuperam origem; zero locks/loads órfãos e UI utilizável. |
| TR05 | `LegacySave_DefaultsToExterior`, `InteriorSave_RoundTripsHouseAndLocalPose`, `UnknownHouse_FallsBackSafely`: save antigo sem seção não muda; interior novo restaura identidade/pose; antigo playerPosition é safeTown, não offset físico. |
| TR06 | `SaveDuringTransition_IsDeferredUntilCommit`: autosave grava uma vez após Finished; manual informa indisponibilidade; nenhuma mistura dos dois espaços. |
| TR07 | `Visit_DoesNotResetTimeStockInventoryOrQuests`: snapshots de inventário/gold/stock/quests e relógio têm continuidade conforme elapsed normal; nenhuma restauração/new game oculta. |
| TR08 | `SingleTransitions_RemainUnchanged`: smoke Farm↔Town e Town↔Cave usa router legado, sem reset de run/snapshots; somente fluxo, não modificar Cave. |
| TR09 | Player build: cena incluída, entrar/sair/load-save funciona fora do Editor (sem EditorSceneManager fallback); vídeo+timeline com cold/warm tempos medidos e sem corte de câmera. |

EditMode filtros `TownInteriorTransitionTests;TownInteriorLocationSaveTests`; PlayMode `TownInteriorVisitPlayModeTests`, runners existentes em tools/unity, paths `Logs/town-interior-transition-*.xml/.log`: failed=0, nenhuma seleção vazia. Report `docs/validation/town_interiors_hybrid_v1/S02.md` mapeia TR01–09, versão/hashes, erros preservados, bloqueios NOT RUN. Cobertura de Player/input não pode ser atribuída a Camera.Render.

# /speckit.tasks

- [x] TR-T01 — auditor reconfirma reuso, port de Player, dependências e save-order; contratos revisados, sem inferir save runtime validado (TR05/08).
- [x] TR-T02 — máquina/contratos/config e request/router opt-in; oito testes focados PASS (não comprova TR01/04 físico).
- [ ] TR-T03 — loader aditivo, root, overlay, Player lock e thresholds só no piloto Renko (TR01–04/07).
- [ ] TR-T04 — provider/save restore ordenado e compatibilidade (TR05/06), sem expor 4096 no save legado.
- [ ] TR-T05 — evidência PlayMode e Player, regressão router/boot (TR01–09); só então disponibilizar contrato a S03.
- [ ] TR-T06 — auditoria independente + relatório/finish-spec; feature permanece desativada em produção até S03/S04.

## 23. Falhas reais a tratar

Destino/HouseId inexistente; mudança de casa durante load; saída ocupada por NPC; jogador morre durante fade; dois colliders de limiar; save solicitado no blackout; jogo fechado durante autosave adiado; cena nova fora do build; callback de sceneLoaded prematuro; Domain Reload desativado mantendo guard; Pause/Menu aberto ao interagir. Cada caso tem estado terminal seguro: rollback ou save anterior válido, sem apagar slot ou teletransportar NPC para fabricar passagem.
