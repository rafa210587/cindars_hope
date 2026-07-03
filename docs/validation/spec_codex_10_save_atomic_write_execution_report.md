# Execution Report — spec_codex_10_save_atomic_write

> **Spec:** `.specs/a_implementar/spec_codex_10_save_atomic_write.md`
> **Status:** BUILD_VALIDATED (Phase 2-3 DEFERRED_TO_FINAL_HUMAN_VALIDATION; EditMode test run NOT RUN — Unity Editor already open)
> **Type:** Runtime / Reliability fix (save write path)
> **Date:** 2026-07-03

---

## Acceptance criteria extracted

Da se��o "14. Crit�rios de aceite" da spec:

| # | Critério | Evidência |
|---|---|---|
| 14.1 | Sem janela de perda na escrita � `WriteTextSafely` nunca deixa `path` ausente e sem `.backup` recuper�vel simultaneamente | `WriteTextSafely` (`SaveManager.Migration.cs`) reescrito: se `path` j� existe, usa `File.Replace(tempPath, path, backupPath, true)` � opera��o at�mica do SO que substitui o destino e move o conte�do antigo para `backupPath` em uma �nica chamada, sem instante intermedi�rio em que `path` esteja ausente. Se `path` n�o existe ainda, s� h� `File.Move(tempPath, path)` direto (nada a perder). Fallback manual (`PlatformNotSupportedException`) cria backup via `SaveBackupService.TryCreateBackup` **antes** de qualquer `Delete`, abortando com exce��o se o backup falhar. Testes: `WriteTextSafely_ExistingFile_ReplacesContentAndCreatesBackupOfOldContent`, `WriteTextSafely_ExistingFile_NeverLeavesPathMissing`, `WriteTextSafely_NoExistingFile_CreatesFileDirectly` |
| 14.2 | Recupera��o autom�tica no load � se `path` ausente/corrompido e `.backup` existe, o load restaura do backup e loga isso claramente | `TryRecoverFromBackupIfNeeded(path)` (novo, `SaveManager.Migration.cs`) chamado no in�cio de `SaveManager.LoadGame()` (`SaveManager.cs`). Verifica `IsReadableValidSave(path)`; se inv�lido/ausente e `.backup` v�lido existe, chama `SaveBackupService.TryRestoreBackup` e loga via `Debug.LogWarning` com o path e o motivo. Testes: `RecoverFromBackup_MainMissing_BackupValid_RestoresFromBackup`, `RecoverFromBackup_MainCorrupted_BackupValid_RestoresFromBackup` |
| 14.3 | Sem regress�o de fluxo normal � save/load sem falha simulada continua id�ntico; `dotnet build` PASS | Fluxo normal (`path` ausente, sem `.backup`) preservado: `TryRecoverFromBackupIfNeeded` retorna `false` sem tocar nada, `LoadGame()` segue exatamente o mesmo caminho de antes (`File.Exists` ? `false` ? `"Save file not found"`). Teste: `RecoverFromBackup_MainMissing_NoBackup_ReturnsFalse_NoRegression`, `RecoverFromBackup_MainValid_DoesNotTouchBackupOrOverwrite`. `dotnet build` Assembly-CSharp e Assembly-CSharp-Editor: PASS, exit 0 (ver bloco de valida��o) |

---

## Existing systems audit (Phase 0)

