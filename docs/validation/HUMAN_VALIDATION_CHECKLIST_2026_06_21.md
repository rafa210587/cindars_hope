# Checklist de Validação Humana — Estado Atual (2026-06-21)

> Roteiro passo a passo para validar em Play Mode as ~73 specs FABLE implementadas (status `BUILD_VALIDATED` / `_WITH_WARNINGS`, Phase 2-3 diferida). Execute **na ordem das fases**: cada fase é pré-requisito da seguinte. Marque `[x]` ao passar; anote o que falhar.
>
> Cenários detalhados por spec vivem em `docs/validation/playmode/*_human_test_scenario.md` — esta lista os referencia onde aplicável.
>
> Convenção: **Ação → Esperado**. Se um passo falhar, pare a fase, anote (spec + sintoma) e reporte.

---

## FASE 0 — Pré-voo (preparar o projeto e limpar o estado)

Objetivo: deixar dados/cenas consistentes antes de qualquer teste. Rode os menus **nesta ordem**.

- [ ] **Reparar primeiro** → menu `CindarsHope/Reparar e Reconstruir`. **Esperado:** loga `[Reparar] Asset duplicado/legado DELETADO: ...` (4×: Item_Cenoura/Trigo/Semente_Cenoura/Semente_Trigo) e recria o StatusEffectDatabase se preciso.
- [ ] **Inicializar** → `CindarsHope/Inicializar Projeto`. **Esperado:** gera dados → recria as 3 cenas → religa diálogos → registra build scenes. Diálogo final "X OK, Y falhas".
- [ ] **Validar** → `CindarsHope/Validar Projeto`. **Esperado:** `Duplicate=0` na ItemDatabase. (Erros de `WEAPON_ITEM_NO_WEAPON_ID`/`MAGIC_ITEM_NO_SPELL_ID`/loot/projectile são **débito de wiring de conteúdo conhecido** — anotar, não bloqueia o smoke.)
- [ ] **Console limpo ao abrir cena** → abra a CaveScene. **Esperado:** SEM enxurrada de "There are no audio listeners" e SEM flood de `CombatLog:` (gate `CombatLog.Verbose=false`). Só logs pontuais + warnings reais.
- [ ] Confirmar **3 cenas no Build Settings** (File ▸ Build Settings): FarmScene, TownScene, CaveScene presentes.

> ⚠️ Se `Duplicate` ≠ 0 após o passo 1+3, pare: a deleção foi bloqueada (version control no asset). Reporte.

---

## FASE 1 — Boot & Smoke (cada cena sobe sem erro)

Para **cada** cena (Farm, Town, Cave): abrir → Play.

- [ ] **Sem erros vermelhos** no Console ao entrar em Play (warnings de conteúdo conhecido OK).
- [ ] **Player aparece** e move com WASD/setas.
- [ ] **HUD inicial** aparece (vida/stamina/relógio). *(F14/F20 — pode estar em estado parcial se o canvas não foi wired; anotar.)*
- [ ] **Dicas de onboarding** aparecem 1× num save novo ("WASD para mover", etc.) — `docs/validation/playmode/fable_62_human_test_scenario.md`.
- [ ] Sai do Play sem exceção.

---

## FASE 2 — Loops críticos (o coração do jogo)

### 2A — Fazenda
- [ ] Plantar → regar → (avançar dia) → colher um crop. **Esperado:** item de colheita no inventário.
- [ ] **Metas diárias** atualizam no widget de HUD ao cumprir uma ação — `fable_65`.
- [ ] **Shipping/forrageamento overnight**: depositar item, dormir, conferir pagamento no dia seguinte.
- [ ] **Dormir** avança o dia e restaura needs (fadiga/stamina) — `player-needs-survival`.

### 2B — Cidade
- [ ] Falar com um NPC → árvore de diálogo abre, fecha com Esc/back.
- [ ] **Loja**: comprar e vender um item; ouro debita/credita certo.
- [ ] **Serviço único de NPC** (ex.: reparo) funciona — `fable_25`.
- [ ] **Presente** a um NPC muda amizade conforme o gosto — `docs/validation/playmode/fable_72_human_test_scenario.md`.

### 2C — Caverna (loop de risco)
- [ ] Entrar na caverna → nível gera, inimigos aparecem, player no spawn.
- [ ] **Combate**: atacar inimigo, tomar dano, matar; loot dropa.
- [ ] **Feel** (F71): hit-stop + screen shake ao acertar — `docs/validation/playmode/fable_71_human_test_scenario.md`.
- [ ] **Sair e revisitar o mesmo nível (mesmo seed)** → layout/inimigos/recursos **idênticos** (cave stable-run, ADR-0005) — `docs/validation/playmode/fable_44_human_test_scenario.md`.
- [ ] **Morte na caverna** → tela de morte + recuperação de corpo no respawn — `docs/validation/playmode/fable_64_human_test_scenario.md` + `fable_66`.

---

## FASE 3 — Validação por sistema (referência aos cenários escritos)

