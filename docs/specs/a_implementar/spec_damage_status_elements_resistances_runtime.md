# SPEC - Damage, status, elements e resistances runtime

> Spec ID: spec_damage_status_elements_resistances_runtime
> Status: A implementar
> Ordem de execucao: 11
> Depende de: 00-10
> Bloqueia: 12, 13, 14, 17
> Tipo: Runtime/UI minima
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Completar pipeline unica de dano, tipos de dano, defesa, resistencias por multiplicador, vulnerabilidade global por janela de timing, status effects temporarios, save/load de status quando aplicavel, eventos e floating damage numbers simples.
> Fora de escopo: critico, accuracy/evasion, combos elementais avancados, balanceamento final, VFX final, UI final de combate consolidada, sprites custom de dano, Packages, ProjectSettings e docs_old.

Fontes absorvidas:
- specs/FASE9E_DAMAGE_STATUS_FORMULA/spec.md
- docs/specs/a_implementar/spec_fase9e_damage_status_elements_complete.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_damage_status_elements_resistances.md

---

# /speckit.specify

## Contexto

O projeto ja possui `DamageCalculator` MVP e skeletons de status vindos do overnight. Esta spec consolida a regra runtime oficial para dano direto, tipos de dano, defesa, resistencias por multiplicador, vulnerabilidade, imunidades, status temporarios e exibicao minima de dano flutuante.

Specs anteriores relevantes:

```text
09 - Hunger/stamina/status/time: status MVP, stamina e tempo
10 - Equipment/durability/environment/loot: stats de equipamento, AttackSpeed, resistencias ambientais Toxic/Cold/Heat
```

A spec 10 introduz `EnvironmentalResistance` para ambiente. Esta spec 11 introduz `CombatResistanceProfile` para dano. As duas coisas podem somar beneficios indiretos, mas nao sao o mesmo contrato.

## Pre-condicoes

Implementar runtime somente depois de specs 02-10 estarem realmente implementadas:

```text
02 - save schema migration
03 - inventory slots/capacity/painel de itens
04 - farm irrigacao/solo/menu contextual
05 - world activities/fishing/trees/pickups/loot
06 - economy/shop/dialogue modal
07 - crafting/workstations/modal stack
08 - town/npc/dialogue
09 - hunger/stamina/status/time
10 - equipment/durability/environment/loot
```

Antes de alterar codigo, revalidar:

```text
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Enemy/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/**
```

Se spec 10 nao tiver `ItemInstanceId`, stats derivados e resistencias ambientais implementados, nao criar fallback paralelo; registrar bloqueio ou integrar no contrato real existente.

## Problema

Sem uma formula unica, armas, spells, inimigos, equipamentos e IA podem aplicar dano por caminhos diferentes. Isso quebra balanceamento, save/load de status, resistencias, vulnerabilidade e integracao futura com equipamentos, skill actions e enemy AI.

Gaps atuais:

- `DamageType` oficial ainda nao esta consolidado.
- Defesa ainda nao entra de forma unica na formula.
- Combat resistance/fraqueza/imunidade nao esta integrada.
- Environmental resistance da spec 10 precisa se relacionar, mas nao substituir, combat resistance.
- Status effect nao esta integrado a weapon/spell/enemy hit.
- Nao ha tick de dano ao longo do tempo integrado ao combate.
- Nao ha regra clara de refresh/stack/duracao.
- Nao ha eventos oficiais de status aplicado/removido.
- Dano nao e exibido de forma clara sobre a criatura atingida.

## Objetivo

Todo dano direto deve passar por uma unica pipeline oficial, com:

- `DamageRequest`;
- `DamageCalculator`;
- `DamageResult`;
- defesa flat;
- tipos de dano;
- resistencia/fraqueza/imunidade por multiplicador;
- vulnerability window global;
- status effects temporarios;
- floating damage numbers;
- logs/debug de decomposicao do dano;
- eventos oficiais.

