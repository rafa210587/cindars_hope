using UnityEngine;

namespace CindarsHope.UI.Quests.Runtime
{
    /// <summary>
    /// Runtime binder for QuestLogPanelController.
    /// Listens for J key input to open/close the Quest Log.
    ///
    /// WAVE_INTEGRATION_15 — Quest Giver + Quest Log Real
    ///
    /// Attach to the same GameObject as QuestLogPanelController.
    /// No direct ref to gameplay systems — all communication via QuestLogPanelController.
    /// </summary>
    [RequireComponent(typeof(QuestLogPanelController))]
    public class QuestLogRuntimeBinder : MonoBehaviour
    {
        private QuestLogPanelController _panel;

        private void Awake()
        {
            _panel = GetComponent<QuestLogPanelController>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                if (_panel.IsOpen)
                    _panel.Close();
                else
                    _panel.Open();
            }
        }
    }
}
