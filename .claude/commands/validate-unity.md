# /validate-unity

Use quando a tarefa altera runtime C#, scenes, prefabs, assets ou ProjectSettings.

## Quando usar

- Depois de qualquer mudança em runtime code
- Depois de modificar scenes ou prefabs
- Depois de mudar asset settings (sprites, scriptable objects, etc.)
- Como parte do fechamento da tarefa

## Procedimento

### Step 1: Docs Validation (sempre)

```powershell
.\tools\docs\validate_docs.ps1
```

**Resultado esperado:** PASS ou lista de issues específicos de docs a corrigir.

### Step 2: Unity Compile Validation (se runtime mudou)

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -ProjectPath "." `
  -LogFile ".\Logs\unity-compile-validation.log"
```

**Resultado esperado:**
- PASS: "Tundra build success"
- FAIL: lista de compilation errors com referências file:line

**Se o path do Unity diferir ou for desconhecido:**
- Verifique o environment: `$env:UNITY_EDITOR_PATH`
- Verifique as versões instaladas: `ls "C:\Program Files\Unity\Hub\Editor\"`
- Registre em `.claude/settings.local.json` para execuções futuras

### Step 3: Log Scanner (se o Step 2 rodou)

```powershell
.\tools\unity\ScanUnityLogs.ps1 `
  -LogFile ".\Logs\unity-compile-validation.log"
```

**Resultado esperado:**
- PASS: nenhum novo C# error
- WARNING: Assembly warnings preexistentes (aceitável)
- FAIL: novos erros introduzidos por esta tarefa

## Tratamento de falha

Se o Step 2 não puder rodar por causa de:
- Sandbox environment
- Instalação do Unity ausente
- Problema de permissões
- Timeout

**Documentação obrigatória no fechamento da tarefa:**

```text
Unity validation: NOT RUN
Reason: <motivo específico>
Command attempted: <comando que falhou>
Residual risk: Unity compile not validated locally
```

**NÃO declare runtime como validado sem evidência.**

## Critérios de sucesso

- ✅ Docs validation: PASS
- ✅ Unity compile: PASS (se executado)
- ✅ Log scan: nenhum erro novo (se executado)
- ✅ Se não executado: motivo e risco claros registrados

## Não faça

- Pular a validação e alegar que "provavelmente funciona"
- Ignorar a docs validation mesmo que runtime não tenha mudado
- Esconder falhas de validação no resumo
- Avançar para o fechamento se houver issue bloqueante de validação

---

**Próximo:** Se a validação passar, siga para `/finish-spec` para o fechamento da tarefa.
