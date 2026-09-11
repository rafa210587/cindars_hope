---
name: monobehaviour-decomposition
description: Separa responsabilidades misturadas em MonoBehaviour preservando comportamento, serialization e wiring. Usar em refatoração autorizada quando regras e adapters Unity têm razões independentes para mudar.
---

# Skill: Decomposição de MonoBehaviour

O projeto usa adapters Unity sobre regras coesas em C# puro e possui services/projections reutilizáveis.

**Regra central: extrair uma responsabilidade real e provar o comportamento da costura, sem quota de classes.**

## Quando usar
- MonoBehaviour mistura regra, input, apresentação, persistência ou lifecycle e isso dificulta a mudança autorizada.

## Checklist essencial
- [ ] Spec/ownership e consumidores identificados; dirty preservado.
- [ ] Regra pura distinguida do adapter e estado mantido com seu owner.
- [ ] Reuso avaliado antes de criar tipos.
- [ ] API, campos serializados, GUIDs e ordem de lifecycle/eventos preservados.
- [ ] Evidência cobre comportamento afetado, inclusive falhas relevantes.

## Procedimento
1. Leia o alvo e consumidores necessários; descreva inputs, resultados, efeitos e invariantes.
2. Use [solid-refactoring](../solid-refactoring/SKILL.md) para desenhar a menor costura coesa;
   consulte `system-reuse-audit` antes de criar core/contrato que pode já existir.
3. Extraia lógica com passos revisáveis. Mantenha campos serializados e referências no adapter
   quando isso preservar o wiring. Não confunda reordenação textual de campos com quebra automática;
   renome/tipo/migração precisam de análise da serialization e spec.
4. Gameplay entre sistemas usa GameEventBus; helpers, queries e policies locais podem ser
   dependências explícitas. Não converter toda chamada síncrona em evento.
5. Valide o comportamento em pontos úteis da extração com os testes pertinentes existentes;
   adicione caracterização/casos de falha apenas quando necessário para cobrir a costura.
6. Aplique gates da `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`; reusar evidência vigente,
   sem build obrigatório após cada classe extraída nem testes que só espelham implementação.

## Validação
Testes podem cobrir uma responsabilidade composta por várias classes. Extração de classe, por si só,
não exige teste novo; sucesso/falha, efeitos e lifecycle alterados determinam a cobertura necessária.
Se wiring Unity mudar, selecionar `unity-validation` e cenário de jogo aplicável. Compile não prova comportamento.
Preservar restrições de `unity-assets`: usar Editor API para scenes/prefabs, sem edição manual incidental de YAML.

## Quando NÃO usar
- Bug pontual solucionável localmente: `bugfix`.
- Apenas reduzir LOC, criar interfaces especulativas ou adicionar cabeçalho por classe.

## Quando parar e reportar
Mudança exige contrato/API/schema fora do escopo ou não há evidência para comportamento afetado:
registre limitação concreta e continue partes independentes autorizadas.

## Saída esperada
Responsabilidades antes/depois, reuso/extração justificada, invariantes preservados,
testes e gates realmente executados, riscos de serialization/wiring ainda pendentes.
