# SPEC — Componentes físicos e portas da Town

**Spec ID:** `town_components_doors_and_collision_v1`  
**Status:** A_IMPLEMENTAR — solicitação humana adicional em2026-09-10  
**Executor:** subagentes de arte/wiring/validação; raiz orquestra e revisa  
**Ordem de execucao:** extensão coordenada do rework Town; não bloquear o diagnóstico visual04  
**Depende de:** `spec_town_spatial_expansion_and_access_v1`, TOWN.V1 e `spec_town_native_architecture_v1`; contrato opt-in de HouseDoorInteractable/RoofRevealController já produzido e revisado  
**Bloqueia:** `spec_town_npc_visuals_navigation_v1` e `spec_town_integrated_acceptance_v1`

required_adrs: []
required_game_rules: [city_rules, event_rules]

# /speckit.specify

## Pedido e objetivo

O humano identificou casas em baixa resolução e pediu portas com animação, interior visível e física de colisão nos itens da cidade. Refinar e implementar os componentes como um conjunto: aparência não prova colisão, e collider desligado não prova que a porta desenhada abriu. Todo objeto sólido recebe suporte físico correspondente; grama, flores rasteiras, sombras, efeitos e telhados elevados não viram barreiras arbitrárias.

Referências obrigatórias:

- `docs/design/gameplay/city/TOWN_COMPONENT_BEHAVIOR_RULE_v1.md`.
- `docs/design/gameplay/city/CITY_KEYART_PROPORTION_RULE_v1.md` e `TOWN_KEYART_ACCEPTANCE_RULE_v2.md` no mesmo diretório.
- `Assets/_Game/Scripts/World/HouseDoorInteractable.cs` e `RoofRevealController.cs` (leitura/integração; edição é ownership da tarefa Farm).
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs` e seus helpers `City/TownKeyart*.cs`.
- `Assets/_Game/Scripts/Editor/Dev/TownKeyartPlayModeCapture.cs` e `Validation/TownKeyartLivePhysicsProbe.cs`.
- Keyart `docs/art_catalog/_reference_keyart_GPT/town_keyart_layout_chatgpt_web_candidate_v2.png`.

## Existing Systems Audit

Searched: Door/HouseDoor/RoofReveal, Sprite/Frame/Animated/Tween, factories Town e testes correspondentes.
Found: HouseDoorInteractable é a porta física da Town (reuse/extend opt-in pelo owner Farm); RoofRevealController revela o interior no mesmo lugar (reuse); DoorInteractable é porta de teleporte da mesma cena, consumidor distinto/legado, não alternativa a introduzir na Town; NpcWalkAnimator/PlayerWalkAnimator permanecem intactos. Não foi encontrado um animador genérico de portas nos nomes buscados. Nenhum sistema paralelo criado nesta preparação.

Diagnóstico confirmado: HouseDoorInteractable hoje move/esconde a leaf instantaneamente; sprites inteiros de edifícios ainda contêm a porta fechada pintada. RoofRevealController conta também o trigger de interação do Player, podendo revelar antes do corpo entrar. A tarefa Farm assumiu os dois runtimes compartilhados; Town não os editará em paralelo.

Contrato aplicado pelo owner Farm e lido integralmente pela raiz: `HouseDoorInteractable.ConfigurePresentation(SpriteRenderer renderer, Sprite[] closedToOpenFrames, float secondsPerFrame=0.08f)`; sequência fechado→aberto, reversa ao fechar, último frame aberto visível, blocker liberado somente no último frame e fechamento protegido pela ocupação do vão mesmo com blocker desabilitado. `RoofRevealController.ConfigureFeetOccupancy(BoxCollider2D interiorTrigger)` é opt-in, ignora colliders trigger e exige pés no interior útil, usando Stay. Configure legado permanece disponível. Fixture fresca: `docs/validation/farm_keyart_v4/editmode-door-api-rerun.xml`, 11/11 PASS em 2026-09-10 03:36:37 UTC. Primeira execução 10/11 preservada; correção só da fixture para invocar OnDisable no EditMode, sem alterar runtime/afrouxar assert. API aceita para integração experimental; callbacks invocados nos testes não provam movimento real nem passagem automática NPC, que continuam gates separados.

## Escopo e preservação

Town-only: TOWN.A1 entrega fachada, roof, vão e folha-base aprovados; esta spec possui somente frames/
timing da folha, AnimationClips quando exigidos pelo consumidor, interiores, colliders, feedback,
validator/captura/testes e wiring correspondente. Reusar paths e IDs existentes. Novos frames de porta
usam prefixo `town_`, sem mudar import de sprites compartilhados. Runtime compartilhado exige lock único.

Sem alteração de NPC IDs/schedules/dialogue, House IDs, saves, inventário/economia, FarmScene ou CaveScene. Não tocar runtime compartilhado sem novo acordo explícito de ownership. Não substituir casas físicas por teleporte ou outra cena. Não atribuir interação a toda decoração só porque ela tem collider.

# /speckit.plan

## Contrato de implementação

1. Casas: aprovar primeiro piloto nativo na escala real; separar fachada com vão, folha animada e camada ocultável sem perder a referência fechada. Reabertura Aseprite/alpha/registro e hashes antes de promover. Não aumentar a casa inteira para corrigir só a porta.
2. Porta: estados visuais fechado→abrindo→aberto→fechando; sequência curta one-shot com dobradiça/pivô fixos. Número/timing de frames definidos com a API opt-in, não pelo tamanho arbitrário de uma sheet. Reversão/acionamentos rápidos não podem deixar arte e blocker divergentes. Fechamento sobre ator no vão exige política segura documentada.
3. Interior: corpo sólido entra pela abertura, telhado/camada de cobertura revela piso e móveis; trigger de interação sozinho não basta. Saída do último collider elegível recobre. NPCs continuam podendo atravessar pelas regras existentes. Curral aberto mantém semântica externa, sem telhado fictício.
4. Colisão: produzir censo por categoria e objeto materializado, incluindo motivo de exceção. Alinhar shapes aos pés/bases/obstáculos, não a todo canvas opaco. Água bloqueia, ponte/cais têm passagem real, portas alternam bloqueio; paredes, móveis sólidos, árvores, pedras, bancas, bancos e cercas não permitem atravessar sua base. Nenhum sólido invisível.
5. Revalidar caminhos com todos os sólidos finais (não só oito troncos antigos), duas entradas, portas, áreas de trabalho e interação. Corrigir placement quando o novo collider bloqueia uma rota; não apagar collider para fabricar PASS.
6. Interior: cada walk-in recebe piso, limite visual, entrada livre e conjunto mínimo coerente com sua
   função. O retângulo vazio observado em `House_Prison` não satisfaz o aceite.
7. Feedback: publicar evento físico específico de porta aberta/fechada no `GameEventBus` e mapear SFX
   distinto; o `PlayerActionFeedbackEvent` genérico/`UiToast` não é áudio físico suficiente.
8. Classificar 24/24 lotes como `walk_in_reveal`, `teleport_interior`, `shop_hours_gated` ou
   `exterior_only`. Na Town atual, o modelo esperado é23 walk-in e AnimalYard exterior; qualquer
   `teleport_interior` exige justificativa/amendment. `shop_hours_gated` acumula walk-in + regra de horário.
9. Loja fechada mantém porta bloqueada e aviso de horário; loja aberta libera entrada/serviço. A folha
   não toca SFX por frame/trigger: no máximo um evento por transição concluída de abrir ou fechar.

## Censo mínimo obrigatório

O relatório lista instância, categoria, bounds visuais, collider/trigger, exceção e resultado. A fonte
autoritativa é a união enumerável do catálogo de suporte + objetos materializados elegíveis na cena.
O validator exige igualdade de conjuntos: `eligibleIds == censusIds`, zero desconhecido, duplicado ou não
classificado. Deve cobrir,
sem limitar-se a: 24 edifícios e folhas de porta; muralha/torres; água/margens/cais; árvores/rochas;
cercas/portões; bancas/bancos/mesas; caixas/fardos; estações; todos os móveis dos interiores. O audit
anterior encontrou ao menos 24 bancos/mesas externos, 13 caixas, 2 fardos e 117 candidatos internos;
esses números são baseline de investigação, não contagem de aceite a ser hardcodada.

## Critérios de aceite

- C-1: casas piloto/família nativas reabertas em Aseprite e aprovadas em escala de jogo, não apenas por dimensões do PNG.
- C-2: nenhuma porta fechada permanece pintada sobre o vão quando a folha abre; observar sequência completa e cronometrada no Unity, além dos frames.
- C-3: fechado bloqueia o corpo real, aberto permite atravessar; interação continua disponível dos dois lados e não fecha sobre um ator preso no vão. Testar manual e autoabertura NPC, com estados finais coerentes.
- C-4: entrada/saída reais por passos de física revelam/recobrem interior; proximidade pelo InteractionTrigger não revela; móveis sólidos colidem e não bloqueiam percurso útil.
- C-5: igualdade entre IDs elegíveis materializados e IDs do censo, zero desconhecido/duplicado/não
  classificado, zero collider sem suporte visual. Exceções decorativas possuem motivo explícito.
- C-6: rotas e acessos com collider real passam no estado final; preservação de IDs, profiles/saves/globais por evidência, sem mudanças de Farm/Cave feitas pela Town.
- C-7: arte estática, reprodução externa da animação, observação Unity, input humano e transições entre cenas têm status separados. NOT RUN nunca vira PASS por inferência.
- C-8: 24/24 lotes têm classificação de porta/interior aplicável; três famílias distintas completam o
  ciclo aprofundado por física real. Todas as famílias aplicáveis passam validação estrutural de
  fachada/vão/frames/blocker/interior; todos os interiores aplicáveis têm piso, limite, saída e móveis.
- C-9: abrir/fechar produz um único SFX físico distinto por transição concluída, sem spam e sem quebrar
  consumidores existentes de feedback/UI.
- C-10: loja fechada bloqueia e mostra horário; aberta permite entrada/serviço. Yael/Maelor permanecem
  indisponíveis de dia e disponíveis à noite pelo contrato existente.

# /speckit.tasks

- [x] Registrar pedido, auditar consumidores e separar ownership compartilhado com Farm.
- [x] Receber contrato proposto da API opt-in de porta/roof reveal da Farm.
- [x] Revisar implementação/evidência dessa API antes de integrar; observação PlayMode permanece pendente.
- [x] Aprovar candidata de casa nativa e separar fachada/vão/folha em Aseprite; aprovação limitada a teste de integração, não C-1/C-2 finais.
- [ ] Autorizar sequência/timing com o consumidor; revisar reprodução e integração Town.
- [ ] Classificar e materializar colisão de cada sólido; corrigir placement e rotas.
- [ ] Executar roteiro funcional no estado final e reportar testes/limites, sem falso closeout.
- [ ] Materializar interiores coerentes por função e validar entrada/reveal/mobiliário em três famílias.
- [ ] Integrar feedback audiovisual específico via evento, com unsubscribe e testes do mapa SFX.
