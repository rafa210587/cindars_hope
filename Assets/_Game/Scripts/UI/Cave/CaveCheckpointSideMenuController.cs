using System.Collections.Generic;
using System.Linq;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.UI.Cave
{
    /// <summary>
    /// Side menu shown when the player reaches a checkpoint in the cave.
    /// Subscribes to CheckpointMenuOpenedEvent and shows a list of unlocked
    /// checkpoints for warp selection. Uses OnGUI as functional fallback.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CaveCheckpointSideMenuController : MonoBehaviour
    {
        private bool _isOpen;
        private List<int> _checkpoints = new List<int>();
        private int _selectedIndex;

        private void OnEnable()
        {
            GameEventBus.Subscribe<CheckpointMenuOpenedEvent>(OnCheckpointMenuOpened);
            GameEventBus.Subscribe<CheckpointMenuClosedEvent>(OnCheckpointMenuClosed);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CheckpointMenuOpenedEvent>(OnCheckpointMenuOpened);
            GameEventBus.Unsubscribe<CheckpointMenuClosedEvent>(OnCheckpointMenuClosed);
        }

        private void OnCheckpointMenuOpened(CheckpointMenuOpenedEvent evt)
        {
            var runManager = GameBootstrap.Instance?.CaveRunManager;
            if (runManager == null) return;

            _checkpoints = runManager.State.UnlockedCheckpoints.OrderBy(cp => cp).ToList();
            if (_checkpoints.Count == 0) _checkpoints.Add(1);

            _selectedIndex = 0;
            _isOpen = true;
        }

        private void OnCheckpointMenuClosed(CheckpointMenuClosedEvent evt)
        {
            _isOpen = false;
        }

        private void Update()
        {
            if (!_isOpen) return;

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                _selectedIndex = (_selectedIndex - 1 + _checkpoints.Count) % _checkpoints.Count;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                _selectedIndex = (_selectedIndex + 1) % _checkpoints.Count;
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
            {
                ConfirmSelection();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        private void OnGUI()
        {
            if (!_isOpen) return;

            var width = 280f;
            var height = Mathf.Max(160f, 60f + _checkpoints.Count * 28f);
            var rect = new Rect(
                Screen.width - width - 16f,
                Screen.height * 0.5f - height * 0.5f,
                width,
                height);

            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label("Cave Checkpoints");
            GUILayout.Space(4);

            for (var i = 0; i < _checkpoints.Count; i++)
            {
                var label = i == _selectedIndex
                    ? $"> Level {_checkpoints[i]}"
                    : $"  Level {_checkpoints[i]}";
                GUILayout.Label(label);
            }

            GUILayout.Space(8);
            GUILayout.Label("W/S navigate  |  E confirm  |  Esc close");
            GUILayout.EndArea();
        }

        private void ConfirmSelection()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _checkpoints.Count) return;

            GameEventBus.Publish(new CaveCheckpointSelectedEvent(_checkpoints[_selectedIndex]));
            Close();
        }

        private void Close()
        {
            _isOpen = false;
            GameEventBus.Publish(new CheckpointMenuClosedEvent());
        }
    }
}
