# SpecKit â€” FASE9E Damage, Elementos, Status e FÃ³rmula de Combate

> **Feature:** FASE9E_DAMAGE_STATUS_FORMULA  
> **Status:** especificaÃ§Ã£o funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9E_DAMAGE_STATUS_FORMULA_SPEC_v1.0.md`

---

## 1. User story

Como jogador e designer do jogo, quero que dano, atributos, elementos, vulnerabilidades e status sigam regras previsÃ­veis para que combate, armas, monstros e balanceamento sejam consistentes.

---

## 2. Objetivos funcionais

### O1 â€” FÃ³rmula Ãºnica

Todo dano direto deve passar por `DamageCalculator`.

### O2 â€” Atributos ofensivos

Dano do jogador soma atributo conforme fonte:

- Strength para soco/melee;
- Dexterity para arco/fÃ­sico Ã  distÃ¢ncia;
- Intelligence para magia.

### O3 â€” Elementos

Ataques e DoTs podem ter elemento. Alvos podem resistir, ser vulnerÃ¡veis ou imunes.

### O4 â€” Imunidade

ElementMultiplier 0.0 existe e resulta em dano 0.

### O5 â€” Dano mÃ­nimo

Se BaseDamage > 0 e ElementMultiplier > 0.0, dano final mÃ­nimo Ã© 1.

### O6 â€” Vulnerabilidade

Janela vulnerÃ¡vel aplica 1.5x em dano direto, mas nÃ£o em DoT no MVP.

### O7 â€” Status negativos

Status podem afetar jogador e criaturas, possuem duraÃ§Ã£o, chance, power e efeito prÃ³prio.

### O8 â€” Burn/Poison

Burn e Poison podem coexistir e seus danos sÃ£o cumulativos. Mesmo status reaplicado renova duraÃ§Ã£o, sem stackar Power no MVP.

### O9 â€” Save/load

Status ativos devem ser persistidos com duraÃ§Ã£o restante.

---

## 3. Non-goals

Fora de escopo:

- defesa/armadura;
- crÃ­tico;
- accuracy/evasion;
- combos elementais avanÃ§ados;
- buffs complexos;
- stack avanÃ§ado de mesmo status;
- UI final de combat text;
- peso/durabilidade/velocidade detalhada de armas.

---

## 4. Regras de negÃ³cio

### R1 â€” FÃ³rmula

```text
scaledBase = BaseDamage + AttributeBonus
finalDamage = scaledBase Ã— ElementMultiplier Ã— VulnerabilityMultiplier Ã— StatusReceivedDamageMultiplier
finalDamage = Mathf.RoundToInt(finalDamage)
```

### R2 â€” Imunidade

Se `ElementMultiplier == 0.0`, `finalDamage = 0`.

### R3 â€” Dano mÃ­nimo

Se `ElementMultiplier > 0.0` e `BaseDamage > 0`, `finalDamage >= 1`.

### R4 â€” Atributos iniciais

Player comeÃ§a com:

```text
Strength = 1
Dexterity = 1
Intelligence = 1
```

### R5 â€” Arco/flecha

Arco nÃ£o define elemento no MVP. Elemento vem da flecha.

### R6 â€” Arma melee

Armas corpo a corpo podem ter elemento prÃ³prio.

### R7 â€” Burn

Burn dura 3s, considera Fire multiplier e ticka dano contÃ­nuo.

### R8 â€” Status temporÃ¡rio

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

## 6. CritÃ©rios de aceite

### CA1 â€” DamageCalculator

Todo dano direto playerâ†’inimigo usa `DamageCalculator`.

### CA2 â€” Atributos

Strength/Dexterity/Intelligence entram como soma direta conforme fonte.

### CA3 â€” Imunidade

Alvo imune ao elemento recebe 0 dano.

### CA4 â€” Dano mÃ­nimo

ResistÃªncia nÃ£o imune nÃ£o reduz dano para 0.

### CA5 â€” Vulnerabilidade

Janela vulnerÃ¡vel multiplica dano direto por 1.5.

### CA6 â€” DoT sem vulnerability

Burn/Poison nÃ£o recebem vulnerability window multiplier no MVP.

### CA7 â€” Burn

Fire Spark pode aplicar Burn por 3s, conforme chance configurada.

### CA8 â€” Burn elemental

Burn considera resistÃªncia/vulnerabilidade Fire.

### CA9 â€” Burn + Poison

Burn e Poison coexistem e seus danos por tick somam.

### CA10 â€” Status em player e inimigos

Stun e outros status podem afetar jogador e criaturas.

### CA11 â€” Save/load status

Status ativo Ã© salvo/restaurado com duraÃ§Ã£o restante.

### CA12 â€” Observabilidade

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

## 8. DependÃªncias

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

