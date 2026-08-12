# SPEC — City schedule: colliders de tronco e placeholders de fachada

> **Spec ID:** `spec_content_city_schedule_colliders_facades_v1`
> **Status:** A implementar
> **Wave:** WAVE CONTENT — Integridade da TownScene
> **Priority:** P2
> **Type:** Data / Content (cena) — Validation
> **Domain:** City
> **Parallelizable:** NO
> **Parallel group:** N/A
> **Can run with:** N/A (mexe na TownScene / gerador de cena, recurso compartilhado)
> **Must not run with:** qualquer spec que edite `TownScene.unity`, `CreateMvpTownScene.cs`, `TownCityLayout.cs` ou `ValidateFableCitySchedule.cs`
> **Repo lock scope:** `Assets/_Game/Scenes/TownScene.unity`, `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs`, `Assets/_Game/Scripts/Editor/Validation/ValidateFableCitySchedule.cs`, `Assets/_Game/Scripts/World/City/TownCityLayout.cs`
> **Depends on:** `docs/project/CURRENT_STATE.md`
> **Blocks:** gate `CindarsHope/Validar Projeto` verde para a cidade
> **Scope:** Zerar os 2 checks que falham em `ValidateFableCitySchedule` — colliders de tronco só nas 8 árvores internas, e placeholder de fachada semântica em todo prédio v8 — decidindo na Fase 0 se é conteúdo faltando na cena ou validador desatualizado.
> **Out of scope:** arte final de fachada; layout novo da cidade; NPCs/schedule (os outros checks já passam); qualquer landmark novo.
> **Validation level alvo:** UNITY_VALIDATED (validador de editor) — PLAYMODE deferido
> **Executor:** Claude | Codex

## 5. Contexto
`ValidateFableCitySchedule` reporta 2 falhas ("Only the 8 internal trees carry trunk colliders", "Every v8 building has a replaceable semantic facade placeholder"). Os demais checks do validador passam. É débito de conteúdo/cena, não regressão de código. Fechar isto deixa o `Validar Projeto` da cidade verde.

## 6. Problema
Dois invariantes da cidade não são satisfeitos pela TownScene atual:
1. **Trunk colliders:** o validador espera que APENAS as 8 árvores internas tenham collider de tronco; a cena diverge (árvores extras com/sem collider, ou as 8 sem).
2. **Facade placeholders:** o validador espera que todo prédio v8 tenha um placeholder de fachada semântica substituível (âncora para arte futura); prédios sem esse placeholder falham.
Sem fechar, o gate de validação da cidade fica vermelho e a arte de fachada não tem âncora consistente.

## 7. Objetivo
Ao final, `ValidateFableCitySchedule` retorna 0 erros, com as 8 árvores internas (e só elas) carregando trunk collider e todo prédio v8 com placeholder de fachada — sem alterar layout, landmarks ou schedule já válidos.

## 8. Fontes obrigatórias lidas
`Assets/_Game/Scripts/Editor/Validation/ValidateFableCitySchedule.cs` (métodos `Check` das linhas ~129+); `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs`; `Assets/_Game/Scripts/World/City/TownCityLayout.cs`; `.claude/rules/unity-assets.md`; `.claude/rules/editor-generation-orchestration.md`; `.claude/skills/tilemap-world-rendering/SKILL.md`.

## 9. Estado atual do repo — Phase 0 (auditado 2026-08-11)
```text
ValidateFableCitySchedule.cs: os 2 checks que FALHAM são
  linha ~129: Check("Only the 8 internal trees carry trunk colliders", <cond>)
  linha ~131: Check("Every v8 building has a replaceable semantic facade placeholder", <cond>)
Demais checks (roof reveals >=24, portas >=24, floors, landmarks, 84 anchors) PASSAM.
A cena é GERADA por CreateMvpTownScene.cs (Inicializar Projeto recria a TownScene — destrutivo).
```
> A Fase 0 de EXECUÇÃO deve LER as duas condições exatas em `ValidateFableCitySchedule.cs` (o que conta como "8 internal trees" e "semantic facade placeholder": nome/tag/componente) e cruzar com o que `CreateMvpTownScene` produz hoje. **Decisão da Fase 0:** (A) **conteúdo faltando** → ajustar `CreateMvpTownScene` para (a1) marcar trunk collider só nas 8 árvores internas, (a2) instanciar o placeholder de fachada em cada prédio v8; ou (B) **validador desatualizado** → se a cena já reflete o design atual e o check ficou obsoleto (ex.: contagem/critério mudou), corrigir a asserção em `ValidateFableCitySchedule` com justificativa. **Default = (A)**; só ir para (B) com evidência de que a cena está correta e o check é que drifou.

## 13. Regras de não duplicação
Não criar novo gerador de cidade nem segundo validador de schedule. Ajustar os existentes (`CreateMvpTownScene`, `ValidateFableCitySchedule`). Sem `[MenuItem]` avulso — a geração roda por `CindarsHope/Inicializar Projeto` (rule `editor-generation-orchestration`).

