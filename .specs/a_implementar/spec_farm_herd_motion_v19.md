# Farm v19 — expansão dos ciclos de animais

Status: INTEGRATED_SCOPED_PASS; continuação autorizada após piloto v18. Revisão independente: SOUND_WITH_AMENDMENTS incorporada; revisão visual final sem bloqueadores nas amostras. Promoção pendente de aceitação humana.
Baseline: spec_farm_boat_chicken_motion_v18 e motion_v18/REPORT.md. Reusar movimento, interação, recuperação e save existentes.

## Spec
Criar ciclos próprios para animal_cow, animal_sheep, animal_goat, resolvidos porID. Preservar galinha/barco, economia, save e originais. Animais precisam de idle, caminhar frente/costas/lado, pastar e repousar; contato dos pés estável, sem cortes. Vaca maior que pequenos ruminantes; cabra distinguível de ovelha. Não criar animais gratuitos.
AC: três perfis íntegros, sprites Point/None/no-mips, 19 frames por folha na ordem validada. Soltura e restauração preservam perfil, AnimalDataId e produto; cabra e ovelha compartilham enum, mas não identidade. Em Play os três tipos caminham, param e exibem estados; corpo contido e sem penetrar sólidos além da tolerância de 0,011u do piloto. É obrigatório testar o celeiro na capacidade de quatro animais (duas vacas, cabra e ovelha), todos visíveis e acessíveis antes e após restauração. Captura e revisão independente devem comparar silhueta e apoio dos pés com personagem e porta na câmera real, além de canvas/PPU. Registrar limites de input/humano.

## Plan
Arte: dev/art/aseprite/keyart-v4/herd-v19/** emAseprite, fontes cow/sheep_keyart_v4 como referência, semsobrescrever. Canvas48x48,19frames horizontais,apoio(24,8),PPU32 inicialmente. Idle0–1,down2–5,up6–9,side10–13,graze14–16,rest17–18. Perfil da vaca corpo~.9x.55,ruminantes~.7x.45; calibrar pela silhueta/cenacapturada, não aumentar indiscriminadamente.
Editor: generalizar FarmAnimalMotionAuthoring com descritores limitados dosquatroIDs, mantendo contrato da galinha e sprites IDs estáveis. Novos assets animals/herd_motion_v19/{cow,sheep,goat}_v19.png +Data/Animals perfis correspondentes. GeneratedSpriteImporter exceçõesexatasMultiple32. CreateMvpFarmScene wiringallprofiles e áreaBarn segura emterraaberta, medidana composição.
Validação: reusar lógica v18; testesperfil/ID se necessários, testesexistentesmotioncare afetados. Estender probe opcional viaenv separado para rebanho, manter modo chicken intacto. RootúnicoUnityowner. NãoeditarSave/runtimebusinesssemdefeito demonstrado.

## Tasks / ownership
- [x] T1 Revisar contratos/arte/área barn; revisão independente SOUND_WITH_AMENDMENTS incorporada antes da integração.
- [x] T2 Arte offline: três folhas em Aseprite, chifres frontais da cabra corrigidos após revisão independente.
- [x] T3 Root: authoring, importer, cena e perfis integrados; sem mudança no framework runtime.
- [x] T4 Probe Editor: quatro animais, soltura, restore, interação e geometria; revisão estática concluída.
- [x] T5 Root: geração, 65/65 testes, Play B de 60s com quatro animais PASS, HTML e closeout proporcional.

Allowed: arquivos dePlan,Editor/Dev/FarmHerdMotionCapture.cs e FarmPlayModeCaptureSession.cs se necessário, testsEditMode/Farm/HerdMotionProfileTests.cs, docsvalidation/herd_v19/** e status/spec. Preservar trabalhoalheio; nenhumaedição manualYAML. Validar30–60s porherdconjunto, nãoumsmoke separado porsprite. SemalegarfluidezporPNGapenas.

## Evidência parcial
- Geração Unity: exit 0; quatro perfis gerados. 71 árvores mantêm os mesmos transforms.
- EditMode: 65/65 PASS em `docs/validation/farm_keyart_v4/herd_v19/editmode.xml`.
- Play A: FAIL na soltura da ovelha; `InventoryManager.AddItem(item_animal_sheep_lamb)` retornou false. Investigar integração do catálogo existente antes de repetir. Não substituir o fluxo real por mock.

## Emenda de integração — item de cordeiro ausente
O catálogo e AnimalData existentes referenciam `item_animal_sheep_lamb`, mas o gerador omite esse asset. A autorização de implementar os refinamentos inclui corrigir essa dependência demonstrada. Ampliar escopo apenas para `GenerateFarmAnimalAssets.cs`, o novo item e sua referência em `Data/Registries/ItemDatabase.asset`, sempre via Editor. Criar idempotentemente o item com MaxStack 99 e BaseValue 180, seguindo o cabrito como valor inicial explícito; nenhum preço existente muda e não adicionar oferta à loja nesta entrega. Esse valor é uma escolha de implementação, não um preço histórico recuperado. A soltura do cordeiro deve funcionar sem mock nem item de teste substituto.

## Closeout
Play B: quatro solturas/restaurações, todos os 19 sprites e quatro modos por animal, interação pausada e estado preservado; zero erros runtime. 315 frames reais e quatro contextos. Relatório e cenário humano restante: [REPORT](../../docs/validation/farm_keyart_v4/herd_v19/REPORT.md). Human visual/input acceptance: DEFERRED_TO_FINAL_HUMAN_VALIDATION. Spec permanece aqui; scoped PASS não é aprovação global da fazenda.
