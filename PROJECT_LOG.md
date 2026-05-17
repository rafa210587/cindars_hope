# Cindar's Hope Ã¢â‚¬â€ Project Log

> Fonte operacional de continuidade do projeto.
> Todo agente humano ou IA deve ler este arquivo antes de executar mudanÃ§as e atualizÃ¡-lo ao final de qualquer tarefa relevante.

---

## 1. Protocolo obrigatÃ³rio para ChatGPT, Codex, Claude e agentes

Antes de qualquer tarefa:

1. Ler `PROJECT_LOG.md`.
2. Ler `AGENTS.md` e/ou `CLAUDE.md`.
3. Ler os documentos de referÃªncia citados no PR/tarefa.
4. Confirmar branch atual e escopo permitido.
5. NÃ£o iniciar implementaÃ§Ã£o se houver divergÃªncia entre branch, docs e estado real do projeto.

Durante a tarefa:

1. Manter escopo pequeno.
2. NÃ£o implementar V2/FULL quando o PR Ã© MVP.
3. NÃ£o alterar docs de design sem pedido explÃ­cito.
4. NÃ£o mexer em arquivos fora da lista permitida do PR.
5. Registrar dÃºvidas ou desvios em vez de decidir silenciosamente.

Ao final da tarefa:

1. Atualizar este log com uma nova entrada.
2. Informar arquivos alterados.
3. Informar testes executados ou nÃ£o executados.
4. Informar pendÃªncias, riscos e prÃ³ximo passo recomendado.
5. Nunca apagar histÃ³rico anterior; este arquivo Ã© append-only, salvo correÃ§Ã£o factual explÃ­cita.

Modelo de entrada:

```md
## YYYY-MM-DD Ã¢â‚¬â€ TÃ­tulo curto

**ResponsÃ¡vel:** Humano / ChatGPT / Codex / Claude
**Branch:** nome-da-branch
**Escopo:** resumo curto

### AlteraÃ§Ãµes
- ...

### Testes
- [ ] ...

### PendÃªncias / riscos
- ...

### PrÃ³ximo passo recomendado
- ...
```

---

## 2. Estado atual consolidado

### Repo

- RepositÃ³rio: `rafa210587/cindars_hope`
- Branch base estÃ¡vel: `main`
- Branch de desenvolvimento: `dev`
- Fluxo recomendado: `main` Ã¢â€ â€™ `dev` Ã¢â€ â€™ `feature/fase8-pr-XXX-*`

### Fase atual

- Fase 8 Ã¢â‚¬â€ implementaÃ§Ã£o do MVP Fazenda.
- Objetivo do MVP: `BootScene Ã¢â€ â€™ FarmScene Ã¢â€ â€™ inventÃ¡rio inicial Ã¢â€ â€™ plantar Ã¢â€ â€™ avanÃ§ar dias Ã¢â€ â€™ colher Ã¢â€ â€™ vender Ã¢â€ â€™ salvar Ã¢â€ â€™ fechar Ã¢â€ â€™ reabrir Ã¢â€ â€™ estado restaurado`.

### Status dos PRs da Fase 8

| PR | Status |
|---|---|
| PR-001 | Mergeado em `dev` |
| PR-002 | Mergeado em `dev` |
| PR-003 | Mergeado em `dev` |
| PR-004 | Mergeado em `dev` |
| PR-005 | Mergeado em `dev` |
| PR-006 | Mergeado em `dev` |
| PR-007 | Mergeado em `dev` |
| PR-008 | Mergeado em `dev` |
| PR-009 | Mergeado em `dev`, documental pÃ³s PR-008 |
| PR-010 | Mergeado em `dev`, movimento/input mÃ­nimo |
| PR-011 | Mergeado em `dev`, runtime state hardening |
| PR-012 | Implementado; pendente validação Unity e merge |
| PR-013 a PR-017 | Implementados nesta wave; pendentes validação Unity e merge |
| PR-018 a PR-024 | Implementados nesta wave; pendentes validação Unity e merge |
| PR-025 a PR-030 | Implementados nesta wave; pendentes validação Unity e merge |
| Próximo PR runtime | Pesca/árvores ou hardening de save/load após validação |

### PR-001

Status: aplicado no repositÃ³rio.

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

ObservaÃ§Ã£o: alguns eventos tÃªm campos extras em relaÃ§Ã£o ao contrato mÃ­nimo. Por enquanto isso foi aceito como nÃ£o bloqueante porque os campos continuam sendo tipos simples/IDs, sem referÃªncias Unity pesadas.

### Higiene Git

Status: corrigido na branch `dev`.

Arquivos adicionados:

```text
.gitignore
.gitattributes
```

`.gitignore` ignora pastas geradas pelo Unity, IDEs e outputs locais.
`.gitattributes` configura Git LFS para assets Unity, imagens, Ã¡udio, cenas, prefabs e arte Aseprite.

### Ambiente local

DecisÃ£o: o projeto nÃ£o deve ficar dentro de OneDrive/Dropbox/Google Drive.
Caminho recomendado:

```text
C:\dev\cindars_hope
```

Motivo: Unity Package Manager pode falhar com `EPERM` ao renomear pacotes em `Library/PackageCache` quando OneDrive/antivÃ­rus segura lock.

---

## 3. PrÃ³ximo passo recomendado

Validar Unity apÃ³s PR-011:

1. Console sem erro vermelho.
2. `CindarsHope/Validate/Validate MVP Data` passa.
3. `CindarsHope/Scenes/Create MVP FarmScene` executa.
4. Play Mode entra sem erro.
5. Player ainda move com WASD/setas.

PrÃ³ximo PR runtime:

- PR-012 â€” Interaction System mÃ­nimo.
- NÃ£o implementar plantio ainda no PR-012.
- NÃ£o implementar UI final ainda no PR-012.
- NÃ£o implementar save/load ainda.

Nota: o plano histÃ³rico em `docs/FASE8_EXECUTION_PLAN_CODEX_v1.0.md` tem numeraÃ§Ã£o antiga preservada para contexto. A fonte operacional de sequÃªncia passa a ser o topo atualizado do `PROJECT_LOG.md`.

---

## 4. Guia de teste no Unity Ã¢â‚¬â€ Smoke Test atual

Este checklist valida se o projeto estÃ¡ pronto para continuar a Fase 8.

### 4.1 Abertura do projeto

- [ ] Unity abre o projeto sem erro modal.
- [ ] Projeto estÃ¡ fora de OneDrive/Dropbox/Google Drive.
- [ ] Package Manager termina de resolver pacotes.
- [ ] Console nÃ£o mostra erro vermelho de package resolution.
- [ ] Console nÃ£o mostra erro vermelho de compilaÃ§Ã£o C#.

### 4.2 Estrutura mÃ­nima no Project

Verificar no painel Project:

- [ ] `Assets/_Game/` existe.
- [ ] `Assets/_Game/Scripts/Core/GameEventBus.cs` existe.
- [ ] `Assets/_Game/Scripts/Core/Events/` existe.
- [ ] Os 9 eventos do PR-001 existem.
- [ ] Nenhum script aparece com Ã­cone quebrado/erro de importaÃ§Ã£o.

### 4.3 CompilaÃ§Ã£o

- [ ] Unity recompila scripts automaticamente.
- [ ] Console nÃ£o mostra erro `CS...`.
- [ ] Console nÃ£o mostra erro de namespace ausente.
- [ ] Console nÃ£o mostra erro de tipo duplicado.
- [ ] Console nÃ£o mostra erro de pacote ausente.

### 4.4 Package Manager

Abrir `Window Ã¢â€ â€™ Package Manager` e validar:

- [ ] `2D Sprite` ou pacote 2D equivalente estÃ¡ resolvido.
- [ ] `Visual Studio Editor` ou IDE package estÃ¡ resolvido.
- [ ] NÃ£o hÃ¡ pacote preso em instalaÃ§Ã£o.
- [ ] NÃ£o hÃ¡ erro `EPERM` em `Library/PackageCache`.

### 4.5 ConfiguraÃ§Ãµes Unity recomendadas

Verificar em `Edit Ã¢â€ â€™ Project Settings`:

- [ ] Editor Ã¢â€ â€™ Asset Serialization = `Force Text`.
- [ ] Editor Ã¢â€ â€™ Version Control Mode = `Visible Meta Files`.
- [ ] Player Ã¢â€ â€™ Product Name = `Cindar's Hope` ou equivalente.
- [ ] Player Ã¢â€ â€™ Default Screen Width = `1280`.
- [ ] Player Ã¢â€ â€™ Default Screen Height = `720`.

### 4.6 Sorting Layers

Verificar em `Project Settings Ã¢â€ â€™ Tags and Layers Ã¢â€ â€™ Sorting Layers`.

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

Se ainda nÃ£o existirem, registrar como pendÃªncia. NÃ£o Ã© bloqueante para PR-002, mas serÃ¡ necessÃ¡rio para cenas/arte.

### 4.7 Git dentro do Unity

Depois de abrir/fechar Unity, no terminal:

```bash
git status
```

Resultado esperado:

