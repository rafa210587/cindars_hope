# Bíblia de Skills — Árvore MAGIC (Cindar's Hope)

> **Status:** PROPOSTA (design, sem código). Documento de referência exaustivo da árvore MAGIC.
> Aguarda confirmação humana antes de virar spec de implementação. Não promove nada nem move specs.
> **Data:** 2026-06-23.
> **Fonte de verdade:** `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs` (`BuildMagicNodes`) +
> números reais de `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs`.
> Base de design: `docs/design/SKILL_CATALOG_DESIGN_REVIEW_v1.0.md`.

---

## Legenda

- **Árvore:** sempre `Magic`.
- **Tier (T1–T5):** profundidade na árvore. T5 = capstone.
- **Prereq:** nó obrigatório anterior (`—` = nó raiz/sem prereq).
- **Custo SP:** sempre `1 SP` por nó (passivas podem subir de rank com 1 SP/rank).
- **Tipo:** `Passiva` ou `Ativa (slot 1–4)`.
- **Status no saneamento:**
  - `já-implementada` — efeito real registrado e funcionando no `ActiveSkillExecutionController`.
  - `mantida` — passiva real já aplicada por modificador de stat.
  - `nova(implementar)` — efeito ainda é `FeedbackOnly` (placeholder); a implementar em `fable_70`.
- **Status ids reais no projeto:** `status_chill`, `status_poison`, `status_bleed`. **Propostos por este doc:** `status_burn`, `status_resist_ward`, `status_slow`.
- **Bandas de balance:** Custo mana 8–20 · CD rápido 2.5–3s · CD médio 6s · CD pesado 8s · CD buff/zona 12s · Buff 15–30s.

### Mapa da árvore (prereqs reais)

```
T1  magic_mana_well (—)
     ├─ magic_quick_channel
     │    └─ magic_ice_bind (T2) ─ magic_lightning_chain (T3) ─ magic_slowing_sigils (T4) ─ CAPSTONE (T5)
     └─ magic_arcane_edge
          ├─ magic_arcane_bolt_mastery (passiva)
          └─ magic_fire_spark (T1) ─┬─ magic_toxic_cloud (T3) ─ magic_elemental_ward (T2*)
                                     └─ magic.chama_breve (T2) ─ magic.rajada_gelida (T3)
```
\* `magic_elemental_ward` tem prereq `magic_toxic_cloud` (T3), mas é classificada como skill de tier baixo de efeito (T2) por ser buff defensivo. Mantemos o rótulo T2 do design review e o prereq real do catálogo.

---

# PASSIVAS

### Poço de Mana (`magic_mana_well`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Magic / T1 / — / 1 SP / Passiva
- **Status no saneamento:** mantida
- **Resumo (1 frase):** Aumenta a reserva máxima de mana do conjurador.
- **Descrição completa:** Nó raiz da árvore arcana. Abre o acesso a toda a linha de mana (canalização, gelo, raio) e a toda a linha de dano arcano (fio arcano, fogo). Cada rank acrescenta capacidade de mana, permitindo encadear mais spells antes de ficar sem recurso. É o investimento de entrada obrigatório para qualquer build mágica.
- **Mecânica (PASSIVA):**
  - **Efeito:** eleva o teto de mana (`MaxMana`).
  - **Modificador:** `MaxManaFlat` +10 (flat).
  - **Stack por rank?** Sim — +10 MaxMana por rank. Cap recomendado: 5 ranks (+50 MaxMana total).
  - **Condição:** nenhuma; sempre ativo após compra.
  - **Retroativo?** Sim — aplicado imediatamente ao pool de mana atual via recálculo de stats derivados; a mana atual não é reduzida.
- **Q&A fechado:**
  1. **Quantos ranks?** Até 5 (cap recomendado), 1 SP cada, +10 MaxMana por rank.
  2. **A mana atual sobe junto ao comprar?** O teto sobe; a mana atual não é preenchida automaticamente — regenera/é restaurada normalmente.
  3. **É obrigatória para o resto da árvore?** Sim. É prereq de `quick_channel` e `arcane_edge`; sem ela, nenhuma outra magic skill é comprável.
  4. **Empilha com itens/buffs de mana?** Sim, soma flat antes de multiplicadores externos.
  5. **Retroativo a spells já desbloqueadas?** É um stat de pool; não altera spells, apenas quantas você consegue lançar.

---

