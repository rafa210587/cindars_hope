using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.Fonte
{
    /// <summary>
    /// Interactable físico da Fonte de Anya (F17). Menu IMGUI no padrão dos painéis atuais
    /// (F14 substituirá por Canvas). Regra canônica: função não desbloqueada aparece como "???"
    /// — nunca revela o que está selado.
    /// </summary>
    [DisallowMultipleComponent]
    public class FonteInteractable : MonoBehaviour, IInteractable
    {
        private static FonteInteractable _openMenu;

        public string InteractionPrompt => "Fonte de Anya";

        public bool CanInteract(GameObject interactor)
        {
            return _openMenu == null || _openMenu == this;
        }

        public void Interact(GameObject interactor)
        {
            _openMenu = _openMenu == this ? null : this;
        }

        private void OnDisable()
        {
            if (_openMenu == this)
            {
                _openMenu = null;
            }
        }

        private void Update()
        {
            if (_openMenu == this && Input.GetKeyDown(KeyCode.Escape))
            {
                _openMenu = null;
            }
        }

        private void OnGUI()
        {
            if (_openMenu != this)
            {
                return;
            }

            CindarsHope.UI.MenuGuiStyle.Apply();
            var service = FonteRuntimeService.Instance;
            var rect = new Rect(Screen.width * 0.5f - 170f, Screen.height * 0.5f - 130f, 340f, 260f);
            GUILayout.BeginArea(rect, GUI.skin.window);
            GUILayout.Label("Fonte de Anya");
            GUILayout.Space(6f);

            if (service == null)
            {
                GUILayout.Label("A Fonte permanece silenciosa.");
            }
            else
            {
                DrawFunctions(service);
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Fechar (Esc)"))
            {
                _openMenu = null;
            }

            GUILayout.EndArea();
        }

        private void DrawFunctions(FonteRuntimeService service)
        {
            GUILayout.Label($"Estado: {service.Section.FonteState}");
            GUILayout.Label("Ponto de Retorno — ativo (voce desperta aqui ao cair).");

            // Água Viva: visível apenas com o Fragmento da Água (canon: sem spoiler).
            if (service.Section.LivingWater.Unlocked)
            {
                var charges = service.Section.LivingWater.CurrentCharges;
                var label = $"Coletar Agua Viva ({charges} carga(s), 1x/dia)";
                if (GUILayout.Button(label))
                {
                    var result = service.TryCollectLivingWater();
                    if (!result.Success)
                    {
                        GameEventBus.Publish(new PlayerActionFeedbackEvent($"A Fonte nao concedeu agua ({result.FailureReason})."));
                    }
                }
            }
            else
            {
                GUILayout.Label("??? (a agua esta parada)");
            }

            // Respec: visível apenas com o Fragmento da Memória.
            if (service.Section.Respec.Unlocked)
            {
                if (GUILayout.Button("Redistribuir habilidades (Respec)"))
                {
                    TryRespec();
                }
            }
            else
            {
                GUILayout.Label("??? (ecos adormecidos)");
            }
        }

        private void TryRespec()
        {
            var bootstrap = GameBootstrap.Instance;
            var skillManager = CindarsHope.Skills.SkillTreeManager.Instance;
            var playerManager = bootstrap != null ? bootstrap.PlayerManager : null;
            var progressionManager = bootstrap != null ? bootstrap.PlayerProgressionManager : null;
            if (skillManager == null || playerManager == null || progressionManager == null)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("A Fonte nao conseguiu tocar suas memorias agora."));
                return;
            }

            var gold = playerManager.CurrentGold;
            if (skillManager.TryRespec(ref gold, progressionManager.Level))
            {
                playerManager.SetGold(gold);
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Suas habilidades retornaram a Fonte."));
            }
            else
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Respec indisponivel no momento."));
            }
        }
    }
}
