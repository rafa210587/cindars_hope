# Bíblia de Skills — Árvore RANGED (Cindar's Hope)

> **Status:** PROPOSTA (design, sem código). Contrato de design para alimentar a implementação.
> Não promove specs nem aprova cortes. Aguarda confirmação humana antes de virar spec.
> **Data:** 2026-06-23.
> **Escopo:** Apenas a árvore **Ranged** (arco). As demais árvores têm suas próprias bíblias.
> **Fontes de verdade:** `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs` (`BuildRangedNodes`),
> `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs` (números reais de projétil),
> `docs/design/SKILL_CATALOG_DESIGN_REVIEW_v1.0.md` (vereditos de saneamento).

---

## Legenda do template

Cada skill segue o mesmo bloco, sem exceção:

- **Cabeçalho:** `### <Nome> (`<id>`)`.
- **Linha de metadados:** Árvore / Tier / Prereq / Custo SP / Tipo (Passiva ou Ativa com slot).
- **Status no saneamento:** `mantida` (já no roster, sem mudança) · `já-implementada` (executor real existe) · `nova(implementar)` (precisa de código).
- **Resumo (1 frase).**
- **Descrição completa** — parágrafo sem ambiguidade.
- **Mecânica (ATIVA)** ou **Mecânica (PASSIVA)** — todos os campos preenchidos com valor concreto.
- **Q&A fechado** — 5–10 perguntas reais respondidas em definitivo.

### Convenções de balance da árvore (bandas)

| Parâmetro | Banda Ranged |
|---|---|
| Custo de stamina (ativas) | 16–24 |
| Cooldown rápido | 2–3 s |
| Cooldown médio | 6 s |
| Cooldown pesado | 7–8 s |
| Status de sangramento | `status_bleed` (id real) |
| Recurso consumido | **Stamina** (todas as ativas ranged são físicas) |

### Invariantes que valem para TODA ativa ranged

Estes valem para todas as ativas abaixo; o Q&A de cada skill apenas confirma a aplicação local.

1. **Sem alvo no alcance → dispara mesmo assim, na direção do *facing* atual do player.** A projétil viaja até o `range` e some; não há "trava de alvo" — é tiro direcional 2D.
2. **Pode atirar em movimento** (exceto onde a própria mecânica disser o contrário — só `charged_shot` tem nuance de carga, ver lá).
3. **Funciona em boss** (boss não é imune a dano de projétil). Efeitos de *status* podem ter duração reduzida em boss — declarado por skill.
4. **Custo de stamina é cobrado ANTES de executar.** Se a stamina for insuficiente, a skill **não dispara**, **não entra em cooldown**, e publica `PlayerActionFeedbackEvent` com `FailureReason` "Stamina insuficiente" (toast + SFX de recusa). Ver nota de pré-requisito abaixo.
5. **Requer arco equipado.** Sem arco (ex.: arma melee equipada), a skill **não dispara**, **não consome stamina**, **não entra em cooldown**, e publica `FailureReason` "Requer arco equipado". As ativas ranged não "viram" ataque melee.
6. **Pierce/projétil para em parede.** Qualquer projétil colide com geometria sólida (parede/obstáculo) e é destruído ali, mesmo que ainda tivesse `pierce` ou `range` sobrando.

> **Pré-requisito de implementação (vale para toda a árvore):** o `ActiveSkillExecutionController` ainda tem o enforcement de custo marcado como `TODO_INTEGRATION_NOT_FINAL`. Até a cobrança de stamina via `StaminaManager.TrySpend` ser ligada, o balance de toda a árvore é fictício (skills viram spam grátis). **Ligar a cobrança de custo é pré-requisito de qualquer número de balance abaixo ser real.**

---

# PASSIVAS

### Mão Firme (`ranged_steady_hand`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Ranged / T1 / — / 1 SP / Passiva
- **Status no saneamento:** mantida (já-implementada)
- **Resumo (1 frase):** Aumenta o dano base de arco em +1 por rank.
- **Descrição completa:** Nó raiz da árvore ranged e a porta de entrada para todo o resto. Cada rank soma +1 ao `BowDamage` do player, um bônus pequeno mas que escala com ranks e serve de sink de longo prazo. É a base de qualquer build de arco; sem ele, nenhum outro nó ranged é comprável.
- **Mecânica (PASSIVA):** Efeito exato: soma `BowDamageFlat` ao dano de todo ataque com arco · Modificador: `BowDamageFlat`, valor **+1.0 por rank** · Stack por rank? **Sim**, +1/rank, **cap 5 ranks (+5 total)** · Condição: arco equipado (o bônus só se aplica a dano de arco) · Retroativo? **Sim** — aplica-se a disparos básicos E a ativas ranged que usam dano físico de projétil.

