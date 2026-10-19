# Skill Catalog — Design Review & Saneamento (v1.0)

> Status: PROPOSTA (design, sem código). Aguarda confirmação humana dos vereditos de corte/merge
> antes de virar spec de implementação. Não promove nada nem move specs.
> Data: 2026-06-23. Autor: análise assistida (Claude). Fonte de verdade do catálogo:
> `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs` (69 nós, 5 árvores).

---

## 0. Objetivo

Responder três perguntas, nesta ordem:

1. **As 5 categorias fazem sentido?** → Sim, com saneamento de Survival e Crafting (decisão humana já tomada: manter 5 trees).
2. **As skills estão equilibradas, usáveis e atraentes para gastar skill points?** → Não totalmente; este doc lista os problemas e as correções.
3. **Quais das ~14 ativas dormentes merecem existir** (terão animação/mecânica interessante) **e devem ser implementadas**, e quais devem ser cortadas/mescladas/convertidas?

Decisões humanas já fixadas:
- Manter as 5 árvores (Melee / Ranged / Magic / Survival / Crafting); sanear Survival e Crafting.
- Antes de implementar as dormentes, decidir uma a uma se fazem sentido. As que fizerem → spec completa e implementação.
- Entregar catálogo primeiro, HUD depois.

---

## 1. Estado factual (baseline)

| Árvore | Nós | Ativas | Passivas | Ativas que funcionam | Ativas dormentes |
|---|---|---|---|---|---|
| Melee | 14 | 7 | 7 | 7 ✅ | 1 (block) |
| Magic | 13 | 8 | 5 | 6 ✅ | 2 (ward, sigils) |
| Ranged | 11 | 4 | 7 | 3 ✅ | 1–2 (marked, charged*) |
| Survival | 16 | 6 | 10 | 2 ✅ | 4 |
| Crafting | 15 | 8 | 7 | 1 ✅ | 7 |
| **Total** | **69** | **33** | **36** | **~19** | **~14** |

\* `ranged_charged_shot` está marcada `NotYetExecutable=true` no catálogo, **mas** tem executor real registrado (`combat.ranged.charged_shot`, dano 20). A flag dormente está inconsistente — falta só a mecânica de *segurar para carregar*; disparado hoje, ele atira como tiro normal forte.

**Economia de pontos:** 1 skill point a cada 2 níveis (50 no cap 100) + quests/bestiário/contratos (~20–30) ≈ **70–80 pontos/run**. Com ranks (até 5 por nó passivo, 1 pt cada), o investimento total possível é de várias centenas → **há escassez real e identidade de build se sustenta**. O problema não é falta de pontos; é onde eles são gastos.

**Inconsistências de wiring encontradas (a corrigir):**
- `skill_survival_emergency_roll` → mapeado para `farm.crop.water_skill` (efeito de **regar plantação**, placeholder de debug).
- `skill_crafting_field_patch` → mapeado para `farm.crop.water_skill` (mesmo placeholder).
- `skill_survival_last_breath` e `skill_crafting_quick_repair` → **sem mapping nenhum** no dicionário → "sem efeito implementado".
- `FinalHudGuardValidator` **proíbe Dash/Dodge/Block** em active slots — em tensão direta com `melee_battle_dash` (lunge), `melee_guarded_block` e `survival_emergency_roll` ocuparem slot.

---

## 2. As categorias fazem sentido?

Sim, mas duas das cinco estão estruturalmente fracas. Decisão (humana): **manter 5 trees, sanear Survival/Crafting.**

| Tree | Identidade | Saúde hoje | Ação |
|---|---|---|---|
| Melee | Arquétipo de combate por arma | ✅ Saudável (7 ativas reais, capstone XOR) | Nenhuma (só polir parecidos) |
| Magic | Arquétipo elemental | ✅ Saudável (6 spells reais, capstone XOR) | Implementar ward+sigils |
| Ranged | Arquétipo de arco | 🟡 Magra (3 ativas reais) | Dar densidade: charged real + marked |
| **Survival** | Defesa/caverna/utilidade | 🔴 Passiva demais (10 passivas, só 2 ativas reais) | Saneamento abaixo |
| **Crafting** | Produção/economia | 🔴 Quase inerte (7 dormentes + 4 hooks mortos) | Saneamento abaixo |

