# Auditoria de habilidades, capacidades e interface — 2026-09-09

> Escopo: código, dados serializados e ligação entre sistemas de habilidades; contexto geral pelo CURRENT_STATE.
> Natureza: auditoria estática. Não representa execução ou aceitação visual em Play Mode.
> Base: HEAD `8c37246911072209dd544d6f21b1c2f2495e1fd1` **mais working tree existente**. Há alterações concorrentes extensas.
> Entrega associada: [refinamento proposto](../refinements/a_implementar/ref_skills_capabilities_gameplay_ui_pixelart_v1.md).

## 1. Onde o projeto está

Existe infraestrutura extensa: progressão, cinco árvores, compras, ranks, slots, agregação de bônus,
executores, combate, necessidades, crafting e providers de save. O problema não é ausência de sistema.
É a distância entre catálogo, comportamento efetivo, objetos conectados na cena e apresentação ao jogador.

O CURRENT_STATE de 08/09 registra implementação ampla do MVP, aceitação humana pendente, quatro falhas
de farm no baseline e falhas de wiring de assets/gates, incluindo Fireball. Esses resultados são
**históricos**, não testes executados nesta auditoria. Este trabalho não reaudita integralmente os demais domínios.

A hipótese do usuário é sustentada: há investimentos sem efeito, efeitos diferentes dos anunciados e
uma interface técnica incompleta. Entretanto, dizer que nenhuma habilidade tem efeito seria incorreto:
existem dano, projéteis, restauração, slow e bônus efetivamente consumidos.

## 2. Método e limites

- Leitura do roteador, CURRENT_STATE, ADR-0010, regras de skills e direção visual aplicável.
- Rastreamento catálogo → compra/rank → slot → executor → consumidor → feedback → save.
- Buscas `rg` por consumidores, chamadas e GUIDs; inspeção read-only de cenas/assets.
- Auditorias independentes de execução, progressão/passivas e UI; consolidação e checagens pelo agente principal.
- Codex CLI `gpt-5.3-codex-spark`, reasoning high, produziu proposta textual em sessão efêmera
  `01a088ff-1ed3-77c0-bb35-2c0bd0af815c`. Leituras do subprocesso foram bloqueadas; sua redação utilizou
  o pacote de evidências fornecido. Sua saída não foi aceita como verificação independente do código.
- O principal revisou a proposta, removendo afirmações sem evidência e cenários contraditórios.
- Uma segunda execução Spark (`01a08907-0f5f-7350-bd45-032b432c956c`, identificador do turno) revisou
  o documento fornecido integralmente, sem ferramentas. O principal incorporou matriz de falhas/custos,
  ordem de migração/respec, projeção de cooldown, compatibilidade por nó e critérios visuais; não adotou
  sugestão de executar respec durante load nem exigência prematura de um novo ledger/evento persistido.
- Não foram alterados runtime, cenas, prefabs, assets ou regras aceitas por esta entrega.

As referências abaixo usam caminhos relativos à raiz e linhas do snapshot auditado. Edição concorrente
pode deslocar linhas; nomes de classes/métodos e IDs são as âncoras duráveis.

## 3. Inventário reconciliado

| Superfície | Resultado |
|---|---|
| Catálogo construído | 66 nós: Melee 13, Ranged 11, Magic 13, Survival 15, Crafting 14 |
| Categorias | 35 passivos/capstones; 31 ativas |
| Execução | 30 registros: 7 melee, 11 projéteis, 1 slow, 4 restauração, 1 irrigação, 6 feedback-only |
| Ativa sem mapeamento | `skill_crafting_quick_repair` |
| Passivos sem efeito conectado | Pelo menos 9/35; os demais incluem efeitos parciais/divergentes |
| Arte persistida de nós | 68 assets encontrados; todos com `Icon: {fileID: 0}`. Isso não significa 68 nós no catálogo vivo |
| Menu instalado | `SkillTreeGameplayPanelController`, IMGUI |
| HUD Canvas de skills | Classe/projeção existentes; referências visuais não ligadas no caminho de criação auditado |

`SkillTreeManager.BuildCatalog` (`Assets/_Game/Scripts/Skills/SkillTreeManager.cs:97`) usa assets somente
se registry e database estiverem presentes; caso contrário, constrói `DefaultSkillCatalog`.
Os dois campos estão nulos nos managers inspecionados em Cave (`CaveScene.unity:1200`),
Farm (`FarmScene.unity:320923`) e Town (`TownScene.unity:996292`). Portanto corrigir apenas os 68 assets
não garante alterar o catálogo usado nessas cenas.

