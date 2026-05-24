# CLAUDE.md Ã¢â‚¬â€ Cindar's Hope

## Contexto do projeto

Jogo 2D pixel art RPG + farm sim desenvolvido em Unity LTS com C#.
Mundo: Vaalara, cidade Cindar's Hope, regiÃƒÂ£o Dornecia.
Arte: Aseprite como ferramenta principal; Pixelorama/LibreSprite como fallback, sprites 32x32px, resoluÃƒÂ§ÃƒÂ£o 1280x720.
GeraÃƒÂ§ÃƒÂ£o de cÃƒÂ³digo: Codex (VS Code) + Claude.
Spec: GitHub SpecKit com fluxo Specify Ã¢â€ â€™ Plan Ã¢â€ â€™ Tasks Ã¢â€ â€™ Implement.

---

## Fluxo operacional

Antes de qualquer tarefa, seguir:

- `docs/operations/AGENT_EXECUTION_PROTOCOL.md`
- `docs/operations/READING_MATRIX.md`

Leitura mÃƒÂ­nima:

- `AGENTS.md` ou `CLAUDE.md`.
- `PROJECT_LOG.md` Ã¢â‚¬â€ somente topo/entradas recentes.
- `docs/IMPLEMENTATION_STATUS.md`.
- `docs/operations/AGENT_EXECUTION_PROTOCOL.md`.

Leitura adicional depende do tipo de tarefa.

NÃƒÂ£o ler por padrÃƒÂ£o:

- `docs_old/**`
- crosswalk completo
- GDD completo
- arquitetura completa
- todos os registries
- todos os refinements
- documentos historicos fora do alvo; usar apenas `docs/specs/` quando a tarefa exigir

Ler esses arquivos apenas quando o protocolo ou a matriz indicar.

Ao finalizar implementaÃƒÂ§ÃƒÂ£o de spec:

- atualizar spec implementada;
- atualizar refinement implementado;
- atualizar registries afetados;
- atualizar maps de refinements afetados;
- atualizar `docs/IMPLEMENTATION_STATUS.md`;
- atualizar `PROJECT_LOG.md`;
- rodar `tools/docs/validate_docs.ps1`.

---

## Estrutura documental ativa

- `docs/design/` Ã¢â‚¬â€ design ativo do jogo.
- `docs/architecture/` Ã¢â‚¬â€ arquitetura ativa.
- `docs/operations/` Ã¢â‚¬â€ instruÃƒÂ§ÃƒÂµes operacionais, ambiente, polÃƒÂ­tica de specs e handoff LLM.
- `docs/roadmap/` Ã¢â‚¬â€ roadmap ativo.
- `docs/backlog/` Ã¢â‚¬â€ backlog e ideias futuras.
- `docs/amendments/` Ã¢â‚¬â€ amendments ativos.
- `docs/validation/` Ã¢â‚¬â€ smoke tests e validaÃƒÂ§ÃƒÂµes.
- `docs/specs/implementados/` Ã¢â‚¬â€ specs consolidadas do que jÃƒÂ¡ existe no repo.
- `docs/specs/a_implementar/` Ã¢â‚¬â€ specs futuras aprovadas ou preparadas.
- `docs/refinements/implementados/` Ã¢â‚¬â€ refinamentos/audits/handoffs de coisas jÃƒÂ¡ implementadas.
- `docs/refinements/a_implementar/` Ã¢â‚¬â€ refinamentos futuros ainda nÃƒÂ£o implementados.
- `docs/specs/` - fonte unica oficial de specs.
- `docs_old/` Ã¢â‚¬â€ histÃƒÂ³rico integral preservado; consultar para auditoria, nÃƒÂ£o editar como fonte ativa.

A pasta raiz `spec/` foi absorvida e nÃƒÂ£o deve ser recriada.

---

## Regra operacional de Git para agentes

Agentes podem preparar commits locais, mas nÃƒÂ£o devem executar operaÃƒÂ§ÃƒÂµes remotas ou destrutivas, salvo pedido humano explÃƒÂ­cito na conversa.

Permitido ao agente:

- criar ou usar branch local indicada pelo humano;
- alterar somente arquivos explicitamente permitidos no escopo da tarefa;
- criar commits locais em portuguÃƒÂªs;
- atualizar `PROJECT_LOG.md` ao final de tarefa relevante;
- atualizar `docs/IMPLEMENTATION_STATUS.md` ao final de tarefa relevante;
- entregar ao humano a lista de commits, arquivos alterados, testes executados e testes pendentes.

Proibido ao agente sem autorizaÃƒÂ§ÃƒÂ£o explÃƒÂ­cita:

- executar `git push`;
- abrir PR/MR;
- fazer merge;
- deletar branches locais ou remotas;
- executar `git stash`;
- executar `git clean`;
- executar `git reset --hard`;
- commitar arquivos fora do escopo permitido.

Push, PR/MR, merge e limpeza de branches sÃƒÂ£o responsabilidade humana por padrÃƒÂ£o, salvo autorizaÃƒÂ§ÃƒÂ£o explÃƒÂ­cita na sessÃƒÂ£o.

---

## Modelo de LLM padrÃƒÂ£o

claude-sonnet-4-6

---

## Regras INVIOLÃƒÂVEIS de cÃƒÂ³digo

