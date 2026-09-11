# TOWN.A1 — autoria/inventário de assets

## Resultado

Inventário TOWN.A1 corrigido com cobertura explícita dos 24 lotes e quatro famílias estruturais.
Os três pilotos e as famílias residenciais têm candidatos Aseprite editáveis, exports PNG nativos e
peças `facade/roof/door_leaf` em `art/town-keyart-rework/architecture/pilots/`. As peças foram
importadas em `Assets/_Game/Art/Generated/World/building/` e ligadas ao pipeline Town existente.

## Inspeção executada

- Referência de town aberta e registrada no manifesto.
- Bakery native half e Watermill native half abertas em inspeção visual a 100%/escala ampliada.
- Captura real `playmode-final-r4/watermill.png` aberta para conferir o uso atual em câmera.
- Fontes Aseprite existentes mantidas intactas; hashes dos pilotos e baselines registrados.
- Evidência anterior `final-delivery-verification.txt` confirma 15/15 reaberturas nativas, alpha parcial 0 e mismatch de export 0.
- Alchemy recebeu `town_alchemy_native_repixel_v02` em canvas nativo 204x229, com camada de
  reautoria de pixels (sem nearest-upscale) e exports modulares separados.
- Aseprite disponível e identificado como `1.3.18.5-x64`; nenhuma escrita foi feita nas fontes durante esta fatia.
- `create_native_pilots.lua` reabriu cada fonte, preservou o baseline oculto, adicionou camada de
  máscara de separação de roof e camada visível de door leaf base, salvou `.aseprite` separado e
  exportou PNG nativo + preview nearest 3x.
- Todos os três exports têm alpha parcial 0 e preservam as dimensões nativas: bakery 543x724,
  watermill 627x627, alchemy 204x229. Camadas/cels foram reabertos e conferidos nos `.inspect.txt`.
- Além do courtyard-wing, foram autoradas `town_house_hipped_stone_native_v01` e
  `town_house_l_dormer_native_v01`, com alas/telhados/dormers estruturais próprios (não recolor).
  O manifesto agora cobre 24/24 com `buildingFamilyId`: gable/courtyard-wing/hipped-stone/l-dormer
  em 6 lotes cada (25%, nenhuma família acima de 50%).
- `VisualEnlargement` de arquitetura foi fixado em 1.0; os shells preservam footprint/porta e não
  usam os fatores rejeitados 0.76/0.82/3.40/3.53.
- Para remover a escala efetiva residual dos três ofícios, foram reautorizados canvases nativos
  `town_bakery_native_scale1_v03` (347x463), `town_watermill_native_scale1_v03` (285x285) e
  `town_alchemy_native_scale1_v03` (369x414), dimensionados para os footprints 10/8/9u a PPU32;
  o exportador agora compõe todas as camadas visíveis, mantendo a repixelização nativa de alchemy
  no facade/roof/leaf final.
  `Fit` reconhece somente esses três stems e materializa `localScale=Vector3.one`; não é apenas
  uma declaração de helper: os pixels/door anchors foram reexportados nos novos canvases.
- O capturador canônico agora inclui `town-detail-alchemy-door.png` e overlay correspondente.
- Alchemy v04 substitui o raster reamostrado: `town_alchemy_native_pixel_v04` é uma autoria
  pixel-perfect direta em canvas 288x384, com fachada assimétrica, telhado separado e folha
  artística fechada. A folha é copiada do layer autoral, sem retângulo técnico; a captura normal
  nunca instancia o overlay de colisores (este permanece somente em `town-overlay-*`).
- `TownKeyartBuildingArt` resolve facade nativa e fornece roof/door leaf; `CreateMvpTownScene`
  instancia as peças como filhos do mesmo shell e inclui-as no `RoofRevealController`.
  `TownKeyartSpriteImporter` possui os paths nativos dos quatro módulos e novas famílias; GUIDs/meta
  foram criados pelo Unity quando o lock for liberado.

## Gates aplicáveis nesta fase

| Gate | Resultado |
|---|---|
| cobertura de lotes | PASS — 24/24 no manifesto |
| auditoria de fonte/hashes | PASS — pilotos/baselines registrados |
| alpha e pixels dos candidatos existentes | PASS na evidência anterior; candidatos native-web sem Unity |
| inspeção visual estática | PASS parcial — bakery/watermill e captura real abertas |
| pilotos Aseprite/PNG produzidos | PASS — 3 fontes separadas, 3 exports nativos, previews 3x |
| Unity import Point/None/mipmap/PPU/pivô | PASS — importação Editor, GUIDs/meta gerados e paths owned |
| escala real Unity, sorting, acesso e física | PASS parcial — cena regenerada e saved-scene access 8/8 |
| regeneração Town sob lock | PASS — `CreateMvpTownScene.CreateScene`, 24 casas preservadas |
| captura ortho58/8 | PASS — `town-native-a1-20260911`, 14 imagens |
| família residencial estrutural >=4 | PASS estrutural — 4 silhuetas e distribuição 6/6/6/6 em 24 lotes |
| aprovação raiz | PENDING |