### Princípios de saneamento (aplicados a Survival e Crafting)

1. **Nenhum nó "fantasma".** Todo nó comprável deve ter efeito real **agora** — ou ser cortado, ou ter o efeito implementado nesta leva. Sem "efeito pendente" comprável.
2. **Toda ativa precisa de fantasia distinta + animação/feedback.** Se não dá pra descrever a animação e o motivo de equipar num slot (de 4), vira passiva ou é cortada.
3. **Passivas pequenas demais (+1 stat, +0.05) podem subir de rank** — então valem como sink de longo prazo, mas não devem ser obrigatórias no caminho para o capstone se forem puro filler.

---

## 3. Veredito por skill ativa dormente (o coração desta revisão)

Legenda de decisão:
- **IMPLEMENTAR** — mecânica distinta e divertida; vale uma spec e animação.
- **CONVERTER** — o conceito é bom mas não como ativa de slot (vira passiva ou ability não-slot).
- **MESCLAR** — redundante; fundir com outra.
- **CORTAR** — não agrega; remove da árvore.

Reuso disponível (building blocks que já existem): `ProjectileSkillEffectExecutor` (projétil/leque/pierce/status), `SelfRestoreSkillEffectExecutor` (HP/Stam/Mana), `MeleeStrikeSkillEffectExecutor` (arco/lunge/knockback/posture), `FarmCropSkillEffectExecutor` (farm), sistema de status (`status_bleed/chill/poison`). Faltam dois executores novos: **buff/debuff temporário** e **spawn de entidade (decoy/zona)**.

### MELEE

| Skill | Conceito | Diversão/distinção | Animação/mecânica | Decisão |
|---|---|---|---|---|
| `melee_guarded_block` | Bloqueio via slot | ❌ Block já existe como ability (Shift). Slot duplicado, e o HUD guard proíbe Block em slot. | — | **CONVERTER** → passiva ("Postura: +X% redução ao bloquear com Shift") OU **CORTAR** o nó de slot e deixar Block como ability pura. |

### RANGED

| Skill | Conceito | Diversão/distinção | Animação/mecânica | Decisão |
|---|---|---|---|---|
| `ranged_charged_shot` | Segurar p/ carregar; soltar dispara mais forte/longe | ✅ Clássico de arqueiro, ótimo feel | Hold-to-charge: barra/indicador de carga, projétil maior ao soltar. Executor de projétil já existe; falta a **mecânica de input de carga**. | **IMPLEMENTAR** (resolver flag dormente inconsistente). |
| `ranged_marked_prey` | Marca um alvo; disparos nele causam bônus / revela | ✅ Fantasia de caçador, sinergia com a árvore inteira | Aplica status "marcado" no inimigo (ícone sobre o alvo); consumido/aproveitado por outros disparos. Precisa do **executor de debuff**. | **IMPLEMENTAR**. |

### MAGIC

| Skill | Conceito | Diversão/distinção | Animação/mecânica | Decisão |
|---|---|---|---|---|
| `magic_elemental_ward` | Buff temporário de resistência elemental | ✅ Botão defensivo, lê bem antes de salas perigosas | Aura/escudo no player por N s (sprite overlay). Precisa do **executor de buff** (aplica status de resistência temporário a si). | **IMPLEMENTAR**. |
| `magic_slowing_sigils` | Campo de lentidão em pequena área | ✅ Controle de área, distinto do chill single-target | Marca no chão + AoE que aplica Slow a quem entra por N s. Precisa de **executor de zona** (ou AoE com tick de status). | **IMPLEMENTAR**. |

### SURVIVAL

