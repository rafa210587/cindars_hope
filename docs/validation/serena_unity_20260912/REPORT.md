# Serena em fonte Unity real — 2026-09-12

**Integração local habilitada; consultas no recorte Foundation/Gameplay/Runtime PASS.** Nenhum `.csproj` ou fonte original foi editado pelo piloto. Consulte o [closeout do integrador](ORCHESTRATOR_REVIEW.md) e o [probe final em fonte real congelada](FROZEN_PROBE_SUMMARY.md). Unity Editor não foi executado e não é gate aplicável a esta mudança de tooling.

- Snapshot externo por sessão: 83 + 14 + 1309 entradas `Compile` (1390 caminhos únicos), cópia SHA-256 conferida. `.csproj` de cópia preservam defines, referências e project references; apenas `HintPath` relativo vira caminho absoluto de DLL existente e diretórios `obj/bin` ficam externos. O LSP restaurou os três projetos no snapshot; a rodada final não registrou dependências não resolvidas nem erro de restore. Isto **não** é compile Unity.
- `tools/list` antes da consulta: somente `read_file`, `get_symbols_overview`, `find_symbol`, `find_referencing_symbols`. `read_only: true`, dashboard desativado, stdio e resposta limitada no proxy.
- `StableHash32.Compute`: declaração correta; 5 referências reais em Runtime — quatro invocações (`EnemyActionExecution` duas, `EliteAffix` uma, `CraftingPassiveRngState` uma) e um XML `cref` em `EliteAffix`. `GameplayInputBlocker.Acquire`: declaração em Gameplay e chamada em `FarmPlotMenuController` (Runtime), cross-assembly correta. Resultados `isError=false`.
- Rodada final com venv durável e ambiente MCP mínimo: initialize 17,691 s; consultas 15,869 s, 11,191 s, 4,173 s, 5,143 s. Não usar esses tempos como economia de tokens ou comparação direta com `rg`.
- Primeira rodada sem `TMP` no ambiente reduzido do cliente: auto-restore do Roslyn falhou em `NuGet.targets(782)` com `Value cannot be null (path1)`; consultas ainda retornaram referências, mas integridade semântica não foi assumida. O proxy passou a definir `TMP` externo antes de restaurar. A rodada final não reproduziu a falha. Diagnósticos C# completos não foram coletados; ausência de warnings de project load não equivale a compilação limpa.
- Guard: hashes de fontes originais, `.csproj` e DLLs são verificados antes de cada consulta; cópias de fonte e `.csproj` verificadas depois. Alteração entre snapshot e resposta invalida a sessão. Diretórios `s/<id>` independentes evitam escrita concorrente; não há atualização silenciosa de sessão ativa.

Evidência bruta: [rodada final](unity-probe-final-raw.txt), [primeira rodada](unity-probe-first-raw.txt), [rodada com ambiente completo](unity-probe-raw.txt). Nenhum push, PR, Unity Editor ou build no original.

As rodadas acima são anteriores ao endurecimento final. A evidência vigente do launcher
é a rodada em fonte congelada indicada no início, com 20–32 s por consulta. O guard também
fixa hashes de analyzers e configurações normalizadas; bloqueia caminhos impróprios, destino
reutilizado, excesso de resposta, timeout e deriva durante a consulta. Testes negativos usam
fixtures externas e backend fake, separados das consultas com LSP real. Não há economia de
tokens comprovada nem promessa de cobertura de Editor/testes/fontes ausentes dos csproj.
