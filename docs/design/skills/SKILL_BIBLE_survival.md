# Bíblia de Skills — Árvore SURVIVAL

> **Status:** PROPOSTA (design, sem código). Documento descritivo de referência. Não promove nada nem move specs.
> **Data:** 2026-06-23.
> **Escopo:** árvore Survival do catálogo de skills de Cindar's Hope (RPG pixel art 2D + farm sim, Unity/C#).
> **Fonte de verdade do catálogo:** `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs` (`BuildSurvivalNodes`).
> **Fonte de verdade dos executores ativos:** `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs`.
> **Decisões de saneamento:** `docs/design/SKILL_CATALOG_DESIGN_REVIEW_v1.0.md` (seção 7 — decisões fechadas).
> **Implementação prevista:** os itens marcados "nova(implementar)" e "mesclada" entram em **fable_70**.

---

## Legenda

- **Status no saneamento:**
  - `mantida` — passiva já existente, sem mudança mecânica.
  - `já-implementada` — ativa com executor real registrado hoje.
  - `nova(implementar)` — ativa cujo conceito está aprovado mas o executor real ainda não existe (placeholder ou ausente) → implementar em fable_70.
  - `mesclada` — nó que absorve o conceito de outro nó cortado/fundido.
  - `cortada` — removida dos active slots.
- **Tipo:** Passiva (efeito automático) ou Ativa (equipável em 1 dos 4 active slots, teclas 1–4).
- **SP:** todo nó custa **1 Skill Point** por rank.

## Bandas de balance (referência para todos os valores propostos)

| Categoria | Banda |
|---|---|
| Restore HP | 15–30 |
| Restore Stamina | 25–50 |
| Restore Mana | 15 |
| Cooldown utilitário/restore | 30–60 s |
| Buff: duração | 4–10 s |
| Buff: cooldown | 15–30 s |
| Custo de stamina de utilitário | 0–15 |

---

## Mapa da árvore SURVIVAL (após saneamento)

A árvore é **utilidade/caverna/defesa**: resistências ambientais, economia de recursos (stamina/fome), sobrevivência em swarm e botões de pânico. O saneamento removeu o excesso de passivas mortas e o nó de slot fantasma (`emergency_roll`), e converteu `sinal_retirada` num buff de **disengage** de verdade.

```
T1  cave_lungs ──┬── hard_skin ──┬── toxic_sense ── (T3) status_recovery
                 │               ├── cold_habit
                 │               └── heat_temper
                 └── low_rations ── (T2) safe_step ──┬── (T2) DISENGAGE (sinal_retirada mesclada)
                                                     └── (T2) isca_improvisada ── (T3) instinto_sobrevivencia

(T3) status_recovery ── (T4) last_breath
(T2) DISENGAGE ── (T3) kit_emergencia ── (T4) campo_seguro ── (T5) capstone_caveborn
```

### Reaponte de prereqs após o saneamento (explicação curta)

1. **`survival_emergency_roll` foi cortado.** Ele estava em T2 entre `safe_step` e nada à frente (era folha). Como era folha, removê-lo **não quebra nenhuma cadeia** — nenhum nó tinha `emergency_roll` como prereq. Reaponte: nenhum necessário.
2. **`sinal_retirada` vira o buff DISENGAGE** e continua com `prereq: safe_step` (T2). Ela **absorve** o conceito de disengage; permanece o nó de entrada das ativas de recurso.
3. **`kit_emergencia`** mantém `prereq: sinal_retirada` (agora DISENGAGE). A cadeia de recursos (DISENGAGE → kit → campo_seguro → capstone) fica intacta e coerente: você passa pelo buff de fuga antes de ganhar os botões de recovery.
4. **`isca_improvisada`** continua em `prereq: safe_step` (T2), abrindo o ramo tático (isca → instinto), separado do ramo de recovery. Isso dá duas rotas distintas a partir de `safe_step`.
5. **`capstone_caveborn`** mantém `prereq: campo_seguro` e `reqNodes: 8` — o caminho mais curto até ele passa por nós que o jogador quer (DISENGAGE, kit, campo_seguro), não por filler puro.

---

# PASSIVAS

### Pulmões da Caverna (`survival_cave_lungs`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Survival / T1 / — / 1 SP / Passiva
- **Status no saneamento:** mantida
- **Resumo (1 frase):** Aumenta a Stamina máxima do player.
- **Descrição completa:** Nó-raiz da árvore Survival. Treino de respiração que expande o fôlego em ambientes fechados/profundos, elevando o teto de Stamina e permitindo mais ações antes da exaustão.
- **Mecânica (PASSIVA):**
  - **Efeito:** eleva `MaxStamina`.
  - **Modificador:** `MaxStaminaFlat`, **+10** por rank.
  - **Stack por rank?** Sim — valor escala por rank comprado. Cap recomendado: 5 ranks (+50 total).
  - **Condição:** sempre ativo (não exige caverna).
  - **Retroativo?** Sim — eleva o teto imediatamente ao comprar; a stamina atual não é preenchida, mas o novo teto vale na hora.
