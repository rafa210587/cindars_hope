# Cindar's Hope — Companions Direction

> **Status:** documento canônico de direção do sistema de companions  
> **Local:** `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`  
> - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`  
> - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`  
> - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`  
> - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`  
> - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`  
> - `docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md`  
> - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`  
> - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`  
> **Função:** definir como companions funcionam na fazenda, cidade, caverna, combate, vínculo, jobs, romance/casamento, economia, HUD, balance e save/load.  
> **Não é spec implementável.** Specs futuras devem converter isto em dados, runtime, UI e validações de Unity.

---

## 0. Regra anti-duplicação

Este documento **não** redefine:

```text
roster completo de NPCs;
quests pessoais de cada NPC;
romance/casamento detalhado;
IA de inimigos;
fórmulas finais de dano/HP/MP/Stamina;
stats concretos de monstros;
loot tables;
layout da cidade/fazenda;
pets.
```

Fontes canônicas:

```text
CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
  NPCs concretos, classe funcional, romance, religião, serviços, visitas e stats autorados.

FARM_DESIGN_DIRECTION_v1.3.md
  papel da fazenda, companions ajudando, jobs de fazenda, rotina, pets e visitas.

COMBAT_CORE_DIRECTION.md
  regras de combate, inputs, Stamina, dano, Block, Dash, Dodge, HUD e feeling.

ENEMY_BEHAVIORS_DIRECTION.md
  reação de inimigos a player, companions e pets; target priority; leash; pack coordination.

CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
  active combat budget, TTK, companions/pets e balance de caverna.

LOOT_CRAFTING_ECONOMY_DIRECTION.md
  recompensas, encomendas, storage, economia, loot e crafting.

MAGIC_SPELLS_ACTIONS_DIRECTION.md
  spells, cura, buffs, barreiras e suporte mágico.
```

Regra:

```text
Este documento define o sistema de companions.
NPC concreto continua no roster da cidade.
Pet continua sistema separado.
Romance/casamento detalhado será documento separado.
```

---

# PARTE A — Identidade do sistema

## 1. O que é companion

Companion é um NPC com vínculo funcional suficiente para acompanhar ou ajudar o jogador em atividades específicas.

Um companion pode:

```text
acompanhar o jogador na caverna;
ajudar em jobs da fazenda;
visitar a fazenda;
ajudar em quests;
oferecer suporte social, econômico ou narrativo;
ter passivas de vínculo;
ter diálogo e reações a eventos;
participar de combate com limites claros.
```

Regra principal:

```text
Companion ajuda, mas não joga pelo jogador.
```

## 2. O que companion não é

Companion não deve:

```text
tankar boss infinitamente;
curar sem limite;
substituir pet;
substituir build do jogador;
substituir skill tree;
minerar/farmar tudo sozinho no early game;
ignorar active combat budget da caverna;
gerar loot extra infinito;
resolver puzzle/quest sem decisão do jogador;
ser obrigatório para terminar o jogo;
ser obrigatório para romance/casamento;
ser romance automático.
```

## 3. Diferenças entre sistemas

| Conceito | Definição |
|---|---|
| NPC comum | personagem da cidade com rotina, serviço, diálogo ou quest |
| Companion | NPC que pode ajudar em atividades e/ou acompanhar o jogador |
| Romance candidate | NPC elegível a romance/casamento, não necessariamente companion |
| Spouse | NPC casado com o jogador, pode ter rotinas e bônus próprios |
| Pet | animal/mascote do jogador; sistema separado, menos social e mais instintivo |
| Worker/Helper | função temporária de job; pode ser companion ou NPC contratado futuro |

Regra:

```text
Nem todo NPC é companion.
Nem todo companion é romance.
Nem todo romance vira companion de combate.
Nem todo spouse deve ser combat companion.
```

---

# PARTE B — Escopo por fase

## 4. Escopo atual recomendado

Para specs iniciais, companions devem cobrir:

```text
1 companion ativo por vez na caverna;
1 pet ativo opcional em sistemas futuros, separado do companion;
jobs simples de fazenda por companion;
visitas à fazenda;
vínculo/reputação básica;
assistência leve em combate;
ferimento/recuo ao cair;
HUD simples;
save/load do estado.
```

## 5. Escopo futuro explícito

Futuro, não implementar sem roadmap:

```text
party com múltiplos companions;
romance/casamento com rotinas profundas;
companion equipment completo;
companion skill tree própria grande;
farm defense/invasões;
companion permadeath;
comandos táticos complexos;
AI squad avançada;
coop multiplayer;
companions em cidade durante eventos hostis.
```

Regra:

```text
Specs atuais não devem implementar sistemas futuros apenas porque aparecem neste documento.
```

---

# PARTE C — Elegibilidade e recrutamento

## 6. Companion eligibility

O roster da cidade deve poder declarar flags:

```text
CanBeFarmCompanion
CanBeCaveCompanion
CanBeQuestCompanion
CanBeSocialCompanion
CanBeRomanceCompanion
CanBeSpouseCompanion
CompanionLockedByStory
CompanionLockedByReputation
CompanionLockedByQuest
CompanionUnavailable
```

Regra:

```text
Companion eligibility deve ser dado do NPC, não hardcode por nome.
```

## 7. Recrutamento

Um companion pode ser desbloqueado por:

```text
amizade/reputação;
quest pessoal;
serviço contratado;
evento de história;
romance/casamento;
resgate na caverna;
favor para a cidade;
progressão de guilda/templo/fazenda;
```

Baseline:

```text
FarmCompanion comum: relação média + quest simples.
CaveCompanion: relação média/alta + quest ou prova de confiança.
QuestCompanion: temporário por quest.
SpouseCompanion: depende de casamento e personalidade do NPC.
```

Regra:

```text
O jogador não deve recrutar companion de combate forte só comprando serviço cedo.
```

## 8. Companion temporário vs permanente

```text
TemporaryCompanion
  acompanha em quest/evento específico.
  sai ao concluir objetivo.

