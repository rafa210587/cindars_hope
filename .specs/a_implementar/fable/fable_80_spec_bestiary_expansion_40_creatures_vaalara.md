---
status: implemented
implemented_date: 2026-06-23
phase_status: BUILD_VALIDATED
evidence: docs/validation/fable_80_spec_bestiary_expansion_40_creatures_vaalara_execution_report.md
phase_2_asset_gen: BLOCKED — humano deve rodar CindarsHope/Generate/Bestiary no Unity Editor
phase_3_human_validation: DEFERRED_TO_FINAL_HUMAN_VALIDATION — docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
commit: a72e5dbb
---

# SPEC — Dados: Expansão do Bestiário +40 Criaturas (Lore Vaalara + D&D One)

> **Spec ID:** `fable_80_spec_bestiary_expansion_40_creatures_vaalara`
> **Status:** A implementar
> **Wave:** FABLE Batch 6
> **Priority:** P1
> **Type:** Data / Editor
> **Domain:** Cave / Bestiary
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_batch_6
> **Can run with:** specs que não tocam catálogo/roster/spawn planner
> **Must not run with:** F79 (escala — depende dela), F33, F24, F81 (mesma cadeia roster/spawn)
> **Repo lock scope:** `Combat/Bestiary/CanonicalBestiaryCatalog.*.cs`, geradores de EnemyDataSO, tabelas de banda
> **Depends on:** F79 (eixo de escala player-relative), F33 (infra de 64 fichas + gerador), F24 (22 moves canônicos)
> **Blocks:** F81 (packs procedurais usam o roster expandido)
> **Scope:** adicionar 40 criaturas novas ao bestiário canônico (5–6 por banda), cada uma fundamentada no lore de Vaalara/Dornecia + arquétipos D&D One, com size class, role, moves, ataque-assinatura, fraqueza, motivação e razão de pertencer ao seu grupo.
> **Out of scope:** novos comportamentos de AI (F24/F82 são donos); novos packs (F81); arte/sprites; novos bosses de gate.

required_adrs: [ADR-0005]
required_game_rules: [cave_rules.md, combat_rules.md]

---

# /speckit.specify

## Contexto

O roster canônico tem 64 fichas (60 banda + 4 finais). A caverna profunda (Band Deep) tem
só 9 fichas e bandas inteiras concentram-se em Medium/Large — pouca variedade de tamanho e
de arquétipo dentro de comuns. O design pede **mais variedade**: tamanhos do diminuto ao
colossal, mais inimigos para popular packs procedurais, todos coerentes com o mundo.

O lore canônico (VAALARA_GAME_CANON_DIRECTION, CAVE_BESTIARY_CATALOG_DIRECTION) define 7
bandas/biomas (Stone, Fungal, Ice, Fire, Ruins, Deep, Void), facções renomeadas
(**Drow → Veilkin**, **Duergar → Gravedelver**; Goblin/Kobold/Orc/Lich/Construct mantidos),
entidades (**Veyraath** abissal, **Anya** desaparecida, **Pedra Negra**/corrupção,
**Elyndor**/portais, **Bromécia**/ruínas, divindade **Kaand** do conflito) e famílias
(Insect, Plant, Humanoid, Beast, Elemental, Undead, Construct, Aberration, Dragon).

Esta spec autora **40 fichas novas** sobre essa infra, seguindo o gerador idempotente da
F33 e a fórmula de escala player-relative da F79.

## Problema

Sem mais criaturas, os packs procedurais (F81) repetem meia dúzia de inimigos por banda, a
variedade de tamanho fica achatada e o mundo perde densidade de lore. Adicionar criaturas
sem fundamentação ("monstro genérico nº 41") quebraria o tom canônico — cada inimigo precisa
de motivação e de uma razão para estar naquele grupo/bioma.

## Objetivo