- **Q&A fechado:**
  1. **Precisa estar na caverna?** Não. Apesar do nome temático, o bônus é global.
  2. **É retroativo / vale na hora?** Sim. O teto sobe no instante da compra.
  3. **Empilha por rank?** Sim, +10 por rank até o cap.
  4. **Enche a stamina atual ao comprar?** Não — só o teto sobe; a barra atual não é recarregada.
  5. **Conflita com outras fontes de MaxStamina (capstone)?** Não — soma de forma aditiva com o `+5` do capstone e qualquer outra fonte.
  6. **É prereq de quê?** De `hard_skin` e `low_rations` (gatekeeper de toda a árvore).

### Pele Dura (`survival_hard_skin`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Survival / T1 / `survival_cave_lungs` / 1 SP / Passiva
- **Status no saneamento:** mantida
- **Resumo (1 frase):** Aumenta o HP máximo do player.
- **Descrição completa:** Calos e resistência física acumulados pela vida dura na caverna; eleva o teto de vida.
- **Mecânica (PASSIVA):**
  - **Efeito:** eleva `MaxHP`.
  - **Modificador:** `MaxHPFlat`, **+5** por rank.
  - **Stack por rank?** Sim. Cap recomendado: 5 ranks (+25 total).
  - **Condição:** sempre ativo.
  - **Retroativo?** Sim — teto sobe na hora; HP atual não é preenchido.
- **Q&A fechado:**
  1. **Precisa de caverna?** Não, global.
  2. **Retroativo?** Sim, teto sobe imediatamente.
  3. **Empilha por rank?** Sim, +5 por rank até o cap.
  4. **Cura ao comprar?** Não, só o teto.
  5. **Conflito?** Soma aditiva com qualquer outro `MaxHPFlat`.
  6. **Abre o quê?** É prereq de `toxic_sense`, `cold_habit` e `heat_temper` (ramo das resistências ambientais).

### Rações Curtas (`survival_low_rations`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Survival / T1 / `survival_cave_lungs` / 1 SP / Passiva
- **Status no saneamento:** mantida
- **Resumo (1 frase):** Reduz a velocidade com que a fome aumenta.
- **Descrição completa:** Disciplina alimentar — o corpo aprende a render mais com menos. Reduz o dreno de fome ao longo do tempo, estendendo o tempo entre refeições.
- **Mecânica (PASSIVA):**
  - **Efeito:** reduz a taxa de `HungerDrain`.
  - **Modificador:** `HungerDrainReduction`, **−0.1** por rank.
  - **Stack por rank?** Sim, mas com cuidado de cap — não permitir dreno ≤ 0 (caso contrário a fome nunca subiria). Cap recomendado: até o sistema de fome definir o piso; sugerido máximo 3 ranks (−0.3).
  - **Condição:** sempre ativo.
  - **Retroativo?** Sim — a nova taxa vale a partir da compra; não recupera fome já gasta.
- **Q&A fechado:**
  1. **Precisa de caverna?** Não, global.
  2. **Retroativo?** Sim, a taxa muda na hora; não restaura fome já consumida.
  3. **Empilha por rank?** Sim, −0.1 por rank, com piso (dreno nunca chega a 0/negativo).
  4. **Restaura comida/fome ao comprar?** Não — só desacelera o ganho de fome.
  5. **Conflito?** Pode somar com itens/buffs que reduzam fome; respeitar o piso.
  6. **Abre o quê?** É prereq de `safe_step`.

### Senso Tóxico (`survival_toxic_sense`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Survival / T1 / `survival_hard_skin` / 1 SP / Passiva
- **Status no saneamento:** mantida
- **Resumo (1 frase):** Aumenta a resistência a dano/efeito tóxico.
- **Descrição completa:** Tolerância adquirida a esporos, gases e venenos da caverna. Eleva a resistência tóxica do player.
- **Mecânica (PASSIVA):**
  - **Efeito:** eleva `ToxicResistance`.
  - **Modificador:** `ToxicResistanceBonus`, **+1** por rank.
  - **Stack por rank?** Sim. Cap recomendado: conforme o teto de resistência do sistema de combate (sugerido 5 ranks).
  - **Condição:** sempre ativo.
  - **Retroativo?** Sim.
- **Q&A fechado:**
  1. **Precisa de caverna?** Não, global.
  2. **Retroativo?** Sim.
  3. **Empilha por rank?** Sim, +1 por rank até o cap do sistema.
  4. **Conflito?** Aditivo com o `+1` Toxic do capstone.
  5. **Abre o quê?** É prereq de `status_recovery`.

### Hábito do Frio (`survival_cold_habit`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Survival / T1 / `survival_hard_skin` / 1 SP / Passiva
- **Status no saneamento:** mantida
- **Resumo (1 frase):** Aumenta a resistência ao frio.
- **Descrição completa:** Aclimatação a ambientes gelados; eleva a resistência ao frio do player.
- **Mecânica (PASSIVA):**
  - **Efeito:** eleva `ColdResistance`.
  - **Modificador:** `ColdResistanceBonus`, **+1** por rank.
  - **Stack por rank?** Sim. Cap recomendado: 5 ranks.
  - **Condição:** sempre ativo.
  - **Retroativo?** Sim.
