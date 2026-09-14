# SPEC — materialização, arte e aceitação das 24 construções

**Spec ID:** spec_town_interior_materialization_v1 (S04)  
**Revisão:** 1.0 / 2026-09-12  
**Status:** READY_FOR_IMPLEMENTATION — execução autorizada em 2026-09-12 após dependências; sem geração de arte paga automática  
**Wave:** TOWN_INTERIORS_HYBRID_V1 (lote local)  
**Type/Domain/Priority:** Editor + Data + Art + Validation / City / P1  
**Executor:** Claude ou Codex; um executor de integração, um reviewer independente  
**Parallelizable:** CONDITIONAL — arte por prefab após contratos; TownScene/generator/Unity sob um owner  
**Repo lock scope:** generator Town, cena TownInteriorScene, prefabs/arte interiores e validação da classificação.  
**Ordem de execucao:** 4 de 4  
**Depende de:** [S01](spec_town_interior_catalog_v1.md), [S02](spec_town_interior_transition_save_v1.md), [S03](spec_town_interior_npc_commerce_v1.md).  
**Bloqueia:** aceite integrado do novo modelo e replicação artística final.  
**Scope:** 24 destinos, três pilotos, conteúdo programado, portas/arte/física/anchors, evidência integrada.  
**Out of scope:** redesenhar toda a cidade/fazenda, aumentar fachadas indiscriminadamente, inventar NPC/serviço/loot, fechar a fidelidade global da keyart.  
**Validation level alvo:** PLAYMODE + Player + revisão visual/humana; nesta autoria NOT RUN.

required_adrs: []
<!-- Prioridade humana de 2026-09-13: materializar agora somente Renko com NPC existente, sem esperar todos os gates globais S02/S03. Preservar pendências e gate dos três pilotos; não replicar para outras casas nesta exceção. -->
required_game_rules: [city_rules, event_rules]

# /speckit.specify

## 5–9. Contexto e Phase 0

As capturas `docs/validation/town_room_repair_20260912_run2` mostram piloto GateKeeper6×4 com três móveis. Sete testes e nove waypoints no relatório anterior não são aceite de habitabilidade ou desta mudança de arquitetura. A Town atual usa `CreateMvpTownScene` e `TownKeyartComponentPhysics.ApplyMeasuredSolids` para físicos finais; `TownKeyartDoorClassification` espera14 walk-in +9 shop-hours +1 exterior. A transição nova invalida esse literal para lotes migrados, **não** os checks de colisão/rotas.

Fontes: S01–03; `Editor/SceneCreation/CreateMvpTownScene.cs`, `City/{TownKeyartInteriorPresentation,TownKeyartComponentSupportCatalog,TownKeyartComponentPhysics,TownKeyartDoorClassification}.cs`, `Editor/Validation/{TownAllEntriesPlayModeProbe,ValidateTownKeyartScene,ValidateTownRouteDetourRatio}.cs`, `World/{HouseDoorInteractable,RoofRevealController}.cs`, `Editor/Art/FarmHouseInteriorAuthoring.cs` somente como precedente de autoria, sem editar Farm.

Skills obrigatórias: `pixel-art-direction`, `aseprite-authoring` quando pixels forem editados, `sprite-scene-integration`, `scene-composition-review`, `scene-physics-review`, `unity-asset-generation`, `unity-validation`; `sprite-animation-review` para portas. Antes de gerar arte, inspecionar assets reais. `chair.png` já foi usado como armário com louça: filename não comprova objeto correto.

## 10. Hierarquia e templates

Scene aditiva:

```text
TownInteriorScene
  InteriorSceneRoot                  # origem física S02; sem managers/câmera/Player
    Instance_<HouseId>               # exatamente uma instância do catálogo
      Floor
      Walls / DoorFrames
      Rooms/<RoomId>
      Props/<StablePropId>/Visual + Support
      Anchors/<StableAnchorId>
      ExitThreshold
      CameraBounds
```

`HouseId` é chave estável; `TemplateId` só reuso de desenho. Propostas de template determinísticas:

| TemplateId | Definições |
|---|---|
| temple_residence | Temple |
| civic_office | Chamber |
| detention | Prison |
| manor | Manor |
| archive_residence | Registry, Archive, Tovin (variações de área/props de S01) |
| covered_market | MarketHall |
| shop_residence | Bakery, Residential_2, Residential_3 |
| inn_tavern | Inn |
| industrial_residence | Blacksmith, AlchemyLab, Workshop, Tannery, Fishery |
| home_single | Residential_4, Residential_1, Dagna, Pip |
| home_shared | CarvalhoTorto |
| annex_home | GateKeeper, AnimalYard |