- [ ] NÃ£o aparecer `Library/`.
- [ ] NÃ£o aparecer `Temp/`.
- [ ] NÃ£o aparecer `Obj/`.
- [ ] NÃ£o aparecer `.csproj`/`.sln` como arquivos para commit.
- [ ] SÃ³ aparecerem mudanÃ§as reais de projeto, se houver.

### 4.8 CritÃ©rio de liberaÃ§Ã£o para PR-002

PR-002 sÃ³ deve comeÃ§ar se:

- [ ] Unity abre.
- [ ] Package Manager resolve pacotes.
- [ ] Console nÃ£o tem erro vermelho.
- [ ] `git status` estÃ¡ limpo ou apenas com mudanÃ§as intencionais.
- [ ] Branch local estÃ¡ em `dev` atualizada.

### 4.9 ValidaÃ§Ã£o pÃ³s-PR-007/PR-008

- [ ] Menu `CindarsHope/Scenes/Create MVP FarmScene` executa sem erro.
- [ ] `FarmScene.unity` abre.
- [ ] Hierarquia contÃ©m `_Bootstrap`, `Player`, `Ground`, `Bounds` e `Main Camera`.
- [ ] `_Bootstrap` tem `GameBootstrap`, `PlayerManager`, `InventoryManager`, `TimeManager` e `SaveManager`.
- [ ] `GameBootstrap` referencia `PlayerData.asset` e `ItemDatabase.asset`.
- [ ] Console sem erro vermelho.
- [ ] Eventos do PR-008 existem em `Assets/_Game/Scripts/Core/Events`.
- [ ] Menu `CindarsHope/Validate/Validate MVP Data` continua passando.

---

## 5. Log de atividades

## 2026-05-16 Ã¢â‚¬â€ CriaÃ§Ã£o do protocolo de log operacional

**ResponsÃ¡vel:** ChatGPT
**Branch:** dev
**Escopo:** criar log raiz e orientar continuidade entre ChatGPT, Codex e Claude.

### AlteraÃ§Ãµes
- Criado `PROJECT_LOG.md` na raiz.
- Registrado protocolo obrigatÃ³rio para agentes.
- Registrado estado atual do repo, PR-001 e higiene Git.
- Adicionado guia de smoke test Unity.

### Testes
- [x] Arquivo criado no GitHub na branch `dev`.
- [ ] Unity nÃ£o testado pelo ChatGPT; precisa validaÃ§Ã£o local.

### PendÃªncias / riscos
- Atualizar `AGENTS.md` e `CLAUDE.md` para apontar explicitamente para `PROJECT_LOG.md`.
- Validar Unity localmente antes do PR-002.

### PrÃ³ximo passo recomendado
- Atualizar `AGENTS.md` e `CLAUDE.md` com regra de leitura/atualizaÃ§Ã£o do log.
- Rodar o smoke test Unity.
- Criar `feature/fase8-pr-002-data-contracts-registries`.

## 2026-05-16 Ã¢â‚¬â€ Versionamento dos metas Unity do core

**ResponsÃ¡vel:** Humano orientado por ChatGPT
**Branch:** dev
**Escopo:** limpar arquivos locais indevidos apÃ³s sincronizaÃ§Ã£o e versionar `.meta` Unity necessÃ¡rios para PR-001.

### AlteraÃ§Ãµes
- Removidos localmente do working tree: `.vscode/`, `Assets/MobileDependencyResolver/`, `Assets/Resources/` e `cindars_hope.slnx`.
- Adicionados e enviados para `origin/dev` os `.meta` de `Assets/_Game/Scripts/Core` e `Assets/_Game/Scripts/Core/Events`.
- Adicionado `ProjectSettings/PackageManagerSettings.asset`.
- Commit local enviado: `7238f66 chore: adicionar metas unity do core`.

### Testes
- [x] `git pull origin dev` executou com fast-forward.
- [x] `git commit` criou 15 arquivos Unity/meta.
- [x] `git push origin dev` concluiu com sucesso.
- [ ] Unity ainda precisa ser aberto e validado localmente apÃ³s este push.

### PendÃªncias / riscos
- Stash de backup ainda pode existir localmente; nÃ£o aplicar `git stash pop` novamente.
- Descartar o stash somente apÃ³s o Unity abrir sem erros.
- Validar se `ProjectSettings/PackageManagerSettings.asset` Ã© compatÃ­vel com a versÃ£o local do Unity.

### PrÃ³ximo passo recomendado
- Rodar `git status --short`.
- Abrir Unity e executar smoke test da seÃ§Ã£o 4.
- Se Unity estiver limpo, descartar o stash de backup com `git stash drop stash@{0}`.
- Depois criar `feature/fase8-pr-002-data-contracts-registries`.

## 2026-05-17 - PR-002 Data contracts e registries

**Responsavel:** Codex
**Branch:** feature/fase8-pr-002-data-contracts-registries
**Escopo:** criar contratos de dados identificaveis, registries ScriptableObject por ID e dados minimos de Item, Seed e Player para o MVP Fazenda.

### Alteracoes
- Criados contratos `IIdentifiedData` e `IDataRegistry<T>`.
- Criado `DataRegistrySO<T>` com indice por ID, validacao de item nulo, ID vazio e ID duplicado.
- Criados registries `ItemDatabaseSO` e `SeedDatabaseSO`.
- Criados dados `ItemDataSO`, `SeedDataSO`, `PlayerDataSO` e enum `ItemCategory`.
- Nenhum sistema de gameplay, UI, cena, prefab, save/load ou inventario funcional foi implementado.

### Arquivos alterados
- `Assets/_Game/Scripts/Core/Data/IIdentifiedData.cs`
- `Assets/_Game/Scripts/Core/Data/IDataRegistry.cs`
- `Assets/_Game/Scripts/Core/Data/DataRegistrySO.cs`
- `Assets/_Game/Scripts/Core/Data/ItemDatabaseSO.cs`
- `Assets/_Game/Scripts/Core/Data/SeedDatabaseSO.cs`
- `Assets/_Game/Scripts/Farm/Data/SeedDataSO.cs`
- `Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs`
- `Assets/_Game/Scripts/Inventory/Data/ItemCategory.cs`
- `Assets/_Game/Scripts/Player/Data/PlayerDataSO.cs`
- `PROJECT_LOG.md`

### Testes
- [x] Verificada a branch esperada via `.git/HEAD`.
- [x] Busca estatica nos arquivos novos por `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType` e `StreamingAssets` sem ocorrencias.
- [ ] Unity nao executado nesta sessao.
- [ ] Compilacao C# local nao executada: `dotnet`, `csc`, `msbuild` e `git` nao estavam disponiveis no PATH do terminal.

### Pendencias / riscos
- Abrir Unity e confirmar que os menus `CindarsHope/Data/*` e `CindarsHope/Database/*` aparecem no Create Asset Menu.
- Criar manualmente assets de teste de Item, Seed, Player e databases para validar campos no Inspector.
- Unity pode gerar `.meta` para as novas pastas e scripts ao abrir o projeto.

### Proximo passo recomendado
- Executar smoke test no Unity: criar um `ItemDataSO`, um `SeedDataSO`, um `PlayerDataSO`, `ItemDatabaseSO` e `SeedDatabaseSO`; preencher IDs validos; confirmar Console sem erro.
- Depois seguir para PR-003 - Bootstrap managers vazios.

## 2026-05-17 - PR-003 Assets de dados MVP

**Responsavel:** Codex
**Branch:** feature/fase8-pr-003-mvp-data-assets
**Escopo:** criar assets ScriptableObject reais do MVP Fazenda para itens, sementes, PlayerData e registries usando os contratos do PR-002.

### Alteracoes
- Criadas pastas de dados `Items`, `Seeds`, `Config` e `Registries` em `Assets/_Game/Data/`.
- Criados 6 assets `ItemDataSO` do MVP: sementes de trigo/cenoura, trigo, cenoura, peixe comum e madeira.
- Criados 2 assets `SeedDataSO`: trigo e cenoura, com referencias para seed item, harvest item e amounts.
- Criado `PlayerData.asset` com velocidade, HP, ouro inicial e sementes iniciais.
- Criados `ItemDatabase.asset` e `SeedDatabase.asset` com referencias aos assets do MVP.
- Nenhum script C#, cena, prefab, UI, inventario, plantio ou save/load foi alterado/implementado.

