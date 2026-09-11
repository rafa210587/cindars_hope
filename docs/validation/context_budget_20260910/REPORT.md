# Auditoria de contexto e progressive disclosure — 2026-09-10

## Veredito

Skills integradas e visíveis no catálogo desta sessão. MCPs Unity/Aseprite não instalados por
esta tarefa. O ajuste autorizado nesta continuação colocou os gatilhos dos commands no início
das descrições e converteu as seis entradas mais pesadas em roteadores com referências condicionais.
Nenhuma skill foi desabilitada.

## Método e limites

[Script](measure.py) executado com Python local e tiktoken 0.14.0; encoding `o200k_base`, LF
normalizado, roundtrip Unicode verificado. [Dados](measurements.json).
Não é tokenizer confirmado do Astra nem telemetria do prompt serializado/faturamento/cache.
Catálogo reconstruído com nomes/descrições/caminhos, descrições limitadas a 110 caracteres,
aproximando a apresentação observada nesta sessão. Prefixos de plugins e wrappers do host podem
diferir; 24 skills externas correspondem ao catálogo disponibilizado, não a todo o disco.

## Contexto-base identificável

| Parcela | Tokens de referência |
|---|---:|
| Catálogo do projeto: 94 entradas (78 skills + 16 comandos), antes | 4.215 |
| Catálogo do projeto após ajuste | 4.044 |
| Catálogo externo: 24 entradas | 1.026 |
| AGENTS.md do projeto | 1.417 |
| Descrições de 11 agentes, reconstrução sem schema da ferramenta | 530 |
| Soma aproximada após ajuste | 7.017 |

Essa soma exclui instruções do aplicativo, schemas de ferramentas, ambiente, texto humano,
histórico, imagens e saídas de ferramentas. Não é total exato de cada chamada. Conteúdo pode
permanecer no histórico; não significa que seja acrescido como uma nova cópia em cada mensagem.
A contabilização de cache/compactação não está disponível por componente nesta auditoria.

Somente as duas skills novas acrescentam aproximadamente 84 tokens ao catálogo reconstruído.
As 94 entradas completas caíram de 100.266 para 86.039 tokens. Esse corpus NÃO é injetado
integralmente; a redução afeta o momento em que uma dessas entradas é selecionada.

## Resultado do ajuste

| Entrada | Antes | Depois | Redução |
|---|---:|---:|---:|
| non-regression-review | 3.821 | 338 | 91,2% |
| crop-farming-systems | 2.819 | 290 | 89,7% |
| time-calendar-weather | 2.567 | 286 | 88,9% |
| tilemap-world-rendering | 2.225 | 321 | 85,6% |
| inventory-transactions | 2.216 | 288 | 87,0% |
| localization-authoring | 2.030 | 293 | 85,6% |

Os contratos removidos das entradas foram preservados em duas referências por domínio. Cada
roteador explica quando ler a referência operacional e quando carregar validação/closeout.
Descrições dos 16 commands agora começam pelo comportamento funcional; `implement-spec` expõe
o estado deprecated e `resolve-spec-dependency-chain` expõe seu uso interno antes das notas históricas.

## Leitura sob demanda

| Material | Tokens adicionais se lido inteiro |
|---|---:|
| CLAUDE.md | 636 |
| CURRENT_STATE.md | 1.522 |
| HARNESS_INDEX.md | 2.927 |
| unity-mcp-operations | 647 |
| unity-performance-profiling | 543 |
| hud-canvas-binding | 679 |
| Operação MCP + unity-validation | 1.731 |
| Aseprite: entrada + CLI/Lua + acceptance | 2.268 |
| Animação: animator/operations + review/diagnostics, além do fluxo Aseprite | 1.947 |

Valores de cenário são pacotes explícitos, não previsão de todo prompt: specs, código, matriz de
gates e outras referências necessárias ficam fora. Releitura de conteúdo já disponível deve ser
evitada. O índice completo não deve ser leitura obrigatória quando a skill já foi identificada.

## Findings e prioridades

1. **Catálogo amplo:** 94 entradas locais incluem commands legados. Rever aliases/deprecações,
   evitando apagar IDs usados. Skills externas somam aproximadamente 1 mil tokens; desabilitar
   seletivamente pode ajudar, mas isso altera capacidades do usuário e não foi feito nesta auditoria.
2. **Índice duplicado por leitura:** ler HARNESS_INDEX inteiro custa ~2,9 mil tokens adicionais,
   além da lista do host. Buscar apenas o ID/seção necessário, sem carregar catálogo por hábito.
3. **Histórico e tool output:** retornos extensos de web, git status e logs podem superar o custo
   de várias skills. Preferir filtros, trechos e artefatos com resumos. Isso vale também para esta
   tarefa: auditoria do corpus lê mais conteúdo que uma tarefa normal de gameplay.

As novas entradas MCP/profiling/HUD são pequenas e encaminham referências condicionalmente.
O ADR e relatórios não estão embutidos em AGENTS. As cópias `.claude`/`.agents` não significam
duas leituras automáticas. Configuração local de hooks lista PreToolUse/PostToolUse/Stop;
não há SessionStart/UserPromptSubmit configurado ali para despejar o corpus a cada prompt.
Isso não prova ausência de toda injeção do host ou execução efetiva de hooks.

## Fontes oficiais

[Skills](https://learn.chatgpt.com/docs/build-skills): metadata inicial, corpo sob demanda e
encurtamento de descrições conforme orçamento; por isso gatilho deve vir primeiro.
[AGENTS.md](https://learn.chatgpt.com/docs/agent-configuration/agents-md): descoberta de
instruções por cadeia de diretórios. Comportamento exato depende do host e da sessão.

Nenhuma otimização de tokens faturados foi medida. Medições anteriores foram preservadas em
[measurements-before-adjustment.json](measurements-before-adjustment.json); o resultado atual está
em [measurements.json](measurements.json). Próxima otimização possível: revisar aliases legados e
descrições externas por uso real, sem remover capacidades silenciosamente.
