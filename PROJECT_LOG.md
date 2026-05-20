# Cindar's Hope — Project Log

> Fonte operacional curta de continuidade do projeto.  
> Histórico completo preservado em `docs/logs/PROJECT_LOG_ARCHIVE_2026-05-18_FULL_BEFORE_SPLIT.md`.  
> Status curto de capacidades/specs preservado em `docs/IMPLEMENTATION_STATUS.md`.

---

## 1. Handoff atual

### Estado real validado

- Repositório: `rafa210587/cindars_hope`.
- Branch de trabalho: `dev`.
- Branch default do GitHub: `main`.
- A `dev` contém MVPs de Farm, Town, Crafting, Save/Load, Cave/Combat básico, HUD debug, transições Farm/Town/Cave e docs/specs da FASE9E/FASE9F/FASE9G.
- `PROJECT_LOG.md` foi reduzido para handoff operacional curto.
- O histórico completo anterior foi arquivado sem perda intencional em `docs/logs/PROJECT_LOG_ARCHIVE_2026-05-18_FULL_BEFORE_SPLIT.md`.
- Tracking curto de capacidades/specs implementadas criado em `docs/IMPLEMENTATION_STATUS.md`.
- Política de evolução de specs registrada em `docs/SPEC_EVOLUTION_POLICY_v1.0.md`.

### Specs recentes aprovadas

- `docs/FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT_SPEC_v1.0.md`
- `docs/FASE9E_DAMAGE_STATUS_FORMULA_SPEC_v1.0.md`
- `docs/FASE9E_ITEM_TAXONOMY_IDS_SPEC_v1.0.md`
- `docs/FASE9E_ITEM_EXAMPLES_VARIATIONS_v1.0.md`
- `docs/FUTURE_IDEAS_TODO_v1.0.md`
- `docs/FASE9E_SAVE_SCHEMA_MIGRATION_SPEC_v1.0.md`
- `docs/FASE9E_PLAYER_LEVEL_UP_PROGRESSION_SPEC_v1.0.md`
- `docs/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md`
- `specs/FASE9F_CAVE_RESOURCES_ENCOUNTERS/spec.md`
- `docs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY_SPEC_v1.0.md`
- `specs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY/spec.md`
- `docs/SPEC_EVOLUTION_POLICY_v1.0.md`

### Próximo passo recomendado

Começar **FASE9F-A — Cave Procedural Foundation** com PRs pequenos, mantendo FASE9G como baseline de design para bestiário/faction locks quando a implementação de cave procedural chegar em enemy ecology.

1. PR-170 — Cave procedural contracts.
2. PR-171 — Cave procedural generator MVP.
3. PR-172 — Cave run regeneration on player defeat/KO.
4. PR-173 — Cave checkpoints a cada 15 níveis.
5. PR-174 — ResourceNode contracts com ferramenta/tier/stamina/fallback.

Em paralelo, este chat pode continuar refinando novas specs. Specs aprovadas antigas não devem ser reescritas destrutivamente; correções entram como amendments/corrections.

---

## 2. Protocolo obrigatório para agentes

Antes de qualquer tarefa:

1. Ler `PROJECT_LOG.md`.
2. Ler `docs/IMPLEMENTATION_STATUS.md`.
3. Ler `AGENTS.md` e/ou `CLAUDE.md`.
4. Ler os documentos de referência do PR/tarefa.
5. Confirmar branch atual e escopo permitido.
6. Validar estado real no GitHub/repo antes de planejar.
7. Se o trabalho tocar specs, ler `docs/SPEC_EVOLUTION_POLICY_v1.0.md`.

Durante a tarefa:

1. Manter escopo pequeno.
2. Não implementar V2/FULL quando o PR é MVP.
3. Não alterar docs de design sem pedido explícito.
4. Não mexer em arquivos fora da lista permitida do PR.
5. Registrar dúvidas/desvios em vez de decidir silenciosamente.
6. Não reescrever spec aprovada de forma destrutiva; usar nova spec, amendment, correction ou errata.

Ao final de tarefa relevante:

1. Atualizar `PROJECT_LOG.md` com nova entrada curta.
2. Atualizar `docs/IMPLEMENTATION_STATUS.md` com status curto de capacidades/specs.
3. Informar arquivos alterados.
4. Informar testes executados ou não executados.
5. Informar pendências, riscos e próximo passo recomendado.
6. Se a entrada ficar grande demais, criar novo archive em `docs/logs/` e manter este arquivo curto.

