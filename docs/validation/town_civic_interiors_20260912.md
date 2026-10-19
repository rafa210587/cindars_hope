# Town — interiores cívicos, 2026-09-12

## Escopo / critérios extraídos

Continuação autorizada de `spec_town_components_doors_and_collision_v1`, composição funcional de House_Prison, House_Archive, House_Registry e House_GateKeeper. Critérios deste recorte: C-4 (móveis sólidos, entrada/reveal/saída), C-8 (interiores utilizáveis/coerentes), limites visuais e preservação de C-3/C-6. Não implementa prisão funcional, pesquisa, burocracia, combate ou novas interações.

## Sistemas reutilizados e mudanças

- Criador canônico `CreateMvpTownScene.CreateScene`, `CreateInteriorProp`, paredes existentes, HouseDoorInteractable e RoofRevealController. Sem YAML manual, novo runtime ou troca de scene.
- Arquivo: três armários provisórios e mesa de consulta lateral, sem cama/cozinha. Registro: armários e balcão, sem cama/cozinha. Prisão: cama simples, mesa e armazenamento; sem cozinha. Guarita: mesa lateral, armazenamento e cama de descanso. A intenção de arquivo/documentos ainda não é representada pela arte específica dos armários.
- Sprites escolhidos pela imagem real. Os arquivos históricos são semanticamente trocados: `chair.png` contém armário com louças, não livros; `kitchen_counter.png` contém mesa; `bed.png` contém cama. Curadoria é local aos quatro prédios. Não renomeamos ou alteramos as imagens compartilhadas, nem corrigimos silenciosamente os outros dezenove interiores.
- Escala uniforme calculada por bounds/PPU, mantendo proporção da arte. Root aumentou camas, moveu as mesas do arquivo/guarita para liberar o centro e ativou a representação existente das paredes nesses quatro interiores; o reveal continua ocultando as paredes quando o jogador está fora.
- Mapeamento explícito dos novos nomes locais no catálogo de suporte e nas larguras de apresentação. Não foram alterados recipes/hashes de sprites ou regras globais de física.
- Captura pelo MainCamera real a 800×600 com enquadramento determinístico pelos bounds do lote; câmera/RenderTexture restaurados e recursos liberados. Não é captura de teclado/HUD nem aprovação humana.

## Evidência e preservação

Baseline salvo SHA256 `A596653704F27296E2085F0C0CCE4D2594C1E30D271478DE756D8FF2EA87A8E7`. Backup em `C:/Users/Rafa/AppData/Local/Temp/town-interiors-baseline-4e5cb4c4-392e-42b5-b959-6a71265809e5/TownScene.unity`.

Capturas antes: [Prisão](town_all_entries_20260912_233702/interior_prison.png), [Arquivo](town_all_entries_20260912_233702/interior_archive.png), [Registro](town_all_entries_20260912_233702/interior_registry.png), [Guarita](town_all_entries_20260912_233702/interior_gatekeeper.png). Raiz inspecionou as quatro.

O passe baseline de captura não repetiu o 23/23 da rodada anterior: os quatro alvos passaram, mas houve interferência do NPC Savra no seletor de Residential_4 e precondição inconclusiva de porta fechada em Pip/Residential_3. NPCs permaneceram ativos. Esse resultado é preservado em `town_all_entries_20260912_233702/town-all-entries.json`; não é atribuído a mudanças de interiores ainda não geradas.

Primeiro comando de captura falhou por colisão de namespace `Camera`; raiz corrigiu os tipos para `UnityEngine.Camera` e r2 produziu as imagens. Geração inicial completou com exit 0; uma segunda geração incorpora a mesa da guarita na lateral. Resultados finais serão acrescentados após execução.

## Gates e organização

Unity 6000.5.7f1; owner único root. Dois executores Terra: composição/testes e captura. Root revisou os artefatos, corrigiu proporção, posicionamento, namespace e localização do RoofReveal na fixture. Um revisor adicional não pôde ser iniciado por limite de threads; revisão de integração ficou com root, sem claim de auditoria independente adicional.

