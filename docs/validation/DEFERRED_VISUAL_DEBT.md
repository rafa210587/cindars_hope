# Deferred Visual / Play Mode Debt

| Spec | Item diferido | Tipo | Como fechar | Bloqueia? |
|---|---|---|---|---|
| `spec_city_preservation_first_coherent_relayout` | composição, clearance, portas N/S/E/W e ciclo dos 28 NPCs | Play Mode scenario | executar `docs/validation/playmode/spec_city_preservation_first_coherent_relayout_human_test_scenario.md` | bloqueia ACCEPTED, não bloqueia build |

## Sprites de inimigo defeituosos (skin catalog — regenerar)

Removidos dos bindings (`Assets/_Game/Resources/EnemySkins/enemy_skin_bindings.json`) para não aparecerem; regenerar a arte depois e re-adicionar ao pool.

| Slug | Defeito | Ação tomada | Como fechar |
|---|---|---|---|
| `orc_berserker` | dois personagens fundidos (orc + anão) num sprite só | tirado dos bindings (`enemy_orc_kaand_berserker`, `enemy_orc_berserker` usam `orc_grunt`/`gen_orc`) | re-slice do sheet original com separação, ou regenerar |
| `gen_kobold` | mão/garra extra malformada colada no corpo (conectada, não recortável) | tirado do binding `enemy_kobold_scout` (fica `kobold_sentry`+`kobold_trapmaster`) | regenerar o sprite |
| `corrupted_bone_knight.png` | arquivo PNG corrompido/ilegível, órfão | deletado (não referenciado) | regenerar se quiser esse id |
