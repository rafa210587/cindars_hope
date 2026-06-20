---
name: unity-validation
description: Valida compile do Unity, logs, scenes, prefabs e residual risks. Use quando a tarefa altera runtime C#, scenes, prefabs, assets ou ProjectSettings.
---

# Skill: Validação do Unity

Use quando a tarefa altera runtime C#, scenes, prefabs, assets ou ProjectSettings.

## Quando validar

- **Sempre:** Se qualquer arquivo `.cs` mudou
- **Sempre:** Se qualquer scene ou prefab foi modificado
- **Sempre:** Se qualquer asset setting mudou (sprite import, scriptable object, etc.)
- **Sempre:** Se qualquer ProjectSettings foi alterado
- **Opcional:** Para tarefas puramente de documentação (pule, a menos que integrada a uma tarefa de runtime)

## Passos de validação

### Passo 1: Docs Validation (obrigatório para todas as tarefas)

```powershell
.\tools\docs\validate_docs.ps1
```

**Resultados esperados:**
- ✅ PASS: Todos os docs consistentes
- ⚠️ WARNING: Problemas menores (preexistentes, aceitáveis)
- ❌ FAIL: Problemas de docs que quebram (precisam ser corrigidos)

**Ação:**
- Se PASS ou WARNING: Continue
- Se FAIL: Corrija os docs, rode de novo, depois continue

### Passo 2: Unity Compile Validation (se tarefa de runtime)

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -ProjectPath "." `
  -LogFile ".\Logs\unity-compile-validation.log"
```

**Resultados esperados:**
- ✅ PASS: "Tundra build success" no log
- ⚠️ WARNING: Warnings de assembly (preexistentes, aceitáveis)
- ❌ FAIL: Novos erros de C# (precisam ser corrigidos)

**Se o path do Unity for desconhecido:**
- Cheque: `$env:UNITY_EDITOR_PATH`
- Ou liste: `ls "C:\Program Files\Unity\Hub\Editor\"`
- Ou busque: `Get-ChildItem -Path "C:\Program Files\Unity" -Recurse -Filter "Unity.exe" | Select-Object -First 3`
- Registre em `.claude/settings.local.json` para execuções futuras

**Se não conseguir rodar:**
- Documente: `Reason: [sandbox|permissions|timeout|missing]`
- Registre no closeout como NOT RUN com residual risk

### Passo 3: Log Scanner (se o Passo 2 teve sucesso)

```powershell
.\tools\unity\ScanUnityLogs.ps1 `
  -LogFile ".\Logs\unity-compile-validation.log"
```

**Resultados esperados:**
- ✅ PASS: Nenhum novo erro de C# introduzido
- ⚠️ WARNING: Warnings preexistentes de Assembly firstpass (aceitáveis)
- ❌ FAIL: Novos erros encontrados (precisam ser corrigidos)

**Ação:**
- Se PASS ou WARNING: Validação completa
- Se FAIL: Identifique os erros, corrija no código, rode de novo Passo 2 e 3

## Formato do relatório de validação

```text
Validation Summary
─────────────────

Docs validation: ✅ PASS
  - No issues found

Unity compile:   ✅ PASS
  - Tundra build success
  - Log: .\Logs\unity-compile-validation.log

Log scan:        ✅ PASS
  - No new C# errors
  - Preexisting Assembly warnings noted

Overall: ✅ READY FOR CLOSEOUT
```

## Tratamento de erros

### Erros de compilação

Se o Passo 2 mostra erros de compilação:

1. **Leia a mensagem de erro:** `file.cs:line: error CS####: message`
2. **Categorize o erro:**
   - **Missing using directive** → Adicione ao arquivo
   - **Type mismatch** → Corrija o tipo ou faça cast
   - **Missing method** → Implemente ou use o nome de método correto
   - **Namespace conflict** → Resolva o naming
3. **Corrija no código**
4. **Rode a validação de novo**

### Validação não consegue rodar

Se o script de validação não consegue executar por causa do ambiente:

**Documente no closeout:**

```text
Unity validation: NOT RUN
Reason: [sandbox environment | missing Unity editor | timeout | permissions]
Command attempted: .\tools\unity\RunUnityCompileValidation.ps1
Residual risk: Unity compile not validated locally. Features may break on manual build.
Mitigation: User must validate in Unity Editor before production build.
```

## Regras

- [ ] NÃO pule a docs validation mesmo que só o código tenha mudado
- [ ] NÃO declare "compile success" sem rodar o Passo 2
- [ ] NÃO ignore a saída do Passo 3 se houver erros listados
- [ ] NÃO corrija erros silenciosamente sem rodar a validação de novo
- [ ] NÃO esconda falhas de validação no relatório final
- [ ] NÃO declare validado sem evidência (arquivo de log)

## Critérios de sucesso

- ✅ Docs validation: PASS ou WARNING preexistente
- ✅ Unity compile: PASS ou NOT RUN com reason documentada
- ✅ Log scan: PASS ou NOT RUN com reason documentada
- ✅ Todas as falhas corrigidas antes do closeout
- ✅ Evidência (arquivos de log) preservada para auditoria

## Relacionados

- **Spec Execution** → Chama esta skill se for tarefa de runtime
- **Implementation Closeout** → Exige os resultados de validação
- **Non-Regression Review** → Audita o diff separadamente (não substitui esta)

**Próximo:** Se a validação passa, siga para `/finish-spec` para o closeout da tarefa.