UnlockedCompanion
  fica disponível para convites/jobs após desbloqueio.

ScheduledCompanion
  disponível apenas em certos dias/horários por rotina.

StoryCompanion
  acompanha por motivo narrativo, com limites próprios.
```

Regra:

```text
TemporaryCompanion não precisa desbloquear todos os sistemas de companion.
```

---

# PARTE D — Papéis funcionais

## 9. Companion roles

Papéis de companion devem derivar das classes funcionais do jogo, não classes de D&D.

Roles possíveis:

```text
FarmWorker
Forager
Miner
Fighter
Guardian
Healer
Alchemist
Researcher
Scout
Merchant
Builder
AnimalCaretaker
Crafter
MusicianSupport
```

## 10. Papéis na fazenda

```text
Plantador
  ajuda a plantar em área/plano permitido.

Colhedor
  coleta crops maduras em área permitida.

Lenhador
  coleta madeira/corta árvores marcadas.

Minerador
  coleta pedras/minérios em área permitida, especialmente pedreira late.

Tratador
  cuida de animais/pets, alimentação e coleta produtos.

Artesão
  opera workshop/processadores simples.

Alquimista
  processa poções/fertilizantes autorizados.

Construtor
  ajuda em construção/movimento de estruturas se permitido.
```

Regras:

```text
Companion de fazenda deve reduzir repetição, não remover planejamento.
Jobs exigem área marcada, ferramenta/estação e limites de tempo/stamina.
No early game, companion faz pouco e ensina o sistema.
No mid/late, companion melhora automação controlada.
```

## 11. Papéis na caverna

```text
Fighter
  dano leve/moderado e pressão.

Guardian
  proteção, interceptação limitada, anti-stagger leve.

Healer
  cura rara, cooldown alto, suporte.

Alchemist
  status/poções/buffs curtos.

Scout
  alerta de perigo, treasure hint limitado, posicionamento.

Researcher
  lore, bestiary hints, leitura de ruínas.

MusicianSupport
  buff curto, moral, redução de Fear/ConfusionLite leve.
```

Regras:

```text
Companion de caverna deve ter função clara.
Não deve replicar todas as funções de uma party completa.
```

---

# PARTE E — Limite de party e presença

## 12. Limite ativo

Baseline recomendado:

```text
Caverna: 1 companion ativo.
Fazenda: múltiplos companions podem visitar, mas apenas jobs limitados por board/capacidade.
Cidade: companions seguem agenda própria, não seguem o jogador o tempo todo.
Quest: pode permitir companion temporário extra se a quest exigir, mas não como regra geral.
```

Regra:

```text
Balance da caverna deve assumir no máximo 1 companion + sistemas de pet quando implementados.
```

## 13. Pet separado

Quando pets existirem:

```text
Pet não conta como companion.
Pet tem funções menores/instintivas.
Pet não substitui companion.
Companion não substitui pet.
```

Regra:

```text
Balance deve considerar player + 1 companion + 1 pet como teto comum futuro, não party grande.
```

---

# PARTE F — Stats e recursos

## 14. Stats usados

Companions usam os mesmos atributos básicos do jogo:

```text
HP
MP
Stamina
Força
Constituição
Destreza
Inteligência
Vontade
Carisma
```

Regra:

```text
Breath/Fôlego não existe como atributo/recurso de companion.
Se algum documento antigo citar Breath em NPC, deve ser refatorado para Stamina, Cansaço ou removido.
```

## 15. Recursos

```text
HP
  vida em combate/exploração.

MP
  usado apenas por companions com magia/suporte.

Stamina
  ações físicas, combate, jobs e esforço.

Cansaço/Fatigue
  desgaste acumulado por trabalho/expedição, se a spec futura permitir.
