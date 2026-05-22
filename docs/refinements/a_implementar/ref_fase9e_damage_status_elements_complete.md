# REF FUTURO — FASE9E damage status elements complete

> Origem histórica: $Source
> Status: Refinamento futuro preservado
> Spec futura relacionada: $Spec

---

## Decisões preservadas

Conteúdo histórico preservado abaixo para evitar perda operacional de decisões, escopo e pendências.

---

# FASE 9E — Damage, Elementos, Status e Fórmula de Combate Spec v1.0

> **Status:** spec aprovada para orientar próximas waves.  
> **Feature:** FASE9E_DAMAGE_STATUS_FORMULA  
> **Base:** FASE9C Player Equipment/Combat + FASE9D Enemy Architecture 40+ Monsters.  
> **Objetivo:** definir fórmula única de dano, elementos, atributos ofensivos, status negativos, dano contínuo, imunidade e janelas de vulnerabilidade para evitar drift entre player, inimigos, armas, magia e DoT.

---

## 1. Problema

O projeto começa a ter múltiplas fontes de dano:

- soco;
- espada;
- flecha;
- magia de fogo;
- ataques de inimigos;
- pulo do Slime;
- projéteis;
- status como Burn e Poison;
- vulnerabilidade temporária de inimigos;
- elementos, resistências e imunidades;
- atributos ofensivos do jogador.

Sem fórmula única, cada sistema pode calcular dano de um jeito diferente. Esta spec define o contrato único de dano e status para o MVP.

---

## 2. User story

Como jogador e designer do jogo, quero que dano, atributos, elementos, vulnerabilidades e status sigam regras previsíveis para que combate, armas, monstros e balanceamento sejam consistentes.

---

## 3. Decisões fechadas

| Pergunta | Decisão |
|---|---|
| Arredondamento | `Mathf.RoundToInt` |
| Burn considera resistência/vulnerabilidade Fire? | Sim |
| Imunidade elemental 0.0 existe no MVP? | Sim |
| Dano mínimo 1 ignora resistência alta? | Sim, exceto imunidade 0.0 |
| Status salva no save/load? | Sim |
| Stun afeta player no MVP? | Sim, pode afetar player e criaturas |
| Poison e Burn coexistem? | Sim, e o dano é cumulativo |
| Weakness/Vulnerable voltam ao normal? | Sim, todo status temporário restaura estado ao expirar |
| Atributos entram no dano? | Sim, soma direta por tipo |
| Atributos iniciais | Strength 1, Dexterity 1, Intelligence 1 |
| Arco tem elemento? | Não; elemento vem da flecha |
| Armas corpo a corpo podem ter elemento? | Sim |
| Peso/durabilidade/velocidade de armas melee | Outra spec |

---

## 4. Objetivos funcionais

### O1 — Fórmula única de dano direto

Todo dano direto deve passar por uma fórmula comum.

### O2 — Atributos ofensivos

Dano do jogador deve somar atributo ofensivo conforme fonte:

- Strength para soco e arma corpo a corpo;
- Dexterity para arco/flecha e dano físico Ã  distância;
- Intelligence para magia.

### O3 — Elementos

Ataques podem ter elemento, e alvos podem ter resistência, vulnerabilidade ou imunidade por elemento.

### O4 — Janela de vulnerabilidade

Inimigos podem abrir uma janela temporária onde recebem dano direto aumentado, default 1.5x.

### O5 — Status negativos

Ataques podem aplicar status negativos como Burn, Poison, Slow, Stun, Paralyze, Knockdown e Blind.

### O6 — Burn MVP

Magia de fogo pode aplicar Burn por 3s, causando dano contínuo e considerando Fire multiplier.

### O7 — Separar hit direto de DoT

Dano direto e dano ao longo do tempo devem ter regras separadas no MVP.

### O8 — Persistência de status

Status ativos devem ser salvos e restaurados com duração restante.

### O9 — Logs/observabilidade

Sistema deve logar cálculo de dano em modo debug para facilitar QA.

---

## 5. Non-goals

Fora de escopo desta spec:

- defesa/armadura completa;
- crítico;
- dodge chance/accuracy;
- elemental combo avançado;
- status positivos/buffs complexos;
- stacking avançado de mesmo status;
- balanceamento final;
- UI final de combat text;
- números flutuantes finais;
- tabela completa de todos os monstros;
- peso, durabilidade e velocidade detalhada de armas.

---

## 6. Tipos de dano

### 6.1 DamageKind

