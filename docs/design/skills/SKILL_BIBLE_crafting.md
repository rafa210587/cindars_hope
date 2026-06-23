# Bíblia de Skills — Árvore CRAFTING (Cindar's Hope)

> **Status:** PROPOSTA (design, sem código de implementação fechado). Aguarda virar spec(s) de implementação (alvo: `fable_70`).
> **Data:** 2026-06-23.
> **Fonte de verdade do roster:** `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs` → `BuildCraftingNodes()`.
> **Fonte de verdade dos números da ativa já implementada:** `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs` → registro `crafting.bomba_improvisada`.
> **Decisões de saneamento:** `docs/design/SKILL_CATALOG_DESIGN_REVIEW_v1.0.md`, seções 3 e 7.

---

## Legenda

| Campo | Significado |
|---|---|
| **mantida** | Nó já existe com efeito real; permanece como está (valor pode ser fixado aqui). |
| **já-implementada** | Ativa com executor real registrado e funcionando hoje. |
| **nova(implementar)** | Nó existe no catálogo mas sem efeito real; precisa de código nesta leva. |
| **convertida** | Era "passiva-fantasma" (hook em 0% de efeito); CONVERTIDA para efeito real e concreto agora. |
| **mesclada** | Dois nós antigos fundidos em um. |
| **cortada** | Removida da árvore. |

> **Princípio que rege toda esta árvore (decisão fechada):** **nenhum nó comprável pode ficar em 0% de efeito.** A árvore Crafting estava quase inerte — 7 ativas dormentes e 4 passivas-fantasma (hooks nomeados que nenhum sistema consumia). Aqui **eliminamos as passivas-fantasma**: cada passiva abaixo tem efeito mecânico real, mensurável e plugado num sistema que já existe (CraftTime, RepairEfficiency, economia, farm/coleta, durability).

---

## Tipos de skill point e bandas de balance (referência para a árvore inteira)

- **Custo SP por nó:** 1 SP (todos). Passivas com rank podem subir até rank 5 (1 SP/rank).
- **Custo de Stamina de ativas utilitárias:** 0–20.
- **Cooldowns:** bomba 12 s · irrigador 8–10 s · reparo 6–10 s · buff 15–30 s.
- **Passivas de %:** craft time / repair / yield / gold ficam na banda **5–15%**.

---

## Mapa da cadeia de prereqs (DEPOIS do saneamento)

A cadeia mudou por causa de um corte (`mecanismo_campo`) e dois merges (`field_patch`+`quick_repair` → `reparo_de_campo`; `marca_eficiencia` → `eficiencia`). Reapontes explicados em cada nó e resumidos abaixo.

```
T1  crafting_fast_hands (raiz)
     ├─ crafting_repair_care ───────────────┐
     │     └─ REPARO DE CAMPO (T2/T3) ◄── absorve field_patch + quick_repair
     │            ├─ irrigador_portatil (T3)
     │            ├─ bomba_improvisada (T3, já-impl)
     │            └─ EFICIÊNCIA (buff, T4) ◄── absorve marca_eficiencia
     ├─ crafting_material_eye (T1) ──► crafting_station_focus (T2) ──► crafting_salvage_method (T3) ──► crafting_durable_finish (T4)
     └─ crafting_pack_order (T1) ──► crafting_shop_sense (T4)

T5  crafting_capstone_master_artisan (capstone, reqNodes 8, prereq durable_finish)
```

**Reapontes de prereq (justificativa):**

1. `mecanismo_campo` **cortado**. Ele era prereq de `marca_eficiencia` e dependia de `irrigador_portatil`. Com o corte, `irrigador_portatil` deixa de ter dependente downstream pelo lado dos gadgets; tudo bem — ele continua pendurado em **REPARO DE CAMPO**.
2. `field_patch` + `quick_repair` → **REPARO DE CAMPO** (um nó). Antes `irrigador_portatil` e `bomba_improvisada` tinham `prereq: "crafting_quick_repair"`. Como `quick_repair` deixa de existir como nó isolado e vira REPARO DE CAMPO, ambos passam a ter **prereq: REPARO DE CAMPO** (mesma posição de cadeia, sem buraco). REPARO DE CAMPO herda o prereq de `field_patch` (que era `crafting_repair_care`), preservando o gate T1→T2.
3. `marca_eficiencia` → **EFICIÊNCIA** (buff). Antes `marca_eficiencia` dependia de `bomba_improvisada`. EFICIÊNCIA passa a ter **prereq: REPARO DE CAMPO** (raiz utilitária da sub-árvore de campo), evitando que o buff fique pendurado atrás de um gadget ofensivo — coerente, já que EFICIÊNCIA é utilitário de farm/craft, não de combate.
4. Lado passivo de economia/material (`material_eye → station_focus → salvage_method → durable_finish → capstone`) **não muda de forma**; apenas troca os efeitos-fantasma por efeitos reais.