### Arquivos alterados
- `Assets/_Game/Data/Items.meta`
- `Assets/_Game/Data/Items/Item_Semente_Trigo.asset`
- `Assets/_Game/Data/Items/Item_Semente_Trigo.asset.meta`
- `Assets/_Game/Data/Items/Item_Semente_Cenoura.asset`
- `Assets/_Game/Data/Items/Item_Semente_Cenoura.asset.meta`
- `Assets/_Game/Data/Items/Item_Trigo.asset`
- `Assets/_Game/Data/Items/Item_Trigo.asset.meta`
- `Assets/_Game/Data/Items/Item_Cenoura.asset`
- `Assets/_Game/Data/Items/Item_Cenoura.asset.meta`
- `Assets/_Game/Data/Items/Item_Fish_Common.asset`
- `Assets/_Game/Data/Items/Item_Fish_Common.asset.meta`
- `Assets/_Game/Data/Items/Item_Wood.asset`
- `Assets/_Game/Data/Items/Item_Wood.asset.meta`
- `Assets/_Game/Data/Seeds.meta`
- `Assets/_Game/Data/Seeds/Seed_Trigo.asset`
- `Assets/_Game/Data/Seeds/Seed_Trigo.asset.meta`
- `Assets/_Game/Data/Seeds/Seed_Cenoura.asset`
- `Assets/_Game/Data/Seeds/Seed_Cenoura.asset.meta`
- `Assets/_Game/Data/Config.meta`
- `Assets/_Game/Data/Config/PlayerData.asset`
- `Assets/_Game/Data/Config/PlayerData.asset.meta`
- `Assets/_Game/Data/Registries.meta`
- `Assets/_Game/Data/Registries/ItemDatabase.asset`
- `Assets/_Game/Data/Registries/ItemDatabase.asset.meta`
- `Assets/_Game/Data/Registries/SeedDatabase.asset`
- `Assets/_Game/Data/Registries/SeedDatabase.asset.meta`
- `PROJECT_LOG.md`

### Testes
- [x] Verificada a branch esperada via `.git/HEAD`.
- [x] Verificada existencia dos assets e metas em `Assets/_Game/Data`.
- [x] Verificadas referencias por GUID entre PlayerData, seeds, itens e registries.
- [x] Verificado que `ItemDatabase.asset` referencia 6 itens e `SeedDatabase.asset` referencia 2 sementes.
- [ ] Unity nao executado nesta sessao.
- [ ] Compilacao C# local nao executada: ferramentas `git`, `dotnet`, `csc` e `msbuild` seguem indisponiveis no PATH do terminal.

### Pendencias / riscos
- Abrir Unity para reimportar os assets e confirmar que nao ha missing scripts nem referencias quebradas no Inspector.
- Confirmar no Console se os registries nao acusam ID vazio/duplicado. Os IDs de item de semente e SeedData usam o mesmo valor por design do prompt (`seed_wheat`, `seed_carrot`) em databases separados.
- Se Unity regenerar metas automaticamente, revisar se os GUIDs foram preservados.

### Proximo passo recomendado
- Executar smoke test no Unity: selecionar `PlayerData`, `ItemDatabase`, `SeedDatabase`, `Seed_Trigo` e `Seed_Cenoura` e validar campos/referencias no Inspector; Console sem erro.
- Depois seguir para o proximo PR de codigo planejado, mantendo escopo pequeno e sem alterar estes assets fora de necessidade explicita.

## 2026-05-17 - PR-003 ajuste Cana Basica

**Responsavel:** Codex
**Branch:** feature/fase8-pr-003-mvp-data-assets
**Escopo:** ajustar os assets de dados do PR-003 para incluir a ferramenta inicial obrigatoria do MVP.

### Alteracoes
- Criado `Assets/_Game/Data/Items/Item_Cana_Basica.asset`.
- `PR-003` agora inclui 7 itens, incluindo `Cana Basica`.
- `PlayerData.asset` agora inclui `Cana Basica x1` junto com `Semente de Trigo x5` e `Semente de Cenoura x3`.
- `ItemDatabase.asset` agora referencia 7 itens.
- `SeedDatabase.asset` continua referenciando 2 seeds.
- Removidos `Assets/Resources/BillingMode.json` e `Assets/Resources/BillingMode.json.meta`, que estavam fora do escopo do PR-003.
- Nenhum script C#, gameplay, cena, prefab ou UI foi alterado.

### Arquivos alterados
- `Assets/_Game/Data/Items/Item_Cana_Basica.asset`
- `Assets/_Game/Data/Items/Item_Cana_Basica.asset.meta`
- `Assets/_Game/Data/Registries/ItemDatabase.asset`
- `Assets/_Game/Data/Config/PlayerData.asset`
- `Assets/Resources/BillingMode.json` removido
- `Assets/Resources/BillingMode.json.meta` removido
- `PROJECT_LOG.md`

### Testes
- [x] `ItemDatabase.asset` referencia 7 itens.
- [x] `PlayerData.asset` referencia 3 `StartingItems`.
- [x] `SeedDatabase.asset` continua com 2 seeds.
- [x] `Assets/Resources/BillingMode.json` e `.meta` nao existem mais.
- [x] Nenhum script C# foi editado nesta etapa.
- [ ] Unity nao executado nesta sessao.

### Pendencias / riscos
- Abrir Unity para validar o asset `Item_Cana_Basica` no Inspector e confirmar Console sem missing reference.

### Proximo passo recomendado
- Reimportar assets no Unity e validar `PlayerData`, `ItemDatabase` e `SeedDatabase` antes de seguir para o proximo PR.

## 2026-05-17 - PR-004 Editor Data Validator

**Responsavel:** Codex
**Branch:** feature/fase8-pr-004-editor-data-validator
**Escopo:** criar uma ferramenta Editor-only para validar os assets de dados MVP antes de iniciar managers e gameplay.

### Alteracoes
- Criado `CindarsHopeDataValidator.cs` em `Assets/_Game/Scripts/Editor/DataValidation/`.
- Adicionado menu `CindarsHope/Validate/Validate MVP Data`.
- O validator localiza `ItemDatabaseSO`, `SeedDatabaseSO` e `PlayerDataSO` via `AssetDatabase`.
- O validator verifica IDs obrigatorios, nulos, IDs vazios, IDs duplicados, dados de seeds e itens iniciais do `PlayerData`.
- Em sucesso, loga `Cindar's Hope MVP data validation passed.`.
- Em falha, loga cada erro com `Debug.LogError` e lanca excecao ao final.
- Nenhum script runtime, gameplay, cena, prefab, UI ou asset de dados foi alterado.

### Arquivos alterados
- `Assets/_Game/Scripts/Editor.meta`
- `Assets/_Game/Scripts/Editor/DataValidation.meta`
- `Assets/_Game/Scripts/Editor/DataValidation/CindarsHopeDataValidator.cs`
- `Assets/_Game/Scripts/Editor/DataValidation/CindarsHopeDataValidator.cs.meta`
- `PROJECT_LOG.md`

### Testes
- [x] Verificada a branch esperada via `.git/HEAD`.
- [x] Busca estatica no validator por `GameObject.Find`, `FindObjectOfType` e `FindObjectsByType` sem ocorrencias.
- [x] Verificacao estatica dos assets atuais: `ItemDatabase.asset` referencia 7 itens.
- [x] Verificacao estatica dos assets atuais: `SeedDatabase.asset` referencia 2 seeds.
- [x] Verificacao estatica dos assets atuais: `PlayerData.asset` referencia 3 `StartingItems`.
- [ ] Unity nao executado nesta sessao; menu ainda precisa ser rodado manualmente no Editor.
- [ ] Compilacao C# local nao executada: ferramentas `git`, `dotnet`, `csc` e `msbuild` nao estao disponiveis no PATH do terminal.

### Pendencias / riscos
- Abrir Unity e confirmar que o menu `CindarsHope/Validate/Validate MVP Data` aparece.
- Rodar o menu com os assets atuais e confirmar sucesso no Console.
- Testar falha manual removendo uma referencia obrigatoria e confirmar `Debug.LogError` + excecao.

### Proximo passo recomendado
- Validar PR-004 no Unity antes de iniciar o proximo PR de managers/bootstrap.

## 2026-05-17 - PR-005 Bootstrap managers vazios

**Responsavel:** Codex
**Branch:** feature/fase8-pr-005-bootstrap-managers
**Escopo:** criar a estrutura minima de bootstrap e managers runtime, sem implementar gameplay, inventario funcional, tempo funcional ou save/load real.

### Alteracoes
- Criado `GameBootstrap` com singleton simples apenas para o bootstrap.
- `GameBootstrap` inicializa managers referenciados por `[SerializeField]` e loga warning quando alguma referencia estiver ausente.
- Criados `PlayerManager`, `InventoryManager`, `TimeManager` e `SaveManager` com `IsInitialized`, `Initialize()` e `Shutdown()`.
- Nenhum manager implementa movimento, inventario funcional, ciclo de tempo, save/load, plantio, colheita, UI, cena ou prefab.
- Nao foram usadas buscas globais em runtime (`GameObject.Find`, `FindObjectOfType`, `FindObjectsByType`).

### Arquivos alterados
- `Assets/_Game/Scripts/Core/Bootstrap.meta`
- `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs`
- `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs.meta`
- `Assets/_Game/Scripts/Core/Time.meta`
- `Assets/_Game/Scripts/Core/Time/TimeManager.cs`
- `Assets/_Game/Scripts/Core/Time/TimeManager.cs.meta`
- `Assets/_Game/Scripts/Inventory/InventoryManager.cs`
- `Assets/_Game/Scripts/Inventory/InventoryManager.cs.meta`
- `Assets/_Game/Scripts/Player/PlayerManager.cs`
- `Assets/_Game/Scripts/Player/PlayerManager.cs.meta`
- `Assets/_Game/Scripts/Save.meta`
- `Assets/_Game/Scripts/Save/SaveManager.cs`
- `Assets/_Game/Scripts/Save/SaveManager.cs.meta`
- `PROJECT_LOG.md`

