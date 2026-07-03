# SPEC â€” FarmTillingInputController com Stamina e Dia Reais

> **Spec ID:** `spec_codex_03_farm_tilling_real_params`
> **Status:** A implementar
> **Wave:** WAVE CODEX CONVERGENCE â€” Honestidade de ValidaÃ§Ã£o
> **Priority:** P1
> **Type:** Runtime
> **Domain:** Farm / Player
> **Parallelizable:** YES
> **Parallel group:** codex_convergence
> **Can run with:** spec_codex_01, spec_codex_02, spec_codex_04, spec_codex_05, spec_codex_06, spec_codex_07, spec_codex_08
> **Must not run with:** N/A
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/Runtime/FarmTillingInputController.cs`
> **Depends on:**
> - Nenhuma
> **Depends on (Depende de):**
> - Nenhuma spec deste lote. Depende apenas de sistemas existentes: `StaminaManager` (`TrySpendStamina`/`CurrentStamina`), `Core/Time/TimeManager.CurrentDay`, `FarmTilledSoilService.TillTile/WaterTile` (assinatura jÃ¡ suporta os parÃ¢metros), e o pipeline `PlayerActionFeedbackEvent` jÃ¡ usado para recusa de tile.
> **Blocks:**
> - Nenhuma
> **Blocks (Bloqueia):**
> - Nenhuma. NÃ£o bloqueia nenhuma outra spec do lote `codex_convergence`.
> **Scope:** `FarmTillingInputController` passa a checar stamina real via `StaminaManager.TrySpendStamina`/`CurrentStamina` e dia real via `TimeManager.CurrentDay`, removendo os literais fixos `staminaOk: true` e `currentDay: 1`; recusa por stamina publica `PlayerActionFeedbackEvent`.
> **Out of scope:** Alterar `FarmTilledSoilService`/`FarmPlotLogic` (a lÃ³gica de `temporarySliceMode` jÃ¡ existe e Ã© preservada); alterar o parÃ¢metro `temporarySliceMode` em si (nÃ£o faz parte do pedido â€” Ã© sobre ferramenta, nÃ£o stamina/dia).

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

Achado verificado: `FarmTillingInputController.cs` L89-92 (`TillTile`) e L102-106 (`WaterTile`) passam `staminaOk: true` fixo e `currentDay: 1` fixo, com comentÃ¡rio explÃ­cito no cÃ³digo ("temporario; integrar StaminaManager em spec futura"). Esta Ã© essa spec futura.

## 6. Problema

Hoje o player nunca Ã© bloqueado por falta de stamina ao arar/regar (mesmo com stamina zerada, a aÃ§Ã£o sempre "passa" a checagem de stamina), e toda rega registrada usa sempre "dia 1" independentemente do dia real do jogo, o que corrompe a lÃ³gica de janela de rega (`FarmWateringService.ApplyManualWatering(waterState, currentDay)`) usada para saber se a rega Ã© vÃ¡lida para o dia atual.

## 7. Objetivo

Ao final desta spec, `FarmTillingInputController` consulta `StaminaManager` real (mesmo padrÃ£o de `TreeNode.Interact()`: checar `CurrentStamina` antes, gastar via `TrySpendStamina` depois do sucesso da aÃ§Ã£o) e `TimeManager.CurrentDay` real, mantendo `temporarySliceMode: true` como estÃ¡ (fora de escopo mudar a checagem de ferramenta). Recusa por stamina insuficiente publica `PlayerActionFeedbackEvent` (mesmo pipeline jÃ¡ usado para recusa de tile).

## 8. Fontes obrigatÃ³rias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.claude/skills/player-needs-survival/SKILL.md
.claude/skills/action-feedback-pipeline/SKILL.md
.claude/skills/bootstrap-wiring/SKILL.md
```

## 9. Estado atual do repo (Phase 0 â€” auditado nesta sessÃ£o)