**Q&A fechado:**
1. Empilha por rank? **Sim**, +1 por rank, até 5 ranks (+5 total).
2. Há cap? **Sim**, 5 ranks.
3. Precisa de arco equipado para o bônus valer? **Sim** — só afeta dano de arco; com arma melee o bônus fica inerte (mas o nó continua comprado).
4. É retroativo às ativas ranged? **Sim**, afeta `charged_shot`, `line_piercer`, `multishot_fan`, `bleeding_arrow` (que usam dano de projétil físico).
5. Afeta dano de status (bleed)? **Não** — só o dano de impacto; o tick de bleed é fixo pelo status.
6. É pré-requisito de quê? De `ranged_long_sight` e `ranged_quick_nock` (ambos T1).

---

### Mira Longa (`ranged_long_sight`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Ranged / T1 / `ranged_steady_hand` / 1 SP / Passiva
- **Status no saneamento:** mantida (já-implementada)
- **Resumo (1 frase):** Aumenta o alcance de arco em +0.5 por rank.
- **Descrição completa:** Estende o `BowRange`, permitindo acertar inimigos mais distantes antes que o projétil expire. É o nó que viabiliza o estilo "kiter" (atirar de longe e recuar). Prereq direto de `ranged_charged_shot`.
- **Mecânica (PASSIVA):** Efeito exato: soma metros ao alcance máximo dos disparos de arco · Modificador: `BowRangeFlat`, valor **+0.5 m por rank** · Stack por rank? **Sim**, +0.5/rank, **cap 5 ranks (+2.5 m total)** · Condição: arco equipado · Retroativo? **Sim** — afeta o `range` efetivo dos básicos e das ativas ranged.

**Q&A fechado:**
1. Empilha por rank? **Sim**, +0.5 m por rank, cap 5 ranks (+2.5 m).
2. Precisa de arco? **Sim** — irrelevante com arma melee.
3. Afeta o range das ativas (ex.: `line_piercer` 10 m)? **Sim**, o bônus soma ao range base da ativa.
4. Afeta a velocidade do projétil? **Não** — só o alcance (distância). Velocidade vem de `ranged_projectile_tuning`.
5. Aumentar o range deixa o projétil mais lento por viajar mais longe? **Não** — velocidade é independente; o projétil só vive mais tempo até cobrir a distância maior.
6. É retroativo a disparos já em voo? **Não** — aplica-se a novos disparos a partir do momento da compra.

---

### Encaixe Rápido (`ranged_quick_nock`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Ranged / T1 / `ranged_steady_hand` / 1 SP / Passiva
- **Status no saneamento:** mantida (já-implementada)
- **Resumo (1 frase):** Aumenta a velocidade de ataque com arco em +0.1 por rank.
- **Descrição completa:** Reduz o intervalo entre disparos de arco via bônus de `AttackSpeed`. Na prática, encaixa flechas mais rápido e melhora o cooldown efetivo de disparo básico. Prereq de `ranged_line_piercer`. (No catálogo a descrição interna diz "melhora cooldown de arco", que é o efeito sentido do bônus de attack speed.)
- **Mecânica (PASSIVA):** Efeito exato: aplica bônus multiplicativo de cadência de ataque com arco · Modificador: `AttackSpeedBonus`, valor **+0.1 por rank** · Stack por rank? **Sim**, +0.1/rank, **cap 5 ranks (+0.5 total)** · Condição: arco equipado · Retroativo? **Sim**.

**Q&A fechado:**
1. Empilha por rank? **Sim**, +0.1 por rank, cap 5 ranks (+0.5).
2. Precisa de arco? **Sim** — o bônus é específico de arco.
3. Reduz o cooldown das ATIVAS ranged? **Não** — o cooldown das ativas (6–8 s) é fixo pela skill; `AttackSpeed` afeta a cadência do disparo **básico** de arco.
4. Afeta dano? **Não** — só cadência.
5. Há cap? **Sim**, 5 ranks.
6. É retroativo? **Sim**, aplica imediatamente ao equipamento atual.

---

