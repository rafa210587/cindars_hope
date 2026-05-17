# Cindar's Hope — Fase 8: Plano de Execução com Codex v1.0

> **Fase:** 8 de 13  
> **Status:** Pronto para execução local  
> **Objetivo:** implementar o MVP Fazenda por PRs pequenos, rastreáveis e revisáveis  
> **Depende de:** `GDD_v2.6.md`, `ARCH_fase4_v2.2.md`, `FASE7_SPEC_MVP_FARM_v2.2.md`, `CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md`, `CLAUDE_v1.2.md`

---

## 1. Princípio central

O Codex deve implementar **fatias pequenas**, não “o jogo inteiro”.

A unidade correta de trabalho é:

```text
1 PR pequeno → 1 objetivo → poucos arquivos → teste manual claro → commit em português
```

O objetivo da Fase 8 é concluir este loop:

```text
BootScene → FarmScene → inventário inicial → plantar → avançar dias → colher → vender → salvar → fechar → reabrir → estado restaurado
```

---

## 2. Regras obrigatórias para qualquer prompt Codex

Todo prompt para Codex deve conter:

1. Documentos a ler.
2. PR alvo.
3. Escopo permitido.
4. Arquivos permitidos.
5. Arquivos proibidos.
6. Critérios de aceite.
7. Teste manual.
8. Regras de arquitetura.
9. O que não fazer.

### Prompt base

```md
Leia primeiro:
- CLAUDE_v1.2.md
- docs/GDD_v2.6.md
- docs/ARCH_fase4_v2.2.md
- docs/FASE7_SPEC_MVP_FARM_v2.2.md
- docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md

Implemente somente:
<PR-ID> — <nome>

Objetivo:
<uma frase>

Arquivos permitidos:
- <lista exata>

Arquivos proibidos:
- docs/GDD_v2.6.md
- docs/ARCH_fase4_v2.2.md
- qualquer arquivo fora da lista permitida

Regras:
- Não implementar V2/FULL.
- Não ampliar escopo.
- Não usar GameObject.Find, FindObjectOfType ou FindObjectsByType em runtime.
- Não usar StreamingAssets para save.
- Não serializar referências Unity em JSON.
- Dados de jogo devem ficar em ScriptableObject.
- Comunicação entre sistemas deve usar GameEventBus.

Ao final, entregue:
- arquivos alterados;
- resumo técnico curto;
- teste manual executável;
- riscos/pendências;
- próximo PR sugerido.
```

---

## 3. Branch strategy

```bash
git checkout dev
git pull
git checkout -b feature/fase8-pr-001-core-foundation
```

Padrão:

```text
feature/fase8-pr-<numero>-<nome-curto>
```

Exemplos:

```text
feature/fase8-pr-001-core-foundation
feature/fase8-pr-008-plantio-basico
feature/fase8-pr-016-load-boot
```

Commits em português:

```bash
git commit -m "feat: adicionar fundação de eventos core"
git commit -m "fix: corrigir serialização do inventário por id"
git commit -m "test: adicionar validação de stack do inventário"
```

---

## 4. Definition of Ready por PR

Antes de abrir tarefa no Codex:

- [ ] O PR cabe em revisão humana curta.
- [ ] Há lista de arquivos permitidos.
- [ ] Há lista de arquivos proibidos.
- [ ] Há teste manual objetivo.
- [ ] O escopo é MVP, não V2/FULL.
- [ ] O repositório está limpo (`git status`).
- [ ] Unity abre sem erro antes da mudança.

---

## 5. Definition of Done por PR

Um PR só está pronto quando:

- [ ] Unity compila sem erro.
- [ ] Console não tem erros novos.
- [ ] Teste manual passa.
- [ ] Regras do `CLAUDE_v1.2.md` foram respeitadas.
- [ ] Não há busca global em runtime.
- [ ] Não há hardcode de dados de jogo em MonoBehaviour.
- [ ] Save, se envolvido, usa `Application.persistentDataPath`.
- [ ] Save, se envolvido, usa IDs e tipos simples.
- [ ] Alterações estão pequenas e revisáveis.
- [ ] Commit em português criado.

---

## 6. Sequência de PRs da Fase 8