---

# PASSIVAS (todas com efeito REAL — zero passiva-fantasma)

### Mãos Ágeis (`crafting_fast_hands`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Crafting / T1 / — / 1 SP / Passiva
- **Status no saneamento:** mantida (efeito real já existe).
- **Resumo (1 frase):** Reduz em 10% o tempo de qualquer craft.
- **Descrição completa:** Raiz da árvore. Acelera todo job de crafting/processing. É o gate de entrada de toda a árvore Crafting.
- **Mecânica (PASSIVA):** Efeito EXATO: `CraftTimeReductionPercent +0.10` (−10% no tempo de craft). · Modificador: tipo `CraftTimeReductionPercent`, valor `0.10`. · Stack por rank: não definido como rankável neste roster (rank 1; se rankear no futuro, banda 5–15% por rank com cap em −30% somado a station_focus). · Condição: aplica a todo job de craft/processing que leia o multiplicador. · Retroativo: aplica a jobs **iniciados após** a compra (jobs já em andamento mantêm o tempo calculado na partida).
- **Q&A fechado:**
  1. *Atua em qual sistema?* No pipeline de crafting/processing que já lê `CraftTimeReductionPercent` (mesmo consumidor de `station_focus` e do capstone).
  2. *Empilha com station_focus e capstone?* Sim, somam-se: fast_hands −10% + station_focus −5% + capstone −15% = −30% no caminho completo.
  3. *Afeta job já em andamento?* Não; o tempo é fixado quando o job começa.
  4. *É filler no caminho do capstone?* Não — é a raiz obrigatória e o efeito é o pilar de identidade da árvore.
  5. *Tem efeito em combate?* Nenhum; é puramente produção.

---

### Cuidado no Reparo (`crafting_repair_care`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Crafting / T1 / `crafting_fast_hands` / 1 SP / Passiva
- **Status no saneamento:** mantida.
- **Resumo (1 frase):** Aumenta a eficiência de reparo em +0.1 (cada ponto de material/kit repara mais durability).
- **Descrição completa:** Faz reparos (kit, bancada e a ativa REPARO DE CAMPO) recuperarem mais durability pelo mesmo custo. Prereq direto da ativa REPARO DE CAMPO.
- **Mecânica (PASSIVA):** Efeito EXATO: `RepairEfficiencyBonus +0.1` (+10% de durability recuperada por unidade de reparo). · Modificador: tipo `RepairEfficiencyBonus`, valor `0.1`. · Stack por rank: rank 1 neste roster; se rankear, banda 5–15%/rank. · Condição: aplica sempre que um reparo é resolvido (qualquer fonte). · Retroativo: aplica do momento da compra em diante (não "des-repara" o que já foi reparado).
- **Q&A fechado:**
  1. *Afeta a ativa REPARO DE CAMPO?* Sim — REPARO DE CAMPO usa `EquipmentDurabilityTracker` e o bônus de eficiência se aplica ao montante reparado.
  2. *Afeta reparo na bancada/kit?* Sim, qualquer caminho de reparo.
  3. *Empilha com o capstone (+0.15)?* Sim: repair_care +0.1 + capstone +0.15 = +0.25 de eficiência.
  4. *Reduz o custo de material do reparo?* Não — aumenta o **resultado** (durability recuperada), não baixa o custo de entrada.
  5. *Retroativo a gear já danutilizado?* Aplica ao próximo reparo desse gear, não automaticamente.

---

### Olho de Material (`crafting_material_eye`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Crafting / T1 / `crafting_fast_hands` / 1 SP / Passiva
- **Status no saneamento:** **convertida** (era "chance futura de bônus de recurso" = 0% de efeito).
- **Resumo (1 frase):** +5% de yield em coleta de recursos do mundo/farm.
- **Descrição completa:** O artesão reconhece material melhor: toda colheita/coleta de recurso natural (nós de recurso da caverna, plantas, drops de coleta no farm) rende um pouco mais. Efeito CONCRETO e plugado na coleta — não é mais um hook morto.
- **Mecânica (PASSIVA):** Efeito EXATO: ao resolver uma coleta de recurso, a quantidade base é multiplicada por **1.05** (arredondamento: acumula fração; cada +1 inteiro acumulado vira +1 unidade de drop, evitando que +5% sobre quantidades pequenas vire sempre 0). · Modificador: novo tipo `HarvestYieldBonusPercent` = `0.05` (ou reuso de um `ResourceYieldBonus` equivalente se já existir; conferir em `system-reuse-audit` antes de criar). · Onde atua: no **resolver de yield de coleta** (resource node / harvest do farm — mesmo ponto que `crop-farming-systems`/`scene-interactable-wiring` usam para conceder recompensa de coleta). · Stack por rank: rank 1 neste roster; rankável até cap +15% (banda). · Condição: só coleta de recurso (não loot de combate, não compra). · Retroativo: vale para toda coleta após a compra.
- **Q&A fechado:**
  1. *Atua em qual sistema REAL agora?* No resolver de recompensa de coleta de recurso (resource node / harvest), o mesmo que distribui itens ao colher — não em combate nem em loja.
  2. *Afeta drop de inimigo?* Não; só coleta de recurso/colheita.
  3. *Como evita arredondar sempre para 0 em quantidades pequenas?* Acumula a fração ao longo das coletas; a cada inteiro acumulado, concede +1 unidade.
  4. *Empilha com fertilizer/qualidade do farm?* Sim, é um multiplicador independente sobre a quantidade base.
  5. *Rankável?* Sim, banda 5–15% (cap +15%).
  6. *Ainda é "chance futura"?* Não — virou bônus determinístico real de +5%.

