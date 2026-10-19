# Cindar's Hope — NPC Gift Taste Matrix v1.0

> **Status:** direção ativa de gostos de presente por NPC (loved / liked / neutral / disliked / hated).
> **Local:** `docs/design/gameplay/city/NPC_GIFT_TASTE_MATRIX_v1.0.md`
> **Origem:** decisão **2.4 (A + CUSTOM)** de `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md` + acréscimo **2** da re-auditoria de código (2026-06-13).
> **Depende de (canon de elenco):** `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md` (23 NPCs, IDs estáveis, deuses, classes, relações).
> **Depende de (vocabulário de item/tags):** `docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md` (artefato A4 define os `gift_*` e as tags `gift_*`).
> **Alinha com:** `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md` §8 (categorias de preferência: Loved/Liked/Neutral/Disliked/Hated/Forbidden).
> **Consumida por:** `fable_26_spec_friendship_state_contract` (a amizade por presente LÊ o gosto desta matriz; ver emenda 2026-06-13-V3 na fable_26).
> **Não é spec implementável.** O wiring de runtime (leitura/escrita/geração/validação da preferência) é débito de implementação registrado na §6 abaixo.

---

## 0. Por que este documento existe

A decisão **2.4 (CUSTOM)** descartou o modelo "presente genérico = +3 fixo" da fable_26.
Cada NPC passa a **reagir de forma diferente** a presentes, em cinco níveis:

```text
loved     → afinidade ALTA positiva (presente raro/pessoal do NPC)
liked     → afinidade média positiva (tags do gosto do NPC)
neutral   → afinidade pequena positiva (default — qualquer Giftable não classificado)
disliked  → afinidade pequena NEGATIVA
hated     → afinidade ALTA NEGATIVA (REDUZ a amizade — específico por NPC)
```

`hated` é a novidade que a fable_26 original não previa: um presente pode **piorar** a relação.

---

## 1. Reuso de código existente (re-auditoria 2026-06-13)

A re-auditoria confirmou que **já existe** uma struct para isto, hoje **código morto**
(nunca lida/escrita/gerada/validada):

```text
Assets/_Game/Scripts/NPC/NpcDefinition.cs
  public class NpcGiftPreferences {
      List<string> LikedItemTags;     // ← liked  (por tag)
      List<string> LovedItemIds;      // ← loved  (por id de item)
      List<string> DislikedItemTags;  // ← disliked (por tag)
      int          DailyGiftLimit = 1;
  }
  // NpcDefinition.GiftPreferences = new NpcGiftPreferences();  (campo já presente)
```

Também já existe a tag de item `ItemTag.Giftable` (`Assets/_Game/Scripts/Items/ItemTag.cs`,
`1L << 6`), **não atribuída a nenhum item ainda**.

**Diretriz de reuso (não criar paralelo):**

```text
REUTILIZAR a struct NpcGiftPreferences (não criar uma segunda).
ESTENDER a struct com as DUAS categorias que faltam:
    List<string> NeutralItemTags  (opcional — default já é "neutral" se não classificado)
    List<string> HatedItemTags
REUTILIZAR a tag ItemTag.Giftable como porteiro do "que pode ser presente".
```

As demais categorias do `SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION` §8.1
(`ForbiddenGiftTags`, `RomanticGiftTags`, `MarriageGiftTags`, `PolyCommitmentGiftTags`)
ficam **fora desta matriz v1.0** (são camada de romance/casamento, fora do escopo da
fable_26). Esta matriz cobre apenas as 5 reações de **amizade**.

---

## 2. Valores de afinidade por nível (proposta canônica)

A fable_26 original dava `+3` fixo por presente. Esta matriz substitui por:

| Nível | Pontos de amizade | Direção | Observação |
|---|---|---|---|
| `loved` | **+12** | positiva alta | item raro/pessoal; poucos por NPC (1-3) |
| `liked` | **+6** | positiva média | por tag (categorias do gosto do NPC) |
| `neutral` | **+2** | positiva mínima | default: qualquer `Giftable` não classificado |
| `disliked` | **-2** | **negativa pequena** | tag que o NPC não aprecia |
| `hated` | **-6** | **negativa alta** | tag/item que o NPC repudia (REDUZ amizade) |

