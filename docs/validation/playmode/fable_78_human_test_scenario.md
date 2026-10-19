# Human Play Mode Scenario — fable_78 Caverna Viva (Ecossistema + Conflito Inter-Monstro)

> **Spec:** `fable_78_spec_cave_ecosystem_population_runtime`
> **Status:** DEFERRED_TO_FINAL_VALIDATION (Play Mode não rodado na sessão de implementação)
> **Slices 1-6 lado-código completas.** Esta validação humana exige primeiro os pré-requisitos
> DEFERRED_UNITY abaixo (geração de assets + wiring na cena), que SÓ podem ser feitos no Unity Editor.

---

## PRÉ-REQUISITOS DEFERRED_UNITY (fazer no Unity Editor ANTES de testar)

Sem estes passos, o ecossistema não materializa e o conflito é no-op seguro (logado). Faça nesta ordem:

1. **Gerar os data assets** (menus de editor — rode todos; idempotentes, best-effort):
   - `CindarsHope/Cave/Ecosystem/Generate Ecosystem Balance` → cria `Assets/_Game/Data/Cave/CaveEcosystemBalance.asset`.
   - `CindarsHope/Cave/Ecosystem/Generate Biome Ore Nodes` → cria `ResourceNode_ore_*` (por banda) + `ResourceNodeDatabase.asset`.
   - `CindarsHope/Cave/Ecosystem/Generate Environment Element Profiles` → cria 7 `cave_elem_profile_*` + `CaveEnvironmentElementDatabase.asset`.
   - (Opcional) `CindarsHope/Cave/Ecosystem/Generate Wounded Status Effect (optional)` — **NÃO necessário**: o
     "Ferido" runtime é uma janela transitória em `EnemyHealth` que lê `WoundedDefenseMultiplier` do balance SO;
     este gerador só cria um SO canônico para uso futuro e NÃO o registra no database.
2. **Rodar o validator** `CindarsHope/Validation/Validate Cave Ecosystem (fable_78)` → EXIGIR `Errors=0`.
   Warnings sobre database/itens são aceitáveis se os geradores acima rodaram; `Errors>0` é falha.
3. **Subir a versão de geração** (invalida snapshots legados → regeneração determinística limpa):
   no asset `CaveGenerationConfig_Default.asset`, incrementar `GenerationConfigVersion` (o default em
   código já foi bumpado nas slices 2-3; o **asset** precisa refletir o novo valor).
4. **Wire no materializer da CaveScene** (`CaveRuntimeMaterializer`):
   - `_ecosystemBalance` ← `CaveEcosystemBalance.asset`
   - `_environmentElementDatabase` ← `CaveEnvironmentElementDatabase.asset`
   - `_decorElementPrefab` ← prefab de decor placeholder (pedra/fungo/cristal/destroço — pode ser um quad simples)
   - `_waterTilePrefab` ← prefab de tile de lago placeholder
   - Garantir o `ResourceNodeDatabase.asset` ligado onde os nós mineráveis são resolvidos.
5. **Rodar o EditMode Test Runner** (Window → General → Test Runner → EditMode) na suíte `Cave`:
   `CaveMapSizeScalingTests`, `CaveEnvironmentElementPlannerTests`, `CaveThreatBudgetTests`,
   `CaveEcosystemConflictPlannerTests`, `InterMonsterCombatTests`, `CaveWanderingMerchantStockTests`,
   `CaveSaveBackCompatTests` → todas PASS.
6. **Replay validator stable-run** (FASE9F) → PASS (composição/posições/IDs estáveis na revisita).

> Se algum gerador/validator/Test Runner não puder rodar (Unity lock, licença, timeout): registrar
> `NOT RUN` com motivo e risco residual no execution report (rule validation-truth) — nunca converter em PASS.

---

## Setup do teste

1. Novo jogo (para garantir `CaveRunSeed` limpo) ou save com a cave acessível.
2. Player com **picareta** (Pickaxe) de tiers variados no inventário — para minerar veios de banda profunda
   (Diamond para mithril, Gold para cristal arcano).
3. Entrar na CaveScene. Confirmar nos logs que o materializer encontrou balance + database (sem
   "wiring-error" de `_ecosystemBalance`/`_environmentElementDatabase` null).

