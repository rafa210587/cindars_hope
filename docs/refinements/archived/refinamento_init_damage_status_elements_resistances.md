# refinamento_init_damage_status_elements_resistances

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `.specs/a_implementar/spec_damage_status_elements_resistances_runtime.md`
> Objetivo: evoluir formula MVP de dano para pipeline completo com tipos de dano, defesa, resistencias por multiplicador, vulnerabilidade global, status, efeitos temporais e floating damage numbers.

---

## 1. Estado atual

`DamageCalculator` calcula dano direto simples com:

```text
baseDamage
attributeBonus
typeMultiplier
```

Existem dados iniciais de status effect, mas ainda sem integracao completa com o pipeline de hit/damage.

---

## 2. Gaps

- Nao ha `DamageType` oficial completo.
- Defesa do inimigo ainda nao entra na formula principal.
- Resistencias/fraquezas/imunidades de combate nao estao integradas.
- EnvironmentalResistance da spec 10 precisa se relacionar com dano, mas nao substituir combat resistance.
- Status effect nao e aplicado por weapon/spell/enemy hit.
- Nao ha tick de dano ao longo do tempo integrado ao combate.
- Nao ha imunidade, stack rule, refresh rule ou duracao clara.
- Nao ha eventos de status aplicado/removido.
- Dano ainda nao aparece visualmente acima da criatura afetada.

---

## 3. Decisoes aprovadas

- DamageType MVP:

```text
Physical
Fire
Ice
Toxic
Lightning
Arcane
True
```

- `Poison` e status, nao damage type separado.
- Defesa entra como flat antes dos multiplicadores.
- Combat resistance usa multiplicador direto:

```text
Normal = 1.0
Resistant = 0.5
Weak = 1.5
Immune = 0.0
```

- Environmental resistance da spec 10 adiciona resistencia leve/indireta contra dano correlato, mas nao vira automaticamente o `CombatResistanceMultiplier` principal.
- Exemplo: `ColdResistance` pode adicionar leve resistencia a `Ice`, mas nao substitui o multiplicador de resistance/fraqueza/imunidade.
- Vulnerability window vale para tudo: toda criatura pode ter uma janela em que um ataque causa mais dano se executado no momento certo.
- Vulnerability multiplier inicial: `1.5x`.
- Critico fica fora desta spec.
- Accuracy/evasion fica fora desta spec.
- True damage ignora Defense e CombatResistanceMultiplier, mas nao ignora imunidade especial explicita, se existir.
- Status MVP:

```text
Burn
Poison
Bleed
Slow
Stun
```

- Freeze fica fora por enquanto.
- Mesmo `StatusId` reaplicado renova duracao e nao soma `Power` no MVP.
- Status diferentes podem coexistir.
- Damage numbers devem aparecer subindo acima da criatura afetada.
- Damage numbers devem ser implementados sem sprites custom obrigatorios, usando texto runtime, world-space canvas, TextMeshPro ou componente equivalente.

---

## 4. Formula oficial

Pipeline:

```text
rawDamage = BaseDamage + AttributeBonus + SourceFlatBonus

if DamageType == True:
    mitigatedDamage = rawDamage
    elementAdjustedDamage = mitigatedDamage
else:
    mitigatedDamage = max(0, rawDamage - Defense)
    elementAdjustedDamage = mitigatedDamage * CombatResistanceMultiplier

vulnerabilityAdjustedDamage = elementAdjustedDamage * VulnerabilityMultiplier
statusAdjustedDamage = vulnerabilityAdjustedDamage * StatusReceivedDamageMultiplier
finalDamage = RoundToInt(statusAdjustedDamage)
```

Regras:

- Se `BaseDamage > 0`, alvo nao e imune e `finalDamage` arredondado ficou 0, aplicar `finalDamage = 1`.
- Se `CombatResistanceMultiplier = 0.0`, dano final = 0, exceto se `DamageType == True` e nao houver imunidade especial explicita contra True.
- True ignora Defense e CombatResistanceMultiplier.
- True ainda respeita VulnerabilityMultiplier e StatusReceivedDamageMultiplier.

