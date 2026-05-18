# Cindar's Hope — Fase 8: Plano de Execução com Codex v1.1

> **Fase:** 8 de 13  
> **Status:** Pronto para execução local, com sequência de PRs sincronizada em 2026-05-16  
> **Objetivo:** implementar o MVP Fazenda por PRs pequenos, rastreáveis e revisáveis  
> **Depende de:** `PROJECT_LOG.md`, `AGENTS.md`, `CLAUDE.md`, `GDD_v2.6.md`, `ARCH_fase4_v2.2.md`, `FASE7_SPEC_MVP_FARM_v2.2.md`, `CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md`

---

## 0. Nota de sincronização v1.1

Durante a execução real da Fase 8, a sequência foi ajustada para reduzir risco de retrabalho:

- `PR-001` continua sendo **Core foundation**.
- `PR-002` continua sendo **Data contracts e registries**.
- `PR-003` passa a ser **Assets de dados MVP**, antes de managers e gameplay.
- A partir do antigo `PR-003`, os PRs de código foram deslocados.
- A ferramenta inicial **Cana Básica** deve entrar no pacote de dados MVP, porque o inventário inicial planejado já depende dela.
- `GrowthStageSprites` pode ficar vazio no `PR-003`, mas todo código futuro de `CropTile` deve tratar array vazio/nulo com fallback visual.
- Qualquer agente deve ler e atualizar `PROJECT_LOG.md` antes/depois de tarefas relevantes.

Esta versão substitui a ordem prática da seção 6. Se houver conflito entre esta sequência e versões antigas citadas em outros documentos, usar esta sequência e registrar a divergência no `PROJECT_LOG.md`.

---

## 0.2 Nota de sincronização pós PR-008

PR-001 a PR-008 já foram executados e sincronizados em `dev`. O PR-009 é documental e não altera runtime, assets, cenas, prefabs ou gameplay.

O próximo PR runtime será movimento/input. Antes dele:

- Confirmar New Input System instalado/ativo (`com.unity.inputsystem`).
- Confirmar Cinemachine instalado se o PR usar Cinemachine (`com.unity.cinemachine`).
- Criar `PlayerInputActions.inputactions` no PR de movimento/input, com Action Map `Player`:
  - `Move`
  - `Interact`
  - `Inventory`
  - `Sleep`

Política de VFX:

- VFX não é dependência obrigatória dos sistemas MVP.
- Sistemas de gameplay não devem depender de prefabs VFX inexistentes.
- Feedbacks visuais podem ficar para PR dedicado de polish/feedback.

Nota de sincronização pós PR-011:

- PR-009, PR-010 e PR-011 já foram executados após a nota anterior.
- Sequência operacional atual:
  - PR-009 — Docs sync pós PR-008
  - PR-010 — PlayerInputActions + PlayerController mínimo
  - PR-011 — Runtime State Hardening
  - PR-012 — Interaction System mínimo
- As seções antigas de PR-008 em diante são histórico/plano base e não devem ser usadas cegamente sem consultar `PROJECT_LOG.md`.

---

## 1. Princípio central

O Codex deve implementar **fatias pequenas**, não “o jogo inteiro”.

A unidade correta de trabalho é:

```text
1 PR pequeno → 1 objetivo → poucos arquivos → teste manual claro → commit em português
```

O objetivo da Fase 8 é concluir este loop:

```text
BootScene → FarmScene → inventário inicial → plantar → avançar dias → colher → vender → salvar → fechar → reabrir → estado restaurado
```

---

## 2. Regras obrigatórias para qualquer prompt Codex

Todo prompt para Codex deve conter:

1. Documentos a ler.
2. PR alvo.
3. Escopo permitido.
4. Arquivos permitidos.
5. Arquivos proibidos.
6. Critérios de aceite.
7. Teste manual.
8. Regras de arquitetura.
9. O que não fazer.
10. Instrução para atualizar `PROJECT_LOG.md`.

### Prompt base

