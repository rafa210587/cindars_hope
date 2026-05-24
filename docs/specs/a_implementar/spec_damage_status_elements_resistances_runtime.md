# SPEC - Damage, status, elements e resistances runtime

> Spec ID: spec_damage_status_elements_resistances_runtime
> Status: A implementar
> Ordem de execucao: 11
> Depende de: 00-10
> Bloqueia: 12, 13, 14
> Tipo: Runtime
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Completar pipeline de dano, elementos, resistencias, vulnerabilidades e status effects.
> Fora de escopo: UI final de combat text, balanceamento final, critico/accuracy/evasion completos e combos elementais avancados.

Fontes absorvidas:

- specs/FASE9E_DAMAGE_STATUS_FORMULA/spec.md
- docs/specs/a_implementar/spec_fase9e_damage_status_elements_complete.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_damage_status_elements_resistances.md

---

# /speckit.specify

## Contexto

O projeto ja possui `DamageCalculator` MVP e skeletons de status vindos do overnight. Esta spec consolida a regra final de runtime para dano direto, dano elemental, resistencias, vulnerabilidades e status temporarios.

## Problema

Sem uma formula unica, armas, spells, inimigos, equipamentos e IA podem aplicar dano por caminhos diferentes. Isso quebra balanceamento, save/load de status e integracao futura com equipment, skill actions e enemy AI.

## Objetivo

Todo dano direto deve passar por uma unica pipeline de calculo, com atributos ofensivos, elementos, resistencias, vulnerabilidades, imunidades e status temporarios persistiveis quando aplicavel.

## User stories / engineering stories

- Como jogador, quero que ataques, magias e inimigos causem dano de forma previsivel.
- Como designer, quero uma formula unica para balancear armas, spells, monstros e equipamentos.
- Como desenvolvedor, quero contratos claros para status temporarios e save/load.
- Como agente, devo integrar esta spec somente depois de equipment/durability/environment estar reconciliado.

## Regras funcionais obrigatorias

### Formula unica

Todo dano direto deve passar por `DamageCalculator` ou equivalente oficial.

```text
scaledBase = BaseDamage + AttributeBonus
finalDamage = scaledBase * ElementMultiplier * VulnerabilityMultiplier * StatusReceivedDamageMultiplier
finalDamage = RoundToInt(finalDamage)
```

### Atributos ofensivos

- Strength: soco e melee fisico.
- Dexterity: arco e fisico a distancia.
- Intelligence: magia.

### Elementos e resistencia

Ataques e efeitos temporarios podem ter elemento. Alvos podem ter resistencia, vulnerabilidade ou imunidade. Imunidade usa multiplicador `0.0` e resulta em dano final `0`.

### Dano minimo

Se `BaseDamage > 0` e `ElementMultiplier > 0.0`, o dano final minimo apos arredondamento deve ser `1`.

### Vulnerabilidade

Janela vulneravel aplica multiplicador padrao `1.5x` em dano direto. Efeitos temporarios nao precisam receber esse multiplicador no primeiro slice, salvo decisao explicita.

### Status temporarios minimos

A spec deve cobrir pelo menos:

- efeito de dano continuo por toxina/veneno;
- efeito de dano continuo por fogo/calor;
- efeito fisico continuo simples.

Mesmo status reaplicado renova duracao, mas nao soma `Power` no MVP. Status diferentes podem coexistir.

### Save/load

Status ativos devem ser persistidos com DTOs simples:

```text
StatusId
SourceId opcional
RemainingDuration
Power
ElementId opcional
TickProgress opcional
```

Nunca serializar referencias Unity.

## Criterios de aceite

- Todo dano direto usa a pipeline oficial.
- Dano minimo, imunidade e vulnerabilidade sao testaveis.
- Pelo menos tres status temporarios minimos existem como runtime ou contratos preparados e integrados ao pipeline.
- Reaplicacao de mesmo status renova duracao sem stackar power.
- Status ativos podem ser salvos/carregados com DTOs simples quando o runtime estiver completo.
- Logs/debug conseguem exibir dano base, bonus, multiplicadores, dano final e status aplicado.
- A spec nao e marcada como implementada sem evidencia no repo e validacao Unity aplicavel.

---

# /speckit.plan

## Arquitetura

Sistemas afetados:

```text
Assets/_Game/Scripts/Combat/DamageCalculator.cs
DamageRequest
DamageResult
DamageType
status runtime/manager oficial
enemy health/player health
equipment stats/resistances
future weapon/spell/skill actions
save DTOs de status
```

A logica de calculo deve ficar fora de MonoBehaviour pesado.

## Fluxo de dano direto

1. Fonte cria request com BaseDamage, DamageType, ElementId opcional, atributo ofensivo e source id.
2. Pipeline resolve atributo ofensivo.
3. Pipeline resolve resistencia, vulnerabilidade ou imunidade do alvo.
4. Pipeline aplica multiplicadores de status recebidos.
5. Pipeline arredonda e aplica regra de dano minimo.
6. Target health aplica resultado.
7. Eventos/logs sao publicados.
8. Chance de status e status payload sao avaliados apos dano direto quando aplicavel.

## Dados / DTOs / IDs

Usar IDs estaveis e tipos simples. Se ja existir enum/ID oficial, usar o existente e nao duplicar.

## Eventos

Eventos candidatos:

```text
DamageAppliedEvent
StatusAppliedEvent
StatusTickedEvent
StatusExpiredEvent
StatusRemovedEvent
```

Nomes finais devem seguir padrao `*Event` do projeto.

## Save/load

Adicionar status ativos ao save apenas se o runtime de status estiver realmente integrado. Caso contrario, registrar pendencia explicitamente.

## UI/debug

UI final fica fora de escopo, mas DebugHud/logs podem exibir:

```text
DamageSource
BaseDamage
AttributeBonus
ElementMultiplier
VulnerabilityMultiplier
FinalDamage
StatusApplied
RemainingDuration
```

## Riscos de regressao

- Player combat e enemy AI podem criar formulas paralelas se esta spec for ignorada.
- Equipment resistances podem divergir se damage/status for implementado antes da spec 10.
- Save schema pode quebrar se status persistir sem migration versionada.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar estado real de `DamageCalculator`, `DamageRequest`, `DamageResult` e status skeletons.
- [ ] Consolidar uma unica formula oficial.
- [ ] Definir como atributos ofensivos entram no dano.
- [ ] Definir matriz inicial de elementos/resistencias/imunidades.
- [ ] Implementar ou consolidar status temporarios minimos.
- [ ] Integrar status ao dano direto e ao dano continuo.
- [ ] Persistir status ativos se runtime completo for entregue.
- [ ] Atualizar logs/debug.
- [ ] Atualizar specs implementadas, registries, refinements, `IMPLEMENTATION_STATUS.md` e `PROJECT_LOG.md`.

## Arquivos permitidos

Somente durante implementacao futura:

```text
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
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
- Status temporarios minimos integrados.
- Resistencias, imunidades e vulnerabilidades testaveis.
- Save/load de status implementado ou pendencia mantida sem falso positivo.
- Validacao documental e Unity registrada.

## Validacao

- `./tools/docs/validate_docs.ps1`
- Unity batchmode se houver alteracao C#.
- Play Mode minimo: dano normal, dano imune, dano vulneravel, status temporario, save/load se aplicavel.
