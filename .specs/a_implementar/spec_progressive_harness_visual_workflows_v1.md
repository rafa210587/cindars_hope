# Harness progressivo e workflows de arte

> **Status:** CODE_COMPLETE; harness validado, docs global FAIL; sem promoção.
> **Type:** Tooling / Governance
> **Domain:** Harness / Art workflows

Autorizado pelo usuário em 2026-09-09 após auditoria do harness.

> **Ordem de execucao:** contratos e fontes → geração sequencial → revisão e evidência.
> **Depende de:** auditoria do harness nesta sessão; sem dependência externa de implementação.
> **Bloqueia:** adoção dos novos workflows visuais como entrega validada.

required_adrs: []
required_game_rules: []

# /speckit.specify

## Objetivo
Reduzir leitura obrigatória e contradições, manter skills específicas sob demanda e tornar reproduzível o fluxo direção visual → imagem/frames → Unity → evidência. Não prometer economia medida de tokens nem ausência de bugs.

# /speckit.plan

## Escopo e plano
Fonte canônica `.claude/`; `.agents/`, `.codex/` e bloco gerado AGENTS vêm do gerador. Preservar working tree anterior e regras de segurança, save, arquitetura e evidência. Nenhum arquivo `Assets/**`, cena, save ou pipeline de gameplay será modificado.

# /speckit.tasks

- [x] P01: criar skills `pixel-art-direction`, `visual-asset-review`, `sprite-animation-review`, `sprite-scene-integration` e agente `pixel-art-scene-reviewer`. Entradas curtas, checklist essencial e links condicionais para referências; não carregar todas por padrão. Atualizar skills antigas de prompt/geração web/NPC walk para reutilizar referências e ferramentas existentes, sem URLs pessoais, APIs presumidas, retry infinito ou scripts ausentes.
- [x] P02: reduzir roteadores iniciais, mover catálogos completos para referência sob demanda e adaptar gerador para indexação/paridade. Preservar descoberta dos comandos, regras, agentes e skills. Delegação proporcional ao tamanho/risco, independente de aliases de modelos de outro provedor; testes pequenos permitidos ao implementador; revisor independente quando justificado. Unity validator pode escrever evidência e executar runners, sem implementar correções. Corrigir checklist de Stop com resultados predefinidos e sincronização repetida por dirty persistente usando identidade dos inputs com prova.
- [x] P03: alinhar decomposição MonoBehaviour a SOLID/testes por comportamento, corrigir rule keyart sobre capacidade dos agentes, ajustar padrão/autoria/auditoria para progressive disclosure e seleção por risco. Não impor cotas cegas de interfaces/classes/testes ou frontmatter por classe.
- [x] P04: gerar cópias só após fontes congeladas; conferir links e referências transitivas, paridade, descoberta, regressão dos scripts alterados e idempotência de geração. Exercitar cenários de roteamento: bug simples, refactor, sprite estático, ciclo animado, montagem de cena, validação sem edição de gameplay. Registrar tamanho em bytes/caracteres antes/depois; isso é proxy, não tokens/faturamento medidos. Relatório em `docs/validation/PROGRESSIVE_HARNESS_VISUAL_WORKFLOWS_20260909.md`.

## Ownership
Art worker: quatro novas skills + referências, novo agente e skills antigas `npc-walk-animation`, `chatgpt-web-sprite-gen`, `pixel-art-prompt-authoring`. Routing worker: `CLAUDE.md`, catálogos novos `.claude/`, gerador/testes `tools/codex/`, agentes existentes, hooks `stop-summary-check` e `sync-harness-and-tracing`. Quality worker: `HARNESS_AUTHORING_STANDARD`, skills `monobehaviour-decomposition`, `harness-authoring`, `harness-audit`, `spec-authoring` quando necessário para proporcionalidade; rules `keyart-scene-fidelity`/`solid-and-ai-context`, relatório P04 e validação final. Integrador gera cópias sem edições manuais e coordena escopo de testes.