### 2.1 Regras de cálculo

```text
Classificação por PRECEDÊNCIA (mais forte vence, do mais negativo ao mais positivo):
    hated  > disliked > loved (por id) > liked (por tag) > neutral (default)
Justificativa: alinha com SOCIAL §8.2 — "item odiado de alta qualidade continua odiado";
    a rejeição pessoal vence a tag genérica positiva.
Item sem a tag Giftable NÃO é presenteável (recusa silenciosa; sem ganho/perda).
Qualidade (Normal/Prata/Ouro) é MULTIPLICADOR FUTURO sobre presentes positivos
    (loved/liked/neutral); NÃO vira positivo o que é disliked/hated. (Balance futuro;
    fable_26 v3 trata como +0% por enquanto.)
Pontos de amizade nunca descem abaixo de 0 (clamp no piso do nível 0/Desconhecido).
```

### 2.2 Limite diário (`DailyGiftLimit`)

```text
Default por NPC: 1 presente relevante por dia (campo DailyGiftLimit já existente = 1).
Alinha com SOCIAL §8.3 (1/dia/NPC; 2/semana é refinamento futuro, fora desta v1.0).
A fable_26 já tem o marcador lastGiftDay por NPC no save — reusar como chave do cap.
Presente além do limite diário: recusa amigável, sem ganho/perda (não consome o item).
```

---

## 3. Vocabulário de tags de gosto (`gift_*`)

As tags abaixo são **propostas de tag de presente** que o artefato A4 (ITEM_CATALOG)
deve materializar como tags `gift_*` aplicáveis aos itens. Os exemplos de item usam IDs
reais do `ITEM_CATALOG_DIRECTION_v1.0.md`. A matriz por NPC (§4) referencia estas tags.

| Tag de gosto | Significado | Exemplos de item (catálogo) |
|---|---|---|
| `gift_food_hearty` | comida farta/reconfortante | item_consumable_food_pumpkin_soup, _miners_ration, _carrot_stew |
| `gift_food_fine` | comida fina/premium | item_consumable_food_crystal_jam, _festival_cake, _starroot_pie |
| `gift_drink` | bebida/fermentado | item_consumable_food_vale_wine, _grape_juice |
| `gift_flower_crop` | flor/crop ornamental rural | item_crop_alihana_tear, item_crop_thandra_wheat, item_crop_carrot |
| `gift_herb_reagent` | erva/reagente alquímico | item_essence_toxic, item_crop_shadowroot, item_crop_senya_pepper |
| `gift_ore_metal` | minério/metal/lingote | item_material_iron_ore, _silver_ore, _mithril_ore, _copper_ore |
| `gift_gem_crystal` | cristal/gema arcana | item_material_arcane_crystal, item_essence_arcane |
| `gift_book_lore` | livro/registro/inscrição | item (lore) — A4 §scrolls/relíquias / registros de arquivo |
| `gift_craft_tool` | ferramenta/objeto artesanal | item_tool_* (martelo, ferramentas) |
| `gift_religious_kanthor` | objeto devocional de Kanthor | item_relic_kanthor + ofertas de ordem |
| `gift_religious_thandra` | oferenda de Thandra (natureza) | item_crop_thandra_wheat, sementes/ervas |
| `gift_religious_senya` | objeto/comida festiva de Senya | item_crop_senya_pepper, comida apimentada |
| `gift_religious_nyx` | objeto noturno/segredo (Nyx) | item_crop_shadowroot, itens "que não existem" |
| `gift_religious_finan` | item de sorte/comércio (Finan) | item raro de barganha, moeda/sorte |
| `gift_religious_merithus` | contrato/selo/registro (Merithus) | documento/selo registrado |
| `gift_religious_thoren` | minério/forja (Thoren) | item_material_*_ore, lingotes |
| `gift_animal_product` | produto animal/ração | item_consumable_food_goat_cheese, egg, milk, leather |
| `gift_fish` | peixe/produto de pesca | item_consumable_food_grilled_fish, mirrorfin, fish_pale |
| `gift_junk_lowvalue` | quinquilharia barata | item_material_wood, item_material_stone (em excesso) |
| `gift_volatile_explosive` | item instável/explosivo | reagentes voláteis da Ozzra (perigo para alguns) |
| `gift_undocumented_blackmarket` | item sem procedência/contrabando | unidentified_trinket_N, pedras escuras sem registro |