### PR-001 — Core foundation

**Objetivo:** criar fundação mínima de eventos.

**Specs relacionadas:** FARM-001, ARCH seção GameEventBus.

**Arquivos permitidos:**

```text
Assets/_Game/Scripts/Core/GameEventBus.cs
Assets/_Game/Scripts/Core/Events/DayStartedEvent.cs
Assets/_Game/Scripts/Core/Events/GoldChangedEvent.cs
Assets/_Game/Scripts/Core/Events/InventoryChangedEvent.cs
Assets/_Game/Scripts/Core/Events/SeedPlantedEvent.cs
Assets/_Game/Scripts/Core/Events/CropHarvestedEvent.cs
Assets/_Game/Scripts/Core/Events/TreeChoppedEvent.cs
Assets/_Game/Scripts/Core/Events/FishCaughtEvent.cs
Assets/_Game/Scripts/Core/Events/HungerChangedEvent.cs
Assets/_Game/Scripts/Core/Events/GameSavedEvent.cs
```

**Não fazer:** Player, cena, UI, inventário.

**Teste manual:** Unity compila; nenhum erro no Console.

**Prompt Codex:**

```md
Implemente PR-001 — Core foundation. Crie somente GameEventBus e eventos mínimos listados. Eventos devem ser payloads simples, sem GameObject, Transform, MonoBehaviour ou ScriptableObject. Não implemente managers.
```

---

### PR-002 — Data contracts e registries

**Objetivo:** criar ScriptableObjects base e registries por ID.

**Specs relacionadas:** FARM-011, FARM-021, FARM-071.

**Arquivos permitidos:**

```text
Assets/_Game/Scripts/Core/Data/IIdentifiedData.cs
Assets/_Game/Scripts/Core/Data/IDataRegistry.cs
Assets/_Game/Scripts/Core/Data/ItemDatabaseSO.cs
Assets/_Game/Scripts/Core/Data/SeedDatabaseSO.cs
Assets/_Game/Scripts/Farm/Data/SeedDataSO.cs
Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs
Assets/_Game/Scripts/Inventory/Data/ItemCategory.cs
Assets/_Game/Scripts/Player/Data/PlayerDataSO.cs
```

**Teste manual:** criar assets manualmente no Unity; campos aparecem no Inspector.

---

### PR-003 — Bootstrap managers vazios

**Objetivo:** criar managers persistentes mínimos sem gameplay completo.

**Arquivos permitidos:**

```text
Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs
Assets/_Game/Scripts/Player/PlayerManager.cs
Assets/_Game/Scripts/Inventory/InventoryManager.cs
Assets/_Game/Scripts/Core/Time/TimeManager.cs
Assets/_Game/Scripts/Save/SaveManager.cs
```

**Regras:** managers podem compilar com métodos mínimos/stubs, mas não devem fingir feature completa.

**Teste manual:** BootScene ou objeto bootstrap inicializa managers uma vez; não duplica em reload.

---

### PR-004 — NewGameState e inventário inicial

**Objetivo:** iniciar estado novo com ouro, HP e itens iniciais a partir de `PlayerDataSO`.

**Specs relacionadas:** FARM-021.

**Entregável:** inventário interno com stack, sem UI ainda.

**Teste manual:** Debug/Inspector mostra Semente Trigo x5, Semente Cenoura x3, Cana Básica x1, ouro inicial.

---

### PR-005 — FarmScene mínima

**Objetivo:** criar cena mínima com tilemap, player placeholder e câmera.

**Specs relacionadas:** FARM-001, FARM-003.

**Preferência:** criar Editor script `CreateMvpFarmScene.cs` para reproduzir a cena.

**Teste manual:** abrir FarmScene e ver jogador, chão e bordas.

---

### PR-006 — Movimento e colisão

**Objetivo:** player se move com WASD/setas, câmera segue e bordas bloqueiam.

**Specs relacionadas:** FARM-002.

**Teste manual:** mover nas 4 direções; diagonal não acelera; jogador não atravessa borda.

---

### PR-007 — Interação genérica

**Objetivo:** implementar `IInteractable`, `InteractionSystem` e hint textual simples.

**Specs relacionadas:** FARM-014.

