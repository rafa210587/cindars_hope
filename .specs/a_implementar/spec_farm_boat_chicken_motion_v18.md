# Farm v18 — barco e piloto de animais

Status: INTEGRATED_SCOPED_PASS. Autorização humana: criar spec/plan/tasks, revisar e implementar.
Fonte: docs/refinements/a_implementar/ref_farm_boat_animal_motion_v1.md.

## Spec
Entrega coesa: barco animado e galinha comprada/solta/restaurada com apresentação e passeio seguros. Vaca/ovelha reutilizam segurança do movimento existente; novos ciclos específicos ficam na expansão após piloto. Não apresentar sprites estáticos como animação concluída dessas espécies.
AC1 Barco: ciclo4–6s, até1pixel-fonte de deslocamento, silhueta completa, sem drift/rotação; canvas76x68 e apoio(35,24), transformações/colliders estáveis. Dois ciclos reais em câmera gameplay.
AC2 Galinha: idle/walk/peck/rest, frente/costas/lateral coerentes; caminhada segue deslocamento real, para contra obstáculo. Perfil de conteúdoSO, origem Aseprite em camadas; sem novos custos/produtos.
AC3 Um runtime porAnimalInstanceId na soltura/restauração. Limites pelo abrigo e corpo inteiro; não atravessar sólidos/água, tentativas limitadas e idle seguro se bloqueado. Animais cedem espaço entre si; não bloqueiam player. Alimentação/coleta continuam acessíveis. Dead/Unavailable param.
AC4 Estados cosméticos não alteramFedToday/CareScore/produtos/inventário. RNGlocal de apresentação independente; saveDTOs intactos; reset de fase visual permitido. Sem animais grátis.
AC5 Evidência: testes determinísticos de movimento/limites/bloqueio e interação/restauração pertinente; PlayMode observado, capturas e revisão visual independente. Sem claim de aceitação global.

## Plan
P1 Reutilizar FarmAnimalRuntime, AnimalReleaseHandler, FarmAnimalRegistry e serviços care/product. Extrair decisões/temporização para AnimalMotionState(puro) e parâmetros AnimalMotionProfileSO, com adapterUnity e apresentação AnimalSpriteAnimator. Nomes finais podem ajustar à convenção existente sem duplicar classes equivalentes. TiposSave/serviçosdecare permanecem.
P2 AnimalReleaseHandler deve fornecer limites world-space compartilhados do abrigo, ponto seguro de spawn, perfil e sprites. Mesmo wiring emrestore. FarmAnimalRuntime delega movimento com sweepCollider2D não-trigger-equivalente ou cast explícito de corpo, excluindo triggers/player e respeitando layer mask; seu collider de interação permanece trigger. Recuperação tem timeout/tentativaslimitados; semclamp/teleporte visível. Separe player de obstáculos; considere footprint real. Não implantarNavMesh/framework.
P3 Dados em Assets/_Game/Data/Animals/ e sprites em Assets/_Game/Art/Generated/World/animals/chicken_motion_v18/. GeneratorEditor próprio em Editor/Farm para importarPoint/None/no-mips, pivotpés fixo e criarSO; adaptar GenerateFarmAnimalAssets/assemblypath apenas necessário. Perfis/ciclos cow/sheep opcionaisfallbackestático identificados, não fingir cobertura.
P4 Barco: ampliar FarmAmbientAnimationAuthoring pelo caminho existente; sheetAseprite+JSON em ambient/boat_v18, usar sprite binding jáexistente. Mesmo rendererGround5, apoio/canvas do barco17.4poses iniciais, holds suaves em4–6s. Semnovo ambientmanager. Não alterarfonte/cascata/peixe.
P5 Galinha: asset novo se ausência de fonte compatível confirmada; desenho pixelart original emAseprite autorizado pelo fluxo anterior. Ideal32x32 comsilhueta de galinha legível, escala calibrada no jogo antes deintegrar; proporção pela referência de animais/cenário.4quadroswalk×front/back/side,2idle,3peck,2rest; reutilizarcels quandoidênticos. Orçamento não é critério de qualidade. Não gerarvaca/ovelha antesdopiloto.
P6 Testes scoped: FarmAnimalsRuntimeTests,FarmAnimalCareTests,AnimalProductCollectionTests e novos testes comportamentais de motion. ProbeEditor isolado usaSaveInputoff, cena descartada apósPlay, não save real do usuário. Setup obtém instância por release/restoração canônica quando possível; conferir unicidade/estado via APIsreais. Movimento60–90s comcenárioobstruído, pause, interação/saúde; dois ciclosbarco. Registrar lacunas honestamente.

