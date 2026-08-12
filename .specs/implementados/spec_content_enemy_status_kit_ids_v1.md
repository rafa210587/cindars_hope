# SPEC — Kits de ataque de inimigo referenciam StatusEffect IDs ausentes

> **Spec ID:** `spec_content_enemy_status_kit_ids_v1`
> **Status:** Implementado e UNITY_VALIDATED
> **Evidência (2026-08-12):** `StatusEffectType.Buff` adicionado; 12 status materializados via `GenerateCanonicalStatusEffects` (Inicializar Projeto). `ValidateEnemyAttackKits` 43 erros → **0** (PASSED, 127 checks). EditMode `EnemyKitStatusIdsTests` **12/12**. Suíte EditMode 2858/2858. Commit do batch de execução. Play Mode humano (status aplicando em combate) permanece `DEFERRED_TO_FINAL_VALIDATION` (checklist `spec_validation_human_playmode_smoke_v1`).
> **Wave:** WAVE CONTENT — Integridade de dados de combate
> **Priority:** P2
> **Type:** Data / Content
> **Domain:** Combat
> **Parallelizable:** CONDITIONAL
> **Parallel group:** N/A
> **Can run with:** specs que não toquem StatusEffect nem `Assets/_Game/Data/Enemies/Actions/**`
> **Must not run with:** qualquer spec que edite `StatusEffectDatabaseSO`, o gerador de status, ou os EnemyActionSO
> **Repo lock scope:** `Assets/_Game/Scripts/Combat/Data/StatusEffectDatabaseSO.cs`, `Assets/_Game/Data/Combat/StatusEffects/**`, `Assets/_Game/Data/Enemies/Actions/**`, o gerador de status em `Assets/_Game/Scripts/Editor/**`
> **Depends on:** `docs/project/CURRENT_STATE.md`; `.claude/rules/id-stability.md`
> **Blocks:** validação humana de combate (status visível em Play Mode)
> **Scope:** Fazer os 12 StatusEffect IDs referenciados por 43 EnemyActionSO existirem no `StatusEffectDatabaseSO`, zerando os 43 erros de `ValidateEnemyAttackKits`.
> **Out of scope:** balance dos status; arte/VFX; novos comportamentos de IA; Fireball ProjectilePrefab e WeaponId de picareta (specs próprias).
> **Validation level alvo:** UNITY_VALIDATED (validadores de editor) + EditMode
> **Executor:** Claude | Codex

## 5. Contexto
O `CindarsHope/Validar Projeto` falha em `ValidateEnemyAttackKits` com 43 erros. Débito de conteúdo pré-existente (não é regressão): os kits foram autorados citando variantes "minor"/buffs de status que o `StatusEffectDatabaseSO` nunca recebeu. Fechar isto destrava o gate de validação e a aplicação real de status em combate.

## 6. Problema
43 `EnemyActionSO` em `Assets/_Game/Data/Enemies/Actions/**` referenciam 12 StatusEffect IDs ausentes do `StatusEffectDatabaseSO`. Efeito duplo: (1) `ValidateEnemyAttackKits` retorna exit 1; (2) em runtime o resolver de status não encontra o efeito e o status **nunca é aplicado** (slow/burn/haste etc. não acontecem no combate).

## 7. Objetivo
Ao final, os 12 IDs existem no `StatusEffectDatabaseSO` (via o gerador canônico), `ValidateEnemyAttackKits` retorna 0 erros, e cada kit resolve seu status — sem alterar balance, IA ou arte.

## 8. Fontes obrigatórias lidas
`.claude/rules/id-stability.md`; `.claude/skills/data-catalog-authoring/SKILL.md`; `.claude/skills/editor-validator-authoring/SKILL.md`; `.claude/rules/editor-generation-orchestration.md`; `Assets/_Game/Scripts/Combat/Data/StatusEffectDatabaseSO.cs`; `Assets/_Game/Scripts/Editor/Validation/ValidateEnemyAttackKits.cs`; `Assets/_Game/Scripts/Editor/Combat/ValidateStatusEffectDatabase.cs`.