- **Q&A fechado:**
  1. **Precisa de bioma frio para valer?** Não — o bônus existe sempre; só é relevante quando há dano/efeito de frio.
  2. **Retroativo?** Sim.
  3. **Empilha por rank?** Sim, +1 por rank.
  4. **Conflito?** Aditivo com o `+1` Cold do capstone.
  5. **Folha?** Sim — não é prereq de outro nó (caminho de identidade, não de progressão).

### Têmpera do Calor (`survival_heat_temper`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Survival / T1 / `survival_hard_skin` / 1 SP / Passiva
- **Status no saneamento:** mantida
- **Resumo (1 frase):** Aumenta a resistência ao calor.
- **Descrição completa:** Aclimatação a forjas, câmaras de lava e regiões quentes; eleva a resistência ao calor do player.
- **Mecânica (PASSIVA):**
  - **Efeito:** eleva `HeatResistance`.
  - **Modificador:** `HeatResistanceBonus`, **+1** por rank.
  - **Stack por rank?** Sim. Cap recomendado: 5 ranks.
  - **Condição:** sempre ativo.
  - **Retroativo?** Sim.
- **Q&A fechado:**
  1. **Precisa de bioma quente?** Não — bônus sempre presente, relevante sob dano/efeito de calor.
  2. **Retroativo?** Sim.
  3. **Empilha por rank?** Sim, +1 por rank.
  4. **Conflito?** Aditivo com o `+1` Heat do capstone.
  5. **Folha?** Sim — não é prereq de outro nó.

### Recuperação Instintiva (`survival_status_recovery`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Survival / T3 / `survival_toxic_sense` / 1 SP / Passiva
- **Status no saneamento:** mantida
- **Resumo (1 frase):** Reduz a duração dos status negativos Poison, Burn e Slow.
- **Descrição completa:** O corpo metaboliza venenos, queimaduras e lentidão mais rápido. Encurta a duração de Poison, Burn e Slow aplicados ao player.
- **Mecânica (PASSIVA):**
  - **Efeito:** reduz a duração restante de Poison/Burn/Slow ao serem aplicados.
  - **Modificador:** `StatusDurationReduction`, **−0.1** por rank (interpretado como −0.1 s ou −10% conforme a unidade do sistema de status; documentar na implementação fable_70).
  - **Stack por rank?** Sim, com piso (a duração nunca pode ir a 0/negativa, senão o status nunca aplica). Cap recomendado: respeitar piso do sistema.
  - **Condição:** sempre ativo; aplica-se aos três status nomeados (Poison, Burn, Slow). Não afeta outros status.
  - **Retroativo?** Não para status já ativos no momento da compra — vale a partir da próxima aplicação. (Sugestão: aplicar só na aplicação do status, não em tick contínuo.)
- **Q&A fechado:**
  1. **Reduz qualquer status?** Não — só Poison, Burn e Slow.
  2. **Encurta um status que já está ativo agora?** Não — vale para as próximas aplicações.
  3. **Empilha por rank?** Sim, −0.1 por rank, com piso.
  4. **Precisa de caverna?** Não, global.
  5. **Pode zerar a duração (imunidade efetiva)?** Não — há piso; o objetivo é encurtar, não imunizar.
  6. **Abre o quê?** É prereq de `last_breath`.

### Passo Seguro (`survival_safe_step`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Survival / T2 / `survival_low_rations` / 1 SP / Passiva
- **Status no saneamento:** mantida
- **Resumo (1 frase):** Aumenta a velocidade de movimento, reduzindo a penalidade de terreno difícil.
- **Descrição completa:** Pisada firme em terreno irregular da caverna. Reduz a penalidade de movimento em terreno difícil, resultando em um pequeno ganho líquido de velocidade.
- **Mecânica (PASSIVA):**
  - **Efeito:** eleva `MoveSpeed` (modela a penalidade de terreno reduzida).
  - **Modificador:** `MoveSpeedBonus`, **+0.05** por rank.
  - **Stack por rank?** Sim, mas micro-passiva — manter como sink de longo prazo. Cap recomendado: 3 ranks (+0.15).
  - **Condição:** sempre ativo.
  - **Retroativo?** Sim — velocidade muda na hora.
- **Q&A fechado:**
  1. **Só em terreno difícil?** O efeito é modelado como bônus global de move speed (a "penalidade reduzida" é a justificativa de fantasia); na prática vale sempre.
  2. **Retroativo?** Sim.
  3. **Empilha por rank?** Sim, +0.05 por rank (micro-passiva).
  4. **Bloqueia o caminho do capstone?** Não — é nó de passagem desejável (abre DISENGAGE e isca), não filler puro.
  5. **Abre o quê?** É prereq de **DISENGAGE** (`sinal_retirada`) e de `isca_improvisada` — bifurca os dois ramos ativos.