> **Nota A4:** este documento NÃO cria os itens nem aplica as tags — isso é da emenda do
> ITEM_CATALOG (artefatos A4/2.x). Aqui só fixamos o **vocabulário** e o **mapeamento por NPC**.

---

## 4. Matriz de gostos por NPC

Convenção de cada bloco:

```text
loved    → 1-3 itens/tags muito pessoais (+12)
liked    → tags do gosto cotidiano (+6)
neutral  → DEFAULT implícito (qualquer Giftable não classificado, +2) — não repetido
disliked → tag que desagrada (-2)
hated    → tag/item que o NPC repudia (-6)
```

Origem das escolhas: deus principal, simpatias/desconfianças, classe e relações do roster v1.1.

### 4.1 npc_corvus — Padre Corvus (Curandeiro/Guardião · Kanthor)
```text
loved:    gift_religious_kanthor; item_consumable_food_thandra_loaf
liked:    gift_food_hearty; gift_book_lore; gift_religious_merithus
disliked: gift_drink (excesso de taverna); gift_religious_senya
hated:    gift_religious_nyx; gift_undocumented_blackmarket
```

### 4.2 npc_mara — Mara Vellum (Escriba/Comerciante · Merithus)
```text
loved:    gift_religious_merithus (selo/contrato registrado); gift_book_lore
liked:    gift_food_fine; gift_religious_kanthor
disliked: gift_junk_lowvalue; gift_volatile_explosive
hated:    gift_undocumented_blackmarket; gift_religious_nyx
```

### 4.3 npc_sylveth — Sylveth (Plantador/Curandeiro · Thandra)
```text
loved:    gift_flower_crop; item_crop_alihana_tear
liked:    gift_herb_reagent; gift_religious_thandra; gift_food_fine
disliked: gift_ore_metal; gift_gem_crystal (tecnologia bromeciana sem cuidado)
hated:    gift_volatile_explosive; gift_undocumented_blackmarket
```

### 4.4 npc_brumdar — Brumdar Ferro-Quieto (Artesão/Combatente · Thoren)
```text
loved:    gift_ore_metal (minério de procedência); gift_craft_tool
liked:    gift_food_hearty; gift_drink; gift_religious_thoren
disliked: gift_flower_crop; gift_religious_senya
hated:    gift_undocumented_blackmarket (metal sem origem); gift_religious_nyx
```

### 4.5 npc_nimble — Nimble Galhobaixo (Construtor/Artesão · Merithus)
```text
loved:    gift_craft_tool; item_material_wood (madeira boa de obra)
liked:    gift_food_hearty; gift_religious_finan
disliked: gift_book_lore (burocracia); gift_religious_merithus em excesso
hated:    gift_volatile_explosive
```

### 4.6 npc_gurd — Gurd Carvalho-Torto (Construtor/Combatente · Kaand)
```text
loved:    gift_food_hearty (muita comida); gift_drink
liked:    gift_ore_metal; gift_craft_tool; gift_religious_senya
disliked: gift_book_lore; gift_flower_crop
hated:    gift_religious_kanthor (controle); gift_undocumented_blackmarket
```

### 4.7 npc_hund — Hund Carvalho-Torto (Guardião/Construtor · Kanthor)
```text
loved:    gift_craft_tool; gift_religious_kanthor
liked:    gift_food_hearty; gift_ore_metal; gift_religious_nyx (noite/vigilância)
disliked: gift_volatile_explosive; gift_food_fine (sem frescura)
hated:    gift_undocumented_blackmarket
```

