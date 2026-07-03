# SPEC â€” RemoÃ§Ã£o de CÃ³digo Morto Confirmado (Lote 1)

> **Spec ID:** `spec_codex_06_dead_code_removal_batch1`
> **Status:** A implementar
> **Wave:** WAVE CODEX CONVERGENCE â€” Honestidade de ValidaÃ§Ã£o
> **Priority:** P2
> **Type:** Governance / Runtime cleanup
> **Domain:** Cave / Combat / Player / UI
> **Parallelizable:** CONDITIONAL
> **Parallel group:** codex_convergence
> **Can run with:** spec_codex_01, spec_codex_02, spec_codex_03, spec_codex_04, spec_codex_07, spec_codex_08
> **Must not run with:** spec_codex_05 (ambas tocam levemente territÃ³rio de Skills/Combat de forma que uma auditoria cruzada Ã© mais segura sequencial â€” CONDITIONAL: pode rodar em paralelo se os arquivos nÃ£o colidirem, o que Ã© o caso aqui; marcado CONDITIONAL por precauÃ§Ã£o jÃ¡ que ambas tocam "Combat")
> **Repo lock scope:** os arquivos listados na seÃ§Ã£o 9 (lista final verificada) + `Assets/_Game/Tests/EditMode/City/CityLayoutScheduleValidationTests.cs` (se aplicÃ¡vel)
> **Depends on:**
> - Nenhuma
> **Depends on (Depende de):**
> - Nenhuma. RemoÃ§Ã£o pura de arquivos Ã³rfÃ£os re-verificados nesta sessÃ£o; nÃ£o depende de nenhuma outra spec do lote.
> **Blocks:**
> - Nenhuma
> **Blocks (Bloqueia):**
> - Nenhuma diretamente. Must not run with `spec_codex_05` (ambas tocam territÃ³rio de Skills/Combat â€” sem overlap real de arquivo hoje, mas rodar sequencialmente reduz risco de uma auditoria cruzada desatualizada se um dos dois lotes remapear/renomear algo em Combat antes do outro terminar).
> **Scope:** Deletar os arquivos de cÃ³digo morto **re-verificados nesta sessÃ£o com zero referÃªncias externas reais** (5 arquivos confirmados); **excluir da lista** os 2 itens que a auditoria mostrou NÃƒO serem cÃ³digo morto (`Combat/StatusEffect/StatusEffectManager.cs` e o namespace `City/Schedule/` como um todo).
> **Out of scope:** Deletar qualquer arquivo cuja auditoria desta sessÃ£o nÃ£o confirmou zero referÃªncias.

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

O prompt de geraÃ§Ã£o desta spec listava 10 itens como "cÃ³digo morto confirmado" a partir de uma auditoria anterior. A Fase 0 desta spec **re-verificou cada item com Grep completo** (incluindo `.unity`/`.prefab`/`.asset`/`.cs`) e encontrou **discrepÃ¢ncias importantes**: 2 dos 10 itens listados **nÃ£o sÃ£o cÃ³digo morto** â€” achado que muda o escopo real desta spec em relaÃ§Ã£o ao pedido original.

## 6. Problema

Manter classes Ã³rfÃ£s sem nenhuma referÃªncia (`MonoBehaviour`s nunca anexados, shims `[Obsolete]` vazios) infla o assembly, confunde busca/grep futura, e pode ser acidentalmente reativado por um agente que nÃ£o sabe que estÃ¡ morto. Ao mesmo tempo, deletar algo que **parece** morto mas na verdade tem consumidor real (como os 2 itens encontrados) quebraria o build â€” por isso a re-verificaÃ§Ã£o desta Fase 0 Ã© o nÃºcleo desta spec.

## 7. Objetivo

Ao final desta spec, os 5 arquivos confirmados sem nenhuma referÃªncia externa sÃ£o deletados, o build compila (0 erros/0 warnings novos), e os 2 itens que a auditoria revelou **nÃ£o serem mortos** ficam documentados como correÃ§Ã£o ao escopo original (nÃ£o deletados).