### Recuperação Instintiva vs. capstone — nota de não-conflito
Os bônus de resistência do capstone e as passivas de resistência (`toxic_sense`/`cold_habit`/`heat_temper`) **somam de forma aditiva**; nenhum substitui o outro.

---

# ATIVAS

> Todas as ativas ocupam **1 dos 4 active slots** (teclas 1–4) e são executadas pelo `ActiveSkillExecutionController`. Regras gerais hoje:
> - **Cooldown:** rastreado por slot (`_slotCooldowns`); uma ativa em cooldown não dispara e deve expor `FailureReason` ("em recarga").
> - **Custo de recurso:** o enforcement de custo de Stamina/Mana está marcado `TODO_INTEGRATION_NOT_FINAL` no controller. Quando ligado (pré-requisito de balance honesto), faltar recurso → não executa + `FailureReason` "recurso insuficiente" + feedback (toast/SFX via `PlayerActionFeedbackEvent`).
> - **Combate:** as ativas Survival são utilitárias/restore; **podem ser usadas em combate** (não há gate de "fora de combate") salvo onde indicado, mas seu valor é estratégico, não burst de dano.

### Kit de Emergência (`survival.kit_emergencia`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Survival / T3 / `survival.sinal_retirada` (DISENGAGE) / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada
- **Resumo (1 frase):** Usa um kit que restaura HP instantaneamente, com cooldown médio.
- **Descrição completa:** Botão de cura tático. O player aplica um kit de primeiros socorros, restaurando vida na hora. Cooldown impede spam; serve para estabilizar entre lutas ou no meio de uma sala perigosa.
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla do slot equipado (1–4).
  - **Alvo:** self.
  - **Alcance/raio:** —
  - **Efeito:** restore (cura).
  - **Valores (reais hoje):** **+30 HP**, +0 Stamina, +0 Mana. (No teto da banda de restore HP 15–30.)
  - **Status aplicado:** nenhum.
  - **Custo:** sem custo de Stamina hoje (custo de recurso pendente do enforcement global). Não consome item de inventário — é uma carga abstrata da skill, gateada pelo cooldown. (Decisão de design: **não consome item**; o cooldown É o custo.)
  - **Cooldown:** **45 s** (por slot).
  - **Movimento:** não trava o movimento (uso instantâneo).
  - **Telegraph/Animação/VFX:** flash verde de cura sobre o sprite do player + partícula curta de "+". Sem telegraph para inimigos (é defensivo/self).
- **Q&A fechado:**
  1. **Pode usar em combate ou só fora?** Em combate, sim — é o uso principal (estabilizar sob pressão).
  2. **Trava movimento?** Não — uso instantâneo, o player continua livre.
  3. **Funciona em boss?** Não interage com o boss (é self-cura); útil em qualquer luta.
  4. **Consumo e falta de recurso?** Não consome item de inventário; o gate é o cooldown de 45 s. Quando o custo de Stamina global for ligado, faltar stamina → não executa + "recurso insuficiente".
  5. **Empilha com outra cura/buff? refresh?** A cura é instantânea (não é HoT), então não "empilha" — soma ao HP atual respeitando o teto de MaxHP. Não há refresh (não é status).
  6. **Cura acima do MaxHP?** Não — clampa no MaxHP.
  7. **Em cooldown, o que acontece?** Não executa; expõe `FailureReason` "em recarga".

### Campo Seguro (`survival.campo_seguro`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Survival / T4 / `survival.kit_emergencia` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada (executor self-restore) — proposta de aura adicional em fable_70
- **Resumo (1 frase):** Restaura HP, Stamina e Mana de uma vez e cria por alguns segundos uma pequena aura que reduz cansaço/fome.
- **Descrição completa:** O "acampamento relâmpago" da árvore. O player monta um ponto seguro improvisado: recupera um pouco de tudo (vida, fôlego e mana) instantaneamente. **Esclarecimento de design (hoje vs. alvo):** o executor atual é **self-restore puro** (sem zona física). A proposta de identidade para fable_70 é manter o restore self e adicionar uma **aura curta de redução de ganho de cansaço/fome por alguns segundos** centrada no player — não uma zona colocável no chão. Isso preserva a fantasia de "campo seguro" sem precisar de spawn de entidade persistente.
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla do slot (1–4).
  - **Alvo:** self (a aura segue o player, não é colocada no chão).
  - **Alcance/raio:** aura proposta ~2 tiles ao redor do player (afeta só o player nesta versão single-player).
  - **Efeito:** restore (cura tripla) + buff curto de economia (proposta).
  - **Valores (reais hoje):** **+15 HP, +25 Stamina, +15 Mana** (todos dentro das bandas: HP 15–30, Stamina 25–50, Mana 15).
  - **Status aplicado (proposta fable_70):** `status_safe_field` por **~6 s** — reduz ganho de cansaço/fome enquanto ativo. (Sem este status, o nó já funciona como restore tripla.)
  - **Custo:** sem custo de item; gateado por cooldown. (Custo de Stamina sob o enforcement global futuro.)
  - **Cooldown:** **60 s** (por slot).
  - **Movimento:** não trava (uso instantâneo; a aura proposta acompanha o player).
  - **Telegraph/Animação/VFX:** breve halo/círculo de luz suave no chão sob o player + três flashes (verde/azul/roxo) indicando HP/Mana/Stamina. Sem telegraph hostil.
