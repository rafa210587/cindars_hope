# Rule: Segurança & File Safety

Complementa [no-unsafe-git.md](./no-unsafe-git.md) (git destrutivo) e [unity-assets.md](./unity-assets.md) (Unity YAML protegido). Esta rule cobre secrets e arquivos sensíveis que não são código.

## Nada de secrets no repo

- Nunca escreva secrets em código, config versionada ou logs: API keys, tokens, private keys, passwords, connection strings com credentials.
- Use placeholders/valores fictícios em exemplos; carregue valores reais de uma config não versionada ou de uma environment variable.
- Imposto mecanicamente pelo hook `guard-secrets.ps1` (PreToolUse Edit/Write), que bloqueia conteúdo que casa com formatos comuns de secret.

## Arquivos sensíveis — não modifique sem um pedido explícito

`.env`, arquivos de secret/credential, keystores, certificates, signing keys, e **arquivos de save reais do usuário**. Ler para audit é ok; editar exige que a tarefa nomeie o arquivo explicitamente.

> Nota: saves reais do player são runtime data, não o *schema* de save. Mudar save DTOs/migrations é trabalho normal de spec (skill `save-load-pattern`); sobrescrever o arquivo de save real de um usuário em disco não é.

## Scripts de build / deploy / tooling

Antes de editar qualquer coisa sob `tools/`, build scripts, ou config de CI/deploy, leia o arquivo inteiro e explique o impacto primeiro. Esses scripts rodam validation gates e asset pipelines — uma mudança silenciosa pode mascarar um gate quebrado (rule `validation-truth`).

## Geral

- Prefira mudanças reversíveis e diffs pequenos.
- Nenhum comando de cleanup amplo (`Remove-Item -Recurse`, `git clean`, mass delete) para resolver um problema local sem dizer exatamente o que será removido (ver rule `no-unsafe-git`).

## Enforcement

- Hook `guard-secrets.ps1` (PreToolUse) bloqueia conteúdo com cara de secret.
- `permissions.ask` (settings.json) gating de Packages/, ProjectSettings/, e Unity asset YAML.
- Revisado pela skill `non-regression-review` antes do closeout.
