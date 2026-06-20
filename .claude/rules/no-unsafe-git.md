# Rule: No Unsafe Git

## Regra

Não execute operações de git destrutivas ou de shared-state sem autorização humana explícita para cada instância.

## Por que existe

Operações de git destrutivas (push, reset --hard, clean, stash, rebase) podem causar perda de dados irreversível ou afetar colaboradores. Autorização para uma operação não implica autorização para a mesma operação em um contexto diferente.

## Onde se aplica

Toda tarefa de agent que envolva comandos git.

## Proibido sem autorização explícita (por instância)

```
git push
git push --force
git reset --hard
git clean -f / git clean -fd
git stash
git stash drop
git rebase (interactive or otherwise)
git branch -D
git checkout -- .
git restore .
```

## Sempre permitido

```
git status
git diff
git log
git branch
git add <specific files>
git commit -m "..."
```

## O que fazer se a exceção for necessária

O humano precisa dizer explicitamente: "run git push" ou "force-push is authorized" no turn atual. Autorização de um turn anterior não se propaga para frente.

## Validação

`.claude/hooks/pre-bash-guard.ps1` (always enabled) bloqueia os patterns mais perigosos em tempo de execução.
