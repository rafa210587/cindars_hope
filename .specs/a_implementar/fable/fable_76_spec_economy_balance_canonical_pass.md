# SPEC — Economy Balance Pass: Valores Canônicos + Convergência dos Caminhos de Preço

> **Spec ID:** `fable_76_spec_economy_balance_canonical_pass`
> **Status:** A implementar
> **Wave:** FABLE Batch 12
> **Priority:** P2
> **Type:** Runtime / Balance / Data
> **Domain:** Economy / Trade / Farm
> **Parallelizable:** YES
> **Parallel group:** fable_bloco_economy
> **Can run with:** fable_74, fable_75, fable_71
> **Must not run with:** fable_25 (NPC services — preços de serviços urbanos),
>   fable_49 (crafting tier alto — preços de gear)
> **Repo lock scope:**
>   `Assets/_Game/Scripts/Economy/Validation/EconomyBalanceValidator.cs`,
>   `Assets/_Game/Scripts/Economy/` (ShopPricingService ou equivalente),
>   `Assets/_Game/Scripts/Farm/Shipping/ShippingService.cs` (ou SellPoint),
>   `Assets/_Game/Data/Economy/` (ShopDataSO assets — só via SerializedObject/gerador),
>   `Assets/_Game/Scripts/Editor/Economy/` (gerador/validator de preços)
> **Depends on:**
>   - fable_32 (catálogo de itens — `ItemDataSO.BaseValue` por item)
>   - WAVE_INTEGRATION_07 (SellPoint/ShippingService já wired)
>   - WAVE_INTEGRATION_12C (ShopDataSO de 25 NPCs existentes)
> **Blocks:**
>   - fable_42 (curva de progressão usa gold/hora como referência de ritmo)
>   - fable_51 (contratos do Zrix na caverna — pagamentos precisam de referência balanceada)
> **Scope:** (1) definir valores canônicos de gold/hora por fonte de renda (farm, caverna,
> pesca, serviços); (2) convergir os dois caminhos de preço (SellAll via `BaseValue` vs
> `Economy/Pricing` services); (3) substituir os thresholds placeholder do
> `EconomyBalanceValidator`; (4) criar gerador/validator que valida todos os ShopDataSO
> contra a tabela canônica.
> **Out of scope:** sistema de inflação dinâmica, preços de serviços da cidade (F25),
> preços de gear endgame (F49), sistema de reputação/desconto (F57), novos itens.

required_adrs: []
required_game_rules: [economy_rules.md, farm_rules.md]

---

# /speckit.specify

## Contexto

O código documenta dois problemas de balance open:

**1. Thresholds placeholder no validator** — `EconomyBalanceValidator.cs:23,83,87`:
```csharp
// Placeholder thresholds — final tuning deferred
public float MaxSafeGoldPerHour { get; set; } = 5000f;
// 8.5 Gold/hour budget placeholder
report.Warnings.Add($"GOLD_HOUR_BUDGET: ... exceeds placeholder max=5000 — requires balance pass");
```

**2. Dois caminhos de venda paralelos** — documentado em `docs/game_rules/economy_rules.md`
como Open Question:
- **Caminho A:** `ShippingService` / `SellPoint` usa `ItemDataSO.BaseValue` como preço cheio
- **Caminho B:** `Economy/Pricing` services (ShopPricingService) aplica multiplicadores
  por categoria, reputação e promoções

Hoje, vender pela caixa de envio (farm) e vender para um NPC podem dar valores diferentes
para o mesmo item, sem lógica de jogo que justifique a diferença.

## Problema

A economy loop (farm → vender → gold → comprar/melhorar) não tem valor de referência
canônico. O validator avisa em toda rodada que está usando placeholder. O jogador pode
explorar a diferença de preços entre os dois caminhos sem que seja design intencional.
Sem valores canônicos, balance de quests (recompensas em gold) e crafting (custo de
receitas) ficam sem âncora.

## Objetivo

Ao final desta spec:

1. **Tabela canônica de gold/hora** existe em um `EconomyBalanceConfigSO` (SO de balance)
   com valores por fonte: farm normal, farm premium (fertilizante), pesca, caverna rasa,
   caverna profunda. Fonte: decisões do Refinamento v2 + curvas de F42.

2. **Convergência de caminho de preço:** `ShippingService`/`SellPoint` usa a mesma tabela
   de multiplicadores que `ShopPricingService`, ou ambos são unificados em um
   `ItemPriceResolver` único que recebe um `SellContext` (farm/npc/event).

3. **`EconomyBalanceValidator` thresholds** são lidos de `EconomyBalanceConfigSO` — sem
   mais magic numbers hardcoded.