```md
Leia primeiro:
- PROJECT_LOG.md
- AGENTS.md
- CLAUDE.md
- docs/GDD_v2.6.md
- docs/ARCH_fase4_v2.2.md
- docs/FASE7_SPEC_MVP_FARM_v2.2.md
- docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md
- docs/FASE8_EXECUTION_PLAN_CODEX_v1.0.md

Implemente somente:
<PR-ID> — <nome>

Objetivo:
<uma frase>

Branch esperada:
feature/fase8-pr-<numero>-<nome-curto>

Arquivos permitidos:
- <lista exata>

Arquivos proibidos:
- docs/GDD_v2.6.md
- docs/ARCH_fase4_v2.2.md
- docs/FASE7_SPEC_MVP_FARM_v2.2.md
- docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md
- qualquer arquivo fora da lista permitida

Regras:
- Não implementar V2/FULL.
- Não ampliar escopo.
- Não usar GameObject.Find, FindObjectOfType ou FindObjectsByType em runtime.
- Não usar StreamingAssets para save.
- Não serializar referências Unity em JSON.
- Dados de jogo devem ficar em ScriptableObject.
- Comunicação entre sistemas deve usar GameEventBus.
- Atualizar PROJECT_LOG.md ao final.

Ao final, entregue:
- arquivos alterados;
- resumo técnico curto;
- teste manual executável;
- riscos/pendências;
- próximo PR sugerido.
```

---

## 3. Branch strategy

```bash
git checkout dev
git pull origin dev
git checkout -b feature/fase8-pr-001-core-foundation
```

Padrão:

```text
feature/fase8-pr-<numero>-<nome-curto>
```

Exemplos:

```text
feature/fase8-pr-001-core-foundation
feature/fase8-pr-003-mvp-data-assets
feature/fase8-pr-010-plantio-basico
feature/fase8-pr-018-load-boot
```

Commits em português:

```bash
git commit -m "feat: adicionar fundação de eventos core"
git commit -m "feat: adicionar assets de dados do mvp"
git commit -m "fix: corrigir serialização do inventário por id"
git commit -m "test: adicionar validação de stack do inventário"
```

---

## 4. Definition of Ready por PR

Antes de abrir tarefa no Codex:

- [ ] O PR cabe em revisão humana curta.
- [ ] Há lista de arquivos permitidos.
- [ ] Há lista de arquivos proibidos.
- [ ] Há teste manual objetivo.
- [ ] O escopo é MVP, não V2/FULL.
- [ ] O repositório está limpo (`git status`).
- [ ] Unity abre sem erro antes da mudança.
- [ ] `PROJECT_LOG.md` foi consultado.
- [ ] Branch atual corresponde ao PR.

---

## 5. Definition of Done por PR

Um PR só está pronto quando:

- [ ] Unity compila sem erro.
- [ ] Console não tem erros novos.
- [ ] Teste manual passa.
- [ ] Regras de `AGENTS.md` e `CLAUDE.md` foram respeitadas.
- [ ] `PROJECT_LOG.md` foi atualizado.
- [ ] Não há busca global em runtime.
- [ ] Não há hardcode de dados de jogo em MonoBehaviour.
- [ ] Save, se envolvido, usa `Application.persistentDataPath`.
- [ ] Save, se envolvido, usa IDs e tipos simples.
- [ ] Alterações estão pequenas e revisáveis.
- [ ] Commit em português criado.

---

## 6. Sequência de PRs da Fase 8 — ordem vigente

### PR-001 — Core foundation

**Objetivo:** criar fundação mínima de eventos.

**Specs relacionadas:** FARM-001, ARCH seção GameEventBus.

**Arquivos permitidos:**

```text
Assets/_Game/Scripts/Core/GameEventBus.cs
Assets/_Game/Scripts/Core/Events/DayStartedEvent.cs
Assets/_Game/Scripts/Core/Events/GoldChangedEvent.cs
Assets/_Game/Scripts/Core/Events/InventoryChangedEvent.cs
Assets/_Game/Scripts/Core/Events/SeedPlantedEvent.cs
Assets/_Game/Scripts/Core/Events/CropHarvestedEvent.cs
Assets/_Game/Scripts/Core/Events/TreeChoppedEvent.cs
Assets/_Game/Scripts/Core/Events/FishCaughtEvent.cs
Assets/_Game/Scripts/Core/Events/HungerChangedEvent.cs
Assets/_Game/Scripts/Core/Events/GameSavedEvent.cs
```

**Não fazer:** Player, cena, UI, inventário.

**Teste manual:** Unity compila; nenhum erro no Console.

---

### PR-002 — Data contracts e registries

**Objetivo:** criar contratos de dados por ID, ScriptableObjects base e registries.

**Specs relacionadas:** FARM-011, FARM-021, FARM-071.

**Arquivos permitidos:**

