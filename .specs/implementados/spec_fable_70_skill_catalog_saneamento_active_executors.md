# SPEC — Saneamento do Catálogo de Skills + Completar Ativas Dormentes

> **Spec ID:** `fable_70_spec_skill_catalog_saneamento_active_executors`
> **Status:** A implementar
> **Wave:** WAVE FABLE — Skill System Polish
> **Priority:** P1
> **Type:** Runtime + Data
> **Domain:** Player / Combat / Skills
> **Parallelizable:** CONDITIONAL
> **Parallel group:** skills-polish
> **Can run with:** specs de UI/HUD que NÃO alterem `Assets/_Game/Scripts/Skills/**` (ex.: fable_71 HUD pode rodar depois, não em paralelo nos mesmos arquivos)
> **Must not run with:** qualquer spec que altere `DefaultSkillCatalog.cs`, `ActiveSkillExecutionController.cs`, o registro de executores, ou os assets em `Assets/_Game/Data/Skills/**`
> **Repo lock scope:** `Assets/_Game/Scripts/Skills/**`, `Assets/_Game/Data/Skills/**`, `Assets/_Game/Tests/EditMode/Skills/**`
> **Depends on:**
> - `docs/design/SKILL_CATALOG_DESIGN_REVIEW_v1.0.md` (decisões fechadas seção 7)
> - fable_29 (catálogo canônico) — já BUILD_VALIDATED
> **Blocks:**
> - `fable_71` (HUD de active skills — depende do catálogo saneado e do enforcement de custo)
> **Scope:** Sanear o catálogo (cortes, merges, conversão de passivas-fantasma, fix de wiring), cobrar custo de stamina/mana antes da execução, e implementar as ativas dormentes aprovadas via dois novos executores reutilizáveis (Buff e Spawn) + reuso de equipment repair + mecânica de carga.
> **Out of scope:** HUD/botões (fable_71), capstones XOR de Ranged/Survival/Crafting, pool de pontos separado, arte/SFX final.

---

# /speckit.specify

## 5. Contexto

O catálogo canônico (fable_29, 69 nós, 5 árvores) está code-driven em `DefaultSkillCatalog.cs`. A revisão de design `docs/design/SKILL_CATALOG_DESIGN_REVIEW_v1.0.md` constatou: ~14 ativas "dormentes" (equipam mas só publicam toast via `FeedbackOnlySkillEffectExecutor`), 4 passivas de Crafting com hook nomeado mas **nenhum consumidor** (0% de efeito), inconsistências de wiring (ativas mapeadas para `farm.crop.water_skill` como placeholder de debug; `last_breath`/`quick_repair` sem efeito), e custo de stamina/mana **não cobrado** (`TODO_INTEGRATION_NOT_FINAL` em `ActiveSkillExecutionController`). Esta spec executa as decisões fechadas da seção 7 do design review.

## 6. Problema

Sem este saneamento: (a) o jogador gasta skill point **e** um dos 4 active slots em skills que não fazem nada (compra-armadilha, erode confiança no sistema); (b) toda ativa é spam grátis (sem custo), tornando qualquer balance fictício; (c) há nós compráveis em 0% de efeito; (d) wiring de debug (regar planta no slot de "rolamento de emergência") está em produção. A HUD (fable_71) iria expor exatamente esses slots quebrados.

## 7. Objetivo

Ao final desta spec, o catálogo deve ter **somente nós com efeito real**, as ativas aprovadas devem **executar mecânicas distintas** (buff, debuff/marca, zona, decoy, carga, reparo, restore), o custo de stamina/mana deve ser **cobrado antes da execução** com `FailureReason` honesto, e os testes do catálogo devem refletir a nova contagem — sem alterar a HUD nem o save schema.

## 8. Fontes obrigatórias lidas