### Passos de Kiting (`ranged_kiting_steps`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Ranged / T2 / `ranged_multishot_fan` / 1 SP / Passiva
- **Status no saneamento:** mantida (já-implementada)
- **Resumo (1 frase):** Concede um pequeno bônus de velocidade de movimento por uma janela curta após cada disparo.
- **Descrição completa:** Recompensa o estilo hit-and-run: imediatamente após disparar (básico ou ativa ranged), o player ganha `+0.05 MoveSpeed` por uma janela curta, facilitando reposicionar e manter distância. **Definição de duração do bônus:** o bônus dura **1.5 s** após cada disparo e **reseta (refresh) a cada novo disparo** — atirar de novo dentro da janela reinicia os 1.5 s, mantendo o buff ativo enquanto o player continuar disparando. O bônus **não empilha em magnitude** (continua +0.05 por rank, não +0.10 ao disparar duas vezes).
- **Mecânica (PASSIVA):** Efeito exato: pós-disparo, ativa um buff temporário de move speed por 1.5 s · Modificador: `MoveSpeedBonus`, valor **+0.05 por rank** · Stack por rank? **Sim** em magnitude (+0.05/rank, **cap 5 ranks = +0.25**); **não** empilha por número de disparos (refresh de duração, não soma) · Condição: ocorre apenas após um disparo com arco · Retroativo? **Sim**.

**Q&A fechado:**
1. Quanto dura o bônus? **1.5 s** após cada disparo.
2. Disparar de novo soma ou reinicia? **Reinicia (refresh)** a janela de 1.5 s; não soma magnitude.
3. Empilha por rank? **Sim**, +0.05/rank em magnitude, cap 5 ranks (+0.25), mas a duração não muda por rank (sempre 1.5 s).
4. Precisa de arco? **Sim** — só dispara o buff após um disparo de arco; ataque melee não ativa.
5. Vale para disparos de ativas ou só básico? **Ambos** — qualquer disparo de arco (básico ou ativa ranged) reinicia a janela.
6. O bônus persiste se eu trocar para arma melee depois de disparar? **Sim**, pela duração restante da janela já ativa (1.5 s), mas novos ataques melee não a renovam.

---

### Afinação de Projétil (`ranged_projectile_tuning`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Ranged / T4 / `ranged_kiting_steps` / 1 SP / Passiva
- **Status no saneamento:** mantida (já-implementada)
- **Resumo (1 frase):** Aumenta a velocidade dos projéteis de arco em +1 por rank.
- **Descrição completa:** Faz as flechas viajarem mais rápido (`ProjectileSpeed +1`), reduzindo o tempo de voo até o alvo e tornando mais fácil acertar inimigos em movimento. Não muda o alcance nem o dano — só a velocidade. Prereq do capstone.
- **Mecânica (PASSIVA):** Efeito exato: soma à velocidade de todos os projéteis de arco · Modificador: `BowProjectileSpeedFlat`, valor **+1.0 por rank** · Stack por rank? **Sim**, +1/rank, **cap 5 ranks (+5 total)** · Condição: arco equipado · Retroativo? **Sim** — soma ao `speed` base das ativas ranged também.

**Q&A fechado:**
1. Empilha por rank? **Sim**, +1 por rank, cap 5 ranks (+5).
2. Afeta a velocidade das ativas (ex.: `multishot_fan` speed 11)? **Sim**, o bônus soma ao `speed` base de cada ativa.
3. Afeta alcance ou dano? **Não** — só velocidade do projétil.
4. Precisa de arco? **Sim**.
5. Projétil mais rápido reduz o alcance efetivo? **Não** — alcance é distância máxima e independe da velocidade; só o tempo de voo encurta.
6. É retroativo? **Sim**.

---

# ATIVAS

> Todas as ativas abaixo são equipáveis em um dos 4 slots ativos (teclas 1–4). A "forma de uso" é: equipar no slot, apertar a tecla do slot. Todas obedecem aos **Invariantes da árvore** acima.

