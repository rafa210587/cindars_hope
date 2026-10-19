# Bíblia de Skills — Árvore MELEE (Cindar's Hope)

> **Status:** PROPOSTA (design, sem código). Contrato de design que alimenta a implementação.
> **Data:** 2026-06-23.
> **Escopo:** Árvore Melee completa após o saneamento da seção 7 do `SKILL_CATALOG_DESIGN_REVIEW_v1.0.md`.
> **Fontes de verdade:** `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs` (BuildMeleeNodes + ApplyTiers) e
> `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs` (RegisterCombatExecutors, melee strikes).
>
> Os números de combate (dano, range, arco, stamina, cooldown, lunge, knockback, posture) abaixo são os **valores reais
> já registrados** nos executores `MeleeStrikeSkillEffectExecutor`. Onde um efeito depende de um sistema ainda inexistente
> (taunt real, posture/stagger gameplay, cobrança de stamina), está marcado **"implementar em fable_70"**.

---

## Legenda do template

Cada skill segue **exatamente** esta ordem:

- **Cabeçalho** com nome e id.
- **Árvore / Tier / Prereq / Custo SP / Tipo** — metadados de árvore.
- **Status no saneamento** — mantida | já-implementada | nova(implementar) | mesclada | convertida | cortada.
- **Resumo** — uma frase.
- **Descrição completa** — parágrafo sem ambiguidade.
- **Mecânica (ATIVA)** ou **Mecânica (PASSIVA)** — todos os campos de comportamento.
- **Q&A fechado** — perguntas de uso reais respondidas em definitivo.

**Convenções de combate (todas as ativas melee):**
- Input: teclas numéricas **1–4** mapeiam para os active slots 0–3 (`ActiveSkillExecutionController.Update`).
- Alvo: resolvido por `SkillTargetResolver` a partir do `TargetType` do executor (inimigo mais próximo no arco/range à frente do facing).
- Dano é **Físico** (`MeleeStrikeSkillEffectExecutor`); status só quando explicitado.
- **Cobrança de stamina ainda não é enforced** em runtime (`TODO_INTEGRATION_NOT_FINAL` em `ActiveSkillExecutionController`).
  Os custos abaixo são o contrato de design; o gating real de stamina é **implementar em fable_70**.
- **Posture/stagger** ainda não é um sistema de gameplay; o multiplicador existe como parâmetro do executor mas o efeito de stagger é **implementar em fable_70**.
- Modal aberto bloqueia execução de slot (o controller retorna cedo se `ModalManager.HasActiveModal`).

---

## 1. Passivas

### Pegada de Ferro (`melee_iron_grip`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Melee / T1 / — / 1 SP / Passiva
- **Status no saneamento:** já-implementada
- **Resumo (1 frase):** Aumenta o Attack do player quando empunhando uma arma melee.
- **Descrição completa:** Nó-raiz da árvore Melee. É o ponto de entrada de qualquer build corpo a corpo e o prereq de três ramos (offhand/dual wield, defesa/two-handed via guarded_stance, e o ramo de avanço `melee.avanco_aco`). Concede um bônus plano de ataque que só faz sentido (e só deve ser exibido como ativo) quando a arma equipada é melee. Existe para dar peso ao primeiro ponto investido e abrir a árvore.
- **Mecânica (PASSIVA):** Efeito exato: `+1 Attack` (modificador `AttackFlat`, valor `1`). · Modificador: tipo `AttackFlat`, valor `1f` por rank. · Stack por rank? Sim — `+1` por rank adquirido; cap de rank conforme `SkillTierRules`/SKILL_NUMERIC_ADDENDUM (T1 permite rank alto; ver cap dinâmico por tier). · Condição de aplicação: aplica-se ao stat global de Attack; a fantasia é "com arma melee" — o efetivo de combate só importa quando o golpe é melee. · Retroativo a gear já equipado? Sim — é um stat do player, aplicado imediatamente ao comprar/upar, independentemente de quando a arma foi equipada.
- **Q&A fechado:**
  1. Empilha por rank? Sim, `+1 Attack` por rank.
  2. Qual o cap? Definido pelo cap dinâmico de rank por tier (T1) do SKILL_NUMERIC_ADDENDUM; não é ilimitado.
  3. Conflita com passiva similar? Não — `magic_arcane_edge` também dá `AttackFlat +1`, mas são nós distintos e somam (não há regra de exclusividade entre passivas de stat).
  4. Retroativa a gear já equipado? Sim, é um stat de player aplicado na hora.
  5. Condição de aplicação? Stat global; a intenção de design é benefício de combate melee.
  6. É obrigatória para o capstone? Sim — é o nó-raiz; todos os caminhos da árvore passam por ele.
  7. Funciona fora de combate (farm/town)? O stat existe sempre, mas só tem efeito quando há um golpe melee; não altera farm/town.