---

### Foco de Bancada (`crafting_station_focus`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Crafting / T2 / `crafting_material_eye` / 1 SP / Passiva
- **Status no saneamento:** **convertida** (era "bônus de craft em workstation" sem consumidor → agora usa `CraftTimeReductionPercent` real, já suportado).
- **Resumo (1 frase):** −5% de tempo de craft adicional quando o job roda numa workstation.
- **Descrição completa:** Trabalhar numa bancada dedicada acelera ainda mais o craft. Usa o mesmo modificador real de tempo de craft que `fast_hands` e o capstone — portanto efeito imediato e mensurável.
- **Mecânica (PASSIVA):** Efeito EXATO: `CraftTimeReductionPercent +0.05` (−5%). · Modificador: tipo `CraftTimeReductionPercent`, valor `0.05`. · Condição: aplica a jobs de craft (no roster atual é incondicional sobre o multiplicador de craft; se o pipeline distinguir "workstation vs. campo", restringir a workstation — caso contrário aplica a todo craft, que é o comportamento suportado hoje). · Stack por rank: rank 1; somável com os outros redutores. · Retroativo: jobs iniciados após a compra.
- **Q&A fechado:**
  1. *Efeito real agora?* Sim — `CraftTimeReductionPercent` é um modificador já consumido pelo pipeline de craft (zero código novo para o efeito funcionar).
  2. *Soma com fast_hands e capstone?* Sim: −10% + −5% + −15% = −30%.
  3. *Só em workstation?* O modificador suportado hoje é global de craft; se/quando o pipeline diferenciar workstation, gatear ali. Documentado como "−5% de craft (preferencialmente workstation)".
  4. *Afeta reparo?* Não — só tempo de craft.
  5. *É filler no caminho do capstone?* Não; é um redutor real que o jogador quer.

---

### Mochila Ordenada (`crafting_pack_order`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Crafting / T1 / `crafting_fast_hands` / 1 SP / Passiva
- **Status no saneamento:** **convertida** (era "hook para futura organização de inventário" = 0%).
- **Resumo (1 frase):** Aumenta o limite de empilhamento de materiais de craft em +50% (stacks maiores → menos slots ocupados).
- **Descrição completa:** Decisão e justificativa: **CONVERTER, não cortar.** O efeito mais barato, real e implementável sobre o `InventoryManager` existente é **aumentar o stack máximo de itens de categoria material/recurso**. Isso é um benefício de produção concreto (carrega mais minério/madeira/fibra por slot), encaixa na fantasia de "mochila ordenada" e plugga em `inventory-transactions` sem novo subsistema.
  - *Por que não auto-sort?* Auto-sort exige UI/input e regras de ordenação — maior superfície e mais frágil. Stack-cap é um único número por categoria.
- **Mecânica (PASSIVA):** Efeito EXATO: para itens de categoria **material/recurso de craft**, `MaxStack` efetivo × **1.5** (ex.: 99 → 148). · Modificador: novo tipo `MaterialStackCapBonusPercent` = `0.5` lido pelo `InventoryManager` ao calcular capacidade de stack daquela categoria. · Onde atua: cálculo de capacidade de stack em `inventory-transactions` (add/merge respeita o cap aumentado). · Stack por rank: rank 1; se rankear, +25%/rank com cap razoável (ex.: ×2). · Condição: só itens de material/recurso (não consumíveis, não equip). · Retroativo: aplica imediatamente — stacks existentes podem crescer até o novo cap em merges subsequentes.
- **Q&A fechado:**
  1. *Cortada ou convertida?* **Convertida.** Tem efeito real implementável (stack-cap de materiais), então não há motivo para cortar.
  2. *Atua em qual sistema?* No `InventoryManager` (cálculo de `MaxStack` por categoria), mesmo caminho usado por add/merge.
  3. *Afeta todos os itens?* Não; só material/recurso de craft.
  4. *Stacks já existentes mudam na hora?* O cap aumenta na hora; o stack real cresce conforme você junta/coleta mais (merge respeita o novo cap).
  5. *Dá slots novos no inventário?* Não — dá **densidade** (mais por slot), não mais slots.
  6. *Prereq de quê?* Continua sendo prereq de `shop_sense` (T4).

