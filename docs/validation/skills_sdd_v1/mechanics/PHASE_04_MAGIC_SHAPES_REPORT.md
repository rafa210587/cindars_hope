# Fase mecânica 4 — formas e suporte mágico

Data: 2026-09-10  
Spec: `spec_skills_14_magic_shapes_v1`  
Resultado técnico: PASS

## Resultado jogável

- Fagulha e Laço preservam a leitura de projétil; Laço distingue Chill leve do Slow forte de repetição.
- O relógio preciso do Chill remove seu token ao expirar ou ao fechar a janela do chefe, evitando desaceleração residual por arredondamento do ticker geral.
- Tokens de Chill carregam origem runtime: a skill reconhece Chill de magia comum e remove apenas o token que ela própria criou.
- Chama Breve usa cone de 70 graus, limite de quatro alvos, queda de 100/70% e refresh de Burn por alvo.
- Rajada Gélida abre três projéteis em leque e limita a dois impactos no mesmo alvo.
- Nuvem Tóxica cria uma zona de quatro pulsos globais sem dano retroativo na reentrada.
- Corrente escolhe saltos em ordem estável, respeita obstáculos e só causa postura extra em Constructos vulneráveis a Lightning.
- Guarda Elemental reduz dois impactos elementais diretos antes de Defense/resistência. DoT e físico não consomem carga. O Combat acessa o efeito por uma porta neutra em Foundation.
- Sigilos cria uma zona fixa de Slow, sem somar campos; elite e chefe respeitam os limites de duração e janela.
- Todos os custos, cooldowns, alcances, formas, intensidades e timings ficam nos dados gerados de `SkillActionSO`.

## Benchmark determinístico de equilíbrio

| Cenário | Ação | Direto + DoT | Ciclo | DPS | MP/s | Controle | TTK do cenário |
|---|---:|---:|---:|---:|---:|---:|---:|
| neutro 100 HP | Fagulha R5 | 20 + 0 | 3,43 s | 5,83 | 2,92 | 0 | 17,15 s |
| neutro 100 HP | Chama R3 | 12 + 8 | 3,20 s | 6,25 | 2,50 | 0 | 16,00 s |
| neutro 100 HP | Nuvem R5 | 48 + 12 | 9,20 s | 6,52 | 1,96 | 0 | 18,40 s |
| trio 300 HP total | Chama R3 | 28 + 24 | 3,20 s | 16,25 | 2,50 | 0 | 19,20 s |
| trio 300 HP total | Nuvem R5 | 144 + 36 | 9,20 s | 19,57 | 1,96 | 0 | 18,40 s |
| elite | Sigilos R5 | 0 | 9,20 s | 0 | 1,96 | 0,40 do cooldown | sem dano |
| chefe fora da janela | Sigilos R5 | 0 | 9,20 s | 0 | 1,96 | 0 | sem dano |
| chefe na janela | Sigilos R5 | 0 | 9,20 s | 0 | 1,96 | Slow 10% | sem dano |

Chama fica em 107,2% do DPS single da Fagulha, abaixo do teto de 110%. Nuvem fica em 111,8%, abaixo do teto de 115%. Fagulha gasta 2,92 MP/s e é sustentável com regeneração 5 MP/s; a rotação Fagulha + Chama + Laço + Nuvem drena a reserva. O teto de controle de elite fecha em 3,2/8 = 0,40, e chefe recebe zero controle fora da janela.

## Evidência fresca

- Catálogo/compile: `Logs/skills-phase14-generate9.log` — PASS, 66 nós, 5 árvores e 31 ações; novos scripts importados com `.meta`.
- EditMode de Skills: `Logs/skills-phase14-editmode9.xml` — PASS, 83 executados, 83 aprovados, 0 falhas. O output XML contém as linhas `MAGIC_BALANCE` derivadas dos assets gerados e das regras runtime.
- Regressão do status compartilhado: `Logs/skills-phase14-status-regression.xml` — PASS, 8 executados, 8 aprovados, 0 falhas.
- PlayMode integrado: `Logs/skills-phase14-integrated-playmode9.xml` — PASS, 35 executados, 35 aprovados, 0 falhas.
- Cobertura PlayMode: projéteis, cone, zonas, corrente, ward, controles, formas melee e ranged anteriores no runtime Unity.
- Revisão independente final: COMPLIANT; AC01–AC09 PASS, nenhum finding atual, risco de manutenção LOW.

## Limite da conclusão de equilíbrio

A árvore mágica está coerente com os envelopes aprovados, sem dominância single-target ou exploit conhecido de controle, recurso, sobreposição e reentrada. A comparação final entre todas as árvores permanece na Fase 21, com cenários comuns e builds completas.
