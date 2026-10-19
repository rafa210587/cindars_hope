# Refinamento — Expansão espacial e acessibilidade da Town v1

> **Status:** APROVADO PARA EXECUÇÃO — footprint 160×112 autorizado em 2026-09-10
> **Data:** 2026-09-10
> **Domínio:** TownScene / layout / circulação / navegação de Player e NPCs
> **Origem:** pedido humano para ampliar a cidade, desfazer amontoamentos e revisar entradas
> **Baseline:** `revision-06-scale20-r2`, Town 120×90, construções visuais em 1,20×

## Resultado pretendido

A Town deve parecer uma cidade ocupável, não um conjunto de prédios grandes comprimidos no mesmo
tabuleiro. O jogador precisa reconhecer cada fachada, alcançar serviços sem raspar em props ou NPCs e
cruzar os caminhos principais com outro ator. NPCs devem chegar aos anchors de trabalho, sociais e de
casa sem usar pontos geometricamente livres porém desconectados. O aumento não pode transformar a Town
em um mapa vazio ou alongar deslocamentos cotidianos sem controle.

## Evidência atual

Fatos observados na cena e no código materializados:

- `TownDistrictLayout` define 120×90 (`x=-60..60`, `y=-45..45`), área de 10.800 tiles².
- As construções da `revision-06-scale20-r2` usam `VisualEnlargement=1.20`, mas seus footprints físicos
  permaneceram no tamanho anterior. Isso permitiu aprovação física enquanto as silhuetas de Prefeitura,
  Alquimia e Ferreiro se sobrepunham visualmente.
- O menor afastamento entre footprints é 2,0u (`House_Blacksmith`–`House_Tannery`). Outros gargalos:
  Workshop–Residential_2=3,0u; GateKeeper–AnimalYard=4,0u; Alchemy–Workshop=4,0u.
- `House_AnimalYard` está a apenas 3,0u do limite atual; GateKeeper e Dagna ficam a 5,0/5,5u.
- Vias atuais têm 3,0–3,4u nos eixos principais, 2,2u na saída leste e 2,0u nas ligações de porta.
- O validator salvo usa círculo de raio 0,35u e prova passagem de um ator em grid de 0,5u. Resultado
  vigente: `TownKeyartPhysics PASS:187 FAIL:0`, 28.388 células alcançáveis. Isso não prova cruzamento
  Player×NPC, comportamento dinâmico ou conforto de entrada.
- A cena preserva 24 prédios, 29 NPCs materializados, 28 NPCs agendados, 84 anchors, 23 stalls de NPC,
  6 bancas de mercado, 2 spawns e 554 árvores. O constante `BaselineTreeCount=497` está defasado frente
  ao censo real de 554 e não pode autorizar remoção.
- Player base move speed = 5u/s. A captura comparável é 1536×1024; gameplay usa ortho8.
- A revisão visual independente aceitou o aumento de 20%, mas não a fidelidade global. Persistem malha
  espaçada/ortogonal, resolução desigual e distrito animal pobre.

## Diagnóstico

O problema não é somente o footprint do mapa. Há três contratos desconectados:

1. **Footprint físico:** lotes e colliders podem não se tocar.
2. **Envelope visual:** telhado, chaminé e fachada podem ultrapassar o lote e esconder outro prédio.
3. **Envelope de circulação:** um ponto livre para raio0,35 não garante duas pessoas se cruzando,
   aproximação de uma porta, fila diante de serviço ou giro dentro de uma casa.

Continuar deslocando prédios dentro de 120×90 troca um conflito por outro. Ampliar sem orçamento de
viagem, por outro lado, produziria gramados vazios e caminhada improdutiva. A mudança coerente combina
novo limite, regras de vias/entradas e relayout por distritos.

## Alternativas avaliadas

| Opção | Efeito | Decisão |
|---|---|---|
| Manter 120×90 e rearranjar | Menor diff, mas não cria reserva para envelopes visuais, pátios e fluxo bidirecional | Rejeitada |
| 144×108 (+20% por eixo) | Compensa o upscale atual e aumenta área em 44% | Insuficiente como margem para novos pátios/props |
| **160×112** | +33% largura, +24% altura, +66% área; preserva distância cotidiana controlável | **Recomendada** |
| 168×112 (3:2 exato) | Mais próximo do aspect ratio da keyart, porém adiciona 40% de largura e incentiva dispersão | Adiada; só se 160×112 falhar nos gates |

Em 160×112, o aumento máximo de meia-extensão é 20u na horizontal e 11u na vertical. Na velocidade-base
de 5u/s isso adiciona, no pior deslocamento direto, cerca de 4s/2,2s. O layout não deve usar toda essa
distância: serviços cotidianos permanecem ao redor da praça e as bordas recebem lago, curral, portais,
floresta e marcos de baixa frequência.

## Decisões

