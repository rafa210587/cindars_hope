# SPEC — moradores entre espaços e comércio sem duplicação

**Spec ID:** spec_town_interior_npc_commerce_v1 (S03)  
**Revisão:** 1.0 / 2026-09-12  
**Status:** READY_FOR_IMPLEMENTATION — execução autorizada em 2026-09-12; dependências e revisão independente permanecem obrigatórias  
**Wave:** TOWN_INTERIORS_HYBRID_V1 (lote local)  
**Type/Domain/Priority:** Runtime + Data + Save / NPC, City, Economy / P0  
**Executor:** Claude ou Codex, integração e auditoria de save/agenda  
**Parallelizable:** NO para runtime; fixtures podem ficar com o mesmo implementador  
**Repo lock scope:** NPC/Schedule, NpcManager/SaveData/NpcShopController, City/Services e generator de anchors; sem concorrência com S02 no SaveManager/installer.  
**Ordem de execucao:** 3 de 4  
**Depende de:** [S01](spec_town_interior_catalog_v1.md) e [S02](spec_town_interior_transition_save_v1.md).  
**Bloqueia:** rollout/aceite S04.  
**Scope:** residência e circulação entre exterior/interior, estoque único, atendimento/access e migração dos anchors.  
**Out of scope:** novos NPCs, lojas, preços, receitas, romance/crime, simular NPC fora da Town em outros mapas.  
**Validation level alvo:** PLAYMODE + save compatibility; nesta autoria NOT RUN.

required_adrs: []
<!-- Prioridade humana de 2026-09-13: executar agora somente o piloto Renko (mobiliário + NPC existente), em paralelo delimitado com S04. Pendências S02 continuam PARTIAL; esta exceção de ordem não autoriza rollout nem dispensa validação afetada. -->
required_game_rules: [city_rules, event_rules]

# /speckit.specify

## 5–9. Fontes e estado auditado

`NpcScheduleService.RegisterProfile/RegisterAnchor/ResolveAllNpcs/ResolveNpc` usa listas de controllers e um `NpcTownRouteGraph`. `NpcScheduleBlockResolver` define Shopkeeper Work09–18/Social18–22, Guard Work06–22, Night Work20–02, Wanderer Social08–20. **Patrol mapeia Guard mesmo se profissão é carpinteiro/mineradora**; preservar perfil atual, não inferir horário pela descrição.

`NpcManager.CaptureSaveData` percorre listas de controllers; `NpcSaveData` contém NpcId/SceneId/Position/HasMet. Não há localização por HouseId. `NpcsSectionProvider` captura com cena ativa Town. `NpcShopController` usa o ShopManager/facade/UI e disponibilidade existentes. `NpcServiceCatalog` tem oito serviços já atribuídos. Gate de porta atual aplica Shopkeeper09–22 a nove prédios, não ao local efetivo de cada comerciante. `TownCityLayout.AllNpcPlaces` dá 28 lugares, 23 NPCs têm ShopDataPath.

Fontes obrigatórias: S01/S02, `Assets/_Game/Scripts/NPC/{NpcManager,NpcManagerSaveData,NpcShopController}.cs`, `NPC/Schedule/{NpcScheduleService,NpcScheduleBlockResolver,NpcScheduleAnchor,NpcScheduleProfile}.cs`, `City/Services/TownShopDoorHoursPolicy.cs`, `Editor/SceneCreation/City/TownShopDoorHoursWiring.cs`, `CreateMvpTownScene.cs` blocos de homes/anchors/shop binding, `NPC/Services/NpcServiceCatalog.cs`.

Reconfirmar IDs/perfis/ShopData antes da execução. Skills: `npc-dialogue-authoring`, `state-machine-design`, `event-bus-pattern`, `save-load-pattern`, `scene-interactable-wiring`, `scene-physics-review`, `non-regression-review`. Não reescrever agenda diária nem criar NpcManager2.

## 10. Venue principal de cada fornecedor (23/23)

`shop_*` abaixo referencia os assets existentes `Assets/_Game/Data/Economy/Shop_<Nome>.asset`; validar Id real do asset, nunca regenerar por inferência se divergir. “Interno” é local principal de trabalho, não segunda instância do NPC.