## 4. Achados prioritários

### A01 — P1: origem da execução pode ser o bootstrap, não o personagem

`ActiveSkillExecutionController` resolve caster por `GameBootstrap.PlayerManager.gameObject`; executores
usam esse Transform para origem/direção/movimento e o resolver busca interação nesse objeto.
Farm/Town possuem PlayerManager no `_Bootstrap` e PlayerController no Player separado. A ligação
não equivale à referência ao corpo do jogador. Há risco concreto de projétil nascer na origem errada,
avanço mover bootstrap e irrigação não resolver interação. Cave apresenta dois PlayerManagers;
o registro deve ser reconciliado para não depender da ordem de inicialização.

Critério de investigação dinâmica: comparar instance IDs e posições de caster, corpo físico e origem do
efeito nas três cenas antes de avaliar alcance/dano. Isto é um achado estático, não uma captura do bug.

Âncoras: `ActiveSkillExecutionController.cs:140,228`; `ProjectileSkillEffectExecutor.cs:84`;
`MeleeStrikeSkillEffectExecutor.cs:72–82`. Cenas: Farm PlayerController em `107517`, PlayerManager em
`320833`; Town em `796977` e `996456`; Cave PlayerManagers em `340` e `1324`.

### A02 — P1: pontos de skill possuem saldos divergentes

`SkillRespecService.cs:33` calcula reembolso por nível, omitindo pontos de atos. `SkillTreeManager.cs:237`
recalcula efeitos, sem reembolsar o saldo da progressão. A próxima compra, em `:136`, sobrescreve o saldo
local com `PlayerProgressionManager.UnspentSkillPoints`.

Reprodução derivada do código: nível 10, cinco pontos gastos e zero disponíveis → respec dá cinco no
state da árvore → próxima compra sincroniza zero e falha. Não foi encontrado subscriber de
`SkillTreeRespecCompletedEvent` que feche essa lacuna. Grants de atos e reembolso de IDs removidos também
precisam manter a mesma autoridade: manager assina nível, enquanto progressão publica grant específico.

### A03 — P1: o menu não expõe rank-up nem escolha de capstone

`SkillTreeGameplayPanelController.cs:276,400` chama compra sem variante; `:396` desabilita compra após
aquisição. `SkillTreeManager.cs:166` tem rank-up, mas o painel não chama essa operação. A compra de
capstones exige variante em `SkillPurchaseService.cs:99`. A operação existe no domínio e falta no fluxo.
`RequirementsMet` do painel (`:427`) não considera o gate de pontos por tier usado pela compra.

### A04 — P1: variantes de capstone sem consequências distintas

`DefaultSkillCatalog.cs:115` define Kanthor/Kaand e Anya/Senya. A escolha é validada e persistida,
mas `SkillEffectAggregator.cs:51` não lê a variante. Melee não tem payload; Magic aplica os mesmos
AttackFlat/ManaRegen independentemente da escolha. Isso não cumpre a promessa de especialização.

### A05 — P1: sucesso vazio e ativa sem executor

Seis registros em `ActiveSkillExecutorCatalog.cs` usam `FeedbackOnlySkillEffectExecutor`:
`combat.ranged.marked_prey`, `combat.magic.elemental_ward`, `survival.sinal_retirada`,
`survival.isca_improvisada`, `crafting.field_patch`, `crafting.marca_eficiencia`.
Retornam sucesso e o controller inicia cooldown. `crafting_quick_repair` está no catálogo, mas sua
action não está no `SkillActionEffectCatalog`. A flag `NotYetExecutable` é metadado insuficiente:
há flags antigas em ações com executor e ausência de aplicação efetiva da flag na cadeia de uso.

### A06 — P1: status configurado com probabilidade zero

Os registros `combat.ranged.bleeding_arrow`, `combat.magic.ice_bind`, `combat.magic.toxic_cloud` e
`magic.rajada_gelida` passam ID de status sem sobrescrever a chance padrão zero do executor de projéteis.
Não basta apontar a database de status: a probabilidade efetiva impede aplicar bleed/chill/poison nessa rota.

Âncoras: `ProjectileSkillEffectExecutor.cs:48`, `ActiveSkillExecutorCatalog.cs:35,38,39,42` e
`ProjectileSpawnService.cs:61`, que só inicializa o status quando chance > 0.

### A07 — P1: descrição não corresponde à mecânica

