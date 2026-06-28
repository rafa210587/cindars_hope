# Play Mode Human Test — Visibilidade de colisões + NPC em casa

> Pré-requisito: regenerar a TownScene pelo menu `CindarsHope/...` (roda `CreateMvpTownScene`)
> ANTES de testar, senão a cena em disco ainda é a antiga.

## 1. Bordas e paredes visíveis
- [ ] Andar até cada borda da cidade (N/S/L/O): existe uma faixa de **parede cinza-pedra** visível, e o jogador é bloqueado por ela (não atravessa).
- [ ] Não há mais "parede invisível" — onde trava, há visual.

## 2. Entrar na casa não "vai pra longe"
- [ ] Aproximar de uma porta de casa, apertar E.
- [ ] A câmera **corta direto** para o interior (não faz um pan voando pelo mapa).
- [ ] Dentro: as **4 paredes do quarto são visíveis** (cor terrosa) e bloqueiam.
- [ ] Apertar E na porta de saída ("Sair"): corta de volta para o exterior, na frente da casa.

## 3. NPC acessível em casa só no bloco "home"
- [ ] De dia: o NPC está na rua (posto de trabalho) e é interagível normalmente.
- [ ] Avançar o tempo até a noite (bloco "home" do NPC).
- [ ] Entrar na casa mais próxima do posto desse NPC: o **NPC está dentro do interior** e é alcançável.
- [ ] Não houve NPC "deslizando" pelo mapa em direção ao topo (y>40) — ele apareceu direto no interior.

## 4. Loja fechada à noite (regra preservada)
- [ ] À noite, a porta da casa do vendedor mostra o prompt com aviso "loja fechada", mas **permite entrar**.
- [ ] Falar com o vendedor à noite: a **venda é recusada** com feedback de horário (comércio continua travado).
- [ ] De dia: vender/comprar funciona normalmente.

## Resultado
- Data: ___  | Build regenerada: SIM/NÃO  | Validator fable_11: PASS/FAIL
- Observações:
