# SPEC — Cidade: Reconciliação dos Sistemas Duplicados de Schedule + Serviços/Licenças Vivos

> **Spec ID:** `fable_19_spec_city_services_schedule_reconciliation`
> **Status:** A implementar
> **Wave:** FABLE — Corretivas de Aderência (auditoria 00B)
> **Priority:** P1 (corretiva — duplicação ativa de sistemas)
> **Type:** Integration / Runtime
> **Domain:** City
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_corretivas
> **Can run with:** fable_04, fable_09, fable_16
> **Must not run with:** fable_11 (DEVE executar ANTES de F11 — define qual sistema de schedule vive)
> **Repo lock scope:** `City/**`, `NPC/Schedule/**`
> **Depends on:** WAVE 08 (City/), WI-25 (NPC/Schedule)
> **Blocks:** `fable_11`
> **Scope:** eleger o sistema canônico de schedule (absorvendo o outro) e ligar serviços/licenças/contratos urbanos a pelo menos 2 NPCs.
> **Out of scope:** movimento físico por blocos (F11), interiores (F11), reputação econômica completa.

required_adrs: []
required_game_rules: [city_rules.md]

---

# /speckit.specify

## Contexto

Auditoria 00B encontrou **dois sistemas de schedule paralelos**: `City/Schedule/`
(WAVE 08: NpcScheduleDefinition/Resolver/SchedulePeriod/BedDefinition) e `NPC/Schedule/`
(WI-25: NpcScheduleProfile/Block/Anchor/Service/RuntimeBootstrap) — nenhum move NPC real.
Além disso, `City/Services/` (CityServiceDefinition, ContractDefinition, LicenseDefinition,
CityServiceAvailabilityResolver) está completo e órfão: o Tovin vende "selos e formulários"
só no diálogo; nenhuma licença existe mecanicamente. Isso viola a regra de não-duplicação
do projeto e bloqueia F11 (que precisa de UM sistema para wirar).

## Problema

Duplicação é dívida composta: F11 wiraria o sistema errado ou os dois; serviços/licenças da
CITY_NPC_ROSTER direction (identidade de Tovin/Mara) não existem em gameplay; validators de
ambos os sistemas dão falsa confiança.

## Objetivo

