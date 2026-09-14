# SPEC — catálogo, moradores e programa das construções

**Spec ID:** spec_town_interior_catalog_v1 (S01)  
**Revisão:** 1.0 / 2026-09-12  
**Status:** UNITY_VALIDATED — dados e revisão independente PASS; [evidência S01](../../../docs/validation/town_interiors_hybrid_v1/S01.md). Não implica aceite visual/humano dos interiores.  
**Wave:** lote local TOWN_INTERIORS_HYBRID_V1; nenhuma nova macro-wave  
**Type/Domain/Priority:** Data + Governance / City / P1  
**Executor:** Claude ou Codex; implementador de dados + auditor independente  
**Parallelizable:** NO no catálogo; arte posterior pode ser independente  
**Repo lock scope:** `Assets/_Game/Scripts/City/Interiors/`, `Assets/_Game/Data/City/Interiors/`, generator de catálogo, amendment.  
**Ordem de execucao:** 1 de 4  
**Depende de:** aprovação deste pacote e [refinamento](../../../docs/refinements/a_implementar/ref_town_interiors_hybrid_v1.md); fontes atuais auditadas em §9.  
**Bloqueia:** S02/S03/S04.  
**Scope:** 24 registros, residência/custódia, programa espacial, acesso e venue principal.  
**Out of scope:** materializar cenas, mover NPCs, criar receitas/economia, romance, novas famílias, escritura de imóvel.  
**Validation level alvo:** UNITY_VALIDATED para dados + revisão de design humana; nesta autoria NOT RUN.

required_adrs: []
required_game_rules: [city_rules, event_rules]

# /speckit.specify

## 5–8. Objetivo e fontes

Cada construção deve ter conteúdo que justifique sua função e moradores. Dormir não é o único requisito: alimentação, armazenamento, trabalho e circulação devem caber. Owner significa responsável/residente operacional; prédio público possui custódia, não transferência de propriedade a um NPC.

Fontes obrigatórias: AGENTS/CLAUDE, CURRENT_STATE, refinamento, `TownCityLayout.cs`, `CreateMvpTownScene.cs` (TownNpcHomes, TownNpcSpecs, TryAddCraftingStation), `TownShopDoorHoursWiring.cs`, `NpcServiceCatalog.cs`, spec ativa de componentes. Skills: `data-catalog-authoring`, `registry-catalog-pattern`, `scene-composition-review`, `scene-physics-review`; matriz `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`.

## 9. Phase 0 auditada

`TownCityLayout` declara 24 lotes; `TownNpcHomes` tem 23 Indoor/5 Outdoor (21 moradias únicas); `TownNpcSpecs` tem 23 ShopDataPath preenchidos. `TownShopDoorHoursWiring.ProviderByHouse` contém nove lojas, não todos os comerciantes. Os oito serviços estão em `Assets/_Game/Scripts/NPC/Services/NpcServiceCatalog.cs`. Catalogação nova `TownInteriorDefinitionSO` não encontrada na busca `rg --files Assets/_Game/Scripts | rg 'Interior'`; encontrados somente ferramentas de arte Farm e apresentação Town, não catálogo runtime de moradias.

Antes da execução repetir as buscas do refinamento. Registros abaixo são o delta proposto; os owners residentes vêm do mapa real, dimensões e interiores novos são projeto, não observação de assets existentes.

## 10. Regras de escala e leitura das fichas

