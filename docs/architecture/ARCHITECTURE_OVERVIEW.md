# Architecture Overview — Cindar's Hope

> Panorama geral da arquitetura e do ferramental que usamos. Visão de alto nível — sem detalhe de
> código interno. Para contratos autoritativos ver `docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_*`
> e decisões em `docs/decisions/ADR-*`.

**Stack:** Unity 6000.4.7f1 · C# · 2D pixel art · RPG + farm sim. Mundo: Vaalara / Cindar's Hope / Dornecia.

---

## Visão em uma frase

Conteúdo é **gerado em editor-time** por geradores idempotentes → o **runtime** roda sobre um
event bus + bootstrap central, organizado por domínios de gameplay → tudo é **persistido** por seções →
e cada mudança é **governada** por specs, um harness de regras/agents e um gate de validação de build.

---

## Pilares de arquitetura (o que usamos e por quê)

| Pilar | O que usamos | Por quê |
|---|---|---|
| **Engine** | Unity LTS (6000.4.7f1), C#, render 2D pixel art | Base do jogo |
| **Comunicação** | Event bus central (publish/subscribe tipado) | Sistemas não se conhecem; zero acoplamento direto |
| **Composição** | Bootstrap central (service locator) + wiring por cena | Um ponto de inicialização; dependências explícitas |
| **Dados de jogo** | Catálogos em ScriptableObject indexados por ID estável | Conteúdo data-driven, editável sem recompilar |
| **Persistência** | Save por seções (um provider por domínio) + migrations versionadas | Save modular e evolutivo, sem refs de engine |
| **Conteúdo** | Geração editor-time via 3 comandos canônicos | "Rodar do zero" materializa todos os dados e cenas |
| **Arte** | Pipeline de sprites por IA (local + híbrido) | Produção de arte sem artista dedicado |

---

## As 5 camadas

```
1 · GERAÇÃO DE CONTEÚDO (editor-time)
        │ materializa dados + cenas
        ▼
2 · RUNTIME: bootstrap + event bus ──▶ 3 · DOMÍNIOS DE GAMEPLAY
        │                                    │
        ▼                                    ▼
4 · PERSISTÊNCIA (save por seções) ◀──── cada domínio expõe sua seção

5 · GOVERNANÇA (specs + harness + validação) → controla TODA mudança
```

### 1 · Geração de conteúdo (editor-time)
Tudo passa por **3 comandos canônicos** num orquestrador único (sem menu avulso por gerador):
- **Inicializar Projeto** — gera todos os dados (itens, inimigos, skills, loot, etc.) e recria as 3 cenas (Farm/Town/Cave).
- **Validar Projeto** — checagens somente-leitura (IDs, refs, catálogos, ranges).
- **Reparar e Reconstruir** — recuperação: corrige nulls, duplicados e DBs quebrados.

**Pipeline de arte:** prompts → geração por IA (SDXL local na AMD, ou híbrido GPT+rembg) → pós-processamento (corte/paleta/escala) → import no Unity.

### 2 · Runtime — boot & comunicação
- **Bootstrap central:** inicializa e segura todos os managers/databases globais (persistente entre cenas).
- **Event bus:** única via de comunicação de gameplay; eventos carregam só dados puros.
- **Wiring por cena:** cada cena se auto-conecta ao bootstrap depois de carregar, sem busca global.

### 3 · Domínios de gameplay
Organizado por domínio, cada um como adapter fino sobre lógica C# pura:
Player & Needs · Combat · Cave (run procedural) · Farm · Inventory & Equipment · NPC & City ·
Quests · Economy & Crafting · Skills & Magic · UI/HUD · World & Time · Audio & Bestiary.
Dados de cada domínio vêm de catálogos ScriptableObject com IDs estáveis.

### 4 · Persistência
Save central fatiado em seções (uma por domínio), serializado em JSON versionado, com migrations entre versões. DTOs só com tipos simples + IDs.

### 5 · Governança, specs & validação
- **Specs** (formato SpecKit) são o contrato de execução; só promovem com evidência.
- **Harness `.claude/`** — skills (workflows), rules (invariantes), agents (papéis), commands e hooks (enforcement mecânico: bloqueiam git destrutivo, YAML manual, secrets, APIs proibidas).
- **Truth-gate de build** — `run_strict_validation.ps1`: compila os dois assemblies, valida docs e roda testes EditMode. Build com sucesso = exit 0, ou não passou.
- **Roteamento de trabalho:** decisão/design no loop principal (Opus); execução delegada a Sonnet; exploração read-only em Haiku.