```text
docs/design/SKILL_CATALOG_DESIGN_REVIEW_v1.0.md   (decisões fechadas)
docs/design/SPEC_SOURCE_MAP.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.claude/rules/testing-quality-gate.md
.claude/rules/id-stability.md
.claude/rules/no-magic-balance-values.md
.claude/skills/ability-effect-composition/SKILL.md
.claude/skills/equipment-durability-repair/SKILL.md
.claude/skills/data-catalog-authoring/SKILL.md
.claude/skills/unity-asset-generation/SKILL.md
```

## 9. Estado atual do repo

| Item | Estado | Path |
|---|---|---|
| Catálogo canônico (69 nós) | Completo | `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs` |
| Controller de execução + input 1-4 | Completo, **sem cobrança de custo** | `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs` |
| Executores reais | Melee/Projectile/SelfRestore/FarmCrop | `…/Runtime/Effects/*SkillEffectExecutor.cs` |
| Executor feedback-only (placeholder) | Completo | `…/Runtime/Effects/FeedbackOnlySkillEffectExecutor.cs` |
| Stamina/Mana spend | Existe | `Assets/_Game/Scripts/Player/StaminaManager.cs::TrySpendStamina`, `ManaManager.cs::TrySpendMana` |
| Equipment repair | Existe | `Assets/_Game/Scripts/Equipment/EquipmentManager.cs`, `EquipmentDurabilityTracker.cs` |
| Status apply (bleed/chill/poison) | Existe (usado por ProjectileExecutor via `statusEffectId`) | auditar na Fase 0 |
| Assets gerados de skill | Existem | `Assets/_Game/Data/Skills/{Nodes,Trees}/*.asset` |
| Gerador de catálogo | Existe | `Assets/_Game/Scripts/Editor/Skills/GenerateCanonicalSkillCatalog.cs` |
| Tests do catálogo | Existem (assertam `CanonicalNodeCount`=69) | `Assets/_Game/Tests/EditMode/Skills/CanonicalCatalogTests.cs` |

> Estado real do sistema de status e do contrato exato de `SkillEffectResult` (campos de cooldown/failure) deve ser auditado na Fase 0 antes de implementar. Não recriar executor/registry existente.

## 10. Engineering stories

```text
Como jogador, quero que toda ativa que equipo num slot faça algo real, para não desperdiçar ponto/slot.
Como jogador, quero que skills custem stamina/mana, para que haja decisão de uso e não spam grátis.
Como designer, não quero nós compráveis em 0% de efeito.
Como maintainer, quero executores reutilizáveis (Buff, Spawn) para não criar um executor por skill.
Como sistema de save, quero que cortar nós não quebre saves (refund de pontos para IDs desconhecidos já existe).
```

## 11. Escopo

```text
Inclui:
A) CORTES (remover dos active slots / da árvore):
   - melee_guarded_block  → remover nó equipável; Block segue como ability Shift.
   - survival_emergency_roll → remover nó equipável; Dodge segue como ability Space.
   - crafting.mecanismo_campo → remover da árvore.
B) MERGES (1 buff por tree):
   - survival.sinal_retirada → "Disengage" (buff curto: +MoveSpeed e −custo Stamina de movimento/dodge).
   - crafting.marca_eficiencia → "Eficiência" (buff curto: −custo Stamina de ações agrícolas/craft próximas).
C) CONVERSÃO de passivas-fantasma de Crafting para efeito real imediato:
   - crafting_station_focus → CraftTimeReductionPercent real (já suportado).
   - crafting_shop_sense, crafting_material_eye → plugar no consumidor real (economia/farm) OU efeito real equivalente.
   - crafting_salvage_method → efeito real de tool/recurso OU conversão. Nenhum nó comprável em 0%.
D) IMPLEMENTAR ativas aprovadas (executores reais):
   - ranged_charged_shot (mecânica de carga: segurar tecla → soltar dispara mais forte/longe; remover flag dormente inconsistente).
   - ranged_marked_prey (aplica status "marcado" no alvo; bônus de dano de disparos no marcado).
   - magic_elemental_ward (buff de resistência elemental temporário no caster).
   - magic_slowing_sigils (zona/AoE que aplica Slow por N s).
   - survival_last_breath (SelfRestore com CD alto — reuso direto do executor existente).
   - survival.isca_improvisada (spawn de decoy temporário que rouba aggro de inimigos simples; boss imune).
   - crafting.irrigador_portatil (rega grupo de plots — reuso do padrão FarmCrop).
   - crafting_field_patch + crafting_quick_repair → MERGE em "Reparo de Campo" (repara item de equipamento ativo via durability; custo material/stamina).
E) ENFORCEMENT de custo: cobrar stamina/mana (e cooldown já existente) ANTES de executar; senão FailureReason + feedback, sem aplicar cooldown.
F) Corrigir mapeamentos de debug em SkillActionToEffectId (remover apontamentos para farm.crop.water_skill).
G) Atualizar CanonicalNodeCount e CanonicalCatalogTests para a nova contagem.
H) Regerar assets de Nodes/Trees via GenerateCanonicalSkillCatalog (com evidência).
```

