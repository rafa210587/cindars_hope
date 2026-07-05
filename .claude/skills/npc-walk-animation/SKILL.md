---
name: npc-walk-animation
description: Gerar, AUDITAR e wire folhas de caminhada 5x5 dos NPCs (ChatGPT web + NpcWalkAnimator). Usar ao criar/regerar walk sheets, ao auditar se os frames realmente animam (passada), ou ao ligar a folha ao NPC no Unity.
---

# Skill: Animação de Caminhada de NPC (gerar + auditar + wire)

Pipeline completo para dar caminhada aos NPCs da cidade: folha 5x5 gerada por IA (ChatGPT web) → **auditoria de movimento** → import/slice → runtime `NpcWalkAnimator`. Complementa [[chatgpt-web-sprite-gen]] (mecânica do Chrome MCP) e a memória `project-npc-walk-animation-pipeline`.

## Quando usar
- Criar ou **regerar** a folha de caminhada de um NPC.
- **Auditar** folhas geradas antes de aceitar (o passo que mais economiza dinheiro — ver abaixo).
- Fazer o wiring da folha no NPC dentro do Unity.

## Quando NÃO usar
- Retratos de NPC (busto/expressões) → [[project-npc-portraits-pipeline]].
- Sprites de mundo/props/tiles → [[chatgpt-web-sprite-gen]].

## Formato canônico da folha (fixo)
Grade **5 colunas x 5 linhas = 25 poses**. Colunas = 5 frames do ciclo de caminhada. Linhas (topo→baixo): `walk_down`, `walk_downleft`, `walk_right`, `walk_up`, `walk_upleft`. As outras 3 direções (`left`, `downright`, `upright`) são **espelho (flipX)** em runtime — não gerar. Fundo 100% transparente. Staging cru: `art/npc_anim_gpt/raw/gpt_<id>_walk.png`.

## Regra de OURO: os 5 frames precisam ANIMAR (não é opcional)
O gpt-image-1 tende a gerar 5 quase-cópias da mesma pose (personagem "deslizando"). Isso é o erro mais comum e o mais fácil de deixar passar. **Cada linha DEVE ser um ciclo de passada real** com os pés/pernas mudando de posição a cada frame.

Prompt de geração (keyframes explícitos por coluna) — versão REFORÇADA (usar sempre; a versão fraca deixou "mesmas pernas"):
> "É ERRADO repetir a mesma pose de perna. Compare os 5 frames de cada linha: se as pernas estiverem na mesma posição em 2 frames, está ERRADO. Frame 1 = perna ESQUERDA bem à FRENTE (passo largo), pé direito atrás no chão, braço direito à frente. Frame 2 = pernas CRUZANDO no meio (passagem), corpo levemente elevado. Frame 3 = perna DIREITA bem à frente (passo largo OPOSTO), braço esquerdo à frente. Frame 4 = pernas cruzando (passagem oposta). Frame 5 = quase fechando o ciclo. Os PÉS têm que estar em posições visivelmente DIFERENTES em cada um dos 5 frames — é um ciclo de caminhada alternando pé esquerdo/direito, não uma pose parada repetida. Vale para TODAS as linhas, inclusive frente (walk_down) e costas (walk_up)."

**Ordem dos frames (consistência com o player):** o `PlayerWalkAnimator` toca os frames de cada direção em ordem (arquivos 01,02,03…) em loop. O `NpcWalkAnimator` toca as colunas 0→4 em loop. Logo a coluna 0 é o começo do ciclo e as colunas devem estar em ordem de passada (contato → passagem → contato oposto → passagem → contato). O prompt acima autora exatamente essa ordem. NÃO embaralhar colunas.

Sempre junto: "imagem aproximadamente QUADRADA, EXATAMENTE 5 colunas x 5 linhas = 25 poses, fundo 100% TRANSPARENTE sem glow/vinheta, pixel art NÍTIDO com outline (não aquarela/lápis), bastante espaço entre as células."

## Fidelidade: siga a ARTE-base, não o roster
O `NpcTownRosterRegistry` pode estar **stale** (ex.: Zrix e Renko marcados "Humano"/"Goblin" mas a arte é outra). Abra o sprite-base em `Assets/_Game/Resources/NpcSprites/<arquivo>.png`, descreva cor de pele/roupa/props exatamente como estão, e valide contra ele — não contra o label de raça.

## Auditoria de movimento (obrigatória antes de aprovar)
Não julgue pela silhueta/cor só. Monte tiras rotuladas das linhas `walk_down` (frente) e `walk_right` (perfil) de todas as folhas e olhe **se os pés mudam de posição entre os 5 frames**. Script pronto: `scratchpad/audit_montage.ps1` (recorta uma linha, amplia com NearestNeighbor, empilha rotulado). Padrão observado:
- **Perfil e diagonais** costumam animar bem (perna clara).
- **Frente/costas** são as fracas — pior em personagens de **manto** (perna escondida) e figuras escuras. Se a frente/costas ficar quase parada → **reprovar e regerar** com o prompt de keyframes.
Classifique em APROVADO / REGERAR e reporte ao humano antes de gastar novas gerações.

