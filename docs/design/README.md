# Cindar's Hope — Design Documents

> Status: fonte ativa de direção de produto, gameplay, lore e UX.

Esta pasta guarda documentos de **descrição ampla e direcional** do projeto.

Eles não são specs implementáveis e não devem ser tratados como backlog direto de Codex/Claude Code.

## Regra de documento canônico único

Para cada tema de design, deve existir **um único documento ativo**.

Não manter versões paralelas como:

```text
*_v1.0.md
*_v1.1.md
*_v1.2.md
```

Quando um refinamento alterar uma área já documentada:

```text
Atualizar o mesmo arquivo canônico.
Não criar uma nova versão paralela.
Não deixar documento antigo coexistindo na mesma pasta.
Usar o histórico do Git como versionamento.
```

Exceção: specs implementáveis em `.specs/` podem ter nomes específicos por entrega, porque representam unidades diferentes de implementação.

## Mapa e processo obrigatórios para criação de specs

Antes de criar qualquer spec em `.specs/a_implementar/`, consultar:

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
```

O `SPEC_SOURCE_MAP.md` define quais documentos de design devem ser lidos antes de criar/refinar cada spec.

O `SPECIFICATION_PROCESS.md` define como transformar design direction em spec implementável.

Toda spec deve declarar uma seção:

```md
## Fontes obrigatórias lidas

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/...
```

## Diferença entre design, refinamento e spec

| Tipo | Local | Função |
|---|---|---|
| Design direction | `docs/design/` | Descrever como cada parte do jogo deve funcionar, qual é a visão, regras, mecanismos, lore e intenção de produto. Deve existir um único arquivo canônico por tema. |
| Source map | `docs/design/SPEC_SOURCE_MAP.md` | Dizer quais documentos devem ser lidos antes de criar/refinar cada spec. |
| Specification process | `docs/design/SPECIFICATION_PROCESS.md` | Definir o método obrigatório para transformar design em spec implementável. |
| Pré-refinamento transitório | `docs/refinements/a_implementar/pre_refinamentos/` | Rascunhos vivos, análises e exploração de decisões antes de virar design consolidado ou spec. |
| Refinamento implementado | `docs/refinements/implementados/` | Contexto histórico, auditorias, waves e decisões já absorvidas. |
| Spec implementável | `.specs/a_implementar/` | Documento operacional para execução com escopo, dependências, arquivos permitidos/proibidos, critérios de aceite e validação. |
| Spec implementada/parcial | `.specs/implementados/` | Registro do que já existe no repo e seu estado real. |

## Regra principal

Documentos em `docs/design/` respondem:

```text
Como esta parte do jogo deve funcionar?
Por que ela existe?
Quais são as regras de produto?
Quais mecanismos e funcionalidades compõem o sistema?
Como ela se conecta a lore, progressão, economia, UI e save?
```

Specs em `.specs/` respondem:

```text
O que exatamente será implementado agora?
Quais arquivos podem mudar?
Quais critérios de aceite validam a entrega?
Como testar no Unity?
```

## Estrutura recomendada

```text
docs/design/
  README.md
  SPEC_SOURCE_MAP.md
  SPECIFICATION_PROCESS.md
  gameplay/
    farm/
      FARM_DESIGN_DIRECTION_v1.3.md
      FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
    city/
      CITY_DESIGN_DIRECTION_v1.2.md
      CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
      CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
    cave/
      CAVE_DESIGN_DIRECTION.md
      CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md
      CAVE_MONSTER_ROSTER_DIRECTION.md
      CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
      CAVE_MONSTER_VISUAL_SPRITE_DIRECTION.md
    player/
      PLAYER_CORE_SYSTEMS_DIRECTION.md
      PLAYER_SKILL_TREES_DIRECTION.md
      PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
    combat_magic_progression/
      COMBAT_MAGIC_PROGRESSION_DESIGN_DIRECTION_v1.0.md
    companions/
      COMPANIONS_DESIGN_DIRECTION_v1.0.md
    ui_ux/
      UI_UX_DESIGN_DIRECTION_v1.0.md
  lore/
    VAALARA_GAME_CANON_DIRECTION_v1.0.md
    WORLD_LORE_DIRECTION_v1.0.md
    CINDAR_ANYA_FONTE_ELYNDOR_DIRECTION_v1.0.md
```

## Fluxo recomendado

```text
Ideia / discussão
  ↓
Pré-refinamento em docs/refinements/a_implementar/pre_refinamentos/
  ↓
Design direction consolidado em docs/design/
  ↓
Consulta obrigatória a docs/design/SPEC_SOURCE_MAP.md
  ↓
Consulta obrigatória a docs/design/SPECIFICATION_PROCESS.md
  ↓
Spec quebrada em .specs/a_implementar/
  ↓
Implementação
  ↓
Registro em .specs/implementados/ e docs/refinements/implementados/
```

## Estado atual

Canon jogável ativo de Vaalara:

```text
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
```

Mapa de fontes para specs:

```text
docs/design/SPEC_SOURCE_MAP.md
```

Processo canônico de especificação:

```text
docs/design/SPECIFICATION_PROCESS.md
```

Documento ativo consolidado de direção ampla da fazenda:

```text
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
```

Escala, layout, footprints, construções, props e pixels da fazenda:

```text
docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
```

Documento ativo consolidado de direção ampla da cidade:

```text
docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
```

Roster ativo de cidadãos/serviços/relacionamentos da cidade:

```text
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
```

Layout, construções, interiores, camas, schedules, física e roadmap da cidade:

```text
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
```

Documento ativo consolidado de direção ampla da caverna:

```text
docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
```

Geração procedural, tamanho dos níveis e randomização de biomas da caverna:

```text
docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md
```

Roster ativo de monstros, bosses, packs, XP, drops e bestiário da caverna:

```text
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
```

Balanceamento, vulnerabilidades e janelas de crítico da caverna:

```text
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
```

Direção visual de sprites, silhuetas, animações e variações dos monstros da caverna:

```text
docs/design/gameplay/cave/CAVE_MONSTER_VISUAL_SPRITE_DIRECTION.md
```

Documento ativo consolidado dos sistemas centrais do personagem:

```text
docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
```

Direção detalhada das skill trees do personagem:

```text
docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
```

Direção dos atributos derivados do personagem:

```text
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
```

## Leitura mínima antes de specs

Toda spec de gameplay/lore deve começar lendo:

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
```

Specs de fazenda devem ler também:

```text
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
```

Specs de cidade devem ler também:

```text
docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
```

Specs de caverna devem ler também:

```text
docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_VISUAL_SPRITE_DIRECTION.md
docs/game_rules/cave_rules.md
docs/decisions/ADR-0005-cave-stable-run-and-replay.md
```

Specs de personagem devem ler também:

```text
docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
```
