# Human Play Mode Scenario — spec_cave_visual_polish_runtime (CV04 polimento visual)

> **Spec:** `spec_cave_visual_polish_runtime`
> **Status:** DEFERRED_TO_FINAL_VALIDATION (Play Mode humano pendente).
> **Escopo:** aproxima a cave da keyart `art/world_gpt/raw/cave_guides/gpt_cave_keyart_stone_cavern.png`
> com 4 frentes puramente visuais (nenhum gerador procedural tocado): (1) borda de rocha arredondada nas
> paredes, (2) cascalho denso no chão, (3) musgo/vegetação na base de parede, (4) vinheta + feixe de luz.

---

## PRÉ-REQUISITOS (confirmar no Unity Editor ANTES de testar)

1. **Rodar `CindarsHope/Inicializar Projeto` 1x** — passo OBRIGATÓRIO desta spec (diferente de CV01-CV03,
   que já tinham os assets regenerados por sessões anteriores). Isso:
   - Preenche `CaveBiomeArtProfile_biome_stone_cavern.asset` com os 4 pools/sprite NOVOS: `WallEdgeSprites`
     (`wall_edge_top/side/corner_a/corner_b.png`), `GroundScatterSprites` (9 peças `litter_*.png`),
     `WallSurfaceSprites` (`litter_moss.png` + `litter_mushrooms_small.png`), `LightShaftSprite`
     (`light_shaft.png`).
   - Sem esse passo, `TryGetWallEdgeSprite`/`TryGetGroundScatterSprite`/`TryGetWallSurfaceSprite`/
     `TryGetLightShaftSprite` retornam `false` (pool vazio) e a cave fica visualmente IDÊNTICA ao estado
     pré-CV04 — não é bug, é o fallback null-safe esperado.
2. `CindarsHope/Validar Projeto`:
   - Confirmar que `ValidateCaveBiomeArtProfiles` não reporta ERROR novo (WARNINGs de pool vazio em
     biomas 2-8 são esperados — só o bioma 1 tem arte).
3. Confirmar que a `CaveScene` tem `CaveRuntimeMaterializer` wireado com `_biomeArtProfiles` e
   `_ecosystemBalance` (dependência pré-existente de CV01/fable_78; se ausente, nada desta spec aparece).
4. Unity Test Runner → EditMode → suíte `Cave` → confirmar `CaveVisualPolishOverlayTests` (14/14),
   `CaveVisualPolishSpritePoolTests` (12/12), e que `CaveDecorContextTests` (agora com 6 testes a mais)
   continua passando.

---

## Setup do teste

1. Novo jogo (ou save existente — nenhum snapshot precisa ser invalidado; `GenerationConfigVersion` não
   mudou nesta spec, ver execution report).
2. Entrar na `CaveScene` e descer até um nível do bioma 1 (`biome_stone_cavern`, níveis 1-10).

## Scenario A — Borda de rocha arredondada (critério 14.1)

1. Observar as paredes das salas, especialmente onde a parede encontra o chão (face sul-voltada) e os
   cantos internos das salas.
2. **EXPECT:** uma peça de contorno (`wall_edge_top`/`wall_edge_side`/`wall_edge_corner_a`/`_corner_b`)
   aparece SOBREPOSTA à massa de parede nas células de borda, dando um acabamento arredondado/detalhado
   em vez da parede em bloco reto anterior.
3. **EXPECT:** em corredores laterais (parede com chão a leste OU oeste, não só ao sul), a peça lateral
   aparece — e no lado oeste ela deve estar ESPELHADA horizontalmente em relação ao lado leste.
4. **FAIL** se: a borda aparecer flutuando longe da parede, colidir com o player (não deveria ter
   `BoxCollider2D` — a colisão da parede continua vindo do collider original, inalterado), ou o miolo de
   parede (sem nenhum vizinho andável) também receber a peça (só a borda deveria).
5. **Revisita:** sair e voltar à mesma sala (ou salvar/recarregar) — a MESMA peça de borda deve aparecer
   na MESMA célula (stable-run).

## Scenario B — Cascalho denso no chão (critério 14.2)

1. Observar o chão aberto das salas (longe de parede).
2. **EXPECT:** muitas células de chão (aprox. 1/3) exibem uma peça pequena de cascalho/detrito
   (`litter_pebbles`/`rocks`/`crack_a`/`gravel`/`moss`/`mushrooms_small`/`rock_single`/`bone`/`crack_b`),
   dando ao chão uma leitura mais texturizada/densa do que antes desta spec.
