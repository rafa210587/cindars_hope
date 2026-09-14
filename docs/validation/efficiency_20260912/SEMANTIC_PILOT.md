# Contrato do próximo piloto de navegação

A consulta atual usa Roslyn sintático e declara corretamente que não resolve referências. Acrescentar callers exige montar compilations com fontes, defines, referências de metadata e dependências entre projetos. Uma busca textual por nome não satisfaz esse contrato.

Inspeção local em 2026-09-12: Foundation declara 76 fontes e 152 referências de metadata; Gameplay, 9 e 152; Runtime, 1.301 e 303, além de duas referências de projeto. Os HintPaths examinados existem. Isso verifica disponibilidade dos arquivos, não atualidade dos csproj nem sucesso de compilação semântica.

O piloto seguinte deve usar uma cópia isolada dos scripts de navegação, sem modificar os csproj gerados ou os assets. Começar com referência a tipo/membro totalmente qualificado, overloads, partials, aliases, extension methods e branches condicionais em fixtures pequenas. Retornar localização, assembly, hash dos inputs, configuração e status de completude. Diagnósticos de compilação e símbolos ambíguos impedem alegar conjunto completo de callers.

Comparar o conjunto de referências com um resultado conhecido em fixture e com consumidores reais revisados. Medir saída, tempo e leituras contra `rg` mais `Get-ClassContext`; inicialização do analisador entra no tempo total. Referências via reflexão, IDs serializados e GameEventBus não são automaticamente chamadas diretas: expor limites e consultar wiring/eventos separadamente.

Critério de avanço: referências corretas nas fixtures, nenhuma falsa afirmação de completude com inputs inválidos, saída limitada com contagem de omitidos e benefício observado em tarefas reais. Cache persistente só entra se o custo de análise justificar e a invalidação incluir fontes, defines, referências e versão do analisador.

Serena é candidato de comparação após esse baseline, com versão fixada e modo read-only verificado. Stryker depende de um runner .NET compatível para os testes selecionados; csproj de produção por si só não fornece esse runner. CI depende de escolha do ambiente e licença. Instalação e execução desses candidatos ainda não ocorreram.
