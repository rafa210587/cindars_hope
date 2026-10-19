# SPEC — bases físicas de props na Farm e Town

> Spec ID: spec_playtest_farm_town_prop_physics_v1
> Status: PARTIAL; EditMode 23/23 e PlayMode focal 3/3 PASS, rotas gerais/cais Town/captura visual pendentes.
> Type: Scene Physics/Editor
> Domain: Farm/Town
> Priority: P1
> Owner: geradores e contratos físicos Farm/Town, testes focais e relatório próprio.
> Depende de: composição atual de FarmScene/TownScene; não substitui specs de keyart.
> Bloqueia: aceite de circulação física e leitura de props do playtest.
> Ordem de execucao: após auditoria estática D24, antes do aceite integrado de Farm/Town.
> Validation: EditMode focal + diagnóstico da cena salva + PlayMode headless de colisão; caminhada humana pendente.
required_adrs: []
required_game_rules: []

# /speckit.specify

## Objetivo e estado inicial

Alinhar bases sólidas acessíveis de postes, caixas, feno, poço e cercas com a arte atual, sem bloquear chão decorativo ou corredores. Em FarmScene, `Visual_SouthYardWestPost` e `Visual_SouthYardEastPost` são só Transform+SpriteRenderer; o gerador dá bases a crate/trough/hay em `FarmSettlementVisualComposer.Prop`, mas não a fence. `Visual_HomesteadHay`, `Visual_HouseCrates` e `Visual_ShedCrate` também são visuais puros. Em TownScene, `TownWell` e `GardenFence_00` não têm collider próprio; o censo anterior declarou 2.661 apresentações isentas e seu hash não coincide com a cena atual. Pontes Town já têm abertura da água e deck caminhável, mas o encaixe dos apoios visíveis não foi medido. Esses fatos demonstram ausência local, não a ausência de qualquer collider sobreposto.

## Escopo e contratos

- Em cada candidato, verificar support efetivo na cena e acessibilidade do pé antes de criar outro sólido. Usar base curta no chão, nunca o alpha completo de copa, telhado, toldo ou encosto.
- Criar suporte apenas para props volumétricos acessíveis. **Matinhos, arbustos, flores, grama, tapetes, sombras e efeitos não recebem Collider2D.** Props de fundo ou já cobertos por sólido agrupado não recebem duplicata.
- Farm: preservar footprints, portais, cultivo, água, tint/alpha da Tilemap, IDs e layers. Não salvar/regenerar FarmScene se isso apagar cores de células; nesse caso, preferir wiring idempotente no owner runtime existente ou deixar materialização pendente de solução segura.
- Town: preservar 24 portas, 29 NPCs, 84 âncoras, pontes com deck livre, bordas de água e rotas de player/NPC. Não usar collider sobre todo o deck.
- Arquivos permitidos: `FarmPropPhysicsInstaller.cs` (novo), `FarmSceneRuntimeBootstrap.cs` (hook e solo), `TownPropPhysicsInstaller.cs` (novo), `TownSceneRuntimeReferenceInstaller.cs` (hook), `PlaytestPropPhysicsInstallerTests.cs` (EditMode), `PlaytestPropPhysicsScenePlayModeTests.cs` (PlayMode), esta spec e `docs/validation/spec_playtest_farm_town_prop_physics_v1/REPORT.md`. Se a cena puder ser salva com preservação comprovada, a alternativa de gerador usa `CreateMvpFarmScene.cs`, `FarmSettlementVisualComposer.cs`, `CreateMvpTownScene.cs` e helpers Town, mas esses arquivos não são necessários no patch atual. Cenas somente via Editor API após preservação de hash/tints; YAML manual proibido.
- Fora: Cave, NPC/Enemy AI, save, gameplay de interação, novo sistema global de colisão e arte.

# /speckit.plan

## Plano de execução

1. Auditar collider efetivo e camadas por candidato. Registrar suporte existente, exceção de fundo ou lacuna física.
2. No início de `FarmSceneRuntimeBootstrap.Start`, chamar `FarmPropPhysicsInstaller.Ensure(scene)`, que usa os roots canônicos e cinco visuais nominados para criar `SolidFloorContact` apenas se não houver suporte estático efetivo; depois registrar os bounds novos na máscara de solo não arável. No início de `TownSceneRuntimeReferenceInstaller.Start`, chamar `TownPropPhysicsInstaller.Ensure(scene)` para `TownWell` e `GardenFence_*`. `Awake` ocorre antes de a cena materializada estar completamente carregada e não é gate confiável para inventário de roots. Nenhum save/regen da Tilemap.
3. Adicionar testes de mapeamento/idempotência e validar cenas salvas: `PlaytestPropPhysicsInstallerTests` (2/2 EditMode PASS, rodada combinada 23/23); `PlaytestPropPhysicsScenePlayModeTests` (3/3 PlayMode PASS) verifica suporte efetivo/vegetação, cais Farm e água adjacente, e faz `Collider2D.Cast` horizontal com o corpo real nas duas pontes.
4. Registrar separadamente código, cena materializada, consultas físicas e movimento. O root coordena Unity headless; sem controle de tela.

# /speckit.tasks

## Critérios de aceite

- AC01: postes do pátio sul e props Farm volumétricos acessíveis mapeados para exatamente um suporte efetivo ou exceção documentada; `FarmScene` preserva todas as cores/alphas de Tilemap e IDs.
- AC02: poço/cercas Town acessíveis têm suporte alinhado ao pé; apresentações de fundo recebem exceção explícita; censo usa hash da TownScene vigente.
- AC03: duas pontes e cais mantêm centro transitável e água/rails bloqueados conforme design; cast bilateral do corpo real não encontra novo bloqueio.
- AC04: nenhuma instância de matinho/arbusto/flor/grama ganha collider; testes enumeram as famílias afetadas.
- AC05: rotas e interações existentes permanecem; regressões reais ficam FAIL, não mascaradas por tolerância ou isenção genérica.

## Falhas e limites

Se a cena salva não puder ser alterada com segurança, código de gerador PASS não conclui AC01/AC02. Diagnóstico de collider presente não substitui caminhada/contato. Se uma base coincide com footprint existente, registrar o responsável em vez de acrescentar colisor. Evidência de PlayMode headless não equivale ao aceite visual humano.