| NPC | ShopId | Atendimento principal | Por quê |
|---|---|---|---|
| npc_corvus | shop_corvus | Temple, ala pública | Ofertas/consulta religiosas, não altar funcional novo. |
| npc_mara | shop_mara | Registry, balcão | Registro civil com arquivo próximo. |
| npc_tovin | shop_tovin | Tovin, escritório | Licenças em seu endereço atual; não fundir com Mara. |
| npc_sylveth | shop_sylveth | Exterior, banca de sementes | Compra recorrente no loop Farm sem entrada obrigatória. |
| npc_renko | shop_renko | Residential_2, balcão | Loja geral com estoque cenográfico e moradia atrás. |
| npc_mirela | shop_mirela | Residential_3, ateliê | Atendimento junto ao tear. |
| npc_orlan | shop_orlan | Inn, recepção | Hospedagem/refeição; prato do dia continua de Orlan. |
| npc_gruta | shop_gruta | Inn, balcão da taverna | Cozinha/banho e salão noturno. |
| npc_brumdar | shop_brumdar | Blacksmith, balcão | Ferramentas e reparo junto à forja. |
| npc_dagna | shop_dagna | Exterior, rota da pedreira | A profissão ocorre fora da moradia. |
| npc_hund | shop_hund | Exterior, posto/rota atual | Não converter a casa compartilhada em loja. |
| npc_thalindra | shop_thalindra | Archive, mesa de consulta | Pesquisa/análise no arquivo. |
| npc_pip | shop_pip | Exterior, rota da entrada | Mensageiro permanece circulando. |
| npc_nimble | shop_nimble | Workshop, balcão | Carpintaria junto à estação existente. |
| npc_gurd | shop_gurd | Exterior, posto de construção | Serviços de obra fora da casa. |
| npc_yael | shop_yael | Exterior, mercado noturno | Identidade noturna e encomenda de livro preservadas. |
| npc_zrix | shop_zrix | Exterior, estrada da caverna | Preparação/contrato de caça sem entrar numa casa nova. |
| npc_savra | shop_savra | Exterior, rota de ervas | Venda e coleta junto à floresta. |
| npc_ozzra | shop_ozzra | AlchemyLab, balcão | Reagentes e identificação de relíquias. |
| npc_eiran | shop_eiran | Exterior, curral | Animais/ração/pasto premium no local de uso. |
| npc_sael | shop_sael | Exterior, cais | Venda de pesca e acesso ao lago. |
| npc_mella | shop_mella | Bakery, balcão | Pães na frente da padaria. |
| npc_hess | shop_hess | Tannery, balcão | Couro/barris junto à oficina atual. |

Treze internos em doze endereços; dez externos. Alaric, Velorin, Maelor, Liora e Tibbet não ganham loja. As 23 bancas NPC existentes podem permanecer como exposição/sinalização dos provedores internos, mas **não** instanciam vendedor/UI adicional. As seis bancas de mercado não ganham seis ShopIds fictícios. Placa do comércio indica owner, local atual/horário e fechado/ausente com feedback, não loja remota.

Preservar também os dois serviços de `CityServiceCatalog`: `service_tovin_market_stall`/`license_market_stall` e `service_mara_farm_registry`/`contract_farm_registry`, flags,200g/100g e efeitos atuais. O censo é oito serviços pessoais **mais dois cívicos**, não “oito serviços totais”. Assets legados `Shop_General_Store`, `Shop_Blacksmith`, `Shop_Cave_Supplies`, `Shop_Seeds_Tools` e outros não ligados ao roster atual não entram como novos vendedores nem são apagados por esta spec.

## 11. Agenda, acesso e horário — políticas explícitas

- `Work`: fornecedor interno caminha até porta exterior, atravessa e vai ao balcão interno; externo mantém rota/posto. `Home/Night`: vai ao quarto de S01, não fica na porta/fachada.
- `Social`: conservar anchor canônico externo, exceto Orlan/Gruta, cuja socialização proposta é no salão da própria Inn (salão aberto, não quartos). Esta é mudança de destino explícita, não de disponibilidade/horário. O atendimento que hoje é permitido em Social continua disponível ao encontrar o **mesmo NPC**, inclusive na praça; não criar catálogo diferente fora do balcão.
- Horário do balcão = intervalo de bloco no venue + disponibilidade canônica. Porta comercial pode abrir no intervalo de atendimento mesmo durante a curta chegada do funcionário; placa informa “Atendente a caminho/ausente”, sem venda a distância. Check de transação exige NPC presente, disponível e interação no mesmo espaço.
- Não generalizar toda porta para09–22: Nimble é Patrol, por exemplo. Derivar informação da agenda real e da atribuição de venue. Não mudar BlockResolver para caber numa placa.
- Loja fechada: entrada Player negada com horário/localização e saída irrestrita. Casas mistas de acesso único não oferecem bypass “Visitar moradia” que viole Rule4; acesso residencial do próprio NPC continua válido. Residências puras recebem visita06–22. Exceção proposta da Inn é somente lobby/salão24h, com balcões/áreas de serviço fechados quando indisponíveis; precisa constar no amendment. Sem chave/amizade/gold novos.
- Áreas privadas têm separação física e rótulo; aposentos de outro morador são visitáveis06–22 nesta proposta, não sistema de invasão. Home/Night não passam a oferecer diálogo/loja automaticamente; conservar gate atual. À noite entrada residencial recusada, quem já está dentro pode sair a qualquer hora.
- Temple06–22, Chamber/Prison público09–18, MarketHall06–22; celas sempre fechadas para Player, sem chave/sistema penal. Inn lobby/salão24h, NPCs não trabalham dormindo; camas de hóspedes usam pagamento existente. Sair do banho/quarto não exige NPC disponível.
- Residente pode acessar sua moradia fora da janela de visita, com identidade validada; Player não recebe bypass de NPC. Política de porta identifica lado/ator, não usa um bool global aberto que desbloqueia todos.
- Se fechamento ocorre durante modal/transação: concluir/cancelar atomicamente pelo fluxo existente antes da partida, nunca cobrar duas vezes nem fechar a loja no meio de commit. Depois disso NPC segue agenda; saída não é bloqueada.

