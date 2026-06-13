# SPEC — Acessórios e Relíquias: 3 Slots + Catálogo de 12 + 4 Relíquias

> **Spec ID:** `fable_23_spec_accessories_relics_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 5
> **Priority:** P2
> **Type:** Runtime / Data
> **Domain:** Inventory / Player
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_batch_5
> **Can run with:** specs que não toquem EquipmentSlot/EquipmentManager/DerivedStats (locks abaixo)
> **Must not run with:** F18, F22 (equipment/provider locks)
> **Repo lock scope:** EquipmentSlot enum, EquipmentManager, DerivedStatsCalculator inputs
> **Depends on:**
> - F18 (vitals applier — efeitos via provider)
> - F32 (itens gerados)
> **Blocks:** N/A
> **Scope:** slots Ring/Amulet/Charm + 12 acessórios + 4 relíquias funcionais via DerivedStats.
> **Out of scope:** Belt/Totem (futuros), efeitos que exijam sistemas inexistentes (detecção de trap → flag dormante).

required_adrs: []
required_game_rules: [player_rules.md]

---

# /speckit.specify

## Contexto

Decisão humana (`FABLE_DECISOES_RESPOSTAS_v1.0.md` §4): 3 slots (Ring/Amulet/Charm) +
catálogo de 12 CONFIRMADO — "confirmado e crie eles" (Q4.1) — e relíquias divinas SIM
(Q4.2: 4º tipo, raras, 1 por deus maior, drop boss/quest). Os tipos canônicos existem
na direction de equipment (EQUIPMENT_WEAPONS_ARMOR_MATERIALS PARTE F); o catálogo
nominal completo está no ITEM_CATALOG §16 (12 acessórios com BV, slot, efeito e fonte
— ex.: item_acc_ring_finan +5% ouro em vendas, item_acc_charm_stoneheart -50% knockback)
e §17 (4 relíquias: kanthor "Julgamento", kaand "Fúria", anya "Esperança", alihana
"Véu" — relíquia ocupa o slot do tipo correspondente e só 1 relíquia equipada).

A filosofia canônica do §15 governa o design: acessório nunca dá dano direto; dá a
"vida ao redor do dano" — economia, sustain, resistência, conforto. Nada disso existe
em runtime: o EquipmentSlot atual não tem slots de acessório. Esta spec adiciona os 3
slots de forma ADITIVA (fim do enum, save-safe), roteia efeitos de stats pelo
DerivedStatsCalculator/provider (F02/F18) e efeitos condicionais por hooks pontuais
nomeados (gold%, durabilidade, fadiga...), equipáveis pela tela de equipamento (F14).

## Problema

Sem os slots e o catálogo, 16 itens canônicos do ITEM_CATALOG (12 acessórios + 4
relíquias) ficam órfãos (erro de validator F30), a tela de equipamento da F14 não tem
o que exibir nos slots planejados, e a progressão horizontal do jogador (build de
conforto/economia/sustain) não existe. Implementar errado — efeitos com `if` espalhado
pelos sistemas — criaria acoplamento irreversível; e mexer no meio do enum de slots
quebraria saves existentes.

## Objetivo

Ao final desta spec, o projeto deve ter os slots Ring/Amulet/Charm funcionais no
EquipmentManager (enum aditivo, persistência pelo save de equipment existente), os 12
acessórios + 4 relíquias gerados por editor script com efeitos reais (stats via
DerivedStats; condicionais via 6 hooks pontuais nomeados), regra de não-stack e de
1-relíquia validadas no equip — permitindo builds de utilidade sem dano direto e sem
alterar o schema de save.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md (PARTE F)
docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md (§15-17)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§4)
.claude/rules/testing-quality-gate.md
.claude/skills/unity-asset-generation/SKILL.md
.claude/skills/spec-execution/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- EquipmentSlot enum (mãos/armadura — auditar valores exatos);
- EquipmentManager (equip/unequip/save de equipment);
- DerivedStatsCalculator (consome equipment — caminho canônico de stats);
- EquipmentDataSO (asset de dados de equipamento);
- geradores editor de itens (padrão F32) e entradas de loja/drop.
Não existe:
- slots de acessório, itens de acessório/relíquia, efeitos condicionais
  (gold%, loot roll extra, fadiga noturna, durabilidade, food effect, knockback resist).
Auditar Fase 0:
- enum aditivo é save-safe? (valores novos no FIM; saves antigos sem entradas);
- pontos de hook nos 6 sistemas-alvo (EconomyManager, EquipmentManager,
  EnemyLootResolver F06, fadiga F16, FoodConsumer, KnockbackController);
- disponibilidade das dependências de relíquia (F27 perfect block, F17 água viva,
  F37 calendário) — hooks dormentes se ausentes.
```

