---
doc_type: validation
status: evidence
spec_id: SKILLS_SDD_V1_REFINEMENT
validation_type: design_and_documentation
result: PASS_SCOPED
date: 2026-09-10
executor: Codex + game-design-reviewer
source_of_truth: false
validated_adrs: []
validated_game_rules: []
---

# Validation Report — refinamento funcional das skills

> Este relatório valida o refinamento como entrada de design para specs. Não declara as mecânicas
> implementadas nem substitui validação Unity das futuras fatias de runtime.

## Acceptance criteria extracted

- Cobrir os 66 nós preservados, separados em 31 ações equipáveis e 35 passivas/capstones.
- Definir requisito, custo, escala, efeito, reação e apresentação para cada ação.
- Definir consumidor, condição e ganho por rank para cada passiva.
- Manter coerência com 50 pontos base, aproximadamente 55 efetivos, tier gates 0/5/11/18/26 e predominância de uma árvore principal.
- Explicitar riscos de sustain, controle, sobreposição, recuperação e arbitragem.
- Separar fatos atuais, propostas de design e decisões humanas pendentes.

## Existing systems audit

O catálogo vivo, os executores, os efeitos, as regras de progressão e as fontes de direção foram
comparados. O runtime contém 66 IDs e 31 ações equipáveis; parte das ações usa efeitos substitutos ou
feedback-only. O refinamento não trata essas lacunas como conclusão. Referências antigas a 70 nós foram
classificadas como material histórico, preservando os IDs existentes e a compatibilidade de save.

## Spec Compliance Matrix

| Critério | Obrigatório | Resultado | Evidência |
|---|---|---|---|
| Contagem do catálogo | Sim | PASS | verificação estrutural: 31 + 35 = 66 |
| Contrato das ações | Sim | PASS | cinco tabelas de ações com custo, número, forma, reação e estado |
| Contrato das passivas | Sim | PASS | cinco tabelas com requisito, condição, ganho e consumer pendente |
| Progressão | Sim | PASS | nível 100 = 50 base + 1/ato; gates 0/5/11/18/26; dois T5 exigem ao menos 52 |
| Capstones | Sim | PASS_PROPOSED | escada R1–R3 numérica, condicionada à decisão D02 |
| Equilíbrio analítico | Sim | PASS_PROVISIONAL | dano/CD e dano/recurso conferidos; requer PlayMode com builds reais |
| UI, pixel art e animação | Sim | PASS_AS_REQUIREMENT | profiles, estados e critérios visuais definidos; assets ainda não produzidos |
| Links das specs guarda-chuva | Sim | PASS | specs 00, 03, 04 e 05 apontam para o contrato v3 |

## Validation

| Verificação | Resultado | Observação |
|---|---|---|
| Contagem automática de linhas | PASS | 31 ativas, 35 passivas, total 66 |
| Estrutura das tabelas Markdown | PASS | nenhuma linha estrutural inválida |
| Links locais nas specs relacionadas | PASS | todos os destinos existem |
| Validador documental global | FAIL_BASELINE | falhas em specs antigas de UI/farm/town fora deste escopo |
| Revisão independente de game design | APPROVED | confirmação final após três amendments: nenhum bloqueador restante |
| Unity/EditMode/PlayMode | NOT RUN | alteração somente documental; runtime não mudou |

## Errors Found

Nenhum erro estrutural foi encontrado no refinamento. O validador global apontou dívida documental
preexistente fora do escopo: nomes antigos em `.specs/implementados/` e cabeçalhos/markers ausentes em
specs de cave, UI, farm e town. Nenhum desses erros referencia os arquivos de skills revisados.

A primeira rodada do revisor encontrou teto efetivo, status, pré-requisitos, recuperação gratuita e
fórmulas inconsistentes. A segunda delimitou três pontos restantes. O documento final resolve todos:
Domínio do Raio Arcano fica não comprável sem consumer; elites recebem orçamento máximo de controle igual
a 40% do cooldown; e `encounterId` mantém identidade por desengage/load até resolução real do grupo.

## Honest status rationale

`PASS_SCOPED` significa que o documento está completo e rastreável como proposta para gerar specs. Os
valores continuam sujeitos às decisões D02–D08 e ao playtest. As skills não passam a ser consideradas
funcionais no jogo por causa deste relatório; cada spec de runtime deverá provar efeito mecânico, reação,
feedback, save quando aplicável e equilíbrio no contexto Farm/Town/Cave.

## Next Action

Aceitar ou ajustar D02–D08 e decompor o contrato nas dez fatias executáveis listadas no refinamento, com
plan, tasks, testes determinísticos e cenário PlayMode próprios.
