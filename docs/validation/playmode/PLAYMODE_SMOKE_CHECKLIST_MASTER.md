# Playmode Smoke Checklist Master

> **Spec de origem:** `spec_validation_human_playmode_smoke_v1`
> **Status:** Ativo — checklist guarda-chuva
> **Tipo:** Rede de validação humana final (não automatizada)

## Papel deste documento (declaração de guarda-chuva)

Este é o checklist humano ÚNICO e consolidado de smoke de Play Mode do projeto. Ele cobre os fluxos
essenciais transversais do jogo — não substitui nem invalida os cenários individuais existentes em
`docs/validation/playmode/` (um por spec, gerados pela skill `gameplay-test-scenario`), que continuam
válidos para o escopo local de cada mudança.

**Nenhuma spec de arquitetura sensível que mexa em UI/cena/runtime visual (ex.:
`spec_arch_cave_integration_boundary_residual_v1`) deve ser marcada como implementada/aceita sem citar
este smoke (ou um smoke equivalente/mais específico) como evidência final**, quando aplicável ao escopo
da mudança. Este checklist é a REDE final de lote/wave (ver `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`) —
não é para ser rodado a cada micro-spec individual; é executado pelo humano no fim do lote/wave, por
decisão do usuário.

Este documento **não é uma execução**. Um checklist em branco (sem relatório preenchido) **não conta
como smoke PASS** — ver rule `validation-truth`: sem claim sem evidência. A execução real gera um
relatório separado (ver seção "Relatório de execução" abaixo).

## Como usar

1. Rode o jogo em Play Mode (Unity Editor) ou no build mais recente, a partir do commit/estado que se
   quer validar.
2. Siga cada fluxo, na ordem. Cada passo tem UMA condição de PASS observável — não avalie "parece
   funcionar", avalie exatamente a condição escrita.
3. Preencha o relatório usando o template da seção "Relatório de execução", copiando-o para
   `docs/validation/playmode/PLAYMODE_SMOKE_REPORT_<YYYY-MM-DD>.md`.
4. Qualquer console `ERROR`/exception não listado como esperado em um passo é FAIL desse passo, mesmo
   que o comportamento visual pareça correto.

---

## Fluxo 1 — TownScene

**Cena:** `Assets/_Game/Scenes/TownScene.unity` (ou path canônico equivalente da build atual)

| # | Passo | Resultado esperado |
|---|---|---|
| 1.1 | Carregar a TownScene (boot direto ou via menu). | Cena carrega sem exception no Console; player spawna em posição válida (não fora do mapa). |
| 1.2 | Mover o player nas 4 direções (WASD/setas) por alguns segundos. | Player se move suavemente, animação de walk toca, sem stutter nem teleporte. |
| 1.3 | Colidir com um NPC (encostar nele). | Player é bloqueado pelo collider do NPC; não atravessa. |
| 1.4 | Colidir com um prédio (parede/fachada). | Player é bloqueado; não atravessa parede. |
| 1.5 | Observar o Console durante os passos 1.1–1.4. | Nenhum `ERROR` ou exception no Console. |

## Fluxo 2 — FarmScene

**Cena:** `Assets/_Game/Scenes/FarmScene.unity` (ou path canônico equivalente)

| # | Passo | Resultado esperado |
|---|---|---|
| 2.1 | Carregar a FarmScene. | Cena carrega sem exception; player spawna em posição válida. |
| 2.2 | Plantar uma semente em um tile de solo lavrado/válido. | Sprite de planta aparece no tile; item de semente é consumido do inventário. |
| 2.3 | Regar o tile plantado. | Estado visual de "regado" aplicado ao tile (tile mais escuro/molhado ou ícone equivalente). |
| 2.4 | Avançar o dia (dormir ou comando de debug de avanço de tempo). | Dia avança no calendário/HUD; crop progride de estágio (se elegível) sem erro. |
| 2.5 | Colher a planta quando madura. | Item colhido entra no inventário; tile volta a estado vazio/lavrado. |
| 2.6 | Observar o Console durante os passos 2.1–2.5. | Nenhum `ERROR` ou exception no Console. |

## Fluxo 3 — CaveScene

**Cena:** `Assets/_Game/Scenes/CaveScene.unity` (ou path canônico equivalente)

