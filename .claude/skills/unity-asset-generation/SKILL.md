---
name: unity-asset-generation
description: Roda generators de assets do Unity editor, verifica os assets gerados e documenta generation bloqueada. Use quando uma spec exige assets de ScriptableObject, scene, prefab ou registry gerados por editor tools do Unity.
---

# Skill: Geração de Assets do Unity

Use esta skill quando uma spec exige assets de ScriptableObject, scene, prefab ou registry gerados por editor tools do Unity.

## Antes de rodar

1. Identifique o menu exato ou o alvo de `-executeMethod`.
2. Verifique se o método é `public static`.
3. Verifique as output folders.
4. Cheque se o Unity já está aberto para o mesmo projeto.
5. Prefira execuções sequenciais do Unity; não rode múltiplos generators em batchmode em paralelo para o mesmo projeto.

## Comando padrão

Resolve the Editor matching `ProjectSettings/ProjectVersion.txt` and the absolute project path
before running. The variables below must contain observed paths, not a copied version default.
If the same project is open, use its authorized Editor command when available through
[unity-mcp-operations](../unity-mcp-operations/SKILL.md); do not launch a competing batch instance.
Inspect command side effects and output ownership before invoking broad generators.

```powershell
& $unityExe `
  -batchmode `
  -quit `
  -projectPath $projectPath `
  -executeMethod <Namespace.Type.Method> `
  -logFile Logs\<spec>_<generator>.log
```

## Depois de cada generator

Inspecione o log:

```powershell
Select-String -Path Logs\<spec>_<generator>.log `
  -Pattern "error CS|Exception|Fatal|created|updated|complete|PASSED|FAILED" `
  -CaseSensitive:$false
```

Conte os output assets:

```powershell
Get-ChildItem -Path "<asset-folder>" -Filter "*.asset" -Recurse |
  Measure-Object |
  Select-Object -ExpandProperty Count
```

## Documentação obrigatória

Registre em `docs/validation/<SPEC>_<date>.md`:

- Método de generator tentado.
- Arquivo de log.
- Exit code.
- Assets esperados.
- Assets encontrados.
- Divergência do nome de menu pedido, se houver.
- Se a generation foi bloqueada por Unity lock, license, timeout, sandbox ou approval.

## Tratamento de falha

Se o Unity diz que outra instância está aberta:

```text
Unity asset generation: BLOCKED
Reason: another Unity instance is running with this project open
Command attempted: <command>
Residual risk: generated assets were not refreshed locally
```

Se nenhum `error CS` existe mas package/test assemblies estão "not valid", trate como ruído de tooling, a menos que o generator não tenha rodado.

## Regras

- Não rode generators do Unity em paralelo para o mesmo projeto.
- Não trate um nome de menu ausente como falha se existe um editor method equivalente; documente a divergência.
- Não forje manualmente assets gerados, a menos que a spec permita explicitamente.
- Não marque a asset generation como completa sem evidência de log e de asset-count.