## Engineering stories

```text
Como jogador, quero equipar anel/amuleto/charm com efeitos de utilidade visíveis
  (ouro, sustain, resistência), para construir builds além do dano.
Como DerivedStatsCalculator, quero que efeitos de stats de acessórios entrem pelos
  meus inputs existentes, sem caminho paralelo de cálculo.
Como sistema condicional (economia/loot/fadiga...), quero UM hook nomeado por efeito,
  para que a remoção/auditoria do efeito seja trivial.
Como save, quero que slots novos no fim do enum carreguem saves antigos com slots
  vazios, sem migration.
```

## Escopo

```text
Inclui:
- EquipmentSlot += Ring, Amulet, Charm (fim do enum — aditivo);
- AccessoryEffectType enum + campos em EquipmentDataSO (efeito tipado + magnitude);
- efeitos roteados: stats (HP/resist/block) via DerivedStats; condicionais via hooks:
  GoldGainModifier (EconomyManager), ToolDurabilityModifier (EquipmentManager),
  ExtraLootRollChance (EnemyLootResolver F06), NightFatigueModifier (F16),
  FoodEffectModifier (FoodConsumer), KnockbackResistModifier (KnockbackController);
- regra: mesmo efeito não stacka entre slots (o maior vale); relíquia ocupa o slot do tipo
  e SÓ 1 relíquia equipada (validação no equip);
- gerador editor: 12 acessórios + 4 relíquias (ITEM_CATALOG) + entradas de loja/drop;
- relíquias: kanthor (perfect block cura 2% — hook F27), kaand (crit estende janela +0.5s),
  anya (água viva +1/dia — hook F17), alihana (sonho mensal revela calendário — hook F37);
- tela de equipamento (F14) ganha os 3 slots (emenda interna);
- EditMode tests: equip/unequip por slot, não-stack, 1-relíquia, cada hook condicional.
```

## Fora de escopo

```text
Não inclui:
- slots Belt/Totem (futuros);
- efeitos que exijam sistemas inexistentes (detecção de trap da nyx → flag dormante
  documentada);
- dano direto por acessório (PROIBIDO por regra canônica §15);
- balance fino de magnitudes (valores do ITEM_CATALOG são canônicos nesta spec);
- arte/ícones finais;
- romance/presentes (qualidade de presente é sistema futuro).
```

## Regras de não duplicação

```text
Acessório NUNCA dá dano direto (regra canônica §15).
Efeitos de stats passam pelos inputs existentes do DerivedStatsCalculator — sem segundo
  caminho de cálculo.
Efeitos condicionais: proibido if espalhado — cada hook é UM ponto único nomeado
  (GoldGainModifier, ExtraLootRollChance, ...).
Não criar segundo EquipmentManager/registry de itens — gerador segue padrão F32.
```

## Critérios de aceite

### CA-1 Slots funcionais e persistentes

- Ring/Amulet/Charm equipam/desequipam itens do tipo correto; estado persiste pelo
  save de equipment existente (sem seção nova).
- Evidência: EditMode tests de equip/unequip/round-trip por slot.

### CA-2 Efeitos mensuráveis

- Anel de Finan: +5% gold em vendas mensurável no hook GoldGainModifier; Anel de
  Alihana: +10% chance de roll extra de loot raro (teste no EnemyLootResolver).
- Evidência: EditMode tests por hook com valores sintéticos.

### CA-3 Não-stack e 1-relíquia

- 2 anéis de Finan ≠ +10% (o maior vale — não-stack por efeito); equipar 2ª relíquia
  é recusado com feedback ao jogador.
- Evidência: EditMode tests do router não-stack e da validação de relíquia.

### CA-4 Compatibilidade de save

- Saves antigos carregam com os 3 slots vazios, sem erro e sem migration (enum aditivo
  no fim).
- Evidência: EditMode test de load legado.

### CA-5 Catálogo gerado completo