Ao final desta spec, o bestiário canônico tem **104 fichas** (64 + 40), as 40 novas
materializadas como `EnemyDataSO` pelo gerador idempotente (por enemyId), cada uma com:
banda/faixa de nível, `BestiarySizeClass`, `EnemyRole`, `MovePrimary` (+ `MoveSecondary`
quando aplicável, dos 22 da F24), ataque-assinatura, fraqueza/counterplay, família, drop
primário e **nota de lore com motivação + razão de pertencer ao bioma/pack** — preenchendo
as lacunas de tamanho (Tiny/Small/Gargantuan-via-função) e de arquétipo por banda, com o
validador de consistência (F30) e o replay validator PASS.

## Fontes obrigatórias lidas

```text
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
docs/design/gameplay/cave/CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md
docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md
Assets/_Game/Scripts/Combat/Bestiary/CanonicalBestiaryCatalog.*.cs (padrão das fichas)
Assets/_Game/Scripts/Combat/Bestiary/BestiaryCreatureDef.cs (campos)
Assets/_Game/Scripts/Editor/Enemies/GenerateCanonicalBestiary.cs (gerador idempotente)
.specs/a_implementar/fable/fable_33_spec_bestiary_data_expansion_60_creatures.md
.specs/a_implementar/fable/fable_24_spec_enemy_moves_elite_affixes_runtime.md (22 moves)
.specs/a_implementar/fable/fable_79_spec_enemy_scale_player_relative_unification.md (escala)
.claude/rules/cave-stable-run.md ; .claude/rules/id-stability.md
.claude/skills/data-catalog-authoring/SKILL.md ; .claude/skills/registry-catalog-pattern/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- BestiaryCreatureDef + CanonicalBestiaryCatalog.All (agrega bandas via yield);
- GenerateCanonicalBestiary (gerador idempotente por enemyId);
- CaveBandSpawnTable / CaveEnemySpawnPlanner (pool por banda, exclui boss/miniboss);
- 22 moves (F24) e EliteAffix; fórmula de escala player-relative (F79);
- ValidateEnemyCaveSpawnCoverage (F30).
Não existe:
- as 40 fichas desta spec; arquétipos Tiny/Gargantuan em várias bandas.
Auditar Fase 0:
- IDs já usados (evitar colisão — id-stability); famílias/Notes por banda;
  faixas de nível livres dentro de cada banda; drops por banda no item catalog (F32).
```

## Engineering stories

```text
Como jogador, quero encontrar inimigos visivelmente diferentes em tamanho e comportamento
  por bioma (enxame diminuto, bruto colossal, duelista, conjurador), com identidade própria.
Como leitor do codex (F45), quero que cada criatura tenha uma motivação e um lugar no mundo.
Como F81 (packs), quero um roster denso o bastante para montar packs temáticos coerentes
  (líder + flankers + apoio) por bioma.
Como stable-run, quero fichas data-only determinísticas (sem GUID/timestamp).
```

## Escopo

```text
Inclui:
- 40 BestiaryCreatureDef novas nas bandas existentes (ver tabela canônica abaixo),
  distribuídas: Stone 5, Fungal 6, Ice 6, Fire 6, Ruins 6, Deep 6, Void 5;
- cada ficha com: EnemyId (prefixo enemy_, id-stability), DisplayName (estilo D&D),
  Family, Band, MinLevel/MaxLevel, BestiarySizeClass, Role, MovePrimary (+MoveSecondary
  quando voador/híbrido), HP/Damage/Defense/Xp base (curva da F33: +12% HP / +8% DMG por
  nível acima do mínimo, aplicada no planner), PrimaryDropItemId, PrimaryDamageTypeId,
  VulnerabilityMatrixProfileId, flags (IsAquatic/IsNocturnal/IsNonAggressive), Notes
  (motivação + razão de pertencer ao bioma/pack + fraqueza/counterplay);
- materialização via GenerateCanonicalBestiary (idempotente, por enemyId);
- cobertura de tamanho: incluir Tiny (enxames) e Small onde faltam; usar função
  miniboss/boss apenas onde o lore justificar (a maioria são comuns/elites);
- EditMode tests: contagem 104, unicidade de IDs, todo MovePrimary ∈ 22 moves,
  toda BestiarySizeClass válida, toda ficha com banda/faixa coerente.
```

