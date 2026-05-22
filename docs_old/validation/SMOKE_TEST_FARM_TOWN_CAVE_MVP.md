# Smoke Test Farm/Town/Cave MVP

## Preparacao

- [ ] `git checkout dev`
- [ ] `git pull origin dev`
- [ ] Abrir Unity sem Safe Mode.
- [ ] Confirmar Console sem erro vermelho.
- [ ] Rodar `CindarsHope/Validate/Validate MVP Data`.
- [ ] Rodar `CindarsHope/Validate/Validate Farm Town MVP`.
- [ ] Rodar `CindarsHope/Validate/Validate Cave MVP`.
- [ ] Rodar `CindarsHope/Validate/Validate All MVP Scenes`.

## Geracao de cenas

- [ ] Rodar `CindarsHope/Scenes/Create MVP FarmScene`.
- [ ] Rodar `CindarsHope/Scenes/Create MVP TownScene`.
- [ ] Rodar `CindarsHope/Scenes/Create MVP CaveScene`.
- [ ] Abrir `FarmScene`.
- [ ] Abrir `TownScene`.
- [ ] Abrir `CaveScene`.

## Farm

- [ ] Entrar em Play Mode na FarmScene.
- [ ] Player move.
- [ ] HUD aparece e nao duplica.
- [ ] Interacao por `E` funciona.
- [ ] Plantar em plot.
- [ ] Avancar dia.
- [ ] Crop cresce.
- [ ] Colher crop.
- [ ] Cortar arvore.
- [ ] Pescar.
- [ ] Comprar semente.
- [ ] Vender item.
- [ ] Craftar madeira processada.
- [ ] Coletar pickup.
- [ ] Salvar e carregar.

## Town

- [ ] Farm -> Town pelo portal.
- [ ] Spawn correto na Town.
- [ ] HUD segue visivel e unico.
- [ ] Falar com Pip.
- [ ] Comprar trigo.
- [ ] Comprar cenoura.
- [ ] Vender no SellBox.
- [ ] Salvar e carregar na Town.
- [ ] Town -> Farm sem perder estado basico.

## Cave

- [ ] Farm -> Cave pelo portal.
- [ ] Spawn correto na Cave.
- [ ] HUD segue visivel e unico.
- [ ] Slime existe.
- [ ] Slime persegue o player.
- [ ] Atacar Slime com `J`.
- [ ] Slime toma dano, hit flash e knockback quando aplicavel.
- [ ] Encostar no Slime causa dano por contato com cooldown.
- [ ] Matar Slime.
- [ ] Confirmar drop no inventario.
- [ ] Salvar e carregar na Cave.
- [ ] Cave -> Farm sem quebrar referencias.

## Criterio de aprovacao

- [ ] Console sem erro vermelho ao final.
- [ ] Save/load nao volta indevidamente para outra cena.
- [ ] HUD nao duplica em nenhuma transicao.
- [ ] Inventario/ouro/HP/fome/dia permanecem coerentes.
