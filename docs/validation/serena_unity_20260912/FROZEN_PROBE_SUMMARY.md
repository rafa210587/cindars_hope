# Probe Unity real em fonte congelada

PASS, exit 0. Origem: snapshot externo `D:/Projetos/Cindars_Hope/.serena-runtime/s/77e2f6fb/src`, derivado dos projetos Unity reais. Antes da rodada, os hashes SHA-256 de 1390 fontes copiadas e três `.csproj` conferiram com o manifesto (zero diferenças). O launcher final criou nova sessão `s/36189495`; não usou o checkout Quest mutável como fonte.

`tools/list` expôs só quatro ferramentas de leitura. As quatro consultas retornaram `isError=false`: `StableHash32.Compute` encontrou quatro invocações reais e um XML `cref`; `GameplayInputBlocker.Acquire` encontrou a chamada cross-assembly em `FarmPlotMenuController`. Initialize 27,577 s; consultas 27,489 / 31,874 / 25,781 / 20,135 s. Sem `unresolved dependencies`, erro de restore ou `fail-closed` no [raw](unity-probe-frozen-source-raw.txt).

Este resultado prova navegação naquele snapshot Unity capturado; **não prova que o checkout atual continua idêntico**. A rodada anterior no checkout vivo foi interrompida corretamente após mudança em `QuestRuntimeBootstrap.cs`. Não houve Unity Editor, build, teste Unity, push ou PR.
