# Skills — fase mecânica 9: aceitação comparativa de equilíbrio

> **Spec ID:** spec_skills_21_balance_acceptance_v1  
> **Status:** APROVADA — autorização humana de 2026-09-10  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Validation+Tuning / Skills+Combat+Economy / P0  
> **Parallelizable:** NO; **Depends on:** spec_skills_20_survival_crafting_capstones_v1  
> **Blocks:** spec de UI da árvore  
> **Validation:** EditMode determinístico + PlayMode gráfico na Cave/Farm + revisão de game design

# /speckit.specify

## Spec

Aceitar ou ajustar o catálogo completo por comparação entre builds reais. Fórmulas unitárias são pré-condição;
equilíbrio só passa com telemetria reproduzível e inspeção em jogo.

- **AC01:** executar builds melee Kanthor/Kaand, ranged Alihana/Senya/Nyx, magic Anya/Senya, survival
  Telisandra e crafting Thoren em R1/R3, níveis 20/50 e equipamento coerente com o estágio.
- **AC02:** TTK permanece nas bandas canônicas do `TelemetryTargetEvaluator`: Common 3–6 s, Elite 12–20 s,
  Miniboss 45–90 s, Boss 120–240 s. Nenhuma build supera outra de mesmo estágio em single target por mais
  de 25% na mediana sem pagar risco/recurso equivalente.
- **AC03:** registrar dano causado/recebido, stamina/mana inicial-final, casts recusados, whiffs, acertos por
  alvo e uptime de controle. Common/Elite/Boss não podem permanecer sob controle forte por mais de
  65/40/20% da janela observada.
- **AC04:** cura/sustain não permite loop infinito sem provisões; uma run curta e uma longa preservam decisão
  entre consumo, retorno e risco. Load/reaggro/respec não rearmam usos únicos.
- **AC05:** Marca de Eficiência prova break-even por rank em 1/2/4/6 ações; Thoren e passivas econômicas
  permanecem abaixo de 20% da renda diária mediana e nenhum ciclo buy→respec→sell gera lucro.
- **AC06:** toda mudança de tuning volta ao `SkillActionSO`/nó/perfil autorado, regenera assets e repete a
  matriz afetada. O relatório separa PASS, ajuste aplicado e risco residual.
- **AC07:** uma sessão gráfica comprova origem no avatar, telegraph/tempo legível, reação do inimigo e ausência
  de travamento nas três cenas; captura sem graphics device não conta como validação visual.

# /speckit.plan

Reusar `CombatTelemetryService`, `CombatTelemetrySession`, `TelemetryTargetEvaluator` e os cenários de Cave
existentes. Criar somente um runner/projection de skills se a telemetria atual não carregar `actionId`, rank,
variant e fase. Fixtures usam seeds fixas; resultados vão para
`docs/validation/skills_sdd_v1/balance/PHASE_09_BALANCE_ACCEPTANCE_REPORT.md` e JSON/CSV bruto em `Logs/`.

Não criar inimigos artificiais para substituir a matriz viva. Um fixture puro pode validar fórmulas, mas pelo
menos um Common, Elite e Boss reais devem participar da aceitação. Alterações acima de 10% em dano/custo/CD
exigem nova revisão de design; menores podem ser aplicadas e registradas no relatório.

### Plano técnico após audit de reuse

1. **21A — observabilidade:** estender `CombatTelemetrySession`, `CombatTelemetryReport` e
   `CombatTelemetryService` para guardar buildId, nível, rank, variant, actionId, resultado da ação,
   acertos por alvo e janelas de controle. Reusar `PlayerOffensiveActionCommittedEvent`,
   `PlayerActionFeedbackEvent` e os eventos de dano/status; só ampliar um evento quando os dados não
   existirem em nenhuma fonte atual. DTOs novos são aditivos e preservam leitura dos relatórios antigos.
2. **21B — matriz determinística:** criar um projection/runner de aceitação que seleciona assets reais
   das builds e Bestiary, fixa seed/nível/equipamento e produz uma linha por cenário. O runner calcula
   mediana, TTK, dano recebido, custo de recurso, recusas, whiffs, hits/target e uptime de controle, sem
   duplicar as fórmulas do gameplay.
3. **21C — survival/economia:** usar os estados reais de Telisandra/Thoren e pricing/crafting existentes
   para rodadas curta/longa e save/load/respec. A fixture de renda mediana deve derivar seus valores de
   atividades canônicas do estágio e registrar a composição; nenhum número de renda é inventado no teste.
4. **21D — runtime real:** executar Common, Elite e Boss reais e uma sessão gráfica Farm/Town/Cave. Os
   mesmos identificadores de build/cenário usados no runner devem aparecer nos JSON/CSV e no relatório.
5. **21E — tuning:** qualquer ajuste fica no `SkillActionSO`, nó ou perfil autorado. Regerar assets e
   repetir somente a matriz afetada; delta acima de 10% volta para revisão de game design.

Ownership serial padrão, porque sessão, serviço e relatório formam um contrato compartilhado:

- MODIFY `Assets/_Game/Scripts/Combat/Telemetry/CombatTelemetrySession.cs`
- MODIFY `Assets/_Game/Scripts/Combat/Telemetry/CombatTelemetryReport.cs`
- MODIFY `Assets/_Game/Scripts/Combat/Telemetry/CombatTelemetryService.cs`
- MODIFY `Assets/_Game/Scripts/Combat/Telemetry/TelemetryTargetEvaluator.cs` somente se faltar uma
  regra de aceitação; não duplicar as bandas existentes
- MODIFY eventos existentes somente após provar ausência do dado na superfície atual
- MODIFY `Assets/_Game/Tests/EditMode/Combat/CombatTelemetryTests.cs` e
  `CombatTelemetryPhaseOneTests.cs`
- CREATE uma fixture PlayMode de aceitação da fase 21 e seu gerador de JSON/CSV em `Logs/`

Falha fechada: cenário sem build, nível, target class, asset real ou campo obrigatório é `NOT RUN` para
aquela linha e impede aceitação global; não preencher zero como se fosse observação válida.

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [ ] | T01 | 21A: adicionar build/action/rank/variant, outcome, hits/target e uptime de controle à telemetria; compatibilidade dos relatórios antigos | AC01–AC03 |
| [ ] | T02 | 21B: criar matriz/seed/builds níveis 20/50 e EditMode de envelopes, DR e comparação de medianas | AC01–AC03 |
| [ ] | T03 | 21C: fixture canônica de renda, break-even, sustain e save/load/respec, incluindo `<20%` por estágio | AC04–AC05 |
| [ ] | T04 | 21D: PlayMode Common/Elite/Boss reais com builds ofensivas R1/R3 e JSON/CSV bruto | AC01–AC03, AC06 |
| [ ] | T05 | 21D: PlayMode survival/crafting curta/longa + load/respec | AC04–AC05 |
| [ ] | T06 | 21D: sessão gráfica Farm/Town/Cave e capturas com telegraph/reação/travamento | AC07 |
| [ ] | T07 | 21E: aplicar tuning necessário, regerar/repetir matriz e obter revisão independente | AC01–AC07 |
