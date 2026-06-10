# WAVE_INTEGRATION_19 — Human Play Mode Checklist

**Date:** 2026-06-10
**Status:** PENDING_HUMAN_EXECUTION

---

## Pré-condições

1. Unity Editor aberto, sem erros de compilação
2. Branch `dev` atualizado: `git pull origin dev`
3. FarmScene como cena inicial

---

## Checklist

| Step | Ação | Resultado Esperado | Pass/Fail | Notas |
|---|---|---|---|---|
| 1 | Abrir FarmScene no Unity Editor | Sem erros vermelhos no Console | | |
| 2 | Pressionar Play | Player spawna, DebugHud visível dos dois lados | | |
| 3 | Verificar HUD | Gold/HP/Stamina/Hunger/Equipment visíveis no DebugHud | | |
| 4 | Mover player (WASD/Arrows) | Player se move sem travar | | |
| 5 | Aproximar de objeto interagível | Prompt aparece: "Interacao: {prompt}" | | |
| 6 | Pressionar I (Inventory) | Painel de inventory abre, DebugHud ainda visível | | |
| 7 | Tentar mover player com inventory aberto | Player NÃO se move (input bloqueado) | | |
| 8 | Tentar dash (Space) com inventory aberto | Dash NÃO executa | | |
| 9 | Pressionar Escape com inventory aberto | Inventory fecha corretamente | | |
| 10 | Pressionar L (Equipment) | Painel de equipment abre | | |
| 11 | Tentar dash com equipment aberto | Dash NÃO executa | | |
| 12 | Pressionar Escape | Equipment fecha | | |
| 13 | Pressionar U (SkillTree) | Painel de skill tree abre com slots R/T/Y/G | | |
| 14 | Tentar dash com skill tree aberto | Dash NÃO executa | | |
| 15 | Pressionar Escape | Skill tree fecha | | |
| 16 | Pressionar J (QuestLog) | Quest Log abre, mostra "(nenhuma missão ativa)" | | |
| 17 | Tentar dash com quest log aberto | Dash NÃO executa (ModalType.QuestLog na stack) | | |
| 18 | Pressionar Escape ou J | Quest Log fecha | | |
| 19 | Pressionar F5 (Save) | Feedback no HUD: "Jogo salvo." | | |
| 20 | Pressionar F9 (Load) | Feedback no HUD: "Jogo carregado." | | |
| 21 | Verificar que estado foi restaurado | Gold/inventory/quest state mantidos | | |
| 22 | Ir para Town (se WAVE16 wired) | Transição de cena funciona, TownScene carrega | | |
| 23 | Interagir com Thalindra | Diálogo abre com opções: "! Qual é a tarefa?", "Comprar", "Vender", "Adeus" | | |
| 24 | Selecionar "! Qual é a tarefa?" | QuestOffer panel abre com detalhes da quest | | |
| 25 | Tentar dash durante QuestOffer aberto | Dash NÃO executa | | |
| 26 | Aceitar quest | Feedback: "Quest aceita: {questId}" | | |
| 27 | Abrir QuestLog (J) | Quest aparece como ativa com objetivos | | |
| 28 | Salvar após aceitar quest | Feedback: "Jogo salvo." | | |
| 29 | Parar Play e reiniciar Play + F9 | Quest ainda aparece no QuestLog | | |
| 30 | Interagir com Thalindra | Opção de quest mudou (progress/turn-in) | | |
| 31 | Selecionar Comprar | Shop buy panel abre | | |
| 32 | Tentar dash durante shop | Dash NÃO executa | | |
| 33 | Comprar item | Feedback: "Economia: ..." | | |
| 34 | Pressionar Escape | Shop fecha corretamente | | |
| 35 | Entrar na cave (se WAVE16 wired) | Feedback HUD: "Entrando na caverna (nível 1)" | | |
| 36 | Derrotar inimigo (se WAVE17 wired) | Feedback HUD: "Inimigo derrotado: ... | Loot: ..." | | |
| 37 | Sair da cave (se WAVE16 wired) | Feedback HUD: "Retornando à superfície → FarmScene" | | |
| 38 | Completar objetivos da quest | Feedback: "Pronto para entregar: {questId}" | | |
| 39 | Entregar quest na Thalindra | Feedback: "Quest concluída: {questId}" + "Recompensa: Ng" | | |
| 40 | Abrir QuestLog | Quest aparece como concluída | | |

---

## Verificação do slot_1.json

Após play mode com quest salva, verificar manualmente o arquivo `slot_1.json`:

```json
{
  "SchemaVersion": 5,
  "Quests": {
    "Version": 1,
    "QuestStates": [
      {
        "QuestId": "...",
        "State": 3,
        "GrantedRewardIds": ["..."]
      }
    ]
  }
}
```

---

## Blocking Issues

| Issue | Severidade | Resolução |
|---|---|---|
| Nenhum feedback no HUD após F5 | BLOCKING | Verificar que DebugHud está wired + GameSavedEvent publicado |
| Quest não aparece no QuestLog após reload | BLOCKING | Verificar WAVE18 save/load gap (QuestStateSectionSaveData) |
| Dash executa durante modal | BLOCKING | Verificar HasActiveModal check em PlayerDashController |
| QuestLog não abre (J key) | BLOCKING | Verificar QuestLogRuntimeBinder + QuestLogPanelController wiring |
| Thalindra não mostra quest option | BLOCKING | Verificar NpcShopController + QuestService wiring |
| Cave não tem inimigos | NON-BLOCKING | WAVE17 human wiring pendente — debt explícito |
| Cave entrance não aparece | NON-BLOCKING | WAVE16 human wiring pendente — debt explícito |

---

## Status no Completion

Atualizar ao executar:

```text
docs/validation/WAVE_INTEGRATION_19_HUD_UX_ACCEPTANCE_REPORT.md
docs/project/CURRENT_STATE.md
```

Mudança de status:
- `BUILD_VALIDATED_HUD_UX_ACCEPTANCE_READY_PENDING_HUMAN_PLAYMODE` → se builds passam, código wired, Play Mode não executado
- `ACCEPTED` → se todos os 40 steps passam sem issues blocking
- `BLOCKED_BY_SCENE_WIRING` → se WAVE16 wiring impede cave flow test
