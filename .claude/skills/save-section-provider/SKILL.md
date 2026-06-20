---
name: save-section-provider
description: Extrai um save domain do SaveManager monolítico para um ISaveSectionProvider, seguindo o precedente do HotbarSectionProvider. Use para a closure de save debt da fable_13 ou qualquer spec que adicione/refatore save sections.
---

# Skill: Save Section Provider

**Estado do projeto:** `ISaveSectionProvider` existe (`Assets/_Game/Scripts/Save/ISaveSectionProvider.cs`, SPEC_10) com exatamente UMA implementação (`Providers/HotbarSectionProvider.cs`). O resto do capture/restore de save é monolítico dentro do `SaveManager`. `SaveProviderArchitectureRoadmap.cs` documenta a migration pretendida. fable_13 (save debt closure) está na fila.

## O contrato (siga exatamente)

```csharp
public interface ISaveSectionProvider
{
    string ProviderId { get; }                    // "hotbar", "bestiary", ... lowercase stable id
    object Capture(GameSaveData existingSaveData); // null = skip section; use existing data as fallback
    void Restore(object sectionData);              // MUST null-guard (provider may have been skipped)
}
```

## Procedimento (um domain por passo)

1. Localize o código de capture/restore do domain dentro do `SaveManager` e o seu DTO field em `GameSaveData`.
2. Crie `Assets/_Game/Scripts/Save/Providers/<Domain>SectionProvider.cs`:
   - constructor-inject o runtime state holder (como `HotbarSectionProvider(HotbarState)`) — nunca o localize via scene search (rule: unity-architecture);
   - `Capture`: se o state holder for null, faça fallback para `existingSaveData?.<Section> ?? new <Section>SaveData()` (preserva os dados quando o sistema não está carregado);
   - `Restore`: null-guard tanto o holder quanto `sectionData`; faça `as`-cast do DTO e ignore em caso de mismatch.
3. Registre o provider onde o SaveManager monta sua lista de providers, **preservando a restore order documentada** (registries/IDs antes dos consumers — ver a save restore order contract spec em `executadas_build_validated/`).
4. Apague o código inline agora morto do SaveManager na mesma mudança (sem dual path).
5. Regras de DTO: simple types + stable IDs apenas (rule: unity-architecture §3).

## Testes (skill: editmode-test-authoring)

- Capture com live state → DTO bate com o state.
- Capture com holder null → faz fallback para o existing save data.
- Restore com section null → no-op, sem throw.
- Restore com DTO type errado → no-op, sem throw.
- Round-trip capture→restore → state igual.
- Legacy save sem a section → defaults aplicados.

## Fechamento

O execution report documenta: ownership da section (qual provider é dono de qual `GameSaveData` field), posição na restore order, e backward compatibility com saves pré-provider.
