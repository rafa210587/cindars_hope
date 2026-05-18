using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.World.Data;
using UnityEngine;

namespace CindarsHope.World
{
    [DisallowMultipleComponent]
    public class TreeNode : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _treeIndex;
        [SerializeField] private TreeDataSO _treeData;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public int HitsTaken { get; private set; }
        public bool IsChopped { get; private set; }
        public string InteractionPrompt => IsChopped ? "Cortada" : "Cortar";

        public void Configure(int treeIndex, TreeDataSO treeData, InventoryManager inventoryManager, SpriteRenderer spriteRenderer)
        {
            _treeIndex = treeIndex;
            _treeData = treeData;
            _inventoryManager = inventoryManager;
            _spriteRenderer = spriteRenderer;
            UpdateVisual();
        }

        public bool CanInteract(GameObject interactor)
        {
            return !IsChopped && _treeData != null && _inventoryManager != null;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                Debug.Log($"TreeNode {_treeIndex} cannot be chopped now.", this);
                return;
            }

            HitsTaken++;
            Debug.Log($"TreeNode {_treeIndex} hit {HitsTaken}/{_treeData.RequiredHits}.", this);

            if (HitsTaken < _treeData.RequiredHits)
            {
                UpdateVisual();
                return;
            }

            if (!_inventoryManager.AddItem(_treeData.WoodItemId, _treeData.WoodAmount))
            {
                Debug.LogWarning($"TreeNode {_treeIndex} could not add wood '{_treeData.WoodItemId}' x{_treeData.WoodAmount}.", this);
                HitsTaken = Mathf.Max(0, HitsTaken - 1);
                return;
            }

            IsChopped = true;
            UpdateVisual();
            GameEventBus.Publish(new TreeChoppedEvent(_treeData.Id, _treeData.WoodItemId, _treeData.WoodAmount, HitsTaken, GetTilePosition()));
            Debug.Log($"TreeNode {_treeIndex} chopped. Added '{_treeData.WoodItemId}' x{_treeData.WoodAmount}.", this);
        }

        public TreeSaveData CaptureSaveData()
        {
            return new TreeSaveData
            {
                TreeIndex = _treeIndex,
                TreeId = _treeData != null ? _treeData.Id : string.Empty,
                HitsTaken = HitsTaken,
                IsChopped = IsChopped
            };
        }

        public void RestoreFromSaveData(TreeSaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogWarning($"TreeNode {_treeIndex} cannot restore from null save data.", this);
                return;
            }

            HitsTaken = Mathf.Max(0, saveData.HitsTaken);
            IsChopped = saveData.IsChopped;
            UpdateVisual();
        }

        public void RebindInventoryManager(InventoryManager inventoryManager)
        {
            if (inventoryManager == null)
            {
                Debug.LogWarning($"TreeNode {_treeIndex} received null InventoryManager for rebind.", this);
                return;
            }

            _inventoryManager = inventoryManager;
        }

        private void Reset()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnValidate()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void UpdateVisual()
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            if (IsChopped)
            {
                _spriteRenderer.color = new Color(0.45f, 0.45f, 0.45f, 0.45f);
                return;
            }

            var hitRatio = _treeData == null || _treeData.RequiredHits <= 0
                ? 0f
                : Mathf.Clamp01((float)HitsTaken / _treeData.RequiredHits);
            _spriteRenderer.color = Color.Lerp(new Color(0.24f, 0.48f, 0.22f), new Color(0.58f, 0.42f, 0.24f), hitRatio);
        }

        private Vector2Int GetTilePosition()
        {
            return Vector2Int.RoundToInt(transform.position);
        }
    }
}
