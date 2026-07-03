# SPEC â€” Smoke de IntegraÃ§Ã£o de Boot (EditMode, sem Play Mode)

> **Spec ID:** `spec_codex_07_boot_smoke_editmode`
> **Status:** A implementar
> **Wave:** WAVE CODEX CONVERGENCE â€” Honestidade de ValidaÃ§Ã£o
> **Priority:** P2
> **Type:** Validation / Testing
> **Domain:** Core / Bootstrap
> **Parallelizable:** YES
> **Parallel group:** codex_convergence
> **Can run with:** spec_codex_01, spec_codex_02, spec_codex_03, spec_codex_04, spec_codex_05, spec_codex_06, spec_codex_08
> **Must not run with:** N/A
> **Repo lock scope:** `Assets/_Game/Tests/EditMode/Boot/**` (novo), leitura de `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs`
> **Depends on:**
> - Nenhuma
> **Depends on (Depende de):**
> - Nenhuma dependÃªncia dura. RecomendÃ¡vel rodar depois de `spec_codex_06_dead_code_removal_batch1` (nÃ£o obrigatÃ³rio): o smoke reflete a estrutura real de `GameBootstrap`, e rodar apÃ³s a remoÃ§Ã£o de cÃ³digo morto evita que o teste de reflection cite/valide managers ligados a classes que a spec_codex_06 jÃ¡ confirmou Ã³rfÃ£s.
> **Blocks:**
> - Nenhuma
> **Blocks (Bloqueia):**
> - Nenhuma. NÃ£o bloqueia nenhuma outra spec do lote `codex_convergence`.
> **Scope:** Testes EditMode estÃ¡ticos que provam, sem Play Mode, que `GameBootstrap` referencia os managers esperados via serialized fields, que os `*RuntimeBootstrap` seguem o padrÃ£o de self-wiring documentado, e que a lÃ³gica de resoluÃ§Ã£o de IDs do bootstrap nÃ£o tem nulls Ã³bvios â€” seguindo exatamente o procedimento da skill `boot-integration-smoke`.
> **Out of scope:** Testar wiring real de cena (isso Ã© Editor/batchmode, fora de EditMode puro); testar lifecycle real de `RuntimeInitializeOnLoadMethod` (sÃ³ Play Mode); criar um validador de cena novo (fora de escopo, mencionado como follow-up).

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

Quase toda spec runtime do projeto fecha em "Play Mode deferred" â€” o build compila e testes EditMode de lÃ³gica pura passam, mas nada prova que o wiring de boot (managers injetados, `*RuntimeBootstrap` conectados, subscribers assinando) realmente funciona. A skill `boot-integration-smoke` (`.claude/skills/boot-integration-smoke/SKILL.md`) jÃ¡ define exatamente o que dÃ¡ para verificar sem Play Mode e o que fica para humano â€” esta spec aplica esse procedimento pela primeira vez ao `GameBootstrap` real do projeto.

## 6. Problema

Sem nenhum smoke automatizado, um wiring quebrado no `GameBootstrap` (ex.: um serialized field esquecido, uma referÃªncia nÃ£o injetada) sÃ³ Ã© descoberto em Play Mode manual â€” caro e tardio. Reduzir esse gap com um sinal barato e repetÃ­vel diminui o que sobra para validaÃ§Ã£o humana final.

## 7. Objetivo

Ao final desta spec, existe uma suÃ­te de testes EditMode em `Assets/_Game/Tests/EditMode/Boot/` que verifica estaticamente a estrutura de `GameBootstrap` (todos os managers declarados tÃªm propriedade pÃºblica correspondente, seguindo o padrÃ£o 1:1 jÃ¡ usado) e, quando praticÃ¡vel, extrai lÃ³gica de resoluÃ§Ã£o (ex.: se houver alguma regra de "qual manager depende de qual" hoje implÃ­cita em cÃ³digo) para um mÃ©todo puro testÃ¡vel â€” sem depender de Play Mode.