1. NUNCA usar `GameObject.Find()` ou `FindObjectOfType()`.
2. NUNCA criar comunicaÃƒÂ§ÃƒÂ£o direta entre sistemas quando a comunicaÃƒÂ§ÃƒÂ£o for de gameplay; usar `GameEventBus.Publish()` e `Subscribe()`.
3. NUNCA hardcodar dados de jogo em `MonoBehaviour` quando forem dados de balanceamento/conteÃƒÂºdo; usar ScriptableObject em `Assets/_Game/Data/`.
4. SEMPRE fazer unsubscribe em `OnDisable` ou `OnDestroy`.
5. NUNCA escrever lÃƒÂ³gica de negÃƒÂ³cio pesada em `MonoBehaviour`; `MonoBehaviour` deve ser ponte Unity/runtime.
6. SEMPRE prefixar ScriptableObjects: `ItemDataSO`, `SeedDataSO`, `ToolDataSO`, `WeaponDataSO`, etc.
7. SEMPRE prefixar eventos: `DayStartedEvent`, `ItemCraftedEvent`, `ToolEquippedEvent`, etc.
8. SEMPRE commits em portuguÃƒÂªs.
9. NUNCA implementar feature sem spec aprovada.
10. Sprites: SEMPRE importar com Filter Mode `Point`, Compression `None`, Generate Mip Maps `false`.
11. DireÃƒÂ§ÃƒÂ£o visual: cozy farm pixel art inspirado por Harvest Moon/Stardew Valley, mas com identidade prÃƒÂ³pria; nÃƒÂ£o copiar assets, personagens, UI ou paleta proprietÃƒÂ¡ria.
12. SEMPRE atualizar `PROJECT_LOG.md` ao final de tarefa relevante.
13. SEMPRE atualizar `docs/IMPLEMENTATION_STATUS.md` ao final de tarefa relevante.
14. Save deve persistir IDs e tipos simples, nunca referÃƒÂªncias Unity.
15. NÃƒÂ£o usar `StreamingAssets` para save editÃƒÂ¡vel; usar `Application.persistentDataPath`.
16. NÃƒÂ£o serializar `ScriptableObject`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, `Collider` ou `Rigidbody` em DTOs de save.

---

## Regras de manutenÃƒÂ§ÃƒÂ£o do tracking

Ao implementar uma spec:

- mover/registrar o item correspondente de pendente/especificado para implementado ou implementado parcial;
- registrar evidÃƒÂªncia curta no repo: arquivo principal, manager, asset, cena ou validator;
- registrar pendÃƒÂªncias reais de validaÃƒÂ§ÃƒÂ£o ou polish;
- nÃƒÂ£o marcar como implementado sem evidÃƒÂªncia;
- manter `docs_old/` intacto.

Ao criar nova spec aprovada:

- adicionar em `docs/specs/a_implementar/spec_*.md`;
- adicionar ou atualizar refinement em `docs/refinements/a_implementar/ref_*.md`;
- adicionar no bloco de pendÃƒÂªncias de `docs/IMPLEMENTATION_STATUS.md`;
- adicionar no `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`;
- nÃƒÂ£o apagar specs antigas aprovadas;
- se uma spec antiga mudar, criar amendment/correction/errata conforme `docs/operations/SPEC_EVOLUTION_POLICY_v1.0.md`.

---

## Regra de namespace Ã¢â‚¬â€ Debug PROIBIDO

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

## Estrutura de cÃƒÂ³digo

```text
Assets/_Game/
Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Data/
Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Scripts/
Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Core/
Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Player/
Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Farm/
Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Cave/
Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Combat/
Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Tools/
Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Equipment/
Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Craft/
Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Companion/
Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ NPC/
Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ UI/
Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Save/
Ã¢â€â€š   Ã¢â€â€Ã¢â€â‚¬Ã¢â€â‚¬ Utils/
Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Scenes/
Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Prefabs/
Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Sprites/
Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Animations/
Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Tilemaps/
Ã¢â€â€Ã¢â€â‚¬Ã¢â€â‚¬ Audio/
```

---

## Regra FASE9F Ã¢â‚¬â€ Cave Stable Run

Antes de qualquer alteraÃƒÂ§ÃƒÂ£o em Cave procedural/stable run, agentes devem ler:

- `docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md`
- `docs/specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md`
- `docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md`
- `docs/refinements/implementados/ref_pr170_192_cave_stable_run_pre_implementation_audit.md`

Regra central:

- `CaveLevel` jÃƒÂ¡ visitado dentro da mesma `CaveRunSeed` deve ser carregado por snapshot.
- `ForwardExit` e `BackExit` nÃƒÂ£o podem regenerar layout, composiÃƒÂ§ÃƒÂ£o de inimigos ou composiÃƒÂ§ÃƒÂ£o de resource nodes.
- Procedural sÃƒÂ³ muda em novo jogo, KO/morte/derrota do personagem ou comando debug explÃƒÂ­cito.
- Enemy count por snapshot novo de level deve ficar entre `12` e `20`.
- Resource node count por snapshot novo de level deve ficar entre `4` e `10`.
- Revisitar level nÃƒÂ£o pode rerollar inimigos, resource nodes, layout, entrada ou saÃƒÂ­da.
- `SaveData` de cave deve persistir snapshots com tipos simples e sem Unity refs.
- `UnityEngine.Camera` deve ser usado explicitamente quando o tipo for a cÃƒÂ¢mera da Unity.