- `Assets/_Game/Scripts/Player/StaminaManager.cs`: API pÃºblica real â€”
  - `public int CurrentStamina { get; }`
  - `public bool TrySpendStamina(int amount)` â€” retorna `false` sem gastar se `CurrentStamina < amount`; gasta e publica `StaminaChangedEvent` se suficiente.
  - `public void AddStamina(int amount)` (nÃ£o necessÃ¡rio aqui).
  - PadrÃ£o de uso real jÃ¡ existe em `Assets/_Game/Scripts/World/TreeNode.cs` L62-83: checa `_staminaManager.CurrentStamina < chopStaminaCost` **antes** de tentar a aÃ§Ã£o (para poder recusar com feedback sem gastar recurso), executa a aÃ§Ã£o, e sÃ³ entÃ£o `_staminaManager.TrySpendStamina(chopStaminaCost)`; se o spend falhar depois da aÃ§Ã£o jÃ¡ ter mudado estado, reverte (`_inventoryManager.RemoveItem(...)`) â€” **este Ã© o precedente a seguir**.
- `Assets/_Game/Scripts/Core/Time/TimeManager.cs` L11: `public int CurrentDay { get; private set; } = 1;` â€” API pÃºblica real, sem necessidade de assinar evento; leitura direta Ã© suficiente e Ã© o padrÃ£o jÃ¡ usado por `GameCalendarService`/outros consumidores (Grep confirmou 55 arquivos usando `CurrentDay`, a maioria lendo diretamente).
- `FarmTillingInputController` hoje **nÃ£o tem referÃªncia serializada** a `StaminaManager` nem a `TimeManager` â€” precisa adicionar `[SerializeField] private StaminaManager _staminaManager;` e `[SerializeField] private CindarsHope.Core.Time.TimeManager _timeManager;` (ou obter `TimeManager` via `GameBootstrap.Instance` se esse for o padrÃ£o de acesso already-established â€” auditar `GameBootstrap.cs` na implementaÃ§Ã£o para confirmar se `TimeManager` jÃ¡ Ã© exposto lÃ¡ antes de adicionar um serialized field redundante).
- `EditorWire(SaveManager, Transform, EquipmentManager)` (L117-122) Ã© o mÃ©todo usado pelo gerador de cena para injetar dependÃªncias â€” precisa ganhar os 2 parÃ¢metros novos (com default `null` para nÃ£o quebrar chamadores existentes, seguindo o padrÃ£o jÃ¡ usado para `equipmentManager = null`).
- Custo de stamina para arar/regar: **nÃ£o existe constante hoje** para essa aÃ§Ã£o especÃ­fica â€” a rule `no-magic-balance-values` exige que isso seja uma const nomeada, nÃ£o um literal solto. Usar um valor consistente com outras aÃ§Ãµes de farm/ferramenta (`TreeNode` usa 24 para cortar Ã¡rvore; nada equivalente existe para arar/regar) â€” definir `private const int TillStaminaCost = ...` e `private const int WaterStaminaCost = ...` no prÃ³prio controller (nÃ­vel 6/7 da escada de minimalismo â€” nÃ£o criar SO de balance novo para 2 constantes locais, a menos que jÃ¡ exista um `PlayerNeedsBalanceSO`-like para farm actions; auditar `PlayerNeedsBalanceSO` antes de decidir).
- `_soilService.TillTile(...)`/`WaterTile(...)` jÃ¡ aceitam `staminaOk` e `currentDay` como parÃ¢metros â€” nenhuma mudanÃ§a de assinatura necessÃ¡ria nesses mÃ©todos (`FarmTilledSoilService.cs`), sÃ³ no caller.

## 10. User stories / engineering stories

```text
Como jogador, quero que arar/regar realmente consuma stamina, para a mecÃ¢nica de farm ter custo real.
Como jogador sem stamina suficiente, quero ver feedback claro em vez de a aÃ§Ã£o silenciosamente "passar".
Como sistema de rega, quero que o dia registrado seja o dia real do jogo, para a janela de rega funcionar corretamente.
```

## 11. Escopo