### Postura Guardada (`melee_guarded_stance`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Melee / T1 / melee_iron_grip / 1 SP / Passiva
- **Status no saneamento:** mantida
- **Resumo (1 frase):** Aumenta a Defesa do player de forma permanente.
- **Descrição completa:** Nó defensivo de tier 1, gate do ramo two-handed (`melee_two_handed_momentum`) e, após o corte de `melee_guarded_block`, também o novo prereq de `melee_whirl_cut`. Representa a postura sólida do combatente corpo a corpo. Pequeno em magnitude, mas com rank vira sink de longo prazo e dá identidade tanky ao início da build.
- **Mecânica (PASSIVA):** Efeito exato: `+1 Defense` (`DefenseFlat`, `1`). · Modificador: tipo `DefenseFlat`, valor `1f` por rank. · Stack por rank? Sim — `+1` por rank; cap por tier T1 conforme SKILL_NUMERIC_ADDENDUM. · Condição de aplicação: stat global de Defesa, sempre ativo. · Retroativo a gear já equipado? Sim — stat de player, aplicado na compra.
- **Q&A fechado:**
  1. Empilha por rank? Sim, `+1 Defense`/rank.
  2. Cap? Cap dinâmico de rank por tier (T1).
  3. Conflita com passiva similar? Não — é a única fonte de `DefenseFlat` na árvore Melee; soma com qualquer Defesa de gear.
  4. Retroativa? Sim, imediata.
  5. Condição? Nenhuma; sempre ativa.
  6. Bloqueia caminho do capstone? É prereq de two-handed e whirl_cut; está num dos caminhos, mas não é o único ramo até o capstone (o caminho canônico passa por avanço/dash). Mesmo assim é barata e útil.
  7. Funciona fora de combate? A Defesa reduz dano recebido; só tem efeito quando o player toma dano (cave/combate), não em farm/town puro.

### Fluxo de Duas Lâminas (`melee_dual_wield_flow`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Melee / T1 / melee_iron_grip / 1 SP / Passiva
- **Status no saneamento:** mantida
- **Resumo (1 frase):** Aumenta a velocidade de ataque quando o player está com dual wield.
- **Descrição completa:** Nó condicional que premia builds de duas armas. Gate de `melee_offhand_cut` (a ativa de offhand). Só rende quando o loadout é dual wield, o que torna a build de duas lâminas coesa: a passiva acelera os golpes e a ativa de offhand adiciona um corte extra. Diferente das passivas de stat plano, esta é gated por condição de loadout.
- **Mecânica (PASSIVA):** Efeito exato: `+0.1 AttackSpeed` com dual wield (`DualWieldAttackSpeedBonus`, `0.1`). · Modificador: tipo `DualWieldAttackSpeedBonus`, valor `0.1f` por rank. · Stack por rank? Sim — `+0.1`/rank; cap por tier T1. · Condição de aplicação: **só com dual wield equipado** (duas armas de uma mão). Sem dual wield, o bônus não se aplica. · Retroativo a gear já equipado? Sim — assim que o loadout dual wield está ativo, o bônus vale; ao trocar para uma arma só, deixa de valer.
- **Q&A fechado:**
  1. Empilha por rank? Sim, `+0.1 AttackSpeed`/rank.
  2. Cap? Cap dinâmico T1.
  3. Conflita com passiva similar? Não conflita; coexiste com `melee_two_handed_momentum`, mas as condições são mutuamente exclusivas no uso (dual wield vs. two-handed), então na prática você só ativa uma por loadout.
  4. Retroativa a gear já equipado? Sim, depende do loadout atual, não do momento de compra.
  5. Condição obrigatória? Sim — **só dual wield**. Com arma única ou two-handed, efeito zero.
  6. Vale a pena sem build de dual wield? Não; é nó de nicho. Não está no caminho obrigatório do capstone.
  7. Funciona fora de combate? Velocidade de ataque só importa em combate; sem efeito em farm/town.