```

Regras:

```text
Companion não deve ter Stamina infinita.
Companion não deve regenerar HP/MP/Stamina de forma melhor que o jogador sem motivo.
Companion healer não deve curar sem custo de MP/cooldown.
```

## 16. Scaling de companion

Companion pode escalar por:

```text
nível narrativo/progresso;
vínculo;
quest pessoal;
equipment leve;
job rank;
farm/cave progress;
story flags;
```

Regra:

```text
Companion não deve escalar automaticamente para superar o jogador.
Companion deve ficar útil, não dominante.
```

---

# PARTE G — Companions na fazenda

## 17. Farm jobs

Sistema de jobs deve ser explícito:

```text
JobId
CompanionId
JobType
AllowedArea
AllowedToolOrStation
StartTime
EndTime
StaminaBudget
OutputRules
FailureRules
RelationshipGain
FatigueGain
```

## 18. Job board

A fazenda pode ter um quadro de jobs.

Função:

```text
atribuir tarefa;
limitar automação;
mostrar tempo/custo;
mostrar resultado esperado;
evitar que NPC faça tudo sem comando;
permitir planejamento diário.
```

Regras:

```text
Job board não deve estar completo no início.
Novos tipos de job abrem com relação, construção, tool upgrades ou progresso.
```

## 19. Limites de automação

Companion de fazenda deve respeitar:

```text
área marcada;
horário;
ferramenta disponível;
stamina diária;
qualidade de vínculo/job;
acesso ao storage autorizado;
estações construídas;
clima/season quando aplicável;
```

Regra:

```text
Companion não deve criar item do nada nem coletar recurso fora do estado real do mundo.
```

## 20. Qualidade de trabalho

Qualidade do job pode depender de:

```text
classe funcional do companion;
vínculo;
ferramenta/estação;
skill/passiva do companion;
condição do dia;
complexidade da tarefa;
```

Exemplo:

```text
Tratador com alto vínculo melhora chance de produto animal de qualidade.
Plantador reduz chance de erro de plantio e consome menos tempo.
Artesão acelera processamento, mas precisa estação e input.
```

---

# PARTE H — Companions na cidade

## 21. Agenda própria

Companion continua sendo NPC da cidade.

Regras:

```text
Companion tem casa/cama/rotina.
Companion pode recusar convite em certos horários/dias.
Companion pode ter trabalho/loja/serviço.
Companion pode visitar a fazenda se relação/reputação permitir.
Companion não deve ficar teleportando sem justificativa visual/sistêmica.
```

## 22. Convite

O jogador pode convidar companion por:

```text
diálogo;
quadro de jobs;
quest;
evento;
agenda combinada;
```

Condições de aceite:

```text
relação mínima;
disponibilidade no horário;
não estar em quest/evento conflitante;
não estar ferido/exausto;
não haver bloqueio narrativo;
```

---

# PARTE I — Companions na caverna

## 23. Entrada na caverna

Antes de entrar:

```text
selecionar companion ativo;
mostrar papel do companion;
mostrar estado HP/MP/Stamina;
mostrar risco;
mostrar restrições;
mostrar se companion está ferido/indisponível;
```

Regra:

```text
Caverna deve permitir entrar sem companion.
Companion é vantagem/opção, não requisito universal.
```

## 24. Comportamento de exploração

Companion deve:

```text
seguir o jogador;
evitar ficar preso;
teleportar/warp curto apenas como fallback técnico;
manter distância por papel;
reagir a combate;
respeitar leash do jogador;
não abrir baú/porta sozinho por padrão;
não acionar trap voluntariamente;
comentar lore/ruína se papel permitir;
```

## 25. Interação com procedural cave

Companion deve funcionar com:

```text
snapshot/replay da run;
level procedural;
checkpoints;
boss gates;
safe spawn;
confinement walls;
leash;
fog/reveal se existir;
```

Regra:

```text
Companion state precisa ser persistido dentro da run se a caverna usa snapshot estável.
```

---

# PARTE J — IA de companion

## 26. CompanionBrain

Campos conceituais:

```text
CompanionBrainProfileId
PrimaryRole
SecondaryRole
FollowDistance
CombatDistance
RetreatThreshold
AssistPriority
TargetPriority
AllowedActions
ForbiddenActions
CooldownRules
ResourceRules
HazardAvoidanceRules
LeashRules
ReviveOrRetreatRules
```

## 27. Estados de IA

```text
Idle
FollowPlayer
MoveToJob
PerformJob
ReturnHome
VisitFarm
ExploreFollow
CombatAssist
DefensiveAssist
HealingAssist
Retreat
Downed
InjuredUnavailable
QuestScripted
```

## 28. Prioridades em combate

Ordem sugerida:

```text
1. sobreviver / sair de hazard;
2. respeitar leash do jogador;
3. proteger-se se HP baixo;
4. usar cura/suporte se papel permitir e cooldown disponível;
5. atacar alvo seguro/prioritário;
6. ajudar em inimigo que ameaça jogador;
7. evitar puxar novo pack;
8. não perseguir fora da área segura.
```

Regra:

```text
Companion não deve aggrar salas novas sozinho.
```

---

# PARTE K — Combate e balance

## 29. Dano de companion

Companion pode causar dano, mas:

```text
DPS menor que o jogador focado em dano;
dano limitado por cooldown/recovery/Stamina/MP;
não deve matar boss sozinho;
não deve farmar enemies sem player ativo;
```

Baseline de direção:

```text
Companion DPS comum: 15%-35% do DPS esperado do jogador no mesmo estágio.
Companion ofensivo especializado: 35%-50%, com fragilidade/cooldown/risco.
Companion healer/support: baixo DPS.
```

## 30. Cura e suporte

Companion healer pode:

```text
curar pouco/moderado;
remover status leve;
aplicar barreira curta;
reduzir Fear/ConfusionLite leve;
```

Mas deve ter:

```text
MP cost;
cooldown alto;
range curto;
cast time;
interrupção;
limite por combate/run;
```

Regra:

```text
Companion healer não pode criar imortalidade.
```

## 31. Tank/proteção

Companion Guardian pode:

```text
interceptar hit ocasional;
aplicar taunt curto apenas se sistema permitir;
reduzir dano em janela curta;
bloquear passagem momentaneamente;
```

Mas não deve:

```text
manter aggro permanente de boss;
travar inimigo sem custo;
segurar pack inteiro sozinho;
ignorar guard break/boss mechanics;
```

## 32. Active combat budget

Companion conta no balance da caverna.

Regras:

```text
Active combat budget deve considerar contribuição de companion.
Spawn density não deve aumentar automaticamente só porque há companion no MVP.
Bosses podem ter pequenos ajustes de target logic, não HP inflado arbitrário.
Companion deve ajudar em margem de erro, não dobrar poder do jogador.
```

## 33. Bosses

Contra bosses:

```text
companion recebe redução de eficácia em hard CC;
companion não pode interromper fase crítica sem janela específica;
healing companion tem cooldown maior/limite;
tank companion não segura boss permanentemente;
companion pode abrir micro-janela se design do boss permitir;
```

Regra:

```text
Boss deve continuar exigindo execução do jogador.
```

---

# PARTE L — Targeting inimigo e reações

## 34. Inimigos podem mirar companions

Enemy Behaviors deve permitir target priority para companions.

Direção:

```text
inimigos podem atacar companion se companion causar dano, curar ou bloquear;
inimigos não devem ignorar player sempre;
healer companion pode gerar threat;
Guardian companion pode gerar threat controlada;
PackFlanker pode pressionar companion frágil;
Boss pode alternar alvo por fase/mecânica;
```

## 35. Anti-cheese

Regras:

```text
Companion não deve ser isca infinita.
Inimigos podem reacquirir player.
Companion downed não deve continuar segurando aggro.
Leash impede kiting abusivo com companion.
Boss arena impede deixar companion lutar sozinho fora de risco.
```

---

# PARTE M — Ferimento, derrota e recuperação

## 36. Downed

Quando HP chega a 0:

```text
Companion entra em Downed ou Retreat.
Não morre permanentemente por padrão.
Não continua lutando.
Pode ser resgatado ou recuar sozinho dependendo do contexto.
```

## 37. InjuredUnavailable

Após derrota séria:

```text
companion fica indisponível por 1+ dia ou até recuperação;
pode precisar de cura, descanso, item ou serviço;
relação pode cair se jogador abandonou/repetiu risco, mas não de forma punitiva demais;
```

Regra:

```text
Sem permadeath de companion no sistema base.
Permadeath, se existir, é evento narrativo único/futuro.
```

## 38. Recuperação

Recuperação pode ocorrer por:

```text
descanso diário;
curandeiro da cidade;
itens de cura;
Fonte de Anya em condições específicas;
quest/evento;
```

Regra:

```text
Fonte de Anya não deve virar reset grátis infinito para companion abuse.
```

---

# PARTE N — Progressão de vínculo

## 39. Bond / vínculo

Companion usa vínculo próprio conectado a relacionamento, mas não idêntico a romance.

Campos:

```text
CompanionBondLevel
CompanionTrust
CompanionFatigue
CompanionInjuryState
CompanionUnlockedRoles
CompanionJobRank
CompanionCaveRank
```

## 40. Ganho de vínculo

Vínculo aumenta por:

```text
quests pessoais;
trabalho bem-sucedido;
exploração de caverna;
presentes adequados;
diálogo;
eventos;
proteção/retorno seguro;
fazer escolhas alinhadas aos valores do NPC;
```

Perde ou trava por:

```text
ferimentos repetidos;
abandonar companion downed;
escolhas contra valores fortes;
usar altares/deuses que o NPC rejeita, conforme sistema social;
falhar quest pessoal relevante;
```

Regra:

```text
Penalidade deve ser narrativa e recuperável, salvo escolhas graves.
```

## 41. Benefícios de vínculo

Benefícios possíveis:

```text
maior disponibilidade;
melhor job output;
menor fatigue em job;
pequena passiva de combate;
maior chance de proteger/ajudar;
novas conversas/quests;
visitas à fazenda;
romance/casamento se elegível;
serviços melhores/descontos se NPC comerciante;
```

Regra:

```text
Benefício de vínculo não deve virar pay-to-win social obrigatório.
```

---

# PARTE O — Skills/passivas de companion

## 42. Modelo simples

Companion não deve ter árvore de skill grande no início.

Preferir:

```text
1 trait principal;
1 passiva de vínculo;
1 ação de combate;
1 job specialty;
1 fraqueza/limite;
```

## 43. Campos de CompanionAbilitySO

```text
AbilityId
CompanionId optional
RoleRequired
AbilityType
Trigger
Cooldown
ResourceCost
Range
TargetRules
EffectTags
ScalingRules
BondRequirement
CaveRankRequirement
FarmJobRequirement
BossEffectModifier
DebugTags
```

## 44. Tipos de habilidade

```text
PassiveBond
FarmJobBonus
CombatAssist
HealingAssist
DefensiveAssist
ExplorationHint
LootHint
StatusCleanse
BuffShort
QuestOnly
```

Regras:

```text
Habilidade de companion deve ser pequena e legível.
Não criar builds paralelas complexas antes do player core estar sólido.
```

---

# PARTE P — Equipamentos de companion

> **ALTERADO PELA EMENDA 2026-06-13-V3 (decisão 3.4, OVERRIDE).** O texto original abaixo dizia que equipment de companion era "futuro/não-MVP". A decisão 3.4 (vinculante) move equipment (arma + acessório) para o **sistema base**. As §45 e §46 abaixo ficam marcadas como SUPERSEDED; a regra canônica vigente está na seção "EMENDA 2026-06-13-V3 → 3.4 Equipment no sistema base" no fim deste documento.

## 45. Direção inicial ~~(SUPERSEDED pela EMENDA 2026-06-13-V3 / decisão 3.4)~~

> **SUPERSEDED 2026-06-13-V3 (3.4):** a afirmação "companion não precisa de equipment completo / equipment é futuro" deixa de valer. Companion equipa **arma + acessório** no sistema base. Mantido aqui apenas como registro histórico; ver emenda no fim do documento.

No MVP/direção inicial (texto histórico):

```text
Companion não precisa de equipment completo.
Companion pode ter equipamento autorado/narrativo.
Stats podem escalar por progressão/vínculo, não por inventário completo.
```

## 46. Futuro ~~(SUPERSEDED pela EMENDA 2026-06-13-V3 / decisão 3.4)~~

> **SUPERSEDED 2026-06-13-V3 (3.4):** equipment de companion deixa de ser "futuro". O conjunto de slots base (arma + acessório) e as regras de recusa por personalidade/classe migram para a emenda no fim do documento. Item único de companion por quest pessoal permanece válido como recompensa.

Se equipment de companion existir (texto histórico):

```text
slots limitados;
sem microgerenciamento excessivo;
equipment não deve transformar companion em segundo player;
alguns NPCs podem recusar tipos de gear por personalidade/classe;
item único de companion pode vir de quest pessoal;
```

Regra (texto histórico — SUPERSEDED):

```text
Specs atuais não devem criar inventário completo de companion sem decisão explícita.
```

> **Regra vigente (EMENDA 2026-06-13-V3 / 3.4):** o sistema base implementa **arma + acessório** por companion (2 slots), sem inventário completo. Ver emenda no fim do documento para o contrato exato.

---

# PARTE Q — Loot e recompensas

## 47. Loot em caverna

Regra base:

```text
Companion não gera rolagem de loot separada por padrão.
```

Pode ajudar em:

```text
identificar drop;
aumentar pequena chance de componente específico se role permitir;
reduzir perda de item em retreat;
abrir hint de treasure;
carregar pequena quantidade futura se sistema permitir;
```

Mas não deve:

```text
dobrar drop;
criar ouro extra infinito;
farmar sem player;
invalidar LootTableSO;
```

## 48. Recompensas de companion quests

Podem dar:

```text
bond;
recipe;
serviço;
discount;
companion ability;
job specialty;
spell source se NPC apropriado;
unique item narrativo;
```

Regra:

```text
Quest de companion deve recompensar vínculo/sistema, não apenas ouro.
```

---

# PARTE R — Romance, casamento e spouse

## 49. Relação com romance

Romance é sistema separado.

Regras:

```text
Romance candidate pode ser companion, mas não é obrigatório.
Companion pode ser romance, mas não vira spouse automaticamente.
Casamento não deve ser requisito para companion forte.
Companion de combate não deve ser restrito só a romance.
```

## 50. Spouse companion

Se spouse puder ajudar:

```text
ajuda deve respeitar personalidade, rotina e função;
não deve ser sempre melhor que companion não-romântico;
não deve punir jogador que não casa;
pode oferecer bônus doméstico/social único;
```

Regra:

```text
Casamento deve ampliar vida social/fazenda, não ser meta obrigatória de poder.
```

---

# PARTE S — HUD e feedback

## 51. HUD mínimo

Mostrar:

```text
companion ativo;
HP/estado simplificado;
MP/Stamina apenas se relevante;
estado: following, job, combat, downed, injured;
cooldown de habilidade importante;
aviso de recuo/ferimento;
ícone de job na fazenda;
```

Regra:

```text
HUD de companion deve ser discreto.
Não competir com HP/MP/Stamina do player.
```

## 52. Feedback de job

Mostrar:

```text
job atribuído;
área alvo;
tempo estimado;
resultado parcial/final;
falta de ferramenta/recurso;
stamina insuficiente;
job bloqueado por relação/horário;
```

## 53. Feedback de combate

Mostrar:

```text
companion usando habilidade;
companion em perigo;
companion downed;
healing/barrier recebido;
companion recuando;
```

---

# PARTE T — Save/load

## 54. Estado persistido

Salvar:

```text
CompanionId
UnlockedCompanions
CompanionBondLevel
CompanionTrust
CompanionAvailability
CompanionInjuryState
CompanionFatigueState
CompanionJobAssignments
CompanionJobProgress
CompanionCaveState in run
ActiveCompanionId
CompanionQuestFlags
CompanionAbilityUnlocks
CompanionScheduleOverrides
CompanionFarmVisitState
CompanionRomanceLink optional
```

## 55. Estado recalculável

Não persistir como fonte primária:

```text
DPS final;
stat derivado final;
chance final de job output;
tooltip final;
AI score momentâneo;
```

Regra:

```text
Stats derivados devem recalcular no load a partir de NPC base, vínculo, progress flags e modifiers.
```

---

# PARTE U — Data assets esperados

## 56. Assets

```text
CompanionDataSO
CompanionEligibilitySO
CompanionBrainProfileSO
CompanionRoleProfileSO
CompanionAbilitySO
CompanionJobProfileSO
CompanionBondProfileSO
CompanionCaveProfileSO
CompanionFarmProfileSO
CompanionScheduleOverrideSO
CompanionRecoveryProfileSO
CompanionHUDProfileSO
CompanionBalanceProfileSO
```

## 57. CompanionDataSO mínimo

```text
CompanionId
NpcId
DisplayName
EligibilityFlags
PrimaryCompanionRole
SecondaryCompanionRole
BaseStatsReference
AllowedContexts
UnlockRules
AvailabilityRules
BondProfileId
BrainProfileId
FarmProfileId
CaveProfileId
AbilityIds
RecoveryProfileId
RelationshipLinks
RomanceCompatibilityRef optional
DebugTags
```

## 58. CompanionJobProfileSO mínimo

```text
JobProfileId
AllowedJobTypes
RequiredTools
RequiredStations
StaminaCostRules
TimeCostRules
OutputRules
QualityRules
FailureRules
BondGainRules
FatigueRules
StorageAccessRules
DebugTags
```

## 59. CompanionBalanceProfileSO mínimo

```text
ProfileId
MaxActiveCaveCompanions
MaxFarmJobCompanionsByFarmLevel
DpsContributionTargets
HealingCooldownRules
BossEffectMultipliers
DownedRules
InjuryDurationRules
FarmAutomationLimits
LootBonusLimits
DebugTags
```

---

# PARTE V — Integração com documentos futuros

## 60. Com Pets

Quando `PETS_DIRECTION.md` existir:

```text
Pet tem vínculo próprio;
Pet tem funções instintivas;
Pet não usa romance/social complexo;
Pet pode interagir com companion;
Pet não substitui companion.
```

## 61. Com Social/Romance

Quando `SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md` existir:

```text
romance/casamento devem referenciar companion, não redefinir companion;
spouse routines devem respeitar companion availability;
presentes e vínculo podem afetar companion unlocks;
```

## 62. Com Quests

Quando `QUESTS_MAIN_PROGRESSION_DIRECTION.md` existir:

```text
quest pode criar temporary companion;
quest pessoal pode desbloquear companion;
quest não deve exigir companion específico sem alternativa, salvo história explícita;
```

---

# PARTE W — Roadmap de specs futuras

```text
spec_companion_data_contract.md
spec_companion_unlock_availability_runtime.md
spec_companion_farm_jobs_runtime.md
spec_companion_cave_follow_and_leash_runtime.md
spec_companion_combat_assist_runtime.md
spec_companion_healing_guardian_support_limits.md
spec_companion_downed_injury_recovery_runtime.md
spec_companion_bond_progression_runtime.md
spec_companion_hud_feedback.md
spec_companion_save_load_state.md
spec_companion_balance_playtest_profile.md
```

---

# PARTE X — Decisões fechadas

```text
Companion ajuda, mas não joga pelo jogador.
Nem todo NPC é companion.
Nem todo companion é romance.
Nem todo romance vira companion de combate.
Nem todo spouse deve ser combat companion.
Baseline de caverna: 1 companion ativo.
Pet é sistema separado e não conta como companion.
Companion usa HP, MP, Stamina, Força, Constituição, Destreza, Inteligência, Vontade e Carisma.
Breath/Fôlego não existe como atributo/recurso de companion.
Companion não tem Stamina infinita.
Companion healer não cura sem MP/cooldown/cast/limite.
Companion tank não segura boss permanentemente.
Companion não gera rolagem separada de loot por padrão.
Companion não deve puxar packs novos sozinho.
Companion downed não tem permadeath no sistema base.
Companion equipment completo é futuro, não MVP.
Companion jobs de fazenda exigem área, ferramenta, estação, tempo e Stamina.
Companion de caverna deve respeitar leash, safe spawn, boss gates e active combat budget.
Romance/casamento não devem ser caminho obrigatório de poder.
```

---

# PARTE Y — Pendências abertas

```text
Limpar Breath/Fôlego do CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md em refactor próprio.
Definir quais NPCs concretos serão companions de fazenda, caverna ou quest no roster da cidade.
Definir primeiro conjunto de 3-5 companions iniciais para MVP.
Definir pets em PETS_DIRECTION.md.
Definir social/romance em SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md.
Definir quest flags de companion em QUESTS_MAIN_PROGRESSION_DIRECTION.md.
Validar DPS/healing de companion contra Cave Combat Balance.
Validar farm job output contra economia/fazenda.
Validar save/load de companion em cave snapshot/replay.
```

---

# EMENDA 2026-06-13-V3 (Refinamento v3)

> **Fonte vinculante:** `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md`, BLOCO 3 (decisões 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.8, 3.11, 3.12) + acréscimos #3 e #4 da re-auditoria de código de 2026-06-13.
> **Catálogo de papéis associado (artefato A7):** `docs/design/gameplay/companions/COMPANION_ROLES_CATALOG_v1.0.md` — define, por papel, bônus (combate + fora de combate) e conjunto de ações. As decisões abaixo devem ser lidas em conjunto com esse catálogo; em conflito de papel/ação, o catálogo é a fonte detalhada e esta emenda fixa as regras de sistema.
> **Natureza:** esta emenda **adiciona** regras e **supersede** trechos pontuais marcados (PARTE P §45-46). Todo o texto anterior do documento permanece como registro; quando uma decisão v3 conflita com texto antigo, a decisão v3 vence (documento mais novo, conforme cabeçalho da v3.0).
> **Escopo de implementação:** as specs `14_spec_companion_*` permanecem **gated na WAVE 14** (decisão 3.12). Esta emenda destrava o **contrato** (specs viram spec-ready), não antecipa a execução de runtime.

## V3.1 — Comandos e Stances (decisões 3.2 e 3.3)

A direção original (PARTE J) descreve estados de IA, mas não fixava o conjunto de **comandos do jogador** nem o conjunto de **stances**. A v3 fecha ambos.

### Comandos do jogador (3 comandos diretos)

```text
Ficar / Esperar
  companion permanece na posição atual; não segue; mantém a stance vigente para autodefesa.

