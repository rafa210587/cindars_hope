# Prompt para Claude Code/Codex — Cindar's Hope

**Use o fluxo oficial:**

```text
/implement-spec spec_11_damage-status-resistances
```

Isso delegará ao comando `/implement-spec` que orquestra validações, regressão e closeout automático.

**Consulte:**
- `.claude/commands/implement-spec.md` — Fluxo completo
- `.claude/skills/spec-execution/SKILL.md` — Pattern de execução

**Regras:** Implementar somente SPEC 11, não ampliar escopo, validação humana no final.


---

# SPEC 11 — Damage, Status, Elements e Resistances

## Branch sugerida

```bash
git checkout dev
git pull
git checkout -b feature/spec-11-damage-status-resistances
```

Se a branch já existir, reutilize-a. Antes de alterar, rode `git status` e registre se o working tree não estiver limpo.

## Spec alvo

```text
docs/specs/implementados/spec_damage_status_elements_resistances_runtime.md
```

## Dependências diretas

Depende de 10. Bloqueia 12, 13 e 14.

## Objetivo desta execução

Fechar pipeline de dano/status/resistência com UI mínima e validação Play Mode de dano, vulnerabilidade, resistência e morte.

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

- UI final de status effects ou contrato claro com SPEC 17.
- Play Mode: dano físico/mágico, resistência, vulnerabilidade, status apply/tick/remove, morte.
- Balanceamento inicial de damage documentado.
- Confirmar que DamageCalculator consulta equipment/resist profiles reais.
- Eliminar duplicidade confusa de managers se não estiver documentada como intencional; se intencional, registrar responsabilidades.

## Áreas permitidas para alteração

- `Assets/_Game/Scripts/Combat/**`
- `Assets/_Game/Scripts/Equipment/**`
- `Assets/_Game/Scripts/Player/**`
- `Assets/_Game/Scripts/UI/**`
- `Assets/_Game/Scripts/Core/Events/**`
- `Assets/_Game/Scripts/Save/**`
- `docs/specs/implementados/spec_damage_status_elements_resistances_runtime.md`

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

- DamageRequest/Result usados por player/enemies/spells.
- Status tem aplicação, refresh, tick, remoção e save/load se aplicável.
- Floating numbers funcionam sem exceptions.
- Resistências/vulnerabilidades alteram o dano observável.

## Validação Unity e Play Mode

Use as skills obrigatórias:

- `SPEC Validation Pattern` para compilar e escanear logs.
- `Scene Wiring Validation Pattern` se tocar managers, cenas, bootstrap, installers ou scripts de criação de cena.
- `Unity Asset Creation Pattern` para assets/SOs/configs.
- `Play Mode Manual Validation Checklist` para registrar o fluxo funcional desta spec.

Formato obrigatório no resumo final:

```text
PLAY MODE TEST: SPEC 11 — Damage, Status, Elements e Resistances
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
git commit -m "feat: finalizar spec 11 - damage status resistances"
```

Não faça push.