Total12 templates/24 definições; não fabricar24 cenas copiadas. Templates podem compartilhar paredes/kit, mas cada definição instancia programa/área de S01. Alterar template não muda cama, owner ou estoque de outra casa.

## 11. Produção em duas etapas

**Pilotos obrigatórios:** Renko (loja+moradia), CarvalhoTorto (2 Orcs), GateKeeper (posto cutaway + portal privado). Nenhum rollout das21 restantes até revisão de escala/uso nos três.

1. Blockout com props existentes aprovados, planta clara e actor profiles finais. Mostrar screenshot gameplay + planta anotada com circulação e footprint dos móveis. Não gerar pixel art nova para consertar planta errada.
2. Após planta aceita, curadoria/arte dos slots em falta e composição definitiva. Portas/parede/fundo e objetos com função real. Uma rodada coesa, no máximo3 discrepâncias prioritárias. Após2 rodadas sem melhora, parar a abordagem e reapresentar método, não continuar gastando em rerender igual.

Renko piloto precisa sala de venda/estoque/copa/quarto; CarvalhoTorto precisa duas camas/armários e cozinha/refeição; GateKeeper precisa posto externo + aposento privado, não só retirar cama da guarita. Arte “provisória” pode habilitar desenvolvimento técnico, mas não cumprir gate visual final.

## 12. Critérios de composição e uso

- Fundo com face de parede; laterais finas mas visíveis e com volume/coerência; frente cutaway dentro da cena interior. Divisórias devem comunicar ambientes sem esconder passagem. Piso não ultrapassa parede, cantos se encontram e nenhuma linha atravessa o meio da sala sem função.
- Porta de entrada comunica espessura/soleira, folha animada e sentido da travessia. Não aceitar oval preto isolado como porta final. Fachada não fica com duas portas pintadas (sprite antiga+folha nova), nem telhado some ao entrar em casa Portal.
- Personagem e móveis compartilham perspectiva, pixel clusters, luz e escala. O mobiliário pode variar por raça do dono sem reduzir o corredor do Player. Imports Point/None/noMipmaps e alpha verificado não bastam para PASS artístico.
- Áreas de trabalho próximas ao atendimento; cama/cozinha separadas de forno/reagentes/tinas. Objetos visíveis explicam profissão e uso, não preenchimento aleatório. Conteúdo obrigatório e contagem de camas em S01 não podem ser apagados para passar física.
- Classificar cada objeto D/S/E; piso/tapete sem collider sólido, armário/mesa/cama bloqueiam **base**, paredes/frente cutaway mantêm barreira física correspondente. Superfície elevada não bloqueia pela silhueta completa. Raiz+filhos contam como união efetiva, não aprovar pelo BoxCollider menor.
- Posicionar Player diante/atrás/lado de móveis altos e porta. Sorting usa componente de câmera já existente; cabeça não é cortada por mesa quando Player está à frente. Captura central não comprova sorting em circulação.
- Nenhum inventário de propriedade privada, alimento coletável, cura gratuita ou dormir em cama de NPC por adicionar collider/interactable genérico. Lavatório/forno privado são cenográficos neste escopo.
- Guardar propósito e motivo de cada slot na ficha: 0 props sem propósito, 0 ativos interagíveis sem interação autorizada, 0 moradias sem as necessidades de S01.

# /speckit.plan

## 13/15–16. Tipos e APIs propostos

Reutilizar CreateMvpTownScene, WorldSpriteLibrary, catalog/recipes de suporte, door/reveal, scene-loader/ports S02 e NPC/commerce S03. Não criar nova API de interação/physics/save.

CRIAR:

```csharp
// Editor/SceneCreation/City/GenerateTownInteriorScenes.cs
public static class GenerateTownInteriorScenes {
    public static void GeneratePilots(); // Renko, CarvalhoTorto, GateKeeper, sem rollout implícito
    public static void GenerateAll();    // 24, somente após pilot gate
}
// Editor/Validation/ValidateTownInteriorInstances.cs
public static class ValidateTownInteriorInstances {
    public static void ValidatePilots();
    public static void ValidateAll();
}
```

Gerar prefab em `Assets/_Game/Prefabs/City/Interiors/<HouseId>.prefab`; cada um recebe componente marcador já definido/implementado pelo loader S02 com HouseId, bounds, lista explícita de renderers/colliders/anchors. Se S02 não forneceu marker, criar nesta spec `SceneManagement/TownInteriorInstance.cs` com `public string HouseId`, `public Transform ContentRoot`, `public Transform[] Anchors`, `public Collider2D CameraBounds`; é referência Unity de cena, nunca save DTO. `InteriorPrefabKey` de S01 resolve via catálogo carregado, sem `Resources.Load` por filename arbitrário.