### Canalização Rápida (`magic_quick_channel`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Magic / T1 / `magic_mana_well` / 1 SP / Passiva
- **Status no saneamento:** mantida
- **Resumo (1 frase):** Aumenta a regeneração passiva de mana por segundo.
- **Descrição completa:** Sustenta builds de spam de spell, devolvendo mana ao longo do tempo. É o prereq da linha de controle elemental (gelo → raio → sigilos → capstone), então todo build que quer chegar ao capstone passa por aqui. Quanto mais ranks, menos tempo ocioso esperando mana entre combates.
- **Mecânica (PASSIVA):**
  - **Efeito:** acelera regen de mana.
  - **Modificador:** `ManaRegenFlat` +1/s (flat por segundo).
  - **Stack por rank?** Sim — +1/s por rank. Cap recomendado: 3 ranks (+3/s total).
  - **Condição:** regen ativo nas mesmas condições do regen base (não regenera durante consumo no mesmo frame; regenera andando e parado).
  - **Retroativo?** Sim — entra no cálculo de regen imediatamente.
- **Q&A fechado:**
  1. **Quantos ranks?** Até 3, +1/s cada, +3/s no total.
  2. **Regenera em combate?** Sim, o regen é contínuo; não há gate de "fora de combate".
  3. **Regenera andando?** Sim — é regen passivo, independente de movimento.
  4. **Empilha com o regen do capstone?** Sim — o capstone soma +1/s adicional sobre este.
  5. **É prereq de quê?** De `magic_ice_bind`, que abre toda a linha de controle até o capstone.

---

### Fio Arcano (`magic_arcane_edge`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Magic / T1 / `magic_mana_well` / 1 SP / Passiva
- **Status no saneamento:** mantida
- **Resumo (1 frase):** Aumenta o dano arcano/de ataque do conjurador.
- **Descrição completa:** A espinha ofensiva da árvore. Eleva o dano base que alimenta o ArcaneBolt e os spells de projétil. É prereq direto da `Fagulha Ígnea` (primeira spell ativa) e do `Domínio do Raio Arcano`. Builds de dano puro investem ranks aqui cedo.
- **Mecânica (PASSIVA):**
  - **Efeito:** aumenta dano arcano.
  - **Modificador:** `AttackFlat` +1 (flat). (Mapeado internamente como bônus de Attack que o canal arcano consome.)
  - **Stack por rank?** Sim — +1 por rank. Cap recomendado: 5 ranks (+5 dano).
  - **Condição:** aplica-se a ataques/projéteis que escalam com Attack.
  - **Retroativo?** Sim — afeta spells já desbloqueadas que usam o stat de dano.
- **Q&A fechado:**
  1. **Quantos ranks?** Até 5, +1 cada.
  2. **Afeta os spells elementais (fogo/gelo/raio)?** Afeta a componente que escala com Attack; o dano-base fixo do projétil (ex.: 12 Fire) não é substituído, mas recebe o bônus de stat.
  3. **Retroativo a spells já compradas?** Sim.
  4. **É o mesmo bônus que o `arcane_bolt_mastery`?** Ambos dão +1 Attack; eles somam (ver Q&A daquele nó).
  5. **É prereq de quê?** De `magic_fire_spark`, `magic_arcane_bolt_mastery` e (via fire_spark) de toda a sub-linha de fogo.

---

### Domínio do Raio Arcano (`magic_arcane_bolt_mastery`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Magic / T1 / `magic_arcane_edge` / 1 SP / Passiva
- **Status no saneamento:** mantida
- **Resumo (1 frase):** Aprimora o ArcaneBolt básico e adiciona dano arcano.
- **Descrição completa:** Upgrade passivo do ataque arcano básico (ArcaneBolt). Funciona como um segundo investimento ofensivo após o `Fio Arcano`, recompensando quem mantém o ArcaneBolt como ferramenta de dano sustentado. Não é uma ativa de slot — é melhoria permanente do ataque arcano padrão.
- **Mecânica (PASSIVA):**
  - **Efeito:** melhora o ArcaneBolt (`UpgradeSkillAction`) + dano arcano geral.
  - **Modificador:** `AttackFlat` +1 (flat).
  - **Stack por rank?** Sim — +1 por rank. Cap recomendado: 3 ranks (+3).
  - **Condição:** ArcaneBolt deve estar disponível ao player (ataque arcano básico).
  - **Retroativo?** Sim.
- **Q&A fechado:**
  1. **Quantos ranks?** Até 3, +1 cada.
  2. **Soma com `arcane_edge`?** Sim — os dois bônus de +1 Attack são aditivos (ex.: edge rank 3 + mastery rank 2 = +5 Attack).
  3. **Ocupa um slot ativo?** Não — é passiva; melhora o ArcaneBolt que já existe.
  4. **Retroativo?** Sim, aplica ao ArcaneBolt e aos cálculos de dano arcano na hora.
  5. **Precisa do ArcaneBolt equipado?** Não há slot a equipar; o ArcaneBolt é o ataque arcano base e recebe o upgrade automaticamente.

---

# ATIVAS

