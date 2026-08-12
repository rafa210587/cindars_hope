# Resumo — Specs de `features_futuras/`

> Gerado por leitura direta dos 53 arquivos `.md` em `.specs/a_implementar/features_futuras/` (headers + Contexto/Scope). Documento **somente informativo** — não altera specs, não promove nada para execução. Para executar qualquer uma delas é preciso decisão humana explícita + mover a spec de volta para `.specs/a_implementar/` (ver `README.md` da pasta).

## Visão geral

A pasta contém **53 specs "future mapped"**: contratos preparados com antecedência para sistemas que ainda não entraram na fila de execução ativa. Não são features prontas para implementar — são "reserva de lugar" para não travar decisões futuras (ex.: não deixar o save schema quebrar quando pets ou companions avançados forem implementados de verdade).

Os grandes temas, por volume:

- **Companions** (14 specs no total, contando a wave 14, a ponte com social na 17 e a versão avançada na 24): brain de combate na caverna, elegibilidade/recrutamento, jobs de fazenda automatizados, UI/HUD, e uma versão avançada (party múltipla, tática, equipment completo) só mapeada, não gated para nenhuma wave concreta.
- **Bestiário / Conhecimento** (8 specs, waves 13 e 22): estado de conhecimento persistido sobre inimigos, eventos de descoberta, UI de compêndio, overlay de fraquezas no combate, serviços de pesquisa via NPC/livros/ruínas.
- **UI/UX** (6 specs, waves 04 e 18): navegação por gamepad, tela de detalhe social de NPC, diálogo avançado, localization keys, notificações/journal, menu de sistema/acessibilidade.
- **Calendário / Clima / Festival** (6 specs, waves 15 e 24): quadro público de calendário, festivais/minigames, modificadores sazonais de economia, modificadores profundos de clima/lua na caverna.
- **Cave save & Endgame presentation** (4 specs, wave 16): política de save dentro da caverna, snapshot/restore, cinemática da escolha final, modificadores de pós-jogo.
- **Social / Relacionamentos** (3 specs, wave 17, fora da ponte com companion): diálogo social, presentes, estado de relacionamento (amizade/romance/poliamor).
- **Level 100/101 (endgame)** (4 specs, wave 19): pré-condições do boss gate, encontros/recompensas anti-farm, câmara de sequência fixa, UI de save/portal.
- **Mana / Água Viva** (4 specs, wave 20): raiz dormente de mana na fazenda endgame, condições de crescimento via Água Viva, economia/crafting de mana, eventos lunares de corrupção.
- **Automação de fazenda** (4 specs, wave 21): planos/blueprints, relatórios de risco/custo, I/O de storage idempotente, governor global de automação.
- **Pets** (4 specs, wave 23): **todas em HOLD explícito** até aprovação de escopo — identidade/vínculo, alertas na caverna, área/alimentação, HUD/economia.
- **Quests** (4 specs, wave 03): anti-softlock, hooks de descoberta de bestiário, ferramentas de debug, hooks com a progressão principal/Fonte.

Quase todas trazem `Priority: P1/P2 Future Hook` e a instrução explícita de **não implementar UI/conteúdo final agora** — o objetivo declarado é impedir que um sistema futuro precise de refactor destrutivo por falta de contrato hoje.

---

## Tabela por domínio

### Quests (Wave 03)

| Spec | O que é (1 linha) | Nota |
|---|---|---|
| `03_spec_quest_anti_softlock_validation_future` | Validação formal para garantir que nenhuma quest crítica (principalmente a main quest) fique impossível de completar por item vendido, NPC fora de horário, clima perdido, morte etc. | Wave 03, P1 |
| `03_spec_quest_bestiary_discovery_objectives_future` | Hook para quests do tipo "descubra a fraqueza/comportamento de um monstro", sem montar a UI de bestiário ainda. | Future Hook, P2 |
| `03_spec_quest_debug_validation_tools_future` | Ferramenta dev mínima para inspecionar quests ativas, objectives, condições falhando e flags — não é UI de jogador. | Future Dev Tool, P2 |
| `03_spec_quest_fonte_main_progression_hooks_future` | Ponte entre o sistema genérico de quest e a progressão principal (4 atos/fragmentos) e a Fonte de Anya, sem duplicar estado. | Future Hook, P2 |

### UI / UX (Waves 04 e 18)