## Fora de escopo

```text
Não inclui:
- novos comportamentos de AI / novos moves (F24/F82);
- novos packs procedurais (F81 consome este roster);
- novos bosses de gate (gates já definidos);
- arte/sprites/animações; balance fino além da curva canônica;
- novas damage types/vulnerability matrices (reusar as existentes);
- novos itens de drop (reusar item catalog F32; se faltar, marcar TODO p/ F32, não criar aqui).
```

## Regras de não duplicação

```text
Reusar BestiaryCreatureDef/gerador/spawn table — NÃO criar catálogo paralelo.
Reusar os 22 moves (F24) — NÃO inventar move novo aqui.
Reusar damage types / vulnerability matrices / drops existentes.
IDs únicos e estáveis (id-stability) — sem GUID/timestamp.
Respeitar renomes canônicos (Veilkin, Gravedelver); não reintroduzir Drow/Duergar.
```

## Roster canônico das 40 criaturas

> Formato: `enemy_id` — DisplayName — Família — Size/Role — MovePrimary — assinatura — fraqueza — motivação & razão no bioma/pack.

### Banda 1 — Stone (níveis 1–10) — 5

```text
enemy_glimmer_centipede — Glimmering Centipede — Insect — Tiny/Swarm — SwarmErratic —
  mordida bioluminescente (veneno leve) — fogo — caça por luz própria que atrai presas
  cegas; enxame de fundo, preenche packs de abertura.
enemy_stone_burrower — Stone Burrower — Beast — Small/Burrower — BurrowAmbush —
  investida vertical surpresa — golpe contundente — embosca em gargalos de pedra; isca/
  abre-pack que pune avanço descuidado.
enemy_roost_cave_bat — Roost Cave Bat — Beast — Tiny/Swarm — FloatingSlow —
  guincho desorientante + rasante — radiante/luz — colônia noturna que defende o teto;
  enxame aéreo de Nyx, ativo à noite.
enemy_bandit_scavenger — Bandit Scavenger — Humanoid — Medium/Chaser — GroundChase —
  rajada desesperada de adaga — qualquer — gente quebrada que fugiu para baixo e saqueia
  recém-chegados; "líder" humano improvisado de packs Stone.
enemy_cracked_golem_shard — Cracked Golem Shard — Construct — Small/Guard — GuardStationary —
  pancada lenta de pedra — relâmpago (imune a corrupção) — relíquia bromeciana partida que
  ainda guarda escombros; âncora estacionária perto de tesouro inicial.
```

### Banda 2 — Fungal (níveis 11–25) — 6

```text
enemy_rotcap_cluster — Rotcap Cluster — Plant — Small/Control — GuardStationary —
  estouro de esporos (nuvem de veneno) — fogo — colônia que toma corredores; hazard
  estacionário que cobre packs fúngicos.
enemy_mycelial_warden — Mycelial Warden — Plant — Large/Tank — ProtectAnchor —
  chicote de raiz + regen perto de esporos — fogo — guarda os leitos de esporo do Patriarca;
  tanque-âncora do pack fúngico.
enemy_goblin_shredder — Goblin Shredder — Humanoid — Small/Chaser — PackFlanker —
  investida de lâmina dupla — qualquer — flanqueador do bando que segue o tambor; precisa
  de líder vivo (PackFlanker).
enemy_orc_drummer — Orc War-Drummer — Humanoid — Medium/Caster(buff) — RetreatAndCall —
  tambor de guerra (frenesi nos aliados) — silêncio/atordoamento — reúne os bandos de Kaand;
  líder de apoio que chama reforço.
enemy_cave_stalker_cat — Cave Stalker — Beast — Medium/Chaser — Leaper —
  bote da escuridão — luz o revela — emboscador solitário que disputa presas com goblins;
  elite ocasional fora de pack.
enemy_spore_amalgam — Spore Amalgam — Aberration — Medium/Tank — TankSlowPush —
  engolfar + lentidão — fogo/radiante — fusão sem mente de coisas mortas que transbordou;
  bruto de pressão em packs fúngicos densos.
```

