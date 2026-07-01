# Rule: Orquestração de Geração de Assets (3 Comandos Canônicos)

Toda geração, validação e reparo de assets do projeto é alcançada por **exatamente três comandos
raiz** no menu `CindarsHope/` — nunca por um `[MenuItem]` avulso por gerador. O orquestrador único
é `Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs` (faxina de menus 2026-06-21, que reduziu ~100
entradas para 4 comandos).

## Os três comandos

| Comando | Papel | Muta assets? |
|---|---|---|
| `CindarsHope/Inicializar Projeto` | Gera TODOS os dados (status effects, itens, **bestiário**, skills, armas, loot, pesca, animais, magia, **inimigos/movement profiles**, **packs de spawn**, boss gates, escala visual) e recria as 3 cenas. | Sim (recriar cena é destrutivo) |
| `CindarsHope/Validar Projeto` | Roda validadores em modo **somente leitura**. Não gera, não repara, não muta. | Não |
| `CindarsHope/Reparar e Reconstruir` | Recuperação "algo bugou" — único que **deleta/conserta** assets (nulls em registries, duplicados legados, DBs quebrados, re-sync). | Sim |

(`CindarsHope/Build Standalone Windows` é build de shipping, propósito separado — não é gerador.)

## A regra

- **Nenhum gerador/validador/criador novo expõe `[MenuItem]` próprio.** Ele expõe um método
  `public static` (idempotente, sem Play Mode, sem `FindObjectOfType`, sem editar YAML na mão) e é
  **registrado como um `RunStep(...)`** dentro do comando apropriado em `CindarsHopeMenu.cs`:
  - cria/gera dados → passo em `InicializarProjeto` (na FASE A, antes das cenas; respeite a ordem de
    dependência — ex.: packs de spawn rodam depois do bestiário e dos movement profiles);
  - só lê e reporta → passo em `ValidarProjeto`;
  - deleta/conserta estado quebrado → passo em `RepararEReconstruir`.
- O orquestrador usa o padrão **best-effort `RunStep`**: loga em PT antes, roda em `try/catch` que
  conta OK/falha e **continua** (uma falha nunca aborta o lote), e mostra resumo + dialog no fim.
- Ao instruir o humano a regenerar, cite **sempre** um dos 3 comandos — nunca um menu por gerador
  (que não deve mais existir). Um sub-orquestrador interno (ex.: `GenerateAndWireSpec13GAssets`)
  pode chamar vários geradores, desde que ele próprio seja chamado por um dos 3 comandos e não
  tenha `[MenuItem]`.

## Por que existe

Menus avulsos por gerador (1) repoluíram o menu que a faxina de 2026-06-21 limpou, (2) fazem o
humano (e o agente) adivinhar qual de N menus rodar e em que ordem, e (3) deixam geração órfã: um
asset novo que "existe no gerador" mas nunca entra no fluxo de 1 clique, então some na próxima
`Inicializar Projeto`. Centralizar nos 3 comandos garante ordem de dependência correta, idempotência
e que "rodar o projeto do zero" materializa tudo.

## Exemplos

```csharp
// ERRADO — menu avulso por gerador (repolui o menu, geração órfã do fluxo)
public static class GenerateThematicPacks
{
    [MenuItem("CindarsHope/EnemySpawn/Gerar Packs")]   // ❌ proibido
    public static void GeneratePacks() { ... }
}

// CORRETO — método público idempotente, registrado em CindarsHopeMenu
public static class GenerateThematicPacks
{
    public static void GeneratePacks() { ... }          // ✅ sem [MenuItem]
}
// CindarsHopeMenu.InicializarProjeto():
RunStep("Gerar packs tematicos de spawn por bioma",
    () => CindarsHope.Editor.EnemyTaxonomy.GenerateThematicPacks.GeneratePacks());
```

## Onde se aplica

Qualquer artefato sob `Assets/_Game/Scripts/Editor/**` que gere, valide ou repare `ScriptableObject`,
cena, prefab ou registry. Específico do projeto Cindar's Hope.

## Exceções permitidas

- Ferramentas de **Dev/diagnóstico** pontuais já marcadas como tal (ex.: `CindarsHope/Dev/...`) que
  não fazem parte do pipeline de geração canônica — devem ficar sob o submenu `Dev/` e nunca ser a
  fonte de verdade de um asset materializado por `Inicializar Projeto`.
- `Build Standalone Windows` (shipping).

## Enforcement

Revisional (sem hook mecânico dedicado): `/audit-harness`, `architecture-reviewer` e a skill
`editor-tooling-orchestration` checam novos `[MenuItem]` de geração fora dos 3 comandos. Ao adicionar
um gerador, registre-o em `CindarsHopeMenu.cs` no mesmo PR. A skill `editor-tooling-orchestration`
ensina o padrão de orquestração e a higiene do menu `CindarsHope/`.
