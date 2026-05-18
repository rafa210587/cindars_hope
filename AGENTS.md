# AGENTS.md — Fluxo de trabalho para agentes Claude em Cindar's Hope

## Regras operacionais de agentes

### Branch e ambiente
- Agentes trabalham na branch designada (tipicamente `feature/fase9a-*` ou similar).
- Antes de alterar qualquer arquivo, SEMPRE verificar:
  ```
  git branch --show-current
  git status --short
  git log --oneline --decorate -10
  ```
- Se working tree estiver sujo, PARAR e explicar ao humano.
- Se a branch não for a esperada, PARAR e explicar.

### Leitura obrigatória antes de qualquer tarefa
1. `PROJECT_LOG.md` — estado operacional, decisões recentes, pendências.
2. `AGENTS.md` e `CLAUDE.md` — regras permanentes.
3. Documentos de referência do PR/tarefa (specs, plans, etc).

### Criação de commits
- Agentes podem criar commits locais **apenas em português**.
- PRs pequenas, seguindo escopo definido (1 PR = 1 tarefa).
- Arquivos alterados **apenas** os permitidos por PR.
- Antes de cada commit, rodar:
  ```
  git status --short
  git diff --stat
  ```
- Nenhum arquivo proibido pode ser alterado (Packages/, ProjectSettings/, .claude/, .vscode/, *.sln, etc).

### Git operations — PROIBIDO
- ❌ `git push` — Push é responsabilidade do humano.
- ❌ Abrir PR/MR — Pull requests e merges são responsabilidade do humano.
- ❌ Deletar branches locais/remotas — Deleção é responsabilidade do humano.
- ❌ `git stash` — Usar em caso de conflito seria destruir trabalho.
- ❌ `git clean` — Risco de deletar arquivos importantes.
- ❌ `git reset --hard` — Risco de perder trabalho não commitado.

### Entrega ao humano
Ao final de cada PR ou pacote, entregar:
- Lista de commits locais criados (SHA + mensagem).
- Arquivos alterados por PR.
- Testes executados (✓) e testes pendentes (✗).
- Instruções reproduzíveis para validação local.
- Confirmação de que nenhum arquivo proibido foi alterado.
- Comandos sugeridos para o humano executar (push, PR criação, merge), mas **sem executá-los**.

### Atualização de PROJECT_LOG.md
- **Obrigatória** ao final de cada PR/tarefa.
- Registrar:
  - Branch usada.
  - Escopo executado.
  - Arquivos alterados.
  - Testes executados/pendentes.
  - Pendências e riscos.
  - Próximo passo recomendado.
- `PROJECT_LOG.md` é append-only: não apagar histórico anterior.

## Fluxo esperado para um pacote

1. **Leitura:** Ler PROJECT_LOG.md, AGENTS.md, CLAUDE.md, specs do pacote.
2. **Verificação:** Confirmar branch, status, logs.
3. **Implementação:** Executar cada PR dentro do escopo, 1 por 1.
4. **Commit:** Após cada PR, criar commit local com mensagem em português.
5. **Documentação:** Atualizar PROJECT_LOG.md com entrada da PR.
6. **Entrega:** Ao final do pacote, listar tudo que foi feito e entregar ao humano.
7. **Push/PR:** O humano faz push, abre PR, faz merge, etc.

## Regras técnicas (resumo)

Leia [CLAUDE.md](CLAUDE.md) para regras completas de código, mas em resumo:
- Nunca `GameObject.Find()`, `FindObjectOfType()`.
- Sempre eventos via `GameEventBus.Publish/Subscribe`.
- Nunca hardcodar dados.
- Sempre `Unsubscribe` em `OnDisable/OnDestroy`.
- Nunca lógica de negócio em MonoBehaviour.

## Contato/Dúvidas

Se encontrar conflito de regras ou ambiguidade, parar e explicar ao humano antes de prosseguir.
