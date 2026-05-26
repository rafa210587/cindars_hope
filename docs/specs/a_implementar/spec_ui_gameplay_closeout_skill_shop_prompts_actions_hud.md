# SPEC 17C - Closeout UI Gameplay: Skill Tree, Equipment Panel, Shop, Prompts e Actions HUD

**Status:** Implementado em codigo - gates automaticos executados; validacao humana final pendente
**Branch alvo:** `dev`
**Tipo:** Bugfix / closeout / estabilizacao
**Ordem de execucao:** 17C
**Depende de:** SPEC 15, SPEC 16 e incremento MVP da SPEC 17
**Bloqueia:** fechamento humano final da SPEC 17

## /speckit.specify

Corrigir os bloqueios encontrados na validacao do MVP de UI:

- `U` deve abrir Skill Trees com `SkillTreeManager` disponivel e as cinco arvores visiveis.
- `K` deve abrir apenas atributos/progressao; `L` deve abrir equipamento.
- Shop deve comprar e vender sem sessao ausente ou manager nao inicializado.
- Cenas gameplay e prefabs em `Assets/_Game` devem ter zero missing scripts.
- HUD deve documentar os comandos vigentes.
- Prompts e registries devem refletir que SPEC 15/16 estao implementadas em codigo, com Play Mode humano pendente.

## /speckit.plan

1. Registrar reproducao e auditar wiring serializado nas cenas.
2. Instalar/referenciar `SkillTreeManager` no bootstrap e no save de cada cena gameplay.
3. Separar as superficies `K` e `L` no controller de personagem/equipamento.
4. Tornar falhas de wiring do shop explicitas e validar as sessoes inicializadas.
5. Fortalecer o scanner Editor de missing scripts para cenas gameplay e prefabs.
6. Atualizar Actions HUD, prompts, ordem/status e log operacional.
7. Rodar compile, scans e checks documentais; manter validacao Play Mode como gate humano final.

## /speckit.tasks

- [x] Skill tree ligada ao bootstrap/save em `FarmScene`, `TownScene` e `CaveScene`.
- [x] `K` atributos/progressao e `L` equipamento, ambos modais.
- [x] Buy/Sell com inicializacao validada e erros diagnosticos.
- [x] Scanner automatizado para tres cenas e prefabs, sem missing scripts.
- [x] Actions HUD e documentacao de teclas atualizados.
- [x] Prompts 15/16 removidos da fila ativa e registries reconciliados.
- [x] Evidencias automaticas registradas; Play Mode humano final pendente.

## Criterios de aceite

- Unity compila sem erros.
- Scan de missing scripts retorna zero para `FarmScene`, `TownScene`, `CaveScene` e prefabs `Assets/_Game`.
- `U`, `K`, `L`, buy/sell e save/load ficam preparados para validacao Play Mode final.
- Nenhuma spec e declarada fechada sem a evidencia humana requerida.