### 4.8 npc_ozzra — Ozzra Fumaçazul (Alquimista/Artesão · Senya)
```text
loved:    gift_herb_reagent; gift_volatile_explosive (adora o perigoso)
liked:    gift_gem_crystal; gift_religious_senya; gift_food_fine
disliked: gift_religious_kanthor (rígido); gift_religious_merithus
hated:    gift_junk_lowvalue (desperdício de material)
```

### 4.9 npc_gruta — Gruta Panela-Funda (Comerciante/Músico · Senya)
```text
loved:    gift_food_fine; gift_drink (taverna)
liked:    gift_food_hearty; gift_religious_senya; gift_fish
disliked: gift_religious_merithus (burocracia); gift_book_lore
hated:    gift_volatile_explosive (perto dos clientes); gift_religious_nyx que ameaça clientes
```

### 4.10 npc_zrix — Zrix das Estradas (Explorador/Comerciante · Finan)
```text
loved:    gift_religious_finan; gift_book_lore (mapas/rotas)
liked:    gift_food_hearty; gift_ore_metal; gift_craft_tool
disliked: gift_flower_crop; gift_food_fine
hated:    gift_undocumented_blackmarket (material sem origem clara — desconforto profissional)
```

### 4.11 npc_yael — Yael Noite-Mansa (Comerciante/Explorador · Nyx)
```text
loved:    gift_undocumented_blackmarket; gift_religious_nyx; item_crop_shadowroot
liked:    gift_gem_crystal; gift_fish; gift_food_fine
disliked: gift_religious_kanthor; gift_junk_lowvalue
hated:    gift_religious_senya (festa intensa/exposição pública)
```

### 4.12 npc_thalindra — Thalindra Véu-de-Lua (Pesquisador/Alquimista · Alihana)
```text
loved:    gift_book_lore; item_crop_alihana_tear
liked:    gift_gem_crystal; gift_herb_reagent; gift_religious_merithus (registro)
disliked: gift_food_hearty (distante da terra/mesa); gift_drink
hated:    gift_religious_senya; gift_religious_kaand (Kaand)
```

### 4.13 npc_dagna — Dagna Rocha-Morna (Minerador/Combatente · Thoren)
```text
loved:    gift_ore_metal; gift_gem_crystal
liked:    gift_craft_tool; gift_food_hearty; gift_religious_thoren
disliked: gift_flower_crop; gift_religious_senya
hated:    gift_religious_nyx; gift_undocumented_blackmarket (cultos subterrâneos)
```

### 4.14 npc_pip — Pip Semente-Solta (Comerciante/Explorador · Finan · jovem)
```text
loved:    gift_food_fine (doce); gift_religious_finan (sorte)
liked:    gift_food_hearty; gift_fish; gift_junk_lowvalue (acha graça)
disliked: gift_book_lore (chato); gift_religious_merithus
hated:    gift_volatile_explosive (perigoso para uma criança); gift_undocumented_blackmarket
```

### 4.15 npc_alaric — Ser Alaric Veyr (Guardião/Combatente · Kanthor)
```text
loved:    gift_craft_tool (arma/escudo bem-feito); gift_religious_kanthor
liked:    gift_food_hearty; gift_ore_metal; gift_religious_thoren
disliked: gift_drink em excesso; gift_religious_finan oportunista
hated:    gift_undocumented_blackmarket; gift_religious_nyx
```

### 4.16 npc_mirela — Mirela dos Laços (Artesão/Comerciante · Merithus)
```text
loved:    gift_animal_product (lã/tecido); gift_craft_tool (costura)
liked:    gift_food_fine; gift_flower_crop; gift_religious_finan
disliked: gift_ore_metal; gift_volatile_explosive
hated:    gift_undocumented_blackmarket
```