## Integração documental
Inclui `tools/codex/README.md`, o command `audit-harness` e os registros de closeout
(`PROJECT_LOG.md` e `docs/IMPLEMENTATION_STATUS.md`), para alinhar consumidores do novo catálogo.

## Critérios
Instruções obrigatórias e segurança preservadas; um único índice fonte legível; novos recursos localizáveis; referências disponíveis nas cópias Codex; nenhum carregamento recursivo obrigatório de todas as referências; sem claims de modelo barato por esforço; sem resultado NOT RUN/PASS fixo; regeneração idempotente para fontes iguais; scripts testados com falhas relevantes e sem rodar Unity por alteração exclusiva de harness. Agente novo será disponível após recarregamento da configuração quando a sessão não fizer descoberta dinâmica.

## Limites
Autorização para implementação de harness e documentação, não para instalação de plugins, mudança global de modelos, novos gastos API, push/PR ou refactor de runtime. Medição de economia real requer baseline comparável de tarefas e ficará explicitamente pendente.

## Evidência de fechamento
O fechamento abaixo refere-se à primeira entrega; continuação autorizada descrita a seguir.
[Relatório final](../../docs/validation/PROGRESSIVE_HARNESS_VISUAL_WORKFLOWS_20260909.md).
Contratos 36+14 PASS; cópias e idempotência verificadas. Docs global FAIL com 59 diagnósticos
fora desta spec; permanece sem promoção global. Tokens reais NOT MEASURED; Unity não aplicável.

## Continuação autorizada — templates e exemplos

Status da continuação: CODE_COMPLETE; conteúdo e publicação PASS; docs global FAIL.

Pedido humano em 2026-09-09: incorporar templates/examples ao progressive disclosure das skills.
Escopo: complementar as quatro skills visuais novas, pixel-art-prompt-authoring, delegated-execution,
solid-refactoring, unity-validation, spec-authoring e harness-authoring. Reusar modelos existentes;
não criar arquivos vazios nem replicar exemplos em todas as skills sem necessidade.

- [x] T01: modelos copiáveis em assets/templates e exemplos didáticos preenchidos sob referências;
  links condicionais na entrada com finalidade explícita. Exemplos não são evidência de execução.
- [x] T02: padrão de autoria orienta template versus exemplo versus script e exige placeholders
  explícitos; inserir exemplo de skill com disclosure condicional para próximas autorias.
- [x] T03: validar links fonte/cópia, placeholders, ausência de claims reais nos exemplos, paridade
  e geração sem alterar Assets. Atualizar relatório e índice; Unity não aplicável.

Ownership: visual worker nas cinco skills visuais; routing worker nas quatro skills de execução;
root em harness-authoring, padrão, spec e relatório. Gerar cópias somente após freeze combinado.

## Piloto autorizado — inglês e avaliação

Status: CODE_COMPLETE. Usuário aceitou migrar uma amostra equivalente antes de ampliar ao restante.
Traduzir as dez skills da continuação (incluindo referências/templates/exemplos), CLAUDE,
padrão de autoria e agente visual. Atualizar descrições correspondentes no catálogo. IDs, paths,
regex, nomes de menus e marcadores consumidos por validadores permanecem literais. Conversa,
documentação humana e commits permanecem em português. Não duplicar versões canônicas.

- [x] E01: baseline dos arquivos antes de traduzir; inglês sem mudança de contratos.
- [x] E02: comparação com tokenizer explicitamente identificado; estimativa local não equivale
  a custo da sessão, auto-routing garantido ou tokenizer confirmado do Astra.
- [x] E03: revisar semântica, links, paridade e roteamento por cenário; registrar limites do piloto.

Ownership: visual worker cinco skills visuais; routing worker cinco skills de execução/autoria;
root roteador, padrão, agente visual, catálogo e evidência. Outras skills/rules/agentes ficam para
expansão posterior; este piloto não declara migração de todo o corpus.

Evidência do piloto: [relatório inglês/tokens](../../docs/validation/ENGLISH_HARNESS_PILOT_20260909.md).
