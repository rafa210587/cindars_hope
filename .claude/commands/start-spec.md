# /start-spec

Use ao preparar a implementação de uma spec. Entrega um plano de execução sem tocar em código.

**Argumentos:** `$ARGUMENTS` — número ou nome de arquivo da spec (ex.: `spec_09` ou `spec_09_status_effect_database`)

---

## Objetivo

Entender o escopo, as dependências e os riscos da spec. Entregar um plano conciso. NÃO implementar.

---

## Leitura mínima

1. `CLAUDE.md` — roteamento e stop conditions
2. `docs/project/CURRENT_STATE.md` — fila ativa, blockers
3. `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md` — governança de execução e taxonomia de fases
4. `.specs/SPEC_VALIDATION_MATRIX_MASTER.md` — matriz de requisitos de validação
5. Spec alvo: `.specs/a_implementar/spec_<name>.md`

## Leitura opcional (só se a spec citar)

- Um refinement específico listado no frontmatter `depends_on` ou `required_read` da spec
- Uma spec implementada listada como dependência
- Um documento de arquitetura específico referenciado pela spec

## Não ler por padrão

```
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md
memory/ (a não ser que a tarefa cite um pattern anterior explicitamente)
SPEC_EXECUTION_ORDER.md (use a fila de CURRENT_STATE.md no lugar)
ROADMAP.md
```

---

## Procedimento

1. Leia os arquivos obrigatórios (acima)
2. A partir da spec, identifique:
   - Objetivo e entregáveis
   - Escopo: arquivos permitidos, arquivos proibidos
   - Dependências: blocked? ready?
   - Validações obrigatórias (docs, build, Unity, Play Mode)
   - Skills aplicáveis de `.claude/skills/`
   - Requisitos de Phase 2-3 (esta spec precisa de Unity validators ou Play Mode?)
3. Entregue o plano (ver formato de saída)
4. Pare. Aguarde confirmação humana antes de implementar.

---

## Edições permitidas

Nenhuma. Este command é read-only.

---

## Edições proibidas

- Sem mudanças de código
- Sem mover spec
- Sem atualizar status

---

## Validação

Nenhuma necessária. Este command não altera arquivos.

---

## Quando parar e reportar

- Spec e CURRENT_STATE conflitam (ex.: spec diz que a dependência está done mas CURRENT_STATE diz blocked)
- Spec não está em `a_implementar/` — pode ter sido movida ou nunca esteve lá
- ID da spec não encontrado

---

## Saída esperada

```
Spec: <SPEC_ID> — <Title>
Objective: <1-2 sentences>

Dependencies:
  - <DEP_ID> (<status: ready/blocked>)

Scope:
  Permitted: <list>
  Forbidden: <list>

Validations required:
  - [ ] docs validation (if docs change)
  - [ ] dotnet build runtime (if C# changes)
  - [ ] dotnet build editor (if editor C# changes)
  - [ ] Unity validators (if spec requires Phase 2)
  - [ ] Play Mode (if spec requires Phase 3)

Phase 2-3 requirement: <YES / NO — docs-only spec>

Skills applicable:
  - <skill-name>: <why>

Risks:
  - <risk>

Ready to proceed?
```