### Testes
- [x] Verificada a branch esperada via `.git/HEAD`.
- [x] Criados apenas arquivos dentro da lista permitida, alem de `.meta` dos novos assets/pastas.
- [x] Busca estatica nos arquivos do PR por `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType` e `StreamingAssets` sem ocorrencias.
- [x] Busca estatica confirmou ausencia de metodos funcionais de inventario/save/tempo como `AddItem`, `RemoveItem`, `AdvanceDay`, `Save(` e `Load(` nos novos managers.
- [ ] Unity nao executado nesta sessao.
- [ ] Compilacao C# local nao executada: ferramentas `git`, `dotnet`, `csc` e `msbuild` nao estao disponiveis no PATH do terminal.

### Pendencias / riscos
- Abrir Unity e confirmar Console sem erro vermelho.
- Teste manual: criar temporariamente um GameObject em uma cena de teste, adicionar `GameBootstrap` e os quatro managers no Inspector, entrar em Play Mode e confirmar que nao ha erro.
- Nao salvar cena/prefab neste PR.

### Proximo passo recomendado
- Validar PR-005 no Unity e depois seguir para PR-006, onde o estado de novo jogo e o inventario inicial devem consumir `PlayerDataSO` sem hardcode de dados em manager.

## 2026-05-17 - PR-006 NewGameState e inventario inicial

**Responsavel:** Codex
**Branch:** feature/fase8-pr-006-new-game-state-inventory
**Escopo:** criar estado inicial runtime usando `PlayerDataSO`, `ItemDatabaseSO`, `PlayerManager` e `InventoryManager`, sem UI, cena, prefab, save/load ou gameplay de plantio.

### Alteracoes
- Criado `InventoryStack` imutavel com `ItemId` e `Amount`, validando ID vazio e quantidade negativa.
- `InventoryManager` agora mantem inventario interno em `Dictionary<string, int>` usando IDs estaveis.
- `InventoryManager.InitializeFromStartingItems` inicializa itens iniciais a partir de `PlayerDataSO.StartingItems` e valida IDs pelo `ItemDatabaseSO`.
- `InventoryManager.AddItem` e `RemoveItem` validam entrada, respeitam `MaxStack` do `ItemDataSO` e publicam `InventoryChangedEvent`.
- `PlayerManager` inicializa `CurrentGold` e `CurrentHP` a partir de `PlayerDataSO`.
- `PlayerManager` publica `GoldChangedEvent` ao inicializar ouro e ao mudar ouro por `SetGold`, `AddGold` ou `TrySpendGold`.
- `GameBootstrap` recebeu referencias serializadas para `PlayerDataSO` e `ItemDatabaseSO` e usa essas referencias para inicializar o estado inicial.
- Nao houve UI, cena, prefab, save/load JSON, movimento, plantio, colheita, pesca ou venda.

### Arquivos alterados
- `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs`
- `Assets/_Game/Scripts/Player/PlayerManager.cs`
- `Assets/_Game/Scripts/Inventory/InventoryManager.cs`
- `Assets/_Game/Scripts/Inventory/InventoryStack.cs`
- `Assets/_Game/Scripts/Inventory/InventoryStack.cs.meta`
- `PROJECT_LOG.md`

### Testes
- [x] Verificada a branch esperada via `.git/HEAD`.
- [x] Busca estatica nos arquivos do PR por `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType`, `StreamingAssets`, `JsonUtility`, `File.` e `Directory.` sem ocorrencias.
- [x] Conferida publicacao de `InventoryChangedEvent(itemId, delta, newAmount)` e `GoldChangedEvent(delta, newTotal)` conforme eventos existentes.
- [x] Conferido que os arquivos alterados ficam dentro do escopo permitido do PR-006.
- [ ] Unity nao executado nesta sessao.
- [ ] Compilacao C# local nao executada: ferramentas `git`, `dotnet`, `csc` e `msbuild` nao estao disponiveis no PATH do terminal.

### Pendencias / riscos
- Abrir Unity e confirmar Console sem erro vermelho.
- Teste manual: em uma cena temporaria nao salva, criar `GameBootstrap` e managers, arrastar `PlayerData.asset` e `ItemDatabase.asset`, entrar em Play Mode e confirmar managers inicializados.
- Como o inventario interno e um dicionario por ID, o `MaxStack` atual limita a quantidade total por item neste PR; expansao para multiplos slots/stacks fica para PR futuro se necessario.
- Nao salvar cena/prefab neste PR.

### Proximo passo recomendado
- Validar PR-006 no Unity antes de iniciar PR-007/FarmScene minima ou a proxima fatia definida no plano.

## 2026-05-17 - PR-007 FarmScene minima

**Responsavel:** Codex
**Branch:** feature/fase8-pr-007-farmscene-minima
**Escopo:** criar uma FarmScene minima e reproduzivel via Editor script, com bootstrap, managers, player placeholder, camera e limites basicos, sem gameplay.

### Alteracoes
- Criado Editor script `CreateMvpFarmScene.cs` em `Assets/_Game/Scripts/Editor/SceneCreation/`.
- Adicionado menu `CindarsHope/Scenes/Create MVP FarmScene`.
- O Editor script cria uma cena vazia, adiciona `_Bootstrap`, `Player`, `Ground`, `Bounds` e `Main Camera`.
- `_Bootstrap` recebe `GameBootstrap`, `PlayerManager`, `InventoryManager`, `TimeManager` e `SaveManager`.
- O Editor script configura referencias serializadas do `GameBootstrap` para os managers criados.
- O Editor script tenta carregar `PlayerData.asset` e `ItemDatabase.asset`; se nao encontrar, loga warning claro.
- Criada `Assets/_Game/Scenes/FarmScene.unity` com hierarquia minima e referencias principais.
- Player e Ground usam `SpriteRenderer` placeholder sem sprite asset final; a substituicao por arte/tilemap fica pendente para PR futuro.
- Nao houve gameplay, movimento, input, interacao, plantio, UI, prefab ou save/load JSON.

### Arquivos alterados
- `Assets/_Game/Scenes.meta`
- `Assets/_Game/Scenes/FarmScene.unity`
- `Assets/_Game/Scenes/FarmScene.unity.meta`
- `Assets/_Game/Scripts/Editor/SceneCreation.meta`
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs.meta`
- `PROJECT_LOG.md`

### Testes
- [x] Verificada a branch esperada via `.git/HEAD`.
- [x] Busca estatica em `CreateMvpFarmScene.cs` e `FarmScene.unity` por `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType`, `PlayerController`, `JsonUtility`, `StreamingAssets`, `Canvas`, `TextMeshPro`, `UnityEngine.UI` e APIs de input sem ocorrencias.
- [x] Verificada presenca estatica de `_Bootstrap`, `Player`, `Ground`, `Bounds`, `Top`, `Bottom`, `Left`, `Right` e `Main Camera` na cena.
- [x] Verificadas referencias serializadas de `GameBootstrap` para managers, `PlayerData.asset` e `ItemDatabase.asset` no arquivo de cena.
- [ ] Unity nao conseguiu executar o menu em batchmode porque ja havia outra instancia do Unity com este projeto aberta.
- [ ] Validacao visual/Play Mode ainda precisa ser executada manualmente no Editor.
- [ ] `git status` nao executado: `git` nao esta disponivel no PATH do terminal.

### Pendencias / riscos
- Rodar manualmente `CindarsHope/Scenes/Create MVP FarmScene` no Unity aberto para deixar a cena serializada diretamente pelo Editor local.
- Confirmar se Sorting Layers `Ground` e `Characters` existem; se nao existirem, o Editor script usa default e loga warning.
- Substituir placeholders sem sprite por sprites/tilemap reais em PR futuro de arte/cena.
- Nao salvar prefab nem adicionar UI neste PR.

### Proximo passo recomendado
- Abrir Unity, rodar o menu `CindarsHope/Scenes/Create MVP FarmScene`, abrir `Assets/_Game/Scenes/FarmScene.unity`, entrar em Play Mode e confirmar Console sem erro vermelho antes do PR-008.

## 2026-05-17 - PR-007 ajuste sprite builtin placeholder

**Responsavel:** Codex
**Branch:** feature/fase8-pr-007-farmscene-minima
**Escopo:** corrigir o Editor script da FarmScene minima para que Player e Ground usem sprite builtin visivel quando disponivel.

### Alteracoes
- `GetBuiltinSprite()` agora tenta carregar `AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd")`.
- Se o sprite builtin nao for encontrado, o Editor script mantem warning claro.
- Player e Ground continuam usando `SpriteRenderer` com cores placeholder e ficam visiveis quando o sprite builtin existir.
- Nenhum asset novo de sprite foi criado.
- Nenhum runtime, dado, cena manual, prefab, UI, movimento, input ou gameplay foi alterado.