Art candidates editados em Aseprite ficam em `Assets/_Game/Art/Source/TownInteriors/`; exports aprovados `Assets/_Game/Art/Generated/World/town_interiors/`. Confirmar convenção da skill antes de criar esses paths; se ela exige outra raiz, atualizar blueprint e manifesto antes de escrever, sem duplicar origem. Não editar assets externos compartilhados de Farm/NPC para acomodar este kit.

## 17/20. Edições exatas e rollout

1. Generator novo lê catálogo e faz preflight de todos IDs/refs antes de materializar. Regerar por HouseId com GUIDs mantidos; backup/salvar setup do usuário; falhar se cena dirty de outro dono. Sem escrever YAML.
2. Criar planta dos3 pilotos seguindo S01, props footprint/posição calibrados. Emitir manifest `HouseId,RoomId,PropId,Purpose,Role,SourceAsset,Hash,PPU,Pivot,Scale,WorldBounds,EffectiveColliders,OwnerId,UseAnchorId` por instância e imagens annotated separadas da captura real.
3. `CreateMvpTownScene.CreateWalkInHouse`/bloco de furniture/reveal: branch por modo do catálogo **após aprovação do amendment**. Portal conserva shell/fachada/telhado, remove só piso/móveis/anchors internos legados daquela casa e instala threshold/return anchors. Cutaway GateKeeper conserva posto; AnimalYard conserva recinto. Nenhuma remoção de House_* root, NPC, ShopId, exterior stall ou rua.
4. `TryAddCraftingStation` migra6 estações para o prefab correto, preservando ID real `station_<houseName.ToLowerInvariant()>` e tipo. Nunca deixar estação ativa em exterior e interior simultaneamente. Preservar `GuestBed_Inn` e gate de pagamento; add GuestBed_Inn_02 pelo mesmo sistema, sem freeSleep.
5. Materializar suportes somente depois da apresentação final. Aplicar recipes aprovadas também aos filhos; validar união e passagens. Não usar `TownKeyartInteriorPresentation.ApplyCandidate` global para reposicionar NPC-moradia que pertence a S01 sem revisar sua compatibilidade.
6. `TownKeyartDoorClassification` passa a dois eixos: EntryMode de S01 + acesso de S03; shop_hours não é terceiro modo de carregamento. `ValidateTownKeyartScene` e `TownAllEntriesPlayModeProbe` escolhem checks pelo modo, sem reduzir total de24 lotes. Provar22 portais de fachada +2 anexos e os2 acessos externos preservados; não apagar check porque RoofReveal deixa de existir em Portal.
7. Atualizar rotas/return pockets com todos sólidos finais. Revalidar162 pares de anchors externos (baseline) e28 residentes; se número mudar, justificar por bindings movidos para espaço lógico e conservar cobertura equivalente por NPC, não reduzir testes silenciosamente.
8. Após aceite dos3 pilotos, GenerateAll cria restantes21 e censo24. Relatórios por casa, sem copiar screenshot de Renko para todas. Unload/reload de template entre owners deve provar que nada vaza.

## 18/19. Permissões de implementação

Permitidos: novos generators/validators/marker listados, `Assets/_Game/Prefabs/City/Interiors/`, assets/SOs S01, cena TownInteriorScene e TownScene via Editor; fontes de classificação/probe/generator citadas; arte candidata/export nos paths acima; `Assets/_Game/Tests/EditMode/City/TownInteriorInstanceTests.cs`; `Assets/_Game/Tests/PlayMode/Composition/TownInteriorAcceptancePlayModeTests.cs`; `docs/validation/town_interiors_hybrid_v1/`.

Proibidos: Farm/Cave/assets de personagem, preços/receitas/serviços, save/contratos S02–03 fora de correção aprovada pelo owner, Packages/ProjectSettings salvo inclusão da cena já autorizada S02, apagar exterior para “ganhar espaço”, alterar escala global/zoom para fabricar proporção. Se houver interseção com trabalho paralelo, source freeze antes de compilar, não só exclusividade de Unity.

## 14/22. Critérios binários e evidência