3. **FAIL** se: o cascalho bloquear o movimento do player (não deveria ter collider — deve ser possível
   andar livremente sobre qualquer peça de cascalho) ou se aparecer em cima de props grandes já
   existentes de forma visualmente quebrada (sobreposição leve é aceitável, já que é decor miúdo de
   fundo).
4. **Revisita:** mesma célula mantém o mesmo cascalho (ou ausência dele) entre visitas.

## Scenario C — Musgo/vegetação na base de parede (critério 14.3)

1. Observar as bases das paredes (onde a parede encosta no chão, em qualquer direção).
2. **EXPECT:** ALGUMAS células de parede (poucas — chance baixa por design, ~12%) exibem musgo
   (`litter_moss`) ou cogumelo pequeno (`litter_mushrooms_small`) na base, dando uma sensação de
   vegetação/umidade na caverna.
3. **FAIL** se: TODA parede tiver musgo (densidade calibrada errada — reportar como ajuste de
   `CaveEcosystemBalanceSO.WallSurfaceChance`, não bug) ou se o musgo bloquear passagem.

## Scenario D — Vinheta + feixe de luz (critério 14.4)

1. Percorrer o nível do centro até as bordas externas do GRID do nível (não da tela/câmera).
2. **EXPECT:** conforme o player se aproxima da borda externa do nível gerado, a cena fica
   progressivamente mais escura (vinheta) — o centro do nível permanece claro/normal.
3. **EXPECT:** perto da entrada do nível (ponto de spawn/entrada), 1 sprite de feixe de luz translúcido
   (`light_shaft`) é visível, dando uma sensação de luz entrando de cima.
4. **FAIL** se: a vinheta cobrir a cena inteira (deveria ser só perto das bordas), obscurecer
   completamente a jogabilidade no centro do nível, ou não houver NENHUM escurecimento perceptível nas
   bordas. Também FAIL se o feixe de luz aparecer longe da entrada ou não aparecer de forma alguma no
   bioma 1 (com `LightShaftSprite` preenchido).
5. **Performance:** a vinheta/feixe NÃO devem ter custo por-frame perceptível (são overlays estáticos
   criados 1x por materialização) — não deve haver flicker ou update visível ao longo do tempo.

## Scenario E — Fallback preservado em biomas sem arte nova (regressão)

1. Descer a um bioma sem arte de polimento visual (bioma 2+, ex. `biome_forest`).
2. **EXPECT:** a cave continua funcionando normalmente (Tilemap/decor CV01-CV03 preservados); nenhum
   overlay de borda/cascalho/musgo/vinheta/feixe aparece (pools vazios = fallback silencioso), sem
   nenhum erro/exception.
3. **FAIL** se: aparecer erro de null-reference relacionado a `CaveWallEdgeKind`, `GroundScatterSprites`,
   `WallSurfaceSprites` ou `CaveVignetteController`.

## Scenario F — Não-bloqueio (anti-regressão, crítico)

1. Tentar andar por TODAS as células de chão de uma sala inteira, incluindo onde há cascalho/musgo.
2. **EXPECT:** nenhuma célula de chão fica bloqueada por causa do cascalho; nenhuma célula de parede
   ganha uma passagem nova por causa do musgo/borda (a colisão da parede continua sendo a mesma
   `BoxCollider2D` de sempre).
3. **FAIL** se: o player ficar preso, ou conseguir atravessar uma parede que antes bloqueava.

---

## Resultado esperado consolidado

| Critério da spec | Cenário | Resultado esperado |
|---|---|---|
| 14.1 Borda de rocha arredondada | A | Overlay determinístico nas células parede-encosta-chão; revisita idêntica |
| 14.2 Cascalho denso, não bloqueia | B + F | ~1/3 do chão aberto com litter; player anda livremente |
| 14.3 Musgo na base de parede | C | Poucas células de parede com moss/mushroom (chance baixa) |
| 14.4 Vinheta + feixe | D | Bordas do nível escurecem progressivamente; 1 feixe perto da entrada |
| Anti-regressão | E + F | Fallback intacto em biomas sem arte; nenhum bloqueio/desbloqueio indevido |

Se qualquer FAIL acima ocorrer, registrar no execution report
(`docs/validation/spec_cave_visual_polish_execution_report.md`) como regressão e não promover a spec
além de `BUILD_VALIDATED`.
