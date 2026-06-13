# Cindar's Hope — Spec Generation Roadmap Testing Quality Gate Addendum

> **Status:** addendum operacional do roadmap master.  
> **Relaciona-se com:** `.specs/SPEC_GENERATION_ROADMAP_MASTER.md`  
> **Spec criada:** `.specs/a_implementar/spec_test_harness_editmode_playmode_quality_gate.md`  
> **Regra criada:** `.claude/rules/testing-quality-gate.md`  
> **Função:** tornar explícito que a fundação de quality gate de testes deve entrar antes das próximas specs runtime em massa.

---

## 1. Ajuste de Wave 01

Adicionar mentalmente à WAVE 01 do roadmap master:

```text
01Q — spec_test_harness_editmode_playmode_quality_gate.md
```

Posição recomendada:

```text
Depois de:
  01_spec_save_restore_order_contract_runtime.md
  01_spec_save_section_ownership_registry.md

Antes de:
  qualquer spec runtime de domínio das Waves 02+.
```

---

## 2. Bloqueio operacional

Nenhuma nova spec runtime das Waves 02+ deve ser executada em massa sem antes fechar ou aceitar explicitamente o risco desta spec:

```text
.specs/a_implementar/spec_test_harness_editmode_playmode_quality_gate.md
```

Motivo:

```text
As próximas specs mexem em save/load, quests, tempo, economia, inventory, combat, UI e bestiary.
Esses sistemas precisam de testes automatizados ou cenários Play Mode claros para evitar regressão silenciosa.
```

---

## 3. Critério de qualidade adicionado ao roadmap

Todo lote futuro de specs deve declarar:

```text
quais specs exigem EditMode tests;
quais specs exigem PlayMode test ou cenário humano;
quais specs exigem regression test;
quais specs aceitam justificativa de não automação;
qual comando de validação será executado;
qual risco residual ficará documentado.
```

---

## 4. Regra de geração de specs

Toda nova spec runtime deve conter uma seção obrigatória:

```md
## Testing Quality Gate

- Changed deterministic logic: YES/NO
- Requires EditMode tests: YES/NO
- Requires PlayMode automated or human scenario: YES/NO
- Requires regression test: YES/NO
- Minimum validation evidence for ACCEPTED: <text>
```

---

## 5. Onde consolidar depois

Quando o roadmap master for revisado, incorporar este addendum diretamente em:

```text
.specs/SPEC_GENERATION_ROADMAP_MASTER.md
```

E então arquivar este addendum em `docs/archive/documentation_reorg/`.
