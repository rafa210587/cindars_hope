using CindarsHope.Core.Data;
using System;
using UnityEngine;

namespace CindarsHope.UI.Menu
{
    [CreateAssetMenu(fileName = "MenuSystem_", menuName = "CindarsHope/UI/MenuSystem")]
    public class MenuSystemDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string MenuName;
        public MenuType Type;
        public int DisplayOrder;
        public bool RequiresGamePause = true;
        public bool AllowGameplayWhileOpen = false;

        string IIdentifiedData.Id => Id;
    }

    public enum MenuType
    {
        None,
        Main,
        Pause,
        Inventory,
        Equipment,
        Skills,
        Character,
        Map,
        Settings,
        Quit
    }

    [Serializable]
    public class MenuState
    {
        public MenuType CurrentMenu;
        public bool IsGamePaused;
        public float TimeScale = 1f;
    }
}
