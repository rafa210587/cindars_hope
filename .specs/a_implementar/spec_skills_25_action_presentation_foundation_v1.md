# Skills — fase visual 2: apresentação e animações de ações

> **Spec ID:** spec_skills_25_action_presentation_foundation_v1  
> **Status:** APROVADA — autorização humana de 2026-09-10  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Runtime+Art / Skills+Presentation / P1  
> **Parallelizable:** NO; **Depends on:** spec_skills_24_ui_pixel_art_v1  
> **Blocks:** spec_skills_26_action_animation_catalog_v1  
> **Validation:** EditMode+PlayMode gráfico

# /speckit.specify

## Spec

Criar o contrato data-driven que sincroniza windup, Active, recovery, animação, VFX, feedback e SFX sem colocar apresentação dentro dos executores de mecânica.

- **AC01:** cada ação resolve `AnimationProfile`, `VFXProfile`, `FeedbackProfile` e `SFXProfile` por ID estável. Ausência marca `PresentationMissing` e registra diagnóstico, mas uma mecânica válida continua executando.
- **AC02:** animação e telegraph começam no Windup; hit/projectile/zone nasce em Active após commit; recovery encerra a ação sem segundo dano.
- **AC03:** cancelamento pré-commit limpa apresentação; interrupção pós-commit mantém custo/cooldown e encerra visuais órfãos.
- **AC04:** eventos são DTOs simples no `GameEventBus`, com unsubscribe completo; áudio usa o pipeline de eventos existente.
- **AC05:** a apresentação não adiciona churn recorrente. Pool existente é reutilizado quando comprovado; pool novo exige cenário medido e não puxa refactor global de floating text.
- **AC06:** testes provam uma única aplicação mecânica por cast e alinhamento temporal de cada fase.
- **AC07:** `SkillActionSO` e `SkillCastTimeline` são a única autoridade de windup/Active/recovery; profiles adaptam frames e não duplicam duração, commit, root lock ou movimento.
- **AC08:** a integração estende a sequência sprite-driven de `PlayerWalkAnimator` para oito direções ou espelhamento declarado. Timeline/executor mantém ownership de `MovementLocked`; idle/walk atuais têm testes de caracterização.
- **AC09:** Disparo Carregado possui profile Charge start/loop/cancel/release. Charge curto ou cancelado limpa visuais sem custo/cooldown; release entra no windup/commit canônico sem criar uma segunda autoridade temporal.

# /speckit.plan

Adicionar `SkillAnimationProfileSO`, `SkillVfxProfileSO`, `SkillFeedbackProfileSO`, `SkillSfxProfileSO` e um
presentation coordinator fino. Reusar `SkillCastTimeline`, eventos de
execução, `PlayerWalkAnimator`, projectile runtime, pooling comprovado e audio-event wiring. Não alterar números de balanceamento.

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [ ] | T01 | Definir profiles, database e DTOs de apresentação | AC01, AC04 |
| [ ] | T02 | Integrar coordinator com timeline/cancelamento | AC02–AC03, AC06 |
| [ ] | T03 | Integrar animator, pooling, feedback e áudio | AC04–AC05 |
| [ ] | T04 | Gerador/validator de cobertura dos 31 actionIds | AC01 |
| [ ] | T05 | Caracterização, charge, testes determinísticos e PlayMode gráfico base | AC02–AC09 |
