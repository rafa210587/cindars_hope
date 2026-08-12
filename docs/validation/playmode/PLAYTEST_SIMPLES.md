# Playtest — lista direta (o que VOCÊ faz e o que tem que ver)

> Objetivo: rodar o jogo e confirmar que os fluxos essenciais funcionam. Se algum passo falhar ou
> aparecer **erro vermelho no Console**, anote o passo + o erro. Cada passo tem um ✅ (o que esperar).
> Comece dando **Play** a partir da cena inicial (Farm).

## 1. Fazenda (FarmScene)
- Andar com o player. ✅ move sem travar, sem erro no Console.
- Plantar → regar → dormir/avançar o dia → colher. ✅ a planta cresce e dá pra colher.

## 2. Ir pra Cidade (transição de cena) — IMPORTANTE
- Da fazenda, **ir pra Cidade** pelo caminho normal (não recarregar a Town direto). ✅ a Town carrega, player aparece na entrada.

## 3. Loja do Pip (o bug que corrigimos)
- Falar com o **Pip** (NPC da entrada da cidade) e **abrir a loja**. ✅ o menu abre, **sem** erro `_playerManager ... null` no Console.
- **Comprar** um item e **vender** um item. ✅ item entra/sai do inventário, o ouro atualiza.
- Com a loja aberta, ver se o **tempo pausa** (o relógio não anda). ✅ pausado.

## 4. Inventário
- Abrir o inventário, mover/empilhar item, fechar. ✅ abre, mexe, fecha sem travar o controle.

## 5. Crafting
- Abrir uma estação de crafting, craftar um item que você tem os ingredientes. ✅ recebe o item.

## 6. Conversar com NPC
- Iniciar diálogo com um NPC, passar as falas, fechar. ✅ diálogo abre e fecha normal.

## 7. Quest
- Pegar uma quest com um NPC → cumprir o objetivo → voltar e entregar. ✅ a recompensa chega.

## 8. Combate (Caverna)
- Entrar na **Caverna**, atacar um inimigo. ✅ o inimigo toma dano, morre e dropa loot.
- (bônus) Ver se algum ataque de inimigo aplica **status** (queimadura/lentidão etc.). ✅ o status aparece.

## 9. Caverna — mesmo nível de novo
- Sair e voltar pro **mesmo nível** da caverna. ✅ o nível é o mesmo (inimigos/recursos não trocam de lugar).

## 10. Morte / respawn
- Morrer (deixar a vida zerar). ✅ respawna no ponto esperado, sem travar.

## 11. Salvar / Carregar
- Salvar → fechar/recarregar → carregar. ✅ o estado volta como estava (ouro, inventário, dia, progresso).

---

## Como me reportar
Pra cada número, só me diga **OK** ou **FALHOU: <o que aconteceu + erro do Console se tiver>**.
Ex.: "1 OK, 2 OK, 3 FALHOU: menu do Pip não abriu, erro X no console, 4 OK...".

Com isso eu fecho (dou baixa) todas as specs que dependiam só desse teste — são ~29 esperando exatamente este smoke.
