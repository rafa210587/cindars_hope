# SPEC â€” Corrigir Mapeamento Legado de Skill + Ledger de DÃ©bito das Skills Feedback-Only

> **Spec ID:** `spec_codex_05_skill_placeholder_debt`
> **Status:** Implementado e BUILD_VALIDATED (docs-migration 2026-08-12)
> **Wave:** WAVE CODEX CONVERGENCE â€” Honestidade de ValidaÃ§Ã£o
> **Priority:** P1
> **Type:** Runtime / Data / Docs
> **Domain:** Skills / Combat
> **Parallelizable:** YES
> **Parallel group:** codex_convergence
> **Can run with:** spec_codex_01, spec_codex_02, spec_codex_03, spec_codex_04, spec_codex_06, spec_codex_07, spec_codex_08
> **Must not run with:** N/A
> **Repo lock scope:** `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs`
> **Depends on:**
> - Nenhuma
> **Depends on (Depende de):**
> - Nenhuma spec deste lote. Depende apenas de sistemas existentes: `ActiveSkillExecutionController.SkillActionToEffectId`, `FarmCropSkillEffectExecutor` (efeito real de `farm.crop.water_skill`), `FeedbackOnlySkillEffectExecutor`, e (leitura apenas) `EquipmentDurabilityTracker` para avaliar viabilidade de reparo real.
> **Blocks:**
> - Nenhuma
> **Blocks (Bloqueia):**
> - Nenhuma. NÃ£o bloqueia nenhuma outra spec do lote `codex_convergence`.
> **Scope:** Corrige o mapeamento legado `skill_crafting_field_patch â†’ farm.crop.water_skill` (efeito errado); mantÃ©m as 6 skills feedback-only (nenhum sistema de apoio real existe hoje para nenhuma delas â€” confirmado na auditoria), registrando-as em um ledger de dÃ©bito canÃ´nico com critÃ©rio de fechamento por skill.
> **Out of scope:** Implementar sistemas de apoio novos (marking/aggro, ward/shield, aggro reduction, lure, farm watering tool, crafting speed buff) â€” nenhum existe hoje, e criÃ¡-los Ã© escopo de specs de gameplay completas, nÃ£o desta spec de honestidade/dÃ©bito.

required_adrs: []
required_game_rules: []

---

## Evidência de implementação (docs-migration 2026-08-12)

```text
Status: Implementado e BUILD_VALIDATED
Execution report: docs/validation/spec_codex_05_skill_placeholder_debt_execution_report.md (2026-07-03)
Re-verificação nesta sessão (Grep/Read no disco):
  - Assets/_Game/Scripts/Gameplay/SkillActionEffectCatalog.cs (mapeamento foi refatorado para este
    arquivo após o execution report original, mas a correção persiste):
    `{ "skill_crafting_field_patch", "crafting.field_patch" }` e
    `{ "skill_crafting_irrigador_portatil", "farm.crop.water_skill" }` confirmados — mapeamento
    legado corrigido.
  - docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECT_CATALOG.md contém "Skill Effect Debt Ledger".
  - Assets/_Game/Tests/EditMode/Skills/ActiveSkillExecutionControllerMappingTests.cs existe no disco.
Build: dotnet build PASS (ambos assemblies).
Play Mode: DEFERRED_TO_FINAL_VALIDATION (spec seção 25) — evidência automatizada suficiente para
  esta baixa; cenário humano documentado no execution report.
```

---

# /speckit.specify

## 5. Contexto

Achados verificados em `ActiveSkillExecutionController.cs`:
- L43: `{ "skill_crafting_field_patch", "farm.crop.water_skill" }, // DEBUG legado: substituir no merge de reparo (deferido)` â€” skill de "Reparo de Campo" estÃ¡ mapeada para o efeito de regar cultura, o que Ã© semanticamente errado (comentÃ¡rio no prÃ³prio cÃ³digo jÃ¡ assume isso: "DEBUG legado").
- L195-202: 6 skills com `FeedbackOnlySkillEffectExecutor` â€” `combat.ranged.marked_prey`, `combat.magic.elemental_ward`, `survival.sinal_retirada`, `survival.isca_improvisada`, `crafting.irrigador_portatil`, `crafting.marca_eficiencia` â€” cada uma com um comentÃ¡rio explÃ­cito `DEFERRED_RUNTIME_EFFECT`/`TODO_INTEGRATION_NOT_FINAL` jÃ¡ no cÃ³digo, citando que o "sistema alvo" (marking, wards, slow fields, traps, efficiency buffs) nÃ£o existe.