### Ímpeto de Duas Mãos (`melee_two_handed_momentum`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Melee / T2 / melee_guarded_stance / 1 SP / Passiva
- **Status no saneamento:** mantida
- **Resumo (1 frase):** Adiciona dano aos golpes quando o player empunha uma arma two-handed.
- **Descrição completa:** Contraparte de `dual_wield_flow` para builds pesadas. Gate de `melee_battle_dash` (o gap-closer que abre o ramo T3 ofensivo). Premia o jogador que escolheu arma de duas mãos com dano extra por golpe, reforçando o arquétipo "poucos golpes, cada um pesado". É um nó de caminho — atravessá-lo é a rota natural para dash/leap.
- **Mecânica (PASSIVA):** Efeito exato: `+1 dano` com arma two-handed (`TwoHandedDamageBonus`, `1`). · Modificador: tipo `TwoHandedDamageBonus`, valor `1f` por rank. · Stack por rank? Sim — `+1`/rank; cap por tier T2. · Condição de aplicação: **só com arma two-handed equipada**. · Retroativo a gear já equipado? Sim — vale enquanto a two-handed estiver equipada; ao trocar de arma, deixa de valer.
- **Q&A fechado:**
  1. Empilha por rank? Sim, `+1 dano`/rank.
  2. Cap? Cap dinâmico T2.
  3. Conflita com passiva similar? Mutuamente exclusivo no uso com `dual_wield_flow` (loadouts diferentes); não há erro, apenas só um fica ativo por loadout.
  4. Retroativa? Sim, depende do loadout atual.
  5. Condição obrigatória? Sim — só two-handed.
  6. Bloqueia o caminho do capstone? Está no caminho ofensivo (gate de battle_dash → leap → capstone), então a build canônica passa por ele; é desejável, não filler.
  7. Funciona fora de combate? Não; o bônus de dano só importa em golpes de combate.

### Treino de Esquiva (`melee_dodge_training`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Melee / T3 / melee_battle_dash / 1 SP / Passiva
- **Status no saneamento:** mantida
- **Resumo (1 frase):** Reduz o custo de stamina do Dodge (ability pura, tecla dedicada).
- **Descrição completa:** Prereq direto do capstone `melee_capstone_battle_rhythm`. Importante: o Dodge **não** é uma skill de slot — é uma ability pura (não ocupa slot 1–4), conforme a decisão de saneamento que removeu rolagens dos active slots. Esta passiva torna a esquiva mais sustentável, baixando o custo de stamina por uso. É o último nó antes do capstone, logo está no caminho obrigatório.
- **Mecânica (PASSIVA):** Efeito exato: `-0.1` no custo de stamina do Dodge (`DodgeCostReduction`, `0.1`). · Modificador: tipo `DodgeCostReduction`, valor `0.1f` por rank. · Stack por rank? Sim — `-0.1`/rank; cap por tier T3 (a redução não deve zerar o custo — o consumidor de Dodge aplica um piso; piso real é **implementar em fable_70** se ainda não houver). · Condição de aplicação: aplica-se ao Dodge (ability pura), não a nenhuma ativa de slot. · Retroativo a gear já equipado? N/A para gear; aplica-se ao sistema de Dodge imediatamente ao comprar/upar.
- **Q&A fechado:**
  1. Empilha por rank? Sim, `-0.1` custo de Dodge por rank.
  2. Cap? Cap dinâmico T3; com piso de custo para não chegar a Dodge grátis.
  3. Conflita com passiva similar? `melee_dodge_training` é a única fonte na árvore Melee; `survival_safe_step` (outra árvore) afeta move speed, não custo de dodge — não conflitam.
  4. Afeta as lunge-strikes (battle_dash etc.)? Não — afeta só o Dodge puro. As lunge-strikes têm custo de stamina próprio definido em seus executores.
  5. Retroativa? Sim, imediata.
  6. Bloqueia caminho do capstone? É o prereq direto do capstone — está no caminho obrigatório, mas é genuinamente útil para qualquer build melee que esquive.
  7. Funciona fora de combate? Reduz custo de Dodge sempre; você pode esquivar em qualquer cena, então tem efeito onde quer que o Dodge seja usável.

---

## 2. Ativas

> Todas as ativas abaixo são `EquippableSkill` (ocupam um dos 4 active slots). Input por tecla numérica do slot.
> Os números são os do `MeleeStrikeSkillEffectExecutor` real. As lunge-strikes (avanço, dash, leap, investida)
> **permanecem nos slots** — são ataques com gap-closer, não traversal; o `FinalHudGuardValidator` só proíbe
> Dash/Dodge/Block **puros** (IDs `skill_dash`/`skill_dodge`/`skill_block`), e estas têm IDs `skill_melee_*`.