- Dimensões abaixo são envelope de **piso útil em unidades Unity**, não metros nem canvas de sprite; paredes ficam fora. Reserva interna de circulação já deve caber nesse envelope. Não afirmar habitabilidade apenas por atingir a área.
- Referência visual: Player final Town, alpha ~2,496u. Medir também Smallfolk/Dwarf/Orc/Dragonborn reais. Não normalizar raças para uma altura única. Cama projetada e personagem vertical têm eixos diferentes.
- Usar as mesmas escalas/pixel density calibradas em exterior e interior. Se falta espaço, aumentar planta; não diminuir ator, collider, móvel ou afastar câmera para mascarar.
- Porta simples/corredor de uso: largura livre `max(1.8u, Dmax + 0.35u)`; corredor compartilhado principal: `max(2.4u, Dplayer + DmaiorMorador + 0.50u)`. D são larguras finais de suporte físico, incluindo todos os colliders. São margens propostas deste projeto, verificadas em Play Mode, não lei universal.
- `Dormitório` implica cama individual, guarda-roupa/baú pessoal, apoio/luminária e lado de acesso; `doméstico` inclui cozinha separada do ofício, refeição/assento e lavatório/biombo. Sanitário é cenário privado, sem nova necessidade fisiológica.
- Todo móvel tem finalidade e classificação: **S** sólido não interagível; **D** decoração (tapete/quadro etc.); **E** interação existente, preservar ID/efeito; **N** nova interação NÃO autorizada neste pacote.
- Uma cama por residente: 23 camas pessoais no total, mais duas camas de hóspede da pousada (25). Orlan/Gruta não ganham cama de casal por inferência; Hund/Gurd têm camas adequadas ao corpo Orc. Nenhum NPC novo.
- Norte é o fundo; sul é a chegada. Zonas abaixo são posições relativas e dimensões a reservar no envelope, não coordenadas globais. Divisórias não podem recortar corredor/porta. Portas internas não carregam outra cena.

## 11. Catálogo integral: 24 fichas normativas propostas

### H01 — House_Temple — Templo de Kanthor

Residência: `npc_corvus`; custódia religiosa de Corvus. Tibbet trabalha no cemitério e **não** ganha dormitório aqui. Fachada 16×11; interior proposto 18×16, portal.

Sul: vestíbulo 18×3 e corredor axial. Centro: nave 12×7 com 4 bancos S, altar S ao norte e dois nichos D; circulação lateral até altar. Ala leste 6×7: consulta/ofertas com mesa E do NPC e armário de materiais S. Norte: sacristia/arquivo 10×6 e aposento de Corvus 8×6 com cama, copa e lavatório. Símbolos de Kanthor pela direção de arte existente; não criar buff de altar.

Comércio: `shop_corvus`, posto interno na ala pública; diálogo/ofertas já existentes. Motivo: religião e atendimento convivem com retiro privado. Visita pública 06–22; aposento segue política residencial. Preço/estoque atual inalterado.

### H02 — House_Chamber — Câmara do conselho

Sem residente; custódia `npc_velorin`, que continua morando na Manor. Fachada 8×7; interior 14×12, portal.

Sul: recepção/espera 14×3, 2 assentos S, mural D. Centro/norte: sala de conselho 10×6, mesa S e 4 cadeiras S sem NPCs extras. Leste: gabinete 4×6 com mesa de Velorin E, selo/documentos D; norte restante: arquivo administrativo 14×3 com duas estantes S. Sem cama/cozinha residencial. Atendimento político, não nova loja ou venda de licenças.

Visita 09–18, saída irrestrita. Motivo: separar trabalho institucional da residência do líder.

### H03 — House_Prison — Posto de custódia

Sem residente; custódia de `npc_alaric`, sem duplicar Alaric no posto e na prisão. Fachada 8×7; interior 14×12, portal.

Sul: recepção/registro 14×3, mesa S e banco de visitantes S. Oeste: equipamento 4×6 (suporte de armas D/S conforme base, baú de uniforme S). Centro: corredor 3×6. Leste: duas celas separadas 3,5×6 no total 7×6, cada uma com banco/leito fixo S e lavatório D; não contam como camas de residentes. Norte 14×3: serviço/depósito. Grades sólidas, sem preso inventado, crime, punição ou loot implementados.

Área pública 09–18; portas das celas bloqueadas para Player com explicação, saída pública sempre livre. Não criar ShopId.

### H04 — House_Manor — Residência de Velorin

Residente `npc_velorin`; fachada 9×8; interior 16×14, portal.

