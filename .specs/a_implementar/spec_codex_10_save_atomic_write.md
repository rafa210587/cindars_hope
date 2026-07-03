# SPEC — Escrita Atômica de Save (WriteTextSafely sem Janela de Perda)

> **Spec ID:** `spec_codex_10_save_atomic_write`
> **Status:** A implementar
> **Wave:** WAVE CODEX CONVERGENCE — Honestidade de Validação (Lote 2)
> **Priority:** P0
> **Type:** Runtime
> **Domain:** Save / Reliability
> **Parallelizable:** YES
> **Parallel group:** codex_convergence_lote2
> **Can run with:** spec_codex_09, spec_codex_11, spec_codex_12, spec_codex_13
> **Must not run with:** N/A
> **Repo lock scope:** `Assets/_Game/Scripts/Save/SaveManager.Migration.cs`
> **Depends on (Depende de):**
> - Nenhuma spec deste lote. Depende apenas de `Assets/_Game/Scripts/Save/Migrations/SaveBackupService.cs` (já existente, usado hoje só no fluxo de migração).
> **Blocks (Bloqueia):**
> - Nenhuma outra spec do lote `codex_convergence_lote2`.
> **Scope:** Tornar `WriteTextSafely` (`SaveManager.Migration.cs` L305-327) resiliente a crash entre a deleção do save antigo e o move do `.tmp` — eliminando a janela onde um crash perde o save. Adicionar recuperação: se o save principal estiver ausente/corrompido no load e existir um `.backup`, restaurar a partir dele.
> **Out of scope:** Mudar schema de save DTOs; mudar o fluxo de migração existente que já usa `SaveBackupService`; introduzir versionamento de múltiplos backups (rolling); mudar o formato de serialização (JSON continua JSON).

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

Achado verificado: `Assets/_Game/Scripts/Save/SaveManager.Migration.cs` L305-327, método `WriteTextSafely`:

```csharp
private static void WriteTextSafely(string path, string contents)
{
    var directory = Path.GetDirectoryName(path);
    if (!Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
    }

    var tempPath = $"{path}.tmp";
    File.WriteAllText(tempPath, contents);

    if (!File.Exists(tempPath))
    {
        throw new IOException($"Temporary save file was not written: {tempPath}");
    }

    if (File.Exists(path))
    {
        File.Delete(path);   // <-- L323: se o processo morrer AQUI, o save original já foi apagado
    }

    File.Move(tempPath, path); // <-- L326: e o .tmp ainda não virou o arquivo final
}
```

Entre `File.Delete(path)` (L323) e `File.Move(tempPath, path)` (L326) existe uma janela onde, se o processo crashar, o jogo perdeu o crash lock, ou o disco falhar, o save original já foi deletado e o novo ainda não existe como `path` — resultado: save perdido.

O projeto já tem `SaveBackupService` (`Assets/_Game/Scripts/Save/Migrations/SaveBackupService.cs`) com `TryCreateBackup`, `TryRestoreBackup`, `TryDeleteBackup` — hoje usado **apenas no fluxo de migração**, não no save regular via `WriteTextSafely`.

## 6. Problema

Um crash, falha de disco, ou kill do processo exatamente entre L323 e L326 apaga o save do jogador sem deixar nenhum arquivo válido em `path`. Isso é uma perda de dados silenciosa e irreversível para o jogador, sem qualquer mecanismo de recuperação hoje.

## 7. Objetivo

Ao final desta spec, `WriteTextSafely` nunca deixa `path` inexistente entre a escrita do novo conteúdo e sua efetivação: usa `File.Replace` (atômico no NTFS, cria backup do arquivo antigo como parte da própria operação) quando o arquivo de destino já existe, ou reusa `SaveBackupService` para mover o original para `.backup` **antes** de mover o `.tmp` para `path` — nunca `Delete` antes de garantir que o novo conteúdo já está seguro em outro lugar. Adiciona também um caminho de recuperação no load: se `path` não existe ou falha ao parsear e existe um `.backup`, restaurar a partir dele.

