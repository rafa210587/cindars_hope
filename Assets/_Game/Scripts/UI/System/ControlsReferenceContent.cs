using System.Collections.Generic;

namespace CindarsHope.UI.SystemTab
{
    /// <summary>
    /// fable_62 — conteudo estatico e read-only da tela "Controles" da aba Sistema (F56).
    ///
    /// Espelha docs/game_rules/input_map.md (F67) — a fonte canonica das teclas. NAO e uma segunda
    /// fonte de verdade: se uma tecla mudar, atualiza-se input_map.md e ENTAO este conteudo (regra
    /// da spec). Somente bindings CANONICOS (gameplay/paineis); teclas de debug/dev NUNCA entram
    /// aqui (input_map.md as marca non-contract). Classe pura (sem UnityEngine), testavel em
    /// EditMode; o MonoBehaviour da tela apenas renderiza estas linhas.
    /// </summary>
    public static class ControlsReferenceContent
    {
        public readonly struct ControlLine
        {
            public readonly string Keys;
            public readonly string Action;

            public ControlLine(string keys, string action)
            {
                Keys = keys;
                Action = action;
            }
        }

        public readonly struct ControlGroup
        {
            public readonly string Title;
            public readonly IReadOnlyList<ControlLine> Lines;

            public ControlGroup(string title, IReadOnlyList<ControlLine> lines)
            {
                Title = title;
                Lines = lines;
            }
        }

        private static readonly ControlGroup[] Groups =
        {
            new ControlGroup("Movimento", new[]
            {
                new ControlLine("WASD / Setas", "Mover"),
                new ControlLine("Espaco", "Desviar (dodge)"),
                new ControlLine("Segurar Shift", "Bloquear / guardar"),
                new ControlLine("Toque duplo direcao", "Dash direcional"),
            }),
            new ControlGroup("Combate", new[]
            {
                new ControlLine("E", "Atacar / interagir / coletar"),
                new ControlLine("Segurar E", "Ataque carregado (mao direita)"),
                new ControlLine("Q", "Ataque pesado / mao esquerda"),
                new ControlLine("Segurar Q", "Ataque carregado (mao esquerda)"),
                new ControlLine("1 / 2 / 3 / 4", "Usar slot de skill ativa"),
                new ControlLine("R / T / Y / G", "Equipar skill no slot ativo"),
                new ControlLine("H", "Consumir comida"),
            }),
            new ControlGroup("Paineis", new[]
            {
                new ControlLine("I", "Inventario"),
                new ControlLine("K", "Personagem / Equipamento"),
                new ControlLine("L", "Atributos (no painel de equipamento)"),
                new ControlLine("U", "Arvore de skills"),
                new ControlLine("J", "Registro de quests"),
                new ControlLine("C", "Craft de bolso"),
                new ControlLine("Esc", "Fechar modal / voltar"),
            }),
            new ControlGroup("Dentro de menus", new[]
            {
                new ControlLine("WASD / Setas", "Navegar opcoes"),
                new ControlLine("Enter / Espaco / E", "Confirmar / selecionar"),
                new ControlLine("Esc", "Cancelar / voltar"),
            }),
        };

        /// <summary>Grupos de controles em ordem estavel (read-only).</summary>
        public static IReadOnlyList<ControlGroup> All => Groups;
    }
}
