# Vínculo de habilidades ao avatar e aplicação real dos efeitos

> **Spec ID:** spec_skills_01_avatar_execucao_v1
> **Status:** RASCUNHO — implementação não autorizada
> **Wave:** SKILLS_SDD_V1
> **Priority:** P0
> **Type:** Runtime
> **Domain:** Skills
> **Revision:** 1 — 2026-09-09
> **Parallelizable:** NO — serial por padrão; só delegar ownership disjunto após fechar contratos
> **Repo lock scope:** arquivos listados no Plan; novos paths propostos só após revisão do escopo
> **Depends on:** Nenhuma dependência de mudança de design
> **Blocks:** [03](spec_skills_03_dados_rank_readiness_v1.md), [05](spec_skills_05_mecanicas_ativas_v1.md), [08](spec_skills_08_equilibrio_aceitacao_v1.md)
> **Validation level alvo:** Unity compile + testes aplicáveis + Play Mode; visual quando aplicável
> **Executor:** Codex ou Claude; mesma spec e mesmas regras
> **Stage:** Spec + Plan + Tasks redigidos; pendências abaixo impedem promoção automática

# /speckit.specify

## Spec — problema e resultado

ActiveSkillExecutionController deriva o caster do GameObject de PlayerManager, que pode estar em _Bootstrap, separado de PlayerController. Quatro efeitos de status têm ID mas chance default zero. Movimento de golpe pode disputar a física do avatar.

**Escopo:** Conectar o caster e InteractionSystem ao avatar da cena, garantir unicidade de dano e aplicar os status já especificados.

**Fora do escopo:** Sem novo movimento base, rebalanceamento de dano, mudança de cave procedural ou reescrita da composição.

### Contexto que deve ser preservado

RPG de ação classless em Vaalara/Dornécia, cinco árvores e quatro slots; Farm/Town/Cave se complementam.
D&D inspira papéis e ferramentas, sem copiar classes, d20 ou regras de descanso.
Anya/Fonte/Fruto Mana não viram substitutos genéricos de recursos comuns; mistérios de lore permanecem.
IDs e save existentes são preservados. Estado canônico atual prevalece sobre contagens históricas.

### Phase 0 observada

Auditoria de código desta sessão: [relatório](../../../docs/validation/SKILLS_CAPABILITIES_AUDIT_2026_09_09.md).
Baseline analítica: [relatório de equilíbrio](../../../docs/validation/skills_balance_v1/BALANCE_REVIEW.md).
O fato que motiva esta fatia é o problema descrito acima; não é evidência de correção.
Na execução, reconfirmar os arquivos abaixo com `rg -n` para as classes/métodos citados.
Se houver drift, atualizar spec/plan/tasks antes de alterar comportamento. Não reexecutar backlog antigo para reverter CURRENT_STATE.

### Critérios de aceitação

- **AC01:** Em Farm, Town e Cave, origem/targeting seguem PlayerController mesmo quando PlayerManager permanece em _Bootstrap.
- **AC02:** Sem avatar/alvo obrigatório válido, nenhuma aplicação, débito ou cooldown; há recusa localizada.
- **AC03:** Um receptor com dois colliders sofre um hit por golpe; movimento não atravessa parede nem é sobrescrito pelo input.
- **AC04:** Os quatro status configurados têm probabilidade explícita coerente com a direção e podem ser observados no receptor.

# /speckit.plan

## Plan — contratos e solução

APIs existentes: `ActiveSkillExecutionController.TryUseSlot(int)`, `SkillTargetResolver.SetInteractionSystem(InteractionSystem)`.
Contrato proposto em ActiveSkillExecutionController:
`public void BindAvatar(GameObject avatar, InteractionSystem interactionSystem)`.
Argumentos nulos desligam o vínculo; não persistir referências Unity no save.
O installer/adaptador de ciclo de vida deve fornecer o avatar ativo pela composição existente. Não usar PlayerManager.gameObject como posição por inferência nem procurar objetos globalmente.
Ao descarregar a cena, invalidar contexto antes de qualquer próximo input. Bind repetido do mesmo avatar é idempotente.

MeleeStrikeSkillEffectExecutor: obter identidade do receptor, deduplicar por receptor por execução e usar a rota de deslocamento consumida no FixedUpdate do PlayerController. Não introduzir uma segunda escrita concorrente de Rigidbody2D.
Status: autoria explícita da probabilidade já prevista para cada efeito; IDs sem chance configurada devem gerar erro de validação, não um acerto visual falso.

### Ownership e fontes concretas

MODIFICAR somente os arquivos de código/arte explicitamente previstos nas tasks, depois de promover o pacote.
Arquivos de lore e relatórios listados como fontes são somente leitura, salvo atualização documental expressamente descrita.
CRIAR somente contratos/entregáveis/testes nomeados no plano; caminhos ainda não identificados são pendência, não autorização por wildcard.

- `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/SkillTargetResolver.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/MeleeStrikeSkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ProjectileSkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutorCatalog.cs`
- `Assets/_Game/Scripts/Player/PlayerController.cs`
- `Assets/_Game/Scripts/Composition/DomainRuntimeInstallers.cs`

Padrões aplicáveis: **bootstrap-wiring, player-ability-runtime, ability-effect-composition**. Carregar as skills pelo gatilho, sem copiar suas instruções para o runtime.
Reusar os sistemas citados. Não criar managers/catálogos paralelos para evitar a integração existente.
Testes C# novos ficam na assembly apropriada em `Assets/_Game/Tests/EditMode/` ou `Assets/_Game/Tests/PlayMode/`, nunca em Scripts.