## Decisoes aprovadas

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
- Vulnerability window vale para tudo: toda criatura pode ter janela de vulnerabilidade em que um ataque causa mais dano se executado no momento certo.
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

## Formula oficial

Todo dano direto deve passar por `DamageCalculator` ou equivalente oficial.

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

Regra de dano minimo:

```text
Se BaseDamage > 0, alvo nao e imune e finalDamage arredondado ficou 0, aplicar finalDamage = 1.
```

Regra de imunidade:

```text
Se CombatResistanceMultiplier = 0.0, dano final = 0, exceto se DamageType == True e nao houver imunidade especial explicita contra True.
```

Regra de True damage:

```text
True ignora Defense e CombatResistanceMultiplier.
True ainda respeita VulnerabilityMultiplier e StatusReceivedDamageMultiplier, salvo decisao futura explicita.
True pode ser bloqueado por uma imunidade especial futura, mas essa imunidade nao faz parte do MVP.
```

## Atributos ofensivos

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

## DamageRequest / DamageResult

`DamageRequest` minimo:

```text
SourceId
TargetId
BaseDamage
DamageType
OffensiveAttributeType opcional
AttributeBonus
SourceFlatBonus
CanTriggerVulnerability
StatusApplicationRules[] opcional
IsDamageOverTimeTick
```

`DamageResult` minimo:

```text
SourceId
TargetId
BaseDamage
AttributeBonus
SourceFlatBonus
Defense
DamageType
CombatResistanceMultiplier
EnvironmentalResistanceContribution opcional
VulnerabilityMultiplier
StatusReceivedDamageMultiplier
FinalDamage
WasImmune
WasVulnerable
AppliedStatuses[]
DebugBreakdown
```

## Combat resistance vs environmental resistance

### CombatResistanceProfile

Usado pela pipeline de dano:

```text
DamageType -> CombatResistanceMultiplier
```

Exemplo:

```text
Fire: 0.5
Ice: 1.5
Toxic: 0.0
```

### EnvironmentalResistance

Vem da spec 10:

```text
ToxicResistance
ColdResistance
HeatResistance
```

Regras de relacao:

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

Ou seja: environmental resistance pode reduzir ate 25% do dano correlato no MVP, mas nao muda o multiplicador principal nem cria imunidade.

Se isso for complexo demais no runtime atual, preparar hook de `EnvironmentalResistanceContribution` e manter valor 0, sem mentir que esta implementado.

## Vulnerability window

Toda criatura pode ter uma vulnerability window.

Conceito:

```text
Uma janela curta de oportunidade em que um ataque executado no timing correto causa mais dano.
```

Regras MVP:

- Vulnerability window e geral, nao por tipo de dano.
- Aplica em dano direto de qualquer `DamageType`, inclusive True, salvo decisao futura.
- Multiplicador padrao: `1.5x`.
- Efeitos DoT/status nao recebem vulnerability automaticamente a cada tick.
- O primeiro hit que aplica um status pode receber vulnerability no dano direto.
- Sistema de UI/VFX final de timing fica para specs de combate/UI, mas contrato runtime deve existir:

```text
TargetVulnerabilityState.IsVulnerable
TargetVulnerabilityState.RemainingSeconds
VulnerabilityMultiplier
```

## Status effects MVP

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

## Status runtime

Criar/consolidar:

```text
StatusEffectManager
StatusEffectInstance
StatusEffectRuntime
StatusEffectTarget
```

Regras:

- StatusEffectManager aplica, renova, remove e ticka status.
- Damage over time deve gerar `DamageRequest` marcado como `IsDamageOverTimeTick = true`.
- DoT deve usar a pipeline oficial de dano quando fizer sentido.
- Slow altera move speed via modificador controlado.
- Stun bloqueia acoes, mas nao deve travar UI, menus ou save.
- Status nao deve executar ticks quando o tempo/logica estiver pausada pela spec 09.

