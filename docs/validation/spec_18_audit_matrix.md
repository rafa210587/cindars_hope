# SPEC_18 Baseline Validation and Spec Cleanup - Audit Matrix

**Date:** 2026-06-01  
**Scope:** Validation and cleanup only — no feature implementation, no refactoring, no architecture changes  
**Status:** PRE-EXECUTION AUDIT

---

## 1. Validações a Executar

| # | Validacao | Tipo | Comando/Menu | Bloqueante? | Esperado | Fallback |
|---|-----------|------|--------------|------------|----------|----------|
| 1 | C# Runtime Build | Automated | `dotnet build .\Assembly-CSharp.csproj --no-restore` | SIM | 0E/0W | N/A - FALHA = STOP |
| 2 | C# Editor Build | Automated | `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` | SIM | 0E/0W ou 2W pré-existentes | N/A - FALHA = STOP |
| 3 | Docs Validation | Automated | `tools/docs/validate_docs.ps1` | SIM | ALL PASS | N/A - FALHA = STOP |
| 4 | Repair TownScene | Manual (Unity) | `CindarsHope/Repair/Scenes/Repair TownScene Combat Bootstrap Wiring` | MEDIA | Logs: Starting/Completed | Docs NOT RUN |
| 5 | Project Repair/Validate | Manual (Unity) | `CindarsHope/Repair and Validate Project` | MEDIA | No new errors | Docs NOT RUN |
| 6 | Projectile Prefabs | Manual (Unity) | `CindarsHope/Validate/Combat/Validate Projectile Prefabs` | MEDIA | PASS / 0 ERRORS | Docs NOT RUN |
| 7 | Combat Databases | Manual (Unity) | `CindarsHope/Validate/Combat/Validate Combat Databases` | MEDIA | 0 ERRORS | Docs NOT RUN |
| 8 | Farm Town MVP | Manual (Unity) | `CindarsHope/Advanced/Legacy/Validate/Validate Farm Town MVP` | MEDIA | 0 ERRORS em TownScene/FarmScene | Docs NOT RUN |
| 9 | Cave MVP | Manual (Unity) | `CindarsHope/Advanced/Legacy/Validate/Validate Cave MVP` | MEDIA | 0 ERRORS em CaveScene | Docs NOT RUN |
| 10 | Play Mode TownScene | Manual (Human) | Open TownScene, Play Mode, manual checklist | CRITICA | Nenhum new error no Console | Docs NOT RUN |

---

## 2. Arquivos Que PODEM Ser Alterados

| Arquivo | Razao | Limite |
|---------|-------|--------|
| `PROJECT_LOG.md` | Registrar resumo de execucao | Apenas append de entrada SPEC_18 |
| `docs/IMPLEMENTATION_STATUS.md` | Atualizar status real se mudou | Apenas correções de status factual |
| `docs/specs/a_implementar/reorg/README_STATUS.md` | CRIAR — declarar reorg fechado | Arquivo novo |
| `docs/validation/spec_mvp_closeout_18_baseline_validation_and_spec_cleanup_execution_report.md` | CRIAR — relatório de execucao | Arquivo novo |
| `docs/backlog/reorg_architecture_residual_backlog.md` | Atualizar se itens foram resolvidos | Apenas updates de status residual existente |

---

## 3. Arquivos que NÃO Podem Ser Alterados

| Arquivo | Razao |
|---------|-------|
| `Assets/_Game/Scripts/**` (runtime) | Regra: não alterar gameplay, não refatorar |
| `GameBootstrap.cs` | Regra: não refatorar managers |
| `PlayerAttackController.cs` | Regra: não refatorar combate |
| `SaveManager.cs` | Regra: não refatorar save |
| `Assets/_Game/Scenes/**` (YAML) | Regra: não editar cenas manualmente |
| `Assets/_Game/Data/**` (assets) | Regra: não alterar dados de balanceamento |
| Qualquer arquivo fora de escopo de validacao/cleanup | Regra: SPEC_18 é validacao, nao feature |

---

## 4. Estado Esperado da Repair TownScene

**Antes do Repair:**
- GameBootstrap._manaManager = null
- GameBootstrap._weaponDatabase = null
- GameBootstrap._spellDatabase = null
- GameBootstrap._statusEffectDatabase = null
- CombatRuntimeInstaller logs: "is null" errors

**Depois do Repair:**
- _Bootstrap tem componente ManaManager
- GameBootstrap._manaManager = componente ManaManager
- GameBootstrap._weaponDatabase = WeaponDatabase.asset
- GameBootstrap._spellDatabase = SpellDatabase.asset
- GameBootstrap._statusEffectDatabase = StatusEffectDatabase.asset (ou warning se nao existir)
- CombatRuntimeInstaller logs: "Install completed. ItemDb=ItemDatabase, WeaponDb=WeaponDatabase, SpellDb=SpellDatabase..."
- **Nao aparecem:** "is null" messages

---

## 5. Riscos de Regressao

| Risco | Probabilidade | Mitigacao | Deteccao |
|-------|---------------|-----------|----------|
| Build falha por nova dependencia | BAIXA | Validar .csproj antes de alterar | dotnet build |
| Docs validation falha | BAIXA | Usar templates aprovados | validate_docs.ps1 |
| Unity validators divergem | MEDIA | Validators em codigo/estao provados | Rodar em Unity |
| Play Mode mostra novo erro | MEDIA | Repair é idempotente, nao altera gameplay | Play Mode manual |
| Reorg incompleto causou outra falha | MEDIA | Audit matrix detecta | Validators |

---

## 6. Stop Conditions (Execucao Não Continua Se...)

- [ ] `dotnet build` falha com erro
- [ ] `tools/docs/validate_docs.ps1` falha
- [ ] Repair script não compila
- [ ] Repair falha ao rodar no Unity (se Unity disponível)
- [ ] Play Mode gera NEW critical error (não relacionado a conhecidos)
- [ ] Qualquer arquivo alterado sair do escopo de "validacao/cleanup"

---

## 7. Criterios de Aceite SPEC_18

- [x] Audit matrix criada
- [ ] Build runtime PASS (dotnet)
- [ ] Build editor PASS (dotnet)
- [ ] Docs validation PASS
- [ ] Repair TownScene executado (ou NOT RUN com motivo)
- [ ] Unity validators rodados (ou NOT RUN com motivo)
- [ ] Play Mode testado (ou NOT RUN com motivo)
- [ ] Reorg marcado como CLOSED/DO NOT REEXECUTE
- [ ] SPEC_19 liberada ou bloqueada com motivo claro
- [ ] Relatório completo criado

---

## 8. Decisao Final Esperada

**SPEC_19 Será Liberada Se:**
- Build runtime/editor PASS
- Docs validation PASS
- Repair TownScene não mostrou erros (ou errros foram mitigados)
- Play Mode não mostrou NEW critical errors (conhecidos são OK)
- Nenhum stop condition foi acionado

**SPEC_19 Será Bloqueada Se:**
- Qualquer stop condition foi acionado
- Validators mostraram erro critico não resolvível dentro de SPEC_18
- Reorg ficou incompleto ou instável

---

## Status: READY FOR EXECUTION

Auditoria completada. Próximo: Fase 1 — Validações Automated (dotnet + docs).
