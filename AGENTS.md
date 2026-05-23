# AGENTS.md — Cindar's Hope

## Contexto do projeto

Jogo 2D pixel art RPG + farm sim desenvolvido em Unity LTS com C#.
Mundo: Vaalara, cidade Cindar's Hope, região Dornecia.
Arte: Aseprite como ferramenta principal; Pixelorama/LibreSprite como fallback, sprites 32x32px, resolução 1280x720.
Geração de código: Codex (VS Code) + Claude.
Spec: GitHub SpecKit com fluxo Specify → Plan → Tasks → Implement.

---

## Fluxo operacional

Antes de qualquer tarefa, seguir:

- `docs/operations/AGENT_EXECUTION_PROTOCOL.md`
- `docs/operations/READING_MATRIX.md`

Leitura mínima:

- `AGENTS.md` ou `CLAUDE.md`.
- `PROJECT_LOG.md` — somente topo/entradas recentes.
- `docs/IMPLEMENTATION_STATUS.md`.
- `docs/operations/AGENT_EXECUTION_PROTOCOL.md`.

Leitura adicional depende do tipo de tarefa.

Não ler por padrão:

- `docs_old/**`
- crosswalk completo
- GDD completo
- arquitetura completa
- todos os registries
- todos os refinements
- todos os arquivos de `specs/`

Ler esses arquivos apenas quando o protocolo ou a matriz indicar.

Ao finalizar implementação de spec:

- atualizar spec implementada;
- atualizar refinement implementado;
- atualizar registries afetados;
- atualizar maps de refinements afetados;
- atualizar `docs/IMPLEMENTATION_STATUS.md`;
- atualizar `PROJECT_LOG.md`;
- rodar `tools/docs/validate_docs.ps1`.

---

## Estrutura documental ativa

- `docs/design/` — design ativo do jogo.
- `docs/architecture/` — arquitetura ativa.
- `docs/operations/` — instruções operacionais, ambiente, política de specs e handoff LLM.
- `docs/roadmap/` — roadmap ativo.
- `docs/backlog/` — backlog e ideias futuras.
- `docs/amendments/` — amendments ativos.
- `docs/validation/` — smoke tests e validações.
- `docs/specs/implementados/` — specs consolidadas do que já existe no repo.
- `docs/specs/a_implementar/` — specs futuras aprovadas ou preparadas.
- `docs/refinements/implementados/` — refinamentos/audits/handoffs de coisas já implementadas.
- `docs/refinements/a_implementar/` — refinamentos futuros ainda não implementados.
- `specs/` — SpecKit operacional por feature.
- `docs_old/` — histórico integral preservado; consultar para auditoria, não editar como fonte ativa.

A pasta raiz `spec/` foi absorvida e não deve ser recriada.

---

## Regra operacional de Git para agentes

Agentes podem preparar commits locais, mas não devem executar operações remotas ou destrutivas, salvo pedido humano explícito na conversa.

Permitido ao agente:

- criar ou usar branch local indicada pelo humano;
- alterar somente arquivos explicitamente permitidos no escopo da tarefa;
- criar commits locais em português;
- atualizar `PROJECT_LOG.md` ao final de tarefa relevante;
- atualizar `docs/IMPLEMENTATION_STATUS.md` ao final de tarefa relevante;
- entregar ao humano a lista de commits, arquivos alterados, testes executados e testes pendentes.

Proibido ao agente sem autorização explícita:

- executar `git push`;
- abrir PR/MR;
- fazer merge;
- deletar branches locais ou remotas;
- executar `git stash`;
- executar `git clean`;
- executar `git reset --hard`;
- commitar arquivos fora do escopo permitido.

Push, PR/MR, merge e limpeza de branches são responsabilidade humana por padrão, salvo autorização explícita na sessão.

---

## Modelo de LLM padrão

claude-sonnet-4-6

---

## Regras INVIOLÁVEIS de código

