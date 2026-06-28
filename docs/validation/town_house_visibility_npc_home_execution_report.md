# Execution Report — Visibilidade de colisões da cidade + NPC acessível em casa

- **Tipo:** Mudança direta solicitada pelo humano (não-spec); runtime + scene generator.
- **Data:** 2026-06-22
- **Branch:** dev
- **Status honesto:** `BUILD_VALIDATED` — Phase 2 (Unity batchmode) e Phase 3 (Play Mode) PENDENTES (exigem regenerar a cena no Editor).

## Objetivo (pedido do humano)

1. As casas / o ponto de entrada teleportavam o jogador para um "lugar distante" e voltavam — devia parecer a área da casa.
2. Toda área com colisão/travação deve ter um elemento visual (inclusive as bordas da cidade — paredes e árvores).
3. As casas precisam de componentes visuais que travam, garantindo que os NPCs, se estiverem em casa, sejam acessíveis quando o jogador entra.

## Decisões do humano (AskUserQuestion)

- Interiores: **manter teleporte, deixar coeso** (sem troca de cena; corte de câmera).
- NPC em casa: **acessível só no bloco "home"** (de dia ele está na rua trabalhando).

## Acceptance criteria extracted (→ evidência)

| Critério | Implementação | Evidência |
|---|---|---|
| Entrar na casa não parece "ir pra longe" | Câmera corta (snap) no teleporte via `CameraSnapRequestedEvent` em vez de `SmoothDamp` pelo mapa | `CameraFollow2D.HandleSnapRequested`; `DoorInteractable.Interact` publica o evento |
| Bordas da cidade visíveis | `CreateBound` adiciona visual de parede (filho escalado) alinhado ao collider | `CreateMvpTownScene.AddWallVisual` (sortingLayer `Wall`, cor pedra) |
| Paredes internas das casas visíveis | `CreateInteriorWall` ganha o mesmo visual | `AddWallVisual` (cor terrosa) |
| NPC acessível dentro de casa no bloco "home" | Anchor `home` agora aponta para DENTRO do interior da casa mais próxima do NPC | `InteriorCenterForHouseIndex` + `NearestHouseIndexTo`; `NpcScheduleService` teleporta na faixa de interior |
| Entrar na casa do vendedor à noite | Porta não bloqueia mais a ENTRADA; só avisa "loja fechada". Comércio segue travado no NPC | `DoorInteractable.IsShopClosedNow` (informativo); `NpcShopController.Interact` já recusa venda fora de horário |

## Existing systems audit (sistemas auditados — reuso, não criação)

- `GameEventBus` — canal único; novo `CameraSnapRequestedEvent` segue o padrão `OnEnable`/`OnDisable`.
- `NpcScheduleService` / `NpcScheduleBlockResolver` / `NpcScheduleAnchor` — reaproveitados; só o alvo do anchor `home` mudou (rua → interior) + pop imediato na faixa de interior.
- `NpcShopController` — já gateia comércio por horário (`NpcScheduleAvailabilityGate`); por isso foi seguro abrir a porta sem habilitar comércio noturno.
- `TownDistrictLayout` — reaproveitado para reposicionar casas no footprint 48×42.
- Interior center: extraído para `InteriorCenterForHouseIndex` (fonte única usada por porta+morador), eliminando fórmula duplicada.

## Mudança que afeta regra documentada (atenção do humano)

`city_rules` Rule 4 / decision v2 §6.4-A diziam: porta de loja fechada **bloqueia a entrada**. Para cumprir a decisão #2 (NPC acessível em casa no bloco "home", que coincide com o horário em que a loja fecha), a porta passou a **permitir a entrada** e apenas avisar "loja fechada" — a recusa de **comércio** noturno permanece intacta no `NpcShopController`. O validator `ValidateFableCitySchedule` continua satisfeito (dados `IsShopDoor`/`LinkedNpcId` preservados; ≥1 porta com shop-gate; 12 interiores y>+40; ≥3 anchors/NPC). Se o humano preferir manter a porta bloqueando, é reverter o `Interact` de `DoorInteractable`.

## Spec Compliance Matrix

| Requirement | Status |
|---|---|
| Colisões de borda visíveis | OK |
| Colisões de parede interna visíveis | OK |
| Câmera coesa ao entrar/sair | OK |
| NPC dentro do interior no bloco home | OK (Play Mode pendente) |
| Não habilitar comércio noturno | OK (gate no NpcShopController) |

## Validation (validação)

```
Validation method: run_strict_validation.ps1
Docs validation:        PASS
Assembly-CSharp:         PASS (0 erros, 0 avisos)
Assembly-CSharp-Editor:  PASS (0 erros; 3 avisos pré-existentes, não relacionados)
Unity batchmode (cena):  NOT RUN — exige regenerar TownScene via menu CindarsHope (Editor)
Play Mode:               NOT RUN — exige Play Mode após regenerar
```

## Testing Quality Gate

```
Changed runtime code:           YES (CameraFollow2D, DoorInteractable, NpcScheduleService)
Changed deterministic logic:    PARCIAL (helpers privados do gerador: InteriorCenterForHouseIndex, NearestHouseIndexTo)
Changed Unity scene/prefab:     YES (gerador CreateMvpTownScene — cena regenerada pelo menu)
Automated tests added/updated:  NO
Automated tests not added: JUSTIFIED
Automated tests command:        NOT RUN
Manual Play Mode scenario:      docs/validation/playmode/town_house_visibility_npc_home_human_test_scenario.md
Justification if no tests:      Mudança é majoritariamente scene-generator + wiring de runtime que
                                exige inspeção no Editor e Play Mode (justificativa permitida pela
                                testing-quality-gate: "wiring de scene/prefab que exige inspeção").
                                Os helpers determinísticos são private static do gerador (Editor),
                                fora do alcance de EditMode test sem expor API.
Residual risk:                  Posições exatas de anchor/interior e o feel do corte de câmera só se
                                confirmam regenerando a cena e entrando/saindo de casa no Play Mode.
```

## Remaining work (ações no Unity — manuais)

1. Regenerar a cena: menu `CindarsHope/...` → criar MVP TownScene (roda `CreateMvpTownScene`).
2. Rodar o validator `CindarsHope/Validate/Fable City Schedule (fable_11)`.
3. Play Mode: entrar/sair de uma casa (checar corte de câmera + paredes visíveis); avançar para a noite e confirmar o NPC dentro da casa correspondente.
