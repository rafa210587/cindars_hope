# Harness progressivo e workflows visuais — 2026-09-09

Status: CODE_COMPLETE; contratos do harness e paridade PASS. Docs global FAIL, sem promoção global.
Escopo exclusivo de harness, tooling e documentação.
Spec: [plano e tasks](../../.specs/a_implementar/spec_progressive_harness_visual_workflows_v1.md).

## Acceptance criteria extracted

Criar especializações de direção visual, revisão de imagem, animação e integração; carregar detalhes
sob demanda; corrigir roteamento/validação contraditórios; manter descoberta e paridade Codex;
comprovar mecanismos alterados sem executar Unity para mudanças sem C#/assets.

## Existing systems audit

A auditoria inicial encontrou 67 skills canônicas, 16 commands e 10 agentes. Os 83 SKILL.md do
Codex incluíam os commands sintetizados; essa diferença não era duplicação acidental.
Reutilizados agentes de implementação, wiring, testes e validação. Adicionado somente um
revisor visual; especialização de domínio permanece em skills. Fonte canônica: `.claude/`.

## Entrega

| Artefato | Responsabilidade |
|---|---|
| `pixel-art-direction` | Paleta, perspectiva, resolução nativa, proporções e brief baseado na arte aprovada |
| `visual-asset-review` | Comparação da imagem real, identidade, alpha, recortes e legibilidade |
| `sprite-animation-review` | Key poses, apoio, timing, direções, armas, loops e transições |
| `sprite-scene-integration` | PPU, pivot, câmera, sorting, footprint, interação e evidência no jogo |
| `pixel-art-scene-reviewer` | Auditoria visual independente; não implementa suas próprias correções |

Progressive disclosure: roteador essencial → skill pertinente → somente referências necessárias.
O [catálogo](../../.claude/HARNESS_INDEX.md) conserva a descoberta completa sem repetir suas tabelas
em cada entrada. As descrições de descoberta ainda ocupam contexto; não se afirma custo zero.
Referências de skills são copiadas recursivamente pelo gerador.

Prompts, geração web e caminhada NPC passam a reutilizar esses workflows. Sem URLs pessoais,
APIs de browser presumidas, scripts temporários inexistentes ou repetição ilimitada de geração.
Análise estática, playback e observação em jogo têm evidências distintas. Não há número universal
de frames nem obrigação de aumentar a resolução para melhorar movimento.

Delegação depende de independência/risco; o implementador pode escrever o teste pequeno da própria
correção. Revisão arquitetural ampla não é duplicada por rotina no review do diff. O validador pode
escrever evidência sem implementar gameplay. Esforço não identifica modelo ou preço.

SOLID é aplicado por responsabilidade e contratos. Não foi imposto frontmatter manual em cada
classe, nem cotas de interfaces, patterns ou testes. Permanecem comentários curtos de intenção e
contexto de classe obtido pelo tooling existente quando necessário.

## Spec Compliance Matrix

| Task | Evidência | Resultado |
|---|---|---|
| P01: workflows visuais | Fontes e referências novas; revisão de contratos | PASS |
| P02: roteamento/geração/hooks | 36 + 14 testes de contratos e paridade gerada | PASS |
| P03: regras proporcionais | Revisão independente de autoria, decomposição e keyart | PASS |
| P04: integração | Logs, hashes, links e cenários; falha documental explícita | PASS para harness; docs global FAIL |

## Validation

Esta seção registra a primeira entrega. A continuação de templates/exemplos e suas novas cópias
tem evidência própria ao final; os snapshots abaixo não representam os recursos adicionados depois.