---

### Método de Salvage (`crafting_salvage_method`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Crafting / T3 / `crafting_station_focus` / 1 SP / Passiva
- **Status no saneamento:** **convertida** (era "melhor retorno ao salvage" sem efeito real).
- **Resumo (1 frase):** +5% de material recuperado ao desmontar/salvage um item.
- **Descrição completa:** Ao desmontar equipamento/ferramenta para reaproveitar material, retorna mais. Efeito concreto sobre o resultado do salvage.
- **Mecânica (PASSIVA):** Efeito EXATO: quantidade de material retornada no salvage × **1.05** (mesma regra de acúmulo de fração de `material_eye` para não zerar em quantidades pequenas). · Modificador: novo tipo `SalvageReturnBonusPercent` = `0.05`, lido pelo resolver de salvage/desmonte. · Onde atua: no caminho de salvage/desmonte (se ainda não existir um, o efeito fica documentado como dependente do sistema de salvage — mas, **alternativa de fallback já implementável**: aplicar como **eficiência de ferramenta** real, reduzindo o consumo de durability de ferramenta por uso de coleta em 5%, plugado em `equipment-durability-repair`). Decisão de implementação: **se o salvage existir, plugar no salvage; senão, aplicar a redução de desgaste de ferramenta (−5%)** — ambos efeito real, sem 0%. · Stack por rank: rank 1; rankável até 15%. · Retroativo: do momento da compra em diante.
- **Q&A fechado:**
  1. *Efeito real agora?* Sim — ou +5% de retorno no salvage, ou (fallback) −5% de desgaste de ferramenta; nunca 0%.
  2. *Qual dos dois é o canônico?* Salvage tem prioridade se o sistema de salvage existir no estado atual; o desgaste de ferramenta é o fallback garantido.
  3. *Atua em combate?* Não.
  4. *Empilha com material_eye?* Sim — material_eye é coleta no mundo; salvage_method é desmonte de item. Caminhos distintos.
  5. *Rankável?* Sim, banda 5–15%.

---

### Acabamento Durável (`crafting_durable_finish`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Crafting / T4 / `crafting_salvage_method` / 1 SP / Passiva
- **Status no saneamento:** mantida (valor fixado aqui).
- **Resumo (1 frase):** Equipamento craftado por você nasce com +10% de durability máxima.
- **Descrição completa:** Itens produzidos pelo jogador recebem um bônus permanente de `DurabilityMax`, durando mais entre reparos. Valor definido: **+10%**.
- **Mecânica (PASSIVA):** Efeito EXATO: ao craftar um item de gear, `DurabilityMax` desse item × **1.10**. · Modificador: novo tipo `CraftedDurabilityMaxBonusPercent` = `0.10` aplicado no momento do craft, gravado no registro de durability do item (`EquipmentDurabilityTracker`). · Stack por rank: rank 1; se rankear, +5%/rank com cap +25%. · Condição: só itens **craftados pelo jogador após** a compra do nó (não retroage a gear já existente — o bônus é "carimbado" no craft). · Retroativo: não (é propriedade do item no momento do craft).
- **Q&A fechado:**
  1. *Vale para gear comprado/lootado?* Não — só para o que **você crafta** após ter o nó.
  2. *Retroage a itens já craftados antes?* Não; o bônus é gravado no momento do craft.
  3. *Aumenta durability atual ou só o máximo?* O máximo (e o item nasce no novo máximo).
  4. *Empilha com reparo?* São independentes: reparo recupera; este aumenta o teto.
  5. *Onde grava?* No `EquipmentDurabilityTracker` (id do item → durabilityMax).

---

### Senso de Mercado (`crafting_shop_sense`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Crafting / T4 / `crafting_pack_order` / 1 SP / Passiva
- **Status no saneamento:** **convertida** (era "hook para bônus de venda/compra futuro" = 0%).
- **Resumo (1 frase):** +5% de gold ao **vender** itens.
- **Descrição completa:** O artesão sabe negociar: toda venda na loja rende mais gold. Plugado na economia real (mesmo ponto de preço de venda usado por `economy-balance-tuning`).
- **Mecânica (PASSIVA):** Efeito EXATO: preço de venda recebido × **1.05** (arredondamento padrão de gold, mínimo +1 quando o preço base ≥ 20). · Modificador: novo tipo `SellGoldBonusPercent` = `0.05`, lido pelo cálculo de venda da loja. · Onde atua: no resolver de transação de venda (shop sell), antes de creditar gold. · Stack por rank: rank 1; rankável até +15%. · Condição: só **venda** (não afeta preço de compra). · Retroativo: vale para toda venda após a compra.
- **Q&A fechado:**
  1. *Afeta compra também?* Não — só o gold recebido em **venda** (escopo definido para evitar dupla economia).
  2. *Atua em qual sistema REAL?* No cálculo de preço de venda da loja (economia existente), não num hook morto.
  3. *Empilha com eventos/sazonalidade de preço?* Sim, é um multiplicador independente sobre o preço final de venda.
  4. *Rankável?* Sim, banda 5–15%.
  5. *Tem mínimo?* Para itens de preço ≥ 20 garante ≥ +1 gold para o efeito não sumir no arredondamento.

