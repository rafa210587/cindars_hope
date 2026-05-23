using UnityEngine;

namespace CindarsHope.UI.Menu
{
    public class MenuManager : MonoBehaviour
    {
        private MenuState _currentState = new MenuState();

        public MenuState GetMenuState() => _currentState;

        public void OpenMenu(MenuType menuType)
        {
            _currentState.CurrentMenu = menuType;

            if (menuType == MenuType.Pause)
            {
                PauseGame();
            }
            else if (_currentState.IsGamePaused && menuType == MenuType.None)
            {
                ResumeGame();
            }
        }

        public void CloseMenu()
        {
            if (_currentState.IsGamePaused)
                ResumeGame();

            _currentState.CurrentMenu = MenuType.None;
        }

        private void PauseGame()
        {
            _currentState.IsGamePaused = true;
            _currentState.TimeScale = 0f;
            Time.timeScale = 0f;
        }

        private void ResumeGame()
        {
            _currentState.IsGamePaused = false;
            _currentState.TimeScale = 1f;
            Time.timeScale = 1f;
        }

        public bool IsGamePaused() => _currentState.IsGamePaused;

        public MenuType GetCurrentMenu() => _currentState.CurrentMenu;
    }
}
