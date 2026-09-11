# Contexto de código para AI

Estado: ativo, 2026-09-08. Invariante canônica em `.claude/rules/solid-and-ai-context.md`;
procedimento em `.claude/skills/solid-refactoring/SKILL.md`.

## Decisão sobre frontmatter

Não inserir YAML/comentários estruturais repetitivos em toda classe. O usuário pediu avaliar
criticamente essa proposta; foi recomendada e comunicada a alternativa abaixo, seguida na ausência
de resposta à pergunta opcional. Não há experimento que demonstre redução de tokens neste projeto.

- Fatos extraíveis — arquivo, assembly, bases, assinaturas — vêm da consulta gerada.
- Responsabilidade e invariantes não óbvios vêm de XML `<summary>`/`<remarks>` junto do tipo.
- Sem listas manuais de métodos/callers, datas, autores, contagens ou resumo artificial de nomes.
- Ausência de comentário é registrada como `null`; não inventar intenção para preencher cobertura.

Um cabeçalho útil pode diminuir navegação; repetir informações do código aumenta o prompt e cria
duas fontes que podem divergir. A presença de XML não certifica qualidade. A revisão desta própria
refatoração corrigiu dois comentários que já exageravam a extração ou descreviam a API antiga.

## Consultar apenas o necessário

Execute a partir da raiz com PowerShell 7 (`pwsh`); Roslyn já acompanha esse runtime.

```powershell
# Localização, assembly e contrato documentado; no máximo dez resultados por padrão.
pwsh -File tools/architecture/Get-ClassContext.ps1 -Type InventoryManager

# Todas as partes da classe partial; membros públicos/protected sob demanda.
pwsh -File tools/architecture/Get-ClassContext.ps1 -Type SaveManager -Members

# Busca por domínio ou wildcard; testes são opt-in.
pwsh -File tools/architecture/Get-ClassContext.ps1 -Module Inventory
pwsh -File tools/architecture/Get-ClassContext.ps1 -Type '*Slot*' -IncludeTests

# Inventário, sem despejar todas as declarações no prompt.
pwsh -File tools/architecture/Get-ClassContext.ps1 -IncludeTests -Summary

# Exportação reproduzível opcional, fora de Assets; não carregar inteira por padrão.
pwsh -File tools/architecture/Get-ClassContext.ps1 -IncludeTests -OutputPath TestResults/solid-ai/class-index.jsonl
```

O comando lê o checkout atual a cada execução. O JSONL exportado é um snapshot opcional:
regenerar após edits; `sourceHash` permite identificar divergência. Nenhum índice versionado
gigante é obrigatório. Summary sem filtros mostra contagens; Type/Module mostram registros filtrados.

## Cobertura e limites

- Parser Roslyn, não regex de declarações: ignora comentários e strings, preserva generics,
  nested types e localizações de partials. Cobre classes, structs, records, interfaces e enums;
  delegates não entram neste inventário.
- Assembly vem da asmdef ancestral; o projeto atual não usa asmref. Não interpreta asmref.
- Defines vêm do csproj Unity gerado correspondente. Branches condicionais inativos não entram;
  um projeto ausente é sinalizado, e csproj desatualizado pode representar configuração antiga.
- Não resolve símbolos, callers, dependências semânticas, contratos ou comportamento de gameplay.
- Erro de sintaxe impede exportar um índice incompleto. OutputPath sob Assets é recusado.
- Antes de editar: abrir a classe, a costura e seus consumidores/testes. O registro é navegação,
  não substituto da leitura nem evidência de economia de tokens.

## Verificação

`pwsh -File tools/architecture/Test-ClassContext.ps1`: 15 contratos, incluindo partials,
namespaces, generics, XML, defines, interfaces implícitas, parâmetros opcionais, limites de saída,
idempotência, recusa de Assets e falha de sintaxe sem sobrescrever índice válido.

Consulta de 2026-09-08: 1698 arquivos próprios incluindo testes; 2633 declarações de tipos,
2618 nomes distintos na configuração corrente; 1120 declarações com summary XML. São números
do snapshot, não metas ou contagens a copiar para regras.
