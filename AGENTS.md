# AGENTS.md — Cindar's Hope

## Contexto do projeto

Jogo 2D pixel art RPG + farm sim desenvolvido em Unity LTS com C#.
Mundo: Vaalara, cidade Cindar's Hope, região Dornecia.
Arte: Aseprite como ferramenta principal; Pixelorama/LibreSprite como fallback, sprites 32x32px, resolução 1280x720.
IA de arte: ChatGPT/DALL-E para conceito e ícones simples; PixelLab/Scenario opcionais para sprites/tilesets; Aseprite obrigatório para acabamento final.
Geração de código: Codex (VS Code) + Claude.
Spec: GitHub SpecKit com fluxo Specify → Plan → Tasks → Implement.

---

## Estado documental atual

A documentação foi reorganizada. Usar a nova estrutura ativa:

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

## Regra operacional de continuidade

Antes de qualquer tarefa, ler obrigatoriamente:

1. `PROJECT_LOG.md` — log operacional, decisões recentes, pendências e próximo passo.
2. `docs/IMPLEMENTATION_STATUS.md` — tracking curto de capacidades/specs implementadas e pendentes.
3. `AGENTS.md` e/ou `CLAUDE.md` — regras permanentes de agente.
4. `docs/specs/SPEC_SOURCE_OF_TRUTH.md` — fonte de verdade das specs/refinements.
5. `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md` — specs já implementadas/parciais.
6. `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md` — specs futuras.
7. `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md` — rastreabilidade entre docs antigos e docs ativos.
8. Documentos específicos da tarefa.

Se o trabalho tocar specs, ler também:

- `docs/operations/SPEC_EVOLUTION_POLICY_v1.0.md`
- spec ativa em `docs/specs/a_implementar/spec_*.md` ou `docs/specs/implementados/spec_*.md`
- refinement correspondente em `docs/refinements/a_implementar/ref_*.md` ou `docs/refinements/implementados/ref_*.md`
- SpecKit operacional em `specs/<FEATURE>/`, se existir.

Ao final de qualquer tarefa relevante, atualizar obrigatoriamente:

- `PROJECT_LOG.md` com branch usada, escopo executado, arquivos alterados, testes executados/não executados, pendências/riscos e próximo passo recomendado.
- `docs/IMPLEMENTATION_STATUS.md` com status curto, verificável e comparável de capacidades/specs.

`PROJECT_LOG.md` é append-only por padrão: não apagar histórico anterior salvo correção factual explícita.

`docs/IMPLEMENTATION_STATUS.md` deve ser curto: marcar `Implementado`, `Implementado parcial`, `Implementado em código — validação Unity pendente`, `Especificado` ou `Pendente`. Se houver dúvida, marcar `Parcial` e registrar pendência.

---

## Fluxo obrigatório para implementar specs

Antes de implementar:

1. Confirmar branch e escopo com o humano ou com a tarefa recebida.
2. Ler `PROJECT_LOG.md`.
3. Ler `docs/IMPLEMENTATION_STATUS.md`.
4. Ler `docs/specs/SPEC_SOURCE_OF_TRUTH.md`.
5. Ler `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`.
6. Ler `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.
7. Localizar a spec em `docs/specs/a_implementar/spec_*.md` ou a spec implementada/parcial em `docs/specs/implementados/spec_*.md`.
8. Localizar o refinement correspondente em `docs/refinements/a_implementar/ref_*.md` ou `docs/refinements/implementados/ref_*.md`.
9. Ler o SpecKit operacional em `specs/<FEATURE>/`, se existir.
10. Validar dependências já implementadas em `docs/specs/implementados/`.
11. Validar histórico em `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md` quando houver dúvida.

Durante a implementação:

- Não implementar nada fora da spec/refinement.
- Não misturar feature grande em PR único se a spec exigir PRs pequenos.
- Se precisar mudar escopo aprovado, criar amendment/correction/errata conforme `docs/operations/SPEC_EVOLUTION_POLICY_v1.0.md` antes de implementar.
- Não marcar nada como implementado sem evidência no repo.

Ao finalizar uma implementação:

1. Criar ou atualizar a spec implementada em `docs/specs/implementados/spec_*.md`.
2. Criar ou atualizar o refinement implementado em `docs/refinements/implementados/ref_*.md`.
3. Marcar a spec futura correspondente em `docs/specs/a_implementar/` como substituída/movida, ou removê-la apenas se o conteúdo tiver sido preservado na spec implementada.
4. Marcar o refinement futuro correspondente em `docs/refinements/a_implementar/` como substituído/movido, ou removê-lo apenas se o conteúdo tiver sido preservado no refinement implementado.
5. Atualizar `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`.
6. Atualizar `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.
7. Atualizar `docs/refinements/implementados/ref_implementados_map.md`.
8. Atualizar `docs/refinements/a_implementar/ref_futuro_map.md`.
9. Atualizar `docs/IMPLEMENTATION_STATUS.md`.
10. Atualizar `PROJECT_LOG.md`.
11. Registrar testes executados e não executados.
12. Se houver migração documental, atualizar `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md`.

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

## Documentos de referência principais

Ler conforme a tarefa:

- `PROJECT_LOG.md`
- `docs/IMPLEMENTATION_STATUS.md`
- `docs/specs/SPEC_SOURCE_OF_TRUTH.md`
- `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`
- `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`
- `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md`
- `docs/operations/SPEC_EVOLUTION_POLICY_v1.0.md`
- `docs/design/GDD_v2.6.md`
- `docs/architecture/ARCH_fase4_v2.2.md`
- `docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md`
- `docs/operations/FASE5_ambiente_v1.2.md`
- `docs/operations/LLM_HANDOFF_INSTRUCTIONS.md`
- `specs/` — SpecKit operacional por feature.

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