Seguir
  companion acompanha o jogador respeitando FollowDistance, leash e safe spawn (PARTE I/J).

Atacar alvo marcado
  o jogador marca um inimigo; o companion prioriza esse alvo dentro dos limites da stance,
  do leash e do active combat budget (PARTE K). Não puxa pack novo para alcançar o alvo.
```

### Stances de combate (3 universais + 1 modo de papel)

```text
Agressivo
  ataca qualquer inimigo dentro de um raio de até 12 tiles do jogador.

Passivo
  não ataca ninguém; só executa comandos diretos (ex.: Atacar alvo marcado continua válido
  como comando explícito) e ações de autopreservação/retreat.

Defensivo (DEFAULT)
  só ataca quem chega a <= 4 tiles do jogador OU quem ataca o jogador.
  É a stance padrão ao ativar um companion.

Suporter (modo de papel, NÃO é 4a stance universal)
  disponível apenas para papéis de suporte (cura/buff/Healer/Alchemist/MusicianSupport e afins
  no catálogo de papéis). Quando ativo, o companion prioriza cura/buff/limpeza de status conforme
  as regras de cura/suporte da PARTE K (MP, cooldown, range, cast, limite por combate/run).
```

Reconciliação (3.3 × 3.2, conforme `FABLE_DECISOES_RESPOSTAS_v3.0.md` "Ambiguidades interpretadas" #1):

```text
Existem 3 stances universais: Agressivo, Defensivo (default), Passivo.
Suporter é um MODO exposto apenas por papéis de suporte, não uma stance disponível a todos.
Raios (12 tiles agressivo / 4 tiles defensivo) são budget de design; o tuning final valida
contra Cave Combat Balance e leash.
```

## V3.2 — Papéis de companion (decisões 3.1 e 3.3)

```text
Há mais papéis do que os 3 inicialmente propostos.
O conjunto canônico de papéis, com bônus (combate + fora de combate) e ações por papel,
vive no Catálogo de Papéis de Companion (artefato A7):
  docs/design/gameplay/companions/COMPANION_ROLES_CATALOG_v1.0.md