> Todas as ativas seguem o mesmo pipeline real do `ActiveSkillExecutionController`: **input numérico 1–4 → resolve slot equipado → valida nó comprado e equipável → resolve EffectId → resolve alvo (`SkillTargetResolver`) → executa → aplica cooldown do executor → publica `PlayerActionFeedbackEvent`.** Conjuração não tem trava de movimento no controller — o player pode disparar andando (ver Q&A de cada skill).
>
> **Nota global sobre custo de mana:** o `ActiveSkillExecutionController` ainda tem `TODO_INTEGRATION_NOT_FINAL` — a dedução de mana **não é cobrada hoje**. A banda de custo (8–20 mana) por skill abaixo é o **design alvo**; a cobrança será ligada a `ManaManager.TrySpend`. Comportamento alvo de falta de mana: a skill **não dispara**, **não entra em cooldown**, e publica `FailureReason` "Mana insuficiente" via HUD/feedback + SFX de recusa (pipeline `action-feedback-pipeline`). Nenhuma skill consome mana parcial.

---

### Fagulha Ígnea (`magic_fire_spark`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Magic / T1 / `magic_arcane_edge` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada (`combat.magic.fire_spark`)
- **Resumo (1 frase):** Projétil de fogo rápido e barato — a spell de abertura do conjurador.
- **Descrição completa:** A primeira magia ofensiva da árvore e a mais econômica. Dispara uma fagulha de fogo em linha reta na direção do alvo (ou do facing), com custo baixo de mana e cooldown curto, servindo como ferramenta de poke confiável. Não aplica status — é dano elemental puro de Fire.
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla numérica do slot equipado (1–4).
  - **Alvo:** projétil único.
  - **Alcance/raio:** range 7 m, speed 10.
  - **Nº projéteis/spread:** 1 projétil, sem spread.
  - **Pierce/chain:** nenhum (atinge o primeiro alvo e termina).
  - **Dano:** 12, tipo **Fire**.
  - **Status aplicado:** nenhum.
  - **Custo (mana):** 10 (banda).
  - **Cooldown:** 3 s (rápido).
  - **Movimento:** pode conjurar andando — sem trava de movimento.
  - **Telegraph/Animação/VFX:** pose curta de conjuração; faísca laranja 2D com trilha de partículas; impacto com flash de fogo pequeno.
- **Q&A fechado:**
  1. **Sem alvo, dispara no facing?** Sim — sem inimigo resolvido pelo `SkillTargetResolver`, o projétil sai na direção que o player encara.
  2. **Conjura em movimento?** Sim, sem penalidade.
  3. **Funciona em boss/elite?** Sim — dano elemental normal; sem imunidade.
  4. **Consome mana — e se faltar?** Alvo: 10 mana; faltando, não dispara, sem CD, feedback "Mana insuficiente".
  5. **Aplica status?** Não. É dano puro de Fire.
  6. **Atinge aliados/decoy?** Atinge o primeiro corpo hostil na trajetória; não fere o player. Decoy de `isca_improvisada` pode ser atingido se estiver na linha (intencional — projétil físico de trajetória).
  7. **Pierce?** Não — para no primeiro alvo.

---

### Chama Breve (`magic.chama_breve`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Magic / T2 / `magic_fire_spark` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada (`magic.chama_breve`)
- **Resumo (1 frase):** Jato curto de fogo, baratíssimo e de CD muito baixo, com chance de Burn.
- **Descrição completa:** Evolução de pressão da `Fagulha Ígnea`: troca alcance por economia extrema e a chance de aplicar queimadura (`status_burn`). É a spell de DPS sustentado mais spammável da árvore — o custo de 8 mana e o CD de 2.5 s a tornam o "ataque básico mágico" de quem investiu na linha de fogo.
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla do slot (1–4).
  - **Alvo:** projétil único de curto alcance.
  - **Alcance/raio:** range 6 m, speed 10.
  - **Nº projéteis/spread:** 1 projétil, sem spread.
  - **Pierce/chain:** nenhum.
  - **Dano:** 8, tipo **Fire**.
  - **Status aplicado:** `status_burn` (proposto) — **chance 35%** por acerto · dano **2/tick** · tick a cada 1 s · **duração 3 s** (3 ticks = 6 dano). Não empilha em stacks: ao reaplicar, **refresca a duração** para 3 s (sem somar dano de ticks).
  - **Custo (mana):** 8 (piso da banda).
  - **Cooldown:** 2.5 s (rápido).
  - **Movimento:** pode conjurar andando.
  - **Telegraph/Animação/VFX:** flash rápido de chama na mão; projétil pequeno e veloz; ao aplicar Burn, ícone de chama sobre o alvo + partículas de brasa por 3 s.
