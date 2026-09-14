---
doc_type: refinement
status: inbox
source_of_truth: false
implemented_by: ""
superseded_by: ""
must_be_self_contained_in_spec: true
read_by_executor_by_default: false
---

# Skills fase 21: arbitragem entre Senso de Mercado e canais de venda

> 2026-09-13. Proposta de design para AC05 da
> [spec aprovada](../../../.specs/a_implementar/spec_skills_21_balance_acceptance_v1.md).
> Não altera preços nem autoriza edição de runtime. O PlayMode de 2026-09-13 confirmou
> o ciclo pão do Renko → SellPoint materializado na FarmScene em
> `Logs/skills-phase21-renko-materialized-economy-playmode-20260913.xml`.

## Problema reproduzível pelas regras atuais

**Limite de acesso descoberto em 2026-09-13:** o cálculo 17g→20g foi executado
em estado de preço isolado; `crafting_shop_sense` hoje exige `crafting_pack_order`,
mas a Mochila Ordenada está corretamente `NotYetExecutable` pela spec 18. O jogador
não consegue comprar Senso de Mercado pelo grafo atual. A correção mínima proposta
é ligar Senso a `crafting_material_eye`, mantendo Mochila dormente e o gate Tier 4;
o catálogo e o asset gerado precisam concordar. Após essa aresta, os orçamentos
teóricos L50 R1+Thoren R1 e L54 R3+Thoren R3 fecham em 27/27 e 29/29 pontos.
O reparo posterior do catálogo/asset foi gerado por `GenerateRepairedSkillNodesOnly` e
o PlayMode de grafo passou 1/1 para compra R1 L50/R3 L54, mas o exploit ainda é
transação controlada de componentes, não jornada Town→Farm do jogador; AC05 continua FAIL.

`crafting_shop_sense` R3 com foco em compra aplica `0,85` ao preço da loja. O pão
`item_consumable_food_bread` tem `BaseValue=20`, está no `Shop_Renko` referenciado pela
TownScene com multiplicador de compra `1,0` e estoque de 5 por dia: compra por **17g**.
`EconomyManager.SellAll` paga
`BaseValue` integral, **20g**; o `SellPoint` da Farm usa o multiplicador MVP configurado, também
integral no perfil atual. O PlayMode executou a compra, o respec e a venda por um componente
`SellPoint` materializado na FarmScene, religado a managers isolados, e `FullRespec` de estado de
preço isolado, sem publicar um evento global de respec: o assert sem lucro falhou
com carteira final de 20g. A margem é **+3g por unidade**, até **+15g por estoque diário** do
Renko, sem produção ou combate. O gate de licença da venda urbana não bloqueia a rota
Farm. A compra e a venda na mesma loja usam `EconomyPricingService` e não têm essa margem;
o teste antigo cobria somente esse caso.

Uma auditoria estática dos 28 assets de loja encontrou 91 pares item/loja: **76 com margem
positiva** nessa rota de compra R3 → SellAll, 14 sem lucro e 1 bloqueado pela política de venda.
A conferência das referências da Town restringiu o universo operacional: **23/28 lojas** estão
ligadas à cena, com **70 pares vendáveis e 27 SKUs distintos**. Em cotações de compra R3 e
revenda neutra por `SellAll`, **56/70 pares** têm margem positiva. São quotes das regras e
assets presentes, não 56 transações executadas nem prova de acesso pelo jogador a cada loja.
A General Store soma até **88g por reposição diária teórica** se todo seu estoque for comprado
e revendido, mas seu asset não tem referência nas cenas/prefabs atuais; não é uma rota
jogável confirmada. O PlayMode de Renko inicializa a sessão de loja na Farm e usa
árvore/respec de estado isolado; não é uma run completa de UI/entrada de Town.
O cálculo leu `ItemDatabaseSO`, `BuyPriceOverride`, multiplicador e arredondamento de
`ShopManager`, estoque e `BaseValue`; é um teto potencial, não uma run que tenha provado acesso
a todas as lojas, ouro inicial ou espaço de inventário.

