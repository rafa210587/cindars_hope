# Piloto Serena 1.7.0 — 2026-09-12

**Resultado: piloto de fixture PASS, produção Unity NOT RUN; adoção ainda não decidida.** A consulta de referências retornou quatro chamadas e também o `XML cref`, comportamento compatível com o contrato da ferramenta (referências, não apenas invocações). Um consumidor que queira somente callers precisa filtrar os resultados. O projeto Unity real **não** foi ativado; seu comportamento nos `.csproj` Unity continua NOT RUN.

## Escopo e isolamento

- Instalação em `C:\Users\Rafa\AppData\Local\Temp\cindars-serena-20260912-01\venv` (Python 3.12.14), sem `uv tool install`, sem configuração global Codex. `SERENA_HOME` apontou para `...\serena_home` somente no subprocesso.
- `serena-agent` PyPI 1.7.0, wheel SHA-256 `6dbf1459670d96fb0595f84932adef34260a6fe14ba5135b901fdb3c8c76e891` verificado antes da instalação; 75 pacotes resolvidos/instalados pelo `uv`. `dotnet` 10.0.400, PowerShell 7.6.5.
- Código instalado inspecionado antes da execução: backend C# usa Roslyn LSP `5.5.0-2.26078.4`, download NuGet `roslyn-language-server.win-x64` com SHA-256 embutido `7f3d4119e75305399e6faa81a68240b33c48b94ad523a904594abd00db95572a`. Log confirma download/extração bem-sucedidos. A extração passou por diretório temporário curto devido ao limite de path do Windows.
- Configuração Serena, projeto, logs e cache externos ao repo; dashboard desabilitado; transporte MCP `stdio`. `fixed_tools` e `read_only: true` configurados. `tools/list` **antes das queries** expôs exatamente `read_file`, `get_symbols_overview`, `find_symbol`, `find_referencing_symbols`. Isso limita chamadas MCP, não prova ausência de efeitos do backend.
- Fixture `net10.0` externo com cópia byte-idêntica de `Assets/_Game/Scripts/Foundation/StableHash32.cs` e quatro invocações sintéticas + um `XML cref`. Nenhum arquivo `Assets`, `.csproj` real, configuração Codex global ou worktree MSBuild foi alterado pelo piloto.

## Resultados medidos

| Operação | Tempo de parede | Resultado |
|---|---:|---|
| inicialização MCP (segunda execução) | 2,567 s | lista allowlist confirmada |
| `find_symbol` (`StableHash32/Compute`) | 13,367 s | declaração `Compute(string) : int`, linha 11 do fixture (0-based 10), correta |
| `find_referencing_symbols` | 2,870 s | 5 referências: quatro chamadas nas linhas 6–9 e `cref` na linha 3; não é falso positivo da API |

Uma terceira sessão, agora warm com LSP baixado, usou solução externa de **dois** projetos `net10.0`: `Pilot.csproj` define `Engine` parcial e dois overloads de `Mix`; `Caller.csproj` referencia o primeiro projeto e usa alias `CoreEngine`. `dotnet new sln` e `dotnet sln add` geraram a solução no fixture externo. `tools/list` repetiu a mesma allowlist. Inicialização 2,273 s; `find_symbol Engine/Mix` 1,646 s, retornando os overloads `Mix(int): int` e `Mix(string): string` separados; referências do overload `[0]` 2,925 s (somente chamada com inteiro via alias), do `[1]` 0,680 s (somente chamada com string via alias), e `Engine/Extra` 0,633 s (método na segunda declaração partial, chamada via alias). Todas as quatro consultas tiveram `isError=false` e acertaram arquivo/linha no fixture. São testes de **project reference entre dois `.csproj` externos**, não evidência de compatibilidade com os assemblies Unity reais.

O Roslyn declarou dependências não resolvidas e executou `dotnet restore` no `Pilot.csproj` **externo**, criando `fixture/obj/**` (nove arquivos observados). Criou também `project_data/fixture/{project.yml,.gitignore,cache/csharp/*.pkl}` e logs externos. Não houve `obj` nem `.serena` novos no repositório, segundo `git status --short --untracked-files=all` filtrado após a execução. O subprocesso MCP enviou shutdown ao LSP; checagem `Win32_Process` após a execução não encontrou `python`, `serena` ou `dotnet` com o caminho do piloto. A instalação/cache externos foram retidos para auditoria, não configurados para uso regular.

Primeira tentativa: cliente MCP falhou após `tools/list` porque passou `int` onde a biblioteca exigia `timedelta` em `read_timeout_seconds`; corrigido no script externo e segunda execução concluiu. Sem alteração do servidor ou do worktree como correção.

## Limites e decisão

O fixture demonstrou declaração, referências cross-project, partial, overload e alias. A ferramenta chama `find_referencing_symbols`, não `find_callers`; o `cref` deve ser tratado como referência válida e filtrado se o contrato de aplicação pedir invocações. Não foram testados os quatro callers de produção em seus assemblies Unity. A latência não é comparação pareada com E7: alvo e aquecimento diferem. Sem alegação de economia de tokens, inferência offline ou leitura sem efeitos.

Evidência bruta e configuração: `C:\Users\Rafa\AppData\Local\Temp\cindars-serena-20260912-01\pilot-output.txt`, `pilot-advanced-output.txt`, `pilot.py`, `serena_home/serena_config.yml`, `project_data/fixture/project.yml`, `fixture/`. O código-fonte instalado define `FindReferencingSymbolsTool` como localizador de *references* e usa `find_referencing_symbols` do backend; não promete apenas chamadas. O log da primeira sessão registra restore externo; a terceira sessão reaproveitou o Roslyn baixado (warm), sem novo download. O diretório temporário foi **retido** para auditoria; limpeza futura deve remover somente esse caminho após revisão, não há servidor persistente. Não houve Unity validation, build real, push ou PR.