### Combate & progressão
- [ ] Status effects aplicam e expiram (burn/bleed/poison/stun…) — `fable_01`.
- [ ] Ações de arma + stats derivados; perfect block/parry — `fable_02`, `fable_27`.
- [ ] Sprint consome stamina — `fable_69`.
- [ ] Arco + flechas elementais — `fable_48`.
- [ ] Inimigos: pack/elite/afixos e bosses por fase — `fable_04`, `fable_24`, `fable_05`.
- [ ] XP/level sobem; classe inferida reflete o estilo — `fable_42`, `fable_39`.

### Magia, equipamento, itens
- [ ] Aprender magia, lançar (formas/mira) — `fable_07`, `fable_08`.
- [ ] Itens mágicos não-identificados → identificar — `fable_31`.
- [ ] Durabilidade cai com uso; reparo/upgrade — `equipment-durability-repair`.
- [ ] Forja de têmpera / crafting high-tier — `fable_22`, `fable_49`.

### UI (telas) — referência F14
- [ ] Painel único: abas Inventário/Skills/Quests/Bestiário/Mapa/Calendário/Sistema; Tab cicla; Esc fecha; **player não move com modal aberto** — `docs/validation/playmode/fable_14_human_test_scenario.md`.
- [ ] **Bestiário** codex: fichas gated por descoberta — `docs/validation/playmode/fable_45_human_test_scenario.md`.
- [ ] **Minimapa** com fog-of-war — `docs/validation/playmode/fable_38_human_test_scenario.md`.
- [ ] **Calendário/relógio** no HUD + detalhe do dia — `docs/validation/playmode/fable_20_human_test_scenario.md`.
- [ ] **Aba Sistema** + tela de título (Salvar/Carregar/Volume/Sair) — `docs/validation/playmode/fable_56_human_test_scenario.md`.

### Quests & narrativa
- [ ] **Intro de new game** + carta na cama → engata a main quest — `docs/validation/playmode/fable_63_human_test_scenario.md`.
- [ ] **Main quest Ato 1** jogável (A Fonte do Esquecimento) — `fable_10`.
- [ ] **Atos 2-4** progridem; Vaelrion no Ato 2; fragmentos integram — `fable_36`.
- [ ] **Endgame Ato 5**: escolha final + epílogos — `fable_43`.
- [ ] **Side quests** por NPC (2 levas) + rivalidade Sethra↔Yael — `fable_35`, `fable_70`.
- [ ] **Secretas de caverna** (goblin/warden/ovo) — `fable_52`.
- [ ] **Romance**: estágio sobe por amizade; limite de 2 parceiros — `fable_46`.

### Mundo, NPC, tempo
- [ ] Interiores/portas/horários da cidade — `docs/validation/playmode/fable_11_human_test_scenario.md`.
- [ ] Aniversários/estalagem/reputação — `fable_57`.
- [ ] Festivais/eventos lunares no calendário — `fable_37`, `fable_53`.
- [ ] **Marcas dos deuses/altares** (oração 1×/dia) — `fable_68` *(placement de altar pode estar pendente de wiring — anotar)*.
- [ ] Áudio: SFX em hits/colheita/UI; música muda em combate — `fable_58`.

---

## FASE 4 — Persistência & regressão

- [ ] **Save → carregar**: posição, inventário, ouro, dia, quests, amizade preservados.
- [ ] **Idempotência de recompensa**: completar quest, salvar, recarregar → recompensa **não** duplica.
- [ ] **Cave stable-run**: novo save = novo seed; revisita no mesmo run = igual; KO/death pode rerollar.
- [ ] **Recuperação de corpo** após morte funciona no reload.
- [ ] Sem refs Unity quebradas no save (carregar save antigo não dá NRE).

---

## FASE 5 — Aceitação final (build)

- [ ] `CindarsHope/Build Standalone Windows` gera o `.exe` sem erro.
- [ ] Rodar o `.exe`: abre no título → Novo Jogo → joga 5 min cobrindo Farm + Town + Cave sem crash.
- [ ] Confirmar identidade: nome do produto "Cindar's Hope", versão; ícone (débito de arte conhecido).

---

## Débitos conhecidos (NÃO são falhas de validação — já mapeados)

- **Wiring de conteúdo**: vários itens de arma/magia sem `WeaponId`/`SpellId`; 1 loot table de boss e 2 projectile prefabs ausentes; 1 NPC sem GiftPreferences. (Validar Projeto reporta.)
- **Placement de cena pendente**: altares (F68), e binding fino de alguns canvases/widgets — onde o cenário disser "DEFERRED_UI_VISUAL".
- **~25 logs `[XxxBootstrap]`** (1× no boot) e alguns serviços de caverna ainda logam — cosmético.
- **`ValidateSceneTransitions`** acusa "Unloading the last loaded scene" — bug do validador, não do jogo.

> Registre cada falha real como: `FASE X / spec_NN / sintoma / passos pra reproduzir`. Specs só promovem para `implementados/` após a fase relevante passar.

---

*Gerado: 2026-06-21. Base: 73 specs FABLE BUILD_VALIDATED + fixes de estabilização (dedup de itens, AudioListener, gate de log, consolidação de menus).*