- **Q&A fechado:**
  1. **Sem alvo, dispara no facing?** Sim.
  2. **Conjura em movimento?** Sim.
  3. **Funciona em boss/elite?** Sim; o Burn também se aplica (sujeito a resistências de fogo do alvo).
  4. **Consome mana — e se faltar?** Alvo: 8 mana; faltando, não dispara, sem CD, feedback de recusa.
  5. **Burn empilha ou refresca? duração?** **Refresca** — nunca soma stacks; cada acerto que rola a chance reinicia os 3 s. Dano 2/tick por 1 s, 3 ticks.
  6. **A chance de Burn é por acerto?** Sim — 35% por projétil que conecta; um disparo, uma rolagem.
  7. **Atinge aliados/decoy?** Só hostis na trajetória; não fere o player; pode acertar decoy na linha.
  8. **Pierce?** Não.

---

### Laço de Gelo (`magic_ice_bind`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Magic / T2 / `magic_quick_channel` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada (`combat.magic.ice_bind`)
- **Resumo (1 frase):** Projétil de gelo que causa dano leve e aplica Chill (lentidão) ao alvo.
- **Descrição completa:** A ferramenta de controle single-target da árvore. O dano é secundário; o valor está no `status_chill` que reduz a velocidade do alvo, abrindo espaço para kitar ou encadear outras spells. Abre a linha de controle (raio → sigilos → capstone).
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla do slot (1–4).
  - **Alvo:** projétil único.
  - **Alcance/raio:** range 7 m, speed 9.
  - **Nº projéteis/spread:** 1 projétil, sem spread.
  - **Pierce/chain:** nenhum.
  - **Dano:** 10, tipo **Ice**.
  - **Status aplicado:** `status_chill` — **Slow 35%** (reduz move speed do alvo em 35%) · **duração 3 s** · não empilha em stacks: reaplicar **refresca** os 3 s (a magnitude de slow não soma).
  - **Custo (mana):** 14 (banda).
  - **Cooldown:** 6 s (médio).
  - **Movimento:** pode conjurar andando.
  - **Telegraph/Animação/VFX:** cristal azul de conjuração; projétil de gelo com trilha gélida; ao aplicar Chill, overlay azulado/cristais no alvo e partículas de frio por 3 s.
- **Q&A fechado:**
  1. **Sem alvo, dispara no facing?** Sim.
  2. **Conjura em movimento?** Sim.
  3. **Funciona em boss/elite?** Sim. O dano e o Chill aplicam; bosses podem ter resistência reduzida ao slow (decisão de balance por boss — padrão: Chill aplica, magnitude pode ser atenuada por tag de boss).
  4. **Consome mana — e se faltar?** Alvo: 14 mana; faltando, não dispara, sem CD, feedback.
  5. **Chill empilha ou refresca? duração?** **Refresca** — Slow 35% por 3 s; reaplicar reinicia a duração, não soma magnitude.
  6. **Chill stacka com a Rajada Gélida?** Ambos usam `status_chill` — é o mesmo status; o segundo acerto **refresca** a duração (não soma para 70%).
  7. **Atinge aliados/decoy?** Só hostis na trajetória; não afeta o player; decoy na linha pode ser atingido.
  8. **Pierce?** Não.

---

### Rajada Gélida (`magic.rajada_gelida`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Magic / T3 / `magic.chama_breve` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada (`magic.rajada_gelida`)
- **Resumo (1 frase):** Leque curto de 3 projéteis de gelo que aplica Chill em quem for atingido.
- **Descrição completa:** Versão em área-cone do `Laço de Gelo`: dispara três fragmentos de gelo num spread de 30°, cobrindo um arco frontal próximo. Cada fragmento aplica Chill, tornando-a a ferramenta de controle contra dois ou três alvos agrupados. Ramo de fogo→gelo (vem da `Chama Breve`), oferecendo controle a builds que começaram ofensivas.
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla do slot (1–4).
  - **Alvo:** leque de projéteis (área frontal por spread).
  - **Alcance/raio:** range 6 m, speed 9.
  - **Nº projéteis/spread:** **3 projéteis**, spread **30°**.
  - **Pierce/chain:** nenhum (cada projétil para no primeiro alvo).
  - **Dano:** 6 por projétil (tipo **Ice**) — até 18 se os 3 acertarem o mesmo alvo de perto.
  - **Status aplicado:** `status_chill` — Slow 35% · duração 3 s · refresca (idêntico ao do `Laço de Gelo`).
  - **Custo (mana):** 16 (banda).
  - **Cooldown:** 6 s (médio).
  - **Movimento:** pode conjurar andando.
  - **Telegraph/Animação/VFX:** gesto amplo de braço; três estilhaços azuis em leque; névoa fria no arco frontal; alvos atingidos recebem overlay de Chill.
