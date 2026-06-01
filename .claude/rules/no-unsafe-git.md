# Rule: No Unsafe Git

## Rule

Do not execute destructive or shared-state git operations without explicit human authorization for each instance.

## Why

Destructive git operations (push, reset --hard, clean, stash, rebase) can cause irreversible data loss or affect collaborators. Authorization for one operation does not imply authorization for the same operation in a different context.

## Applies To

All agent tasks involving git commands.

## Prohibited Without Explicit Authorization (per-instance)

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

## Always Allowed

```
git status
git diff
git log
git branch
git add <specific files>
git commit -m "..."
```

## What To Do If Exception Is Needed

The human must explicitly say: "run git push" or "force-push is authorized" in the current turn. Authorization from a previous turn does not carry forward.

## Validation / Detection

`.claude/hooks/pre-bash-guard.ps1` (always enabled) blocks the most dangerous patterns at execution time.
