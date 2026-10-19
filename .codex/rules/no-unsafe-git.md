# Rule: No Unsafe Git

Não execute operações de git destrutivas sem autorização humana explícita **por instância, no turn atual**. Autorização anterior não se propaga.

## Proibido sem autorização explícita

```
git push / git push --force
git reset --hard
git clean -f / git clean -fd
git stash / git stash drop
git rebase
git branch -D
git checkout -- . / git restore .
```

## Sempre permitido

```
git status / git diff / git log / git branch
git add <arquivos específicos>
git commit -m "..."
```

## Enforcement

Hook `pre-bash-guard.ps1` bloqueia os patterns mais perigosos.