| Skill | Conceito | Diversão/distinção | Animação/mecânica | Decisão |
|---|---|---|---|---|
| `survival_emergency_roll` | Rolamento de emergência | ❌ Dodge já é ability não-slot; HUD guard proíbe Dodge em slot; hoje mapeado p/ regar planta (debug) | — | **CORTAR** o nó de slot. Se quiser valor: **CONVERTER** em passiva de dodge (i-frames/custo). |
| `survival_last_breath` | Cura/escudo emergencial, CD alto | ✅ "Panic button" — tensão de quando usar | SelfRestore com CD alto + flash de cura. **Reuso direto** de `SelfRestoreSkillEffectExecutor`. | **IMPLEMENTAR** (trivial; já existe o executor). |
| `survival.sinal_retirada` | Buff curto de evasão (-custo stamina ao recuar) | 🟡 Conceito ok, mas fino e sobrepõe `marca_eficiencia` | Self-buff curto. Precisa do **executor de buff**. | **MESCLAR** com `last_breath` ou virar buff de disengage real (move speed + stamina). Decisão humana. |
| `survival.isca_improvisada` | Lança isca que distrai criaturas simples | ✅ Utilidade tática distinta (puxa aggro), ótimo em swarm | Spawna decoy temporário que rouba aggro de inimigos simples (boss imune). Precisa do **executor de spawn de entidade**. | **IMPLEMENTAR** (compartilha executor com sigils/zona). |

### CRAFTING

| Skill | Conceito | Diversão/distinção | Animação/mecânica | Decisão |
|---|---|---|---|---|
| `crafting.irrigador_portatil` | Rega grupo de plots próximos | ✅ Utilidade de farm real, sinergia com o pilar farm-sim | Reusa o padrão `FarmCropSkillEffectExecutor` (já existe p/ água); estende para raio/grupo. | **IMPLEMENTAR**. |
| `crafting_field_patch` / `crafting_quick_repair` | Reparo de equipamento em campo | 🟡 Útil (durability existe), mas dois nós quase idênticos | Aplica reparo via `EquipmentManager`/durability (skill `equipment-durability-repair`). | **MESCLAR os dois em um** ("Reparo de Campo": repara item ativo, custo material) e **IMPLEMENTAR**. |
| `crafting.bomba_improvisada` | Bomba arremessada (stagger/swarm) | ✅ Já implementada (projétil tóxico 18 dmg). | OK | **MANTER** (já real). |
| `crafting.mecanismo_campo` | "Puxa item ou ativa mecanismo" | ❌ Vago; sem fantasia clara nem alvo de sistema | — | **CORTAR** — ou redefinir cristalino (ex.: "Gancho": puxa item/recurso distante) antes de aceitar. Decisão humana. |
| `crafting.marca_eficiencia` | Buff curto -custo stamina de ações agrícolas | 🟡 Fino; sobrepõe sinal_retirada | Self-buff. | **MESCLAR** num único buff utilitário de Crafting ou **CORTAR**. |

### Passivas-fantasma de Crafting (hooks sem consumidor)

`crafting_shop_sense` (gold), `crafting_material_eye` (harvest), `crafting_station_focus` (craft cost), `crafting_salvage_method` (tool), `crafting_pack_order` (inventário) publicam hook nomeado que **nenhum sistema consome**. Efeito real = 0%.

- **Decisão recomendada:** ou **implementar o consumidor** do hook nesta leva (economia/farm já existem para shop_sense, material_eye, station_focus), ou **converter** em passiva com efeito real imediato (ex.: `station_focus` vira `CraftTimeReductionPercent` de verdade, que já é suportado), ou **CORTAR**. Nada comprável pode ficar em 0%.

---

## 4. Correções de balance (todas as trees)

1. **Cobrar custo de stamina/mana antes de executar** (`TODO_INTEGRATION_NOT_FINAL` em `ActiveSkillExecutionController`). Sem isso, todas as ativas são spam grátis e o balance inteiro é fictício. Lookup de custo → `StaminaManager`/`ManaManager.TrySpend` → senão, `FailureReason` "recurso insuficiente" + feedback. **Pré-requisito de qualquer balance honesto.**
2. **Capstones de Ranged/Survival/Crafting** hoje são só +stat (anticlímax). Dar a eles um payoff de identidade (ativa-assinatura ou escolha XOR como Melee/Magic), ou ao menos um efeito não-trivial.
3. **Filler imposto no caminho do capstone:** tier-gating obriga comprar 8 nós. Garantir que os 8 do caminho "mais curto" sejam nós que o jogador *queira* (não só +1 stat). Reordenar prereqs se necessário.
4. **Micro-passivas (+0.05 move, +1 stat):** mantê-las como sink de rank de longo prazo é ok, mas não devem bloquear o caminho principal. Conferir prereqs.
5. **Slots vs. quantidade de ativas:** com só 4 slots e ~19 ativas reais, não falta *quantidade* de ação — falta cada ativa **funcionar e ser distinta**. Densificar Ranged (charged + marked) é a única adição de ação realmente necessária.