```csharp
public enum DamageKind
{
    Direct,
    DamageOverTime,
    Environmental
}
```

### 6.2 DamageElement

```csharp
public enum DamageElement
{
    Physical,
    Fire,
    Ice,
    Lightning,
    Poison,
    Earth,
    Wind,
    Water,
    Light,
    Shadow,
    Arcane
}
```

### 6.3 DamageSourceType

```csharp
public enum DamageSourceType
{
    Unknown,
    PlayerUnarmed,
    PlayerMeleeWeapon,
    PlayerRangedWeapon,
    PlayerMagic,
    EnemyContact,
    EnemyMelee,
    EnemyRanged,
    EnemyMagic,
    StatusEffect,
    Environment
}
```

---

## 7. Player combat attributes

O jogador deve começar com:

```text
Strength = 1
Dexterity = 1
Intelligence = 1
```

Uso no dano:

| Fonte | Atributo somado |
|---|---|
| Soco / desarmado | Strength |
| Arma corpo a corpo | Strength |
| Arco/flecha | Dexterity |
| Arma física Ã  distância não mágica | Dexterity |
| Magia | Intelligence |

Regra MVP:

- atributo é somado direto ao BaseDamage;
- progressão de atributos será tratada em outra spec.

---

## 8. DamageRequest vNext

O `DamageRequest` deve evoluir para carregar dados suficientes para cálculo único.

```csharp
public readonly struct DamageRequest
{
    public readonly string SourceId;
    public readonly string TargetId;
    public readonly DamageSourceType SourceType;
    public readonly DamageKind DamageKind;
    public readonly DamageElement Element;
    public readonly int BaseDamage;
    public readonly float KnockbackForce;
    public readonly Vector2 HitDirection;
    public readonly bool CanTriggerVulnerabilityMultiplier;
}
```

Regras:

- `SourceId` pode ser weaponId, enemyId, projectileId ou statusId.
- `TargetId` pode ser enemyId/playerId quando disponível.
- Eventos podem usar esses IDs.
- Nenhum evento deve carregar `GameObject`, `Transform` ou `ScriptableObject`.

---

## 9. Fórmula de dano direto MVP

### 9.1 Fórmula

```text
scaledBase = BaseDamage + AttributeBonus
finalDamage = scaledBase
            Ã— ElementMultiplier
            Ã— VulnerabilityMultiplier
            Ã— StatusReceivedDamageMultiplier
```

Depois:

```text
finalDamage = Mathf.RoundToInt(finalDamage)
```

### 9.2 Dano mínimo e imunidade

- Se `ElementMultiplier == 0.0`, dano final é `0`.
- Imunidade elemental existe no MVP.
- Se `ElementMultiplier > 0.0` e `BaseDamage > 0`, dano final mínimo é `1`.
- Resistência muito alta não pode reduzir dano para `0`, salvo imunidade explícita.
- Dano negativo nunca é permitido.

### 9.3 Exemplos de atributo

```text
Sword BaseDamage 3
Player Strength 1
scaledBase = 4
```

```text
Fire Spark BaseDamage 1
Player Intelligence 1
scaledBase = 2
```

---

## 10. Armas, flechas e elementos

Regras fechadas:

- Armas corpo a corpo podem ter elemento próprio.
- Arcos não definem elemento do tiro no MVP.
- Em ataques de arco, o elemento vem da flecha.
- Arcos aumentam dano, velocidade da flecha e velocidade de ataque.
- Flechas podem ser elementais.
- Magias têm elemento próprio.

Observação: atributos avançados de arma corpo a corpo, como durabilidade, peso e impacto na velocidade de movimento, serão tratados em outra spec para não misturar escopo.

---

## 11. ElementMultiplier

### 11.1 ElementModifier

```csharp
[Serializable]
public struct ElementModifier
{
    public DamageElement Element;
    public float DamageMultiplier;
}
```

### 11.2 EnemyElementProfileSO

```csharp
[CreateAssetMenu(fileName = "EnemyElementProfile", menuName = "CindarsHope/Combat/Enemy Element Profile")]
public class EnemyElementProfileSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public ElementModifier[] Modifiers;
}
```

### 11.3 Multiplicadores sugeridos

| Relação | Multiplier |
|---|---:|
| Imune | 0.0 |
| Muito resistente | 0.25 |
| Resistente | 0.5 |
| Levemente resistente | 0.75 |
| Neutro | 1.0 |
| Levemente vulnerável | 1.25 |
| Vulnerável | 1.5 |
| Muito vulnerável | 2.0 |