- **Q&A fechado:**
  1. **Sem alvo, dispara no facing?** Sim — o leque sai centrado no facing.
  2. **Conjura em movimento?** Sim.
  3. **Funciona em boss/elite?** Sim; vários estilhaços podem atingir o boss (somando dano), mas o Chill **refresca** em vez de empilhar.
  4. **Consome mana — e se faltar?** Alvo: 16 mana; faltando, não dispara, sem CD, feedback.
  5. **Chill empilha entre os 3 projéteis?** Não — é o mesmo `status_chill`; múltiplos acertos no mesmo alvo refrescam a duração, não somam slow.
  6. **Quantos alvos atinge?** Até 3 alvos distintos (1 por projétil) se espalhados pelo leque; ou concentra dano num só.
  7. **Atinge aliados/decoy?** Só hostis; cada projétil para no primeiro corpo hostil; decoy na trajetória pode ser atingido.

---

### Nuvem Tóxica (`magic_toxic_cloud`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Magic / T3 / `magic_fire_spark` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada (`combat.magic.toxic_cloud`)
- **Resumo (1 frase):** Leque de 3 baforadas tóxicas que envenena os alvos atingidos.
- **Descrição completa:** Spell de dano-sobre-tempo em área. Dispara três jatos tóxicos num leque amplo (40°) e aplica `status_poison` a quem for atingido, ferindo continuamente ao longo da duração. É a ferramenta de pressão contra grupos e de dano sustentado contra alvos resistentes a impacto direto.
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla do slot (1–4).
  - **Alvo:** leque de projéteis (área frontal por spread).
  - **Alcance/raio:** range 6 m, speed 7.
  - **Nº projéteis/spread:** **3 projéteis**, spread **40°**.
  - **Pierce/chain:** nenhum.
  - **Dano:** 8 por projétil de impacto (tipo **Toxic**) — até 24 se os 3 acertarem.
  - **Status aplicado:** `status_poison` — dano **3/tick** · tick a cada 1 s · **duração 4 s** (4 ticks = 12 dano). Reaplicar **refresca** a duração para 4 s (sem somar stacks).
  - **Custo (mana):** 18 (banda alta).
  - **Cooldown:** 8 s (pesado).
  - **Movimento:** pode conjurar andando.
  - **Telegraph/Animação/VFX:** gesto de dispersão; três baforadas verdes em leque com partículas tóxicas persistentes; alvos envenenados recebem ícone de gota verde + bolhas por 4 s.
- **Q&A fechado:**
  1. **Sem alvo, dispara no facing?** Sim.
  2. **Conjura em movimento?** Sim.
  3. **Funciona em boss/elite?** Sim; o Poison aplica e tickeia (sujeito a resistência tóxica do alvo).
  4. **Consome mana — e se faltar?** Alvo: 18 mana; faltando, não dispara, sem CD, feedback.
  5. **Poison empilha ou refresca? dano/tick e duração?** **Refresca** — 3/tick por 4 s; múltiplos acertos reiniciam os 4 s, não somam ticks.
  6. **Quantos alvos? raio?** Até 3 alvos (1 por baforada) no arco de 40°/6 m; concentra em 1 se de perto.
  7. **Atinge aliados/decoy?** Só hostis na trajetória; não fere o player; decoy pode ser atingido.

---

### Corrente Relâmpago (`magic_lightning_chain`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Magic / T3 / `magic_ice_bind` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada (`combat.magic.lightning_chain`)
- **Resumo (1 frase):** Raio veloz que salta entre até 4 inimigos próximos.
- **Descrição completa:** A spell de limpeza de grupo da árvore. Atinge um alvo inicial e **encadeia** para inimigos próximos, distribuindo dano de Lightning por até 4 corpos. Vem da linha de controle (gelo), recompensando builds que usam Chill para agrupar/segurar antes de detonar a corrente. É a spell mais cara e de maior alcance de projétil (9 m, speed 16).
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla do slot (1–4).
  - **Alvo:** projétil que encadeia (chain).
  - **Alcance/raio:** range 9 m, speed 16. **Alcance do salto:** o raio salta para um novo alvo dentro de **4 m** do alvo anterior.
  - **Nº projéteis/spread:** 1 projétil inicial.
  - **Pierce/chain:** **salta até 4 alvos** no total (alvo inicial + 3 saltos). Cada salto procura o hostil não-atingido mais próximo dentro de 4 m; não repete alvos.
  - **Dano:** 12 por alvo, tipo **Lightning** (mesmo valor em cada salto — sem falloff, por design de payoff de tier).
  - **Status aplicado:** nenhum.
  - **Custo (mana):** 20 (teto da banda).
  - **Cooldown:** 8 s (pesado).
  - **Movimento:** pode conjurar andando.
  - **Telegraph/Animação/VFX:** estouro elétrico na conjuração; arco branco-azulado ziguezagueando entre alvos; flash em cada salto.
