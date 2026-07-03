# SPEC — Remoção de Código Morto (Lote 2: 15 arquivos + 4 eventos)

> **Spec ID:** `spec_codex_12_dead_code_removal_batch2`
> **Status:** A implementar
> **Wave:** WAVE CODEX CONVERGENCE — Honestidade de Validação (Lote 2)
> **Priority:** P2
> **Type:** Runtime / Cleanup
> **Domain:** Cross-cutting (Farm, Quests, UI, World, City, Loot, Core, Save, Cave)
> **Parallelizable:** YES
> **Parallel group:** codex_convergence_lote2
> **Can run with:** spec_codex_09, spec_codex_10, spec_codex_11, spec_codex_13
> **Must not run with:** N/A
> **Repo lock scope:** os 15 arquivos listados na seção 9 + seus `.meta`, os 4 eventos listados na seção 9 + seus `.meta` (quando em arquivo próprio), `Assets/_Game/Scripts/Editor/Validation/ValidateSpec11Damage.cs`
> **Depends on (Depende de):**
> - Nenhuma spec deste lote.
> **Blocks (Bloqueia):**
> - Nenhuma outra spec do lote `codex_convergence_lote2`.
> **Scope:** Deletar 15 arquivos de código morto (zero referências confirmadas por Grep de nome de classe + GUID do `.meta` em `.unity`/`.prefab`/`.asset` + varredura de `RuntimeInitializeOnLoadMethod`) e 4 eventos mortos do GameEventBus (sem publish nem subscribe), ajustando `ValidateSpec11Damage.cs` na mesma spec para não quebrar por causa da remoção de `StatusTickedEvent`.
> **Out of scope:** Remover qualquer arquivo não listado nesta spec, mesmo que pareça morto — precisa de re-verificação Grep própria; qualquer arquivo de `Combat/StatusEffect/StatusEffectManager.cs` (confirmado EM USO real via `EnemyHealth` na auditoria fable_00B, não incluir); qualquer arquivo de `City/Schedule/SchedulePeriod.cs` (confirmado vocabulário ativo, não incluir).

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

Auditoria de convergência (Grep de nome de classe + GUID do `.meta` correspondente em todo `.unity`/`.prefab`/`.asset` do projeto + varredura de `RuntimeInitializeOnLoadMethod` para excluir auto-bootstrap invisível) confirmou 15 arquivos de classe e 4 eventos do `GameEventBus` sem nenhuma referência ativa — nem instanciação em cena/prefab/asset, nem subscribe, nem publish, nem auto-bootstrap.

Esta é uma continuação do lote 1 de remoção de código morto (`spec_codex_06_dead_code_removal_batch1.md`, que já removeu 11 arquivos e excluiu 2 falsos-positivos do pedido original: `Combat/StatusEffect/StatusEffectManager.cs` está em uso real via `EnemyHealth`, e `City/Schedule/SchedulePeriod.cs` é vocabulário ativo — só `NpcScheduleResolver`/`NpcScheduleDefinition` eram obsoletos-mas-testados naquele lote). Esta spec 2 aplica o mesmo protocolo de verificação a um segundo conjunto de 15 arquivos + 4 eventos.

## 6. Problema

Classes e eventos sem nenhuma referência ativa aumentam a superfície de manutenção, confundem buscas (Grep/agentes assumindo que algo "existe e funciona" só porque compila), e um dos eventos mortos (`StatusTickedEvent`) tem um validator (`ValidateSpec11Damage.cs`) que checa a presença da string no source — se o evento for removido sem ajustar o validator, o validator vai falsamente reportar erro (ou, pior, checar por uma string que não existe mais e nunca mais reportar corretamente o que pretendia validar).

## 7. Objetivo

Ao final desta spec, os 15 arquivos e os 4 eventos mortos são deletados (arquivo + `.meta`), `ValidateSpec11Damage.cs` é ajustado para não depender da string `StatusTickedEvent` (removendo a checagem ou o item do checklist, decisão de Fase 0 conforme o que o validator realmente precisa verificar), e o build permanece limpo (0 erros / 0 warnings novos).