| AC | Resultado/DoD esperado depois de implementar |
|---|---|
| MAT01 | `ValidatePilots` imprime `TOWN_INTERIOR_PILOTS: PASS instances=3 missingRooms=0 missingResidents=0`; humano/independente aprova planta antes do rollout. |
| MAT02 | `ValidateAll` imprime `TOWN_INTERIOR_INSTANCES: PASS definitions=24 loadedMax=1 unknownIds=0`; 24 prefabs com owner/programa correto; `Generator_IsIdempotentAndPreservesGuids` confirma segunda geração sem mudança semântica. |
| MAT03 | `ResidentPrograms_HaveBedsKitchenStorageAndWash` confirma23 camas pessoais+2 hóspedes,0 moradores perdidos; conta prisão como institucional e curral como exterior+anexo. |
| MAT04 | `AllEffectiveSolids_MatchSupportAndRoutes` censo completo; PlayMode percorre entrada→atendimento→uso→moradia permitida→saída de24 destinos, corpo real, NPCs presentes. Todos paths previstos executados, sem false PASS de cast. |
| MAT05 | Evidência visual por24 destinos: gameplay-scale, close-up nativo, mesma escala do actor, planta/colliders em imagem separada; no mínimo3 poses de oclusão por interior e vídeo de porta/transição. Revisor rejeita quebra de perspectiva/pixel density/limite/função sem fazer média. |
| MAT06 | `OriginalStationAndServiceIds_ArePreserved` encontra6 estações únicas,8 serviços pessoais +2cívicos,23shops e28NPCs, sem novo bootstrap/managers/câmera no conteúdo. |
| MAT07 | `HybridExteriors_RemainWalkableAndOwned`: posto de Alaric/curralEiran visíveis, casa privada acessível, todos moradores em anchors válidos; nunca roof sobre recinto animal. |
| MAT08 | `AllAddresses_ReturnToMatchingDoor` e save/load alternado Renko→Mirela→Renko preservam retorno/owner/stock, sem herdar posição ou cômodo do template anterior. |
| MAT09 | Regressão de exteriores: snapshots dos24 lotes/28 NPCs/rotas, sem rua/fachada deslocada não autorizada; `ValidateTownRouteDetourRatio.Validate` termina Errors=0 para cobertura pertinente, sem apagar pares. |
| MAT10 | Cena no Player build, input real,10 ciclos de entrada/saída em cada piloto, nenhum leak de instância/lock/subscriber; report de memória estabiliza sem crescimento por ciclo após warm-up. Medir, sem inventar orçamento de hardware. |

Runner futuro EditMode `TownInteriorInstanceTests`; PlayMode `TownInteriorAcceptancePlayModeTests` em ferramentas existentes. Expected XML failed=0, skipped obrigatórios=0; relatório por AC. Imports verificados no asset real. Audio/overlay/input precisam runtime/Player, Camera.Render não é evidência desses componentes.

Entrega `docs/validation/town_interiors_hybrid_v1/index.html` com24 cards (nome,owner/função,antes/depois,planta,poses,logs,status), `S04.md`, manifest porHouseId, inputhashes e falhas preservadas. Abrir galeria no navegador e fornecer imagem local inline ao humano. Render aprovado por agente não é aprovação humana nem percentual de fidelidade à keyart exterior.

# /speckit.tasks

- [ ] MAT-T01 — inventário de arte/plantas3 pilotos, fits e manifestos (MAT01/03/05).
- [ ] MAT-T02 — materializar pilotos via generator, porta/câmera/física/anchors e6 interações legadas quando presentes (MAT01/04/06/07).
- [ ] MAT-T03 — teste e revisão independente/humana dos3; corrigir até fechar critérios ou mudar abordagem após2 rodadas sem melhora (MAT01/04/05/08/10).
- [ ] MAT-T04 — materializar21 restantes com ownership de prefab separado; integração Town serializada (MAT02–09).
- [ ] MAT-T05 — execução consolidada Unity/Player,24cards e mapa de falhas (MAT02–10).
- [ ] MAT-T06 — `finish-spec`, aprovação humana e reconciliação do amendment/status antigos; não encerrar specs de fidelidade global fora do escopo.

## 23. Bordas e bloqueadores

Template altera outro owner: isolamento/ID inválido, reprovar. Porta pintada duplicada: corrigir fonte/camadas, não cobrir de preto. Móvel com colisores filhos excedentes: corrigir união/materialização, não só root. Orc não passa: aumentar circulação/planta. Geração tenta sobrescrever cena dirty: abortar com backup/setup intactos. Fonte sprite falta: candidate rotulado, gate visual FAIL. Entrada/retorno bloqueados com NPC: diagnosticar contatos/replanejamento sem desativá-los. Arte aprovada mas planta inabitável: reprovar MAT03/04. EditMode verde com imagem incoerente: MAT05 FAIL e rollout bloqueado.