### Sequência técnica

1. **Binding:** Em ActiveSkillExecutionController.Bootstrap/WireInteractionSystem e callbacks de cena, substituir resolução por BindAvatar; installer fornece referências existentes e limpa no unload.
2. **Targeting:** Em SkillTargetResolver, invalidar interação antiga ao receber null; contexto captura avatar vivo antes de executar.
3. **Física e hits:** Em MeleeStrikeSkillEffectExecutor, deduplicar receptores e encaminhar deslocamento à rota de PlayerController usada em FixedUpdate; cancelar no bloqueio físico.
4. **Status:** Em ActiveSkillExecutorCatalog e executores, preencher chance explícita dos quatro status a partir de direção aceita e verificar resolução do ID.
5. **Verificação:** Adicionar testes de binding, recusa e multi-collider e executar cenário em três cenas com troca de cena e morte/respawn.

### Falhas, migração e limites

A referência concreta de ciclo de vida da composição deve ser confirmada na Fase 0 de execução. Não assumir que Instância singleton prove qual avatar é ativo.

Ausência de dependência obrigatória deve ser diagnosticada; não esconder falha por fallback de busca global.
Não publicar sucesso antes do efeito/commit correspondente. Não salvar referências Unity.
Sem edição manual de YAML. Wiring/assets exigem ferramenta Editor e evidência de geração.
Cave visited-level snapshots permanecem intactos; esta spec não autoriza regeneração.
Leitura de cena/testes antigos não prova Play Mode vigente.

# /speckit.tasks

## Tasks — revisão 1

Todos os itens estão **não executados**. Tarefas de pesquisa/decisão fecham o Plan; não autorizam preencher lacunas enquanto se implementa.

| Done | ID | Dependência | Arquivo/método e edição — ownership | Cobertura |
|---|---|---|---|---|
| [ ] | T01 | Dependências da spec | Binding — Em ActiveSkillExecutionController.Bootstrap/WireInteractionSystem e callbacks de cena, substituir resolução por BindAvatar; installer fornece referências existentes e limpa no unload. | AC01, AC02 |
| [ ] | T02 | T01 | Targeting — Em SkillTargetResolver, invalidar interação antiga ao receber null; contexto captura avatar vivo antes de executar. | AC01, AC02 |
| [ ] | T03 | T02 | Física e hits — Em MeleeStrikeSkillEffectExecutor, deduplicar receptores e encaminhar deslocamento à rota de PlayerController usada em FixedUpdate; cancelar no bloqueio físico. | AC03 |
| [ ] | T04 | T03 | Status — Em ActiveSkillExecutorCatalog e executores, preencher chance explícita dos quatro status a partir de direção aceita e verificar resolução do ID. | AC04 |
| [ ] | T05 | T04 | Verificação — Adicionar testes de binding, recusa e multi-collider e executar cenário em três cenas com troca de cena e morte/respawn. | AC01, AC02, AC03, AC04 |
| [ ] | T06 | T05 | Evidência e closeout: registrar testes, diff, critérios pendentes e revisão de não regressão conforme /finish-spec | AC01, AC02, AC03, AC04 |

### Testes e evidência esperada

- **AvatarBinding_UsesPlayerControllerObject:** posição do caster igual à do avatar, diferente da de _Bootstrap.
- **MissingAvatar_DoesNotSpend:** deltas de HP/MP/stamina e cooldown iguais a zero.
- **Melee_MultipleColliders_SingleHit:** receptor recebe exatamente um hit por execução.
- **StatusChanceConfigured_AppliesAtCertainChance:** fixture com chance 1 aplica; fixture com chance 0 não aplica, sem substituir os valores de produção.

Para testes EditMode implementados, usar `tools/unity/RunUnityEditModeTests.ps1 -TestFilter <fixture implementada>`.
Resultado esperado do runner: `UNITY_EDITMODE: PASS;` com `failed=0`; guardar XML/log reais.
Play Mode usa a assembly/cenário aplicável, identificando cena, build e resultado. Testes nomeados acima são especificações de comportamento, ainda não existem por terem sido escritos aqui.
A spec de validação também exige os relatórios/capturas definidos no contrato.

### Revisão de consistência e prontidão

- ACs possuem tasks e verificações nomeadas; nenhuma task está marcada concluída.
- Dependências são anteriores no lote; 05 exige decomposição adicional antes de implementação.
- Arquivos compartilhados impedem paralelismo automático. Serial é a ordem segura inicial.
- A referência concreta de ciclo de vida da composição deve ser confirmada na Fase 0 de execução. Não assumir que Instância singleton prove qual avatar é ativo.
- **Prontidão:** documento revisável; não promover enquanto pendências de contrato/ownership/decisão relevantes não estiverem fechadas e o Depth Gate não passar.
- **Autorização:** o pedido desta sessão é gerar e refinar specs/plan/tasks; não executar gameplay.

## Fontes de design

[Refinamento técnico v1](../../../docs/refinements/a_implementar/ref_skills_capabilities_gameplay_ui_pixelart_v1.md) e
[revisão de mundo/equilíbrio v2](../../../docs/refinements/a_implementar/ref_vaalara_skills_design_and_balance_v2.md).
As decisões necessárias estão transcritas neste pacote; os refinamentos não precisam virar leitura histórica obrigatória para cada executor.