### Corte da Mão Secundária (`melee_offhand_cut`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Melee / T2 / melee_dual_wield_flow / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada
- **Resumo (1 frase):** Golpe curto e rápido com a arma da mão secundária, num arco amplo à frente.
- **Descrição completa:** A ativa-assinatura da build dual wield. Golpe leve, barato e de cooldown curto, pensado para encaixar entre os ataques normais e manter pressão. Arco largo (140°) compensa o alcance curto, pegando inimigos ligeiramente fora do facing exato. É a ativa de "spam controlado": baixo custo, baixo cooldown, dano modesto.
- **Mecânica (ATIVA):** Input/slot: tecla do slot equipado. · Alvo: inimigo à frente (resolver por arco/range). · Alcance/arco/raio: range `1.2m`, arco `140°`. · Nº de golpes: 1 golpe em arco. · Dano: `8` (Físico). · Status aplicado: nenhum. · Custo: `10` stamina. · Cooldown: `2.5s`. · Movimento: não trava locomoção de forma significativa (golpe parado, sem lunge — `lungeDistance = 0`). · Posture/knockback: nenhum knockback; sem multiplicador de posture. · Telegraph/Animação/VFX: animação curta de corte cruzado com a mão secundária (1 frame de antecipação + 2–3 frames de swing), trilha branca/azulada fina no arco, faísca pequena no impacto. Leve e rápido visualmente.
- **Q&A fechado:**
  1. O que acontece se não houver inimigo no alcance? O golpe executa no ar (swing + VFX), entra em cooldown e consome stamina; nenhum dano é aplicado. É um "miss", não um erro.
  2. Trava o movimento durante a execução? Praticamente não — é um golpe parado e curtíssimo; sem lunge, sem lock prolongado.
  3. Funciona contra boss/elite? Sim — aplica `8` de dano físico normalmente; bosses não têm imunidade a melee.
  4. O que consome e o que acontece se faltar stamina? Consome `10` de stamina (gating real = fable_70). Sem stamina suficiente: a execução deve ser recusada com `FailureReason` "stamina insuficiente" + feedback (implementar gating em fable_70).
  5. Empilha com outra instância dela mesma / outra skill melee? Não há buff persistente para empilhar; o cooldown de `2.5s` impede re-uso imediato. Você pode encadear com outra ativa melee em outro slot (cooldowns são por slot).
  6. Pode ser cancelada / interrompida por stagger ou dano? A animação é tão curta que, na prática, ou já saiu ou não; se o player tomar stagger antes do hitframe, o golpe é interrompido (regra de stagger do player = fable_70).
  7. Funciona fora de combate (farm/town)? A skill executa (swing + cooldown), mas não há alvo para danificar; sem efeito prático fora de combate. Não interage com crops/objetos de farm.

### Corte Giratório (`melee_whirl_cut`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Melee / T2 / **melee_guarded_stance** (ajustado — ver nota) / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada (prereq reapontado)
- **Resumo (1 frase):** Ataque circular de 360° ao redor do player, golpeando todos os inimigos adjacentes.
- **Descrição completa:** Ferramenta anti-swarm da árvore Melee. Gira a arma em volta do player atingindo tudo num raio à volta, ideal quando cercado. Custo de stamina alto e cooldown médio equilibram o controle de área. **Mudança de prereq:** originalmente seu prereq era `melee_guarded_block`; como `guarded_block` foi **cortado dos active slots** (Bloqueio virou ability pura, tecla Shift, sem slot), o prereq de `whirl_cut` passa a ser **`melee_guarded_stance`**. Justificativa: `guarded_stance` é o nó de tier 1 que sobrou no mesmo ramo defensivo de onde `guarded_block` saía, mantendo `whirl_cut` no tier 2 e preservando a posição da skill na árvore sem criar um nó órfão.
- **Mecânica (ATIVA):** Input/slot: tecla do slot. · Alvo: área 360° ao redor do player (todos os inimigos no raio). · Alcance/arco/raio: range `1.7m`, arco `360°`. · Nº de golpes: 1 golpe que atinge todos no raio. · Dano: `10` (Físico) a cada alvo atingido. · Status aplicado: nenhum. · Custo: `22` stamina. · Cooldown: `6s`. · Movimento: golpe parado (sem lunge); o player fica ancorado durante o giro. · Posture/knockback: nenhum knockback; sem multiplicador de posture. · Telegraph/Animação/VFX: rodopio completo da arma com rastro circular contínuo (anel de trail), pequeno windup de 2 frames, faíscas em cada inimigo tocado. Leitura clara de "AoE em volta de mim".
- **Q&A fechado:**
  1. O que acontece se não houver inimigo no alcance? Executa o giro completo (VFX), cooldown e stamina são gastos, zero dano aplicado.
  2. Trava o movimento? Sim, brevemente — o player fica ancorado durante o rodopio (sem lunge); é um golpe estacionário.
  3. Funciona contra boss/elite? Sim — atinge o boss se ele estiver no raio de `1.7m`, aplicando `10` de dano. Sem imunidade.
  4. O que consome / falta de stamina? Consome `22` stamina (alto). Sem stamina: recusa com `FailureReason` (gating em fable_70).
  5. Empilha consigo / outra melee? Não há buff a empilhar; cooldown de `6s`. Pode ser combinada com outras ativas em outros slots.
  6. Pode ser cancelada / interrompida? Se o player levar stagger durante o windup, o giro é interrompido antes dos hits (regra de stagger = fable_70).
  7. Funciona fora de combate? Executa o giro, mas sem alvos não faz nada; não interage com farm/town.
- **Nota de prereq:** documentar no comentário do catálogo que `whirl_cut.prereq` mudou de `guarded_block` → `guarded_stance` por causa do corte de `guarded_block`.

