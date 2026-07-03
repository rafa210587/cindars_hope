# SPEC â€” Drafts de ADR para DÃ©bitos Estruturais Pesados (DOC-ONLY)

> **Spec ID:** `spec_codex_08_convergence_decisions`
> **Status:** A implementar
> **Wave:** WAVE CODEX CONVERGENCE â€” Honestidade de ValidaÃ§Ã£o
> **Priority:** P2
> **Type:** Governance / Docs
> **Domain:** Architecture / Docs
> **Parallelizable:** YES
> **Parallel group:** codex_convergence
> **Can run with:** spec_codex_01, spec_codex_02, spec_codex_03, spec_codex_04, spec_codex_05, spec_codex_06, spec_codex_07
> **Must not run with:** N/A
> **Repo lock scope:** `docs/decisions/**`
> **Depends on:**
> - Nenhuma
> **Depends on (Depende de):**
> - Nenhuma. Spec doc-only (`docs/decisions/**`); nÃ£o depende de nenhuma outra spec deste lote nem de cÃ³digo.
> **Blocks:**
> - Nenhuma
> **Blocks (Bloqueia):**
> - Nenhuma. Doc-only â€” nÃ£o bloqueia nenhuma outra spec do lote `codex_convergence`.
> **Scope:** Produzir 6 drafts de ADR (um por item pesado de dÃ©bito estrutural) com recomendaÃ§Ã£o, custo, risco e o que exige aprovaÃ§Ã£o humana explÃ­cita â€” nenhuma mudanÃ§a de cÃ³digo.
> **Out of scope:** Implementar qualquer uma das mudanÃ§as propostas; decidir por conta prÃ³pria em nome do humano nos itens marcados como "decisÃ£o humana obrigatÃ³ria".

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

A auditoria de convergÃªncia arquitetural (citada no prompt de geraÃ§Ã£o deste lote, cruzada com achados desta sessÃ£o) identificou 6 dÃ©bitos estruturais pesados que nenhuma das outras 7 specs deste lote resolve, porque cada um Ã© uma decisÃ£o de arquitetura com trade-offs â€” nÃ£o um bug pontual. Esta spec Ã© **doc-only**: produz o material de decisÃ£o (ADR draft) para cada um, sem tocar cÃ³digo, para que o humano decida antes de qualquer implementaÃ§Ã£o.

## 6. Problema

Sem um registro formal de decisÃ£o, esses 6 dÃ©bitos ficam apenas como observaÃ§Ãµes soltas de auditoria (memÃ³ria, comentÃ¡rios de cÃ³digo, conversas) â€” nÃ£o hÃ¡ um documento Ãºnico que o time possa revisar, aprovar ou rejeitar formalmente, e cada um tem risco/custo real o suficiente para exigir essa formalizaÃ§Ã£o antes de qualquer execuÃ§Ã£o.

## 7. Objetivo

Ao final desta spec, existem 6 arquivos ADR-draft em `docs/decisions/` (numeraÃ§Ã£o sequencial a partir de `ADR-0020`, prÃ³ximo nÃºmero livre confirmado nesta sessÃ£o â€” 19 ADRs existentes hoje, de `ADR-0001` a `ADR-0019`), cada um cobrindo um dos itens (a)-(f) do prompt de geraÃ§Ã£o, com recomendaÃ§Ã£o, custo, risco, e uma seÃ§Ã£o explÃ­cita "Requer decisÃ£o humana: SIM/NÃƒO, motivo".