### Banda 3 — Ice (níveis 26–40) — 6

```text
enemy_frostshard_wisp — Frostshard Wisp — Elemental — Tiny/Caster — FloatingOrbit —
  raio de frio (dreno de stamina) — fogo — estilhaços da vontade do Rimelock; orbitador
  de apoio que pune stamina.
enemy_crystal_hound — Crystal Hound — Beast — Medium/Chaser — PackLeader —
  bote de presa-de-gelo (lidera o bando) — contundente — caça em pares a serviço do culto;
  líder de pack de feras.
enemy_veilkin_iceblade — Veilkin Iceblade — Humanoid — Medium/Chaser — CircleStrafe —
  rapieira de geada com ripostes — fogo — duelistas do Véu a serviço do culto frio;
  flanqueador técnico.
enemy_coldcult_preacher — Cold Cult Preacher — Humanoid — Medium/Caster — CasterKeepAway —
  nova de frio + cântico de chill — silêncio — converte os perdidos à fome de Husinord;
  conjurador-líder do culto.
enemy_frostbound_revenant — Frostbound Revenant — Undead — Large/Tank — TankSlowPush —
  talho congelado, ergue-se uma vez — fogo/radiante — mineiros congelados em pleno labor que
  nunca param; tanque que regressa, núcleo de packs mortos.
enemy_glacier_tick — Glacier Tick — Insect — Small/Swarm — SwarmErratic —
  agarrar + dreno de frostbite — fogo — parasita de enxame da banda de gelo; fodder que cobre
  conjuradores.
```

### Banda 4 — Fire (níveis 41–55) — 6

```text
enemy_magma_slug — Magma Slug — Elemental — Large/Tank — TankSlowPush —
  trilha de magma (hazard) — água/gelo — pastador lento de fornalha que bloqueia caminhos;
  controlador de espaço.
enemy_ember_scorpion — Ember Scorpion — Beast — Medium/Chaser — ChargeLine —
  investida em chamas + ferrão — gelo — territorial perto de veios de magma; perseguidor
  de carga em packs de fogo.
enemy_sulfur_wyrmling — Sulfur Wyrmling — Dragon — Medium/Ranged — KiteRanged —
  cone de sopro sulfúrico — gelo/água — ninhada menor do Cindershard; atirador que mantém
  distância, líder de mini-ninho.
enemy_emberroot_horror — Emberroot Horror — Plant — Large/Control — ProtectAnchor —
  chicote de vinha flamejante — água — crescimento corrompido alimentado por lava + Pedra
  Negra; âncora de hazard.
enemy_veilkin_pyrecaller — Veilkin Pyrecaller — Humanoid — Medium/Caster — CasterKeepAway —
  bola de fogo + chuva de brasas — silêncio/água — culto do Véu de Kaand do fogo; conjurador-
  líder.
enemy_steam_golem_proto — Steam Golem Prototype — Construct — Large/Guard —
  BossArenaControl (primitivo) — jato de vapor + pancada — relâmpago — unidade de forja
  bromeciana defeituosa; guarda pesado de câmara.
```

### Banda 5 — Ruins / Bromécia (níveis 56–70) — 6

