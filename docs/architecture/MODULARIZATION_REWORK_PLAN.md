# Plano de Rework Modular — Cindar's Hope

> Documento operacional para modularizar o projeto sem alterar o comportamento existente.
>
> **Status:** Fase 0 concluída e publicada em `dev` (`8a887244`)
> atual ser commitado e publicado.
>
> **Branch planejada:** `rework/modular-architecture`
>
> **Regra central:** nenhuma etapa pode trocar arquitetura e gameplay no mesmo lote.

## 1. Objetivo

Transformar o monólito atual baseado em `Assembly-CSharp`, singletons, auto-bootstraps e acesso global
em uma arquitetura modular com dependências explícitas, compilação incremental e componentes
reutilizáveis, preservando integralmente:

- comportamento de gameplay;
- IDs estáveis e catálogos;
- saves existentes e migrations;
- referências serializadas do Unity;
- cenas, quantidades, posições e navegação;
- input e atalhos atuais;
- ordem observável de eventos;
- geração idempotente de dados e cenas;
- compatibilidade entre Codex e Claude Code.

O trabalho usa **Strangler Pattern**: as APIs atuais permanecem como fachadas compatíveis enquanto
implementações internas são extraídas e substituídas gradualmente.

## 2. Por que modularizar agora

Snapshot da auditoria de 2026-07-04, sujeito a drift enquanto o projeto continua em desenvolvimento:

- aproximadamente 1.342 arquivos C# e 144 mil linhas em `Assets/_Game/Scripts`;
- zero `.asmdef`;
- 51 arquivos com `RuntimeInitializeOnLoadMethod`;
- aproximadamente 66 acessores estáticos `Instance`/`Active`/`Current`;
- aproximadamente 911 usos textuais do `GameEventBus`;
- 34 arquivos runtime lendo `Input` diretamente;
- 90 mutações diretas de inventário/ouro;
- 16 componentes runtime usando `OnGUI`;
- 13 pares de dependência mútua entre áreas de primeiro nível;
- `GameBootstrap` depende diretamente de cerca de 12 domínios;
- `SaveManager` importa cerca de 14 domínios e registra dezenas de providers manualmente;
- qualquer alteração runtime tende a recompilar toda `Assembly-CSharp`;
- existem muitos testes EditMode, mas nenhum `UnityTest`/PlayMode automatizado.

Esses números justificam módulos, mas também tornam uma migração big-bang insegura.

## 3. Não objetivos

Este rework não autoriza:

- reescrever o jogo;
- trocar Unity por outra engine;
- migrar para ECS/DOTS;
- alterar balanceamento, itens iniciais, quantidades, IDs ou regras de gameplay;
- eliminar conteúdo visual para obter performance;
- renomear namespaces em massa;
- editar YAML de cena/prefab manualmente;
- alterar schema de save sem migration dedicada;
- introduzir framework externo de dependency injection;
- migrar todos os assets para Addressables de uma vez;
- consolidar os bootstraps existentes em uma única alteração;
- criar um `.asmdef` por pasta ou por classe.

## 4. Invariantes de não regressão

### 4.1 Unity e serialização

- Preservar todo arquivo `.meta` e GUID existente.
- Não mover ou recriar scripts sem mover seu `.meta` correspondente.
- Não renomear campos `[SerializeField]` sem `[FormerlySerializedAs]` e teste de cena/prefab.
- Não alterar `MonoBehaviour`/`ScriptableObject` serializado em cena sem busca de GUID e validação.
- Não editar `ProjectSettings/*.asset`, `.unity`, `.prefab` ou `.asset` manualmente quando houver
  gerador ou API de Editor canônica.
- Após mudança estrutural, reimportar no Unity e verificar missing scripts.

### 4.2 Save

- Carregar fixtures de saves de todas as versões ainda suportadas.
- Preservar nomes e tipos de campos de `GameSaveData`.
- Preservar ordem funcional de restore entre providers.
- Comparar semanticamente o save regravado; diferenças devem estar justificadas por migration.
- Nunca mover tipo persistido entre assemblies se seu nome qualificado for armazenado.

### 4.3 Gameplay

