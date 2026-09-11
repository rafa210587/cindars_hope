# Farm v17 — barco e recortes

Em execução. Fonte boat_keyart_v4.png76×68; a extremidade esquerda foi recortada porque estava oculta pelo cais na imagem de origem. Correção localizada em candidato Aseprite separado, mantendo canvas, apoio, posição e colisão.
Spec/plan/tasks: .specs/a_implementar/spec_farm_clipped_props_v17.md.

Evidência anterior: contact_v16/rock_fix/gameplay/contact/fishing_tip_0152.png. Não confundir revisão de recortes com validação global da fazenda.

## Auditoria independente dos demais componentes

30fontes keyartv3/v4 medidas. Sprites inspecionados incluem cais, ponte, quadro, suporte de ferramentas, banca, poço, galinheiro, queijaria, vinícola, estufa, árvores, cascata. Nenhuma outra silhueta claramente amputada identificada nesse conjunto.
Waterfall82×125 toca o topo em28pixels(x23..50), mas a junção fica inserida no maciço rochoso da cena: conexão intencional, sem corte exposto equivalente ao barco. Water_ripples toca bordas por ser textura de efeito.
Finding separado: emenda retangular de grama junto ao gate leste, vista em rock_fix/scene/farm_capture_region_border_east.png (aprox804..1328,345..556). É transição entre materiais/camadas, não recorte alpha; permanece como ajuste de terreno, fora da reconstrução do barco.
Margem alpha não prova silhueta completa: a máscara interna do barco tinha margem e ainda assim estava incompleta. Não usar apenas teste de pixels na borda para aprovar props.

## Integração concluída

Unity6000.5.7f1 RegenAndCapture exit0, generate.log e scene/*.png. Root abriu captura water_bridge e confirmou popa fechada na cena. Revisão independente aprovou candidato sem halo/corte novo.76×68,106pixels alterados,81novos opacos,3camadasAseprite,alpha binário. Manifesto offline registra hashes e fonte intacta.
Asset novo boat_keyart_v17.png; apenas seleção do sprite alterada em CreateMvpFarmScene. Apoio(35,24),canvas,altura opaca32,PPU128,transformações e física preservados. ImporterPoint/None/no-mips; overrides plataforma desativados.
Testes de gameplay não repetidos: alteração visual sem mudança de geometria/comportamento; evidência contact_v16/rock_fix permanece pertinente para os contatos anteriores. Capturas são EditMode, não nova execuçãoPlay. Sem porcentagem de semelhança ou aprovação global.