- **Call sites de `WriteTextSafely` confirmados via Grep (2 total):** `SaveManager.Migration.cs:157` (dentro de `TryReadSaveWithMigration`, write-back p�s-migra��o) e `SaveManager.cs:313` (dentro de `SaveGame()`, save regular). Ambos continuam chamando a mesma assinatura `private static void WriteTextSafely(string path, string contents)` � nenhuma mudan�a de contrato p�blico.
- **Load principal identificado:** `SaveManager.LoadGame()` (`SaveManager.cs:331`) � o ponto que hoje faz `if (!File.Exists(savePath)) { ... return false; }` **antes** de qualquer tentativa de leitura/parse � este � o gate que precisava do hook de recupera��o. Como esse m�todo vive em `SaveManager.cs` (n�o em `SaveManager.Migration.cs`, que era o �nico arquivo no repo lock scope original), a Fase 0 confirmou a necessidade de expandir o escopo de arquivos permitidos, conforme a spec j� previa na se��o 19 ("confirmar em Fase 0 e, se sim, expandir arquivos permitidos com justificativa no relat�rio"). Justificativa: sem esse hook em `LoadGame()`, a recupera��o de backup nunca seria acionada no fluxo real do jogo � ela ficaria implementada mas morta.
- **`SaveBackupService` (`Assets/_Game/Scripts/Save/Migrations/SaveBackupService.cs`) n�o foi modificado.** `TryCreateBackup`/`TryRestoreBackup`/`TryDeleteBackup` foram reusados como est�o. Confirmado achado da spec (se��o 9): `TryRestoreBackup` tamb�m tem a mesma classe de risco (`File.Delete(originalPath)` antes de `File.Copy`) � n�o corrigido aqui (fora do repo lock scope original, e a spec explicitamente instruiu n�o expandir sem necessidade estrita comprovada). Registrado como d�bito na se��o "Notas para execu��o posterior" abaixo, conforme instru�do pela pr�pria spec (YAGNI).
- **`File.Replace(string, string, string, bool)` confirmado dispon�vel** � � API `System.IO` da BCL, presente e utiliz�vel em builds Mono/IL2CPP do Unity (n�o h� bloqueio de plataforma documentado neste projeto; nenhuma refer�ncia pr�via a `PlatformNotSupportedException` para esta API foi encontrada em uso real). Adotado como caminho prim�rio por ser a solu��o mais alinhada � escada de minimalismo (degrau 3 � BCL nativa) e por ser at�mica em uma �nica chamada de SO, eliminando completamente a janela de risco sem orquestra��o manual de Copy+Delete+Move. O catch de `PlatformNotSupportedException` cobre o cen�rio defensivo caso um ambiente de build espec�fico n�o suporte a API (ex.: alguns sistemas de arquivo de rede) � nesse caso, cai no fallback manual seguro descrito no crit�rio 14.1.
- **Nenhum sistema paralelo criado.** Reuso de `SaveBackupService` existente; nenhum novo formato de nome de backup (mantido `.backup`, j� usado por `SaveBackupService`); nenhuma segunda implementa��o de "criar/restaurar backup".
- **Decis�o de abstra��o de teste (se��o "Escopo", item 3 da spec):** optou-se por **n�o** criar um wrapper/interface de `System.IO` para tornar a l�gica test�vel sem I/O real. Os testes usam arquivos reais em um diret�rio tempor�rio isolado (`Path.GetTempPath()/cindars_hope_save_atomic_write_tests_<guid>`, criado em `[SetUp]` e removido em `[TearDown]`), o que a pr�pria spec j� autorizava como alternativa aceit�vel ("se o teste s� puder ser feito com arquivos reais em um diret�rio temp de teste, isso tamb�m � aceit�vel e mais simples" � YAGNI). Nenhum save real do usu�rio � tocado em nenhum momento.
- **Acesso a m�todos privados nos testes:** `WriteTextSafely` e `TryRecoverFromBackupIfNeeded` s�o `private static` em `SaveManager` (partial class). Os testes os invocam via reflection (`MethodInfo.Invoke`, `BindingFlags.NonPublic | BindingFlags.Static`) em vez de expor uma nova API p�blica apenas para teste � segue o princ�pio de minimalismo (n�o abrir superf�cie de API sem necessidade de produ��o) e n�o colide com nenhum contrato externo.

---

## Spec Compliance Matrix