- **Q&A fechado:**
  1. **É uma zona colocável ou self?** Hoje é **self-restore**. A proposta fable_70 é self-restore + aura curta que acompanha o player (não um objeto fixo no chão).
  2. **Pode usar em combate?** Sim. É o "respiro" pesado; o CD de 60 s impede abuso.
  3. **Trava movimento?** Não — instantâneo; a aura proposta segue o player.
  4. **Outros (NPCs/aliados) entram na aura?** Não — single-player; a aura afeta só o player.
  5. **Empilha com outra cura? refresh da aura?** O restore é instantâneo (clampa nos tetos). A aura proposta `status_safe_field` faz refresh (não stack) se reusada — mas o CD de 60 s torna isso improvável.
  6. **Funciona em boss?** Não interage com o boss; é puramente defensivo/self.
  7. **Falta de recurso?** Não consome item; gate é o CD. Sob enforcement de custo futuro, sem stamina → não executa + feedback.

### Instinto de Sobrevivência (`survival.instinto_sobrevivencia`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Survival / T3 / `survival.isca_improvisada` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** nova(implementar como REVEAL) — **o executor atual é placeholder** (`SelfRestore` +50 Stamina). O efeito real é REVELAR.
- **Resumo (1 frase):** Revela brevemente recursos, perigos leves e interagíveis próximos ao player.
- **Descrição completa:** Pulso de percepção aguçada. Ao ativar, o player destaca por alguns segundos o que está ao redor: nós de recurso minerável/coletável, armadilhas/perigos leves, e objetos interagíveis (baús, alavancas, saídas). **Atenção de implementação:** o executor registrado hoje (`survival.instinto_sobrevivencia` → SelfRestore, +50 Stamina, CD 30 s) é **placeholder de slice**; o `+50 Stamina` NÃO é o efeito de design. Em **fable_70**, substituir por um efeito de **REVEAL** (utility, sem dano, sem restore). Documentar a troca no execution report.
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla do slot (1–4).
  - **Alvo:** self (origem do pulso é o player); efeito em área ao redor.
  - **Alcance/raio:** **raio ~6 tiles** ao redor do player (proposta).
  - **Efeito:** reveal — destaca por outline/ícone os alvos válidos no raio.
  - **Valores:** **duração do reveal ~8 s** (proposta). Sem cura, sem dano.
  - **O que destaca:** (a) nós de recurso (minério/planta/coletável), (b) perigos leves (armadilhas, hazards ambientais detectáveis), (c) interagíveis (baús, alavancas, saídas/exits). **Não revela** inimigos ocultos de alto nível nem conteúdo de boss — é "leve".
  - **Status aplicado:** nenhum no player; aplica um destaque visual temporário nos objetos no raio (não é um status de combate).
  - **Custo:** sem custo de item; gate por cooldown. (Stamina sob enforcement futuro.)
  - **Cooldown:** **30 s** (mantém o CD do executor placeholder atual; dentro da banda utilitária 30–60 s).
  - **Movimento:** não trava (pulso instantâneo; o destaque persiste enquanto o player anda).
  - **Telegraph/Animação/VFX:** onda concêntrica de "ping" saindo do player (pixel pulse) + outlines coloridos por categoria (amarelo=recurso, vermelho=perigo, azul=interagível) por ~8 s. Sem telegraph hostil.
- **Q&A fechado:**
  1. **O +50 Stamina é real?** Não — é placeholder de slice. O efeito de design é **reveal**; será trocado em fable_70.
  2. **Pode usar em combate?** Sim, mas o valor é de exploração/scouting; não dá vantagem ofensiva direta.
  3. **Trava movimento?** Não — pulso instantâneo; o destaque continua enquanto o player se move.
  4. **Reveal — raio, duração, o que revela?** Raio ~6 tiles, duração ~8 s; revela recursos, perigos leves e interagíveis. Não revela boss nem inimigos de elite ocultos.
  5. **Funciona em boss/sala de boss?** Não destaca o boss nem mecânicas de boss; só conteúdo "leve" (recurso/interagível/armadilha simples).
  6. **Empilha/refresh?** Reusar antes de expirar faz **refresh** do destaque (reinicia a duração), não acumula raio.
  7. **Falta de recurso?** Não consome item; gate é o CD de 30 s. Sob enforcement futuro, sem stamina → não executa + feedback.

