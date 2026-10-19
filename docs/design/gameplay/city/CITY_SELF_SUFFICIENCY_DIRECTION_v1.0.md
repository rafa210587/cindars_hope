# Cindar's Hope — Direção de Autossuficiência da Vila (v1.0)

> **Tipo:** Documento de direção de design (não é spec executável)
> **Propósito:** Racionalizar se a vila se sustenta como assentamento e fechar as lacunas — papéis,
> cadeias de produção, estações, casas e NPCs novos — para algo **completo e funcional**.
> **Status:** Direção aprovada pelo usuário (alvo híbrido: a vila depende de você, cresce com você e
> tem um mínimo de simulação sustentável). Implementação posterior via specs.
> **Fontes auditadas:** ItemCategory + catálogo de itens, receitas de craft (WorkshopType), FarmAnimalCatalog,
> Shop_*.asset, NpcTownRosterRegistry, CreateMvpTownScene. **Última atualização:** 2026-06-23.

---

## 0. Decisão de design — a raiz que define tudo

**Quem é o artesão: o jogador OU a vila?** → **Híbrido com o jogador no centro.**

- O **jogador é o artesão principal**: cozinha, forja, alquimia, curte, tece e mói **nas estações** que
  ficam dentro das casas-ofício. (Padrão farm-sim; o jogo já funciona assim.)
- Cada **NPC artesão tem identidade de produtor explícita** e **vende** o resultado do seu ofício, para a
  vila parecer viva mesmo quando você não está craftando.
- Um **mínimo de simulação**: as cadeias se fecham com **fornecedores** (você abastece a matéria-prima e
  destrava o tier do artesão) — slice futura `spec_village_supplier_chains`. Sem simular moeda interna.

Consequência: o conserto **não é inflar a cidade de NPCs**, é (1) **fechar 3 cadeias quebradas**,
(2) **dar estação visível** a cada casa-ofício, (3) **dar identidade de produtor** a quem já existe, e
(4) **adicionar 4 NPCs** que cobrem buracos reais (peixe, pão, couro, zelo dos mortos) — cada um com
gancho e profundidade própria.

---

## 1. Veredito — matriz de cobertura do assentamento

| Função de assentamento | Quem cobre hoje | Status |
|---|---|---|
| Água | Poço (TownWell) | ✅ |
| Lavoura | Sylveth + fazenda do jogador (12 crops) | ✅ |
| Criação animal | Eiran (galinha/cabra/vaca) | ✅ |
| Pesca | sistema de pesca (10 peixes) | ⚠️ **sem pescador/comércio** |
| Cozinha / pão | 19 receitas (jogador) + Estalagem | ⚠️ **sem cozinheiro nem padeiro explícitos** |
| Metal / ferramentas | Brumdar (ferreiro) + Dagna (minas) | 🟡 **sem forja visível na cena** |
| Alquimia / remédio | Ozzra, Thalindra, Savra, Corvus (12 receitas) | 🟡 **sem alambique visível** |
| Construção | Nimble, Gurd, Hund | ✅ |
| Defesa / lei | Alaric, Hund, Câmara, Prisão | ✅ |
| Governo | Velorin, Mara/Tovin | ✅ |
| Fé / morte | Corvus + Templo / Cemitério | 🟡 **cemitério sem zelador → dar a Corvus** |
| Comércio | Pip, Zrix, Yael, Orlan, Renko | ✅ |
| Cultura | Liora, festivais, Maelor | ✅ |
| **Couro (tanoaria)** | Mirela **vende**, ninguém **produz** | ❌ **cadeia quebrada** |
| **Tecido / roupa** | estação `Sewing` com **0 receitas** | ❌ **cadeia quebrada** |
| **Lã** | item declarado, **sem ovelha** | ❌ **órfã** |

**Veredito:** ~65% crível. Sustenta o essencial; cai em 3 cadeias quebradas + ofícios sem oficina visível.
Com os consertos abaixo, vai a ~90% **sem inchar a cidade**.

---

## 2. As 3 cadeias QUEBRADAS e os consertos

