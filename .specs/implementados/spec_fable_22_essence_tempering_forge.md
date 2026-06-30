# SPEC — Têmpera de Essência (Forja Elemental Permanente do Brumdar)

> **Spec ID:** `fable_22_spec_essence_tempering_forge`
> **Status:** A implementar
> **Wave:** FABLE Batch 5
> **Priority:** P2
> **Type:** Runtime / Data
> **Domain:** Combat / Economy
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_batch_5
> **Can run with:** specs que não toquem equipment/itens/NpcShopController (locks abaixo)
> **Must not run with:** F03, F06, F23 (equipment/itens locks)
> **Repo lock scope:** ItemInstance/equipment save, NpcShopController (Brumdar), tags de arma
> **Depends on:**
> - F03 (MaterialTags/StatusTags em WeaponDataSO)
> - F06 (essências dropando)
> - F32 (itens de essência)
> **Blocks:** N/A
> **Scope:** infusão permanente de 1 elemento por arma via essências, na forja do Brumdar.
> **Out of scope:** têmpera de armadura, 3º tier, VFX de arte (tint de cor placeholder).

required_adrs: []
required_game_rules: [economy_rules.md, combat_rules.md]

---

# /speckit.specify

## Contexto

Decisão humana (`FABLE_DECISOES_RESPOSTAS_v1.0.md` §C3 + Q3.1): a aplicação elemental
canônica do jogo — óleos temporários (4, ITEM_CATALOG §8), flechas elementais e
materiais — ganha uma via PERMANENTE e gated: a Têmpera de Essência. As 6 essências
elementais já são itens canônicos (ITEM_CATALOG §10: fire/ice/toxic/lightning/arcane/
void, com drop de 8% nas criaturas da banda, 100% em minibosses, elites +25%) e
declaram explicitamente seu uso: "Têmpera de Essência na forja do Brumdar (versão
PERMANENTE dos óleos — fable_22)".

A regra-mãe do combate é preservada: uma tag elemental só rende bônus contra
vulnerabilidade DECLARADA do inimigo (EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER — o
matching da F06 é o único caminho de cálculo). A têmpera não cria multiplicador
próprio: apenas grava `infusion {element, tier}` no ItemInstance da arma e aplica a
StatusTag canônica correspondente, que entra no matching normalmente. O acesso é
gated por narrativa (cadeia do Brumdar concluída — flag `sq_brumdar_3_done` — e Ato 1),
conforme a conexão de quests aprovada nas decisões (§6: "têmpera atrás do Ato 1").

## Problema

Sem a têmpera, as essências (F32) são itens órfãos de uso final — violando a regra do
catálogo "item órfão é erro de validator" — e a progressão de gear perde a via
permanente prometida pelas decisões: o jogador de mid/late game não tem sink de ouro
e essências nem motivo para caçar elites/minibosses da banda. Se implementada errado
(bônus fora do adapter), quebraria a regra central de counterplay do combate.

## Objetivo

Ao final desta spec, o projeto deve ter um `TemperingService` acessível pela opção
"Temperar" no diálogo do Brumdar (gated por flag + Ato 1) que grava infusão permanente
{element, tier} no ItemInstance da arma, aplica a StatusTag canônica do elemento e
persiste no save de equipment com campos aditivos — permitindo bônus elemental
permanente APENAS contra vulnerabilidade declarada (matching F06), sem alterar óleos
(que sobrepõem temporariamente e mantêm seu nicho).

## Fontes obrigatórias lidas

```text
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§C3)
docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md (§17 tags)
docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md (§10 essências)
.claude/rules/testing-quality-gate.md
.claude/skills/combat-data-wiring/SKILL.md
.claude/skills/save-load-pattern/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- EquipmentManager com ItemInstance + durabilidade (FASE9H);
- tags por arma (F03 — MaterialTags/StatusTags em WeaponDataSO);
- essências como itens (F32): item_essence_fire/_ice/_toxic/_lightning/_arcane/_void;
- fluxo Conversar do Brumdar (NpcShopController/TownNpcDialogueLibrary);
- QuestFlagService (gates de flag/ato);
- matching tag×vulnerabilidade (F06 — único caminho de bônus).
Não existe:
- infusão (campo/conceito), UI de têmpera, persistência de infusão.
Auditar Fase 0:
- shape do ItemInstance no save de equipment (campo aditivo cabe? defaults seguros?);
- como o cálculo de dano lê as tags da arma (ponto único onde a tag de infusão entra);
- como a opção de diálogo condicional é registrada no fluxo Conversar do Brumdar.
```

