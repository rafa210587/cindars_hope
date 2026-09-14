# Guarita — correção após rejeição visual

Escopo: pedido humano “agora corrija o interior”, House_GateKeeper, C-4/C-6/C-8 de `.specs/a_implementar/spec_town_components_doors_and_collision_v1.md`. Não promove a spec completa nem replica o mobiliário para as outras casas.

## Resultado e critérios

- Piso útil 6×4u, alinhado às faces internas dos colliders, sem decking excedente. Soleira 1,8×1u cobre o canal da porta.
- Fundo com face de pedra de 1,622u; laterais de 0,30u para fora do piso; frente recortada, com dois batentes que preservam o vão livre de 1,8u. As dimensões antigas de 0,18u foram revistas após a rejeição, não são regra universal.
- Cama ~2,18u de altura projetada, mesa ~1,08u e caixote ~1u. Escala uniforme. Referência: silhueta opaca do Player final ~2,496u, não a escala inicial do generator. Essas razões são específicas deste layout/projeção, não prova isolada de proporção.
- Armário com louças substituído por caixote existente, sem geração/edição de imagem. Arte de guarda dedicada permanece pendente.
- Parede/porta/reveal/IDs preservados; todos os suportes efetivos dos três móveis considerados, inclusive filhos medidos. Colisão não implica Rigidbody dinâmico em cada objeto.
- Correção necessária de câmera Town: reutilizado `CameraTransparencySort2D`, já usado pela Farm. A mudança afeta a ordenação World da câmera Town, não apenas a guarita; nenhum runtime novo ou alteração de câmera Farm. `GraphicsSettings` tinha modo Orthographic e eixo Y sem CustomAxis efetivo. A primeira rodada mostrou mesa cobrindo a cabeça do Player; a mesma pose na segunda rodada não apresenta o corte.

## Evidência atual

Unity 6000.5.7f1. [Comparação real antes/depois](town_room_repair_comparison_20260912.html). Capturas 800×600 por MainCamera, enquadramento determinístico do lote, igual entre as rodadas. Não são screenshots de HUD/teclado nem prova de todos os enquadramentos de gameplay.

- Geração: `Logs/town-room-repair-regen-20260912-r2.log`, término explícito return code 0.
- EditMode: **PASS 7/7**, `Logs/town-room-repair-edit-20260912-r2.xml`, `.log`, runner exit 0. Filtro `TownCivicInteriorTests;CorralScheduleLanes_KeepHundAndGurdTransitionsWithinDetourLimit`. Sem erros críticos, warnings de APIs obsoletas preservados no log.
- Física Play Mode local: **PASS 1/1 casa, 9/9 waypoints**, [JSON run2](town_room_repair_20260912_run2/town-all-entries.json). Três poses de aproximação da cama/mesa/caixote, movimento real de Rigidbody2D em FixedUpdate, casts complementares, NPCs mantidos. Porta bloqueia fechada, abre com animação, passagem/reveal/saída e proteção ao fechar ocupada verificados. Cena/saves/setup preservados. Não prova entrada via tecla E nem encontro deliberado Player×NPC em todos os horários.
- SHA-256 TownScene final/probe: `0834EA82142DA4AFBCD487E11C8009B460905A3EDC7F536B62527C1A28401B23`.
- Revisão visual root: piso/perímetro, escala relativa e oclusão nas quatro imagens run2 melhoraram de forma observável; aceite humano pendente. Sem referência específica aprovada do interior, fidelidade à keyart: **NOT RUN**, nenhum percentual atribuído.
- Revisão independente `town_final_visual_review`: **PASS para correções visuais alvo** nas quatro capturas run2 (oclusão, piso/perímetro, retornos/batentes). Arte da porta aberta e identidade genérica permanecem resíduos; não concede aceite humano. Run1 explicitamente reprovado pela sobreposição da cabeça e entrada pouco legível.
- Validador estático de desvios NPC: **PASS 162 pares**, 0 erros/0 warnings, pior desvio 2,9× (`npc_mella_social` → `npc_mella_home`), `Logs/town-room-repair-detours-20260912.log`, return code 0. Comando Unity `-batchmode -nographics -executeMethod CindarsHope.Editor.Validation.ValidateTownRouteDetourRatio.Validate -quit`. Não equivale a dia completo de agendas em Play Mode.

## Falhas preservadas

