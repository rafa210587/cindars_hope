# Spec Quality Gate — Cindar's Hope

## Regra Central

**Build passing is not enough.**

Uma spec só pode ser `BUILD_VALIDATED` se houver evidência clara de que os critérios centrais da spec foram implementados, auditados e documentados.

---

## Status Permitidos

### Aceitáveis durante execução:
- `BUILD_VALIDATED` — critérios centrais atendidos, report criado, validações passaram
- `BUILD_VALIDATED_WITH_WARNINGS` — núcleo implementado, integração/PlayMode deferida
- `CONTRACT_ONLY` — apenas DTO/model/interface criado, sem integração
- `CONTRACT_ONLY_NEEDS_INTEGRATION` — contrato criado, integração com sistema existente deferida
- `DEFERRED_UI_VISUAL` — lógica pronta, visual/scene/prefab deferido
- `NEEDS_REWORK` — P0/P1 spec não atende critério central
- `BLOCKED` — não pode continuar sem Packages/ProjectSettings/scene/pets/future

### Proibidos durante execução:
- `ACCEPTED` — final acceptance só após todas as fases
- `PLAYMODE_VALIDATED` — PlayMode só em final gate
- `PARTIAL` — ambíguo, use status específico
- `COMPLETE` — ambíguo, use status específico
- `PENDING` — ambíguo, use status específico

---

## Quando Usar BUILD_VALIDATED

Usar **ONLY IF** todos forem verdadeiros:

1. ✓ Execution report individual criado em `docs/validation/<spec_id>_execution_report.md`
2. ✓ Spec inteira foi lida e compreendida
3. ✓ Acceptance criteria foram extraídos e documentados
4. ✓ Sistemas existentes foram auditados (não criados parallelos)
5. ✓ Código criado/reutilizado atende todos os critérios centrais
6. ✓ Spec Compliance Matrix preenchida com status OK (não DEFERRED ou FAIL)
7. ✓ `.\tools\docs\validate_docs.ps1` — PASS
8. ✓ `dotnet build .\Assembly-CSharp.csproj --no-restore` — 0E, 0W (build-related)
9. ✓ `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` — 0E, pre-existing W only
10. ✓ Nenhum arquivo proibido foi alterado (Packages, ProjectSettings, scenes, prefabs, assets)
11. ✓ Testes, se criados, estão ONLY em `Assets/_Game/Tests/EditMode/**`, NEVER em `Assets/_Game/Scripts/**`
12. ✓ Report contém seção "Honest status rationale" explicando por que o status não está inflado

---

## Quando Usar BUILD_VALIDATED_WITH_WARNINGS

Usar quando:
- Núcleo da spec foi implementado e testado
- MAS há integração visual/PlayMode/human validation deferida
- MAS há adapter ainda headless (não sincronizado com ModalManager/GameplayInputRouter/etc)
- MAS há policy/config deferida

Exemplos:
- UI focus routing implementado, PlayMode integration deferred
- Save provider structure created, PlayMode reload testing deferred
- Event system wired, human scenario pending

---

## Quando Usar CONTRACT_ONLY

Usar quando foi criado apenas:
- DTO (data transfer object)
- Model / View-Model
- Enum
- Interface
- Adapter headless (sem sincronização real)
- Projection (read-only model)

Características:
- Compila
- Pode ter testes EditMode
- NÃO integra com sistema existente
- NÃO tem lógica de negócio operacional
- NÃO bloqueia próxima wave se a spec for fundacional

Exemplo: `CalendarDayDetailModel.cs` sem integração com `GameCalendarService` é `CONTRACT_ONLY`.

---

## Quando Usar CONTRACT_ONLY_NEEDS_INTEGRATION

Usar quando:
- Contrato implementado (DTO, model, interface)
- Compila
- MAS depende de integração futura com sistema real

Exemplo: `EquipmentCompareViewModel` sem `EquipmentManager` wiring é `CONTRACT_ONLY_NEEDS_INTEGRATION`.

Bloqueia próxima wave se a atual for fundacional.

---

## Quando Usar DEFERRED_UI_VISUAL

Usar quando:
- Lógica de negócio está pronta
- MAS parte visual/scene/prefab/canvas está fora de escopo

Exemplo: Quest condition evaluation logic ready, but quest log UI screen deferred.

---

## Quando Usar NEEDS_REWORK

Usar quando:
- P0 ou P1 spec não atende critério central
- Implementação é stub/incompleta
- Implementação criou sistema paralelo quando deveria reuser existente

Exemplos:
- Spec exige 10 focus states, código tem 5 → NEEDS_REWORK
- Spec exige modal stack, código não tem stack → NEEDS_REWORK
- Spec exige integração com ModalManager, código cria ModalStackRouter paralelo → NEEDS_REWORK

Bloqueia próxima spec.

---

## Quando Usar BLOCKED

Usar quando continuar exigiria:
- Alteração de Packages/
- Alteração de ProjectSettings/
- Criação de scene/prefab/asset
- Execução de WAVE futura
- Acesso a pets/future/mapped specs
- Rewrite amplo de sistema existente
- Unity Test Runner obrigatório (EditMode não o é)
- PlayMode obrigatório em phase de implementation (deferred é OK)

Bloqueia próxima spec.

---

## Proibições Absolutas

