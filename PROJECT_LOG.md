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

Começar **FASE9E-D — HUD Debug v2 + Tool Gating + Attribute Allocation** antes de retomar Cave Procedural.

1. PR-132 — DebugHud layout v2.
2. PR-133 — Action feedback event.
3. PR-134 — Tool gating contracts.
4. PR-135 — Tool gating para árvore e pesca.
5. PR-136 — Hotbar seed gating para FarmPlot.
6. PR-137 — Attribute allocation debug MVP.
7. PR-138 — DebugHud progression/tool/hotbar polish.
8. PR-139 — Handoff para Cave Procedural.

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
---

## 2026-05-20 - PR-101 a PR-130 reconciliacao consolidada pos PR-099

**Responsavel:** Codex/ChatGPT
**Branch:** `feature/pr-101-reconciliar-branches-pr099`
**Escopo:** executar em um unico commit, por decisao humana explicita, a reconciliacao PR-101 a PR-130 antes de retomar Cave Procedural.

### Alteracoes

- Criada auditoria `docs/audits/PR101_PR099_BRANCH_RECONCILIATION.md`.
- Criados docs `docs/audits/PR116_ITEM_ID_AUDIT.md`, `docs/audits/PR130_RECONCILIACAO_HANDOFF.md` e `docs/validation/SMOKE_TEST_FARM_TOWN_CAVE_MVP.md`.
- Aplicado hardening estatico em Combat: `EnemyHealth`, `EnemyContactDamage`, `EnemyChaseController`, `HitFlashController`, `KnockbackController`, `EnemyDropSpawner` e `EnemyDataSO`.
- Adicionados contratos MVP de dano, ferramentas, equipamento, hotbar e progressao.
- Integrado save/load simples para Equipment, Hotbar e PlayerProgression.
- Atualizados Bootstrap, geradores/instaladores de cena, DebugHud e validators para reconhecer o estado consolidado.

### Testes

- [x] Revisao estatica de escopo e arquivos alterados.
- [x] Metas Unity adicionadas para scripts/pastas novos.
- [ ] Unity nao executado nesta sessao.
- [ ] Regeneracao de cenas nao executada nesta sessao.

### Pendencias / riscos

- Validar compilacao no Unity.
- Validar smoke test Farm/Town/Cave.
- `item_material_stone` e `ore_copper` seguem pendentes como assets/IDs futuros de Cave Resources.
- As cenas locais e `Assets/MobileDependencyResolver/**` permaneceram ignorados por instrucao humana.

### Proximo passo recomendado

- Validar este commit no Unity.
- Depois de aprovado/mergeado, retomar Cave Procedural Foundation como PR-131+.

---

## 2026-05-20 - PR-131 sync de tracking pos reconciliacao

**Responsavel:** Codex/ChatGPT
**Branch:** `feature/pr-131-sync-status-pos-reconciliacao`
**Escopo:** sincronizar tracking documental depois do consolidado PR-101 a PR-130 na `dev`.

### Alteracoes

- Atualizado `docs/IMPLEMENTATION_STATUS.md` para marcar PR-101 a PR-130 como implementado parcial.
- Equipment/Hotbar, Progression/LevelUp e Damage Formula MVP passaram de `Especificado` para `Implementado parcial`.
- Cave Procedural/Resources permaneceu como pendente.
- Atualizado handoff PR-130 para apontar a sequencia vigente PR-132 a PR-145.
- Atualizado este log com o proximo bloco recomendado da FASE9F-A.

### Testes

- [x] Revisao estatica documental.
- [ ] Unity nao executado; PR documental.

### Pendencias / riscos

- Validar Unity antes de avancar em PRs de codigo se houver erro vermelho local.
- Executar PR-132 antes de iniciar contratos procedurais.

### Proximo passo recomendado

- PR-132 - Pre-flight Unity hardening antes da Cave Procedural.

---

## 2026-05-20 - PR-131 sync de validacao HUD/tools/progressao

**Responsavel:** Codex/ChatGPT
**Branch:** `feature/pr-131-sync-validacao-hud-tools-progression`
**Escopo:** registrar estado real validado de HUD debug, hotbar, tools, progressao e cave fixed MVP antes de novas features.

### Alteracoes

- Criado `docs/audits/PR131_VALIDACAO_HUD_TOOLS_PROGRESSION.md`.
- Atualizado `docs/IMPLEMENTATION_STATUS.md` para registrar lacunas reais:
  - tool existe, mas ainda nao bloqueia arvore/pesca;
  - hotbar existe, mas plantio ainda nao usa slot selecionado;
  - XP/level/pontos existem, mas nao ha distribuicao debug de atributos;
  - cave ainda e fixed MVP, sem seed/procedural.
- Atualizado o proximo bloco recomendado para FASE9E-D PR-132 a PR-139 antes da Cave Procedural.

### Testes

- [x] Revisao estatica documental e inspeção dos arquivos relevantes.
- [ ] Unity nao executado; PR documental.

### Pendencias / riscos

- Validar no Unity o estado relatado antes de mergear se houver divergencia local.
- Cave Procedural deve aguardar o handoff PR-139.

### Proximo passo recomendado

- PR-132 - DebugHud layout v2.

---

## 2026-05-20 - PR-132-FIX HUD split + tool/hotbar gating

**Responsavel:** Codex/ChatGPT
**Branch:** `feature/pr-132-fix-hud-tool-hotbar-gating`
**Escopo:** corrigir implementacao parcial anterior de HUD debug, feedback de acoes, gating por ferramenta equipada e plantio por hotbar.

### Alteracoes

- Criado `PlayerActionFeedbackEvent` para mensagens temporarias de acoes bloqueadas.
- Reorganizado `DebugHud` em painel esquerdo de acoes e painel direito de informacoes.
- Adicionado status fixo da cave no HUD: `Cave: fixed MVP` e `Seed: unavailable`.
- Adicionados contratos `HasTool` e mensagem de ferramenta ausente em `EquipmentManager`.
- `TreeNode` agora exige `Axe/Basic` equipado antes de contabilizar hit.
- `FishingSpot` agora exige `FishingRod/Basic` equipado antes de adicionar peixe.
- `FarmPlot` agora usa o item selecionado na hotbar para plantar e bloqueia slot vazio/item nao-seed/seed ausente no inventario.

### Testes

- [x] Revisao estatica dos arquivos alterados.
- [x] Busca estatica por `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType`, `StreamingAssets` e dependencia nova de tag nos arquivos alterados.
- [ ] Unity nao executado nesta sessao.

### Pendencias / riscos

- Validar no Unity se o HUD fica bem posicionado em 1280x720 e nao sobrepoe conteudo relevante.
- Validar em Play Mode: ferramenta None/Axe/FishingRod, plantio por hotbar e mensagens temporarias.
- Este PR nao implementa distribuicao debug de atributos nem Cave Procedural.

### Proximo passo recomendado

- Validar PR-132-FIX no Unity.
- Depois seguir para distribuicao debug de atributos ou handoff FASE9E-D, conforme prioridade.