| Requirement (spec) | Implementation | Status |
|---|---|---|
| §11 Escopo — reescrever `WriteTextSafely` sem janela de perda, preferindo `File.Replace` | Implementado com `File.Replace(tempPath, path, backupPath, true)` como caminho primário quando `path` existe | DONE |
| �11 Escopo � fallback manual seguro via `SaveBackupService` se `File.Replace` n�o aplic�vel | Implementado: catch de `PlatformNotSupportedException` cria backup via `SaveBackupService.TryCreateBackup` antes de `Delete`+`Move`; aborta com `IOException` se o backup falhar | DONE |
| �11 Escopo � extrair decis�o de sequ�ncia como l�gica test�vel em EditMode, avaliando viabilidade sem superengenharia | Avaliado: optou-se por testes com arquivos reais em diret�rio temp (mais simples, sem abstra��o de I/O) � decis�o documentada acima | DONE (via decis�o YAGNI documentada) |
| �11 Escopo � caminho de recupera��o no load a partir de `.backup` | `TryRecoverFromBackupIfNeeded` + hook em `LoadGame()` | DONE |
| �11 Escopo � EditMode tests (escrita segura, recupera��o, fluxo normal) | 7 testes em `SaveAtomicWriteTests.cs` cobrindo os 3 cen�rios + 2 variantes extras (backup tamb�m corrompido; save principal j� v�lido n�o deve disparar recupera��o) | DONE |
| �12 Fora de escopo � n�o mudar schema de save DTOs | Nenhum DTO alterado | RESPECTED |
| �12 Fora de escopo � n�o mudar fluxo de migra��o existente | `TryReadSaveWithMigration` inalterado em sua l�gica; s� chama a mesma `WriteTextSafely` (agora mais segura) e `SaveBackupService` como antes | RESPECTED |
| �12 Fora de escopo � n�o introduzir rolling/multi-backup | Apenas um `.backup`, como j� era o padr�o de `SaveBackupService` | RESPECTED |
| �13 Regras de n�o duplica��o | Nenhuma segunda implementa��o de backup; sufixo `.backup` reusado | RESPECTED |

