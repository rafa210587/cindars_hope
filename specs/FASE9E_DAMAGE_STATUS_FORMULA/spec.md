# SpecKit — FASE9E Damage, Elementos, Status e Fórmula de Combate

> **Feature:** FASE9E_DAMAGE_STATUS_FORMULA  
> **Status:** especificação funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9E_DAMAGE_STATUS_FORMULA_SPEC_v1.0.md`

---

## 1. User story

Como jogador e designer do jogo, quero que dano, atributos, elementos, vulnerabilidades e status sigam regras previsíveis para que combate, armas, monstros e balanceamento sejam consistentes.

---

## 2. Objetivos funcionais

### O1 — Fórmula única

Todo dano direto deve passar por `DamageCalculator`.

### O2 — Atributos ofensivos

Dano do jogador soma atributo conforme fonte:

- Strength para soco/melee;
- Dexterity para arco/físico à distância;
- Intelligence para magia.

### O3 — Elementos

Ataques e DoTs podem ter elemento. Alvos podem resistir, ser vulneráveis ou imunes.

### O4 — Imunidade

ElementMultiplier 0.0 existe e resulta em dano 0.

### O5 — Dano mínimo

Se BaseDamage > 0 e ElementMultiplier > 0.0, dano final mínimo é 1.

### O6 — Vulnerabilidade

Janela vulnerável aplica 1.5x em dano direto, mas não em DoT no MVP.

### O7 — Status negativos

Status podem afetar jogador e criaturas, possuem duração, chance, power e efeito próprio.

### O8 — Burn/Poison

Burn e Poison podem coexistir e seus danos são cumulativos. Mesmo status reaplicado renova duração, sem stackar Power no MVP.

### O9 — Save/load

Status ativos devem ser persistidos com duração restante.

---

## 3. Non-goals

Fora de escopo:

- defesa/armadura;
- crítico;
- accuracy/evasion;
- combos elementais avançados;
- buffs complexos;
- stack avançado de mesmo status;
- UI final de combat text;
- peso/durabilidade/velocidade detalhada de armas.

---

## 4. Regras de negócio

### R1 — Fórmula

```text
scaledBase = BaseDamage + AttributeBonus
finalDamage = scaledBase × ElementMultiplier × VulnerabilityMultiplier × StatusReceivedDamageMultiplier
finalDamage = Mathf.RoundToInt(finalDamage)
```

### R2 — Imunidade

Se `ElementMultiplier == 0.0`, `finalDamage = 0`.

### R3 — Dano mínimo

Se `ElementMultiplier > 0.0` e `BaseDamage > 0`, `finalDamage >= 1`.

### R4 — Atributos iniciais

Player começa com:

```text
Strength = 1
Dexterity = 1
Intelligence = 1
```

### R5 — Arco/flecha

Arco não define elemento no MVP. Elemento vem da flecha.

### R6 — Arma melee

Armas corpo a corpo podem ter elemento próprio.

### R7 — Burn

Burn dura 3s, considera Fire multiplier e ticka dano contínuo.

### R8 — Status temporário

Ao expirar, status restaura o estado normal.

---

## 5. Entidades funcionais

- `DamageKind`
- `DamageElement`
- `DamageSourceType`
- `StatusEffectType`
- `StatusApplicationData`
- `DamageRequest`
- `DamageCalculationContext`
- `DamageResult`
- `DamageCalculator`
- `EnemyElementProfileSO`
- `ElementModifier`
- `ActiveStatusSaveData`

---

## 6. Critérios de aceite

### CA1 — DamageCalculator

Todo dano direto player→inimigo usa `DamageCalculator`.

### CA2 — Atributos

Strength/Dexterity/Intelligence entram como soma direta conforme fonte.

### CA3 — Imunidade

Alvo imune ao elemento recebe 0 dano.

### CA4 — Dano mínimo

Resistência não imune não reduz dano para 0.

### CA5 — Vulnerabilidade

Janela vulnerável multiplica dano direto por 1.5.

### CA6 — DoT sem vulnerability

Burn/Poison não recebem vulnerability window multiplier no MVP.

### CA7 — Burn

Fire Spark pode aplicar Burn por 3s, conforme chance configurada.

### CA8 — Burn elemental

Burn considera resistência/vulnerabilidade Fire.

### CA9 — Burn + Poison

Burn e Poison coexistem e seus danos por tick somam.

### CA10 — Status em player e inimigos

Stun e outros status podem afetar jogador e criaturas.

### CA11 — Save/load status

Status ativo é salvo/restaurado com duração restante.

### CA12 — Observabilidade

Evento/log mostra base, atributo, elemento, multiplicadores e dano final.

---

## 7. Elemental status mapping

| Elemento | Status principal |
|---|---|
| Fire | Burn |
| Lightning | Paralyze |
| Wind | Knockdown |
| Earth | Slow |
| Shadow | Poison |
| Light | Blind |
| Ice | Slow ou Root |
| Poison | Poison |
| Water | Wet futuro / Slow leve |
| Arcane | Vulnerable ou Silence futuro |
| Physical | Bleed ou Stun futuro |

---

## 8. Dependências

- `EnemyHealth`
- `PlayerManager`
- `DamageRequest`
- `EnemyVulnerabilityController`
- `EnemyStatusReceiver`
- `PlayerStatusReceiver` futuro
- `WeaponDataSO`
- `ProjectileController`
- `SaveManager`
- `GameEventBus`

---

## 9. Pronto para Plan quando

- Esta spec estiver aprovada.
- `docs_old/FASE9E_DAMAGE_STATUS_FORMULA_SPEC_v1.0.md` estiver lida.
- Estado real em `dev` tiver sido validado.