Inclui:
- Adicionar referÃªncias reais a `StaminaManager` e a fonte de dia real (`TimeManager` ou `GameBootstrap.Instance` â€” decidir na Fase 0 de implementaÃ§Ã£o conforme o padrÃ£o jÃ¡ estabelecido) em `FarmTillingInputController`.
- Definir consts de custo de stamina para arar e regar (nomeadas, nÃ£o magic numbers).
- Substituir `staminaOk: true` por checagem real antes da chamada + `TrySpendStamina` depois do sucesso, com reversÃ£o de estado se o spend falhar (seguindo o precedente de `TreeNode`) â€” auditar se `FarmTilledSoilService.TillTile`/`WaterTile` jÃ¡ encapsulam a checagem de stamina internamente (sim, via parÃ¢metro `staminaOk`) de forma que nÃ£o hÃ¡ "estado a reverter" no controller (a checagem Ã© pre-condiÃ§Ã£o, nÃ£o pÃ³s-condiÃ§Ã£o, entÃ£o o padrÃ£o de reversÃ£o do TreeNode pode nÃ£o se aplicar 1:1 â€” implementar a variante correta: checar `CurrentStamina >= cost` **antes** de chamar `TillTile`, passar `staminaOk` jÃ¡ calculado, e sÃ³ chamar `TrySpendStamina` se `TillTile`/`WaterTile` retornar `true`).
- Substituir `currentDay: 1` por `_timeManager.CurrentDay` (ou equivalente real).
- Publicar `PlayerActionFeedbackEvent` explÃ­cito quando a aÃ§Ã£o falhar por stamina insuficiente (distinto da mensagem genÃ©rica atual "Nao e possivel arar aqui").
- Atualizar `EditorWire(...)` para aceitar as novas dependÃªncias (parÃ¢metros opcionais, default null, com fallback permissivo + `Debug.LogWarning` igual ao jÃ¡ existente para `EquipmentManager` ausente).
- EditMode test (se a lÃ³gica puder ser extraÃ­da/testada sem MonoBehaviour completo â€” ex.: testar a decisÃ£o "tem stamina suficiente?" como mÃ©todo puro, ou testar `FarmTilledSoilService.TillTile`/`WaterTile` diretamente com `staminaOk` calculado a partir de valores reais).

Fora:
- Alterar `FarmTilledSoilService`, `FarmPlotLogic`, `FarmWateringService`.
- Alterar `temporarySliceMode` (fora do pedido do usuÃ¡rio).
- Criar um novo `PlayerNeedsBalanceSO` field se um jÃ¡ existir e for reusÃ¡vel â€” reusar primeiro.

## 12. Fora de escopo

```text
NÃ£o inclui: mudanÃ§a na regra de watering window; balance final dos custos de stamina; UI de feedback nova (reusa o pipeline existente).
```

## 13. Regras de nÃ£o duplicaÃ§Ã£o

```text
NÃ£o criar um segundo StaminaManager ou um wrapper â€” usar a instÃ¢ncia real via serialized ref/GameBootstrap.
NÃ£o criar um novo evento de feedback â€” reusar PlayerActionFeedbackEvent existente.
```

## 14. CritÃ©rios de aceite

### 14.1 Stamina real bloqueia a aÃ§Ã£o

- Com `StaminaManager.CurrentStamina` abaixo do custo de arar/regar, a aÃ§Ã£o falha e publica `PlayerActionFeedbackEvent` com mensagem especÃ­fica de stamina insuficiente (nÃ£o a mensagem genÃ©rica de "tile invÃ¡lido").
- Com stamina suficiente, a aÃ§Ã£o sucede e `TrySpendStamina` Ã© chamado com o custo correto.
- EvidÃªncia esperada: leitura do cÃ³digo + EditMode test cobrindo a decisÃ£o de stamina (via teste do serviÃ§o/mÃ©todo extraÃ­do, jÃ¡ que o MonoBehaviour completo nÃ£o Ã© diretamente testÃ¡vel em EditMode sem harness de scene).

### 14.2 Dia real usado na rega

- `WaterTile` Ã© chamado com `currentDay` vindo de `TimeManager.CurrentDay` real, nÃ£o mais `1` fixo.
- EvidÃªncia esperada: leitura do cÃ³digo.