---

# ATIVAS

### Bomba Improvisada (`crafting.bomba_improvisada`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Crafting / T3 / **REPARO DE CAMPO** (era `crafting_quick_repair`) / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** **já-implementada** (executor real registrado).
- **Resumo (1 frase):** Arremessa uma bomba tóxica que atinge até 3 inimigos na linha de voo, boa para stagger e swarms.
- **Descrição completa:** Gadget ofensivo craftado. Hoje é a única ativa de Crafting com executor real. Projétil tóxico de curto alcance, perfurante.
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla do slot equipado (1–4).
  - **Alvo:** direcional (projétil arremessado na direção que o player encara); atinge inimigos no caminho. **Não é AoE radial** — o pierce de 3 é o "área" dela (linha perfurante).
  - **Alcance/raio:** range **5 m**; velocidade do projétil **8**; perfura até **3 alvos** (`maxHitsPerProjectile: 3`).
  - **Efeito:** **18 de dano Toxic** por alvo atingido.
  - **Valores (reais, do `ActiveSkillExecutionController`):** `baseDamage: 18`, `speed: 8f`, `range: 5f`, `damageType: Toxic`, `maxHitsPerProjectile: 3`, `resourceCost: 20`, `cooldownSeconds: 12f`.
  - **Status aplicado:** o registro atual **não** declara `statusEffectId` (diferente da Nuvem Tóxica de Magic, que aplica `status_poison`). Hoje é dano Toxic puro. *Proposta de polish:* poderia aplicar `status_poison`, mas isso é mudança de valor a confirmar — **documentado como hoje é**: sem status, só dano.
  - **Custo:** **20 de Stamina** (`resourceCost: 20`). *Decisão de design:* não consome material/carga craftada no estado atual (custo é só stamina). Se for desejado custo de material, é mudança futura — hoje **não** consome item.
  - **Cooldown:** **12 s**.
  - **Movimento:** não trava o movimento (arremesso rápido).
  - **Telegraph/Animação/VFX (pixel 2D):** animação curta de arremesso; sprite de bomba girando; ao colidir/expirar, pequeno splash tóxico (partículas verdes). Sem telegraph para o inimigo (é o player que ataca).
- **Q&A fechado:**
  1. *Precisa de alvo válido? E se não houver?* Não precisa de alvo travado — é direcional. Se não acertar ninguém, gasta cooldown e stamina e simplesmente não causa dano (arremesso "no vazio").
  2. *Trava movimento?* Não.
  3. *Consome material/carga — e se faltar?* No estado atual **não consome material**, só 20 de Stamina. Se faltar stamina, a ação é recusada com `FailureReason` "recurso insuficiente" (assim que o enforcement de custo do `ActiveSkillExecutionController` estiver ligado) + feedback de HUD; não dispara nem entra em cooldown.
  4. *Dano em área? Friendly fire? Boss ok?* **Não é AoE radial** — é linha perfurante (até 3 alvos). Sem friendly fire (só atinge inimigos). Funciona em boss normalmente (dano Toxic; boss não é imune por padrão).
  5. *Por que o dano é "baixo" (18)?* É um gadget de **stagger/swarm**, não burst — o valor reflete utilidade de controle, não dano de pico.
  6. *Aplica veneno?* No estado atual **não** (só dano Toxic). Aplicar `status_poison` é proposta de polish, não implementado.

---