### Último Fôlego (`survival_last_breath`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Survival / T4 / `survival_status_recovery` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** nova(implementar via `SelfRestoreSkillEffectExecutor` + escudo) — **sem mapping hoje** (efeito não implementado).
- **Resumo (1 frase):** Botão de pânico — cura emergencial e escudo temporário, com cooldown muito alto.
- **Descrição completa:** A última carta da árvore Survival. Quando tudo dá errado, o player puxa um fôlego final: cura uma boa porção de HP e ganha um escudo temporário que absorve dano por alguns segundos. CD altíssimo torna o timing crítico. **Implementação:** reusar `SelfRestoreSkillEffectExecutor` para a parte de cura; adicionar a aplicação de um status de escudo (`status_last_breath_shield`) em fable_70.
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla do slot (1–4).
  - **Alvo:** self.
  - **Alcance/raio:** —
  - **Efeito:** restore (cura) + escudo temporário (absorção de dano).
  - **Valores (proposta):** cura **+25 HP** (banda 15–30) + escudo que absorve **30** de dano por **6 s**.
  - **Status aplicado:** `status_last_breath_shield`, duração **6 s** (some ao expirar ou ao esgotar a absorção).
  - **Custo:** sem custo de item; gate por cooldown alto. (Stamina sob enforcement futuro — sugerido baixo ou zero, pois é botão de pânico.)
  - **Cooldown:** **90 s** (por slot) — acima da banda utilitária padrão de propósito; é uma ferramenta de emergência.
  - **Gate de ativação:** **sempre ativável** (não exige estar abaixo de X% de HP). Decisão de design: deixar o player decidir o timing; um gate de "só abaixo de 30% HP" tornaria a skill frustrante quando se quer pré-emptar um golpe forte. (Documentar essa escolha no execution report.)
  - **Movimento:** não trava (uso instantâneo).
  - **Telegraph/Animação/VFX:** flash branco intenso + bolha/escudo translúcido ao redor do player enquanto o escudo durar (sprite overlay piscante quando próximo de expirar).
- **Q&A fechado:**
  1. **Só ativa abaixo de uma % de HP?** Não — **sempre ativável**, para permitir pré-emptar um golpe forte. (Decisão fechada.)
  2. **Pode usar em combate?** Sim — é o uso principal (panic button no meio da luta).
  3. **Trava movimento?** Não — instantâneo; o escudo acompanha o player.
  4. **Funciona em boss?** Sim, no sentido de que o escudo absorve dano do boss; não há taunt nem interação especial com a IA do boss.
  5. **Cura + escudo empilham com outra fonte? refresh?** A cura é instantânea (clampa no MaxHP). O escudo é um status; reusar faz refresh (não soma duas bolhas). Com CD 90 s, refresh é raro.
  6. **O escudo bloqueia status (Poison/Burn)?** Não — absorve **dano direto**; status negativos ainda aplicam (mas `status_recovery`, prereq desta skill, encurta vários deles).
  7. **Falta de recurso?** Não consome item; gate é o CD de 90 s. Sob enforcement futuro, custo de stamina baixo/zero proposto.

### Isca Improvisada (`survival.isca_improvisada`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Survival / T2 / `survival_safe_step` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** nova(implementar) — precisa de **executor de Spawn de entidade** (decoy).
- **Resumo (1 frase):** Arremessa uma isca/decoy que atrai e distrai criaturas simples por alguns segundos.
- **Descrição completa:** Ferramenta tática anti-swarm. O player lança uma isca (boneco/chamariz improvisado) num ponto próximo; criaturas simples no raio passam a focar a isca em vez do player, dando tempo para curar, reposicionar ou fugir. Bosses e inimigos de elite **não** são totalmente distraídos.
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla do slot (1–4).
  - **Alvo:** ponto no chão (arremesso à frente do player) — spawn de entidade decoy.
  - **Alcance/raio:** **alcance do arremesso ~4 tiles**; a isca, ao pousar, gera um **raio de aggro ~4 tiles** ao redor dela.
  - **Efeito:** decoy — rouba aggro de inimigos simples no raio, redirecionando o alvo deles para a isca.
  - **Valores (proposta):** **duração do decoy ~6 s** (dentro da faixa "alguns segundos"); a isca tem HP simbólico (ou é indestrutível por tempo) — proposta: indestrutível, expira só por tempo.
  - **Quantos inimigos atrai:** todos os inimigos **simples** dentro do raio de aggro no momento e enquanto durar — **cap recomendado: 4 inimigos** simultâneos para não trivializar swarms grandes.
  - **Status aplicado:** aplica um redirecionamento de aggro nos inimigos afetados (não é um status de dano); some quando a isca expira.
  - **Custo:** consome **1 carga** (gadget craftado abstrato) + gate por cooldown. Custo de Stamina **~10** sob enforcement futuro (utilitário, banda 0–15).
  - **Cooldown:** **20 s** (por slot).
  - **Movimento:** o arremesso é instantâneo; **não trava** o movimento (lança e segue andando).
  - **Telegraph/Animação/VFX:** pequeno arco de arremesso do objeto + a isca pulsa no chão (chamariz piscante) com um indicador de raio sutil; ícones de "?" sobre os inimigos que trocaram de alvo.