---

## 3. Política de evolução de specs

Fonte completa: `docs/SPEC_EVOLUTION_POLICY_v1.0.md`.

Resumo operacional:

```text
Implementar specs aprovadas pode acontecer em paralelo ao refinamento de novas specs.
Specs antigas aprovadas não devem ser reescritas destrutivamente.
Mudanças futuras entram como nova spec, amendment, correction ou errata.
PR iniciado segue a spec vigente no início, salvo bug crítico ou decisão humana explícita.
```

Regras:

- Specs aprovadas são baseline de implementação.
- Implementação pode continuar em outro chat usando a spec aprovada vigente.
- Novas specs podem ser escritas em paralelo neste chat.
- Correções em specs antigas devem indicar impacto em PRs futuros.
- Amendments/corrections devem ficar preferencialmente em `docs/amendments/`.

---

## 4. Estado consolidado curto

Fonte curta e atualizável: `docs/IMPLEMENTATION_STATUS.md`.

### Implementado no repo

- Core `GameEventBus` e eventos base.
- Data contracts e registries por ID.
- Farm MVP: plots, seeds, plantio, crescimento, colheita.
- Economy/Hunger/HUD debug.
- Save/load JSON local com cena atual e rebind cross-scene.
- World activities: árvores, pesca básica, pickups persistentes.
- Crafting MVP: receita de madeira processada e crafting point.
- Town MVP: portal Farm/Town, NPC Pip, compra/venda básica.
- Cave/Combat MVP: CaveScene, portal Farm/Cave, Slime, melee punch, chase, contact damage, drops, hit flash, knockback.
- Scene generators: FarmScene, TownScene, CaveScene.
- Validators de dados/cenas MVP.

### Especificado para próximas waves

- UI/Hotbar/Inventory/Equipment.
- Damage/Elementos/Status/Fórmula única.
- Item Taxonomy/IDs.
- Save Schema/Migration.
- Player Level Up/Progression.
- Cave/Resources/Encounters/Procedural progression.
- Cave Bestiary/Faction Locks/Portal Ecology.

---

## 5. Decisões FASE9F Cave

- Primeira entrada começa em `CaveLevel = 1`.
- Checkpoints permanentes a cada 15 níveis: `1, 15, 30, 45, 60, 75, 90`.
- Jogador pode escolher qualquer checkpoint liberado ao entrar na caverna.
- Cave usa `CaveWorldSeed` persistente e `CaveRunSeed` por run.
- Ao sofrer KO/derrota, a cave run é regenerada com nova `CaveRunSeed`; checkpoints permanecem.
- Cada nível deve ser grande, labiríntico e explorável.
- ResourceNode exige ferramenta correta, tier mínimo e consome stamina.
- Minério exige Pickaxe; sem pickaxe/tier suficiente, fallback gera `1x item_material_stone`, não entrega minério principal e não depleta node.
- Renovação diária só reseta nodes com `RespawnsDaily = true`.
- Baús ficam depois de nodes + enemies.
- Slime especial deve ter cor/visual diferente.
- Toda mudança de bioma tem boss poderoso e difícil.
- Recursos variam por nível/faixa; árvores subterrâneas podem dar madeiras melhores que exigem refinamento.

---

## 6. Decisões FASE9G Cave Bestiary/Faction Locks

- Geração procedural deve travar `FactionLock` por subfaixa de 3–5 níveis.
- Cada CaveLevel tem um FactionLock principal.
- Inimigos incompatíveis não aparecem no mesmo nível salvo exceções explícitas.
- Exceções: `AmbientFauna`, `RareIntruder`, `BossOverride`, `ConflictEncounter`.
- `ConflictEncounter` fica fora do MVP.
- `RareIntruder` entra com chance baixa e limitado por bioma.
- Boss e miniboss têm 3 opções procedurais por marco.
- Boss checkpoint persiste por save.
- Miniboss persiste por run.
- Humanoides inimigos são facções/exilados/cultistas/corrompidos/guardiões, não raças malignas por natureza.
- Beholder-like vira Observador/Olho de Elyndor.
- Duergar-like vira Anão da Forja Sem Sol / Anão Profundo Exilado.
- Drakes/wyverns antes do 90; dragão verdadeiro só late game/boss.
- Level 100 tem três possíveis final bosses por save.
- Luas modificam pesos, não quebram lock.

---

## 7. Checklist pendente