## 12. Fora de escopo

```text
Não inclui:
- HUD, botões, cooldown visual, custo na tela (fable_71).
- Capstones XOR para Ranged/Survival/Crafting.
- Pool de skill points separado para utilidade.
- Arte/animação/SFX final (notas de animação ficam como guia; VFX placeholder ok).
- Rebalance numérico fino além dos valores iniciais aqui propostos.
- Alteração de save schema/migrations.
```

## 13. Regras de não duplicação

```text
Não criar novo registry/controller de skill — usar SkillEffectRegistry + ActiveSkillExecutionController.
Não criar wrapper Result<T> — executores já retornam SkillEffectResult (bool Success + FailureReason).
Não criar segundo sistema de status — reusar o já consumido por ProjectileSkillEffectExecutor (statusEffectId).
Não criar segundo sistema de durability — reusar EquipmentManager/EquipmentDurabilityTracker.
Não recriar FarmCropSkillEffectExecutor — estender/parametrizar para o irrigador.
Não duplicar StaminaManager/ManaManager — usar TrySpendStamina/TrySpendMana.
```

## 14. Critérios de aceite

### 14.1 Cortes aplicados
- `melee_guarded_block`, `survival_emergency_roll`, `crafting.mecanismo_campo` não existem mais como nós equipáveis no catálogo nem nas Trees.
- Nenhum mapeamento residual em `SkillActionToEffectId` para os IDs cortados.
- Evidência: diff de `DefaultSkillCatalog.cs`; EditMode test confirmando ausência dos IDs nas trees.

### 14.2 Merges aplicados
- Existe exatamente 1 buff de disengage em Survival e 1 buff de eficiência em Crafting; `marca_eficiencia` antigo consolidado.
- `crafting_field_patch`/`crafting_quick_repair` consolidados num único nó "Reparo de Campo".
- Evidência: diff + test de contagem por tree.

### 14.3 Sem passiva-fantasma
- Nenhum nó com `EffectPending=true` permanece comprável; cada um tem efeito real ou foi cortado.
- Evidência: EditMode test "no purchasable node has EffectPending=true".

### 14.4 Ativas aprovadas executam efeito real
- Cada ativa da seção 11.D resolve para um executor real (não `FeedbackOnlySkillEffectExecutor`) e retorna `Success=true` quando recurso/cooldown permitem.
- Evidência: EditMode tests por executor (Buff aplica status; Spawn cria decoy; charge altera dano; repair consome durability/material; restore cura).

### 14.5 Custo cobrado
- Executar uma ativa com stamina/mana insuficiente retorna `Success=false`, publica feedback de recurso insuficiente, e **não** aplica cooldown.
- Executar com recurso suficiente deduz o custo via `TrySpendStamina`/`TrySpendMana` antes de `executor.Execute`.
- Evidência: EditMode test de enforcement (suficiente vs. insuficiente).

### 14.6 Wiring de debug removido
- Nenhuma ativa mapeia para `farm.crop.water_skill` exceto a própria skill de farm.
- Evidência: diff de `SkillActionToEffectId`.