### Irrigador Portátil (`crafting.irrigador_portatil`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Crafting / T3 / **REPARO DE CAMPO** (era `crafting_quick_repair`) / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** **nova(implementar)** — alvo `fable_70`. Reusa o padrão `FarmCropSkillEffectExecutor`.
- **Resumo (1 frase):** Rega de uma vez todos os crop plots num raio ao redor do player, economizando voltas com o regador.
- **Descrição completa:** Utilidade de farm-sim que sinergiza com o pilar de fazenda. Em vez de regar planta a planta, dispara um pulso de irrigação que molha um grupo de plots próximos.
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla do slot equipado (1–4).
  - **Alvo:** **plots de farm** (tiles de solo plantável/plantado) — não inimigos. Modo área centrado no player (regra padrão); opcionalmente plot apontado pelo cursor como centro.
  - **Alcance/raio:** raio **2 tiles** ao redor do centro → cobre um bloco **5×5** = **até ~25 plots** (rega só os que existem e precisam de água; vazios/já-regados são ignorados). *Banda de design:* iniciar conservador em raio 2 (≈ até 25 plots); ajustável.
  - **Efeito (rega):** marca os plots no raio como **regados neste dia** (mesmo efeito do regador manual / chuva), via o caminho de água do `crop-farming-systems`.
  - **Valores:** raio 2 tiles · até ~25 plots afetados · sem dano.
  - **Status aplicado:** estado "regado" no solo (não é status de combate).
  - **Custo:** **stamina 10** (banda utilitária 0–20). *Sem custo de água como recurso separado neste roster* — o custo é a stamina; se um sistema de "carga de água do irrigador" existir depois, plugar ali. Hoje: **só stamina, sem consumir item de água**.
  - **Cooldown:** **8 s** (banda 8–10).
  - **Movimento:** não trava o movimento.
  - **Telegraph/Animação/VFX (pixel 2D):** o player ergue o irrigador; pulso/spray de gotas se espalhando em anel pelos tiles do raio; tiles regados ficam com o overlay de "solo molhado" (escurecido).
- **Q&A fechado:**
  1. *Precisa de plot válido? E se não houver nenhum no raio?* Não trava em alvo; se não houver plot regável no raio, a ação **não é gasta** (ou: dispara mas sem efeito) — proposta: recusar com feedback "nenhum plot para regar" **sem** consumir cooldown/stamina, para não punir clique acidental. Decisão de implementação registrada em `fable_70`.
  2. *Quantos plots, qual raio, custa água/stamina?* Raio **2 tiles** (5×5, até ~25 plots); custo **10 de stamina**; **não** consome água como item separado neste roster.
  3. *Trava movimento?* Não.
  4. *Rega plots já regados de novo (desperdício)?* Ignora plots já regados/vazios; só age nos que precisam.
  5. *Funciona dentro da caverna?* Só onde há plots de farm (FarmScene/estufa); na caverna não há plots → cai no caso (1).
  6. *Conta como chuva para efeitos de crescimento?* Sim — usa o mesmo estado "regado neste dia" que o crescimento de crop consome.
  7. *Friendly fire / atinge inimigo?* Não interage com combate; só solo/crop.

---

# BUFF MESCLADO

### Eficiência (`crafting.eficiencia`) — absorve `crafting.marca_eficiencia`
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Crafting / T4 / **REPARO DE CAMPO** / 1 SP / Ativa (slot 1–4, buff)
- **Status no saneamento:** **nova(implementar mesclada)** — alvo `fable_70` (executor de Buff). **Documentado: este nó absorve `crafting.marca_eficiencia`** (o id antigo deixa de ser um nó separado; o conceito vira este buff único e mais forte).
- **Resumo (1 frase):** Por alguns segundos, ações de farm/craft custam menos Stamina — janela de produção barata.
- **Descrição completa:** Buff utilitário de produção. Ativa um período curto em que regar, colher, lavrar e craftar próximos consomem menos stamina. Não acumula: reativar **renova** a duração.
- **Mecânica (ATIVA / buff):**
  - **Input/slot:** tecla do slot equipado (1–4).
  - **Alvo:** **self** (o player ganha o buff).
  - **Alcance/raio:** n/a (afeta as ações do próprio player).
  - **Efeito (buff):** aplica o status **`status_efficiency`** (novo) ao player.
  - **Valores:** **duração 20 s** (banda utilitária 15–30; escolhido 20 s por ser farm-utilitário, pode ser mais longo que um buff de combate); **redução de custo de stamina −25%** em ações agrícolas/craft enquanto ativo.
  - **Status aplicado:** `status_efficiency` (proposto) — flag temporária lida pelo cálculo de custo de stamina dessas ações; sprite/ícone de buff ativo.
  - **Custo:** **stamina 0–5** para ativar (utilitário; proposto **5**). *Sem custo de material.*
  - **Cooldown:** **20 s** (banda 15–30). Como não acumula, reativar antes de expirar apenas **renova** a duração (refresh), não empilha.
  - **Movimento:** não trava o movimento.
  - **Telegraph/Animação/VFX (pixel 2D):** breve flash/aura em volta do player ao ativar; ícone de buff "Eficiência" no HUD com timer; leve brilho nas ferramentas durante a janela.
- **Q&A fechado:**
  1. *Absorve qual nó antigo?* `crafting.marca_eficiencia` — este é o nó canônico; o antigo deixa de existir como nó separado.
  2. *Acumula se reativar?* Não — **refresh** (renova a duração); a redução não dobra.
  3. *Quanto reduz e por quanto tempo?* −25% de custo de stamina por **20 s**.
  4. *Quais ações ficam mais baratas?* Ações agrícolas (regar, colher, lavrar/plantar) e craft próximas; **não** reduz custo de ataques/abilities de combate (escopo definido para não virar buff universal).
  5. *Precisa de alvo? Trava movimento?* Não e não — é self, instantâneo.
  6. *E se faltar a pequena stamina de ativação?* Recusa com `FailureReason` "recurso insuficiente" + feedback; não entra em cooldown.
  7. *Funciona na caverna?* Sim para o que for craft; o ganho agrícola só importa onde há farm.