| Cadeia | Hoje | Conserto |
|---|---|---|
| **Couro**: hide → couro → armadura de couro | armadura de couro só é **comprada**; couro-material não é produzível | receita **Curtir** (hide → couro) + receita couro → armadura leve; hide vem de animais (Eiran) e drops; estação **Bancada de Curtir** na **Tanoaria do Hess** (curtidor novo) |
| **Tecido**: fibra/lã → tecido → roupa/robe | `WorkshopType.Sewing` existe com **0 receitas**; robes só na loja | receitas **Sewing**: fibra/lã → tecido → roupa de pano / robe; estação **Tear** na casa da **Mirela** (tecelã/alfaiate) |
| **Lã**: ovelha → lã → tecido | item lã existe, **sem animal** | adicionar **Ovelha** ao `FarmAnimalCatalog` (já descrita no guia de arte!) → fecha a cadeia de tecido |

> As três se resolvem com **receitas + 1 animal + 2 estações** — não exigem NPC novo (Mirela cobre couro+tecido).

---

## 3. Identidade de PRODUTOR por NPC (dar função ao que já existe)

Cada artesão ganha um ofício explícito + uma **estação visível** na casa dele (o "as casas têm a mesma lógica"):

| NPC | Ofício explícito | Estação na casa | Produz (jogador crafta) | Vende |
|---|---|---|---|---|
| Brumdar | Ferreiro | **Forja + bigorna** | armas/ferramentas/armadura de metal | minério, lingote, ferramentas |
| Dagna | Mineradora | **Pilha de minério + bancada de pedra** | (fornece matéria-prima) | minério, pedra, gema, picareta |
| Ozzra | Alquimista | **Alambique + prateleira de frascos** | poções, óleos | reagentes, poções, óleos |
| Nimble | Carpinteiro/Engenhoqueiro | **Bancada de carpintaria** | madeira processada, móveis, upgrades | madeira, ferramentas, suprimentos |
| **Mirela** | **Tecelã / Alfaiate** | **Tear** | tecido, roupa de pano, robe | tecido, roupas, fibra/lã |
| **Hess** (NOVO) | **Curtidor** | **Bancada de curtir + tanques de tanino** | couro, armadura leve | couro, hides (animal e caverna) |
| **Eiran** | Tratador + **Laticínio** | **Curral + batedeira/queijaria** | ovo, leite, lã, **queijo** | filhotes, ração, laticínio |
| **Gruta** | **Cozinheira/Taberneira** | **Fogão + barris de fermentação** | comida cozida + **cerveja/vinho** | comida, bebida, cama (estalagem) |
| Sylveth | Plantadora/Herbalista | **Canteiros + secador de ervas** | (fornece sementes/ervas) | sementes, ervas, poções básicas |
| Corvus | Padre/Curandeiro (cemitério zelado por **Tibbet**) | **Altar (templo)** | (cura/serviço) | itens de cura, relíquias |
| **Tibbet** (NOVO) | Coveiro + coroinha (segredo: adora Nyx) | **Sacristia + cova** | (enterros, bênçãos, quest de fé) | — |
| Savra | Curandeira/Herbalista | **Mesa de ervas** | (remédios) | medicinas, ervas raras |
| Thalindra | Pesquisadora | **Estantes/biblioteca (arquivo)** | (conhecimento) | pergaminhos, identificar |
| Mara / Tovin | Escribas / Registro | **Mesa de registros (Câmara)** | (permissões) | livros, documentos, escrituras |
| Alaric / Hund | Guarda | **Posto/quartel** | (defesa) | — (Hund vende armadura) |
| Velorin | Líder/Ancião | **Mansão / Câmara** | (governo, encomendas) | — |
| Pip/Zrix/Yael/Orlan/Renko | Mercadores (mundo externo) | banca / loja | (trazem bens de fora) | bens importados, raridades |
| Liora | Música | praça/estalagem | (cultura) | — |
| Maelor | Explorador/lore | cemitério (noturno) | (mistério/quests) | — |

> Nenhum NPC acima é novo. O que muda: **identidade de produtor declarada** + **estação visível na casa**.

---

## 4. NPCs NOVOS — 4 (cada um com a riqueza dos demais: história, segredo, personalidade)

> **Regra de elenco:** todo NPC novo segue a profundidade dos existentes — gancho/segredo, raça e lore de
> Vaalara, personalidade e aparência específica. Nada de "NPC-função genérico".