### 14.3 Sem quebra de wiring existente

- `EditorWire(...)` aceita as novas dependÃªncias como parÃ¢metros opcionais; geradores de cena existentes que nÃ£o passam esses parÃ¢metros continuam funcionando (fallback com warning, como jÃ¡ ocorre para `EquipmentManager`).
- EvidÃªncia esperada: `dotnet build` PASS + Grep confirmando nenhum outro caller de `EditorWire` quebrado.

---

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Runtime/
  FarmTillingInputController.cs   (stamina real + dia real + consts de custo)

Assets/_Game/Tests/EditMode/Farm/
  FarmTillingStaminaTests.cs (novo, ou extensÃ£o de teste existente do domÃ­nio Farm)
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
N/A â€” sem novo SO nesta spec (reusar `PlayerNeedsBalanceSO` se aplicÃ¡vel, senÃ£o consts locais).

### 16.2 Runtime contracts
`FarmTillingInputController.EditorWire(SaveManager, Transform, EquipmentManager, StaminaManager = null, TimeManager = null)` â€” assinatura estendida, retrocompatÃ­vel.

### 16.3 Event contracts
Reusa `PlayerActionFeedbackEvent` existente; nenhum evento novo.

### 16.4 Save contracts
N/A.

### 16.5 UI contracts
N/A â€” feedback via pipeline de toast jÃ¡ existente.

## 17. Sistemas afetados

```text
Farm (tilling/watering input)
Player (StaminaManager, leitura)
Time (TimeManager, leitura)
Action feedback pipeline
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/Runtime/FarmTillingInputController.cs
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/**
```

## 19. Arquivos proibidos

```text
Assets/_Game/Scripts/Farm/FarmTilledSoilService.cs (leitura ok, ediÃ§Ã£o nÃ£o â€” assinatura jÃ¡ suporta o necessÃ¡rio)
Assets/_Game/Scripts/Farm/FarmPlotLogic.cs
Assets/_Game/Scripts/Editor/SceneCreation/** (a menos que a Fase 0 de implementaÃ§Ã£o confirme que o gerador de cena PRECISA passar as novas deps para nÃ£o regressar â€” se sim, autorizar ediÃ§Ã£o pontual e documentar)
Assets/**/*.unity, *.prefab, *.asset
```

## 20. EstratÃ©gia de implementaÃ§Ã£o

```md
### Fase 0 â€” Auditoria: confirmar como GameBootstrap expÃµe TimeManager (serialized ref direta vs Instance) e se PlayerNeedsBalanceSO jÃ¡ tem custo de farm action reusÃ¡vel
### Fase 1 â€” Consts de custo + referÃªncias reais
### Fase 2 â€” LÃ³gica de stamina (checar antes, gastar depois do sucesso) + dia real
### Fase 3 â€” Feedback de recusa por stamina
### Fase 4 â€” Testes/validaÃ§Ã£o
### Fase 5 â€” RelatÃ³rio
```

## 21. Ordem de execucao (ordem segura)

```text
1. Confirmar acesso real a TimeManager (serialized field vs GameBootstrap.Instance.TimeManager, se existir).
2. Confirmar se existe custo de stamina de farm action jÃ¡ definido em SO de balance.
3. Adicionar consts/campos e wiring.
4. Reescrever TillTile/WaterTile callers com stamina e dia reais.
5. Adicionar feedback de recusa por stamina.
6. Atualizar EditorWire.
7. Escrever EditMode tests da lÃ³gica de decisÃ£o.
8. dotnet build + EditMode tests.
9. Registrar relatÃ³rio.
```

## 22. ParalelizaÃ§Ã£o

```md
- Parallelizable: YES
- Parallel group: codex_convergence
- Can run with: demais specs do lote
- Must not run with: N/A
- Shared files/systems that require lock: Farm/Runtime/FarmTillingInputController.cs (lock local)
- Reason: escopo isolado; nÃ£o toca Skills/World/Quests das outras specs do lote
```