## 8. Fontes obrigatórias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.claude/rules/docs-governance.md (delete guard)
.specs/a_implementar/spec_codex_06_dead_code_removal_batch1.md (protocolo de verificação a replicar — ler antes de executar)
```

## 9. Estado atual do repo (Phase 0 — auditado nesta sessão)

**15 arquivos confirmados sem referência ativa (Grep de classe + GUID .meta em .unity/.prefab/.asset + zero RuntimeInitializeOnLoadMethod de auto-bootstrap):**

```text
Assets/_Game/Scripts/Farm/Lots/FarmLotSignInteractable.cs
Assets/_Game/Scripts/Quests/QuestConditionService.cs   (e ConditionDefinition/QuestContext no mesmo arquivo, se existirem — confirmar em Fase 0)
Assets/_Game/Scripts/UI/Cave/CaveCheckpointSideMenuController.cs
Assets/_Game/Scripts/UI/Notification/NotificationToastController.cs
Assets/_Game/Scripts/UI/Runtime/ClockCalendarHudWidgetView.cs
Assets/_Game/Scripts/World/Altars/GodMarkAltarInteractable.cs
Assets/_Game/Scripts/World/WorldTimeProvider.cs
Assets/_Game/Scripts/Cave/Loot/CaveTreasureProfile.cs
Assets/_Game/Scripts/Cave/Runtime/CaveConfinementValidator.cs
Assets/_Game/Scripts/Cave/Validation/CaveBossGateValidator.cs
Assets/_Game/Scripts/City/Layout/CityZoneDefinition.cs
Assets/_Game/Scripts/Core/Progression/LevelUpManager.cs
Assets/_Game/Scripts/Farm/Harvest/HarvestCommand.cs
Assets/_Game/Scripts/Loot/RewardTableDefinition.cs
Assets/_Game/Scripts/Save/SaveSectionOwnershipRegistry.cs
```

Todos os 15 arquivos foram confirmados existentes por Grep de definição de classe nesta sessão.

**4 eventos mortos (sem publish nem subscribe em nenhum lugar do código):**

```text
PauseClosedEvent       — Assets/_Game/Scripts/Core/Events/UIEvents.cs:5 (struct { }; arquivo compartilhado com outros eventos — remover só o struct, não o arquivo)
EquipmentUpgradedEvent — Assets/_Game/Scripts/Core/Events/EquipmentUpgradedEvent.cs (arquivo próprio — remover arquivo inteiro)
SpellCastFailedEvent   — Assets/_Game/Scripts/Core/Events/PlayerCombatEvents.cs:31 (arquivo compartilhado — remover só a definição)
StatusTickedEvent      — Assets/_Game/Scripts/Core/Events/StatusAndDamageEvents.cs:33 (arquivo compartilhado — remover só a definição)
```

**Atenção especial — `StatusTickedEvent` e `ValidateSpec11Damage.cs`:**

`Assets/_Game/Scripts/Editor/Validation/ValidateSpec11Damage.cs:62` tem a checagem:

```csharp
if (script != null && script.text.Contains("class DamageAppliedEvent") && script.text.Contains("StatusAppliedEvent") && script.text.Contains("StatusTickedEvent"))
```

Esta linha checa, via leitura de texto do source (não reflexão), se um determinado script contém as 3 strings simultaneamente — provavelmente para confirmar que um arquivo de eventos de dano/status está completo/presente. Ao remover `StatusTickedEvent`, esta condição nunca mais será satisfeita, fazendo o validator reportar incorretamente (falso negativo/falso positivo, a depender do que a condição controla — Fase 0 deve ler o método completo em volta da L62 para entender o efeito exato antes de decidir). A correção correta é: remover `&& script.text.Contains("StatusTickedEvent")` da condição (ou o bloco de checagem inteiro, se ele só existir para essa validação específica), documentando a mudança no relatório.

**3 arquivos são de Cave** (`CaveTreasureProfile.cs`, `CaveConfinementValidator.cs`, `CaveBossGateValidator.cs`) — confirmado que nenhum tem referência ativa, portanto a remoção não afeta o stable-run contract (rule `cave-stable-run`): não há estado runtime de cave sendo alterado, apenas classes não instanciadas sendo removidas. Citar a rule no relatório mesmo assim, por serem arquivos sob `Cave/`.

## 10. User stories / engineering stories

```text
Como desenvolvedor, quero que o código morto confirmado seja removido para reduzir ruído de manutenção e falsos precedentes de arquitetura.
Como validator de dano, quero continuar funcionando corretamente após a remoção de um evento morto que eu checava.
```

## 11. Escopo

Inclui:
- Re-verificação Grep obrigatória (mesmo protocolo do lote 1) IMEDIATAMENTE ANTES de cada delete: nome da classe + GUID do `.meta` correspondente em todo `.unity`/`.prefab`/`.asset`, e ausência de `RuntimeInitializeOnLoadMethod`/auto-bootstrap que instancie a classe implicitamente. Isto é obrigatório porque o estado do repo pode ter mudado entre a auditoria original e a execução desta spec.
- Deletar os 15 arquivos `.cs` + seus `.meta` correspondentes.
- Remover as 4 definições de evento (2 arquivos próprios inteiros se for o caso, 2 definições dentro de arquivos compartilhados — não deletar o arquivo compartilhado inteiro).
- Ajustar `ValidateSpec11Damage.cs` para não depender de `StatusTickedEvent` na condição de checagem (remover o termo da condição, ou o item do checklist correspondente, conforme o que fizer sentido lendo o método completo).
- Re-Grep pós-deleção confirmando zero referências residuais aos 15 nomes de classe e aos 4 nomes de evento em todo o código (exceto o próprio `ValidateSpec11Damage.cs` já ajustado).

Fora:
- Remover qualquer arquivo não listado explicitamente aqui.
- Tocar em `Combat/StatusEffect/StatusEffectManager.cs` ou `City/Schedule/SchedulePeriod.cs` (falsos-positivos já excluídos pela auditoria fable_00B).
- Reescrever `ValidateSpec11Damage.cs` além do ajuste pontual da condição/checklist relacionado a `StatusTickedEvent`.

## 12. Fora de escopo

```text
Não inclui: remoção de arquivos não listados; refactor de ValidateSpec11Damage.cs além do ajuste pontual; mudança de outros validators.
```

## 13. Regras de não duplicação

```text
N/A — esta spec só remove código, não cria.
```

## 14. Critérios de aceite

### 14.1 15 arquivos removidos com zero referências

- Cada um dos 15 arquivos (+ `.meta`) foi deletado.
- Grep pós-deleção confirma zero referências de nome de classe em código, cenas, prefabs e assets.
- Evidência esperada: output de Grep antes/depois documentado no relatório.

### 14.2 4 eventos removidos com zero referências

- As 4 definições de evento foram removidas (2 arquivos inteiros, 2 definições pontuais em arquivos compartilhados).
- Grep pós-deleção confirma zero publish/subscribe residual.

### 14.3 ValidateSpec11Damage.cs ajustado

- A condição em L62 (ou o item de checklist equivalente) não depende mais da string `StatusTickedEvent`.
- O validator continua compilando e sua lógica remanescente (checagem de `DamageAppliedEvent`/`StatusAppliedEvent`) permanece intacta.

### 14.4 Build limpo

- `dotnet build` PASS (0 erros / 0 warnings novos) para `Assembly-CSharp.csproj` e `Assembly-CSharp-Editor.csproj`.

---

# /speckit.plan

## 15. Arquitetura alvo

```text
(arquivos deletados, sem substituição — nenhuma nova estrutura)