```text
enemy_rune_sentry_mk2 — Rune Sentry Mk.II — Construct — Medium/Ranged — GuardStationary —
  varredura de feixe rúnico — relâmpago/EMP — sentinela de Elyndor que nunca soube que o
  império caiu; torre de fogo de cobertura.
enemy_mirror_golem — Mirror Golem — Construct — Large/Tank — ProtectAnchor —
  reflete projétil + empurrão — contundente — guarda os salões espelhados do Arquivo;
  tanque-âncora anti-ranged.
enemy_gravedelver_runepriest — Gravedelver Runepriest — Humanoid — Medium/Caster —
  CasterKeepAway — barreira rúnica + raio arcano — silêncio — exilados que barganham com as
  máquinas antigas; conjurador-líder técnico.
enemy_ninrorin_echo_warrior — Ninrorin Echo Warrior — Undead — Medium/Chaser — CircleStrafe —
  flurry de lâmina-fantasma — radiante — eco-memória dos que serviram antes do fim; flanqueador
  espectral.
enemy_chromatic_hoardling — Chromatic Hoardling — Aberration — Small/Ambush —
  TreasureIdleAmbush — bote de baú-mímico — qualquer (após revelado) — atrai os gananciosos;
  armadilha solitária junto a tesouro.
enemy_runic_warbeast — Runic Warbeast — Beast — Large/Chaser — ChargeLine —
  investida rúnica de chifres — relâmpago — fera de guerra bromeciana domesticada que voltou
  ao selvagem; bruto de carga.
```

### Banda 6 — Deep / Profundezas (níveis 71–85) — 6

```text
enemy_void_brood_larva — Void Brood Larva — Aberration — Tiny/Swarm — SwarmErratic —
  mordida corrosiva (durability stress) — radiante — prole do ninho de Veyraath; enxame que
  desgasta equipamento, cobre emboscadas.
enemy_mindbound_thrall — Mindbound Thrall — Humanoid — Medium/Chaser — PackFlanker —
  golpe-fantoche — radiante/purify — mortais escravizados pelo sussurro de Veyraath;
  flanqueador que depende do mestre (líder).
enemy_veilkin_voidassassin — Veilkin Void-Assassin — Humanoid — Medium/Chaser —
  PhaseShortBlink — backstab por blink — revelar/AoE — elite do Véu que bebeu o silêncio
  profundo; assassino que reposiciona, elite de pack.
enemy_gloomspine_lurker — Gloomspine Lurker — Aberration — Large/Burrower — BurrowAmbush —
  arrasto de tentáculo + agarrar — radiante — emboscador paciente do teto; abre encontros de
  emboscada.
enemy_corrupt_pseudowyrm — Corrupted Pseudo-Wyrm — Dragon — Medium/Caster — FloatingOrbit —
  feixe de sopro-vazio — purify — prole draconica deformada por Pedra Negra; orbitador
  conjurador.
enemy_nyx_shade_elemental — Nyx Shade — Elemental — Medium/Control — HazardLure —
  agarrar de sombra (cegueira) — luz/radiante — sombra com vontade sob a lua oculta de Nyx;
  controlador que atrai para hazard.
```

### Banda 7 — Void / Pedra Negra (níveis 86–99) — 5

```text
enemy_void_tendril_watcher — Void Tendril Watcher — Aberration — Huge/Control —
  ProtectAnchor — feixe-olho + campo de tentáculos — só radiante (purify falha) —
  fragmento do olhar de Veyraath; âncora-controlador de câmara.
enemy_reality_render — Reality Render — Aberration — Large/Chaser — ChargeLine —
  bote de rasgo-de-fase (silêncio) — radiante — desfaz o que toca; perseguidor de pressão de
  endgame.
enemy_veilkin_voidknight — Veilkin Void-Knight — Humanoid — Large/Chaser — CircleStrafe —
  combo de montante-vazio — radiante — evolução final dos traidores do Véu; elite duelista.
enemy_sealed_observer — Sealed Observer — Construct — Large/Guard — GuardStationary —
  pulso de julgamento (sentinela Nymiriana não-corrompida; pode poupar) — imune a corrupção —
  vigia feito por Anya que ainda mantém o posto; guarda neutro/condicional.
enemy_dread_chorister — Dread Chorister — Undead — Medium/Caster — FloatingOrbit —
  hino do medo (oscila HUD) — radiante (resiste silêncio) — canta a litania de Veyraath;
  conjurador de apoio de packs do vazio.
```

## Critérios de aceite

### CA-1 40 fichas canônicas
- 40 BestiaryCreatureDef novas existem em `CanonicalBestiaryCatalog.All` (total 104),
  com todos os campos preenchidos conforme a tabela.