## Save/load de status

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
- Se `StatusId` nao existir ao carregar, logar warning e ignorar com seguranca.

## Floating damage numbers

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

Dados minimos:

```text
DamageNumberValue
WorldPosition
TargetId opcional
WasImmune
WasVulnerable
DamageType
```

Eventos:

```text
DamageNumberRequestedEvent opcional
```

## Eventos

Usar existentes se houver equivalentes. Criar somente se necessario, seguindo `*Event`:

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

Nao duplicar eventos com mesmo significado.

## Logs/debug

Debug/log deve conseguir exibir:

```text
DamageSource
TargetId
BaseDamage
AttributeBonus
SourceFlatBonus
Defense
DamageType
CombatResistanceMultiplier
EnvironmentalResistanceContribution
VulnerabilityMultiplier
StatusReceivedDamageMultiplier
FinalDamage
WasImmune
WasVulnerable
AppliedStatuses
```

## UI/modal/pause

- Damage numbers nao sao modal.
- Damage numbers nao pausam tempo.
- Damage numbers nao bloqueiam input.
- Damage numbers nao devem ficar ativos em alvos destruidos/desativados sem null-safe handling.
- Dialog/Shop/Crafting/Inventory modals continuam seguindo modal stack das specs anteriores.

## Invariantes anti-regressao

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

Se alguma integracao ainda nao existir, expor contrato e registrar pendencia em vez de criar formula paralela ou falso positivo.

## Criterios de aceite

- Todo dano direto usa pipeline oficial.
- DamageType MVP existe: Physical, Fire, Ice, Toxic, Lightning, Arcane, True.
- Defense flat entra antes dos multiplicadores.
- Combat resistance usa multiplicadores Normal/Resistant/Weak/Immune.
- Environmental resistance pode contribuir levemente, sem substituir combat resistance.
- Vulnerability window global aplica 1.5x em dano direto quando ativa.
- True damage ignora Defense e CombatResistanceMultiplier.
- Critico nao e implementado.
- Accuracy/evasion nao e implementado.
- Burn, Poison, Bleed, Slow e Stun existem como status MVP.
- Reaplicar mesmo status renova duracao sem stackar power.
- Status diferentes coexistem.
- Status para de ticar em target morto/desativado.
- Player status persiste em save/load.
- Enemy status persiste somente se enemy snapshot/save estiver disponivel; caso contrario, pendencia clara.
- Damage numbers aparecem subindo acima da criatura afetada sem exigir sprites custom.
- Logs/debug exibem decomposicao do dano.
- Eventos oficiais de dano/status/vulnerability existem ou sao integrados aos equivalentes existentes.
- Nenhum item das invariantes anti-regressao e quebrado.
- Validacao documental e Unity compile validation registradas.

---

# /speckit.plan

## Arquitetura

Sistemas afetados:

```text
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Combat/StatusEffect/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Enemy/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/UI/Combat/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Combat/**
Assets/_Game/Data/StatusEffects/**
```

A logica de calculo deve ficar fora de MonoBehaviour pesado. MonoBehaviours devem ser bridges para health, UI e Unity lifecycle.

## Ordem segura de implementacao

1. Revalidar `DamageCalculator`, `DamageRequest`, `DamageResult`, health e status skeletons.
2. Confirmar specs 02-10 implementadas antes de runtime.
3. Criar/consolidar `DamageType` oficial.
4. Criar/consolidar `CombatResistanceProfile`.
5. Atualizar formula oficial com defense, true damage, vulnerability e status multipliers.
6. Integrar environmental resistance contribution como hook/config leve.
7. Criar/consolidar `StatusEffectSO` e status runtime.
8. Implementar Burn, Poison, Bleed, Slow e Stun.
9. Integrar status DoT com DamageCalculator.
10. Implementar vulnerability window runtime contract.
11. Implementar save/load de player status e contrato para enemy status.
12. Implementar floating damage numbers sem sprite custom obrigatorio.
13. Criar eventos/logs/debug.
14. Validar anti-regressao e atualizar tracking.

