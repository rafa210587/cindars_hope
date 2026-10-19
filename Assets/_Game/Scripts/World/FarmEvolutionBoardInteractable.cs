using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.World
{
    /// <summary>
    /// Quadro de Evolucoes da Fazenda. Substitui os lotes fable_41 (Norte/Leste/Oeste).
    /// Stub: abre um painel IMGUI listando as evolucoes disponiveis; compras sao deferidas
    /// para a spec futura de progressao/evolucao.
    /// Criado em 2026-06-26 (spec_farm_scene_relayout_v4).
    /// DECISAO: lotes fixos fable_41 removidos; crops de pomar preservados no catalogo.
    /// </summary>
    public class FarmEvolutionBoardInteractable : MonoBehaviour, IInteractable
    {
        private bool _panelOpen = false;

        public string InteractionPrompt => "Consultar Quadro de Evolucoes";

        public bool CanInteract(GameObject interactor)
        {
            return true;
        }

        public void Interact(GameObject interactor)
        {
            _panelOpen = !_panelOpen;
            if (_panelOpen)
            {
                Debug.Log("[FarmEvolutionBoard] Quadro de Evolucoes aberto. Funcionalidade completa aguarda spec de progressao.");
            }
        }

        private void OnGUI()
        {
            if (!_panelOpen)
            {
                return;
            }

            // Painel stub IMGUI — sera substituido por UI real na spec de evolucoes.
            var panelRect = new Rect(Screen.width * 0.5f - 200f, Screen.height * 0.5f - 150f, 400f, 300f);
            GUI.Box(panelRect, "Quadro de Evolucoes da Fazenda");

            GUILayout.BeginArea(new Rect(panelRect.x + 10f, panelRect.y + 30f, panelRect.width - 20f, panelRect.height - 60f));
            GUILayout.Label("Evolucoes disponiveis (em desenvolvimento):");
            GUILayout.Space(8f);
            GUILayout.Label("  - Expandir bosque");
            GUILayout.Label("  - Construir segundo celeiro");
            GUILayout.Label("  - Desbloquear estufa avancada");
            GUILayout.Label("  - Pomar de frutas (maça/cereja/pera/ameixa)");
            GUILayout.Space(8f);
            GUILayout.Label("[Compras disponiveis na spec de evolucao]");
            GUILayout.EndArea();

            if (GUI.Button(new Rect(panelRect.x + panelRect.width - 80f, panelRect.y + panelRect.height - 35f, 70f, 26f), "Fechar"))
            {
                _panelOpen = false;
            }
        }
    }
}
