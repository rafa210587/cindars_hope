# Play Mode Scenario — spec_farm_till_anywhere_tilemap

**Spec ID:** `spec_farm_till_anywhere_tilemap`
**Timing:** DEFERRED_TO_FINAL_VALIDATION (apos Spec A ligar o input e configurar a cena)
**Status:** NOT RUN — aguardando input binding e cena configurada.

---

## Pre-requisitos

- [ ] Spec A (`spec_farm_scene_relayout_v4`) aplicada: cena com `FarmTileGrid` configurado via `SetBounds()` e zonas nao-araveis registradas.
- [ ] Input da enxada redirecionado para `FarmTilledSoilService.TillTile(tile sob o jogador)`.
- [ ] `FarmTilesRuntimeBootstrap` (ou equivalente) configurado na cena para registrar as zonas e o SaveManager.FarmTileGrid.
- [ ] Jogo em Play Mode na FarmScene.

---

## Cenario 1 — Arar em qualquer lugar da fazenda

**Objetivo:** Confirmar que a enxada ara qualquer tile livre.

1. Entrar na FarmScene em Play Mode.
2. Mover o jogador para uma area aberta de terra.
3. Equipar a enxada e usar a acao de arar.
4. **Esperado:** O tile sob/a frente do jogador muda para estado "arado" (indicador visual, mesmo que placeholder).
5. Mover para outra posicao e repetir.
6. **Esperado:** Cada tile arado individualmente, sem limitar a posicoes fixas.

---

## Cenario 2 — Nao arar em construcao/agua/montanha

**Objetivo:** Zonas nao-araveis bloqueiam a acao.

1. Mover o jogador para uma construcao (celeiro, casa, etc.).
2. Usar a enxada.
3. **Esperado:** Acao recusada (feedback de "nao pode arar aqui" — recusa via GameEventBus ou log).
4. Mover para perto da agua ou montanha e repetir.
5. **Esperado:** Acao recusada nas duas zonas.

---

## Cenario 3 — Arar dentro da estufa

**Objetivo:** Interior de estufa e aravel.

1. Entrar na estufa (se presente na cena).
2. Usar a enxada no interior.
3. **Esperado:** Tile arado com sucesso.

---

## Cenario 4 — Ciclo completo: plantar / regar / crescer / colher

**Objetivo:** Ciclo de cultivo com os mesmos numeros de hoje.

1. Arar um tile em uma area aberta.
2. Abrir o menu de plantio e plantar uma semente (ex.: cenoura, 3 dias).
3. **Esperado:** Tile muda para estado "plantado seco".
4. Usar o regador no tile.
5. **Esperado:** Tile muda para "plantado regado".
6. Avancar para o proximo dia (se possivel via debug ou espera).
7. Repetir rega por 3 dias.
8. **Esperado:** No 3o dia, tile muda para "pronto para colher".
9. Usar a acao de colher.
10. **Esperado:** Item de colheita adicionado ao inventario; tile volta para "arado seco" (sem regrowth) ou permanece (com regrowth).

---

## Cenario 5 — Save / Load mantem estado dos tiles

**Objetivo:** Persistencia por coordenada sobrevive ao save/load.

1. Arar vários tiles em posicoes diferentes.
2. Plantar semente em um tile, regar em outro.
3. Salvar o jogo (tecla de save ou menu).
4. Fechar e reabrir o Play Mode (simula um load).
5. Carregar o save.
6. **Esperado:** Todos os tiles arados/plantados/regados estao nos mesmos estados e coordenadas.
7. Verificar que tiles que NAO foram arados continuam como terra normal.

---

## Criterios de aceite humano

- [ ] Arar funciona em qualquer tile aravel (nao apenas em posicoes fixas).
- [ ] Zonas bloqueadas (construcao/agua/montanha) recusam a acao.
- [ ] Estufa e aravel.
- [ ] Ciclo plantar/regar/crescer/colher funciona com os mesmos numeros do motor existente.
- [ ] Save/load preserva estado por coordenada de tile.
- [ ] Saves anteriores (sem tiles araveis livres) carregam sem erro.

---

## Notas de execucao

- Timing: este cenario deve ser executado apos a Spec A configurar a cena e o input.
- Evidencia: screenshots ou video mostrando arar em multiplos pontos livres e o ciclo de cultivo.
- Se o indicador visual de tile arado estiver placeholder, isso e aceitavel para esta spec.
