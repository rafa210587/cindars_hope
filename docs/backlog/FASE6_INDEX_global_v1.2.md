# Cindar's Hope — Fase 6: Índice de Épicos e Backlog Global v1.2

> **Fase:** 6 de 13
> **Status:** ✅ Índice base — FARM detalhado e specs MVP geradas; demais épicos seguem em esboço
> **Última atualização:** 2026-05-16

---

## HANDOFF

### Status dos épicos

| Épico | Arquivo | Status |
|---|---|---|
| **FARM** — Fazenda completa | `FASE6_FARM_backlog_v1.2.md` | ✅ Stories escritas; specs MVP geradas na Fase 7 |
| **CHAR** — Personagem, atributos, skill tree | FASE6_CHAR_backlog.md | â³ Esboço abaixo |
| **SAVE** — Sistema de save/load | Dentro do FARM no MVP; separar em backlog próprio pós-MVP | ðŸ”„ Parcial no MVP |
| **CITY** — Cidade, NPCs, comércio | FASE6_CITY_backlog.md | â³ Esboço abaixo |
| **CRAFT** — Workshops, receitas | FASE6_CRAFT_backlog.md | â³ Esboço abaixo |
| **CAVE** — Caverna procedural | FASE6_CAVE_backlog.md | â³ Esboço abaixo |
| **COMBAT** — Sistema de combate | FASE6_COMBAT_backlog.md | â³ Esboço abaixo |
| **COMP** — Companions | FASE6_COMP_backlog.md | â³ Esboço abaixo |
| **UI** — Interface, HUD, menus | FASE6_UI_backlog.md | â³ Esboço abaixo |

---

## SEQUÊNCIA DE DESENVOLVIMENTO

### Ponte para Fase 7 e Fase 8

O detalhamento implementável do MVP Fazenda está em `FASE7_SPEC_MVP_FARM_v2.2.md`. A próxima execução prática é a Fase 8, começando pelas specs FARM-001, FARM-002, FARM-003, FARM-011, FARM-012, FARM-013, FARM-014, FARM-015, FARM-016, FARM-021, FARM-022, FARM-031, FARM-032, FARM-041, FARM-042, FARM-051, FARM-052, FARM-061, FARM-062, FARM-071, FARM-072 e FARM-SELL.


Ordem de implementação baseada em dependências:

```
FASE 8 — MVP FAZENDA
  FARM-001 a FARM-072 (MVP)
  CHAR-001 a CHAR-020 (atributos básicos, HP, fome)
  SAVE-001 a SAVE-010 (save simples slot único)
  UI-001 a UI-020 (HUD básico: HP, fome, dia, inventário texto)

FASE 9A — EXPANSÃO FAZENDA
  FARM-V2 completo (tempo automático, animações, pesca variada)
  CITY-001 a CITY-040 (cidade básica, NPCs, venda)
  CRAFT-001 a CRAFT-030 (workshops nível 1, 3 receitas)

FASE 9B — CAVERNA E COMBATE
  CAVE-001 a CAVE-050 (geração procedural, navegação)
  COMBAT-001 a COMBAT-040 (combate básico físico)
  CHAR-021 a CHAR-040 (skill tree ramo físico)

FASE 9C — COMPANIONS
  COMP-001 a COMP-040 (recrutamento, jobs, caverna)
  COMBAT-041 a COMBAT-060 (magia, combate mágico)
  CHAR-041 a CHAR-060 (skill tree ramo mágico)

FASE 9D — SISTEMAS COMPLETOS
  CAVE-FULL (100 níveis, biomas, checkpoints, mercadores)
  CRAFT-FULL (todos workshops nível 3, todas receitas)
  CITY-FULL (reputação, quests, companions recrutáveis)

FASE 10 — ARTE E POLISH
  Sprites finais, animações, audio, VFX

FASE 11–13 — TESTES, BUILD, LAUNCH
```

---

## ESBOÇO — ÉPICO CHAR (Personagem)

*Detalhamento completo em arquivo próprio após aprovação do FARM*

### Stories previstas:

**MVP:**
- CHAR-001: Jogador tem HP máximo (100 base, hardcodado)
- CHAR-002: Jogador tem barra de fome (ver FARM-061/062)
- CHAR-003: Jogador pode morrer (HP = 0) e respawna na fazenda
- CHAR-004: Penalidade de morte: perde itens não equipados + 50% do ouro

**V2:**
- CHAR-010: Sistema de XP — jogador ganha XP por colheita, pesca, combate
- CHAR-011: Level up — ao atingir XP necessário, sobe de nível
- CHAR-012: Pontos de atributo — level up dá 2 pontos para distribuir
- CHAR-013: Atributos afetam stats (Força → dano, CON → HP máximo, etc.)
- CHAR-014: Seleção de raça na criação (Anão / Humano) com bônus corretos

**FULL:**
- CHAR-020: Skill tree — 3 ramos, pontos por level
- CHAR-021: Ramo Produção ativo e funcional
- CHAR-022: Ramo Combate Físico ativo
- CHAR-023: Ramo Combate Mágico ativo
- CHAR-024: Status negativos completos (frio, calor, medo, veneno, exaustão)