- Gerador editor produz os 12 acessórios + 4 relíquias com IDs/BVs/efeitos do
  ITEM_CATALOG §16-17 + entradas de loja/drop conforme fonte declarada por item.
- Evidência: log do gerador + contagem de assets esperada vs. real (regra
  generated-asset-evidence).

---

# /speckit.plan

## Arquitetura alvo

```text
EquipmentSlot enum (valores aditivos no fim) + EquipmentDataSO (campos aditivos)
Assets/_Game/Scripts/Equipment/
  AccessoryEffectRouter.cs       (NOVO — agregação não-stack + consulta por efeito)
Hooks pontuais nomeados nos 6 sistemas-alvo (1 ponto único cada):
  EconomyManager (GoldGainModifier), EquipmentManager (ToolDurabilityModifier),
  EnemyLootResolver (ExtraLootRollChance), fadiga F16 (NightFatigueModifier),
  FoodConsumer (FoodEffectModifier), KnockbackController (KnockbackResistModifier)
Assets/_Game/Scripts/Editor/Equipment/
  GenerateAccessoriesRelics.cs   (NOVO — gerador 12+4 + lojas/drops)
Assets/_Game/Tests/EditMode/Player/
  AccessoriesTests.cs            (NOVO)
docs/validation/
  fable_23_spec_accessories_relics_runtime_execution_report.md
```

## Contratos

### Data contracts

`AccessoryEffectType` enum (efeito tipado + magnitude em EquipmentDataSO). Catálogo
canônico §16: 12 acessórios (ring_thoren, ring_finan, ring_alihana, ring_swiftcurrent,
ring_rootguard, ring_emberward, amulet_anya, amulet_kanthor, amulet_senya, amulet_nyx,
charm_thandra, charm_stoneheart) com slot/efeito/BV/fonte do catálogo. §17: 4 relíquias
(item_relic_kanthor/kaand/anya/alihana) — flag IsRelic; ocupam o slot do tipo.

### Runtime contracts

`AccessoryEffectRouter`: agrega efeitos dos 3 slots aplicando não-stack (maior valor
por AccessoryEffectType); API de consulta usada pelos hooks (ex.:
`GetModifier(AccessoryEffectType.GoldGain)`). Validação no equip: tipo de item × slot
e máximo 1 relíquia equipada. Stats (HP/resist/block) entram nos inputs do
DerivedStatsCalculator.

### Event contracts

N/A — nenhum evento novo (justificativa: efeitos são consultas síncronas nos hooks;
feedback de recusa usa o canal de feedback existente da tela de equipamento).

### Save contracts

Sem schema novo: o save de equipment existente cobre os slots novos (enum aditivo no
fim). Saves antigos = slots vazios. Sem refs Unity; sem migration.

### UI contracts

Tela de equipamento (F14) ganha os 3 slots (emenda interna da F14 — esta spec fornece
slots/validações; a tela renderiza). Feedback de recusa de 2ª relíquia.

## Sistemas afetados

```text
Equipment (enum/manager/data — aditivo)
DerivedStats (inputs de stats de acessório)
Economia (hook gold), Loot (hook roll extra), Fadiga F16, FoodConsumer,
  KnockbackController, durabilidade de ferramenta
Geradores editor de itens + lojas/drops
UI tela de equipamento (F14)
Validation reports
```

## Arquivos permitidos

```text
EquipmentSlot enum / EquipmentDataSO (aditivos)
Assets/_Game/Scripts/Equipment/AccessoryEffectRouter.cs
Hooks pontuais nos 6 sistemas-alvo (1 ponto único nomeado cada)
Assets/_Game/Scripts/Editor/Equipment/GenerateAccessoriesRelics.cs
Assets/_Game/Tests/EditMode/Player/**
docs/validation/**
csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset (edição manual de YAML — proibida; assets só via gerador
  AssetDatabase)
Packages/**
ProjectSettings/**
SaveManager core / schema de save (nenhuma seção nova)
DerivedStatsCalculator além dos inputs aditivos documentados
Sistemas-alvo além do hook pontual nomeado
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Auditar EquipmentSlot enum (valores), save de equipment (enum aditivo é save-safe?),
pontos de hook nos 6 sistemas e dependências de relíquia (F27/F17/F37 presentes?).

### Fase 1 — Slots e contratos
EquipmentSlot += Ring/Amulet/Charm (fim do enum); AccessoryEffectType + campos em
EquipmentDataSO; validações de equip (tipo×slot, 1-relíquia).

### Fase 2 — Router e hooks
AccessoryEffectRouter (não-stack) + 6 hooks pontuais nomeados + inputs de stats no
DerivedStats. Testes por hook.

### Fase 3 — Relíquias
4 relíquias com hooks F27/F17/F37 (dormentes documentados se dependência ausente);
kaand (crit estende janela +0.5s) no ponto de janela de vulnerabilidade existente.

### Fase 4 — Geração e fechamento
GenerateAccessoriesRelics (12+4 + lojas/drops) com evidência de geração; slots na tela
F14; testes; csproj; run_strict_validation; report.
```

