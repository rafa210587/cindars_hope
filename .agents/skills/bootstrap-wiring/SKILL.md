---
name: bootstrap-wiring
description: Faz o wiring de novos runtime services, managers e databases na nova arquitetura de composição (GameRuntimeCompositionRoot + DomainRuntimeInstallers) e o idiom self-healing de SerializeField + fallback Resources.Load para sistemas instanciados por cena (ex.: cadeia visual da cave). Use ao adicionar um serviço/manager/database SO que outros sistemas acessem, ao migrar um *RuntimeBootstrap para installer, ou ao wirar um asset de dados numa cena.
---

# Skill: Wiring de Bootstrap / Composição

> **Mudança estrutural (2026-07): o wiring migrou de `GameBootstrap` para uma raiz de composição.**
> Serviços runtime instalam por `GameRuntimeCompositionRoot` → installers em `DomainRuntimeInstallers.cs`.
> `GameBootstrap` ainda existe (legado, em migração) — não adicione serviço novo lá; use o padrão abaixo.

## Quando usar

- Adicionar um **runtime service/manager** que outros sistemas acessam (antes ia no GameBootstrap).
- **Migrar** um `*RuntimeBootstrap` auto-instalado (`RuntimeInitializeOnLoadMethod`/`EnsureInstance`) para o padrão de installer.
- Wirar um **database/asset SO** consumido por um sistema instanciado por cena (materializer, controller de cena).
- Scene creators (`CreateMvp*Scene.cs`) que precisam apontar um asset num `[SerializeField]`.

## Quando NÃO usar

- UI/HUD de gameplay → `hud-canvas-binding` / `ui-projection-pattern`.
- Objeto interativo de cena (crop, chest, resource) → `scene-interactable-wiring`.
- Save section nova → `save-section-provider`.

## Leitura mínima

1. `CLAUDE.md` (regra 5 — fronteiras modulares) + spec alvo
2. `Assets/_Game/Scripts/Composition/GameRuntimeCompositionRoot.cs` (ordem de instalação)
3. `Assets/_Game/Scripts/Composition/DomainRuntimeInstallers.cs` (installers por domínio)

## Arquitetura atual (2 padrões, escolha por tipo de sistema)

### A) Runtime service (singleton cross-scene) → installer na raiz de composição

`GameRuntimeCompositionRoot` (roda `BeforeSceneLoad`, `DontDestroyOnLoad`) instala:
- em `InstallRuntimeServices()` — serviços **independentes de cena** (combat state, input router, skills, crafting, cave conflito);
- em `Start()` — serviços **dependentes de cena / AfterSceneLoad** (npc, world, farm, item, player, `CaveSceneRuntimeInstaller`, narrative, quest, audio, presentation).

Passos para adicionar um serviço novo:

1. No sistema, exponha um `public static void Install(Transform owner)` (idempotente) — substitui o antigo `EnsureInstance()`/`RuntimeInitializeOnLoadMethod`. Ele cria o host (`new GameObject` + `SetParent(owner)`) **ou** anexa componente em objeto existente; owner ignorado quando anexa em algo já presente.
2. Em `DomainRuntimeInstallers.cs`, chame `SeuSistema.Install(owner)` dentro do installer de domínio apropriado (`CaveSceneRuntimeInstaller`, `WorldRuntimeInstaller`, etc.) — installers são `internal static class` finos, uma linha por sistema.
3. Se for um domínio novo sem installer, crie um `internal static class SeuDomainRuntimeInstaller` e chame-o no `Start()` ou `InstallRuntimeServices()` do `GameRuntimeCompositionRoot`, na posição correta de ordem.
4. **Não** reintroduza `RuntimeInitializeOnLoadMethod` de auto-bootstrap (CLAUDE.md rule 5: "não criar novo auto-bootstrap").

### B) Sistema instanciado por CENA (materializer/controller) → SerializeField + fallback Resources ("self-healing")

Sistemas como a cadeia visual da cave (`CaveRuntimeMaterializer`) são **scene-component**, não serviços. Recebem databases/profiles por `[SerializeField]`. Como o campo pode ficar null numa cena recriada (o scene creator nem sempre wira), o idiom canônico é **resolver com fallback Resources**, deixando o SerializeField como override e o Resources como rede de segurança:

```csharp
[SerializeField] private FooDatabaseSO _fooDatabase;

private FooDatabaseSO ResolveFooDatabase()
{
    if (_fooDatabase != null) return _fooDatabase;
    var resolved = Resources.Load<FooDatabaseSO>("FooDatabase"); // Assets/_Game/Resources/FooDatabase.asset
    // log one-shot [Cave][Wiring]: de onde veio (serialized/resources) ou WARNING se null ("rode Inicializar Projeto")
    return resolved;
}
```

Precedentes reais: `CaveRuntimeMaterializer.ResolveEnvironmentElementDatabase()`, `ResolveBiomeArtProfiles()` (registry via Resources), `EnsureCombatDatabasesBound()`. Para o fallback funcionar, **o gerador de editor materializa o asset em `Assets/_Game/Resources/`** (idempotente, como `GenerateCaveEnvironmentElementProfiles` faz o database e `GenerateCaveBiomeArtProfiles` faz o registry) — registrado como `RunStep` em `Inicializar Projeto` (ver rule `editor-generation-orchestration`).

> Regra prática: se o sistema é um serviço cross-scene → padrão A (installer). Se é componente de cena que lê um asset SO → padrão B (SerializeField + ResolveX via Resources + gerador materializa em Resources). Wirar o mesmo asset no scene creator é opcional/redundante quando o fallback Resources já cobre.

## Regras invariantes

- Sem `GameObject.Find()`/`FindObjectOfType()` — nunca (hook `runtime-code-guard` sinaliza).
- Consumer com null-check + `LogWarning` claro (scene/component/field/id/como corrigir) — ver skill `observability-and-logging`.
- Asset de dados só materializado via editor API (`AssetDatabase`), nunca YAML na mão.
- Novo gerador/materializador de asset não expõe `[MenuItem]` avulso — entra como `RunStep` nos 3 comandos canônicos (rule `editor-generation-orchestration`).

## Validação

Selecionar gates na SPEC_VALIDATION_MATRIX_MASTER. Para feedback .NET aplicável, usar
`tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`; projetos vêm dos asmdefs/solução.
Drift de csproj pede investigação/regeneração, não reexecução automática. Manter smoke
de composição/lifecycle pertinente; reusar evidência verificada sobre os mesmos inputs.

## Regressões comuns

- Adicionar auto-bootstrap novo (`RuntimeInitializeOnLoadMethod`) em vez de installer.
- SerializeField de asset sem `ResolveX()` de fallback → null na cena recriada → sistema silenciosamente inerte (foi exatamente o bug de `_environmentElementDatabase`/`_ecosystemBalance` da cave).
- Criar o fallback Resources mas esquecer o gerador materializar o asset em `Assets/_Game/Resources/`.
- Wirar no scene creator errado; editar `.unity` na mão.

## Quando parar e reportar

- A spec proíbe tocar `GameRuntimeCompositionRoot`/`DomainRuntimeInstallers` mas o serviço precisa de instalação → pare, reporte.
- Scene YAML precisaria de edição manual → materialize o asset em Resources + use o fallback, ou peça regeneração da cena via `Inicializar Projeto`.
