# Cindar's Hope — Project Log

> Fonte operacional de continuidade do projeto.
> Todo agente humano ou IA deve ler este arquivo antes de executar mudanças e atualizá-lo ao final de qualquer tarefa relevante.

---

## 1. Protocolo obrigatório para ChatGPT, Codex, Claude e agentes

Antes de qualquer tarefa:

1. Ler `PROJECT_LOG.md`.
2. Ler `AGENTS.md` e/ou `CLAUDE.md`.
3. Ler os documentos de referência citados no PR/tarefa.
4. Confirmar branch atual e escopo permitido.
5. Não iniciar implementação se houver divergência entre branch, docs e estado real do projeto.

Durante a tarefa:

1. Manter escopo pequeno.
2. Não implementar V2/FULL quando o PR é MVP.
3. Não alterar docs de design sem pedido explícito.
4. Não mexer em arquivos fora da lista permitida do PR.
5. Registrar dúvidas ou desvios em vez de decidir silenciosamente.

Ao final da tarefa:

1. Atualizar este log com uma nova entrada.
2. Informar arquivos alterados.
3. Informar testes executados ou não executados.
4. Informar pendências, riscos e próximo passo recomendado.
5. Nunca apagar histórico anterior; este arquivo é append-only, salvo correção factual explícita.

Modelo de entrada:

```md
## YYYY-MM-DD — Título curto

**Responsável:** Humano / ChatGPT / Codex / Claude
**Branch:** nome-da-branch
**Escopo:** resumo curto

### Alterações
- ...

### Testes
- [ ] ...

### Pendências / riscos
- ...

### Próximo passo recomendado
- ...
```

---

## 2. Estado atual consolidado

### Repo

- Repositório: `rafa210587/cindars_hope`
- Branch base estável: `main`
- Branch de desenvolvimento: `dev`
- Fluxo recomendado: `main` → `dev` → `feature/fase8-pr-XXX-*`

### Fase atual

- Fase 8 — implementação do MVP Fazenda.
- Objetivo do MVP: `BootScene → FarmScene → inventário inicial → plantar → avançar dias → colher → vender → salvar → fechar → reabrir → estado restaurado`.

### PR-001

Status: aplicado no repositório.

Arquivos esperados:

```text
Assets/_Game/Scripts/Core/GameEventBus.cs
Assets/_Game/Scripts/Core/Events/DayStartedEvent.cs
Assets/_Game/Scripts/Core/Events/GoldChangedEvent.cs
Assets/_Game/Scripts/Core/Events/InventoryChangedEvent.cs
Assets/_Game/Scripts/Core/Events/SeedPlantedEvent.cs
Assets/_Game/Scripts/Core/Events/CropHarvestedEvent.cs
Assets/_Game/Scripts/Core/Events/TreeChoppedEvent.cs
Assets/_Game/Scripts/Core/Events/FishCaughtEvent.cs
Assets/_Game/Scripts/Core/Events/HungerChangedEvent.cs
Assets/_Game/Scripts/Core/Events/GameSavedEvent.cs
```

Observação: alguns eventos têm campos extras em relação ao contrato mínimo. Por enquanto isso foi aceito como não bloqueante porque os campos continuam sendo tipos simples/IDs, sem referências Unity pesadas.

### Higiene Git

Status: corrigido na branch `dev`.

Arquivos adicionados:

```text
.gitignore
.gitattributes
```

`.gitignore` ignora pastas geradas pelo Unity, IDEs e outputs locais.
`.gitattributes` configura Git LFS para assets Unity, imagens, áudio, cenas, prefabs e arte Aseprite.

### Ambiente local

Decisão: o projeto não deve ficar dentro de OneDrive/Dropbox/Google Drive.
Caminho recomendado:

```text
C:\dev\cindars_hope
```

Motivo: Unity Package Manager pode falhar com `EPERM` ao renomear pacotes em `Library/PackageCache` quando OneDrive/antivírus segura lock.

---

## 3. Próximo passo recomendado

Antes de iniciar PR-002:

1. Sincronizar local:

```bash
git fetch origin
git checkout dev
git pull origin dev
git lfs install
git lfs pull
```

2. Abrir Unity pelo caminho fora do OneDrive.
3. Validar checklist Unity da seção 4.
4. Se Unity estiver sem erro, criar branch:

```bash
git checkout -b feature/fase8-pr-002-data-contracts-registries
```

5. Executar PR-002 com Codex.

---

## 4. Guia de teste no Unity — Smoke Test atual

Este checklist valida se o projeto está pronto para continuar a Fase 8.

### 4.1 Abertura do projeto

- [ ] Unity abre o projeto sem erro modal.
- [ ] Projeto está fora de OneDrive/Dropbox/Google Drive.
- [ ] Package Manager termina de resolver pacotes.
- [ ] Console não mostra erro vermelho de package resolution.
- [ ] Console não mostra erro vermelho de compilação C#.

### 4.2 Estrutura mínima no Project

Verificar no painel Project:

