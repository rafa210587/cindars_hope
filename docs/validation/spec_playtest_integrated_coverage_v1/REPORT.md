# Pacote de correções do playtest — evidência de 2026-09-27

Status: PARTIAL. Fonte: `docs/refinements/a_implementar/ref_playtest_correcoes_e_experiencia_v1.md` (D01–D22). As sete specs permanecem em `.specs/a_implementar/`; nenhuma foi promovida ou aceita.

## Execução verificada

- Unity EditMode integrado: `TestResults/playtest-corrections-editmode.xml`, 166/166 PASS, failed=0, runner exit 0. Abrange save/crafting, derrota/respawn, UI, pesca, wiring e validadores.
- Após ajustes de diálogo e revelação da estufa existente: `TestResults/playtest-corrections-final-editmode.xml`, 29/29 PASS, failed=0, runner exit 0. Comando: `pwsh -File tools/unity/RunUnityEditModeTests.ps1 -TestFilter 'GreenhouseRevealWiringTests|RoofRevealControllerTests|DialogueOverflowTests|PauseMenuControllerTests|CraftingSaveRoundTripTests|DeathRecoveryRegressionTests' -ResultsPath TestResults/playtest-corrections-final-editmode.xml -LogFile Logs/playtest-corrections-final-editmode.log`.
- Após revisão independente: `TestResults/playtest-input-closeout-editmode.xml` 25/25 PASS e `TestResults/playtest-risk-closeout-editmode.xml` 35/35 PASS, runner exit 0. O último inclui timeout da pesca e ID de spawn contra a FarmScene serializada.
- Após detectar que o validador não contava o ID literal de retorno à Farm: `TestResults/playtest-transition-closeout-editmode.xml` 22/22 PASS, runner exit 0; o alias continua incapaz de resolver portal cujo target literal não existe.
- PlayMode automatizado `-batchmode -nographics -testFilter GameRuntimeCompositionRootPlayModeTests`: `TestResults/playtest-composition-playmode.xml` 2/2 PASS, exit 0. Confere composição única, carregamento de Farm/Town/Cave sem scripts ausentes e reveal criado pelo host da estufa na FarmScene serializada. A cena não foi salva; não substitui jogar as rotas A/B.
- `CindarsHope.Editor.CindarsHopeMenu.ValidateProjectBatch` em batch/read-only: **FAIL**, exit 1, `Logs/playtest-project-validation.log`. FarmScene 64/64 PASS; as duas picaretas não têm erro WeaponId (apenas INFO opcional de AllowedEquipmentSlots). Único passo com erro: transições, 3 diagnósticos estáticos da Cave (`UNRESOLVED_TARGET_ANCHOR`, `MISSING_GATE`, `MISSING_ANCHOR`). Ausência de wiring estático não foi convertida em PASS.
- Reparo focal das duas picaretas via Editor API: `Logs/playtest-repair.log`, primeira execução mudou 2, segunda mudou 0. Somente `item_tool_pickaxe_iron.asset` e `item_tool_pickaxe_steel.asset` foram gravados; não houve geração global.
- `git diff --check` nos arquivos alterados do pacote: exit 0; avisos CRLF sem erro de whitespace.
- FarmScene: tentativa de salvar via Unity removeu tint/alpha de 12.541 células; o arquivo da cena foi restaurado ao estado anterior. A revelação da estufa já presente na cena é conectada em runtime pelo `GreenhouseRuntimeHost`, coberta por `ExistingSceneHost_WiresRevealWithoutSavingOrRegeneratingScene`. Nenhuma alteração de FarmScene permanece no diff.

## Gates e limites

| Spec | Nível comprovado | Promoção | Pendente decisivo |
|---|---|---|---|
| crafting_save_roundtrip | UNITY_VALIDATED | NO | save/load jogado na rota A |
| death_recovery | UNITY_VALIDATED | NO | derrota, Fonte e corpo jogados na rota B |
| ui_access_feedback | UNITY_VALIDATED | NO | foco, layout e feedback visual da rota A |
| fishing_lifecycle | UNITY_VALIDATED | NO | janela de mordida jogada na rota B |
| wiring_validation | UNITY_VALIDATED | NO | equipamento/portais no jogo e strict global |
| world_readability | PARTIAL | NO | Cave, escala, circulação, combate e comparação visual |
| integrated_coverage | PARTIAL | NO | AC02/AC03/AC05 e strict global |

- Docs strict: **SCOPED_PASS**, `Logs/playtest-docs-strict.log`. Metadados de 29 specs legadas reconciliados; duas Town históricas movidas para `.specs/absorvidas/city_relayout_reconciliation/` e indexadas. Os quatro IDs `04_spec_*` continuam estáveis, com formato reconhecido pelo validador.
- Architecture strict: **FAIL**, 13 violações em módulos preexistentes fora das sete specs; nenhuma em `CraftingModal.cs` ou `DialogueModal.cs` após concentrar a leitura das teclas mantendo a prioridade. Um falso positivo em comentário de `PlayerAttackController` foi removido. As restantes envolvem estado/lifecycle de skills, owners de input/cena/spawn e consultas físicas NPC/combate; exigem fatias próprias de refatoração, não alteração cosmética do baseline. Log: `Logs/playtest-architecture-strict.log`. O gate global continua impedindo aceite.
- AC01 da spec integrada: testes de comportamento relevantes PASS. AC02 e AC03: **NOT RUN após as correções**. AC04: cobertura restante listada abaixo. AC05: closeout parcial; nenhum PASS global alegado.
- Usuário solicitou não assumir controle da tela neste momento. Nenhuma interação visual, navegação em Play Mode ou captura da Cave/Farm/Town foi feita após essa instrução. EditMode headless não prova layout, foco, circulação ou sensação de combate.
- D15: retângulo claro na Cave permanece sem renderer confirmado; `InspectCaveRenderComposition` é diagnóstico read-only, não prova a causa. D17–D19: escala, circulação, timing/efeitos de combate requerem observação e input contínuo. Sem modificação especulativa de cena/procedural.
- Pré-checagem estática D17: FarmScene ortho 8,5, TownScene ortho 8; PixelPerfectCamera em ambas declara PPU 128; `FarmScaleContract` declara ator 32×48 px. Os valores de cena/catálogo não substituem medida do sprite renderizado nem as capturas comparáveis exigidas.
- Quests, skills, atributos, demais estações, consumíveis, bosses, áudio e gamepad: **NOT RUN** no pacote integrado. Abertura anterior de menus não valida cada contrato.

## Próximo cenário de aceite

Quando houver autorização para controle da tela, executar rota A: boot → pausa/retomar → inventário/equipamento → loja → craft → save/load, conferindo foco, feedback e quantidades restauradas. Executar rota B: cultivo → pesca (antes/durante/depois da mordida) → Cave → combate → derrota com lágrima → Fonte → recuperação do corpo, conferindo IDs/itens/posição. Observar Cave, estufa, diálogos longos, clipping/layout em 1280×720 e 1920×1080; registrar renderer do retângulo claro e medir bloqueios de circulação antes de alterar arte/colisões. Anotar preparação controlada, esperado/observado, resultado e evidência por etapa. Nenhum save pessoal do usuário.

Promoção por `/finish-spec`: **NO** para as sete specs. `CODE_COMPLETE` ou `UNITY_VALIDATED` vale apenas para critérios cobertos; status do pacote **PARTIAL** por gates strict e rotas/aceites visuais pendentes.