### Avanço de Aço (`melee.avanco_aco`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Melee / T2 / melee_iron_grip / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada
- **Resumo (1 frase):** Avanço curto ofensivo até o inimigo à frente, terminando em golpe.
- **Descrição completa:** Primeiro gap-closer da árvore, plugado direto na raiz (`iron_grip`), e gate de `melee.grito_desafio`. Fecha distância rapidamente e abre com um golpe de dano sólido. Respeita colisão (não atravessa paredes). É a porta de entrada para o sub-ramo de controle (grito → investida). Diferencia-se de `battle_dash` por um lunge mais curto (2.5m vs 3m) e dano maior (12 vs 8), num tier mais baixo.
- **Mecânica (ATIVA):** Input/slot: tecla do slot. · Alvo: inimigo à frente, na direção do facing. · Alcance/arco/raio: range `1.3m`, arco `110°`. · Nº de golpes: 1 golpe ao fim do avanço. · Dano: `12` (Físico). · Status aplicado: nenhum. · Custo: `22` stamina. · Cooldown: `6s`. · Movimento: **lunge de `2.5m`** na direção do facing; respeita colisão (para na parede). · Posture/knockback: nenhum knockback explícito; sem multiplicador de posture. · Telegraph/Animação/VFX: passo rápido para frente (motion blur curto + linha de impulso no chão), arma puxada para trás no início do dash e cravada à frente no impacto; faísca/poeira no ponto final.
- **Q&A fechado:**
  1. O que acontece se não houver inimigo no alcance? O player ainda avança os `2.5m` (gap-closer) e desfere o golpe no vazio; cooldown e stamina gastos, sem dano. Útil só como mobilidade.
  2. Trava o movimento? Sim, durante o lunge o controle de locomoção é tomado pela skill até o avanço completar.
  3. Funciona contra boss/elite? Sim — aplica `12` de dano físico ao boss. O lunge fecha distância normalmente.
  4. O que consome / falta de stamina? Consome `22` stamina. Sem stamina suficiente: recusado com feedback (gating em fable_70).
  5. Empilha consigo / outra melee? Sem buff persistente; cooldown `6s` por slot. Combinável com outras ativas em outros slots.
  6. Pode ser cancelada / interrompida? Durante o lunge o player está comprometido; um stagger recebido pode interromper o avanço (regra em fable_70). Não é cancelável por input.
  7. Funciona fora de combate? Sim como movimento — o lunge de `2.5m` ocorre em qualquer cena (respeitando colisão), mas sem alvo não há dano. É um deslocamento ofensivo, não uma traversal pura (não é pego pelo HUD guard).

### Arrancada de Combate (`melee_battle_dash`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Melee / T3 / melee_two_handed_momentum / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada
- **Resumo (1 frase):** Investida com o lunge mais longo da árvore, fechando bastante distância e abrindo com um golpe leve.
- **Descrição completa:** O gap-closer de maior alcance (lunge `3m`), gate de `melee_leap_attack`. Tem o dano mais baixo entre as lunge-strikes (8), porque seu valor é o reposicionamento agressivo: alcançar um arqueiro/caster, fechar com o inimigo, ou reentrar no combate. É um **ataque com gap-closer, não uma traversal pura** — por isso permanece num slot e **não** é bloqueado pelo `FinalHudGuardValidator` (que só pega IDs `skill_dash` puros, não `skill_melee_battle_dash`).
- **Mecânica (ATIVA):** Input/slot: tecla do slot. · Alvo: inimigo à frente, na direção do facing. · Alcance/arco/raio: range `1.2m`, arco `100°`. · Nº de golpes: 1 ao fim do dash. · Dano: `8` (Físico). · Status aplicado: nenhum. · Custo: `20` stamina. · Cooldown: `5s`. · Movimento: **lunge de `3m`** (o mais longo da árvore), respeita colisão. · Posture/knockback: nenhum. · Telegraph/Animação/VFX: arranque com linhas de velocidade horizontais, silhueta esticada no dash, pequeno "pop" de poeira na largada e no impacto; leitura de "investida rápida e longa".
- **Q&A fechado:**
  1. O que acontece se não houver inimigo no alcance? Avança os `3m` e golpeia o vazio; cooldown e stamina gastos. Serve como reposicionamento.
  2. Trava o movimento? Sim — durante o dash a locomoção é controlada pela skill.
  3. Funciona contra boss/elite? Sim, aplica `8` de dano; o uso típico contra boss é fechar gap rapidamente.
  4. O que consome / falta de stamina? `20` stamina. Sem stamina: recusado com feedback (fable_70).
  5. Empilha consigo / outra melee? Cooldown `5s` por slot, sem buff persistente. Combinável com leap/outras em slots distintos.
  6. Pode ser cancelada / interrompida? Comprometido durante o dash; stagger pode interromper (fable_70). Não cancelável por input.
  7. Funciona fora de combate? Sim como deslocamento — é um ataque com gap-closer, não traversal pura; o lunge ocorre em qualquer cena. Não é pego pelo HUD guard.