## Pendências para o owner de integração

1. Reimportação/regeneração/captura v2 deve ser executada pelo owner que já detém o lock Unity PID 52632.
2. PlayMode/input real continuam fora deste gate; a captura é saved-scene, não aceitação humana.
3. Door leaf/portal e colliders continuam contratos TOWN.C1; o wiring preserva o shell e o
   `HouseDoorInteractable`, mas não certifica traversal real.

Unity validation: NOT RUN for v3 by explicit owner instruction
Command attempted: none (Unity deliberately not launched)
Residual risk: owner must run scoped importer, Town regeneration, validators and capture v3; PlayMode/input and human visual acceptance remain pending.

## Validação TOWN.A1 v2 — 2026-09-11

Owner Unity: validação executada no projeto `D:\Projetos\Cindars_Hope\cindars_hope`, Unity `6000.5.7f1`. As instâncias Unity do projeto `D:\Projetos\Jogos\game_semnome\Unity` foram preservadas.

| Gate | Resultado | Evidência |
|---|---|---|
| Reimport owned town assets | PASS | `TownKeyartSpriteImporter.ApplyOwnedImportSettings`, exit 0, `Logs/town_a1_v2_import.log`; 47 assets (`[TownKeyartSpriteImporter] PASS owned imports configured: 47`) |
| Regeneração canônica TownScene | PASS | `CreateMvpTownScene.CreateScene`, exit 0, `Logs/town_a1_v2_generate.log`; `TownScene.unity` salvo |
| Saved-scene validator | PASS | `ValidateTownKeyartScene.Validate`, exit 0, `Logs/town_a1_v2_validator.log`; physics 194/194 e acesso 8/8 |
| Captura ortho58 + detalhes | PARTIAL | `TownSceneCapture.CaptureRevision`, exit 0, `Logs/town_a1_v2_capture_d3d.log`; 14 PNGs em `art/town-keyart-rework/evidence/town-native-a1-v2-20260911/`, overview/bakery/watermill presentes |
| Alchemy detail | NOT RUN | entrypoint canônico atual não possui view alchemy; nenhum arquivo alchemy foi inventado |
| PlayMode / input / aceitação humana | NOT RUN | captura é saved-scene e o log registra `PlayMode NOT RUN` |

A primeira tentativa de captura com `-nographics` falhou por crash do TilemapRenderer em `Camera.Render` (`Logs/town_a1_v2_capture.log`); a repetição D3D passou. O blocker restante é a ausência de captura dedicada de alchemy no tooling canônico, além da aceitação humana/PlayMode pendente. Status honesto: **PARTIAL**; não promover para UNITY_VALIDATED/ACCEPTED.

Revisão visual do agente (não aceitação humana) abriu overview, bakery e watermill v2; os três arquivos são capturas reais 1536×1024/ortho58 ou ortho8 e estão legíveis.

Comandos de verificação locais: `Aseprite.exe --version`; `Get-FileHash -Algorithm SHA256` nos
pilotos/baselines; `ConvertFrom-Json` no manifesto; `view_image` nos candidatos e na captura real.

## Validação TOWN.A1 v3 — 2026-09-11

| Gate | Resultado | Evidência |
|---|---|---|
| Reimport town assets | PASS | `TownKeyartSpriteImporter.ApplyOwnedImportSettings`, exit 0, `Logs/town_a1_v3_import.log`; 59 assets |
| Regeneração canônica | PASS | `CreateMvpTownScene.CreateScene`, exit 0, `Logs/town_a1_v3_generate.log` |
| Saved-scene validator | PASS | `ValidateTownKeyartScene.Validate`, exit 0, `Logs/town_a1_v3_validator.log`; TownAccess 8/8 e physics 194/194 |
| Capturas D3D | PASS | `TownSceneCapture.CaptureRevision`, exit 0, `Logs/town_a1_v3_capture_d3d.log`; 17 PNGs + `inputs.json` em `art/town-keyart-rework/evidence/town-native-a1-v3-20260911/`, incluindo overview, bakery, watermill e alchemy |
| Escala efetiva | PASS | `inputs.json` linhas 440–448: bakery/watermill/alchemy facade, roof e door leaf com `scale=(1.00, 1.00, 1.00)` |
| PlayMode/aceitação humana | NOT RUN | captura saved-scene; não certifica input/traversal nem aceite humano |