---

# REPARO MESCLADO

### Reparo de Campo (`crafting.reparo_de_campo`) — absorve `crafting_field_patch` + `crafting_quick_repair`
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Crafting / T2–T3 / `crafting_repair_care` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** **nova(implementar mesclada)** — alvo `fable_70`. **Documentado: absorve os dois nós antigos** `crafting_field_patch` e `crafting_quick_repair` (eram quase idênticos — "remendo de campo" + "reparo rápido com custo de material"). Vira um único nó. Reusa `EquipmentManager` / `EquipmentDurabilityTracker`.
- **Resumo (1 frase):** Repara em campo o item de equipamento ativo, gastando material e stamina, sem precisar voltar à bancada.
- **Descrição completa:** Ação de manutenção de emergência. Restaura durability do gear **equipado/ativo** durante a exploração, ao custo de um pouco de material e stamina. Beneficiado pelas passivas de reparo (`repair_care`, capstone).
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla do slot equipado (1–4).
  - **Alvo:** o **item de equipamento ATIVO** do player. Definição: o **arma/ferramenta atualmente equipada** (slot ativo); se houver ambiguidade entre vários equipados, repara o **com menor durability % atual** entre os equipados (regra determinística, evita escolha manual em campo). Registrado para `fable_70`.
  - **Alcance/raio:** self (n/a).
  - **Efeito (reparo):** restaura **+30% da `DurabilityMax`** do item (valor base, antes de `RepairEfficiencyBonus`). Com `repair_care` (+0.1) e capstone (+0.15), o montante efetivo escala (ex.: +30% × (1 + 0.25) = +37.5%). *Banda de design:* 30% base mantém a ação útil sem trivializar bancada.
  - **Valores:** reparo base **30% da durabilidade máxima** · custo material **1 unidade** de material de reparo (sucata/fibra — id a definir em `fable_70`) · custo stamina **10**.
  - **Status aplicado:** nenhum (efeito é a mudança de durability no `EquipmentDurabilityTracker`).
  - **Custo:** **1 material de reparo + 10 stamina**.
  - **Cooldown:** **8 s** (banda 6–10).
  - **Movimento:** não trava o movimento (ou trava por uma fração de segundo de animação — proposta: não travar, ação rápida).
  - **Telegraph/Animação/VFX (pixel 2D):** o player ajoelha/martela rápido a ferramenta; faíscas/centelhas curtas; a barra de durability do item sobe com um "tick" e brilho.
- **Q&A fechado:**
  1. *Absorve quais nós antigos?* `crafting_field_patch` e `crafting_quick_repair` — viram este único nó.
  2. *Repara qual item (equipado ativo?), quanto, custa o quê?* O **equipamento ativo** (equipado; em empate, o de menor durability %). Repara **+30% da durability máxima** (mais com as passivas de reparo). Custa **1 material de reparo + 10 stamina**.
  3. *Precisa de item válido? E se nenhum gear estiver danificado/equipado?* Se não houver gear equipado danificado, recusa com `FailureReason` ("nada para reparar") + feedback, **sem** consumir material/stamina/cooldown.
  4. *Consome material — e se faltar?* Sim, 1 unidade. Se faltar material **ou** stamina, recusa com `FailureReason` "recurso insuficiente"; não repara nem entra em cooldown.
  5. *Trava movimento?* Proposta: não (ação rápida).
  6. *Pode reparar até 100% repetindo?* Sim, mas cada uso custa material + stamina + 8 s de CD; em campo isso limita o spam. Não há cap além do custo.
  7. *As passivas de reparo afetam?* Sim — `repair_care` e o capstone aumentam o montante efetivo recuperado via `RepairEfficiencyBonus`.

---

# CAPSTONE

