# Cindar's Hope ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â 2D Pixel Art RPG + Farm Sim

Um jogo 2D em pixel art combinando simulaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o de fazenda com exploraÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o, combate, comÃƒÆ’Ã‚Â©rcio, crafting, cave runs e progressÃƒÆ’Ã‚Â£o RPG.

## Estado atual

- **Branch principal de desenvolvimento:** `dev`
- **Estado documental:** reorganizado atÃƒÆ’Ã‚Â© FASE9L.
- **HistÃƒÆ’Ã‚Â³rico antigo:** preservado em `docs_old/`.
- **Fonte unica de specs:** `docs/specs/`; a pasta raiz `specs/` foi removida e nao deve ser recriada.
- **Specs implementadas/parciais:** `docs/specs/implementados/`.
- **Specs futuras:** `docs/specs/a_implementar/`.
- **Refinamentos implementados:** `docs/refinements/implementados/`.
- **Refinamentos futuros:** `docs/refinements/a_implementar/`.

## Estado implementado/parcial

Resumo curto; detalhes em `docs/IMPLEMENTATION_STATUS.md` e `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`.

- Core/event bus/bootstrap: implementado/parcial.
- Data/IDs/registries/ScriptableObjects: implementado.
- Save/load JSON cross-scene: implementado parcial.
- Inventory/itens/gold/stacks: implementado parcial; usa `Dictionary<string, int>` como stack agregada por itemId, sem slots reais/UI final.
- Farm loop: implementado parcial.
- World pickups persistentes: implementado parcial.
- Economy, hunger, crafting e town: implementado parcial/MVP.
- Combat MVP, damage MVP e enemy data-driven stats: implementado parcial.
- UI/debug, tools, hotbar e progression: implementado parcial.
- Cave runtime/procedural/stable run/boss gates/visual fixes: implementado em cÃƒÆ’Ã‚Â³digo/parcial, com validaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o Unity pendente em vÃƒÆ’Ã‚Â¡rias partes.
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
- FASE9L: UI/UX full gameplay ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â placeholder controlado; precisa virar spec completa antes de implementaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o.
- Fishing/combat integration final.
- UI/menu systems final.
- Future ideas TODO.

## Estrutura documental ativa

```text
docs/
ÃƒÂ¢Ã¢â‚¬ÂÃ…â€œÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ design/                    # GDD, changelog e deltas de design
ÃƒÂ¢Ã¢â‚¬ÂÃ…â€œÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ architecture/              # arquitetura e contratos core
ÃƒÂ¢Ã¢â‚¬ÂÃ…â€œÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ operations/                # instruÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Âµes de agentes, ambiente e polÃƒÆ’Ã‚Â­tica de specs
ÃƒÂ¢Ã¢â‚¬ÂÃ…â€œÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ roadmap/                   # roadmap ativo
ÃƒÂ¢Ã¢â‚¬ÂÃ…â€œÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ backlog/                   # backlog e ideias futuras
ÃƒÂ¢Ã¢â‚¬ÂÃ…â€œÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ amendments/                # amendments ativos
ÃƒÂ¢Ã¢â‚¬ÂÃ…â€œÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ validation/                # smoke tests e validaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Âµes
ÃƒÂ¢Ã¢â‚¬ÂÃ…â€œÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ specs/
ÃƒÂ¢Ã¢â‚¬ÂÃ¢â‚¬Å¡   ÃƒÂ¢Ã¢â‚¬ÂÃ…â€œÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ implementados/         # spec_*.md do que jÃƒÆ’Ã‚Â¡ existe no repo
ÃƒÂ¢Ã¢â‚¬ÂÃ¢â‚¬Å¡   ÃƒÂ¢Ã¢â‚¬ÂÃ¢â‚¬ÂÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ a_implementar/         # spec_*.md futuras/preparadas
ÃƒÂ¢Ã¢â‚¬ÂÃ¢â‚¬ÂÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ refinements/
    ÃƒÂ¢Ã¢â‚¬ÂÃ…â€œÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ implementados/         # ref_*.md implementados/audits/handoffs
    ÃƒÂ¢Ã¢â‚¬ÂÃ¢â‚¬ÂÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ a_implementar/         # ref_*.md futuros

docs_old/                      # histÃƒÆ’Ã‚Â³rico integral preservado
docs/specs/                    # fonte unica oficial de specs
```

A pasta raiz `spec/` foi absorvida e nÃƒÆ’Ã‚Â£o deve ser recriada.

## ExecuÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o por agentes

Para reduzir custo de contexto, agentes devem seguir:

- `docs/operations/AGENT_EXECUTION_PROTOCOL.md`
- `docs/operations/READING_MATRIX.md`

NÃƒÆ’Ã‚Â£o ler `docs_old/**`, crosswalk completo, GDD completo ou registries inteiros por padrÃƒÆ’Ã‚Â£o.

