# Relatório da Fase 5 — Assemblies explícitas

Data: 2026-07-05

Status técnico: `COMPLETE`

## Grafo implementado

```text
CindarsHope.Foundation
        ^
        |
CindarsHope.Runtime
        ^
        +-------------------+
        |                   |
CindarsHope.Editor   CindarsHope.Tests.EditMode
                            |
              CindarsHope.Tests.PlayMode.Composition
```

O runtime amplo é deliberado: os 49 pares de dependência mútua impedem uma divisão honesta por
domínio neste momento. A fronteira única remove o código do jogo da assembly predefinida sem criar
ciclos falsos; a subdivisão depende dos ports da Fase 6.

`Assembly-CSharp` permanece somente para 34 scripts de exemplos importados do TextMesh Pro. Todo o
código runtime, editor e testes de Cindar's Hope usa assemblies explícitas.

## Compatibilidade de testes

`InternalsVisibleTo("CindarsHope.Tests.EditMode")` preserva o acesso intencional dos testes a três
tipos internos já mapeados na Fase 2. Nenhum tipo runtime foi tornado público só para compilar teste.

O builder passou a usar `cindars_hope.slnx` como fonte autoritativa, ignorando `.csproj` antigos que
o Unity deixa no diretório.

## Evidência

- Unity compile: exit 0;
- 6 projetos ativos restaurados e compilados: 0 warnings, 0 erros;
- arquitetura: 7/7 PASS;
- fixtures de save: 6/6 PASS;
- ratchet: PASS (`RuntimeInitialize=63`, `Singleton=55`);
- hardcode de assembly predefinida: 0;
- suíte EditMode completa: 2.579/2.660 PASS, 81 FAIL.

As 81 falhas revelam testes que nunca eram descobertos pela assembly Editor predefinida. Elas foram
registradas como baseline de dívida da Fase 5; incluem contratos antigos conflitantes com expansões
posteriores, estado estático entre testes, arredondamento e as 13 falhas já conhecidas. A Fase 6 deve
corrigir somente defeitos comprovados, sem regredir conteúdo atual para satisfazer expectativas
obsoletas.