```text
Assets/_Game/Scripts/Core/Data/IIdentifiedData.cs
Assets/_Game/Scripts/Core/Data/IDataRegistry.cs
Assets/_Game/Scripts/Core/Data/DataRegistrySO.cs
Assets/_Game/Scripts/Core/Data/ItemDatabaseSO.cs
Assets/_Game/Scripts/Core/Data/SeedDatabaseSO.cs
Assets/_Game/Scripts/Farm/Data/SeedDataSO.cs
Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs
Assets/_Game/Scripts/Inventory/Data/ItemCategory.cs
Assets/_Game/Scripts/Player/Data/PlayerDataSO.cs
PROJECT_LOG.md
```

**Teste manual:** criar assets manualmente no Unity; menus `CindarsHope/Data/*` e `CindarsHope/Database/*` aparecem; campos aparecem no Inspector.

---

### PR-003 — Assets de dados MVP

**Objetivo:** criar os assets ScriptableObject reais do MVP Fazenda, incluindo itens, sementes, PlayerData e registries.

**Motivo da posição:** managers e inventário inicial devem depender de dados concretos, não de dados hardcoded.

**Arquivos pe$��m��mo�L�D��;�%g�?w��ŷ���ovH0��a�5��*�ؒ��l͛�S�iy�r�O7����%L]��%���hk ����>v1�HB������d\�(eoIx�>3�6BS%���(
��f$�h�����eԎ���H���`ݶf{�Fo�Y���@00uMb�z-��XI$&�gf���7Ӵ�u|'K.�oP
P���F�.��o��9B<~. ����[����<٭�$�����{1�A��.�bKx�L������'�u8n5���e ,]�H����V��Ww�$�C�el��|zys��K�i-�q�ݬbk,wnG��;�� ~�e�r͒���~'1`V⦫�-*[��L�K�'2@����仪��n���2�N� �ƶ�G���i/U��'E�@�`H��;J�������+J�n#���6ڴ�ĹG���N�G�'�Z!�����Wi��NJ�@���A��Z|�[��$q}i�ҷ�QbtTEC$��m��mo�L�D��;�%g�?w��ŷ���ovH0��a�5��*�ؒ��l͛�S�iy�r�O7����%L]��%���hk ����>v1�HB������d\�(eoIx�>3�6BS%���(
��f$�h�����eԎ���H���`ݶf{�Fo�Y���@00uMb�z-��XI$&�gf���7Ӵ�u|'K.�oP
P���F�.��o��9B<~. ����[����<٭�$�����{1�A��.�bKx�L������'�u8n5���e ,]�H����V��Ww�$�C�el��|zys��K�i-�q�ݬbk,wnG��;�� ~�e�r͒���~'1`V⦫�-*[��L�K�'2@����仪��n���2�N� �ƶ�G���i/U��'E�@�`H��;J�������+J�n#���6ڴ�ĹG���N�G�'�Z!�����Wi��NJ�@���A��Z|�[��$q}i�ҷ�QbtTEC$��m��mo�L�D��;�%g�?w��ŷ���ovH0��a�5��*�ؒ��l͛�S�iy�r�O7����%L]��%���hk ����>v1�HB������d\�(eoIx�>3�6BS%���(
��f$�h�����eԎ���H���`ݶf{�Fo�Y���@00uMb�z-��XI$&�gf���7Ӵ�u|'K.�oP
P���F�.��o��9B<~. ����[����<٭�$�����{1�A��.�bKx�L������'�u8n5���e ,]�H����V��Ww�$�C�el��|zys��K�i-�q�ݬbk,wnG��;�� ~�e�r͒���~'1`V⦫�-*[��L�K�'2@����仪��n���2�N� �ƶ�G���i/U��'E�@�`H��;J�������+J�n#���6ڴ�ĹG���N�G�'�Z!�����Wi��NJ�@���A��Z|�[��$q}i�ҷ�QbtTEC$��m��mo�L�D��;�%g�?w��ŷ���ovH0��a�5��*�ؒ��l͛�S�iy�r�O7����%L]��%���hk ����>v1�HB������d\�(eoIx�>3�6BS%���(
��f$�h�����eԎ���H���`ݶf{�Fo�Y���@00uMb�z-��XI$&�gf���7Ӵ�u|'K.�oP
P���F�.��o��9B<~. ����[����<٭�$�����{1�A��.�bKx�L������'�u8n5���e ,]�H����V��Ww�$�C�el��|zys��K�i-�q�ݬbk,wnG��;�� ~�e�r͒���~'1`V⦫�-*[��L�K�'2@����仪��n���2�N� �ƶ�G���i/U��'E�@�`H��;J�������+J�n#���6ڴ�ĹG���N�G�'�Z!�����Wi��NJ�@���A��Z|�[��$q}i�ҷ�QbtTEC$��m��mo�L�D��;�%g�?w��ŷ���ovH0��a�5��*�ؒ��l͛�S�iy�r�O7����%L]��%���hk ����>v1�HB������d\�(eoIx�>3�6BS%���(
��f$�h�����eԎ���H���`ݶf{�Fo�Y���@00uMb�z-��XI$&�gf���7Ӵ�u|'K.�oP
P���F�.��o��9B<~. ����[����<٭�$�����{1�A��.�bKx�L������'�u8n5���e ,]�H����V��Ww�$�C�el��|zys��K�i-�q�ݬbk,wnG��;�� ~�e�r͒���~'1`V⦫�-*[��L�K�'2@����仪��n���2�N� �ƶ�G���i/U��'E�@�`H��;J�������+J�n#���6ڴ�ĹG���N�G�'�Z!�����Wi��NJ�@���A��Z|�[��$q}i�ҷ�QbtTEC$��m��mo�L�D��;�%g�?w��ŷ���ovH0��a�5��*�ؒ��l͛�S�iy�r�O7����%L]��%���hk ����>v1�HB������d\�(eoIx�>3�6BS%���(
��f$�h�����eԎ���H���`ݶf{�Fo�Y���@00uMb�z-��XI$&�gf���7Ӵ�u|'K.�oP
P���F�.��o��9B<~. ����[����<٭�$�����{1�A��.�bKx�L������'�u8n5���e ,]�H����V��Ww�$�C�el��|zys��K�i-�q�ݬbk,wnG��;�� ~�e�r͒���~'1`V⦫�-*[��L�K�'2@����仪��n���2�N� �ƶ�G���i/U��'E�@�`H��;J�������+J�n#���6ڴ�ĹG���N�G�'�Z!�����Wi��NJ�@���A��Z|�[��$q}i�ҷ�QbtTEC$��m��mo�L�D��;�%g�?w��ŷ���ovH0��a�5��*�ؒ��l͛�S�iy�r�O7����%L]��%���hk ����>v1�HB������d\�(eoIx�>3�6BS%���(
��f$�h�����eԎ���H���`ݶf{�Fo�Y���@00uMb�z-��XI$&�gf���7Ӵ�u|'K.�oP
P���F�.��o��9B<~. ����[����<٭�$�����{1�A��.�bKx�L������'�u8n5���e ,]�H����V��Ww�$�C�el��|zys��K�i-�q�ݬbk,wnG��;�� ~�e�r͒���~'1`V⦫�-*[��L�K�'2@����仪��n���2�N� �ƶ�G���i/U��'E�@�`H��;J�������+J�n#���6ڴ�ĹG���N�G�'�Z!�����Wi��NJ�@���A��Z|�[��$q}i�ҷ�QbtTEC$��m��mo�L�D��;�%g�?w��ŷ���ovH0��a�5��*�ؒ��l͛�S�iy�r�O7����%L]��%���hk ����>v1�HB������d\�(eoIx�>3�6BS%���(
��f$�h�����eԎ���H���`ݶf{�Fo�Y���@00uMb�z-��XI$&�gf���7Ӵ�u|'K.�oP
P���F�.��o��9B<~. ����[����<٭�$�����{1�A��.�bKx�L������'�u8n5���e ,]�H����V��Ww�$�C�el��|zys��K�i-�q�ݬbk,wnG��;�� ~�e�r͒���~'1`V⦫�-*[��L�K�'2@����仪��n���2�N� �ƶ�G���i/U��'E�@�`H��;J�������+J�n#���6ڴ�ĹG���N�G�'�Z!�����Wi��NJ�@���A��Z|�[��$q}i�ҷ�QbtTEC$��m��mo�L�D��;�%g�?w��ŷ���ovH0��a�5��*�ؒ��l͛�S�iy�r�O7����%L]��%���hk ����>v1�HB������d\�(eoIx�>3�6BS%���(
��f$�h�����eԎ���H���`ݶf{�Fo�Y���@00uMb�z-��XI$&�gf���7Ӵ�u|'K.�oP
P���F�.��o��9B<~. ����[����<٭�$�����{1�A��.�bKx�L������'�u8n5���e ,]�H����V��Ww�$�C�el��|zys��K�i-�q�ݬbk,wnG��;�� ~�e�r͒���~'1`V⦫�-*[��L�K�'2@����仪��n���2�N� �ƶ�G���i/U��'E�@�`H��;J�������+J�n#���6ڴ�ĹG���N�G�'�Z!�����Wi��NJ�@���A��Z|�[��$q}i�ҷ�QbtTEC$��m��mo�L�D��;�%g�?w��ŷ���ovH0��a�5��*�ؒ��l͛�S�iy�r�O7����%L]��%���hk ����>v1�HB������d\�(eoIx�>3�6BS%���(
��f$�h�����eԎ���H���`ݶf{�Fo�Y���@00uMb�z-��XI$&�gf���7Ӵ�u|'K.�oP
P���F�.��o��9B<~. ����[����<٭�$�����{1�A��.�bKx�L������'�u8n5���e ,]�H����V��Ww�$�C�el��|zys��K�i-�q�ݬbk,wnG��;�� ~�e�r͒���~'1`V⦫�-*[��L�K�'2@����仪��n���2�N� �ƶ�G���i/U��'E�@�`H��;J�������+J�n#���6ڴ�ĹG���N�G�'�Z!�����Wi��NJ�@���A��Z|�[��$q}i�ҷ�QbtTEC$��m��mo�L�D��;�%g�?w��ŷ���ovH0��a�5��*�ؒ��l͛�S�iy�r�O7����%L]��%���hk ����>v1�HB������d\�(eoIx�>3�6BS%���(
��f$�h�����eԎ���H���`ݶf{�Fo�Y���@00uMb�z-��XI$&�gf���7Ӵ�u|'K.�oP
P���F�.��o��9B<~. ����[����<٭�$�����{1�A��.�bKx�L������'�u8n5���e ,]�H����V��Ww�$�C�el��|zys��K�i-�q�ݬbk,wnG��;�� ~�e�r͒���~'1`V⦫�-*[��L�K�'2@����仪��n���2�N� �ƶ�G���i/U��'E�@�`H��;J�������+J�n#���6ڴ�ĹG���N�G�'�Z!�����Wi��NJ�@���A��Z|�[��$q}i�ҷ�QbtTEC$��m��mo�L�D��;�%g�?w��ŷ���ovH0��a�5��*�ؒ��l͛�S�iy�r�O7����%L]��%���hk ����>v1�HB������d\�(eoIx�>3�6BS%���(
��f$�h�����eԎ���H���`ݶf{�Fo�Y���@00uMb�z-��XI$&�gf���7Ӵ�u|'K.�oP
P���F�.��o��9B<~. ����[����<٭�$�����{1�A��.�bKx�L������'�u8n5���e ,]�H����V��Ww�$�C�el��|zys��K�i-�q�ݬbk,wnG��;�� ~�e�r͒���~'1`V⦫�-*[��L�K�'2@����仪��n���2�N� �ƶ�G���i/U��'E�@�`H��;J�������+J�n#���6ڴ�ĹG���N�G�'�Z!�����Wi��NJ�@���A��Z|�[��$q}i�ҷ�QbtTEC$��m��mo�L�D��;�%g�?w��ŷ���ovH0��a�5��*�ؒ��l͛�S�iy�r�O7����%L]��%���hk ����>v1�HB������d\�(eoIx�>3�6BS%���(
��f$�h�����eԎ���H���`ݶf{�Fo�Y���@00uMb�z-��XI$&�gf���7Ӵ�u|'K.�oP
P���F�.��o��9B<~. ����[����<٭�$�����{1�A��.�bKx�L������'�u8n5���e ,]�H����V��Ww�$�C�el��|zys��K�i-�q�ݬbk,wnG��;�� ~�e�r͒���~'1`V⦫�-*[��L�K�'2@����仪��n���2�N� �ƶ�G���i/U��'E�@�`H��;J�������+J�n#���6ڴ�ĹG���N�G�'�Z!�����Wi��NJ�@���A��Z|�[��$q}i�ҷ�QbtTEC$��m��mo�L�D��;�%g�?w��ŷ���ovH0��a�5��*�ؒ��l͛�S�iy�r�O7����%L]��%���hk ����>v1�HB������d\�(eoIx�>3�6BS%���(
��f$�h�����eԎ���H���`ݶf{�Fo�Y���@00uMb�z-��XI$&�gf���7Ӵ�u|'K.�oP
P���F�.��o��9B<~. ����[����<٭�$�����{1�A��.�bKx�L������'�u8n5���e ,]�H����V��Ww�$�C�el��|zys��K�i-�q�ݬbk,wnG��;�� ~�e�r͒���~'1`V⦫�-*[��L�K�'2@����仪��n���2�N� �ƶ�G���i/U��'E�@�`H��;J�������+J�n#���6ڴ�ĹG���N�G�'�Z!�����Wi��NJ�@���A��Z|�[��$q}i�ҷ�QbtTEC$��m��mo�L�D��;�%g�?w��ŷ���ovH0��a�5��*�ؒ��l͛�S�iy�r�O7����%L]��%���hk ����>v1�HB������d\�(eoIx�>3�6BS%���(
��f$�h�����eԎ���H���`ݶf{�Fo�Y���@00uMb�z-��XI$&�gf���7Ӵ�u|'K.�oP
P���F�.��o��9B<~. ����[����<٭�$�����{1�A��.�bKx�L������'�u8n5���e ,]�H����V��Ww�$�C�el��|zys��K�i-�q�ݬbk,wnG��;�� ~�e�r͒���~'1`V⦫�-*[��L�K�'2@����仪��n���2�N� �ƶ�G���i/U��'E�@�`H��;J�������+J�n#���6ڴ�ĹG���N�G�'�Z!�����Wi��NJ�@���A��Z|�[��$q}i�ҷ�QbtTEC$��m��mo�L�D��;�%g�?w��ŷ���ovH0��a�5��*�ؒ��l͛�S�iy�r�O7����%L]��%���hk ����>v1�HB������d\�(eoIx�>3�6BS%���(
��f$�h�����eԎ���H���`ݶf{�Fo�Y���@00uMb�z-��XI$&�gf���7Ӵ�u|'K.�oP
P���F�.��o��9B<~. ����[����<٭�$�����{1�A��.�bKx�L������'�u8n5���e ,]�H����V��Ww�$�C�el��|zys��K�i-�q�ݬbk,wnG��;�� ~�e�r͒���~'1`V⦫�-*[��L�K�'2@����仪��n���2�N� �ƶ�G���i/U��'E�@�`H��;J�������+J�n#���6ڴ�ĹG���N�G�'�Z!�����Wi��NJ�@���A��Z|�[��$q}i�ҷ�QbtTEC→ dormir 3x → colher → vender → salvar → fechar → reabrir → dia/ouro/inventário/canteiros restaurados
```

---

## 7. Como revisar saída do Codex

### 7.1 Comandos

```bash
git status
git diff --stat
git diff
```

### 7.2 Busca por violações

```bash
grep -R "GameObject.Find\|FindObjectOfType\|FindObjectsByType\|StreamingAssets" Assets/_Game/Scripts
```

Se aparecer em código runtime, revisar ou rejeitar.

### 7.3 Sinais de PR ruim

- Muitos arquivos alterados fora do escopo.
- Implementação de V2/FULL escondida.
- Dados hardcoded em MonoBehaviour.
- Save serializando `ScriptableObject`.
- Managers se chamando diretamente sem evento.
- Código compila, mas cena depende de configuração manual não documentada.
- `PROJECT_LOG.md` não atualizado.

---

## 7.4 Entrega de commits pelo agente

Ao final de cada PR, o agente **não** faz `git push` nem abre PR/MR. Em vez disso:

1. **Cria commits locais** com mensagens em português.
2. **Lista commits criados** para o humano revisar:
   ```bash
   git log --oneline -N
   ```
3. **Entrega ao humano:**
   - SHA + mensagem de cada commit.
   - Arquivos alterados (via `git diff --stat`).
   - Testes executados/pendentes.
   - Riscos/pendências documentadas em `PROJECT_LOG.md`.
4. **O humano faz push/PR/merge** usando os comandos sugeridos.

Nunca executar `git push` ou abrir PR automaticamente. Agente prepara; humano entrega.

---

## 8. Política de rollback

Se Codex gerar algo ruim:

```bash
git restore <arquivo>
# ou, se for tudo da tentativa:
git reset --hard HEAD
```

Nunca tentar “consertar em cima” de uma alteração grande e confusa. Melhor reduzir o prompt e refazer.