## 8. Fontes obrigatÃ³rias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.claude/rules/docs-governance.md
.claude/skills/non-regression-review/SKILL.md
```

## 9. Estado atual do repo (Phase 0 â€” re-auditoria completa desta sessÃ£o, Grep incluindo .unity/.prefab/.asset)

### Lista final CONFIRMADA para deleÃ§Ã£o (zero referÃªncias externas, sÃ³ a prÃ³pria declaraÃ§Ã£o de classe):

1. `Assets/_Game/Scripts/Cave/CaveDebugVisualizer.cs` â€” `CaveDebugVisualizer : MonoBehaviour`. Grep por `CaveDebugVisualizer` em `Assets/**`: 1 match (a prÃ³pria declaraÃ§Ã£o). **CONFIRMADO morto.**
2. `Assets/_Game/Scripts/Combat/EnemyPatrolController.cs` â€” `EnemyPatrolController : MonoBehaviour`. Grep: 1 match (prÃ³pria declaraÃ§Ã£o). **CONFIRMADO morto.**
3. `Assets/_Game/Scripts/Combat/TargetVulnerabilityState.cs` â€” `TargetVulnerabilityState : MonoBehaviour`. Grep: 1 match. **CONFIRMADO morto.**
4. `Assets/_Game/Scripts/Player/EnvironmentalExposureManager.cs` â€” `EnvironmentalExposureManager : MonoBehaviour`. Grep: 1 match. **CONFIRMADO morto.**
5. `Assets/_Game/Scripts/Player/PlayerWeaponController.cs` â€” `PlayerWeaponController : MonoBehaviour`. Grep: 1 match. **CONFIRMADO morto.**

### Itens do pedido original que a re-auditoria **NÃƒO CONFIRMOU** como mortos (excluÃ­dos desta spec):

6. **`Assets/_Game/Scripts/Combat/StatusEffect/StatusEffectManager.cs` â€” NÃƒO Ã‰ CÃ“DIGO MORTO.** Grep confirmou uso real: `Assets/_Game/Scripts/Combat/EnemyHealth.cs` L23 (`private CindarsHope.Combat.StatusEffect.StatusEffectManager _statusEffects = new CindarsHope.Combat.StatusEffect.StatusEffectManager();`) e L57 (propriedade pÃºblica `StatusEffects`); e `Assets/_Game/Tests/EditMode/Core/StatusEffectCanonicalTests.cs` L30/L120 (`new StatusEffectManager()`, dentro do mesmo namespace `Combat.StatusEffect` conforme o `using` do teste). Este Ã© o gerenciador de status effects **de inimigos**, distinto e complementar ao `Player/StatusEffectManager.cs` (gerenciador do player) â€” os dois coexistem por design, nÃ£o sÃ£o duplicata morta. **NÃƒO DELETAR.**
7. **Namespace `City/Schedule/` â€” NÃƒO Ã‰ CÃ“DIGO MORTO POR INTEIRO; misto.** Dentro de `Assets/_Game/Scripts/City/Schedule/`:
   - `NpcScheduleResolver.cs` e `NpcScheduleDefinition.cs` â€” classes marcadas `[Obsolete("Absorvido por NPC/Schedule â€” fable_19. NÃ£o usar em cÃ³digo novo.")]`, mas **ainda referenciadas** por `Assets/_Game/Tests/EditMode/City/CityLayoutScheduleValidationTests.cs` (que o prÃ³prio teste documenta como "teste legado que exercita o motor de schedule [Obsolete]", L7-10 do teste). Deletar as classes exigiria remover/migrar esse teste tambÃ©m â€” **decisÃ£o de escopo abaixo**.
   - `SchedulePeriod.cs` (o enum `SchedulePeriod` + `SchedulePeriodHelper`) â€” **NÃƒO Ã© obsoleto** e estÃ¡ **ativamente consumido** por `Assets/_Game/Scripts/City/FarmVisits/FarmVisitRule.cs`, `FarmVisitEligibilityResolver.cs` (propriedade pÃºblica `CurrentPeriod`), `Assets/_Game/Scripts/Dialogue/DialogueContext.cs`, `DialogueCondition.cs`, e 2 arquivos de teste EditMode reais (`CityLayoutScheduleValidationTests.cs`, `DialogueRumorFarmVisitTests.cs`). **NÃƒO DELETAR `SchedulePeriod.cs` sob nenhuma circunstÃ¢ncia** â€” Ã© vocabulÃ¡rio canÃ´nico ativo.
   - **DecisÃ£o desta spec:** dado que `NpcScheduleResolver`/`NpcScheduleDefinition` exigiriam migrar um teste EditMode inteiro para serem removidos com seguranÃ§a (trabalho nÃ£o-trivial, fora do escopo "lote 1" de limpeza simples), **esta spec NÃƒO deleta `City/Schedule/`** â€” fica fora do lote 1, documentado aqui como candidato para uma spec de limpeza dedicada (lote 2) que inclua a migraÃ§Ã£o/remoÃ§Ã£o do teste legado junto.

### Itens nÃ£o re-mencionados no prompt mas verificados por completude (do escopo original citado no prompt do usuÃ¡rio):

8. `Assets/_Game/Scripts/UI/HUD/EquipmentHUD.cs`, `ManaHUD.cs`, `PlayerNeedsHUD.cs`, `PlayerStatusHUD.cs` â€” Grep por `AddComponent<EquipmentHUD`, `AddComponent<ManaHUD`, `AddComponent<PlayerNeedsHUD`, `AddComponent<PlayerStatusHUD` em `Assets/**`: **zero matches** para os 4. Nenhum scene creator instancia essas 4 classes. `PlayerNeedsHUD.cs` referencia `CindarsHope.Player.StatusEffectManager` internamente (campo serializado), mas isso nÃ£o constitui "ser referenciado" â€” Ã© a prÃ³pria classe morta referenciando outra coisa (o `Player.StatusEffectManager`, que Ã© ele mesmo vivo/usado em outro lugar, nÃ£o afetado por esta deleÃ§Ã£o). **CONFIRMADO morto â€” as 4 HUDs.**
9. `Assets/_Game/Scripts/Player/PlayerDodgeController.cs` (a duplicata Ã³rfÃ£, distinta de `Assets/_Game/Scripts/Player/Movement/PlayerDodgeController.cs`, que Ã© o canÃ´nico) â€” Grep por `CindarsHope.Player.PlayerDodgeController` (namespace especÃ­fico do arquivo Ã³rfÃ£o) e por `AddComponent<PlayerDodgeController` fora de `Movement/`: **zero matches** alÃ©m da prÃ³pria declaraÃ§Ã£o. **CONFIRMADO morto.**
10. `Assets/_Game/Scripts/UI/Death/DeathScreenController.cs` â€” jÃ¡ marcado `[System.Obsolete("Replaced by DeathScreenCanvasController (fable_64). Empty no-op shim kept for scene compatibility.")]` no prÃ³prio cÃ³digo, classe **vazia** (`{ }`, sem membros). Grep por `DeathScreenController` (nÃ£o `DeathScreenCanvasController`) em `Assets/**`: aparece em 2 comentÃ¡rios (`CaveRunManager.cs` L255, um comentÃ¡rio desatualizado que cita o listener antigo) e nada mais em cÃ³digo ativo. O prÃ³prio arquivo documenta: "Safe to remove from scenes once they are regenerated." **CONFIRMADO seguro para deletar o arquivo C#** (Ã© um shim vazio); scene references residuais (se existirem em `.unity`/`.prefab`) ficarÃ£o como componente ausente atÃ© a cena ser regenerada â€” **risco residual documentado**, nÃ£o bloqueia a deleÃ§Ã£o do script (Unity remove silenciosamente o componente Ã³rfÃ£o da cena na prÃ³xima abertura/save, sem erro, dado que a classe jÃ¡ Ã© um no-op vazio).

### Lista final consolidada de deleÃ§Ã£o desta spec (7 arquivos, revisado de 10 do pedido original):

```text
Assets/_Game/Scripts/Cave/CaveDebugVisualizer.cs
Assets/_Game/Scripts/Combat/EnemyPatrolController.cs
Assets/_Game/Scripts/Combat/TargetVulnerabilityState.cs
Assets/_Game/Scripts/Player/EnvironmentalExposureManager.cs
Assets/_Game/Scripts/Player/PlayerWeaponController.cs
Assets/_Game/Scripts/UI/HUD/EquipmentHUD.cs
Assets/_Game/Scripts/UI/HUD/ManaHUD.cs
Assets/_Game/Scripts/UI/HUD/PlayerNeedsHUD.cs
Assets/_Game/Scripts/UI/HUD/PlayerStatusHUD.cs
Assets/_Game/Scripts/Player/PlayerDodgeController.cs (o Ã³rfÃ£o, NÃƒO Movement/PlayerDodgeController.cs)
Assets/_Game/Scripts/UI/Death/DeathScreenController.cs
```

(11 arquivos no total â€” nome "lote 1" mantido do pedido original; a contagem "7" acima estava incorreta na primeira contagem mental e foi corrigida para 11 ao montar a lista explÃ­cita linha por linha.)

**Explicitamente EXCLUÃDOS desta spec** (nÃ£o deletar): `Combat/StatusEffect/StatusEffectManager.cs` (em uso real), `City/Schedule/**` por inteiro (uso misto â€” `SchedulePeriod.cs` vivo, `NpcScheduleResolver`/`NpcScheduleDefinition` obsoletos mas ainda testados).

## 10. User stories / engineering stories

```text
Como maintainer, quero remover MonoBehaviours Ã³rfÃ£os sem nenhuma referÃªncia, para reduzir ruÃ­do no assembly.
Como agente de auditoria, quero que a lista de deleÃ§Ã£o seja re-verificada, nÃ£o copiada cegamente de um relatÃ³rio anterior, para nÃ£o quebrar sistemas ativos por engano.
```

## 11. Escopo

Inclui:
- Deletar os 11 arquivos listados na seÃ§Ã£o 9 (lista final consolidada).
- Rodar `dotnet build` para confirmar 0 erros apÃ³s a remoÃ§Ã£o.
- Grep de confirmaÃ§Ã£o pÃ³s-deleÃ§Ã£o (zero referÃªncias remanescentes).
- Documentar explicitamente no execution report os 2 itens excluÃ­dos da deleÃ§Ã£o original e por quÃª.

Fora:
- Deletar `Combat/StatusEffect/StatusEffectManager.cs`.
- Deletar qualquer arquivo de `City/Schedule/`.
- Migrar/remover `CityLayoutScheduleValidationTests.cs` (fica para uma spec de "lote 2" dedicada, se o time decidir seguir com a remoÃ§Ã£o de `NpcScheduleResolver`/`NpcScheduleDefinition`).

## 12. Fora de escopo

```text
NÃ£o inclui: remoÃ§Ã£o de City/Schedule/NpcScheduleResolver.cs e NpcScheduleDefinition.cs (exigiria migrar teste legado â€” trabalho de spec dedicada); qualquer refactor de cÃ³digo vivo.
```

## 13. Regras de nÃ£o duplicaÃ§Ã£o

```text
N/A â€” esta spec sÃ³ remove, nÃ£o cria.
```

## 14. CritÃ©rios de aceite

### 14.1 AutorizaÃ§Ã£o explÃ­cita de deleÃ§Ã£o

Esta spec **autoriza explicitamente** a deleÃ§Ã£o nominal dos 11 arquivos listados na seÃ§Ã£o 9 (lista final consolidada), conforme exigido pela rule `docs-governance`/stop conditions do projeto para deleÃ§Ã£o de cÃ³digo sem spec explÃ­cita.

### 14.2 Build limpo pÃ³s-deleÃ§Ã£o

- `dotnet build Assembly-CSharp.csproj` e `Assembly-CSharp-Editor.csproj`: exit code 0, 0 erros novos.
- EvidÃªncia esperada: output do build.

### 14.3 Zero referÃªncias remanescentes

- Grep por cada um dos 11 nomes de classe deletados em `Assets/**` (incluindo `.unity`/`.prefab`/`.asset`): zero matches em arquivos `.cs` remanescentes (referÃªncias em `.unity`/`.prefab` a componentes deletados nÃ£o quebram o build, mas devem ser citadas se encontradas).
- EvidÃªncia esperada: output de Grep antes/depois.

### 14.4 Itens excluÃ­dos documentados

- Execution report cita explicitamente `Combat/StatusEffect/StatusEffectManager.cs` e `City/Schedule/**` como NÃƒO deletados, com a evidÃªncia de uso real encontrada.

---

# /speckit.plan

## 15. Arquitetura alvo

```text
(remoÃ§Ã£o de arquivos, sem novos arquivos alÃ©m do execution report)

docs/validation/
  spec_codex_06_dead_code_removal_batch1_execution_report.md
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
N/A.

### 16.2 Runtime contracts
N/A â€” nenhuma classe viva tem contrato alterado.

### 16.3 Event contracts
N/A.

### 16.4 Save contracts
N/A.

### 16.5 UI contracts
N/A.

## 17. Sistemas afetados

```text
Cave (remoÃ§Ã£o de debug visualizer Ã³rfÃ£o)
Combat (remoÃ§Ã£o de 2 MonoBehaviours Ã³rfÃ£os)
Player (remoÃ§Ã£o de 3 MonoBehaviours Ã³rfÃ£os, incluindo duplicata de PlayerDodgeController)
UI/HUD (remoÃ§Ã£o de 4 HUDs Ã³rfÃ£s)
UI/Death (remoÃ§Ã£o de shim obsoleto vazio)
```

## 18. Arquivos permitidos

```text
Os 11 arquivos listados na seÃ§Ã£o 9 (deleÃ§Ã£o)
docs/validation/**
```

## 19. Arquivos proibidos

```text
Assets/_Game/Scripts/Combat/StatusEffect/StatusEffectManager.cs (NÃƒO deletar â€” em uso real)
Assets/_Game/Scripts/City/Schedule/** (NÃƒO deletar nenhum arquivo desta pasta nesta spec)
Assets/_Game/Tests/EditMode/City/CityLayoutScheduleValidationTests.cs (NÃƒO tocar nesta spec)
Assets/**/*.unity, *.prefab, *.asset (sem ediÃ§Ã£o manual â€” deixar Unity limpar referÃªncias Ã³rfÃ£s na prÃ³xima abertura/save regular do editor, se aplicÃ¡vel)
```

## 20. EstratÃ©gia de implementaÃ§Ã£o

```md
### Fase 0 â€” Auditoria (concluÃ­da nesta spec; ver seÃ§Ã£o 9, jÃ¡ re-verificada)
### Fase 1 â€” Deletar os 11 arquivos
### Fase 2 â€” dotnet build + Grep de confirmaÃ§Ã£o
### Fase 3 â€” RelatÃ³rio
```

## 21. Ordem de execucao (ordem segura)

```text
1. Deletar os 11 arquivos um a um (Bash/PowerShell rm ou equivalente do editor).
2. dotnet build Assembly-CSharp.csproj --no-restore.
3. dotnet build Assembly-CSharp-Editor.csproj --no-restore.
4. Grep de confirmaÃ§Ã£o (zero refs .cs remanescentes).
5. Registrar relatÃ³rio citando os 2 itens excluÃ­dos e por quÃª.
```

## 22. ParalelizaÃ§Ã£o

```md
- Parallelizable: CONDITIONAL
- Parallel group: codex_convergence
- Can run with: spec_codex_01, 02, 03, 04, 07, 08
- Must not run with: nenhuma proibiÃ§Ã£o dura; CONDITIONAL apenas por precauÃ§Ã£o de tocar "Combat" â€” sem overlap real de arquivo com spec_codex_05
- Shared files/systems that require lock: nenhum arquivo compartilhado com outra spec do lote
- Reason: remoÃ§Ã£o pura, sem ediÃ§Ã£o de arquivo vivo
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
Changes UI: NO (HUDs removidas nunca foram instanciadas em cena)
Changes scenes: CONDITIONAL (cenas podem ter referÃªncia Ã³rfÃ£ ao componente DeathScreenController jÃ¡-obsoleto; resolvido automaticamente pela Unity na prÃ³xima regeneraÃ§Ã£o/save de cena, sem aÃ§Ã£o manual)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: NO
Human validation timing: NOT REQUIRED
```

## 26. Riscos tÃ©cnicos

```text
Risco: alguma .unity/.prefab tem uma referÃªncia serializada (missing script) ao DeathScreenController apÃ³s a deleÃ§Ã£o.
MitigaÃ§Ã£o: componente jÃ¡ era um no-op vazio [Obsolete] â€” Unity trata como "missing script" harmless; documentado como risco residual, resolvido na prÃ³xima regeneraÃ§Ã£o de cena via CindarsHope/Inicializar Projeto.