## 8. Fontes obrigatórias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.claude/skills/save-load-pattern/SKILL.md
.claude/rules/unity-architecture.md (seção save DTOs)
.claude/rules/security-and-files.md (arquivos sensíveis / saves reais)
```

## 9. Estado atual do repo (Phase 0 — auditado nesta sessão)

- `WriteTextSafely` é `private static void` em `SaveManager.Migration.cs`, chamado internamente pelo fluxo de save/migração (Fase 0 de execução deve confirmar todos os call sites via Grep antes de mudar assinatura).
- `SaveBackupService.TryCreateBackup(originalPath, out backupPath, out errorMessage)`: copia (não move) `originalPath` para `originalPath + ".backup"`, sobrescrevendo backup anterior se existir. Retorna `bool`.
- `SaveBackupService.TryRestoreBackup(originalPath, backupPath, out errorMessage)`: deleta `originalPath` se existir e copia `backupPath` para `originalPath`. Nota: este método também tem a mesma classe de risco (delete antes de copy) — não é escopo desta spec corrigir `SaveBackupService` em si (fora do repo lock scope), mas se a Fase 0 de execução decidir que é trivial e necessário, documentar a decisão e, se tocar, ficar restrito ao mínimo (o arquivo não está no repo lock scope declarado — se precisar editá-lo, expandir o repo lock scope explicitamente no relatório e confirmar que não colide com outra spec paralela).
- `SaveBackupService.TryDeleteBackup`: remove o `.backup` se existir; usado hoje ao final de uma migração bem-sucedida (Fase 0 de execução deve confirmar o call site exato via Grep).
- `File.Replace(sourceFileName, destinationFileName, destinationBackupFileName)` é a API .NET nativa para "substituir atomically com backup automático do destino" — mais alinhado à escada de minimalismo (degrau 3: BCL) do que orquestrar manualmente `Copy`+`Delete`+`Move`. Fase 0 deve confirmar se `File.Replace` está disponível no runtime alvo (Mono/IL2CPP do Unity usado pelo projeto) antes de adotá-lo como caminho primário; se não disponível/confiável multiplataforma, usar o padrão manual: `SaveBackupService.TryCreateBackup` (copiar original para `.backup`) **antes** de qualquer delete, depois `File.Move(tempPath, path)` (que já falha atomically se o destino não puder ser sobrescrito diretamente — Fase 0 deve confirmar comportamento de `File.Move` quando destino já existe: em .NET, `File.Move` lança se destino existir, então a ordem correta é: copiar backup → deletar original → mover tmp; ou usar `File.Replace` que faz isso em uma chamada atômica).

## 10. User stories / engineering stories

```text
Como jogador, quero que um crash durante o salvamento nunca apague meu save sem deixar um caminho de recuperação.
Como sistema de save, quero que, se o arquivo principal estiver ausente/corrompido no load, eu tente recuperar automaticamente de um .backup antes de tratar como "sem save".
```

## 11. Escopo

Inclui:
- Reescrever `WriteTextSafely` para eliminar a janela de perda: preferir `File.Replace(tempPath, path, backupPath)` quando `File.Exists(path)` (atômico, com backup automático do .NET); fallback (quando `path` não existe ainda, ou `File.Replace` não aplicável) para `File.Move(tempPath, path)` direto (não há arquivo antigo para perder).
- Se `File.Replace` não estiver disponível/confiável no ambiente de build alvo (confirmar na Fase 0), usar o padrão manual seguro: `SaveBackupService.TryCreateBackup(path, ...)` **antes** de `File.Delete(path)`; só prosseguir com o delete+move se o backup foi criado com sucesso; se o backup falhar, abortar a escrita (lançar exceção, não perder o original).
- Extrair a decisão "que sequência de operações executar" como lógica testável em EditMode, se praticável (ex.: uma função pura que recebe flags booleanas de estado de arquivo e retorna a sequência de ações — sem depender de `System.IO` real, usando um wrapper/interface mínimo se necessário; avaliar viabilidade sem superengenharia — YAGNI: se o teste só puder ser feito com arquivos reais em um diretório temp de teste, isso também é aceitável e mais simples).
- Adicionar caminho de recuperação no load: se `File.Exists(path)` for `false` OU a leitura/parse de `path` falhar, e existir `path + ".backup"`, tentar `SaveBackupService.TryRestoreBackup` antes de tratar como "sem save"/erro fatal. Logar claramente qual caminho foi tomado (novo save vs. recuperado de backup).
- EditMode tests cobrindo: (a) escrita normal com arquivo antigo existente não perde dados em nenhum ponto simulável; (b) load com `path` ausente e `.backup` presente recupera com sucesso; (c) load sem `path` nem `.backup` segue o comportamento atual (sem save = novo jogo, sem regressão).

Fora:
- Mudar schema de save DTOs ou o formato de serialização.
- Rolling/múltiplos backups versionados.
- Alterar o fluxo de migração existente que já usa `SaveBackupService` (esse fluxo já é correto; esta spec estende o USO de `SaveBackupService` para o save regular, não o reescreve).

## 12. Fora de escopo

```text
Não inclui: novo schema de save; UI de recuperação de save; multi-backup versionado; mudança de formato de serialização.
```

## 13. Regras de não duplicação

```text
Não escrever uma segunda implementação de "criar/restaurar backup" — reusar SaveBackupService existente.
Não inventar um novo formato de nome de arquivo de backup — reusar o sufixo ".backup" já usado por SaveBackupService.
```

## 14. Critérios de aceite

### 14.1 Sem janela de perda na escrita

- `WriteTextSafely` nunca deixa `path` ausente e sem um `.backup` recuperável simultaneamente, em nenhum ponto da sequência de operações (verificável por leitura de código + teste que simula interrupção entre passos, se praticável).
- Evidência esperada: leitura do código + teste.

### 14.2 Recuperação automática no load

- Se `path` está ausente ou corrompido e `.backup` existe, o load restaura do backup e loga isso claramente.
- Evidência esperada: EditMode test com arquivos reais em diretório temporário.

### 14.3 Sem regressão de fluxo normal

- Save/load sem nenhuma falha simulada continua funcionando exatamente como antes (round-trip idêntico).
- `dotnet build` PASS.

---

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/Save/
  SaveManager.Migration.cs   (WriteTextSafely reescrito + novo caminho de recuperação no load)

Assets/_Game/Tests/EditMode/Save/
  SaveAtomicWriteTests.cs (novo)
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
Nenhum novo — nenhuma mudança de DTO.

### 16.2 Runtime contracts
`WriteTextSafely(string path, string contents)` mantém a mesma assinatura (é `private static`, sem contrato público a preservar além do comportamento observável: arquivo final em `path` após retorno bem-sucedido).

### 16.3 Event contracts
N/A — nenhum evento novo.

### 16.4 Save contracts
Nenhuma mudança de schema. O `.backup` é um artefato de arquivo, não uma seção de save.

### 16.5 UI contracts
N/A.

## 17. Sistemas afetados

```text
Save (write path, load recovery path)
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Save/SaveManager.Migration.cs
Assets/_Game/Tests/EditMode/Save/**
docs/validation/**
```

## 19. Arquivos proibidos

```text
Assets/_Game/Scripts/Save/Migrations/SaveBackupService.cs (leitura ok; edição só se Fase 0 confirmar necessidade estrita e o relatório documentar a expansão de escopo)
Assets/_Game/Scripts/Save/SaveManager.cs (outras partes do save manager fora de Migration.cs, a menos que o load principal viva lá e precise do hook de recuperação — confirmar em Fase 0 e, se sim, expandir arquivos permitidos com justificativa no relatório)
Assets/**/*.unity, *.prefab, *.asset
```

## 20. Estratégia de implementação

```md
### Fase 0 — Confirmar call sites de WriteTextSafely e do load principal; confirmar disponibilidade de File.Replace no runtime alvo
### Fase 1 — Reescrever WriteTextSafely sem janela de perda
### Fase 2 — Adicionar caminho de recuperação de .backup no load
### Fase 3 — EditMode tests (escrita segura + recuperação + fluxo normal sem regressão)
### Fase 4 — dotnet build + relatório
```

## 21. Ordem de execucao (ordem segura)

```text
1. Grep de todos os call sites de WriteTextSafely e do método de load principal do save.
2. Confirmar viabilidade de File.Replace ou definir fallback manual seguro com SaveBackupService.
3. Reescrever WriteTextSafely.
4. Adicionar recuperação de backup no load.
5. Escrever EditMode tests com arquivos reais em diretório temporário.
6. dotnet build + EditMode tests.
7. Registrar relatório.
```

## 22. Paralelização

```md
- Parallelizable: YES
- Parallel group: codex_convergence_lote2
- Can run with: demais specs deste lote (arquivos não se sobrepõem)
- Must not run with: N/A
- Shared files/systems that require lock: Save/SaveManager.Migration.cs (lock local)
- Reason: escopo isolado a um método privado + um novo caminho de load; único ponto de contato externo é SaveBackupService já estável
```

## 23. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? N/A
```

