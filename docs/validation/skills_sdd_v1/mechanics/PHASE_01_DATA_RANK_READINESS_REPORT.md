# Skills SDD v1 — validação da fase mecânica 1

> **Spec:** `spec_skills_10_mechanics_data_rank_readiness_v1`  
> **Data:** 2026-09-10  
> **Status:** PASS  
> **Escopo:** dados de ação, rank, readiness e slice vertical do Corte Giratório

## Resultado funcional

- O catálogo mantém 66 nós: 31 equipáveis e 35 passivos/capstones.
- Existem 31 definições `SkillActionSO`, uma por ação equipável, e um `SkillActionDatabaseSO` gerado.
- Rank efetivo respeita o menor valor entre a progressão dinâmica e `AuthoredMaxRank`.
- Capstones e ações autoradas R1–3 recusam R4.
- A execução recebe rank, variante e dados autorados.
- Habilidade dormente equipada recusa antes de recurso, projétil e cooldown.
- Melee e projétil consomem dano, custo, alcance, contagem, pierce e cooldown dos dados.
- Corte Giratório R3 aplica 14 aos três primeiros alvos, 10 aos alvos 4–6 e ignora o sétimo. O arredondamento inteiro de `14 × 0,7` é 10.
- A ordem de alvos usa distância e `EnemyInstanceId`; o teste cobre sete instâncias da mesma espécie na mesma posição.
- Bomba Improvisada tóxica consome 20 stamina e preserva mana; o tipo de dano não decide mais o recurso.

## Geração de assets

- Método: `CindarsHope.Editor.Skills.GenerateCanonicalSkillCatalog.Generate`
- Log: `Logs/skills-phase1-generate.log`
- Resultado: 66 nós atualizados, 5 árvores atualizadas, 31 actions criadas.
- Encontrado: 31 action assets, `SkillActionDatabase.asset` presente.
- Observação: a pasta de nós contém 68 assets históricos; o gerador atualiza somente os 66 nós canônicos e não apaga conteúdo fora do catálogo.

## Evidência Unity

| Gate | Resultado | Evidência |
|---|---:|---|
| Unity compile | PASS | `Logs/skills-phase1-compile-final2.log` |
| EditMode Skills | PASS — 10/10 | `Logs/skills-phase1-editmode-final2.xml` e `.log` |
| PlayMode foundation | PASS — 8/8 | `Logs/skills-phase1-playmode-final2.xml` e `.log` |

O primeiro PlayMode revelou criação de ScriptableObject no inicializador de campo de um `MonoBehaviour`.
O registro passou a ser criado em `InitializeAsSingleton`; a mesma suíte foi repetida e passou. Duas execuções
EditMode anteriores falharam por uma fixture acoplada ao singleton global; a verificação de catálogo foi tornada
pura e a garantia transacional foi movida para PlayMode.

## Balanceamento

Os valores desta fase são um baseline coerente para implementação: custo 22, cooldown 6 s e dano total máximo
72 no Corte Giratório R3, com risco posicional e limite de seis alvos. Isto não encerra o balanceamento global.
A aceitação final depende da fase de telemetria/playtest com TTK, disponibilidade real de recursos, frequência de
uso e comparação com ataque básico e demais famílias.

## Revisão independente

**PASS.** Escopo, arquitetura, IDs, rank caps, readiness transacional, geração de assets e compatibilidade
com a fundação foram verificados. Unity compile passou; EditMode Skills 10/10 e PlayMode foundation 8/8
passaram em evidência posterior à última alteração. Nenhum bloqueador permanece.

## Riscos residuais

- As demais ações ainda incluem mecânicas dormentes ou substitutas; serão abertas somente nas fases próprias.
- O banco gerado ainda usa fallback canônico quando não está atribuído no inspector; wiring explícito pertence à
  integração de dados/UI.
- Arte, telegraph e animação não fazem parte desta fase.