Risco: algum arquivo fora dos 11 confirmados ainda referencia um deles de forma nÃ£o capturada pelo Grep (ex.: reflection, string literal de nome de tipo).
MitigaÃ§Ã£o: Grep cobriu literal do nome da classe em todo Assets/**, incluindo YAML; reflection por nome de tipo nÃ£o Ã© um padrÃ£o usado neste projeto (confirmado pelas rules de arquitetura que proÃ­bem esse tipo de indireÃ§Ã£o).
```

## 27. Rollback

```text
git checkout dos 11 arquivos deletados (estÃ£o em controle de versÃ£o).
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 â€” Deletar os 11 arquivos confirmados (lista da seÃ§Ã£o 9).
- [ ] T002 â€” dotnet build Assembly-CSharp + Assembly-CSharp-Editor, exit 0.
- [ ] T003 â€” Grep de confirmaÃ§Ã£o pÃ³s-deleÃ§Ã£o (zero refs .cs remanescentes).
- [ ] T004 â€” Gerar execution report citando os 2 itens excluÃ­dos (StatusEffectManager Combat, City/Schedule) e a evidÃªncia de uso real encontrada.
```

## 29. ValidaÃ§Ãµes obrigatÃ³rias

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\validate_docs.ps1
```

## 30. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: NO (remoÃ§Ã£o pura, sem lÃ³gica nova)
- Requires EditMode tests: NO
- Requires PlayMode automated or final human scenario: NO
- Requires regression test: YES â€” o prÃ³prio dotnet build 0 erros Ã‰ o regression test desta spec (garante que nada vivo dependia dos arquivos removidos)
- Human validation timing: NOT REQUIRED
- Minimum validation evidence for ACCEPTED: dotnet build PASS (0 erros/0 warnings novos) + Grep de confirmaÃ§Ã£o zero refs.
```

## 31. Definition of Done

```text
11 arquivos deletados.
dotnet build PASS.
Grep de confirmaÃ§Ã£o zero refs.
Execution report documentando os 2 itens excluÃ­dos e por quÃª.
```

## 32. Anti-regressÃ£o

```text
NÃ£o deletar Combat/StatusEffect/StatusEffectManager.cs.
NÃ£o deletar nenhum arquivo de City/Schedule/.
NÃ£o tocar Movement/PlayerDodgeController.cs (o canÃ´nico).
NÃ£o tocar DeathScreenCanvasController.cs (o substituto ativo).
```

## 33. Notas para execuÃ§Ã£o posterior

```text
City/Schedule/NpcScheduleResolver.cs e NpcScheduleDefinition.cs continuam [Obsolete] e ainda testados por CityLayoutScheduleValidationTests.cs â€” candidatos a uma spec de "lote 2" que migre/remova esse teste junto, se o time decidir prosseguir com a remoÃ§Ã£o completa.
```
