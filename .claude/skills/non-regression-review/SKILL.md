---
name: non-regression-review
description: Audita o diff de uma implementação em busca de violações arquiteturais e riscos de regressão. Usar após implementar uma spec, antes do closeout, ou sempre que scope/rules mudaram.
---

# Skill: Non-Regression Review

Auditar mudanças antes de fechar uma spec. Produz um report `PASS / WARNING / FAIL` com evidência.

## Quando usar

- Após a implementação de qualquer spec de runtime/código.
- Antes do closeout (`/finish-spec`, `implementation-closeout`).
- Quando scope ou rules mudaram durante a implementação.

## Quando NÃO usar

- Specs docs-only ou asset-only sem mudança de código → pular; usar o checklist de docs governance.
- Como substituto de `run_strict_validation.ps1` — esta skill é auditoria de padrões, não de build.

---

## Checklist de auditoria (9 dimensões)

**1. File & Directory**
```
[ ] Nenhum diretório specs/ ou spec/ criado na raiz
[ ] Nenhuma edição em docs_old/**
[ ] Mudanças dentro do scope declarado pela spec
```

**2. Git Safety**
```
[ ] Nenhum git push, reset --hard, clean, stash executado sem autorização
[ ] Branch e commits esperados (rule: no-unsafe-git)
```

**3. Runtime / Forbidden APIs** (grep nos arquivos mudados)
```
[ ] Sem GameObject.Find / FindObjectOfType / FindObjectsByType em código novo
[ ] Sem chamadas diretas cross-system (ex.: enemy.TakeDamage() de fora do combat)
[ ] Toda comunicação de gameplay via GameEventBus.Publish()
```

**4. Save DTO Safety**
```
[ ] Nenhum campo Unity ref em save DTOs: sem ScriptableObject, Transform, MonoBehaviour, Sprite
[ ] Save usa apenas int, string, float, bool, enum, IDs
```

**5. Balance Values**
```
[ ] Sem literais numéricos de balance inline em métodos (rule: no-magic-balance-values)
[ ] Thresholds/custos em SO de balance ou const nomeada
```

**6. Event Bus**
```
[ ] Todo Subscribe tem Unsubscribe correspondente em OnDisable/OnDestroy
[ ] Eventos carregam IDs/primitivos — sem refs Unity
[ ] Naming: [Noun][Verb]Event
```

**7. Namespaces**
```
[ ] Nenhum namespace CindarsHope.Debug criado
```

**8. Integridade de status**
```
[ ] Spec não marcada ACCEPTED/PLAYMODE_VALIDATED sem evidência
[ ] Nenhum claim de "100% fulfilled" sem evidência no repo
```

**9. Testing Quality Gate** (rule: testing-quality-gate)
```
[ ] Mudança de lógica determinística tem EditMode tests ou justificativa
[ ] Mudança de UI/scene tem human Play Mode scenario ou justificativa
```

---

## Formato de report

```text
Non-Regression Review
─────────────────────
Spec: <id>
Arquivos mudados: <N> (.cs), <N> (docs)

File & Directory:  PASS / FAIL — <detalhe>
Git Safety:        PASS / FAIL
Runtime APIs:      PASS / FAIL — <grep result ou "nenhuma ocorrência nova">
Save DTOs:         PASS / N/A
Balance Values:    PASS / FAIL — <arquivo:linha se falhou>
Event Bus:         PASS / N/A
Namespaces:        PASS
Status claims:     PASS / FAIL
Testing QG:        PASS / JUSTIFIED / FAIL — <justificativa ou caminho do scenario>

Status geral: PASS | WARNING | FAIL

Issues encontrados:
  (lista ou "nenhum")

Ações corretivas:
  (lista ou "nenhuma")

Risco residual:
  (texto explícito)
```

**FAIL bloqueia closeout.** WARNING documenta risco residual e deve ser revisado pelo humano.

## Relacionados

- `(skill: implementation-closeout)` — chama esta auditoria como pré-requisito
- `(rule: unity-architecture)` — items 3, 4, 6
- `(rule: no-magic-balance-values)` — item 5
- `(rule: testing-quality-gate)` — item 9
- `(rule: validation-truth)` — item 8
