---
name: editor-tooling-orchestration
description: Padrão de orquestração e higiene de menus de editor Unity — um comando único que roda geradores/criadores na ordem certa (best-effort, log por passo, resumo) e a convenção CindarsHope/Archive para não poluir o menu. Use ao adicionar um MenuItem, consolidar setup multi-passo, ou quando o menu CindarsHope acumular dezenas de entradas.
---

# Skill: Orquestração de Tooling de Editor

O projeto tem dezenas de `[MenuItem("CindarsHope/...")]` (geradores de SO, scene creators, validadores). Sem disciplina, o menu vira um campo minado em que o humano não sabe a ordem nem o que cada item faz. O precedente canônico é `CindarsHopePrepareEverythingMenu` (`CindarsHope/Prepare Everything (One Click)`): um orquestrador best-effort que roda tudo na ordem certa com log legível. Esta skill padroniza isso e a higiene de menu (`CindarsHope/Archive/...` para legado).

## Quando usar

- Ao adicionar um `MenuItem` novo (decidir top-level vs. `Archive/`).
- Ao precisar de um **setup multi-passo** (gerar dados → recriar cenas → registrar build) num clique.
- Quando o menu `CindarsHope` acumula entradas one-off/legadas e fica ilegível.
- Ao expor uma ação de editor que um humano vai rodar mas não conhece os pré-requisitos.

## Por que existe

Geradores rodados na ordem errada falham (cena referencia SO que ainda não existe); um menu poluído esconde os comandos que importam; um passo que aborta o lote inteiro na primeira falha desperdiça os outros. Um orquestrador ordenado, best-effort e auto-documentado resolve os três.

## Sistemas existentes (reusar, não duplicar)

| Referência | Papel |
|---|---|
| `CindarsHopePrepareEverythingMenu` | Orquestrador 1-clique — precedente do padrão `RunStep` + ordem dados→cenas→pós |
| `CindarsHopeProjectMaintenanceMenu` | "Repair and Validate Project" (top-level) + convenção de subir legado para `Archive/` |
| `BuildSceneRegistrar` (`Register()`) | Uso programático seguro; `RegisterBatch` sai do processo — não chame em orquestração inline |
| `CreateMvp*Scene` (`CreateScene()`) | Scene creators idempotentes que materializam wiring deferido |

## Procedimento

### Orquestrador de setup (padrão `RunStep`)

```csharp
[MenuItem("CindarsHope/<Nome do Orquestrador>", priority = 1)]
public static void Run()
{
    int ok = 0, fail = 0;
    void RunStep(string descricaoPT, System.Action acao)
    {
        Debug.Log($"[Prepare] PASSO: {descricaoPT}");
        try { acao(); ok++; }
        catch (System.Exception e) { fail++; Debug.LogError($"[Prepare] PASSO FALHOU: {descricaoPT} -> {e.Message}"); }
    }

    // 1) DADOS primeiro (SOs existem antes das cenas referenciarem)
    RunStep("Gerar catalogo de itens", () => GenerateCanonicalItemCatalog.Run());
    // ... demais geradores
    AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
    // 2) CENAS (materializam wiring)
    RunStep("Recriar FarmScene", () => CreateMvpFarmScene.CreateScene());
    // 3) POS-CENA (dialogos, build scenes)
    AssetDatabase.SaveAssets(); AssetDatabase.Refresh();

    Debug.Log($"[Prepare] Resumo: {ok} OK, {fail} falhas.");
    EditorUtility.DisplayDialog("Prepare", $"{ok} OK, {fail} falhas. Veja o Console.", "OK");
}
```

Princípios: **ordem dados→cenas→pós**; cada passo isolado em try/catch que **conta e continua** (best-effort, nunca aborta o lote); log em PT antes de cada passo; resumo + `DisplayDialog` no fim; cabeçalho do arquivo lista cada passo e documenta honestamente o que o comando NÃO cobre.

### Higiene de menu

- Top-level `CindarsHope/...`: só comandos que o humano roda regularmente (Prepare, Repair/Validate, Build, Validate/*).
- Legado/one-off/specs antigas: mova o `[MenuItem]` para `CindarsHope/Archive/...` (a classe continua existindo; só some do topo).
- Use `priority` para agrupar; comandos primários com `priority` baixo (topo).

### Invocar comandos existentes

Prefira chamar o **método público estático** da classe (ex.: `GenerateCanonicalItemCatalog.Run()`); se o handler for `private`, exponha um método público ou use reflection — nunca duplique a lógica de geração. Evite `EditorApplication.Exit`/`RegisterBatch` em fluxo inline (mata o processo).

## Regras

- Orquestrador é best-effort: uma falha loga e continua; nunca aborta o lote inteiro.
- Ordem obrigatória: dados (SOs) → `SaveAssets`+`Refresh` → cenas → pós-cena → `SaveAssets`+`Refresh`.
- Cabeçalho do arquivo documenta cada passo em PT **e** o que fica manual (placement não-wired, validadores) — honestidade > cobertura aparente.
- Só Unity Editor APIs; **nunca** editar YAML à mão; nada que exija Play Mode dentro do orquestrador.
- Não duplicar lógica de gerador — orquestrar o que já existe (`(skill: system-reuse-audit)`).
- Scene creators são **destrutivos** (sobrescrevem `.unity`) — diga isso no diálogo/cabeçalho.

## Saída esperada (checklist)

```text
MenuItem novo: top-level (uso regular) ou Archive/ (legado) — decidido: <qual>
Orquestrador usa RunStep best-effort (conta OK/falha, nao aborta): SIM/NAO APLICAVEL
Ordem dados->cenas->pos respeitada: SIM/NAO APLICAVEL
Cabecalho documenta cada passo + o que fica manual: SIM
Chama metodos publicos existentes (sem duplicar logica): SIM
Build do Assembly-CSharp-Editor: 0E
```

## Relacionados

- `(skill: unity-asset-generation)` — rodar geradores e registrar evidência
- `(skill: unity-validation)` — validar compile do editor assembly
- `(skill: hud-canvas-binding)` / `(skill: scene-interactable-wiring)` — o que os scene creators materializam
- `(rule: unity-assets)` — sem YAML manual; evidência de asset gerado