---

## ESBOÇO — ÉPICO CITY (Cidade)

*Detalhamento completo em arquivo próprio após aprovação do FARM*

### Stories previstas:

**V2 (primeira implementação):**
- CITY-001: TownScene existe e é acessível via portal na fazenda
- CITY-002: NPC Pip Miudinho — mercador geral, compra e vende
- CITY-003: Vender itens colhidos na fazenda (recebe ouro)
- CITY-004: Comprar sementes básicas (Trigo, Cenoura)
- CITY-005: UI de loja (lista de texto → grade com ícones)
- CITY-006: Carisma afeta preços de compra e venda

**FULL:**
- CITY-010: Todos os 13 NPCs implementados com diálogos
- CITY-011: Horários de funcionamento dos NPCs
- CITY-012: Sistema de reputação (0–100, thresholds com efeitos)
- CITY-013: Quests básicas de entrega e coleta
- CITY-014: NPCs mudam diálogo conforme lua e season ativas
- CITY-015: Comprar vacas (Gruta ou NPC pecuário)
- CITY-016: Hund & Gurd — contratar expansão da fazenda
- CITY-017: Yael — loja noturna com reagentes raros

---

## ESBOÇO — ÉPICO CRAFT (Workshops)

*Detalhamento completo em arquivo próprio após aprovação do FARM*

### Stories previstas:

**V2:**
- CRAFT-001: Forja nível 1 existe na fazenda e pode ser construída
- CRAFT-002: UI de crafting (menu de texto com receitas disponíveis)
- CRAFT-003: Receita 1 — Poção de Cura Simples (Erva Medicinal x2)
- CRAFT-004: Receita 2 — Ferramenta de Corte (Madeira x5 + Pedra x3)
- CRAFT-005: Receita 3 — Espada de Madeira (Madeira x8 + Pedra x2)
- CRAFT-006: Ingredientes são descontados do inventário ao craftar
- CRAFT-007: Item craftado vai para o inventário

**FULL:**
- CRAFT-010: Todos os 4 workshops implementados
- CRAFT-011: Upgrade de workshops nível 1 → 2 → 3
- CRAFT-012: Blueprints — receitas bloqueadas que precisam ser desbloqueadas
- CRAFT-013: Sistema de tempo de crafting (não é instantâneo)
- CRAFT-014: Companion Artesão crafta automaticamente quando alocado
- CRAFT-015: UI visual completa com ícones e progresso

---

## ESBOÇO — ÉPICO CAVE (Caverna)

*Detalhamento completo em arquivo próprio após aprovação do FARM*

### Stories previstas:

**V2 (primeira implementação):**
- CAVE-001: Entrada da caverna existe na FarmScene (portal)
- CAVE-002: CaveScene carrega com geração procedural (seed hardcodada no V2)
- CAVE-003: Jogador pode navegar pelos corredores gerados
- CAVE-004: Nível 1 da caverna tem mapa finito com saída para nível 2
- CAVE-005: HUD mostra nível atual da caverna

**FULL:**
- CAVE-010: 100 níveis com 8 biomas distintos
- CAVE-011: Seed única por save (gerada no início)
- CAVE-012: Checkpoints permanentes a cada 10 níveis
- CAVE-013: Recursos renováveis a cada 1 dia in-game
- CAVE-014: Saída via Pedra de Retorno ou voltar pelo caminho
- CAVE-015: Guilda das Estradas (mercadores aleatórios nos níveis)
- CAVE-016: Ciclo dia/noite afeta criaturas na caverna

---

## ESBOÇO — ÉPICO COMBAT (Combate)

*Detalhamento completo em arquivo próprio após aprovação do FARM*

### Stories previstas:

**V2:**
- COMBAT-001: Inimigo básico (Slime) existe no nível 1 da caverna
- COMBAT-002: Jogador ataca com tecla (J ou espaço) — hitbox simples
- COMBAT-003: Inimigo tem HP e morre ao chegar a 0
- COMBAT-004: Inimigo ataca jogador por proximidade
- COMBAT-005: Inimigo dropa loot ao morrer (item fixo no V2)

**FULL:**
- COMBAT-010: Múltiplos tipos de inimigos por bioma
- COMBAT-011: Magias disponíveis (3 elementos básicos)
- COMBAT-012: Knockback nos ataques
- COMBAT-013: Status negativos por inimigos (veneno, medo, frio)
- COMBAT-014: Companion combate ao lado do jogador
- COMBAT-015: Chefes nos níveis 10, 25, 40, 55, 70, 85 e 100

---

## ESBOÇO — ÉPICO COMP (Companions)

*Detalhamento completo em arquivo próprio após aprovação do FARM*

### Stories previstas:

**V2:**
- COMP-001: Companion Zrix disponível para recrutar (pré-configurado)
- COMP-002: Companion alocado para job "Lenhador" na fazenda
- COMP-003: Lenhador corta árvoras automaticamente quando disponíveis
- COMP-004: Companion ativo segue o jogador na caverna

