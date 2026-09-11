# Contrato e wiring de caminhada NPC

## Layout específico do consumidor
O contrato atual é 5 colunas × 5 linhas: topo→baixo `walk_down`, `walk_downleft`,
`walk_right`, `walk_up`, `walk_upleft`. Colunas avançam em loop; `left`, `downright`
e `upright` usam espelho. Essa contagem não é regra geral de animação.
Confirme o contrato em `Assets/_Game/Scripts/NPC/NpcWalkAnimator.cs` antes de alterar dados.

Prompt: descreva contatos opostos, passagem e fechamento do ciclo na ordem do consumidor;
exija identidade constante e margem entre células. Não imponha que todo frame tenha pés únicos.
Movimento diagonal pode explicar costas-direita: compare vetor real, bucket e flip antes de culpar a folha.

## Normalização existente
- Staging: `art/npc_anim_gpt/raw/gpt_<id>_walk.png`.
- Ferramenta existente: `tools/npc_walk/normalize_walk_sheets.py`; leia argumentos/dependências
  antes de executar. Saída em `art/npc_anim_gpt/normalized/`.
- A ferramenta remove claro dessaturado conectado às bordas, detecta faixas por ocupação,
  recompõe células pela âncora dos pés e ajusta altura à base. Inspecione o resultado:
  alpha, roupa clara, apêndices, baseline e cortes podem exigir correção no lote.
- O método de fundo desse pipeline não é garantia universal; xadrez pode estar pintado,
  e remover claro globalmente fura roupa/barba. Use medição de alpha e fundo contrastante.
- Compare altura ocupada/PPU × escala efetiva entre base, walk e player. Contact sheets
  em células iguais escondem diferenças físicas. PPU atual do importador: 234; verificar no alvo.

## Wiring existente
- Editor: `Assets/_Game/Scripts/Editor/NPC/GenerateNpcWalkAnimations.cs`.
- O importador escolhe a pasta normalized se ela existir; fallback raw ocorre por pasta,
  não por arquivo. Confirme que todas as folhas esperadas estão na fonte escolhida.
- Mapeamento `ShortNameToNpcId`, saída Resources/NpcWalkSprites e campo
  `NpcDataSO.WalkAnimResourcesPath`; verificar cadastro existente antes de editar.
- Validador: `Assets/_Game/Scripts/Editor/NPC/ValidateNpcWalkAnimations.cs`.
- Executar pelos comandos canônicos Inicializar Projeto/Validar Projeto conforme escopo;
  sem MenuItem avulso ou edição manual de YAML. Reusar generators, não criar runtime por NPC.
- Após materializar, conferir sub-sprites, nomes, ordem, pivot e NPC andando, parando e
  trocando direção. Folha válida no disco não prova wiring em cena.

Se houver só review, leia esses contratos sem executar normalização/import.
Para scale/collider/câmera, consulte [sprite-scene-integration](../../sprite-scene-integration/SKILL.md).