- Evidência: EditMode test de contagem (104) + presença por enemyId.

### CA-2 Coerência de lore e identidade
- Cada ficha tem família correta, banda/faixa coerente e Notes com motivação + razão de
  pertencer ao bioma/pack + fraqueza/counterplay; renomes canônicos respeitados (Veilkin,
  Gravedelver; sem Drow/Duergar).
- Evidência: revisão + teste que checa Notes não-vazio e ausência de termos proibidos.

### CA-3 Variedade de tamanho e arquétipo
- Há novas criaturas Tiny e Small por banda onde faltavam; arquétipos diversos
  (swarm/tank/ranged/caster/control/ambush/chaser) cobertos por banda.
- Evidência: teste de distribuição (≥1 Tiny/Small por banda relevante; ≥4 roles distintos por banda total).

### CA-4 Moves e escala válidos
- Todo MovePrimary/MoveSecondary ∈ 22 moves (F24); toda BestiarySizeClass válida; escala
  resolvida por F79 (player-relative).
- Evidência: teste que valida moves ∈ enum e size ∈ enum + ResolveVisualScale aplicável.

### CA-5 Materialização e validadores PASS
- Gerador idempotente materializa as 40 como EnemyDataSO (por enemyId, sem duplicar);
  ValidateEnemyCaveSpawnCoverage (F30) e replay validator PASS.
- Evidência: log de geração (comando, exit code, contagem) + validadores no report.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Combat/Bestiary/
  CanonicalBestiaryCatalog.BandStone.cs   (+5)
  CanonicalBestiaryCatalog.BandFungal.cs  (+6)
  CanonicalBestiaryCatalog.BandIce.cs     (+6)
  CanonicalBestiaryCatalog.BandFire.cs    (+6)
  CanonicalBestiaryCatalog.BandRuins.cs   (+6)
  CanonicalBestiaryCatalog.BandDeep.cs    (+6)
  CanonicalBestiaryCatalog.BandVoid.cs    (+5)
Assets/_Game/Data/Enemies/Canonical/       (40 EnemyDataSO gerados — evidência)
Assets/_Game/Tests/EditMode/Combat/
  BestiaryExpansion40Tests.cs              (NOVO)
docs/validation/
  fable_80_spec_bestiary_expansion_40_creatures_vaalara_execution_report.md
