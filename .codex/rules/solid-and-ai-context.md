# Rule: SOLID e contexto útil para AI

**Invariante:** Código tem responsabilidade coesa, dependências explícitas e contratos verificáveis; documentação explica intenção sem duplicar fatos extraíveis do código.

## SOLID aplicado ao projeto

- **SRP:** separar razões independentes para mudar (regra, persistência, apresentação,
  lifecycle); manter estado e invariantes com seu owner. LOC é triagem, não aprovação.
- **OCP:** introduzir extensão quando houver variação real. Um switch pequeno e estável
  pode permanecer; não criar framework, Strategy ou interface para caso hipotético.
- **LSP:** implementações preservam pré/pós-condições, efeitos e falhas do contrato.
  Não aceitar substituto que exige casts, fortalece precondições ou lança por operação suportada.
- **ISP:** contratos pequenos definidos pela necessidade do consumidor; não há quota
  de interfaces nem obrigação de uma interface por classe.
- **DIP:** regras de domínio recebem dependências necessárias por contrato ou valor.
  Wiring entra no composition root/installers existentes; sem service locator novo.
- Fatos de gameplay entre sistemas usam `GameEventBus`; chamadas síncronas a helpers,
  policies e queries injetadas dentro da fronteira do owner continuam explícitas.
  Não transformar toda chamada local em evento nem usar o bus para esconder dependências.

## Patterns e refatoração

- Aplicar `system-reuse-audit` e `code-minimalism-ladder` antes de criar abstrações.
- Nomear o problema, a variação real e o precedente ao escolher um design pattern.
- Preservar API, GUIDs, campos serializados, schema e comportamento; testar a costura
  com casos observáveis antes/depois. Mudança intencional exige contrato na spec.
- Domain logic testável em C# puro; `MonoBehaviour` adapta Unity e lifecycle.
- Cobertura verifica comportamento da costura, incluindo falhas e efeitos relevantes; não há
  quota de testes por classe extraída. Reusar testes pertinentes e acrescentar casos onde houver lacuna.

## Contexto de classe

- Usar XML `<summary>` para responsabilidade não óbvia e `<remarks>` para invariantes,
  ownership/lifecycle ou efeitos relevantes. Manter o comentário junto da declaração.
- Não impor YAML/frontmatter manual em toda classe. Não repetir imports, nomes,
  assinaturas, métodos, contagens, autor/data ou consumidores em cabeçalhos manuais.
- Consultar o índice gerado por escopo: `tools/architecture/Get-ClassContext.ps1 -Type 'InventoryManager'`.
  Índice auxilia navegação; ler implementação e consumidores relevantes antes de editar.
- Ler apenas spec, arquivos citados e contexto necessário à costura. Não carregar o
  mapa inteiro nem histórico por padrão; comentar intenção não prova redução de tokens.

## Onde se aplica

Código novo e refatorações autorizadas. Métricas/cabeçalhos não certificam SOLID;
revisão deve citar a responsabilidade ou contrato violado e seu impacto concreto.

## Enforcement

`solid-refactoring` guia execução; `architecture-reviewer` e `non-regression-auditor`
revisam coesão/contratos/testes. `Test-ArchitectureRatchet.ps1` impede aumento da dívida
que mede; não demonstra SOLID nem ausência de todos os ciclos. Gate estrito mantém
exit codes reais e falhas explícitas, incluindo docs.
