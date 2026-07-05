# Prompt de Continuação para Claude — Rework Modular

Copie todo o conteúdo deste documento para uma nova sessão do Claude Code quando a sessão atual
estiver próxima do limite de contexto/tokens.

---

Você está continuando o rework modular do projeto Unity **Cindar's Hope**.

## Objetivo

Modularizar o projeto incrementalmente, aplicando DRY, SOLID, Ports and Adapters, Composition Root,
Strategy, State/Presenter, Command/Unit of Work e assemblies explícitas, sem alterar o comportamento
do jogo.

O plano autoritativo desta execução é:

`docs/architecture/MODULARIZATION_REWORK_PLAN.md`

Leia esse arquivo por completo antes de propor ou alterar qualquer código.

## Regra obrigatória de continuidade

Antes de continuar, **valide de forma independente tudo que o Codex afirma ter feito**. Não use este
handoff, mensagens anteriores ou relatórios como prova. Verifique no disco, no Git e nos resultados
reais dos comandos.

No início de toda nova sessão execute:

```powershell
git fetch origin dev
git branch --show-current
git status --short
git log --oneline -12
git rev-list --left-right --count origin/dev...HEAD
```

Pare e reporte se:

- a branch não for a esperada;
- houver alterações não documentadas neste handoff;
- o working tree estiver misturado com arte/conteúdo de outra tarefa;
- commits citados aqui não existirem;
- o código contradizer o plano;
- algum gate de validação produzir resultado diferente do registrado.

## Estado registrado pelo Codex

### Concluído nesta preparação

- Auditoria arquitetural e de performance realizada no working tree de 2026-07-04.
- Decisão humana recebida para iniciar modularização em branch dedicada.
- Plano detalhado criado em `docs/architecture/MODULARIZATION_REWORK_PLAN.md`.
- Este handoff vivo foi criado.
- O baseline completo foi consolidado no commit `a8fec139` e publicado em `origin/dev`.
- O push enviou 904 objetos Git LFS (150 MB) e terminou com exit 0.
- A validação do estado remoto ainda deve ser refeita no início da próxima sessão; não confie apenas
  neste registro.
- Preflight de 2026-07-05 confirmou branch `dev`, working tree limpo, `origin/dev...HEAD = 0 0` e
  HEAD `aed4f35c`.
- O responsável pelo projeto autorizou executar a Fase 0 na própria `dev`; a exceção foi registrada
  no plano e não autoriza iniciar as fases estruturais seguintes na mesma branch automaticamente.

### Ainda não executado

- Branch `rework/modular-architecture` não foi criada; a Fase 0 foi explicitamente autorizada em
  `dev`.
- Nenhum `.asmdef` foi criado por este rework.
- Nenhum código runtime foi refatorado por este rework.
- Nenhuma fachada ou API antiga foi removida.
- Fase 0 foi concluída no commit `8a887244` e publicada em `origin/dev`; valide o hash e a divergência
  no Git antes de confiar neste registro.

## Baseline publicado

- Branch observada: `dev`.
- HEAD observado antes dos novos commits: `f8892874`.
- Commit principal publicado: `a8fec139` (`feat(projeto): consolidar lote pendente e preparar rework modular`).
- O commit principal contém 1.821 arquivos alterados, 84.855 inserções e 1.541 remoções.
- Antes do push, `dev` estava 21 commits à frente de `origin/dev` e sem commits remotos exclusivos
  após fetch.
- O usuário autorizou explicitamente incluir **todas** as alterações pendentes no commit/push de
  baseline.
- O lote pendente contém código, assets, sprites, cenas, geradores, documentação e ferramentas de
  trabalhos anteriores; não atribua tudo ao rework modular.

## Achados que devem ser revalidados

- zero `.asmdef`;
- aproximadamente 51 arquivos com auto-bootstrap;
- referências hardcoded a `Assembly-CSharp` em geradores/validadores;
- `CSharpProjectPostprocessor.OnGeneratedCSProject` com warning `UNT0006`;
- `_blocks` da telemetria nunca incrementado;
- views de HUD com `Update()` sem trabalho útil;
- `OverlapCircleAll` residual em dois executores de skill;
- `GameEventBus.Publish` usando `handlers.ToArray()`;
- dezenas de acessos globais, inputs diretos e mutações distribuídas de inventário/ouro;
- `GameBootstrap` e `SaveManager` como principais hubs de dependência.

## Sequência obrigatória

1. Verificar este handoff contra Git/disco.
2. Confirmar que o baseline foi publicado e que `dev` está sincronizada.
3. Revalidar os artefatos e resultados da Fase 0 registrados abaixo.
4. Não iniciar Fase 1 ou criar `.asmdef` sem nova decisão explícita sobre branch.
5. Não criar `.asmdef` antes de remover dependências hardcoded e atualizar validators.
6. Não começar por Editor/Tests enquanto runtime continuar preso à `Assembly-CSharp`.
7. Migrar Foundation primeiro.
8. Atualizar este handoff após cada mudança material.

## Atualização obrigatória deste arquivo