**FULL:**
- COMP-010: Todos os 9 NPCs recrutáveis com quests de recrutamento
- COMP-011: 7 tipos de job com eficiência baseada em archetype
- COMP-012: Companion pode morrer; ressurreição na Fonte da Fazenda
- COMP-013: Sistema de afinidade (deferido para versão futura)
- COMP-014: Companion Explorador vai à caverna autonomamente

---

## ESBOÇO — ÉPICO UI (Interface)

*Detalhamento completo em arquivo próprio após aprovação do FARM*

### Stories previstas:

**MVP:**
- UI-001: HUD básico (HP barra, Fome barra, Dia número)
- UI-002: Menu de inventário (lista de texto, tecla I)
- UI-003: Tela de início (novo jogo / continuar)
- UI-004: Mensagem de "Jogo salvo"

**V2:**
- UI-010: Inventário em grade com ícones 32x32
- UI-011: Hotbar de acesso rápido (8 slots)
- UI-012: HUD completo com hora, lua, season
- UI-013: UI de loja (grade com preços)
- UI-014: Tela de seleção de 3 slots de save

**FULL:**
- UI-020: Tela de criação de personagem (raça, gênero, nome)
- UI-021: Skill tree visual navegável
- UI-022: Mapa da fazenda (minimapa)
- UI-023: Journal de quests
- UI-024: Bestiary (criaturas encontradas)

---

*Índice Global v1.1 — FARM detalhado e specs MVP já geradas. Demais épicos seguem em esboço para detalhamento pós-MVP.*


---

## EXECUÇÃO DA FASE 8 — Plano por PRs pequenos

A Fase 8 deve seguir `FASE8_EXECUTION_PLAN_CODEX_v1.0.md`. A ordem abaixo substitui a ideia de implementar uma spec inteira de uma vez.

| PR | Objetivo | Specs relacionadas | Resultado esperado |
|---|---|---|---|
| PR-001 | Core foundation | FARM-001 | GameEventBus + eventos mínimos compila |
| PR-002 | Data contracts | FARM-011, FARM-021, FARM-071 | ItemDataSO, SeedDataSO, registries e save DTOs básicos |
| PR-003 | Bootstrap managers | FARM-001, FARM-031 | PlayerManager, TimeManager, InventoryManager vazios/funcionais |
| PR-004 | New game state | FARM-001, FARM-021 | Inventário inicial e ouro inicial a partir de PlayerDataSO |
| PR-005 | FarmScene mínima | FARM-001, FARM-003 | Cena abre, chão/borda/player placeholder |
| PR-006 | Movimento e colisão | FARM-002 | Player se move, câmera segue, bordas bloqueiam |
| PR-007 | Interação genérica | FARM-014 | IInteractable + InteractionSystem + hint |
| PR-008 | Canteiros e plantio | FARM-012, FARM-016 | 9 plots, menu de plantio textual, consome semente |
| PR-009 | Crescimento por dia | FARM-013, FARM-031, FARM-032 | TAB avança dia, planta cresce, HUD mostra dia |
| PR-010 | Colheita e inventário | FARM-015, FARM-021, FARM-022 | Colher adiciona itens e inventário mostra stack |
| PR-011 | Fome e consumo | FARM-061, FARM-062 | Fome diminui/restaura e HUD atualiza |
| PR-012 | Árvores | FARM-041, FARM-042 | Cortar árvore gera madeira |
| PR-013 | Lago e pesca | FARM-051, FARM-052 | Pescar gera peixe comum |
| PR-014 | Venda | FARM-SELL, FARM-032 | Vender item gera ouro e HUD atualiza |
| PR-015 | Save | FARM-071 | Save JSON em persistentDataPath |
| PR-016 | Load/Boot | FARM-072 | Reabrir restaura dia, inventário, ouro e plots |
| PR-017 | VFX/UI mínimo | FARM-015, FARM-031, FARM-042, FARM-052 | Feedbacks simples sem bloquear MVP |
| PR-018 | Hardening MVP | Todas MVP | Correções, testes e checklist vertical final |

### Definition of Ready para cada PR

- [ ] Escopo cabe em uma revisão pequena.
- [ ] Arquivos permitidos estão listados.
- [ ] Arquivos proibidos estão listados.
- [ ] Teste manual está definido.
- [ ] Critério de rollback é simples.
- [ ] Não mistura MVP com V2/FULL.

### Definition of Done para cada PR

- [ ] Unity compila sem erro.
- [ ] Console sem erros novos.
- [ ] Critério de aceite da spec foi validado.
- [ ] Nenhuma regra do `CLAUDE_v1.2.md` foi violada.
- [ ] Não há `GameObject.Find`, `FindObjectOfType` ou `FindObjectsByType` em runtime.
- [ ] Save, quando envolvido, usa `Application.persistentDataPath`.
- [ ] Dados persistidos usam IDs, não referência Unity.
- [ ] Commit em português feito.