## Engineering stories

```text
Como jogador, quero infundir permanentemente 1 elemento na minha arma na forja do
  Brumdar, gastando essências e ouro, para consolidar uma build elemental.
Como sistema de combate, quero que a tag de infusão entre no matching F06 como qualquer
  tag — sem multiplicador paralelo — preservando o counterplay canônico.
Como economia, quero custos e devolução determinísticos (T1/T2, metade na troca) para
  que essências e ouro tenham sink claro.
Como save, quero campos aditivos no DTO de equipment com defaults vazios, para que
  saves antigos carreguem sem infusão e sem migration.
```

## Escopo

```text
Inclui:
- TemperingService: ApplyTempering(weaponInstanceId, element, tier) — valida arma, grava
  infusion, aplica tag (FireEdge/FrostEdge/ShockEdge/PoisonEdge/ArcaneEdge/VoidEdge);
- custos (C3): T1 = 3 essências + 150g → tag + 10% chance de status no hit;
  T2 = 6 essências + 1 essence_void + 600g → tag + 20% chance;
- regras: 1 elemento por arma; re-temperar substitui DEVOLVENDO metade das essências;
  ÓLEO sobrepõe têmpera temporariamente (óleo mantém nicho); ferramentas só T1 (machado
  fire = +1 carvão por árvore; picareta ice = 10% minério duplo em fire band);
- gate: opção "Temperar" no diálogo do Brumdar só com flag sq_brumdar_3_done + Ato 1;
- save: campo aditivo no DTO de equipment (infusionElement/infusionTier, defaults vazios);
- visual: tint da arma no HUD/tooltip (cor do elemento — placeholder);
- dano: a tag entra no matching F06 normalmente (sem multiplicador extra próprio);
- EditMode tests: aplicar/substituir/devolução, gates, persistência, óleo-sobrepõe.
```

## Fora de escopo

```text
Não inclui:
- têmpera de armadura (futuro);
- 3º tier de têmpera;
- VFX/arte final (tint de cor é placeholder);
- alteração do matching F06 ou criação de multiplicadores novos;
- balance fino de custos (valores C3 são canônicos nesta spec);
- UI nova além da opção de diálogo + confirmação.
```

## Regras de não duplicação

```text
Não criar segundo sistema de tags — usa as canônicas da baseline §17/F03.
Não duplicar matching (F06 é o único caminho de bônus contra vulnerabilidade).
Não criar UI nova além da opção de diálogo + confirmação (fluxo Conversar existente).
Não criar segundo caminho de save — campos aditivos no DTO de equipment existente.
```

## Critérios de aceite

### CA-1 Tag de infusão entra no matching canônico

- Arma temperada fire aplica FireEdge; o bônus ocorre SÓ contra inimigo com
  vulnerabilidade declarada compatível (teste via matching F06); contra não-vulnerável,
  dano idêntico ao sem-têmpera.
- Evidência: EditMode tests usando o matching F06 com vulnerabilidades sintéticas.

### CA-2 Invariante de 1 elemento + devolução

- Re-temperar troca o elemento e devolve metade das essências gastas; nunca existem
  2 infusões na mesma arma (invariante validado no service).
- Evidência: EditMode tests de substituição/devolução/invariante.

### CA-3 Gate narrativo

- Opção "Temperar" indisponível antes de sq_brumdar_3_done + Ato 1; disponível depois
  (flags sintéticas em teste).
- Evidência: EditMode tests do gate.

### CA-4 Persistência aditiva

