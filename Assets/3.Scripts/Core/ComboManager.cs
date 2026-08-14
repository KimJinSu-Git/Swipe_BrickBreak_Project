using System;
using Bird.Ball;
using Bird.Data;
using UnityEngine;

namespace Bird.Core
{
    public class ComboManager : MonoBehaviour
    {
        [Header("Dependencies")] 
        [SerializeField] private ComboData comboData;
        [SerializeField] private BallManager ballManager;
        [SerializeField] private CoinManager coinManager;

        [Header("Runtime")] 
        [SerializeField] private int currentCombo = 0;
        [SerializeField] private int pendingCoins = 0;
        
        public event Action<int> OnComboChanged;

        /// <summary>
        /// 공이 블록과 충돌할 때마다 호출되어 콤보를 1씩 증가시킵니다.
        /// </summary>
        public void AddCombo()
        {
            currentCombo++;
            OnComboChanged?.Invoke(currentCombo);
        }
        
        /// <summary>
        /// 블록이 파괴될 때마다 즉시 코인을 얻지 않고 달아둡니다.
        /// </summary>
        public void AddPendingCoin(int amount)
        {
            pendingCoins += amount;
        }
        
        /// <summary>
        /// 턴 종료 시점에 호출되어 최종 배율을 적용하고 코인을 지급합니다.
        /// </summary>
        public void ApplyTurnEndRewards()
        {
            int ballCount = ballManager != null ? ballManager.PlayerDeck.Count : 1; 
            
            // 데이터에 현재 콤보와 공 개수를 넘겨 최종 배율을 받아옵니다.
            float multiplier = comboData != null ? comboData.GetCoinMultiplier(currentCombo, ballCount) : 1f;

            // 계산서에 적힌 코인에 배율을 곱해 최종 지급액을 산정합니다.
            int finalCoins = Mathf.RoundToInt(pendingCoins * multiplier);
            
            if (finalCoins > 0 && coinManager != null)
            {
                coinManager.AddCoins(finalCoins);
                Debug.Log($"[정산 완료] 기본 코인: {pendingCoins} -> 배율({multiplier}x) 적용 -> 최종 지급: {finalCoins}");
            }

            // 다음 턴을 위해 계산서와 콤보를 초기화합니다.
            currentCombo = 0;
            pendingCoins = 0;
            OnComboChanged?.Invoke(currentCombo);
        }
    }
}