### Arquivos alterados
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`
- `PROJECT_LOG.md`

### Testes
- [x] Verificada a branch esperada via `.git/HEAD`.
- [x] Busca estatica confirmou uso de `AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd")`.
- [x] Busca estatica no Editor script por `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType`, `PlayerController`, `JsonUtility`, `StreamingAssets`, `Canvas`, `TextMeshPro`, `UnityEngine.UI` e APIs de input sem ocorrencias.
- [x] Confirmado que `casesensitivetest` nao existe no workspace.
- [ ] `.claude` e `cindars_hope.slnx` existem localmente, mas nao foram alterados nesta tarefa.
- [ ] Unity nao executado nesta sessao.
- [ ] `git status` nao executado: `git` nao esta disponivel no PATH do terminal.

### Pendencias / riscos
- Rodar manualmente `CindarsHope/Scenes/Create MVP FarmScene` no Unity aberto para recriar a cena com sprite builtin visivel.
- Revisar antes de commit para garantir que `.claude/**` e `cindars_hope.slnx` nao entrem no PR.

### Proximo passo recomendado
- Abrir Unity, rodar o menu de recriacao da FarmScene e confirmar visualmente Player e Ground.

## 2026-05-17 Ã¢â‚¬â€ RevisÃ£o pÃ³s-PR-007 e gaps futuros

**ResponsÃ¡vel:** Codex/ChatGPT
**Branch:** feature/fase8-pr-007-farmscene-minima
**Escopo:** limpar PR-007, corrigir placeholder visual e registrar decisÃµes sobre gaps futuros.

### AlteraÃ§Ãµes
- PR-001 a PR-006 nÃ£o precisam ser refeitos.
- PR-007 precisa remover arquivos fora de escopo antes do merge.
- `GetBuiltinSprite()` deixava Player/Ground invisÃ­veis quando retornava `null`; foi corrigido para tentar carregar `AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd")`.
- Se o sprite builtin nÃ£o for encontrado, o Editor script mantÃ©m warning claro.
- Nenhum asset novo de sprite foi criado.
- Nenhum tilemap, `PlayerController`, movimento, input, gameplay, UI, prefab, dado runtime ou doc de design foi alterado.

### DecisÃµes sobre gaps futuros
- Eventos ausentes nÃ£o bloqueiam PR-007, mas devem ser tratados antes dos PRs que dependem deles.
- Antes de movimento/input: definir dono de `PlayerInputActions.inputactions` e validar pacotes Input System/Cinemachine/2D Extras se aplicÃ¡vel.
- Antes de fome: adicionar `PlayerStepEvent`, `HungerCriticalEvent`, `HungerEmptyEvent` e `HPChangedEvent`.
- Antes de save/load: alinhar `PlayerSaveData` canÃƒÂ´nico entre `CORE_CONTRACTS` e `FASE7`.
- Antes de Ã¡rvores: decidir `TreeDataSO`/`TreeDatabaseSO`.
- VFX nÃ£o deve ser dependÃªncia obrigatÃ³ria dos sistemas MVP; feedbacks podem ficar para PR dedicado.
- `FishingSpot` nÃ£o precisa ser salvo no MVP; registrar como decisÃ£o quando chegar em pesca/save.