### Mestre Artesão (`crafting_capstone_master_artisan`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Crafting / T5 (capstone, `reqNodes: 8`) / `crafting_durable_finish` / 1 SP / Passiva (capstone)
- **Status no saneamento:** mantida + payoff de identidade extra definido aqui.
- **Resumo (1 frase):** Mestre do ofício: craft muito mais rápido, reparos muito mais eficientes e chance de craft "limpo" que não gasta um material.
- **Descrição completa:** Recompensa de fim de árvore. Soma redutores de tempo e bônus de reparo no topo da banda, e ganha um payoff de **identidade** (não só +stat anticlímax): chance de economizar material ao craftar.
- **Mecânica (PASSIVA):**
  - **Efeito EXATO (mantido):** `CraftTimeReductionPercent +0.15` (−15% craft) **e** `RepairEfficiencyBonus +0.15` (+15% eficiência de reparo).
  - **Payoff de identidade extra (proposto):** **15% de chance, ao concluir um craft, de não consumir 1 unidade de um dos materiais** (escolhe o material mais "caro"/raro do recipe; nunca reduz abaixo de 0). Modificador: novo tipo/flag `CraftMaterialRefundChance` = `0.15`, lido pelo resolver de consumo de material do craft. Determinismo: usar RNG seeded por sistema (`rng-and-determinism`), nunca `UnityEngine.Random`.
  - **Modificador:** três efeitos — `CraftTimeReductionPercent 0.15`, `RepairEfficiencyBonus 0.15`, `CraftMaterialRefundChance 0.15`.
  - **Stack por rank:** capstone é rank 1 (não rankável).
  - **Condição:** requer 8 nós da árvore comprados (`reqNodes: 8`) e `durable_finish`. O refund só dispara em craft concluído.
  - **Retroativo:** os redutores aplicam a jobs futuros; o refund vale para crafts feitos após a compra.
- **Q&A fechado:**
  1. *O que soma com as outras passivas?* CraftTime: fast_hands −10% + station_focus −5% + capstone −15% = −30%. Repair: repair_care +0.1 + capstone +0.15 = +0.25.
  2. *Qual é o payoff de identidade (não-stat)?* 15% de chance de craft sem consumir 1 material (o mais valioso do recipe).
  3. *O refund pode zerar o custo do recipe inteiro?* Não — economiza no máximo **1 unidade de 1 material** por craft.
  4. *É determinístico/abusável por save-scum?* Usa RNG seeded por sistema; não rerollável por reload no mesmo job.
  5. *Requer o caminho inteiro?* Sim: `reqNodes: 8` + prereq `durable_finish` (T4).
  6. *É rankável?* Não — capstone é único.

---

# CORTADAS

### Mecanismo de Campo (`crafting.mecanismo_campo`) — REMOVIDA
- **Status:** **cortada** (sai da árvore).
- **Motivo:** o conceito ("dispositivo temporário que puxa item próximo ou ativa mecanismo leve") era vago demais — sem fantasia clara, sem alvo de sistema definido e sem animação descritível. Não passa no critério de "toda ativa precisa de fantasia distinta + animação/feedback".
- **Impacto na cadeia:** era `prereq: "crafting.irrigador_portatil"` e prereq de `marca_eficiencia`. Com o corte, `irrigador_portatil` perde esse dependente e `marca_eficiencia` (agora EFICIÊNCIA) foi reapontada para REPARO DE CAMPO (ver mapa de prereqs no topo).
- **Futuro:** se um dia virar um "Gancho" claro (puxa item/recurso distante para o player, com alcance, alvo e animação definidos), entra como **nova spec**, não como ressurreição deste id vago.

---

## Resumo do roster Crafting saneado

| # | Skill | Id | Tipo | Status saneamento |
|---|---|---|---|---|
| 1 | Mãos Ágeis | `crafting_fast_hands` | Passiva | mantida |
| 2 | Cuidado no Reparo | `crafting_repair_care` | Passiva | mantida |
| 3 | Olho de Material | `crafting_material_eye` | Passiva | convertida (+5% yield coleta) |
| 4 | Foco de Bancada | `crafting_station_focus` | Passiva | convertida (−5% craft real) |
| 5 | Mochila Ordenada | `crafting_pack_order` | Passiva | convertida (+50% stack material) |
| 6 | Método de Salvage | `crafting_salvage_method` | Passiva | convertida (+5% salvage / −5% desgaste) |
| 7 | Acabamento Durável | `crafting_durable_finish` | Passiva | mantida (+10% DurabilityMax craftado) |
| 8 | Senso de Mercado | `crafting_shop_sense` | Passiva | convertida (+5% gold venda) |
| 9 | Mestre Artesão | `crafting_capstone_master_artisan` | Passiva (capstone) | mantida + payoff (15% refund material) |
| 10 | Bomba Improvisada | `crafting.bomba_improvisada` | Ativa | já-implementada |
| 11 | Irrigador Portátil | `crafting.irrigador_portatil` | Ativa | nova(implementar) |
| 12 | Eficiência | `crafting.eficiencia` | Ativa (buff) | nova/mesclada (absorve marca_eficiencia) |
| 13 | Reparo de Campo | `crafting.reparo_de_campo` | Ativa | nova/mesclada (absorve field_patch + quick_repair) |
| — | Mecanismo de Campo | `crafting.mecanismo_campo` | — | **cortada** |

**13 skills no roster final** (9 passivas incl. capstone + 4 ativas, sendo 2 mescladas), **1 cortada**. Todas as passivas têm efeito real — as passivas-fantasma foram eliminadas.
