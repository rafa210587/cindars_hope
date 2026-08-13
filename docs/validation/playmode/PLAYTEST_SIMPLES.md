# Playtest humano — roteiro operacional

> Objetivo: validar os 11 fluxos que liberam o fechamento das specs `PLAYTEST_ONLY`.
> Não altere código, cenas ou assets durante o teste. Em cada falha, copie o erro vermelho completo
> do Console e informe em qual passo ocorreu.

## Setup obrigatório — preparar o kit

1. Abra `FarmScene`, limpe o Console e entre em Play Mode.
2. Aguarde o player e a HUD aparecerem.
3. No menu superior do Unity, execute
   `CindarsHope > Integration > Debug > Provision Farm Smoke Loadout` uma única vez.
4. No Console, confirme a linha `READY FOR PLAYTEST`. Se aparecer `REQUIRED FAILURES (N)`, pare o
   teste e reporte a linha inteira; o kit não está pronto.
5. Confirme a hotbar: `1` enxada, `2` regador, `3` picareta de ferro, `4` vara de pesca, `5` arco e
   `6` sementes de cenoura. O provisionador deixa a enxada equipada.

Controles usados neste roteiro: `WASD/setas` movem; `E` interage/confirma; `Esc` fecha; `I` abre o
inventário; `L` abre equipamento; `C` abre crafting de bolso; `J` abre quests; `1..6` selecionam a
hotbar; `F` usa ferramenta de fazenda; `Q/E` atacam; `Tab` avança o dia (atalho de debug); `F5` salva
e `F9` carrega. Em menus, use `W/S` para navegar e `E/Enter/Espaço` para confirmar.

## 1. Fazenda — plantar, regar, crescer e colher

- **Pré-condição:** setup concluído com `READY FOR PLAYTEST`; stamina suficiente; player na área cultivável.
- **Item/slot:** enxada `1` já equipada; regador `2`; sementes de cenoura `6`.
- **Ação exata:** pressione `F` num tile válido para arar. Interaja com o plot usando `E`, escolha
  `Plantar cenoura` com `W/S` e confirme com `E`. Abra `L`, selecione o regador e equipe-o na mão
  esquerda; volte ao plot e pressione `F` para regar. Durma na cama com `E` ou use `Tab` até a cultura
  ficar pronta; interaja com `E` e escolha colher.
- **Esperado:** solo muda de estado a cada ação, uma semente é consumida, a cultura cresce e a cenoura
  colhida entra no inventário sem erro vermelho.
- **Se falhar:** anote qual ação falhou, item equipado, mensagem da HUD e erro completo do Console.
- **Resultado:** [ ] OK [ ] FALHOU

## 2. Fazenda → Cidade

- **Pré-condição:** player livre, sem modal aberto, ainda na FarmScene.
- **Item/slot:** nenhum.
- **Ação exata:** caminhe pela saída normal da fazenda e use `E` no portal/saída para a cidade; não
  abra Town diretamente pelo Editor.
- **Esperado:** Town carrega e o player surge na entrada correta, com inventário e hotbar preservados.
- **Se falhar:** registre posição/saída usada, cena observada e Console.
- **Resultado:** [ ] OK [ ] FALHOU

## 3. Loja do Pip

- **Pré-condição:** Town carregada pelo passo 2; Pip localizado na entrada.
- **Item/slot:** cenoura, peixe ou material provisionado para venda; ouro para uma compra barata.
- **Ação exata:** aproxime-se de Pip, pressione `E`, avance o diálogo e abra a loja. Com `W/S`, escolha
  um item e confirme a compra com `E/Enter`; mude para venda conforme a opção exibida e venda um item.
  Deixe a loja aberta por alguns segundos e observe o relógio; feche com `Esc`.
- **Esperado:** menu abre sem `_playerManager ... null`; item e ouro atualizam uma única vez; relógio
  permanece pausado enquanto a loja está aberta.
- **Se falhar:** informe se foi abertura, compra, venda ou pausa e copie o erro do Console.
- **Resultado:** [ ] OK [ ] FALHOU

## 4. Inventário

- **Pré-condição:** nenhum outro modal aberto.
- **Item/slot:** uma pilha de sementes de cenoura provisionada (x20) e pelo menos um slot vazio.
- **Ação exata:** pressione `I`, selecione a pilha de sementes e confirme com `E/Enter`. Escolha
  `Split` para criar duas pilhas. Selecione uma delas, confirme e escolha `Mover/Mesclar`; a tela pede
  o destino. Selecione a outra pilha com `WASD`/setas ou clique nela e confirme com `E/Enter`.
  Confirme que volta a existir uma única pilha com a quantidade total. Repita escolhendo um slot vazio
  para mover uma pilha. Pressione `Esc` durante a seleção de destino para confirmar que cancela sem
  alterar os itens; feche o inventário com `Esc` e volte a andar.
- **Esperado:** o inventário abre, representa quantidades reais, oferece `Mover/Mesclar`, mescla ou
  move sem perder/duplicar itens e devolve o controle ao player sem movê-lo por trás do modal.
- **Se falhar:** registre a operação, slots/quantidades antes e depois e Console.
- **Resultado:** [ ] OK [ ] FALHOU

