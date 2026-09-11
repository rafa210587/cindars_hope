# Town Keyart Acceptance Rule v1

Status: SUPERSEDED — usar `TOWN_KEYART_ACCEPTANCE_RULE_v2.md`. A alegação anterior de aprovação humana do rubric detalhado foi incorreta; houve autorização de executar e criar critérios, não validação da pontuação.  
Reference: `docs/art_catalog/_reference_keyart_GPT/town_keyart_layout_chatgpt_web_candidate_v2.png`  
Comparison capture: `docs/validation/playmode/town_capture_full.png`  
Camera: 1536x1024, centro (0,0), orthographic size 60, mesma captura em toda revisão.

## Regra de aprovação

A TownScene só pode ser declarada visualmente aderente quando:

1. obtiver **80/100 ou mais** no scorecard abaixo;
2. nenhuma categoria visual obtiver menos de 50% dos seus pontos;
3. todos os gates obrigatórios de preservação e física passarem;
4. a avaliação usar a captura real da cena regenerada, nunca mockup ou keyart isolada.

Uma imagem de fundo esticada, decal único ou reprodução chapada da keyart não conta. A cena deve continuar formada por Tilemaps, sprites/props, objetos percorríveis e colliders reais.

## Scorecard de fidelidade — 100 pontos

### A. Composição macro — 20 pontos

- 4: templo dominante no noroeste e prefeitura dominante no nordeste.
- 4: praça/fonte circular é o foco central inequívoco.
- 4: mercado/estalagem/bancas formam o núcleo oeste.
- 4: ferraria/ofícios formam o núcleo leste e curral/animais ocupam sudeste.
- 4: lago/moinho/cais ocupam sudoeste e o portão sul fecha o eixo de entrada.

### B. Ruas e praça orgânicas — 20 pontos

- 5: praça circular ou octogonal, com anel claro ao redor da fonte/estátua.
- 5: caminhos conectam distritos por curvas, degraus ou bordas irregulares; não leem como um grande sinal de `+`.
- 5: caminhos secundários têm 2–3 tiles, vias principais 3–5 tiles e variação visual coerente.
- 5: todas as 24 portas alcançam visual e fisicamente a malha conectada.

### C. Arquitetura e identidade — 20 pontos

- 5: templo e prefeitura preservam as sprites detalhadas e dominância da referência.
- 5: mercado, pousada, padaria, ferraria, alquimia e moinho são reconhecíveis sem depender de texto.
- 5: casas residenciais variam telhado, jardim, cerca e silhueta sem perder a mesma perspectiva 3/4.
- 5: escala arquitetônica segue a regra 32 px/tile: casas 8x7, serviços 8–14 tiles e landmarks 14–16 tiles de largura visual/footprint equivalente.

### D. Densidade ambiental — 15 pontos

- 5: canteiros, arbustos e flores quebram os vazios de grama nos sete distritos.
- 5: postes, bancos, barris, caixas, cercas e placas apoiam as funções dos bairros.
- 5: nenhum vazio gramado contínuo maior que aproximadamente 14x10 tiles permanece dentro da muralha sem função visual ou jogável.

### E. Borda natural e água — 10 pontos

- 3: borda mistura árvores, rochas/elevação e clareiras; não parece uma moldura retangular uniforme.
- 3: água possui margem orgânica, vegetação aquática e ligação visual com moinho/cais.
- 2: entrada norte/caverna e portão sul são legíveis e não bloqueados.
- 2: floresta externa mantém profundidade sem ocultar landmarks ou entradas.

### F. Paleta, pixel e profundidade — 10 pontos

- 3: paleta quente de pedra/madeira é equilibrada pelos azuis de Kanthor e vegetação verde.
- 2: iluminação/contraste direcionam o olhar à praça e aos dois landmarks do norte.
- 2: sprites usam Point, Compression None, mipmaps off e pixel density consistente.
- 3: pivô/sorting permitem player e NPCs passarem corretamente à frente/atrás de casas e árvores.

### G. Vida e preservação — 5 pontos

- 2: NPCs aparecem distribuídos pelos distritos e não agrupados no centro.
- 3: os gates obrigatórios abaixo passam integralmente.

## Gates obrigatórios — reprovação imediata

- 24 `House_*` estáveis e percorríveis.
- 28 NPCs canônicos com 84 anchors `work/social/home`; NPC extra permitido sem remover canônicos.
- 23 bancas de NPC, 6 bancas de mercado e 2 spawns estáveis.
- zero lote-lote, lote-via, porta desconectada ou clearance de muralha inválida.
- lago não invade lote ou via; prefeitura, água, muralha, casas e árvores têm collider coerente com o footprint visual.
- `town_default`, `town_from_farm`, portal sul e IDs de NPC/casa não mudam.
- nenhuma edição manual de YAML; TownScene é regenerada pelo creator canônico.

## Procedimento de revisão

1. Regenerar `TownScene.unity` pelo creator canônico.
2. Rodar `TownLayoutTests` e `ValidateFableCitySchedule`.
3. Gerar `town_capture_full.png` com `TownSceneCapture` usando a câmera fixa acima.
4. Abrir a captura e a referência lado a lado.
5. Pontuar cada item somente com evidência visível; registrar total, perdas e ajustes pedidos.
6. Se `<80`, REPROVADO: outro passe de implementação é obrigatório.
