# Tasks — FASE9E Damage, Elementos, Status e Fórmula de Combate

> **Feature:** FASE9E_DAMAGE_STATUS_FORMULA  
> **Spec:** `spec.md`  
> **Plan:** `plan.md`

---

## PR-140 — Damage/status contracts

### Escopo

Criar contratos base.

### Arquivos esperados

- `DamageKind.cs`
- `DamageElement.cs`
- `DamageSourceType.cs`
- `StatusEffectType.cs`
- `StatusApplicationData.cs`

### Critérios

- [ ] Compila.
- [ ] Nenhum gameplay alterado.
- [ ] Nenhuma cena alterada.

---

## PR-140.1 — Player combat attributes contract

### Escopo

Preparar atributos ofensivos do jogador.

### Critérios

- [ ] Strength existe e inicia em 1.
- [ ] Dexterity existe e inicia em 1.
- [ ] Intelligence existe e inicia em 1.
- [ ] Progressão de atributos fica fora de escopo.

---

## PR-141 — DamageRequest vNext + DamageCalculator

### Escopo

Evoluir DamageRequest e criar DamageCalculator.

### Critérios

- [ ] DamageRequest carrega SourceType, DamageKind, Element e BaseDamage.
- [ ] DamageCalculator calcula scaledBase.
- [ ] DamageCalculator usa RoundToInt.
- [ ] Imunidade 0.0 gera dano 0.
- [ ] Dano mínimo 1 funciona quando não há imunidade.

---

## PR-142 — EnemyElementProfileSO

### Escopo

Criar profiles elementais.

### Critérios

- [ ] EnemyElementProfileSO existe.
- [ ] ElementModifier existe.
- [ ] Elemento ausente usa multiplier 1.0.
- [ ] Multiplier 0.0 é suportado.

---

## PR-143 — Vulnerability multiplier integration

### Escopo

Integrar janela vulnerável ao cálculo.

### Critérios

- [ ] Dano direto em janela vulnerável recebe 1.5x.
- [ ] DoT não recebe 1.5x.
- [ ] Logs mostram multiplier.

---

## PR-144 — EnemyStatusReceiver MVP

### Escopo

Criar receptor de status para inimigos.

### Critérios

- [ ] Burn funciona.
- [ ] Poison funciona.
- [ ] Slow funciona.
- [ ] Stun funciona.
- [ ] Reaplicar mesmo status renova duração.
- [ ] Burn e Poison coexistem.

---

## PR-144.1 — PlayerStatusReceiver MVP

### Escopo

Permitir status no jogador.

### Critérios

- [ ] Stun pode afetar jogador.
- [ ] Burn/Poison podem afetar jogador se aplicados por inimigo.
- [ ] Slow pode afetar jogador.
- [ ] Status expira e restaura estado normal.

---

## PR-145 — Burn Fire Spark integration

### Escopo

Integrar Fire Spark com Burn.

### Critérios

- [ ] Fire Spark possui chance configurável de aplicar Burn.
- [ ] Burn dura 3s.
- [ ] Burn ticka 1 dano/s antes de multiplicador.
- [ ] Burn considera Fire multiplier.

---

## PR-145.1 — Elemental status mapping

### Escopo

Registrar relação elemento/status em dados/contratos.

### Critérios

- [ ] Fire pode causar Burn.
- [ ] Lightning pode causar Paralyze.
- [ ] Wind pode causar Knockdown.
- [ ] Earth pode causar Slow.
- [ ] Shadow pode causar Poison.
- [ ] Light pode causar Blind.

---

## PR-145.2 — Save/load active statuses

### Escopo

Persistir status ativos.

### Critérios

- [ ] Status ativo salva tipo, source, target, element, power e duração restante.
- [ ] Load restaura status com duração restante.
- [ ] Status expirado não salva.
- [ ] Target inexistente no load gera warning e ignora status.

---

## PR-146 — Damage/status events + debug logs

### Escopo

Adicionar eventos/logs.

### Critérios

- [ ] DamageCalculatedEvent publicado.
- [ ] StatusAppliedEvent publicado.
- [ ] StatusTickEvent publicado.
- [ ] StatusExpiredEvent publicado.
- [ ] Debug log mostra base, atributo, elemento, multipliers e final.

---

## PR-147 — Validator/handoff

### Escopo

Validar dados e documentar entrega.

### Critérios

- [ ] Validator detecta ElementMultiplier inválido.
- [ ] Validator detecta status com duração inválida.
- [ ] Validator detecta chance fora de 0–1.
- [ ] Handoff contém smoke test.

---

## Smoke test final

- [ ] Espada dano = base + Strength.
- [ ] Arco/flecha dano = base + Dexterity.
- [ ] Magia dano = base + Intelligence.
- [ ] Fire vulnerability aumenta dano.
- [ ] Fire immunity zera dano.
- [ ] Vulnerability window aumenta dano direto 1.5x.
- [ ] Burn aplica por 3s.
- [ ] Burn considera Fire multiplier.
- [ ] Poison e Burn coexistem.
- [ ] Stun afeta inimigo e player.
- [ ] Status salva/carrega com duração restante.
- [ ] Console sem erro vermelho.

