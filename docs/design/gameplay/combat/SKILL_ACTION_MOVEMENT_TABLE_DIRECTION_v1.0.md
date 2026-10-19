# Cindar's Hope — Skill Action Movement Table Direction v1.1

> **Status:** documento canônico de execução física das skills ativas
> **Local:** `docs/design/gameplay/combat/SKILL_ACTION_MOVEMENT_TABLE_DIRECTION_v1.0.md`
> **Depende de:**
> - `PLAYER_SKILL_TREES_DIRECTION.md` (a LISTA de skills e ranks — vence em conteúdo)
> - `COMBAT_CORE_DIRECTION.md` (custos, buffers, % de movimento durante ações — vence em regra)
> - `docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md` (Q7: deslocamento não atravessa; só Dodge)
> **Função:** dar a cada skill ATIVA o seu corpo físico — telegraph, deslocamento, shape,
> tempo de execução, recovery — para que as specs de executores e a futura animação 2D
> tenham um contrato único.
> **Não é spec implementável.**

---

## 0. Regras gerais de execução

```text
1. NENHUMA skill com deslocamento atravessa inimigos ou paredes — o lunge para na colisão,
   sem dano de impacto. SOMENTE o Dodge atravessa corpos (decisão Q7.2).
2. Telegraph mínimo: flash no sprite do caster + outline da área (cor por categoria:
   físico âmbar, magia azul, suporte verde). Sem arte final, o outline é o telegraph.
3. Velocidade de movimento DURANTE a execução segue a tabela do Combat Core §15.
4. Buffer de input: 0.10-0.18s (canon). Cancelar durante o telegraph devolve 50% do custo;
   após o início da execução, não cancela.
5. Cooldowns finais pertencem aos executores (specs F); faixas: golpes 2.5-9s,
   utilidades 8-15s, restauros 30-60s.
6. Modal aberto bloqueia tudo (invariante do projeto).
```

---

# PARTE A — Tabela mestra

## 1. Skills ativas das árvores canônicas

| Skill (árvore/tier) | Telegraph | Deslocamento | Shape / alcance | Execução | Recovery | Custo |
|---|---|---|---|---|---|---|
| Corte Amplo (Melee T2) | 0.15s | — | arco 140° × 1.3 tiles | 0.35s | 0.30s | 22 STA |
| Golpe de Ruptura (Melee T4) | 0.30s | lunge 1.5 tiles | círculo 1.0 no destino | 0.45s | 0.50s | 35 STA |
| Contra-Ataque (Melee T3, proc) | flash azul | step 0.5 | alvo único frontal | 0.25s | 0.15s | grátis (janela 1.5s pós-perfect-block) |
| Marcador de Presa (Ranged T1) | 0.10s | — | projétil 8 tiles · 9 t/s | 0.20s | 0.20s | 10 STA |
| Disparo Carregado (Ranged T1) | charge 0.65-1.10s | recuo 0.3 | projétil 9 tiles · 12 t/s | no release | 0.40s | 38 STA |
| Disparo de Interrupção (Ranged T3) | 0.10s | — | projétil RÁPIDO 8 tiles · 16 t/s | 0.15s | 0.35s | 25 STA |
| Flecha Perfurante (Ranged T3) | 0.25s | — | linha 6 tiles · pierce 2/3/4 | 0.30s | 0.45s | 30 STA |
| Armadilha de Caçador (Ranged T3) | 0.20s (plantio) | — | trap 0.8 tile (arma em 0.5s) | 0.30s | 0.25s | 20 STA |
| Projétil Arcano (Magic T1) | cast 0.30s | — | bolt 7 tiles · 8 t/s | no release | 0.25s | 8 MP |
| Selo de Proteção (Magic T2) | cast 0.40s | — | self · barreira 2/3/4s | — | 0.30s | 18 MP |
| Toque Restaurador (Magic T3) | cast 0.80s **interrompível** | — | self · cura % por rank | — | 0.40s | 30 MP |
| Capstones Anya/Senya (proc) | aura 0.5s | — | self buff | — | — | gatilho: 35% do MP máx |
| Descanso Curto (Survival T3) | canal 2.0s (quebra com dano) | — | self | — | — | grátis · fora de combate |
| Forja Viva de Thoren (Crafting, proc) | brilho na bancada | — | crafting | — | — | 1×/dia |

*(Afinidades elementais, especializações e passivas não têm linha própria: modificam as
ativas/ataques existentes. A lista completa de skills e ranks vive na direction de skill trees.)*

## 2. Leitura por categoria

```text
GOLPES (Corte Amplo, Ruptura): telegraph curto, recovery honesto — o risco mora no recovery.
PROJÉTEIS DE TÉCNICA: a Interrupção é a mais rápida do jogo (16 t/s) — essa é a função dela.
CARREGADOS: charge segura o jogador (movimento 0-25%); soltar cedo = versão fraca.
CANALIZADOS (Toque, Descanso): pagam em vulnerabilidade — interromper é counterplay do inimigo.
PROCS (Contra-Ataque, capstones, Thoren): nunca custam input extra; custam CONDIÇÃO.
```

---

# PARTE B — Interação com o resto do combate

## 3. Deslocamento e colisão

```text
Lunges: burst de 12 t/s até o destino; param em qualquer colisão (corpo ou parede), sem
dano de parede e sem empurrar o alvo além do knockback declarado do golpe.
Trap/zonas: snap ao grid de tiles (legibilidade), nunca dentro de collider.
```

## 4. Skills vs janelas de vulnerabilidade

```text
Toda skill de golpe respeita o sistema de janelas: dano em CriticalWindow segue a regra
canônica de crítico (chance + janelas que garantem). Skills NÃO abrem janelas por padrão —
abrir janela é privilégio de posture break, perfect block e mecânica de inimigo.
```

---

# PARTE C — Decisões fechadas

```text
Só o Dodge atravessa corpos; nenhum lunge de skill atravessa nada.
Telegraph mínimo por outline colorido até a fase de arte.
Cancelamento só no telegraph, devolvendo 50% do custo.
Disparo de Interrupção é o projétil mais rápido do jogador (16 t/s).
Procs não consomem input; consomem condição.
```

# PARTE D — Pendências

```text
Tempos finais por RANK (ranks altos reduzem recovery — interpolar nos executores).
SFX/VFX por categoria na fase de arte (cores do telegraph já reservadas).
Tabela equivalente para as ações dos COMPANIONS quando a WAVE 14 for promovida.
```
