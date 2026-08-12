# SPEC — Limpeza dos warnings de serialização UAC (Unity 6000.5)

> **Spec ID:** `spec_cleanup_uac_serialization_6000_5_v1`
> **Status:** A implementar
> **Wave:** WAVE CLEANUP — Higiene pós-upgrade 6000.5
> **Priority:** P3
> **Type:** Runtime (cleanup)
> **Domain:** Core / NPC / Save / World
> **Parallelizable:** YES
> **Parallel group:** cleanup
> **Can run with:** specs que não toquem `DialogueLineCondition`/`DialogueLineSelector`, `FestivalRegistry`, `SaveManager`
> **Must not run with:** specs que editem esses 3 arquivos
> **Repo lock scope:** `Assets/_Game/Scripts/NPC/DialogueLineCondition.cs`, `Assets/_Game/Scripts/NPC/DialogueLineSelector.cs`, `Assets/_Game/Scripts/World/Calendar/FestivalRegistry.cs`, `Assets/_Game/Scripts/Save/SaveManager.cs`
> **Depends on:** `docs/project/CURRENT_STATE.md`; `.claude/rules/unity-architecture.md`
> **Blocks:** nada (higiene) — reduz ruído do Console no 6000.5
> **Scope:** Zerar os 7 warnings UAC1001/UAC1010 do analisador de serialização do Unity 6000.5, tratando cada caso pela sua causa real (nullable value type serializável, tipo sem `[Serializable]`, `[SerializeField]` morto).
> **Out of scope:** mudar comportamento de diálogo/festival/save; migração de save; qualquer outro warning fora dos 3 arquivos.
> **Validation level alvo:** BUILD_VALIDATED (compile 0 warnings UAC) + EditMode de caracterização
> **Executor:** Claude | Codex

## 5. Contexto
Após o upgrade para Unity 6000.5.7f1, o analisador de serialização passou a emitir 7 warnings (UAC1001/UAC1010) em 3 arquivos. Não bloqueiam compile, mas poluem o Console e alguns indicam **serialização silenciosamente quebrada** (campo que o Unity ignora). Limpar deixa o Console honesto e o modelo de dados correto.

## 6. Problema
Três causas distintas:
1. **`DialogueLineCondition`** (`[Serializable]`) tem campos `Season?`, `WeatherType?`, `int? MinFriendship`, `DialogueTimeBand?`, `int? MinRomanceStage` — **UAC1001**: Unity não serializa nullable value types, então esses gates NÃO persistem se a condição for Unity-serializada. É usada em `DialogueLineSelector.Condition` (campo público).
2. **`FestivalRegistry`**: `[SerializeField] private List<Festival> _festivals` com `Festival` **sem `[Serializable]`** — **UAC1010**: o `[SerializeField]` é ignorado; a lista só existe porque `InitializeDefaultFestivals()` a popula em código.
3. **`SaveManager`**: `[SerializeField] private CorpseRecoveryManager _corpseRecoveryManager` — **UAC1010**: `CorpseRecoveryManager` é classe C# pura (não `[Serializable]`, não MonoBehaviour), criada por fábrica no corte Core|Player. O `[SerializeField]` é leftover morto.

## 7. Objetivo
Ao final, os 7 warnings UAC somem, sem mudar o comportamento observável de diálogo, festival ou save — cada caso corrigido pela sua causa (não por supressão cega).

## 8. Fontes obrigatórias lidas
`.claude/rules/unity-architecture.md`; os 4 arquivos do lock scope; `Assets/_Game/Scripts/NPC/DialogueLineSelector.cs`; a doc do analisador (UAC1001/UAC1010 — link no warning). Para o caso 1: `.claude/skills/npc-dialogue-authoring/SKILL.md`.

## 9. Estado atual do repo — Phase 0 (auditado 2026-08-11)
```text
DialogueLineCondition.cs: [Serializable]; campos nullable Season?/WeatherType?/int?/DialogueTimeBand?/int? (5 warnings UAC1001).
DialogueLineSelector.cs:13  public DialogueLineCondition Condition;  → a condição É um campo público.
  A CONFIRMAR na Fase 0: DialogueLineSelector/DialogueLineCondition é serializada pelo Unity (SO/cena/prefab)
  ou SEMPRE construída em C# por geradores (npc-dialogue-authoring)? A resposta define o fix do caso 1.
FestivalRegistry.cs: struct/classe aninhada Festival { FestivalType Type; int DayInYear; string DisplayName; bool IsHidden; }
  sem [Serializable]; _festivals populado por InitializeDefaultFestivals() em OnEnable (UAC1010).
SaveManager.cs:84  [SerializeField] private CorpseRecoveryManager _corpseRecoveryManager; (UAC1010)
  A CONFIRMAR na Fase 0: _corpseRecoveryManager é LIDO em algum lugar do SaveManager? (Grep). Se não, é morto.
```

## 13. Regras de não duplicação
Sem novo tipo/serviço. Correções mínimas nos 3 arquivos. Não introduzir `Result<T>` nem camada de serialização paralela (rule csharp-style / code-minimalism-ladder).