Sul 16×4: hall e sala de visita com sofá/banco S, 2 cadeiras e mesa S. Centro: estudo 8×5 (mesa, mapas D, biblioteca S) + jantar/cozinha 8×5 (mesa com 2 lugares, fogão/armário S). Norte: dormitório 10×5 e lavatório/despensa 6×5. Uma cama, nenhum criado inventado. Elementos civis remetem à função do owner sem transformar residência em segunda Câmara.

Sem loja; visita residencial 06–22. Maior conforto que casas simples sem alterar escala do Player.

### H05 — House_Registry — Cartório e residência de Mara

Residente `npc_mara`; fachada 8×7; interior 16×12, portal.

Sul 16×5: balcão de registros E, espera com 2 assentos S, estantes de formulários S e mesa de autenticação S. Norte: apartamento 10×7 (cama, cozinha/mesa/lavatório) e arquivo fechado 6×7, com corredor conectando entrada/serviço/moradia sem atravessar balcão. Arquivo civil, não armário de louça rotulado como livros.

`shop_mara` e `service_mara_farm_registry` internos; preservar `contract_farm_registry`,100g e bônus5% no shipping, sem novo gate da Farm. Tovin não é movido para cá: seu work/home atuais pertencem a House_Tovin. Motivo: preservar identidade e fornecer privacidade real à moradora.

### H06 — House_Archive — Arquivo e pesquisa de Thalindra

Residente `npc_thalindra`; fachada 9×8; interior 16×14, portal.

Sul 16×4: recepção/consulta E, 2 assentos. Centro 16×5: duas fileiras de estantes S nas laterais, mesa de pesquisa S e circulação central; bancada de análise E em nicho próprio sem criar segunda interface. Norte 16×5: dormitório com cama, copa e lavatório separados da coleção. Livros, pergaminhos e espécimes D; sem produtos de cozinha nas prateleiras do arquivo.

`shop_thalindra` + `service_thalindra_analysis` internos, preservando custo e insumo. Moradia não pode desaparecer ao remover o kit doméstico do salão.

### H07 — House_MarketHall — Mercado coberto comunitário

Sem residente ou vendedor exclusivo; custódia institucional, `ResponsibleNpcId` vazio justificado. Fachada 9×8; interior 16×12, portal.

Sul 16×3: passagem de entrada/saída ampla. Oeste/centro 10×6: duas mesas de exposição S com produtos D; sem inventários próprios. Leste 6×6: bancada E `Station_Workbench` e área de uso desobstruída. Norte 16×3: estoque cenográfico S e mural D. Não duplicar as seis bancas da praça nem deslocar Yael para dentro.

Aberto 06–22; crafting existente, sem ShopId novo. Motivo: espaço coberto de preparo e exposição, não clone do mercado aberto.

### H08 — House_Bakery — Padaria e casa de Mella

Residente `npc_mella`; fachada 10×7; interior 16×12, portal.

Sul 16×4: balcão de pães E e 2 assentos S. Centro 16×4: mesa de preparo, forno, farinha e prateleiras S; forno é cenário nesta entrega, sem inventar recipe station. Norte 16×4: cama e armário num quarto, copa/mesa/lavatório domésticos em outro. Separação visual clara entre forno de trabalho e cama.

`shop_mella` interno. Motivo: venda na frente, produção visível atrás e descanso protegido, respeitando a residência atual.

### H09 — House_Inn — Estalagem e taverna de Orlan/Gruta

Residentes `npc_orlan`, `npc_gruta`; fachada 14×11; interior 24×18, portal.

Sul 24×6: recepção E de Orlan, salão com 3 mesas/6 assentos S, balcão E de Gruta, eixo de entrada livre. Centro 24×6: cozinha 8×6 (`Station_CookingStation` E), banho 6×6 ligado a `service_gruta_thermal_bath` (efeito existente; cenário não dá banho grátis), corredor 3×6, despensa/apoio 7×6. Norte 24×6: 2 quartos de hóspede de 6×6, quarto de Orlan 6×6 e quarto de Gruta 6×6. Cada quarto com cama, apoio, armazenamento e acesso próprio. Banho/lavatório compartilhados.

