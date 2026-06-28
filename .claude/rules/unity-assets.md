# Rule: Segurança de Assets & Editor Unity

**1. Sem edits manuais de YAML.** Não edite `.unity`, `.prefab`, `.asset` a menos que a spec autorize explicitamente E a via Unity Editor API seja inviável. Se inevitável, documente:
```text
Unity YAML edit: BLOCKED / EXECUTED WITH AUTHORIZATION
Residual risk: GUID/ref integrity not verified; must be checked in Unity Editor
```
`permissions.ask` solicita aprovação humana por instância para cada Edit/Write nesses arquivos.

**2. Assets gerados exigem evidência.** Registre no closeout: método usado, log path, exit code, contagem esperada vs. real. Se não puder rodar:
```text
Asset generation: BLOCKED
Reason: <Unity lock | license | timeout | sandbox | compile error>
Residual risk: assets may be stale/missing until generated in Unity
```
Nunca afirme que assets gerados existem só porque o código do generator existe.

**3. Sem Unity batchmode em paralelo.** Rode generators/validators sequencialmente; aguarde o exit de cada um. Se Unity já estiver aberto, registre BLOCKED — nunca lance instâncias paralelas.

## Enforcement

Hook `pre-bash-guard.ps1` bloqueia batchmode enquanto processo Unity está rodando.
