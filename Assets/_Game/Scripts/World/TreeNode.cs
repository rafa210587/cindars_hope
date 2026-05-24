using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.Tools;
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
        public int CurrentHp { get; private set; }
        public bool IsChopped { get; private set; }
        public int RegrowthRemainingDays { get; private set; }
        public string InteractionPrompt => IsChopped ? "Cortada" : "Cortar";

        public void Configure(int treeIndex, TreeDataSO treeData, InventoryManager inventoryManager, SpriteRenderer spriteRenderer)
        {
            _treeIndex = treeIndex;
            _treeData = treeData;
            _inventoryManager = inventoryManager;
            _spriteRenderer = spriteRenderer;
            CurrentHp = GetMaxHp();
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

            if (!HasRequiredTool())
            {
                const string message = "Requires Axe.";
                GameEventBus.Publish(new PlayerActionFeedbackEvent(message));
                Debug.Log($"TreeNode {_treeIndex} blocked chop. {message}", this);
                return;
            }

            var isFinalHit = CurrentHp <= 1;
            var dropAmount = RollWoodAmount(isFinalHit);
            if (!_inventoryManager.AddItem(_treeData.WoodItemId, dropAmount))
            {
                Debug.LogWarning($"TreeNode {_treeIndex} could not add wood '{_treeData.WoodItemId}' x{dropAmount}.", this);
                return;
            }

            HitsTaken++;
            CurrentHp = Mathf.Max(0, CurrentHp - 1);
            Debug.Log($"TreeNode {_treeIndex} hit {HitsTaken}. Hp={CurrentHp}/{GetMaxHp()}, wood={dropAmount}.", this);

            if (CurrentHp <= 0)
            {
                IsChopped = true;
                RegrowthRemainingDays = _treeData.RegrowthDays;
                GameEventBus.Publish(new TreeChoppedEvent(_treeData.Id, _treeData.WoodItemId, dropAmount, HitsTaken, GetTilePosition()));
                Debug.Log($"TreeNode {_treeIndex} chopped. Added final '{_treeData.WoodItemId}' x{dropAmount}.", this);
            }

            UpdateVisual();
        }

        public TreeSaveData CaptureSaveData()
        {
            return new TreeSaveData
            {
                TreeIndex = _treeIndex,
                TreeId = _treeData != null ? _treeData.Id : string.Empty,
                HitsTaken = HitsTaken,
                CurrentHp = CurrentHp,
                IsChopped = IsChopped,
                IsStump = IsChopped,
                RegrowthRemainingDays = RegrowthRemainingDays
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
            CurrentHp = saveData.CurrentHp > 0 ? saveData.CurrentHp : Mathf.Max(0, GetMaxHp() - HitsTaken);
            IsChopped = saveData.IsChopped;
            RegrowthRemainingDays = Mathf.Max(0, saveData.RegrowthRemainingDays);
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

        private void Awake()
        {
            if (CurrentHp <= 0 && !IsChopped)
            {
                CurrentHp = GetMaxHp();
            }

            UpdateVisual();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnValidate()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            if (!IsChopped || RegrowthRemainingDays <= 0)
            {
                return;
            }

            RegrowthRemainingDays--;
            if (RegrowthRemainingDays > 0)
            {
                return;
            }

            IsChopped = false;
            HitsTaken = 0;
            CurrentHp = GetMaxHp();
            UpdateVisual();
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

        private static bool HasRequiredTool()
        {
            EquipmentManager equipmentManager = null;
            if (GameBootstrap.Instance != null)
            {
                equipmentManager = GameBootstrap.Instance.EquipmentManager;
            }

            return equipmentManager != null && equipmentManager.HasTool(ToolType.Axe, ToolTier.Basic);
        }

        private int GetMaxHp()
        {
            if (_treeData == null)
            {
                return 1;
            }

            return Mathf.Max(1, Mathf.Max(_treeData.MaxHp, _treeData.RequiredHits));
        }

        private int RollWoodAmount(bool isFinalHit)
        {
            if (_treeData == null)
            {
                return 1;
            }

            var amount = Random.Range(_treeData.WoodPerHitMin, _treeData.WoodPerHitMax + 1);
            if (isFinalHit)
            {
                amount = Mathf.Max(amount * _treeData.FinalHitMultiplier, amount * 2);
            }

            return Mathf.Max(1, amount);
        }

        private Vector2Int GetTilePosition()
        {
            return Vector2Int.RoundToInt(transform.position);
        }
    }
}