## 8. Fontes obrigatÃ³rias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.claude/skills/boot-integration-smoke/SKILL.md (fonte primÃ¡ria â€” procedimento jÃ¡ definido)
.claude/skills/editmode-test-authoring/SKILL.md
.claude/skills/runtime-bootstrap-pattern/SKILL.md
.claude/rules/unity-architecture.md
```

## 9. Estado atual do repo (Phase 0 â€” auditado nesta sessÃ£o)

- `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs`: `[DisallowMultipleComponent] public class GameBootstrap : MonoBehaviour`, singleton via `public static GameBootstrap Instance => _instance;`. Confirma 24 `[SerializeField]` managers/databases (L29-53) e uma propriedade pÃºblica `=>` 1:1 para cada um (L58-80+, lista continua alÃ©m do trecho lido â€” auditar o arquivo completo na implementaÃ§Ã£o para a lista exaustiva). Exemplos confirmados: `PlayerManager`, `InventoryManager`, `TimeManager`, `GameTimeManager`, `ModalManager`, `SaveManager`, `HungerManager`, `StaminaManager`, `CraftingManager`, `EconomyManager`, `ShopManager`, `EquipmentManager`, `PlayerProgressionManager`, `StatusEffectManager`, `CaveRunManager`, `CorpseRecoveryManager` (nÃ£o serializado â€” provavelmente resolvido em cÃ³digo, auditar), `AnyaFountain`, `SkillTreeManager`, `BestiaryManager`, `ItemDatabase` (SO, nÃ£o manager).
- NÃ£o Ã© possÃ­vel instanciar um `GameBootstrap` MonoBehaviour real e popular `[SerializeField]`s em EditMode puro sem uma cena/prefab â€” portanto o teste desta spec **nÃ£o** testa "os campos estÃ£o de fato preenchidos em produÃ§Ã£o" (isso Ã© Editor/batchmode com `AssetDatabase`, explicitamente fora de escopo pela skill), mas sim **verifica a estrutura via reflection**: para cada campo `[SerializeField]` privado de tipo manager/database em `GameBootstrap`, existe uma propriedade pÃºblica correspondente que o expÃµe (usando `System.Reflection`, sem instanciar `MonoBehaviour`, sem `FindObjectOfType`). Isso pega o caso real de regressÃ£o: alguÃ©m adiciona um novo manager como campo mas esquece a propriedade pÃºblica (ou vice-versa), quebrando consumidores como `ActiveSkillExecutionController.WireInteractionSystem()` que lÃª `GameBootstrap.Instance.PlayerManager`.
- NÃ£o existem hoje testes com "Boot" no nome em `Assets/_Game/Tests/EditMode/**` (Grep confirmado: zero resultados).
- A skill `boot-integration-smoke` jÃ¡ define a saÃ­da esperada (checklist) â€” esta spec deve produzir exatamente esse checklist preenchido no execution report.

## 10. User stories / engineering stories

```text
Como agente executor, quero um teste EditMode que aponte se um manager novo foi adicionado ao GameBootstrap sem a propriedade pÃºblica correspondente, antes de descobrir isso em Play Mode.
Como maintainer, quero um sinal barato de "a estrutura do bootstrap estÃ¡ consistente" que rode em toda validaÃ§Ã£o EditMode padrÃ£o.
```

## 11. Escopo

Inclui:
- Teste EditMode via reflection: para cada `[SerializeField]` privado de `GameBootstrap` cujo tipo seja um manager/database do domÃ­nio do jogo (heurÃ­stica: nÃ£o Ã© tipo primitivo, estÃ¡ nos namespaces do jogo), existe uma propriedade pÃºblica em `GameBootstrap` do mesmo tipo (mesmo nome ou nome derivÃ¡vel do campo, seguindo a convenÃ§Ã£o `_camelCase` â†’ `PascalCase` jÃ¡ usada 1:1 no arquivo).
- Teste EditMode confirmando que `GameBootstrap.Instance` Ã© `null` fora de Play Mode (comportamento esperado do singleton estÃ¡tico, documentando a limitaÃ§Ã£o em vez de tentar contornar).
- Se a Fase 0 de implementaÃ§Ã£o encontrar lÃ³gica de wiring extraÃ­vel (ex.: alguma regra condicional sobre qual manager Ã© opcional vs obrigatÃ³rio), extrair para mÃ©todo puro e testar; se nÃ£o houver tal lÃ³gica hoje (parece ser wiring direto sem regras condicionais), documentar isso e nÃ£o inventar lÃ³gica nova.
- Preencher o checklist de saÃ­da da skill `boot-integration-smoke` no execution report.

Fora:
- Validador de cena (Editor/batchmode) para confirmar que os serialized refs estÃ£o de fato preenchidos na `TownScene`/`FarmScene`/`CaveScene` reais â€” mencionado como follow-up natural, nÃ£o implementado aqui.
- Testar `*RuntimeBootstrap` (`RuntimeInitializeOnLoadMethod`) em si â€” isso sÃ³ roda em Play Mode; fora de escopo, documentado como residual.
- Testar contrato de evento (`GameEventBus` publishâ†’subscribe) â€” nenhum evento especÃ­fico foi pedido nesta spec; se uma spec futura quiser isso, Ã© escopo novo.

## 12. Fora de escopo

```text
NÃ£o inclui: validador de cena; teste de RuntimeInitializeOnLoadMethod; novo evento de smoke.
```

## 13. Regras de nÃ£o duplicaÃ§Ã£o

```text
NÃ£o criar um segundo padrÃ£o de smoke â€” seguir exatamente o procedimento jÃ¡ documentado na skill boot-integration-smoke.
NÃ£o recriar GameBootstrap nem um bootstrap paralelo de teste â€” usar reflection sobre a classe real.
```

## 14. CritÃ©rios de aceite

### 14.1 ConsistÃªncia campoâ†”propriedade

- Para todo `[SerializeField]` de tipo manager/database em `GameBootstrap`, existe uma propriedade pÃºblica do mesmo tipo.
- EvidÃªncia esperada: teste EditMode passando, citando quantos campos foram verificados.

### 14.2 Checklist da skill preenchido

- O execution report contÃ©m o checklist de saÃ­da da skill `boot-integration-smoke` (seÃ§Ã£o "SaÃ­da esperada") totalmente preenchido, com SIM/NÃƒO APLICÃVEL justificado para cada linha.

### 14.3 Sem falso claim de Play Mode

- Nenhum teste ou doc desta spec afirma "Play Mode validado" â€” o smoke Ã© explicitamente rotulado como sinal, nÃ£o prova de gameplay (rule `validation-truth`).

---

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Tests/EditMode/Boot/
  GameBootstrapWiringSmokeTests.cs (novo)

docs/validation/
  spec_codex_07_boot_smoke_editmode_execution_report.md
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
N/A.

### 16.2 Runtime contracts
N/A â€” teste Ã© read-only via reflection sobre `GameBootstrap`, sem alterar a classe.

### 16.3 Event contracts
N/A â€” fora de escopo nesta spec.

### 16.4 Save contracts
N/A.

### 16.5 UI contracts
N/A.

## 17. Sistemas afetados

```text
Core/Bootstrap (leitura via reflection, sem ediÃ§Ã£o)
Testing (EditMode)
```

## 18. Arquivos permitidos

```text
Assets/_Game/Tests/EditMode/Boot/**
docs/validation/**
```

## 19. Arquivos proibidos

```text
Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs (leitura apenas, nenhuma ediÃ§Ã£o)
Assets/_Game/Scripts/**/*RuntimeBootstrap.cs (leitura apenas)
Assets/**/*.unity, *.prefab, *.asset
```

## 20. EstratÃ©gia de implementaÃ§Ã£o

```md
### Fase 0 â€” Auditoria (concluÃ­da nesta spec; ver seÃ§Ã£o 9) + leitura completa de GameBootstrap.cs para lista exaustiva de campos
### Fase 1 â€” Teste de reflection campoâ†”propriedade
### Fase 2 â€” Teste de Instance == null fora de Play Mode (documentaÃ§Ã£o da limitaÃ§Ã£o)
### Fase 3 â€” Preencher checklist da skill no execution report
### Fase 4 â€” RelatÃ³rio
```

## 21. Ordem de execucao (ordem segura)

```text
1. Ler GameBootstrap.cs por inteiro (lista exaustiva de SerializeFields e propriedades).
2. Escrever teste de reflection campoâ†”propriedade.
3. Escrever teste de Instance == null fora de Play Mode.
4. dotnet build + EditMode tests.
5. Preencher checklist da skill e gerar relatÃ³rio.
```

## 22. ParalelizaÃ§Ã£o

```md
- Parallelizable: YES
- Parallel group: codex_convergence
- Can run with: demais specs do lote
- Must not run with: N/A
- Shared files/systems that require lock: nenhum (sÃ³ leitura de GameBootstrap, sem ediÃ§Ã£o)
- Reason: teste novo isolado, zero ediÃ§Ã£o de cÃ³digo de produÃ§Ã£o
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
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: NO (smoke Ã© substituto parcial, nÃ£o elimina a necessidade de Play Mode final, mas nÃ£o exige um novo aqui)
Human validation timing: NOT REQUIRED
```

## 26. Riscos tÃ©cnicos

```text
Risco: reflection sobre nomes de campo pode ter falsos positivos se a convenÃ§Ã£o de nomes nÃ£o for 100% uniforme (ex.: um campo _fooManager mapeado para propriedade FooBar em vez de Foo).
MitigaÃ§Ã£o: se a Fase 0 de implementaÃ§Ã£o encontrar exceÃ§Ãµes Ã  convenÃ§Ã£o 1:1, listar explicitamente essas exceÃ§Ãµes como "known mismatches" no teste (allowlist), nÃ£o falhar o teste por elas, e documentar no report.
```

## 27. Rollback

```text
Remover a pasta Assets/_Game/Tests/EditMode/Boot/.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 â€” Ler GameBootstrap.cs por inteiro (lista exaustiva de campos/propriedades).
- [ ] T002 â€” Escrever GameBootstrapWiringSmokeTests.cs (reflection campoâ†”propriedade + Instance==null fora de Play Mode).
- [ ] T003 â€” Rodar dotnet build + EditMode tests.
- [ ] T004 â€” Preencher o checklist de saÃ­da da skill boot-integration-smoke no execution report.
- [ ] T005 â€” Gerar execution report final.
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

