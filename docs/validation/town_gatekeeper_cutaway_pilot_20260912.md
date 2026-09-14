# Guarita — piloto de paredes recortadas e proporção

> Histórico: a imagem deste piloto foi rejeitada pelo usuário. O texto abaixo não representa aceite visual atual. Correção e evidência mais recentes: [nova rodada](town_room_repair_20260912.md).

## Escopo autorizado

Pedido humano de 2026-09-12: parede de fundo visível, laterais finas, frente recortada, móveis físicos e proporcionais ao personagem. Piloto House_GateKeeper sob C-4/C-6/C-8 de `.specs/a_implementar/spec_town_components_doors_and_collision_v1.md`. Não promove a spec completa nem aplica o piloto aos demais interiores.

## Critérios do piloto

- Fundo com face vertical; laterais com espessura visual máxima de 0,18 u; frente sem face alta, rodapé de até 0,10 u permitido. Vão da porta preservado.
- Limites visuais alinhados à borda interna das paredes físicas; colliders das paredes e integração de porta/reveal preservados.
- Escala uniforme dos móveis, baseada na altura opaca do personagem com perfil Town final, não na escala inicial do criador. Medir cama, mesa e armário; não confundir padding com altura visível.
- Três bases sólidas materializadas. Cast do collider real do personagem, partindo do pocket livre, encontra cada base; isso é evidência de consulta física, não de caminhada em todos os ângulos.
- Entrada, circulação central, proteção ao fechar a porta e saída continuam funcionando com Player físico e NPCs ativos.
- Comparação pelo MainCamera no mesmo enquadramento de 800×600. Ordenação visual e aceite humano são separados dos testes de colisão.

## Preservação e baseline

Antes: `town_all_entries_20260912_235022/interior_gatekeeper.png`. Backup da TownScene atual: `C:/Users/Rafa/AppData/Local/Temp/town-gatekeeper-pilot-aa00ce02-1a10-474e-a62f-ba9ec098b9bf/TownScene.unity`.

Sem novos sprites, alteração de saves/IDs, runtime compartilhado, Farm/Cave ou NPC schedules. Reutilização de arte existente; armário com louças continua provisório para uma guarita. Não há claim de porcentagem de fidelidade à keyart.

## Validação

Unity 6000.5.7f1, owner único root, comandos sequenciais com projeto `D:/Projetos/Cindars_Hope/cindars_hope`.

- Geração canônica: PASS, exit 0, uma TownScene esperada/materializada. `-batchmode -nographics -executeMethod CindarsHope.Editor.SceneCreation.CreateMvpTownScene.CreateScene -quit`; log `Logs/town-gatekeeper-pilot-regen-20260912.log`.
- EditMode SCOPED: PASS 6/6, exit 0 e log scan sem erros críticos. Runner `tools/unity/RunUnityEditModeTests.ps1 -TestFilter 'TownCivicInteriorTests;CorralScheduleLanes_KeepHundAndGurdTransitionsWithinDetourLimit'`, outputs `Logs/town-gatekeeper-pilot-edit-20260912.xml` e `.log`.
- Primeiro início da captura: FAIL antes do probe, exit 1. `Logs/town-gatekeeper-pilot-live-20260912.log` registra CS0246 em testes Phase21 concorrentes, fora do escopo. Nenhum arquivo Phase21 foi alterado por esta tarefa. Após observar alterações nesses arquivos pela outra sessão, foi tentado novo início.
- Primeiro PlayMode completo: PASS 23/23, 161 registros físicos sem falhas; cena/saves/setup preservados. JSON `town_gatekeeper_pilot_20260912_run1/town-all-entries.json`, log `Logs/town-gatekeeper-pilot-live-20260912-r2.log`. Três casts do Player encontraram as bases dos móveis da guarita a partir de origem livre. Os hits também identificam suportes medidos filhos existentes: o collider da raiz não é a única barreira desses móveis.
- Revisão da primeira captura: móveis maiores e porta utilizável, porém laterais quase invisíveis e fundo excessivamente escuro. O sprite builtin não tinha bounds de 1 u; o teste só de largura máxima não detectava o encolhimento. Corrigido para `wall_stone` tiled com tamanho real, cor de fundo mais clara e testes com limite mínimo e máximo. Não houve alteração física nesta correção visual.
- Regeneração visual r2: exit 1 antes do método; causa não estabelecida pelo log. r3 materializou e salvou TownScene, mas ficou parado em encerramento por mais de 80 s. Root verificou identidade e encerrou somente seu batch PID 78672 e worker filho 83564; não há exit 0 natural para r3. Log `Logs/town-gatekeeper-pilot-regen-20260912-r3.log`; validade da cena é decidida por reabertura/testes/captura posteriores, não por alegação de encerramento bem-sucedido.
- EditMode final: PASS 6/6, exit 0, `Logs/town-gatekeeper-pilot-edit-20260912-r2.xml` e `.log`. Inclui largura mínima/máxima dos trims, proporções, sólidos e corral. Warnings de APIs obsoletas em testes concorrentes não foram ocultados; sem erros críticos no scan.
- Tentativa isolada: FAIL, `SCOPED_GATEKEEPER_ONLY` / 1 lote, JSON `town_gatekeeper_pilot_20260912_run2/town-all-entries.json`, log `Logs/town-gatekeeper-pilot-live-20260912-r3.log`. Player parou em (30.90,-45.99) na fase walking_inside, porta aberta e blocker desabilitado. Cast anterior retornou passagem e destino livres. Causa dinâmica não isolada; não atribuir automaticamente ao NPC nem considerar resolvida pela próxima rodada.
- Captura final obtida no percurso completo: guarita PASS, 22/23 gerais e resultado agregado FAIL. House_Pip inconclusiva: porta permaneceu aberta com NPCs ativos, impossibilitando precondição de porta fechada. JSON `town_gatekeeper_pilot_20260912_run3/town-all-entries.json`, log `Logs/town-gatekeeper-pilot-live-20260912-r4.log`. Cena/saves/setup preservados. O probe completo continua exigindo os 23 lotes quando a variável de escopo é omitida.