> Cita a rule `cave-stable-run` — dentro do mesmo `CaveRunSeed`, revisitar o mesmo `CaveLevel` não pode
> dar reroll de layout/entrance/exit/enemy/resource node composição.

| # | Passo | Resultado esperado |
|---|---|---|
| 3.1 | Entrar na cave a partir da entrada canônica. | CaveScene carrega; `CaveLevel` 1 materializa layout, inimigos e resource nodes sem erro. |
| 3.2 | Anotar (visualmente ou via debug log) layout, posições de inimigos e de resource nodes do nível atual. | Estado inicial documentável (mentalmente ou print de debug) para comparação no passo 3.4. |
| 3.3 | Avançar para o próximo nível e depois retornar ao nível anterior pelo mesmo `CaveRunSeed` (sem morrer/new game). | Retorno ao nível anterior não abre uma tela de loading infinita nem crasha. |
| 3.4 | Comparar o nível revisitado com a anotação do passo 3.2. | Layout, entrance/exit, composição/posições de inimigos e resource nodes **idênticos** ao primeiro acesso — nenhum reroll (invariante `cave-stable-run`). |
| 3.5 | Observar o Console durante os passos 3.1–3.4. | Nenhum `ERROR` ou exception no Console. |

## Fluxo 4 — Inventário

| # | Passo | Resultado esperado |
|---|---|---|
| 4.1 | Abrir a UI de inventário (tecla/botão canônico). | Painel de inventário abre; ModalManager registra o modal (input de movimento do player bloqueado). |
| 4.2 | Mover um item de um slot para outro slot vazio (drag ou equivalente). | Item aparece no novo slot; slot de origem fica vazio. |
| 4.3 | Empilhar dois itens do mesmo tipo/stack no mesmo slot (se aplicável ao item testado). | Quantidade do stack soma corretamente; nenhum item duplicado ou perdido. |
| 4.4 | Fechar o inventário (tecla/botão ou ESC). | Painel fecha; input de movimento do player volta a funcionar imediatamente (sem travar). |
| 4.5 | Observar o Console durante os passos 4.1–4.4. | Nenhum `ERROR` ou exception no Console. |

## Fluxo 5 — Crafting

| # | Passo | Resultado esperado |
|---|---|---|
| 5.1 | Abrir uma estação de crafting com o player tendo os ingredientes necessários no inventário. | UI de crafting abre; recipe elegível aparece disponível/craftável. |
| 5.2 | Craftar o item (confirmar a receita). | Ingredientes são consumidos do inventário nas quantidades corretas. |
| 5.3 | Verificar o inventário após o craft. | Item craftado aparece no inventário na quantidade esperada. |
| 5.4 | Observar o Console durante os passos 5.1–5.3. | Nenhum `ERROR` ou exception no Console. |

## Fluxo 6 — Loja (shop de NPC)

| # | Passo | Resultado esperado |
|---|---|---|
| 6.1 | Falar com um NPC que oferece shop e abrir o menu de loja. | Menu de shop abre, listando itens disponíveis e o saldo de ouro atual do player. |
| 6.2 | Comprar um item (com ouro suficiente). | Item entra no inventário; saldo de ouro do player diminui pelo preço do item. |
| 6.3 | Vender um item do inventário. | Item sai do inventário; saldo de ouro do player aumenta pelo valor de venda. |
| 6.4 | Observar o Console durante os passos 6.1–6.3. | Nenhum `ERROR` ou exception no Console. |

### 6b — Loja do Pip via transição de cena (caso de regressão — OBRIGATÓRIO)

> Regressão histórica: bug corrigido em `eaf42f1c` (guarda de duplicata no `DomainManagerRegistry`).
> Sem a transição de cena, a duplicata de `GameBootstrap` **não** é destruída e o teste dá falso-PASS —
> testar direto na TownScene (boot na Town) **não reproduz** o cenário do bug.