## 24. Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO
```

## 25. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES (save/load é gameplay-crítico; cenário humano deve confirmar save/load normal sem regressão)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos

```text
Risco: File.Replace pode não estar disponível ou ter comportamento diferente sob IL2CPP/Mono do Unity.
Mitigação: Fase 0 confirma via teste isolado ou documentação; fallback manual com SaveBackupService se necessário.

Risco: tocar em SaveManager.Migration.cs pode ter efeitos colaterais em outros call sites não mapeados.
Mitigação: Grep exaustivo de call sites na Fase 0 antes de qualquer edição; não mudar assinatura pública.
```

## 27. Rollback

```text
Reverter WriteTextSafely para a versão original (Delete-then-Move).
Remover o caminho de recuperação de backup no load.
Remover testes novos.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Grep de call sites de WriteTextSafely e do load principal; confirmar disponibilidade de File.Replace.
- [ ] T002 — Reescrever WriteTextSafely sem janela de perda (File.Replace ou fallback com SaveBackupService).
- [ ] T003 — Adicionar caminho de recuperação de .backup no load (path ausente ou parse falho).
- [ ] T004 — Escrever EditMode tests (escrita segura, recuperação, fluxo normal sem regressão).
- [ ] T005 — dotnet build + EditMode tests; gerar execution report.
```

## 29. Validações obrigatórias

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\validate_docs.ps1
```