`shop_orlan` e `shop_gruta` internos; `service_orlan_daily_dish` permanece de Orlan, não trocar pelo nome da profissão. Preservar `GuestBed_Inn` como uma das duas camas de hóspede e acrescentar `GuestBed_Inn_02` pelo mesmo `InnBedPaymentGate`, sem mudar tarifa/noite. Duas camas de hóspedes não implicam novos NPCs.

Recepção/salão acessíveis 24h para hóspedes; compra/serviços dependem do NPC disponível. À noite o Player não fica preso. Não introduzir reserva persistente de quarto; interação paga da cama usa o sistema atual.

### H10 — House_Fishery — Pescaria/moinho de Sael

Residente `npc_sael`; fachada 8×9; interior 14×12, portal; porta exterior preserva deslocamento tangencial −2u, roda e cais não viram área interior.

Sul 14×4: redes, suportes de vara e mesa de limpeza S. Centro 14×4: depósito de cestos e área do moinho/engrenagem cenográfica S, isolada da passagem. Norte 14×4: quarto, copa e lavatório de Sael. A roda/moinho não cria sistema produtivo novo.

`shop_sael` principal **externo no cais**, para compra rápida de pesca. Interior é trabalho/descanso, não segunda loja. Gate de loja anterior do prédio vira política residencial/visita; disponibilidade da banca não muda.

### H11 — House_Blacksmith — Forja e residência de Brumdar

Residente `npc_brumdar`; fachada 12×9; interior 18×14, portal.

Sul 18×4: atendimento E, exposição de ferramentas D sobre suporte S. Centro 18×5: forja E `Station_Forge`, bigorna S, armazenamento de minério S e zona térmica visualmente marcada; nenhuma cama junto ao fogo. Norte: apartamento 12×5 com cama/copa/lavatório e reserva de ferramentas 6×5. Corredor até estação e atendimento sem cruzar suporte da bigorna.

`shop_brumdar`, reparo/tempera existentes e `service_brumdar_repair_discount` internos. Não mudar balanceamento nem criar dano de fogo ambiental.

### H12 — House_AlchemyLab — Laboratório e residência de Ozzra

Residente `npc_ozzra`; fachada 9×9; interior 16×14, portal.

Sul 16×4: balcão E e expositor S. Centro 16×5: `Station_Alchemy` E, bancada S, frascos D e armário de reagentes S; acesso separado de armazenamento. Norte 16×5: quarto/copa/lavatório; dormir/comer separados dos reagentes. Móveis de Smallfolk são individuais, mas o corredor suporta o Player.

`shop_ozzra` + `service_ozzra_relic_identification` internos. Frascos são decoração, não poções coletáveis.

### H13 — House_Workshop — Oficina/casa de Nimble

Residente `npc_nimble`; fachada 8×9; interior 16×12, portal.

Sul 16×4: balcão E e mesa de encomendas S. Centro 16×4: `Station_Carpentry` E, bancada/serras cenográficas S e madeira armazenada S. Norte 16×4: cama, copa/mesa e lavatório, separado de serragem/peças. Nenhum gerador automático de materiais.

`shop_nimble` interno. O perfil Patrol atual não torna Nimble guarda de profissão; horários vêm da agenda real, não de inferência por título.

### H14 — House_Tannery — Curtume/tanoaria e casa de Hess

Residente `npc_hess`; fachada 9×9; interior 18×14, portal.

Sul 18×4: atendimento E e exposição de couro/barris D. Centro 18×5: cavaletes S, mesa S, barris/tinas S; corredor protegido de ferramentas. Norte 18×5: quarto/copa/lavatório separados por parede. Porta e mobiliário pessoal calibrados ao Dragonborn e Player.

`shop_hess` interno; manter catálogo mesmo com nome histórico “Tanoaria do Hess”. Não transformar o nome ambíguo em nova cadeia de crafting. Motivo: preservar os dois indícios visuais existentes sem inventar profissão incompatível.