## 12. NPC único e espaços não carregados

Identidade e progresso são lógicos, não quantidade de renderers. Existem 28 registros canônicos e no máximo uma apresentação por NpcId. Town permanece simulada; só um interior é apresentado.

- Manter controllers/registro originais, sem clonar prefab por visita. Para NPC dentro de casa não carregada, suspender **apresentação/física/interação** por adaptador, mantendo identidade/agenda/HasMet/snapshot. Não retirar o NPC do ledger nem salvar posição de “estacionamento”.
- Ao carregar a casa em que está, apresentar o mesmo NPC no anchor local ou posição local válida. Ao descarregar, capturar estado lógico antes de suspender apresentação.
- Não mover corpo pela distância de4096u. Planejar rota em segmentos: caminho Town até limiar → passagem da porta → mudança de espaço → caminho local. O grafo externo não deve ganhar aresta reta cruzando mapa inteiro até o root interior.
- NPC observado atravessa a porta com animação/colisão: não some no meio da rua, nem aparece instantaneamente na mesa. Ao sair de interior descarregado, materialização ocorre no lado oculto da porta e só é visível após abertura/passagem; pocket ocupado espera/replaneja.
- Progresso offscreen não é permissão para salto arbitrário: conservar estado EnRoute, destino e tempo restante do segmento, baseado no caminho e velocidade existentes. Se a porta/destino fica bloqueado, manter espera/retry limitado e registrar falha. Não alterar relógio para “chegar a tempo”.
- Ao assistir ao mesmo interior, Player e NPC usam colisão real. Posição local inválida no load: anchor de uso livre; caso sem anchor seguro, NPC fica em estado pendente fora da apresentação e relatório FAIL, não sem collider.
- Uma ausência à noite não prova NPC removido: censo deve informar LogicalCount/PresentedCount/SuspendedCount e owner de cada casa.

# /speckit.plan

## 13/15. Reuso e responsabilidades

`NpcScheduleService` continua dono de resolução de blocos/ordens; `NpcWanderer` e `NpcTownRouteGraph` continuam movimento externo; `NpcManager` continua save/HasMet; `ShopManager` continua stock; `NpcShopController` continua UI/transações. Não criar outra agenda ou sistema de comércio.

CRIAR em `Assets/_Game/Scripts/NPC/Schedule/`:

- `NpcSpaceLocation.cs`: valor puro HouseId/Anchor/local pose; vazio=Town.
- `NpcInteriorRoutePlanner.cs`: monta segmentos de uma ordem existente; não escolhe o horário.
- `NpcInteriorPresentationAdapter.cs`: ponte de visibilidade/collider/interação para o controller registrado; sem identidade/progresso próprios.

CRIAR em `Assets/_Game/Scripts/City/Services/`:

- `TownServiceVenuePolicy.cs`: decisão pura de presença/venue/bloco, mensagens por reasonId.
- `TownBuildingAccessPolicy.cs`: implementação de política de acesso por ator/lado/finalidade; reutiliza o contrato opt-in da porta, delegando a decisão pura.

MODIFICAR: NpcScheduleService, NpcScheduleAnchor/Profile apenas para vínculo de espaço, NpcManager/SaveData, NpcShopController no guard de mesma localização; generator Town de anchors/horários; `TownShopDoorHoursWiring` delega ao catálogo, remove tabela duplicada quando todos consumidores migrarem; `TownShopDoorHoursPolicy` preserva caminho legado não migrado. Não refatorar UI inteira.