Após **cada mudança material**, edite este documento antes do próximo commit. Acrescente uma entrada
ao log abaixo contendo:

- data/hora;
- objetivo;
- arquivos alterados;
- comportamento preservado;
- validações executadas e exit codes;
- testes não executados e motivo;
- commit produzido;
- riscos residuais;
- próximo passo exato.

Não declare PASS por compilação textual ou por relato de outro agente. Use exit code real.

## Log de execução

### 2026-07-05 — Autorização e preflight das Fases 1 e 2 na `dev`

- Objetivo: executar toda a Fase 1 e, após seu fechamento, iniciar a Fase 2 sem criar `.asmdef`.
- Decisão humana: manter os dois lotes na branch `dev`.
- Preflight: `git fetch origin dev`, branch `dev`, working tree limpo, divergência `0 0`, HEAD
  `807cee4a`.
- Limites: sem mudanças de balanceamento, conteúdo, save, cenas ou comportamento observável; Fase 2
  somente depois do commit/push da Fase 1.
- Arquivos alterados neste marco:
  - `docs/architecture/MODULARIZATION_REWORK_PLAN.md`;
  - `docs/architecture/CLAUDE_MODULARIZATION_HANDOFF.md`.
- Validação: preflight Git concluído; validações de código ainda pendentes.
- Commit: pendente.
- Próximo passo: auditar as cinco correções locais enumeradas na Fase 1 e definir testes de
  não-regressão para cada uma.

### 2026-07-05 — Fase 1.1: callback de geração de projeto C#

- Objetivo: corrigir a assinatura Unity inválida de `OnGeneratedCSProject` sem mudar o XML gerado.
- Alterações:
  - callback agora retorna `string`, conforme o contrato do Unity;
  - transformação retorna o conteúdo modificado em vez de escrever o `.csproj` diretamente;
  - guards preservam conteúdo vazio/não-editor;
  - três testes cobrem projeto não-editor, inclusão da referência e idempotência.
- Comportamento preservado: `Assembly-CSharp-Editor.csproj` continua recebendo uma única referência
  privada-falsa a `Assembly-CSharp.dll`.
- Validação:
  - `dotnet build Assembly-CSharp-Editor.csproj`: exit 0; warning `UNT0006` eliminado;
  - Unity EditMode `CSharpProjectPostprocessorTests`: 3/3 PASS;
  - evidência: `TestResults/modularization-phase1-postprocessor.xml`.
- Commit: `1de0e1b9` (`fix(editor): corrigir callback de geração do csproj`).
- Próximo passo: commitar esta correção isolada e então corrigir a telemetria de block.

### 2026-07-05 — Fase 1.2: contador de block da telemetria

- Objetivo: fazer `_blocks` representar blocks normais reais, sem alterar dano ou timing de block.
- Alterações:
  - `PlayerNormalBlockEvent` publicado depois da mitigação já existente;
  - `CombatTelemetryService` assina/desassina o evento;
  - `CombatTelemetrySession.RecordBlock` incrementa o contador;
  - gap obsoleto de evento inexistente removido;
  - testes cobrem agregação e payload de dano antes/depois da mitigação.
- Comportamento preservado: fórmula, perfect block, dano final e feedback permanecem inalterados; o
  evento é observacional.
- Validação:
  - builds runtime/editor: exit 0;
  - Unity EditMode `CombatTelemetryPhaseOneTests`: 2/2 PASS;
  - warning de `_blocks` eliminado;
  - evidência: `TestResults/modularization-phase1-telemetry.xml`.
- Alteração concorrente detectada: `Assets/_Game/Scripts/Enemy/EnemyAnimator.cs` passou a conter um
  lote de idle animation durante a execução Unity. Não pertence à Fase 1, não foi revertida e deve
  ser excluída dos commits desta modularização.
- Commit: pendente neste registro.
- Próximo passo: commitar somente telemetria/eventos/testes/handoff e preservar o arquivo externo.

### 2026-07-05 — Início controlado da Fase 0 na `dev`

- Objetivo: fechar baseline e proteção arquitetural sem alterar gameplay.
- Preflight: `git fetch origin dev`, branch `dev`, working tree limpo, divergência `0 0`, HEAD
  `aed4f35c`.
- Decisão humana: executar a Fase 0 na branch atual; fases de assemblies/composition continuam fora
  deste escopo.
- Arquivos alterados neste marco:
  - `docs/architecture/MODULARIZATION_REWORK_PLAN.md`;
  - `docs/architecture/CLAUDE_MODULARIZATION_HANDOFF.md`.
- Comportamento preservado: nenhuma alteração runtime, de cena, asset, save ou gameplay.
- Validação deste marco: preflight Git concluído; builds/testes serão executados após os artefatos da
  Fase 0 estarem implementados.
- Commit: pendente até o fechamento do primeiro lote documental/técnico.
- Implementado após o preflight:
  - baseline detalhado em `docs/architecture/MODULARIZATION_PHASE0_BASELINE.md`;
  - dez regras de ratchet compartilhadas por CLI e EditMode;
  - limites por arquivo para impedir aumento da dívida arquitetural;
  - baseline de path/GUID para 16 arquivos Unity sensíveis;
  - cinco fixtures de save, v1 a v5;
  - testes EditMode de ratchet, GUIDs e migração/round-trip de save.