## 9. Estado atual do repo — Phase 0 (auditado 2026-08-11)
```text
Grep "status_(slow|burn|bleed|chill|poison|confuse|root)_minor|status_(haste|shield|frenzy|regen|guard)"
  em Assets/_Game/Data/Enemies/Actions → 12 IDs / 43 refs:
  slow_minor×10, burn_minor×9, chill_minor×5, haste×5, frenzy×3, poison_minor×3,
  bleed_minor×2, shield×2, confuse_minor×1, guard×1, regen×1, root_minor×1.
Existe: StatusEffectDatabaseSO.cs (Combat/Data). SCHEMA de StatusEffectSO A CONFIRMAR na Fase 0.
Existe: ValidateEnemyAttackKits.cs, ValidateStatusEffectDatabase.cs (editor).
Ausente: os 12 IDs no database.
Não localizado: um StatusEffectCatalog de const dedicado — Fase 0 confirma ONDE os IDs existentes são declarados (const vs asset gerado) e QUEM é o gerador que popula o database.
```
> A Fase 0 de EXECUÇÃO deve: (a) re-rodar o Grep; (b) ler `StatusEffectDatabaseSO.cs` para o schema exato de `StatusEffectSO`; (c) localizar o gerador que materializa os status (buscar em `Assets/_Game/Scripts/Editor/**`). **Decisão da Fase 0:** (A) adicionar as 12 definições no gerador do database — *preferido*, kits ficam intactos; ou (B) remapear os 43 kits para IDs existentes — *só* se o design confirmar que "minor" era alias de um status já existente. **Default = (A).**

## 13. Regras de não duplicação
Não criar novo catálogo/enum de status: estender `StatusEffectDatabaseSO` e o gerador existentes. Não criar `[MenuItem]` avulso — registrar no fluxo `CindarsHope/Inicializar Projeto` (rule `editor-generation-orchestration`).

## 15. Arquitetura alvo — CRIAR vs MODIFICAR
```text
MODIFICAR (caminho A, preferido):
  Assets/_Game/Scripts/Editor/<...>/<GeradorDeStatus>.cs  — adicionar as 12 definições; pattern: (skill: data-catalog-authoring)
  Assets/_Game/Scripts/Combat/Data/StatusEffectDatabaseSO.cs — só se faltar const/acessor de ID (rule id-stability)
CRIAR:
  Assets/_Game/Tests/EditMode/Combat/EnemyKitStatusIdsTests.cs — asserta que cada ID de kit resolve no database
GERADO (evidência, não editar YAML à mão):
  Assets/_Game/Data/Combat/StatusEffects/*.asset — materializados por Inicializar Projeto
```

## 16. Contratos, dados e eventos
### 16.1 Data contracts
```csharp
// IDs canônicos como const (rule id-stability), prefixo status_. Os 12:
public const string StatusSlowMinor    = "status_slow_minor";
public const string StatusBurnMinor    = "status_burn_minor";
public const string StatusChillMinor   = "status_chill_minor";
public const string StatusPoisonMinor  = "status_poison_minor";
public const string StatusBleedMinor   = "status_bleed_minor";
public const string StatusConfuseMinor = "status_confuse_minor";
public const string StatusRootMinor    = "status_root_minor";
public const string StatusHaste = "status_haste";   public const string StatusShield  = "status_shield";
public const string StatusFrenzy= "status_frenzy";  public const string StatusRegen   = "status_regen";
public const string StatusGuard = "status_guard";
// Campos por StatusEffectSO — LER o schema real na Fase 0: { Id, Kind, Magnitude, DurationSeconds, MaxStacks }.
// "minor" = variante fraca da existente (mesma Kind, magnitude/duração menores).
```
### 16.2 Runtime contracts  » N/A — usa o resolver/aplicador de status existente; nenhum contrato novo.
### 16.3 Event contracts  » N/A   ### 16.4 Save contracts  » N/A (status persiste por ID; sem campo novo)   ### 16.5 UI contracts  » N/A

## 17. Sistemas afetados
Combat / StatusEffect database / Enemy attack kits / Editor validation / EditMode tests.

## 18. Arquivos permitidos
```text
Assets/_Game/Scripts/Editor/**                (o gerador de status)
Assets/_Game/Scripts/Combat/Data/StatusEffectDatabaseSO.cs
Assets/_Game/Data/Combat/StatusEffects/**     (data asset gerado)
Assets/_Game/Tests/EditMode/Combat/**
docs/validation/**
```
## 19. Arquivos proibidos
```text
Assets/_Game/Data/Enemies/Actions/**  (salvo caminho B autorizado explicitamente)
Assets/**/*.unity, Assets/**/*.prefab, Packages/**, ProjectSettings/**, docs_old/**
```