## Fluxo de dano direto

```text
Fonte cria DamageRequest
Pipeline resolve atributos ofensivos
Pipeline resolve Defense
Pipeline resolve CombatResistanceMultiplier
Pipeline resolve EnvironmentalResistanceContribution se configurado
Pipeline resolve VulnerabilityMultiplier
Pipeline resolve StatusReceivedDamageMultiplier
Pipeline aplica regras de True/Immune/min damage
Health aplica FinalDamage
Eventos/logs publicados
Floating damage number solicitado
StatusApplicationRules avaliadas apos dano direto
```

## Fluxo de status

```text
Hit ou sistema aplica status
StatusEffectManager valida StatusId
Se mesmo status ativo, renova duracao
Se novo status, adiciona instancia
A cada tick ativo, aplica efeito
DoT cria DamageRequest oficial
Ao expirar, remove e publica evento
```

## Fluxo de damage number

```text
DamageAppliedEvent ou DamageResult recebido
Resolver posicao acima do target
Instanciar/reusar texto world-space
Animar subida curta
Fade/remover
Null-safe se target desaparecer
```

## Riscos de regressao

- Formula paralela em player combat/enemy AI.
- Environmental resistance virar imunidade acidental.
- True damage bypassar tudo de forma abusiva.
- DoT causar dano por frame.
- Status continuar ticar em alvo morto/desativado.
- Damage number criar muitos objetos sem cleanup.
- Save de status serializar referencias Unity.

## Mitigacao

- DamageCalculator unico.
- Contribution ambiental limitada/configuravel.
- TickIntervalSeconds por status.
- Null-safe target state.
- Pooling simples ou cleanup garantido para damage numbers.
- DTOs simples com IDs.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar estado real de `DamageCalculator`, `DamageRequest`, `DamageResult`, health e status skeletons.
- [ ] Confirmar specs 02-10 implementadas antes de runtime.
- [ ] Criar/consolidar `DamageType` oficial.
- [ ] Criar/consolidar `CombatResistanceProfile`.
- [ ] Implementar formula oficial com defense flat, multipliers e true damage.
- [ ] Implementar vulnerability window global.
- [ ] Integrar environmental resistance contribution leve/configuravel ou hook claro.
- [ ] Criar/consolidar `StatusEffectSO`.
- [ ] Criar/consolidar `StatusEffectManager` e `StatusEffectInstance`.
- [ ] Implementar Burn, Poison, Bleed, Slow e Stun.
- [ ] Integrar DoT com DamageCalculator.
- [ ] Implementar save/load de player status.
- [ ] Preparar contrato de enemy status save quando snapshot existir.
- [ ] Implementar floating damage numbers sem sprite custom obrigatorio.
- [ ] Criar eventos oficiais de dano/status/vulnerability.
- [ ] Atualizar logs/debug de decomposicao do dano.
- [ ] Atualizar docs implementados/status/registries/refinements/log.

## Arquivos permitidos

```text
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Enemy/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/UI/Combat/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Combat/**
Assets/_Game/Data/StatusEffects/**
docs/specs/**
docs/refinements/**
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

## Arquivos proibidos

```text
docs_old/**
Packages/**
ProjectSettings/**
```

## Definition of Done

- Formula oficial aplicada.
- Damage types, defense, resistencias, imunidades, vulnerability e true damage testaveis.
- Status temporarios minimos integrados.
- Damage numbers aparecem acima da criatura atingida.
- Save/load de player status implementado.
- Enemy status save implementado ou pendencia mantida sem falso positivo.
- Validacao documental e Unity registrada.

## Validacao obrigatoria

Rodar:

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
```

Play Mode minimo:

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
