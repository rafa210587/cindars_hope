# Pixel art da tela e feedback visual das habilidades

> **Spec ID:** spec_skills_07_pixelart_feedback_v1
> **Status:** RASCUNHO — implementação não autorizada
> **Wave:** SKILLS_SDD_V1
> **Priority:** P2
> **Type:** Integration
> **Domain:** Skills
> **Revision:** 1 — 2026-09-09
> **Parallelizable:** NO — serial por padrão; só delegar ownership disjunto após fechar contratos
> **Repo lock scope:** arquivos listados no Plan; novos paths propostos só após revisão do escopo
> **Depends on:** [03](spec_skills_03_dados_rank_readiness_v1.md), [06](spec_skills_06_ui_canvas_skills_v1.md)
> **Blocks:** [08](spec_skills_08_equilibrio_aceitacao_v1.md)
> **Validation level alvo:** Unity compile + testes aplicáveis + Play Mode; visual quando aplicável
> **Executor:** Codex ou Claude; mesma spec e mesmas regras
> **Stage:** Spec + Plan + Tasks redigidos; pendências abaixo impedem promoção automática

# /speckit.specify

## Spec — problema e resultado

Os 68 assets de nós auditados não têm ícone e o feedback genérico de SkillBolt não comunica identidade mecânica. Uma tela funcional sem arte/binding ainda não entrega a UI pedida.

**Escopo:** Direção visual, produção de arte por estado/nó operacional e integração verificável no Canvas e feedback das ações.

**Fora do escopo:** Sem aceitar geração conceitual como sprite final, copiar iconografia de D&D ou inventar paleta/resolução canônica sem referência.

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

- **AC01:** Toda habilidade operacional exposta tem ícone final ligado ao SO correto; dormentes são visualmente identificáveis e não prometem efeito ativo.
- **AC02:** Estados da tela são distinguíveis em escala nativa sem depender apenas de cor; textos e foco não colidem com ornamentos.
- **AC03:** Em cada ação coberta, origem, forma, alvo e duração visual correspondem ao efeito medido; falha não toca apresentação de sucesso.
- **AC04:** Assets importados seguem Point/None/sem mipmaps e têm evidência de alpha, tamanho e capturas de tela em jogo.

# /speckit.plan

## Plan — contratos e solução

Entregáveis de autoria em paths propostos `Assets/_Game/Art/UI/Skills/` e `Assets/_Game/Art/VFX/Skills/`, somente após conferir convenção real.
Manifesto de arte: artId, nodeId/actionId, arquivo-fonte, tamanho nativo, paleta, alpha, estados/frames, timing, pivot/PPU ou sizing UI, destino de binding e evidência em jogo.
Reusar Icon dos SOs atuais; nenhum SkillActionIconCatalogSO novo é necessário.
Estados mínimos: bloqueado, disponível, selecionado, comprado, rank máximo, dormente, equipado e em recarga. Cor não pode ser a única diferença.
Primeiro produzir uma amostra em escala real contendo cinco árvores, detalhe de nó e quatro slots; comparar com arte aprovada existente antes de expandir o conjunto.
Separar símbolo de árvore, ícone de nó e VFX de execução. Ícone comunica função (cone/área/projétil/defesa/utilidade), enquanto Vaalara fornece identidade de materiais e motivos.
Animação comunica antecipação, execução, impacto e duração; efeito persistente não parece hit instantâneo.
Import: Point, Compression None, mipmaps false; alpha real e bordas sem halo. Quantidade final acompanha a matriz reconciliada, não força os 68 assets antigos.
Eventos de apresentação seguem GameEventBus e pipeline existente. PlayerActionFeedbackEvent não substitui estado de gameplay; VFX/SFX não decide sucesso.

### Ownership e fontes concretas

MODIFICAR somente os arquivos de código/arte explicitamente previstos nas tasks, depois de promover o pacote.
Arquivos de lore e relatórios listados como fontes são somente leitura, salvo atualização documental expressamente descrita.
CRIAR somente contratos/entregáveis/testes nomeados no plano; caminhos ainda não identificados são pendência, não autorização por wildcard.

- `Assets/_Game/Scripts/Skills/SkillNodeDataSO.cs`
- `Assets/_Game/Scripts/Skills/SkillActionSO.cs`
- `Assets/_Game/Scripts/UI/Skills/SkillTreePanel.cs`
- `Assets/_Game/Scripts/UI/HUD/Views/ActiveSkillSlotsHudView.cs`
- `docs/refinements/a_implementar/ref_skills_capabilities_gameplay_ui_pixelart_v1.md`
- `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`

Padrões aplicáveis: **pixel-art-direction, pixel-art-prompt-authoring, visual-asset-review, game-feel-checklist, audio-event-wiring**. Carregar as skills pelo gatilho, sem copiar suas instruções para o runtime.
Reusar os sistemas citados. Não criar managers/catálogos paralelos para evitar a integração existente.
Testes C# novos ficam na assembly apropriada em `Assets/_Game/Tests/EditMode/` ou `Assets/_Game/Tests/PlayMode/`, nunca em Scripts.