- Mesmos IDs, regras, custos, recompensas e gates.
- Mesmos eventos públicos e mesma ordem observável.
- Mesmos atalhos e bloqueios de input/modal.
- Mesmos seeds e resultados determinísticos da cave.
- Nenhuma remoção de fallback antes de a substituição estar materializada em todas as cenas.

### 4.4 Cenas e arte

- Farm, Town e Cave devem continuar abrindo sem erro.
- Geradores devem produzir cenas semanticamente equivalentes.
- Nenhuma alteração visual ou de layout entra em commits de modularização.
- Árvores, NPCs, casas, portais, lojas e interações permanecem acessíveis.

## 5. Arquitetura alvo

```text
CindarsHope.Foundation
    ↑
    ├── CindarsHope.Gameplay
    ├── CindarsHope.World
    └── CindarsHope.Save.Contracts
             ↑
       CindarsHope.Application
          ↑             ↑
CindarsHope.Adapters   CindarsHope.Presentation
          \             /
       CindarsHope.Composition

CindarsHope.Editor ── depende apenas dos módulos runtime necessários
CindarsHope.Tests  ── depende explicitamente dos módulos testados
```

### 5.1 `CindarsHope.Foundation`

Conteúdo permitido:

- IDs e value objects puros;
- `Result`/erros de domínio;
- hash determinístico;
- interfaces pequenas de clock, RNG, inventário, wallet, pause e asset resolution;
- DTOs/event contracts realmente compartilhados;
- utilitários sem dependência de gameplay concreto.

Conteúdo proibido:

- `GameBootstrap`;
- managers concretos;
- acesso a cenas;
- `Resources.Load`;
- input;
- lógica de UI;
- dependência em Farm, Combat, Cave, NPC, Save ou outros domínios.

Quando possível, usar `noEngineReferences`, mas somente depois de confirmar que nenhum tipo Unity é
necessário.

### 5.2 `CindarsHope.Gameplay`

Primeira fronteira ampla para evitar ciclos prematuros:

- Player;
- Inventory;
- Equipment;
- Combat;
- Economy;
- Craft/Crafting;
- Skills/Magic.

Só será subdividida quando o grafo interno estiver sem ciclos relevantes.

### 5.3 `CindarsHope.World`

- Farm;
- Cave;
- NPC/City;
- Quests/Narrative;
- World/Time;
- Locations.

Também começa amplo. Separar Farm/Cave/NPC cedo demais apenas converteria dependências implícitas em
ciclos de assemblies.

### 5.4 `CindarsHope.Application`

- casos de uso;
- commands;
- transactions/unit of work;
- coordenação entre domínios;
- fluxo de scene transition;
- readiness barrier;
- orchestration de save/load.

Não contém `MonoBehaviour` nem acessa `Input`, `Physics2D`, `Resources` ou `SceneManager` diretamente.

### 5.5 `CindarsHope.Adapters`

- adapters Unity;
- input de teclado;
- física;
- relógio Unity;
- RNG Unity;
- carregamento de assets;
- scene loading;
- bridges para APIs legadas.

### 5.6 `CindarsHope.Presentation`

- HUD e menus;
- presenters e view models;
- views Unity finas;
- tradução de input de UI para commands.

### 5.7 `CindarsHope.Composition`

- `GameBootstrap` e futuros composition roots;
- ordem explícita de inicialização;
- wiring de adapters, application services e views;
- nenhum cálculo de gameplay.

O namespace existente pode permanecer inicialmente. Assembly e namespace não precisam mudar juntos.

## 6. Estratégias e padrões que serão aplicados

### 6.1 Ports and Adapters

Criar interfaces pequenas e orientadas ao consumidor:

- `IGameClock`;
- `IRandomSource`;
- `IInventoryReader`;
- `IInventoryWriter`;
- `IWallet`;
- `ISceneTransitionService`;
- `IPauseService`;
- `IAssetResolver<T>`.

Managers atuais implementam ou recebem adapters. Não substituir todos de uma vez.

### 6.2 Composition Root

- Novos sistemas não podem criar novos auto-bootstraps independentes.
- Bootstraps existentes continuam funcionando até migração dedicada.
- Cada migração registra ordem anterior, dependências e cenário de PlayMode.
- Consolidar em lotes pequenos, nunca os 51 entry points simultaneamente.

