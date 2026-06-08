using System.Collections.Generic;

namespace CindarsHope.UI.Quest
{
    public enum QuestLogTab { Active = 0, Completed = 1, Failed = 2, MainProgression = 3 }

    public class QuestLogMenuState
    {
        public QuestLogTab SelectedTab { get; set; }
        public int SelectedQuestIndex { get; set; }
        public bool IsOpen { get; set; }
        public List<string> TabOrder { get; set; } = new List<string> { "Active", "Completed", "Failed", "Main" };
    }
}