## 15. Arquitetura alvo — CRIAR vs MODIFICAR
```text
MODIFICAR (caminho A):
  Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs
    — na criação das árvores: aplicar trunk collider SÓ nas 8 internas (as demais sem);
    — na criação de cada prédio v8: instanciar o placeholder de fachada semântica (mesmo componente/tag que o validador procura).
MODIFICAR (caminho B, só se Fase 0 provar validador obsoleto):
  Assets/_Game/Scripts/Editor/Validation/ValidateFableCitySchedule.cs — corrigir a asserção divergente, com comentário de justificativa.
GERADO:
  Assets/_Game/Scenes/TownScene.unity — recriada por Inicializar Projeto (evidência por rule unity-assets; NÃO editar YAML à mão).
```

## 16. Contratos, dados e eventos
### 16.1 Data contracts » N/A (sem novo tipo de dado; usa marcador/tag/componente já esperado pelo validador — Fase 0 identifica o exato).
### 16.2–16.5 » N/A (sem runtime/evento/save/UI novos).

## 17. Sistemas afetados
City / TownScene generation / Editor validation. (Sem runtime de gameplay.)

## 18. Arquivos permitidos
```text
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs
Assets/_Game/Scripts/Editor/Validation/ValidateFableCitySchedule.cs   (só caminho B)
Assets/_Game/Scripts/World/City/TownCityLayout.cs                     (só se o marcador for constante lá)
Assets/_Game/Scenes/TownScene.unity                                   (via Inicializar Projeto, não YAML manual)
docs/validation/**
```
## 19. Arquivos proibidos
```text
Outros .unity/.prefab; Packages/**; ProjectSettings/**; docs_old/**; qualquer sistema fora de City/editor de cidade.
```

## 20. Estratégia de implementação
```md
### Fase 0 — Auditoria: ler as 2 condições em ValidateFableCitySchedule; cruzar com o output de CreateMvpTownScene; decidir A vs B.
### Fase 1 — (A) editar CreateMvpTownScene: trunk collider nas 8 internas + placeholder de fachada por prédio v8. (B) editar a asserção com justificativa.
### Fase 2 — Geração: rodar CindarsHope/Inicializar Projeto (recria a TownScene). Registrar evidência (rule unity-assets).
### Fase 3 — Validação: rodar CindarsHope/Validar Projeto → ValidateFableCitySchedule 0 erros.
### Fase 4 — Relatório: docs/validation/<spec_id>_execution_report.md.
```

## 21. Ordem segura de execução
1. Fase 0 (ler condições + cruzar). 2. Editar gerador (ou validador em B). 3. Inicializar Projeto. 4. Validar Projeto. 5. Relatório.

## 14. Critérios de aceite — binários + evidência
```md
### 14.1 Trunk colliders corretos
- Resultado: exatamente as 8 árvores internas têm trunk collider; nenhuma outra.
- Evidência: ValidateFableCitySchedule check "Only the 8 internal trees carry trunk colliders" PASS.
### 14.2 Placeholders de fachada
- Resultado: todo prédio v8 tem o placeholder de fachada semântica.
- Evidência: check "Every v8 building has a replaceable semantic facade placeholder" PASS.
### 14.3 Nenhum outro check regrediu
- Resultado: os checks que já passavam (anchors, portas, roof reveals, landmarks) continuam PASS.
- Evidência: ValidateFableCitySchedule report 0 falhas no total; log em docs/validation/.
### 14.4 Compila e cena coerente
- Resultado: editor compila; Inicializar Projeto sem erro novo.
- Evidência: RunUnityCompileValidation.ps1 exit 0 + log do Inicializar sem exceção nova.
```

## 23. Edge cases / falhas
- **Definição de "8 internal trees" e "facade placeholder" é do validador** → a Fase 0 DEVE ler o predicado exato (nome/tag/componente que o check conta); marcar a árvore/placeholder errados falha do mesmo jeito. Não adivinhar.
- **Caminho B (validador obsoleto) é perigoso** → afrouxar uma asserção pode mascarar drift real da cena. Só ir para B com evidência de que a cena reflete o design atual; comentar a justificativa no código; nunca baixar a contagem "pra passar".
- **Editar o gerador pode regredir outros checks** (roof reveals, portas, floors, 84 anchors) → o critério 14.3 exige `report 0 falhas no total`, não só os 2 alvos.
- **Recriação de cena é destrutiva** → rodar Inicializar Projeto isolado; git diff deve mostrar só `TownScene.unity` mudada; registrar evidência (rule unity-assets); nunca editar YAML à mão.
- **Trunk collider em árvore errada quebra pathing/navegação** → confirmar que só as 8 internas ganham collider; as externas (floresta densa) permanecem sem, senão o player/NPC podem travar.

## 22. Validação e gates
Nível-alvo: UNITY_VALIDATED. Smoke visual da cidade em Play Mode = `DEFERRED_TO_FINAL_VALIDATION` (checklist `spec_validation_human_playmode_smoke_v1`). Recriação de cena é destrutiva — registrar evidência de geração (rule unity-assets); nunca editar o `.unity` YAML à mão (rule).
