# LLM Handoff Instructions — Cindar's Hope

Este arquivo orienta qualquer LLM/agente que continue o desenvolvimento do projeto.

## Estado atual da documentação

A documentação ativa foi reorganizada.

- `docs/` contém a documentação ativa.
- `docs_old/` preserva o histórico integral e não deve ser editado como fonte ativa.
- `docs/specs/implementados/` contém specs consolidadas do que já existe no repo.
- `docs/specs/a_implementar/` contém specs futuras ou preparadas.
- `docs/refinements/implementados/` contém refinamentos, audits e handoffs implementados.
- `docs/refinements/a_implementar/` contém refinamentos futuros.
- `specs/` contém o SpecKit operacional por feature.
- A pasta raiz `spec/` foi absorvida e não deve ser recriada.

## Ordem obrigatória de leitura

Antes de planejar ou alterar qualquer coisa, leia:

1. `PROJECT_LOG.md`
2. `docs/IMPLEMENTATION_STATUS.md`
3. `AGENTS.md`
4. `CLAUDE.md`
5. `docs/specs/SPEC_SOURCE_OF_TRUTH.md`
6. `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`
7. `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`
8. `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md`

Para tarefas de implementação, leia também:

1. A spec em `docs/specs/a_implementar/spec_*.md` ou `docs/specs/implementados/spec_*.md`.
2. O refinement correspondente em `docs/refinements/a_implementar/ref_*.md` ou `docs/refinements/implementados/ref_*.md`.
3. O SpecKit operacional em `specs/<FEATURE>/`, se existir.
4. `docs/operations/SPEC_EVOLUTION_POLICY_v1.0.md` se houver alteração de spec, amendment, correction ou errata.

Se houver divergência entre documentos antigos e estado real do repositório, considerar como fonte mais confiável:

1. código atual na branch `dev`;
2. `PROJECT_LOG.md` mais recente;
3. `docs/IMPLEMENTATION_STATUS.md`;
4. registries em `docs/specs/`;
5. `AGENTS.md` / `CLAUDE.md`;
6. `docs_old/` como histórico/auditoria.

## Branches

- `main`: base estável.
- `dev`: branch de desenvolvimento.
- `feature/*`, `fix/*`, `docs/*` ou `wave/*`: branches de trabalho.

Nunca trabalhar diretamente em `main`.

Preferir não trabalhar diretamente em `dev`, salvo tarefa documental explícita ou pedido direto do humano.

## Protocolo antes de alterar

Executar:

```powershell
git fetch origin
git checkout dev
git pull origin dev
git status --short
```

Se houver arquivo modificado ou untracked inesperado, parar e reportar.

## Protocolo de tarefa

Toda tarefa deve ter:

- objetivo claro;
- branch esperada;
- arquivos permitidos;
- arquivos proibidos;
- spec lida;
- refinement lido;
- SpecKit operacional lido, quando existir;
- Definition of Done;
- teste Unity esperado ou justificativa de não execução;
- atualização do `PROJECT_LOG.md`;
- atualização do `docs/IMPLEMENTATION_STATUS.md` quando a tarefa mudar status de capacidade/spec.

Não alterar arquivos fora do escopo.

## Fluxo obrigatório para implementar uma spec

Antes de implementar:

1. Ler registries e fonte de verdade em `docs/specs/`.
2. Identificar a spec futura em `docs/specs/a_implementar/`.
3. Identificar o refinement futuro em `docs/refinements/a_implementar/`.
4. Verificar dependências em `docs/specs/implementados/`.
5. Ler `specs/<FEATURE>/`, se existir.
6. Confirmar se há amendment ativo em `docs/amendments/`.
7. Verificar crosswalk em `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md` se houver dúvida histórica.

Durante a implementação:

- Seguir a spec e o refinement.
- Não ampliar escopo sem amendment/correction.
- Não misturar waves grandes em um único PR.
- Registrar limitações reais.

Ao finalizar:

1. Criar/atualizar spec implementada em `docs/specs/implementados/spec_*.md`.
2. Criar/atualizar refinement implementado em `docs/refinements/implementados/ref_*.md`.
3. Atualizar `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`.
4. Atualizar `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.
5. Atualizar `docs/refinements/implementados/ref_implementados_map.md`.
6. Atualizar `docs/refinements/a_implementar/ref_futuro_map.md`.
7. Atualizar `docs/IMPLEMENTATION_STATUS.md`.
8. Atualizar `PROJECT_LOG.md`.
9. Registrar testes executados/não executados.
10. Manter `docs_old/` intacto.

## Arquivos normalmente proibidos salvo pedido explícito

- `Packages/**`
- `ProjectSettings/**`
- `Assets/_Game/Input/**`
- `Assets/_Game/Scripts/Core/Events/**`
- `Assets/_Game/Scripts/Core/GameEventBus.cs`
- `.claude/**`
- `*.sln`
- `*.slnx`

`docs/**` só deve ser alterado em tarefa documental ou quando a implementação mudar o status de specs/refinements.

## Estado implementado/parcial registrado

O estado real curto está em:

- `docs/IMPLEMENTATION_STATUS.md`
- `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`

Resumo atual:

- Farm, Town, Crafting, Save/Load e Cave/Combat MVP existem.
- FASE9E está parcialmente implementada em UI/debug, tools, hotbar, progression e damage.
- FASE9F está parcialmente implementada em cave procedural/resources/stable run/replay.
- FASE9G está parcialmente implementada em boss gates/checkpoints/confinement; bestiary/faction locks/ecology seguem como specs futuras.
- FASE9H, FASE9I, FASE9J, FASE9K e FASE9L estão em `docs/specs/a_implementar/`.
- FASE9L é placeholder controlado e precisa de spec completa antes de implementação.

## Regras técnicas

- Não usar `GameObject.Find`.
- Não usar `FindObjectOfType`.
- Não usar `FindObjectsByType`.
- Não usar `StreamingAssets`.
- Save deve usar IDs e tipos simples.
- Não serializar referências Unity em JSON.
- Não instalar Input System ou Cinemachine sem PR específico.
- Sempre atualizar `PROJECT_LOG.md` ao final de tarefas relevantes.
- Sempre atualizar `docs/IMPLEMENTATION_STATUS.md` quando houver mudança de capacidade/spec.
- Nunca criar namespace `CindarsHope.*.Debug`; usar `Runtime`, `DebugTools`, `Diagnostics` ou `Editor`.

## Encerramento de tarefa

Antes de commit:

```powershell
git status --short
git diff --stat
```

Antes de merge, revisar diff contra `dev`.

Após merge em `dev`:

```powershell
git checkout dev
git pull origin dev
git status --short
```

## Observações atuais

- Unity Play Mode ainda precisa ser executado em tarefa separada para as partes marcadas como `Implementado em código — validação Unity pendente`.
- FASE9L não deve ser implementada direto; precisa virar spec completa.
- Qualquer correção descoberta no Unity deve ser registrada no `PROJECT_LOG.md` e isolada em branch própria, salvo pedido explícito do humano.
