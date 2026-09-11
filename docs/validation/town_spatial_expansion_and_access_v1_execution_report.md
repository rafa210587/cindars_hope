# Execution report — Town spatial expansion and access v1

**Spec:** `spec_town_spatial_expansion_and_access_v1`  
**Data:** 2026-09-10  
**Status:** DEFERRED_TO_FINAL_HUMAN_VALIDATION — gates automatizados da expansão aprovados; cenário humano ainda pendente  
**Modelo/delegação:** implementação Town delegada em fatia única; raiz validou artefatos e usou revisor independente. Escalonar somente se os gates Unity revelarem falha de contrato.

## Acceptance criteria extracted

Town 160×112; escala visual1,20; preservar24 prédios/29NPCs/84anchors/23stallsNPC/6market/2spawns/≥554 árvores;
gap≥4; vias4/3,2/2,4; apron4×3; porta≥1,4; interior lane1,4+pocket2×2; comfort radius0,50;
spawn sul→praça≤75 e praça→porta≤90; saved validator187+8; PlayMode8; overview1536×1024 ortho58.

## Existing systems audit

Reusados `TownDistrictLayout`, `TownCityLayout`, `CreateMvpTownScene`, `TownKeyartGeometry`,
`ValidateTownKeyartScene`, `TownKeyartLivePhysicsProbe` e o runner canônico
`TownKeyartPlayModeCapture`. Nenhum runtime service, evento, save schema, sprite ou gerador paralelo foi criado.

## Implementação

- Bounds ampliados para ±80/±56; 24 lotes redistribuídos sem gap físico abaixo de4u.
- Hierarquia de vias e conexões de porta atualizadas para4,0/3,2/2,4u.
- `TownAccessMetrics` calcula aprons, interiores, gaps, oclusão e shortest path por endpoints/junções/projeções.
- Chão, água, borda, floresta, landmarks, props e anchors usam os novos bounds/coordenadas.
- Geração apenas consome/verifica arte e Tiles preexistentes; não cria/reimporta/marca `Assets/_Game/Art/**` dirty.
- Saved validator mantém o contador legado e adiciona oito gates `TownAccess` com censo materializado.
- Probe adiciona oito cenários físicos com Player/NPC reais e restauração de transforms/velocidades.
- Captura final preparada para overview ortho58, seis details e seis overlays derivados do layout.
- Dez testes determinísticos novos; expectativas dimensionais antigas atualizadas sem alterar o gate de escala1,20.

## Spec compliance matrix

| AC | Estado | Evidência atual |
|---|---|---|
| AC01–AC08 | PASS | cena materializada:24 lotes; gap/aprons/interiores/vias/portais PASS; sul→praça51,87u; máximo praça→porta89,33u |
| AC09 | PASS | City EditMode `281/281` em Unity6000.5.7f1 |
| AC10 | PASS | `[TownKeyartPhysics] PASS:187 FAIL:0`; `[TownAccess] PASS:8 FAIL:0` |
| AC11 | PASS | PlayMode r5:8/8 cenários,107/107 registros de rota, `sceneUnchanged=true`, `savesUnchanged=true` |
| AC12 | PASS da expansão | overview+6 details+6 overlays 1536×1024; revisão independente final separa expansão aceita do gate artístico global, fora do escopo desta spec |

## Validation

- `git diff --check` no escopo Town: PASS (avisos de EOL pertencem a mudanças concorrentes).
- Scan de APIs proibidas `GameObject.Find`/`FindObjectOfType`: PASS.
- Scan de APIs mutantes de Art em `TownKeyartGround`/`TownKeyartSceneArt`: PASS (nenhuma ocorrência).
- Revisão independente inicial: FAIL; quatro findings corrigidos (shortest path, censo29 no gate materializado,
  apron lateral e margem). Segunda revisão encontrou fachada/interior/imutabilidade de Art e token AC03;
  todos foram corrigidos. Micro-audit final: PASS, oito `AccessCheck` preservados.