**Teste manual:** objeto fake interagível mostra hint e responde ao E.

---

### PR-008 — Canteiros e plantio

**Objetivo:** 9 canteiros, menu textual e plantio de sementes.

**Specs relacionadas:** FARM-012, FARM-016.

**Teste manual:** aproximar do canteiro, pressionar E, escolher trigo, semente reduz, canteiro muda visual.

---

### PR-009 — Crescimento por dia

**Objetivo:** TAB avança dia, publica `DayStartedEvent`, plantas crescem.

**Specs relacionadas:** FARM-013, FARM-031, FARM-032.

**Teste manual:** plantar trigo, avançar 3 dias, canteiro fica pronto.

---

### PR-010 — Colheita e inventário visual

**Objetivo:** colher planta pronta e ver item no inventário textual.

**Specs relacionadas:** FARM-015, FARM-021, FARM-022.

**Teste manual:** colher trigo, inventário mostra quantidade correta, canteiro volta vazio.

---

### PR-011 — Fome e consumo

**Objetivo:** HungerSystem, HUD simples e consumo de comida.

**Specs relacionadas:** FARM-061, FARM-062.

**Teste manual:** fome diminui por passos; usar trigo/peixe restaura fome.

---

### PR-012 — Árvores

**Objetivo:** cortar árvore e receber madeira.

**Specs relacionadas:** FARM-041, FARM-042.

**Teste manual:** E na árvore adiciona madeira e muda nível visual.

---

### PR-013 — Lago e pesca

**Objetivo:** pescar Peixe Comum no lago com cana básica.

**Specs relacionadas:** FARM-051, FARM-052.

**Teste manual:** E no FishingSpot espera 3s e adiciona peixe.

---

### PR-014 — Venda

**Objetivo:** SellPoint e SellMenu textual para vender colheitas/peixe/madeira.

**Specs relacionadas:** FARM-SELL, FARM-032.

**Teste manual:** vender trigo aumenta ouro e remove item.

---

### PR-015 — Save

**Objetivo:** salvar estado em JSON.

**Specs relacionadas:** FARM-071.

**Regras específicas:**

- Path: `Application.persistentDataPath/saves/slot_1.json`.
- Serializar IDs e tipos simples.
- Incluir `SchemaVersion`.

**Teste manual:** TAB/dormir gera `slot_1.json` com dia, ouro, fome, inventário e plots.

---

### PR-016 — Load e Boot

**Objetivo:** carregar save existente e restaurar estado.

**Specs relacionadas:** FARM-072.

**Teste manual:** plantar, avançar dias, colher/vender, salvar, fechar, abrir, estado restaurado.

---

### PR-017 — Feedbacks mínimos

**Objetivo:** popups/fade/VFX mínimos sem alterar regras de gameplay.

**Specs relacionadas:** FARM-015, FARM-031, FARM-042, FARM-052.

**Teste manual:** feedback visual aparece e desaparece sem travar input.

---

### PR-018 — Hardening MVP

**Objetivo:** corrigir bugs, remover logs temporários, validar loop vertical.

**Teste final:**

```text
Novo jogo → plantar trigo → dormir 3x → colher → vender → salvar → fechar → reabrir → dia/ouro/inventário/canteiros restaurados
```

---

## 7. Como revisar saída do Codex

### 7.1 Comandos

```bash
git status
git diff --stat
git diff
```

### 7.2 Busca por violações

```bash
grep -R "GameObject.Find\|FindObjectOfType\|FindObjectsByType\|StreamingAssets" Assets/_Game/Scripts
```

Se aparecer em código runtime, revisar ou rejeitar.

### 7.3 Sinais de PR ruim

- Muitos arquivos alterados fora do escopo.
- Implementação de V2/FULL escondida.
- Dados hardcoded em MonoBehaviour.
- Save serializando `ScriptableObject`.
- Managers se chamando diretamente sem evento.
- Código compila, mas cena depende de configuração manual não documentada.

---

## 8. Política de rollback

Se Codex gerar algo ruim:

```bash
git restore <arquivo>
# ou, se for tudo da tentativa:
git reset --hard HEAD
```

Nunca tentar “consertar em cima” de uma alteração grande e confusa. Melhor reduzir o prompt e refazer.