## 5. Crafting

- **Pré-condição:** madeira, pedra, cobre e ferro provisionados; nenhum modal aberto.
- **Item/slot:** materiais ficam fora da hotbar.
- **Ação exata:** pressione `C` para crafting de bolso ou use `E` numa estação. Navegue com `W/S`,
  escolha uma receita habilitada pelos materiais e confirme com `E/Enter/Espaço`; feche com `Esc`.
- **Esperado:** ingredientes são consumidos uma vez e o resultado entra no inventário.
- **Se falhar:** anote receita, ingredientes/quantidades exibidos e erro do Console.
- **Resultado:** [ ] OK [ ] FALHOU

## 6. Diálogo com NPC

- **Pré-condição:** NPC acessível e nenhum modal aberto.
- **Item/slot:** nenhum.
- **Ação exata:** aproxime-se de um NPC, pressione `E`, avance falas com `E/Enter/Espaço`, use `W/S`
  se houver escolha e encerre com a opção final ou `Esc` quando permitido.
- **Esperado:** diálogo abre, bloqueia movimento, avança sem repetir indevidamente e devolve o controle.
- **Se falhar:** informe NPC, fala/opção em que travou e Console.
- **Resultado:** [ ] OK [ ] FALHOU

## 7. Quest

- **Pré-condição:** NPC com marcador/oferta de quest disponível.
- **Item/slot:** itens pedidos pela quest, se aplicável; `J` abre o registro.
- **Ação exata:** fale com o NPC usando `E`, aceite a quest, abra `J` e anote o objetivo. Cumpra o
  objetivo indicado, volte ao NPC e confirme a entrega.
- **Esperado:** quest muda de oferecida para ativa e concluída; progresso aparece em `J`; recompensa
  é concedida uma única vez.
- **Se falhar:** registre NPC, ID/título da quest, objetivo e estado mostrado no registro.
- **Resultado:** [ ] OK [ ] FALHOU

## 8. Combate na Caverna

- **Pré-condição:** entre na caverna pela transição normal; encontre um inimigo em área segura.
- **Item/slot:** arco `5` + flechas fora da hotbar; equipe ambos em mãos compatíveis pelo painel `L`.
- **Ação exata:** aproxime-se mantendo distância e ataque com `Q` ou `E` conforme a mão equipada até
  derrotar o inimigo; recolha o drop com `E`. Opcionalmente receba um ataque que aplique status.
- **Esperado:** ataque consome flecha quando aplicável, inimigo recebe dano, morre e concede loot/XP
  uma vez; status, se aplicado, aparece e expira sem erro.
- **Se falhar:** anote equipamento/mãos, tecla, HP observado, consumo de flecha e Console.
- **Resultado:** [ ] OK [ ] FALHOU

## 9. Caverna — revisitar o mesmo nível

- **Pré-condição:** memorize ou capture uma imagem do layout, inimigos derrotados e recursos do nível.
- **Item/slot:** nenhum.
- **Ação exata:** saia pelo retorno do nível e volte pelo mesmo acesso sem iniciar nova run e sem usar
  o comando debug de reroll (`Shift+R`).
- **Esperado:** layout é idêntico e inimigos/recursos já consumidos não reaparecem nem mudam de lugar.
- **Se falhar:** compare imagens antes/depois e registre seed/andar e Console.
- **Resultado:** [ ] OK [ ] FALHOU

## 10. Morte e respawn

- **Pré-condição:** save feito com `F5`; inimigo acessível; não use item de cura durante este passo.
- **Item/slot:** poção provisionada fica disponível apenas para confirmar que não foi usada.
- **Ação exata:** deixe um inimigo reduzir o HP a zero; na tela de morte, navegue com setas/WASD e
  confirme a opção de respawn com `Enter`.
- **Esperado:** tela de morte aparece e o player respawna no ponto previsto, com controle restaurado e
  sem duplicação de itens/penalidade.
- **Se falhar:** registre opção escolhida, local antes/depois, inventário afetado e Console.
- **Resultado:** [ ] OK [ ] FALHOU

## 11. Salvar e carregar

- **Pré-condição:** faça uma mudança observável em ouro, inventário, dia ou quest.
- **Item/slot:** qualquer item cuja quantidade possa ser conferida.
- **Ação exata:** pressione `F5`, aguarde a confirmação, altere algo no estado e pressione `F9`; depois
  pare e reinicie o Play Mode, carregando o mesmo save pelo fluxo disponível.
- **Esperado:** volta o estado salvo de ouro, inventário/hotbar/equipamento, dia e progresso; nenhuma
  referência some e não há erro vermelho.
- **Se falhar:** informe campos divergentes, momento do save/load e Console.
- **Resultado:** [ ] OK [ ] FALHOU

## Como reportar

Envie uma linha por número: `1 OK` ou `1 FALHOU: <ação, observado, esperado e erro do Console>`.
Inclua também a linha final do provisionador (`READY FOR PLAYTEST` ou `REQUIRED FAILURES`) e qualquer
warning recorrente. Play Mode só será considerado validado depois desse retorno humano.