## Ownership / files
Animal implementer: Assets/_Game/Scripts/Farm/Animals/{FarmAnimalRuntime,AnimalReleaseHandler,AnimalMotion*,AnimalSprite*}.cs; Editor/Farm/GenerateFarmAnimalAssets.cs e novo FarmAnimalMotionAuthoring.cs; Tests/EditMode/Farm/*Motion*Tests.cs e testes runtime existentes se necessário. Pode lerregistry/care/save mas nãoeditar semrevisão.
Art implementer: dev/art/aseprite/keyart-v4/motion-v18/**, candidatosoffline barco+galinha; semAssets/Unity.
Root: integraçãoassets/EditorArt/FarmAmbientAnimationAuthoring.cs,SceneCreation/CreateMvpFarmScene.cs wiring, Editor/Dev probe, generatedFarmScene e docs. Nenhuma alteraçãoTown/Skills/Cave/save/asmdef/package. Preserve edits alheios e originals.

## Tasks
- [x] T1 Revisão de spec e riscos antes de runtime.
- [x] T2 CandidatosAseprite barco+galinha e contrato de export.
- [x] T3 Movimento testável, wiringrelease/restore e perfil/presentação galinha.
- [x] T4 Integração nativa barco+dadosgalinha e geração serialUnity.
- [x] T5 Testes scoped e PlayMode; corrigir somentefalhascomprovadas.
- [x] T6 Revisão independente,HTML,relatório ecloseout proporcional.

Gate: após revisãoT1 sembloqueador, execução autorizada semnova confirmação. Se arte/physics não atender, registrarFAIL e corrigir; não reduzirAC para fechar. Duas tentativas sem ganho exigem rever método, não repetir cegamente. Capturas reais distinguem edição/PlayMode e input físico. Next expansion cow/sheep requires pilot evidence.

## T1 review — accepted amendments
SOUND_WITH_AMENDMENTS, no blocking design issue for pilot. Preserve all FOUR catalog IDs including animal_goat (goat and sheep share enum): resolve presentation by AnimalDataId, never blindly bySpecies. Nonchicken keeps existing fallback explicitly. Freeze briefly during interaction; respect existing pause/time convention. Probe disables save input and restores in-memory scene without writing user saves. T1done; statusEXECUTING. Models: existing art/reviewer inherited; multi-component runtime usesSol per project routing, escalate on failed contract.


Wiring concreto: FarmSceneCapture.RegenAndCapture chama FarmAnimalMotionAuthoring.Generate antesCreateScene. CreateAnimalHousing carregaChickenProfilePath viaAssetDatabase eSetMotionProfiles; coopSetPenBounds(-2.2,-1.8)..(2.2,.1) noapronfrontal, evitaSolidBase sul+.4. Nãoalegarcercadoartísticofechado: limitesnavegáveis sãoexplícitos. RootownsFarmSceneCapture.cs também.

Importer necessário: GeneratedSpriteImporter.cs forçaSingle128 emtodoGenerated; rootadiciona exceção PORCAMINHOEXATO chicken_v18.png paraMultiple32, preservandooutrasregras. Slicingperspritecontinuaauthoring.

Ajustevisualmedido: área coop passa paraoffsets(-.4,-2.6)..(2.2,-1.0), ao ladodocomedouro queocultava asposes noPlay. Colisores/sortingpreservados. Probeusa contençãoinclusiva (Rect.Contains excluimax exato), mantémcorpoint epenetration asserts.

Executionreport:docs/validation/farm_keyart_v4/motion_v18/REPORT.md.62/62tests,PlayC60s19spritesPASS. Tasks deliveredwithinpilot;fullherd/input/globalvisualacceptance remainpending, noautomaticpromotion. Specnotclaimingcow/sheep/goatcyclesimplemented.