## 20. Estratégia de implementação
```md
### Fase 0 — Auditoria: re-rodar Grep §9; ler o schema de StatusEffectSO; localizar o gerador; decidir A vs B.
### Fase 1 — Dados: em `<GeradorDeStatus>.GenerateAll()`, após o bloco dos status existentes, iterar `StatusMinorDefs` chamando `db.AddOrUpdate(new StatusEffectSO{ Id=id, Kind=kind, Magnitude=mag, DurationSeconds=dur, MaxStacks=stacks })`. Consts de ID por id-stability. Pattern: (skill: data-catalog-authoring). Fórmula (pseudo-código):
```
para cada (id, parentId) em StatusMinorDefs:
  parent = db.Get(parentId)            // ex.: "status_burn" para "status_burn_minor"
  mag = parent != null ? round(parent.Magnitude * 0.60) : FALLBACK_MIN_MAG   // minor = 60% do pai
  dur = parent != null ? parent.DurationSeconds        : FALLBACK_MIN_DUR
  Kind = parent?.Kind ?? KindFromIdSuffix(id)          // slow/burn/chill/... ; buffs (haste/shield/frenzy/regen/guard) = Kind Buff
  db.AddOrUpdate(id, Kind, mag, dur, parent?.MaxStacks ?? 1)
```
### Fase 2 — Geração: rodar CindarsHope/Inicializar Projeto (materializa os .asset). Evidência por rule unity-assets.
### Fase 3 — Testes/validação: EditMode EnemyKitStatusIdsTests; rodar CindarsHope/Validar Projeto.
### Fase 4 — Relatório: docs/validation/spec_content_enemy_status_kit_ids_v1_execution_report.md (bloco validation-truth).
```

## 21. Ordem segura de execução
1. Fase 0. 2. Consts + 12 defs no gerador. 3. Inicializar Projeto. 4. EditMode test. 5. Validar Projeto. 6. Relatório.

## 14. Critérios de aceite — binários + evidência
```md
### 14.1 Os 12 IDs existem no database
- Resultado: cada um dos 12 status_* resolve no StatusEffectDatabaseSO.
- Evidência: EditMode EnemyKitStatusIdsTests verde (12 asserts) via RunUnityEditModeTests.ps1, exit 0.
### 14.2 Validador de kits limpo
- Resultado: nenhum erro "StatusApplicationId ... nao existe".
- Evidência: CindarsHope/Validar Projeto → ValidateEnemyAttackKits 0 erros (era 43); log em docs/validation/.
### 14.3 Sem regressão de conteúdo
- Resultado: ValidateStatusEffectDatabase segue 0 erros; nenhum ID existente renomeado/removido.
- Evidência: validador exit 0 + git diff mostra só ADIÇÕES de status_*_minor/buffs.
### 14.4 Compila
- Resultado: Assembly-CSharp + Editor compilam.
- Evidência: RunUnityCompileValidation.ps1 exit 0.
```

## 23. Edge cases / falhas
- **ID colide com status já existente** → usar `AddOrUpdate` (idempotente): atualiza em vez de duplicar; o critério 14.3 pega qualquer renome/remoção acidental via git diff.
- **Variante minor sem parent no database** (ex.: um `*_minor` cujo base não existe) → aplicar `FALLBACK_MIN_MAG`/`FALLBACK_MIN_DUR` const nomeadas (no-magic-values) e LOGAR aviso; não travar a geração.
- **Buffs (haste/shield/frenzy/regen/guard) não têm sufixo `_minor`** → `KindFromIdSuffix` deve mapear esses 5 para `Kind=Buff` explicitamente (tabela), não inferir de "_minor".
- **Caminho B escolhido na Fase 0** (remapear kits) → editar os 43 EnemyActionSO exige autorização de asset (rule unity-assets) + migration se algum ID estiver em save; por isso o default é A.
- **Regeneração via Inicializar Projeto recria cenas** → rodar isolado; confirmar que só os `.asset` de StatusEffects mudaram (git diff), não cenas.
- **Balance:** 60% é placeholder conservador; ajuste fino de balance é `economy-balance-tuning`, fora desta spec.

## 22. Validação e gates
Nível-alvo: UNITY_VALIDATED (validadores editor) + EditMode. Play Mode humano (status aplicando em combate) = `DEFERRED_TO_FINAL_VALIDATION` — residual risk documentado, não PASS (rules validation-truth / testing-quality-gate). Referenciar o checklist `spec_validation_human_playmode_smoke_v1`.