### Disparo Carregado (`ranged_charged_shot`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Ranged / T1 / `ranged_long_sight` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada (executor real existe), **mas nova-mecânica(implementar)** — falta o sistema de carga (hold-to-charge). Será implementada em **fable_70**.
- **Resumo (1 frase):** Segurar a tecla do slot carrega o disparo; soltar atira uma flecha forte de longo alcance.
- **Descrição completa:** O clássico tiro carregado de arqueiro. Ao **segurar** a tecla do slot, o player começa a carregar (com barra/indicador de carga visível); ao **soltar**, dispara. Com carga máxima entrega o dano e alcance cheios; soltar antes do tempo entrega um dano proporcional (com piso mínimo). É a primeira ativa da árvore e o nó de entrada para `multishot_fan`. Hoje o executor `combat.ranged.charged_shot` já existe e dispara como tiro forte normal — o que falta é **a mecânica de input de carga** (a flag dormente `NotYetExecutable` está inconsistente com isso). **A mecânica de carga será implementada em fable_70.**
- **Mecânica (ATIVA):**
  - **Input/slot:** segurar a tecla do slot (1–4) carrega; soltar dispara. (Diferente das outras ativas, que são tap.)
  - **Alvo:** direcional (facing do player). Sem trava de alvo.
  - **Alcance:** **9 m** (base do executor; soma `BowRangeFlat` de passivas).
  - **Nº de projéteis / spread:** 1 projétil, sem spread.
  - **Pierce:** nenhum (atinge o primeiro alvo e para).
  - **Dano:** **20** (carga máxima), tipo **Físico**. Escala proporcional à carga (ver abaixo).
  - **Status aplicado:** nenhum.
  - **Custo (stamina):** **20** — cobrado no momento do **disparo (soltar)**, não ao iniciar a carga.
  - **Cooldown:** **6 s** (médio) — começa a contar no disparo, não na carga.
  - **Movimento:** **pode iniciar a carga e disparar andando.** Mover **não cancela** a carga (o player carrega enquanto se reposiciona). Tomar dano/stagger **cancela** a carga (sem disparo, sem custo, sem cooldown).
  - **Telegraph/Animação/VFX (pixel art 2D):** pose de tração do arco que se intensifica com a carga; barra/indicador de carga sobre o player (0→cheia); ao soltar, projétil maior/brilhante para carga máxima, menor para carga parcial; SFX de tensão crescente + "twang" no disparo.
  - **Mecânica de carga (definição):**
    - **Tempo de carga máxima:** **0.8 s** segurando.
    - **Soltar antes de 0.8 s:** dano **proporcional ao tempo segurado**, com **piso de 50%** (mínimo **10 de dano** mesmo em tap rápido). Fórmula: `dano = 20 × clamp(tempoSegurado / 0.8, 0.5, 1.0)`. Alcance escala junto na mesma proporção (mín. 50% do range, máx. 9 m).
    - **Custo e cooldown** são fixos (20 stamina, 6 s CD) independentemente do nível de carga — não há "desconto" por carga parcial.
    - **Mover durante a carga:** **não cancela** (permitido carregar em movimento).

**Q&A fechado:**
1. Sem alvo no alcance, o projétil ainda dispara na direção do facing? **Sim** — é direcional; viaja 9 m e some.
2. Pode atirar andando? **Sim** — pode carregar E disparar em movimento; mover não cancela a carga.
3. Funciona em boss? **Sim** — dano normal; sem status, então nada a reduzir.
4. Consumo e falta de stamina? Custa **20 stamina, cobrado ao soltar**. Sem stamina, o disparo é recusado (`FailureReason` "Stamina insuficiente"), **sem cooldown e sem consumo**; a carga é descartada.
5. O que acontece se eu soltar antes da carga cheia? Dano proporcional ao tempo segurado, **piso de 50% (mín. 10 de dano)**; alcance escala junto; custo e cooldown não mudam.
6. Tomar dano cancela a carga? **Sim** — stagger/hit cancela a carga sem disparo, sem custo e sem cooldown.
7. Pierce: para em parede? Quantos alvos? **Sem pierce** — atinge o primeiro alvo (ou a parede) e para.
8. Precisa de arco equipado? E se estiver com arma melee? **Sim, requer arco.** Com melee, segurar a tecla **não inicia carga** e publica "Requer arco equipado" (sem custo, sem cooldown).
9. Onde será implementada a mecânica de carga? Em **fable_70** (a flag dormente inconsistente será resolvida lá).

---

### Linha Perfurante (`ranged_line_piercer`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Ranged / T3 / `ranged_quick_nock` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada (executor real, números finais)
- **Resumo (1 frase):** Disparo reto que perfura até 5 inimigos em linha.
- **Descrição completa:** Uma flecha rápida e perfurante que atravessa fileiras de inimigos, ideal contra grupos enfileirados ou corredores estreitos da caverna. Atinge até 5 alvos antes de parar; é destruída ao bater em parede. Prereq de `bleeding_arrow`.
- **Mecânica (ATIVA):**
  - **Input/slot:** tap na tecla do slot.
  - **Alvo:** direcional (facing).
  - **Alcance:** **10 m** (+ `BowRangeFlat`).
  - **Nº de projéteis / spread:** 1 projétil, sem spread.
  - **Pierce:** **sim, até 5 alvos** (`maxHitsPerProjectile: 5`).
  - **Dano:** **12** por alvo atingido, tipo **Físico**.
  - **Status aplicado:** nenhum.
  - **Custo (stamina):** **18**.
  - **Cooldown:** **7 s** (pesado).
  - **Movimento:** pode atirar andando.
  - **Telegraph/Animação/VFX:** flecha alongada com rastro de "linha"/trail reto; pequeno flash em cada inimigo perfurado; SFX seco de perfuração.