## 23. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? N/A
```

## 24. Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO
```

## 25. Impacto em UI/Unity

```text
Changes UI: NO (reusa toast/feedback existente)
Changes scenes: CONDITIONAL (sÃ³ se o gerador de cena precisar ser atualizado para wiring; documentar se ocorrer)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES (comportamento de stamina/dia Ã© gameplay real)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos tÃ©cnicos

```text
Risco: se o gerador de cena (CreateMvpFarmScene ou similar) nÃ£o for atualizado para injetar StaminaManager/TimeManager via EditorWire, o fallback permissivo (com warning) mascara o gap em produÃ§Ã£o.
MitigaÃ§Ã£o: documentar claramente no execution report se o gerador de cena nÃ£o foi atualizado nesta spec (fora do arquivo permitido a menos que autorizado); registrar como risco residual.

Risco: custo de stamina arbitrÃ¡rio sem balance real.
MitigaÃ§Ã£o: usar valor conservador documentado como "placeholder de balance, sujeito a tuning" â€” nÃ£o bloqueia a honestidade funcional da checagem.
```

## 27. Rollback

```text
Reverter FarmTillingInputController.cs para os literais staminaOk: true / currentDay: 1.
Remover testes novos.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 â€” Confirmar acesso a TimeManager real (GameBootstrap vs serialized field) e custo de stamina existente em balance SO.
- [ ] T002 â€” Adicionar consts de custo de stamina (till/water) + campos de referÃªncia.
- [ ] T003 â€” Reescrever a lÃ³gica de TillTile/WaterTile com stamina real (checar antes, gastar depois do sucesso).
- [ ] T004 â€” Usar TimeManager.CurrentDay real em WaterTile.
- [ ] T005 â€” Publicar PlayerActionFeedbackEvent especÃ­fico de stamina insuficiente.
- [ ] T006 â€” Atualizar EditorWire com os parÃ¢metros novos (opcionais, com fallback).
- [ ] T007 â€” Escrever EditMode tests da lÃ³gica de decisÃ£o de stamina/dia.
- [ ] T008 â€” Gerar execution report (citando se o gerador de cena precisa de follow-up).
```

## 29. ValidaÃ§Ãµes obrigatÃ³rias

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\validate_docs.ps1
```

Unity Test Runner â€” EditMode: rodar se praticÃ¡vel; senÃ£o `NOT RUN` com motivo.

## 30. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: YES
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (consumo real de stamina em gameplay)
- Requires regression test: YES (nÃ£o quebrar arar/regar quando StaminaManager/TimeManager ausentes â€” fallback permissivo)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: dotnet build PASS + EditMode tests da lÃ³gica de decisÃ£o PASS + cenÃ¡rio de Play Mode documentado (arar sem stamina recusa com feedback; arar com stamina gasta stamina; regar usa dia real).
```

## 31. Definition of Done

```text
FarmTillingInputController usa stamina e dia reais, sem literais fixos.
Feedback de recusa por stamina publicado.
EditorWire retrocompatÃ­vel.
EditMode tests novos passando.
Execution report criado, citando follow-up de scene wiring se necessÃ¡rio.
```

## 32. Anti-regressÃ£o

```text
NÃ£o quebrar arar/regar quando StaminaManager ou TimeManager nÃ£o estiverem wired (fallback permissivo com warning, igual ao padrÃ£o de EquipmentManager ausente).
NÃ£o alterar a assinatura de FarmTilledSoilService.TillTile/WaterTile.
NÃ£o alterar temporarySliceMode.
```

## 33. Notas para execuÃ§Ã£o posterior

```text
Se o gerador de cena de Farm nÃ£o for atualizado para wiring real de StaminaManager/TimeManager nesta spec (por estar fora dos arquivos permitidos), abrir isso como follow-up explÃ­cito no execution report â€” sem isso, o fallback permissivo mascara o gap em produÃ§Ã£o atÃ© a cena ser regenerada.
Balance de custo de stamina Ã© placeholder; tuning fica para spec de economy-balance-tuning futura.
```