4. **Validator de ShopDataSO** verifica que todos os 25 ShopDataSO existentes têm preços
   dentro dos bounds canônicos (warn se acima de 3× `BaseValue`, error se abaixo de 10%
   `BaseValue`).

5. **Build PASS + validate_docs exit 0.**

## Fontes obrigatórias lidas

```text
Assets/_Game/Scripts/Economy/Validation/EconomyBalanceValidator.cs
Assets/_Game/Scripts/Economy/               (ShopPricingService, ShippingService/SellPoint)
Assets/_Game/Data/Economy/                  (ShopDataSO assets — leitura)
Assets/_Game/Scripts/Farm/Shipping/         (ShippingService ou SellPoint)
docs/design/FABLE_DECISOES_RESPOSTAS_v2.0.md   (decisão 2.1/2.2 kit/baú; ritmo geral)
docs/design/FABLE_BALANCE_CURVES.md            (curvas de XP e gold referência)
docs/game_rules/economy_rules.md
docs/game_rules/farm_rules.md
.claude/rules/no-magic-balance-values.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
EconomyBalanceValidator.cs
  MaxSafeGoldPerHour = 5000f     (placeholder)
  "8.5 Gold/hour budget placeholder" (comment)
  Warnings gerados mas não bloqueantes

ShippingService / SellPoint
  Usa ItemDataSO.BaseValue × 1.0 (sem modificador)
  Publica EconomyTransactionCompletedEvent com valor

ShopPricingService (se existir) ou NpcShopController
  Aplica BuyPrice = BaseValue × BuyMultiplier
  Aplica SellPrice = BaseValue × SellMultiplier (pode diferir do Shipping)

ShopDataSO (25 assets em Data/Economy/)
  Cada um tem lista de ShopItemEntry com Price explícito
  ValidateTownShopCatalogIntegrity verifica se item existe; não verifica range de preço
```

## Design canônico de gold/hora (a confirmar no SO)

> Implementador deve ler `FABLE_BALANCE_CURVES.md` e `FABLE_DECISOES_RESPOSTAS_v2.0.md`
> para extrair os valores. Os ranges abaixo são orientação de escala:

| Fonte | Gold/hora estimado | Notas |
|-------|-------------------|-------|
| Farm básico (plantar/colher sem fertilizante) | ~80-120 | Ritmo lento, estável |
| Farm premium (fertilizante + irrigação) | ~180-250 | Requer insumos |
| Pesca (Farm pond) | ~60-100 | RNG; peixe raro multiplica |
| Pesca (Cave) | ~200-350 | Risco de caverna incluso |
| Caverna rasa (níveis 1-30) | ~150-300 | Drop + venda para Zrix |
| Caverna profunda (níveis 60+) | ~500-800 | Alta dificuldade |

`MaxSafeGoldPerHour` no SO = 800f (caverna profunda como ceiling de balance); warn acima disso.

## Tarefas

### T1 — Audit (Phase 0)

- [ ] Mapear todos os call sites de preço de venda no código (ShippingService, NpcShopController,
      SellPoint, ShopPricingService) — documentar qual usa qual fórmula
- [ ] Listar todos os 25 ShopDataSO e verificar se têm `Price` explícito ou derivado
- [ ] Ler `FABLE_BALANCE_CURVES.md` e extrair gold/hora de referência por fonte
- [ ] Criar `docs/validation/fable_76_phase0_audit_matrix.md`

### T2 — EconomyBalanceConfigSO

- [ ] Criar `Assets/_Game/Scripts/Economy/EconomyBalanceConfigSO.cs`:
  ```csharp
  [CreateAssetMenu(...)]
  public sealed class EconomyBalanceConfigSO : ScriptableObject {
      [SerializeField] public float FarmBasicGoldPerHour = 100f;
      [SerializeField] public float FarmPremiumGoldPerHour = 220f;
      [SerializeField] public float FishingFarmGoldPerHour = 80f;
      [SerializeField] public float FishingCaveGoldPerHour = 270f;
      [SerializeField] public float CaveShallowGoldPerHour = 200f;
      [SerializeField] public float CaveDeepGoldPerHour = 650f;
      [SerializeField] public float MaxSafeGoldPerHour = 800f;
      [SerializeField] public float ShippingBuybackMultiplier = 1.0f;
      [SerializeField] public float NpcSellMultiplier = 0.9f;  // NPCs pagam 10% menos
      [SerializeField] public float NpcBuyMultiplier = 1.3f;   // NPCs vendem 30% acima
  }
  ```
- [ ] Criar asset `Assets/_Game/Data/Config/EconomyBalanceConfig.asset` via
      `CreateAssetMenu` no Unity (ou gerador Editor)