| Artefato | Causa / correção | Estado |
|---|---|---|
| `town_gatekeeper_pilot_20260912_run3/interior_gatekeeper.png` | Usuário rejeitou piso, paredes e proporções; redesenhada a composição nesta rodada. | Revisão antiga não vale como aceite; novo candidato aguarda humano. |
| `town_room_repair_20260912_run1/interior_gatekeeper_approach_6.png` | Mesa corta cabelo: ordenação de câmera não estava em CustomAxis/Y. Reutilizado componente existente. | Corrigido visualmente na mesma pose run2. |
| `Logs/town-room-repair-edit-20260912.log` | Erros CS0246/CS0103 em `Phase21CombatAcceptancePlayModeTests`, outra tarefa. Responsável corrigiu imports; freeze de fontes coordenado. | TestRunner r2 PASS 7/7, não repetido sobre entrada inalterada. |
| `town_gatekeeper_pilot_20260912_run2/town-all-entries.json` | Stall histórico em walking_inside, causa dinâmica não isolada. Novo probe registra contatos e NPCs próximos em futuros stalls. | **Não resolvido**: duas rodadas isoladas atuais passaram, mas não explicam a falha antiga. |
| `town_gatekeeper_pilot_20260912_run3/town-all-entries.json` | Pip: precondição de porta fechada inconclusiva com NPCs ativos. | Fora deste piloto; não declarar 23/23 atual. |

## Reprodução

1. Generator canônico: Unity `-batchmode -nographics -projectPath <projeto> -executeMethod CindarsHope.Editor.SceneCreation.CreateMvpTownScene.CreateScene -quit -logFile Logs/town-room-repair-regen-20260912-r2.log`.
2. `tools/unity/RunUnityEditModeTests.ps1` com filtro e caminhos indicados acima; resultados novos, sem sobrescrever evidência anterior.
3. Probe: `CINDARS_TOWN_ALL_ENTRIES_HOUSE=House_GateKeeper`, `CINDARS_TOWN_ALL_ENTRIES_OUTPUT=docs/validation/town_room_repair_20260912_run2`; Unity `-batchmode -force-d3d11 -projectPath <projeto> -executeMethod CindarsHope.Editor.Validation.TownAllEntriesPlayModeProbe.RunBatch -logFile Logs/town-room-repair-play-20260912-run2.log`, sem `-quit`. Para repetir, escolher nova pasta vazia.

## Inputs / preservação

Backup pré-rodada: `C:/Users/Rafa/AppData/Local/Temp/town-room-repair-a0b1bec3-ad9d-41ef-a281-f3ab2268854b/TownScene.unity`, SHA `1B3FE43610DC43E1A78FC07E44E9806A2569C8F953A7B2EDA6A1B7B045A9753A`.

SHA-256 dos fontes usados no EditMode r2 e probe run2:

| Arquivo | SHA-256 |
|---|---|
| CreateMvpTownScene.cs | C35798E2BA99E7A3B2ADE5C48AD9829889BC0B8A2A1EDF806E0E95787FE75444 |
| TownKeyartInteriorPresentation.cs | BC0081A7672E6F0AC15BA831D9A41E2CA975D63546AC2C79F2387D9D33DFB8EB |
| TownKeyartComponentSupportCatalog.cs | 237B633F7C3A597AF4879C23D909C1F14FF656FB087F0B3CA2B81452027A6F0C |
| TownAllEntriesPlayModeProbe.cs | F82092BD2974E98625110AB39E8DB608CA8AA7037B4AFFA56C0B52D5B4BDAEC3 |
| TownCivicInteriorTests.cs | 370DDAFF7D30589011D0E353207F345A72B59F7E61256DF9A645A42231BD28CC |

Revisão de não regressão: edições de conteúdo restritas à guarita e correção de câmera descrita; nenhum save DTO, runtime de porta, schedule NPC, Farm/Cave ou novo asset de arte alterado nesta rodada. `git diff --check` passou nos fontes tracked em escopo. Checkout compartilhado dirty preservado; sem commit/push. Executor `gatekeeper_room_rebuild`, correção de sorting/testes/probe pelo root, auditor `town_final_visual_review` read-only. Skills de composição/física mantiveram reprovação visual independente dos testes verdes.

Próximo refinamento artístico: tornar porta aberta mais legível e adicionar identidade de posto de guarda com arte própria revisada. Não replicar automaticamente para os demais interiores sem revisar proporção, uso e circulação de cada um.