## 6. Problema

`skill_crafting_field_patch` (Reparo de Campo) hoje rega uma cultura em vez de reparar/consertar algo â€” um jogador que equipe essa skill esperando reparo (durabilidade de ferramenta, cerca, etc.) recebe um efeito de irrigaÃ§Ã£o sem relaÃ§Ã£o, o que Ã© confuso e semanticamente incorreto mesmo funcionando "sem erro". As 6 skills feedback-only nÃ£o fazem nada alÃ©m de mostrar um texto â€” isso Ã© aceitÃ¡vel como estado transitÃ³rio documentado, mas hoje nÃ£o hÃ¡ um ledger canÃ´nico rastreando essas 6 skills como dÃ©bito com critÃ©rio de fechamento; ficam soltas em comentÃ¡rios de cÃ³digo.

## 7. Objetivo

Ao final desta spec: (a) `skill_crafting_field_patch` nÃ£o aponta mais para `farm.crop.water_skill`; aponta para um efeito honesto â€” ou um executor real, se a auditoria (feita nesta sessÃ£o, ver seÃ§Ã£o 9) achar um sistema de apoio real para "reparo", ou (se nÃ£o houver) um `FeedbackOnlySkillEffectExecutor` com mensagem especÃ­fica de reparo (nunca mais reusando o efeito de water); (b) as 6 skills feedback-only permanecem feedback-only (nenhuma tem sistema de apoio real, confirmado) mas passam a estar listadas em um ledger de dÃ©bito canÃ´nico com sistema-alvo e critÃ©rio de fechamento por skill.

