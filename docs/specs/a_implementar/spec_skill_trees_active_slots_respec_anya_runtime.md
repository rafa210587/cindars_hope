# SPEC - Skill trees, active slots, respec Anya runtime

> Spec ID: spec_skill_trees_active_slots_respec_anya_runtime
> Status: A implementar
> Ordem de execucao: 16
> Depende de: 00-15
> Bloqueia: 17
> Tipo: Runtime/UI
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Completar skill trees, active slots, capstones, save/load e respec na Fonte de Anya.
> Fora de escopo: Implementar gameplay nesta tarefa documental; alterar Assets, Packages, ProjectSettings, docs_old ou codigo C#.

Fontes absorvidas:
- specs/FASE9K_SKILL_TREES_FULL_NODE_AND_UNLOCK/spec.md
- specs/FASE9E_PLAYER_LEVEL_UP_PROGRESSION/spec.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_skill_trees_active_slots_respec_anya.md

---

# /speckit.specify

## Contexto
Cindar's Hope usa Unity LTS, C#, pixel art 2D e fluxo SpecKit. A partir da reconciliacao documental, esta spec vive somente em docs/specs/a_implementar/ e substitui qualquer equivalente que existia em specs/.

## Problema
A area ainda esta parcial, fragmentada ou dependente de skeleton/backend. Sem uma spec consolidada, agentes podem duplicar regras, marcar estado incorreto ou implementar fora de ordem.

## Objetivo
Completar skill trees, active slots, capstones, save/load e respec na Fonte de Anya.

## User stories / engineering stories
- Como jogador, quero que a capacidade funcione de forma previsivel, persistente quando aplicavel e coerente com os demais sistemas.
- Como desenvolvedor, quero contratos claros de dados, eventos, save/load e UI antes de alterar runtime.
- Como agente, devo implementar somente depois que dependencias anteriores estiverem reconciliadas e sem pendencia bloqueadora.

## Criterios de aceite
- A implementacao respeita as regras de codigo do projeto: sem GameObject.Find(), sem FindObjectOfType(), gameplay via GameEventBus, dados de conteudo em ScriptableObject e unsubscribe obrigatorio.
- Save/load usa IDs e tipos simples; nenhum DTO serializa referencias Unity.
- A validacao documental e runtime aplicavel fica registrada em PROJECT_LOG.md, docs/IMPLEMENTATION_STATUS.md, registries e refinements.
- A spec nao e marcada como implementada sem evidencia curta no repo.

---

# /speckit.plan

## Arquitetura
Progression, skills, active slots, Anya, save e UI. devem seguir managers/bridges Unity finos, dados em ScriptableObject e logica de negocio fora de MonoBehaviour pesado.

## Sistemas afetados
Progression, skills, active slots, Anya, save e UI.

## Fluxos
1. Validar dependencias anteriores em docs/specs/SPEC_EXECUTION_ORDER.md.
2. Confirmar estado real no codigo e nos docs implementados.
3. Implementar contratos de dados/eventos/save antes de UX final quando a spec exigir.
4. Registrar evidencias e pendencias reais ao finalizar.

## Dados / DTOs / IDs
Usar IDs estaveis e tipos simples. Conteudo/balanceamento deve ficar em ScriptableObject sob Assets/_Game/Data/ quando houver implementacao futura.

## Eventos
Comunicacao de gameplay deve ocorrer por eventos prefixados, publicados e assinados via GameEventBus.

## Save/load
Persistir somente estado necessario, com schema version/migration quando aplicavel. Nunca serializar ScriptableObject, GameObject, Transform, MonoBehaviour, Sprite, Collider ou Rigidbody.

## UI, se aplicavel
UI deve refletir estado runtime real, sem hardcode de gameplay e sem esconder pendencias de validacao Unity.

## Riscos de regressao
UI final pode expor skill tree incompleta ou sem persistencia.

---

# /speckit.tasks

## Tasks
- [ ] Revalidar estado real do repo antes de alterar runtime.
- [ ] Confirmar dependencias anteriores e pendencias bloqueadoras.
- [ ] Implementar dados, eventos, runtime, save/load e UI conforme escopo.
- [ ] Atualizar spec implementada, refinement implementado, registries, maps, docs/IMPLEMENTATION_STATUS.md e PROJECT_LOG.md.
- [ ] Rodar validacao documental e validacao Unity aplicavel.

## Arquivos permitidos
- Durante implementacao futura: somente arquivos citados pela spec/refinement aprovado e dependencias diretas.

## Arquivos proibidos
- docs_old/** para edicao.
- Alteracoes fora do escopo aprovado.

## Definition of Done
- Criterios de aceite atendidos.
- Evidencia curta registrada.
- Pendencias reais mantidas como pendencias, nao como completo.

## Validacao
- ./tools/docs/validate_docs.ps1
- Validacao Unity local/batchmode ou Play Mode quando a spec envolver runtime.