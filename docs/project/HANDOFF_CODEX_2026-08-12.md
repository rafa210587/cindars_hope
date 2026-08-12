# Handoff para o Codex — 2026-08-12

> Estado de entrega desta sessão (Claude), para o **Codex CLI** continuar. Branch `dev`, sincronizada
> com `origin/dev`. O harness do Codex (`AGENTS.md` + `.codex/` + `.agents/skills/`) é **gerado** de
> `.claude/` e **se auto-atualiza** no Stop pelo hook `sync-harness-and-tracing.ps1` — então tudo que o
> Claude usa (skills, rules, agents) já está disponível pro Codex.

## Ambiente (importante)
- **Unity 6000.5.7f1** (o projeto foi atualizado de 6000.4.7f1 nesta sessão; commit `210e78f6`).
- csproj **modularizados**: `CindarsHope.Foundation/Runtime/Gameplay/Editor/Tests.EditMode.csproj`
  (NÃO existe mais `Assembly-CSharp*`). Compile fallback: `dotnet build <proj>.csproj` (com restore).
- **Batchmode Unity**: `& Unity.exe` NÃO bloqueia (app GUI) → use `Start-Process -Wait` (ver
  `tools/unity/RunUnityEditModeTests.ps1`). NUNCA rode batchmode com o Editor aberto (lock).
- Comandos de validação: `CindarsHope/Inicializar Projeto` (gera dados + recria 3 cenas, DESTRUTIVO),
  `CindarsHope/Validar Projeto` (validadores read-only), `RunUnityEditModeTests.ps1 -UnityPath ...6000.5...`.
- **Churn do Inicializar**: recriar cenas suja ~180 `.meta` de Resources + Farm/CaveScene + `.slnx`.
  É regenerável — descarte com `git restore` e commite só os entregáveis reais.

## O que foi ENTREGUE nesta sessão (commitado + pushado)
1. **Bugfix loja do Pip** (`eaf42f1c`): `NpcShopController._playerManager` null era regressão da
   modularização — managers do `DomainManagerRegistry` (Player/Inventory/Modal) sem guarda de duplicata
   na troca de cena. Corrigido + validado em Play Mode + limpeza da linha morta `_modalManager` no gerador.
2. **Skill `spec-authoring`** + `.specs/_templates/SPEC_DEEP_TEMPLATE.md` + exemplo: padrão de spec
   PROFUNDA (blueprint: assinaturas, classes criar/modificar, plano por edição, critério binário com DoD,
   §23 edge cases, matriz por TIPO de spec).
3. **Hook `sync-harness-and-tracing.ps1`** (Stop): re-gera paridade Codex + `SPEC_INDEX` quando
   `.claude/**`/`CLAUDE.md`/`.specs/**` mudam. **Rode `tools/codex/Generate-CodexHarness.ps1` após editar `.claude/`.**
4. **Specs executadas + validadas no Unity 6000.5 + baixadas** para `.specs/implementados/`:
   - `spec_content_enemy_status_kit_ids_v1`: `StatusEffectType.Buff` + 12 status materializados;
     `ValidateEnemyAttackKits` **43 erros → 0**; EditMode 12/12.
   - `spec_content_city_schedule_colliders_facades_v1`: `CreateMvpTownScene` (colliders + placeholders);
     `ValidateFableCitySchedule` **PASS 21 / FAIL 0**.
   - Suíte EditMode completa: **2858/2858**.
5. **`spec_cleanup_uac_serialization_6000_5_v1` — PARCIAL** (ainda em `a_implementar/`): casos 2/3 feitos
   (Festival `[Serializable]`, remove `[SerializeField]` morto); **caso 1 DEFERIDO** (ver pendências).
6. **13 specs codex já implementadas** baixadas (`50fc1d37`); `codex_12` mantido na fila (ver pendências).
7. **Docs de backlog** (para decidir o futuro da fila): `docs/backlog/FEATURES_FUTURAS_RESUMO.md`,
   `docs/backlog/SPECS_NAO_IMPLEMENTADAS_RESUMO.md`, `docs/validation/playmode/PLAYTEST_SIMPLES.md`.

## PENDÊNCIAS (o trabalho a continuar) — em ordem de valor

