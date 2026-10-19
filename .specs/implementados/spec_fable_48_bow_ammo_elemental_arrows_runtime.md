# SPEC — Munição de Arco: Dano de Flecha, Tags Elementais e Tipos do Catálogo

> **Spec ID:** `fable_48_spec_bow_ammo_elemental_arrows_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 10
> **Priority:** P1
> **Type:** Runtime / Data / Integration
> **Domain:** Combat / Inventory
> **Parallelizable:** NO (lock em BowArrowAttackService e ItemDataSO ammo)
> **Parallel group:** N/A
> **Can run with:** specs de quest/festival que não tocam combate nem itens (F51/F52/F53)
> **Must not run with:** F02, F06, F22, F24 (pipeline de dano/tags), F32 (geradores de item), F49 (ItemDataSO/crafting)
> **Repo lock scope:** BowArrowAttackService, ProjectileSpawnRequest/pipeline de dano, ItemDataSO (campos ammo), geradores de item de munição
> **Depends on:**
> - `fable_03_spec_equipment_mechanical_baselines_runtime` (EXECUTADA — baselines de bow/derived stats)
> - `fable_32_spec_item_catalog_data_expansion` (E18 — itens `item_ammo_arrow_*` do catálogo)
> - `fable_06_spec_enemy_loot_tables_vulnerability_tags_runtime` (E19 — matching de vulnerabilidade por tag)
> **Blocks:** balance fino de archery late game; flechas raras futuras (Shock/Barbed/Arcane/Purifying)
> **Scope:** dano da flecha somado ao do arco, tag elemental da flecha entrando no matching de vulnerabilidade, 6 tipos de flecha do catálogo, prioridade/seleção de flecha equipada.
> **Out of scope:** flechas além das 6 do catálogo v1, óleos de arma (F22/consumíveis), crafting de flecha (receitas ficam com a Ozzra/F49), UI nova de munição.

required_adrs: [ADR-0007-event-bus-gameplay-communication.md]
required_game_rules: [combat_rules.md, inventory_equipment_rules.md, event_rules.md]

---

# /speckit.specify

## Contexto

O EQUIPMENT_MECHANICAL_BASELINES (§18) é taxativo: arco usa arma + flecha —
`BowDamage = BowWeaponDamage + ArrowDamage + MaterialModifier + SkillBonus` — e o §19
define a tabela canônica de arrows: cada ArrowType tem ArrowDamage próprio, DamageType e
TAGS aplicadas (ex.: SilverArrow = +4, Pierce + Silver, anti-undead/shadow SE o inimigo
declarar a vulnerabilidade; FireArrow = +3, Pierce + Fire, chance Burn baixa). A regra de
ouro do §19: "Arrow elemental só recebe bônus se inimigo tiver vulnerabilidade compatível"
— exatamente o adapter de matching que a F06 implementa para armas/óleos.

O ITEM_CATALOG (§8) fecha o roster v1 em 6 flechas vendáveis/craftáveis:
`item_ammo_arrow_wood (BV 2) · _iron (4) · _steel (6) · _silver (12) · _fire (8) ·
_frost (8)` — "stats e tags da tabela canônica de arrows".

O repo JÁ tem a metade estrutural: `BowArrowAttackService.TryFire` valida arco na outra
mão, consome 1 item de munição por tiro do slot de ammo, aplica cooldown/stamina e spawna
projétil com dano derivado (F02/F18). `ItemDataSO` tem `AmmoType` e `WeaponDataSO` tem
`AllowedAmmoType` (string). O que NÃO existe: o dano da flecha não entra na soma (o
projétil sai só com o dano do arco), as tags da flecha não viajam no projétil até o
matching de vulnerabilidade, os 6 itens de flecha do catálogo não estão autorados com
stats/tags, e esvaziar a pilha de flechas simplesmente bloqueia o tiro sem selecionar a
próxima munição compatível.

## Problema

Sem esta spec, archery é uma arma sem metade da própria fórmula: todas as flechas são
mecanicamente idênticas (dano = só arco), prata não countera undead, fogo não aplica Burn,
e a decisão de design "arrows especiais devem ser consumíveis com identidade" (§19) fica
sem dono. A F06 entrega o matching de vulnerabilidade, mas o único consumidor ranged
(flecha) não fornece tags — o counterplay elemental do arco morre antes de nascer.

## Objetivo

Ao final desta spec, atirar com arco deve: somar `ArrowDamage` do item de munição ao dano
do arco ANTES dos derived stats; carregar as tags da flecha no projétil até o ponto de
dano, onde o matching F06 concede bônus apenas contra vulnerabilidade declarada; aplicar
chance baixa de Burn/Chill nas flechas fire/frost via pipeline de status existente;
oferecer os 6 tipos do catálogo como itens com stats/tags corretos; e, ao esvaziar a pilha
equipada, selecionar deterministicamente a próxima munição compatível do inventário
(ordem canônica do catálogo) — tudo sem segundo serviço de ataque e sem tocar no consumo
1/tiro que já funciona.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md (§18-19, §28, §31)
docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md (§8 flechas)
docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md (regra do matching)
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/combat-data-wiring/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- BowArrowAttackService (valida arco na outra mão, cooldown, stamina, CONSOME 1 munição/tiro,
  spawna projétil com VisualStyle.Arrow, derived stats F02/F18 via StatsProvider);
- ItemDataSO.AmmoType + AllowedEquipmentSlots (SPEC_08) e WeaponDataSO.AllowedAmmoType,
  MaterialTagsApplied/StatusTagsApplied;
- ProjectileSpawnRequest/ProjectileSpawnService (spawn com damage/DamageType/knockback);
- pipeline de status effects (F01) e EnemyHealth/dano.
Não existe:
- ArrowDamage somado na fórmula do tiro (projétil sai só com dano do arco);
- tags da flecha viajando no projétil até o matching (F06);
- os 6 itens item_ammo_arrow_* autorados com stats/tags do §19;
- seleção automática determinística da próxima munição compatível.
Auditar Fase 0:
- shape exato do payload de dano do projétil (onde anexar tags sem quebrar magias);
- estado da F06 no momento da execução (se matching ainda não existir, gravar tags no
  payload e deixar o bônus elemental como integração documentada);
- quais item_ammo_* o gerador de itens (F32) já criou e com quais campos.
```