Assets/_Game/Scripts/Editor/Validation/
  ValidateSpec11Damage.cs   (condição ajustada, StatusTickedEvent removido da checagem)
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
Nenhuma mudança — apenas remoção de classes não referenciadas.

### 16.2 Runtime contracts
N/A — nenhuma API pública em uso é afetada (por definição, zero referências).

### 16.3 Event contracts
Remove 4 eventos mortos do catálogo de eventos (`PauseClosedEvent`, `EquipmentUpgradedEvent`, `SpellCastFailedEvent`, `StatusTickedEvent`). Se o projeto mantiver um catálogo vivo de eventos (skill `event-catalog-and-tracing`), atualizar a entrada correspondente removendo os 4 eventos, se o catálogo for um doc/arquivo versionado dentro do repo lock scope; se for fora do scope declarado, citar no relatório como pendência.

### 16.4 Save contracts
N/A.

### 16.5 UI contracts
`NotificationToastController` e `ClockCalendarHudWidgetView` são views de UI sem uso — confirmar em Fase 0 que nenhuma cena/prefab referencia o GUID do `.meta` antes de deletar (checagem já parte do protocolo de re-verificação).

## 17. Sistemas afetados

```text
Farm (lots), Quests (condition service), UI (cave side menu, notification, clock widget), World (altars, time provider), Cave (loot profile, confinement/boss gate validators), City (zone definition), Core (progression, events), Save (section ownership registry), Loot (reward table definition)
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/Lots/FarmLotSignInteractable.cs (delete)
Assets/_Game/Scripts/Quests/QuestConditionService.cs (delete)
Assets/_Game/Scripts/UI/Cave/CaveCheckpointSideMenuController.cs (delete)
Assets/_Game/Scripts/UI/Notification/NotificationToastController.cs (delete)
Assets/_Game/Scripts/UI/Runtime/ClockCalendarHudWidgetView.cs (delete)
Assets/_Game/Scripts/World/Altars/GodMarkAltarInteractable.cs (delete)
Assets/_Game/Scripts/World/WorldTimeProvider.cs (delete)
Assets/_Game/Scripts/Cave/Loot/CaveTreasureProfile.cs (delete)
Assets/_Game/Scripts/Cave/Runtime/CaveConfinementValidator.cs (delete)
Assets/_Game/Scripts/Cave/Validation/CaveBossGateValidator.cs (delete)
Assets/_Game/Scripts/City/Layout/CityZoneDefinition.cs (delete)
Assets/_Game/Scripts/Core/Progression/LevelUpManager.cs (delete)
Assets/_Game/Scripts/Farm/Harvest/HarvestCommand.cs (delete)
Assets/_Game/Scripts/Loot/RewardTableDefinition.cs (delete)
Assets/_Game/Scripts/Save/SaveSectionOwnershipRegistry.cs (delete)
Assets/_Game/Scripts/Core/Events/UIEvents.cs (edit — remover só PauseClosedEvent)
Assets/_Game/Scripts/Core/Events/EquipmentUpgradedEvent.cs (delete)
Assets/_Game/Scripts/Core/Events/PlayerCombatEvents.cs (edit — remover só SpellCastFailedEvent)
Assets/_Game/Scripts/Core/Events/StatusAndDamageEvents.cs (edit — remover só StatusTickedEvent)
Assets/_Game/Scripts/Editor/Validation/ValidateSpec11Damage.cs (edit — ajuste pontual da condição)
docs/validation/**
```

