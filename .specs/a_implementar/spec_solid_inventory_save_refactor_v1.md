# SPEC — Responsabilidades de inventário e escrita de save

> **Spec ID:** `spec_solid_inventory_save_refactor_v1`
> **Status:** CODE_COMPLETE_WITH_GLOBAL_GATES_FAILING — execução e evidência em docs/validation
> **Wave:** SOLID_AI — manutenção autorizada pelo pedido humano desta sessão
> **Priority:** P1
> **Type:** Runtime / Save
> **Domain:** Inventory / Save / Quest
> **Parallelizable:** CONDITIONAL
> **Parallel group:** SOLID_AI
> **Can run with:** `spec_solid_ai_harness_hardening_v1`
> **Must not run with:** edições nos arquivos listados em §18
> **Repo lock scope:** InventoryManager, InventorySlotOperations, SaveManager, SaveBackupService, três consumidores de quest ID e testes listados
> **Depends on:** auditoria do checkout e baseline em `docs/validation/SOLID_AI_PROJECT_AUDIT.md`
> **Blocks:** revisão de não regressão SOLID_AI
> **Scope:** extrair mutações determinísticas de slots, concentrar escrita segura no serviço de arquivos existente e consumir ID de quest canônico.
> **Out of scope:** features, save schema, assets, scenes, cave procedural, alterações concorrentes de farm.
> **Validation level alvo:** BUILD_VALIDATED, condicionado a todos os gates reais
> **Executor:** Codex ou Claude
> **Ordem de execucao:** caracterização antes da extração; validação e revisão depois
> **Depende de:** baseline/auditoria e contratos existentes citados nesta spec
> **Bloqueia:** closeout SOLID_AI de inventário/save

required_adrs: []
required_game_rules: []

Sem nova decisão de schema ou gameplay; mantém contratos existentes e cita as rules aplicáveis em §8.

# /speckit.specify

## 5. Contexto

O usuário pediu análise do projeto, regras para SOLID/clean code e refatoração orientada por evidência. Este contrato delimita a execução desse pedido; não representa aprovação humana de gameplay nem de uma spec anteriormente revisada.

## 6. Problema

InventoryManager mistura mutações de slots com catálogo, bootstrap e publicação. SaveManager mistura escrita de arquivos com serialização, providers e fluxo de cena. Três consumidores repetem um ID já definido em QuestRuntimeIds.

## 7. Objetivo

Mutações de slots serão testáveis sem objetos Unity. A escrita segura terá uma API direta no SaveBackupService existente. Os consumidores usarão a identidade canônica sem mudar seu valor.

## 8. Fontes obrigatórias

- `AGENTS.md`, `docs/project/CURRENT_STATE.md`
- `.claude/rules/testing-quality-gate.md`, `.claude/rules/unity-architecture.md`
- `.claude/skills/monobehaviour-decomposition/SKILL.md`, `.claude/skills/system-reuse-audit/SKILL.md`
- `.claude/skills/spec-execution/SKILL.md`, `.claude/skills/save-load-pattern/SKILL.md`
- Todos os arquivos listados em §18, e `InventorySlot.cs`, `InventoryChangedEvent.cs`, `QuestRegistry.cs`.

## 9. Estado atual — Phase 0

Leitura integral dos métodos e auditoria em 2026-09-08: InventoryManager tem 937 linhas; SaveManager tem 880 + 391 no partial Migration. `TryMoveOrMergeSlot` está em InventoryManager:546; `WriteTextSafely` em SaveManager.Migration:297. SaveBackupService já cria/restaura backups. Seis testes de move/merge e oito de escrita/recuperação existem.

Baseline: sete builds .NET e compile Unity exit 0. EditMode: 2888 total, 2884 pass, quatro falhas em trabalho concorrente de farm. Docs: 73 erros preexistentes. Não converter essas falhas em aprovação. Reexecutar os comandos de §22 para detectar mudanças desde a auditoria.

## 13. Regras de não duplicação

Reusar InventorySlot, InventoryManager, GameEventBus, SaveBackupService e QuestRuntimeIds. Não criar outro inventário, backend de backup, catálogo, interface de uso único ou barramento. Pattern: adapter Unity sobre operações puras (`monobehaviour-decomposition`); escrita é extração de responsabilidade para serviço existente, sem pattern adicional.

# /speckit.plan

## 15. Arquitetura alvo

CRIAR `InventorySlotOperations`: operações puras de split, move, merge e swap sobre slots existentes; não possui catálogo, aggregate ou eventos.

MODIFICAR InventoryManager: manter validação de catálogo, slots e índices, ownership, aggregate e eventos; delegar as mutações. MODIFICAR SaveBackupService: receber `WriteTextSafely`. MODIFICAR SaveManager e seu partial: chamar o serviço. MODIFICAR três consumidores de quest ID: usar a const existente. XML curto descreve responsabilidade/invariantes nos pontos alterados.

## 16. Contratos

```csharp
public static class InventorySlotOperations
{
    public static bool TrySplit(InventorySlot source, InventorySlot destination);
    public static bool TryMoveOrMerge(InventorySlot source, InventorySlot destination,
        int maxStack, out string failureReason);
}
// Método novo em classe existente:
public static void SaveBackupService.WriteTextSafely(string path, string contents);
```