**Q&A fechado:**
1. Sem alvo no alcance, dispara no facing? **Sim** — viaja 10 m e some.
2. Pode atirar andando? **Sim**.
3. Funciona em boss? **Sim** — 12 de dano; um boss conta como 1 dos 5 alvos do pierce.
4. Consumo e falta de stamina? **18 stamina**; sem stamina, recusa com feedback, sem CD nem consumo.
5. Empilha algum status? **Não aplica status**, então não há stack/refresh.
6. Pierce: para em parede? Quantos alvos? **Para em parede** (destruída no impacto com geometria sólida, mesmo com hits sobrando) e perfura **no máximo 5 alvos**.
7. Cada alvo perfurado leva o dano cheio? **Sim**, 12 por alvo (sem falloff por alvo).
8. Precisa de arco? E com arma melee? **Sim, requer arco**; com melee é recusada (sem custo/CD).

---

### Tiro Triplo (`ranged_multishot_fan`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Ranged / T3 / `ranged_charged_shot` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada (executor real, números finais)
- **Resumo (1 frase):** Dispara 3 flechas em leque (centro + duas diagonais).
- **Descrição completa:** Um leque de 3 flechas com abertura de 28°, cobrindo uma faixa frontal. Excelente para cobrir área, acertar alvos que se movem e limpar pequenos grupos. Cada flecha é independente (uma única flecha pode acertar um alvo; alvos diferentes podem ser atingidos por flechas diferentes). É a ativa mais cara em stamina da árvore. Prereq de `ranged_kiting_steps`.
- **Mecânica (ATIVA):**
  - **Input/slot:** tap na tecla do slot.
  - **Alvo:** direcional (facing); o leque é centrado no facing.
  - **Alcance:** **7 m** (+ `BowRangeFlat`).
  - **Nº de projéteis / spread:** **3 projéteis**, **spread 28°** (centro + ±14°).
  - **Pierce:** nenhum (cada flecha para no primeiro alvo que atinge).
  - **Dano:** **8 por flecha** (físico). Um único alvo pode ser atingido por mais de uma flecha se estiver no cone de sobreposição (até 24 de dano potencial de perto).
  - **Status aplicado:** nenhum.
  - **Custo (stamina):** **24** (teto da banda).
  - **Cooldown:** **8 s** (pesado).
  - **Movimento:** pode atirar andando.
  - **Telegraph/Animação/VFX:** pose de disparo amplo; 3 trails divergindo em leque; SFX de múltiplas cordas.

**Q&A fechado:**
1. Sem alvo no alcance, dispara no facing? **Sim** — as 3 flechas viajam 7 m e somem.
2. Pode atirar andando? **Sim**.
3. Funciona em boss? **Sim** — de perto, um boss grande pode ser atingido por 2–3 flechas (até 24 de dano).
4. Consumo e falta de stamina? **24 stamina**; sem stamina, recusa com feedback, sem CD nem consumo.
5. Empilha algum status? **Não aplica status.**
6. Pierce: para em parede? Quantos alvos? **Sem pierce** — cada flecha para no 1º alvo ou na parede.
7. As 3 flechas podem acertar o mesmo alvo? **Sim**, se ele estiver no overlap do cone de perto (cada flecha aplica 8 independentemente).
8. Precisa de arco? E com melee? **Sim, requer arco**; com melee é recusada (sem custo/CD).

---