Unity Test Runner — EditMode: rodar se praticável; senão `NOT RUN` com motivo.

## 30. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: YES (decisão de sequência de operações de escrita/recuperação)
- Requires EditMode tests: YES (escrita segura + recuperação de backup + fluxo normal)
- Requires PlayMode automated or final human scenario: YES (save/load real em Play Mode, incluindo cenário de save corrompido manualmente para validar recuperação)
- Requires regression test: YES (fluxo de save/load normal não pode regredir)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: dotnet build PASS + EditMode tests PASS + cenário de Play Mode documentado (save normal, save com arquivo principal deletado manualmente + .backup presente confirma recuperação).
```

## 31. Definition of Done

```text
WriteTextSafely não tem mais janela de perda entre delete e move.
Load recupera automaticamente de .backup quando o save principal está ausente/corrompido.
EditMode tests cobrindo os 3 cenários (escrita segura, recuperação, fluxo normal) criados e passando.
Execution report documenta a decisão tomada (File.Replace vs. fallback manual) e por quê.
```

## 32. Anti-regressão

```text
Não mudar schema de save DTOs.
Não mudar o fluxo de migração existente que já usa SaveBackupService.
Não remover o comportamento de "sem save = novo jogo" quando nem path nem .backup existem.
```

## 33. Notas para execução posterior

```text
Se a Fase 0 revelar que SaveBackupService.TryRestoreBackup também tem uma janela de risco (delete antes de copy), registrar como débito para uma spec futura dedicada — não expandir esta spec para reescrever SaveBackupService inteiro sem necessidade comprovada (YAGNI).
```