### H15 — House_Residential_4 — Casa de Savra

Residente `npc_savra`; fachada 8×7; interior 14×12, portal.

Sul 14×4: sala/mesa de visitas S. Centro 14×4: preparo doméstico/copa e nicho de ervas secas S/D; sem estação alquímica duplicada. Norte 14×4: quarto com cama adequada ao Dragonborn, armazenamento e lavatório. Plantas/rotas de coleta D explicam a função.

`shop_savra` principal **externo na rota de ervas/floresta**; visita residencial, sem balcão adicional em casa.

### H16 — House_CarvalhoTorto — Casa de Hund e Gurd

Residentes `npc_hund`, `npc_gurd`; fachada 8×7; interior 18×14, portal.

Sul 18×4: sala/refeição com mesa de 4 lugares S (não quatro residentes). Centro 18×5: cozinha/lavatório 10×5 e depósito de ferramentas/apoio 8×5; equipamento de guarda separado da carpintaria pesada. Norte 18×5: dois quartos 9×5, uma cama Orc e um baú pessoal em cada. Compartilhar casa não autoriza inventar casal/parentesco além do já registrado.

`shop_hund` e `shop_gurd` principais **externos** nas rotas/postos/construção. Casa é moradia, não segunda guilda ou canteiro comercial. Corredor principal testado Player×Orc e Orc×Orc.

### H17 — House_Residential_1 — Casa/horticultura de Sylveth

Residente `npc_sylveth`; fachada 8×7; interior 14×12, portal.

Sul 14×4: recepção/sala e mesa S. Centro 14×4: preparo de sementes S e copa doméstica separados, caixas rotuladas D e armário S. Norte 14×4: cama, roupas e lavatório. Não plantar crops funcionais dentro nem criar estoque independente.

`shop_sylveth` principal **externo na banca de sementes**, próximo à circulação de quem vem da Farm. Interior explica a atividade sem impor loading a uma compra recorrente.

### H18 — House_Dagna — Casa de Dagna

Residente `npc_dagna`; fachada 8×7; interior 12×10, portal.

Sul 12×3: sala/refeição S; centro 12×3: copa/lavatório + nicho de capacete/ferramentas S/D; norte 12×4: quarto/armário, amostras de pedra D. Uma cama Dwarf, passagens utilizáveis pelo Player. Equipamento de mineração não vira veio minerável dentro de casa.

`shop_dagna` principal **externo na rota da pedreira**. Sem forja ou mineração funcional no quarto.

### H19 — House_Residential_2 — Armazém/casa de Renko

Residente `npc_renko`; fachada 8×7; interior 16×12, portal; primeiro piloto de loja-moradia.

Sul 16×5: balcão E e 3 expositores S/D nas laterais; eixo central e lugar do atendente atrás do balcão. Centro 16×3: estoque cenográfico S e copa. Norte 16×4: quarto com cama Smallfolk, armário e lavatório. Estoque visível não é contêiner grátis.

`shop_renko` interno, fonte única de compras/vendas; banca externa fica exposição/sinalização, sem NPC fantasma. Compra recorrente justifica transição curta e indicação de atendente antes de entrar.

### H20 — House_Pip — Casa do mensageiro

Residente `npc_pip`; fachada 8×7; interior 12×10, portal.

Sul 12×3: mesa de triagem S, bolsas/rotas D e banco S. Centro 12×3: copa/lavatório, baú pessoal S; norte 12×4: cama Smallfolk e armazenamento. Cartas não criam sistema de correio nem quests novas.

`shop_pip` principal **externo na rota/entrada da cidade**. Visita residencial; casa não captura o mensageiro durante expediente.

### H21 — House_Residential_3 — Ateliê/casa de Mirela

Residente `npc_mirela`; fachada 8×7; interior 16×12, portal.

Sul 16×4: atendimento E e manequim D/S conforme base. Centro 16×4: `Station_Sewing` E, mesa de corte S e rolos de tecido S; zona de uso livre. Norte 16×4: quarto, copa e lavatório separados. Uma cama; manequim não usa NPCController.