| Ação | O que a rota atual faz | Promessa não entregue |
|---|---|---|
| Disparo Carregado | Projétil imediato | Segurar, carregar e soltar |
| Nuvem Tóxica | Leque de três projéteis | Área persistente com ticks |
| Corrente Relâmpago | Projétil com múltiplos hits/perfuração | Saltos entre alvos próximos |
| Instinto de Sobrevivência | Restaura 50 stamina | Revelação de recursos/perigos |
| Campo Seguro | Restaura HP/stamina/mana imediatamente | Zona contextual com duração |
| Kit de Emergência | Cura 30 HP | Consumo de kit, contexto fora de combate e função anunciada |
| Bomba Improvisada | Projétil Toxic perfurante com custo de recurso | Bomba craftada com carga, explosão e controle |

Fonte: registros de `ActiveSkillExecutorCatalog.cs` confrontados com `DefaultSkillCatalog.cs`.
Valores acima descrevem código atual; não são aprovação de balanceamento.

Qualificação posterior após leitura do addendum canônico completo: ele permite restore + reveal para
Instinto e Kit sem item, e descreve Nuvem como leque apesar da direção de área por ticks. As divergências
da tabela são contra descrições do catálogo, não prova de que remover restauração/cobrar item/criar área
sejam as únicas soluções canônicas. Ver [revisão de design v2](../refinements/a_implementar/ref_vaalara_skills_design_and_balance_v2.md).

### A08 — P1: respec punitivo sem wiring completo

`SkillTierEquipGate.RegisterRequirement` não tem chamada runtime encontrada. `EquipmentManager.cs:161`
consulta `itemInstanceId`, enquanto o gate documenta `itemId`. Não há revalidação do equipamento já vestido
na rota de respec. A regra aceita de bloquear uso após tier relock não está demonstrada pela existência do gate.

### A09 — P2: cooldown e rank não protegem o investimento

Controller armazena cooldown por índice de slot; atribuição aceita a mesma action em mais de um slot.
Mover/duplicar a ação permite acessar outro cooldown. A regra aceita também diz cooldown por slot:
uma mudança para cooldown por action exige reconciliação explícita, não fix silencioso.
Executores de ativas não recebem rank nem aplicam curva por rank; aumentar rank não demonstra ganho.
Rank cap de capstones chega ao teto dinâmico 5; ADR/regras mencionam três ranks de capstone.

O controller retorna antes de decrementar cooldown quando há modal (`Update:159`). O refinamento deve
decidir a política de tempo explicitamente. Não atribuir uma decisão de pausa ao simples uso de deltaTime.

### A11 — P2: movimento e aplicação de efeito precisam de invariantes físicas

`MeleeStrikeSkillEffectExecutor.cs:74–87` agenda um MovePosition e consulta hits imediatamente, enquanto
`PlayerController.FixedUpdate:170` também movimenta o corpo. O executor não ativa a trava de deslocamento.
O caminho de dash já usa `PlayerMovementDisplacementResolver.TryDisplace`; deve ser reutilizado.
O loop de hits (`:93–126`) não deduplica EnemyHealth: um alvo com vários colliders pode receber dano repetido.
`SlowFieldSkillEffectExecutor.cs:77` pode contar alvo afetado mesmo sem encontrar status; o custo já foi pago.
São necessários testes de parede, multi-collider e dependência ausente, além do caso feliz.

## 4.1. Matriz completa das 31 ativas

IDs abaixo são action IDs com prefixo comum `skill_`. “Efeito existente” não significa wiring ou arte aceitos.
Todas as habilidades espaciais continuam sujeitas ao problema A01.