Status v3: **UNITY_VALIDATED** para import/regeneração/validator/captura scoped; PlayMode e aceite visual humano permanecem pendentes.

## Validação TOWN.A1 v4 — 2026-09-11

| Gate | Resultado | Evidência |
|---|---|---|
| Reimport town assets | PASS | `TownKeyartSpriteImporter.ApplyOwnedImportSettings`, exit 0, `Logs/town_a1_v4_import.log`; 63 assets |
| Regeneração canônica | PASS | `CreateMvpTownScene.CreateScene`, exit 0, `Logs/town_a1_v4_generate.log` |
| Saved-scene validator | PASS | `ValidateTownKeyartScene.Validate`, exit 0, `Logs/town_a1_v4_validator.log`; física 194/194 e acesso 8/8 |
| Capturas D3D | PASS | 17 PNGs + `inputs.json` em `art/town-keyart-rework/evidence/town-native-a1-v4-20260911/`; overview/bakery/watermill/alchemy presentes |
| Escala efetiva | PASS | `inputs.json` linhas 440, 443 e 446: bakery/watermill/alchemy facade com `scale=(1.00, 1.00, 1.00)` |
| Alchemy normal sem retângulos técnicos | FAIL | inspeção do agente em `town-detail-alchemy-door.png`: fachada contém grade/retângulos técnicos visíveis; overlay separado também contém os retângulos esperados |

Primeiro blocker v4: `town-detail-alchemy-door.png` não é uma captura normal limpa; arte/scene apresenta elementos técnicos na imagem normal. Não corrigi código/assets. Status: **FAIL** apesar dos gates de import, geração, escala, acesso e física passarem.

## Validação TOWN.A1 v6 final — 2026-09-11

| Gate | Resultado | Evidência |
|---|---|---|
| Reimport town assets | PASS | `TownKeyartSpriteImporter.ApplyOwnedImportSettings`, exit 0, `Logs/town_a1_v6_import.log`; 67 assets |
| Regeneração canônica | PASS | `CreateMvpTownScene.CreateScene`, exit 0, `Logs/town_a1_v6_generate.log` |
| Saved-scene validator | PASS | `ValidateTownKeyartScene.Validate`, exit 0, `Logs/town_a1_v6_validator.log`; física 194/194 e acesso 8/8 |
| Capturas D3D | PASS | 17 PNGs + `inputs.json` em `art/town-keyart-rework/evidence/town-native-a1-v6-20260911/` |
| Scale/alpha | PASS | `inputs.json` linhas 440–448: bakery/watermill/alchemy facade, roof e door leaf com `alphaMeasured=True` e `scale=(1.00, 1.00, 1.00)` |
| Alchemy normal/overlay | PASS | inspeção do agente: `town-detail-alchemy-door.png` mostra o novo sprite detalhado sem grade/retângulos; retângulos aparecem somente em `town-overlay-alchemy-door.png` |
| Famílias | PASS | manifesto TOWN.A1 mantém 4 famílias estruturais |

Status v6: **UNITY_VALIDATED** scoped; PlayMode e aceitação visual humana continuam fora desta execução.

## Validação TOWN.A1 v7 final — 2026-09-11

| Gate | Resultado | Evidência |
|---|---|---|
| Reimport somente alchemy v06 | PASS | Unity refresh exit 0, `Logs/town_a1_v7_alchemy_import.log`; importer registra exatamente `town_alchemy_native_v06_door_leaf.png` e `town_alchemy_native_v06_facade.png` |
| Regeneração canônica | PASS | `CreateMvpTownScene.CreateScene`, exit 0, `Logs/town_a1_v7_generate.log` |
| Saved-scene validator | PASS | `ValidateTownKeyartScene.Validate`, exit 0, `Logs/town_a1_v7_validator.log`; física 194/194 e acesso 8/8 |
| Capturas D3D | PASS | 17 PNGs + `inputs.json` em `art/town-keyart-rework/evidence/town-native-a1-v7-20260911/` |
| Escala 1× / alpha | PASS | `inputs.json` linhas 440–448: bakery/watermill/alchemy com `alphaMeasured=True`, `scale=(1.00, 1.00, 1.00)` |
| Alchemy close normal/overlay | PASS | inspeção do agente: normal mostra porta detalhada com janela arqueada âmbar e ferragens; sem retângulo simplificado. Retângulos aparecem apenas no overlay técnico |
| Abertura/reveal PlayMode | NOT RUN | permanece validação separada |

Status v7: **UNITY_VALIDATED** scoped; abertura/reveal e aceitação humana continuam pendentes.