- Geração final r10: PASS; cena160×112 salva e13 capturas produzidas.
- City EditMode r2: PASS281/281; scan do log sem erro crítico de compilação.
- Saved scene: física legada187/187; TownAccess8/8; agenda21/21.
- PlayMode r5: PASS; cobertura completa48.769 nós,106 destinos+1 conector, Player/NPC bidirecionais
  nas três classes de via, quatro fases de porta/interior PASS e restauração PASS.
- Revisão visual independente amendada: `ACCEPT_EXPANSION`; escala exata23/23 e aglomeração reduzida.
  O parecer não aprova fidelidade artística global>=80%, que permanece fora desta spec e `NOT PROVEN`.
- Imutabilidade: hash da cena before/after PlayMode
  `C0107BD5C783013D1B3CD3C749D177C642143485D003D7075CD72AFBDA16CCB7`; saves unchanged.
- Duas identidades NPC continuam marcadas como arte provisória (`npc_corvus`,
  `npc_vaalara_wanderer_01`). É dívida visual explícita da spec de fidelidade, não regressão de acesso.

```text
Unity validation: PASS
Commands: `GenerateValidateAndCaptureRevision`; `RunUnityEditModeTests.ps1 -TestFilter CindarsHope.Tests.EditMode.City`; `TownKeyartPlayModeCapture.RunBatch`.
Evidence: `Logs/town_expansion_v1_generate_r10.log`, `Logs/town_expansion_v1_editmode_r2.log`, `Logs/town_expansion_v1_playmode_r5.log`.
Residual risk: cenário humano ainda não executado; fidelidade artística global >=80% e os dois NPCs provisórios pertencem ao passe visual subsequente.
```

## Arquivos e artefatos

- Fontes/editor/testes: os15 paths permitidos da spec; hashes SHA256 capturados no terminal desta execução.
- Baseline congelada: `art/town-keyart-rework/evidence/town-expansion-v1/before/`.
- Cena humana: `docs/validation/playmode/town_spatial_expansion_and_access_v1_human_test_scenario.md`.
- Arquivos proibidos modificados por esta spec: nenhum. O worktree contém mudanças concorrentes fora do escopo.

## Evidência final

- Cena: SHA256 `C0107BD5C783013D1B3CD3C749D177C642143485D003D7075CD72AFBDA16CCB7`.
- Overview: SHA256 `03028E3214982E87C0DD82242E4D088E91D52AC577EFB6EEA1BA0D9992930088`.
- Saved captures: `art/town-keyart-rework/evidence/town-expansion-v1/final/`.
- PlayMode imutável: `art/town-keyart-rework/evidence/town-expansion-v1/playmode-final-r5/`.
- Cenário humano: `docs/validation/playmode/town_spatial_expansion_and_access_v1_human_test_scenario.md` (`DEFERRED_TO_FINAL_VALIDATION`).

## Remaining work fora desta spec

1. Executar o cenário humano no passe final do jogo.
2. Substituir os dois NPCs provisórios e medir novamente o gate artístico global >=80% na spec de fidelidade Town.

## Decisão `/finish-spec`

- Promoção física: **NO**.
- Motivo: implementação atingiu `PLAYMODE_VALIDATED` e AC12 independente aceitou a expansão, mas o
  protocolo do projeto proíbe declarar aceitação humana pelo cenário escrito. A spec permanece em
  `.specs/a_implementar/` como `DEFERRED_TO_FINAL_HUMAN_VALIDATION` até o teste humano final.

## Honest status rationale

O status não é `ACCEPTED`: todos os gates automatizados aplicáveis e o parecer independente da
expansão passaram, mas o humano ainda não executou o cenário final. O check global de qualidade de
specs também permanece FAIL por arquivos e relatórios concorrentes fora deste escopo; o único finding
deste report (`Honest status rationale`) foi corrigido aqui. Não há evidência de regressão Town nesses
findings globais, e eles não foram reclassificados como PASS.