## Engineering stories

```text
Como jogador, quero que flecha de ferro bata mais forte que a de madeira — a munição importa.
Como jogador, quero flecha de prata punindo undead/shadow vulneráveis, e fogo aplicando Burn.
Como jogador, quero que ao acabar a pilha o arco pegue a próxima flecha compatível sem eu
abrir o inventário no meio da luta.
Como pipeline de dano, quero tags da flecha no mesmo formato das tags de arma/material,
para o matching F06 tratar tudo num ponto único.
```

## Escopo

```text
Inclui:
- ArrowBallisticsResolver (puro): item de munição → {arrowDamage, damageType, tags[],
  statusChanceId} pela tabela canônica §19, restrito aos 6 tipos do catálogo §8:
  wood +2 | iron +4 | steel +6 | silver +4 (Pierce+Silver) | fire +3 (Pierce+Fire, Burn
  chance baixa) | frost +3 (Pierce+Ice, Chill chance baixa);
- soma do dano: finalDamage usa (bowWeapon.BaseDamage + arrowDamage + bônus derivados) —
  fórmula §18 com SkillBonus/MaterialModifier já cobertos pelos derived stats F02;
- tags no projétil: campo aditivo no ProjectileSpawnRequest (ex.: AppliedTags[]) propagado
  ao evento/payload de dano; matching F06 concede bônus SÓ contra vulnerabilidade
  declarada (regra do adapter — sem bônus genérico);
- status on-hit: fire→Burn, frost→Chill com chance baixa via pipeline F01 (chance única
  canônica, definida em UM ponto no resolver);
- dados: garantir os 6 itens item_ammo_arrow_* (gerador F32 — campos AmmoType="Arrow",
  stats/tags; se o gerador já os cria, apenas reconciliar campos novos);
- compatibilidade: WeaponDataSO.AllowedAmmoType respeitado (bow aceita "Arrow"); flecha
  incompatível = erro claro existente;
- seleção/prioridade: ao consumir a última flecha equipada, auto-equipar a próxima
  munição compatível do inventário em ordem canônica determinística (wood→iron→steel→
  silver→fire→frost), publicando evento de troca (bus); sem munição = bloqueio atual;
- EditMode tests: soma da fórmula, mapa tipo→tags/status, bônus elemental só com
  vulnerabilidade compatível (alvo sintético), auto-seleção determinística, regressão do
  consumo 1/tiro e do bloqueio sem munição.
```