### 4.1 Sael Maré-Quieta — Pescador / Peixeiro (Tiefling)
- **Raça:** Tiefling (pele azul-noite) — **primeiro tiefling nomeado da vila** (o guia de arte descreve a raça, nenhum existia).
- **Objetivo:** elo da **pesca** — abastece peixe, habilita a pesca, e dá ao distrito do **Lago (SW)** um dono e função.
- **História/gancho:** subiu o rio anos atrás, vindo dos pântanos do sul; o lago lembra um lar que ele se recusa
  a nomear. Lê a água como quem lê rostos — jura que "o lago lembra o que se afogou nele". Todo crepúsculo deixa
  um peixe sobre uma pedra lisa: uma oferenda a um espírito-do-rio que mais ninguém honra.
- **Personalidade:** silencioso, paciente, seco; gentil por baixo. Contraponto sereno aos mercadores agitados.
- **Estação/casa:** cabana sobre estacas + **doca de pesca** + **defumador**, à beira do lago.
- **Vende:** peixe, isca, varas, peixe defumado. **Compra:** pescado cru (inclusive de caverna).

### 4.2 Mella Forno-Quente — Padeira / Moleira (Humana de Mana)
- **Raça:** Humana de Mana, dornécia.
- **Objetivo:** fecha **grão → farinha → pão**; âncora de comida do **distrito de mercado**; laço direto com a fazenda.
- **História/gancho:** viúva que veio recomeçar; o pão dela é a cola silenciosa da vila — dá os pães não vendidos
  do dia a quem parece com fome, e **se recusa a falar disso**. Guarda a mó do marido morto, que arrastou por três
  províncias; no aniversário da morte dele assa o dobro e chora quando ninguém vê.
- **Personalidade:** acolhedora, feroz em alimentar os outros, riso fácil sobre um luto que não mostra.
- **Estação/casa:** **Padaria + Moinho** — **mó** (grão→farinha) + **forno de pão**.
- **Vende:** farinha, pão, doces, bolo de festival. **Compra:** trigo/thandra_wheat, ovo, leite.

### 4.3 Hess Couro-Fundo — Curtidor (Draconato terroso) — split do couro de Mirela
- **Raça:** Draconato de escamas marrom-terra (distinto de Zrix cobre e Savra verde).
- **Objetivo:** fecha a cadeia do **couro** (hide → couro → armadura leve), em **parceria com Mirela** (ele curte,
  ela costura e forra) — uma relação entre dois NPCs, não dois ofícios soltos.
- **História/gancho:** o mais velho da vila pela própria conta; fala devagar, em provérbios. Curte as peles das
  feras que o jogador traz da caverna e **"agradece" cada pele** — acredita que toda pele guarda a memória da
  criatura. Vive um pouco à margem, pelo cheiro do ofício e por gosto pelo silêncio.
- **Personalidade:** paciente, sábio-rústico, humor seco; respeita quem trabalha com as mãos.
- **Aparência:** escamas marrom-terra opacas, placas gastas, olhos âmbar pálidos; avental de couro rígido marcado
  pelo tanino; mãos enormes e calejadas.
- **Estação/casa:** **Tanoaria** na borda da cidade — **bancada de curtir + tanques de tanino + bastidores de pele**.
- **Vende:** couro, armadura leve, tiras/correias. **Compra:** hides de animais e de feras da caverna.

### 4.4 Tibbet Vela-Torta — Coveiro e Coroinha; adorador SECRETO de Nyx (Gnomo) — auxiliar de Corvus
- **Raça:** Gnomo (distinto de Tovin).
- **Objetivo:** auxiliar do **Padre Corvus** — serve no altar do Templo de dia (coroinha) e cuida do **cemitério**
  (coveiro). Resolve o "cemitério sem zelador" sem tirar a fé de Corvus.
- **História/gancho (o segredo dele):** ama Corvus e o trabalho com sinceridade — mas **adora Nyx em segredo**, uma
  pequena heresia debaixo do nariz do padre de Kanthor. Acredita que os mortos pertencem a **Nyx (a Noite)**, não a
  Kanthor (a Ordem); faz ritos minúsculos e privados sobre as covas à noite. Vive dividido entre a lealdade ao
  mestre e a fé proibida — tensão dramática constante. Uma **luna negra** (símbolo de Nyx) escondida sob a gola.