#### Harness em números (2026-06-29)

| Artefato | Qtde | Papel |
|---|---:|---|
| Skills | 62 | Workflows on-demand (carregados só quando a tarefa casa) |
| Rules | 21 arquivos | 20 invariantes ativas + RULES.md índice (stubs históricos removidos 2026-06-29; ADRs/game_rules apontam para a rule consolidada) |
| Commands | 16 | Fluxos nomeados (start/implement/validate/finish spec, audit, etc.) |
| Hooks | 19 | Guard-rails PowerShell (PreToolUse / PostToolUse / Stop) |
| Agents | 10 | Papéis especializados delegáveis (reviewers, implementer, validator…) |

O design é deliberadamente *lazy*: cada rule mantém só o invariante carregado em todo turno e empurra o
detalhe verboso para a skill correspondente, que entra em contexto apenas quando relevante. Manutenção de
higiene (reconciliar índices, podar stubs) é feita via `/audit-harness` + skill `harness-audit`.

#### Agents (papéis delegáveis)

10 papéis especializados delegáveis. Princípio central: **separar quem executa de quem julga** — o
implementador nunca é o juiz do próprio trabalho.

| Grupo | Agents | Papel |
|---|---|---|
| **Executores** (Sonnet) | `spec-implementer` · `asset-wiring-specialist` · `bugfix-investigator` · `test-author` | Implementam spec, fazem wiring de assets, corrigem bug, escrevem EditMode tests |
| **Revisores/auditores** (audit-only, nunca editam) | `architecture-reviewer` · `game-design-reviewer` · `non-regression-auditor` · `performance-auditor` | Olhar independente: arquitetura (pré-wave), design de gameplay (pré-implementação), regressão (pré-closeout), performance |
| **Validação & governança** (Sonnet) | `unity-validator` · `docs-curator` | Roda validações com triagem honesta (PASS/FAIL/NOT RUN); governança de docs |

**Por que importam:** dão independência adversarial antes de promover (defesa contra drift e contra
"narrou e não fez" — casa com a regra *resultado de subagent não é evidência*); encadeiam um pipeline de
qualidade (design-review → implementa → valida → audita → fecha); e o roteamento de modelo mantém o custo
proporcional — execução em Sonnet, julgamento de arquitetura/design herda Opus. `architecture-reviewer` e
`game-design-reviewer` herdam Opus; os demais rodam em Sonnet.

#### Specs executadas

| Conjunto | Qtde |
|---|---:|
| `.specs/implementados/` | **239** (75 originais + 164 promovidas em 2026-06-29) |
| Restantes em `a_implementar/` | wave/fable não build-validated (18 revertidas pós-auditoria) · `closeout_mvp/` 12 · `features_futuras/` 53 · 5 soltas |

> Promovidas = build-validated (compile dos 2 assemblies + validações + execution report por spec).
> **Não** inclui Play Mode — a validação de gameplay / aceitação final segue deferida para ação humana no Unity.

---

## Pipeline de sprites (como fizemos)

Não temos artista dedicado, então construímos um pipeline próprio de arte por IA + pós-produção local.
**Regra autoral:** a IA chega perto; o retoque/import garante identidade própria, paleta do projeto e
nada copiado de outro jogo.

### O que tentamos e o que ficou
Geração de pixel art **estática** com IA funciona. Para **animação**, o SDXL local (ComfyUI, na GPU AMD)
não dá consistência frame-a-frame. O que fechou de ponta a ponta foi um pipeline **híbrido**: master
sheet gerado na nuvem (ChatGPT/DALL·E) + **pós-produção 100% local**.

### O fluxo (provado no player, 8 direções)
```
1. Gerar master sheet na IA (descrição conversacional + sheet aprovada como referência de estilo)
2. Tirar o fundo            → rembg (modelo u2net) — keying por cor falha no gradiente que a IA devolve
3. Fatiar os frames         → slice_sheet.py (detecta bandas por gap, downscale LANCZOS)
4. Ordenar por direção      → manual, com regra de espelho (down/up dobram com flip; perfil não espelha)
5. Exportar por direção     → célula uniforme, ancorada bottom-center, transparente
6. Importar no Unity        → preset pixel art (Point, sem compressão; 128 PPU no player)
7. Animar                   → script-driven (troca de sprite por direção/input), sem Animator YAML
```