## 16. Contratos propostos (não implementados)

```csharp
public readonly struct NpcSpaceLocation {
    public readonly string HouseId, AnchorId;
    public readonly float LocalX, LocalY;
}
public enum NpcInteriorRouteLeg { TownPath, DoorCrossing, InteriorPath, WaitingForSpace }
public sealed class NpcInteriorRoutePlanner {
    public NpcInteriorRouteLeg[] Plan(NpcSpaceLocation source, NpcSpaceLocation target);
}
// Adições ao NpcScheduleService; manter RegisterProfile/ResolveBlock existentes.
public bool TryGetSpaceLocation(string npcId, out NpcSpaceLocation location);
public void RegisterSpaceAnchor(string npcId, string anchorId, string houseId, float localX, float localY);
public void NotifyPresentedSpace(string houseId, bool loaded);

public readonly struct TownVenueQuery {
    public readonly string NpcId, HouseId;
    public readonly int Hour;
    public readonly bool ProviderAvailable, ProviderPresent, SameSpace;
}
public static class TownServiceVenuePolicy {
    public static bool CanTransact(TownVenueQuery query, out string reasonId);
}
```

Policies de acesso precisam ator e lado, que `IHouseDoorAccessPolicy.CanOpen(out reason)` atual não fornece. **Não quebrar essa interface**: adicionar opt-in Foundation `IHouseDoorActorAccessPolicy.CanOpen(string actorId, bool exiting, out string reason)` e consultar somente quando implementado. Porta continua proprietária de animação/safety; não transferir collider ownership para policy. Adicionar `HouseDoorInteractable` à lista permitida apenas nessa adaptação, não trocar Farm.

Save: extender **NpcSaveData existente**, sem registro duplicado em GameSaveData:

```csharp
public string InteriorHouseId;   // vazio = exterior legado
public string InteriorAnchorId;
public float InteriorLocalX, InteriorLocalY;
public string RouteTargetAnchorId;
public int RouteLeg;             // enum simples; validar faixa
public float RouteRemainingSeconds;
```

`SceneId` continua TownScene; `Position` recebe posição segura exterior enquanto interior lógico existe, não4096. `HasMet`/amizade/shop stock continuam nas seções originais. Legacy sem campos: usar exterior/agenda atual, preservando HasMet. Mudança de dia resolve bloco pela fonte restaurada; não reaplicar orçamento de rota antigo a destino incompatível.

## 17/20. Plano por edição

1. `CreateMvpTownScene.TownNpcHomes`/criação de anchors: gerar bindings de espaço a partir de S01; conservar IDs `npc_<id>_home/work/social`, acrescentar HouseId/coordenadas locais. Não registrar dois anchors de mesmo ID; modo de migração troca vínculo, mantém alias no save.
2. `NpcScheduleService.ResolveAllNpcs`: iterar perfis canônicos/estado lógico, não apenas controllers apresentados; resolver Work/Social/Home com o resolver atual. `ResolveNpc`: segmentar se HouseId difere; rota externa usa grafo Town, interna usa anchors/obstáculos da instância. Remover dependência de faixa Y somente nos caminhos migrados.
3. `NpcInteriorPresentationAdapter`: cache de enable-state de renderers/colliders e gates, suspensão/restauração sem `SetActive(false)` do root que desregistre identidade. Cancelar MoveOrder físico ao suspender e preservar progresso lógico. Nunca desativar collider de NPC apresentado para destravar corredor.
4. `NpcManager.CaptureSaveData`: mesclar estado lógico por NpcId, validar unicidade e incluir suspensos; `RestoreFromSaveData`: restore lógico antes de apresentação, aplicar posição do espaço correto e depois HasMet; restore de stock independe do load de casa.
5. `NpcShopController` guard antes de abrir/comitar: mesmo espaço, NPC disponível, ator presente; usar IDs de reason no feedback existente. EconomyFacade/ShopManager mantêm transação/stock, nenhum asset de preço editado.
6. `TownShopDoorHoursWiring.Configure` e classificação de entrada: consumir EntryMode/acesso de S01/S03. Salvar horário de visita em dados/config, não constantes em view; renovar placa em tick/bloco e contexto de presença, sem scanning global por frame.

## 18/19. Arquivos permitidos e limites