- **Q&A fechado:**
  1. **Pode usar em combate ou só fora?** Em combate — é o uso central (controle de swarm).
  2. **Trava movimento?** Não — arremesso instantâneo; o player segue livre.
  3. **Funciona em boss (decoy/taunt)?** **Não totalmente** — bosses e inimigos de elite ignoram (ou só desaceleram brevemente). Só criaturas **simples** trocam de alvo. (Boss imune ao redirecionamento.)
  4. **Quantos inimigos atrai e por quanto tempo?** Até **4** inimigos simples no raio, por **~6 s**.
  5. **Raio de aggro e alcance do arremesso?** Arremesso ~4 tiles; raio de aggro da isca ~4 tiles.
  6. **A isca pode ser destruída pelos inimigos?** Proposta: indestrutível, expira só por tempo (mantém o efeito previsível). Alternativa documentável: HP simbólico.
  7. **Consumo e falta de recurso?** Consome 1 carga + CD 20 s; sem carga/CD ativo → não executa + `FailureReason` ("sem carga"/"em recarga").
  8. **Empilha (duas iscas)?** Reusar antes de expirar **substitui** a isca anterior (uma isca ativa por vez) — não soma duas.

---

# BUFF MESCLADO

### Disengage (Sinal de Retirada) (`survival.sinal_retirada`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Survival / T2 / `survival_safe_step` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** mesclada — **absorve o conceito de "Sinal de Retirada"** e o transforma no buff de disengage canônico da árvore. Precisa de **executor de Buff** (implementar em fable_70).
- **Resumo (1 frase):** Buff curto de fuga: aumenta a velocidade de movimento e reduz o custo de Stamina de mover-se/esquivar por alguns segundos.
- **Descrição completa:** A skill de "sair de fininho". Ao ativar, o player ganha por um curto período mais velocidade e gasta menos Stamina em movimento e Dodge — ideal para criar distância, reposicionar antes de um restore, ou escapar de uma sala apertada. **Decisão fechada:** este nó **absorve** a ideia de disengage; é o único buff de evasão da Survival (não há um nó separado de "disengage" além deste).
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla do slot (1–4).
  - **Alvo:** self.
  - **Alcance/raio:** —
  - **Efeito:** buff temporário (move speed up + custo de stamina down).
  - **Valores (proposta):** duração **N = 6 s** (banda 4–10 s); **+20% MoveSpeed**; **−50% do custo de Stamina** de movimento/Dodge enquanto ativo.
  - **Status aplicado:** `status_disengage`, duração **6 s**.
  - **Custo:** custo de Stamina da própria skill **~8** (utilitário, banda 0–15) sob enforcement futuro; gate por cooldown.
  - **Cooldown:** **18 s** (por slot; banda de buff 15–30 s).
  - **Movimento:** **não trava** — pelo contrário, o ponto é se mover melhor; o buff começa no instante da ativação.
  - **Telegraph/Animação/VFX:** rastro/afterimage curto atrás do player + ícone de "vento/pés" no HUD enquanto `status_disengage` durar (pisca ao expirar).
- **Q&A fechado:**
  1. **Ela substitui qual nó?** Absorve o conceito de "sinal_retirada/disengage" — é o buff de fuga único da Survival.
  2. **Pode usar em combate?** Sim — é justamente para reposicionar/fugir no meio da luta.
  3. **Trava movimento?** Não — ao contrário, melhora o movimento durante a duração.
  4. **Funciona em boss?** Não interage com a IA do boss (é self-buff); útil para criar distância em qualquer luta.
  5. **Empilha com outro buff de move speed? refresh?** Não soma duas instâncias de `status_disengage`; reusar faz **refresh** da duração. Combina aditivamente com `safe_step` (passiva) e outras fontes de move.
  6. **A redução de custo de Stamina afeta o Dodge (ability pura)?** Sim — reduz o custo de Stamina de mover-se e de Dodge enquanto ativo (sinergia explícita com a ability de esquiva).
  7. **Consumo e falta de recurso?** Custo de Stamina ~8 + CD 18 s; sem stamina/CD ativo → não executa + `FailureReason`.
  8. **Duração N e valores definidos?** Sim: N = 6 s, +20% move, −50% custo de stamina de movimento/Dodge.

---

# CAPSTONE