Decisão MVP:

- se elemento não existir no profile, usar 1.0;
- imunidade elemental 0.0 existe no MVP;
- imunidade 0.0 ignora regra de dano mínimo 1.

---

## 12. VulnerabilityMultiplier

### 12.1 Regra

Quando `EnemyVulnerabilityController.IsVulnerable == true`, dano direto pode ser multiplicado.

Default:

```text
VulnerabilityMultiplier = 1.5
```

### 12.2 O que recebe multiplicador

- dano direto: sim;
- DoT/Burn/Poison: não;
- dano ambiental: não, salvo config futura.

### 12.3 Status Vulnerable/Weakness

`Vulnerable` e `Weakness` podem existir como status negativos. Eles alteram temporariamente dano recebido ou dano causado e, ao expirar a duração, o alvo volta ao normal.

Decisão de escopo:

- conceito definido nesta spec;
- implementação pode vir depois de Burn/Poison/Slow/Stun/Blind/Paralyze/Knockdown;
- todo status temporário precisa restaurar o estado normal ao expirar.

---

## 13. StatusEffectType

```csharp
public enum StatusEffectType
{
    None,
    Burn,
    Poison,
    Bleed,
    Slow,
    Stun,
    Paralyze,
    Knockdown,
    Root,
    Blind,
    Weakness,
    Vulnerable,
    Fear,
    Silence,
    Confusion
}
```

---

## 14. StatusApplicationData

```csharp
[Serializable]
public struct StatusApplicationData
{
    public StatusEffectType StatusType;
    public float Chance;
    public float DurationSeconds;
    public int Power;
    public DamageElement Element;
}
```

---

## 15. Status MVP

Todo status negativo tem:

- tipo;
- duração;
- chance de aplicação;
- potência/power;
- forma própria de afetar alvo;
- alvo possível: jogador, criatura ou ambos.

Status podem afetar jogador e criaturas. O mesmo sistema deve suportar efeitos lançados pelo jogador contra inimigos e efeitos aplicados por inimigos contra o jogador.

### 15.1 Burn

Aplicado por magias/flechas/armas de fogo.

```text
Duration: 3s
Tick: 1 dano por segundo, escalável por Power no futuro
Element: Fire
Stack: não soma Power; reaplicar renova duração
```

Burn deve considerar resistência/vulnerabilidade Fire no MVP.

### 15.2 Poison

Aplicado por magia negra, flechas venenosas, criaturas venenosas ou armas especiais.

```text
Duration: 5s
Tick: 1 dano por segundo
Element: Poison
Stack: não soma Power do mesmo Poison; reaplicar renova duração
```

### 15.3 Slow

Aplicado por magia de terra, gelo, lama, peso ou efeitos de terreno.

```text
Duration: 3s
Effect: MoveSpeed Ã— 0.6
Stack: reaplicar renova duração
```

Ao expirar, velocidade volta ao normal.

### 15.4 Stun

Pode afetar jogador e inimigos no MVP.

```text
Duration: 0.5s a 1.0s
Effect: bloqueia movimento e ação
Stack: reaplicar renova duração até limite futuro
```

### 15.5 Paralyze

Aplicado principalmente por magia elétrica.

```text
Duration: curta
Effect: bloqueia movimento e/ou ação por pulsos curtos
Stack: reaplicar renova duração
```

### 15.6 Knockdown

Aplicado principalmente por magia de vento, impacto físico forte ou algumas criaturas.

```text
Duration: curta
Effect: derruba/interrompe ação e impede controle temporariamente
Stack: não stacka; reaplicar durante ativo apenas renova se permitido
```

### 15.7 Blind

Aplicado principalmente por magia de luz, poeira, flash ou ataques especiais.

```text
Duration: curta/média
Effect em inimigo: reduz detecção/precisão ou força erro de ação
Effect no jogador: reduz feedback visual/alcance de percepção em versão futura
```

### 15.8 Weakness

Status temporário que reduz dano causado pelo alvo.

```text
Effect: OutgoingDamageMultiplier < 1.0
Ao expirar: dano causado volta ao normal
```

### 15.9 Vulnerable

Status temporário que aumenta dano recebido pelo alvo.

```text
Effect: ReceivedDamageMultiplier > 1.0
Ao expirar: dano recebido volta ao normal
```

---

## 16. DoT — Damage over time

### 16.1 Regra geral MVP

DoT deve:

