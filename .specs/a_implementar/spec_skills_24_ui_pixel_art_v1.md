# Skills — fase visual 1: pixel art da árvore, loadout e HUD

> **Spec ID:** spec_skills_24_ui_pixel_art_v1  
> **Status:** APROVADA — autorização humana de 2026-09-10  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Art+UI / Presentation / P1  
> **Parallelizable:** NO; **Depends on:** spec_skills_23_skill_loadout_hud_v1  
> **Blocks:** spec_skills_25_action_presentation_foundation_v1  
> **Validation:** asset validator+PlayMode+capturas gráficas

# /speckit.specify

## Spec

Aplicar uma linguagem de pixel art coerente com Vaalara às telas funcionais sem mudar regras de gameplay.

- **AC01:** criar molduras 9-slice, tabs, linhas de conexão, cursor/foco e painéis de detalhe legíveis na escala nativa.
- **AC02:** criar ícones distintos para as 31 ações e emblemas reutilizáveis para passivas/capstones; a silhueta comunica função, não apenas a cor da árvore.
- **AC03:** Locked, Purchasable, Owned, Equipped, Cooldown, Blocked e Dormant reutilizam o ícone-base com moldura/overlay consistente.
- **AC04:** paletas diferenciam Melee, Ranged, Magic, Survival e Crafting sem quebrar a identidade visual comum de Cindar's Hope.
- **AC05:** todo sprite usa Point, Compression None, mipmaps false, alpha correto, PPU/pivot documentados e nenhum upscale fracionário.
- **AC06:** contraste, foco e leitura passam em 1280×720 e 1920×1080; texto não depende de cor para comunicar estado.
- **AC07:** os 66 nós canônicos deixam de possuir `Icon: null`; o validator prova 66 nodes + 31 actionIds e lista separadamente assets órfãos sem religá-los.
- **AC08:** direção visual aprovada registra referências realmente abertas, motivos de Cindar's Hope/Dornecia, paleta, bordas e fontes editáveis em camadas. O gate fixa ícone 32×32, socket de nó 40×40, slot HUD 40×40, overlay 32×32 e tiles 9-slice múltiplos de 8 px.
- **AC09:** fonte pixel inclui acentos PT-BR e strings longas; comparação lado a lado usa fonte PNG, alpha medido, zoom nativo e captura em escala real.

# /speckit.plan

Definir direção em um documento que nomeie as referências aprovadas, produzir sprites fonte em camadas, importar por generator Editor e
ligar assets por ID estável. Validar sprites isolados e dentro da árvore/loadout/HUD nas três cenas.
T01 é gate humano bloqueante: T02–T04 não começam antes da aprovação explícita da direção e das referências.

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [ ] | T01 | Aprovar referências, direção, grade, fonte, paleta e escala | AC01, AC04–AC06, AC08–AC09 |
| [ ] | T02 | Produzir chrome 9-slice, conexões e overlays | AC01, AC03 |
| [ ] | T03 | Produzir e mapear ícones de ações/passivas/capstones | AC02–AC04, AC07 |
| [ ] | T04 | Gerar/importar assets com settings canônicos | AC05, AC07 |
| [ ] | T05 | Revisão visual lado a lado e capturas nas duas resoluções | AC01–AC09 |
