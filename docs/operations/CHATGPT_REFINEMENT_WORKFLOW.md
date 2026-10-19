# ChatGPT Refinement Workflow — Cindar's Hope

> Status: ativo
> Escopo: processo externo de refinamento, revisão crítica e consolidação documental
> Não substitui: validação no Unity, validação humana final, leitura do estado real do repositório

## 1. Decisão

O ChatGPT faz parte do fluxo do projeto como ferramenta externa de refinamento, consolidação e revisão crítica.

Ele não é um agente local do repositório e não deve ser tratado como substituto de `CLAUDE.md`, `AGENTS.md`, Codex, Claude Code ou validação no Unity.

Por isso, o papel do ChatGPT fica documentado aqui, em `docs/operations/`, e não dentro de `CLAUDE.md` ou `AGENTS.md`.

## 2. Papel do ChatGPT no projeto

O ChatGPT deve ser usado para:

- consolidar visão geral do jogo;
- refinar lore, sistemas, escopo e regras;
- identificar lacunas, conflitos, riscos e decisões pendentes;
- transformar ideias soltas em documentos rastreáveis;
- preparar prompts para Codex, Claude Code e outros agentes;
- revisar specs antes de execução;
- gerar checklists de validação humana;
- comparar intenção de produto contra escopo implementável;
- ajudar a decidir onde cada documento deve entrar no repositório.

## 3. O que o ChatGPT não deve fazer sem revalidação

O ChatGPT não deve afirmar estado real do código sem antes revalidar o repositório.

Exemplos de informações que exigem revalidação no repo:

- quais arquivos existem hoje;
- quais PRs foram mergeados;
- quais sistemas estão implementados;
- quais bugs ainda existem;
- se uma spec já foi absorvida;
- se uma validação Unity passou;
- se uma pasta ou arquivo pode ser removido.

Quando a pergunta envolver estado real do projeto, a resposta deve deixar claro se houve ou não revalidação do repo.

## 4. Relação com Codex e Claude Code

O fluxo recomendado é:

```text
Ideia / dúvida / refinamento
  ↓
ChatGPT refina, critica, consolida e gera material rastreável
  ↓
Documento entra em docs/ ou docs/refinements/
  ↓
Codex / Claude Code executam tarefas pequenas no repo
  ↓
Validação automatizada possível
  ↓
Validação humana final no Unity
```

ChatGPT não deve ser usado para executar código diretamente no Unity nem para substituir testes locais.

## 5. Onde colocar saídas geradas com apoio do ChatGPT

| Tipo de saída | Local recomendado |
|---|---|
| Visão geral consolidada do jogo | `docs/design/` ou `docs/00_*` se o repo ainda usar documento-mãe no topo de docs |
| Refinamento ainda não implementável | `docs/refinements/a_implementar/pre_refinamentos/` |
| Refinamento aprovado e estável | `docs/refinements/a_implementar/` ou `docs/refinements/implementados/` conforme status |
| Spec pronta para implementação | `.specs/a_implementar/` |
| Spec implementada/parcial | `.specs/implementados/` |
| Processo operacional | `docs/operations/` |
| Arquitetura | `docs/architecture/` |
| Design/lore/GDD | `docs/design/` |
| Validação e checklists | `docs/validation/` |

A pasta raiz `specs/` não deve ser recriada.

## 6. Regras de refinamento

Todo refinamento produzido com apoio do ChatGPT deve deixar claro:

- objetivo do documento;
- escopo;
- fora de escopo;
- dependências documentais;
- decisões fechadas;
- decisões pendentes;
- impacto em gameplay, arquitetura, UI/UX, save, arte ou validação;
- se é MVP, V2 ou FULL;
- próximos passos sugeridos;
- critérios mínimos para virar spec executável.

## 7. Regras para prompts gerados pelo ChatGPT

Prompts para Codex ou Claude Code devem conter:

- branch base;
- documentos que precisam ser lidos;
- arquivos permitidos;
- arquivos proibidos;
- escopo exato;
- critérios de aceite;
- validação esperada;
- restrições de arquitetura;
- o que não fazer.

Quando a tarefa for de código, o prompt deve evitar pedir leitura ampla de lore ou GDD completo se isso não for necessário.

## 8. Validação humana

A validação humana deve ocorrer no final de blocos relevantes, não a cada microetapa.

Exemplos de blocos relevantes:

- fim de uma spec;
- fim de uma wave;
- fim de um pacote de PRs;
- antes de promover refinamento para spec;
- antes de considerar uma feature como implementada;
- antes de remover documentos antigos.

## 9. Princípio de contexto

O projeto deve evitar carregar contexto excessivo sem necessidade.

A leitura deve ser sob demanda:

```text
Tarefa de código MVP/Farm:
  ler spec específica + contratos core se necessário

Tarefa de lore/cidade/NPC:
  ler docs de design/lore relevantes

Tarefa de arquitetura:
  ler docs/architecture + contratos afetados

Tarefa de validação:
  ler docs/validation + status/registry afetado

Tarefa de refinamento:
  ler documento-mãe relevante + refinamentos relacionados
```

## 10. Fonte de verdade

A fonte de verdade do projeto é o repositório.

O ChatGPT pode ajudar a interpretar, organizar e refinar, mas qualquer afirmação sobre o estado atual do jogo precisa ser reconciliada com os arquivos reais do repo.

## 11. Resumo operacional

```text
ChatGPT = refinamento, documentação, crítica, prompts e checklists.
Codex/Claude Code = execução assistida no repositório.
Unity = validação real do jogo.
Humano = decisão final de produto e aceite.
```