## 8. Fontes obrigatÃ³rias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.claude/skills/ability-effect-composition/SKILL.md
.claude/skills/skill-tree-authoring/SKILL.md
.claude/skills/equipment-durability-repair/SKILL.md
```

## 9. Estado atual do repo (Phase 0 â€” auditado nesta sessÃ£o, por skill)

Auditoria de sistema de apoio real para cada uma das 6 skills feedback-only + o mapeamento legado:

| Skill ID | Efeito atual | Sistema de apoio necessÃ¡rio | Existe no repo? |
|---|---|---|---|
| `skill_crafting_field_patch` | `farm.crop.water_skill` (**errado** â€” reusa water) | Reparo (durabilidade de ferramenta/equipamento ou estrutura) | **SIM, parcialmente** â€” `Assets/_Game/Scripts/Equipment/` tem `EquipmentDurabilityTracker`/reparo de equipamento (skill `equipment-durability-repair` confirma sistema real de repair). Mas "Reparo de Campo" como skill de combate/utilidade ativa nÃ£o tem um alvo natural Ã³bvio (reparar o quÃª, em campo, durante combate?) â€” a auditoria de implementaÃ§Ã£o deve decidir entre (a) reusar `EquipmentDurabilityTracker.Repair(...)` para reparar o item equipado atual por uma quantidade fixa, se isso fizer sentido semÃ¢ntico como "field patch", ou (b) manter feedback-only com mensagem correta de "reparo" em vez de reusar water. **DecisÃ£o mÃ­nima obrigatÃ³ria desta spec: parar de apontar para `farm.crop.water_skill`.** Se a Fase 0 de implementaÃ§Ã£o confirmar que ligar a `EquipmentDurabilityTracker` Ã© trivial (poucas linhas, reusando API existente), implementar; senÃ£o, feedback-only correto Ã© aceitÃ¡vel e cumpre o objetivo desta spec (honestidade > feature nova).
| `combat.ranged.marked_prey` | Feedback-only | Sistema de marcaÃ§Ã£o de alvo (aggro tag, bÃ´nus de dano contra alvo marcado) | **NÃƒO** â€” Grep nÃ£o encontrou nenhum `MarkedTarget`/`PreyMark`/sistema de marcaÃ§Ã£o em `Assets/_Game/Scripts/Combat/**`. |
| `combat.magic.elemental_ward` | Feedback-only | Shield/ward que absorve dano elemental | **NÃƒO** â€” Grep nÃ£o encontrou `Ward`/`Shield` como sistema de mitigaÃ§Ã£o de dano ativo (status effects existentes sÃ£o buffs/debuffs de dano-sobre-tempo/controle, nÃ£o barreira de absorÃ§Ã£o). |
| `survival.sinal_retirada` | Feedback-only | ReduÃ§Ã£o de aggro em Ã¡rea / retirada segura | **NÃƒO** â€” nÃ£o hÃ¡ sistema de "aggro" explÃ­cito reduzÃ­vel por skill; enemy AI usa detecÃ§Ã£o direta, sem um valor de aggro acumulÃ¡vel manipulÃ¡vel por skill do player. |
| `survival.isca_improvisada` | Feedback-only | Lure/isca que atrai inimigos para um ponto | **NÃƒO** â€” nenhum `Lure`/`Bait`/objeto atrator de IA encontrado em `Assets/_Game/Scripts/Combat/**` ou `Assets/_Game/Scripts/World/**`. |
| `crafting.irrigador_portatil` | Feedback-only | Regar uma Ã¡rea de cultivo remotamente (utilidade de farm) | **PARCIAL** â€” `farm.crop.water_skill` (o efeito real que hoje estÃ¡ mal-mapeado em `field_patch`) jÃ¡ existe e faz exatamente "regar via skill". A auditoria de implementaÃ§Ã£o deve considerar se `crafting.irrigador_portatil` Ã© o candidato semanticamente correto para receber esse efeito real (nome sugere "irrigador" = regar), em vez de `field_patch`. Se sim, isso resolve dois problemas de uma vez: tira o mapeamento errado de `field_patch` e dÃ¡ um executor real a `irrigador_portatil`. **RecomendaÃ§Ã£o forte desta auditoria**: mover `farm.crop.water_skill` de `field_patch` para `irrigador_portatil`.
| `crafting.marca_eficiencia` | Feedback-only | Buff temporÃ¡rio de velocidade/eficiÃªncia de crafting | **NÃƒO** â€” nenhum sistema de "crafting speed multiplier" ativo por skill encontrado em `Assets/_Game/Scripts/Craft*/**` (o crafting existente processa por tempo fixo de job, sem hook de multiplicador de skill).

- `_registry.Register(...)` em `RegisterFeedbackExecutors()` (L189-203) Ã© o ponto Ãºnico de registro dos 6 placeholders â€” qualquer skill promovida a executor real sai desse mÃ©todo e entra em `RegisterCombatExecutors()` (ou um novo `RegisterFarmExecutors()`/`RegisterUtilityExecutors()` se a categoria nÃ£o se encaixar nos existentes).
- `FarmCropSkillEffectExecutor` jÃ¡ existe e Ã© o executor real de `farm.crop.water_skill` (registrado separadamente em `Bootstrap()` L126, fora do dicionÃ¡rio `SkillActionToEffectId` â€” o dicionÃ¡rio sÃ³ mapeia `skillActionId â†’ effectId`; o executor real jÃ¡ estÃ¡ registrado no `_registry` independente de qual `skillActionId` aponta para ele).

## 10. User stories / engineering stories

```text
Como jogador, quero que "Reparo de Campo" nÃ£o regue uma plantaÃ§Ã£o â€” quero honestidade sobre o que a skill faz, mesmo que ainda nÃ£o tenha efeito mecÃ¢nico completo.
Como maintainer, quero um ledger Ãºnico listando as skills feedback-only com o sistema que falta e o critÃ©rio de fechamento, em vez de comentÃ¡rios espalhados no cÃ³digo.
```

## 11. Escopo

Inclui:
- Remover o mapeamento `{ "skill_crafting_field_patch", "farm.crop.water_skill" }` do dicionÃ¡rio `SkillActionToEffectId`.
- DecisÃ£o A (recomendada pela auditoria, aplicar se a Fase 0 de implementaÃ§Ã£o confirmar viabilidade trivial): mapear `skill_crafting_field_patch` para um `FeedbackOnlySkillEffectExecutor` novo com mensagem correta de reparo (ex.: "Reparo de Campo aplicado. (Efeito de reparo pendente.)"), e mapear `crafting.irrigador_portatil` para o executor real `farm.crop.water_skill` jÃ¡ existente (resolvendo a semÃ¢ntica de "irrigador" = regar).
- DecisÃ£o B (fallback se A nÃ£o for trivial): manter `crafting.irrigador_portatil` feedback-only, mas ainda assim tirar `field_patch` de apontar para water â€” `field_patch` vira feedback-only com mensagem de reparo (nunca mais reusa water_skill).
- Registrar as 6 skills confirmadas sem sistema de apoio (marked_prey, elemental_ward, sinal_retirada, isca_improvisada, marca_eficiencia, e field_patch OU irrigador_portatil conforme a decisÃ£o A/B acima) em um ledger de dÃ©bito canÃ´nico (`docs/backlog/` ou `docs/game_rules/`, conforme convenÃ§Ã£o existente do projeto â€” auditar nome de arquivo padrÃ£o antes de criar um novo), citando: skill ID, efeito atual (feedback-only), sistema-alvo necessÃ¡rio, critÃ©rio de fechamento (quando o sistema existir, ex.: "fechar quando houver sistema de marking de alvo").
- EditMode test cobrindo: `SkillActionToEffectId["skill_crafting_field_patch"]` nÃ£o Ã© mais `"farm.crop.water_skill"`; se a decisÃ£o A for aplicada, `SkillActionToEffectId["skill_crafting_irrigador_portatil"] == "farm.crop.water_skill"`.

Fora:
- Implementar qualquer sistema de apoio novo (marking, ward, aggro, lure, crafting speed buff).
- Rebalancear custos/cooldowns das skills existentes.

## 12. Fora de escopo

```text
NÃ£o inclui: sistemas de gameplay novos para as 6 skills; UI de skill tree; rebalanceamento.
```

## 13. Regras de nÃ£o duplicaÃ§Ã£o

```text
NÃ£o criar um segundo dicionÃ¡rio de mapeamento â€” editar SkillActionToEffectId existente.
NÃ£o criar um segundo FeedbackOnlySkillEffectExecutor genÃ©rico por skill se jÃ¡ existe a classe â€” reusar a classe existente, sÃ³ variar a mensagem/categoria no construtor.
```

## 14. CritÃ©rios de aceite

### 14.1 Mapeamento legado corrigido

- `skill_crafting_field_patch` nÃ£o aponta mais para `farm.crop.water_skill`.
- EvidÃªncia esperada: leitura do cÃ³digo + EditMode test.

### 14.2 Ledger de dÃ©bito criado

- Doc canÃ´nico lista as skills feedback-only remanescentes (5 ou 6, conforme decisÃ£o A/B) com sistema-alvo e critÃ©rio de fechamento.
- EvidÃªncia esperada: leitura do doc.

### 14.3 Sem regressÃ£o de execuÃ§Ã£o de skill

- Todas as skills do dicionÃ¡rio continuam resolvendo para um executor registrado (nenhum `effectId` Ã³rfÃ£o).
- EvidÃªncia esperada: `dotnet build` PASS + teste confirmando que todo valor do dicionÃ¡rio tem um executor correspondente registrado (pode reusar/estender teste existente se houver; senÃ£o criar).

---

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/Skills/Runtime/Effects/
  ActiveSkillExecutionController.cs   (mapeamento corrigido)

docs/backlog/ (ou docs/game_rules/, conforme convenÃ§Ã£o existente)
  <ledger de dÃ©bito de skill effects feedback-only> (novo ou seÃ§Ã£o adicionada)

Assets/_Game/Tests/EditMode/Skills/ (ou pasta anÃ¡loga existente)
  ActiveSkillExecutionControllerMappingTests.cs (novo, ou extensÃ£o de teste existente)
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
N/A â€” sem novo SO.

### 16.2 Runtime contracts
`SkillActionToEffectId` (dicionÃ¡rio estÃ¡tico privado) â€” valores alterados, chaves preservadas (nenhum skill ID removido/renomeado, respeitando `id-stability`).

### 16.3 Event contracts
N/A â€” nenhum evento novo.

### 16.4 Save contracts
N/A.

### 16.5 UI contracts
N/A â€” mensagens de feedback sÃ£o strings, nÃ£o afetam schema de UI.

## 17. Sistemas afetados

```text
Skills (effect mapping)
Combat/Farm effect executors (leitura, sem mudanÃ§a de contrato)
Docs (ledger de dÃ©bito)
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs
Assets/_Game/Tests/EditMode/Skills/**
docs/backlog/**
docs/game_rules/**
docs/validation/**
```

## 19. Arquivos proibidos

```text
Assets/_Game/Scripts/Farm/**, Assets/_Game/Scripts/Equipment/** (leitura ok para auditoria de reuso, ediÃ§Ã£o nÃ£o)
Assets/**/*.unity, *.prefab, *.asset
```

## 20. EstratÃ©gia de implementaÃ§Ã£o

```md
### Fase 0 â€” Auditoria (concluÃ­da nesta spec; ver seÃ§Ã£o 9) + decisÃ£o A vs B para field_patch/irrigador_portatil
### Fase 1 â€” Corrigir mapeamento
### Fase 2 â€” Criar/estender ledger de dÃ©bito
### Fase 3 â€” Testes/validaÃ§Ã£o
### Fase 4 â€” RelatÃ³rio
```

## 21. Ordem de execucao (ordem segura)

```text
1. Decidir A (mover water_skill para irrigador_portatil) ou B (field_patch vira feedback-only puro) â€” critÃ©rio: trivialidade da mudanÃ§a sem tocar Farm.
2. Editar SkillActionToEffectId conforme a decisÃ£o.
3. Ajustar RegisterFeedbackExecutors()/RegisterCombatExecutors() conforme necessÃ¡rio (mover registro se decisÃ£o A).
4. Criar/estender ledger de dÃ©bito com as skills confirmadas sem sistema de apoio.
5. Escrever/estender EditMode test do mapeamento.
6. dotnet build + EditMode tests.
7. Registrar relatÃ³rio com a decisÃ£o tomada e por quÃª.
```

## 22. ParalelizaÃ§Ã£o

```md
- Parallelizable: YES
- Parallel group: codex_convergence
- Can run with: demais specs do lote
- Must not run with: N/A
- Shared files/systems that require lock: Skills/Runtime/Effects/ActiveSkillExecutionController.cs (lock local)
- Reason: escopo isolado; leitura de Farm/Equipment Ã© sÃ³ para decisÃ£o de reuso, sem ediÃ§Ã£o
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
Changes UI: NO (mensagens de toast textuais apenas)
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: CONDITIONAL (sÃ³ se decisÃ£o A mover um efeito real â€” nesse caso sim, para confirmar irrigador_portatil rega de fato)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos tÃ©cnicos