- ❌ Marcar `BUILD_VALIDATED` sem report individual
- ❌ Marcar `BUILD_VALIDATED` só porque compilou (build passing != spec fulfilled)
- ❌ Commitar `.claude/*.lock` (operational artifact)
- ❌ Criar `*Tests.cs` dentro de `Assets/_Game/Scripts/**` (must use `Assets/_Game/Tests/EditMode/**`)
- ❌ Executar próxima spec se a atual for `NEEDS_REWORK` ou `BLOCKED`
- ❌ Avançar wave se houver spec fundacional `CONTRACT_ONLY_NEEDS_INTEGRATION`
- ❌ Mover para `implementados/` se status for:
  - `CONTRACT_ONLY`
  - `CONTRACT_ONLY_NEEDS_INTEGRATION`
  - `DEFERRED_UI_VISUAL`
  - `NEEDS_REWORK`
  - `BLOCKED`
  - `HOLD`
  - `BLOCKED_SCOPE`
  - Future/mapped
  - Pets

---

## Evidência Obrigatória em Todo Report

Todo `*_execution_report.md` deve conter:

1. **Acceptance criteria extracted** — tabela com critérios centrais e evidência
2. **Existing systems audit** — quais sistemas foram encontrados/reutilizados/criados
3. **Spec Compliance Matrix** — mapeamento spec requirement → implementation
4. **Validation** — docs/build/editor/test results
5. **Honest status rationale** — por que o status não está inflado
6. **Remaining work** — o que ficou para fases futuras

---

## Matriz de Decisão: Qual Status Usar

| Situação | Status | Motivo |
|----------|--------|--------|
| Código compila + spec critério central atendido + tests + report | BUILD_VALIDATED | Tudo feito |
| Código compila + núcleo pronto + PlayMode/visual deferred | BUILD_VALIDATED_WITH_WARNINGS | Núcleo OK, UI deferred |
| Apenas DTO/model/enum criado | CONTRACT_ONLY | Sem integração |
| DTO criado + depende de integração futura | CONTRACT_ONLY_NEEDS_INTEGRATION | Contrato OK, integração deferred |
| Lógica pronta + visual/scene deferred | DEFERRED_UI_VISUAL | Lógica OK, visual deferred |
| P0/P1 com critério central não atendido | NEEDS_REWORK | Incompleto |
| Não pode continuar sem Packages/scene/pets | BLOCKED | Bloqueado |

---

## Loop Batch Policy — Up to 10 Specs

`/loop` pode executar batches de até 10 specs via `/execute-spec-strict`, desde que:

### Per-Spec Validation (não ao final do batch)

1. **One spec per iteration** — cada ciclo do loop executa UMA spec exatamente
2. **Individual report** — cada spec gera `*_execution_report.md` individual
3. **Individual validation**:
   - ✓ `docs validation` — PASS (no new errors)
   - ✓ `dotnet build Assembly-CSharp` — 0E, 0W runtime
   - ✓ `dotnet build Assembly-CSharp-Editor` — 0E, pre-existing W only
   - ✓ `check_spec_quality.ps1` — PASS (no critical violations)
4. **Individual commit** — cada spec bem-sucedida faz commit próprio ou para com status claro
5. **Loop stop conditions** — parar IMEDIATAMENTE se:
   - Status é `BLOCKED`
   - Status é `NEEDS_REWORK`
   - Status é `CONTRACT_ONLY_NEEDS_INTEGRATION` em spec fundacional
   - Status é `DEFERRED_UI_VISUAL` em spec fundacional
   - Build falha
   - Docs validation tem erro novo
   - Quality check falha criticamente
   - Arquivo proibido foi alterado (Packages/, ProjectSettings/, scenes, prefabs, assets, runtime)
   - Teste foi criado em pasta errada
   - Report individual não existe
   - Status está inflado (BUILD_VALIDATED sem evidence)

### Batch Configuration

| Wave Type | Recommended Max | Reason |
|-----------|-----------------|--------|
| New/unstable wave | 3 specs | Higher risk, need tight feedback loop |
| Established wave (WAVE 02+) | 10 specs | Patterns known, lower failure rate |
| Homogeneous specs (all UI VMs) | 10 specs | Same pattern repeated, easy to validate |
| Never | >10 specs | Without external code review |

### Batch Status Is NOT Wave Acceptance

Completing 10 specs in a loop does NOT mean `ACCEPTED`.

O batch pode produzir no máximo:
- `BUILD_VALIDATED`
- `BUILD_VALIDATED_WITH_WARNINGS`
- `CONTRACT_ONLY`
- `CONTRACT_ONLY_NEEDS_INTEGRATION`
- `DEFERRED_UI_VISUAL`
- `NEEDS_REWORK`
- `BLOCKED`

`ACCEPTED` e `PLAYMODE_VALIDATED` continuam proibidos durante implementation phase.

### Loop Output Format

Após cada spec no loop, output DEVE conter:

```text
SPEC_RESULT:
Spec:
Status:
Docs validation:
Assembly-CSharp:
Assembly-CSharp-Editor:
Quality check:
Commit:
Can continue next spec: YES/NO
Can start next wave: NO (always no during loop)
```

---

*Created: 2026-06-08 (Spec Quality Gate + Loop Batch Policy)*  
*Applies to all agent-run spec execution tasks.*
