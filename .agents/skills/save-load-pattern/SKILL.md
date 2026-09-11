---
name: save-load-pattern
description: Implementa save/load com IDs estáveis, DTOs simples e providers existentes. Usar ao criar ou alterar persistência, sections, migrations e testes de round-trip.
---

# Skill: Save/load com identidade estável

O projeto usa `ISaveSectionProvider`, `SaveProviderRegistry` e `SaveMigrationRegistry`;
`HotbarSectionProvider` é um precedente concreto de adapter com dependência explícita.

**Regra central: preservar identidade e dados legados; save contém IDs canônicos e tipos simples, nunca referências Unity.**

## Quando usar

- Criar/alterar DTO, provider, registro, migration ou recuperação de save.
- Capturar/restaurar estado por IDs, validar compatibilidade ou snapshot de cave.

## Checklist antes de editar

- [ ] Ler DTO/provider/registro atuais do domínio e fixtures de versões afetadas.
- [ ] IDs de conteúdo são `string` canônicas; índices e quantidades não viram identidade.
- [ ] Nenhuma Unity ref em DTO: SO, GameObject, Transform, componentes, Sprite ou Rigidbody.
- [ ] Nenhuma atribuição de ID pela ordem de array, nome de arquivo, `OnValidate`, data ou RNG.
- [ ] IDs antigos não são removidos/renomeados/reutilizados sem migration e evidência.
- [ ] Contrato de campo ausente, ID desconhecido e fallback explícito definido.
- [ ] Teste de round-trip e compatibilidade cobre estado real, não apenas serialize vazio.

## Leitura mínima e precedentes

Paths abaixo são relativos a `Assets/_Game/Scripts/`:

- `Save/ISaveSectionProvider.cs`: `ProviderId`, `Capture(GameSaveData)`, `Restore(object)`.
- `Save/Providers/HotbarSectionProvider.cs`: adapter de estado injetado e null handling.
- `Save/SaveProviderRegistry.cs` e `Save/SaveManager.cs`: registro tipado e ordem de providers.
- `Save/Migrations/SaveMigrationRegistry.cs`: caminho explícito entre versões e erro de migration.
- `Inventory/InventorySaveData.cs`: `string ItemId`, `int SlotIndex`, `int Amount`.

Ler somente os precedentes relevantes e o domínio da spec. Não copiar providers antigos
que usem busca global; adotar wiring atual por composition root/installers.

## Identidade e DTOs

- IDs de conteúdo vêm do catalog canônico com `public const string`, conforme `id-stability`.
- `SlotIndex`, quantidade, profundidade e schema version podem ser `int`; não são IDs de conteúdo.
- Identidade de instância segue o contrato atual do domínio. Não trocar tipos em saves
  existentes apenas para uniformizar nomenclatura; mudança de tipo exige migration.
- DTOs persistem primitives, enums, coleções de valores simples e DTOs aninhados.
  Estado Unity é convertido para valores (ex.: componentes de posição), sem refs.
- Validators detectam ID vazio/duplicado e referência ausente. Nunca reatribuem IDs
  pela posição do elemento ou ordem de importação; reordenar catálogo não altera identidade.
- No load, resolver pelo database/registry existente e tratar ID desconhecido segundo
  o contrato: erro acionável, fallback documentado ou preservação para recuperação.
  Não descartar silenciosamente inventário/quest/progresso.

## Procedimento

1. Identificar owner do estado, DTO e provider atuais; aplicar `system-reuse-audit`.
2. Classificar mudança como aditiva ou breaking antes de editar schema.
3. Capturar valores/IDs no provider existente ou novo provider de domínio coeso.
   Reusar `ISaveSectionProvider`; não criar um segundo sistema de persistência.
4. Registrar tipo/ordem no `SaveProviderRegistry` pelo wiring existente.
   Preservar captura/restauração e fallback exigidos pelo save atual.
5. Resolver IDs no load com dependências explícitas. Não serializar interface/manager
   nem supor que `[SerializeField]` injeta interfaces automaticamente.
6. Preservar escrita atômica, backup e recuperação já existentes. Save editável usa
   `Application.persistentDataPath`; `StreamingAssets` serve apenas dados read-only.
7. Validar round-trip, null/section ausente, ID desconhecido, versões legadas e falhas
   relevantes. Smoke de wiring é necessário se a composição do provider mudou.

## Migration: aditivo vs. breaking

| Mudança | Contrato exigido |
|---|---|
| Campo/section novo | Default seguro testado em save legado; ausência não perde progresso |
| Renome/remover ID ou campo | Migration explícita e teste com fixture da versão anterior |
| Tipo/shape/semântica de dado alterado | Migration por versão no fluxo existente |
| Layout de arquivo/slots alterado | Scope dedicado e recuperação/backup testados |

Uma section aditiva não dispensa compatibilidade. Atualizar versão quando o contrato
atual exigir; não inventar versão ou migration paralela. Nunca editar saves reais do humano.

## Cave stable run

Antes de alterar snapshots/procedural ler:
- `docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md`.
- `docs/refinements/implementados/ref_pr170_192_cave_stable_run_pre_implementation_audit.md`.

`CaveLevel` já visitado na mesma `CaveRunSeed` carrega snapshot. ForwardExit/BackExit
não regeneram layout, inimigos ou resources. Novo procedural só nos gatilhos autorizados
pelo contrato stable-run. Usar `cave-stable-run-guard`; nenhum schema alternativo aqui.

## Validação e saída esperada

Reportar DTO/provider/registro afetados, compatibilidade, teste de round-trip/fixture,
comandos e exit codes. Build não comprova recuperação de save nem gameplay.
Quando Unity não rodar: `Unity validation: NOT RUN`, motivo, comando tentado e risco residual.

## Quando NÃO usar

- Estado apenas transitório sem persistência: usar o contrato de runtime do domínio.
- Nova seção independente: complementar com `save-section-provider`, sem duplicar este fluxo.

## Quando parar e reportar

Spec não autoriza mudança breaking necessária, não há fixture para validar risco de perda,
ou regra stable-run conflita com proposta. Não mascarar incompatibilidade como default seguro.

## Relacionados

- `(rule: id-stability)`; `(rule: unity-architecture)`; `(rule: validation-truth)`.
- `(skill: save-section-provider)`; `(skill: rng-and-determinism)`; `(skill: cave-stable-run-guard)`.