## 8. Fontes obrigatÃ³rias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.claude/skills/decision-rule-extraction/SKILL.md
.claude/skills/runtime-bootstrap-pattern/SKILL.md
docs/decisions/ADR-0008-unity-yaml-editing-policy.md
```

## 9. Estado atual do repo (Phase 0 â€” auditado nesta sessÃ£o)

- `docs/decisions/` tem 19 ADRs hoje (`ADR-0001` a `ADR-0019`), confirmado via Glob. PrÃ³xima numeraÃ§Ã£o livre: `ADR-0020`.
- **(a) .asmdef incremental:** confirmado **zero arquivos `.asmdef`** em `Assets/**` (Glob `*.asmdef` retornou vazio). Todo o cÃ³digo roda no assembly implÃ­cito `Assembly-CSharp`/`Assembly-CSharp-Editor`. Introduzir `.asmdef` Ã© uma mudanÃ§a estrutural que afeta tempo de compilaÃ§Ã£o incremental e forÃ§a fronteiras explÃ­citas de dependÃªncia â€” candidato natural: `Core`/`Editor`/`Tests` primeiro (menor superfÃ­cie, jÃ¡ sÃ£o fisicamente segregados por pasta).
- **(b) Unity YAML fora do LFS:** `.gitattributes` hoje tem YAML da Unity (`.unity`/`.prefab`/`.asset`) em LFS â€” mudar isso reescreve histÃ³rico de tracking do Git (nÃ£o o conteÃºdo, mas como o Git armazena esses arquivos) e Ã© uma decisÃ£o que **ADR-0008** (`unity-yaml-editing-policy`) jÃ¡ toca parcialmente; este item deve referenciar/estender ADR-0008 em vez de duplicÃ¡-lo, ou ser um adendo a ele.
- **(c) ConsolidaÃ§Ã£o dos ~70 auto-bootstraps:** confirmado `[RuntimeInitializeOnLoadMethod]` em 70 pontos (contagem do prompt, coerente com o padrÃ£o `*RuntimeBootstrap` jÃ¡ documentado na skill `runtime-bootstrap-pattern`). Consolidar num composition root Ãºnico Ã© uma mudanÃ§a de startup order com risco real de regressÃ£o silenciosa (ordem de inicializaÃ§Ã£o implÃ­cita hoje via `RuntimeInitializeLoadType.AfterSceneLoad` pode ocultar dependÃªncias entre bootstraps). Deve conciliar explicitamente com a skill `runtime-bootstrap-pattern` existente (nÃ£o substituÃ­-la sem justificar).
- **(d) Floresta externa da TownScene â†’ Tilemap/chunks:** confirmado no prompt: `CreateMvpTownScene.cs` tem 3331 linhas e a `TownScene` materializada tem ~2826 GameObjects, dos quais ~1153 sÃ£o Ã¡rvores individuais. Migrar para Tilemap/chunked rendering Ã© uma mudanÃ§a de performance/arquitetura de cena que precisa preservar IDs/posiÃ§Ãµes existentes (rule `id-stability` + o trabalho recente de `spec_city_preservation_first_coherent_relayout`, que jÃ¡ documentou manifesto before/after de 497 Ã¡rvores â€” auditar se esse nÃºmero bateu com a contagem atual de ~1153 antes de escrever o ADR, citando a fonte de cada nÃºmero).
- **(e) MigraÃ§Ã£o IMGUIâ†’Canvas:** o projeto jÃ¡ fez parte dessa migraÃ§Ã£o (`DeathScreenCanvasController` substituiu `DeathScreenController` IMGUI via fable_64, confirmado nesta sessÃ£o). Restam outros IMGUI (`OnGUI` â€” 18 ocorrÃªncias confirmadas no prompt) a auditar/migrar; o ADR deve listar quantos ainda restam (Grep `OnGUI` em `Assets/_Game/Scripts/**` na Fase 0 de implementaÃ§Ã£o) e propor uma ordem de migraÃ§Ã£o incremental seguindo o precedente jÃ¡ bem-sucedido do death screen.
- **(f) Resources â†’ referÃªncias diretas/Addressables:** `Resources` folder confirmado em 54.6 MB (do prompt). Migrar para referÃªncias diretas ou Addressables reduz carregamento de assets nÃ£o usados e acoplamento a paths string, mas Addressables Ã© uma dependÃªncia de pacote nova (verificar se jÃ¡ estÃ¡ no `Packages/manifest.json` antes de recomendar â€” se nÃ£o estiver, recomendar referÃªncias diretas primeiro como passo 1, Addressables como passo 2 opcional).

## 10. User stories / engineering stories

```text
Como time, quero um ADR draft por dÃ©bito estrutural pesado, para decidir formalmente antes de qualquer execuÃ§Ã£o.
Como agente futuro, quero saber exatamente quais dessas decisÃµes exigem aprovaÃ§Ã£o humana explÃ­cita antes de tocar cÃ³digo.
```

## 11. Escopo

Inclui:
- 6 arquivos ADR-draft novos em `docs/decisions/`, numerados `ADR-0020` a `ADR-0025` (ou a numeraÃ§Ã£o livre real confirmada no momento da execuÃ§Ã£o, se outras specs/ADRs tiverem sido criados entre a geraÃ§Ã£o desta spec e sua execuÃ§Ã£o â€” reconfirmar o prÃ³ximo nÃºmero livre na Fase 0 de implementaÃ§Ã£o):
  - `ADR-0020-incremental-asmdef-adoption.md` (item a)
  - `ADR-0021-unity-yaml-lfs-removal.md` (item b, referenciando/estendendo ADR-0008)
  - `ADR-0022-runtime-bootstrap-composition-root.md` (item c)
  - `ADR-0023-townscene-forest-tilemap-chunking.md` (item d)
  - `ADR-0024-imgui-to-canvas-migration-remaining.md` (item e)
  - `ADR-0025-resources-to-direct-refs-addressables.md` (item f)
- Cada ADR segue o formato canÃ´nico jÃ¡ usado pelos ADRs existentes (auditar `ADR-0008` ou `ADR-0005` como referÃªncia de estrutura antes de escrever) e contÃ©m obrigatoriamente: contexto, decisÃ£o proposta (nÃ£o decidida â€” draft), alternativas consideradas, custo estimado, risco, e uma seÃ§Ã£o final `## Requer decisÃ£o humana` com SIM/NÃƒO + motivo.
- Nenhuma mudanÃ§a de cÃ³digo, `.unity`, `.prefab`, `.asset`, `Packages/`, `ProjectSettings/`.

Fora:
- Implementar qualquer uma das 6 mudanÃ§as.
- Marcar qualquer ADR como "aceito"/"decidido" â€” todos ficam como draft atÃ© aprovaÃ§Ã£o humana explÃ­cita.

## 12. Fora de escopo

```text
NÃ£o inclui: qualquer mudanÃ§a de cÃ³digo, cena, prefab, asset, package manifest ou project settings.
```

## 13. Regras de nÃ£o duplicaÃ§Ã£o

```text
NÃ£o duplicar ADR-0008 (Unity YAML policy) â€” o item (b) desta spec deve referenciÃ¡-lo/estendÃª-lo, nÃ£o recriar do zero.
NÃ£o recriar a skill runtime-bootstrap-pattern em forma de ADR â€” o item (c) deve conciliar com ela, citando-a.
```

## 14. CritÃ©rios de aceite

### 14.1 6 ADR drafts criados

- Os 6 arquivos existem em `docs/decisions/`, seguindo o formato canÃ´nico do projeto.
- EvidÃªncia esperada: listagem dos arquivos + trecho de cada um mostrando a seÃ§Ã£o "Requer decisÃ£o humana".

### 14.2 Nenhuma mudanÃ§a de cÃ³digo

- `git diff` (ou `git status`) confirma que sÃ³ arquivos sob `docs/decisions/` foram tocados.
- EvidÃªncia esperada: output de `git status --short`.

### 14.3 Docs validation

- `.\tools\docs\validate_docs.ps1` roda sem introduzir novos erros (os 6 ADRs seguem convenÃ§Ã£o de link/path).

---

# /speckit.plan

## 15. Arquitetura alvo

```text
docs/decisions/
  ADR-0020-incremental-asmdef-adoption.md
  ADR-0021-unity-yaml-lfs-removal.md
  ADR-0022-runtime-bootstrap-composition-root.md
  ADR-0023-townscene-forest-tilemap-chunking.md
  ADR-0024-imgui-to-canvas-migration-remaining.md
  ADR-0025-resources-to-direct-refs-addressables.md
```

## 16. Contratos, dados e eventos

### 16.1-16.5
N/A â€” spec doc-only, sem contratos de cÃ³digo/save/evento/UI.

## 17. Sistemas afetados

```text
Docs/governanÃ§a apenas (nenhum sistema de runtime tocado)
```

## 18. Arquivos permitidos

```text
docs/decisions/**
docs/validation/**
```

## 19. Arquivos proibidos

```text
Assets/**
Packages/**
ProjectSettings/**
.specs/** (exceto os Ã­ndices atualizados pelo processo de geraÃ§Ã£o de specs, fora desta spec em si)
```

## 20. EstratÃ©gia de implementaÃ§Ã£o

```md
### Fase 0 â€” Auditoria (concluÃ­da nesta spec; ver seÃ§Ã£o 9) + reconfirmar prÃ³ximo nÃºmero ADR livre
### Fase 1 â€” Escrever os 6 ADR drafts
### Fase 2 â€” Docs validation
### Fase 3 â€” RelatÃ³rio
```

## 21. Ordem de execucao (ordem segura)

```text
1. Reconfirmar prÃ³ximo nÃºmero ADR livre (Glob docs/decisions/ADR-*.md).
2. Escrever os 6 ADRs, um por um, seguindo o formato canÃ´nico existente.
3. Rodar docs validation.
4. Registrar relatÃ³rio.
```

## 22. ParalelizaÃ§Ã£o

```md
- Parallelizable: YES
- Parallel group: codex_convergence
- Can run with: demais specs do lote (nenhum overlap de arquivo â€” sÃ³ docs/decisions/)
- Must not run with: N/A
- Shared files/systems that require lock: nenhum
- Reason: doc-only, sem tocar Assets/
```

## 23. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? N/A
```

## 24. Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO
```

## 25. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: NO
Human validation timing: NOT REQUIRED
```

## 26. Riscos tÃ©cnicos

```text
Risco: os nÃºmeros de contagem citados no prompt (70 bootstraps, 2826 GameObjects, 54.6 MB Resources) podem ter mudado desde a auditoria original.
MitigaÃ§Ã£o: cada ADR deve re-confirmar a contagem relevante na Fase 0 de implementaÃ§Ã£o (Grep/du) e citar a data da re-confirmaÃ§Ã£o, nÃ£o copiar cegamente o nÃºmero do prompt.
```

## 27. Rollback

```text
Deletar os 6 arquivos ADR-draft criados.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 â€” Reconfirmar prÃ³ximo nÃºmero ADR livre.
- [ ] T002 â€” Escrever ADR-0020 (asmdef incremental).
- [ ] T003 â€” Escrever ADR-0021 (Unity YAML fora do LFS, referenciando ADR-0008).
- [ ] T004 â€” Escrever ADR-0022 (composition root dos auto-bootstraps).
- [ ] T005 â€” Escrever ADR-0023 (floresta da TownScene â†’ Tilemap/chunks).
- [ ] T006 â€” Escrever ADR-0024 (IMGUIâ†’Canvas remanescente).
- [ ] T007 â€” Escrever ADR-0025 (Resourcesâ†’referÃªncias diretas/Addressables).
- [ ] T008 â€” Rodar docs validation.
- [ ] T009 â€” Gerar execution report.
```

## 29. ValidaÃ§Ãµes obrigatÃ³rias

```powershell
.\tools\docs\validate_docs.ps1
```

## 30. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: NO
- Requires EditMode tests: NO
- Requires PlayMode automated or final human scenario: NO
- Requires regression test: NO
- Human validation timing: NOT REQUIRED
- Minimum validation evidence for ACCEPTED: docs validation PASS; nenhum arquivo fora de docs/decisions/** alterado; os 6 ADRs contÃªm a seÃ§Ã£o "Requer decisÃ£o humana" preenchida.
```

## 31. Definition of Done

```text
6 ADR drafts criados com recomendaÃ§Ã£o, custo, risco e seÃ§Ã£o de decisÃ£o humana.
Nenhuma mudanÃ§a de cÃ³digo.
Docs validation PASS.
Execution report criado.
```

## 32. Anti-regressÃ£o

```text
NÃ£o marcar nenhum ADR como decidido/aceito â€” todos ficam como draft.
NÃ£o duplicar ADR-0008 nem a skill runtime-bootstrap-pattern.
NÃ£o alterar nenhum arquivo fora de docs/decisions/.
```

## 33. Notas para execuÃ§Ã£o posterior

```text
Nenhuma das 6 mudanÃ§as propostas deve ser executada sem aprovaÃ§Ã£o humana explÃ­cita e uma spec de implementaÃ§Ã£o dedicada e prÃ³pria â€” esta spec sÃ³ produz o material de decisÃ£o.
```