| Spec | O que é (1 linha) | Nota |
|---|---|---|
| `04_spec_ui_menu_gamepad_navigation_future` | Contrato de navegação por gamepad (focus order, back/cancel/confirm) para toda tela futura, sem mexer em Input Actions ainda. | Future, P2 |
| `04_spec_ui_social_npc_detail_future_runtime` | Tela de "cartão de NPC" (retrato, ocupação, preferências descobertas etc.) mostrando só o que o jogador já descobriu — não é o social system completo. | Future Hook, P2 |
| `18_spec_ui_dialogue_choice_confirmation_advanced_future_runtime` | Diálogos com escolhas que precisam de confirmação extra (ações especiais, texto sem spoiler). | Future mapped, P1 |
| `18_spec_ui_localization_text_keys_icon_conventions_future_runtime` | Convenção de chaves de texto/ícone para localização e textos sem spoiler, preparando tradução futura. | Future mapped, P2 |
| `18_spec_ui_notification_journal_feedback_history_future_runtime` | Histórico/journal de notificações com prioridade, sem duplicar aviso e sem travar a tela. | Future mapped, P1 |
| `18_spec_ui_system_menu_options_accessibility_hooks_future_runtime` | Menu de opções e hooks de acessibilidade (volume, reduzir flash, navegação), sem tocar em ProjectSettings ainda. | Future mapped, P1 |

### Bestiário / Conhecimento (Waves 13 e 22)

| Spec | O que é (1 linha) | Nota |
|---|---|---|
| `13_spec_bestiary_knowledge_state_save_load_future_runtime` | Estado salvável do que o jogador já sabe sobre cada inimigo (sem criar stats/drops novos). | Future mapped, P1 |
| `13_spec_bestiary_knowledge_ui_projection_future_runtime` | ViewModels/projeção para exibir esse conhecimento em telas e tooltips, sem montar a tela final. | Future mapped, P2 |
| `13_spec_knowledge_discovery_event_runtime_future` | Roteamento de eventos legítimos (combate, drops, quests) que alimentam o registro de conhecimento descoberto. | Future mapped, P1 |
| `13_spec_knowledge_research_npc_books_ruins_services_future_runtime` | Serviços de pesquisa via NPC, livros, ruínas e mapas, com nível de confiança e proteção contra spoiler. | Future mapped, P2 |
| `22_spec_bestiary_combat_hud_known_weakness_overlay_future_runtime` | Overlay no HUD de combate mostrando fraquezas/resistências já conhecidas do inimigo alvo. | Future mapped, P1 |
| `22_spec_bestiary_compendium_knowledge_log_ui_future_runtime` | Tela completa de bestiário/compêndio (cards, filtros, estados de descoberta). | Future mapped, P1 |
| `22_spec_knowledge_books_collections_documentation_achievements_future_runtime` | Livros/coleções/enciclopédia colecionáveis e achievements de documentação, com proteção de spoiler. | Future mapped, P2 |
| `22_spec_research_service_npc_laboratory_knowledge_unlock_future_runtime` | Serviço de pesquisa em laboratório via NPC — pedidos, custos, amostras, tempo, limites de desbloqueio. | Future mapped, P1 |

### Companions (Waves 14, ponte com 17, avançado 24)

| Spec | O que é (1 linha) | Nota |
|---|---|---|
| `14_spec_companion_cave_assist_brain_balance_future_runtime` | IA de combate do companion na caverna: seguir/atacar/stances, balance de DPS, estado caído→resgate. | SPEC-READY, **gated na WAVE 14**, P1 |
| `14_spec_companion_eligibility_recruitment_state_save_future_runtime` | Quem pode ser recrutado, estado base (vínculo/confiança/fadiga), equipamento do companion, save round-trip. | SPEC-READY, **gated na WAVE 14**, P1 |
| `14_spec_companion_farm_jobs_board_automation_future_runtime` | Endurece o quadro de 9 jobs de fazenda por companion já existente (atribuição, automação diária, toggle de fertilizante). | SPEC-READY, **gated na WAVE 14**, P1 |
| `14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime` | HUD compacto do companion, convite/visita e hooks de diálogo — só projeção/leitura, não muta estado. | SPEC-READY, **gated na WAVE 14**, P2 |
| `17_spec_partner_helper_companion_bridge_future_runtime` | Ponte entre o parceiro romântico e o sistema de companion/ajudante da fazenda, sem implementar IA ou cenas de romance. | Future mapped, P2 |
| `24_spec_companion_advanced_party_equipment_tactical_ai_future_runtime` | Mapeamento (não implementação) de companions avançados: múltiplos ao mesmo tempo, equipamento completo, skill tree própria, IA tática. | Future mapped, P2, **fechamento futuro sem wave definida** |

