# Plan — FASE9E Damage, Elementos, Status e Fórmula de Combate

> **Feature:** FASE9E_DAMAGE_STATUS_FORMULA  
> **Spec:** `spec.md`

---

## 1. Estratégia

Implementar em etapas pequenas:

1. contratos de dano/status;
2. atributos ofensivos do jogador;
3. DamageRequest vNext;
4. DamageCalculator puro;
5. profiles elementais;
6. integração com vulnerabilidade;
7. status receiver;
8. Burn/Poison/Slow/Stun MVP;
9. save/load de status;
10. eventos/logs;
11. validator/handoff.

---

## 2. Arquitetura proposta

### Novas pastas

```text
Assets/_Game/Scripts/Combat/Damage/
Assets/_Game/Scripts/Combat/Elements/
Assets/_Game/Scripts/Combat/Status/
Assets/_Game/Data/Combat/Elements/
Assets/_Game/Data/Combat/Status/
```

### Novos tipos

- `DamageKind`
- `DamageElement`
- `DamageSourceType`
- `StatusEffectType`
- `StatusApplicationData`
- `DamageCalculationContext`
- `DamageResult`
- `ElementModifier`
- `ActiveStatusSaveData`

### Novas classes

- `DamageCalculator`
- `EnemyElementProfileSO`
- `EnemyStatusReceiver`
- `PlayerStatusReceiver` ou contrato futuro equivalente

---

## 3. Integrações

### DamageRequest

Evoluir para carregar source, target, type, kind, element, base damage, knockback e flag de vulnerability.

### PlayerManager / PlayerStats

Adicionar ou preparar atributos:

- Strength = 1;
- Dexterity = 1;
- Intelligence = 1.

### EnemyHealth

Usar `DamageCalculator` antes de aplicar dano.

### EnemyVulnerabilityController

Fornecer `TargetIsVulnerable` e multiplier.

### EnemyStatusReceiver

Receber e tickar status.

### SaveManager

Persistir status ativos com duração restante.

### GameEventBus

Publicar eventos de cálculo, status aplicado, tick e expiração.

---

## 4. Fórmula técnica

```text
scaledBase = BaseDamage + AttributeBonus
rawFinal = scaledBase × ElementMultiplier × VulnerabilityMultiplier × StatusReceivedDamageMultiplier
rounded = Mathf.RoundToInt(rawFinal)
```

Regras:

- se ElementMultiplier == 0.0, final = 0;
- se ElementMultiplier > 0.0 e BaseDamage > 0, final mínimo = 1;
- DoT não recebe vulnerability window multiplier no MVP;
- Burn e Poison podem coexistir;
- mesmo status reaplicado renova duração.

---

## 5. Riscos

| Risco | Mitigação |
|---|---|
| Quebrar DamageRequest atual | criar adaptação/backwards compatibility por PR pequeno |
| Status salvar target inexistente | ignorar com warning no load |
| DoT ficar forte demais | manter tick base 1 e sem stack de Power |
| Imunidade deixar inimigo impossível | evitar muitos inimigos imunes no MVP |
| Stun no player frustrante | duração curta no MVP |

---

## 6. Testes manuais mínimos

- Espada dano = base + Strength.
- Arco/flecha dano = base + Dexterity.
- Magia dano = base + Intelligence.
- Fire vulnerability aumenta dano.
- Fire immunity zera dano.
- Vulnerability window aumenta dano direto 1.5x.
- Burn aplica por 3s.
- Burn considera Fire multiplier.
- Poison e Burn coexistem.
- Stun afeta inimigo e player.
- Status salva/carrega com duração restante.
- Logs exibem cálculo.

---

## 7. Fora de escopo técnico

- defesa/armadura;
- crítico;
- balance final;
- UI final;
- peso/durabilidade/velocidade avançada de armas;
- árvore de atributos;
- status positivos complexos.
