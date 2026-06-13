# FABLE 00B — Auditoria de Aderência (Código Real) + Triagem da Fila de Specs

> **Tipo:** auditoria/triagem — NÃO é spec executável.
> **Data:** 2026-06-12
> **Método:** leitura direta do código (grep de instanciação/consumo, leitura de classes-chave),
> não confiança em reports de wave. Cada veredicto abaixo cita a evidência.

---

## 1. Veredicto central da auditoria

As waves 02-11 produziram módulos C# corretos e testados (EditMode) que em grande parte
**nunca foram instanciados ou ligados ao loop de jogo**. "BUILD_VALIDATED" mediu compilação
e testes do módulo isolado — não aderência à direction em gameplay. O fable_00 (índice
original) classificou de bom grado várias directions como "implementadas"; esta auditoria
corrige essa classificação.

### Sistemas ÓRFÃOS confirmados (classe existe, NADA a usa em runtime)

| Sistema | Evidência | Direction traída | Spec corretiva |
|---|---|---|---|
| `DerivedStatsCalculator` | zero chamadas a `DerivedStatsCalculator.Calculate` no repo | PLAYER_DERIVED_ATTRIBUTES (bônus de equipment/passivas inertes; MaxHP/resistências nunca aplicados) | F02 (Attack/ASPD) + **F18** (vitals/resistências) |
| `RainIrrigationIntegration` (MonoBehaviour) | nenhum `AddComponent`/gerador de cena o cria | SEASONS/WEATHER ("chuva molha áreas externas") — chuva nunca rega | **F15** |
| `WeatherGenerator` | único consumidor é o órfão acima | clima inteiro fora do gameplay | **F15** |
| `FarmResourceRefreshProcessor` | construtor nunca chamado | FARM v1.3 (refresh de nós) | **F15** |
| `CropQualityResolver` + `FertilizerApplicationService` | `FarmPlot.cs` não referencia nenhum dos dois | FARM v1.3 (qualidade/fertilizante) | **F15** |
| `FatigueSystem`/`SleepRecoveryCalculator`/`FatigueState` | referenciados só por si mesmos; sem manager/cena | PLAYER_CORE (Cansaço, sono, colapso 02:00) | **F16** |
| Fonte de Anya física | zero menções a "Fonte" nos 3 geradores de cena | FARM/QUESTS (hub central: respawn, Água Viva, respec) — `FonteState`/`FonteFunctionUnlockService` (WAVE 10) sem corpo no mundo | **F17** |
| `City/Schedule/*` (WAVE 08: NpcScheduleDefinition/Resolver/SchedulePeriod) | **sistema DUPLICADO** do `NPC/Schedule/*` (WI-25); nenhum dos dois move NPC | CITY_LAYOUT (rotinas) | **F19** (reconciliar) + F11 (wiring) |
| `CityServiceDefinition/ContractDefinition/LicenseDefinition` (WAVE 08) | não referenciados por NpcShopController/town runtime | CITY_NPC_ROSTER (serviços/licenças/contratos) | **F19** |

### Confirmados FUNCIONANDO de verdade (amostra verificada)

```text
ShopManager: assina DayStartedEvent (restock diário ativo).
Quest system (WI-15/18/26): registry→service→bridge→save, em uso real.
Cave runtime (cave_001-008 + slice): materializa, estável por seed.
TimeManager/DayStarted, Hunger/Stamina/Mana managers, ModalManager: em uso real.
FarmPlot plantar/regar/colher; FishingSpot; resource interactables; SellPoint.
EnemyBrain/spawn planner/boss gates; skill tree + 21 executores reais.
GameCalendarService/LunarCycleService: existem como serviço; consumo de gameplay parcial
  (estação/lua não afetam crops/NPCs — coberto por F15 e specs futuras 15_).
```

### Implicação de processo

```text
"BUILD_VALIDATED" ≠ "integrado". Toda spec fable exige seção 'Estado atual do repo' com
auditoria de INSTANCIAÇÃO (quem cria? quem chama?), não só existência de arquivo.
As corretivas F15-F19 são specs de WIRING, não de criação de sistema.
```

---

## 2. Specs corretivas geradas (este lote)

