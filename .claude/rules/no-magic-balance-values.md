# Rule: Sem Magic Values de Balance

Valores tuneáveis de gameplay (dano, custo, duração, threshold, percentagem) devem viver em ScriptableObjects de balance ou constants nomeadas — nunca inline em métodos ou condicionais.

## O que é um magic balance value

Qualquer literal numérico que determina comportamento de gameplay:
- Thresholds (20% de fome crítica, 0 de HP)
- Custos (250g de respec, 10 de stamina por dash)
- Duração (2.5s de cooldown, 3s de toast)
- Dano (15 HP/s de fome zero, 25 de dano base)
- Escalas e multipliers (1.15x bônus de acessório)
- Horas do dia (02:00 de colapso por fadiga)

## Precedentes canônicos — copiar, não reinventar

| Precedente | Sistema | Tipo |
|---|---|---|
| `PlayerNeedsBalanceSO` | `BaseStaminaRegenRate`, `ZeroHungerDamagePerSecond`, `CriticalHungerThreshold` | SO de balance |
| `SkillTierRules.TierPointThresholds` | Pontos gastos para desbloquear tier 2, 3... | const array |
| `SkillRespecService.DefaultRespecCostGold = 250` | Custo de respec | const de classe |
| `DerivedFollowupFormulas.CraftTimeMultiplier` | Multiplicador de crafting | método puro configurável |
| `FatigueSystem._collapseHour` | Hora de colapso | `[SerializeField]` |

## Regra

```csharp
// ERRADO — magic number inline
if (hunger < 20) PublishCriticalEvent();
yield return new WaitForSeconds(3.5f);
if (damage > 150) TriggerKnockback();

// CORRETO — threshold nomeado em SO ou const
if (hunger < _balance.CriticalHungerThreshold) PublishCriticalEvent();
yield return new WaitForSeconds(_config.ToastDurationSeconds);
if (damage > _combat.KnockbackThreshold) TriggerKnockback();
```

## Onde colocar o valor

| Tipo de valor | Onde |
|---|---|
| Tuneável em runtime pelo designer | `[SerializeField]` num SO de balance dedicado |
| Técnico fixo (não muda entre builds) | `private const float X = ...` no topo da classe |
| Compartilhado entre sistemas | SO de balance dedicado (ex.: `PlayerNeedsBalanceSO`) |
| Threshold de regra de domínio | `const` no serviço responsável pela regra |

## O que NÃO é magic value

- `0` e `1` em comparações de flag/bool (`if (count == 0)`)
- Índices de array bem documentados (`[0]` = slot primário)
- IDs de string — cobertos pela rule `id-stability`
- Contagem de items numa lista fixa com contexto óbvio

## Enforcement

Revisional — `/code-review`, `architecture-reviewer`, e `non-regression-review`. Sem hook mecânico (falsos positivos altos para literais numéricas genéricas). O hook `runtime-code-guard` não cobre este padrão.
