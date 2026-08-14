# Keyart de referência da fazenda (APROVADA)

`farm_keyart_layout_aprovado_v1.png` (1672×941) — keyart **aprovada pelo usuário em 2026-08-14**
como o alvo visual/estrutural da FarmScene. Gerada no ChatGPT (projeto "Sprites - Fazendeiro",
chat "Recuperação de mapa fazenda"), estilo Stardew Valley + Children of Morta, top-down 3/4.

> **Regra:** todo ajuste visual da fazenda deve ser validado contra esta imagem. Ver memória
> `feedback-validate-against-farm-keyart`.

## Mapa keyart → coordenadas do jogo (grid 64×44, origem centrada, x∈[-32,32] leste+, y∈[-22,22] norte+)

| Região na keyart | Elemento | Coord no código (CreateMvpFarmScene) |
|---|---|---|
| Topo, faixa inteira | Muralha de montanha/penhasco | MountainBarrier y[18,22] |
| Topo-esquerda | Entrada da caverna/mina + quadro do Zrix | CaveEntrance (-28,18.5); ZrixBoard (-22,16) |
| Topo-centro | Veios de minério bloqueados | LockedOre (-18,-9,0,9 @ y18.5) |
| Topo-direita | Nascente + cachoeira do rio | RiverSpring (22,18) |
| Borda direita (topo→baixo) | Rio + ponte → lago | River east; Bridge (21,3) |
| Direita, parte alta | Casa + estufa (homestead) | House (28,9); Greenhouse (24,10) |
| Direita, fileira abaixo da casa | Bancada/forja/forno + envio + venda + quadro evolução | Workbench(25,1) Forge(27,1) Cooking(29,1); ShippingBin(28,4); SellPoint(24,5); EvolutionBoard(30,4) |
| Canto inf-direito | Lago + píer/pesca | Lake centro (18,-13), x[5,31] y[-20,-6]; Fishing (8,-9) |
| Esquerda (topo→meio) | Floresta densa de pinheiros | Bosque oeste x[-32,-22] y[0,17] |
| Esquerda, clareira | Fonte da Anya (glifo Três Águas, brilho azul) | FonteAnya (-24,4) |
| Esquerda-baixo | Forrageio (moitas/cogumelos/tocos) + expansão | Forage (-31..-29, -2..-6); Exp_West (-30,-10) |
| Borda inferior, centro-esq | Galinheiro + celeiro + queijaria + barril | Coop(-22,-19) Barn(-13,-19) CheesePress(-28,-19) WineBarrel(-8,-19) |
| Centro | Hortas/canteiros de cultivo | FarmPlots (~1,-2.25) |
| Borda leste-meio | Portão/saída pra cidade | Town portal (31,-1) |

## Desvios menores da keyart vs jogo (aceitos)
- Veios de minério no topo ficaram discretos (o jogo tem pedras bloqueadas explícitas).
- Portão de saída leste não está explícito na keyart (só a ponte).
