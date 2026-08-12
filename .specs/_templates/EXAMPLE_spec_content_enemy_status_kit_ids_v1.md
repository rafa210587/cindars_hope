# EXEMPLO (referência de profundidade) — não executar como spec ativa

> Este arquivo é o **exemplo-âncora** citado pela skill `spec-authoring`. Mostra o nível-alvo de densidade. Para virar spec ativa, copie para `.specs/a_implementar/`, e a Fase 0 de execução **reconfirma** os números (podem ter mudado). Escrito a partir de uma Phase 0 real de 2026-08-11.

---

# SPEC — Kits de ataque de inimigo referenciam StatusEffect IDs ausentes

> **Spec ID:** `spec_content_enemy_status_kit_ids_v1`
> **Status:** A implementar
> **Wave:** WAVE CONTENT — Integridade de dados de combate
> **Priority:** P2   » (bloqueia o gate `Validar Projeto` e quebra aplicação de status em combate)
> **Type:** Data / Content
> **Domain:** Combat
> **Parallelizable:** CONDITIONAL
> **Parallel group:** N/A
> **Can run with:** specs que não toquem StatusEffect nem `Assets/_Game/Data/Enemies/Actions/**`
> **Must not run with:** qualquer spec que edite `StatusEffectDatabaseSO`, o gerador de status, ou os EnemyActionSO
> **Repo lock scope:** `Assets/_Game/Scripts/Combat/Data/StatusEffectDatabaseSO.cs`, `Assets/_Game/Data/Combat/StatusEffects/**`, `Assets/_Game/Data/Enemies/Actions/**`
> **Depends on:** `docs/project/CURRENT_STATE.md`; `.claude/rules/id-stability.md`
> **Blocks:** validação humana de combate (status visível em Play Mode)
> **Scope:** Fazer os 12 StatusEffect IDs referenciados por 43 EnemyActionSO existirem no `StatusEffectDatabaseSO`, zerando `ValidateEnemyAttackKits`.
> **Out of scope:** balance dos status; arte/VFX; novos comportamentos de IA; Fireball ProjectilePrefab e WeaponId de picareta (specs próprias).
> **Validation level alvo:** UNITY_VALIDATED (validadores de editor) + EditMode
> **Executor:** Claude | Codex

## 5. Contexto
O `Validar Projeto` falha em `ValidateEnemyAttackKits` (43 erros). Débito de conteúdo pré-existente (não regressão): os kits foram autorados citando variantes "minor"/buffs de status que o `StatusEffectDatabaseSO` nunca recebeu. Destrava o gate de validação e a aplicação real de status em combate.

## 6. Problema
43 `EnemyActionSO` em `Assets/_Game/Data/Enemies/Actions/**` referenciam 12 StatusEffect IDs distintos ausentes do `StatusEffectDatabaseSO` → o validador falha (exit 1) e, em runtime, o `IStatusApplier` não resolve o efeito, então o status **nunca é aplicado** (slow/burn/haste etc. não acontecem).

## 7. Objetivo
Ao final, os 12 IDs existem no `StatusEffectDatabaseSO` (via o gerador canônico), `ValidateEnemyAttackKits` retorna 0 erros e cada kit resolve seu status — sem alterar balance, IA ou arte.

## 8. Fontes obrigatórias lidas
`.claude/rules/id-stability.md`; `.claude/skills/data-catalog-authoring/SKILL.md`; `.claude/skills/editor-validator-authoring/SKILL.md`; `Assets/_Game/Scripts/Combat/Data/StatusEffectDatabaseSO.cs`; `Assets/_Game/Scripts/Editor/Validation/ValidateEnemyAttackKits.cs`; `Assets/_Game/Scripts/Editor/Combat/ValidateStatusEffectDatabase.cs`.

## 9. Estado atual do repo — Phase 0 (auditado 2026-08-11)
```text
Comando: Grep "status_(slow|burn|bleed|chill|poison|confuse|root)_minor|status_(haste|shield|frenzy|regen|guard)" em Assets/_Game/Data/Enemies/Actions
Resultado (12 IDs, 43 refs): slow_minor×10, burn_minor×9, chill_minor×5, haste×5, frenzy×3,
  poison_minor×3, bleed_minor×2, shield×2, confuse_minor×1, guard×1, regen×1, root_minor×1.
Existe: StatusEffectDatabaseSO.cs (Combat/Data) — schema de StatusEffectSO A CONFIRMAR na Fase 0 (campos: id, tipo, magnitude, duração, stack?).
Existe: ValidateEnemyAttackKits.cs e ValidateStatusEffectDatabase.cs (editor validators).
Ausente: os 12 IDs no database. Não há StatusEffectCatalog de const dedicado localizado — Fase 0 confirma ONDE os IDs existentes são declarados (const vs asset gerado).
```
» A Fase 0 de EXECUÇÃO deve re-rodar o Grep e ler `StatusEffectDatabaseSO.cs` para o schema exato + descobrir o **gerador** que popula o database (buscar em `Assets/_Game/Scripts/Editor/**` por quem escreve StatusEffect). **Decisão da Fase 0:** (A) adicionar as 12 definições no gerador do database [preferido: kits ficam intactos], ou (B) remapear os 43 kits para IDs existentes [só se o design disser que "minor" era alias]. Default = (A).