---

## 5. Roster final proposto (após saneamento) — visão alvo

> Aprovação humana converte isto na spec de implementação. Contagens mudam conforme cortes/merges aprovados.

- **Melee (sem mudança estrutural):** 7 ativas reais; `guarded_block` convertido/cortado.
- **Ranged:** +charged_shot real, +marked_prey → 5 ativas reais. Capstone com payoff.
- **Magic:** +elemental_ward, +slowing_sigils → 8 ativas reais.
- **Survival (saneada):** kit_emergencia, campo_seguro, last_breath, isca_improvisada (+ sinal_retirada se não mesclada) → 4–5 ativas reais; emergency_roll cortado. Restante passivo segue.
- **Crafting (saneada):** bomba_improvisada, irrigador_portatil, reparo_de_campo (merge field_patch+quick_repair) → 3 ativas reais; mecanismo_campo/marca_eficiencia cortados ou redefinidos; hooks-fantasma convertidos para efeito real ou cortados.

Novos executores a criar (compartilhados): **(A) BuffSkillEffectExecutor** (ward, marked como debuff, sinal_retirada, marca_eficiencia) e **(B) SpawnSkillEffectExecutor** (isca/decoy, slowing_sigils/zona). Mais a **mecânica de charge** (input) para charged_shot e o **reuso de equipment repair** para reparo_de_campo.

---

## 6. Próximos passos

1. **[humano]** Confirmar os vereditos da seção 3 (em especial os CORTAR/MESCLAR: guarded_block, emergency_roll, mecanismo_campo, marca_eficiencia, sinal_retirada; e o destino dos 4 hooks-fantasma de Crafting).
2. **[spec]** Escrever spec descritiva de implementação para os IMPLEMENTAR aprovados: dois executores novos (Buff, Spawn), charge input, equipment-repair reuse, correção das inconsistências de wiring, e enforcement de custo. Inclui números de balance por skill, status novos e notas de animação/VFX.
3. **[spec]** Escrever spec da HUD de active skills (cooldown visual, custo, 4 botões clicáveis, estados bloqueados) sobre o catálogo já saneado.

---

## 7. Decisões fechadas (humano, 2026-06-23)

- **Cortes confirmados:** `melee_guarded_block`, `survival_emergency_roll`, `crafting.mecanismo_campo` são **removidos dos active slots**. Block e Dodge seguem como abilities puras (Shift/Space), não ocupam slot. mecanismo_campo sai da árvore.
- **Buffs finos mesclados:** `survival.sinal_retirada` + ideia de disengage → **1 buff de Survival** (disengage: move speed + redução de custo de stamina, curto). `crafting.marca_eficiencia` → **1 buff de Crafting** (eficiência: −custo stamina de ações agrícolas/craft próximas, curto). Cada um mais forte; um único nó por tree.
- **Passivas-fantasma de Crafting → efeito real agora:** `station_focus` usa `CraftTimeReductionPercent` real (já suportado); `shop_sense` e `material_eye` plugam na economia/farm existentes; `salvage_method` recebe efeito real de tool/recurso ou é convertida. **Nada comprável fica em 0%.**
- **Dash/Dodge/Block em slot vs. HUD guard (RESOLVIDO — avaliado):** o `FinalHudGuardValidator.IsDashDodgeBlock` só sinaliza IDs que começam com `skill_dash`/`skill_dodge`/`skill_block`. Os lunge-strikes têm IDs `skill_melee_*` / `skill_survival_*`, então **não são pegos** — o guard já mira apenas as abilities puras Shift/Space/Block. **Decisão: `battle_dash` e demais lunge-strikes permanecem nos slots** (são ataques com gap-closer, não traversal). Nenhuma mudança no guard. Documentar a intenção no comentário do guard para não regredir.

## 8. Decisões ainda abertas (baixa prioridade)

- **Pool único vs. separado:** mantido pool único (decisão tomada). Registrado que Survival/Crafting são utilidade universal competindo com builds de combate — aceitável, mas é o motivo de elas atraírem menos pontos.
- **Capstones fracos (Ranged/Survival/Crafting):** transformá-los em capstones com escolha XOR custa design extra; confirmar apetite numa leva futura (não bloqueia a spec de saneamento/implementação).
