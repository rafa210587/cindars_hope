using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Player.Data;
using CindarsHope.Save;
using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class PlayerManager : MonoBehaviour
    {
        public bool IsInitialized { get; private set; }
        public int CurrentGold { get; private set; }
        public int CurrentHP { get; private set; }
        public int MaxHP { get; private set; }
        public int Strength { get; set; } = 0;

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;
        }

        public void Initialize(PlayerDataSO playerData)
        {
            Initialize();

            if (playerData == null)
            {
                Debug.LogWarning("PlayerManager cannot initialize starting state because PlayerDataSO is missing.", this);
                return;
            }

            var previousGold = CurrentGold;
            CurrentGold = Mathf.Max(0, playerData.StartingGold);
            MaxHP = Mathf.Max(1, playerData.BaseHP);
            CurrentHP = MaxHP;

            var delta = CurrentGold - previousGold;
            if (delta != 0 || CurrentGold != 0)
            {
                GameEventBus.Publish(new GoldChangedEvent(delta == 0 ? CurrentGold : delta, CurrentGold));
            }

            GameEventBus.Publish(new HPChangedEvent(CurrentHP, CurrentHP, MaxHP));
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            IsInitialized = false;
        }

        public void SetGold(int newGold)
        {
            newGold = Mathf.Max(0, newGold);
            if (CurrentGold == newGold)
            {
                return;
            }

            var delta = newGold - CurrentGold;
            CurrentGold = newGold;
            GameEventBus.Publish(new GoldChangedEvent(delta, CurrentGold));
        }

        public bool TrySpendGold(int amount)
        {
            if (amount <= 0 || CurrentGold < amount)
            {
                return false;
            }

            SetGold(CurrentGold - amount);
            return true;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            SetGold(CurrentGold + amount);
        }

        // F18: máximo derivado (equipment/passivas) preservando a proporção corrente (floor 1).
        public void SetMaxHP(int newMaxHP, bool preserveRatio = true)
        {
            newMaxHP = Mathf.Max(1, newMaxHP);
            if (newMaxHP == MaxHP)
            {
                return;
            }

            var ratio = MaxHP > 0 ? (float)CurrentHP / MaxHP : 1f;
            MaxHP = newMaxHP;
            var newCurrent = preserveRatio ? Mathf.Max(1, Mathf.RoundToInt(newMaxHP * ratio)) : Mathf.Min(CurrentHP, newMaxHP);
            SetHP(newCurrent);
        }

        public void SetHP(int newHP)
        {
            MaxHP = Mathf.Max(1, MaxHP);
            newHP = Mathf.Clamp(newHP, 0, MaxHP);
            if (CurrentHP == newHP)
            {
                return;
            }

            var delta = newHP - CurrentHP;
            CurrentHP = newHP;
            GameEventBus.Publish(new HPChangedEvent(delta, CurrentHP, MaxHP));
        }

        public void DamageHP(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            SetHP(CurrentHP - amount);
        }

        public void RestoreHP(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            SetHP(CurrentHP + amount);
        }

        public PlayerSaveData CaptureSaveData(int currentHunger, int maxHunger, Vector2 playerPosition)
        {
            return new PlayerSaveData
            {
                CurrentHP = CurrentHP,
                MaxHP = MaxHP,
                Gold = CurrentGold,
                CurrentHunger = currentHunger,
                MaxHunger = maxHunger,
                PlayerPosition = playerPosition
            };
        }

        public void RestoreFromSaveData(PlayerSaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogWarning("PlayerManager cannot restore from null PlayerSaveData.", this);
                return;
            }

            var previousGold = CurrentGold;
            var previousHp = CurrentHP;

            MaxHP = Mathf.Max(1, saveData.MaxHP);
            CurrentHP = Mathf.Clamp(saveData.CurrentHP, 0, MaxHP);
            CurrentGold = Mathf.Max(0, saveData.Gold);

            var goldDelta = CurrentGold - previousGold;
            if (goldDelta != 0)
            {
                GameEventBus.Publish(new GoldChangedEvent(goldDelta, CurrentGold));
            }

            var hpDelta = CurrentHP - previousHp;
            GameEventBus.Publish(new HPChangedEvent(hpDelta, CurrentHP, MaxHP));
        }
    }
}
