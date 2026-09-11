# Skills — fase visual 3: catálogo completo de animações e efeitos

> **Spec ID:** spec_skills_26_action_animation_catalog_v1  
> **Status:** APROVADA — autorização humana de 2026-09-10  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Art+Runtime / Skills+Presentation / P1  
> **Parallelizable:** por família de arte após profiles; **Depends on:** spec_skills_25_action_presentation_foundation_v1  
> **Validation:** asset validator+PlayMode gráfico+capturas

# /speckit.specify

## Spec

Completar o catálogo visual das 31 ações com leitura inequívoca na escala real do jogo.

- **AC01:** Melee e Ranged têm poses/telegraphs distintos para corte, giro, avanço, dash, salto, grito, quebra-guarda, charge, pierce, fan e marca.
- **AC02:** Magic substitui placeholders por fagulha assimétrica, gelo angular, toxicidade em bolha/fumaça e lightning segmentado; impactos possuem 3–5 frames.
- **AC03:** Survival e Crafting exibem sinais, áreas, consumíveis e ferramentas coerentes com a ação, sem fingir execução para ações dormentes.
- **AC04:** variações de rank ampliam leitura/intensidade quando a mecânica muda, sem criar animação ornamental para rank idêntico.
- **AC05:** direção, timing, contato, continuidade, alpha, PPU, pivot, sorting e escala passam revisão visual.
- **AC06:** matriz por cada um dos 31 `actionId` registra estado, cena/contexto, profile IDs, sequência, oito direções ou espelhamento, frames/duração, contato, VFX/SFX, sorting/pivot/escala e falhas aplicáveis; casos sem alvo marcam InvalidTarget/whiff como N/A.
- **AC07:** cobertura final informa Operational/Dormant e evidencia qualquer profile ou asset pendente; não há placeholder genérico em ação marcada Operational. Procs dos capstones são inventariados à parte dos 31 ativos.
- **AC08:** playback temporizado em jogo ou gravação prova timing/continuidade; sprite sheet e screenshot isolado não bastam. Pivô, arma e offhand permanecem estáveis.
- **AC09:** catálogo herda a direção aprovada na fase 24 e compara com player/world art real. Capstones usam símbolos canônicos de Kanthor, Kaand, Anya e Senya, preservando as restrições de canon, inclusive o altar de Anya.

# /speckit.plan

Executar em três subfases: 26A Melee+Ranged, 26B Magic, 26C Survival+Crafting e inventário separado de
capstones. Fontes de arte podem ser paralelas; `PlayerWalkAnimator`, Resources, profile database, generator,
validator e prefabs têm um integrador serial. Produzir fontes editáveis, ligar profiles e validar sprites,
playback e composição em cena.

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [ ] | T01 | 26A Melee+Ranged: sprites, sequências sprite-driven, VFX e feedback | AC01, AC04–AC09 |
| [ ] | T02 | 26B Magic: projéteis, zonas, impactos e feedback | AC02, AC04–AC07 |
| [ ] | T03 | 26C Survival+Crafting+capstones | AC03–AC07 |
| [ ] | T04 | Validador de cobertura e import settings | AC05, AC07 |
| [ ] | T05 | Matriz PlayMode gráfica, playback, capturas e revisão independente | AC01–AC08 |