| # | Passo | Resultado esperado — condição de PASS ÚNICA |
|---|---|---|
| 6b.1 | Iniciar o jogo na **FarmScene** (não na TownScene). | FarmScene carrega normalmente, sem erro. |
| 6b.2 | A partir da FarmScene, **transicionar para a TownScene** pelo caminho normal de transição de cena do jogo (não recarregar a Town como cena inicial). | TownScene carrega após a transição; player spawna no ponto de entrada da Town. |
| 6b.3 | Andar até **`NPC_Pip_TownEntrance`** e iniciar diálogo/interação de loja com ele. | Diálogo/prompt de interação do Pip aparece normalmente. |
| 6b.4 | Abrir a loja do Pip. | Menu de shop do Pip abre. **Console NÃO contém** a string `field '_playerManager' - required reference is null`. |
| 6b.5 | Comprar um item na loja do Pip. | Compra funciona: item entra no inventário, ouro é debitado. Nenhuma exception de referência nula no Console. |
| 6b.6 | Vender um item na loja do Pip. | Venda funciona: item sai do inventário, ouro é creditado. Nenhuma exception de referência nula no Console. |
| 6b.7 | Com o modal da loja do Pip ainda aberto, observar se o tempo de jogo (relógio/calendário) avança. | Tempo de jogo **pausa** enquanto o modal está aberto (ModalManager deve suspender o `IGameClock`/time scale) — o relógio do HUD não avança durante o teste. |
| 6b.8 | Fechar a loja do Pip. | Modal fecha; tempo de jogo volta a avançar normalmente; input de movimento do player volta a funcionar. |

**Condição de PASS explícita do 6b (não ambígua):** todos os sub-passos 6b.1–6b.8 PASS **E** a string
literal `field '_playerManager' - required reference is null` está ausente do Console em toda a
sequência 6b.1–6b.8. A presença dessa string em qualquer ponto é FAIL do fluxo 6b inteiro, mesmo que a
compra/venda pareça ter funcionado visualmente.

## Fluxo 7 — Conversa com NPC

| # | Passo | Resultado esperado |
|---|---|---|
| 7.1 | Aproximar-se de um NPC e iniciar diálogo (interação canônica). | Caixa de diálogo abre com o nome/portrait do NPC e a primeira linha de texto. |
| 7.2 | Avançar as linhas de diálogo até o fim (input de avanço). | Cada avanço mostra a próxima linha; nenhuma linha vazia/travada; diálogo chega ao fim naturalmente. |
| 7.3 | Fechar o diálogo (avançar na última linha ou ESC). | Caixa de diálogo fecha; input de movimento do player volta a funcionar. |
| 7.4 | Observar o Console durante os passos 7.1–7.3. | Nenhum `ERROR` ou exception no Console. |

## Fluxo 8 — Quest offer/turn-in

| # | Passo | Resultado esperado |
|---|---|---|
| 8.1 | Falar com um NPC que oferece uma quest disponível. | Prompt/diálogo de oferta de quest aparece; opção de aceitar está disponível. |
| 8.2 | Aceitar a quest. | Quest aparece como ativa no log/tracker de quests do player. |
| 8.3 | Completar o objetivo da quest (ação especificada pela quest testada). | Progresso do objetivo é registrado (quest tracker reflete o progresso ou marca como completável). |
| 8.4 | Retornar ao NPC e entregar a quest (turn-in). | Prompt de entrega/recompensa aparece; ao confirmar, quest sai do estado "ativa" e vai para "completa"; recompensa (item/ouro/XP) é aplicada ao player. |
| 8.5 | Observar o Console durante os passos 8.1–8.4. | Nenhum `ERROR` ou exception no Console. |

## Fluxo 9 — Combate básico

| # | Passo | Resultado esperado |
|---|---|---|
| 9.1 | Localizar um inimigo (na cave ou zona de combate) e atacar com a arma equipada. | Animação de ataque toca; inimigo recebe dano visível (barra de vida reduz ou floating damage text aparece). |
| 9.2 | Deixar o inimigo atacar o player (ou provocar o ataque). | Player recebe dano visível (barra de vida do player reduz ou feedback visual/sonoro de hit). |
| 9.3 | Continuar atacando até o inimigo morrer. | Inimigo é removido da cena (morte); loot dropa no chão (item físico ou popup de loot). |
| 9.4 | Observar o Console durante os passos 9.1–9.3. | Nenhum `ERROR` ou exception no Console. |

## Fluxo 10 — Morte/respawn