Arquivos novos/modificados de §15–17; `Foundation/IHouseDoorActorAccessPolicy.cs`; `World/HouseDoorInteractable.cs` somente dispatch opt-in com fallback legado; `Editor/SceneCreation/City/TownKeyartDoorClassification.cs` somente classificação derivada; dados do catálogo S01 de venue/access; `Tests/EditMode/NPC/NpcInteriorResidencyTests.cs`, `Tests/EditMode/City/TownServiceVenuePolicyTests.cs`, `Tests/PlayMode/Composition/TownInteriorResidentsPlayModeTests.cs` e reports.

Proibidos: Shop_*.asset/estoques/preços, IDs/efeitos do NpcServiceCatalog, novos residents/NPCs, Farm/Cave, Packages/ProjectSettings, mudanças de relacionamento, YAML manual, restauro de posições antigas fora de contexto. Cena Town regenerada somente junto S04 e com source freeze.

## 14/22. Critérios e DoD futuro

| AC | Evidência esperada |
|---|---|
| NPC01 | `CanonicalResidents_RemainUniqueAcrossVisits` e validator imprimem `TOWN_RESIDENCY: PASS logical=28 duplicateIds=0`; 23 indoor/5 outdoor corretos, suspensos contam. |
| NPC02 | `Schedule_TraversesTownDoorAndLocalAnchor` percorre Work→Social→Home→Work com Player observando; filmar passagem, sem pop na rua, sem caminho até4096. |
| NPC03 | `UnloadedHome_ContinuesScheduleAndRestoresMetState` avança relógio com casa descarregada, reload restaura dono/agenda/HasMet e uma apresentação. |
| NPC04 | `ShopStock_IsSharedAcrossVisitAndSave` compra um item fora/inside conforme NPC real, restante se mantém; estoque não reinicia com prefab e gold não duplica. |
| NPC05 | `UnavailableOrDifferentSpaceProvider_CannotTransact` bloqueia sem cobrar; `SocialProvider_PreservesExistingTradePermission` não remove disponibilidade canônica. |
| NPC06 | `ShopClosing_PreservesExitAndAtomicPurchase` e `ResidentBypass_DoesNotUnlockPlayer`: fechamento23h/02h e corpo no vão não prendem/criam bypass geral. |
| NPC07 | `LegacyNpcSave_DefaultsSafely` e `SuspendedNpc_SaveRoundTrip` preservam23+5 registros/HasMet e posições lógicas; desconhecido não vira outro morador. |
| NPC08 | Cenário 24h acelerado controlado com **28 NPCs canônicos**, relógio atravessa00/02/06/09/18/20/22; todos os28 têm destinos/presença registrados e nenhum perdido. Registrar exceções de bloqueio, não declarar PASS por uma manhã. |
| NPC09 | Censo `TOWN_VENUES: PASS providers=23 indoor=13 outdoor=10 personalServices=8 civicServices=2`, IDs/efeitos/preços iguais ao baseline; seis estações originais permanecem usáveis. |

O requisito é **28** perfis canônicos; extra wanderer fica separado. Fixtures parametrizadas relatam expected/executed/skipped. Runner EditMode com filtros acima; PlayMode `TownInteriorResidentsPlayModeTests`, XML/logs `Logs/town-interior-residents-*`, failed=0; report `docs/validation/town_interiors_hybrid_v1/S03.md`. GUI real/filmagem para claims de fluidez, não só casts.

# /speckit.tasks

- [ ] NPC-T01 — aprovar tabela/visita/social da Inn e auditar perfis/horários atuais (NPC01/05/09).
- [ ] NPC-T02 — espaços/segmentos/anchors e adaptação de apresentação (NPC01–03); contratos S02 concluídos.
- [ ] NPC-T03 — persistência de suspensos e restore ordenado (NPC03/07); um owner do save.
- [ ] NPC-T04 — venue/access/UI feedback, estoque/serviços preservados (NPC04–06/09).
- [ ] NPC-T05 — PlayMode24h, colisões compartilhadas, legado e visita/load (NPC01–09).
- [ ] NPC-T06 — revisão independente/relatório/finish-spec; não declarar48h ou cidade inteira se teste executou um lote.

## 23. Bordas/falhas

Dois NPCs compartilhando casa não compartilham cama/ID. Mesmo NPC vinculado a dois lugares de trabalho deve falhar no catálogo, não duplicar controller. Owner socializando fora não atende balcão remoto. Perfil Home não vira Work para abrir loja. Morte/load durante visita cancela operação antes de restaurar NPC. Descarregar enquanto conversa deve aguardar cancelamento seguro. Mudança de dia invalida rota velha e resolve pela agenda, sem reset de stock antecipado. Trânsito bloqueado mantém corpo/colisor e diagnóstico; não apagar NPC para fabricar PASS.