Para implementar uma spec, ler somente a spec alvo em `docs/specs/`, o refinement/map relacionado e a ordem oficial em `docs/specs/SPEC_EXECUTION_ORDER.md`. Ao finalizar, atualizar specs, refinements, registries afetados, `docs/IMPLEMENTATION_STATUS.md` quando necessÃƒÆ’Ã‚Â¡rio e `PROJECT_LOG.md`.

## Git policy para agentes

Permitido:

- Criar/usar branch local indicada.
- Alterar somente arquivos permitidos no escopo.
- Criar commits locais em portuguÃƒÆ’Ã‚Âªs.
- Atualizar `PROJECT_LOG.md`.
- Atualizar `docs/IMPLEMENTATION_STATUS.md` quando houver mudanÃƒÆ’Ã‚Â§a de status.
- Entregar lista de commits, arquivos e testes.

Proibido sem pedido humano explÃƒÆ’Ã‚Â­cito:

- `git push`
- abrir PR/MR
- `git merge`
- `git stash`
- `git clean`
- `git reset --hard`
- commitar fora do escopo

## Arquitetura tÃƒÆ’Ã‚Â©cnica resumida

- ComunicaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o de gameplay via `GameEventBus.Publish/Subscribe`.
- PersistÃƒÆ’Ã‚Âªncia com `GameBootstrap` singleton + managers persistentes entre cenas.
- Dados de balanceamento/conteÃƒÆ’Ã‚Âºdo via ScriptableObjects em `Assets/_Game/Data/`.
- Save em JSON com IDs e tipos simples; nunca serializar referÃƒÆ’Ã‚Âªncias Unity.
- Scene installers e runtime reference installers para rebind cross-scene.
- NÃƒÆ’Ã‚Â£o usar `GameObject.Find`, `FindObjectOfType` ou `FindObjectsByType`.
- Nunca criar namespace `CindarsHope.*.Debug`; usar `Runtime`, `DebugTools`, `Diagnostics` ou `Editor`.

## Como abrir no Unity

1. Clone o repositÃƒÆ’Ã‚Â³rio fora de OneDrive/Dropbox/Google Drive.
2. Abra em Unity LTS.
3. Regere cenas pelos menus `CindarsHope/Scenes/*` se necessÃƒÆ’Ã‚Â¡rio.
4. Abra FarmScene ou CaveScene.
5. Play Mode: WASD move, E interage, Tab avanÃƒÆ’Ã‚Â§a dia, J ataca.

## Smoke tests principais

Consultar `docs/validation/`.

Checklist mÃƒÆ’Ã‚Â­nimo:

- [ ] Farm: plantar seed ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬â„¢ passar dias ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬â„¢ colher ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬â„¢ vender ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬â„¢ salvar/carregar.
- [ ] Town: andar, falar com Pip, comprar seeds, voltar ÃƒÆ’Ã‚Â  Farm.
- [ ] Cave: ir da Farm, andar na Cave, socar Slime, receber/causar dano, voltar ÃƒÆ’Ã‚Â  Farm.
- [ ] Save: salvar em qualquer cena, fechar jogo, reabrir, estado restaurado.
- [ ] HUD: HP, Gold, Hunger, Inventory aparecem e atualizam.
- [ ] Console: sem erro vermelho.

## DocumentaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o principal

- `PROJECT_LOG.md` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â log operacional e continuidade.
- `AGENTS.md` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â regras para agentes.
- `CLAUDE.md` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â regras equivalentes para Claude/Codex.
- `docs/README.md` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â mapa da documentaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o ativa.
- `docs/IMPLEMENTATION_STATUS.md` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â status curto de implementaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o.
- `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â rastreabilidade entre histÃƒÆ’Ã‚Â³rico e docs ativos.
- `docs/specs/SPEC_SOURCE_OF_TRUTH.md` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â fonte de verdade das specs.
- `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â registry de specs implementadas/parciais.
- `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â registry de specs futuras.
- `docs/operations/AGENT_EXECUTION_PROTOCOL.md` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â protocolo operacional enxuto para agentes.
- `docs/operations/READING_MATRIX.md` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â matriz de leitura por tipo de tarefa.
- `docs/design/GDD_v2.6.md` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â design do jogo.
- `docs/architecture/ARCH_fase4_v2.2.md` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â arquitetura tÃƒÆ’Ã‚Â©cnica.
- `docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â contratos core, eventos, IDs e save.
- `docs/operations/LLM_HANDOFF_INSTRUCTIONS.md` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â protocolo operacional para agentes.

## ObservaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Âµes

- Unity Play Mode ainda precisa validar as partes marcadas como `Implementado em cÃƒÆ’Ã‚Â³digo ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â validaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o Unity pendente`.
- FASE9L nÃƒÆ’Ã‚Â£o deve ser implementada direto; precisa ser detalhada em spec completa antes de qualquer cÃƒÆ’Ã‚Â³digo.
- `docs_old/` deve permanecer intacto.