## Fragilidade do chat (Chrome MCP)
A conversa fica pesada e o render quebra ("Content failed to load", DOM com 0 mensagens). Sintomas e contornos:
- Envio pode não registrar; confirme que um turno novo apareceu.
- Download por `fetch` pode pegar a imagem ANTERIOR (a nova fica em placeholder branco). **Valide pelo CONTEÚDO** (abra o PNG), não pelo alt-text nem pelo tamanho do blob. Se vier o personagem errado, recarregue e re-baixe; se persistir, use chat novo com o batch reanexado.
- Digite o prompt em LINHA ÚNICA e envie com Enter.

## Fundo OPACO de xadrez (o blocker de wiring nº1 — SEMPRE checar)
O gpt-image-1 web entrega a folha como **Format24bppRgb SEM canal alpha**: o "xadrez de transparência" que aparece no visualizador é **pintado** (dois cinzas claros dessaturados, ~RGB 237 e 254), 100% opaco. Se você não remover, o slice gera **caixas opacas** ao redor de cada NPC. Fácil de deixar passar: o Read/preview mostra o xadrez e parece transparente.
- **Detecção:** amostre um pixel de fundo (`GetPixel`) — se `A==255` e RGB é cinza claro, é fundo opaco. Ou meça: 0% de pixels com alpha<16.
- **Remoção correta (preserva roupa branca):** máscara "light" = `min(R,G,B)>=215 && (max-min)<=16` (o gate de saturação poupa creme/branco de roupa, que tem croma) + **flood-fill BFS a partir das bordas** sobre essa máscara (só remove o claro conectado à borda; branco interno cercado por outline fica). NÃO use "claro = transparente" global (fura avental/manga/barba branca). Valide compondo sobre magenta (`scratchpad/clean_bg.py` + `contact_magenta.py`).
- **No pipeline:** o slicer `GenerateNpcWalkAnimations` já faz essa remoção em C# (Texture2D.LoadImage → máscara+BFS → EncodeToPNG) antes de fatiar, então folhas novas em `raw/` só precisam do drop + Inicializar Projeto. Sombras de contato cinza desenhadas pela IA sob os pés podem sobrar (leem como sombra; polir depois se incomodar).

## TAMANHO no jogo (blocker nº3 — o corte pode estar certo e o NPC ainda sair 2× maior)
Walk e base usam o MESMO PPU (234). Se o personagem na folha de walk for maior em px que no sprite base, o NPC **incha ao andar** (medido ~1.8-2×: base ~128px, walk cru ~228px). O normalizador `tools/npc_walk/normalize_walk_sheets.py` **escala cada personagem para casar a altura (mediana) com o sprite base** do NPC (`Resources/NpcSprites/<arquivo>.png`) → mesmo tamanho no jogo (≈ player). **Sempre valide TAMANHO por número** (altura_px/PPU → unidades de mundo), comparando player × base × walk — contact sheet com células de tamanho fixo NÃO revela erro de escala. Ref: `scratchpad/prove_scale.py`.

## Direção 8-vias e o NpcWanderer
`NpcWalkAnimator` é 8-direções (5 linhas geradas + 3 espelhos). O `NpcWanderer` move em ângulo ALEATÓRIO, então diagonais (ex.: cima-direita → costas-direita) são NORMAIS e corretas, não bug de folha. Se um NPC parece "andar de costas indo pra direita", cheque primeiro se o movimento é diagonal (esperado) antes de suspeitar da folha/mapa. Para feel mais cardinal: enviesar o wander ou snap 4-direções pelo eixo dominante.

## Cortes DESALINHADOS (o blocker nº2 — as folhas cruas NÃO são grade uniforme)
O gpt-image-1 não posiciona os 25 personagens numa grade 5×5 perfeita: cada um flutua fora de centro e às vezes a cabeça/apêndice (espada, trança, braço) cruza para a célula vizinha. Cortar numa grade uniforme 5×5 gera frames **descentralizados, com cabeça cortada e "tremidos"** na animação (o personagem pula de lado e de altura em vez de andar no lugar). Medível: os centros de coluna reais (ex.: ~150/386/626/867/1092) não batem com as linhas uniformes (125/376/627/878/1129), e faixas de linha chegam a cruzar a fronteira (cabeça da linha 5 sobe pra célula da linha 4).
- **Fix (normalização content-aware):** detecta as 5 faixas de linha e 5 colunas por projeção de ocupação (alpha), extrai o bbox de cada personagem, e **re-compõe numa grade de células iguais** com cada um centrado pelo **âncora dos pés** (centroide da faixa inferior) e apoiado numa **baseline fixa** → caminhada "no lugar", pivot BottomCenter correto, sem clip/bleed. Ferramenta: `tools/npc_walk/normalize_walk_sheets.py` (faz também a remoção de fundo). Saída: `art/npc_anim_gpt/normalized/`. Valide com overlay de grade + tira de 5 frames sobre magenta (linha de baseline nos pés).
- O slicer `GenerateNpcWalkAnimations` **prefere `normalized/`** (cai para `raw/` se ausente).