## 15. Arquitetura alvo — MODIFICAR (nada a criar, exceto teste)
```text
Caso 1 — DialogueLineCondition.cs:
  SE nunca Unity-serializada (só C#): remover [Serializable] (mantém como dado puro de runtime) OU manter e o warning some ao não ser mais alvo de serialização — Fase 0 decide o mínimo.
  SE Unity-serializada: converter cada nullable em par serializável (bool HasX + valor), atualizando IsMet/ConstrainedAxes e os geradores que a preenchem. (Maior risco — só se confirmado.)
Caso 2 — FestivalRegistry.cs:
  Adicionar [System.Serializable] à classe aninhada Festival. (Torna _festivals de fato serializável; comportamento em runtime idêntico pois InitializeDefaultFestivals só popula se vazio.)
Caso 3 — SaveManager.cs:
  SE _corpseRecoveryManager não é lido: remover o campo [SerializeField]. SE é lido: resolvê-lo via runtime service/registry (não [SerializeField] em classe pura).
CRIAR:
  Assets/_Game/Tests/EditMode/... teste de caracterização mínimo do caso alterado (ex.: DialogueLineCondition.IsMet inalterado; FestivalRegistry defaults inalterados).
```

## 16. Contratos, dados e eventos
### 16.1–16.5 » N/A estrutural. Nenhuma assinatura pública muda (caso 1 preserva `IsMet`/`ConstrainedAxes`; caso 2 só adiciona atributo; caso 3 remove/realoca um campo privado). Se o caso 1 virar par bool+valor, os campos públicos mudam de forma — documentar e ajustar geradores.

## 17. Sistemas afetados
NPC dialogue gating / World calendar (festival) / Save wiring / EditMode tests.

## 18. Arquivos permitidos
```text
Assets/_Game/Scripts/NPC/DialogueLineCondition.cs
Assets/_Game/Scripts/NPC/DialogueLineSelector.cs        (só se o caso 1 exigir)
Assets/_Game/Scripts/World/Calendar/FestivalRegistry.cs
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Tests/EditMode/**
docs/validation/**
```
## 19. Arquivos proibidos
```text
Qualquer .asset/.unity/.prefab; Packages/**; ProjectSettings/**; geradores de conteúdo (salvo se o caso 1 par-bool exigir tocar o gerador de diálogo — então autorizar explicitamente).
```

## 20. Estratégia de implementação
```md
### Fase 0 — Auditoria: (caso 1) Grep se DialogueLineSelector/Condition é serializada (SO/cena) vs só código; (caso 3) Grep usos de _corpseRecoveryManager no SaveManager. Escolher o mínimo por caso.
### Fase 1 — Caso 2: add [Serializable] em Festival (mais seguro, fazer primeiro).
### Fase 2 — Caso 3: remover [SerializeField] morto (ou realocar se usado).
### Fase 3 — Caso 1: aplicar o mínimo decidido na Fase 0 (remover [Serializable] se só-C#, ou par bool+valor se Unity-serializada).
### Fase 4 — Testes/validação: EditMode de caracterização; compile 0 warnings UAC; relatório.
```

## 21. Ordem segura de execução
1. Fase 0 (2 greps). 2. Festival [Serializable]. 3. SaveManager campo morto. 4. DialogueLineCondition (mínimo). 5. EditMode + compile. 6. Relatório.

## 14. Critérios de aceite — binários + evidência
```md
### 14.1 Zero warnings UAC
- Resultado: nenhum UAC1001/UAC1010 nos 3 arquivos.
- Evidência: recompilar no Editor 6000.5 → Console sem UAC1001/UAC1010 (antes: 7). Print/log anexado.
### 14.2 Comportamento inalterado (diálogo)
- Resultado: DialogueLineCondition.IsMet e ConstrainedAxes retornam o mesmo para os casos de teste.
- Evidência: EditMode de caracterização verde (casos com/sem cada gate).
### 14.3 Festival defaults inalterados
- Resultado: FestivalRegistry produz os mesmos festivais default.
- Evidência: EditMode assertando os 3 defaults (Planting/Harvest/Market) e dias.
### 14.4 Compila
- Resultado: Assembly-CSharp + Editor compilam sem erro.
- Evidência: RunUnityCompileValidation.ps1 exit 0.
```

## 23. Edge cases / falhas
- **Caso 1 vira par bool+valor → muda a FORMA pública** de `DialogueLineCondition` (some `Season?`, entra `bool HasSeason; Season Season;`) → TODOS os geradores de diálogo que preenchem a condição precisam ser atualizados; `ConstrainedAxes`/`IsMet` reescritos. Por isso a Fase 0 confirma primeiro se ela é Unity-serializada; se não for, o fix é muito menor.
- **Remover `[Serializable]` de DialogueLineCondition** pode quebrar se algo depende de serialização C# (ex.: um teste que a serializa) → Grep por `JsonUtility`/`BinaryFormatter`/serialização da classe antes de remover.
- **Caso 3: `_corpseRecoveryManager` PODE ser lido** em algum método do SaveManager → se for, remover quebra o compile; a Fase 0 faz o Grep e, se usado, resolve via runtime service em vez de `[SerializeField]`.
- **Caso 2: adicionar `[Serializable]` a `Festival`** faz o Unity passar a serializar `_festivals` → se a `FestivalRegistry` já está numa cena/prefab com a lista vazia, o comportamento continua igual (InitializeDefaultFestivals só popula se `Count==0`); confirmar que nenhuma instância na cena tinha valores inspector "fantasma".
- **Suprimir o warning (pragma) NÃO é fix** → cada caso deve ser resolvido pela causa; supressão só se a Fase 0 provar que o nullable é intencional e nunca Unity-serializado (então preferir remover `[Serializable]`).

## 22. Validação e gates
Nível-alvo: BUILD_VALIDATED + EditMode de caracterização (mudança de código exige teste ou residual risk — rule testing-quality-gate). Sem Play Mode necessário (nenhum comportamento de gameplay muda, salvo se o caso 1 virar par bool+valor — aí adicionar smoke de diálogo ao checklist final).
