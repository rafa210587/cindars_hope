# SPEC — Equipment: Baselines Mecânicos (ASPD, Scaling, Armor/Shield, Charged Profiles)

> **Spec ID:** `fable_03_spec_equipment_mechanical_baselines_runtime`
> **Status:** BUILD_VALIDATED (executada — E08; evidência: docs/validation/fable_03_spec_equipment_mechanical_baselines_runtime_execution_report.md)
> **Wave:** FABLE — Gap Closure Bloco A (combate)
> **Priority:** P1
> **Type:** Runtime / Data
> **Domain:** Combat / Inventory
> **Parallelizable:** NO
> **Parallel group:** fable_bloco_A
> **Can run with:** N/A
> **Must not run with:** fable_01, fable_02, fable_06
> **Repo lock scope:** `WeaponDataSO`, `EquipmentDataSO`, `DamageCalculator`, geradores de combat data
> **Depends on:**
> - `fable_02_spec_combat_weapon_actions_derived_stats_runtime`
> **Blocks:**
> - `fable_06`
> **Scope:** campos mecânicos de baseline em armas/armaduras/escudos aplicados ao runtime de dano.
> **Out de scope:** materiais/tiers completos, upgrade crafting, tooltips de comparação (F14), arte.

required_adrs: []
required_game_rules: [combat_rules.md, economy_rules.md]

---

# /speckit.specify

## Contexto

`EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md` define o baseline mecânico inicial:
WeaponDamage + ASPD + scaling por atributo + StaminaCost por arquétipo de arma; armor
(redução física), shield (block value), e charged effect profiles por tipo de arma.
O repo tem `WeaponDataSO` (BaseDamage, BaseCooldownSeconds, StaminaCost, Range, ArcDegrees,
DamageType, ProjectilePrefab/Speed) e `EquipmentDataSO` com bônus consumidos pelo
`DerivedStatsCalculator`, mas: não há ASPD explícito (só cooldown), não há scaling por
atributo por arma, armor não reduz dano recebido no caminho real (`PlayerManager.DamageHP`
recebe dano cru de `EnemyContactDamage`/`EnemyProjectileBehaviour`), e charged profiles
(F02) usam multiplicadores globais em vez de por arquétipo.

## Problema

Sem baselines: todas as armas do mesmo dano são intercambiáveis, armadura comprada no
Brumdar não protege nada de fato, e o charged de uma adaga é igual ao de um martelo. A
economia de gear (W06) vende itens sem efeito mecânico — quebra a promessa do jogo.

## Objetivo

Ao final desta spec, `WeaponDataSO` deve ter `AttackSpeedMultiplier`, `AttributeScaling`
e `ChargedProfile` consumidos pelo caminho de ataque (F02), e o dano recebido pelo player
deve passar por um redutor central que aplica Defense derivado (armadura) — com gerador
editor atualizando os assets de armas existentes com defaults por arquétipo.

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- WeaponDataSO/WeaponDatabaseSO; EquipmentDataSO/EquipmentManager (durabilidade FASE9H)
- DerivedStatsCalculator (Defense agregado de equipment)
- PlayerCombatStatsProvider (criado em F02)
- PlayerManager.DamageHP / RestoreHP
- EnemyContactDamage e EnemyProjectileBehaviour (fontes de dano ao player)
- Editor: CreateShopTestAssets, geradores de combat data em Editor/
Não existe:
- ASPD/scaling/charged profile por arma
- redutor central de dano recebido aplicando Defense
Auditar Fase 0:
- assets de armas existentes em Assets/_Game/Data/Combat (IDs e arquétipos reais)
```

## Engineering stories

```text
Como jogador, quero que adaga bata rápido/fraco e martelo lento/forte com o mesmo BaseDamage base.
Como comprador de armadura, quero que Defense reduza dano de contato e de projétil inimigo.
Como F02, quero ChargedProfile por arma em vez de multiplicadores globais.
```

## Escopo

```text
Inclui:
- WeaponDataSO: + AttackSpeedMultiplier (default 1), + ScalingAttribute (enum
  PlayerAttributeType existente) + ScalingFactor, + ChargedProfile {HeavyDamageMult,
  ChargedDamageMult, HeavyStaminaMult, ChargedStaminaMult, PostureMult} com defaults = F02 globais;
- PlayerCombatStatsProvider consome ASPD da arma + scaling no FinalDamage/FinalCooldown;
- PlayerDamageReceiver (NOVO, central): EnemyContactDamage/EnemyProjectileBehaviour passam por
  ele; aplica Defense derivado com fórmula da direction (redução com floor mínimo de 1);
- gerador editor "CindarsHope/Combat/Apply Weapon Mechanical Baselines": preenche os campos
  novos dos assets de arma existentes por arquétipo (sword/dagger/hammer/bow/staff);
