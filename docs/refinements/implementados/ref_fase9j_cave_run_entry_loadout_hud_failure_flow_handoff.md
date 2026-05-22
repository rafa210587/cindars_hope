# REF — FASE9J cave run entry loadout HUD failure flow handoff

> Origem histórica: $Source
> Status: Refinamento implementado / handoff / audit preservado
> Relacionado a:
> - $_

---

# Handoff — FASE9J Cave Run Entry, Loadout, HUD & Failure Flow

> **Data:** 2026-05-20  
> **Branch:** dev  
> **Responsável:** ChatGPT  
> **Escopo:** registrar criação da FASE9J como baseline de fluxo completo da run da cave sem reescrever specs antigas.

---

## 1. Arquivos criados

- `docs/FASE9J_CAVE_RUN_ENTRY_LOADOUT_HUD_AND_FAILURE_FLOW_SPEC_v1.0.md`
- `specs/FASE9J_CAVE_RUN_ENTRY_LOADOUT_HUD_AND_FAILURE_FLOW/spec.md`

---

## 2. Decisões registradas

- Ao morrer na cave, jogador retorna para a Farm na Fonte de Anya.
- Ao morrer, jogador perde itens carregados, equipamentos carregados/equipados, ammo e gold para um corpo recuperável.
- Ao morrer, XP acumulado do nível atual zera, mas o jogador não perde level.
- Só existe um corpo recuperável: o da última morte.
- Nova morte substitui o corpo anterior e o conteúdo anterior é perdido.
- CaveRunSeed continua ao sair vivo.
- CaveRunSeed muda em novo jogo ou morte/KO.
- Corpo recuperável usa `CorpseRecoveryRoom` garantida no mesmo CaveLevel após regeneração da run.
- Jogador só sai da cave por pontos de saída ou item de retorno.
- Existe `Pedra de Retorno de Anya` / return item.
- Entrar por checkpoint começa no level exato do checkpoint em sala segura.
- Jogador pode trocar equipamento e editar hotbar dentro da cave.
- Durabilidade 0 impede uso.
- Boss derrotado libera checkpoint/avanço imediatamente.
- Cave HUD será OnGUI MVP próprio separado do DebugHud.

---

## 3. Hardening aplicado

- Morte não remove level, checkpoints, boss defeated flags, deepest level, recipes, quest flags ou skills.
- Corpo recuperável não depende de Unity refs no save.
- Corpo precisa ser acessível mesmo após nova CaveRunSeed.
- Safe room de checkpoint não tem inimigos nem dano ambiental imediato.
- Warnings de loadout aparecem antes da entrada.
- Return item não conta como morte e não troca CaveRunSeed.
- Key progression unlocks sobrevivem Ã  morte.

---

## 4. Validação

- [x] Documento FASE9J criado no repo.
- [x] SpecKit FASE9J criado no repo.
- [x] Handoff documental criado.
- [ ] Unity não executado; alteração documental/spec.
- [ ] `PROJECT_LOG.md` não foi editado para evitar sobrescrever entradas paralelas recentes.

---

## 5. Próximo passo recomendado

Usar FASE9J como baseline quando a implementação chegar em:

- CaveEntryMenu;
- CaveRunHud;
- Fonte de Anya;
- failure flow;
- corpse recovery;
- return item;
- checkpoint safe room;
- warnings de loadout;
- persistência de CaveRunSeed/corpse.

MVP deve começar com OnGUI simples e save DTOs, não UI final polida.