Sem commit/push, sem mudanças de NPC IDs/schedules, saves, FarmScene ou CaveScene nesta entrega. Outros edits concorrentes preservados. Sem geração de imagens paga ou edição Aseprite: o recorte reutiliza arte existente. Skills de integração influenciaram a medição separada de imagem e collider e a comparação antes/depois.

## Status

PARTIAL na composição funcional/arte final; critérios físicos automatizados abaixo passaram. Não promove a spec global nem atribui porcentagem de proximidade à keyart. Permanecem a curadoria dos demais interiores, assets próprios de prisão/guarda/documentos, serviço completo, áudio e aceite artístico/humano.

## Resultados finais — SCOPED

| Critério / gate | Evidência | Resultado |
|---|---|---|
| Geração canônica | `Logs/town-civic-interiors-regen-20260912-r2.log`, `CreateMvpTownScene.CreateScene` | PASS, exit 0; uma TownScene esperada/materializada |
| EditMode cena/apoios/rotas locais | `Logs/town-civic-interiors-edit-20260912.xml` e `.log` | PASS 4/4, exit 0; log scan sem erros críticos |
| C-6 desvios do grafo final | `Logs/town-civic-interiors-detour-20260912.log` | PASS 162 pares, zero erros/avisos, pior 2,9x; exit 0 |
| C-3/C-4 entradas físicas | [JSON final](town_all_entries_20260912_235022/town-all-entries.json), `Logs/town-civic-interiors-after-20260912-r2.log` | PASS 23/23; quatro alvos incluídos |
| Suporte visual local | mesmo JSON | 161 registros, zero falhas; não equivale a 161 móveis únicos nem censo C-5 global |
| Cena/saves/setup | hashes/flags do JSON | Todos preservados após teste |
| C-8 composição por função | [comparação antes/depois](town_civic_interiors_comparison_20260912.html) | PARTIAL: novo layout, arte especializada e sorting pendentes |

Comandos: Unity `-batchmode -nographics -executeMethod CindarsHope.Editor.SceneCreation.CreateMvpTownScene.CreateScene -quit`; runner `tools/unity/RunUnityEditModeTests.ps1 -TestFilter 'TownCivicInteriorTests;CorralScheduleLanes_KeepHundAndGurdTransitionsWithinDetourLimit'`; probe `-batchmode -force-d3d11 -executeMethod CindarsHope.Editor.Validation.TownAllEntriesPlayModeProbe.RunBatch`, sem `-quit`. Todos com `-projectPath D:/Projetos/Cindars_Hope/cindars_hope` e logs explícitos acima.

O primeiro início do EditMode encontrou o lock do gerador ainda encerrando; após confirmar término, a execução ocorreu normalmente, sem matar processos. O primeiro passe pós-geração teve 22/23 e parou numa porta já fechada de Residential_2; o log não comprova por si só a causa do fechamento. O balance real configura fase diurna de três minutos. A precondição de loja aberta agora retorna o relógio ao meio-dia a cada lote, e a rodada final terminou 23/23. Horários/fechamento de serviço são outro cenário; não usamos esse ajuste para alegar validação de 24h.

Cena final SHA256 `1A858CE41ED8981CA729014F009B28A23ABCB742E0129161B97250D4ACF92229`; [inputs verificados](town_civic_interiors_inputs_20260912.txt). O ajuste posterior somente do relógio no probe não invalida os quatro testes EditMode dos mesmos fontes/cena.

### Revisão visual da raiz

Inspecionadas quatro imagens antes e quatro depois no mesmo enquadramento. Melhoras confirmadas: móveis legíveis, cama maior, paredes visíveis e distinção de layout; corredores principais livres no teste. Limites concretos: o armário reutilizado contém louças, não documentos; a mesa do Registry encobre parte da cabeça do jogador no enquadramento, exigindo revisão de sorting; faltam grades/cela e equipamento visual de guarda. Não marcar esses pontos como resolvidos por uma imagem ou por um collider. A galeria usa o primeiro passe após gerar; a geometria/arte são idênticas às da rodada final.