- Infusão sobrevive a save/load (round-trip); saves antigos carregam com armas sem
  infusão (defaults vazios), sem migration.
- Evidência: EditMode tests de round-trip e load legado.

### CA-5 Óleo sobrepõe temporariamente

- Aplicar óleo numa arma temperada usa a tag do óleo pela duração; expirada, a tag da
  têmpera volta a valer (óleo mantém nicho).
- Evidência: EditMode test de sobreposição/expiração.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Economy/
  TemperingService.cs            (NOVO — regras de custo/devolução/invariante/gate)
NpcShopController                (opção "Temperar" no fluxo Conversar do Brumdar)
Equipment save DTO               (campos aditivos infusionElement/infusionTier)
Ponto único de leitura de tags no cálculo de dano (tag de infusão entra com as demais)
Assets/_Game/Tests/EditMode/Economy/
  TemperingTests.cs              (NOVO)
docs/validation/
  fable_22_spec_essence_tempering_forge_execution_report.md
```

## Contratos

### Data contracts

Infusão no ItemInstance: `infusionElement` (string/enum estável: fire/ice/toxic/
lightning/arcane/void → tag FireEdge/FrostEdge/ShockEdge/PoisonEdge/ArcaneEdge/VoidEdge)
+ `infusionTier` (0=sem, 1, 2). Custos canônicos: T1 = 3 essências do elemento + 150g
(10% chance de status no hit); T2 = 6 essências + 1 item_essence_void + 600g (20%).
Ferramentas aceitam só T1 (efeitos utilitários: machado fire +1 carvão por árvore;
picareta ice 10% minério duplo em fire band).

### Runtime contracts

`TemperingService.ApplyTempering(weaponInstanceId, element, tier)`: valida tipo (arma/
ferramenta), consome custos via InventoryManager/EconomyManager, grava infusão, aplica
tag; `RemoveAndRefund` interno na substituição (devolve metade das essências).
Gate consultado via QuestFlagService (sq_brumdar_3_done + Ato 1).

### Event contracts

Novo: `WeaponTemperedEvent` (weaponInstanceId, element, tier) → toast existente.
Nenhum evento existente muda.

### Save contracts

Sem seção nova: campos aditivos no DTO de equipment existente (infusionElement/
infusionTier, defaults vazios/0). Sem migration; sem refs Unity. Load legado = arma
sem infusão.

### UI contracts

Opção "Temperar" na árvore Conversar do Brumdar + diálogo de confirmação (custos
exibidos). Tint placeholder da cor do elemento no HUD/tooltip da arma. Nenhuma tela
nova.

## Sistemas afetados

```text
Equipment (ItemInstance/save — campos aditivos)
Combate (leitura de tag no ponto único de matching F06 — sem mudar a regra)
Economia/Inventário (consumo de essências/ouro, devolução)
NPC/diálogo (opção gated no Brumdar)
Event bus (+1 evento)
Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Economy/TemperingService.cs
NpcShopController / TownNpcDialogueLibrary (opção do Brumdar apenas)
DTO de save de equipment (campos aditivos)
Ponto único de leitura de tags no cálculo de dano (integração da tag de infusão)
Assets/_Game/Tests/EditMode/Economy/**
docs/validation/**
csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset (edição manual de YAML — proibida)
Packages/**
ProjectSettings/**
Matching F06 (consumir, não alterar a regra)
WeaponDataSO/EquipmentDataSO além do necessário aditivo documentado
SaveManager core (apenas campos aditivos no DTO existente)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Auditar ItemInstance/equipment save (campo aditivo cabe?), ponto único de leitura de
tags no dano e registro de opções condicionais no Conversar do Brumdar.

### Fase 1 — Serviço e regras
TemperingService com custos C3, invariante 1-elemento, devolução de metade, ferramentas
T1, validações — tudo puro/testável.

### Fase 2 — Integração de combate
Tag de infusão entra no matching F06 pelo ponto único; chance de status por tier
(10%/20%) no hit; regra óleo-sobrepõe-têmpera por duração.

### Fase 3 — Diálogo e gate
Opção "Temperar" no Brumdar com gate sq_brumdar_3_done + Ato 1; confirmação com custos.

### Fase 4 — Persistência e fechamento
Campos aditivos no DTO + round-trip/legado; efeitos utilitários de ferramenta T1;
WeaponTemperedEvent + toast; csproj; run_strict_validation; report.
```

## Paralelização

- Parallelizable: CONDITIONAL
- Parallel group: fable_batch_5
- Can run with: specs que não toquem equipment/itens/NpcShopController
- Must not run with: F03, F06, F23 (locks de equipment/itens)
- Shared files/systems that require lock: ItemInstance/equipment save,
  NpcShopController (Brumdar), tags de arma
- Reason: F03/F06/F23 editam os mesmos contratos de equipment/tags; rodar junto gera
  conflito de DTO e de matching.

## Impacto em save/load

```text
Does this change save schema? YES — campos aditivos no DTO de equipment existente
Does this add a save section? NO
Does this require migration? NO (defaults vazios; load legado = sem infusão)
Does this persist Unity references? NO (strings/ints simples)
```

## Impacto em eventos

```text
Adds events: YES — WeaponTemperedEvent (toast)
Changes existing events: NO
Requires unsubscribe pattern: NO (publicação pontual; consumidor é o toast existente)
```

## Impacto em UI/Unity

```text
Changes UI: YES — opção de diálogo + confirmação (fluxo existente) + tint placeholder
Changes scenes: NO | Changes prefabs: NO | Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: dupla aplicação de tag (óleo + têmpera somando bônus).
Mitigação: óleo SUPRIME a têmpera pela duração (uma tag ativa por vez) — coberto por teste.
Risco: campo aditivo no DTO quebrar deserialização de saves antigos.
Mitigação: defaults vazios/0 + teste de load legado (CA-4).
Risco: têmpera criar multiplicador paralelo ao adapter.
Mitigação: integração APENAS pelo ponto único de matching F06 (regra de não duplicação).
Risco: devolução de essências permitir exploit de ouro.
Mitigação: devolve apenas metade das ESSÊNCIAS (nunca ouro) — teste de devolução.
```

## Rollback

```text
Remover TemperingService e a opção de diálogo; campos aditivos permanecem inertes no
save (defaults ignorados). Armas já temperadas perdem o efeito sem corromper save.
Não apagar save real do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar ItemInstance/equipment save + ponto único de tags + opções
        condicionais do Conversar.
- [ ] T002 — TemperingService + regras de custo/devolução/invariante + testes.
- [ ] T003 — Integração de tag com matching F06 + chance de status por tier +
        óleo-sobrepõe (testes).
- [ ] T004 — Opção "Temperar" no diálogo do Brumdar + gates por flag/Ato (testes).
- [ ] T005 — Persistência aditiva + ferramentas T1 + WeaponTemperedEvent/toast;
        csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (custos, devolução, invariante, gate, matching)
- Requires EditMode tests: YES (aplicar/substituir/devolução, gates, persistência,
  óleo-sobrepõe)
- Requires PlayMode automated or final human scenario: YES (lote)
- Requires regression test: YES (armas sem têmpera intactas; matching F06 inalterado)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano temperando e vendo
  bônus contra vulnerável

## Definition of Done

```text
Têmpera permanente funcional, gated (sq_brumdar_3_done + Ato 1) e persistida (campos
aditivos); regra do adapter intacta (sem bônus contra não-vulnerável); óleos continuam
úteis (sobrepõem temporariamente); 1 elemento por arma com devolução de metade.
Builds 0E; run_strict_validation exit 0; execution report criado.
```

## Anti-regressão

```text
Sem bônus elemental contra inimigo não-vulnerável (regra-mãe do adapter).
Armas sem têmpera comportam-se exatamente como antes.
Saves antigos carregam sem migration (defaults vazios).
Nenhuma tag nova fora das canônicas de têmpera declaradas.
Óleos não perdem o nicho (sobreposição temporária preservada).
Nenhum multiplicador de dano paralelo ao matching F06.
```
