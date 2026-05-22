# LLM Handoff Instructions — Cindar's Hope

Este arquivo orienta qualquer LLM/agente que continue o desenvolvimento do projeto.

## Ordem obrigatória de leitura

Antes de planejar ou alterar qualquer coisa, leia:

1. `PROJECT_LOG.md`
2. `AGENTS.md`
3. `CLAUDE.md`
4. `docs/FASE8_EXECUTION_PLAN_CODEX_v1.0.md`
5. `docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md`
6. `docs/FASE7_SPEC_MVP_FARM_v2.2.md`

Se houver divergência entre documentos antigos e estado real do repositório, considerar como fonte mais confiável:

1. código atual na branch `dev`;
2. `PROJECT_LOG.md` mais recente;
3. `AGENTS.md` / `CLAUDE.md`;
4. docs históricos.

## Branches

- `main`: base estável.
- `dev`: branch de desenvolvimento.
- `feature/*`, `fix/*`, `docs/*` ou `wave/*`: branches de trabalho.

Nunca trabalhar diretamente em `main`.

Preferir não trabalhar diretamente em `dev`, salvo tarefa documental explícita ou pedido direto do humano.

## Protocolo antes de alterar

Executar:

```powershell
git checkout dev
git pull origin dev
git status --short
```

Se houver arquivo modificado ou untracked inesperado, parar e reportar.

## Protocolo de tarefa

Toda tarefa deve ter:

- objetivo claro;
- branch esperada;
- arquivos permitidos;
- arquivos proibidos;
- Definition of Done;
- teste Unity esperado;
- atualização do `PROJECT_LOG.md`.

Não alterar arquivos fora do escopo.

## Arquivos normalmente proibidos salvo pedido explícito

- `Packages/**`
- `ProjectSettings/**`
- `Assets/_Game/Input/**`
- `Assets/_Game/Scripts/Core/Events/**`
- `Assets/_Game/Scripts/Core/GameEventBus.cs`
- `docs/**`, salvo tarefa documental
- `.claude/**`
- `*.sln`
- `*.slnx`

## Estado atual esperado pós PR-045

A branch `dev` contém:

- Event bus e eventos core.
- Dados via `ScriptableObject` e registries.
- Player movement com fallback legacy input.
- `InteractionSystem` com seleção do interagível mais próximo.
- Farm loop: plantar, avançar dia, crescer e colher.
- Economy/HUD: `SellPoint`, `SeedShopPoint`, `DebugHud`.
- Hunger: fome por passos e por avanço de dia.
- `FoodConsumer` por tecla `H`.
- Save/load JSON em `Application.persistentDataPath/saves/slot_1.json`.
- `FarmPlot` save/load.
- `TreeNode`/`TreeRegistry` save/load.
- `ItemPickup` persistente com `IsCollected`, posição e save/load.
- `FishingSpot` básico sem persistência.
- Data validator expandido para árvores.

## Próximo passo recomendado

Antes de novas features, executar uma wave de hardening/QA pós PR-045.

Validar no Unity:

- Console sem erro vermelho.
- `CindarsHope/Validate/Validate MVP Data` passa.
- `CindarsHope/Scenes/Create MVP FarmScene` executa.
- Player move.
- HUD mostra ouro, HP, fome, inventário, dia e prompt.
- Plantar/colher/vender funcionam.
- Comprar sementes funciona.
- Avançar dia consome fome.
- Fome zerada aplica consequência mínima sem quebrar.
- Cortar árvore adiciona madeira.
- Pescar adiciona peixe se tiver vara.
- Pickup persistente não duplica e volta corretamente no load se o save foi feito antes da coleta.
- `F5`/`F9` restaura dia, ouro, HP, fome, inventário, plots, árvores, pickups e posição.

## Regras técnicas

- Não usar `GameObject.Find`.
- Não usar `FindObjectOfType`.
- Não usar `FindObjectsByType`.
- Não usar `StreamingAssets`.
- Save deve usar IDs e tipos simples.
- Não serializar referências Unity em JSON.
- Não instalar Input System ou Cinemachine sem PR específico.
- Não criar UI final complexa enquanto o HUD debug atender.
- Sempre atualizar `PROJECT_LOG.md` ao final de tarefas relevantes.

## Encerramento de tarefa

Antes de commit:

```powershell
git status --short
git diff --stat
```

Depois de teste:

```powershell
git add <arquivos permitidos>
git commit -m "<mensagem>"
git push origin <branch>
```

Antes de merge, revisar diff contra `dev`.

Após merge em `dev`:

```powershell
git checkout dev
git pull origin dev
git status --short
```

## Observações atuais

- `PROJECT_LOG.md` ainda pode conter trechos antigos com mojibake ou status histórico desatualizado no topo. Quando houver dúvida, validar contra `dev` e contra as entradas mais recentes do próprio log.
- A próxima sessão deve começar com hardening/QA, não com nova macro-wave de features.
- Qualquer correção descoberta no Unity deve ser registrada no `PROJECT_LOG.md` e isolada em branch própria, salvo pedido explícito do humano.