# LLM Handoff Instructions ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â Cindar's Hope

Este arquivo orienta qualquer LLM/agente que continue o desenvolvimento do projeto.

## Estado atual da documentaÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o

A documentaÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o ativa foi reorganizada.

- `docs/` contÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â©m a documentaÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o ativa.
- `docs_old/` preserva o histÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â³rico integral e nÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o deve ser editado como fonte ativa.
- `docs/specs/implementados/` contÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â©m specs consolidadas do que jÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¡ existe no repo.
- `docs/specs/a_implementar/` contÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â©m specs futuras ou preparadas.
- `docs/refinements/implementados/` contÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â©m refinamentos, audits e handoffs implementados.
- `docs/refinements/a_implementar/` contÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â©m refinamentos futuros.
- `docs/specs/` e a fonte unica oficial; a pasta raiz `specs/` foi removida e nao deve ser recriada.
- A pasta raiz `spec/` foi absorvida e nÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o deve ser recriada.

## Ordem obrigatÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â³ria de leitura

Antes de planejar ou alterar qualquer coisa, leia:

1. `PROJECT_LOG.md`
2. `docs/IMPLEMENTATION_STATUS.md`
3. `AGENTS.md`
4. `CLAUDE.md`
5. `docs/specs/SPEC_SOURCE_OF_TRUTH.md`
6. `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`
7. `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`
8. `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md`

Para tarefas de implementaÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o, leia tambÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â©m:

1. A spec em `docs/specs/a_implementar/spec_*.md` ou `docs/specs/implementados/spec_*.md`.
2. O refinement correspondente em `docs/refinements/a_implementar/ref_*.md` ou `docs/refinements/implementados/ref_*.md`.
3. A ordem oficial em `docs/specs/SPEC_EXECUTION_ORDER.md`.
4. `docs/operations/SPEC_EVOLUTION_POLICY_v1.0.md` se houver alteraÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o de spec, amendment, correction ou errata.

Se houver divergÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Âªncia entre documentos antigos e estado real do repositÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â³rio, considerar como fonte mais confiÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¡vel:

1. cÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â³digo atual na branch `dev`;
2. `PROJECT_LOG.md` mais recente;
3. `docs/IMPLEMENTATION_STATUS.md`;
4. registries em `docs/specs/`;
5. `AGENTS.md` / `CLAUDE.md`;
6. `docs_old/` como histÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â³rico/auditoria.

## Branches

- `main`: base estÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¡vel.
- `dev`: branch de desenvolvimento.
- `feature/*`, `fix/*`, `docs/*` ou `wave/*`: branches de trabalho.

Nunca trabalhar diretamente em `main`.

Preferir nÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o trabalhar diretamente em `dev`, salvo tarefa documental explÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â­cita ou pedido direto do humano.

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
- teste Unity esperado ou justificativa de nÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o execuÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o;
- atualizaÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o do `PROJECT_LOG.md`;
- atualizaÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o do `docs/IMPLEMENTATION_STATUS.md` quando a tarefa mudar status de capacidade/spec.

NÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o alterar arquivos fora do escopo.

## Fluxo obrigatÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â³rio para implementar uma spec

Antes de implementar:

1. Ler registries e fonte de verdade em `docs/specs/`.
2. Identificar a spec futura em `docs/specs/a_implementar/`.
3. Identificar o refinement futuro em `docs/refinements/a_implementar/`.
4. Verificar dependÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Âªncias em `docs/specs/implementados/`.
5. Conferir `docs/specs/SPEC_EXECUTION_ORDER.md`.
6. Confirmar se hÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¡ amendment ativo em `docs/amendments/`.
7. Verificar crosswalk em `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md` se houver dÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Âºvida histÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â³rica.

Durante a implementaÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o:

- Seguir a spec e o refinement.
- NÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o ampliar escopo sem amendment/correction.
- NÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o misturar waves grandes em um ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Âºnico PR.
- Registrar limitaÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Âµes reais.

Ao finalizar:

1. Criar/atualizar spec implementada em `docs/specs/implementados/spec_*.md`.
2. Criar/atualizar refinement implementado em `docs/refinements/implementados/ref_*.md`.
3. Atualizar `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`.
4. Atualizar `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.
5. Atualizar `docs/refinements/implementados/ref_implementados_map.md`.
6. Atualizar `docs/refinements/a_implementar/ref_futuro_map.md`.
7. Atualizar `docs/IMPLEMENTATION_STATUS.md`.
8. Atualizar `PROJECT_LOG.md`.
9. Registrar testes executados/nÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o executados.
10. Manter `docs_old/` intacto.

## Arquivos normalmente proibidos salvo pedido explÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â­cito

- `Packages/**`
- `ProjectSettings/**`
- `Assets/_Game/Input/**`
- `Assets/_Game/Scripts/Core/Events/**`
- `Assets/_Game/Scripts/Core/GameEventBus.cs`
- `.claude/**`
- `*.sln`
- `*.slnx`

`docs/**` sÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â³ deve ser alterado em tarefa documental ou quando a implementaÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o mudar o status de specs/refinements.

## Estado implementado/parcial registrado

O estado real curto estÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¡ em:

- `docs/IMPLEMENTATION_STATUS.md`
- `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`

Resumo atual:

- Farm, Town, Crafting, Save/Load e Cave/Combat MVP existem.
- FASE9E estÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¡ parcialmente implementada em UI/debug, tools, hotbar, progression e damage.
- FASE9F estÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¡ parcialmente implementada em cave procedural/resources/stable run/replay.
- FASE9G estÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¡ parcialmente implementada em boss gates/checkpoints/confinement; bestiary/faction locks/ecology seguem como specs futuras.
- FASE9H, FASE9I, FASE9J, FASE9K e FASE9L estÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o em `docs/specs/a_implementar/`.
- FASE9L ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â© placeholder controlado e precisa de spec completa antes de implementaÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o.

## Regras tÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â©cnicas

- NÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o usar `GameObject.Find`.
- NÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o usar `FindObjectOfType`.
- NÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o usar `FindObjectsByType`.
- NÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o usar `StreamingAssets`.
- Save deve usar IDs e tipos simples.
- NÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o serializar referÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Âªncias Unity em JSON.
- NÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o instalar Input System ou Cinemachine sem PR especÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â­fico.
- Sempre atualizar `PROJECT_LOG.md` ao final de tarefas relevantes.
- Sempre atualizar `docs/IMPLEMENTATION_STATUS.md` quando houver mudanÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§a de capacidade/spec.
- Nunca criar namespace `CindarsHope.*.Debug`; usar `Runtime`, `DebugTools`, `Diagnostics` ou `Editor`.

## Encerramento de tarefa

Antes de commit:

```powershell
git status --short
git diff --stat
```

Antes de merge, revisar diff contra `dev`.

ApÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â³s merge em `dev`:

```powershell
git checkout dev
git pull origin dev
git status --short
```

## ObservaÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Âµes atuais

- Unity Play Mode ainda precisa ser executado em tarefa separada para as partes marcadas como `Implementado em cÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â³digo ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â validaÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o Unity pendente`.
- FASE9L nÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o deve ser implementada direto; precisa virar spec completa.
- Qualquer correÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â£o descoberta no Unity deve ser registrada no `PROJECT_LOG.md` e isolada em branch prÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â³pria, salvo pedido explÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â­cito do humano.