---

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 1 (QUALITY_CHECK_FAILURE — pré-existente, ver nota abaixo)
Assembly-CSharp: PASS (0 erros; 5 warnings pr�-existentes n�o relacionados � CombatTelemetrySession, EnemySkinCatalog)
Assembly-CSharp-Editor: PASS (0 erros; 7 warnings pr�-existentes n�o relacionados)
Diff completeness check: PASS
Quality check: FAIL � mas a �NICA causa � "Forbidden files altered" sobre 36 assets `Enemies/Roster/*.asset` + 3 cenas (.unity) que j� estavam modificados no working tree ANTES desta sess�o (confirmado no git status inicial fornecido pelo orquestrador). Nenhum arquivo desta spec (SaveManager.Migration.cs, SaveManager.cs, SaveAtomicWriteTests.cs) aparece na lista de forbidden files. As demais entradas do quality check (report sections missing em reports antigos como fable_28, WAVE_INTEGRATION_24, 05_spec_*, 09_spec_*) s�o WARN, n�o FAIL, e s�o as legacies conhecidas listadas explicitamente pelo orquestrador.
Docs validation: EXPECTED_FAIL_LEGACY_ONLY (spec_npc_physics_cat_companion sem markers speckit; placeholders em tools/codex/Generate-CodexHarness.ps1) — legacies conhecidas citadas pelo orquestrador
Unity Test Runner EditMode: NOT RUN � 8 processos Unity Editor j� em execu��o no ambiente (confirmado via Get-Process); rule unity-assets.md pro�be lan�ar batchmode paralelo enquanto Unity est� aberto. Residual risk: os 7 EditMode tests novos (SaveAtomicWriteTests.cs) compilam com sucesso (confirmado via dotnet build da Assembly de testes atrav�s do Assembly-CSharp-Editor, que inclui o diret�rio Tests/EditMode) mas n�o foram executados pelo Test Runner nesta sess�o. Humano deve rodar via /run-editmode-tests ou Unity Test Runner antes de considerar a spec ACCEPTED.
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json (n�o gerado pelo script nesta vers�o; script n�o grava artifact JSON � apenas stdout)
```

---

## Honest status rationale

- Compile-level (Assembly-CSharp + Assembly-CSharp-Editor): PASS, exit 0, confirmado com `$LASTEXITCODE` checado explicitamente após cada `dotnet build`.
- Quality check FAIL � 100% atribu�vel a estado pr�-existente do working tree (assets de inimigos + 3 cenas j� modificados antes desta sess�o, conforme o git status compartilhado pelo orquestrador no in�cio da tarefa) � n�o introduzido por esta spec. Nenhum arquivo do meu diff aparece na lista de "Forbidden files altered".
- Docs validation FAIL é composto só pelas legacies explicitamente citadas como conhecidas pelo orquestrador (spec_npc_physics_cat_companion, placeholders tools/codex/).
- EditMode test execution real (Unity Test Runner) n�o foi poss�vel nesta sess�o porque o Unity Editor j� estava aberto (m�ltiplos processos) � rodar em paralelo violaria a rule `unity-assets.md`. Isso � reportado como `NOT RUN` com motivo, n�o como PASS.
- Play Mode / cen�rio humano final (save normal + save com arquivo principal deletado manualmente + `.backup` presente) est� `DEFERRED_TO_FINAL_VALIDATION` conforme a pr�pria spec j� definia na se��o 30 (Testing Quality Gate).
- Status: `BUILD_VALIDATED` � n�o `ACCEPTED`, pois falta a evid�ncia de EditMode Test Runner real e o cen�rio Play Mode humano, ambos exigidos pela se��o 30 da spec para `ACCEPTED`.

---

## O que NAO foi feito

- `SaveBackupService.TryRestoreBackup` **n�o foi corrigido** apesar de ter a mesma classe de risco (delete antes de copy) � est� fora do repo lock scope original e a spec instruiu explicitamente n�o expandir sem necessidade estrita comprovada (YAGNI, se��o 33 "Notas para execu��o posterior"). Registrado como d�bito para spec futura dedicada.
- **Unity Test Runner EditMode n�o foi executado** (Unity Editor j� aberto em 8 processos). Apenas a compila��o dos testes foi confirmada via `dotnet build`.
- **Play Mode / cen�rio humano final n�o executado** � deferred conforme a pr�pria spec definia (`Human validation timing: DEFERRED_TO_FINAL_VALIDATION`).
- Nenhuma migration de save foi criada (fora de escopo, confirmado — nenhuma mudança de DTO).
- Nenhum versionamento de múltiplos backups (rolling) foi adicionado (fora de escopo, confirmado).

---

## Rollback

Conforme se��o 27 da spec:
- Reverter `WriteTextSafely` para a vers�o original (Delete-then-Move) em `SaveManager.Migration.cs`.
- Remover a chamada a `TryRecoverFromBackupIfNeeded` em `SaveManager.LoadGame()` (`SaveManager.cs`).
- Remover `Assets/_Game/Tests/EditMode/Save/SaveAtomicWriteTests.cs`.

---

## Arquivos alterados

```text
Assets/_Game/Scripts/Save/SaveManager.Migration.cs   (WriteTextSafely reescrito + TryRecoverFromBackupIfNeeded + IsReadableValidSave novos, private static)
Assets/_Game/Scripts/Save/SaveManager.cs             (LoadGame() chama TryRecoverFromBackupIfNeeded no in�cio � expans�o de escopo justificada acima)
Assets/_Game/Tests/EditMode/Save/SaveAtomicWriteTests.cs (novo — 7 testes)
docs/validation/spec_codex_10_save_atomic_write_execution_report.md (este arquivo)
```
