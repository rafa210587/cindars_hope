# Human Test Scenario — NPCs respeitam barreiras + movimentação viva

**Mudança:** colisão de NPC com paredes/portas (sem bloquear o jogador) + idle-wander universal por tier.
**Pré-requisito:** rodar `CindarsHope/Inicializar Projeto` (regenera TownScene com os corpos sólidos,
wanderers e anchors de porta) antes de testar. Entrar em Play Mode na `TownScene`.

---

## 1. Colisão: NPC respeita paredes, jogador atravessa NPC
- [ ] Empurrar um NPC contra a parede de uma casa: ele **não atravessa** a parede (corpo sólido).
- [ ] Andar com o jogador "para dentro" de um NPC: o jogador **passa por ele** (não trava, não há
      bloco invisível nos pés) — `Physics2D.IgnoreCollision` jogador↔NPC.
- [ ] Conversar com qualquer NPC continua funcionando (chegar perto + E) — o trigger de interação não
      foi afetado.

## 2. Movimentação viva (todos os NPCs)
- [ ] **Lojistas** (Sael, Mella, Hess, ferreiro, etc.): fazem um micro-vaivém (~2 tiles) em torno do
      posto, sem sair de perto da banca/oficina.
- [ ] NPCs de **ronda** (Patrol/WanderWithinZone): andam num raio maior pela vizinhança.
- [ ] O **Peregrino de Vaalara** (Roam): cobre uma área grande da cidade.
- [ ] Nenhum NPC fica 100% imóvel (todos têm pelo menos o micro-vaivém).
- [ ] Durante uma conversa, o NPC **para** de andar; ao fechar o diálogo, volta a se mexer.

## 3. Entrar em casa pela porta (moradores indoor)
- [ ] Avançar o relógio até a noite (bloco Home). Observar um morador (ex.: Sael→Pescaria,
      Mella→Padaria, Hess→Tanoaria, ou um residente clássico) caminhar até a **porta** da sua casa.
- [ ] A porta **abre sozinha** quando o morador chega e ele entra a pé pelo vão (sem teleporte
      atravessando a parede).
- [ ] A porta **fecha** pouco depois de ele entrar.
- [ ] O jogador ainda abre/fecha a mesma porta manualmente com E (comportamento inalterado).
- [ ] Tibbet (outdoor) continua dormindo no cemitério, sem porta.

## 4. Não-regressões
- [ ] Spawn fazenda→cidade continua no portão oeste.
- [ ] Roof reveal: telhado some quando o **jogador** entra (NPC dentro não revela).
- [ ] Sem erro novo no Console relativo a NpcWanderer/anchors.

## Riscos conhecidos (verificar no Play Mode)
- NPCs sólidos podem se acotovelar levemente quando muito próximos (bancas do mercado). Aceitável.
- Se um morador encostar na casa fora do alinhamento da porta, há um fallback de teleporte ao anchor
  após alguns segundos (rede de segurança anti-travamento) — não deve disparar no caso alinhado.

## Resultado
- PASS se todos os checks marcados; registrar divergências com screenshot do Console.