Uma projeção no mesmo CSV aplicou `max(BuyPriceMultiplier, 1,20)` aos itens sem
`BuyPriceOverride`, sem alterar o asset: **0/91 pares lucrativos somente contra SellAll neutro** e **77/91 preços R3 maiores**, distribuídos por 25 das 28 lojas. Na General Store, pão R3 passaria de 17g a 20g,
poção de HP de 34g a 41g e semente de trigo de 4g a 5g. É uma hipótese de política,
não playtest, e **não fecha AC05**: `EconomyManager.SellAll` aplica o Anel de Finan (+5%)
e `WorldEventHooks.ApplySellGold` soma Lua Âmbar (+10%) e CropPriceSurge (+25%) ao
total vendido, sem filtrar itens de cultivo nesse canal. Na combinação máxima permitida
pelo código, a revenda é `round(round(BaseValue × quantidade × 1,05) × 1,35)`; pão
comprado por 20g no cenário 1,20 ainda poderia render 28g. Confirmar acesso simultâneo
e calendário em PlayMode antes de qualificar o ciclo como reproduzido nessa condição.
A projeção também não cobre modificadores de story/raridade ou a cesta completa de
progressão. A simulação e os 91 pares estão em
`Logs/skills-phase21-price-audit-20260913.ps1`/`.csv`.

Outras rotas precisam constar da matriz: o `FarmShippingService` precifica em `0,95` do valor
base e paga no dia seguinte, enquanto o `PricingProfile` canônico atribui `0,90` ao SellPoint
e `0,70` à recompra da loja genérica. Reduzir apenas SellAll a `0,90` não resolve pão
comprado a `0,85`; o envio a `0,95` também continua. A passiva funciona nas lojas via
`MarketSkillModifierProvider`; sua exclusão dos pontos MVP foi definida na spec 18, mas não
constitui proteção contra revenda cruzada.

## Revisão de design da hipótese de preço (2026-09-13)

O parecer independente classifica o piso universal de `1,20` como **rejeitado para AC05**.
Dos 90 pares vendáveis, 48 apenas empatam na venda neutra. Na projeção de estoque diário
inteiro, 86/90 voltam a lucrar só com Anel de Finan (+5%) e 90/90 com anel e Procura em Alta
(+25%). Lua Âmbar e o evento diário podem coexistir no `WorldEventResolver`; o código aplica
ambos ao lote integral. Oito pães da General Store custariam 160g com R3 no cenário de
`1,20` e renderiam 227g no SellAll com anel e os dois eventos. Esses números são projeções
estáticas, não uma transação PlayMode nesse calendário.

| Política hipotética | Fecha o pior SellAll pelo cálculo? | Custo de progressão / risco |
|---|---|---|
| Comprar com piso `1,20 × BV` | Não; 90/90 pares vendáveis ainda podem lucrar na combinação máxima | Uma unidade de cada item da General Store: 81→98g (+21%); pão+kit: 45→54g, acima dos 50g iniciais |
| Comprar com piso ≥`1,66765 × BV` | Só como limite contínuo preliminar; arredondamento, overrides e lotes requerem matriz | Alta próxima de 67% no preço-base, sem cesta ou renda observada que a sustente |
| Limitar revenda de SKU ofertado em loja pela menor compra R3 | Candidata sem alta geral de compra; precisa de catálogo completo e teto após bônus do lote | Item produzido/coletado com mesmo ID pode perder o prêmio de evento; UI deve explicar o teto |
| Rastrear quantidade comprada e limitar só a procedência varejista | Preserva prêmio dos itens produzidos, se a origem sobreviver a split/merge/transformação | Novo estado em inventário/save, migração e prevenção de lavagem por crafting/gifting |

Há 28 SKUs únicos em 91 pares de loja e **nenhum SKU Crop**. Dez pares têm
`BuyPriceOverride>0`, portanto alterar só `BuyPriceMultiplier` não os alcança. O texto de
`CropPriceSurge` fala em um cultivo, mas o hook de SellAll aplica +25% a todos os itens do
inventário; essa divergência exige critério próprio de correção e teste. O ponto urbano
exige licença de 200g, mas estoque com reposição diária torna um ganho por ciclo repetível.
`SellPoint` da Farm é imediato e usa `1,0 × BV` no perfil materializado atual; Shipping usa
`0,95 × BV` e, com contrato +5%, aproxima `0,9975 × BV` antes do arredondamento. Ordens e
festivais precisam ser tratados como exceções condicionadas/limitadas, não como revenda
ordinária livre.