### Calendário / Clima / Festival (Waves 15 e 24)

| Spec | O que é (1 linha) | Nota |
|---|---|---|
| `15_spec_calendar_public_board_forecast_secret_visibility_future_runtime` | Quadro público de calendário com previsão de clima/lua e regra do que é visível vs. segredo. | Future mapped, P1 |
| `15_spec_festival_event_minigames_seasonal_activities_future_runtime` | Contratos de festival/minigame sazonal: janela de participação, recompensas, anti-exploit. | Future mapped, P2 |
| `15_spec_seasonal_economy_restock_demand_modifiers_future_runtime` | Modificadores de economia por estação/clima/festival/lua no reabastecimento de lojas, com proteção anti-arbitragem. | Future mapped, P1 |
| `15_spec_weather_lunar_cave_deep_modifiers_future_runtime` | Modificadores de clima/lua dentro da caverna (risco/recompensa, visibilidade, elite chance). | Future mapped, P2 |
| `24_spec_cave_weather_lunar_deep_modifiers_future_runtime` | Fecha os efeitos profundos de clima/lua na caverna (tempestade, neblina, neve, frio/calor + eventos lunares). | Future mapped, P2 |
| `24_spec_festival_minigames_event_framework_future_runtime` | Framework geral de festivais/minigames cruzando calendário, cidade, fazenda e social. | Future mapped, P2 |

### Cave Save & Apresentação de Final (Wave 16)

| Spec | O que é (1 linha) | Nota |
|---|---|---|
| `16_spec_cave_final_save_restriction_policy_future_runtime` | Política de onde é permitido salvar dentro da caverna (livre, checkpoint-only, sala segura). | Future mapped, P1 |
| `16_spec_cave_snapshot_save_provider_restore_order_future_runtime` | Provider de save da seção Cave: captura/restaura estado (nós esgotados, spawns, chefes derrotados). | Future mapped, P1 |
| `16_spec_final_choice_cinematic_presentation_future_runtime` | Apresentação da escolha final do jogo — preview, aviso, confirmação, cinemática, acessibilidade. | Future mapped, P2 |
| `16_spec_postgame_world_state_modifiers_endings_future_runtime` | Modificadores de mundo pós-jogo para os 3 finais (Proteger/Selar/Usar), sem aplicar visual final ainda. | Future mapped, P2 |

### Social / Relacionamentos (Wave 17)

| Spec | O que é (1 linha) | Nota |
|---|---|---|
| `17_spec_social_dialogue_visits_personal_quest_hooks_future_runtime` | Hooks de diálogo social, visitas de NPC à fazenda, milestones e quests pessoais — sem escrever diálogo final. | Future mapped, P2 |
| `17_spec_social_gifts_preferences_limits_future_runtime` | Sistema de presentes: tags, preferências, limites diário/semanal, descoberta, anti-exploit. | Future mapped, P1 |
| `17_spec_social_relationship_state_save_future_runtime` | Estado social salvável: amizade/confiança/afeto, romance/casamento/poliamor como estados mecânicos futuros. | Future mapped, P1 |

### Level 100/101 — Endgame (Wave 19)

| Spec | O que é (1 linha) | Nota |
|---|---|---|
| `19_spec_level_100_boss_gate_preconditions_future_runtime` | Pré-condições para abrir o boss gate do nível 100 (fragmentos/chaves) e desbloqueio do nível 101. | Future mapped, P1 |
| `19_spec_level_101_encounters_rewards_anti_farm_future_runtime` | Regras de encontro/recompensa do nível 101: bosses fixos, sem farm, sem packs comuns. | Future mapped, P1 |
| `19_spec_level_101_fixed_sequence_chamber_runtime_future` | Sequência fixa de câmaras/estágios de lore do nível 101 (contrato, sem assets/cenas ainda). | Future mapped, P1 |
| `19_spec_level_101_ui_save_portal_handoff_future_runtime` | UI de aviso, limite de save e handoff de portal/escolha final do nível 101. | Future mapped, P2 |

### Mana / Água Viva (Wave 20)

| Spec | O que é (1 linha) | Nota |
|---|---|---|
| `20_spec_dormant_mana_root_endgame_farm_future_runtime` | Raiz Dormente de Mana como cultivo especial na fazenda endgame (não é crop comum). | Future mapped, P1 |
| `20_spec_living_water_mana_growth_conditions_future_runtime` | Água Viva como condição rara para gerar Mana e purificação — evita banalizar cura/MP. | Future mapped, P1 |
| `20_spec_mana_economy_crafting_magic_anti_exploit_future_runtime` | Regras de Mana na economia/crafting/magia: limites de venda, custo de reagente, anti-exploit. | Future mapped, P1 |
| `20_spec_mana_lunar_events_corruption_risk_future_runtime` | Eventos lunares (Alihana/Senya/Nyx) e risco de corrupção ligados à Mana, com proteção da Pedra Negra. | Future mapped, P2 |