### 6.3 Strategy Registry

Primeiro alvo: `EnemyActionRunner`.

- `EnemyActionType` resolve um `IEnemyActionExecutor`.
- Runner mantém seleção, cooldown, telegraph e lifecycle.
- Executor implementa apenas a família de ação.
- Manter switch legado como fallback temporário até cobertura total.

### 6.4 State + Presenter

Alvos:

- `NpcShopController`;
- `InventoryPanelController`;
- crafting modal;
- diálogo;
- death screen.

Extrair estado/transições para C# puro. `MonoBehaviour` permanece fachada e View.

### 6.5 Command + Unit of Work

Operações de inventário/economia devem ser preparadas, validadas e aplicadas atomicamente:

- remover/adicionar item;
- gastar/conceder ouro;
- publicar evento;
- registrar `reason` e `source`;
- rollback em falha.

Migrar crafting, shop, tempering, gifts e quest rewards gradualmente.

### 6.6 Event Channel tipado

Manter a API `GameEventBus.Publish/Subscribe` e trocar apenas a implementação:

- canal genérico por tipo;
- sem `Delegate` e sem `ToArray()` por publicação;
- mutações de subscriptions diferidas durante dispatch;
- `SubscriptionBag` para lifecycle;
- exceção em listener não interrompe os demais.

Testar publish recursivo, subscribe/unsubscribe durante dispatch, duplicatas e dispose idempotente.

### 6.7 Builder de cenas

Extrair duplicações dos três geradores:

- `SerializedReferenceWriter`;
- `SceneBootstrapBuilder`;
- `PlayerSceneBuilder`;
- `PortalBuilder`;
- helpers de sorting/layers.

Primeiro teste obrigatório: gerar antes/depois e provar equivalência das propriedades relevantes.

## 7. Sequência de execução

## Fase 0 — Baseline e proteção

### Entregáveis

- branch `rework/modular-architecture` criada a partir de baseline limpo e publicado;
- relatório de baseline de build/testes;
- lista de cenas, GUIDs e arquivos serializados sensíveis;
- fixtures de save suportadas;
- testes arquiteturais por baseline/ratchet;
- handoff vivo atualizado.

### Ratchets iniciais

Nenhum novo uso de:

- `RuntimeInitializeOnLoadMethod` fora da composition allowlist;
- singleton global;
- `GameObject.Find`/global search runtime;
- `Resources.Load` fora da allowlist;
- `Input.Get*` fora dos adapters;
- `Time.timeScale =` fora do pause adapter;
- `SceneManager.LoadScene` fora do router;
- `Physics2D.*All` em gameplay;
- acesso direto a ouro/inventário fora dos caminhos registrados.

O primeiro teste registra o baseline atual; não tenta corrigir tudo no mesmo commit.

### Gate

- builds runtime/editor;
- testes EditMode atuais;
- nenhum arquivo de gameplay alterado;
- relatório honesto de testes Unity/PlayMode não executados.

## Fase 1 — Correções locais antes da estrutura

Corrigir separadamente:

1. assinatura inválida de `CSharpProjectPostprocessor.OnGeneratedCSProject`;
2. telemetria `_blocks` nunca incrementada;
3. `Update()` vazio nas views de HUD;
4. `OverlapCircleAll` residual nos executores de skill;
5. warnings novos e comentários arquiteturais obsoletos.

Cada correção recebe teste próprio e commit próprio ou lote de risco equivalente.

## Fase 2 — Preparação para assemblies

### Tarefas

- localizar strings `Assembly-CSharp` e remover reflection hardcoded;
- substituir por `typeof`, resolução por interface ou busca controlada de assembly;
- mapear `internal` consumidos por testes/editor;
- mapear ciclos de namespace e dependências concretas;
- atualizar scripts de validação que assumem apenas dois `.csproj`;
- definir naming e referências permitidas.

### Gate

- comportamento ainda em assemblies predefinidas;
- build e testes continuam passando;
- nenhum `.asmdef` criado antes desse gate.

## Fase 3 — `CindarsHope.Foundation`

### Estratégia

