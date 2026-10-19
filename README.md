# Cindar's Hope - 2D Pixel Art RPG + Farm Sim

Jogo 2D em pixel art combinando simulacao de fazenda, exploracao, combate, comercio, crafting, cave runs e progressao RPG.

## Estado atual

- Branch principal de desenvolvimento: `dev`.
- Fonte unica oficial de specs: `.specs/`.
- A pasta raiz `specs/` foi removida e nao deve ser recriada.
- A pasta raiz `spec/` tambem nao deve ser recriada.
- Historico antigo preservado em `docs_old/`.
- Specs implementadas/parciais: `.specs/implementados/`.
- Specs futuras: `.specs/a_implementar/`.
- Ordem oficial de execucao: `.specs/SPEC_EXECUTION_ORDER.md`.
- Pre-refinamentos vivos: `docs/refinements/a_implementar/pre_refinamentos/`.

## Estado implementado/parcial

Resumo curto; detalhes em `docs/IMPLEMENTATION_STATUS.md` e `.specs/SPEC_REGISTRY_IMPLEMENTED.md`.

- Core/event bus/bootstrap: implementado/parcial.
- Data/IDs/registries/ScriptableObjects: implementado.
- Save/load JSON cross-scene: implementado parcial.
- Inventory/itens/gold/stacks: implementado parcial; runtime atual usa `Dictionary<string, int>` como stack agregada por itemId, sem slots reais, multiplas stacks, capacidade final ou UI final.
- Farm loop: implementado parcial.
- World pickups persistentes: implementado parcial.
- Economy, hunger, crafting e town: implementado parcial/MVP.
- Combat MVP, damage MVP e enemy data-driven stats: implementado parcial.
- UI/debug, tools, hotbar e progression: implementado parcial.
- Cave runtime/procedural/stable run/boss gates/visual fixes: implementado em codigo/parcial, com validacao Unity pendente em varias partes.
- Validation/process: implementado parcial.

## Specs futuras rastreadas

A execucao futura deve seguir `.specs/SPEC_EXECUTION_ORDER.md`.

Ordem atual:

1. reconciliacao documental/fonte unica;
2. validacao Unity/cenas/prefabs;
3. save migration;
4. inventory slots/capacity/UI;
5. farm irrigacao/solo/planting UI;
6. world activities/fishing/trees/pickups/loot;
7. economy/shop/stock/pricing/UI;
8. crafting queue/workstations/recipes/UI;
9. town/NPC/dialogue/schedule/quests;
10. hunger/stamina/status balance;
11. equipment/durability/environment/loot runtime;
12. damage/status/elements/resistances runtime;
13. player combat/weapons/spells/skill actions runtime;
14. enemy AI/roster/bestiary/faction locks runtime;
15. cave runtime/generation/checkpoints/boss gates;
16. cave entry/death/Fonte de Anya/corpse recovery;
17. skill trees/active slots/respec/Fonte de Anya;
18. UI/UX full gameplay.

## Estrutura documental ativa

```text
docs/
  design/
  architecture/
  operations/
  roadmap/
  backlog/
  amendments/
  validation/
  specs/
    SPEC_SOURCE_OF_TRUTH.md
    SPEC_REGISTRY_IMPLEMENTED.md
    SPEC_REGISTRY_TO_IMPLEMENT.md
    SPEC_EXECUTION_ORDER.md
    implementados/
    a_implementar/
  refinements/
    implementados/
    a_implementar/
      pre_refinamentos/

docs_old/  # historico integral preservado
```

## Execucao por agentes

Para implementar uma spec:

1. ler a ordem oficial em `.specs/SPEC_EXECUTION_ORDER.md`;
2. ler a spec alvo em `.specs/a_implementar/`;
3. ler o pre-refinamento relacionado, quando existir;
4. ler specs implementadas dependentes diretamente citadas;
5. atualizar tracking ao finalizar.

Nao ler `docs_old/**`, crosswalk completo, GDD completo ou registries inteiros por padrao.

## Git policy para agentes

Permitido:

- criar/usar branch local indicada;
- alterar somente arquivos permitidos no escopo;
- criar commits locais em portugues;
- atualizar `PROJECT_LOG.md`;
- atualizar `docs/IMPLEMENTATION_STATUS.md` quando houver mudanca de status;
- entregar lista de commits, arquivos e testes.

Proibido sem pedido humano explicito:

- `git push`;
- abrir PR/MR;
- `git merge`;
- `git stash`;
- `git clean`;
- `git reset --hard`;
- commitar fora do escopo.

## Arquitetura tecnica resumida

- Comunicacao de gameplay via `GameEventBus.Publish/Subscribe`.
- Persistencia com `GameBootstrap` singleton e managers persistentes entre cenas.
- Dados de balanceamento/conteudo via ScriptableObjects em `Assets/_Game/Data/`.
- Save em JSON com IDs e tipos simples; nunca serializar referencias Unity.
- Scene installers e runtime reference installers para rebind cross-scene.
- Nao usar `GameObject.Find`, `FindObjectOfType` ou `FindObjectsByType`.
- Nunca criar namespace `CindarsHope.*.Debug`; usar `Runtime`, `DebugTools`, `Diagnostics` ou `Editor`.

## Como abrir no Unity

1. Clone o repositorio fora de OneDrive/Dropbox/Google Drive.
2. Abra em Unity LTS.
3. Regere cenas pelos menus `CindarsHope/Scenes/*` se necessario.
4. Abra FarmScene ou CaveScene.
5. Play Mode: WASD move, E interage, Tab avanca dia, J ataca.

## Smoke tests principais

Consultar `docs/validation/`.

Checklist minimo:

- [ ] Farm: plantar seed -> passar dias -> colher -> vender -> salvar/carregar.
- [ ] Town: andar, falar com Pip, comprar seeds, voltar a Farm.
- [ ] Cave: ir da Farm, andar na Cave, socar Slime, receber/causar dano, voltar a Farm.
- [ ] Save: salvar em qualquer cena, fechar jogo, reabrir, estado restaurado.
- [ ] HUD: HP, Gold, Hunger, Inventory aparecem e atualizam.
- [ ] Console: sem erro vermelho.

## Documentacao principal

- `PROJECT_LOG.md` - log operacional e continuidade.
- `AGENTS.md` - regras para agentes.
- `CLAUDE.md` - regras equivalentes para Claude/Codex.
- `docs/README.md` - mapa da documentacao ativa.
- `docs/IMPLEMENTATION_STATUS.md` - status curto de implementacao.
- `.specs/SPEC_SOURCE_OF_TRUTH.md` - fonte de verdade das specs.
- `.specs/SPEC_REGISTRY_IMPLEMENTED.md` - registry de specs implementadas/parciais.
- `.specs/SPEC_REGISTRY_TO_IMPLEMENT.md` - registry de specs futuras.
- `.specs/SPEC_EXECUTION_ORDER.md` - ordem oficial de execucao.
- `docs/operations/AGENT_EXECUTION_PROTOCOL.md` - protocolo operacional para agentes.
- `docs/operations/READING_MATRIX.md` - matriz de leitura por tipo de tarefa.

## Observacoes

- Unity Play Mode ainda precisa validar partes marcadas como `Implementado em codigo - validacao Unity pendente`.
- FASE9L/UI final nao deve ser implementada fora da ordem oficial.
- `docs_old/` deve permanecer intacto.
