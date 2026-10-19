---
name: npc-walk-animation
description: Gera, revisa e integra walk sheets de NPC compatíveis com NpcWalkAnimator. Usar ao criar ou corrigir caminhada 5x5, fidelidade ao sprite-base e transição idle/walk.
---

# Skill: Animação de Caminhada de NPC

O pipeline existente usa folhas 5x5 e NpcWalkAnimator; esse formato é contrato deste consumidor.

**Regra central: validar identidade, movimento e tamanho no jogo como critérios separados.**

## Quando usar
- Criar/regerar folha de caminhada de um NPC ou revisar a integração existente.

## Checklist essencial
- [ ] Sprite-base real aberto; cores, cabelo, roupa e props preservados.
- [ ] Layout compatível com o consumidor; passada avaliada por direção.
- [ ] Alpha, cortes, baseline e escala comparados com a base.
- [ ] Playback e observação em jogo reportados separadamente da análise estática.

## Procedimento
1. Abra a base do NPC em `Assets/_Game/Resources/NpcSprites/`; o label do roster não substitui a arte aprovada.
2. Para gerar, use o brief e o canal solicitado: [pixel-art-prompt-authoring](../pixel-art-prompt-authoring/SKILL.md), ou [chatgpt-web-sprite-gen](../chatgpt-web-sprite-gen/SKILL.md) quando o canal for web.
3. Confira [sprite-animation-review](../sprite-animation-review/SKILL.md) para key poses, contato, direções e playback.
4. Para normalizar/importar, leia [contrato e wiring NPC](references/npc-pipeline.md).
5. Use [visual-asset-review](../visual-asset-review/SKILL.md) se precisar investigar alpha ou drift da base.

## Validação
A folha deve preservar cores de pele e roupa, cabelo/penteado, proporções e acessórios da base.
Tiras revelam poses/cortes, mas não comprovam timing. Confira movimento real também em frente/costas,
inclusive quando mantos ocultarem os pés. Não reprove holds intencionais por repetirem um desenho.
Gates conforme SPEC_VALIDATION_MATRIX_MASTER; se C# mudou e .NET for aplicável, usar
`Invoke-UnityGeneratedProjectsBuild.ps1`. Asset-only não exige .NET; conferir evidência vigente sem repetir builds.

## Quando NÃO usar
- Retratos ou sprites estáticos de mundo: usar o workflow do tipo de imagem.

## Quando parar e reportar
Sem playback/Unity acessível, registre os gates pendentes. Não declare slice materializado por compile.
Se geração falhar, preserve a fila e siga o tratamento limitado do canal usado; não faça retry infinito.

## Saída esperada
Folha/base examinadas, origem, diagnóstico por direção, paths de saída e evidência de import/movimento/escala.