## Paralelização

- Parallelizable: CONDITIONAL
- Parallel group: fable_batch_5
- Can run with: specs que não toquem EquipmentSlot/EquipmentManager/DerivedStats inputs
- Must not run with: F18, F22 (equipment/provider locks)
- Shared files/systems that require lock: EquipmentSlot enum, EquipmentManager,
  DerivedStatsCalculator inputs
- Reason: F18 altera o provider de vitals/derived stats e F22 altera ItemInstance/save
  de equipment — mesmos contratos centrais.

## Impacto em save/load

```text
Does this change save schema? NO (equipment save existente cobre os slots novos)
Does this add a save section? NO
Does this require migration? NO (saves antigos = slots vazios)
Does this persist Unity references? NO
Atenção: enum aditivo SOMENTE no fim (validado por teste de load legado).
```

## Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO (hooks são consultas síncronas, não assinaturas)
```

## Impacto em UI/Unity

```text
Changes UI: YES — 3 slots na tela de equipamento (F14, emenda interna)
Changes scenes: NO | Changes prefabs: NO
Changes ScriptableObjects/assets: YES — via gerador editor (AssetDatabase), nunca YAML manual
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: enum aditivo quebrar serialização de saves antigos.
Mitigação: valores novos só no FIM do enum + teste de load legado (CA-4).
Risco: efeitos condicionais virarem if espalhado.
Mitigação: 1 hook nomeado por efeito consultando o router (regra de não duplicação).
Risco: relíquias dependerem de sistemas ausentes (F27/F17/F37).
Mitigação: hooks dormentes documentados — relíquia equipável, efeito ativa quando a
  dependência existir.
Risco: stack acidental de efeitos iguais.
Mitigação: agregação não-stack centralizada no router + teste CA-3.
```

## Rollback

```text
Remover slots do enum quebraria saves COM acessórios — rollback = desativar
equipabilidade (validação de equip recusa), manter enum (documentado).
Remover router/hooks desliga efeitos; itens permanecem inertes no inventário.
Não apagar save real do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar enum/equipment save + pontos de hook + dependências de relíquia.
- [ ] T002 — Slots (fim do enum) + AccessoryEffectType + router não-stack + validações
        de equip (1-relíquia) + testes.
- [ ] T003 — 6 hooks condicionais (1 ponto único nomeado cada) + inputs DerivedStats
        + testes.
- [ ] T004 — Relíquias (4) com hooks F27/F17/F37 (dormentes se dependência ausente)
        + kaand na janela de vulnerabilidade.
- [ ] T005 — Gerador 12+4 + lojas/drops (evidência de geração); tela F14; csproj;
        run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (router não-stack, validações, hooks)
- Requires EditMode tests: YES (equip/unequip por slot, não-stack, 1-relíquia, cada
  hook condicional, load legado)
- Requires PlayMode automated or final human scenario: YES (lote)
- Requires regression test: YES (equipment atual intacto; saves antigos carregam)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano equipando
  anel/amuleto/charm com efeitos visíveis

## Definition of Done

```text
3 slots + 12 acessórios + 4 relíquias funcionais sem dano direto; não-stack e
1-relíquia validados; persistência OK (saves antigos = slots vazios); efeitos por
hooks pontuais nomeados; geração com evidência.
Builds 0E; run_strict_validation exit 0; execution report criado.
```

## Anti-regressão

```text
Acessório NUNCA dá dano direto.
Equipment atual (mãos/armadura) intacto — equip/save existentes não mudam de comportamento.
Enum de slots: nunca inserir no meio; nunca renumerar valores existentes.
Saves antigos carregam com slots vazios.
Nenhum if de efeito espalhado fora dos hooks nomeados.
Assets só por gerador editor (sem YAML manual).
```