Comando de captura: Unity `-batchmode -force-d3d11 -executeMethod CindarsHope.Editor.Validation.TownAllEntriesPlayModeProbe.RunBatch`, sem `-quit`, com `CINDARS_TOWN_ALL_ENTRIES_OUTPUT` apontando à pasta de cada rodada. Somente a tentativa isolada definiu `CINDARS_TOWN_ALL_ENTRIES_HOUSE=House_GateKeeper`; final omitiu o filtro.

## Revisão final da imagem e limites

Root inspecionou a [captura final](town_gatekeeper_pilot_20260912_run3/interior_gatekeeper.png) e a [comparação](town_gatekeeper_pilot_comparison_20260912.html). Fundo texturizado legível, laterais efetivamente finas, frente sem face alta (rodapé), cama/armário maiores e personagem legível na posição capturada. Alturas: cama 2,70 u (1,08× referência), armário 2,20 u (0,88×), mesa 1,176 u (0,47×). Parede de fundo 1,622 u; lados 0,18 u; rodapé 0,10 u.

Entrega limitada a piloto visual/funcional, não aceite global. Permanecem: travamento da entrada isolada, estabilização do cenário de tráfego em Pip, arte específica de guarda (armário atual tem louças), acabamento de piso/perímetro e observação de sobreposição por todos os lados dos móveis. Ordenação por centro é local à guarita; a captura central não prova todas as situações de profundidade. Nenhum claim de input E real, aprovação humana, 100% da cidade ou spec concluída.

As skills de integração e validação levaram à medição opaca, separação visual/collider e rejeição do primeiro passe visual apesar de seus testes passarem. Sem edição ou geração de imagem: sprites compartilhados preservados.

Validador completo de desvios NPC: NOT RUN nesta rodada. Duas tentativas `-executeMethod CindarsHope.Editor.Validation.ValidateTownRouteDetourRatio.Validate -quit` encerraram na inicialização com exit 1, antes do método (`Logs/town-gatekeeper-pilot-detour-20260912.log` e `-r2.log`). Foi observado outro processo Unity para este projeto durante a janela; ele não foi encerrado por esta tarefa. O teste local do corral passou nos seis EditMode, mas não substitui os 162 pares do validador. Risco residual: desvios completos do grafo regenerado não revalidados.

Comparação disponibilizada em HTML e captura local; pedido de abertura no painel retornou `queued`, não confirmação de navegador visível. Sem commits/push. Diff whitespace dos quatro fontes do piloto: PASS; sem promoção de status global.

Cena da primeira rodada SHA256: `D6F304ECD38BF4A8153A53A1D1FF42486BDA3B243D69050918253779E39938DC`. Cena após revisão visual: `1B3FE43610DC43E1A78FC07E44E9806A2569C8F953A7B2EDA6A1B7B045A9753A`.

Implementação delegada a executor existente; root revisou e corrigiu escala de referência, mesa, suporte inferior do armário e fallback da parede, além de estender o probe. Auditor independente adicional não pôde iniciar por limite de threads; não há claim de auditoria independente.

### Referência de escala medida

Leitura dos PNGs sem editá-los, via System.Drawing: `walk/down/walk_down_01.png` tem canvas 41×72 e alfa ocupado 40×71. PPU 128 e perfil Town 4,5 resultam em altura opaca de referência 2,49609375 u. Isso caracteriza esse frame, não todas as animações. Cama: 61×64; armário `chair`: 49×64; mesa `kitchen_counter`: 98×64, todos com extents opacos iguais ao canvas. A aproximação inicial de 2 u foi descartada antes da geração.

### Inputs finais SHA256

Paths relativos à raiz do projeto:

```text
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs
30DF5D0D93DA31CB781613E9A7AC354F85585BF870E7545FE6AC2A1D717BFED1
Assets/_Game/Scripts/Editor/SceneCreation/City/TownKeyartInteriorPresentation.cs
D9F3B11D8DE5BD3B29C2FB8C1F85E7F6494B63BC75B81AA4B9800E1F183D2DED
Assets/_Game/Scripts/Editor/Validation/TownAllEntriesPlayModeProbe.cs
2BFC89A7C582BEAF77D4DD0962506218F78D8FFB472C51188CF90AA1A0C75828
Assets/_Game/Tests/EditMode/City/TownCivicInteriorTests.cs
053D2B8820C9870A452F9B896A658A86AE21809097FF6033382DB3CDDD356FD9
Assets/_Game/Scenes/TownScene.unity
1B3FE43610DC43E1A78FC07E44E9806A2569C8F953A7B2EDA6A1B7B045A9753A
```