### Nascido da Caverna (`survival_capstone_caveborn`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Survival / T5 (capstone) / `survival.campo_seguro` / 1 SP / Passiva (capstone)
- **Status no saneamento:** mantida (com payoff de identidade proposto)
- **Resumo (1 frase):** Bônus passivo a todas as resistências e à Stamina máxima, mais um traço de identidade de sobrevivente.
- **Descrição completa:** O coroamento da árvore Survival. O player é, em essência, "nascido da caverna": eleva todas as resistências ambientais e o teto de Stamina, e ganha um traço passivo que reforça a fantasia de sobrevivente extremo. Requer **8 nós** comprados na árvore (`reqNodes: 8`) — o caminho mais curto passa por DISENGAGE, kit_emergencia e campo_seguro, todos nós desejáveis (não filler).
- **Mecânica (PASSIVA / capstone):**
  - **Efeito base (real hoje):** `ToxicResistanceBonus +1`, `ColdResistanceBonus +1`, `HeatResistanceBonus +1`, `MaxStaminaFlat +5`.
  - **Modificador:** soma aditiva com as passivas de resistência e de stamina já compradas.
  - **Stack por rank?** Não — capstone é compra única (1 rank).
  - **Condição:** sempre ativo após desbloqueado; exige `reqNodes: 8` na árvore + prereq `campo_seguro`.
  - **Retroativo?** Sim — bônus aplicados na hora da compra.
  - **Payoff de identidade proposto (fable_70 — concreto, escolher UM):**
    - **Opção A (recomendada):** **−20% de dano ambiental** (tóxico/frio/calor de fonte ambiental, ex.: poças de veneno, frio de bioma, lava) — recompensa direta da fantasia de "imune ao ambiente", além do `+1` de cada resistência.
    - **Opção B (alternativa):** **imunidade a 1 tick de status negativo por minuto** — uma vez por minuto, o primeiro tick de Poison/Burn/Slow é anulado. Sinergia temática com `status_recovery`.
    - **Decisão:** adotar **Opção A** (−20% dano ambiental) como payoff canônico do capstone, por ser legível e sempre relevante na caverna; registrar a Opção B como descartada. (Confirmar no execution report.)
- **Q&A fechado:**
  1. **Empilha por rank?** Não — capstone é único (1 rank).
  2. **É retroativo?** Sim — os bônus valem imediatamente ao comprar.
  3. **Qual o payoff de identidade?** Além de `+1` em todas as resistências e `+5` MaxStamina, **−20% de dano ambiental** (Opção A adotada).
  4. **Precisa de caverna para o payoff valer?** Não tecnicamente — mas o `−20% dano ambiental` é, na prática, mais relevante na caverna (onde estão as fontes ambientais).
  5. **Conflita com as passivas de resistência?** Não — soma aditiva; nada é substituído.
  6. **O que destrava o capstone?** Prereq `campo_seguro` + `reqNodes: 8` comprados na árvore Survival.
  7. **Conflita com capstones de outras árvores?** Não — pools/efeitos independentes; o player pode ter múltiplos capstones se gastar SP em várias árvores.

---

# CORTADAS

### Rolamento de Emergência (`survival_emergency_roll`) — REMOVIDA dos active slots
- **Decisão (saneamento, seção 7):** **cortada** dos active slots. O Dodge segue como **ability pura** (tecla Space), não ocupa slot.
- **Motivo:** duplicava o Dodge (já é ability não-slot); o `FinalHudGuardValidator` proíbe Dodge em slot; e o nó estava mapeado por engano para `farm.crop.water_skill` (efeito de **regar plantação** — placeholder de debug).
- **Ação em fable_70:** remover o mapping de debug (`skill_survival_emergency_roll` → regar plantação) e remover/desabilitar o nó de slot. Nenhuma cadeia de prereq quebra (era folha; nenhum nó dependia dele).
- **Valor residual (opcional, não implementar agora):** se quiser preservar a fantasia, converter futuramente numa **passiva de Dodge** (i-frames ou custo de stamina reduzido na esquiva) — fora do escopo desta entrega.

---

## Apêndice — Resumo de valores reais vs. propostos

| Skill | Tipo | Real hoje | Proposta fable_70 |
|---|---|---|---|
| kit_emergencia | Ativa restore | +30 HP, CD 45 s | (manter) |
| campo_seguro | Ativa restore | +15 HP/+25 Stam/+15 Mana, CD 60 s | + aura `status_safe_field` ~6 s |
| instinto_sobrevivencia | Ativa | placeholder +50 Stam, CD 30 s | trocar por **REVEAL** raio ~6, dur ~8 s, CD 30 s |
| last_breath | Ativa | **sem efeito** (sem mapping) | +25 HP + escudo 30/6 s, CD 90 s, sempre ativável |
| isca_improvisada | Ativa decoy | **sem executor de spawn** | decoy ~6 s, raio ~4, até 4 simples, CD 20 s, custo ~10 |
| sinal_retirada (DISENGAGE) | Ativa buff | **sem executor de buff** | `status_disengage` 6 s, +20% move, −50% custo stam mov/Dodge, CD 18 s |
| capstone_caveborn | Passiva capstone | +1 todas resist, +5 MaxStam | + **−20% dano ambiental** (Opção A) |
| emergency_roll | (cortada) | mapeado p/ regar planta (debug) | remover nó + remover mapping |

> Executores novos exigidos por esta árvore (compartilhados com outras): **BuffSkillEffectExecutor** (DISENGAGE) e **SpawnSkillEffectExecutor** (isca/decoy). Reveal pode ser um executor de utility próprio. Cura+escudo reusa `SelfRestoreSkillEffectExecutor` + aplicação de status de escudo.