## Fora de escopo

```text
Não inclui:
- ShockArrow/BarbedArrow/ArcaneArrow/PurifyingArrow (§19 lista, mas catálogo v1 fecha em 6);
- receitas de craft de flecha (balcão da Ozzra — autoria de receita fica com F49/F32);
- UI de seleção de munição (auto-seleção + slot atual bastam; UI rica é F14);
- mudanças em magias/varinhas (apenas arco consome munição);
- rebalance de arcos (tabela §18 já coberta pela F03).
```

## Regras de não duplicação

```text
Não criar segundo serviço de ataque ranged — estender BowArrowAttackService existente.
Não criar segundo formato de tag — usar o MESMO vocabulário de tags de arma/material
(MaterialTagsApplied/StatusTagsApplied) que o matching F06 consome.
Não duplicar a chance de Burn/Chill — ponto único no resolver; pipeline de status F01 aplica.
Não recriar itens — gerador de itens (F32) é o dono dos assets item_ammo_arrow_*.
```

## Critérios de aceite

### CA-1 Fórmula §18 com ArrowDamage

- Tiro com arrow_iron causa exatamente (dano do tiro com arrow_wood) + 2 nas mesmas
  condições; a soma entra ANTES dos multiplicadores derivados.
- Evidência: EditMode test comparando dano final entre tipos de flecha com stats fixos.

### CA-2 Tags e matching de vulnerabilidade

- Projétil de silver carrega tag Silver; contra alvo com vulnerabilidade Silver declarada
  há bônus; contra alvo sem a vulnerabilidade NÃO há bônus algum (regra do adapter).
- Evidência: EditMode tests com alvos sintéticos (com/sem vulnerabilidade).

### CA-3 Status elemental on-hit

- FireArrow aplica Burn e FrostArrow aplica Chill com a chance canônica baixa (ponto
  único), via pipeline de status existente; flechas físicas nunca aplicam status.
- Evidência: EditMode test do resolver (mapa tipo→status/chance) + chamada do pipeline.

### CA-4 Seleção determinística de munição

- Ao consumir a última flecha equipada, a próxima compatível é auto-equipada na ordem
  canônica; sem munição nenhuma, o bloqueio NoArrowsInInventory atual permanece.
- Evidência: EditMode test de auto-seleção (inventário sintético) + regressão do bloqueio.

### CA-5 Catálogo completo e consumo preservado

- Os 6 item_ammo_arrow_* existem com BV/stats/tags do catálogo; consumo segue 1 por tiro.
- Evidência: validator/teste de presença dos 6 IDs + regressão do consumo.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Combat/
  ArrowBallisticsResolver.cs    (NOVO — puro: ammoItemId/ItemDataSO → dano/tags/status)
  BowArrowAttackService.cs      (ADITIVO — soma arrowDamage, anexa tags, auto-seleção)