O parecer independente recomenda o **teto por SKU como contenção v1**, somente após
revisão de payout: **na projeção sobre o catálogo completo de 28 lojas**, dos 27 SKUs
vendáveis listados, 24 ficariam abaixo de BV na venda
neutra, incluindo três peixes capturáveis e itens produzidos. Na mesma hipótese, o peixe
Sael passaria de 66g a 57g (−13,6%) e pão cozinhado de 20g a no máximo 17g (−15%).
**24/27 não é resultado de transação nem de filtro de lojas acessíveis.** Como nenhum cultivo bruto está listado, sua venda direta não sofreria
esse teto; isso não garante que a renda de farm/processamento permaneça equilibrada.
É necessário refazer a contagem com lojas e canais realmente acessíveis antes de usar
essa perda como critério de aceite da política executável.
O efeito de bônus acumulados é maior que a perda neutra: no CSV atual, semente de trigo da
loja de Eiran em lote de cinco cairia de 29g para 20g com Finan+Lua (−31%) e de 35g para
20g com Finan+Lua+CropPriceSurge (−42,9%). Cinco pães do Renko passariam de 116g para 85g
(−26,7%) ou de 142g para 85g (−40,1%) nesses cenários. São **quotes hipotéticos** de um
teto pós-bônus, não vendas executadas; CropPriceSurge hoje afeta itens não-cultivo por
divergência do hook. Mesmo sem esse evento, o teto pode retirar mais de 20% do payout de
itens produzidos. Revisão de design deve aceitar explicitamente essa perda ou substituir
a política antes de qualquer alteração de runtime.
O inventário atual perde qualidade e raridade do peixe capturado; `ShippingBinInteractable`
envia com `qualityTier=0`. Portanto os valores acima não são uma medição de pescado premium
vendido hoje. Uma integração futura que preserve `qualityTier>0` poderia conceder prêmio de
qualidade no shipping, mas um teto por SKU aplicado depois anularia esse prêmio se não houver
exceção explícita por origem/qualidade.
Rastreio de procedência preservaria a renda legítima, mas exige origem por quantidade
durante split/merge, baú, consumo, crafting, shipping e migração de save. Nenhuma das duas
opções corrige a receita atual de madeira processada: uma madeira de BV3 pode virar output
de BV15 com tempo/stamina zero e ainda ser vendida por até 13g no teto hipotético.
Outras cotações condicionais de receita com tempo/STA autorados em zero incluem peixe perch de
12g → grilled fish de 34g e quatro leather somando 52g → armor de 85g. Elas não representam
crafts e vendas executados: dependem de insumos, unlock, estação, inventário e canal de venda.
Essas cadeias precisam de decisão e teste próprios antes de declarar ausência geral de arbitragem.

## Decisão de design proposta

**Decisão humana de 2026-09-13 — procedência comprada.** O jogador escolheu rastrear a
origem dos itens comprados. O teto universal por SKU e o piso global de compra ficam
rejeitados como solução para AC05: cortariam o pagamento de itens produzidos, enquanto
um preço mais alto encareceria sementes e provisões. O draft 21E de teto é histórico de
alternativas; o contrato novo está na spec 21F de procedência. A escolha autoriza o
*direcionamento de design*, não torna testes pendentes PASS nem autoriza um executor a
inventar comportamento para as cadeias de transformação.

**P21-E2 revisada — margem pela procedência, não pelo SKU.** Na compra, guardar por lote
quantidade e ouro total efetivamente pago, já com desconto, arredondamento e override.
Esse custo fica imutável após respec ou troca de canal. Split/merge/remover/soltar/pegar,
save/load, envio e jobs devem conservar a soma de quantidade e custo, inclusive os
restos de divisão de um custo de lote não divisível por unidade. Na venda ordinária,
itens colhidos, pescados ou fabricados sem insumo comprado conservam a cotação normal.
Unidades compradas recebem no máximo o próprio custo atribuído menos 1g **após**
todos os bônus e arredondamentos; compra de 1g exige revenda a 0g ou recusa explícita,
sem o piso global de preço mínimo. O quote exibido e a remoção precisam operar sobre os
mesmos sublotes, com detalhamento do valor reduzido antes da confirmação.

