# Localization migration and closeout

### Migração de hardcode existente (sweep) — só com spec explícita

A `fable_73` entregou a string table, mas **dezenas de specs anteriores hardcodaram PT-BR** (ex.: `QuestRegistry` em `DisplayName`/`Description`, `TownNpcDialogueLibrary`, nós de `DialogueNode.Text` em SOs). Isso é **registered debt** do ADR-0012 — não migre sozinho dentro de outra spec. Quando houver uma spec de sweep no escopo:

1. **Mapear o hardcode** por domínio:
   ```powershell
   Select-String -Path Assets\_Game\Scripts\Quests\Runtime\QuestRegistry*.cs -Pattern 'DisplayName\s*=\s*"'
   Select-String -Path Assets\_Game\Scripts\NPC\*Dialogue*.cs -Pattern '= "'
   ```
2. **Priorizar por visibilidade:** texto de UI/quest/diálogo que o player lê muito primeiro; lore raro depois.
3. **Migrar em fatias por domínio** (um commit por domínio: quests, depois diálogo, depois UI), cada literal vira `LocalizationService.Get("quest.<id>.title")` + entrada em `LocalizationStringTable.SeedEntries()`.
4. **Preservar id-stability:** a *key* de localização é um id de domínio novo (segue a convenção `{domain}.{spec_ou_npc}.{slot}`); o id de sistema (questId, npcId) **não muda** (rule `id-stability`).
5. **Teste de regressão:** o texto exibido antes == `LocalizationService.Get(key)` depois (round-trip via `OverrideForTests`).

Sweep é migração de **apresentação**, não de save nem de id de sistema — nenhum save quebra, nenhum questId muda.

## Regras

- Todo texto visível ao player a partir de P4 usa key → `LocalizationService.Get(key)`.
- **Nunca** `string.IsNullOrEmpty(LocalizationService.Get(id))` para detectar key ausente — o fallback é o próprio id (sempre não-vazio para ids não-vazios). Use `LocalizationService.Has(id)` ou `TryGet`.
- Key ausente é **dado visível ao player** (retorna o id como fallback) — não deve chegar a produção. Registrar como débito se a key ainda não existir na tabela.
- Não criar um segundo sistema de i18n, um `ResourceManager` Unity ou um `Addressables` de texto — ADR-0012 bane localization frameworks em v1.
- Texto pré-P4 (ex.: `DialogueNode.Text` hardcoded em SOs, `TownNpcDialogueLibrary`) é débito registrado — não migrar sem spec explícita.
- `LocalizationStringTable` e `LocalizationService` são classes estáticas puras (sem Unity, sem GameEventBus, sem save).

## Saída esperada (checklist de closeout)

```text
Texto novo voltado ao player: SIM/NÃO
Keys registradas em LocalizationStringTable: SIM/NÃO APLICÁVEL
Convenção de id seguida: SIM/NÃO
LocalizationService.Get usado na view/ViewModel: SIM/NÃO
Hardcode de string removido/ausente: SIM
Teste de round-trip (OverrideForTests): SIM/NÃO APLICÁVEL
```

## Quando NÃO usar

For affected UI, also inspect accented glyphs, text expansion, wrapping/truncation and missing-key
display through `hud-canvas-binding`. A successful key lookup does not prove layout/readability.
Reuse the existing localization system; external skills do not authorize adding Unity Localization.

- Texto de **debug/log** interno (`Debug.Log`, `UnityEngine.Debug`) → não localizar; esses nunca chegam ao player.
- **IDs de sistema** (item IDs, npcIds, evento IDs) → são identificadores, não texto de UI; cobertos pela rule `id-stability`.
- Texto hardcoded **pré-P4 existente** → é registered debt (ADR-0012); não refatorar a menos que a spec inclua isso explicitamente no escopo.
- Texto de **editor tools** e validators (só visto por devs) → não precisa de localização.

## Relacionados

- ADR-0012 — decisão canônica de localization
- `(skill: npc-dialogue-authoring)` — diálogo localizado via keys
- `(skill: ui-projection-pattern)` — a view resolve a key no ViewModel
- `(skill: editmode-test-authoring)` — testar tabela via `OverrideForTests`