O número de companions iniciais para o MVP será fixado DEPOIS do catálogo (decisão 3.1).
As listas de roles da PARTE D (§9-§11) continuam válidas como inventário de papéis;
o catálogo as detalha e o modo Suporter (V3.1) marca quais papéis expõem cura/buff.
```

## V3.3 — Equipment no sistema base (decisão 3.4, OVERRIDE — conflita com PARTE P §45-46)

> Esta seção **supersede** as §45 e §46 (marcadas como SUPERSEDED no corpo do documento).

```text
Companion equipa ARMA + ACESSÓRIO no SISTEMA BASE (2 slots), não como recurso futuro.
Não é inventário completo: apenas os 2 slots (sem microgerenciamento excessivo).
Equipment não deve transformar o companion em segundo player; os limites de DPS/healing/budget
  da PARTE K continuam valendo mesmo com gear equipado.
Alguns NPCs podem recusar tipos de gear por personalidade/classe (regra histórica preservada).
Item único de companion pode vir de quest pessoal (regra histórica preservada).
```

Implicação para as specs 14_*:

```text
A spec de eligibility/state/save deve poder persistir os 2 slots de equipment do companion
  (apenas IDs estáveis, nunca referência Unity — ver V3.6).
A spec de cave assist/brain/balance deve considerar o gear ao calcular contribuição, sem
  estourar os limites de DPS/budget.