| # | Passo | Resultado esperado |
|---|---|---|
| 10.1 | Levar o player a 0 de vida (combate ou comando de debug). | Sequência/feedback de morte do player dispara (animação, tela de KO, ou equivalente). |
| 10.2 | Aguardar/confirmar o respawn. | Player respawna no ponto de respawn esperado (documentado pela skill `fail-state-recovery-design` ou pelo game rule vigente), com vida restaurada conforme a regra atual. |
| 10.3 | Mover o player e interagir com algo após o respawn. | Player responde a input normalmente; nenhum softlock (input travado, cena não responsiva). |
| 10.4 | Observar o Console durante os passos 10.1–10.3. | Nenhum `ERROR` ou exception no Console. |

## Fluxo 11 — Save/load

| # | Passo | Resultado esperado |
|---|---|---|
| 11.1 | Alterar um estado observável do jogo (ex.: posição, item no inventário, ouro, quest ativa, crop plantado). | Estado alterado é visível na UI/cena antes de salvar. |
| 11.2 | Salvar o jogo (comando/menu de save canônico). | Save completa sem erro; confirmação visual ou log de sucesso (se existir). |
| 11.3 | Fechar e reabrir o jogo, ou recarregar o save (comando de load canônico). | Jogo recarrega sem exception. |
| 11.4 | Verificar o estado alterado no passo 11.1 após o load. | Estado restaurado corretamente e idêntico ao salvo (posição, item, ouro, quest, crop — conforme o que foi alterado). |
| 11.5 | Observar o Console durante os passos 11.1–11.4. | Nenhum `ERROR` ou exception no Console. |

---

## Relatório de execução

**Path do relatório (obrigatório ao executar este checklist):**

```text
docs/validation/playmode/PLAYMODE_SMOKE_REPORT_<YYYY-MM-DD>.md
```

Onde `<YYYY-MM-DD>` é a data de execução (ex.: `PLAYMODE_SMOKE_REPORT_2026-08-12.md`). Um relatório em
branco, ausente, ou um checklist marcado como "rodado" sem este arquivo preenchido **não conta como
smoke PASS** (rule `validation-truth`: exit code/evidência explícita, nunca claim sem prova).

### Template de preenchimento (copiar para o arquivo do relatório)

```markdown
# Playmode Smoke Report — <YYYY-MM-DD>

**Build/commit testado:** <hash do commit ou identificação do build>
**Data de execução:** <YYYY-MM-DD>
**Tester:** <nome/handle> (humano — não automatizado)
**Plataforma:** Windows / <outra>

## Resultado por fluxo

| Fluxo | Resultado | Observação |
|---|---|---|
| 1. TownScene | PASS / FAIL | |
| 2. FarmScene | PASS / FAIL | |
| 3. CaveScene (stable run) | PASS / FAIL | |
| 4. Inventário | PASS / FAIL | |
| 5. Crafting | PASS / FAIL | |
| 6. Loja | PASS / FAIL | |
| 6b. Loja do Pip via transição de cena | PASS / FAIL | (obrigatório citar se a string `_playerManager` apareceu ou não) |
| 7. Conversa com NPC | PASS / FAIL | |
| 8. Quest offer/turn-in | PASS / FAIL | |
| 9. Combate básico | PASS / FAIL | |
| 10. Morte/respawn | PASS / FAIL | |
| 11. Save/load | PASS / FAIL | |

## Resultado por passo (detalhado, quando houver FAIL)

Para cada passo com FAIL, listar: número do passo, o que foi observado vs. esperado, log de Console
relevante (copiar a linha de erro/exception literal), e se bloqueia o closeout da spec que motivou a
execução.

## Overall

**PASS** (todos os fluxos PASS, incluindo 6b sem a string de regressão) / **FAIL** (um ou mais fluxos
FAIL — listar quais)

## Notas

- Cenários individuais relevantes já existentes referenciados nesta execução (se algum FAIL exigiu
  cruzar com um cenário específico de `docs/validation/playmode/*_human_test_scenario.md`).
- Limitações conhecidas / passos pulados e por quê.
```

---

## Notas para execução posterior

Revisar este master quando um novo loop/sistema core (não uma feature pontual) for adicionado ao jogo,
para manter os 11 fluxos representativos do estado real. Specs de arquitetura sensível devem citar este
checklist master como evidência de smoke final quando aplicável, complementando (não substituindo) seu
próprio cenário específico via skill `gameplay-test-scenario`.

Este checklist não substitui nem invalida nenhum dos cenários individuais existentes em
`docs/validation/playmode/*_human_test_scenario.md`.