| # | Spec | Fecha |
|---|---|---|
| F15 | `fable_15_spec_world_weather_farm_orphan_systems_wiring.md` | clima/chuva/refresh/qualidade/fertilizante ligados ao loop |
| F16 | `fable_16_spec_player_fatigue_sleep_collapse_wiring.md` | fadiga/sono/colapso 02:00 + cama na fazenda |
| F17 | `fable_17_spec_fonte_anya_physical_interactable_runtime.md` | Fonte física na fazenda + funções por fragmento + respawn |
| F18 | `fable_18_spec_derived_stats_vitals_application_runtime.md` | MaxHP/Stamina/Mana/regen/resistências derivados aplicados |
| F19 | `fable_19_spec_city_services_schedule_reconciliation.md` | dedup dos 2 sistemas de schedule + serviços/licenças urbanos |
| F20 | `fable_20_spec_calendar_clock_hud_day_detail_ui.md` | refatoração densa da 02_spec_calendar_ui (única pendência real da fila) |

Ordem recomendada atualizada: F15 → F16/F18 → F17 → F19 (F20 junto do F14).

---

## 3. Triagem da fila `docs/specs/a_implementar/` (nível raiz)

Classificação por leitura do registry + reports + código:

### Classe A — EXECUTADAS (BUILD_VALIDATED, waves 00-12 + integrações + FIX/test-harness)

~92 arquivos `NN_spec_*`/`spec_wave_integration_*`/`spec_test_harness_*`/`FIX_001_*`.
Estado real: código entregue (com os gaps de wiring desta auditoria); aguardam apenas
validação humana final (Phase 2-3). **Não são fila de trabalho** — mantê-las no nível raiz
de `a_implementar/` é a principal "bagunça" do projeto e induz agentes a re-executá-las.

**Ação executada:** movidas (não deletadas) para
`docs/specs/a_implementar/executadas_build_validated/` com README explicando o estado.
Promoção a `implementados/` continua exigindo evidência Phase 2-3 (regra inalterada).
Bônus: remove 6 erros de docs validation (FIX_001 sem prefixo; test_harness sem headers).

### Classe B — PENDENTE REAL (nunca executada)

| Spec | Avaliação de profundidade | Destino |
|---|---|---|
| `02_spec_calendar_ui_weather_lunar_display.md` | rasa para o estado atual (pré-WI-23; não cita GameplayHudCanvas/ModalManager reais) | refatorada como **F20**; original movida para `executadas_build_validated/_absorvidas_pelo_fable/` |

### Classe C — Avaliação de profundidade das specs antigas (por que refatorar em vez de reusar)

As specs das waves (formato batch 36) declaram fontes e critérios, mas:

```text
1. 'Estado atual do repo' ficou congelado em 2026-06-07 — não refletem WI-13..26 nem o slice
   2026-06-12 (ex.: specs de UI não citam ModalType reais; specs de cave não citam materializer atual).
2. Não exigem auditoria de instanciação — raiz do problema sistêmico da seção 1.
3. Não declaram paralelização/locks no padrão atual do template.
4. Duplicatas literais: 03_spec_quest_* ≈ 09_spec_quest_* (mesmos títulos, duas waves).
As fable_01-20 substituem a função de "fila futura" com estado de repo real e locks.
```

---

## 4. Limpeza executada e candidatos a deleção

```text
EXECUTADO (não destrutivo):
- Classe A movida para executadas_build_validated/ (+README).
- 10 reports antigos de validação ganharam campos validated_adrs/validated_game_rules
  (zera 20 erros de docs validation).
- 2 specs implementadas (fase9h/fase9i) reescritas para citar ADR/game_rule em vez de
  amendment como canônico (zera 2 erros).
- Meta: validate_docs.ps1 exit 0 pela primeira vez.

CANDIDATOS A DELEÇÃO (adicionados a DOCUMENT_DELETE_CANDIDATES.md — exigem confirmação
humana explícita "delete this candidate" por regra):
- docs/validation/playmode (ARQUIVO solto sem extensão; cópia preservada em _templates/)
- docs/specs/a_implementar/executadas_build_validated/FIX_001_runtime_warnings_town_shop_catalog_alignment.md
  (FIX-001/001B já executados e reportados; spec stale)
- duplicatas 09_spec_quest_* OU 03_spec_quest_* (uma das séries; crosswalk no README da pasta)
```
