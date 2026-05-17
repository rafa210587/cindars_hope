using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Player.Data;
using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class PlayerManager : MonoBehaviour
    {
        public bool IsInitialized { get; private set; }
        public int CurrentGold { get; private set; }
        public int CurrentHP { get; private set; }

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
            CurrentHP = Mathf.Max(1, playerData.BaseHP);

            var delta = CurrentGold - previousGold;
            if (delta != 0 || CurrentGold != 0)
            {
                GameEventBus.Publish(new GoldChangedEvent(delta == 0 ? CurrentGold : delta, CurrentGold));
            }
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
    }
}
