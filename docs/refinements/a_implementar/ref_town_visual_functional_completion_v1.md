# Refinamento — conclusão visual e funcional da Town v1

**Status:** APROVADO PARA ESPECIFICAÇÃO — pedido humano de 2026-09-10  
**Origem:** reavaliação da TownScene materializada após a expansão 160×112  
**Escopo:** composição, arquitetura, portas/interiores/colisão, NPCs, ambientação e aceite  
**Não autoriza:** execução automática, mudança de IDs, save schema ou edição manual de YAML

## Resultado da reavaliação

A expansão espacial resolveu escala global, acessos e aglomeração, mas não encerrou a qualidade da cidade.
A captura final vigente alcança aproximadamente **68/100** de proximidade perceptual com a keyart
(faixa razoável 64–72), abaixo do mínimo de 80 definido por
`docs/design/gameplay/city/TOWN_KEYART_ACCEPTANCE_RULE_v2.md`.

O problema não é mais “a cidade é pequena”. O débito está concentrado em cinco frentes:

1. composição ainda ortogonal, com vazios de grama e pouca densidade narrativa;
2. edifícios repetidos ou reamostrados, sem famílias arquitetônicas nativas suficientes;
3. porta/interior/colisão existem como piloto, mas não formam um contrato completo em toda a cidade;
4. dois NPCs continuam como placeholder e o deslocamento usa movimento direto com teleporte por timeout;
5. ambientação, feedback e aceite final ainda não foram demonstrados no runtime real.

## Evidência usada

- referência: `docs/art_catalog/_reference_keyart_GPT/town_keyart_layout_chatgpt_web_candidate_v2.png`;
- visão final: `art/town-keyart-rework/evidence/town-expansion-v1/final/town-overview-ortho58.png`;
- detalhes: templo, moinho e quatro estados de `House_Prison` no pacote de evidência da expansão;
- auditoria: `docs/validation/town_keyart_deep_reassessment.md`;
- regras: `CITY_KEYART_PROPORTION_RULE_v1.md`, `TOWN_KEYART_ACCEPTANCE_RULE_v2.md` e
  `TOWN_COMPONENT_BEHAVIOR_RULE_v1.md` em `docs/design/gameplay/city/`.

## Decisões fechadas

### D-01 — A expansão é fundação, não alvo de nova escala

Manter 160×112, `1 unit = 1 tile`, escala visual +20%, 24 lotes, 29 NPCs materializados,
28 perfis agendados, 84 âncoras e os IDs atuais. Mudanças futuras redistribuem densidade e arte dentro
desse contrato; não voltam a 120×90 e não aumentam novamente a cidade sem nova decisão humana.

### D-02 — Composição deve ganhar organicidade sem quebrar o grafo físico

Curvar bordas e ramificações visuais, formar bolsões e jardins, preencher vazios e reforçar os sete
distritos. A avenida sul, praça, acessos de edifícios, rotas entre spawns e clareiras de trabalho
continuam verificáveis. Decoração nunca ocupa corredor funcional para esconder um vazio.

### D-03 — Identidade arquitetônica é avaliada em escala de jogo

Templo, câmara, ferraria, mercado, pousada, padaria, alquimia, moinho, guarnição, mansão e celeiro
precisam de leitura própria. Casas residenciais podem compartilhar famílias, mas variação só de cor não
conta como família. Bakery, watermill e alchemy são os primeiros alvos de reconstrução nativa.

Todo asset novo ou alterado deve ter fonte Aseprite editável, pixels nativos, alpha real, `Point`,
`Compression None`, mipmaps desligados, PPU/pivô documentados e revisão na escala final do jogo.

### D-04 — Porta é um conjunto, não um sprite sobre fachada pintada

Cada família walk-in usa fachada com vão real, folha separada e sequência fechado/abrindo/aberto/
fechando. O blocker libera somente quando a passagem está visualmente aberta. Ocupação do vão impede
fechamento inseguro. O estado aberto não pode manter uma segunda porta fechada pintada na fachada.

### D-05 — Interior deve comunicar função

Os interiores deixam de ser retângulos vazios. Cada edifício walk-in recebe piso, limite visual,
entrada livre e ao menos um conjunto funcional coerente com sua finalidade. Móveis sólidos colidem pela
base; itens rasos e puramente decorativos são exceções justificadas. O reveal usa os pés do ator dentro
do volume útil, não mera proximidade do trigger de interação.