- **Q&A fechado:**
  1. **Sem alvo, dispara no facing?** Sim — o projétil sai no facing; o primeiro hostil que tocar vira o início da corrente.
  2. **Conjura em movimento?** Sim.
  3. **Funciona em boss/elite?** Sim. Contra um único boss sem adds, atinge só ele (1 alvo, sem saltos) — dano de single hit.
  4. **Consome mana — e se faltar?** Alvo: 20 mana; faltando, não dispara, sem CD, feedback.
  5. **Aplica status?** Não — dano puro de Lightning.
  6. **Chain: quantos alvos, alcance do salto, atinge aliados/decoy?** Até **4 alvos** (inicial + 3 saltos), salto de **4 m**, só hostis não-atingidos. O decoy de `isca_improvisada` **conta como alvo hostil** e pode receber um salto (intencional — decoy "puxa" o raio).
  7. **Salta de volta para um alvo já atingido?** Não — cada alvo é atingido no máximo uma vez por conjuração.

---

### Guarda Elemental (`magic_elemental_ward`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Magic / T2 / `magic_toxic_cloud` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** nova(implementar) — hoje `FeedbackOnly` (`combat.magic.elemental_ward`); implementar em **fable_70 (executor de Buff)**.
- **Resumo (1 frase):** Buff defensivo no conjurador que concede resistência elemental por alguns segundos.
- **Descrição completa:** O único botão puramente defensivo da árvore. Conjurada **no próprio caster** (self-target), ergue uma barreira que reduz o dano elemental recebido (Fire/Ice/Toxic/Lightning) por uma janela curta. Pensada para ser ativada **antes** de entrar em salas perigosas ou ao prever um burst elemental. Aplica o status proposto `status_resist_ward`.
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla do slot (1–4).
  - **Alvo:** **self** (o caster). Não precisa de alvo no mundo.
  - **Alcance/raio:** n/a (self-target).
  - **Nº projéteis/spread:** n/a.
  - **Pierce/chain:** n/a.
  - **Dano:** nenhum (skill defensiva).
  - **Status aplicado:** `status_resist_ward` (proposto) — concede **+X = 8 de resistência elemental** em **todas** as quatro escolas (Fire/Ice/Toxic/Lightning) simultaneamente · **duração N = 20 s** (dentro da banda de buff 15–30s). Não empilha: reconjurar **refresca** a duração para 20 s (a magnitude não soma).
  - **Custo (mana):** 16 (banda).
  - **Cooldown:** 12 s (CD de buff/zona; impede uptime 100% — janela de 8 s descoberto entre wards).
  - **Movimento:** pode conjurar andando.
  - **Telegraph/Animação/VFX:** sprite-overlay de aura/escudo translúcido ao redor do player com tonalidade prismática (cobre os 4 elementos), pulsando por 20 s; flash de conjuração defensiva ao ativar.
- **Q&A fechado:**
  1. **Sem alvo, dispara no facing?** Não se aplica — é self-target; sempre afeta o caster, nunca precisa de alvo.
  2. **Conjura em movimento?** Sim.
  3. **Funciona/protege contra boss/elite?** Sim — reduz dano elemental de qualquer fonte, inclusive ataques elementais de boss.
  4. **Consome mana — e se faltar?** Alvo: 16 mana; faltando, não dispara, sem CD, feedback "Mana insuficiente".
  5. **A ward empilha com outra ward (duração/magnitude)?** Não — reconjurar **refresca** para 20 s; a magnitude permanece +8 (não vira +16).
  6. **Stack com resistências passivas da árvore Survival (toxic/cold/heat)?** Sim — o buff temporário **soma** flat com as resistências passivas permanentes do player.
  7. **Raio/afeta aliados?** Não — afeta apenas o caster; é buff pessoal, sem componente de área.
  8. **Reduz dano físico?** Não — só dano elemental (Fire/Ice/Toxic/Lightning).

---

