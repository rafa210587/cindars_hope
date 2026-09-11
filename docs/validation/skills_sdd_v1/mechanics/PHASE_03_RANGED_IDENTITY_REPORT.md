# Fase mecânica 3 — identidade ranged

Data: 2026-09-10  
Spec: `spec_skills_13_ranged_identity_v1`  
Resultado técnico: PASS

## Resultado jogável

- Disparo Carregado usa carga autorada de 0,20–1,10 s e interpola dano, alcance, posture e stamina a partir de `SkillActionSO`. Interrupção antes do commit não cobra recurso nem inicia cooldown.
- Linha Perfurante limita cinco alvos, deduplica colliders, aplica 100/80/64/51/41% e para em obstáculo sólido.
- Tiro Triplo limita a sobreposição a dois hits no mesmo alvo, com 50% no segundo.
- Flecha Sangrante reaplica o mesmo status por refresh, sem criar pilhas paralelas.
- Presa Marcada seleciona o inimigo válido mais próximo em até 8 tiles, dura 6/8/10 s e concede +8/12/16% aos projéteis ranged do mesmo caster. O contrato cobre skills e os dois caminhos de arco comum.
- Falhas publicam `FailureKey`; custo e cooldown só são confirmados no commit.

## Fonte dos valores

Os endpoints de carga e as curvas de duração/bônus da marca vivem nos assets `SkillActionSO` gerados. Os componentes runtime mantêm apenas estado transitório e interpolação. A geração confirmou 66 nós, 5 árvores e 31 ações.

## Evidência fresca

- Catálogo: `Logs/skills-phase13-generate5.log` — PASS, 66/66 nós, 5/5 árvores, 31/31 ações.
- EditMode: `Logs/skills-phase13-editmode-final5.xml` — PASS, 74 executados, 74 aprovados, 0 falhas.
- PlayMode integrado: `Logs/skills-phase13-regression-playmode-final3.xml` — PASS, 30 executados, 30 aprovados, 0 falhas.
- PlayMode ranged após o teste de limite/fora de alcance: `Logs/skills-phase13-ranged-playmode-final.xml` — PASS, 5 executados, 5 aprovados, 0 falhas.
- Cobertura PlayMode: fundação de execução, formas melee, movimento/posture e identidade ranged no runtime Unity.

## Limite da conclusão de equilíbrio

Os números estão coerentes com o orçamento baseline aprovado e sem exploits conhecidos de cobrança, sobreposição ou propriedade da marca. A conclusão de equilíbrio comparativo do roster permanece reservada à Fase 21, que mede tempo para derrotar, eficiência de recursos, controle e dominância entre builds em cenários comuns.

## Risco residual

`ActiveSkillExecutionController` ainda recria defaults de catálogo em mudanças de cena. A revisão independente classificou a propriedade desses objetos temporários como risco de lifecycle separado da identidade ranged; deve ser removido em uma fase de integração antes do closeout global se o catálogo gerado ainda não for a única fonte runtime.
