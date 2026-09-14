# Guia de Eficiência de Custo — Cindar's Hope

> Princípios operacionais para reduzir contexto, retrabalho e chamadas sem degradar qualidade.
> Não há tabela de preços, contagem de billing ou garantia de cache exposta neste repositório.

## Roteamento

Use [.claude/rules/task-model-routing.md](../../.claude/rules/task-model-routing.md) como fonte
canônica. Ela orienta o modelo mais econômico que atenda ao risco e à validação, com escalada quando
necessário; a escolha efetiva depende dos controles disponíveis no host. Não transforme aliases,
esforço de raciocínio ou tamanho de arquivo em promessa de preço ou economia.

- Documentação local, catálogos seguindo padrão e validações estabelecidas: candidato econômico inicial.
- Bug isolado e implementação delimitada: candidato intermediário; escale por risco, não por volume.
- Integração multi-componente, regressão desconhecida ou arquitetura/save: candidato mais capaz.

Registre uma frase com modelo herdado/escolhido, justificativa e gatilho de escalada quando houver
delegação. Se o host não aceitar override, mantenha a configuração e declare a limitação.

## Contexto, tokens e saída

- Contexto estimado (arquivos/instruções carregáveis) é uma medida de planejamento; não é contagem de
  tokens faturados. Billing, descontos e cache são determinados pelo produto/provedor e não devem ser
  inferidos deste guia.
- Leia somente a política de contexto e os arquivos citados pela tarefa. Reutilize uma evidência válida
  quando entradas, código e critérios não mudaram; não reexecute gate equivalente apenas para atualizar número.
- Limite saídas de comandos e relatórios ao trecho necessário (`rg`, filtros, `max_output_tokens`).
  Saída menor reduz ruído de contexto, mas não prova economia financeira.
- Agrupe trabalho relacionado quando isso preservar estado e critérios; não mantenha sessão aberta por
  expectativa de TTL ou “cache quente”.

## Automação e delegação

Para conteúdo homogêneo, prefira um gerador/script e uma revisão do resultado a iteração item a item.
Delegue somente fatias independentes ou revisão justificada por risco, definindo escopo, entradas,
critérios e evidência. Coordenação, retries e revisão contam no custo total; não reivindique economia
antes de medir o resultado real.

A fila e o protocolo de benchmark desta iniciativa estão em
[efficiency queue](../validation/efficiency_20260912/QUEUE.md); resultados medidos devem substituir
estimativas, sem virar promessa universal.

## Checklist

- [ ] A tarefa está roteada pela regra canônica e o risco justifica o modelo?
- [ ] A leitura está limitada à política e às referências necessárias?
- [ ] Saídas estão filtradas e os gates válidos foram reutilizados quando aplicável?
- [ ] Trabalho repetitivo foi automatizado sem esconder falhas?
- [ ] Delegação tem ownership, critérios e evidência verificável?
- [ ] Nenhum texto promete preço, desconto, TTL ou percentual de economia não medido?