### Ferramentas que usamos (por etapa)

| Etapa | Ferramenta | Papel |
|---|---|---|
| Prompts / manifesto | `prompts.json` + `manifest.*.json` | Entidades, paletas hex, prompts positivo/negativo |
| Geração (nuvem) | ChatGPT / DALL·E, operado via **Chrome MCP** | Master sheet do método híbrido (o que fechou) |
| Geração (local) | **ComfyUI / SDXL** (Amuse) na GPU **AMD** | Alternativa local; boa p/ estática, fraca p/ animação |
| Placeholder | **Aseprite** headless + **Lua** (`gen_sprite.lua`) | Sheets de rascunho por bodytype enquanto a arte final não vem |
| Remoção de fundo | **rembg** (modelo u2net), via Python (PIL/numpy/onnxruntime) | Tira o gradiente que a IA devolve |
| Pós-processamento | **Python**: `slice_sheet.py` (corte por banda + downscale LANCZOS), `export_*.py` (ordem/flip por direção), scripts de paleta | Fatiar, ordenar, padronizar célula |
| Orquestração | **PowerShell**: `comfy_gen.ps1`, `supervisor.ps1` | Rodar geração em lote |
| Import + animação | **Unity**: `GeneratedSpriteImporter.cs` (128 PPU, Point, bottom-center) + `PlayerWalkAnimator.cs` (animação por script, sem Animator YAML) | Trazer pro jogo e animar |

**Usamos pronto:** a ferramenta de imagem da IA e o `rembg`. **Construímos:** os scripts de corte/ordenação/
export, os manifestos/prompts, o importador automático no Unity e o animador por script — método
documentado para reuso em NPCs/monstros.

> Detalhe operacional completo: `tools/aseprite/WALK_PIPELINE.md` (animação) e
> `docs/operations/SPRITE_PIPELINE_AI_ASEPRITE_v1.0.md` (regras autorais, paletas, tamanhos, import).

---

## Pontos fortes do sistema (metodologia)

O que torna esta arquitetura e o nosso processo de trabalho sólidos:

- **Desacoplamento total (event bus).** Sistemas não se conhecem — comunicam por eventos de dados puros.
  Adicionar uma feature (som, HUD, quest) é assinar um evento, sem tocar no código que o publica.
- **Conteúdo data-driven.** Itens, inimigos, skills, loot vivem em catálogos ScriptableObject com IDs
  estáveis. Dá para gerar e editar conteúdo em massa sem recompilar código.
- **Geração reprodutível "do zero".** Três comandos idempotentes materializam todos os dados e as 3 cenas.
  "Rodar o projeto do zero" sempre produz o mesmo estado — nada de asset órfão.
- **Persistência modular.** O save é fatiado por domínio (um provider por seção) com migrations
  versionadas: adicionar um domínio ao save é isolado e não quebra saves antigos.
- **Validação honesta (truth-gate).** Build com sucesso = exit 0, ou não passou. Os níveis de validação
  (compile / Unity / Play Mode) nunca se misturam — sem claim prematuro de "pronto".
- **Enforcement mecânico.** Hooks bloqueiam classes inteiras de erro *antes* de acontecerem (YAML manual,
  git destrutivo, secrets, APIs de busca global proibidas), em vez de depender de disciplina.
- **Trabalho dirigido por spec.** Cada mudança tem um contrato (spec) e evidência; nada vai para
  `implementados/` sem o nível de evidência exigido.
- **Roteamento de modelo por tarefa.** Opus decide/desenha, Sonnet executa, Haiku busca — custo
  proporcional ao trabalho, com verificação do resultado pelo orquestrador.
- **Harness *lazy*.** Cada rule mantém só o invariante carregado em todo turno e empurra o detalhe para a
  skill, que entra em contexto só quando a tarefa casa — contexto enxuto sem perder profundidade.

---

## Invariantes não-negociáveis

- Comunicação de gameplay **apenas** via event bus (sem chamadas diretas entre sistemas).
- Sem busca global de objetos em runtime.
- Save sem referências de engine; IDs de domínio estáveis e versionados.
- Geração de conteúdo só pelos 3 comandos canônicos; sem edição manual de YAML.
- Build válido = exit code 0. Commits em português.

---

*Referência de alto nível. Contratos detalhados: `CORE_CONTRACTS_EVENTS_SAVE_IDS_*`. Decisões: `docs/decisions/ADR-*`.*