Ao final desta spec, `NPC/Schedule/` (WI-25, mais recente e com bootstrap) deve ser o
canônico — absorvendo de `City/Schedule/` o que falta (SchedulePeriod semântico,
BedDefinition → âncora home) com crosswalk documentado e `City/Schedule/` marcado
[System.Obsolete] (remoção via delete candidates após F11); e `CityServiceAvailabilityResolver`
deve estar vivo: Tovin vende `license_market_stall` (licença mecânica exigida para 1 serviço
demonstrável) e Mara emite `contract_farm_registry` (desbloqueia 1 benefício simples) — IDs
persistidos em flags de quest existentes (QuestFlagService) para não criar seção de save.

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/specs/a_implementar/fable/fable_00B_adherence_audit_queue_triage.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe:
- City/Schedule/* (WAVE 08) e NPC/Schedule/* (WI-25) — DUPLICADOS, ambos órfãos de movimento
- City/Services/* (órfão) ; City/{FarmVisitEligibilityResolver, FarmVisitRule} (futuro social — não tocar)
- QuestFlagService (WAVE 09) — storage de flags persistido
- NpcShopController (fluxo Conversar/Comprar/Vender — ponto de venda de licença)
Auditar Fase 0:
- diferenças semânticas reais entre os dois schedules (o que City/ tem que NPC/ não tem)
- shape de CityServiceAvailabilityResolver/LicenseDefinition
```

## Escopo

```text
Inclui:
- crosswalk City/Schedule → NPC/Schedule (campos absorvidos: períodos nomeados, cama/home);
- [System.Obsolete("Absorvido por NPC/Schedule — fable_19")] em City/Schedule/* + atualização
  de validators (CityLayoutScheduleValidator passa a validar o canônico);
- CityServiceCatalog (estático): 2 serviços demonstráveis —
  license_market_stall (Tovin, 200g): exigida pelo SellAllPoint da cidade (sem licença,
  venda no ponto urbano recusa com feedback; venda a NPC continua livre);
  contract_farm_registry (Mara, 100g): +5% no preço de venda do SellPoint da fazenda
  (ShippingPriceResolver hook);
- compra via diálogo (opção no fluxo de loja dos 2 NPCs) gravando flag via QuestFlagService;
- CityServiceAvailabilityResolver ligado: resolve posse por flag;
- EditMode tests: crosswalk sem perda semântica, posse/efeito das 2 licenças, recusa sem licença.
```

## Fora de escopo

```text
Não inclui: deleção física de City/Schedule (delete candidates pós-F11); reputação;
mais serviços; UI dedicada (diálogo basta).
```

## Regras de não duplicação

```text
Resultado final: UM sistema de schedule. Serviços usam QuestFlagService — proibida seção de save nova.
```

## Critérios de aceite

### CA-1 Schedule único
- NPC/Schedule contém semântica absorvida; City/Schedule 100% Obsolete sem consumidores novos;
  builds 0E/0W (warnings de Obsolete suprimidos nos próprios arquivos absorvidos).
### CA-2 Licenças mecânicas
- Sem license_market_stall, ponto de venda urbano recusa com mensagem; com ela, vende.
- contract_farm_registry altera preço do shipping em +5% (teste no resolver).
### CA-3 Persistência por flag
- Posse sobrevive a save/load via QuestFlagService (round-trip já coberto — teste de integração).

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/NPC/Schedule/* (absorções)
Assets/_Game/Scripts/City/Schedule/* (Obsolete)
Assets/_Game/Scripts/City/Services/CityServiceCatalog.cs (NOVO estático)
Assets/_Game/Scripts/Economy/SellAllPoint.cs (gate por licença urbana)
Assets/_Game/Scripts/Farm/Shipping/ShippingPriceResolver.cs (hook contrato)
Assets/_Game/Scripts/NPC/NpcShopController.cs (opção de compra de serviço p/ Tovin/Mara)
Assets/_Game/Tests/EditMode/City/CityServicesReconciliationTests.cs
```

## Paralelização

- Parallelizable: CONDITIONAL — bloqueia F11; locks City/+NPC/Schedule.

## Impacto em save/load

```text
Schema change: NO (flags via QuestFlagService existente).
```

## Impacto em eventos / UI

```text
Adds events: NO | UI: opção de diálogo nova | Play Mode final: YES | DEFERRED_TO_FINAL_VALIDATION
```

## Riscos

```text
Risco: Obsolete gerar warnings e quebrar 0W. Mitigação: #pragma warning disable nos arquivos
absorvidos + teste de build no report.
Risco: gate de licença frustrar fluxo atual. Mitigação: só o ponto URBANO exige; fazenda livre.
```

## Rollback

```text
Remover Obsolete/catalog/gates — estado atual (duplicado e órfão) retorna.
```

# /speckit.tasks

```md
- [ ] T001 — Auditar diferenças semânticas + APIs de services.
- [ ] T002 — Absorver semântica no NPC/Schedule + crosswalk doc.
- [ ] T003 — Obsolete em City/Schedule + validators atualizados.
- [ ] T004 — CityServiceCatalog + posse por flag + resolver vivo.
- [ ] T005 — Gates (SellAllPoint urbano, ShippingPriceResolver) + compra via diálogo.
- [ ] T006 — Testes; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES | EditMode tests: YES | PlayMode/human: YES
- Regression: YES (venda urbana atual vira gated — documentar mudança intencional; venda fazenda intacta)
- Human timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum evidence for ACCEPTED: testes + cenário humano comprando licença e vendendo

## Definition of Done

```text
Um schedule canônico; 2 serviços urbanos mecânicos persistidos por flag; builds 0E/0W; report.
```

## Anti-regressão

```text
Thalindra quest flow intacto. Shipping da fazenda nunca bloqueado. Flags idempotentes.
```