- [ ] `Assets/_Game/` existe.
- [ ] `Assets/_Game/Scripts/Core/GameEventBus.cs` existe.
- [ ] `Assets/_Game/Scripts/Core/Events/` existe.
- [ ] Os 9 eventos do PR-001 existem.
- [ ] Nenhum script aparece com ícone quebrado/erro de importação.

### 4.3 Compilação

- [ ] Unity recompila scripts automaticamente.
- [ ] Console não mostra erro `CS...`.
- [ ] Console não mostra erro de namespace ausente.
- [ ] Console não mostra erro de tipo duplicado.
- [ ] Console não mostra erro de pacote ausente.

### 4.4 Package Manager

Abrir `Window → Package Manager` e validar:

- [ ] `2D Sprite` ou pacote 2D equivalente está resolvido.
- [ ] `Visual Studio Editor` ou IDE package está resolvido.
- [ ] Não há pacote preso em instalação.
- [ ] Não há erro `EPERM` em `Library/PackageCache`.

### 4.5 Configurações Unity recomendadas

Verificar em `Edit → Project Settings`:

- [ ] Editor → Asset Serialization = `Force Text`.
- [ ] Editor → Version Control Mode = `Visible Meta Files`.
- [ ] Player → Product Name = `Cindar's Hope` ou equivalente.
- [ ] Player → Default Screen Width = `1280`.
- [ ] Player → Default Screen Height = `720`.

### 4.6 Sorting Layers

Verificar em `Project Settings → Tags and Layers → Sorting Layers`.

Camadas esperadas:

```text
Background
Ground
Decoration
Characters
TreeTops
Items
UI_World
UI
```

Se ainda não existirem, registrar como pendência. Não é bloqueante para PR-002, mas será necessário para cenas/arte.

### 4.7 Git dentro do Unity

Depois de abrir/fechar Unity, no terminal:

```bash
git status
```

Resultado esperado:

- [ ] Não aparecer `Library/`.
- [ ] Não aparecer `Temp/`.
- [ ] Não aparecer `Obj/`.
- [ ] Não aparecer `.csproj`/`.sln` como arquivos para commit.
- [ ] Só aparecerem mudanças reais de projeto, se houver.

### 4.8 Critério de liberação para PR-002

PR-002 só deve começar se:

- [ ] Unity abre.
- [ ] Package Manager resolve pacotes.
- [ ] Console não tem erro vermelho.
- [ ] `git status` está limpo ou apenas com mudanças intencionais.
- [ ] Branch local está em `dev` atualizada.

---

## 5. Log de atividades

## 2026-05-16 — Criação do protocolo de log operacional

**Responsável:** ChatGPT
**Branch:** dev
**Escopo:** criar log raiz e orientar continuidade entre ChatGPT, Codex e Claude.

### Alterações
- Criado `PROJECT_LOG.md` na raiz.
- Registrado protocolo obrigatório para agentes.
- Registrado estado atual do repo, PR-001 e higiene Git.
- Adicionado guia de smoke test Unity.

### Testes
- [x] Arquivo criado no GitHub na branch `dev`.
- [ ] Unity não testado pelo ChatGPT; precisa validação local.

### Pendências / riscos
- Atualizar `AGENTS.md` e `CLAUDE.md` para apontar explicitamente para `PROJECT_LOG.md`.
- Validar Unity localmente antes do PR-002.

### Próximo passo recomendado
- Atualizar `AGENTS.md` e `CLAUDE.md` com regra de leitura/atualização do log.
- Rodar o smoke test Unity.
- Criar `feature/fase8-pr-002-data-contracts-registries`.

## 2026-05-16 — Versionamento dos metas Unity do core

**Responsável:** Humano orientado por ChatGPT
**Branch:** dev
**Escopo:** limpar arquivos locais indevidos após sincronização e versionar `.meta` Unity necessários para PR-001.

### Alterações
- Removidos localmente do working tree: `.vscode/`, `Assets/MobileDependencyResolver/`, `Assets/Resources/` e `cindars_hope.slnx`.
- Adicionados e enviados para `origin/dev` os `.meta` de `Assets/_Game/Scripts/Core` e `Assets/_Game/Scripts/Core/Events`.
- Adicionado `ProjectSettings/PackageManagerSettings.asset`.
- Commit local enviado: `7238f66 chore: adicionar metas unity do core`.

### Testes
- [x] `git pull origin dev` executou com fast-forward.
- [x] `git commit` criou 15 arquivos Unity/meta.
- [x] `git push origin dev` concluiu com sucesso.
- [ ] Unity ainda precisa ser aberto e validado localmente após este push.

### Pendências / riscos
- Stash de backup ainda pode existir localmente; não aplicar `git stash pop` novamente.
- Descartar o stash somente após o Unity abrir sem erros.
- Validar se `ProjectSettings/PackageManagerSettings.asset` é compatível com a versão local do Unity.

### Próximo passo recomendado
- Rodar `git status --short`.
- Abrir Unity e executar smoke test da seção 4.
- Se Unity estiver limpo, descartar o stash de backup com `git stash drop stash@{0}`.
- Depois criar `feature/fase8-pr-002-data-contracts-registries`.