- EditMode tests: fórmula de redução, scaling, ASPD, defaults neutros.
```

## Fora de escopo

```text
Não inclui: material tiers/modifiers completos; upgrade/repair novo (FASE9H mantém);
tooltip/comparação (F14); balance final; companion equipment.
```

## Regras de não duplicação

```text
Não criar segundo caminho de dano ao player — centralizar no PlayerDamageReceiver e
refatorar os 2 chamadores existentes para usá-lo.
Não recriar DerivedStatsCalculator/EquipmentManager.
Não criar novo database de armas.
```

## Critérios de aceite

### CA-1 Baselines por arma
- Campos novos com defaults neutros (assets antigos compilam e se comportam igual até o gerador rodar).
- Gerador aplica perfis por arquétipo e loga matriz aplicada.
- Evidência: log do gerador + validator de armas (todos os campos > 0).

### CA-2 ASPD e scaling reais
- Arma com AttackSpeedMultiplier 1.5 reduz cooldown efetivo em ~33%; ScalingFactor altera
  FinalDamage conforme atributo. Evidência: testes do provider.

### CA-3 Armadura funcional
- Com Defense derivado N, dano de contato e de projétil inimigo são reduzidos pela fórmula
  documentada (mínimo 1). Evidência: testes do PlayerDamageReceiver + log CombatLog.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Combat/Weapon/WeaponDataSO.cs        (campos novos)
Assets/_Game/Scripts/Combat/PlayerDamageReceiver.cs       (NOVO)
Assets/_Game/Scripts/Combat/PlayerCombatStatsProvider.cs  (integração ASPD/scaling)
Assets/_Game/Scripts/Editor/Combat/ApplyWeaponMechanicalBaselines.cs (NOVO)
Assets/_Game/Tests/EditMode/Core/{PlayerDamageReceiverTests, WeaponBaselineTests}.cs
```

## Contratos

### Data contracts
`WeaponDataSO` campos novos (defaults neutros; OnValidate clampa).
### Runtime contracts
`PlayerDamageReceiver.ApplyDamage(int rawDamage, DamageType type, Vector3 sourcePos, string sourceId)`.
### Event contracts
N/A (PlayerDamagedEvent existente continua publicado pelo receiver).
### Save contracts
N/A.
### UI contracts
N/A.

## Sistemas afetados

```text
Combat (dano dado/recebido), Equipment (efeito real), Editor tooling, Enemy damage sources
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Combat/Weapon/WeaponDataSO.cs
Assets/_Game/Scripts/Combat/{PlayerDamageReceiver.cs,PlayerCombatStatsProvider.cs}
Assets/_Game/Scripts/Combat/EnemyContactDamage.cs (delegar ao receiver)
Assets/_Game/Scripts/Combat/EnemyProjectileBehaviour.cs (delegar ao receiver)
Assets/_Game/Scripts/Editor/Combat/** ; Assets/_Game/Tests/EditMode/Core/**
docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
Assets/**/*.unity, *.prefab; *.asset manual (somente gerador)
Packages/**, ProjectSettings/** ; SaveManager/GameSaveData
```

## Estratégia de implementação

```md
### Fase 0 — Auditar assets de armas reais e os 2 chamadores de DamageHP.
### Fase 1 — Campos novos + provider (ASPD/scaling) + testes.
### Fase 2 — PlayerDamageReceiver + refactor dos chamadores + testes.
### Fase 3 — Gerador por arquétipo + validator.
### Fase 4 — run_strict_validation + report.
```

## Paralelização

- Parallelizable: NO
- Reason: toca o mesmo lock de F02 (caminho de ataque) e fontes de dano inimigo.

## Impacto em save/load

```text
Does this change save schema? NO (campos de SO não são save).
```

## Impacto em eventos

```text
Adds events: NO | Changes existing: NO | Requires unsubscribe: NO
```

## Impacto em UI/Unity

```text
Changes assets: YES (via gerador) | Scenes/prefabs: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: redução de dano tornar contato trivial. Mitigação: floor 1 + tabela de defesa documentada no report.
Risco: assets antigos sem campos. Mitigação: defaults neutros + gerador idempotente.
```

## Rollback