### Flecha Sangrante (`ranged_bleeding_arrow`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Ranged / T3 / `ranged_line_piercer` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** já-implementada (executor real aplica `status_bleed`)
- **Resumo (1 frase):** Flecha que causa dano de impacto e aplica Sangramento (`status_bleed`) no alvo.
- **Descrição completa:** Uma flecha de ponta serrilhada que, além do dano de impacto, aplica `status_bleed` no alvo — dano ao longo do tempo (DoT) que premia acertar e recuar. É a ativa-assinatura de DoT da árvore e prereq de `ranged_marked_prey`. O executor `combat.ranged.bleeding_arrow` já injeta `statusEffectId: "status_bleed"`.
- **Mecânica (ATIVA):**
  - **Input/slot:** tap na tecla do slot.
  - **Alvo:** direcional (facing).
  - **Alcance:** **8 m** (+ `BowRangeFlat`).
  - **Nº de projéteis / spread:** 1 projétil, sem spread.
  - **Pierce:** nenhum (atinge o primeiro alvo e para).
  - **Dano:** **14** de impacto, tipo **Físico**.
  - **Status aplicado:** `status_bleed` — **duração 6 s**, **dano por tick 2** (físico), **tick a cada 1 s** (6 ticks = 12 de dano total de bleed). **Stack:** não empilha em magnitude; um novo acerto **refresca a duração para 6 s** (refresh, não stack — máx. 1 instância de bleed por alvo vindo desta skill).
  - **Custo (stamina):** **16** (piso da banda — a mais barata da árvore).
  - **Cooldown:** **6 s** (médio).
  - **Movimento:** pode atirar andando.
  - **Telegraph/Animação/VFX:** flecha com ponta vermelha; ícone de gota de sangue sobre o alvo enquanto o bleed dura; tick visual de dano vermelho.

**Q&A fechado:**
1. Sem alvo no alcance, dispara no facing? **Sim** — viaja 8 m e some; sem alvo, nenhum bleed é aplicado.
2. Pode atirar andando? **Sim**.
3. Funciona em boss? **Sim** — o impacto (14) é normal. **Em boss, a duração do bleed é reduzida para 3 s** (3 ticks = 6 de dano), para não trivializar lutas longas via DoT.
4. Consumo e falta de stamina? **16 stamina**; sem stamina, recusa com feedback, sem CD nem consumo.
5. Empilha o bleed ou refresca? **Refresca** — reaplica a duração para 6 s (3 s em boss); **não empilha** stacks nem magnitude. Máx. 1 instância desta skill por alvo.
6. Pierce: para em parede? Quantos alvos? **Sem pierce** — 1 alvo; para na parede.
7. O dano de tick escala com `BowDamage` (Mão Firme)? **Não** — o tick é fixo (2/tick); só o impacto (14) escala com passivas de dano.
8. Precisa de arco? E com melee? **Sim, requer arco**; com melee é recusada (sem custo/CD).
9. O bleed cura se o alvo sair do alcance/visão? **Não** — uma vez aplicado, corre os 6 s independentemente da distância.

---

### Presa Marcada (`ranged_marked_prey`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Ranged / **T4** (ver nota de ordem) / `ranged_bleeding_arrow` / 1 SP / Ativa (slot 1–4)
- **Status no saneamento:** **nova(implementar)** — hoje é só `FeedbackOnlySkillEffectExecutor` ("Sistema de marcacao pendente"). Será implementada em **fable_70**.
- **Resumo (1 frase):** Marca um inimigo; enquanto marcado, seus disparos nele causam dano bônus.
- **Descrição completa:** A fantasia de caçador da árvore. Aplica um debuff de **marca** (`status_marked`, **novo**) em um inimigo; enquanto a marca dura, **todos** os disparos de arco do player nesse alvo (básicos e ativas ranged) causam **+25% de dano**. Sinergia direta com a árvore inteira (especialmente `bleeding_arrow` e `multishot_fan`). Hoje o executor é só feedback ("marcacao pendente") — **o sistema de marca/debuff e o novo `status_marked` serão implementados em fable_70**, que entrega o executor de buff/debuff que esta skill (e `marked` como debuff) exige.
  - **Nota de ordem (tier):** no catálogo atual o nó figurava em tier baixo, mas seu prereq é `ranged_bleeding_arrow` (T3). Isso é incoerente (um T1 não pode depender de um T3). **Decisão desta bíblia: `ranged_marked_prey` é T4**, prereq `ranged_bleeding_arrow` (T3) — assim a ordem de tiers fecha (T3 → T4) e a skill fica como recompensa tardia da linha de DoT/precisão.