```

## V3.4 — Derrota do companion: Downed → resgate → Retreat → Injured (decisão 3.5)

Detalha/confirma a PARTE M (§36-§38):

```text
HP do companion chega a 0 -> Downed.
Abre-se uma janela de resgate (o jogador pode resgatar).
Se NÃO resgatado dentro da janela -> Retreat automático (o companion sai por conta própria).
Após Downed/Retreat -> Injured por 1-2 dias (indisponível até recuperação).
Sem permadeath no combate base (ver V3.5 para a exceção narrativa).
```

## V3.5 — Player derrotado com companion ativo: revive 30% (decisão 3.6, CUSTOM)

Regra nova (não existia na direção original):

```text
Quando o PLAYER é derrotado e há um companion ativo:
  o companion TENTA REVIVER o player com 30% de chance.
  Se conseguir: o player volta (ressuscita no local conforme regra de revive do combate).
  Se falhar: o companion ESCAPA e volta Injured por 1 dia (não morre no combate base).
A chance base é 30%; modificadores de papel/bond/perk podem ajustá-la no catálogo/balance profile,
  mas a revive nunca vira garantia (não pode chegar a 100% no sistema base).
```

## V3.6 — Permadeath apenas em eventos narrativos + Fonte de Anya (decisão 3.7, OVERRIDE)

Reconcilia 3.7 (B) com 3.5 (A), conforme `FABLE_DECISOES_RESPOSTAS_v3.0.md` "Conflitos a reconciliar" #2. Supersede a frase da PARTE M §37 ("Permadeath, se existir, é evento narrativo único/futuro") tornando-a explícita e vinculante:

```text
Combate base: SEM permadeath. Companion derrotado vira Injured (V3.4).
Permadeath EXISTE, porém APENAS em eventos narrativos específicos e roteirizados (scripted).
  Esses eventos devem ser marcados explicitamente como permadeath-capable; nenhum combate
  procedural/aleatório de caverna pode matar permanentemente um companion.
