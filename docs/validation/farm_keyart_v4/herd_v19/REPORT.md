# Farm v19 — rebanho integrado

Status: **INTEGRATED_SCOPED_PASS**. Spec/plan/tasks: [spec v19](../../../../.specs/a_implementar/spec_farm_herd_motion_v19.md). [Comparativo e reprodução](index.html).

## Entrega
- Vaca, ovelha e cabra: folhas 48×48, 19 poses, PPU32, pivô (24,8), Point/None/sem mipmaps; fontes Aseprite em camadas preservadas. Galinha e barco v18 mantidos.
- Parada, caminhada em três direções, pastar e repousar reaproveitam o runtime existente, resolvendo perfis por AnimalDataId. Pastar não alimenta nem cria produtos.
- Celeiro usa área relativa (-2,4; -3,4)..(1,1; -1,3), afastada da construção e do feno. Nenhuma mudança no schema de save ou regras de cuidado.
- Item de cordeiro ausente corrigido no gerador e registrado no ItemDatabase. BaseValue inicial 180, como cabrito, documentado na emenda; nenhuma oferta adicionada à loja. Nenhum animal gratuito.

## Evidência
| Gate | Resultado |
|---|---|
| Geração da cena | PASS, `generate.log`, exit 0 |
| EditMode | 65/65 PASS, `editmode.xml` e `editmode.log` |
| Geração do item faltante | PASS, `generate-items.log`, exit 0 |
| Play A | FAIL: item de cordeiro ausente; preservado em `gameplay_a` |
| Play B final | PASS, `playmode-b.log` e [metadata](gameplay_b/capture-metadata.json), exit 0 |
| Capacidade | 4 solturas e 4 restaurações, duas vacas + cabra + ovelha |
| Movimento | 60,00035s; 19 sprites e quatro modos observados por animal; todas as direções |
| Deslocamento real | Vacas 9,55u / 7,48u; ovelha 6,18u; cabra 9,97u |
| Geometria | Corpo contido; distância mínima entre animais -0,00306u, dentro da tolerância 0,011u; sólidos +0,6225u |
| Interação | Quatro pausas observadas, deslocamento congelado 0; CanInteract verdadeiro após restauração |
| Transações | Itens temporários consumidos; feed permaneceu zero; cuidado/produto e inventário preservados |
| Persistência | Cena e arquivos persistentes com hashes antes/depois iguais; zero erros runtime |
| Capturas | 315 imagens amostradas + quatro contextos; arquivos verificados |
| Árvores | 71 transforms idênticos, SHA256 `487BF17A4A6103C6A0B1180EC03BFE0A0D743ED72B337C53CF6661C244A7F7FD` |

Os 65 testes precederam somente a correção do gerador de cordeiro; o batch de geração e o Play B posteriores comprovaram compilação, registro e soltura reais dessa correção. Nenhuma lógica runtime foi alterada nesta rodada.

## Revisão independente
Revisão prévia SOUND_WITH_AMENDMENTS incorporada: capacidade quatro obrigatória, identidade por ID e escala contra personagem/porta. Revisão de arte corrigiu os chifres frontais da cabra. Revisão das capturas start/before_restore/restored e amostras 0050/0100/0171 não encontrou bloqueador de proporção ou corte. Textura dos animais é mais simples que cenário, porém legível; aumentar resolução não foi recomendado.

Hashes das folhas integradas: cow `D6A5DE990BCE8F5DBE887B4CD71DBC9F93608B891FF9AEDD14990CB218C04219`; sheep `EA68613BC74A98CF100046D7EC516392B1CF7D105C2497788D1721853B0F9C54`; goat `41150D6BE1509AE56A293DB4445077EE29050595D9220FC7AC120B9D9F4B0B6B`.

## Limites e fechamento
Promoção: **NO**, aceitação visual/humana pendente. A página reproduz imagens a aproximadamente 5fps; não comprova fluidez na taxa completa. O probe usa quatro IDs e uma execução, não todas as sementes. CanInteract não substitui caminhar até cada animal usando input. Contato direto com o feno não ocorreu nesta área segura. Animais têm triggers para não bloquear o personagem; os testes verificam contenção e separação entre animais/sólidos, não bloqueio físico do jogador pelos animais.

Cenário humano restante: no jogo, soltar os quatro animais, aproximar-se por input, interagir, observar ciclos e escala por um minuto, salvar/carregar pelo fluxo real. O teste automatizado restaurou o provider em memória, sem escrever save. Nenhuma aceitação global da fazenda ou porcentagem de semelhança à keyart é declarada.