### Validação Unity geral

- [ ] Rodar `CindarsHope/Validate/Validate MVP Data`.
- [ ] Rodar `CindarsHope/Scenes/Create MVP FarmScene`.
- [ ] Rodar `CindarsHope/Scenes/Create MVP TownScene`.
- [ ] Rodar `CindarsHope/Scenes/Create MVP CaveScene`.
- [ ] Rodar validators de Farm/Town/Cave quando disponíveis.
- [ ] Testar Play Mode completo Farm → Town → Cave → Farm.
- [ ] Testar save/load em FarmScene.
- [ ] Testar save/load em TownScene.
- [ ] Testar save/load em CaveScene.
- [ ] Confirmar Console sem erro vermelho.

### Validação FASE9F futura

- [ ] CaveLevel 1 gera layout procedural grande.
- [ ] Cave run muda após KO/derrota.
- [ ] Checkpoints permanecem após KO/derrota.
- [ ] ResourceNode consome stamina.
- [ ] ResourceNode valida ferramenta/tier.
- [ ] Fallback sem pickaxe retorna apenas `1x item_material_stone`.
- [ ] Nodes `RespawnsDaily = true` renovam no novo dia.
- [ ] Slime especial tem cor diferente.
- [ ] Boss de bioma bloqueia avanço.

### Validação FASE9G futura

- [ ] CaveLevel gerado possui `BiomeId`, `EncounterEcologyId`, `FactionLockId` e `EnemyFamilyIds`.
- [ ] FactionLock impede mistura incoerente de inimigos.
- [ ] Boss/miniboss é escolhido entre 3 candidatos compatíveis.
- [ ] Boss checkpoint persiste por save.
- [ ] Miniboss persiste por run.
- [ ] DebugHud mostra ecology/faction/boss candidate quando implementado.

---

## 8. Histórico arquivado

O histórico completo antigo do `PROJECT_LOG.md` foi arquivado em:

```text
docs/logs/PROJECT_LOG_ARCHIVE_2026-05-18_FULL_BEFORE_SPLIT.md
```

Esse arquivo preserva o log operacional anterior inteiro antes da redução do log raiz.

---

## 9. Log de atividades recente

## 2026-05-20 - PR-100 Auditoria pos PR-099

**Responsavel:** Codex/ChatGPT  
**Branch:** `feature/pr-100-audit-pos-pr099`  
**Escopo:** auditar o estado real da `dev` apos o PR-099 e reconciliar a fila antes de voltar para Cave Procedural.

### Alteracoes

- Criado `docs/audits/PR100_POST_PR099_REPO_AUDIT.md`.
- Registrado que `PR-099 - Enemy stats data-driven` e o ultimo PR de implementacao confirmado por codigo.
- Registrado que `feature/pr-170-cave-procedural-contracts` existe como codigo adiantado/candidato local e deve ser reaproveitado depois, nao mergeado agora.
- Atualizado `docs/IMPLEMENTATION_STATUS.md` para trocar o proximo bloco recomendado de PR-170+ para reconciliacao PR-100 a PR-130.

### Testes

- [x] `git checkout dev`.
- [x] `git pull origin dev`.
- [x] Branch `feature/pr-100-audit-pos-pr099` criada a partir da `dev`.
- [x] Leitura documental obrigatoria executada.
- [x] Inventario estatico de scripts, dados, cenas e branches executado.
- [ ] Unity nao executado; auditoria documental/estatica.

### Pendencias / riscos

- Existem alteracoes locais ignoradas em `Assets/MobileDependencyResolver/**` e nas cenas MVP; o humano autorizou ignorar esses caminhos neste fluxo.
- `feature/fase9b3-enemy-data-driven-stats` nao apareceu local/remoto, apesar de citada no historico do PR-099.
- `Assets/_Game/Scripts/Cave`, `Tools`, `Equipment`, `UI/Hotbar` e `Player/Progression` ainda nao existem em `dev`.

### Proximo passo recomendado

- PR-101 - reconciliar branches/fixes PR-099 sem merge automatico.

---

## 2026-05-20 — FASE9G Cave Bestiary/Faction Locks

**Responsável:** ChatGPT  
**Branch:** dev  
**Escopo:** criar spec de bestiário, faction locks, ecologia procedural e boss/miniboss candidates para a cave.

### Alterações