- **Mecânica (ATIVA):**
  - **Input/slot:** tap na tecla do slot, mirando o facing (o alvo marcado é o primeiro inimigo atingido pelo projétil de marca, na direção do facing).
  - **Alvo:** 1 inimigo (o atingido pelo projétil de marca). **Só 1 alvo marcado por vez.**
  - **Alcance:** **8 m** (projétil de marca; + `BowRangeFlat`).
  - **Nº de projéteis / spread:** 1 projétil (não dá dano de impacto; só aplica a marca).
  - **Pierce:** nenhum — marca o primeiro inimigo atingido.
  - **Dano:** **0 de impacto** — esta skill não fere; ela **aplica a marca**. O bônus vem dos disparos seguintes.
  - **Status aplicado:** `status_marked` (**novo**) — **duração N = 8 s**; enquanto ativo, disparos de arco do player no alvo marcado causam **+X = +25% de dano**. **Não empilha** (reaplicar troca/renova; ver Q&A). Marca é exclusiva: **1 inimigo marcado por vez** — marcar um novo alvo **remove a marca do anterior**.
  - **Custo (stamina):** **18**.
  - **Cooldown:** **8 s** (pesado) — alinhado para que o jogador não mantenha a marca trivialmente em 100% do tempo.
  - **Movimento:** pode atirar andando.
  - **Telegraph/Animação/VFX:** ícone de alvo/mira flutuando sobre o inimigo marcado (visível à distância); pequeno "ping" de marcação no acerto; o ícone some quando a marca expira ou o alvo morre.

**Q&A fechado:**
1. Sem alvo no alcance, o projétil ainda dispara na direção do facing? **Sim** — o projétil de marca viaja 8 m no facing; se não atingir ninguém, **nenhuma marca é aplicada** (mas custo e cooldown SÃO consumidos, pois o disparo aconteceu).
2. Pode atirar andando? **Sim**.
3. Funciona em boss? **Sim, mas com duração reduzida:** em boss a marca dura **4 s** (metade), para não dar bônus permanente em lutas longas. O **+25%** de dano é o mesmo.
4. Consumo e falta de stamina? **18 stamina**; sem stamina, recusa com feedback, sem CD nem consumo.
5. A marca empilha ou refresca? **Não empilha.** Reacertar o mesmo alvo **renova a duração para 8 s** (4 s em boss). O bônus de dano é fixo em +25%, nunca soma.
6. Só 1 alvo por vez? **Sim** — marcar um novo inimigo **remove a marca do anterior** (a marca é exclusiva do player).
7. A marca some ao matar o alvo ou ao trocar de alvo? **Some ao matar** (alvo morto perde a marca) **e ao marcar outro** (transfere). Também expira por tempo (8 s / 4 s em boss).
8. Pierce: para em parede? **Sim** — o projétil de marca para na parede sem marcar nada (custo/CD consumidos pelo disparo).
9. O +25% afeta o dano de bleed do `bleeding_arrow`? **Não** — o bônus afeta o **dano de impacto** dos disparos no alvo marcado; o tick de bleed continua fixo.
10. Precisa de arco? E com melee? **Sim, requer arco**; com melee é recusada (sem custo/CD). Ataques melee no alvo marcado **não** ganham o bônus (a marca é só para disparos de arco).

---

# CAPSTONE

### Foco da Águia (`ranged_capstone_eagle_focus`)
- **Árvore / Tier / Prereq / Custo SP / Tipo:** Ranged / T5 (capstone) / `ranged_projectile_tuning` / 1 SP / Passiva (capstone, requer 8 nós ranged comprados)
- **Status no saneamento:** mantida (já-implementada nos +stats) · **nova(implementar)** para o bônus de skills ranged (componente novo proposto abaixo)
- **Resumo (1 frase):** Capstone da árvore: aumenta alcance e velocidade de projétil e amplifica todas as skills ranged.
- **Descrição completa:** O ápice da árvore de arco. Concede `BowRange +1` e `ProjectileSpeed +1` de forma permanente e adiciona o pilar de identidade que falta aos capstones de Ranged/Survival/Crafting hoje (que são só +stat, anticlímax — ver review §4.2). **Proposta concreta para o "bônus em skills ranged":** todas as ativas ranged equipadas causam **+15% de dano** (componente novo, a implementar). Isso amarra todo o investimento da árvore num payoff sentido, em vez de mais +1 de stat. Requer 8 nós ranged comprados (`reqNodes: 8`), o que naturalmente puxa o jogador por quase toda a árvore.
- **Mecânica (PASSIVA):**
  - **Efeito exato:** (a) `BowRangeFlat +1`; (b) `BowProjectileSpeedFlat +1`; (c) **+15% de dano em todas as ativas ranged** (`charged_shot`, `line_piercer`, `multishot_fan`, `bleeding_arrow`, e o bônus de `marked_prey` aplicado por cima).
  - **Modificador:** `BowRangeFlat +1.0`, `BowProjectileSpeedFlat +1.0`, e um novo `RangedSkillDamagePercent +15%` (componente a implementar).
  - **Stack por rank?** **Não** — capstone é **rank único** (1 SP, sem ranks). Não empilha.
  - **Condição:** arco equipado para os efeitos de arco valerem; o +15% só se aplica a ativas ranged disparadas com arco.
  - **Retroativo?** **Sim** — afeta básicos e ativas a partir da compra.