- **Personalidade:** doce, prestativo, fala mansa; culpa e devoção em equilíbrio frágil.
- **Aparência:** gnomo pequeno, pele pálida, grandes olhos escuros olheirentos (dorme de dia, vela os mortos de
  noite); túnica branca de coroinha com a bainha sempre manchada de terra de cova; uma vela que vive entortando e
  pingando cera.
- **Estação/casa:** dorme numa **sacristia/casinha do coveiro** anexa ao Templo/cemitério.
- **Vende:** — (serviço: enterros, bênçãos; gancho de quest de fé/segredo). **Compra:** —

> Total: 24 → **28 NPCs**. Cada um fecha um buraco real (peixe, pão, couro, zelo dos mortos) E carrega gancho próprio.
> Estábulo/montaria e escola seguem para o futuro (escola ≈ Arquivo de Thalindra).

---

## 5. Lógica das CASAS (cada casa = morador + função + estação)

Esta é a "mesma lógica para as casas": toda casa-ofício ganha a estação correspondente; residenciais puras
ficam aconchegantes. (Nomes batem com `TownHouseSpecs` no gerador; + 2 prédios novos.)

| Casa | Morador(es) | Função | Estação/Conteúdo |
|---|---|---|---|
| House_Temple (Igreja) | Corvus | fé + cura + cemitério | altar, bancos, sino |
| House_Manor (Mansão) | Velorin | liderança | escrivaninha, mapa da vila |
| House_Chamber (Câmara) | (cívico; Mara/Tovin trabalham) | governo/registro/**encomendas** | mesa de registros, quadro de encomendas |
| House_Prison | (Alaric/Hund vigiam) | lei | cela, grades |
| House_MarketHall (Mercado) | — | comércio | bancas, balcões |
| House_Inn (Estalagem) | Orlan + Gruta | cozinha/cerveja/cama | **fogão + barris** + camas |
| House_Blacksmith | Brumdar | ferraria | **forja + bigorna** |
| House_AlchemyLab | Ozzra | alquimia | **alambique + frascos** |
| House_Workshop | Nimble | carpintaria | **bancada de carpintaria** |
| House_Residential_3 (Mirela) | Mirela | **tecelagem/alfaiataria** | **tear** |
| House_AnimalYard | Eiran | criação + laticínio | **curral + queijaria** |
| House_Archive | Thalindra | conhecimento | estantes, esfera mágica |
| House_Registry | Mara | escriba | mesa de selos/documentos |
| House_Tovin | Tovin | escriba/selos | carimbos, prensa |
| House_Dagna | Dagna | mineração | pilha de minério, picareta |
| House_GateKeeper | Alaric | guarda do portão | posto, armas em rack |
| House_CarvalhoTorto | Gurd + Hund | construção/guarda | ferramentas de obra |
| House_Residential_1 (Sylveth) | Sylveth | herbalista | canteiros, secador de ervas |
| House_Residential_2 (Renko) | Renko | mercador | mostruário |
| House_Residential_4 (Savra) | Savra | curandeira | mesa de ervas |
| House_Pip | Pip | mercador jovem | mochila/mostruário |
| **House_Fishery** (NOVO) | **Sael** | pesca | **doca + defumador** |
| **House_Bakery** (NOVO) | **Mella** | pão/moagem | **moinho + forno** |
| **House_Tannery** (NOVO) | **Hess** | curtume | **bancada de curtir + tanques de tanino + bastidores** |
| Sacristia/casinha do coveiro (anexo ao Templo) | **Tibbet** | coveiro + coroinha | sacristia, ferramentas de cova, vela |

Ao relento (sem casa, por design): Maelor (cemitério), Zrix (gruta), Yael (tenda noturna), Liora (jardim).

---

## 6. Cadeias de produção fechadas (o "funcional")

```
Grão (trigo/thandra)  → [Moinho: Mella] → farinha → [Forno: Mella/jogador] → pão → [Fogão: Gruta] → refeições
Animais (Eiran)        → leite/ovo/lã → [Queijaria: Eiran] → queijo ; [Fogão] → omelete, etc.
Ovelha (NOVA)          → lã → [Tear: Mirela] → tecido → [Costura] → roupa/robe
Caça/animais           → hide → [Curtir: Mirela] → couro → [Costura] → armadura leve
Minério (Dagna/caverna)→ [Forja: Brumdar] → ferramentas/armas/armadura de metal
Ervas/monstros         → [Alambique: Ozzra] → poções/óleos
Madeira                → [Carpintaria: Nimble] → madeira processada/móveis/upgrades de prédio
Peixe (Sael/lago/caverna) → [Defumador: Sael] / [Fogão] → peixe cozido/defumado
Uva                    → [Barris: Gruta] → vinho/cerveja
```

Cada seta tem agora **produtor + estação + vendedor**. O jogador opera as estações; o NPC dá a identidade e
vende o excedente. **Loop fechado** (slice de fornecedor liga a sua produção ao tier das lojas).

---

## 7. Assets a gerar (cross-ref ao Guia de Imagens)

Adicionar ao `docs/design/art/ART_DIRECTION_ILLUSTRATOR_GUIDE.md`:

**Personagens (4 portraits/sprites novos — cada um com aparência rica no guia):**
- Sael Maré-Quieta (Tiefling pescador).
- Mella Forno-Quente (Humana padeira).
- Hess Couro-Fundo (Draconato terroso curtidor).
- Tibbet Vela-Torta (Gnomo coveiro/coroinha; luna negra de Nyx escondida).

**Prédios novos (4):**
- Fishery / cabana de pesca (doca + defumador) à beira do lago.
- Bakery / moinho (mó + forno) perto do mercado.
- Tanoaria (bancada de curtir + tanques de tanino + bastidores) na borda da cidade.
- Sacristia/casinha do coveiro anexa ao Templo/cemitério.

**Animal novo (já descrito no guia):**
- Ovelha (lã) — sprite já especificado; falta só adicionar ao FarmAnimalCatalog.

**Props de estação (dentro das casas-ofício):**
- Forja+bigorna, alambique, bancada de carpintaria, **bancada de curtir**, **tear**, fogão, **barris de
  fermentação**, **batedeira/queijaria**, **moinho**, **forno de pão**, **doca/defumador**, altar,
  mesa de registros, secador de ervas, pilha de minério.

**Ícones de item (receitas novas):**
- couro (material), tecido/pano (material), roupa de pano / robe simples, queijo (já existe goat_cheese —
  estender), farinha, e variações de pão.

---

## 8. Roadmap de implementação (specs futuras — NÃO implementar agora)

| Ordem | Spec | Entrega |
|---|---|---|
| 1 | `spec_city_artisan_stations` | colocar as estações visíveis nas casas-ofício (gerador) |
| 2 | `spec_closed_chains_leather_cloth_wool` | receitas de curtir/costura + ovelha no FarmAnimalCatalog |
| 3 | `spec_npc_fisher_and_baker` | criar Sael + Mella (NpcDataSO, diálogo, loja, casa, schedule, roster, arte) |
| 4 | `spec_village_supplier_chains` | abastecer matéria-prima destrava tier do artesão (o "mínimo de simulação") |
| 5 | `spec_village_orders_board` + `spec_village_reputation` | demanda + laço (já planejado; secundário à fazenda) |
| 6 | `spec_town_development` | financiar melhorias (a vila cresce com você) |

> Nada acima é executado nesta direção. Cada item vira spec própria quando você mandar.

---

## 9. Decisões fechadas (2026-06-23)

1. **Mella (padeira/moleira) entra.** ✅ Fecha grão→farinha→pão e ancora o mercado.
2. **Mirela dividida.** ✅ Mirela = **tecelã/alfaiate** (tecido); entra **Hess Couro-Fundo** = **curtidor** (couro). Parceria entre os dois.
3. **Coveiro = auxiliar do Corvus.** ✅ Entra **Tibbet Vela-Torta** (gnomo): coveiro + coroinha, **adorador secreto de Nyx** (heresia sob o nariz do padre de Kanthor — gancho dramático).
4. **Estábulo/montaria = futuro.** ✅ Adiado para quando houver transporte/montaria.

**Resultado:** 24 → **28 NPCs** (Sael, Mella, Hess, Tibbet). Todos com gancho/segredo próprio, no nível dos demais.
