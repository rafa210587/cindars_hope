# Refatoração do harness Unity/Aseprite — 2026-09-10

## Escopo

Pesquisa externa, [ADR-0031](../decisions/ADR-0031-evidence-driven-unity-aseprite-harness.md),
skills/agentes canônicos, catálogo e geração Codex. Autorização humana nesta tarefa.
Runtime, assets, packages e configurações de servidores não foram alterados por esta entrega.
Working tree já continha trabalho extenso; baseline capturado antes dos edits.

## Mudanças

- Duas skills novas: operação Unity MCP e profiling orientado por medição.
- Refatoração dos agentes de performance/wiring e do HUD; encaminhamento live em validação/bugfix.
- Aseprite: ownership, adaptadores opcionais, metadata auxiliar, undo, coordenadas e índices.
- Referência de adoção externa com origem, revisão, compatibilidade e piloto delimitado.
- Correção de versão fixa antiga no exemplo de geração Unity e critérios visuais de localization.
- Sem cópia de pacotes externos; provenance MIT anterior de animação preservada.
- Continuação de contexto: descrições de command front-loadadas e seis entradas grandes
  convertidas em roteadores; redução medida de 85,6% a 91,2% por entrada.

## Verificações

| Check | Resultado e limite |
|---|---|
| Gerador real | PASS: 78 skills, 16 commands, 11 agents; zero problemas de frontmatter/cópia |
| Test-CodexHarnessGeneration.ps1 | PASS: 38 assertions em fixture isolada; inclui descrição funcional de commands |
| Paridade de recursos de skills | PASS: 136 arquivos fonte/cópia, zero divergências após o ajuste |
| Idempotência | PASS: mesmos inputs, segunda geração sem alteração de hashes |
| Links Markdown locais alterados | PASS: 57 links do harness examinados antes da correção final de encaminhamento, que não adicionou links |
| git diff --check | Harness PASS; ao incluir tracking, aponta blank line at EOF em PROJECT_LOG e IMPLEMENTATION_STATUS fora dos trechos inseridos. Preservados nesta entrega |
| validate_docs.ps1 global | FAIL: baseline 74, intermediário 74, final 59; zero novos diagnósticos. Redução em specs fora do escopo durante trabalho concorrente, não atribuída a esta entrega |
| Revisão independente | Um P2 corrigido e reconferido pelo auditor: ferramentas MCP disponíveis no parent não garantem acesso no subagente |
| Unity/Aseprite execução | NOT RUN: entrega exclusiva de instruções/documentação; não houve alteração de inputs runtime |
| Integração MCP | NOT RUN: fornecedores pesquisados, sem instalação/piloto |

Comandos executados: `tools/codex/Generate-CodexHarness.ps1`,
`tools/codex/Test-CodexHarnessGeneration.ps1`, `tools/docs/validate_docs.ps1`,
comparação SHA256 de fontes/cópias e segunda geração, resolução de links locais e `git diff --check`.
Evidências persistidas em [harness_tooling_20260910](harness_tooling_20260910/).

## Cenários revisados textualmente

Estes checks avaliam decisões das instruções; não são testes reais dos aplicativos.

| Cenário | Decisão esperada e conferida |
|---|---|
| MCP ausente | Runners existentes quando aplicáveis; indisponibilidade localizada, sem ferramenta inventada |
| MCP só no parent | Handoff de operação/inputs ao owner habilitado, sem ampliar allowlist |
| Cena dirty e generator | Identificar outputs/owner antes de executar; sem save-all ou Editor concorrente |
| Timeout ao inserir/modificar | Ler estado antes de retry para impedir duplicação |
| Reload após compile | Reobter identidade/handles, aguardar prontidão e consultar console atual |
| Suspeita de hitch por LINQ | Hipótese estática até captura; não declarar ganho ou ausência de pooling sem evidência |
| HUD aparece mas não clica | Conferir raycast/input/binding e ação/resultado; screenshot insuficiente |
| Aseprite live com clipboard persistido | Preservar documento e user data; auditar metadata auxiliar no candidato |
| Walk com tween/diff alto | Conferir apoio/poses e playback; métrica não aprova movimento |
| Texto acentuado/expandido | Reutilizar localization existente e inspecionar clipping/glyphs na View |

## Limites e próximo passo

Sem GLOBAL_PASS e sem promoção de specs. A refatoração não prova que o host recarregou
skills/agentes nesta sessão, nem mede ganho de tokens, qualidade visual ou velocidade.
O próximo trabalho de integração deve comparar candidatos MCP com versões fixadas no mesmo
piloto definido no ADR; não escolher fornecedor pela quantidade de tools.