**Q&A fechado:**
1. Empilha por rank? **Não** — capstone é rank único; sem cap de ranks (1 SP só).
2. Precisa de arco? **Sim** — todos os componentes são de arco; com melee ficam inertes.
3. O +15% de dano de skills ranged afeta o tick de bleed do `bleeding_arrow`? **Não** — afeta o **dano de impacto** das ativas; o tick de bleed segue fixo.
4. O +15% empilha com o +25% de `marked_prey`? **Sim** — são fontes diferentes; aplicam-se de forma multiplicativa/combinada sobre o dano de impacto no alvo marcado.
5. É preciso ter os 8 nós? **Sim** — `reqNodes: 8`; o capstone só fica comprável após 8 nós ranged.
6. É retroativo? **Sim**, aplica imediatamente ao equipamento/skills atuais.
7. O componente +15% já existe no código? **Não** — é **proposta nova a implementar** (modificador `RangedSkillDamagePercent`); os +stats (range/speed) já existem.

---

## Apêndice A — Tabela-resumo do roster Ranged (números reais)

| Nó | Tier | Prereq | Tipo | Valor-chave |
|---|---|---|---|---|
| `ranged_steady_hand` | T1 | — | Passiva | BowDamage +1/rank (cap 5) |
| `ranged_long_sight` | T1 | steady_hand | Passiva | BowRange +0.5/rank (cap 5) |
| `ranged_quick_nock` | T1 | steady_hand | Passiva | AttackSpeed +0.1/rank (cap 5) |
| `ranged_charged_shot` | T1 | long_sight | Ativa | 20 dmg · 9 m · 20 stam · CD 6 s · carga 0.8 s (fable_70) |
| `ranged_line_piercer` | T3 | quick_nock | Ativa | 12 dmg · 10 m · pierce 5 · 18 stam · CD 7 s |
| `ranged_multishot_fan` | T3 | charged_shot | Ativa | 8×3 dmg · 7 m · spread 28° · 24 stam · CD 8 s |
| `ranged_bleeding_arrow` | T3 | line_piercer | Ativa | 14 dmg · 8 m · 16 stam · CD 6 s · status_bleed |
| `ranged_kiting_steps` | T2 | multishot_fan | Passiva | MoveSpeed +0.05/rank, 1.5 s pós-disparo |
| `ranged_marked_prey` | **T4** | bleeding_arrow | Ativa | marca 8 s · +25% dano no alvo · 18 stam · CD 8 s (fable_70) |
| `ranged_projectile_tuning` | T4 | kiting_steps | Passiva | ProjectileSpeed +1/rank (cap 5) |
| `ranged_capstone_eagle_focus` | T5 | projectile_tuning | Capstone | BowRange +1, ProjSpeed +1, +15% dano skills ranged |

> **Nota de coerência de tiers:** os tiers acima refletem o catálogo + a correção de `marked_prey` (T1→T4). Observa-se que o catálogo atual tem `kiting_steps`(T2) com prereq `multishot_fan`(T3) e `multishot_fan`(T3) com prereq `charged_shot`(T1) — a numeração de tier no código não é estritamente monotônica em relação aos prereqs. A cadeia de **prereqs** (que é o que de fato trava as compras) está consistente e é a fonte de verdade da ordem de desbloqueio; os rótulos de tier são organizacionais. A spec de implementação deve normalizar os rótulos de tier para refletir a profundidade real de prereq.

## Apêndice B — Dependências de implementação (fable_70)

- **`ranged_charged_shot`** — executor de projétil já existe (`combat.ranged.charged_shot`, dano 20); falta a **mecânica de input hold-to-charge** (carga 0.8 s, dano proporcional com piso 50%, mover não cancela, dano cancela). Resolver a flag dormente `NotYetExecutable` inconsistente.
- **`ranged_marked_prey`** — hoje é `FeedbackOnlySkillEffectExecutor`. Precisa do **executor de buff/debuff** e do **novo `status_marked`** (8 s normal / 4 s boss, +25% dano de disparo no alvo, 1 alvo exclusivo).
- **`ranged_capstone_eagle_focus`** — os +stats já existem; o componente **+15% dano de skills ranged** (`RangedSkillDamagePercent`) é novo.
- **Pré-requisito transversal:** cobrança de custo de stamina (`StaminaManager.TrySpend`) antes de executar — hoje `TODO_INTEGRATION_NOT_FINAL`. Sem isso, todos os custos acima são fictícios.