## 13. Regras de não duplicação
Não criar novo catálogo/enum de status: estender o `StatusEffectDatabaseSO` e seu gerador existentes. Não criar `[MenuItem]` avulso — registrar no fluxo `CindarsHope/Inicializar Projeto` (rule `editor-generation-orchestration`).

## 15. Arquitetura alvo — CRIAR vs MODIFICAR
```text
MODIFICAR (caminho A, preferido):
  Assets/_Game/Scripts/Editor/<...>/<GeradorDeStatus>.cs  — adicionar as 12 definições (id + campos do schema); pattern: (skill: data-catalog-authoring)
  Assets/_Game/Scripts/Combat/Data/StatusEffectDatabaseSO.cs — só se faltar const/acessor de ID (rule id-stability)
CRIAR:
  Assets/_Game/Tests/EditMode/Combat/EnemyKitStatusIdsTests.cs — asserta que cada ID de kit resolve no database
GERADO (evidência, não editar YAML à mão):
  Assets/_Game/Data/Combat/StatusEffects/*.asset — materializados por Inicializar Projeto
```

## 16. Contratos, dados e eventos
### 16.1 Data contracts
```csharp
// IDs canônicos como const (rule id-stability), prefixo status_. Ex.:
public const string StatusSlowMinor = "status_slow_minor";
// … 12 no total. Campos por StatusEffectSO (LER schema real na Fase 0): { Id, Kind, Magnitude, DurationSeconds, MaxStacks }
```
### 16.2 Runtime contracts  » N/A — nenhum contrato runtime novo; usa o `IStatusApplier`/resolver existente.
### 16.3 Event contracts  » N/A
### 16.4 Save contracts  » N/A (status runtime já persiste por ID; nenhum campo novo)
### 16.5 UI contracts  » N/A

## 17. Sistemas afetados
Combat / StatusEffect database / Enemy attack kits / Editor validation / EditMode tests.

## 18. Arquivos permitidos
```text
Assets/_Game/Scripts/Editor/**              (o gerador de status)
Assets/_Game/Scripts/Combat/Data/StatusEffectDatabaseSO.cs
Assets/_Game/Data/Combat/StatusEffects/**   (data asset gerado)
Assets/_Game/Tests/EditMode/Combat/**
docs/validation/**
```
## 19. Arquivos proibidos
```text
Assets/_Game/Data/Enemies/Actions/**  (salvo caminho B autorizado)
Assets/**/*.unity, Assets/**/*.prefab, Packages/**, ProjectSettings/**, docs_old/**
```

## 20. Estratégia de implementação
```md
### Fase 0 — Auditoria: re-rodar o Grep §9; ler StatusEffectDatabaseSO.cs (schema); localizar o gerador; escolher A vs B.
### Fase 1 — Dados: adicionar as 12 definições no gerador (caminho A) com magnitude/duração conservadoras espelhando a variante não-minor existente — pattern: (skill: data-catalog-authoring). Consts de ID por id-stability.
### Fase 2 — Geração: rodar `CindarsHope/Inicializar Projeto` (materializa os .asset). Evidência de geração por rule unity-assets.
### Fase 3 — Testes/validação: EditMode EnemyKitStatusIdsTests (cada um dos 12 IDs resolve). Rodar `CindarsHope/Validar Projeto`.
### Fase 4 — Relatório: docs/validation/spec_content_enemy_status_kit_ids_v1_execution_report.md (bloco validation-truth).
```

## 21. Ordem segura de execução
1. Fase 0 (grep + schema + gerador + decisão). 2. Consts + 12 defs no gerador. 3. Inicializar Projeto. 4. EditMode test. 5. Validar Projeto. 6. Relatório.

## 14. Critérios de aceite — binários + evidência
```md
### 14.1 Os 12 IDs existem no database
- Resultado: cada um dos 12 status_* resolve no StatusEffectDatabaseSO.
- Evidência: EditMode `EnemyKitStatusIdsTests` verde (12 asserts) via RunUnityEditModeTests.ps1, exit 0.

### 14.2 Validador de kits limpo
- Resultado: nenhum "StatusApplicationId ... nao existe".
- Evidência: `CindarsHope/Validar Projeto` → ValidateEnemyAttackKits 0 erros (era 43); log em docs/validation/.

### 14.3 Sem regressão de conteúdo
- Resultado: ValidateStatusEffectDatabase segue 0 erros; nenhum ID existente renomeado/removido.
- Evidência: validador exit 0 + git diff mostra só ADIÇÕES de status_*_minor/buffs.

### 14.4 Compila
- Resultado: Assembly-CSharp + Editor compilam.
- Evidência: RunUnityCompileValidation.ps1 exit 0.
```

## 22. Validação e gates
Nível-alvo: UNITY_VALIDATED (validadores editor) + EditMode. Play Mode humano (ver status aplicando em combate) = `DEFERRED_TO_FINAL_VALIDATION` — declarado como residual risk, não como PASS (rule validation-truth / testing-quality-gate).