---

## 5. Atributos ofensivos

MVP:

```text
Strength: soco, melee fisico e armas pesadas
Dexterity: arco, fisico a distancia e armas leves
Intelligence: magia
```

Regras:

- Se atributos ainda nao existirem no runtime, criar hook/default seguro.
- AttributeBonus deve ser calculado em um ponto unico ou recebido no `DamageRequest` ja normalizado.
- Nao criar formula paralela por arma, spell ou inimigo.

---

## 6. Combat resistance vs environmental resistance

### CombatResistanceProfile

Usado pela pipeline de dano:

```text
DamageType -> CombatResistanceMultiplier
```

### EnvironmentalResistance

Vem da spec 10:

```text
ToxicResistance
ColdResistance
HeatResistance
```

Regras:

- Environmental resistance nao substitui CombatResistanceMultiplier.
- Environmental resistance pode adicionar uma reducao leve/indireta contra dano correlato, se configurado.
- `ColdResistance` pode reduzir levemente dano `Ice`.
- `HeatResistance` pode reduzir levemente dano `Fire`.
- `ToxicResistance` pode reduzir levemente dano `Toxic`.
- Essa reducao deve ser configuravel e limitada, para nao virar imunidade involuntaria.
- Combat resistance continua sendo a fonte principal para `Normal/Resistant/Weak/Immune`.

Sugestao MVP:

```text
EnvironmentalResistanceCombatContributionPerPoint = 0.01
MaxEnvironmentalCombatReduction = 0.25
```

---

## 7. Vulnerability window

Toda criatura pode ter uma vulnerability window.

Regras MVP:

- Vulnerability window e geral, nao por tipo de dano.
- Aplica em dano direto de qualquer `DamageType`, inclusive True, salvo decisao futura.
- Multiplicador padrao: `1.5x`.
- Efeitos DoT/status nao recebem vulnerability automaticamente a cada tick.
- O primeiro hit que aplica um status pode receber vulnerability no dano direto.

Contrato runtime:

```text
TargetVulnerabilityState.IsVulnerable
TargetVulnerabilityState.RemainingSeconds
VulnerabilityMultiplier
```

---

## 8. Status effects MVP

Status minimos:

```text
Burn: Fire DoT
Poison: Toxic DoT
Bleed: Physical DoT
Slow: reduz MoveSpeed
Stun: bloqueia acao por duracao curta
```

Freeze fica fora do MVP.

Campos minimos de `StatusEffectSO` ou equivalente:

```text
StatusId
DisplayName
StatusType
DamageType opcional
Power
DurationSeconds
TickIntervalSeconds default 1.0
MoveSpeedMultiplier opcional
BlocksActions opcional
CanPersist
RefreshPolicy
```

`RefreshPolicy` MVP:

```text
RefreshDurationNoPowerStack
```

Regras:

- Mesmo `StatusId` reaplicado renova duracao.
- Mesmo `StatusId` nao soma `Power` no MVP.
- Status diferentes podem coexistir.
- Status para de ticar se target morreu/desativou.
- Status expirado publica evento e remove efeito.
- Status removido manualmente publica evento separado.

---

## 9. Save/load de status

Persistir com DTO simples:

```text
StatusEffectSaveData
- TargetId
- StatusId
- SourceId opcional
- RemainingDuration
- Power
- DamageType opcional
- TickProgress
```

Regras:

- Player status deve persistir.
- Enemy status so precisa persistir se o enemy/snapshot ja tiver save runtime estavel.
- Se inimigos de cave ainda nao persistirem estado de entidade, preparar contrato e nao marcar persistencia enemy como completa.
- Nunca serializar `StatusEffectSO`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, `Collider` ou `Rigidbody`.

---

## 10. Floating damage numbers

Damage numbers sao parte do escopo minimo desta spec.

Objetivo:

```text
Ao sofrer dano, a criatura afetada exibe um numero subindo acima dela, deixando o dano evidente para o jogador.
```

Implementacao permitida sem sprites:

