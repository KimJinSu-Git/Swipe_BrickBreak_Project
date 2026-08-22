using System;
using UnityEngine;

namespace Bird.Core
{
    public class CoinManager : MonoBehaviour
    {
        [SerializeField] private int currentCoins = 0;

        public event Action<int> OnCoinChanged;
        public event Action<int> OnCoinEarned;
        
        public int CurrentCoins => currentCoins;
        
        private void Start() => OnCoinChanged?.Invoke(currentCoins);

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
            
            OnCoinEarned?.Invoke(amount);
            OnCoinChanged?.Invoke(currentCoins);
        }
        
        public void RestoreCoin(int savedCoin)
        {
            currentCoins = savedCoin;
            OnCoinChanged?.Invoke(currentCoins);
        }
    }
}