### Sigilos Lentificantes (`magic_slowing_sigils`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Magic / T4 / `magic_lightning_chain` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** nova(implementar) — hoje `FeedbackOnly` (`combat.magic.slowing_sigils`); implementar em **fable_70 (executor de Spawn/Zona)**.
- **Resumo (1 frase):** Marca uma zona no chão que lentifica inimigos que entram nela.
- **Descrição completa:** A ferramenta de controle de área da árvore, distinta do Chill single-target. Cria um **sigilo no chão** que persiste por alguns segundos; qualquer inimigo dentro do raio sofre Slow enquanto permanecer na zona. É controle posicional — usada para frear hordas em corredores, segurar um boss numa região, ou cobrir uma retirada. Prereq direto do capstone.
- **Mecânica (ATIVA):**
  - **Input/slot:** tecla do slot (1–4).
  - **Alvo:** **zona/área no chão** (spawn de entidade de zona). Posicionada na posição do player (ou à frente, no facing) ao conjurar.
  - **Alcance/raio:** **raio da zona = 2.5 m**. Colocação na posição do caster.
  - **Nº projéteis/spread:** n/a (não é projétil; é zona spawnada).
  - **Pierce/chain:** n/a.
  - **Dano:** nenhum (controle puro).
  - **Status aplicado:** `status_slow` (proposto, área) — **Slow 40%** aplicado a todo inimigo dentro do raio · **enquanto estiver na zona**; ao sair, o slow expira em **1 s** (decaimento curto). A zona dura **N = 6 s**.
  - **Custo (mana):** 18 (banda alta).
  - **Cooldown:** 12 s (CD de zona — maior que a duração da zona, sem uptime infinito).
  - **Movimento:** pode conjurar andando.
  - **Telegraph/Animação/VFX:** runas/sigilo brilhante desenhado no chão (pixel 2D), pulsando; partículas ascendentes no raio; inimigos dentro recebem overlay de lentidão (rastro arrastado).
- **Q&A fechado:**
  1. **Sem alvo, onde nasce a zona?** Na posição do caster ao conjurar (não requer alvo hostil). Variante de design: pode nascer alguns passos à frente no facing — padrão proposto: **sob o player**.
  2. **Conjura em movimento?** Sim; a zona fica fixa onde nasceu (não segue o player).
  3. **Funciona em boss/elite?** Sim — o Slow aplica a bosses dentro do raio (magnitude pode ser atenuada por tag de boss, igual ao Chill).
  4. **Consome mana — e se faltar?** Alvo: 18 mana; faltando, não cria a zona, sem CD, feedback.
  5. **O Slow é por N s ou enquanto dentro?** **Enquanto dentro** da zona; sai da zona → decai em 1 s. A zona em si dura 6 s.
  6. **AoE: quantos alvos, raio, atinge aliados/decoy?** Sem limite de alvos dentro do raio de 2.5 m; afeta **todos os hostis** na zona (inclui decoy de `isca_improvisada`); **não afeta o player**.
  7. **Stack com outra zona/sigilo ou com Chill?** Duas zonas sobrepostas **não somam** magnitude (cap em 40% pelo mesmo `status_slow`). Com `status_chill` (de gelo) são status distintos — o de maior magnitude prevalece (não somam para >40–35%; aplica-se o maior).
  8. **A zona dá dano?** Não — é controle puro.

---

# CAPSTONE

### Confluência Elemental (`magic_capstone_elemental_confluence`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Magic / T5 (capstone) / `magic_slowing_sigils` / 1 SP / Passiva (com escolha XOR exclusiva)
- **Status no saneamento:** mantida (base passiva já-implementada; a escolha XOR é design proposto)
- **Resumo (1 frase):** Capstone arcano que dá um bônus base de dano elemental + regen e força uma escolha exclusiva entre uma Semente de suporte e uma Semente ofensiva.
- **Descrição completa:** O nó terminal da árvore Magic, comprável apenas após os 8 nós do caminho até `slowing_sigils`. Concede um bônus base permanente e, mais importante, **uma escolha de identidade exclusiva (XOR)** entre duas "Sementes": uma vocacionada a **suporte/sustain** (Anya) e outra a **dano puro** (Senya). Escolher uma **bloqueia a outra** até um respec da árvore. É o que separa um "caster utilitário" de um "caster de burst".
- **Mecânica (PASSIVA):**
  - **Efeito base (sempre, independente da escolha):** bônus permanente de dano elemental + regen de mana.
  - **Modificador base:** `AttackFlat` +1 (dano elemental) e `ManaRegenFlat` +1/s.
  - **Stack por rank?** Não — capstone é nó único (1 SP, sem ranks).
  - **Condição:** exige 8 nós comprados na árvore (`reqNodes: 8`) e o prereq `magic_slowing_sigils`.
  - **Retroativo?** Sim — bônus base e a Semente escolhida aplicam imediatamente a todas as spells.
  - **Escolha XOR exclusiva (uma OU outra, nunca ambas):**
    - **Semente de Anya — Suporte/Sustain:** **−50% no custo de MP de todas as spells** (ex.: Corrente Relâmpago cai de 20 → 10 mana) **e +35% na potência de cura** recebida/gerada pelo player (afeta self-restores como `survival.kit_emergencia` e qualquer cura). Pensada para builds de longa permanência na caverna, spam sustentado e suporte.
    - **Semente de Senya — Ofensivo:** **+35% de dano mágico** em todas as spells elementais (Fire/Ice/Toxic/Lightning; ex.: Fagulha Ígnea 12 → ~16, Corrente Relâmpago 12/alvo → ~16/alvo). Sem desconto de mana e sem bônus de cura. Pensada para builds de burst e clear rápido.
  - **Bloqueio/respec:** ao comprar o capstone, o player escolhe **uma** Semente; a outra fica **bloqueada e indisponível** até um respec da árvore Magic (que devolve os SP e zera a escolha, permitindo reescolher).