| Sufixo da action | Comportamento atual | Tratamento necessário |
|---|---|---|
| melee_offhand_cut | Dano em arco | Contexto de arma/mão, origem e feedback |
| melee_whirl_cut | Dano 360 graus | Deduplicação por alvo e animação |
| melee_leap_attack | Dano + MovePosition | Deslocamento físico e leitura de salto |
| melee_battle_dash | Dano + MovePosition | Integração de deslocamento |
| melee_avanco_aco | Dano + MovePosition | Integração e diferenciação de battle_dash |
| melee_grito_desafio | Dano radial/knockback | Decidir e cumprir contrato de provocação |
| melee_investida_quebra_guarda | Dano/avanço/postura x3 | Física, alvo e apresentação de postura |
| ranged_charged_shot | Tiro instantâneo | Charge real; flag dormant desatualizada |
| ranged_line_piercer | Projétil até cinco hits | Origem, alvo único por passagem e arte de flecha |
| ranged_multishot_fan | Leque de três projéteis | Direção, custo e diferença visual |
| ranged_bleeding_arrow | Dano sem bleed efetivo | Probabilidade de status |
| ranged_marked_prey | Feedback-only | Marca real e modificador condicionado |
| magic_fire_spark | Projétil Fire | Rank/custo/arte e origem |
| magic_ice_bind | Dano sem chill efetivo | Probabilidade e leitura do status |
| magic_toxic_cloud | Leque sem poison efetivo | Área por ticks e status |
| magic_lightning_chain | Projétil perfurante | Encadeamento real |
| magic_elemental_ward | Feedback-only | Buff de resistência com duração |
| magic_slowing_sigils | Slow instantâneo em raio | Validar status antes de custo e sucesso |
| magic_chama_breve | Projétil Fire | Diferenciação e promessa de Burn |
| magic_rajada_gelida | Leque sem chill efetivo | Probabilidade e feedback |
| survival_last_breath | +40 HP | Regra de emergência, rank e feedback |
| survival_sinal_retirada | Feedback-only | Buff contextual de retirada |
| survival_isca_improvisada | Feedback-only | Distração limitada e visível |
| survival_kit_emergencia | +30 HP | Contrato de kit/consumo/contexto |
| survival_instinto_sobrevivencia | +50 stamina | Revelação de recursos/perigos |
| survival_campo_seguro | Restauração instantânea de três vitais | Campo persistente contextual |
| crafting_field_patch | Feedback-only | Reparo efetivo com custo |
| crafting_quick_repair | Sem mapping | Mapeamento e distinção do reparo anterior |
| crafting_irrigador_portatil | Rega um canteiro; stamina 10 | Alvo/binding e contrato de área; flag stale |
| crafting_bomba_improvisada | Projétil Toxic perfurante; custa mana | Carga craftada e explosão |
| crafting_marca_eficiencia | Feedback-only | Buff em consumidores agrícolas/crafting |

Os 24 executores que alteram estado incluem substitutos sem identidade correta; seis são feedback-only e
uma action está sem mapping. Testar apenas 30 mappings existentes deixa justamente a action ausente fora
do conjunto. A fonte de cobertura precisa ser o catálogo de 31 ativas expostas.

### A10 — P1/P2: UI visual não entregue pelo wiring atual

`DomainRuntimeInstallers.cs:200` instala IMGUI. `GameplayHudBootstrap` cria o objeto sem Canvas;
`GameplayHudCanvasController.cs:69` usa AddComponent para uma view com arrays serializados não preenchidos.
`ActiveSkillSlotsHudView` possui botões/cooldown/overlay, mas não binding de ícone de habilidade.
`FeedbackToastHudView.cs:18` imprime log; a apresentação depende de DebugHud.
`DebugHud.cs:475` rotula R/T/Y/G, enquanto a execução usa Alpha1–4. A projeção de prontidão considera
vazio/cooldown, sem representar integralmente custos e contexto. Gamepad não foi demonstrado nessa cadeia.

## 5. Matriz dos 35 passivos/capstones

| Nós | Efeito rastreado | Lacuna |
|---|---|---|
| melee_iron_grip; melee_guarded_stance | Attack/Defense em DerivedStatsCalculator | Iron Grip não restringe tipo de arma |
| melee_dual_wield_flow; melee_two_handed_momentum | AttackSpeed/Attack | Bônus incondicionais, sem dual wield/two-handed |
| melee_dodge_training | Campo DodgeCostReduction calculado | Sem leitor gameplay; cooldown prometido ausente |
| melee_capstone_battle_rhythm | Escolha persistida | Sem payload/route; variantes sem efeitos |
| ranged_steady_hand; ranged_long_sight | BowDamage/BowRange em BowArrowAttackService | Caminho existe; aceitação humana pendente |
| ranged_quick_nock | AttackSpeed | Genérico, não exclusivo de arco |
| ranged_kiting_steps | MoveSpeed | Permanente em vez de após disparo |
| ranged_projectile_tuning | BowProjectileSpeed calculado | Sem leitor gameplay |
| ranged_capstone_eagle_focus | BowRange | ProjectileSpeed sem leitor; bônus de skills sem payload |
| magic_mana_well; magic_quick_channel | MaxMana/ManaRegen via PlayerVitalsApplier | Caminho existe |
| magic_arcane_edge; magic_arcane_bolt_mastery | AttackFlat | Genérico, não efeito específico de ArcaneBolt |
| magic_capstone_elemental_confluence | AttackFlat/ManaRegen | Variantes Anya/Senya ignoradas |
| survival_cave_lungs; survival_hard_skin; survival_low_rations | Stamina/HP/HungerDrain | Caminhos via PlayerVitalsApplier |
| survival_toxic_sense; survival_cold_habit; survival_heat_temper | Resistências | Encaminhadas a ResistanceProvider.Source |
| survival_status_recovery | StatusDurationReduction calculado | Sem leitor desse campo |
| survival_safe_step | MoveSpeed | Permanente em vez de redução contextual do terreno |
| survival_capstone_caveborn | Resistências/MaxStamina | Caminhos existentes |
| crafting_fast_hands; crafting_repair_care; crafting_capstone_master_artisan | CraftTime/RepairEfficiency | Consumidos em CraftingRuntime/EquipmentManager |
| crafting_station_focus | CraftTime | CraftCost hook não tem consumidor |
| crafting_material_eye; crafting_salvage_method; crafting_shop_sense | HarvestYield/ToolEfficiency/GoldDrop hooks | Sem consumidores; nomes também divergem das rotas |
| crafting_pack_order; crafting_durable_finish | Nenhum payload/route | Investimentos vazios |

