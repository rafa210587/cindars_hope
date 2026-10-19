# Human Play Mode Scenario — Gameplay Expansion Slice (2026-06-12)

Pré-requisitos (Unity Editor, fora de Play Mode):
1. Rodar `CindarsHope/Create Scenes/Town Scene`
2. Rodar `CindarsHope/Create Scenes/Farm Scene`
3. Rodar `CindarsHope/NPCs/Rebuild Town NPC Dialogues (Expanded)`

## A. Projéteis (FarmScene ou CaveScene)
- [ ] Equipar arco + flechas; atirar (mão da flecha): flecha visível com rastro curto, voa na direção do facing, some no alcance.
- [ ] Equipar magia; lançar: orbe colorido pelo tipo de dano (fogo laranja, gelo azul...), pulsando, com trail.
- [ ] Acertar inimigo: dano aplicado, projétil some (sem pierce).

## B. Skills ativas (1-4)
- [ ] Equipar `Corte Giratorio` em um slot; usar perto de 2+ inimigos: ambos tomam dano (360°); stamina desce; cooldown ~6s no slot.
- [ ] Equipar `Leque de Flechas`: 3 projéteis em leque.
- [ ] Equipar `Perfurador em Linha`: projétil atravessa múltiplos inimigos.
- [ ] Equipar `Faisca de Fogo`: consome mana; orbe de fogo.
- [ ] Equipar `Kit de Emergencia`: HP sobe +30; cooldown longo (45s).
- [ ] Sem stamina/mana suficiente: skill falha com feedback e NÃO entra em cooldown.

## C. Caverna — densidade e comportamentos
- [ ] Entrar na caverna pela entrada da FarmScene (boca de pedra escura, prompt "Entrar na caverna").
- [ ] Nível 1-3: contagem de inimigos visivelmente maior que antes (16+ por nível).
- [ ] Sair e re-entrar no mesmo nível (mesma run): mesmos inimigos nas mesmas posições (stable run).
- [ ] Observar comportamentos: inimigos ranged mantêm distância e atiram projéteis esquiváveis; algum inimigo recua ao ficar com pouca vida; guardas voltam ao posto; patrulhas não derivam para longe do spawn.
- [ ] (Se houver enemy data com BurrowAmbush/Leaper/PhaseShortBlink) verificar submersão semi-transparente, saltos em arranque e teleporte de flanco.

## D. Mercador errante
- [ ] Descer níveis até encontrar o mercador (~1 a cada 4-5 níveis): aviso "Um mercador errante montou banca neste andar...".
- [ ] Comprar uma oferta (ouro desce, item entra no inventário); vender itens no ponto de venda.
- [ ] Sair e voltar ao mesmo nível: mercador no MESMO lugar com MESMAS ofertas.
- [ ] Morrer/novo run: níveis com mercador mudam.

## E. TownScene
- [ ] Praça central: estátua do guerreiro (pedestal, corpo, espada bastarda erguida, escudo redondo, placa); pedestal bloqueia movimento.
- [ ] NPCs espalhados por distritos (templo NW, mercado N, ferraria W, oficina SE, mercado noturno S, estrada da caverna E, curral NE).
- [ ] Cada NPC vendedor tem barraca com toldo da sua cor atrás dele.
- [ ] 12 casas com telhado/porta; 24 árvores; lampiões; poço; cercas do curral.
- [ ] NPCs andarilhos permanecem perto de suas zonas (não convergem ao centro).
- [ ] Portal para a fazenda no portão sul (0, -13.5).

## F. Diálogos
- [ ] Falar com NPC de diálogo (ex.: Liora, Maelor, Alaric): hub com 6 opções (quem é você / cidade / trabalho / conselho / rumor / adeus); ramos com continuação; falas variam ao repetir (random pool).
- [ ] Falar com NPC de loja (ex.: Renko): após a fala de abertura, menu com Conversar/Comprar/Vender/Adeus; "Conversar" abre a árvore; em "Em que voce trabalha?" a opção "Mostre o que voce vende." abre o painel de compra.
- [ ] Thalindra: fluxo de quest inalterado (oferta/entrega de suprimentos + comprar/vender).

## G. FarmScene
- [ ] 24 canteiros em dois campos dentro da zona de cultivo ampliada; plantar/regar/colher funcionando no canteiro 0 (slice sequencial).
- [ ] Entrada da caverna a oeste com visual de boca de caverna; interagir transita para CaveScene no spawn correto.
- [ ] Venda no SellPoint e transição para a cidade continuam funcionando.

Resultado esperado: tudo acima OK → promover o slice para PLAYMODE_VALIDATED no registro.