**P21-E6 — transformação e save legado.** Receitas não podem limpar a procedência: o
custo total dos insumos comprados deve acompanhar todos os outputs, mesmo após
save/load, cancelamento ou coleta. A política conservadora v1, recomendada na revisão
independente, não concede prêmio de *revenda* a outputs derivados de insumos comprados,
mesmo que a receita declare tempo/STA positivo, até medir trabalho e renda por dia.
Para um batch misto, a soma das revendas de todos os outputs não pode superar
`custo pago dos insumos comprados + cotação normal de liquidação dos insumos próprios
- 1g`, truncado em zero e aplicado após bônus. Um batch só de insumos próprios mantém
a cotação normal; produtos mistos podem ter revenda pior que a venda separada dos
ingredientes, exigindo aviso antes do craft/venda. O output derivado conserva sua
classificação e teto residual ao entrar em outra receita; split, yield extra, baú e
save/load não podem multiplicar o limite do lote. O item ainda serve para consumo,
equipamento e quests. Eventual prêmio de trabalho positivo é uma futura decisão de
balanceamento ancorada em mediana L20/L50/L54, não número inventado agora.
Até a implementação e o PlayMode dessas regras, AC05 geral fica FAIL. Slots de saves antigos sem
metadado serão `LegacyUntracked` e manterão a cotação normal, exceção finita declarada
para não desvalorizar produção preexistente; toda compra nova no save migrado recebe
origem antes do commit. O canal agregado de metas diárias continua um defeito separado.

**P21-E7 — exceção agrícola e retornos exatos.** A semente comprada conserva seu
lote até o plantio; se stamina/slot/ação falhar, o rollback devolve a mesma origem
e base paga. Após plantio, dias de crescimento, rega e colheita concluídos, o
trigo é produção `Earned` e recebe preço normal. Essa transformação com trabalho
e calendário efetivos não deve herdar o teto conservador de receitas instantâneas.
Kits de reparo devolvidos por falha, itens largados/recolhidos e recuperação de
corpo após morte devem manter os lotes. Encomenda paga de livro precisa levar
custo no pedido pendente até a entrega se o item for revendável. A auditoria
externa encontrou esses caminhos; a spec 21F deve nomear seus owners e DTOs.

**P21-E1 — aceitar AC05 somente em ciclo multicanal real.** Comparar compra com foco R3,
revenda na mesma loja, SellAll urbano, SellPoint Farm e envio com liquidação diária. Incluir
respec/load, estoque persistido, arredondamento de itens baratos e novos dias. Registrar ouro,
item, canal e dia antes/depois; nenhum ciclo repetível pode aumentar ouro. O teto de ganho
passivo `<20%` ainda exige mediana de renda observada por estágio, não uma meta por hora.

**P21-E2 histórica — reconciliar preços antes de tuning pontual.** Preservar o bônus de compra de até 15%
e exigir que o menor preço após desconto seja pelo menos igual ao maior valor de revenda
alcançável para o mesmo item, incluindo bônus acumuláveis e arredondamento por lote.
O candidato inicial de `1,20` fecha apenas a venda neutra; elevar todos os preços até
cobrir eventos pode encarecer desproporcionalmente sementes e provisões. O executor deve
comparar alternativas por SKU/canal e auditar `BuyPriceOverride`, raridade, qualidade,
eventos, itens baratos e quantidade com o mesmo código de pricing usado no jogo.
Esta hipótese foi superada pela decisão de procedência acima. Nenhum multiplicador está aprovado para migração de assets. Medir custo da cesta de
preparo de Cave e início da Farm antes de fixar a política.

**P21-E3 — não neutralizar a skill por um piso oculto.** Um piso que anule o desconto R3 nas
lojas de multiplicador `1,0` tornaria ranks comprados ornamentais, contrariando AC09 da spec 18.
Se a auditoria exigir piso para exceções, cada entrada deve reportar desconto efetivo, razão
e compensação de conteúdo explícita. Bônus temporários de venda, ordens e festivais têm
condições próprias e não devem ser confundidos com uma revenda ordinária ilimitada.

**P21-E4 — separar origem do item e elegibilidade do evento.** A decisão é rastrear
procedência, conforme o bloco acima. Além disso,
`CropPriceSurge` só pode beneficiar os cultivos elegíveis definidos pelo evento; o atual
bônus sobre o lote completo diverge da descrição de um cultivo. Definir a ordem do teto,
dos bônus de Anel/Lua/evento e do arredondamento por item/lote antes do código. Nenhum
canal deve ocultar no feedback ao jogador por que pagou menos que o valor exibido.