```text
Remover receiver e restaurar chamadas diretas; campos novos com default neutro não quebram assets.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar assets de armas e chamadores de DamageHP.
- [ ] T002 — Campos novos no WeaponDataSO + OnValidate.
- [ ] T003 — Provider: ASPD + scaling + charged profile por arma.
- [ ] T004 — PlayerDamageReceiver + refactor EnemyContactDamage/EnemyProjectileBehaviour.
- [ ] T005 — Gerador ApplyWeaponMechanicalBaselines + validator.
- [ ] T006 — Testes EditMode; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES
- Requires regression test: YES (dano de contato sem armadura permanece igual)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano com armadura reduzindo dano visível

## Definition of Done

```text
ASPD/scaling/charged por arma ativos; armadura reduz dano real; gerador idempotente; builds 0E; report.
```

## Anti-regressão

```text
Dano sem equipment = comportamento atual (defaults neutros).
Durabilidade FASE9H intacta. Sem GameObject.Find. Sem refs Unity em save.
```

## Notas para execução posterior

```text
F06 adiciona Material/Element tags em cima destes campos.
Material tiers completos ficam para promoção de specs equipment futuras.
```


---

## EMENDA 2026-06-12 (pós-canonização dos catálogos — VINCULANTE)

```text
1. WeaponDataSO deve ganhar os 20 CAMPOS CANÔNICOS (EQUIPMENT_MECHANICAL_BASELINES §2),
   não os 3 propostos: BaseASPD, Primary/SecondaryAttribute+pesos, BaseLight/Heavy/Charged
   StaminaCost, AttackRangeTiles, PostureDamageModifier, CritChance/DamageModifier,
   WeightClass, MaterialTagsApplied, StatusTagsApplied, AllowedAmmoType, AllowedDamageTypes,
   DefaultActionSet, ChargedEffectProfileId (defaults neutros preservam assets).
2. O gerador aplica a MATRIZ CANÔNICA §5 (8 tipos) e a tabela de materiais §16 — proibido
   inventar valores; charged effects por arma (§8-15) entram como ChargedEffectProfile.
3. ArmorDataSO/ShieldDataSO seguem os campos canônicos §3-4; baselines §25-27.
4. Fórmula de dano: a canônica §1 (AttackDamage = ...), integrada via provider F02.
```

---

## EMENDA 2026-06-13-V3 (Refinamento v3 — VINCULANTE)

> Fonte: `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md` (decisão 2.7) + re-auditoria de
> código (`reaudit-code-v3`, 2026-06-13). Em conflito com o corpo original, esta emenda vence.

### V3.1 — Expansão save-safe do enum WeaponType (lado de combate)

```text
A decisão 2.7 adiciona Hammer / Wand / Tool ao enum `WeaponType`. A re-auditoria confirmou
que o enum NÃO os tem hoje. Como esta spec é o lock canônico de weapon data/equipment
baselines (`WeaponDataSO`, geradores de combat data), a expansão do enum DEVE ser refletida
aqui, no lado de combate, de forma save-safe.

ESTADO REAL CONFIRMADO (2026-06-13):
  WeaponType em Assets/_Game/Scripts/Combat/Weapon/WeaponDataSO.cs — valores IMPLÍCITOS:
  None=0, Sword=1, Spear=2, Axe=3, Bow=4, Staff=5, Dagger=6. FALTAM Hammer/Wand/Tool.

DIREÇÃO SAVE-SAFE (obrigatória):
- Adicionar Hammer / Wand / Tool com VALORES ALTOS EXPLÍCITOS (ex.: Hammer=100, Wand=101,
  Tool=102), SEM renumerar None..Dagger. Unity serializa enum por inteiro; renumerar
  membros existentes corromperia armas já serializadas em assets/saves.
- Os arquétipos de arma desta spec (matriz canônica §5 — sword/dagger/hammer/bow/staff…)
  passam a incluir explicitamente Hammer, Wand e Tool. O gerador
  ApplyWeaponMechanicalBaselines DEVE conhecer perfis (ASPD/scaling/charged/stamina/weight)
  para os tipos novos, com defaults neutros até que a matriz §5 os cubra — proibido inventar
  números fora da direction (mesma regra da EMENDA 2026-06-12, item 2).
- `Tool` aqui é um TIPO DE ARMA (ferramenta empunhável), distinto do `ItemCategory.Tool`
  (valor 4) que já existe no lado de inventário. NÃO confundir nem unificar os dois enums.
- A CATEGORIA de item correspondente (Armor/Shield/Accessory/Relic/Essence/AnimalProduct)
  e a expansão de `ItemCategory` são tratadas na fable_32 (emenda V3.3) — fora do escopo
  desta spec, que cuida apenas do enum de TIPO de arma no domínio de combate.

EditMode test recomendado: assertar que os valores inteiros de Sword..Dagger permanecem
inalterados após a adição (guarda de regressão save-safe), e que os tipos novos
(Hammer/Wand/Tool) resolvem para um perfil de baseline válido (sem campos zerados onde a
matriz §5 exige > 0).

ESCOPO: estender o enum de forma save-safe e cobrir os tipos novos no gerador/baselines.
NÃO altera o caminho de dano recebido (PlayerDamageReceiver) nem o save schema.
```