As operações recusadas não mutam slots. SlotIndex não se move com o conteúdo. Move/merge/swap preservam soma por item. O adaptador valida catálogo antes de chamar o core e reconstrói aggregate antes de publicar eventos. Split preserva o comportamento legado de dividir pela metade inteira, inclusive a semântica atual de equipamento.

Save/event/UI contracts: nenhuma mudança de schema, payload, assinatura pública existente de gameplay ou string de erro da fachada. Escrita mantém `.tmp`, `.backup`, `File.Replace` e fallback por `PlatformNotSupportedException`. Não alterar recuperação de backup ou migrations.

## 17. Sistemas afetados

Inventory, filesystem de Save e referências de ID em NPC/Quest; sem novo wiring.

## 18. Arquivos permitidos

- `Assets/_Game/Scripts/Inventory/InventoryManager.cs`
- `Assets/_Game/Scripts/Inventory/InventorySlotOperations.cs` e seu `.meta`
- `Assets/_Game/Scripts/Save/SaveManager.cs`
- `Assets/_Game/Scripts/Save/SaveManager.Migration.cs`
- `Assets/_Game/Scripts/Save/Migrations/SaveBackupService.cs`
- `Assets/_Game/Scripts/NPC/NpcController.cs`
- `Assets/_Game/Scripts/NPC/NpcShopController.cs`
- `Assets/_Game/Scripts/Quests/Runtime/QuestGiverInteractable.cs`
- `Assets/_Game/Tests/EditMode/UI/InventorySlotMoveMergeTests.cs`
- `Assets/_Game/Tests/EditMode/UI/InventorySlotOperationsTests.cs` e seu `.meta`
- `Assets/_Game/Tests/EditMode/Save/SaveAtomicWriteTests.cs`
- Esta spec, seu execution report, audit e human test scenario em `docs/validation/`.

## 19. Arquivos proibidos

`.unity`, `.prefab`, `.asset`, `Packages/`, `ProjectSettings/`, `docs_old/`, cave procedural, farm e qualquer arquivo fora de §18. Preservar todos os edits preexistentes. Não usar git stash/clean/reset.

# /speckit.tasks

## 20. Estratégia por edição

1. Caracterizar eventos de swap/merge/recusa/split na fixture existente; testar escrita em subdiretório, substituições repetidas e falha ao escrever temporário.
2. Extrair split e mutação após validação de move para InventorySlotOperations, incluindo copy/swap. Manter chamadas de aggregate/eventos no mesmo ponto.
3. Mover corpo de WriteTextSafely para SaveBackupService; trocar os dois callers e atualizar testes da escrita para API direta. Recuperação permanece no adaptador.
4. Substituir literals de supply quest pela const existente nos três consumidores.
5. Rodar testes afetados, builds, suíte completa, ratchet, docs e strict; documentar diferenças para baseline e cenário humano.

## 21. Ordem segura

Baseline → testes de caracterização → extrações → testes afetados → auditoria → validações finais → evidência. Nenhuma promoção automática.

## 14. Critérios de aceite

- `InventorySlotOperationsTests` executa sem GameObject/SO: move preserva índices/binding, merge limita stack, recusa preserva conteúdo, split ímpar conserva total.
- `InventorySlotMoveMergeTests`: swap publica source e destination uma vez, aggregate já consistente; merge publica uma vez; recusa não publica; split publica delta zero.
- `SaveAtomicWriteTests`: conteúdo e backup anteriores preservados, temporário removido no sucesso, exceção propagada na falha, nenhum save real acessado.
- `rg 'quest_first_supplies_for_cindar'` nos três consumidores retorna zero ocorrências; valor segue definido por QuestRuntimeIds.
- Sete builds e teste afetado retornam exit 0; suíte completa não introduz falhas além das quatro identificadas no baseline. Gate global FAIL continua registrado como FAIL.
- `Test-ArchitectureRatchet.ps1` imprime `Architecture ratchet PASS: no tracked debt increased.` sem aumentar baseline; GUIDs/campos serializados preservados.

## 23. Edge cases / falhas

- Pilha cheia, fonte/destino inválidos/equipados: mesma recusa e nenhum evento.
- Split em inventário cheio ou quantidade <2: false, sem mutação.
- Swap move binding, nunca SlotIndex; merge mantém comportamento de metadados atual.
- Falha no temporário: arquivo principal e backup existentes intocados; erro não vira sucesso.
- Fallback sem File.Replace não é atomicidade garantida contra crash; preserva backup e semântica existente, risco explicitamente mantido.
- Mudança concorrente nos arquivos de farm: não corrigir/reverter neste slice; comparar os nomes das falhas.

## 22. Validação e gates

`tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`, `tools/unity/RunUnityEditModeTests.ps1`, `tools/architecture/Test-ArchitectureRatchet.ps1`, `tools/docs/validate_docs.ps1`, `tools/docs/run_strict_validation.ps1`.

Play Mode humano: `DEFERRED_TO_FINAL_VALIDATION`; executar scenario documentado, nunca reivindicar aprovação com compile ou teste unitário. Spec permanece em a_implementar enquanto gates/promoção estiverem pendentes.