```

## Contratos

### Data contracts
40 `BestiaryCreatureDef` aditivas (yield nas bandas). Nenhum campo novo no struct (reuso
total dos campos da F33). Escala derivada da F79 em runtime. Drops referenciam IDs do item
catalog (F32); falta de item → TODO documentado p/ F32 (não criar item aqui).

### Runtime contracts
Nenhum comportamento novo. As fichas plugam no pool de banda existente
(CaveBandSpawnTable/CaveEnemySpawnPlanner) automaticamente via `All`.

### Save contracts
N/A — data-only; IDs estáveis (id-stability); recomputável por seed.

### UI contracts
Codex (F21/F45) passa a listar 104 criaturas (leitura automática de All). Sem nova tela.

## Sistemas afetados

```text
Bestiary catalog (núcleo) | Gerador de EnemyDataSO | Pool de spawn por banda (leitura)
Codex/bestiary knowledge (leitura) | Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Combat/Bestiary/CanonicalBestiaryCatalog.Band*.cs
Assets/_Game/Data/Enemies/Canonical/** (assets gerados pelo gerador idempotente)
Assets/_Game/Scripts/Tests/EditMode/Combat/** (ou Assets/_Game/Tests/EditMode/Combat/**)
docs/validation/**
csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab (edição manual)
Packages/** ; ProjectSettings/**
EnemyBrain / moves / EnemySpawnPackSO (F24/F82/F81 são donos)
Item catalog (F32) — só referenciar IDs; criar item novo é fora de escopo
SaveManager / GameSaveData
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Mapear IDs/famílias/faixas livres por banda; drops disponíveis (F32); validar que moves da
tabela ∈ 22 (F24) e size ∈ enum. Confirmar F79 mergeada (escala).

### Fase 1 — Autoria por banda
Adicionar as fichas por banda (Stone→Void) conforme a tabela canônica, com Notes de lore.

### Fase 2 — Materialização
Rodar GenerateCanonicalBestiary (idempotente). Registrar comando/exit/contagem (evidência).

### Fase 3 — Testes e validadores
BestiaryExpansion40Tests (contagem/unicidade/moves/size/distribuição); F30 coverage; replay.

### Fase 4 — Fechamento
csproj; run_strict_validation; report com Spec Compliance Matrix + evidência de geração.
```

## Paralelização

- Parallelizable: CONDITIONAL — Must not run with F79/F33/F24/F81 (cadeia roster/escala/spawn).
- Lock: CanonicalBestiaryCatalog.*, geradores, tabelas de banda.
- Reason: edição concorrente do catálogo/gerador gera conflito direto.

## Impacto em save/load

```text
Changes save schema? NO | Adds section? NO | Migration? NO | Persists Unity refs? NO
IDs estáveis novos; nenhum rename de ID existente (id-stability).
```

## Impacto em eventos

```text
Adds events: NO | Changes events: NO
```

## Impacto em UI/Unity

```text
Changes UI: codex lista 104 (automático) | Changes scenes/prefabs: NO
Changes assets: 40 EnemyDataSO gerados (evidência de geração obrigatória)
Requires Play Mode final validation: YES (amostragem visual de novas criaturas em cave)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: colisão de enemyId / rename acidental. Mitigação: auditoria Fase 0 + teste de unicidade.
Risco: move atribuído fora dos 22. Mitigação: teste que valida MovePrimary/Secondary ∈ enum.
Risco: drop apontando item inexistente. Mitigação: validar contra item catalog; TODO p/ F32.
Risco: quebra de stable-run (pool muda composição). Mitigação: replay validator; fichas
  entram no pool por regras determinísticas existentes (sem GUID/timestamp).
Risco: geração de assets bloqueada (Unity lock). Mitigação: documentar BLOCKED com evidência
  (unity-assets) — fichas no catálogo permanecem válidas para F81.
```

## Rollback

```text
Remover as fichas das bandas restaura roster de 64 (assets gerados ficam órfãos, removíveis).
Nenhum comportamento de runtime muda — rollback é puramente de dados.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar IDs/famílias/faixas livres por banda + drops (F32) + moves ∈ 22 (F24).
- [ ] T002 — Autorar +5 Stone, +6 Fungal, +6 Ice (com Notes de lore).
- [ ] T003 — Autorar +6 Fire, +6 Ruins (com Notes de lore).
- [ ] T004 — Autorar +6 Deep, +5 Void (com Notes de lore).
- [ ] T005 — Materializar via gerador (idempotente) + evidência (comando/exit/contagem).
- [ ] T006 — BestiaryExpansion40Tests + F30 coverage + replay; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (dados de catálogo + materialização determinística)
- Requires EditMode tests: YES (contagem 104, unicidade IDs, moves∈enum, size∈enum, distribuição, Notes não-vazio)
- Requires PlayMode/human scenario: YES (amostragem visual em cave — lote final)
- Requires regression test: YES (replay stable-run + F30 coverage)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + geração evidenciada + cenário humano com ≥1 criatura nova por banda

## Definition of Done

```text
104 fichas no catálogo (64+40), 40 materializadas idempotentemente; cada ficha com lore/
motivação/razão-de-pack e fraqueza; moves∈22; size válido; escala via F79; F30 + replay PASS.
Builds 0E; run_strict_validation exit 0; report com Spec Compliance Matrix + evidência de geração.
```

## Anti-regressão

```text
Nenhum ID existente renomeado/removido (id-stability).
Mesmo CaveRunSeed → composição determinística ao revisitar (stable-run).
Sem GUID/timestamp em conteúdo; sem novos moves/comportamentos.
Renomes canônicos respeitados (Veilkin/Gravedelver).
```