Fonte de Anya: pode RESSUSCITAR um companion morto em evento narrativo, com CUSTO PROGRESSIVO
  ("Ressurreição Dolorosa") — cada ressurreição custa mais, evitando reset grátis (preserva
  a regra da PARTE M §38: a Fonte não vira reset infinito).
```

## V3.7 — Fertilizante raro automático: toggle por job Planter, default OFF (decisão 3.8)

Detalha a PARTE G (jobs de fazenda) e a PARTE U (CompanionJobProfileSO):

```text
O job de Planter pode aplicar fertilizante raro automaticamente.
Isso é um TOGGLE por job (granularidade: por job de Planter), com DEFAULT OFF.
Quando ligado, consome fertilizante do storage AUTORIZADO do job (nunca cria do nada,
  nunca acessa inventário do jogador sem comando — preserva a PARTE G §19 e a storage policy
  da spec de farm jobs).
Sem fertilizante disponível no storage autorizado -> o job planta sem fertilizar (não falha por isso).
```

## V3.8 — Vínculo: Bond 0-5 com perks em 2 e 4 (decisão 3.11)

Detalha a PARTE N (§39-§41):

```text
CompanionBondLevel vai de 0 a 5.
Perk passivo concedido nos níveis 2 e 4 (1 perk passivo por marco; 2 perks no total ao chegar a 4).
JobRank e CaveRank são trilhas SEPARADAS do bond (a estrutura já existe no código: CompanionSaveEntry
  tem BondLevel, JobRank e CaveRank distintos).