## Scenario A — Elementos temáticos materializam por bioma (14.2)

1. Descer pelas bandas: Stone (1-10) → Fungal (11-25) → Ice (26-40) → Fire (41-55) → Ruins (56-70) →
   Deep (71-85) → Void (86+).
2. EXPECT por banda (placeholders por família/bioma são aceitáveis — esta spec não entrega arte final):
   - **Stone/Fungal:** pedras + veios de cobre/ferro; Fungal adiciona fungos/cogumelos (decor).
   - **Ice:** **lago** (tile de água) + cristais + veios de ferro/prata.
   - **Fire:** veios de obsidiana/lava (decor bloqueante) + minério ígneo (ferro/prata).
   - **Ruins:** destroços (decor) + prata + cristal arcano raro.
   - **Deep:** lago profundo + mithril/arcane raros.
   - **Void:** mithril/arcane muito raros (sem água).
3. EXPECT: **pedra/decor + pelo menos 1 veio minerável em TODA banda**.
4. EXPECT: o caminho entrada↔saída **nunca** é bloqueado por elementos (consegue sempre atravessar).

## Scenario B — Minério minerável e depleção persistente (14.2)

1. Num nível com veio de minério, equipar a picareta do tier exigido e minerar.
2. EXPECT: o veio entrega o ore canônico correto (ex.: `item_material_copper_ore` no Stone,
   `item_material_mithril_ore` no Deep/Void) e deplete após os hits requeridos.
3. Sair do nível (forward/back exit) e **voltar** ao mesmo nível.
4. EXPECT: o veio minerado continua **depletado** (estado persistido via `CaveLootSnapshotService`);
   posições/tipos dos elementos **idênticos** à primeira visita (stable-run).

## Scenario C — Lago habilita criatura aquática (14.3)

1. Encontrar um nível com lago (banda Ice; eventualmente Deep).
2. EXPECT: inimigos `IsAquatic=true` do roster podem aparecer nesse nível.
3. Num nível **sem** lago, EXPECT: nenhum inimigo aquático.

## Scenario D — Mapa cresce com profundidade + desafio mínimo + entrada segura (14.1, 14.4)

1. Comparar visualmente um nível de banda Stone (~60×60) com um de banda Deep/Void (~90×90).
2. EXPECT: mapa maior em profundidade; densidade distribuída **por sala** (mapas grandes NÃO ficam vazios/esparsos).
3. EXPECT: **nenhum nível regular sai vazio** (há sempre desafio mínimo da banda).
4. Ao **entrar** num nível denso: EXPECT nenhum inimigo/hazard colado no spawn de entrada (raio seguro —
   sem envelopamento imediato).

## Scenario E — Conflito inter-monstro: gatilho + toast (14.5, 16.5)

> Conflito é raro (5% por entrada; 0,5% após o primeiro daquele nível). Para forçar, **entre e saia
> repetidamente** do mesmo nível com ≥2 espécies distintas presentes, ou use um nível conhecido por
> ter 2+ espécies. (Se houver um cheat/debug para forçar o roll, use-o e anote.)

1. Entrar repetidamente num nível com pelo menos 2 espécies diferentes.
2. Quando o conflito disparar, EXPECT: **toast no HUD** "Criaturas em conflito!" (via
   `CaveEcosystemConflictStartedEvent` → `PlayerActionFeedbackEvent`).
3. EXPECT: o evento nomeia duas espécies (`FactionAEnemyId` ≠ `FactionBEnemyId`).
4. Num nível com **<2 espécies distintas**, EXPECT: nunca há conflito.

## Scenario F — Comportamento de conflito em runtime (14.6)

1. Com um conflito ativo, observar os dois lados rivais.
2. EXPECT: os monstros dos lados rivais **se atacam** (além de poderem mirar o player) — aggro dividido.
3. EXPECT: o dano **monstro→monstro** é claramente reduzido (~1/10 do normal — `InterMonsterDamageMultiplier=0.10`).
4. EXPECT: monstros do **mesmo tipo** nunca se atacam.
5. EXPECT: o dano **monstro↔jogador** permanece **normal** (multiplicador NÃO se aplica ao player).
6. EXPECT: um monstro que apanha de um rival fica **"Ferido"** por uma janela curta (defesa reduzida —
   ele toma mais dano enquanto Ferido; intervir nesse momento é vantajoso).

