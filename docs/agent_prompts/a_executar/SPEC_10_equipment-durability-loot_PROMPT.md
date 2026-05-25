# Prompt para Claude Code/Codex — Cindar's Hope

**Use o fluxo oficial:**

```text
/implement-spec spec_10_equipment-durability-loot
```

Isso delegará ao comando `/implement-spec` que orquestra:
- Preparação da spec
- Implementação
- Validação documental
- Validação Unity (se runtime mudou)
- Revisão de não-regressão
- Closeout (mover spec, atualizar logs/status)

**Consulte:**
- `.claude/commands/implement-spec.md` — Fluxo automático
- `.claude/skills/spec-execution/SKILL.md` — Pattern de execução
- `.claude/skills/unity-validation/SKILL.md` — Validações Unity
- `.claude/skills/non-regression-review/SKILL.md` — Auditoria arquitetônica
- `.claude/skills/implementation-closeout/SKILL.md` — Checklist de encerramento

**Regras desta tarefa:**
- Implementar somente SPEC 10.
- Não ampliar escopo para V2/FULL além do necessário para fechar a spec.
- Não alterar arquivos fora da spec (docs_old/, raiz specs/, etc.).
- Não pedir confirmação humana intermediária (validação humana será no final).
- Só parar antes do final se houver bloqueador real (spec ausente, dependência bloqueada, validação falhando).
- Entregar resumo final com todos os arquivos alterados, validações executadas, pendências.


---

# SPEC 10 — Equipment, Durability, Environment e Loot

## Branch sugerida

```bash
git checkout dev
git pull
git checkout -b feature/spec-10-equipment-durability-loot
```

Se a branch já existir, reutilize-a. Antes de alterar, rode `git status` e registre se o working tree não estiver limpo.

## Spec alvo

```text
docs/specs/implementados/spec_equipment_durability_environment_loot_runtime.md
```

## Dependências diretas

Depende de 07 e 09. Bloqueia 11, 12, 13, 14 e 17.

## Objetivo desta execução

Fechar equipment real: slots, seleção por inventory, durabilidade, repair kit, resistências ambientais e integração com loot/drop.

## Passo 0 — reconciliação obrigatória

Antes de implementar qualquer coisa:

1. Leia a spec alvo inteira.
2. Leia o refinement relacionado, se existir.
3. Leia os arquivos de código reais nas áreas permitidas.
4. Compare o status da spec em:
   - `docs/specs/SPEC_EXECUTION_ORDER.md`
   - `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`
   - `docs/IMPLEMENTATION_STATUS.md`
   - `PROJECT_LOG.md`
5. Se houver contradição, considere o código real como fonte operacional e atualize os documentos no encerramento.
6. Não refaça sistema que já existe; feche os gaps reais.

## Gaps que devem ser fechados ou formalmente reclassificados

- Auditar se o sistema antigo `EquipTool` ainda é fonte primária; se for, migrar para slots reais LeftHand/RightHand sem quebrar compatibilidade.
- UI/painel de equipamento funcional mínimo ou contrato com SPEC 17.
- Selecionar slot abre inventory modal filtrado por item válido.
- Drop de item equipado é transacional e persistente.
- Resistance UI/feedback de resistência ativa.
- Repair kit MVP.
- Durability break publica evento e auto-unequip se necessário.
- Stats/resistências recalculam ao equipar/desequipar/load.
- Play Mode: equipar, usar, danificar, reparar, dropar, salvar/recarregar.

## Áreas permitidas para alteração

- `Assets/_Game/Scripts/Equipment/**`
- `Assets/_Game/Scripts/Inventory/**`
- `Assets/_Game/Scripts/UI/**`
- `Assets/_Game/Scripts/Save/**`
- `Assets/_Game/Scripts/Combat/**`
- `Assets/_Game/Scripts/Core/Events/**`
- `Assets/_Game/Scripts/**/Editor/**`
- `Assets/_Game/Data/Items/**`
- `docs/specs/implementados/spec_equipment_durability_environment_loot_runtime.md`

Pode alterar documentação de status/registry/log necessária para fechar a spec:

```text
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
docs/refinements/implementados/ref_implementados_map.md
docs/refinements/a_implementar/ref_futuro_map.md
```

Não altere `docs_old/**` salvo se a spec pedir explicitamente auditoria histórica; neste caso, apenas referencie, não migre em massa.

## Critérios de aceite

- Equipment slots são fonte de verdade para mãos/armaduras/acessórios.
- Durability e broken state têm efeito real.
- RepairKit restaura durabilidade conforme regra da spec.
- Loot/equipment instances têm IDs persistentes.
- Damage/resistance consome dados de equipment.

## Validação Unity e Play Mode

Use as skills obrigatórias:

- `SPEC Validation Pattern` para compilar e escanear logs.
- `Scene Wiring Validation Pattern` se tocar managers, cenas, bootstrap, installers ou scripts de criação de cena.
- `Unity Asset Creation Pattern` para assets/SOs/configs.
- `Play Mode Manual Validation Checklist` para registrar o fluxo funcional desta spec.

Formato obrigatório no resumo final:

```text
PLAY MODE TEST: SPEC 10 — Equipment, Durability, Environment e Loot
Scene used:
Steps executed:
Expected result:
Observed result:
Bugs found:
Passed: YES/NO/NOT RUN
Evidence:
```

## Commit

Crie commit local em português, por exemplo:

```bash
git add <arquivos>
git commit -m "feat: finalizar spec 10 - equipment durability loot"
```

Não faça push.