**P21-E5 — protocolo de mediana diária, amostra controlada L20 executada.** Usar três dos dez canteiros de trigo
em plantio escalonado, mantendo a mesma área e as mesmas ações nos cenários legais L20,
L50 e L54. Registrar D0–D2 como preparo (compra das sementes no Renko, plantio, regas,
tempo/STA e despesas) e D3–D5 como três amostras diárias observadas de colheita, venda no
SellPoint da Farm e replantio. Registrar ouro antes/depois, quantidade, qualidade, custo da
semente e payout por dia; calcular mediana por estágio e diferença com/sem build/passivas.
Somar em coluna própria as recompensas de metas diárias disparadas por plantio/colheita/venda,
pois o `FarmDailyGoalService` credita ouro/XP ao jogador global mesmo quando um diagnóstico
usa outra carteira para a transação; incluir quests/eventos de dia como fontes separadas.
R3 de Senso de Mercado é ilegal em L20 e só entra nos estágios em que a compra da skill é
possível. A janela L20 abaixo é observação controlada; L50/L54 e o teto de 20% continuam proposta de medição, não aceitação de AC05.

O PlayMode dedicado r2 executou três canteiros escalonados em D0–D5 e passou 1/1 com
scan de log PASS (`Logs/skills-phase21-steady-income-l20-r2-20260913.xml/.log`). Em
D3–D5, cada dia observou cinco trigos `Excellent`, venda de 60g e compra da próxima
semente por 5g: mediana transacional isolada de 55g/dia. As duas metas diárias
creditaram 35g/dia à carteira global, reconciliados por ID/evento; mediana composta
controlada de 90g/dia. O rollover `AdvanceDay` ocorre após fechar cada linha; a
fixture não inclui possível renda gerada nesse rollover, logo 90g não é renda
global exaustiva. O JSON/CSV está em
`Logs/skills-phase21-steady-income-l20-three-plots.*`; 17 hashes de insumo no
manifesto `Logs/skills-phase21-steady-income-l20-r2-20260913-inputs.sha256` foram
capturados após o teste. Com compra R3 legal em estágio alto, semente de 4g projetaria
56g líquidos, ainda sem observação. `docs/game_rules/farm_rules.md`
ainda descreve três trigos/50g e diz que rega não está implementada no MVP, em conflito com
o rendimento vivo `+2` em `Excellent` após regas exigidas. Reconciliar a regra antes de usá-la
como baseline. A UI atual de `FarmPlot` depende de `Input.GetKeyDown` para plantar/colher e
não expõe comando público à fixture; os testes verdes de um e três canteiros usam
pontes internas e não comprovam a jornada Town/UI nem as medianas L50/L54.
O teste integrado de Thoren expôs uma recompensa indevida de 20g: venda de espada via
`ShopSell` concluía `daily_goal_sell_first_crop`. O bugfix passou EditMode 22/22 e
PlayMode L50 pós-fix, no qual base e variante q1 pagam 30g por transação e
30g por delta de carteira cada. `SellPoint`, `SellAll` e `shipping` continuam
agregados sem IDs por item e podem acionar a meta sem cultivo; a renda observada
deve separar esses prêmios até o contrato de evento ser corrigido.
Dois PlayModes diagnósticos r3 rodaram em processos Unity separados (1/1 PASS cada): payout
base de 3 trigos +36g na carteira isolada, com +20g de meta na carteira global; ciclo de
um plot D1→D4 com semente Renko 5g, colheita de 5 trigos e venda por 60g, ou +55g na
carteira isolada após a semente, além de +35g de duas metas na carteira global.
O teste restaura carteira/progressão/metas, mas a virada avança quest/clima; não soma esses
valores como renda diária total nem reutiliza o mesmo processo para outra amostra.

## Escopo e aceite de um amendment

A spec 21 aprovada exige ausência de lucro no ciclo, mas seu ownership de tuning limita mudanças
a `SkillActionSO`/nó/perfil. A correção de `InventorySlotOperations`, DTOs e migração de save,
`ShopManager`, `EconomyManager`, `SellPoint`, shipping, crafting e processamento precisa de
spec econômica própria. O pacote 21F deve definir contratos de procedência, arquivos
permitidos, política de migração e testes de regressão Farm/Town/Cave. A decisão atual
não autoriza editar assets de preço nem usar o draft 21E como comando de implementação.

Depois do reparo do grafo, quatro PlayModes separados observaram a mesma janela
controlada de trigo L50/L54: base 90g/dia em ambos, Senso R1+Thoren R1 90g/dia
em L50 e Senso R3+Thoren R3 91g/dia em L54, com 27/27 e 29/29 pontos. A rotina
usa semente, canteiros e SellPoint; não executa Thoren nem cobre renda global.
O teste de exploit falhou com evidência real e AC05 fica **FAIL**; a mediana
diária **global** e o limite de 20% ficam **NOT RUN** até existirem runs representativas. Um teste verde de
shop→shop ou de fórmula pura não promove o equilíbrio econômico.