## Wiring no Unity (já implementado — DRY)
- Runtime: `Assets/_Game/Scripts/NPC/NpcWalkAnimator.cs` (MonoBehaviour fino; reusa o bucket de 8 direções do `PlayerWalkAnimator`; lê `Rigidbody2D.linearVelocity`; espelha as 3 direções não geradas; frame 0 parado).
- Editor slicer/import: `Assets/_Game/Scripts/Editor/NPC/GenerateNpcWalkAnimations.cs` — copia `art/npc_anim_gpt/raw/*` p/ `Resources/NpcWalkSprites/`, importa como Multiple 5x5, popula `NpcDataSO.WalkAnimResourcesPath`. **Registrado como RunStep em `CindarsHopeMenu.InicializarProjeto` (sem `[MenuItem]` avulso — ver rule editor-generation-orchestration).**
- Validador: `ValidateNpcWalkAnimations.cs` (RunStep em ValidarProjeto).
- **Adicionar um NPC novo** = (1) soltar `gpt_<id>_walk.png` em `art/npc_anim_gpt/raw/`; (2) rodar `py tools/npc_walk/normalize_walk_sheets.py <id>` (remove fundo + normaliza cortes → `normalized/`); (3) mapear short-name→npcId em `GenerateNpcWalkAnimations.ShortNameToNpcId`; (4) rodar `CindarsHope/Inicializar Projeto`. Zero código novo por NPC. (Follow-up possível: portar a normalização pra C# no slicer p/ virar 1-clique de novo, sem o passo Python.)

## Validação de FIDELIDADE ao base (OBRIGATÓRIA — o erro mais comum é pular isto)
Não basta validar movimento e formato: a folha TEM que casar com a sprite-base ORIGINAL do NPC (`Assets/_Game/Resources/NpcSprites/<arquivo>.png`). Validar por descrição própria ou por memória NÃO conta — compare pixel a pixel, lado a lado.

Método (use sempre): gere uma montagem **base × frame1 gerado** com `scratchpad/compare_base.ps1` (base upscaled ao lado do canto superior-esquerdo da folha) e confira item por item:
- **Cor da pele** (ex.: Maelor é azul-acinzentado; Mara pele morena quente).
- **Cor de CADA peça de roupa** (ex.: Liora capuz **ROXO/lavanda** NÃO teal; Mara casaco **TEAL** NÃO azul-cinza).
- **Props e acessórios exatos** (livro, harpa, cajado, martelo) e **o que NÃO existe** (ex.: Maelor NÃO tem capa — não invente uma).
- **Cabelo** (cor, formato, coque/tranças) e **proporção** (~4 cabeças chibi). Valide o **PENTEADO/silhueta**, não só a cor: se a sprite-base tem uma **trança** e a folha de walk sai com cabelo **solto**, fica estranho na transição idle→walk (falha real da Sylveth) — o penteado tem que casar com o base e ser consistente em todas as direções.
Só marque PASS se pele + todas as cores de roupa + props + cabelo baterem com o base. Se qualquer um divergir, anote a cor/detalhe exato do base e regere com essa correção explícita no prompt.

> Lição registrada: gerar em chat SEM o batch anexado faz o modelo inventar cor/proporção; e descrições imprecisas (ex.: "azul-acinzentado" p/ um casaco teal) fazem drift. Sempre ler o base, ditar as cores EXATAS, e comparar no fim.

## Verificação (não confie na narração)
- Folha aprovada = auditoria de movimento passou + formato 5x5 + transparente + **fidelidade ao base validada lado-a-lado** (seção acima).
- Assets materializados exigem rodar Inicializar Projeto no Editor (batchmode/GUI) — código compilar não prova o slice. Confira contagem de 25 sub-sprites por folha e um NPC andando em Play Mode (inclui as direções espelhadas).
- Build: `dotnet build Assembly-CSharp.csproj` e `...Editor.csproj`, exit 0 conferido por você.

## Relacionados
- [[chatgpt-web-sprite-gen]] — Chrome MCP + rate limit.
- `project-npc-walk-animation-pipeline` (memória) — estado da fila e lições.
- [[runtime-bootstrap-pattern]] / rule `unity-architecture` — sem FindObjectOfType no runtime.
- rule `editor-generation-orchestration` — os 3 comandos canônicos; sem MenuItem avulso.
