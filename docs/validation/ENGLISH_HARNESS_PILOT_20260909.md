# Piloto de instruções em inglês — 2026-09-09

Status: CODE_COMPLETE; publicação, contagem e revisão de contratos PASS; docs global FAIL.
Continuação autorizada da [spec de harness](../../.specs/a_implementar/spec_progressive_harness_visual_workflows_v1.md).

## Escopo e critérios

Migrar dez skills já dotadas de templates/exemplos, seus recursos, o roteador CLAUDE,
o padrão de autoria e o agente visual. Descrições correspondentes no catálogo acompanham
a tradução. Preservar contratos, IDs, links, campos reconhecidos pelos validadores e
progressive disclosure. Conversa, documentação humana e commits continuam em português.

Esta amostra não representa tradução de todas as 71 skills, regras e agentes existentes.
Arquivos legados em português continuam válidos. Uma única versão canônica ativa em `.claude/`;
baseline arquivado apenas como evidência, não como outra versão para leitura automática.

## Método de medição

Snapshot de 40 arquivos salvo antes da edição em
[baseline](artifacts/progressive-harness-20260909/english-pilot-before.json).
Comparação local com `tiktoken`, encodings `o200k_base` e `cl100k_base`; fins de linha
normalizados nos dois lados. As contagens são determinísticas para esses encodings.
Não foi confirmado que algum deles seja o tokenizer exato do Astra.

Separar total dos 40 arquivos, corpus das skills e apenas entradas SKILL.md.
O roteador/padrão também recebem a nova política de idioma; seu delta não é tradução pura.
Os arquivos não são todos carregados numa tarefa: total de corpus não equivale a consumo
de sessão, faturamento, tempo de execução ou qualidade da resposta.

[Script de comparação](artifacts/progressive-harness-20260909/english-pilot-measure.py)
executa somente leitura das fontes e escreve evidência; não faz chamadas de inferência.
Biblioteca instalada no diretório local ignorado `.claude/.runtime/token-benchmark`.

## Resultado da comparação

`tiktoken 0.14.0`. [Contagens por arquivo e hashes](artifacts/progressive-harness-20260909/english-pilot-tokens.json).

| Corpus | Encoding | Antes | Depois | Redução |
|---|---|---:|---:|---:|
| Dez skills e recursos, 37 arquivos | o200k_base | 19.512 | 16.835 | 13,72% |
| Dez skills e recursos, 37 arquivos | cl100k_base | 22.609 | 16.840 | 25,52% |
| Somente dez entradas SKILL.md | o200k_base | 8.041 | 7.056 | 12,25% |
| Somente dez entradas SKILL.md | cl100k_base | 9.241 | 7.053 | 23,68% |
| Todos os 40 arquivos do piloto | o200k_base | 21.786 | 18.956 | 12,99% |
| Todos os 40 arquivos do piloto | cl100k_base | 25.307 | 18.964 | 25,06% |

Um recurso já estava em inglês e foi preservado. A medição favorece inglês neste corpus,
sem demonstrar que qualquer tradução futura ou tokenizer terá a mesma redução.
O script verificou um caso conhecido de contagem e round-trip de texto Unicode/placeholder.

## Seleção e disclosure

O assistente escolhe skills por descrição e contexto. Depois abre a entrada e os recursos
necessários. Templates são modelos para produção; exemplos esclarecem decisões. Links não
autorizam leitura recursiva de tudo. Agentes são delegados quando há fatia independente,
especialização útil ou revisão por risco, preservando ownership.

Isso é um contrato de uso, não prova de que toda escolha futura será correta ou de que o host
carregou qualquer agente configurado. O agente visual pode exigir recarga da sessão.
Os dez pares template/exemplo estão implementados; as outras skills não receberam recursos
vazios só para uniformizar pastas.

## Validação e limites

Publicação: PASS. Duas gerações bem-sucedidas, 155 arquivos conferidos sem mudança de conteúdo
na repetição; dez skills e 20 templates/exemplos em paridade; 88 links fonte/cópia sem falhas.
[Checks](artifacts/progressive-harness-20260909/english-checks.json),
[geração 1](artifacts/progressive-harness-20260909/english-generation-1.log),
[geração 2](artifacts/progressive-harness-20260909/english-generation-2.log).
`git diff --check` no escopo: PASS. Revisor independente comparou os 40 arquivos com o baseline:
IDs preservados, nenhum link anterior removido e 48 links fonte resolvidos. Oito marcadores
do template de spec intactos, incluindo os literais portugueses consumidos pelo validador.
37/37 arquivos publicados das skills com hashes iguais às fontes. Sem finding bloqueante
na amostra semântica de segurança, evidência, capacidades, papéis e disclosure.

| Cenário de roteamento | Decisão preservada |
|---|---|
| Bug simples | Fix e teste pertinente no mesmo contexto; sem template/delegação obrigatórios |
| Refactor | Skill SOLID; plano de costura quando útil; exemplo apenas para dúvida |
| Sprite estático | Review da imagem; referência alpha somente quando pertinente |
| Animação | Review de movimento; contrato NPC apenas para consumidor NPC |
| Cena | Integração visual e footprint; execução conforme alvo |
| Validação | Matriz e runners pertinentes; template de relatório opcional |

São walkthroughs textuais de contratos, não teste de acerto automático do modelo.
Docs global: FAIL, exit 1, 59 diagnósticos fora da spec do piloto;
[log final](artifacts/progressive-harness-20260909/english-docs-final.log).
Unity não aplicável: nenhuma alteração de C#, gameplay ou assets. Suites anteriores do gerador
e hooks são reutilizáveis se seus scripts permanecerem equivalentes; validar os novos textos,
links, publicação e cenários é necessário.

Avaliação de comportamento em tarefas reais ainda não executada neste piloto. Roteamento textual
e economia no corpus não comprovam menor retrabalho. Próximas tarefas devem registrar skills/recursos
usados, motivo de delegação e correções necessárias para permitir comparação útil.