### Salto Devastador (`melee_leap_attack`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Melee / T3 / melee_battle_dash / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada
- **Resumo (1 frase):** Salto curto que termina num golpe pesado em arco amplo no ponto de aterrissagem.
- **Descrição completa:** O finisher de dano da árvore Melee comum (14 de dano, o maior entre as ativas não-investida). Salta uma distância curta (2.2m) e baixa a arma com força num arco de 120° na aterrissagem, ideal para abrir combate ou punir um inimigo que recua. Combina mobilidade com o golpe mais forte do kit padrão.
- **Mecânica (ATIVA):** Input/slot: tecla do slot. · Alvo: inimigos no arco do ponto de aterrissagem. · Alcance/arco/raio: range `1.4m`, arco `120°`. · Nº de golpes: 1 golpe pesado na aterrissagem. · Dano: `14` (Físico). · Status aplicado: nenhum. · Custo: `25` stamina. · Cooldown: `7s`. · Movimento: **lunge/salto de `2.2m`**, respeita colisão. · Posture/knockback: nenhum knockback explícito; sem multiplicador de posture. · Telegraph/Animação/VFX: agachamento rápido (windup de salto), arco aéreo com a arma erguida, impacto com onda de choque/poeira radial e flash no ponto de queda. Leitura de "golpe pesado de cima".
- **Q&A fechado:**
  1. O que acontece se não houver inimigo no alcance? O salto e o golpe executam; aterrissa, gasta cooldown e stamina, zero dano se ninguém estiver no arco.
  2. Trava o movimento? Sim — durante o salto o player está comprometido até aterrissar.
  3. Funciona contra boss/elite? Sim — `14` de dano físico ao boss no arco de aterrissagem. É uma boa abertura de luta.
  4. O que consome / falta de stamina? `25` stamina (alto). Sem stamina: recusado (fable_70).
  5. Empilha consigo / outra melee? Cooldown `7s`; sem buff. Combinável com dash/avanço em outros slots para um encadeamento gap-close → leap.
  6. Pode ser cancelada / interrompida? Comprometido no ar; stagger antes do hitframe da aterrissagem interrompe (fable_70). Não cancelável por input.
  7. Funciona fora de combate? O salto ocorre em qualquer cena; sem alvo, sem dano. Não interage com farm/town.

### Grito de Desafio (`melee.grito_desafio`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Melee / T3 / melee.avanco_aco / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada (componente de taunt = nova/implementar em fable_70)
- **Resumo (1 frase):** Brado de 360° que dá dano leve, empurra inimigos próximos e os provoca (taunt) a focar no player por curta duração.
- **Descrição completa:** Ferramenta de controle e tanking, gate de `melee.investida_quebra_guarda`. O dano é secundário (6); o valor está no **knockback** que cria espaço e no **taunt** que força inimigos provocáveis a atacar o player — útil para proteger aliados/NPCs ou puxar aggro de um swarm para um corredor. O golpe de dano + knockback já existe no executor; o **efeito de taunt (provocar) ainda não tem sistema de aggro acoplado e deve ser implementado em fable_70**.
- **Mecânica (ATIVA):** Input/slot: tecla do slot. · Alvo: área 360° ao redor do player. · Alcance/arco/raio: range `2.2m`, arco `360°`. · Nº de golpes: 1 pulso de área. · Dano: `6` (Físico) a cada alvo no raio. · Status aplicado: **TAUNT (provocar)** nos inimigos próximos — proposta de id `status_taunt`, **duração `4s`**, sem stacks (re-aplicar refresca a duração). Boss/elite: **não totalmente provocável** — em vez de forçar o alvo, aplica uma versão atenuada: o boss ganha um pequeno aumento de prioridade de aggro no player por `2s`, mas **mantém a capacidade de trocar de alvo** (taunt parcial). · Custo: `18` stamina. · Cooldown: `8s`. · Movimento: golpe parado (sem lunge); player ancorado durante o brado. · Posture/knockback: **knockbackForce `6`** (o maior da árvore) empurrando inimigos para fora do raio; sem multiplicador de posture. · Telegraph/Animação/VFX: pose de brado (peito estufado), onda de som concêntrica (anel expansivo translúcido), ícone de taunt (símbolo de provocação) brevemente sobre as cabeças dos inimigos provocados; poeira radial do empurrão.
- **Q&A fechado:**
  1. O que acontece se não houver inimigo no alcance? Executa o brado (VFX + cooldown + stamina); sem alvos, nada é provocado nem empurrado.
  2. Trava o movimento? Sim, brevemente — player ancorado durante o brado.
  3. Funciona contra boss/elite? Parcialmente — dano `6` e knockback aplicam (knockback em boss pode ser resistido/reduzido conforme massa do boss = fable_70); o **taunt é atenuado**: boss não é forçado a atacar o player, apenas ganha viés de aggro curto e ainda pode trocar de alvo.
  4. O que consome / falta de stamina? `18` stamina. Sem stamina: recusado com feedback (fable_70).
  5. Empilha consigo / outra melee? Re-usar refresca a duração do taunt (não acumula stacks). O viés de boss também só refresca. Combinável com outras ativas.
  6. Pode ser cancelada / interrompida? Stagger durante o windup interrompe o brado (fable_70). Não cancelável por input.
  7. Funciona fora de combate? O brado executa, mas taunt/knockback só fazem sentido contra inimigos; sem efeito em farm/town. **Dependência:** taunt real precisa do sistema de aggro/threat — **implementar em fable_70**.