Evidência: `DefaultSkillCatalog.cs:260–665`; `SkillEffectAggregator.cs:51–80`;
`Player/DerivedStatsCalculator.cs:82–103`; `Player/PlayerVitalsApplier.cs:88–111`;
`Combat/BowArrowAttackService.cs:115–123`; `Craft/CraftingRuntime.cs:128`;
`Equipment/EquipmentManager.cs:338`. Busca de `SkillModifierHooks` em Scripts não encontra consumidor
fora de Skills. Ausência de consumidor não deve ser confundida com ausência de getter ou teste do agregado.

Crafting concentra cinco passivos inteiramente inertes entre nove. Pré-requisitos agravam o problema:
capstone de Crafting passa por durable_finish/salvage_method; Ranged por projectile_tuning;
last_breath por status_recovery. Não se resolve apenas escondendo nós sem rever alcançabilidade.

## 6. Apresentação e arte

Existem visuais mínimos: ProjectileSkillEffectExecutor solicita SkillBolt e RuntimeProjectileFactory gera
círculos/trails/pulse. Isso refuta “zero visual”, mas não comprova identidade artística por habilidade.
Não foi encontrado catálogo de ícones ligado à view; adicionar PNGs sem conectar catálogo/Canvas não resolve.

Referência realmente aberta: `docs/art_catalog/_reference_keyart_GPT/farm_keyart_layout_aprovado_v1.png`.
Observação: madeira e pedra, terras/verde, ferragens discretas, luminosidade ciano localizada na Fonte.
Guia consultado: `docs/design/art/ART_DIRECTION_ILLUSTRATOR_GUIDE.md:9–36`.
Essa referência orienta materiais e linguagem; **não é uma UI aprovada**. Dimensões/paleta de UI propostas
no refinamento são novas decisões, ainda sujeitas a revisão visual em escala de jogo.

## 7. Reconciliar documentação sem reabrir implementações encerradas

O relatório F75 aceita o “melhor executor disponível” e admite dormentes e Play Mode pendente.
Isso não satisfaz a promessa semântica original. Sua tabela de dormantes também difere do código atual.
`dotnet build` listado como comando de testes não demonstra execução dos testes EditMode.
O refinamento cria uma fila residual baseada no estado vivo; não manda reexecutar F29/F70/F75 integralmente.

Preservar ADR-0010, cinco árvores, quatro slots, pontos por nível + atos e respec da Fonte.
Cooldown por action, teto de capstone e política de nós indisponíveis precisam de decisão explícita.
Não foi identificada contradição com CURRENT_STATE: as lacunas detalham sua aceitação humana pendente.

## 8. Validação desta entrega

Revisão estática e documental: realizada. Compilação, testes Unity e Play Mode: **NOT RUN**.

Checagem local de cobertura: extração das chamadas `Node(...)` e `unlockAction` em DefaultSkillCatalog
retornou 66 nós, 31 ativas e 35 passivos/capstones; todos os 31 sufixos de action constam da matriz.
Inventário de `Assets/_Game/Data/Skills/Nodes/*.asset`: 68 arquivos e 68 ícones nulos.
Os dois documentos foram verificados quanto a referências Markdown locais, cercas de código balanceadas
e caracteres de substituição. São verificações documentais, não testes do comportamento do jogo.

```text
Unity validation: NOT RUN
Reason: entrega somente de auditoria/refinamento; não houve alteração de runtime/assets
Command attempted: nenhum comando Unity; não necessário para validar a edição documental
Residual risk: Unity compile not validated locally
```

Limite adicional: os bugs precisam de reprodução dinâmica para medir alcance e impacto no jogo;
os caminhos estáticos já justificam specs corretivas. A arte final e sua aceitação não foram executadas.