- Ratchet CLI: exit 0; contagens atuais iguais ao baseline registrado.
- Comportamento preservado: nenhum arquivo runtime, cena, prefab, asset ou `ProjectSettings` foi
  alterado.
- Validações concluídas:
  - ratchet CLI: exit 0; dez regras dentro dos limites e 16 GUIDs preservados;
  - runtime build: exit 0, 5 warnings preexistentes;
  - editor build: exit 0, 7 warnings preexistentes;
  - EditMode arquitetura/GUID: 2/2 PASS;
  - EditMode save fixtures: 6/6 PASS;
  - EditMode completa: 58/71 PASS, com as mesmas 13 falhas anteriores; zero falhas novas;
  - docs validator: exit 1 apenas pelas dívidas já registradas de specs/harness;
  - PlayMode: não executado porque o lote altera somente testes, ferramentas e documentação.
- Observação de ambiente: após o Unity encerrar e limpar `Temp/obj`, builds `--no-restore` retornam
  `NETSDK1004`; com restore habilitado, runtime e editor passaram.
- Commit técnico/documental: `8a887244` (`test(arquitetura): fechar baseline e ratchets da fase 0`).
- Publicação: `git push origin dev`, exit 0; verificação posterior confirmou HEAD e `origin/dev` em
  `8a8872445230ccf58c09b446fe3842741a945151`, divergência `0 0`.
- Risco residual: as 13 falhas EditMode e os warnings preexistentes permanecem fora do escopo.
- Próximo passo: não repetir a Fase 0. Antes da Fase 1, decidir a branch, revalidar o baseline e
  escolher uma correção local isolada com teste próprio.

### 2026-07-04 — Preparação do rework pelo Codex

- Objetivo: documentar a modularização e preparar o baseline para publicação.
- Arquivos criados:
  - `docs/architecture/MODULARIZATION_REWORK_PLAN.md`;
  - `docs/architecture/CLAUDE_MODULARIZATION_HANDOFF.md`.
- Código runtime alterado pelo rework: nenhum.
- Validações executadas:
  - `dotnet build Assembly-CSharp.csproj`: exit 0, 0 erros, 0 warnings;
  - `dotnet build Assembly-CSharp-Editor.csproj`: exit 0, 0 erros, 0 warnings;
  - `tools/docs/validate_docs.ps1`: exit 1 por dívidas já presentes no lote de trabalho
    (`spec_enemy_attack_kits_v1`, specs legadas de NPC/Town e falsos positivos de placeholder em
    `Generate-CodexHarness.ps1`). Os dois documentos novos não apareceram entre as falhas.
  - Unity Test Runner EditMode em batch: execução concluída com exit 2; 63 testes descobertos,
    50 passaram e 13 falharam. Resultado salvo em
    `TestResults/baseline-before-modularization.xml`.
  - Falhas EditMode observadas: 7 contratos de layout da cidade, 2 testes do
    `ProjectValidationRunner` sem `LogAssert.Expect` e 4 contratos de catálogo de itens/flechas.
    Não corrigir essas falhas dentro de um commit de modularização sem spec/escopo próprio.
- Verificação adicional: `git diff --cached --check` apontou whitespace em arquivos `.meta` gerados
  pelo Unity. Isso foi registrado como ruído preexistente/gerado e não foi normalizado mecanicamente
  neste lote para evitar alterar milhares de metadados sem validação do Editor.
- Commit principal: `a8fec139`.
- Publicação principal: `git push -u origin dev`, exit 0; `a8fec139` enviado para `origin/dev`.
- Risco residual: working tree contém um lote grande de trabalhos anteriores autorizado para commit.
- Próximo passo histórico supersedido pela decisão de 2026-07-05 e pelo marco acima.

## Gates mínimos por mudança

```powershell
dotnet build .\Assembly-CSharp.csproj
dotnet build .\Assembly-CSharp-Editor.csproj
```

Além disso:

- rodar EditMode tests relevantes;
- executar validators afetados;
- criar/rodar PlayMode quando lifecycle, bootstrap, cena, UI ou gameplay observável mudar;
- verificar GUIDs antes/depois de mover/deletar scripts;
- verificar fixtures de save quando tipos, providers ou ordem de restore mudarem;
- usar profiler antes/depois de alegar melhoria de performance.

## Limites absolutos

- Não usar `git reset --hard` ou checkout destrutivo.
- Não apagar alterações do usuário.
- Não editar YAML Unity manualmente.
- Não alterar gameplay junto com arquitetura.
- Não renomear campo serializado sem migration Unity.
- Não mudar IDs, quantidades, balanceamento ou starter items.
- Não promover spec sem evidência exigida.
- Não realizar migração big-bang.

Ao final de cada sessão, deixe este arquivo suficiente para que outra sessão consiga continuar sem
depender do histórico da conversa.

---
