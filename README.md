# Cindar's Hope — 2D Pixel Art RPG + Farm Sim

Um jogo 2D em pixel art combinando simulação de fazenda com exploração, combate, comércio, crafting, cave runs e progressão RPG.

## Estado atual

- **Branch principal de desenvolvimento:** `dev`
- **Estado documental:** reorganizado até FASE9L.
- **Histórico antigo:** preservado em `docs_old/`.
- **SpecKit operacional:** preservado em `specs/`.
- **Specs implementadas/parciais:** `docs/specs/implementados/`.
- **Specs futuras:** `docs/specs/a_implementar/`.
- **Refinamentos implementados:** `docs/refinements/implementados/`.
- **Refinamentos futuros:** `docs/refinements/a_implementar/`.

## Estado implementado/parcial

Resumo curto; detalhes em `docs/IMPLEMENTATION_STATUS.md` e `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`.

- Core/event bus/bootstrap: implementado/parcial.
- Data/IDs/registries/ScriptableObjects: implementado.
- Save/load JSON cross-scene: implementado parcial.
- Inventory/itens/gold/stacks: implementado.
- Farm loop: implementado parcial.
- World pickups persistentes: implementado parcial.
- Economy, hunger, crafting e town: implementado parcial/MVP.
- Combat MVP, damage MVP e enemy data-driven stats: implementado parcial.
- UI/debug, tools, hotbar e progression: implementado parcial.
- Cave runtime/procedural/stable run/boss gates/visual fixes: implementado em código/parcial, com validação Unity pendente em várias partes.
- Validation/process: implementado parcial.

## Specs futuras rastreadas

Detalhes em `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.

- FASE9C remaining: player equipment, items, combat, tools/farm/combat refinement.
- FASE9D: enemy actions, AI, combat e arquitetura de 40+ monstros.
- FASE9E: UI final, attribute allocation, damage/status/elements, item taxonomy, examples, save migration e progression refinada.
- FASE9F: cave resources/encounters complete.
- FASE9G: bestiary, faction locks, portal ecology e amendment de combat roles/AI/status.
- FASE9H: loot, crafting, equipment, durability e environment.
- FASE9I: player combat, weapons, magic e skill actions.
- FASE9J: cave entry, loadout, HUD, death flow, Fonte de Anya e corpse recovery.
- FASE9K: skill trees, nodes, active slots, capstones e respec.
- FASE9L: UI/UX full gameplay — placeholder controlado; precisa virar spec completa antes de implementação.
- Fishing/combat integration final.
- UI/menu systems final.
- Future ideas TODO.

## Estrutura documental ativa

```text
docs/
├── design/                    # GDD, changelog e deltas de design
├── architecture/              # arquitetura e contratos core
├── operations/                # instruções de agentes, ambiente e política de specs
├── roadmap/                   # roadmap ativo
├── backlog/                   # backlog e ideias futuras
├── amendments/                # amendments ativos
├── validation/                # smoke tests e validações
├── specs/
│   ├── implementados/         # spec_*.md do que já existe no repo
│   └── a_implementar/         # spec_*.md futuras/preparadas
└── refinements/
    ├── implementados/         # ref_*.md implementados/audits/handoffs
    └── a_implementar/         # ref_*.md futuros

docs_old/                      # histórico integral preservado
specs/                         # SpecKit operacional por feature
```

A pasta raiz `spec/` foi absorvida e não deve ser recriada.

## Execução por agentes

Para reduzir custo de contexto, agentes devem seguir:

- `docs/operations/AGENT_EXECUTION_PROTOCOL.md`
- `docs/operations/READING_MATRIX.md`

Não ler `docs_old/**`, crosswalk completo, GDD completo ou registries inteiros por padrão.

Para implementar uma spec, ler a spec alvo, o refinement alvo e `specs/<FEATURE>/` quando existir. Ao finalizar, atualizar specs, refinements, registries afetados, `docs/IMPLEMENTATION_STATUS.md` quando necessário e `PROJECT_LOG.md`.

## Git policy para agentes

Permitido:

- Criar/usar branch local indicada.
- Alterar somente arquivos permitidos no escopo.
- Criar commits locais em português.
- Atualizar `PROJECT_LOG.md`.
- Atualizar `docs/IMPLEMENTATION_STATUS.md` quando houver mudança de status.
- Entregar lista de commits, arquivos e testes.

Proibido sem pedido humano explícito:

- `git push`
- abrir PR/MR
- `git merge`
- `git stash`
- `git clean`
- `git reset --hard`
- commitar fora do escopo

## Arquitetura técnica resumida

- Comunicação de gameplay via `GameEventBus.Publish/Subscribe`.
- Persistência com `GameBootstrap` singleton + managers persistentes entre cenas.
- Dados de balanceamento/conteúdo via ScriptableObjects em `Assets/_Game/Data/`.
- Save em JSON com IDs e tipos simples; nunca serializar referências Unity.
- Scene installers e runtime reference installers para rebind cross-scene.
- Não usar `GameObject.Find`, `FindObjectOfType` ou `FindObjectsByType`.
- Nunca criar namespace `CindarsHope.*.Debug`; usar `Runtime`, `DebugTools`, `Diagnostics` ou `Editor`.

## Como abrir no Unity

1. Clone o repositório fora de OneDrive/Dropbox/Google Drive.
2. Abra em Unity LTS.
3. Regere cenas pelos menus `CindarsHope/Scenes/*` se necessário.
4. Abra FarmScene ou CaveScene.
5. Play Mode: WASD move, E interage, Tab avança dia, J ataca.

## Smoke tests principais

Consultar `docs/validation/`.

Checklist mínimo:

- [ ] Farm: plantar seed → passar dias → colher → vender → salvar/carregar.
- [ ] Town: andar, falar com Pip, comprar seeds, voltar à Farm.
- [ ] Cave: ir da Farm, andar na Cave, socar Slime, receber/causar dano, voltar à Farm.
- [ ] Save: salvar em qualquer cena, fechar jogo, reabrir, estado restaurado.
- [ ] HUD: HP, Gold, Hunger, Inventory aparecem e atualizam.
- [ ] Console: sem erro vermelho.

## Documentação principal

- `PROJECT_LOG.md` — log operacional e continuidade.
- `AGENTS.md` — regras para agentes.
- `CLAUDE.md` — regras equivalentes para Claude/Codex.
- `docs/README.md` — mapa da documentação ativa.
- `docs/IMPLEMENTATION_STATUS.md` — status curto de implementação.
- `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md` — rastreabilidade entre histórico e docs ativos.
- `docs/specs/SPEC_SOURCE_OF_TRUTH.md` — fonte de verdade das specs.
- `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md` — registry de specs implementadas/parciais.
- `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md` — registry de specs futuras.
- `docs/operations/AGENT_EXECUTION_PROTOCOL.md` — protocolo operacional enxuto para agentes.
- `docs/operations/READING_MATRIX.md` — matriz de leitura por tipo de tarefa.
- `docs/design/GDD_v2.6.md` — design do jogo.
- `docs/architecture/ARCH_fase4_v2.2.md` — arquitetura técnica.
- `docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md` — contratos core, eventos, IDs e save.
- `docs/operations/LLM_HANDOFF_INSTRUCTIONS.md` — protocolo operacional para agentes.

## Observações

- Unity Play Mode ainda precisa validar as partes marcadas como `Implementado em código — validação Unity pendente`.
- FASE9L não deve ser implementada direto; precisa ser detalhada em spec completa antes de qualquer código.
- `docs_old/` deve permanecer intacto.