`shop_mirela` interno, sem duplicar tecido ou catálogo no expositor.

### H22 — House_Tovin — Escritório/casa de Tovin

Residente `npc_tovin`; fachada 8×7; interior 14×12, portal.

Sul 14×4: escrivaninha de licenças E, 2 assentos S e painel D. Centro 14×4: arquivo pessoal S e copa/lavatório; norte 14×4: cama Smallfolk e armário. A organização pode ser semelhante à de Mara, mas domínio de atendimento e owner são próprios.

`shop_tovin` e `service_tovin_market_stall` internos; preservar `license_market_stall`,200g e gate apenas da venda urbana. Não transferir licenças para Mara nem consolidar os dois ShopIds.

### H23 — House_GateKeeper — Posto e aposento de Alaric

Residente `npc_alaric`; fachada/lote 8×6; **híbrido**, posto cutaway externo preservado e aposento separado 12×10.

Posto existente: mesa de registros, suporte de equipamento e caixa de turno; remover a cama antiga apenas quando a cama do aposento estiver pronta. Portal residencial atrás de porta interna do posto, não no vão da rua. Interior separado: sul 12×3 sala/refeição; centro 12×3 copa/lavatório e mapa/equipamento D/S; norte 12×4 quarto/armário. Uma cama de Alaric.

Sem ShopId. Trabalho de Alaric segue no portão e eventual custódia da prisão, sem spawn duplicado. Dormir deixa de depender de um cômodo de 6×4 com mesa de serviço.

### H24 — House_AnimalYard — Curral e casa de apoio de Eiran

Residente `npc_eiran`; lote 16×12; **híbrido**, curral aberto preservado e aposento 14×12 em anexo com porta.

Exterior: cercas, animais, portão e atendimento de Eiran mantidos; não fechar sob telhado. Anexo visual pequeno deve caber na borda do lote sem cortar manejo/passagem; sem reduzir animais para caber. Interior: sul 14×4 sala de apoio/refeição, centro 14×4 copa/lavatório e equipamento limpo, norte 14×4 dormitório/armário. Ração/depósito de trabalho separados do quarto; nenhum animal ativo no interior residencial.

`shop_eiran` e `service_eiran_premium_pasture` **externos**. A atribuição antiga Indoor não significa que dormir entre animais seja aceitável; o anexo resolve a incoerência sem transformar curral em residência fechada.

## 12. NPCs que não recebem casa

`npc_maelor` (cemitério), `npc_yael` (fundo do mercado noturno), `npc_liora` (jardim), `npc_zrix` (boca da gruta), `npc_tibbet` (cemitério): preservar HomeAssignment. `npc_vaalara_wanderer_01` é apresentação/extra fora do censo de 28 e não recebe owner inventado. Não converter o cemitério em dormitório coletivo nem colocar essas pessoas nas camas vazias da pousada sem nova decisão.

## 13. Não duplicação e amendment

Não recriar TownCityLayout, ShopDataSO/ShopManager, NpcServiceCatalog, receitas/crafting, agenda, save ou interação. O catálogo novo referencia IDs e programa; não substitui balanceamento ou lore.

Ao aprovar, registrar amendment **na fonte canônica** `docs/game_rules/city_rules.md`, seção “Town — interiores híbridos”, com D01–D10 e link para esta spec aprovada:23 walk-ins →22 portais +posto cutaway +curral externo com anexos;24 destinos. Marcar o trecho operacional anterior como histórico na parte substituída e atualizar Rules3/4 pertinentes, além de C-4/C-6/C-8 da spec antiga. Não criar outro arquivo de amendment como fonte concorrente. Registrar exceção de circulação pública24h da Inn, sem liberar serviço fechado. Loja-moradia com acesso único permanece bloqueada para visita Player quando loja fechada; residências puras seguem06–22, saída e entrada do morador são independentes. Não apagar falhas históricas. Até aprovação, baseline antigo continua válido.