Os perks de bond seguem o modelo simples da PARTE O (§42): pequenos, legíveis, sem árvore grande.
```

## V3.9 — Reconciliações de código (acréscimos #3 e #4 da re-auditoria 2026-06-13)

> Estes itens não vêm de uma decisão A/B/C do dono, mas da re-auditoria de código que a v3.0 incorporou. Ficam registrados aqui para que as specs 14_* fechem a dívida.

### V3.9.1 — CompanionManagerSaveData precisa de SaveSectionProvider (acréscimo #3)

```text
Estado atual do código (verificado 2026-06-13):
  CompanionManagerSaveData existe como DTO embutido em SaveData (campo SaveData.Companions),
  com CompanionSaveEntry (CompanionId, NpcId, UnlockState, UnlockedRoles, UnlockedByQuestIds,
  BondLevel, TrustPoints, Fatigue, InjuryState, LastInteractionDay, JobRank, CaveRank).
  NÃO existe um ISaveSectionProvider dedicado para companions (apenas HotbarSectionProvider
  existe em Assets/_Game/Scripts/Save/Providers/).
Dívida a fechar:
  a spec de eligibility/state/save (14_spec_companion_eligibility_recruitment_state_save_*)
  deve criar um CompanionSaveSectionProvider seguindo o precedente HotbarSectionProvider,
  garantindo round-trip (capture/restore) do estado de companion — hoje o DTO existe mas o
  round-trip via provider não está fechado.
  Regra de save mantida: apenas IDs estáveis e tipos simples; sem referência Unity.
```

### V3.9.2 — Romance/Spouse em flags mas não no enum CompanionRole (acréscimo #4)

```text
Estado atual do código (verificado 2026-06-13):
  CompanionEligibilityFlags tem os bools CanBeRomanceCompanion e CanBeSpouseCompanion.
  O enum CompanionRole tem apenas None/FarmCompanion/CaveCompanion/QuestCompanion/SocialCompanion.
  Ou seja, Romance/Spouse existem como ELEGIBILIDADE mas não como PAPEL mecânico.
Reconciliação canônica (registrada aqui; detalhe no Catálogo de Papéis A7):
  Romance e Spouse permanecem ELEGIBILIDADES (flags), NÃO papéis de combate/job.
  Isto é coerente com a PARTE A/R: "nem todo romance vira companion de combate" e
  "nem todo spouse deve ser combat companion".
  Portanto NÃO se deve adicionar Romance/Spouse ao enum CompanionRole como papel mecânico;
  o catálogo de papéis e a spec de eligibility devem documentar que Romance/Spouse são gates
  de elegibilidade/social, e que combate/job derivam de PrimaryRole/SecondaryRole (papéis reais).
  Se uma futura decisão quiser papel social mecânico, será uma decisão explícita à parte.
```

## V3.10 — Status das specs 14_* (decisão 3.12)

```text
Manter WAVE 14 como wave de implementação dos companions.
Este refinamento converte as specs 14_spec_companion_* de "future mapped" para SPEC-READY:
  o contrato/escopo está destravado e validável; a EXECUÇÃO de runtime segue gated na WAVE 14.
Cada uma das 4 specs recebe um bloco "EMENDA 2026-06-13-V3" apontando para as decisões 3.x
  aplicáveis e para o Catálogo de Papéis (A7).
```