Todos os `.meta` correspondentes aos arquivos deletados também são deletados.

## 19. Arquivos proibidos

```text
Assets/_Game/Scripts/Combat/StatusEffect/StatusEffectManager.cs (falso-positivo excluído — EM USO via EnemyHealth)
Assets/_Game/Scripts/City/Schedule/SchedulePeriod.cs (falso-positivo excluído — vocabulário ativo)
Assets/**/*.unity, *.prefab, *.asset (nenhuma edição de cena/prefab/asset — a remoção de código não deve exigir isso, já que por definição não há referência)
```

## 20. Estratégia de implementação

```md
### Fase 0 — Re-Grep obrigatório de cada um dos 15 nomes de classe + 4 eventos + GUIDs .meta correspondentes em todo .unity/.prefab/.asset, imediatamente antes de deletar
### Fase 1 — Deletar os 15 arquivos + .meta
### Fase 2 — Remover as 4 definições de evento (2 arquivos inteiros, 2 pontuais)
### Fase 3 — Ajustar ValidateSpec11Damage.cs (remover dependência de StatusTickedEvent)
### Fase 4 — Re-Grep pós-deleção (zero referências residuais) + dotnet build
### Fase 5 — Relatório
```

## 21. Ordem de execucao (ordem segura)

```text
1. Ler spec_codex_06 (lote 1) para replicar o protocolo de verificação exato.
2. Re-Grep de cada um dos 15 nomes de classe (código + GUID .meta em cenas/prefabs/assets) e dos 4 eventos (publish/subscribe) — confirmar zero referências no estado atual do repo.
3. Deletar os 15 arquivos + .meta.
4. Remover as 4 definições de evento (UIEvents.cs, EquipmentUpgradedEvent.cs, PlayerCombatEvents.cs, StatusAndDamageEvents.cs).
5. Ajustar ValidateSpec11Damage.cs (remover StatusTickedEvent da condição/checklist).
6. Re-Grep pós-deleção confirmando zero referências residuais.
7. dotnet build (Assembly-CSharp + Assembly-CSharp-Editor).
8. Registrar relatório com evidência de Grep antes/depois.
```