# /speckit.plan

## 15–16. Dados e contratos propostos (não existentes)

CRIAR em `Assets/_Game/Scripts/City/Interiors/`:

```csharp
public enum TownInteriorEntryMode { Portal = 0, GuardPostAnnex = 1, AnimalYardAnnex = 2 }
public enum TownInteriorOwnershipKind { Household = 0, Institutional = 1 }
public enum TownInteriorPropRole { SolidScenery = 0, FloorDecoration = 1, ExistingInteraction = 2 }
public sealed class TownInteriorDefinitionSO : ScriptableObject {
    public string HouseId, TemplateId, ResponsibleNpcId;
    public TownInteriorEntryMode EntryMode;
    public TownInteriorOwnershipKind OwnershipKind;
    public string[] ResidentNpcIds, PrimaryIndoorProviderNpcIds;
    public Vector2 UsableFloorSize;
    public string InteriorPrefabKey, EntryAnchorId, ReturnAnchorId;
    public TownInteriorAccessDefinition Access;
    public TownInteriorRoomDefinition[] Rooms;
    public TownInteriorPropDefinition[] Props;
}
[Serializable] public sealed class TownInteriorRoomDefinition {
    public string RoomId, Purpose; public Rect LocalFloorBounds;
    public string[] AuthorizedResidentNpcIds;
}
[Serializable] public sealed class TownInteriorPropDefinition {
    public string PropId, RoomId, AssetKey, Purpose, ExistingInteractionId;
    public TownInteriorPropRole Role; public Vector2 LocalPosition;
}
public sealed class TownInteriorCatalogSO : ScriptableObject {
    public TownInteriorDefinitionSO[] Definitions;
    public bool TryGet(string houseId, out TownInteriorDefinitionSO definition);
}
[Serializable] public sealed class TownInteriorAccessDefinition {
    public int ResidentialVisitStartHour = 6, ResidentialVisitEndHour = 22;
    public int CivicStartHour, CivicEndHour;
    public bool PublicLobbyAlwaysOpen, CommercialEntryFollowsProviderSchedule;
    public bool HasResidentialVisit, ResidentsBypassEntryHours;
}
```

Campos Unity acima são assets de conteúdo, **não DTO de save**. `ResponsibleNpcId` vazio somente em MarketHall institucional; Prison usa Alaric como custodiante, Chamber Velorin, sem adicioná-los a ResidentNpcIds. IDs de prop `HouseId/room/prop`, estáveis. Templates compartilháveis, instanciação por HouseId; nunca ID só do template. Horários comerciais derivam da agenda; Civic só para Temple/Chamber/Prison/MarketHall, PublicLobbyAlwaysOpen somente Inn. Precedência: saída livre → residente autorizado → lobby público → janela comercial → visita de residência pura; não somar visita para contornar comércio fechado.

Revisão de acesso em 2026-09-12: janela pública Civic participa da precedência antes do gate comercial.
`CommercialEntryFollowsProviderSchedule` regula **entrada do edifício**, não transação no balcão:
false em Temple (nave06–22) e Inn (salão24h), sem atendimento fora da agenda dos respectivos NPCs.
Nos demais imóveis comerciais com entrada única permanece true. Política antiga de loja fechada
da Inn só é removida no rollout aprovado; este catálogo não muda a porta atual sozinho.

`GenerateTownInteriorCatalog.Generate()` (Editor, novo em `Scripts/Editor/SceneCreation/City/`) cria/atualiza por HouseId em `Assets/_Game/Data/City/Interiors/`, preservando GUID e referências. As 24 fichas são obrigatórias; transcrever sem inferência. Para Room/Prop bounds, cumprir zoneamento e margens acima; escolhas de arte ficam em S04, não converter filenames em identidade.

## 18–20. Ownership, edições e estratégia

