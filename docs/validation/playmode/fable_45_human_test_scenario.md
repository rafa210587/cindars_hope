# Human Play Mode Scenario — fable_45 Bestiary Codex Tab

> Spec: `fable_45_spec_bestiary_ui_full_codex`
> Status: DEFERRED_TO_FINAL_VALIDATION (Play Mode obrigatório para UI; ambiente sem Unity nesta sessão)
> Cobre: CA-1 (lista/silhueta/tier), CA-2 (ficha gated), CA-3 (narração/completude visual), CA-4 (teclado/modal).

A logica pura (projection/completude/filtros/gating) ja esta coberta por EditMode
(`Assets/_Game/Tests/EditMode/UI/BestiaryCodexTests.cs`). Este cenario valida o que so o
Unity prova: Canvas, foco de teclado, bloqueio de gameplay e rebind reativo na cena.

## Pre-condicoes

- FarmScene/CaveScene carregada com o painel unico F14 disponivel.
- `BestiaryScreenView` registrada na aba 5 (Bestiary) do painel F14 e configurada via
  `Configure(modalManager, enemyKnowledgeService, CanonicalBestiaryCatalog.All)` pelo host
  do painel (mesmo ponto onde as demais abas sao injetadas).
- Save novo (nenhuma criatura descoberta) para o passo de empty state.

## Passos

### CA-1 — Lista com descoberta progressiva
1. Abrir o painel e ir para a aba Bestiario. ESPERADO: empty state
   "Nenhuma criatura observada ainda." (nenhuma criatura vista).
2. Em combate, ser visto por / ver 1 inimigo comum (band 1). Reabrir a aba.
   ESPERADO: a criatura aparece com nome + icone; as demais nao vistas aparecem como
   silhueta (sprite com tint preto) + "???". Agrupadas por banda/familia, ordem estavel.
3. Confirmar que NENHUM gate boss (tier 3) nem os Quatro (tier 4) aparecem na lista
   enquanto a flag de quest correspondente nao estiver setada.

### CA-2 — Ficha gated por categoria
4. Selecionar a criatura descoberta (Enter). ESPERADO: ficha a direita mostra Identidade;
   categorias ainda nao desbloqueadas exibem "???" (nada de dado real).
5. Matar a mesma criatura 5x (threshold F21 drops). Sem fechar, observar o rebind reativo
   (evento `BestiaryKnowledgeUnlockedEvent`) OU reabrir a aba.
   ESPERADO: a linha de Drops passa a mostrar o item real; contador "Derrotas: 5".
6. Documentar totalmente a criatura (identidade + comportamento + vulnerabilidade + drops).
   ESPERADO: linha "Documentado: +3% de dano contra esta criatura." aparece (EMENDA-D).

### CA-3 — Narracao e completude
7. Com a identidade descoberta, confirmar que o texto de narracao do catalogo aparece na
   ficha. Antes da identidade, a narracao NAO aparece.
8. Observar o contador no topo: "Vistos X/N · Documentados Y/N", onde N = entradas visiveis
   (tiers ocultos nao entram no denominador).

### CA-4 — Navegacao e contrato de modal
9. Com a aba aberta, tentar mover o player (WASD). ESPERADO: player NAO se move (gameplay
   bloqueado pelo ModalManager).
10. Navegar a lista com setas; Enter abre a ficha; Esc volta/fecha respeitando o stack.
11. Tab cicla entre as abas do painel (contrato F14 preservado); demais abas intactas.
12. Filtro v1: ciclar All → Seen → Defeated e confirmar que a lista filtra coerentemente.

## Resultado esperado

Aba codex funcional: descoberta progressiva, silhuetas, ficha gated, narracao pos-identidade,
completude correta, navegacao por teclado, sem vazamento de spoiler acima do tier permitido,
sem mover o player com o painel aberto.

## Anti-regressao a verificar

- Nenhuma criatura renderiza acima do SpoilerTier permitido (os Quatro = tier 4).
- F21 (EnemyKnowledgeService) nao foi alterada — somente consumida.
- Shop/Inventory/Equipment/Skill/Quest/Social tabs continuam funcionando.
- Nenhum GameObject.Find/FindObjectOfType em runtime (host injeta via Configure).