- tickar em intervalo previsível;
- aplicar dano via `DamageRequest` com `DamageKind.DamageOverTime`;
- considerar resistência/vulnerabilidade elemental do alvo;
- não acionar vulnerability window multiplier no MVP;
- não causar knockback;
- permitir coexistência entre tipos diferentes de DoT;
- não stackar infinitamente o mesmo status.

Decisões:

- Burn e Poison podem coexistir no mesmo alvo.
- Burn e Poison são cumulativos em dano, pois são status diferentes.
- Dois Burns iguais não somam Power no MVP; reaplicar Burn renova duração.
- Dois Poisons iguais não somam Power no MVP; reaplicar Poison renova duração.

### 16.2 Burn específico

```text
A cada 1s por 3s:
    aplicar 1 Fire DoT damage, ajustado por ElementMultiplier Fire
```

Se alvo for imune a Fire (`ElementMultiplier = 0.0`), Burn não causa dano.

### 16.3 Poison específico

```text
A cada 1s por 5s:
    aplicar 1 Poison DoT damage, ajustado por ElementMultiplier Poison
```

Burn e Poison podem causar dano no mesmo segundo se coexistirem.

---

## 17. Elemental status mapping

| Elemento | Status principal | Observação |
|---|---|---|
| Fire | Burn | dano contínuo |
| Lightning | Paralyze | interrupção/controle curto |
| Wind | Knockdown | derruba/interrompe |
| Earth | Slow | reduz velocidade |
| Shadow | Poison | dano contínuo/degeneração |
| Light | Blind | reduz precisão/percepção |
| Ice | Slow ou Root | congelamento leve; Root pode vir depois |
| Poison | Poison | veneno direto |
| Water | Wet futuro / Slow leve | pode combinar com Lightning depois |
| Arcane | Vulnerable ou Silence futuro | depende de design de magia |
| Physical | Bleed ou Stun futuro | depende de arma/ataque |

Chance de aplicar status:

- magias fracas têm chance baixa;
- magias fortes têm chance maior;
- a chance fica em `StatusApplicationData.Chance`;
- Fire Spark inicial pode ter chance leve de Burn, configurável.

---

## 18. DamageCalculator

Criar classe pura:

```csharp
public static class DamageCalculator
{
    public static DamageResult Calculate(DamageCalculationContext context);
}
```

A mesma classe deve tratar dano direto e dano ao longo do tempo, respeitando as diferenças:

| Regra | Direct | DoT |
|---|---|---|
| BaseDamage | sim | sim |
| AttributeBonus | sim quando source é player | não no MVP, salvo design futuro |
| ElementMultiplier | sim | sim |
| VulnerabilityWindowMultiplier | sim | não |
| StatusReceivedDamageMultiplier | sim | opcional |
| Knockback | possível | não |

### 18.1 DamageCalculationContext

```csharp
public readonly struct DamageCalculationContext
{
    public readonly DamageRequest Request;
    public readonly EnemyElementProfileSO TargetElementProfile;
    public readonly bool TargetIsVulnerable;
    public readonly float VulnerabilityMultiplier;
    public readonly float StatusReceivedDamageMultiplier;
    public readonly int AttributeBonus;
}
```

### 18.2 DamageResult

```csharp
public readonly struct DamageResult
{
    public readonly int FinalDamage;
    public readonly int ScaledBaseDamage;
    public readonly int AttributeBonus;
    public readonly float ElementMultiplier;
    public readonly float VulnerabilityMultiplier;
    public readonly float StatusMultiplier;
    public readonly bool WasReducedToMinimum;
    public readonly bool WasImmune;
}
```

---

## 19. Eventos

### DamageCalculatedEvent

Payload:

- `SourceId`
- `TargetId`
- `BaseDamage`
- `AttributeBonus`
- `ScaledBaseDamage`
- `FinalDamage`
- `Element`
- `ElementMultiplier`
- `VulnerabilityMultiplier`
- `DamageKind`

### StatusAppliedEvent

Payload:

- `SourceId`
- `TargetId`
- `StatusType`
- `DurationSeconds`
- `Power`

### StatusTickEvent

Payload:

- `TargetId`
- `StatusType`
- `Damage`
- `RemainingSeconds`

### StatusExpiredEvent

Payload:

- `TargetId`
- `StatusType`

---

## 20. Save/load de status

Status ativos devem ser persistidos no MVP.

Dados mínimos:

```csharp
[Serializable]
public class ActiveStatusSaveData
{
    public string TargetId;
    public string SourceId;
    public StatusEffectType StatusType;
    public DamageElement Element;
    public float RemainingSeconds;
    public int Power;
}
```

Regras:

- ao carregar, restaurar status ativo com duração restante;
- se target não existir mais, ignorar status com warning;
- status temporário expira normalmente depois do load;
- status expirado não deve ser salvo.

---

## 21. Exemplos

### 21.1 Espada contra inimigo neutro

```text
BaseDamage: 3
Player Strength: 1
ScaledBase: 4
Element: Physical
ElementMultiplier: 1.0
VulnerabilityMultiplier: 1.0
Final: 4
```

### 21.2 Espada durante janela vulnerável

```text
BaseDamage: 3
Player Strength: 1
ScaledBase: 4
Element: Physical
ElementMultiplier: 1.0
VulnerabilityMultiplier: 1.5
4 Ã— 1.5 = 6
RoundToInt = 6
```

### 21.3 Magia de fogo contra inimigo vulnerável a Fire

```text
BaseDamage: 1
Player Intelligence: 1
ScaledBase: 2
Element: Fire
ElementMultiplier: 1.5
VulnerabilityMultiplier: 1.0
2 Ã— 1.5 = 3
Final: 3
Burn aplicado por 3s
```

### 21.4 Burn

```text
Base tick: 1
Element: Fire
ElementMultiplier: 1.0
Tick 1s: 1 dano
Tick 2s: 1 dano
Tick 3s: 1 dano
Total: 3
```

### 21.5 Burn + Poison coexistindo

```text
Burn ativo: 1 Fire damage/s
Poison ativo: 1 Poison damage/s
No mesmo segundo:
    aplicar tick Burn
    aplicar tick Poison
Dano total do segundo: soma dos dois resultados
```

### 21.6 Imunidade elemental

```text
BaseDamage: 4
Element: Fire
ElementMultiplier: 0.0
FinalDamage: 0
Dano mínimo 1 não se aplica porque é imunidade explícita.
```

---

## 22. Critérios de aceite

- Todo dano direto de player contra inimigo passa por `DamageCalculator`.
- Strength soma em dano físico melee/desarmado.
- Dexterity soma em arco/físico Ã  distância.
- Intelligence soma em magia.
- Jogador começa com Strength, Dexterity e Intelligence iguais a 1.
- Inimigo com vulnerabilidade Fire recebe mais dano de Fire.
- Inimigo imune a Fire recebe 0 dano de Fire.
- Inimigo em janela vulnerável recebe dano direto multiplicado por 1.5.
- Burn/Poison não recebem vulnerability window multiplier.
- Fire Spark pode aplicar Burn por 3s conforme chance configurada.
- Burn usa ElementMultiplier Fire.
- Burn causa dano contínuo em ticks previsíveis.
- Reaplicar Burn renova duração, sem somar Power.
- Burn e Poison podem coexistir e seus danos são cumulativos por tick.
- Status negativos podem afetar jogador e inimigos.
- Status ativos devem ser salvos e restaurados no save/load.
- Se BaseDamage > 0 e ElementMultiplier > 0.0, dano final mínimo é 1.
- Se ElementMultiplier == 0.0, dano final é 0.
- Debug log ou evento mostra base, atributo, elemento, multiplicadores e finalDamage.

---

## 23. Tasks sugeridas

- PR-140 — Damage/status contracts.
- PR-140.1 — Player combat attributes contract.
- PR-141 — DamageRequest vNext + DamageCalculator.
- PR-142 — EnemyElementProfileSO.
- PR-143 — Vulnerability multiplier integration.
- PR-144 — EnemyStatusReceiver MVP.
- PR-145 — Burn Fire Spark integration.
- PR-145.1 — Elemental status mapping.
- PR-145.2 — Save/load active statuses.
- PR-146 — Damage/status events + debug logs.
- PR-147 — Validator/handoff.

---

## 24. Decisão final

O combate deve usar uma única fórmula de dano e status. Atributos entram como soma direta por tipo de ataque no MVP. Elementos e status devem ser data-driven e persistíveis. Peso, durabilidade e velocidade detalhada de armas ficam para outra spec.


## Itens que devem virar implementação

- [ ] Refinar em spec executável antes de código, quando aplicável.
- [ ] Validar dependências contra specs implementadas atuais.

## Fora de escopo / cuidado

- Não tratar este refinement como autorização automática de implementação.
- Não sobrescrever specs implementadas sem amendment/correction explícito.