Permitidos: tipos/arquivos em City/Interiors indicados, generator acima, `Assets/_Game/Data/City/Interiors/*.asset` e metas pelo Unity; `Assets/_Game/Scripts/Editor/Validation/ValidateTownInteriorCatalog.cs`; testes `Assets/_Game/Tests/EditMode/City/TownInteriorCatalogTests.cs`; amendment, city_rules.md e atualização pontual da spec antiga após aprovação. Sem editar TownScene nesta spec.

Proibidos: save/runtime de porta/NPC, Farm/Cave, Packages, ProjectSettings, YAML manual, preços/itens e casas existentes. O catálogo usa `(skill: data-catalog-authoring)`; validação `(skill: editor-validator-authoring)`.

1. Reconfirmar IDs/contagens; congelar tabela aprovada e amendment.
2. Criar structs/SOs e generator idempotente `Generate()`: localizar asset por caminho estável, atualizar campos sem recriar GUID; abortar owner desconhecido/HouseId duplicado antes de escrita parcial.
3. Validar manifesto de cômodos/prop purposes, residentes e provedores, sem exigir sprite candidato aprovado antes de S04; `AssetKey` deve ser key resolvível ou explicitamente candidato pendente, nunca silencioso substituto.
4. Relatório `docs/validation/town_interiors_hybrid_v1/S01.md` com diff de ownership e expected/observed; nenhum status artístico PASS por catálogo.

## 14/22. Critérios binários e testes esperados

| AC | Evidência esperada após implementação |
|---|---|
| CAT01 | `ValidateTownInteriorCatalog.Validate` imprime `TOWN_INTERIOR_CATALOG: PASS houses=24 residences=21 residents=23 outdoor=5`; IDs correspondem exatamente a AllBuildings/TownNpcHomes. |
| CAT02 | `ResidentAssignments_MatchCanonicalHomes` preserva cada par NPC→House; `InstitutionalLots_DoNotInventResidents` retorna zero residentes em Chamber/Prison/MarketHall. |
| CAT03 | `Program_BedsCoverResidentsAndTwoGuests` confirma 23 pessoais + 2 hóspedes, sem contar bancos da prisão ou antiga cama do posto. |
| CAT04 | `PrimaryShopVenues_AreUnique` confirma23 fornecedores únicos:13 internos/10 externos, nenhum ShopId novo, oito serviços pessoais +dois cívicos originais. |
| CAT05 | `Generator_SecondRunPreservesGuidsAndContent` confirma segundo run sem alteração semântica/GUID; referências desconhecidas/duplicadas FAIL com HouseId. |
| CAT06 | Revisão humana das 24 fichas e amendment registrada explicitamente; sem aprovação, migração bloqueada. |

Runner futuro `tools/unity/RunUnityEditModeTests.ps1 -TestFilter TownInteriorCatalogTests -ResultsPath Logs/town-interior-catalog.xml -LogFile Logs/town-interior-catalog.log`: todos casos selecionados passam, failed=0; não predefinir número de parametrizações como teste executado.

# /speckit.tasks

## 20–21. Tasks ordenadas

- [x] CAT-T01 — dono dados: reconfirmar Phase0 e aprovar amendment/fichas (CAT01/06).
- [x] CAT-T02 — criar contratos/SOs/generator, 24 registros e zoneamento (CAT01–04); depende T01.
- [x] CAT-T03 — testes negativos e idempotência + validator (CAT01–05); depende T02.
- [x] CAT-T04 — revisão de design/owner e relatório/finish-spec (CAT01–06); dados UNITY_VALIDATED, sem aceite visual/físico ou promoção do pacote completo.

## 23. Falhas e bordas

Owner ausente: rejeitar registro, sem fallback “casa mais próxima”. Shared home: manter IDs individuais de camas/armários. Asset ainda sem arte: dado pode existir, mas publicação visual bloqueada em S04. Renomear House_*: proibido. Programa que não cabe: ajustar envelope/planta com revisão, não cortar cômodo exigido. Conflito com regra antiga: amendment explícito antes de habilitar portal. NPC de profissão ambígua: preservar catálogo/serviço, não escolher outro owner pelo sprite.