### Arquivos alterados
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`
- `PROJECT_LOG.md`

### Testes
- [x] Verificada a branch esperada via `.git/HEAD`.
- [x] Verificado que `GetBuiltinSprite()` tenta carregar `UI/Skin/UISprite.psd`.
- [x] Confirmado que `casesensitivetest` nÃ£o existe no workspace.
- [ ] `.claude/**` e `cindars_hope.slnx` existem localmente; nÃ£o foram alterados nesta tarefa e devem ser mantidos fora do PR/diff antes do merge.
- [ ] Unity nÃ£o executado nesta sessÃ£o.
- [ ] `git status` nÃ£o executado: `git` nÃ£o estÃ¡ disponÃ­vel no PATH do terminal.

### PendÃªncias / riscos
- Rodar manualmente `CindarsHope/Scenes/Create MVP FarmScene` no Unity aberto para confirmar Player e Ground visÃ­veis.
- Antes do merge, revisar o diff em um ambiente com `git` disponÃ­vel e garantir que sÃ³ entrem FarmScene, `CreateMvpFarmScene`, metas correspondentes e `PROJECT_LOG.md`.

### PrÃ³ximo passo recomendado
- Validar PR-007 no Unity com Console sem erro vermelho antes de seguir para movimento/input.

## 2026-05-17 - PR-008 Core contracts hardening

**Responsavel:** Codex
**Branch atual:** dev
**Branch esperada:** feature/fase8-pr-008-core-contracts-hardening
**Escopo:** adicionar contratos de eventos core exigidos por PRs futuros, sem implementar sistemas, gameplay, input, UI, cena ou save/load.

### Alteracoes
- PR-008 adiciona contratos de eventos para PRs futuros.
- Criado `CropReadyEvent` com `SeedId`, `TilePosition` e `DaysGrown`.
- Criado `PlayerStepEvent` com `Position` e `DistanceSinceLastStep`.
- Criados `HungerCriticalEvent` e `HungerEmptyEvent` com `CurrentHunger` e `MaxHunger`.
- Criado `ItemPickedUpEvent` com `ItemId` e `Amount`.
- Criado `PlayerRespawnedEvent` com `Position`, `CurrentHP` e `GoldLost`.
- Criado `HPChangedEvent` com `Delta`, `CurrentHP` e `MaxHP`.
- PR-001 a PR-007 nao foram refeitos.
- Nenhum runtime system novo foi criado.
- Nenhum gameplay, input, movimento, UI, cena, prefab, data asset ou save/load foi implementado.
- `GameEventBus` nao foi alterado.

### Decisoes operacionais
- `PlayerInputActions.inputactions` sera dono do PR de movimento/input.
- Antes de movimento/input, validar pacotes Unity necessarios: Input System e, se usado, Cinemachine.
- `PlayerSaveData` canonico sera decidido antes do PR de save/load.
- VFX nao sera dependencia obrigatoria dos sistemas MVP; feedbacks visuais podem ficar para PR dedicado.
- `FishingSpot` nao precisa ser salvo no MVP.
- `TreeDataSO`/`TreeDatabaseSO` sera decidido antes do PR de arvores.

### Arquivos alterados
- `Assets/_Game/Scripts/Core/Events/CropReadyEvent.cs`
- `Assets/_Game/Scripts/Core/Events/CropReadyEvent.cs.meta`
- `Assets/_Game/Scripts/Core/Events/PlayerStepEvent.cs`
- `Assets/_Game/Scripts/Core/Events/PlayerStepEvent.cs.meta`
- `Assets/_Game/Scripts/Core/Events/HungerCriticalEvent.cs`
- `Assets/_Game/Scripts/Core/Events/HungerCriticalEvent.cs.meta`
- `Assets/_Game/Scripts/Core/Events/HungerEmptyEvent.cs`
- `Assets/_Game/Scripts/Core/Events/HungerEmptyEvent.cs.meta`
- `Assets/_Game/Scripts/Core/Events/ItemPickedUpEvent.cs`
- `Assets/_Game/Scripts/Core/Events/ItemPickedUpEvent.cs.meta`
- `Assets/_Game/Scripts/Core/Events/PlayerRespawnedEvent.cs`
- `Assets/_Game/Scripts/Core/Events/PlayerRespawnedEvent.cs.meta`
- `Assets/_Game/Scripts/Core/Events/HPChangedEvent.cs`
- `Assets/_Game/Scripts/Core/Events/HPChangedEvent.cs.meta`
- `PROJECT_LOG.md`

### Testes
- [x] Verificado via `.git/HEAD` que o workspace esta em `dev`; a branch esperada do PR e `feature/fase8-pr-008-core-contracts-hardening`.
- [x] Busca estatica nos novos eventos por `GameObject`, `ScriptableObject`, `MonoBehaviour`, `Transform`, `FindObject`, `Publish`, `Subscribe`, `JsonUtility`, `File.`, `Directory.`, `Input`, `Canvas` e `PlayerController` sem ocorrencias.
- [x] Conferido que os novos eventos usam namespace `CindarsHope.Core.Events`.
- [x] Conferido que os novos eventos sao `readonly struct` com tipos simples ou Unity structs leves (`Vector2`, `Vector2Int`).
- [ ] Unity nao executado nesta sessao.
- [ ] Compilacao C# local nao executada: ferramentas `git`, `dotnet`, `csc` e `msbuild` nao estao disponiveis no PATH do terminal.

### Pendencias / riscos
- Antes do merge, garantir que as alteracoes estejam na branch `feature/fase8-pr-008-core-contracts-hardening`.
- Abrir Unity e confirmar Console sem erro vermelho.
- Antes de cada PR futuro, confirmar se o evento novo e suficiente para a spec correspondente sem ampliar payload indevidamente.

### Proximo passo recomendado
- Validar PR-008 no Unity e, depois, seguir para a fatia de movimento/input com dono claro para `PlayerInputActions.inputactions` e pacotes Unity confirmados.

## 2026-05-17 - PR-009 Docs sync pos PR-008

**Responsavel:** Codex
**Branch:** docs/fase8-pr-009-sync-pos-pr008
**Escopo:** sincronizar documentos operacionais apos PR-001 a PR-008, sem alterar codigo, assets, cenas, prefabs ou gameplay.

### Alteracoes
- Atualizado o estado atual consolidado com tabela de PR-001 a PR-009 e proximo PR runtime.
- Atualizado o proximo passo recomendado para validar Unity com PR-007/PR-008, confirmar pacotes de input/camera e definir dono de `PlayerInputActions.inputactions`.
- Adicionada validacao pos-PR-007/PR-008 no smoke test do `PROJECT_LOG.md`.
- `CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md` agora registra os eventos criados no PR-008.
- `CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md` marca `PlayerSaveData` deste documento como canonico para o MVP.
- Registradas decisoes de que `FishingSpot` nao e persistido no MVP e que `TreeDataSO`/`TreeDatabaseSO` serao definidos antes do PR de arvores.
- `FASE8_EXECUTION_PLAN_CODEX_v1.0.md` recebeu prerequisitos para movimento/input e politica de VFX.
- PR documental: nenhum runtime, asset, cena, prefab, UI ou gameplay foi alterado.

### Arquivos alterados
- `PROJECT_LOG.md`
- `docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md`
- `docs/FASE8_EXECUTION_PLAN_CODEX_v1.0.md`

### Testes
- [x] Verificada a branch esperada via `.git/HEAD`.
- [x] As edicoes desta tarefa foram aplicadas apenas nos tres Markdown permitidos.
- [ ] `Assets` contem arquivos alterados recentemente de PR anterior; sem `git status`, nao foi possivel validar o diff final por arquivo.
- [ ] Unity nao executado nesta sessao; PR-009 nao altera runtime.
- [ ] `git status` nao executado: `git` nao esta disponivel no PATH do terminal.

### Pendencias / riscos
- Rodar validacao Unity pos-PR-007/PR-008 antes do proximo PR runtime.
- Confirmar `com.unity.inputsystem` e `com.unity.cinemachine`, se Cinemachine for usado.

### Proximo passo recomendado
- Iniciar o proximo PR runtime de movimento/input com `PlayerInputActions.inputactions` sob dono explicito do proprio PR.

## 2026-05-17 - PR-010 PlayerInputActions e PlayerController minimo

**Responsavel:** Codex
**Branch:** feature/fase8-pr-010-player-input-movement
**Escopo:** criar input e movimento minimo do jogador na FarmScene, usando `PlayerDataSO.MoveSpeed` e publicando `PlayerStepEvent`, sem gameplay adicional.

### Alteracoes
- Criada pasta `Assets/_Game/Input`.
- Criado `PlayerInputActions.inputactions` com Action Map `Player` e actions `Move`, `Interact`, `Inventory` e `Sleep`.
- `PlayerController` criado em `CindarsHope.Player`, com movimento por `Rigidbody2D.MovePosition`.
- `PlayerController` usa `PlayerDataSO.MoveSpeed` como fonte de velocidade, com fallback seguro de 5 quando o asset nao estiver atribuido.
- Diagonal e normalizada para evitar aceleracao.
- `PlayerStepEvent` e publicado a cada 1 unidade aproximada percorrida.
- `CreateMvpFarmScene` agora cria `Player` com `Rigidbody2D`, `BoxCollider2D` e `PlayerController`.
- `CreateMvpFarmScene` atribui `PlayerData.asset` e o `Rigidbody2D` ao `PlayerController` quando o asset e encontrado.
- Apenas `Move` e usado neste PR; `Interact`, `Inventory` e `Sleep` ficaram definidos para PRs futuros.
- Nao houve interacao, plantio, colheita, pesca, venda, UI, prefab, save/load, alteracao de eventos ou alteracao de `GameEventBus`.

### Observacao sobre Input System
- `Packages/manifest.json` nao contem `com.unity.inputsystem`.
- `Packages` nao foi alterado neste PR.
- Para manter compilacao possivel no estado atual, `PlayerController` usa Input System apenas quando `ENABLE_INPUT_SYSTEM` existir e mantem fallback por teclado legado.
- Instalar/ativar `com.unity.inputsystem` deve ser ajuste separado antes de validar o fluxo final com New Input System.

### Arquivos alterados
- `Assets/_Game/Input.meta`
- `Assets/_Game/Input/PlayerInputActions.inputactions`
- `Assets/_Game/Input/PlayerInputActions.inputactions.meta`
- `Assets/_Game/Scripts/Player/PlayerController.cs`
- `Assets/_Game/Scripts/Player/PlayerController.cs.meta`
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`
- `PROJECT_LOG.md`

### Testes
- [x] Verificada a branch esperada via `.git/HEAD`.
- [x] Confirmado que `com.unity.inputsystem` nao existe em `Packages/manifest.json`.
- [x] Busca estatica nos arquivos do PR por `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType`, `StreamingAssets`, `JsonUtility`, `File.`, `Directory.`, `InteractionSystem`, `IInteractable`, `Canvas`, `UnityEngine.UI`, `TextMeshPro`, `Cinemachine`, `Save(` e `Load(` sem ocorrencias.
- [x] Conferido que `CreateMvpFarmScene` adiciona `Rigidbody2D`, `BoxCollider2D` e `PlayerController` ao `Player`.
- [x] Conferido que `PlayerController` publica `PlayerStepEvent`.
- [ ] Unity nao executado nesta sessao.
- [ ] Compilacao C# local nao executada: ferramentas `git`, `dotnet`, `csc` e `msbuild` nao estao disponiveis no PATH do terminal.

### Pendencias / riscos
- Instalar/ativar `com.unity.inputsystem` em PR separado ou etapa propria antes de depender do New Input System.
- Rodar `CindarsHope/Scenes/Create MVP FarmScene` no Unity para recriar a cena.
- Entrar em Play Mode e validar WASD/setas, diagonal normalizada e Console sem erro vermelho.
- Revisar o `.meta` de `PlayerInputActions.inputactions` apos instalar o Input System, pois o pacote pode atualizar o importer.

### Proximo passo recomendado
- Validar PR-010 no Unity. Depois, seguir para interacao generica somente apos confirmar o pacote de input e o movimento basico.

## 2026-05-17 - PR-010 ajuste documental Input System

**Responsavel:** Codex
**Branch:** feature/fase8-pr-010-player-input-movement
**Escopo:** registrar explicitamente a decisao de fallback legacy no `PlayerController`, sem alterar comportamento de movimento nem instalar pacote.

### Alteracoes
- Adicionado comentario em `PlayerController.cs` explicando que o PR-010 mantem fallback legacy para permitir movimento sem alterar `Packages`/`ProjectSettings`.
- Registrado que `PlayerInputActions.inputactions` ja existe como contrato para PR futuro de migracao/pacote de input.
- Movimento continua igual: `Move` atual funciona via fallback legacy quando `ENABLE_INPUT_SYSTEM` nao esta ativo.
- Instalar/configurar `com.unity.inputsystem` fica para PR futuro especifico.
- Nenhum `Packages`, `ProjectSettings`, data asset, UI, plantio, interacao, save/load ou gameplay adicional foi alterado.

### Arquivos alterados
- `Assets/_Game/Scripts/Player/PlayerController.cs`
- `PROJECT_LOG.md`

### Testes
- [x] Ajuste restrito aos arquivos permitidos.
- [x] Confirmado que o comportamento de movimento nao foi alterado, apenas comentario/documentacao.
- [ ] Unity nao executado nesta sessao.
- [ ] `git status` nao executado: `git` nao esta disponivel no PATH do terminal.

### Pendencias / riscos
- Instalar/ativar `com.unity.inputsystem` em PR futuro dedicado antes de remover o fallback legacy.

### Proximo passo recomendado
- Validar movimento em Play Mode com o fallback atual e planejar PR especifico para configurar o Input System.

## 2026-05-17 - PR-011 Runtime State Hardening

**Responsavel:** Codex
**Branch:** feature/fase8-pr-011-runtime-state-hardening
**Escopo:** preparar `InventoryManager`, `PlayerManager` e `PlayerDataSO` para proximos PRs de load, fome e sistemas runtime, sem gameplay novo.

### Alteracoes
- `InventoryManager` agora pode receber `ItemDatabaseSO` sem depender de `PlayerDataSO`, via `Initialize(ItemDatabaseSO itemDatabase)`.
- `InventoryManager` agora expoe `HasItemDatabase`.
- `InventoryManager` agora expoe `IsKnownItem(string itemId)`.
- `InventoryManager.InitializeFromStartingItems` passa a chamar `Initialize(itemDatabase)`, depois `Clear()`, e entao processar `StartingItems`.
- `PlayerManager` agora expoe `MaxHP`.
- `PlayerManager.Initialize(PlayerDataSO)` define `MaxHP = Mathf.Max(1, playerData.BaseHP)` e `CurrentHP = MaxHP`.
- `PlayerManager.Initialize(PlayerDataSO)` publica `HPChangedEvent` no boot com `delta = CurrentHP`, `CurrentHP` e `MaxHP`.
- `PlayerDataSO` agora possui configuracoes de fome para PR futuro: `MaxHunger`, `StartingHunger`, `StepsPerHungerTick` e `HungerLossPerTick`.
- `PlayerDataSO.OnValidate` valida os campos de fome: `MaxHunger >= 1`, `StartingHunger` entre `0` e `MaxHunger`, `StepsPerHungerTick >= 1` e `HungerLossPerTick >= 1`.
- `PlayerData.asset` atualizado com `MaxHunger = 100`, `StartingHunger = 100`, `StepsPerHungerTick = 10` e `HungerLossPerTick = 1`.
- Convencao de sementes registrada: `ItemDataSO` de semente e `SeedDataSO` compartilham o mesmo Id, por exemplo `seed_wheat`; isso permite resolver plantio por ID do item no inventario usando `SeedDatabaseSO`.
- `TreeDataSO`/`TreeDatabaseSO` fica para PR de arvores.
- Testes unitarios ficam para PR futuro de hardening/testes.
- Nao houve gameplay novo, UI, plantio, interacao, fome funcional ou save/load.

### Arquivos alterados
- `Assets/_Game/Scripts/Inventory/InventoryManager.cs`
- `Assets/_Game/Scripts/Player/PlayerManager.cs`
- `Assets/_Game/Scripts/Player/Data/PlayerDataSO.cs`
- `Assets/_Game/Data/Config/PlayerData.asset`
- `PROJECT_LOG.md`

### Testes
- [x] Verificada a branch esperada via `.git/HEAD`.
- [x] Conferida assinatura de `HPChangedEvent(int delta, int currentHP, int maxHP)`.
- [x] Busca estatica nos arquivos alterados por `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType`, `StreamingAssets`, `JsonUtility`, `File.`, `Directory.`, `LoadFromSave`, `TakeDamage`, `Heal`, `Respawn`, `IInteractable`, `InteractionSystem`, `Canvas`, `UnityEngine.UI` e `TextMeshPro` sem ocorrencias.
- [x] Conferidos campos de fome no `PlayerData.asset`.
- [ ] Unity nao executado nesta sessao.
- [ ] Compilacao C# local nao executada: ferramentas `git`, `dotnet`, `csc` e `msbuild` nao estao disponiveis no PATH do terminal.

### Pendencias / riscos
- Abrir Unity para reimportar `PlayerDataSO` e confirmar `PlayerData.asset` sem perda de referencias.
- Rodar `CindarsHope/Validate/Validate MVP Data` apos reimport.
- Testes unitarios de inventario/player ficam para PR futuro.

### Proximo passo recomendado
- Validar PR-011 no Unity; depois seguir para o proximo PR runtime mantendo fome funcional, save/load e arvores em fatias separadas.

---

## 2026-05-17 â€” DOC Sync pÃ³s PR-011

**ResponsÃ¡vel:** Codex/ChatGPT
**Branch:** `docs/fase8-sync-pos-pr011`
**Escopo:** sincronizaÃ§Ã£o documental operacional apÃ³s PR-011, sem alterar runtime, assets, cenas, prefabs ou gameplay.

### AlteraÃ§Ãµes
- Atualizado o topo do `PROJECT_LOG.md` para registrar PR-001 a PR-011 como mergeados em `dev`.
- Registrado `PR-012 â€” Interaction System mÃ­nimo` como prÃ³ximo PR runtime.
- Registrado que o plano histÃ³rico em `docs/FASE8_EXECUTION_PLAN_CODEX_v1.0.md` preserva numeraÃ§Ã£o antiga e que a fonte operacional atual Ã© o topo do `PROJECT_LOG.md`.
- Atualizado `docs/FASE8_EXECUTION_PLAN_CODEX_v1.0.md` com nota pÃ³s PR-011.
- Corrigidos caracteres quebrados Ã³bvios nos blocos operacionais atualizados do `PROJECT_LOG.md`.

### Arquivos alterados
- `PROJECT_LOG.md`
- `docs/FASE8_EXECUTION_PLAN_CODEX_v1.0.md`

### Testes
- [x] RevisÃ£o textual dos blocos atualizados.
- [x] Verificado que o escopo ficou restrito aos documentos permitidos.
- [ ] Unity nÃ£o executado; alteraÃ§Ã£o Ã© documental.

### PendÃªncias / riscos
- Validar Unity apÃ³s PR-011 antes de iniciar o PR-012.
- O PR-012 nÃ£o deve implementar plantio, UI final ou save/load.

### PrÃ³ximo passo recomendado
- Executar `PR-012 â€” Interaction System mÃ­nimo` apÃ³s validaÃ§Ã£o local no Unity.

---

## 2026-05-17 â€” PR-012 Interaction System mÃ­nimo

**ResponsÃ¡vel:** Codex/ChatGPT
**Branch:** `feature/fase8-pr-012-interaction-system`
**Escopo:** base runtime genÃ©rica de interaÃ§Ã£o por tecla E e trigger, sem gameplay especÃ­fico.

### AlteraÃ§Ãµes
- Criada interface `IInteractable` em `CindarsHope.Interaction`.
- Criado `InteractionSystem` com detecÃ§Ã£o de interagÃ­veis prÃ³ximos por trigger 2D e interaÃ§Ã£o via `Input.GetKeyDown(KeyCode.E)`.
- Criado `DebugInteractable` apenas para teste manual de interaÃ§Ã£o.
- Atualizado `CreateMvpFarmScene` para criar `InteractionTrigger` filho do Player, adicionar `InteractionSystem` ao Player e criar `DebugInteractable` prÃ³ximo ao inÃ­cio da cena.
- Atualizado o status consolidado para registrar o PR-012 nesta branch.

### Arquivos alterados
- `Assets/_Game/Scripts/Interaction/IInteractable.cs`
- `Assets/_Game/Scripts/Interaction/InteractionSystem.cs`
- `Assets/_Game/Scripts/Interaction/DebugInteractable.cs`
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`
- `PROJECT_LOG.md`

### Testes
- [x] RevisÃ£o estÃ¡tica dos arquivos alterados.
- [x] Verificado que o PR nÃ£o altera PlayerController, PlayerInputActions, GameEventBus, dados, prefabs, UI, save/load ou sistemas de gameplay especÃ­ficos.
- [ ] Unity nÃ£o executado neste terminal; validar compilaÃ§Ã£o e Play Mode no editor.

### PendÃªncias / riscos
- Confirmar no Unity que eventos de trigger 2D chegam ao `InteractionSystem` no Player usando o collider filho `InteractionTrigger`.
- O `DebugInteractable` Ã© somente objeto de teste criado pelo Editor script; nÃ£o representa plantio, pesca, venda ou Ã¡rvore.
- Input segue por fallback legacy `Input.GetKeyDown` atÃ© um PR futuro instalar/configurar o Input System.

### PrÃ³ximo passo recomendado
- Rodar `CindarsHope/Scenes/Create MVP FarmScene`, entrar em Play Mode, aproximar do `DebugInteractable` e apertar E.
- PrÃ³ximo PR recomendado: canteiros/plots ou preparaÃ§Ã£o de farm plot, conforme validaÃ§Ã£o do PR-012.

---

## 2026-05-17 — WAVE Fase 8 Farm Loop PR-013 a PR-017

**Responsável:** Codex/ChatGPT
**Branch:** `wave/fase8-farm-loop-013-017`
**Escopo:** primeiro loop agrícola jogável mínimo: plantar, avançar dias, crescer e colher via interação.

### Alterações
- PR-013: criado skeleton de canteiros com `FarmPlotState` e `FarmPlot`, além de 9 `FarmPlot` gerados pelo `CreateMvpFarmScene`.
- PR-014: adicionado plantio básico por interação, com seleção automática `seed_wheat` antes de `seed_carrot`, removendo 1 semente do `InventoryManager` e publicando `SeedPlantedEvent`.
- PR-015: adicionado avanço mínimo de dia com `TimeManager.AdvanceDay()` e `DayAdvanceInput` usando Tab, publicando `DayStartedEvent`.
- PR-016: adicionado crescimento por `DayStartedEvent`; crops em crescimento incrementam `DaysGrown`, ficam `Ready` ao atingir `GrowthDays` e publicam `CropReadyEvent`.
- PR-017: adicionado harvest básico; crops `Ready` adicionam itens de colheita no `InventoryManager`, publicam `CropHarvestedEvent` e resetam o canteiro.
- `CreateMvpFarmScene` configura `SeedDatabaseSO`, `InventoryManager`, `DayAdvanceInput` e cria os canteiros em grid 3x3.

### Arquivos alterados
- `Assets/_Game/Scripts/Farm/FarmPlot.cs`
- `Assets/_Game/Scripts/Farm/FarmPlotState.cs`
- `Assets/_Game/Scripts/Core/Time/TimeManager.cs`
- `Assets/_Game/Scripts/Core/Time/DayAdvanceInput.cs`
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`
- `PROJECT_LOG.md`

### Fora de escopo preservado
- Não houve UI final.
- Não houve save/load.
- Não houve venda.
- Não houve pesca.
- Não houve árvore.
- Não houve alteração em dados, input, PlayerController, InventoryManager, eventos, GameEventBus, packages ou ProjectSettings.

### Testes
- [x] Revisão estática dos arquivos alterados.
- [x] Verificado que os arquivos tocados ficam dentro do escopo permitido da wave.
- [x] Verificado que não há `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType` ou `StreamingAssets` nos arquivos alterados.
- [!] Checagem por timestamp encontrou arquivos fora do escopo com escrita recente (`Assembly-CSharp.csproj`, `Assets/_Game/Scenes/FarmScene.unity`, `ProjectSettings/SceneTemplateSettings.json`, `Temp/...` e arquivos do PR-012). Eles não foram editados por ferramenta nesta onda, mas exigem revisão com `git status` antes de commit/PR.
- [x] `git status --short` via `C:\Program Files\Git\cmd\git.exe` mostrou somente arquivos permitidos da wave.
- [ ] Unity não executado neste terminal; validar compilação, cena e Play Mode no editor.
- [ ] Commits por etapa não criados: `git` não estava no PATH durante a execução sequencial e não criei commits artificiais depois de localizar `git.exe` por caminho absoluto.

### Pendências / riscos
- Validar no Unity se o `InteractionSystem` do Player detecta os `FarmPlot` criados pelo editor script.
- Validar o loop completo: E para plantar, Tab para avançar dias, E para colher.
- Revisar novamente `git status` antes do PR para garantir que arquivos fora do escopo não entrem.
- A seleção de semente ainda é placeholder automática; UI/lista de escolha fica para PR futuro.
- Se o inventário estiver cheio para o item colhido, o plot permanece `Ready` e registra warning.

### Próximo passo recomendado
- Validar o loop agrícola no Unity.
- Depois decidir entre UI textual mínima de inventário/HUD ou venda.

---

## 2026-05-17 — WAVE Fase 8 Economy + Hunger + HUD PR-018 a PR-024

**Responsável:** Codex/ChatGPT
**Branch:** `wave/fase8-economy-hunger-hud-018-024`
**Escopo:** camada jogável mínima acima do loop agrícola: HUD debug, venda, fome por movimento e consumo de comida.

### Alterações
- PR-018: criado `DebugHud` com OnGUI para mostrar ouro, HP, fome, inventário e comandos básicos.
- PR-019: criado `SellPoint` interagível para ponto de venda simples.
- PR-020: adicionada venda automática de `item_crop_wheat`, `item_crop_carrot`, `item_fish_common` e `item_wood`, usando `ItemDataSO.BaseValue` via `InventoryManager.TryGetItemData`.
- PR-021: criado `HungerManager`, inicializado por `PlayerDataSO`, assinando `PlayerStepEvent` e publicando `HungerChangedEvent`, `HungerCriticalEvent` e `HungerEmptyEvent`.
- PR-022: criado `FoodConsumer` com tecla H para consumir automaticamente cenoura, trigo ou peixe comum usando `ItemDataSO.HungerRestore`.
- PR-023: HUD integrado com ouro, HP, fome, inventário e comandos E/Tab/H.
- PR-024: `CreateMvpFarmScene` agora configura `_Bootstrap` com `HungerManager` e `FoodConsumer`, cria `SellPoint` e cria `DebugHud` com referências serializadas.
- Correção pós-teste Unity: `InteractionSystem` escolhia o primeiro interagível da lista, por ordem de entrada no trigger, causando plantio/interação no canteiro errado quando vários `FarmPlot` estavam próximos. Agora `GetBestCandidate` escolhe o candidato mais próximo do centro do trigger/player.

### Arquivos alterados
- `Assets/_Game/Scripts/UI/DebugHud.cs`
- `Assets/_Game/Scripts/Economy/SellPoint.cs`
- `Assets/_Game/Scripts/Economy/SellableItemPolicy.cs`
- `Assets/_Game/Scripts/Player/HungerManager.cs`
- `Assets/_Game/Scripts/Player/FoodConsumer.cs`
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`
- `Assets/_Game/Scripts/Inventory/InventoryManager.cs`
- `PROJECT_LOG.md`

### Fora de escopo preservado
- Não houve save/load.
- Não houve pesca.
- Não houve árvore.
- Não houve craft.
- Não houve UI final complexa, Canvas, TextMeshPro ou UnityEngine.UI.
- Não houve instalação/configuração de Input System ou Cinemachine.
- Não houve alteração em dados, PlayerController, eventos, GameEventBus, packages ou ProjectSettings.

### Testes
- [x] Revisão estática dos arquivos alterados.
- [x] Verificado que os arquivos tocados ficam dentro do escopo permitido da wave.
- [x] Verificado que não há `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType` ou `StreamingAssets` nos arquivos alterados.
- [x] `git status --short` via `C:\Program Files\Git\cmd\git.exe` mostrou somente arquivos permitidos desta wave.
- [ ] Unity não executado neste terminal; validar compilação, cena e Play Mode no editor.

### Pendências / riscos
- Validar no Unity se o HUD OnGUI aparece e atualiza ouro, fome e inventário em Play Mode.
- Validar venda no `SellPoint` depois de colher itens.
- Validar redução de fome ao andar e consumo com H.
- Revalidar interação com múltiplos `FarmPlot` dentro do trigger para confirmar que E atua no canteiro visualmente mais próximo.
- `DebugHud` é HUD temporário de debug, não UI final.

### Próximo passo recomendado
- Validar a wave no Unity.
- Depois decidir entre save/load ou pesca/árvores conforme resultado da validação.

---

## 2026-05-17 — WAVE Fase 8 Save/Load PR-025 a PR-030

**Responsável:** Codex/ChatGPT
**Branch:** `wave/fase8-save-load-025-030`
**Escopo:** save/load JSON mínimo do MVP para dia, player, fome, inventário, plots e posição do player.

### Alterações
- PR-025: criados contratos `GameSaveData`, `PlayerSaveData`, `InventorySaveData`, `InventoryItemSaveData`, `FarmSaveData` e `FarmPlotSaveData` com `SchemaVersion`.
- PR-026: adicionados export/import runtime em `InventoryManager`, `PlayerManager`, `HungerManager`, `TimeManager` e `FarmPlot`.
- PR-027: `SaveManager` agora salva/carrega JSON em `Application.persistentDataPath/saves/slot_1.json`, cria diretório `saves` e publica `GameSavedEvent` ao salvar.
- PR-028: criado `SaveInput` com F5 para salvar e F9 para carregar.
- PR-029: criado `FarmPlotRegistry` e integrado ao `CreateMvpFarmScene`; `SaveManager` recebe managers, registry e `Player.transform` por referência serializada.
- PR-030: `DebugHud` mostra comandos F5/F9 e `PROJECT_LOG.md` registra a wave.

### Arquivos alterados
- `Assets/_Game/Scripts/Save/SaveData.cs`
- `Assets/_Game/Scripts/Save/SaveManager.cs`
- `Assets/_Game/Scripts/Save/SaveInput.cs`
- `Assets/_Game/Scripts/Farm/FarmPlot.cs`
- `Assets/_Game/Scripts/Farm/FarmPlotSaveData.cs`
- `Assets/_Game/Scripts/Farm/FarmPlotRegistry.cs`
- `Assets/_Game/Scripts/Inventory/InventoryManager.cs`
- `Assets/_Game/Scripts/Player/PlayerManager.cs`
- `Assets/_Game/Scripts/Player/HungerManager.cs`
- `Assets/_Game/Scripts/Core/Time/TimeManager.cs`
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`
- `Assets/_Game/Scripts/UI/DebugHud.cs`
- `PROJECT_LOG.md`

### Fora de escopo preservado
- Não houve alteração em dados, eventos, GameEventBus, PlayerController, packages ou ProjectSettings.
- Não houve pesca, árvore, craft, Cinemachine, Input System package ou UI final complexa.
- Save usa IDs e tipos simples; referências Unity são usadas apenas como referências runtime/scene no `SaveManager`, não serializadas no JSON.

### Testes
- [x] Revisão estática dos arquivos alterados.
- [x] Busca estática por `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType` e `StreamingAssets` nos arquivos alterados sem ocorrências.
- [x] Busca estática nos contratos de save por `ScriptableObject`, `GameObject`, `Transform` e `MonoBehaviour` sem ocorrências.
- [ ] Unity não executado neste terminal; validar compilação, recriação da cena e Play Mode no editor.

### Pendências / riscos
- Validar se `JsonUtility` serializa/restaura `Vector2 PlayerPosition` corretamente no projeto Unity atual.
- Validar F5/F9 em Play Mode após plantar, colher, vender, andar e consumir comida.
- Validar que restore de plots não publica eventos agrícolas e não avança crescimento ao carregar.

### Próximo passo recomendado
- Validar save/load completo no Unity.
- Depois decidir entre pesca/árvores ou hardening de save/load após validação.
