---
doc_type: validation
status: partial
validated_adrs: []
validated_game_rules: [city_rules, event_rules]
---

# Revisão da autoria — interiores híbridos

2026-09-12. Escopo **documental**, não execução do jogo. Pacote [README](../../../.specs/a_implementar/town_interiors_hybrid_v1/README.md), revisão comum1.0. Caminho atualizado após aprovação humana; resultados abaixo preservam o estado da autoria.

## Entrega

Quatro specs completas em Spec/Plan/Tasks, um refinamento e índice local; referência adicionada ao registry sem mudar prioridade/execução de waves. As quatro specs estão em `features_futuras/town_interiors_hybrid_v1/`, não na fila executável. Nenhum Assets/runtime/Unity foi alterado ou executado nesta autoria.

Catálogo:24 fichas com owners/residentes/custódia, área-alvo, zoneamento, móveis/funções, acesso e comércio. Decisão proposta:22 entradas por portal +guarita/curral híbridos com anexos;24 definições de interior em12 templates, uma cena aditiva de conteúdo por vez. Preservar23 moradores Indoor em21 endereços e5 Outdoor;3 prédios institucionais sem residentes inventados.13 fornecedores principais internos,10 externos. Preservar8 serviços pessoais e2cívicos,6estações e efeitos/preços atuais.

## Verificação executada

| Check | Resultado real |
|---|---|
| Estrutura local | PASS:4 specs contêm specify/plan/tasks, ordem/dependência/bloqueia e seção de falhas; todos tasks de implementação desmarcados. |
| Fichas | PASS:24 headers H01–H24,24 HouseIds únicos. Conferência textual das23 atribuições Indoor reais contra ficha correspondente:0 divergências. Isso não prova planta/cena. |
| Comércio | PASS:23 linhas de venue,23NpcIds únicos; leitura dos assets Shop e dos dois catálogos de serviços preserva identidade. |
| Links locais | PASS:16 links dos7 documentos finais,0 quebrados. |
| Diff registry | `git diff --check -- .specs/SPEC_REGISTRY_TO_IMPLEMENT.md`:exit0; edits anteriores preservados. |
| Docs global | FAIL, `powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File tools/docs/validate_docs.ps1`:exit1,173diagnósticos fora do pacote,0 linhas de erro contendo os nomes do novo pacote/refinamento na rodada final. Não declarar GLOBAL_PASS. |
| Unity/Player/arte nova | NOT RUN / não aplicável à entrega documental. Todas evidências nas specs são **esperadas**, não executadas. |
| Revisão independente | NOT RUN:tentativas spawn/followup recusadas por `agent thread limit reached`. Root realizou apenas revisão local; não chamar de aprovação independente. |
| Aprovação de requisitos/lore/acesso | PENDENTE:plantas/áreas, nova forma de entrar e exceção do salão da pousada ainda propostas. |

Método de checks locais: PowerShell `Get-Content` e regex dos headings/links/campos; House assignments extraídos de `CreateMvpTownScene.TownNpcHomes`;24lots em TownCityLayout e23ShopDataPath no roster. Comparação textual é evidência de cobertura documental, não execução de gameplay.

## Correções de consistência realizadas

- Não confundir classificação das9portas comerciais com os23fornecedores reais.
- Preservar Alaric/Eiran como moradores; posto/curral ganham aposento, não cama exposta ou remoção do owner.
- Identificar diferença entre8serviços pessoais e2cívicos; manter Tovin/licença e Mara/registro.
- Retirar proposta de bypass de visita pela porta de loja fechada; respeitar Rule4. Exceção da Inn é explícita, limitada à circulação pública, depende de aprovação.
- Amendment futuro será registrado na regra canônica city_rules, não num arquivo concorrente como fonte de verdade. O primeiro docs run apontou a referência ao caminho de amendment; esse problema local foi corrigido antes da segunda rodada.
- Especificar que router atual não garante blackout antes de load; nova visita exige ack/rollback por token, sem disparar eventos Single globais.
- Não gravar coordenadas de apresentação4096 no PlayerPosition legado; seção aditiva local mantém fallback exterior. NPCs suspensos não são removidos do save.
- Não marcar hipótese de dimensões/pixel art como aceita por testes; três pilotos precisam aprovação antes do rollout.

## Gates pendentes / decisão

Profundidade documental: cabeçalhos, Phase0, contratos propostos, CREATE/MODIFY, per-edit plan, testes nomeados, ACs, tasks e falhas presentes. **Readiness de execução não concedida**: requer revisão independente de arquitetura/contratos e aprovação humana de S01/transição/acesso. Após isso promover pacote/referências juntos e registrar amendment; executar S01→S02→S03→S04, com piloto de Renko/CarvalhoTorto/GateKeeper antes dos demais.

Diagnósticos globais fora do escopo incluem quatro specs implementadas sem prefixo `spec_` e headers/metadata faltantes em specs antigas/concorrentes. Não renomeados/corrigidos nesta tarefa. Sem commits, push, limpeza ou mudanças no trabalho paralelo.
