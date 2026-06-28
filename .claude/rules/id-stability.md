# Rule: Estabilidade de ID

Todo ID de domínio (save files, catálogos, registries, specs, game_rules) deve ser `public const string` no catalog canônico. Use a const no código — nunca o literal.

**Prefixo de domínio obrigatório:** `"{dominio}_{noun}"`. Canônicos: `item_`, `animal_`, `quest_`, `node_`, `npc_`, `recipe_`, `buff_`, `enemy_`.

**Nunca renomear** um ID presente em save files sem migration explícita em `SaveMigrationService` e documentação em `docs/decisions/`. **Nunca remover** sem verificar save files.

**Proibido:** IDs via `Guid.NewGuid()` ou `DateTime.Now`; literais string no código de gameplay.

Cave IDs têm restrição adicional — ver rule `cave-stable-run`.
