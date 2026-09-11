# F05 — plano de validação da rodada de fechamento

Owner exclusivo Unity. Baseline anterior permanece imutável: cena7E2FFD6919591E634444D943AE8EB26D1EFF4AECA886A8F03F0B8CBE60A2EA2C, backup360arquivos/149PNGs verificados e zeroUnity ativo. Capturas anteriores em before_diagnostic/ e before_gameplay/.

Seleção pela SPEC_VALIDATION_MATRIX_MASTER: mudanças de composição/cena exigem geração, imports pertinentes e inspeção; novas colisões/assentamento exigem Play Mode físico. Mudança runtime implica compile e teste focais pertinentes. Reusar11testes anteriores somente após comparar seus inputs; novo teste/contrato exige runner focal. Não repetir .NET, compile-only ou global sem risco correspondente.

Ordem: freeze conjuntoF01–F04 e release root → regeneração e10diagnósticos → revisão visual root/ajustes → teste focal se necessário →8vistas MainCamera e16rotas físicas → prova reforçada de aproximação → hashes/meta/saves/diff → relatório. Fonte congelada durante cada Unity. Não rodar Play Mode enquanto root reprovar a composição intermediária.

## Critérios técnicos

- Compilação Editor comprovada pela geração/testes; cada exit/log explícito e scan único.
- FarmScene materializada via Unity API, sem YAML manual; original, novos arquivos e hashes distinguidos.
-149PNGs anteriores byte-idênticos; imports novos, se houver, Point/None/noMip/PPU/pivot/alpha avaliados conforme emenda.
- Scene/hash atual coincide com metadata das oito vistas; nenhum save/load humano; SaveInput desativado em memória e hashes dos saves preservados.
-16rotas com paths não vazios, collider real e nós visitados; postura/arte da ponte inspecionada separadamente.
- Portas/craft: aproximação física não basta para interação. InteractionSystem usa trigger, IInteractable.CanInteract e distância ao Collider.ClosestPoint limitada por _maxInteractionDistance. Gate adicional deve registrar essas condições no ponto alcançável se implementado pelo captureowner; nenhuma ação mutante de craft/save.
- Porta fechada: aproximar não prova entrada, abertura/fechamento ou roof reveal em deslocamento. Não alegar walkthrough pelo BFS.
- Poço Visual_CentralWell atual possui somente SpriteRenderer; não há collider/IInteractable. Reportar aproximação física como tal, sem inventar funcionalidade.
- Fonte/todos inputs finais equivalentes aos testados; reusar somente evidência pertinente.
- Aceitação artística e decisão de fechamento são do root/usuário; outputs de workers não são prova visual. Técnica SCOPED_PASS não significa arte ACCEPTED.

Residual risk sem gate adicional: aproximações em tolerância1.25u podem ficar fora do alcance real de interação. BFS não simula input, animação, callbacks durante caminhada ou sessão completa. Registrar NOT RUN quando não executado, nunca PASS por imagem.

Command attempted: nenhum Unity iniciado nesta preparação; aguardando freeze/release.