### 14.7 Catálogo/contagem coerentes
- `CanonicalNodeCount` igual à nova contagem; `CanonicalCatalogTests` passa; assets regerados (evidência de geração).
- Evidência: test PASS + log do gerador (menu, exit code, contagem esperada vs. real).

---

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/Skills/Runtime/Effects/
  BuffSkillEffectExecutor.cs        (NOVO — aplica status temporário a self ou target:
                                      ward/resist, marked, disengage, eficiência)
  SpawnSkillEffectExecutor.cs       (NOVO — instancia entidade temporária: decoy de aggro,
                                      zona de slow; usa pooling-friendly spawn)
  RepairSkillEffectExecutor.cs      (NOVO — repara item de equipamento ativo via durability)
  ChargeShotComponent.cs            (NOVO ou extensão — input hold-to-charge p/ charged_shot)
  ActiveSkillExecutionController.cs (EDIT — cost enforcement + remover mapeamentos debug +
                                      registrar novos executores + remover feedback-only dos aprovados)

Assets/_Game/Scripts/Skills/
  DefaultSkillCatalog.cs            (EDIT — cortes, merges, conversão de passivas, tiers/prereqs,
                                      CanonicalNodeCount, remover DormantActiveNodeIds dos aprovados)

Assets/_Game/Data/Skills/{Nodes,Trees}/*.asset  (REGERAR via editor)

Assets/_Game/Tests/EditMode/Skills/
  CanonicalCatalogTests.cs          (EDIT — nova contagem; no-EffectPending; no-cut-ids)
  ActiveSkillCostEnforcementTests.cs (NOVO)
  BuffSpawnRepairExecutorTests.cs    (NOVO)

docs/validation/fable_70_execution_report.md  (NOVO)
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
- `SkillNodeDataSO`: reusar campos existentes. Custo por ativa: definir via campo de custo no nó/ação OU constante nomeada por effect (rule no-magic-balance-values). Auditar na Fase 0 onde colocar custo (preferência: serializado por executor/ação, não literal inline).

### 16.2 Runtime contracts
- Executores continuam **sem** aplicar custo/cooldown (contrato de `ISkillEffectExecutor`). O **controller** cobra custo antes de `Execute` e aplica cooldown depois (já faz cooldown).
- Buff/Spawn/Repair retornam `SkillEffectResult` com `CooldownSeconds` e `FeedbackMessage`.

### 16.3 Event contracts
- Reusar `PlayerActionFeedbackEvent`. Marked/ward podem publicar evento de status já existente (auditar). **Não** criar evento novo sem necessidade — checar `event-catalog-and-tracing`.

### 16.4 Save contracts
```text
Changes save schema? NO
Adds save section? NO
Requires migration? NO (cortar nós: SkillTreeState já refunda pontos de IDs desconhecidos no load)
Persists Unity refs? NO
```

### 16.5 UI contracts
- N/A nesta spec (HUD é fable_71). Os campos de custo/cooldown do ViewModel serão preenchidos lá.

## 17. Sistemas afetados

```text
Skills (catálogo, executores, controller), Player (stamina/mana spend), Equipment (durability repair),
Combat/Status (apply), Farm (irrigador reuse), EditMode tests, Unity asset generation, Validation reports.
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Skills/**
Assets/_Game/Data/Skills/**            (apenas via gerador de editor; sem edit manual de YAML)
Assets/_Game/Tests/EditMode/Skills/**
docs/validation/**
docs/design/SKILL_CATALOG_DESIGN_REVIEW_v1.0.md   (apenas marcar progresso, se necessário)
```

## 19. Arquivos proibidos

```text
Assets/**/*.unity         (sem autorização)
Assets/**/*.prefab        (sem autorização — se decoy/zona exigir prefab, registrar como BLOCKED/asset-gen)
Assets/_Game/Scripts/UI/**  (HUD é fable_71)
docs_old/**, docs/archive/**, Packages/**, ProjectSettings/**
```

> Nota: criar o decoy/zona pode exigir prefab. Se exigir, usar editor script (`PrefabUtility`) com evidência ou registrar `BLOCKED` e entregar a lógica testável via EditMode com um mock de spawn.

## 20. Estratégia de implementação

```md
### Fase 0 — Auditoria
- Confirmar SkillEffectResult (campos), API de status apply, onde definir custo, contrato de spawn/pooling, API de EquipmentDurabilityTracker.
- Confirmar que cortar nós é save-safe (refund) com o test existente.

### Fase 1 — Saneamento de dados (catálogo)
- Cortes (11.A), merges (11.B), conversão de passivas (11.C), remover dormentes aprovadas de DormantActiveNodeIds, ajustar tiers/prereqs, CanonicalNodeCount.
- Remover mapeamentos de debug (11.F).

### Fase 2 — Enforcement de custo (controller)
- Inserir cobrança stamina/mana antes de Execute; sem recurso → Success=false + feedback, sem cooldown.

### Fase 3 — Executores novos + reuso
- BuffSkillEffectExecutor, SpawnSkillEffectExecutor, RepairSkillEffectExecutor; charge input; irrigador via FarmCrop.
- Registrar no controller; remover feedback-only dos aprovados.

### Fase 4 — Testes + asset gen
- EditMode tests (14.3/14.4/14.5); atualizar CanonicalCatalogTests; regerar assets com evidência.

### Fase 5 — Report
- docs/validation/fable_70_execution_report.md com matriz de compliance e evidência.
```

## 21. Ordem segura de execução

```text
1. Fase 0 auditoria (não recriar nada).
2. Saneamento de dados + remover debug mappings.
3. Cost enforcement no controller.
4. Executores + reuso + registro.
5. EditMode tests + regenerar assets.
6. Validações (docs/build/editor) + report.
```

## 22. Paralelização

```md
- Parallelizable: CONDITIONAL
- Parallel group: skills-polish
- Can run with: specs que não toquem Skills/** nem Data/Skills/**
- Must not run with: qualquer spec que edite DefaultSkillCatalog/ActiveSkillExecutionController/executores/assets de skill
- Shared files/systems that require lock: Assets/_Game/Scripts/Skills/**, Assets/_Game/Data/Skills/**
- Reason: altera catálogo central, registro de executores e assets gerados.
```

## 23. Impacto em save/load
```text
Changes save schema? NO
Adds save section? NO
Requires migration? NO
Persists Unity references? NO
```
Cortar nós: IDs removidos no save são refundados no load (comportamento já testado em CanonicalCatalogTests).

## 24. Impacto em eventos
```text
Adds events? NO (preferir reuso de PlayerActionFeedbackEvent + eventos de status existentes)
Changes existing events? NO
Requires unsubscribe pattern? N/A (executores são stateless; spawn/decoy gerencia próprio lifetime)
```

## 25. Impacto em UI/Unity
```text
Changes UI? NO
Changes scenes? NO
Changes prefabs? CONDITIONAL (decoy/zona — preferir spawn por código; se prefab, asset-gen com evidência)
Changes ScriptableObjects/assets? YES (regerar Nodes/Trees via gerador)
Requires Play Mode final validation? YES (feel de carga, decoy aggro, zona slow)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos

```text
Risco: cortar IDs presentes em save → refund inesperado. Mitigação: confirmar test de refund (Fase 0) e documentar.
Risco: custo cobrado mas Execute falha depois → recurso perdido. Mitigação: cobrar APÓS validar tudo e só então Execute; se Execute falhar por motivo não-recurso, considerar refund ou validar antes.
Risco: charge input conflita com input 1-4 atual (GetKeyDown). Mitigação: tratar hold/release na própria skill de carga sem quebrar o slot path.
Risco: decoy/zona exige prefab → bloqueio de asset. Mitigação: spawn por código + EditMode com mock; Play Mode defere o visual.
Risco: regerar assets diverge do código. Mitigação: rodar gerador e comparar contagem; evidência obrigatória (unity-asset-generation).
```

## 27. Rollback
```text
Reverter DefaultSkillCatalog.cs e ActiveSkillExecutionController.cs.
Remover executores novos e tests novos.
Regerar assets a partir do catálogo revertido.
Não apagar save real do usuário.
```

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Fase 0: auditar SkillEffectResult, status apply, local do custo, spawn/pooling, durability API, test de refund.
- [ ] T002 — Cortar guarded_block, emergency_roll, mecanismo_campo (catálogo + trees + mapeamentos).
- [ ] T003 — Merge sinal_retirada→Disengage e marca_eficiencia→Eficiência (1 buff por tree).
- [ ] T004 — Merge field_patch+quick_repair → "Reparo de Campo".
- [ ] T005 — Converter passivas-fantasma de Crafting para efeito real (station_focus/shop_sense/material_eye/salvage_method).
- [ ] T006 — Remover mapeamentos debug (farm.crop.water_skill) de SkillActionToEffectId.
- [ ] T007 — Cost enforcement (stamina/mana) no controller antes de Execute.
- [ ] T008 — BuffSkillEffectExecutor (ward, marked, disengage, eficiência) + registro.
- [ ] T009 — SpawnSkillEffectExecutor (decoy isca, zona slowing_sigils) + registro.
- [ ] T010 — RepairSkillEffectExecutor (reparo de campo) + registro.
- [ ] T011 — Charge input para charged_shot; remover flag dormente inconsistente.
- [ ] T012 — Irrigador via reuso de FarmCropSkillEffectExecutor.
- [ ] T013 — last_breath via SelfRestoreSkillEffectExecutor.
- [ ] T014 — Atualizar CanonicalNodeCount + CanonicalCatalogTests (contagem, no-EffectPending, no-cut-ids).
- [ ] T015 — EditMode tests: cost enforcement + Buff/Spawn/Repair/charge.
- [ ] T016 — Regerar assets Nodes/Trees (evidência).
- [ ] T017 — Execution report.
```

## 29. Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1   # exit 0 obrigatório
```
EditMode (Unity Test Runner) para os tests novos/alterados; se não puder rodar, `NOT RUN` com motivo e risco residual. Asset generation: registrar menu/método, log, exit code, contagem.

## 30. Testing Quality Gate

```md
- Changed deterministic logic: YES (catálogo, cost enforcement, executores)
- Requires EditMode tests: YES (cost enforcement; Buff/Spawn/Repair; no-EffectPending; no-cut-ids; charge dano)
- Requires PlayMode automated or final human scenario: YES (feel de carga, decoy aggro, zona slow) → cenário humano em docs/validation/playmode/
- Requires regression test: YES (refund de save ao cortar nós)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: EditMode PASS + asset-gen evidence + Play Mode scenario executado no lote final.
```

## 31. Definition of Done
```text
Cortes/merges/conversões aplicados; nenhum nó comprável em 0%.
Ativas aprovadas executam efeito real (não feedback-only).
Custo cobrado antes de Execute; sem cooldown em falha de recurso.
Mapeamentos de debug removidos.
CanonicalNodeCount + tests coerentes; assets regerados com evidência.
Build C# (runtime+editor) PASS; docs validation PASS; strict validation exit 0.
Execution report criado. Sem claim ACCEPTED sem Play Mode final.
```

## 32. Anti-regressão
```text
Não alterar IDs de nós mantidos (id-stability) — só remover os cortados (save refunda).
Não quebrar o contrato ISkillEffectExecutor (executor não cobra custo/cooldown).
Não usar GameObject.Find/FindObjectOfType em runtime novo.
Não serializar Unity ref em save.
Não introduzir magic balance values inline (custos/cooldowns nomeados/serializados).
Não regredir as ativas que já funcionam (melee strikes, projéteis, restores).
```

## 33. Notas para execução posterior
```text
Esta spec não implementa a HUD (fable_71) — só deixa o catálogo e o custo prontos.
Capstones XOR de Ranged/Survival/Crafting ficam para leva futura.
Se decoy/zona exigir prefab, registrar como asset-gen/BLOCKED e entregar lógica testável.
```
