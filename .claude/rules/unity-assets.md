# Rule: Segurança de Assets & Editor Unity

Consolida: `unity-yaml-editing-policy`, `generated-asset-evidence`, `no-parallel-unity-batchmode` (os originais são stubs apontando para cá).

## 1. Sem edits manuais de YAML

Não edite manualmente arquivos `.unity`, `.prefab`, `.asset` a menos que a spec autorize explicitamente E o caminho via Unity Editor API (repair menus, editor scripts com `AssetDatabase`/`PrefabUtility`/`SerializedObject`) esteja indisponível.

Permitido: ler YAML para audit; escrever `.cs` scripts; rodar repair menus; editor scripts usando Unity APIs.

Se um edit manual for inevitável, documente:

```text
Unity YAML edit: BLOCKED / EXECUTED WITH AUTHORIZATION
Reason: <Unity Editor unavailable>
Residual risk: GUID/ref integrity not verified; must be checked in Unity Editor
```

Enforcement: `permissions.ask` em `.claude/settings.json` solicita ao humano a cada Edit/Write em `.unity/.prefab/.asset` — a aprovação ali conta como autorização per-instance.

## 2. Assets gerados exigem evidência

Quando uma spec depende de assets gerados do Unity, o closeout deve registrar: menu ou `-executeMethod` usado, log path, exit code, pastas afetadas, contagem esperada vs. real de assets, qualquer divergência de menu/method.

Se a geração não puder rodar:

```text
Asset generation: BLOCKED
Reason: <Unity lock | license | timeout | sandbox | approval | compile error>
Command attempted: <command>
Residual risk: assets may be stale/missing until generated in Unity
```

Nunca afirme que assets gerados existem só porque o código do generator existe. Nunca aceite silenciosamente roster/registry assets descasados.

## 3. Sem Unity batchmode em paralelo

Nunca rode múltiplos processos Unity batchmode para o mesmo projeto simultaneamente. Rode generators/validators sequencialmente, um log file por comando, aguarde o exit. Se o Unity já estiver aberto: feche-o (se autorizado) ou registre a validação como BLOCKED — nunca lance mais instâncias na esperança de que uma vença.

Enforcement: hook `pre-bash-guard.ps1` (PreToolUse) bloqueia o lançamento de batchmode enquanto um processo Unity está rodando.