```text
Risco: mover farm.crop.water_skill para irrigador_portatil (decisÃ£o A) pode ter efeitos colaterais nÃ£o previstos se field_patch jÃ¡ estiver equipado por algum save real de teste/QA.
MitigaÃ§Ã£o: IDs de skill nÃ£o mudam (sÃ³ o effectId associado) â€” nenhum save Ã© afetado (skill tree salva skill node IDs, nÃ£o effect mappings). Confirmar isso na Fase 0 antes de aplicar a decisÃ£o A.
```

## 27. Rollback

```text
Reverter ActiveSkillExecutionController.cs para o mapeamento original.
Remover/reverter o ledger de dÃ©bito.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 â€” Confirmar que mudar effectId nÃ£o afeta save (skill tree salva node IDs, nÃ£o effect mappings).
- [ ] T002 â€” Decidir A ou B para field_patch/irrigador_portatil, documentando o motivo.
- [ ] T003 â€” Editar SkillActionToEffectId conforme a decisÃ£o.
- [ ] T004 â€” Ajustar registro de executores se necessÃ¡rio (decisÃ£o A).
- [ ] T005 â€” Criar/estender ledger de dÃ©bito canÃ´nico com as skills feedback-only remanescentes.
- [ ] T006 â€” Escrever/estender EditMode test do mapeamento (field_patch nÃ£o aponta mais para water; todo valor do dicionÃ¡rio tem executor registrado).
- [ ] T007 â€” Gerar execution report com a decisÃ£o tomada.
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

- Changed deterministic logic: YES (dicionÃ¡rio de mapeamento estÃ¡tico)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: CONDITIONAL (sÃ³ se decisÃ£o A)
- Requires regression test: YES (nenhum effectId Ã³rfÃ£o apÃ³s a mudanÃ§a)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: dotnet build PASS + EditMode test do mapeamento PASS + ledger de dÃ©bito criado.
```

## 31. Definition of Done

```text
skill_crafting_field_patch nÃ£o aponta mais para farm.crop.water_skill.
Ledger de dÃ©bito criado/atualizado com as skills feedback-only remanescentes e critÃ©rio de fechamento.
EditMode test novo/estendido passando.
Execution report criado com a decisÃ£o A/B documentada.
```

## 32. Anti-regressÃ£o

```text
NÃ£o remover nenhum skill ID do dicionÃ¡rio (sÃ³ o effectId associado muda).
NÃ£o deixar nenhum effectId sem executor registrado.
NÃ£o alterar cooldowns/custos das skills reais jÃ¡ implementadas (combat executors existentes).
```

## 33. Notas para execuÃ§Ã£o posterior

```text
As 5-6 skills feedback-only remanescentes exigem sistemas de apoio que hoje nÃ£o existem (marking, ward, aggro, lure, crafting speed) â€” implementÃ¡-los Ã© trabalho de specs de gameplay completas, nÃ£o desta spec.
Se a decisÃ£o A for aplicada e revelar acoplamento inesperado entre farm.crop.water_skill e o slice original de field_patch, documentar e reverter para decisÃ£o B.
```