- **Q&A fechado:**
  1. **Posso ter as duas Sementes?** Não — é XOR estrito; escolher uma bloqueia a outra até respec.
  2. **Como troco de Semente?** Apenas via respec da árvore Magic (devolve SP, zera a escolha). Não há toggle livre em jogo.
  3. **O bônus base (+1 dano elemental, +1 regen) depende da escolha?** Não — o bônus base é sempre concedido; a Semente é um efeito adicional por cima.
  4. **A Semente de Anya reduz custo de quais spells?** De **todas** as spells da árvore (qualquer ativa que consuma mana), −50% MP. O +35% de cura afeta self-restores e curas recebidas.
  5. **A Semente de Senya afeta o dano de status (Burn/Poison)?** Afeta o dano direto elemental das spells; o tick de status segue a regra do status (decisão de balance: o +35% **não** multiplica o tick de DoT por padrão, para não dobrar o valor — só o impacto direto).
  6. **Tem rank?** Não — capstone é nó único, sem ranks.
  7. **Retroativo às spells já equipadas?** Sim — aplica na hora a todas as spells e curas.
  8. **Conta como uma das 4 ativas de slot?** Não — é passiva; não ocupa slot. A escolha XOR é um modo permanente da build.

---

## Apêndice — Resumo de números (referência rápida)

| Skill | Tipo | T | Dano | Tipo elem. | Nº proj/spread | Pierce/chain | Status | Mana | CD | Status saneamento |
|---|---|---|---|---|---|---|---|---|---|---|
| `magic_mana_well` | Passiva | 1 | — | — | — | — | — | — | — | mantida |
| `magic_quick_channel` | Passiva | 1 | — | — | — | — | — | — | — | mantida |
| `magic_arcane_edge` | Passiva | 1 | — | — | — | — | — | — | — | mantida |
| `magic_arcane_bolt_mastery` | Passiva | 1 | — | — | — | — | — | — | — | mantida |
| `magic_fire_spark` | Ativa | 1 | 12 | Fire | 1 / — | — | — | 10 | 3s | já-implementada |
| `magic.chama_breve` | Ativa | 2 | 8 | Fire | 1 / — | — | burn 35%, 2/tick, 3s | 8 | 2.5s | já-implementada |
| `magic_ice_bind` | Ativa | 2 | 10 | Ice | 1 / — | — | chill 35% slow, 3s | 14 | 6s | já-implementada |
| `magic.rajada_gelida` | Ativa | 3 | 6×3 | Ice | 3 / 30° | — | chill 35% slow, 3s | 16 | 6s | já-implementada |
| `magic_toxic_cloud` | Ativa | 3 | 8×3 | Toxic | 3 / 40° | — | poison 3/tick, 4s | 18 | 8s | já-implementada |
| `magic_lightning_chain` | Ativa | 3 | 12/alvo | Lightning | 1 / — | chain 4 alvos, salto 4m | — | 20 | 8s | já-implementada |
| `magic_elemental_ward` | Ativa | 2 | — | — | self | — | resist_ward +8, 20s | 16 | 12s | nova(implementar) — fable_70 |
| `magic_slowing_sigils` | Ativa | 4 | — | — | zona r=2.5m | — | slow 40% em zona, 6s | 18 | 12s | nova(implementar) — fable_70 |
| `magic_capstone_elemental_confluence` | Passiva XOR | 5 | +1 elem | — | — | — | Anya: −50% MP/+35% cura · Senya: +35% dano | — | — | mantida + XOR proposto |

> **Status ids:** reais = `status_chill`, `status_poison`. Propostos por este doc = `status_burn`, `status_resist_ward`, `status_slow`.
> **A implementar (fable_70):** `magic_elemental_ward` (executor de Buff) e `magic_slowing_sigils` (executor de Spawn/Zona). Demais ativas já têm executor real registrado no `ActiveSkillExecutionController`.
> **Pré-requisito transversal:** ligar a cobrança de mana (`ManaManager.TrySpend`) — hoje `TODO_INTEGRATION_NOT_FINAL`; sem isso, os custos acima são fictícios.
