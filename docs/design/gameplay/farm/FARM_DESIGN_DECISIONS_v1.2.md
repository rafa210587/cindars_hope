# Cindar's Hope — Farm Design Decisions v1.2

> **Status:** decisões fechadas de design da fazenda  
> **Complementa:** `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.1.md`  
> **Função:** remover pendências específicas e registrar decisões oficiais antes da quebra em specs.  
> **Não é spec implementável.** Deve alimentar specs futuras em `docs/specs/a_implementar/`.

---

## 1. Decisões fechadas nesta versão

Este documento fecha quatro dúvidas registradas no `FARM_DESIGN_DIRECTION_v1.1.md`.

| Tema | Decisão |
|---|---|
| Cansaço | Será um sistema novo, mas deve afetar/ser refletido pelo `PlayerConditionManager`. |
| Pets | Pet é sistema próprio, não subtipo de companion. |
| Cachorro em combate | Cachorro é separado do slot de companion. Não ocupa slot de companion de caverna. |
| Movimentação de construções | O jogador pode mover casa, SellPoint e construções secundárias, respeitando regras de terreno, colisão, custo e modo construção. |

---

# PARTE A — Cansaço

## 2. Decisão

O **Cansaço** deve ser implementado como um sistema próprio.

Nome conceitual sugerido:

```text
FatigueSystem
```

ou, em padrão de manager:

```text
FatigueManager
```

Esse sistema não deve ser absorvido pelo `HungerManager`.

Ele também não deve ser apenas um campo solto dentro do `PlayerConditionManager`.

## 3. Relação com PlayerConditionManager

O `PlayerConditionManager` deve continuar sendo a camada agregadora/observável das condições do jogador.

A relação desejada é:

```text
FatigueSystem
  calcula, altera e persiste regras específicas de cansaço

PlayerConditionManager
  expõe estado consolidado do jogador para UI, save, eventos e outros sistemas
```

Ou seja:

```text
Cansaço tem sistema próprio.
PlayerConditionManager é afetado por ele e deve refletir o estado final.
```

## 4. Responsabilidades do FatigueSystem

O sistema de cansaço deve controlar:

- aumento natural de cansaço com o tempo;
- aumento adicional por gasto de stamina;
- aumento acelerado por fome;
- aumento diferenciado em caverna;
- efeitos de dormir cedo/tarde;
- penalidades por cansaço alto;
- recuperação ao dormir;
- modificadores vindos de comida, poções, pets, buffs ou Fruto Mana;
- eventos relacionados a mudança de faixa de cansaço.

## 5. Responsabilidades do PlayerConditionManager

O `PlayerConditionManager` deve refletir:

- HP/Vida;
- Stamina;
- Fome;
- Cansaço;
- Pontos de Magia/MP quando o sistema de magia for formalizado;
- estados derivados, como `Exhausted`, `Hungry`, `LowStamina`, `LowHP`.

Ele deve ser o ponto preferencial para UI/HUD e outros sistemas consultarem a condição geral do jogador.

## 6. Eventos candidatos

```text
FatigueChangedEvent
FatigueThresholdReachedEvent
PlayerExhaustedEvent
PlayerRestedEvent
SleepQualityCalculatedEvent
PlayerConditionChangedEvent
```

Não criar eventos duplicados se já existir equivalente.

## 7. Save sugerido

```text
FatigueSaveData
  CurrentFatigue
  LastUpdatedGameTime
  LastSleepDay
  LastSleepHour
  SleepQualityModifier
  TemporaryFatigueModifiers[]
```

O `PlayerConditionSaveData` pode referenciar ou agregar esse valor, mas a regra de cálculo pertence ao sistema de cansaço.

---

# PARTE B — Pets

## 8. Decisão

Pets são sistema próprio.

Pets não são subtipo de companion.

Nome conceitual sugerido:

```text
PetSystem
PetManager
```

## 9. Diferença entre pet e companion

| Aspecto | Pet | Companion |
|---|---|---|
| Papel | vínculo doméstico, bônus, suporte leve | personagem aliado com jobs, combate ou funções maiores |
| Origem | animal doméstico/familiar | NPC ou personagem recrutável |
| Slot de caverna | não ocupa slot principal | ocupa slot de companion ativo, quando aplicável |
| Progressão | vínculo/cuidado | nível, afinidade, jobs, habilidades |
| Morte permanente | não recomendada | pode ficar indisponível até ressurreição |
| Gestão | área de descanso, alimentação, seguir/ficar | job board, moradia, equipamento, caverna |

## 10. Responsabilidades do PetSystem

O sistema de pets deve controlar:

- cadastro de pets do jogador;
- tipo do pet: cachorro, gato, futuros pets especiais;
- nome;
- alimentação;
- vínculo;
- estado de descanso;
- área/cama/casinha atribuída;
- seguir ou ficar na fazenda;
- buffs ativos;
- disponibilidade para combate/suporte;
- save/load.

## 11. Save sugerido

```text
PetSaveData
  PetInstanceId
  PetType
  Name
  BondLevel
  HungerState
  RestState
  IsFollowingPlayer
  AssignedPetBedId
  LastFedDay
  LastInteractionDay
  TemporaryBuffs[]
  CombatAvailabilityState
```

---

# PARTE C — Cachorro em combate

## 12. Decisão

O cachorro é separado do companion.

Ele não ocupa o slot de companion de combate/caverna.

Fluxo desejado:

```text
Player
  + 1 Companion ativo, se houver
  + Cachorro, se estiver seguindo e disponível
```

## 13. Limites do cachorro

O cachorro pode:

- atacar inimigos fracos;
- distrair inimigos;
- alertar emboscadas;
- detectar inimigos próximos;
- encontrar item após combate;
- proteger o jogador em situações leves/intermediárias.

O cachorro não deve:

- substituir companion de caverna;
- carregar combate sozinho;
- derrotar bosses sozinho;
- escalar como personagem principal;
- invalidar builds de jogador;
- remover risco da caverna.

## 14. Estado em combate

Se o cachorro cair em combate:

```text
CombatAvailabilityState = Unavailable
```

Ele deve precisar de descanso, comida, cura simples ou retorno à fazenda.

Recomendação: não usar morte permanente para pets.

## 15. Relação com UI

A UI deve tratar cachorro como pet auxiliar, não como companion principal.

Sugestão:

```text
HUD principal: player
Painel lateral/ícone pequeno: pet seguindo/disponível
Painel de companion: companion ativo separado
```

---

# PARTE D — Movimento de casa, SellPoint e construções

## 16. Decisão

O jogador pode mover:

- casa;
- SellPoint/caixa de envio;
- construções secundárias;
- baús;
- áreas de pet;
- workshops;
- pastos/currais/galinheiros;
- decoração;
- processadores.

A decisão anterior de casa fixa fica substituída por esta regra:

```text
A casa começa em posição fixa, mas pode ser movida depois que o modo construção/expansão permitir.
```

## 17. Regra de liberdade controlada

O jogador pode mover construções, mas o jogo deve preservar:

- acesso por caminho;
- colisão válida;
- footprint em tile válido;
- distância mínima de bordas/água/portais, quando necessário;
- desbloqueio da zona;
- ausência de conflito com área bloqueada;
- consistência visual;
- segurança de save/load.

## 18. Elementos que permanecem fixos

Mesmo com casa e SellPoint móveis, alguns elementos continuam fixos:

- Fonte de Anya;
- lago principal;
- entrada da caverna;
- saída para cidade;
- bordas naturais;
- área da pedreira final;
- portais/marcos de lore;
- áreas de expansão ainda bloqueadas.

## 19. Movimento da casa

Mover a casa deve ser uma ação avançada, não necessariamente disponível no início.

Regras sugeridas:

- exige modo construção;
- pode ter custo em ouro/materiais;
- pode exigir casa vazia de construção em andamento;
- precisa preservar ponto de spawn do jogador;
- precisa atualizar navegação, portas, colisores e save;
- pode levar 1 dia de obra ou ser instantâneo após confirmação, a decidir.

## 20. Movimento do SellPoint

SellPoint/caixa de envio pode ser movido com menos restrição que a casa.

Regras sugeridas:

- deve ficar em tile acessível;
- não pode bloquear entrada/saída;
- deve preservar itens pendentes de envio;
- se houver venda pendente, mover não pode apagar conteúdo;
- pode ter custo baixo ou gratuito dentro do modo construção.

## 21. Save sugerido para construções móveis

```text
BuildingSaveData
  BuildingInstanceId
  BuildingId
  Position
  Rotation
  Level
  State
  ConstructionRemainingDays
  AssignedCompanionId
  InternalInventoryRef
  CanMove
  LastMovedDay
```

Para casa:

```text
FarmHouseSaveData
  BuildingInstanceId
  Position
  DoorPosition
  PlayerSpawnPosition
  HouseLevel
  InternalLayoutId
```

Para SellPoint:

```text
ShippingBinSaveData
  BuildingInstanceId
  Position
  PendingItems[]
  LastShipmentDay
  IsMovable
```

---

# PARTE E — Pendências removidas

As seguintes pendências deixam de ser pendências:

```text
- Cansaço fica em PlayerConditionManager, HungerManager ou sistema novo?
  Decisão: sistema novo, afetando PlayerConditionManager.

- Pet é sistema próprio ou subtipo de companion?
  Decisão: sistema próprio.

- Cachorro ocupa slot de companion em combate ou é pet separado?
  Decisão: pet separado; não ocupa slot de companion.

- O jogador pode mover casa/SellPoint ou apenas construções secundárias?
  Decisão: pode mover casa, SellPoint e construções, com regras de modo construção.
```

---

# PARTE F — Specs futuras impactadas

Estas decisões impactam diretamente as futuras specs:

```text
spec_player_condition_fatigue_sleep_hunger_stamina.md
spec_farm_layout_expansion_zones_free_build.md
spec_farm_buildings_construction_workshops_storage.md
spec_farm_pets_dog_cat_bond_buffs.md
spec_farm_shipping_bin_orders_processing.md
```

Também impactam indiretamente:

```text
spec_ui_ux_full_gameplay_inventory_hotbar_menus.md
spec_cave_entry_death_fonte_corpse_recovery.md
spec_companions_jobs_affinity_morte.md
spec_combat_magic_progression_mp_status.md
```

---

## 22. Próxima atualização recomendada

Quando houver nova revisão ampla da fazenda, consolidar:

```text
FARM_DESIGN_DIRECTION_v1.1.md
+
FARM_DESIGN_DECISIONS_v1.2.md
```

em:

```text
FARM_DESIGN_DIRECTION_v1.2.md
```

Por enquanto, este arquivo é o registro oficial das decisões fechadas após a v1.1.