| Check | Resultado e evidência |
|---|---|
| `tools/codex/Test-CodexHarnessGeneration.ps1` | PASS, 36 assertions, incluindo recursos recursivos, preflight, preservação e arquivo idêntico mapeado: [log](artifacts/progressive-harness-20260909/generator-tests.log) |
| `tools/codex/Test-ProgressiveHarnessHooks.ps1` | PASS, 14 assertions: inputs iguais, referências alteradas, falha/retry, cache corrompido, mutação durante execução e Stop por escopo: [log](artifacts/progressive-harness-20260909/hooks-tests.log) |
| Gerador real, duas últimas execuções | PASS: 71 skills, 16 commands, 11 agents, 23 rules; zero issues/copy failures: [run 3](artifacts/progressive-harness-20260909/generation-run-3.log), [run 4](artifacts/progressive-harness-20260909/generation-run-4.log) |
| Idempotência e preservação | 136 outputs idênticos; 11 referências byte a byte; manual AGENTS/config/status Assets preservados: [checks](artifacts/progressive-harness-20260909/generation-checks.json) |
| Conferência final do integrador | 136 hashes atuais comparados ao snapshot final, zero diferenças; snapshots 3/4 idênticos |
| Review independente | 23 fontes focadas, 42 links fonte e 37 links publicados: zero quebrados; 11/11 corpos de agentes em paridade; 6 cenários coerentes |
| `git diff --check` no escopo | PASS, exit 0; avisos de normalização de fim de linha não são erro de whitespace |
| `tools/docs/validate_docs.ps1` | FAIL, exit 1, 59 diagnósticos em outros documentos/specs: [log final](artifacts/progressive-harness-20260909/docs-validation-final.log). Zero diagnósticos da nova spec |

Execuções PowerShell usaram `powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File <script>`.
O primeiro docs run encontrou 67 diagnósticos, incluindo oito metadados/seções ausentes na nova spec;
esses oito foram corrigidos, e o run final mantém os outros 59. O número histórico 47 de outra
entrega não é um baseline equivalente ao estado atual; não se afirma que a diferença veio desta tarefa.

O run 2 do gerador falhou ao reescrever uma skill idêntica que o host mantinha mapeada no Windows
([log preservado](artifacts/progressive-harness-20260909/generation-run-2.log)). O gerador passou a
evitar reescrita byte-idêntica; novo contrato e runs 3/4 comprovaram a correção. Não foi ocultada a falha.

Os seis cenários foram walkthroughs textuais: bug com teste no mesmo contexto; refactor com
preservação da costura; sprite estático com comparação/alpha; animação com poses e playback;
cena com escala/footprint/captura; validador que escreve evidência sem corrigir runtime.
Isso não é execução de gameplay nem benchmark do comportamento de uma LLM.

Unity/EditMode/PlayMode: não aplicáveis a esta entrega; nenhum C#, sprite ou asset de cena alterado
por esta tarefa. Isso não resolve nem reclassifica falhas anteriores de farm, wiring ou docs.

## Contexto inicial antes/depois

| Arquivo | Antes, bytes | Depois, bytes |
|---|---:|---:|
| `AGENTS.md` | 25.449 | 5.518 |
| `CLAUDE.md` | 16.628 | 2.665 |
| Total desses dois arquivos | 42.077 | 8.183 |

Redução de 80,6% em bytes dessas entradas. Não é redução medida dos tokens totais de uma tarefa:
catálogo, metadados de descoberta, skills/referências selecionadas e coordenação também têm custo.

## Honest status rationale

Não há medição de tokens ou faturamento: `NOT MEASURED`. Tamanho de instruções é apenas proxy.
Novo agente pode exigir recarregamento da configuração; geração de TOML não comprova descoberta
na sessão atual. Testes explícitos de hooks não comprovam disparo automático pelo host.
Nenhum commit, push, PR, mudança global de modelo ou instalação de plugin nesta entrega.
Spec permanece em `a_implementar` com CODE_COMPLETE e evidência vinculada; sem declarar aceitação
humana ou GLOBAL_PASS. A dívida documental global está fora desta fatia de implementação.
O gerador preserva extras locais; ao remover uma fonte futuramente, saídas antigas exigem auditoria
de ownership antes de exclusão. Não foi introduzida remoção automática potencialmente destrutiva.