- Criado `docs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY_SPEC_v1.0.md`.
- Criado `specs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY/spec.md`.
- Atualizado `PROJECT_LOG.md` com decisões FASE9G e checklist futuro.
- Spec inclui uso do Guia de Raças de Vaalara, Vaalara/Daromir/Elyndor, faction locks por subfaixa, bestiário amplo e 3 opções procedurais de boss/miniboss por marco.

### Testes

- [x] Arquivos FASE9G criados no repo.
- [x] Arquivos FASE9G lidos/validados no GitHub.
- [ ] Unity não executado; alteração é documental.

### Pendências / riscos

- Atualizar `docs/IMPLEMENTATION_STATUS.md` para listar FASE9G como especificada.
- FASE9G depende da base FASE9F para implementação real.

### Próximo passo recomendado

- Seguir com implementação FASE9F-A em outro chat.
- Usar FASE9G quando a implementação chegar em enemy ecology/faction lock.

---

## 2026-05-20 — Tracking de implementação

**Responsável:** ChatGPT  
**Branch:** dev  
**Escopo:** criar tracking curto de capacidades/specs implementadas e pendentes, separado do log operacional.

### Alterações

- Criado `docs/IMPLEMENTATION_STATUS.md`.
- Atualizado `PROJECT_LOG.md` para apontar o tracking como leitura obrigatória de agentes.
- Formalizado que todo PR futuro deve atualizar `PROJECT_LOG.md` e `docs/IMPLEMENTATION_STATUS.md`.
- Mantido `PROJECT_LOG.md` como log operacional/histórico curto.

### Testes

- [x] Documento criado diretamente na `dev`.
- [x] `PROJECT_LOG.md` atualizado com link e regra de manutenção.
- [ ] Unity não executado; alteração é documental.

### Pendências / riscos

- Opcional: reforçar a regra também em `AGENTS.md` e `CLAUDE.md`.
- O status de Cave/Combat básico foi mantido conforme `PROJECT_LOG.md`; validar código/Unity antes de marcar qualquer avanço além de MVP básico.

### Próximo passo recomendado

- Iniciar FASE9F-A — Cave Procedural Foundation, começando pelo PR-170.

---

## 2026-05-20 — Política de evolução de specs

**Responsável:** ChatGPT  
**Branch:** dev  
**Escopo:** registrar regra para permitir implementação paralela e refinamento de novas specs sem reescrever specs antigas.

### Alterações

- Criado `docs/SPEC_EVOLUTION_POLICY_v1.0.md`.
- Atualizado `PROJECT_LOG.md` para apontar a política como leitura obrigatória quando o trabalho tocar specs.
- Formalizado que specs aprovadas são baseline imutável.
- Formalizado que mudanças futuras entram como nova spec, amendment, correction ou errata.
- Formalizado que PR iniciado segue a spec vigente no início, salvo bug crítico ou decisão humana explícita.

### Testes

- [x] Política criada no repo.
- [x] `PROJECT_LOG.md` atualizado com resumo e link.
- [ ] Unity não executado; alteração é documental.

### Pendências / riscos

- Opcional: adicionar link explícito para esta política em `AGENTS.md` e `CLAUDE.md` em uma próxima sync documental.

### Próximo passo recomendado

- Implementação em outro chat pode seguir FASE9F.
- Este chat pode continuar escrevendo a próxima spec.

---

## 2026-05-20 — Split operacional do PROJECT_LOG

**Responsável:** ChatGPT  
**Branch:** dev  
**Escopo:** reduzir `PROJECT_LOG.md` para handoff operacional curto e arquivar histórico completo.

### Alterações

- Criado archive completo em `docs/logs/PROJECT_LOG_ARCHIVE_2026-05-18_FULL_BEFORE_SPLIT.md` reaproveitando o blob exato do `PROJECT_LOG.md` anterior.
- Substituído `PROJECT_LOG.md` por versão operacional curta.
- Mantidos links para specs FASE9E e FASE9F.
- Próximo passo recomendado atualizado para PR-170 da FASE9F.

### Testes

- [x] Archive criado a partir do blob antigo do `PROJECT_LOG.md`.
- [x] Novo `PROJECT_LOG.md` mantém handoff, decisões e próximos passos.
- [ ] Unity não executado; alteração é documental.

### Pendências / riscos

- Validar no GitHub se o archive aparece corretamente em `docs/logs/`.
- Próximas entradas devem ser curtas; logs extensos devem ir para novos archives.

### Próximo passo recomendado

- Iniciar PR-170 — Cave procedural contracts.