## 22. Paralelização

```md
- Parallelizable: YES
- Parallel group: codex_convergence_lote2
- Can run with: demais specs deste lote (arquivos não se sobrepõem — nenhum dos 15 arquivos/4 eventos é tocado por spec_codex_09/10/11/13)
- Must not run with: N/A
- Shared files/systems that require lock: os 19 arquivos listados (lock local, delete-only na maioria)
- Reason: remoção pura de código morto confirmado sem referência ativa; risco de conflito com outras specs do lote é zero por escopo de arquivo disjunto
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
Changes existing events: NO (remove 4 eventos mortos, não altera nenhum evento em uso)
Requires unsubscribe pattern: NO (nenhum subscriber ativo a remover, por definição de "morto")
```

## 25. Impacto em UI/Unity

```text
Changes UI: NO (as views removidas não tinham instância em cena)
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: NO (remoção de código morto confirmado; risco coberto por build limpo + Grep de zero referências)
Human validation timing: N/A
```

## 26. Riscos técnicos

```text
Risco: um dos 15 arquivos ou 4 eventos ganhou uma referência nova entre a auditoria original e a execução desta spec.
Mitigação: re-Grep obrigatório imediatamente antes de cada delete (Fase 0 desta spec), não confiar apenas na auditoria já registrada no prompt.

Risco: ajuste em ValidateSpec11Damage.cs pode alterar o que o validator realmente reporta.
Mitigação: ler o método completo em volta da L62 antes de editar; ajustar apenas a condição relacionada a StatusTickedEvent, preservando o restante da lógica de validação.
```

## 27. Rollback

```text
git checkout dos 19 arquivos (15 deletados + UIEvents.cs + EquipmentUpgradedEvent.cs + PlayerCombatEvents.cs + StatusAndDamageEvents.cs + ValidateSpec11Damage.cs).
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Ler spec_codex_06 para replicar protocolo de verificação.
- [ ] T002 — Re-Grep dos 15 nomes de classe + GUIDs .meta em cenas/prefabs/assets.
- [ ] T003 — Re-Grep dos 4 eventos (publish/subscribe).
- [ ] T004 — Deletar os 15 arquivos + .meta.
- [ ] T005 — Remover as 4 definições de evento (2 arquivos inteiros + 2 pontuais).
- [ ] T006 — Ajustar ValidateSpec11Damage.cs.
- [ ] T007 — Re-Grep pós-deleção (zero referências residuais).
- [ ] T008 — dotnet build; gerar execution report com evidência de Grep antes/depois.
```

## 29. Validações obrigatórias

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\validate_docs.ps1
```

## 30. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: NO (remoção de código morto, não lógica de gameplay)
- Requires EditMode tests: NO (nada a testar — código removido não tinha comportamento observável)
- Requires PlayMode automated or final human scenario: NO
- Requires regression test: NO
- Human validation timing: N/A
- Minimum validation evidence for ACCEPTED: dotnet build PASS (0 erros/0 warnings novos) + Grep antes/depois documentado (zero referências residuais) + ValidateSpec11Damage.cs ajustado e compilando.
```

## 31. Definition of Done

```text
15 arquivos + .meta deletados, zero referências residuais confirmadas.
4 eventos mortos removidos (2 arquivos inteiros + 2 definições pontuais), zero publish/subscribe residual.
ValidateSpec11Damage.cs ajustado para não depender de StatusTickedEvent.
dotnet build PASS para ambos os csproj.
Execution report com evidência de Grep antes/depois de cada item.
```

## 32. Anti-regressão

```text
Não remover Combat/StatusEffect/StatusEffectManager.cs nem City/Schedule/SchedulePeriod.cs (falsos-positivos confirmados EM USO).
Não deletar arquivos compartilhados inteiros (UIEvents.cs, PlayerCombatEvents.cs, StatusAndDamageEvents.cs) — só as definições específicas.
Não alterar a lógica remanescente de ValidateSpec11Damage.cs além do ajuste pontual.
```

## 33. Notas para execução posterior

```text
Se o catálogo vivo de eventos (event-catalog-and-tracing) existir como doc versionado e estiver fora do repo lock scope declarado, citar a pendência de atualização no relatório em vez de expandir o escopo sem autorização.
```