- Changed deterministic logic: NO (teste novo, sem lÃ³gica de produÃ§Ã£o nova)
- Requires EditMode tests: YES (Ã© o prÃ³prio entregÃ¡vel)
- Requires PlayMode automated or final human scenario: NO (explicitamente smoke, nÃ£o substitui Play Mode)
- Requires regression test: NO
- Human validation timing: NOT REQUIRED
- Minimum validation evidence for ACCEPTED: dotnet build PASS + EditMode tests novos PASS + checklist da skill preenchido no report.
```

## 31. Definition of Done

```text
GameBootstrapWiringSmokeTests.cs criado e passando.
Checklist da skill boot-integration-smoke preenchido no execution report.
Nenhum claim de Play Mode validado.
```

## 32. Anti-regressÃ£o

```text
NÃ£o alterar GameBootstrap.cs.
NÃ£o usar FindObjectOfType no teste.
NÃ£o elevar o resultado do smoke a PLAYMODE_VALIDATED.
```

## 33. Notas para execuÃ§Ã£o posterior

```text
Follow-up natural: um validador de cena (Editor/batchmode) que confirme os serialized refs preenchidos nas 3 cenas reais â€” nÃ£o implementado aqui, listado como prÃ³ximo passo.
Testar *RuntimeBootstrap e contratos de evento especÃ­ficos fica para quando uma spec citar um sistema puntual que precise disso.
```
