# Cindar's Hope — Design Documents

> Status: fonte ativa de direção de produto, gameplay, lore e UX.

Esta pasta guarda documentos de **descrição ampla e direcional** do projeto.

Eles não são specs implementáveis e não devem ser tratados como backlog direto de Codex/Claude Code.

As versões históricas originais continuam preservadas em `docs_old/`.

## Diferença entre design, refinamento e spec

| Tipo | Local | Função |
|---|---|---|
| Design direction | `docs/design/` | Descrever como cada parte do jogo deve funcionar, qual é a visão, regras, mecanismos, lore e intenção de produto. |
| Pré-refinamento transitório | `docs/refinements/a_implementar/pre_refinamentos/` | Rascunhos vivos, análises e exploração de decisões antes de virar design consolidado ou spec. |
| Refinamento implementado | `docs/refinements/implementados/` | Contexto histórico, auditorias, waves e decisões já absorvidas. |
| Spec implementável | `docs/specs/a_implementar/` | Documento operacional para execução com escopo, dependências, arquivos permitidos/proibidos, critérios de aceite e validação. |
| Spec implementada/parcial | `docs/specs/implementados/` | Registro do que já existe no repo e seu estado real. |

## Regra principal

Documentos em `docs/design/` respondem:

```text
Como esta parte do jogo deve funcionar?
Por que ela existe?
Quais são as regras de produto?
Quais mecanismos e funcionalidades compõem o sistema?
Como ela se conecta a lore, progressão, economia, UI e save?
```

Specs em `docs/specs/` respondem:

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
  gameplay/
    farm/
      FARM_DESIGN_DIRECTION_v1.3.md
    city/
      CITY_DESIGN_DIRECTION_v1.1.md
      CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.0.md
    cave/
      CAVE_DESIGN_DIRECTION_v1.0.md
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
Spec quebrada em docs/specs/a_implementar/
  ↓
Implementação
  ↓
Registro em docs/specs/implementados/ e docs/refinements/implementados/
```

## Estado atual

Canon jogável ativo de Vaalara:

```text
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
```

Documento ativo consolidado de direção ampla da fazenda:

```text
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
```

Documento ativo consolidado de direção ampla da cidade:

```text
docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.1.md
```

Roster ativo de cidadãos/serviços da cidade:

```text
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.0.md
```

As versões anteriores de fazenda ficam como histórico incremental:

```text
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.1.md
docs/design/gameplay/farm/FARM_DESIGN_DECISIONS_v1.2.md
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.2.md
```

Usar `VAALARA_GAME_CANON_DIRECTION_v1.0.md` antes de qualquer novo design direction de cidade, caverna, magia/progressão, companions ou lore.

Usar `FARM_DESIGN_DIRECTION_v1.3.md` antes de quebrar novas specs de layout, crops, fertilizantes, animais, automação, pets, cansaço, Fonte de Anya, Água Viva, Raiz Dormente de Mana, pedreira final e economia agrícola.

Usar `CITY_DESIGN_DIRECTION_v1.1.md` e `CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.0.md` antes de quebrar novas specs de cidade, NPCs, lojas, reputação, calendário, festivais, contratos, guildas, templo de Kanthor, altares, visitas à fazenda e integração com caverna.