### 4.17 npc_renko — Renko Três-Sorrisos (Comerciante/Artesão · Finan)
```text
loved:    gift_religious_finan; gift_gem_crystal (alto valor de revenda)
liked:    gift_food_fine; gift_undocumented_blackmarket (lucro discreto); gift_ore_metal
disliked: gift_religious_kanthor; gift_junk_lowvalue
hated:    gift_religious_merithus (contratos rígidos) em excesso
```

### 4.18 npc_eiran — Eiran Valeclaro (Tratador/Plantador · Thandra)
```text
loved:    gift_animal_product (ração/produto animal); gift_flower_crop
liked:    gift_herb_reagent; gift_religious_thandra; gift_food_hearty
disliked: gift_ore_metal; gift_craft_tool
hated:    gift_volatile_explosive (perigo para os animais); gift_undocumented_blackmarket
```

### 4.19 npc_liora — Liora Canta-Rio (Músico/Pesquisador · Alihana)
```text
loved:    gift_book_lore (canções/registros); item_crop_alihana_tear
liked:    gift_flower_crop; gift_food_fine; gift_religious_thandra
disliked: gift_ore_metal; gift_junk_lowvalue
hated:    gift_religious_nyx (silencia memórias); gift_volatile_explosive
```

### 4.20 npc_orlan — Orlan Pouso-Curto (Comerciante/Escriba · Finan)
```text
loved:    gift_drink (item_consumable_food_vale_wine); gift_religious_finan
liked:    gift_food_fine; gift_book_lore (notícias/registros); gift_food_hearty
disliked: gift_volatile_explosive; gift_junk_lowvalue
hated:    gift_religious_nyx; gift_undocumented_blackmarket
```

### 4.21 npc_savra — Savra Escama-Verde (Curandeiro/Explorador · Telisandra)
```text
loved:    gift_herb_reagent; gift_essence (item_essence_toxic) — domínio de venenos/antídotos
liked:    gift_flower_crop; gift_food_hearty; gift_religious_thandra
disliked: gift_ore_metal; gift_religious_kanthor (simplifica natureza)
hated:    gift_religious_kaand (predatório); gift_volatile_explosive
```

### 4.22 npc_tovin — Tovin Mãos-de-Selo (Escriba/Artesão · Merithus)
```text
loved:    gift_religious_merithus (selo/carimbo/registro); gift_craft_tool (artífice)
liked:    gift_book_lore; gift_gem_crystal; gift_religious_kanthor
disliked: gift_drink; gift_religious_senya
hated:    gift_religious_nyx (sem documento); gift_undocumented_blackmarket
```

### 4.23 npc_maelor — Maelor Cinza (Explorador/Pesquisador · Nyx · romance tardio)
```text
loved:    gift_religious_nyx; gift_book_lore (memória/segredo); item_crop_shadowroot
liked:    gift_gem_crystal; gift_undocumented_blackmarket; gift_food_fine
disliked: gift_religious_kanthor público; gift_religious_merithus que registra tudo
hated:    gift_religious_senya (exposição pública)
```

---

## 5. Tabela-resumo (loved / hated por NPC)