Assets/_Game/Scripts/Combat/Weapon/ProjectileSpawnRequest.cs (campo aditivo AppliedTags)
pipeline de dano do projétil    (propaga tags ao payload consumido pelo matching F06)
Assets/_Game/Scripts/Core/Events/AmmoAutoSelectedEvent.cs (NOVO)
gerador de itens (F32)          (reconciliar os 6 item_ammo_arrow_* — stats/tags)
Assets/_Game/Tests/EditMode/Combat/BowAmmoElementalArrowsTests.cs (NOVO)
docs/validation/fable_48_spec_bow_ammo_elemental_arrows_runtime_execution_report.md
```

## Contratos

### Data contracts

- Tabela canônica no resolver (código, ponto único): `{arrowItemId → arrowDamage:int,
  damageType, tags:string[], statusEffectId?:string, statusChance:float}` — valores do
  §19 restritos aos 6 do §8. Itens: campos existentes (AmmoType) + stats no gerador F32.

### Runtime contracts

- `ArrowBallisticsResolver.Resolve(ItemDataSO ammo)` — puro, determinístico, fallback
  canônico (flecha desconhecida = comportamento WoodenArrow + warning log).
- `BowArrowAttackService.TryFire` — fluxo atual + soma de dano + tags no spawn request +
  auto-seleção pós-consumo (ordem canônica fixa; sem Random).

### Event contracts

- `AmmoAutoSelectedEvent(previousAmmoId, newAmmoId)` (NOVO — HUD/hotbar escutam via bus).
- Eventos de dano/status existentes inalterados (payload ganha tags aditivas).

### Save contracts

- NENHUM campo novo de save: munição é item de inventário/equipamento já persistido.

### UI contracts

- Sem tela nova. Tooltip de bow/arrow (§31) já é responsabilidade da F14/F03; o evento de
  auto-seleção permite feedback (toast existente) sem UI dedicada.

## Sistemas afetados

```text
Combat (BowArrowAttackService, projétil/payload de dano)
Inventory/Equipment (auto-seleção de munição; itens F32)
Status effects (Burn/Chill on-hit via pipeline F01)
Event bus (AmmoAutoSelectedEvent)
Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Combat/** (BowArrowAttackService aditivo, ArrowBallisticsResolver novo,
  ProjectileSpawnRequest/payload de dano — campos aditivos)
Assets/_Game/Scripts/Core/Events/AmmoAutoSelectedEvent.cs
gerador de itens do F32 (Editor — reconciliar os 6 item_ammo_arrow_*; arquivo auditado Fase 0)
Assets/_Game/Tests/EditMode/Combat/**
docs/validation/**
csproj includes (Assembly-CSharp / Assembly-CSharp-Editor)
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML (assets SÓ via gerador)
Packages/** ; ProjectSettings/**
SpellCast/magias (munição é só de arco)
EnemyHealth/matching F06 além do consumo aditivo de tags documentado
SaveManager/** (nenhum schema novo)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Payload de dano do projétil (onde anexar tags), estado da F06 (matching pronto?), itens
ammo já gerados pelo F32 e seus campos.

### Fase 1 — Resolver e fórmula
ArrowBallisticsResolver (tabela §19 × catálogo §8) + soma de arrowDamage no TryFire +
testes da fórmula e do fallback.

### Fase 2 — Tags e status
AppliedTags no ProjectileSpawnRequest → payload de dano → matching F06 (bônus só com
vulnerabilidade declarada) + Burn/Chill on-hit via pipeline F01 + testes com alvo sintético.

### Fase 3 — Dados e seleção
Reconciliar os 6 item_ammo_arrow_* no gerador F32 + auto-seleção determinística pós-consumo
+ AmmoAutoSelectedEvent + testes de seleção/regressão.

### Fase 4 — Fechamento
csproj; run_strict_validation; execution report com Spec Compliance Matrix.
```

## Paralelização

- Parallelizable: NO.
- Parallel group: N/A.
- Can run with: F51/F52/F53 (quest/festival — superfícies disjuntas).
- Must not run with: F02/F06/F22/F24 (pipeline de dano/tags), F32/F49 (ItemDataSO/geradores
  de item).
- Shared files/systems that require lock: BowArrowAttackService, ProjectileSpawnRequest,
  payload de dano, geradores de item.
- Reason: altera a superfície central de dano ranged e o vocabulário de tags que F06/F22
  também escrevem.

## Impacto em save/load

```text
Does this change save schema? NO (munição = itens já persistidos por inventário/equipment)
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: YES — AmmoAutoSelectedEvent (troca automática de munição)
Changes existing events: NO (payload de dano ganha tags aditivas com default vazio)
Requires unsubscribe pattern: NO (serviço puro; consumidores existentes já seguem o padrão)
```

## Impacto em UI/Unity

```text
Changes UI: NO (toast/feedback via evento; tooltip é F14/F03)
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: via gerador apenas (itens de munição F32)
Requires Play Mode final validation: YES (sentir o tiro com cada flecha)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: tags no payload de dano vazarem para magias/melee e inflar dano.
Mitigação: campo aditivo default vazio; só BowArrowAttackService o preenche; teste de
regressão de melee/spell sem tags.

Risco: F06 ainda não executada no momento desta spec (matching ausente).
Mitigação: tags gravadas no payload desde já; bônus elemental vira integração documentada
no report (CONTRACT_ONLY_NEEDS_INTEGRATION do critério CA-2, nunca bônus genérico).

Risco: auto-seleção equipar flecha cara (silver) sem o jogador querer.
Mitigação: ordem canônica fixa barata→cara e evento para feedback; documentado no report.

Risco: divergência entre tabela do resolver e itens gerados (F32).
Mitigação: teste que valida os 6 IDs contra a tabela do resolver (ponto único).
```

## Rollback

```text
Remover resolver + campos aditivos (default vazio) devolve o comportamento atual (dano só
do arco, sem tags). Itens de munição permanecem inertes como itens comuns. Nenhum save é
afetado.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: auditar payload de dano do projétil, estado da F06 e itens ammo do F32.
- [ ] T002 — ArrowBallisticsResolver (tabela §19 × §8) + soma de ArrowDamage no TryFire + testes.
- [ ] T003 — AppliedTags no projétil → payload de dano → matching F06 + Burn/Chill on-hit + testes.
- [ ] T004 — Reconciliar 6 item_ammo_arrow_* no gerador F32 + auto-seleção determinística + evento + testes.
- [ ] T005 — Regressões (consumo 1/tiro, bloqueio sem munição, melee/spell sem tags); csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (fórmula, mapa tipo→tags/status, auto-seleção)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final — sentir cada flecha)
- Requires regression test: YES (consumo 1/tiro; bloqueio sem munição; dano de melee/spell
  inalterado)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes EditMode + cenário humano atirando os 6
  tipos contra alvo vulnerável e não-vulnerável

## Definition of Done

```text
Fórmula §18 com ArrowDamage somado; tags da flecha no matching (bônus só contra
vulnerabilidade declarada); Burn/Chill on-hit com chance canônica em ponto único; 6 itens
do catálogo presentes com stats/tags; auto-seleção determinística com evento; consumo
1/tiro e bloqueios preservados; builds 0E; execution report criado.
```

## Anti-regressão

```text
Consumo de munição continua exatamente 1 por tiro; sem munição = NoArrowsInInventory.
Arco na outra mão continua obrigatório (ArrowRequiresBowInOtherHand intacto).
Melee/magia não ganham tags de flecha (payload default vazio testado).
Bônus elemental NUNCA sem vulnerabilidade declarada (regra do adapter).
Nenhum segundo serviço de ataque; nenhum GameObject.Find; eventos só via GameEventBus.
```