1. NUNCA usar `GameObject.Find()` ou `FindObjectOfType()`.
2. NUNCA criar comunicação direta entre sistemas quando a comunicação for de gameplay; usar `GameEventBus.Publish()` e `Subscribe()`.
3. NUNCA hardcodar dados de jogo em `MonoBehaviour` quando forem dados de balanceamento/conteúdo; usar ScriptableObject em `Assets/_Game/Data/`.
4. SEMPRE fazer unsubscribe em `OnDisable` ou `OnDestroy`.
5. NUNCA escrever lógica de negócio pesada em `MonoBehaviour`; `MonoBehaviour` deve ser ponte Unity/runtime.
6. SEMPRE prefixar ScriptableObjects: `ItemDataSO`, `SeedDataSO`, `ToolDataSO`, `WeaponDataSO`, etc.
7. SEMPRE prefixar eventos: `DayStartedEvent`, `ItemCraftedEvent`, `ToolEquippedEvent`, etc.
8. SEMPRE commits em português.
9. NUNCA implementar feature sem spec aprovada.
10. Sprites: SEMPRE importar com Filter Mode `Point`, Compression `None`, Generate Mip Maps `false`.
11. Direção visual: cozy farm pixel art inspirado por Harvest Moon/Stardew Valley, mas com identidade própria; não copiar assets, personagens, UI ou paleta proprietária.
12. SEMPRE atualizar `PROJECT_LOG.md` ao final de tarefa relevante.
13. SEMPRE atualizar `docs/IMPLEMENTATION_STATUS.md` ao final de tarefa relevante.
14. Save deve persistir IDs e tipos simples, nunca referências Unity.
15. Não usar `StreamingAssets` para save editável; usar `Application.persistentDataPath`.
16. Não serializar `ScriptableObject`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, `Collider` ou `Rigidbody` em DTOs de save.

---

## Regras de manutenção do tracking

Ao implementar uma spec:

- mover/registrar o item correspondente de pendente/especificado para implementado ou implementado parcial;
- registrar evidência curta no repo: arquivo principal, manager, asset, cena ou validator;
- registrar pendências reais de validação ou polish;
- não marcar como implementado sem evidência;
- manter `docs_old/` intacto.

Ao criar nova spec aprovada:

- adicionar em `docs/specs/a_implementar/spec_*.md`;
- adicionar ou atualizar refinement em `docs/refinements/a_implementar/ref_*.md`;
- adicionar no bloco de pendências de `docs/IMPLEMENTATION_STATUS.md`;
- adicionar no `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`;
- não apagar specs antigas aprovadas;
- se uma spec antiga mudar, criar amendment/correction/errata conforme `docs/operations/SPEC_EVOLUTION_POLICY_v1.0.md`.

---

## Regra de namespace — Debug PROIBIDO

NUNCA criar namespace chamado `Debug` dentro de `CindarsHope.*`.

Proibido:

- `namespace CindarsHope.Cave.Debug`
- `namespace CindarsHope.Core.Debug`
- `namespace CindarsHope.UI.Debug`

Motivo: colide com `UnityEngine.Debug` e quebra chamadas `Debug.Log()`, `Debug.LogWarning()`, `Debug.LogError()`.

Usar alternativas:

- `CindarsHope.Cave.Runtime`
- `CindarsHope.Cave.DebugTools`
- `CindarsHope.Cave.Diagnostics`
- `CindarsHope.Cave.Editor`

---

## Estrutura de código

```text
Assets/_Game/
├── Data/
├── Scripts/
│   ├── Core/
│   ├── Player/
│   ├── Farm/
│   ├── Cave/
│   ├── Combat/
│   ├── Tools/
│   ├── Equipment/
│   ├── Craft/
│   ├── Companion/
│   ├── NPC/
│   ├── UI/
│   ├── Save/
│   └── Utils/
├── Scenes/
├── Prefabs/
├── Sprites/
├── Animations/
├── Tilemaps/
└── Audio/
```

---

## Regra FASE9F — Cave Stable Run

Antes de qualquer alteração em Cave procedural/stable run, agentes devem ler:

- `docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md`
- `specs/FASE9F_CAVE_RESOURCES_ENCOUNTERS/amendments/stable_run_replay.md`
- `docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md`
- `docs/refinements/implementados/ref_pr170_192_cave_stable_run_pre_implementation_audit.md`

Regra central:

- `CaveLevel` já visitado dentro da mesma `CaveRunSeed` deve ser carregado por snapshot.
- `ForwardExit` e `BackExit` não podem regenerar layout, composição de inimigos ou composição de resource nodes.
- Procedural só muda em novo jogo, KO/morte/derrota do personagem ou comando debug explícito.
- Enemy count por snapshot novo de level deve ficar entre `12` e `20`.
- Resource node count por snapshot novo de level deve ficar entre `4` e `10`.
- Revisitar level não pode rerollar inimigos, resource nodes, layout, entrada ou saída.
- `SaveData` de cave deve persistir snapshots com tipos simples e sem Unity refs.
- `UnityEngine.Camera` deve ser usado explicitamente quando o tipo for a câmera da Unity.