## Continuação — templates e exemplos sob demanda

Solicitação humana posterior: modelos e exemplos concretos para tornar as skills operáveis.
Escopo: dez skills dos workflows visuais, implementação, validação e autoria do harness.
`assets/templates/` contém material copiável; `references/examples/` contém casos preenchidos
didáticos. A entrada liga cada material à condição em que ajuda, sem leitura obrigatória de ambos.
Modelos profundos existentes em `.specs/_templates` continuam como fonte quando pertinentes.

Padrão de autoria atualizado para distinguir modelo, exemplo, referência e script. Placeholders
devem ser preenchidos/removidos no entregável; exemplos hipotéticos não fornecem evidência real
nem valores universais de frames, pixels, escala ou arquitetura. Não se impõe uma árvore de
pastas idêntica a todas as skills.

Entrega: 10 templates + 10 exemplos, nas dez skills abaixo. Cada entrada contém os links
condicionais; recursos são copiados integralmente para `.agents/skills`.

| Skill | Template / exemplo |
|---|---|
| [pixel-art-direction](../../.claude/skills/pixel-art-direction/SKILL.md) | Brief visual / proporção da ponte |
| [visual-asset-review](../../.claude/skills/visual-asset-review/SKILL.md) | Review de imagem / alpha do cais |
| [sprite-animation-review](../../.claude/skills/sprite-animation-review/SKILL.md) | Review de animação / caminhada e ataque |
| [sprite-scene-integration](../../.claude/skills/sprite-scene-integration/SKILL.md) | Integração / encaixe físico da ponte |
| [pixel-art-prompt-authoring](../../.claude/skills/pixel-art-prompt-authoring/SKILL.md) | Prompt / cais isolado em inglês |
| [delegated-execution](../../.claude/skills/delegated-execution/SKILL.md) | Pacote de delegação / edição documental |
| [solid-refactoring](../../.claude/skills/solid-refactoring/SKILL.md) | Plano da costura / cálculo de capacidade |
| [unity-validation](../../.claude/skills/unity-validation/SKILL.md) | Relatório proporcional / falha antes de compilar |
| [spec-authoring](../../.claude/skills/spec-authoring/SKILL.md) | Spec curta / manutenção coesa |
| [harness-authoring](../../.claude/skills/harness-authoring/SKILL.md) | Entrada de skill / seleção condicional de review visual |

Validação da continuação: PASS para conteúdo/publicação. Conferidos 88 links fonte/cópia,
zero quebrados; zero divergências de paridade nas dez skills; 155 arquivos publicados/configuração
conferidos na repetição, zero mudanças de conteúdo. [Checks](artifacts/progressive-harness-20260909/templates-checks.json),
[geração 1](artifacts/progressive-harness-20260909/templates-generation-1.log) e
[geração 2](artifacts/progressive-harness-20260909/templates-generation-2.log).
Os templates usam placeholders `{{campo}}`; casos preenchidos estão rotulados como hipotéticos.
Na revisão, corrigidos o modelo curto sem marcadores obrigatórios e a distinção entre
execução falha antes de compilar e falha de asserção após compilar. Nenhum resultado fictício
foi tratado como evidência do projeto.

Nenhum script de execução, C# ou asset Unity alterado. Os 36+14 contratos anteriores não foram
reexecutados: scripts equivalentes; verificação desta continuação cobre os recursos novos.
Docs global permanece FAIL, 59 diagnósticos externos; ver log `templates-docs-final.log` nos artefatos.

## Piloto de idioma posterior

As dez skills e seus recursos foram migrados para inglês, preservando IDs e disclosure.
Contagem local e limites estão no [relatório de avaliação](ENGLISH_HARNESS_PILOT_20260909.md).
É uma medição posterior do corpus, não uma revisão retroativa das claims NOT MEASURED anteriores
nem prova do custo total da sessão. Outras skills/rules/agentes continuam fora desse piloto.