### A. Zerar/organizar o `a_implementar` (hoje ~106)
- **Mover as 53 `features_futuras/`** para fora da fila ativa (ex.: `docs/backlog/features_futuras/`).
  São futuras por design — a fila "real" cai para ~52. Ver `FEATURES_FUTURAS_RESUMO.md` (inclui
  sobreposições a repensar: as 2 specs de "pesquisa via NPC" 13/22; as 2 de "cave weather deep" waves 15/24).
- **~29 PLAYTEST_ONLY**: código pronto, esperam o **smoke humano** (`PLAYTEST_SIMPLES.md`). Quando o
  usuário reportar OK, baixar (docs-migration) as que passaram: root (cave art×3, city×2, enemy_kits,
  farm×2, town_layout) + `closeout_mvp/SPEC_18..29` + UI 04_ (hud/shop/quest/skill/dialogue/repair).
- **12 NÃO-IMPLEMENTADAS** (ver `SPECS_NAO_IMPLEMENTADAS_RESUMO.md`, todas "IMPLEMENTAR"):
  - 7 UI órfãs (`04_spec_ui_*`): ViewModel/projection existe, falta ligar a **View/Canvas** — use a skill
    `hud-canvas-binding` (fecha o "DEFERRED: binding do canvas"). Headless-viável.
  - 3 farm (`05_spec_*`): código pronto+testado, mas BLOCKED — resolver a cadeia de dependência e wirar
    (ex.: `CreateMvpFarmScene` não referencia `FarmLevel1LayoutContract`).
  - `spec_village_orders_board`: do zero. `spec_town_building_visuals`: precisa de ARTE externa (humano).

### B. Débito de conteúdo (aparece no `Validar Projeto`, NÃO bloqueia o jogo)
- **2 picaretas** (`item_tool_pickaxe_iron/steel`) sem `WeaponId` → dar WeaponId ou reclassificar.
- **Fireball** (`spell_fireball`) sem `ProjectilePrefab` → atribuir um prefab de projétil.
- (Gerar uma spec pequena `spec_content_*` para cada, no padrão `spec-authoring`.)

### C. Resíduos técnicos
- **UAC caso 1** (`DialogueLineCondition`): campos nullable (`Season?`, etc.) NÃO serializam no Unity
  (5 `UAC1001`). É Unity-serializado de verdade (via `DialogueTreeSO`→`ConditionalLines`, populado por
  `TownNpcDialogueLibrary`/`RomanceDialoguePool`). Fix = par `bool HasX`+valor, reescrever `IsMet`/
  `Specificity` + os 2 geradores. **Decisão pendente**: fazer o refactor OU aceitar como residual documentado.
- **CS0649** em `SaveManager.cs:90` (`_corpseRecoveryManager` lido em :267 mas nunca atribuído — sempre
  null). Bug latente pré-existente da modularização. Investigar/remover (há um chip de task para isso).
- **`codex_12`** (`spec_codex_12_dead_code_removal_batch2`): o último delete (`RewardTableDefinition.cs`)
  NÃO pode ser feito — o arquivo também define `RewardGrantResult`, usado em
  `Assets/_Game/Tests/EditMode/Economy/LootTableContractTests.cs`. Extrair `RewardGrantResult` para arquivo
  próprio ANTES de deletar, ou fechar a spec como "1 item won't-do documentado".

## Regras não-negociáveis (resumo — detalhe em `.claude/rules/`)
- `validation-truth`: exit 0 ou não passou; sem claim de PASS sem evidência.
- `subagent-results-not-evidence`: verifique o disco + re-rode o build; não confie na narração.
- `unity-assets`: sem editar `.unity/.prefab/.asset` YAML à mão; assets gerados exigem evidência.
- `editor-generation-orchestration`: geração só pelos 3 comandos canônicos, sem `[MenuItem]` avulso.
- Commits em português. Sem git destrutivo sem autorização.

## Referências rápidas
- Padrão de spec: skill `spec-authoring` + `.specs/_templates/SPEC_DEEP_TEMPLATE.md`.
- Estado do projeto: `docs/project/CURRENT_STATE.md`. Índice de specs: `.specs/SPEC_INDEX.md`.
- Playtest humano: `docs/validation/playmode/PLAYMODE_SMOKE_CHECKLIST_MASTER.md` (completo) e
  `PLAYTEST_SIMPLES.md` (versão direta).