### Investida Quebra-Guarda (`melee.investida_quebra_guarda`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Melee / T4 / melee.grito_desafio / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada (efeito de posture/stagger real = implementar em fable_70)
- **Resumo (1 frase):** Investida frontal estreita com o maior dano e dano de postura da árvore, projetada para quebrar a guarda de quem está bloqueando.
- **Descrição completa:** O finisher do sub-ramo de controle (avanço → grito → investida) e a ativa de tier mais alto da árvore. Lunge curto e preciso (arco estreito de 90°) que entrega o maior dano (16) e um **multiplicador de posture de 3x**, causando stagger muito maior em alvos que estão bloqueando/com guarda erguida. É a resposta a inimigos defensivos: enquanto outras skills batem na guarda, esta a estilhaça. O dano/lunge/knockback existem no executor; o **efeito de posture/stagger ainda não tem sistema de gameplay e deve ser implementado em fable_70**.
- **Mecânica (ATIVA):** Input/slot: tecla do slot. · Alvo: inimigo à frente (cone estreito). · Alcance/arco/raio: range `1.3m`, arco `90°` (o mais estreito — golpe preciso). · Nº de golpes: 1 golpe ao fim da investida. · Dano: `16` (Físico — o maior da árvore). · Status aplicado: nenhum status persistente; aplica **posture damage** com multiplicador. · Custo: `26` stamina (o mais alto da árvore). · Cooldown: `9s` (o mais longo da árvore). · Movimento: **lunge de `2m`**, respeita colisão. · Posture/knockback: **knockbackForce `3`** + **postureDamageMultiplier `3x`** (stagger muito maior se o alvo estiver bloqueando). · Telegraph/Animação/VFX: ombro/escudo à frente (postura de carga), windup mais visível que as outras (2–3 frames de antecipação por ser um golpe pesado), flash branco forte e estilhaço de "guarda quebrada" (cacos/spark) quando acerta um alvo bloqueando, com hitstop curto.
- **Q&A fechado:**
  1. O que acontece se não houver inimigo no alcance? Investe os `2m` e golpeia o vazio; cooldown longo e stamina alta gastos sem retorno — punição por errar.
  2. Trava o movimento? Sim — durante a investida o player está totalmente comprometido (lunge + golpe).
  3. Funciona contra boss/elite? Sim — `16` de dano físico. O posture/stagger contra boss segue as regras de poise do boss (bosses costumam ter poise alto e podem não stagger completamente) — **definição final em fable_70**.
  4. O que consome / falta de stamina? `26` stamina. Sem stamina: recusado (fable_70). Custo alto + cooldown `9s` exigem timing.
  5. Empilha consigo / outra melee? Sem buff; cooldown `9s`. O bônus de posture é por golpe (não acumula em buff). Combinável com grito (provoca/empurra) → investida (quebra guarda).
  6. Pode ser cancelada / interrompida? Comprometido na investida; stagger recebido antes do hitframe interrompe (fable_70). Não cancelável por input.
  7. Funciona fora de combate? A investida ocorre em qualquer cena; sem alvo bloqueando, o posture não tem efeito. Não interage com farm/town. **Dependência:** sistema de posture/stagger — **implementar em fable_70**.

---

## 3. Capstone