### D-06 — Colisão é censitária e semântica

O censo cobre cada instância materializada de paredes, água, árvores, rochas, cercas, bancas, bancos,
caixas, fardos, estações, móveis e portas. Cada item é classificado como sólido, trigger, transitável ou
decorativo, com motivo. Não basta comparar uma contagem agregada; não pode haver sólido visível
atravessável nem collider invisível bloqueando uma rota.

### D-07 — NPC não pode compensar navegação ruim com teleporte silencioso

Substituir os dois placeholders (`npc_corvus`, `npc_vaalara_wanderer_01`) por sprites finais compatíveis
com `NpcWalkAnimator`. Agenda e wander devem seguir rotas materializadas/waypoints da Town, respeitar
colisões e portas e detectar progresso real. Teleporte, se mantido como recuperação excepcional, deve
ser observável, limitado e nunca ser a trajetória normal.

### D-08 — Ambientação fecha a leitura da cidade

Praça, água/moinho, curral, muralha e transições entre distritos recebem sinais de vida controlados:
água e roda com animação coerente, iluminação/props por função, animais ou evidência ambiental no
distrito rural e feedback audiovisual distinto para portas. Movimento não pode alterar colisores de
forma imprevisível nem introduzir custo de frame sem medição.

### D-09 — Aceite é integrado e comparável

O score final só é calculado após as quatro specs de produção. São obrigatórios dois revisores, mesma
câmera/crop da baseline, categorias mínimas da regra v2 e ausência de bloqueadores. Captura estática não
prova animação, input, física, agenda, transição ou desempenho.

## Decomposição em specs

| Ordem | Spec | Responsabilidade exclusiva |
|---|---|---|
| TOWN.V1 | `spec_town_keyart_fidelity_v1` | macrocomposição, praça, caminhos, muralha, água, curral, vegetação e set dressing |
| TOWN.A1 | `spec_town_native_architecture_v1` | famílias arquitetônicas e pixels nativos; substitui o plano legado de 45 imagens |
| TOWN.C1 | `spec_town_components_doors_and_collision_v1` | portas, interiores e censo físico por instância |
| TOWN.N1 | `spec_town_npc_visuals_navigation_v1` | dois sprites finais, rotas, progresso, portas e recuperação de travamento |
| TOWN.G1 | `spec_town_integrated_acceptance_v1` | runtime, transições, agenda, clima/luzes, performance e score independente >=80 |

`spec_town_layout_v9_organic.md` permanece histórica e não pode reverter 160×112.
`spec_town_building_visuals.md` fica superseded por TOWN.A1: seu inventário histórico é útil, mas a
meta fixa de 15 kits/45 imagens não é aceita sem prova de necessidade e favorece volume sobre qualidade.

## Ordem e locks

1. TOWN.V1 possui layout/chão/água/vegetação/props; TOWN.A1 possui fachada/roof/vão/manifesto; TOWN.C1
   possui frames/timing, interiores, colliders e feedback. Candidatos sem paths comuns podem ser paralelos.
2. Wiring A1 ocorre após freeze de posições V1; regeneração Unity é sempre serial e coordenada com Farm.
3. TOWN.C1 inicia após fachada/vão/folha aprovados e posições estabilizadas.
4. TOWN.N1 inicia após colliders e portas finais, para testar rotas reais.
5. TOWN.G1 é exclusivamente gate; não implementa correções silenciosas.
6. Antes de TOWN.V1/A1, congelar a baseline de performance usada pelo gate G1.

## Critérios de saída do refinamento

- [x] Problemas observados foram separados por owner e dependência.
- [x] Specs antigas conflitantes foram identificadas e receberam sucessão explícita.
- [x] Score visual, física, comportamento e performance possuem gates separados.
- [x] IDs, conteúdo, dimensão 160×112 e escala +20% permanecem invariantes.
- [x] A execução pode ocorrer incrementalmente sem declarar >=80 antes do gate final.

## Revisão independente

Game-design review de 2026-09-11: **ACCEPT**, após reconciliar 160×112/ortho58, separar ownership
V1/A1/C1, preservar horário de lojas e snap offscreen, tornar colisão/navegação/performance binárias e
reservar o score global exclusivamente para TOWN.G1.