- [ ] **NÃO editar YAML manual** — criar via `AssetDatabase.CreateAsset` em script Editor
      ou via menu Unity

### T3 — Convergir caminhos de preço

- [ ] Criar `ItemPriceResolver` (classe pura, testável):
  ```csharp
  public static class ItemPriceResolver {
      public static int ResolveSellingPrice(ItemDataSO item, SellContext ctx,
                                             EconomyBalanceConfigSO config);
  }
  ```
  Onde `SellContext` é `{ Shipping, NpcBuy, EventStall }`.
- [ ] `ShippingService`/`SellPoint`: usar `ItemPriceResolver.ResolveSellingPrice(ctx: Shipping)`
- [ ] `NpcShopController` venda para NPC: usar `ItemPriceResolver.ResolveSellingPrice(ctx: NpcBuy)`
- [ ] Ambos publicam `EconomyTransactionCompletedEvent` com o preço resolvido

### T4 — EconomyBalanceValidator atualizado

- [ ] Injetar `EconomyBalanceConfigSO` via `Resources.Load` ou `AssetDatabase.FindAssets`
- [ ] Substituir `MaxSafeGoldPerHour = 5000f` por `config.MaxSafeGoldPerHour`
- [ ] Remover os comments `// Placeholder thresholds` e `// 8.5 Gold/hour budget placeholder`

### T5 — Validator de range de preços nos ShopDataSO

- [ ] Extender `ValidateTownShopCatalogIntegrity` (ou criar `ValidateShopPriceRanges`):
  - Warn se `ShopItemEntry.Price > ItemDataSO.BaseValue × 3`
  - Error se `ShopItemEntry.Price < ItemDataSO.BaseValue × 0.1`
  - Usa `EconomyBalanceConfigSO` multiplicadores como referência

### T6 — EditMode tests

- [ ] `ItemPriceResolverTests.cs`:
  - shipping price = `BaseValue × ShippingBuybackMultiplier`
  - npc sell price = `BaseValue × NpcSellMultiplier`
  - npc buy price = `BaseValue × NpcBuyMultiplier`
  - preço nunca negativo
- [ ] `EconomyBalanceValidatorTests.cs`:
  - warn gerado quando gold/hora excede `MaxSafeGoldPerHour`
  - sem warn quando dentro do bound

### T7 — Gate de build

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore -clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { exit 1 }
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore -clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { exit 1 }
.\tools\docs\validate_docs.ps1
if ($LASTEXITCODE -ne 0) { exit 1 }
```

## Riscos de regressão

| Risco | Mitigação |
|-------|-----------|
| Convergir preços quebra testes existentes de ShopDataSO | Rodar `ValidateTownShopCatalogIntegrity` antes e depois; preços existentes são mantidos, só o resolver muda |
| `EconomyBalanceConfigSO` não referenciado em runtime | Bootstrap injeta via `Resources.Load("EconomyBalanceConfig")` com fallback a defaults hardcoded |
| ShopDataSO com `Price = 0` vira error no validator | Guard: `Price <= 0` → warn, não error (item grátis pode ser design intencional) |

## Critérios de aceitação

1. `EconomyBalanceConfigSO` asset existe em `Data/Config/EconomyBalanceConfig.asset`
2. `EconomyBalanceValidator` não usa mais magic numbers — lê do SO
3. `ItemPriceResolver` unifica os dois caminhos; testes passam
4. Validator de ShopDataSO emite warn/error para preços fora de range
5. Assembly-CSharp 0E/0W; validate_docs exit 0
6. Em Play Mode: vender uma cenoura pela caixa de envio e para um NPC dá preços diferentes
   mas ambos dentro do design (NPC paga menos), sem discrepância aleatória

## Stop conditions

- `FABLE_BALANCE_CURVES.md` não existe → parar e pedir ao humano
- `EconomyBalanceConfigSO` criação via YAML manual (não gerador/API) → parar
- Qualquer ShopDataSO gerado com erro no validator → reportar antes de commitar

## Report obrigatório

Criar: `docs/validation/fable_76_economy_balance_canonical_pass_execution_report.md`

```text
Testing Quality Gate
────────────────────
Changed runtime code:           YES (ItemPriceResolver, ShippingService, EconomyBalanceValidator)
Changed deterministic logic:    YES (resolução de preço)
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES
Automated tests command:        dotnet test (EditMode)
Manual Play Mode scenario:      docs/validation/fable_76_playmode_scenario.md
Justification if no tests:      N/A
Residual risk:                  valores de gold/hora são estimativas; balanço final requer
                                medição em Play Mode após F42 (curva de progressão) executada
```