| ID | Decisão | Estado |
|---|---|---|
| RT01 | Novo footprint canônico: **160×112**, half extents 80×56 | Aprovado em 2026-09-10 |
| RT02 | Manter `VisualEnlargement=1.20`; esta mudança não aumenta novamente construções | Recomendado |
| RT03 | Usar escala inicial de posições `(x×4/3, (y-4)×56/45+4)` apenas como seed; solver/auditoria decide posições finais | Recomendado |
| RT04 | Larguras livres mínimas: avenida 4,0u; coletora de distrito 3,2u; acesso local 2,4u | Recomendado |
| RT05 | Cada porta pública recebe apron livre 4×3u; vão físico mínimo 1,4u e rota de aproximação testada com raio0,50u | Recomendado |
| RT06 | Cada construção possui envelope visual medido do `SpriteRenderer.bounds`; nenhuma fachada/porta/escada pode ser ocultada por prédio alheio | Recomendado |
| RT07 | Footprints de prédios não anexos mantêm 4,0u de separação; qualquer exceção arquitetônica exige ID e motivo no layout | Recomendado |
| RT08 | Interior walk-in mantém faixa porta→área útil de 1,4u e pocket de giro 2×2u; estação fica a até 3u de caminho navegável | Recomendado |
| RT09 | Player e 28 work anchors passam com raio0,50u; PlayMode prova encontros em praça, mercado e ofícios | Recomendado |
| RT10 | Orçamento: spawn sul→praça ≤75u/15s; praça→qualquer porta pública ≤90u/18s, medidos pelo caminho | Recomendado |
| RT11 | Preservar 24 prédios, 29 NPCs, 84 anchors, 23 stalls NPC, 6 bancas, 2 spawns e **mínimo real de 554 árvores** | Recomendado |
| RT12 | Portão sul e acesso oeste permanecem; saída leste/caverna continua acessível. Nenhum novo portal nesta entrega | Recomendado |
| RT13 | Regeneração exclusivamente pelo gerador Unity; nenhuma edição manual de YAML | Recomendado |

## Distribuição proposta

- **Centro cívico:** praça no mesmo centro lógico `(0,4)`; Bakery/mercado próximos, mas fora do apron.
- **Norte institucional:** Templo a noroeste e Prefeitura a nordeste com jardins próprios e margem visual.
- **Oeste comercial/água:** Inn, MarketHall, Fishery, lago e pontes; duas conexões independentes ao centro.
- **Leste de ofícios:** Alchemy, Blacksmith, Workshop e Tannery em dois pátios, não numa única coluna.
- **Sul residencial:** casas em pequenos quarteirões com ruas locais 2,4u e avenida sul 4u.
- **Sudeste rural:** AnimalYard e GateKeeper ganham curral, apron e acesso separado da saída leste.

Os 66% de área adicional não são preenchidos uniformemente. Aproximadamente um terço vira circulação e
aprons, um terço vira pátios/jardins de identidade e o restante aumenta margens de borda e separação dos
marcos. Props nunca ocupam a largura livre contratada.

## Escopo proposto

- bounds, muralha, chão e câmera de captura da Town;
- posições de distritos, 24 lotes, Prefeitura, lago, praça, portais/spawns e rotas;
- recomputação de work/social/home anchors e stalls pela fonte canônica;
- medição de envelopes visuais e auditoria de acessibilidade física;
- reposicionamento de props/árvores afetados, sem criar famílias novas de arte;
- testes EditMode, validator de cena salva, prova PlayMode e capturas comparáveis.

## Fora de escopo

- aumentar sprites além de 1,20, gerar nova arte ou corrigir a resolução de Bakery/Alchemy/Watermill;
- implementar animações de portas ou remodelar interiores/estações;
- alterar schedule, diálogo, lojas, economia, save, IDs, inventário ou combate;
- adicionar fast travel, novas entradas ou remover conteúdo para abrir espaço;
- declarar fidelidade global de 80% apenas porque o novo layout passou fisicamente.

## Dependências e precedência

- A spec aprovada está em `.specs/a_implementar/spec_town_spatial_expansion_and_access_v1.md`.
- Ela substitui, para esta execução, o limite 120×90 da `spec_town_layout_v9_organic` e todas as
  cláusulas residuais 76×64 de `spec_city_preservation_first_coherent_relayout`.
- `spec_town_keyart_fidelity_v1` e `spec_town_components_doors_and_collision_v1` devem validar sobre a
  cena expandida; não devem consolidar posições definitivas antes desta spec.
- A baseline visual/física continua `revision-06-scale20-r2` até a nova cena passar todos os gates.

## Riscos e limites

- Mais espaço pode piorar densidade visual: mitigar com pátios/jardins e orçamento de viagem, não props
  aleatórios sobre rotas.
- Envelopes AABB podem acusar sobreposição visual aceitável de telhados em perspectiva: o gate binário
  protege porta/escada e área opaca relevante; exceções precisam de catálogo explícito.
- Grid estático não prova dois corpos em movimento: exige PlayMode com Player e NPC reais.
- Reposicionar árvores pela nova borda muda contagem materializada; 554 é piso, não alvo máximo.
- A expansão pode expor câmera além do chão: ground, wall, forest e capture ortho devem mudar juntos.

## Pacote de execução candidato

Uma única spec de integração, serial, com checkpoints internos:

1. congelar manifesto/captura/hash da `revision-06-scale20-r2`;
2. criar contratos/testes de espaço e acesso antes do relayout;
3. ampliar bounds e redistribuir distritos/lotes/rotas;
4. recomputar anchors, stalls, props, água, muralha e floresta;
5. regenerar uma vez, corrigir somente gates objetivos e capturar;
6. executar PlayMode de circulação e revisão visual independente.

O footprint recomendado 160×112 foi aprovado pelo humano em 2026-09-10. Caso ele
não comporte todos os critérios sem exceder os orçamentos
de viagem, a implementação deve parar e pedir amendment; não escalar silenciosamente para 168×112.
