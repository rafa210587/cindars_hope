# Execution Report — Autossuficiência da Vila: 4 NPCs (Sael / Mella / Hess / Tibbet)

**Slice:** village_economy — slice 3 (4 NPCs novos, completos)
**Data:** 2026-06-23
**Branch:** dev
**Status:** `BUILD_VALIDATED_WITH_WARNINGS` (lógica + dados + cena programática prontos; materialização de `.asset` e validadores de cena dependem de `CindarsHope/Inicializar Projeto` no Editor — Play Mode/cena DEFERRED)

---

## Objetivo

Fechar as lacunas de autossuficiência da vila identificadas em
`docs/design/gameplay/city/CITY_SELF_SUFFICIENCY_DIRECTION_v1.0.md` adicionando 4 NPCs com
papéis, propósitos e ganchos próprios, cada um **seguindo a riqueza dos demais** (mesma malha de
roster + diálogo + schedule + loja + casa que os 24 anteriores). Pedido do usuário: *"FAZ OS 4 NPCS
ANTES DE EU VALIDAR, COMPLETOS"*.

| NPC | Raça | Papel | Loja | Casa | Gancho |
|-----|------|-------|------|------|--------|
| **Sael Mare-Quieta** | Tiefling | Pescador / Comerciante | `shop_sael` (peixe, isca, peixe grelhado) | `House_Fishery` (cais SO, junto ao lago) | fonte de proteína/peixe que a vila não tinha |
| **Mella Forno-Quente** | Humana | Padeira / Moleira | `shop_mella` (pão, fornada, bolo, sopa) | `House_Bakery` (praça/mercado N) | converte grão→pão; comida pronta acessível |
| **Hess Couro-Fundo** | Draconato | Curtidor / Comerciante | `shop_hess` (couro, pele, armadura leve) | `House_Tannery` (borda L) | fecha a cadeia pele→couro→armadura |
| **Tibbet Vela-Torta** | Gnomo | Coveiro / Coroinha | — (diálogo) | ao relento, no cemitério (NO) | auxiliar de Corvus; **segredo: adora Nyx** |

Decisões do usuário aplicadas: (1) padaria entra; (2) Mirela dividida (agora só tecelã — couro saiu
para Hess); (3) Corvus ganha auxiliar gnomo (coveiro + coroinha, adorador secreto de Nyx); (4)
estábulo fica para o futuro.

---

## Mudanças por arquivo

### Dados / roster / diálogo (runtime — `Assembly-CSharp`)
- `Assets/_Game/Scripts/NPC/NpcTownRosterRegistry.cs` — +4 entradas (sael/mella/hess/tibbet) após
  Velorin; `CanonicalCount` 24 → **28**. Raças lore-consistentes; status `SceneShopRuntime` para os 3
  lojistas, `DialogueOnly` para Tibbet.
- `Assets/_Game/Scripts/NPC/TownNpcDialogueLibrary.cs` — +4 `NpcDialogueContent` ricos (saudações,
  papel, vila, serviço, conselhos, rumores, despedidas + condicionais de estação/clima/festival/
  amizade/marcos). `FriendClose` de Tibbet revela o segredo Nyx ("Eu rezo para Nyx… Não conte ao
  Padre."). `BuildNodes` gera as 13 nós por NPC (mesma estrutura dos demais).

### Cena programática (editor — `Assembly-CSharp-Editor`)
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs`:
  - **Geradores de asset idempotentes** (não há gerador automático de `NpcDataSO`/`ShopDataSO`):
    `EnsureVillageEconomyNpcAssets()` chamado em `CreateScene` logo após `EnsureChiefNpcAsset()`;
    `EnsureTownNpcAsset(path,id,name,opening,closing,shopId)` ×4 e `EnsureShopAsset(path,id,name,npcId,items[])` ×3
    (load-or-create por caminho, mesmo padrão do líder).
  - **Placement de cena:** +4 entradas em `RefinedCanonicalTownNpcSpecs` (Sael no cais SO, Mella na
    praça N, Hess na borda L, Tibbet no cemitério NO) → cada um ganha automaticamente os 3 schedule
    anchors (work/social/home) e, para os 3 lojistas, uma banca de mercado.
  - **3 casas percorríveis:** +`House_Fishery`/`House_Bakery`/`House_Tannery` em `ResidentialDefs`
    (placer por dispersão posiciona sem overlap; cada uma ganha `RoofRevealController` + `Floor`).
  - **Moradias:** +4 entradas em `TownNpcHomes` (sael→Fishery, mella→Bakery, hess→Tannery indoor;
    tibbet ao relento entre as covas).

### Testes EditMode (contagem)
- `Assets/_Game/Tests/EditMode/City/TownNpcDialogueLibraryTests.cs` — `AreEqual(28, …)`.
- `Assets/_Game/Tests/EditMode/City/DialogueConditionsPoolsTests.cs` — `AreEqual(28, …)`.

> Itens das lojas: todos os 11 IDs verificados existentes no `CanonicalItemCatalog`
> (peixes/peixe grelhado, pão/fornada/bolo/sopa, couro/pele/armadura leve).

---

## Spec Compliance Matrix

| Requisito | Implementação | Evidência |
|-----------|---------------|-----------|
| 4 NPCs completos (roster) | 4 entradas + CanonicalCount=28 | NpcTownRosterRegistry.cs:66-69,92 |
| Diálogo completo (13 nós cada) | 4 `NpcDialogueContent` + BuildNodes | TownNpcDialogueLibrary.cs; testes 28 |
| Cada um "segue a riqueza dos outros" | mesma malha roster+diálogo+schedule+loja+casa | spec placement + homes + anchors |
| 3 lojas funcionais | `EnsureShopAsset` ×3 com IDs válidos | CreateMvpTownScene.cs (Ensure*); IDs verificados |
| Schedule (≥3 anchors cada) | auto-gerado de RefinedCanonicalTownNpcSpecs | CreateNpcScheduleAnchors (work/social/home) |
| Casas físicas percorríveis | 3 ResidentialDefs → placer + RoofReveal + Floor | ResidentialDefs.cs:1063-1066 |
| Mirela só tecelã / Hess curtidor | cadeia de couro movida p/ Hess | direction doc + shop_hess |
| Tibbet auxiliar de Corvus (segredo Nyx) | DialogueOnly + FriendClose revela | TownNpcDialogueLibrary.cs |
| Docs de NPC e de imagem preenchidos | direction + ART_DIRECTION guides atualizados | docs/design/** |

---

## Validação

```text
Validation method: dotnet build por assembly (headless; Unity Editor indisponível na sessão)
Assembly-CSharp:        PASS (exit 0, 1 warning pré-existente)
Assembly-CSharp-Editor: PASS (exit 0, 3 warnings pré-existentes)
Quality check:          n/a nesta etapa (slice, não promoção)
Docs validation:        run_strict_validation falha SOMENTE por .asset de Bestiary/Items sujos
                        ANTES desta sessão (git status inicial), não tocados aqui — EXPECTED_FAIL_LEGACY_ONLY
```

### Testing Quality Gate
```text
Changed runtime code:           YES (roster, dialogue library)
Changed deterministic logic:    YES (contagem/estrutura de diálogo)
Changed Unity scene/prefab:      NO (gerador programático; .asset/.unity materializam no Editor)
Automated tests added/updated:  YES (2 testes de contagem → 28)
Automated tests command:        NOT RUN headless (EditMode roda no Unity Test Runner — DEFERRED)
Manual Play Mode scenario:      docs/validation/playmode/village_economy_four_npcs_human_test_scenario.md
Justification if no tests:      wiring de cena/asset exige Editor; lógica de diálogo coberta por testes de contagem/estrutura existentes (28)
Residual risk:                  .asset de NpcDataSO/ShopDataSO e anchors de cena só existem após `CindarsHope/Inicializar Projeto`; validadores de cena (ValidateFableCitySchedule, população) são DEFERRED até a regeneração no Editor
```

---

## Trabalho restante (não nesta slice)

- Rodar `CindarsHope/Inicializar Projeto` no Editor → materializa os 4 `Npc_*.asset`, 3 `Shop_*.asset`,
  a cena com as 3 casas + bancas + 12 anchors novos; depois rodar `ValidateFableCitySchedule` e a
  população refinada (esperado PASS — os 4 seguem o precedente Velorin: no roster/specs/diálogo, fora
  das listas estritas `ExpectedNpcs`).
- Gerar as artes dos 4 retratos + 3 prédios (specs de imagem já no `ART_DIRECTION_ILLUSTRATOR_GUIDE`).
- Slices seguintes do roadmap: 4 (cadeias de fornecedor), 5 (quadro de pedidos + reputação), 6
  (desenvolvimento da vila) — têm bifurcações de design/balance e aguardam validação humana.

## Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS`: todo o código C# (runtime + editor) compila com exit 0; dados, diálogo,
lojas, schedule e casas estão definidos e ligados pela mesma malha dos NPCs existentes. Não promovo
além disso porque a materialização dos assets e a validação de cena/Play Mode exigem o Unity Editor,
indisponível nesta sessão — reportado como DEFERRED, nunca convertido em PASS.