| NPC | Loved (resumo) | Hated (resumo) |
|---|---|---|
| npc_corvus | religious_kanthor, thandra_loaf | religious_nyx, blackmarket |
| npc_mara | religious_merithus, book_lore | blackmarket, religious_nyx |
| npc_sylveth | flower_crop, alihana_tear | volatile_explosive, blackmarket |
| npc_brumdar | ore_metal, craft_tool | blackmarket, religious_nyx |
| npc_nimble | craft_tool, wood | volatile_explosive |
| npc_gurd | food_hearty, drink | religious_kanthor, blackmarket |
| npc_hund | craft_tool, religious_kanthor | blackmarket |
| npc_ozzra | herb_reagent, volatile_explosive | junk_lowvalue |
| npc_gruta | food_fine, drink | volatile_explosive, religious_nyx(ameaça) |
| npc_zrix | religious_finan, book_lore | blackmarket |
| npc_yael | blackmarket, religious_nyx, shadowroot | religious_senya |
| npc_thalindra | book_lore, alihana_tear | religious_senya, religious_kaand |
| npc_dagna | ore_metal, gem_crystal | religious_nyx, blackmarket |
| npc_pip | food_fine, religious_finan | volatile_explosive, blackmarket |
| npc_alaric | craft_tool, religious_kanthor | blackmarket, religious_nyx |
| npc_mirela | animal_product, craft_tool | blackmarket |
| npc_renko | religious_finan, gem_crystal | religious_merithus(excesso) |
| npc_eiran | animal_product, flower_crop | volatile_explosive, blackmarket |
| npc_liora | book_lore, alihana_tear | religious_nyx, volatile_explosive |
| npc_orlan | drink(vale_wine), religious_finan | religious_nyx, blackmarket |
| npc_savra | herb_reagent, essence_toxic | religious_kaand, volatile_explosive |
| npc_tovin | religious_merithus, craft_tool | religious_nyx, blackmarket |
| npc_maelor | religious_nyx, book_lore, shadowroot | religious_senya |

> **Nota de tag de deus faltante:** `gift_religious_kaand` (Kaand) aparece como `hated`
> em Savra e Thalindra mas não tem `loved` em nenhum NPC do roster v1.1 (nenhum dos 23 cultua
> Kaand como loved giver — Gurd cultua Kaand mas seus `loved` são food/drink). A4 deve criar a
> tag mesmo assim (uso = repúdio), ou registrar como tag sem item positivo nesta v1.0.

---

## 6. Débito de implementação (wiring — NÃO feito por este doc)

A re-auditoria confirmou que a estrutura existe mas está **morta**. Para a matriz virar
runtime, a implementação (fora desta direção, dentro da fable_26 emendada + ITEM_CATALOG/A4)
precisa de:

```text
[ESTRUTURA] Estender NpcGiftPreferences com NeutralItemTags e HatedItemTags
            (reusar LikedItemTags/LovedItemIds/DislikedItemTags + DailyGiftLimit já existentes).
[GERAÇÃO]   Popular NpcGiftPreferences de cada NPC a partir desta matriz §4
            (no inicializador/gerador de NpcDefinition; sem GameObject.Find em runtime).
[TAGS]      A4/ITEM_CATALOG: criar as tags gift_* da §3 e aplicá-las aos itens;
            marcar itens presenteáveis com ItemTag.Giftable (hoje atribuída a zero itens).
[LEITURA]   fable_26 FriendshipService: ao presentear, classificar o item pelo gosto do NPC
            (precedência §2.1) e somar o delta do nível (§2) em vez do +3 fixo.
[ESCRITA]   Reusar o marcador lastGiftDay por NPC já previsto no save da fable_26 (cap diário).
[VALIDAÇÃO] Validator (estilo F30): todo NPC do roster tem GiftPreferences não vazia;
            toda tag gift_* citada na matriz existe no catálogo; todo LovedItemId existe;
            nenhum item loved/liked sem a tag Giftable; nenhum NPC sem ao menos 1 hated.
```

Status atual desta dependência de código: **DÉBITO — não implementado** (struct morta, tag
sem itens, sem leitura/escrita/geração/validação). Esta direção é o contrato de dados; o
runtime é gated pela fable_26 (emenda 2026-06-13-V3) e pela emenda do ITEM_CATALOG.

---

## 7. Fora de escopo desta v1.0

```text
romance/casamento (RomanticGiftTags/MarriageGiftTags/PolyCommitmentGiftTags — SOCIAL §8.1);
presentes proibidos (ForbiddenGiftTags) com penalidade narrativa — futuro;
aniversários e bônus de presente em festival (SOCIAL §8 — futuro);
multiplicador de qualidade aplicado (Prata/Ouro) — balance futuro (v3 trata como +0%);
descoberta gradual do gosto ("presente favorito/odiado descoberto") — UI/codex futuro;
2 presentes/semana e exceções de evento (SOCIAL §8.3) — refinamento futuro.
```