### Automação de Fazenda (Wave 21)

| Spec | O que é (1 linha) | Nota |
|---|---|---|
| `21_spec_farm_automation_plan_blueprint_scheduling_future_runtime` | Planos/blueprints/agendamento de automação: áreas, ordens de trabalho, prioridade. | Future mapped, P1 |
| `21_spec_farm_automation_reports_risk_cost_balance_future_runtime` | Relatórios de automação: custo, risco, output esperado vs. real, falhas. | Future mapped, P2 |
| `21_spec_farm_automation_storage_io_idempotency_future_runtime` | Contrato de entrada/saída de baús para automação, com reservas/commits/rollback (sem duplicar item). | Future mapped, P1 |
| `21_spec_farm_full_automation_governor_future_runtime` | Governor global que limita orçamento/permissões da automação por dia/semana (anti-exploit). | Future mapped, P1 |

### Pets — **todas em HOLD** (Wave 23)

| Spec | O que é (1 linha) | Nota |
|---|---|---|
| `23_spec_pet_cave_alerts_treasure_trap_light_support_future_runtime` | Pet acompanhando na caverna: alertas de tesouro/armadilha, suporte leve de luz. | **HOLD até aprovação de escopo**, P1 |
| `23_spec_pet_core_identity_bond_routine_save_future_runtime` | Identidade/vínculo/humor/energia/rotina do pet e contrato de save. | **HOLD até aprovação de escopo**, P1 |
| `23_spec_pet_home_area_feeding_items_farm_hints_future_runtime` | Área do pet na fazenda (cama/tigela/brinquedo), alimentação e dicas leves de fazenda. | **HOLD até aprovação de escopo**, P1 |
| `23_spec_pet_hud_feedback_data_assets_economy_future_runtime` | HUD leve do pet, feedback e compatibilidade com a economia de itens. | **HOLD até aprovação de escopo**, P2 |

---

## Candidatas a repensar

- **`24_spec_companion_advanced_party_equipment_tactical_ai_future_runtime`** — é um "mapeamento de não fazer agora" sem wave gate definida (diferente das outras specs de companion que são explicitamente gated na WAVE 14); risco de ficar pairando indefinidamente sem nunca virar contrato acionável. Vale decidir se isso deveria ser um documento de design (`docs/design/`) em vez de uma spec formal.
- **As 4 specs de `23_spec_pet_*`** — todas marcadas `HOLD until explicit pet-scope approval`. Especificar 4 contratos completos (identidade, HUD, área/alimentação, cave alerts) para uma feature que nem tem aprovação de escopo é um investimento grande antes da decisão "vamos ter pets ou não". Vale considerar consolidar em 1 spec guarda-chuva até a aprovação vir, e só then quebrar em 4.
- **`13_spec_*` vs `22_spec_*` de Bestiário/Conhecimento** — há sobreposição de domínio entre a wave 13 (estado, eventos de descoberta, UI projection, pesquisa via NPC) e a wave 22 (HUD de combate, compêndio UI, livros/achievements, serviço de pesquisa em laboratório). `13_spec_knowledge_research_npc_books_ruins_services_future_runtime` e `22_spec_research_service_npc_laboratory_knowledge_unlock_future_runtime` em particular parecem descrever a mesma ideia (pesquisa via NPC) em dois momentos diferentes — vale checar se uma supersede a outra antes de gastar esforço nas duas.
- **`15_spec_weather_lunar_cave_deep_modifiers_future_runtime`** (wave 15) e **`24_spec_cave_weather_lunar_deep_modifiers_future_runtime`** (wave 24) — nomes quase idênticos e mesmo domínio (clima/lua na caverna). A wave 24 se descreve como o "fechamento" da wave 15, mas isso deveria estar mais explícito no header da 15 (ou as duas deveriam virar uma spec só com fases).
- **`17_spec_partner_helper_companion_bridge_future_runtime`** — fica no meio-termo entre a wave 17 (social) e a wave 14 (companion); como não tem repo lock scope de companion runtime, existe risco de virar uma spec "fantasma" que nenhuma das duas waves reivindica na hora de implementar.