- mover somente tipos puros ou criar novas fachadas puras;
- preservar namespaces inicialmente;
- mover `.cs` com `.meta` via Unity quando aplicável;
- criar primeiro `.asmdef` pequeno;
- manter `Assembly-CSharp` consumindo Foundation.

### Gate

- Unity recompila e gera assembly nova;
- runtime/editor/tests compilam;
- validators reconhecem a nova assembly;
- nenhuma referência hardcoded quebrada;
- cenas sem missing scripts;
- save fixture round-trip.

## Fase 4 — Composition Root incremental

### Estratégia

- criar `GameRuntimeCompositionRoot` para sistemas novos;
- manter auto-bootstraps existentes grandfathered;
- migrar um bootstrap por commit;
- declarar dependências e ordem;
- adicionar readiness state antes de restore de save.

### Gate por bootstrap

- teste de criação única;
- teste de lifecycle e unsubscribe;
- cena relevante abre;
- PlayMode do fluxo atingido;
- sem duplicação de serviço após transição de cena.

## Fase 5 — Módulos amplos

Ordem sugerida:

1. Gameplay;
2. World;
3. Save Contracts/Save;
4. Application;
5. Adapters;
6. Presentation;
7. Composition;
8. Editor;
9. Tests.

Editor e Tests não vêm primeiro: uma assembly customizada não pode depender diretamente de tipos que
continuam presos à assembly predefinida `Assembly-CSharp`.

### Gate por módulo

- grafo de referências sem ciclo;
- nenhuma referência a módulo de camada superior;
- API pública mínima;
- build/testes;
- validação Unity;
- documentação e handoff atualizados.

## Fase 6 — Reuso e desacoplamento interno

- Strategy para ações inimigas;
- State/Presenter para NPC shop e inventory;
- transaction pipeline para inventário/economia;
- providers de save registrados por descriptor tipado;
- `IGameClock` unificando consumo de dia/fase/hora sem fundir responsabilidades;
- RNG separado em Gameplay, World e Visual;
- pause/timeScale coordenado por tokens.

## Fase 7 — Performance medida

### Instrumentação

Adicionar `ProfilerMarker` em:

- event dispatch;
- NPC schedule;
- cave materialization/cleanup;
- save capture/serialize/write/restore;
- UI rebuild;
- scene transition/readiness.

### Otimizações somente após baseline

- pool de projectiles;
- pool de floating damage numbers;
- pool de rows de shop/dialogue;
- non-alloc physics restante;
- scene loading assíncrono central;
- Tilemap/chunking da floresta e cave com paridade visual;
- migração piloto de um grupo de `Resources` para referências diretas/Addressables.

### Critério

Nenhum lote pode piorar em mais de 5% a métrica medida relevante sem justificativa explícita e
aprovação humana.

## Fase 8 — UI e legado

- migrar um `OnGUI` por spec;
- manter feature parity e atalhos;
- eliminar polling vazio;
- views atualizadas por dirty flag/evento;
- remover fachadas legadas apenas quando busca de referência e PlayMode provarem ausência de uso.

## 8. Validação obrigatória

| Mudança | Build | EditMode | Unity validator | PlayMode | Save fixture | Profiler |
|---|---:|---:|---:|---:|---:|---:|
| Docs/ratchet | Sim | Quando aplicável | Não | Não | Não | Não |
| Correção local runtime | Sim | Sim | Quando aplicável | Fluxo afetado | Se toca estado | Não |
| Novo `.asmdef` | Sim | Sim | Sim | Smoke das 3 cenas | Sim | Compile time |
| Bootstrap | Sim | Sim | Sim | Obrigatório | Se participa do restore | Não |
| EventBus | Sim | Sim | Sim | Smoke amplo | Não | Obrigatório |
| Save | Sim | Sim | Sim | Obrigatório | Obrigatório | Marcadores |
| Pool/performance | Sim | Sim | Sim | Obrigatório | Se estado persistente | Obrigatório |
| UI | Sim | Sim | Sim | Obrigatório | Se tela altera estado | Quando relevante |

## 9. Política de commits

