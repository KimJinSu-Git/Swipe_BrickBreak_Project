using System;
using UnityEngine;

namespace Bird.Core
{
    public class CoinManager : MonoBehaviour
    {
        [SerializeField] private int currentCoins = 0;

        public event Action<int> OnCoinChanged;

        public bool TrySpendCoins(int amount)
        {
            if (currentCoins >= amount)
            {
                currentCoins -= amount;
                OnCoinChanged?.Invoke(currentCoins);
                return true;
            }

            return false;
        }

        public void AddCoins(int amount)
        {
            currentCoins += amount;
            OnCoinChanged?.Invoke(currentCoins);
        }
    }
}