```text
World-space Canvas + TextMeshPro
Unity UI Text em world-space
Componente de texto runtime equivalente
Pooling simples opcional
```

Regras:

- Nao exigir sprite custom.
- Numero sobe ao longo de curta duracao.
- Numero some/fade apos duracao curta.
- Dano 0 por imunidade pode exibir "Immune" ou "0" conforme configuracao.
- Dano critico nao existe ainda; nao criar estilo critico final.
- VFX final, cores finais, fontes finais e polish ficam para spec 17.
- Se TextMeshPro nao estiver disponivel/configurado, usar fallback de texto UI basico e registrar pendencia visual.

---

## 11. Eventos

Usar existentes se houver equivalentes. Criar somente se necessario:

```text
DamageAppliedEvent
DamageBlockedEvent
DamageImmuneEvent
DamageNumberRequestedEvent
StatusAppliedEvent
StatusRefreshedEvent
StatusTickedEvent
StatusExpiredEvent
StatusRemovedEvent
VulnerabilityWindowStartedEvent
VulnerabilityWindowEndedEvent
```

---

## 12. Invariantes anti-regressao

Esta spec nao pode quebrar:

- Equipment stats/resistances da spec 10;
- EnvironmentalResistance da spec 10;
- stamina/status/time da spec 09;
- PlayerHealth/EnemyHealth existentes;
- punch/combat MVP existente;
- future player combat da spec 12;
- enemy AI futuro da spec 13;
- cave/environment exposure;
- save migration e DTOs simples;
- modal stack;
- regra de nao usar `GameObject.Find()` ou `FindObjectOfType()`;
- regra de nao serializar referencias Unity em DTOs.

---

## 13. Definition of Done

- [ ] Todo dano direto usa pipeline oficial.
- [ ] DamageType MVP existe: Physical, Fire, Ice, Toxic, Lightning, Arcane, True.
- [ ] Defense flat entra antes dos multiplicadores.
- [ ] Combat resistance usa multiplicadores Normal/Resistant/Weak/Immune.
- [ ] Environmental resistance pode contribuir levemente, sem substituir combat resistance.
- [ ] Vulnerability window global aplica 1.5x em dano direto quando ativa.
- [ ] True damage ignora Defense e CombatResistanceMultiplier.
- [ ] Critico nao e implementado.
- [ ] Accuracy/evasion nao e implementado.
- [ ] Burn, Poison, Bleed, Slow e Stun existem como status MVP.
- [ ] Reaplicar mesmo status renova duracao sem stackar power.
- [ ] Status diferentes coexistem.
- [ ] Status para de ticar em target morto/desativado.
- [ ] Player status persiste em save/load.
- [ ] Enemy status persiste somente se enemy snapshot/save estiver disponivel; caso contrario, pendencia clara.
- [ ] Damage numbers aparecem subindo acima da criatura afetada sem exigir sprites custom.
- [ ] Logs/debug exibem decomposicao do dano.
- [ ] Eventos oficiais de dano/status/vulnerability existem ou sao integrados aos equivalentes existentes.
- [ ] Invariantes anti-regressao preservadas.

---

## 14. Validacao

1. Dano Physical em alvo com Defense 0.
2. Dano Physical em alvo com Defense alta.
3. Dano Fire contra Resistant 0.5.
4. Dano Ice contra Weak 1.5.
5. Dano Toxic contra Immune 0.0.
6. True damage ignorando Defense e CombatResistanceMultiplier.
7. Vulnerability window global aplicando 1.5x.
8. Burn aplicando DoT por tick.
9. Poison aplicando Toxic DoT por tick.
10. Bleed aplicando Physical DoT por tick.
11. Slow reduzindo MoveSpeed.
12. Stun bloqueando acao curta sem travar UI/menu.
13. Reaplicar mesmo status e validar refresh sem stack de Power.
14. Matar/desativar alvo com status e validar que ticks param sem erro.
15. Damage number aparece acima do alvo, sobe e desaparece.
16. Salvar/carregar player status ativo.
17. Validar logs/debug com decomposicao completa do dano.
18. Validar Unity compile validation e docs validation.