### Sequência técnica

1. **Referências e amostra:** Inspecionar UI/arte aprovada real; registrar paleta, escala e layout no manifesto; produzir e revisar uma amostra de tela antes de completar o set.
2. **Ícones e molduras:** Produzir fontes editáveis e exports para o conjunto operacional definido em 00; desenhar estados e fallback explícito de dormente.
3. **Import e binding:** Gerar/importar pelo Editor, preencher Icon em SkillNodeDataSO/SkillActionSO e referências Canvas de 06; validar manifesto por ID.
4. **Feedback:** Localizar e registrar os arquivos reais do bridge visual/áudio antes de editar; ligar antecipação/impacto/falha a eventos existentes e criar novo DTO apenas se não houver equivalente.
5. **Aceitação visual:** Capturar UI nas resoluções de suporte identificadas no projeto e vídeos de ações; comparar escala, forma, contraste, alpha e timing contra contrato.

### Falhas, migração e limites

Direção/paleta/tamanho e amostra visual ainda precisam ser produzidos e revisados. Este turno entrega a spec de arte, não sprites gerados ou aceitação visual.

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
| [ ] | T01 | Dependências da spec | Referências e amostra — Inspecionar UI/arte aprovada real; registrar paleta, escala e layout no manifesto; produzir e revisar uma amostra de tela antes de completar o set. | AC02 |
| [ ] | T02 | T01 | Ícones e molduras — Produzir fontes editáveis e exports para o conjunto operacional definido em 00; desenhar estados e fallback explícito de dormente. | AC01, AC02 |
| [ ] | T03 | T02 | Import e binding — Gerar/importar pelo Editor, preencher Icon em SkillNodeDataSO/SkillActionSO e referências Canvas de 06; validar manifesto por ID. | AC01, AC04 |
| [ ] | T04 | T03 | Feedback — Localizar e registrar os arquivos reais do bridge visual/áudio antes de editar; ligar antecipação/impacto/falha a eventos existentes e criar novo DTO apenas se não houver equivalente. | AC03 |
| [ ] | T05 | T04 | Aceitação visual — Capturar UI nas resoluções de suporte identificadas no projeto e vídeos de ações; comparar escala, forma, contraste, alpha e timing contra contrato. | AC01, AC02, AC03, AC04 |
| [ ] | T06 | T05 | Evidência e closeout: registrar testes, diff, critérios pendentes e revisão de não regressão conforme /finish-spec | AC01, AC02, AC03, AC04 |

### Testes e evidência esperada

- **SkillArtManifest_AllOperationalIdsBound:** zero ID operacional sem ícone; nenhum ID errado/duplicado.
- **PixelImport_SettingsMatchContract:** todos os sprites do lote Point/None/mipmaps false.
- **HumanVisualScenario:** navegar nos oito estados, ler detalhe e acionar skills; capturas/vídeos anotam resolução, escala e comparação.
- **FailedAction_NoSuccessVfx:** recusa sem custo não reproduz hit/resultado de sucesso.

Para testes EditMode implementados, usar `tools/unity/RunUnityEditModeTests.ps1 -TestFilter <fixture implementada>`.
Resultado esperado do runner: `UNITY_EDITMODE: PASS;` com `failed=0`; guardar XML/log reais.
Play Mode usa a assembly/cenário aplicável, identificando cena, build e resultado. Testes nomeados acima são especificações de comportamento, ainda não existem por terem sido escritos aqui.
A spec de validação também exige os relatórios/capturas definidos no contrato.

### Revisão de consistência e prontidão

- ACs possuem tasks e verificações nomeadas; nenhuma task está marcada concluída.
- Dependências são anteriores no lote; 05 exige decomposição adicional antes de implementação.
- Arquivos compartilhados impedem paralelismo automático. Serial é a ordem segura inicial.
- Direção/paleta/tamanho e amostra visual ainda precisam ser produzidos e revisados. Este turno entrega a spec de arte, não sprites gerados ou aceitação visual.
- **Prontidão:** documento revisável; não promover enquanto pendências de contrato/ownership/decisão relevantes não estiverem fechadas e o Depth Gate não passar.
- **Autorização:** o pedido desta sessão é gerar e refinar specs/plan/tasks; não executar gameplay.

## Fontes de design

[Refinamento técnico v1](../../../docs/refinements/a_implementar/ref_skills_capabilities_gameplay_ui_pixelart_v1.md) e
[revisão de mundo/equilíbrio v2](../../../docs/refinements/a_implementar/ref_vaalara_skills_design_and_balance_v2.md).
As decisões necessárias estão transcritas neste pacote; os refinamentos não precisam virar leitura histórica obrigatória para cada executor.