## Scenario G — Loot de kill monstro-vs-monstro (14.6)

1. Deixar (ou ajudar a causar) um monstro **matar** outro no conflito.
2. EXPECT: o morto deixa um **corpo saqueável com loot reduzido** (×`InterMonsterKillLootMultiplier=0.40`).
3. EXPECT: o player **NÃO** ganha XP nem progresso de quest/bestiário por essa morte (foi
   `EnemyKilledByEnemyEvent`, não `EnemyKilledEvent`). Loot reduzido ≠ loot zero ≠ loot cheio.
4. Conferir que matar o **mesmo** tipo de inimigo pelo **player** continua dando loot/XP cheios (sem regressão).

## Scenario H — Mercador errante enriquecido (14.7)

1. Encontrar o mercador errante (`CaveWanderingMerchant`, ~22%/nível) em bandas diferentes.
2. EXPECT: o **estoque varia por bioma** (ofertas temáticas), com variedade ampliada.
3. EXPECT: itens ofertados são válidos no catálogo; sem oferta duplicada; aparição/seleção determinística por seed.

## Scenario I — Save/Load round-trip + back-compat (14.8)

1. Minerar um veio (Scenario B), disparar um conflito (Scenario E) e **salvar**.
2. **Carregar** o jogo.
3. EXPECT: elementos/posições, veio depletado, presença de lago e o estado de conflito do nível
   (`HasHadConflict`/`EntryCount`) persistiram corretamente; nenhum crash.
4. **Back-compat:** carregar um save ANTIGO (sem os campos novos).
   EXPECT: carrega sem erro; elementos/conflito regenerados deterministicamente (defaults seguros).

## Scenario J — Revisita estável (stable-run / 14.5 carve-out)

1. Revisitar um nível dentro do mesmo `CaveRunSeed`.
2. EXPECT: composição/contagem/posições/IDs de inimigos e nodes **idênticos** (não há reroll de composição).
3. EXPECT: o conflito **pode** mudar entre entradas (comportamento por visita — ADR-0018), mas isso
   **não** altera quais inimigos existem nem onde estão.

---

## Console Expectations

- **Errors:** NENHUM. Qualquer ERROR/Exception é falha.
- **Warnings esperados:** nenhum em runtime; warnings do validator de assets são aceitáveis no Editor.
- **Proibido:** wiring-error de `_ecosystemBalance`/`_environmentElementDatabase` null (significa wiring
  DEFERRED_UNITY incompleto), NRE em conflito/materialização, qualquer log de `GameObject.Find`.

## Pass/Fail Checklist

- [ ] A — elementos temáticos por bioma materializam; pedra/minério em toda banda; path nunca bloqueado
- [ ] B — minério minerável com pickaxe; depleção persiste na revisita
- [ ] C — lago habilita aquático; sem lago = sem aquático
- [ ] D — mapa cresce com profundidade; sem nível vazio; entrada segura sem envelopamento
- [ ] E — conflito dispara (5%/0,5%); toast "Criaturas em conflito!"; espécies distintas; <2 espécies = sem conflito
- [ ] F — monstros rivais se atacam (~1/10 dano); same-type imune; dano ao player inalterado; "Ferido" visível
- [ ] G — corpo de kill monstro-vs-monstro = loot reduzido SEM XP/quest ao player; kill do player = loot/XP cheios
- [ ] H — mercador com estoque temático por bioma, determinístico, itens válidos
- [ ] I — save/load round-trip + back-compat (save antigo carrega + regenera)
- [ ] J — revisita estável (composição idêntica; conflito pode variar por visita)
- [ ] Validator `Validate Cave Ecosystem (fable_78)` = Errors=0
- [ ] 7 suítes EditMode PASS; replay validator stable-run PASS
- [ ] Sem console errors; sem regressão em loot/XP do player

**Overall:** PASS / FAIL

## Notes

- Conflito é raro por design; reserve tempo para forçar o roll por múltiplas entradas, ou use debug se disponível.
- Toast "Criaturas em conflito!" é literal (localização fora do escopo desta spec).
- Arte é placeholder por família/bioma (art pass = spec futura).
- Tested on: __ / Tested by: __ / Date: __