### Ritmo de Batalha (`melee_capstone_battle_rhythm`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Melee / T5 (capstone) / melee_dodge_training / 1 SP / Passiva (capstone XOR exclusivo)
- **Status no saneamento:** mantida (estrutura XOR já no catálogo; payoff condicional das variantes = implementar em fable_70)
- **Resumo (1 frase):** Capstone que reduz o cooldown das skills melee após acertar/matar, com uma escolha exclusiva entre duas variantes (Kanthor XOR Kaand).
- **Descrição completa:** O coroamento da árvore Melee. O efeito base recompensa a agressão: **cada hit/kill reduz o cooldown das ativas melee**, premiando quem mantém o ritmo de ataque (daí o nome). Sobre esse efeito base, o player escolhe **uma de duas variantes exclusivas** (`CapstoneVariants = { "kanthor", "kaand" }`). A escolha é **permanente até um respec na Fonte** — escolher uma **bloqueia a outra**. Isto cria duas identidades de fim de árvore distintas: um caminho de sustain condicional (Kanthor) e um caminho ofensivo puro (Kaand).
- **Mecânica (PASSIVA):** Efeito base: ao acertar ou matar com uma skill melee, reduz o cooldown corrente das ativas melee (redução por hit/kill — valor exato e se afeta todos os slots ou só o usado = **implementar em fable_70**, alinhado ao SKILL_NUMERIC_ADDENDUM). · Modificador: redução de cooldown melee gatilhada por evento de hit/kill. · Stack por rank? Não — capstone é nó único (1 ponto, sem ranks). · Condição de aplicação: requer hit/kill com skill melee; a variante escolhida adiciona seu efeito por cima. · Retroativo? Aplica-se a partir da compra; a variante escolhida vale até respec.
  - **Variante Kanthor — "Julgamento de Aço" (sustain condicional):** além da redução de cooldown base, **cura condicional**: ao matar um inimigo (kill) com skill melee, o player recupera uma pequena fração de HP; opcionalmente, hits em sequência rápida (mantendo o "ritmo") concedem um pequeno regen temporário. É o caminho do combatente sustentável que se mantém em pé batendo. Fantasia: a lâmina "julga" e devolve vitalidade ao portador justo. (Números de cura/threshold = **implementar em fable_70**.)
  - **Variante Kaand — "Fúria de Aço" (ofensivo puro):** em vez de cura, a redução de cooldown base é amplificada e/ou ganha um bônus de dano melee enquanto o player mantém o ritmo (encadeia hits sem deixar o cooldown subir). Nenhuma sustentação — tudo convertido em pressão ofensiva. Fantasia: a fúria acelera e endurece cada golpe. (Magnitudes do bônus de dano / cooldown extra = **implementar em fable_70**.)
- **Q&A fechado:**
  1. Empilha por rank? Não — é nó único de capstone, sem ranks; 1 SP.
  2. Cap? Não há ranks; o efeito base e a variante têm seus próprios limites/balance (fable_70).
  3. Conflita com passiva similar? A escolha **Kanthor XOR Kaand** é a única exclusividade: escolher uma trava a outra até respec na Fonte. Não conflita com outras passivas da árvore.
  4. Posso ter as duas variantes ao mesmo tempo? Não. É XOR — uma bloqueia a outra. Só respec libera a troca.
  5. Como troco de variante? Respec completo na Fonte (mesmo mecanismo do respec geral de skill tree). Não há toggle livre.
  6. A redução de cooldown afeta as lunge-strikes (avanço/dash/leap/investida)? Sim — todas são skills melee, então hits/kills com elas (e com elas mesmas) alimentam a redução de cooldown (regra final = fable_70).
  7. Funciona fora de combate? Não — o gatilho é hit/kill em combate. Sem inimigos (farm/town), não há redução de cooldown nem cura.
  8. A cura de Kanthor cura em farm/town? Não — depende de kill/hit em inimigo; é estritamente de combate.
  9. Retroativa? Vale a partir da compra/escolha; a variante persiste até respec.

---

## 4. Cortadas

> Documentadas para rastreabilidade. Não recebem entrada completa — saíram dos active slots por decisão de saneamento (seção 7).

- **`melee_guarded_block` ("Bloqueio Guardado") — REMOVIDA dos active slots.** O Bloqueio segue como **ability pura (tecla Shift)** e **não ocupa slot**. Motivo: Block já existia como ability de movimento/defesa e o `FinalHudGuardValidator` proíbe Block em slot; o nó de slot era duplicado. **Impacto na árvore:** a skill que tinha `guarded_block` como prereq — `melee_whirl_cut` — passa a ter prereq **`melee_guarded_stance`** (ver entrada de whirl_cut acima). Documentar a mudança de prereq no comentário do catálogo para não regredir.

---

## Apêndice — Nota de implementação consolidada (fable_70)

Itens deste documento que dependem de sistemas ainda inexistentes e devem ser fechados em **fable_70**:
- **Cobrança de stamina** das ativas (hoje `TODO_INTEGRATION_NOT_FINAL`; sem isso o balance é fictício).
- **Sistema de posture/stagger** real (consumidor do `postureDamageMultiplier` de `investida_quebra_guarda` e da regra de interrupção por stagger das ativas).
- **Taunt/aggro** real para `melee.grito_desafio` (status `status_taunt` proposto, duração `4s`; taunt parcial em boss).
- **Payoff condicional do capstone** (cura de Kanthor, amplificação ofensiva de Kaand, regra exata de redução de cooldown por hit/kill).
- **Knockback em boss** (resistência por massa) e **piso de custo do Dodge** (consumidor de `DodgeCostReduction`).