- Um risco arquitetural por commit.
- Commits em português.
- Nada de `git add -A` em working tree misto sem autorização humana explícita.
- Nunca misturar arte/conteúdo com modularização.
- Todo commit atualiza `CLAUDE_MODULARIZATION_HANDOFF.md` com:
  - o que foi feito;
  - arquivos alterados;
  - validações executadas;
  - resultado real;
  - pendências e próximo passo;
  - commit, quando disponível.
- Antes do push, working tree deve estar limpo ou o residual deve estar documentado.

## 10. Rollback

- Reverter o commit específico, nunca usar `reset --hard`.
- Restaurar fachada anterior se a implementação nova falhar.
- Não deletar caminho legado no mesmo commit que introduz o substituto.
- Para `.asmdef`, manter registro das referências anteriores e dos arquivos movidos.
- Para save, manter backup/fixture e migration reversível quando possível.
- Para cenas, regenerar pelos comandos canônicos e comparar antes de aceitar.

## 11. Critério de conclusão do rework

O rework só termina quando:

- runtime, editor e tests usam assemblies explícitas;
- grafo de assemblies não possui ciclos;
- `GameBootstrap` é composition root fino;
- novos sistemas não criam auto-bootstrap independente;
- domínios dependem de Foundation/Application contracts, não de managers concretos de outros domínios;
- input, scene loading, pause, RNG e asset loading passam por ports/adapters;
- EventBus não aloca por dispatch normal;
- transações críticas de inventário/economia são atômicas e auditáveis;
- saves antigos carregam;
- três cenas passam smoke PlayMode;
- performance não regrediu;
- documentação, ADRs, rules, skills de Codex e Claude refletem a arquitetura real.

## 12. Primeiro passo após o baseline atual

1. Confirmar remoto e working tree limpo.
2. Criar `rework/modular-architecture` a partir do `dev` publicado.
3. Executar Fase 0 sem alterar gameplay.
4. Commitar relatório/ratchets.
5. Atualizar o handoff.
6. Só então iniciar a Fase 1.

## 13. Decisão de execução da Fase 0

Em 2026-07-05, o responsável pelo projeto autorizou explicitamente executar e fechar a Fase 0 na
branch `dev`, em vez de criar `rework/modular-architecture` neste momento.

Esta decisão altera somente o destino dos commits da Fase 0. Permanecem obrigatórios:

- working tree limpo antes do início;
- sincronização com `origin/dev`;
- ausência de alterações em gameplay;
- baseline reproduzível e ratchets que aceitam a dívida existente, mas impedem dívida nova;
- inventário de cenas, GUIDs, serialização e saves sensíveis;
- builds, testes aplicáveis, documentação honesta e commits pequenos;
- atualização do handoff vivo antes de cada commit material.

As fases que introduzem `.asmdef`, movem scripts ou alteram composition roots continuam exigindo
uma nova decisão de branch antes de começar. A autorização desta seção não transforma o rework
completo em uma migração big-bang dentro de `dev`.

### Fechamento

A Fase 0 foi concluída em 2026-07-05 pelo commit `8a887244`, publicado em `origin/dev`. O gate
registrado em `docs/architecture/MODULARIZATION_PHASE0_BASELINE.md` inclui ratchets por arquivo,
proteção de GUIDs, fixtures de save v1–v5, builds e comparação da suíte EditMode.

Nenhuma fase seguinte está implicitamente autorizada nesta branch. O próximo lote deve começar por
uma decisão explícita sobre branch e pelo escopo da Fase 1.

## 14. Decisão de execução das Fases 1 e 2

Em 2026-07-05, após o fechamento da Fase 0, o responsável autorizou executar a Fase 1 na branch
`dev` e iniciar a Fase 2 na mesma branch caso todos os gates da Fase 1 sejam satisfeitos.

Condições desta autorização:

- cada correção da Fase 1 deve ser isolada, coberta por teste e registrada no handoff;
- nenhuma mudança pode alterar balanceamento, conteúdo, saves, cenas ou comportamento observável;
- a Fase 1 deve ser commitada e publicada antes do primeiro lote da Fase 2;
- a Fase 2 pode remover dependências hardcoded e preparar validadores/builds;
- nenhum `.asmdef` será criado nesta autorização, pois isso pertence ao gate posterior da Fase 3;
- qualquer falha nova de build/EditMode interrompe a progressão para a fase seguinte.
